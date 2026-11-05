using OoBDev.Documents.ExternalTools;
using OoBDev.Documents.Hpgl;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using OoBDev.Documents.Conversion;
using OoBDev.System.IO;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Wraps the "Convert/Rasterize" mechanisms proposed in
/// docs/design/StreamTools/stream-content-detection.md's "Raster/convert tool integration" section —
/// registered external tools, a single legacy external tool, and the internal HP-GL-to-SVG converter — behind
/// one call so the Stream Monitor windows' "Convert..." action doesn't need to know which is active.
/// </summary>
/// <remarks>
/// Never throws: every failure path (missing config, a process that fails to start or exits
/// non-zero, malformed input) returns a <see cref="StreamConversionResult"/>
/// with <see cref="StreamConversionResult.Success"/> false and an explanatory
/// <see cref="StreamConversionResult.Error"/>, matching <see cref="StreamMonitor"/>'s own
/// tolerant, result-object-returning file I/O, so it can be constructed ad hoc (as
/// <see cref="StreamMonitor"/> itself is, in the TUI/WPF front ends) without a DI container.
/// </remarks>
public sealed class StreamCaptureConverter
{
    private static readonly ITempFileFactory _temp = new TempFileFactory(new ServiceCollection().BuildServiceProvider());

    private readonly IOptions<StreamCaptureConverterOptions> _options;

    public StreamCaptureConverter(IOptions<StreamCaptureConverterOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options;
    }

    /// <summary>Whether a mechanism is configured at all - lets a caller skip offering "Convert..." when there's nothing to do.</summary>
    public bool IsConfigured => _options.Value.Mode != StreamConversionMode.None;

    /// <summary>
    /// Converts <paramref name="capture"/> per the configured mechanism, writing the result next to
    /// its already-saved file (same directory and file-name stem, <see cref="StreamCaptureConverterOptions.OutputExtension"/>
    /// or a per-mechanism default extension).
    /// </summary>
    public async Task<StreamConversionResult> ConvertAsync(StreamMonitorCapture capture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var options = _options.Value;

        return options.Mode switch
        {
            StreamConversionMode.InternalHpglToSvg => await ConvertInternal(capture, options, cancellationToken).ConfigureAwait(false),
            StreamConversionMode.ExternalTool => await ConvertExternalAsync(capture, options, cancellationToken).ConfigureAwait(false),
            StreamConversionMode.Auto => await ConvertWithRegisteredToolAsync(capture, options, null, cancellationToken).ConfigureAwait(false),
            StreamConversionMode.Tool => await ConvertWithRegisteredToolAsync(capture, options, options.ToolName, cancellationToken).ConfigureAwait(false),
            _ => new StreamConversionResult(false, null, "No conversion mechanism is selected. Choose one from the Conversion list next to Convert (or set Stream Convert Mode in the profile)."),
        };
    }

    private static Task<StreamConversionResult> ConvertInternal(StreamMonitorCapture capture, StreamCaptureConverterOptions options, CancellationToken cancellationToken)
    {
        if (capture.Capture.Kind.Format != StreamContentFormat.Hpgl)
        {
            return Task.FromResult(new StreamConversionResult(false, null, $"The internal HP-GL-to-SVG converter only handles HP-GL captures, not {capture.Capture.Kind.DisplayName}."));
        }

        return RunHandlerAsync(capture, new HpglToSvgConversionHandler(), HpglToSvgConversionHandler.SvgContentType, options.OutputExtension ?? "svg", cancellationToken);
    }

    private static Task<StreamConversionResult> ConvertExternalAsync(StreamMonitorCapture capture, StreamCaptureConverterOptions options, CancellationToken cancellationToken) =>
        RunToolAsync(capture, options.ExternalToolPath, options.ExternalToolArguments, options.ExternalToolDpi, options.OutputExtension ?? "png", cancellationToken);

    /// <summary>Runs the tool named <paramref name="toolName"/>, or (null) the first registered tool whose formats include the capture's.</summary>
    private static Task<StreamConversionResult> ConvertWithRegisteredToolAsync(StreamMonitorCapture capture, StreamCaptureConverterOptions options, string? toolName, CancellationToken cancellationToken)
    {
        ExternalToolOptions? tool;
        if (toolName is null)
        {
            tool = options.Tools.FirstOrDefault(t => Handles(t, capture.Capture.Kind));
            if (tool is null)
            {
                return Task.FromResult(new StreamConversionResult(false, null, $"No registered tool handles {capture.Capture.Kind.DisplayName}. Register one in the profile's StreamConvertTools, or pick a tool by name."));
            }
        }
        else
        {
            tool = options.Tools.FirstOrDefault(t => string.Equals(t.Name, toolName, StringComparison.OrdinalIgnoreCase));
            if (tool is null)
            {
                return Task.FromResult(new StreamConversionResult(false, null, $"No registered tool is named '{toolName}'."));
            }
        }

        return RunToolAsync(capture, tool.Path, tool.Arguments, tool.Dpi, options.OutputExtension ?? tool.OutputExtension, cancellationToken);
    }

    /// <summary>Whether <paramref name="tool"/> accepts <paramref name="kind"/>: an empty <c>Formats</c> accepts anything, otherwise a token naming the format or the capture's extension.</summary>
    public static bool Handles(ExternalToolOptions tool, StreamContentKind kind)
    {
        var tokens = tool.Formats.Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
        {
            return true;
        }

        var names = kind.Format switch
        {
            StreamContentFormat.Hpgl => new[] { "hpgl", "plt" },
            StreamContentFormat.PostScript => ["ps", "postscript", "eps"],
            StreamContentFormat.Pcl => ["pcl"],
            StreamContentFormat.Image => ["image"],
            _ => Array.Empty<string>(),
        };
        return tokens.Any(t => names.Contains(t, StringComparer.OrdinalIgnoreCase) || string.Equals(t, kind.Extension, StringComparison.OrdinalIgnoreCase));
    }

    private static Task<StreamConversionResult> RunToolAsync(StreamMonitorCapture capture, string? toolPath, string arguments, int dpi, string outputExtension, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(toolPath))
        {
            return Task.FromResult(new StreamConversionResult(false, null, "No external converter tool path is configured."));
        }

        // The registration's Formats only choose a tool (Handles); a tool picked explicitly runs on any capture, so the handler accepts any source.
        var tool = new ExternalToolOptions { Name = Path.GetFileNameWithoutExtension(toolPath), Path = toolPath, Arguments = arguments, Dpi = dpi, OutputExtension = outputExtension };
        var destinationType = ContentTypeNames.Resolve(outputExtension).FirstOrDefault() ?? outputExtension;
        return RunHandlerAsync(capture, new ExternalToolConversionHandler(tool, _temp), destinationType, outputExtension, cancellationToken);
    }

    private static async Task<StreamConversionResult> RunHandlerAsync(StreamMonitorCapture capture, IDocumentConversionHandler handler, string destinationType, string outputExtension, CancellationToken cancellationToken)
    {
        if (capture.SavedPath is null)
        {
            return new StreamConversionResult(false, null, "This capture was never saved to a file, so there is nowhere to write the converted output next to.");
        }

        var outputPath = OutputPathFor(capture, outputExtension)!;
        try
        {
            using var source = new MemoryStream(capture.Capture.Data.ToArray());
            using var destination = new MemoryStream();
            await handler.ConvertAsync(source, SourceContentType(capture.Capture.Kind), destination, destinationType).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await File.WriteAllBytesAsync(outputPath, destination.ToArray(), cancellationToken).ConfigureAwait(false);
            return new StreamConversionResult(true, outputPath, null);
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return new StreamConversionResult(false, null, ex.Message);
        }
    }

    private static string SourceContentType(StreamContentKind kind) => kind.Format switch
    {
        StreamContentFormat.Hpgl => HpglToSvgConversionHandler.HpglContentType,
        StreamContentFormat.PostScript => "application/postscript",
        StreamContentFormat.Pcl => "application/vnd.hp-pcl",
        _ => ContentTypeNames.Resolve(kind.Extension).FirstOrDefault() ?? "application/octet-stream",
    };

    /// <summary>The converted file's path: the saved capture's own directory and file-name stem, with <paramref name="extension"/> - or null when the capture was never saved.</summary>
    private static string? OutputPathFor(StreamMonitorCapture capture, string extension) =>
        capture.SavedPath is { } savedPath
            ? Path.ChangeExtension(savedPath, extension)
            : null;
}
