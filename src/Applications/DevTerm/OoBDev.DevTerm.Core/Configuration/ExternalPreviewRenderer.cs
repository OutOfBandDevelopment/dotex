using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Documents.ExternalTools;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Draws a PostScript or PCL capture for the Stream Monitor preview by running Ghostscript or GhostPCL as an external
/// process when one is installed (owner decision Q4, 2026-11-09). Nothing is bundled; a missing tool is reported, never an error.
/// The tool runs with <c>-dSAFER</c> and is killed when it does not finish within <see cref="Timeout"/>.
/// </summary>
public sealed class ExternalPreviewRenderer
{
    private static readonly ITempFileFactory _temp = new TempFileFactory(new ServiceCollection().BuildServiceProvider());

    private readonly IReadOnlyList<string> _searchDirectories;

    /// <summary>Creates a renderer that looks for the tools in <paramref name="searchDirectories"/>, or on PATH and under the Ghostscript install folders when null.</summary>
    public ExternalPreviewRenderer(IEnumerable<string>? searchDirectories = null) =>
        _searchDirectories = [.. searchDirectories ?? DefaultSearchDirectories()];

    /// <summary>How long a render may take before it is abandoned.</summary>
    public TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(20);

    /// <summary>Resolution of the preview picture.</summary>
    public int Dpi { get; init; } = 100;

    /// <summary>Whether a capture of this kind could be drawn at all (PostScript or PCL).</summary>
    public static bool Handles(StreamContentKind kind) => kind.Format is StreamContentFormat.PostScript or StreamContentFormat.Pcl;

    /// <summary>The installed tool for <paramref name="kind"/>, or null.</summary>
    public ExternalToolOptions? FindTool(StreamContentKind kind)
    {
        var names = kind.Format switch
        {
            StreamContentFormat.PostScript => new[] { "gswin64c.exe", "gswin32c.exe", "gs" },
            StreamContentFormat.Pcl => ["gpcl6win64.exe", "gpcl6win32.exe", "gpcl6"],
            _ => Array.Empty<string>(),
        };
        foreach (var directory in _searchDirectories)
        {
            foreach (var name in names)
            {
                var path = Path.Combine(directory, name);
                if (File.Exists(path))
                {
                    return new ExternalToolOptions
                    {
                        Name = Path.GetFileNameWithoutExtension(path),
                        Path = path,
                        Arguments = "-dBATCH -dNOPAUSE -dSAFER -sDEVICE=png16m -r{dpi} -sOutputFile={output} {input}",
                        OutputExtension = "png",
                        Dpi = Dpi,
                    };
                }
            }
        }

        return null;
    }

    /// <summary>Renders <paramref name="capture"/> to PNG bytes; <c>Error</c> says why not when the bytes are null.</summary>
    public async Task<(byte[]? Png, string? Error)> RenderAsync(StreamMonitorCapture capture, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(capture);
        var kind = capture.Capture.Kind;
        if (!Handles(kind))
        {
            return (null, $"No external preview for {kind.DisplayName}.");
        }

        var tool = FindTool(kind);
        if (tool is null)
        {
            var wanted = kind.Format == StreamContentFormat.PostScript ? "Ghostscript" : "GhostPCL";
            return (null, $"Install {wanted} to preview {kind.DisplayName} here; the captured bytes were saved as-is.");
        }

        try
        {
            using var source = new MemoryStream(capture.Capture.Data, writable: false);
            using var destination = new MemoryStream();
            var handler = new ExternalToolConversionHandler(tool, _temp);
            await handler.ConvertAsync(source, StreamCaptureConverter.SourceContentType(kind), destination, "image/png")
                .WaitAsync(Timeout, cancellationToken).ConfigureAwait(false);
            return destination.Length == 0 ? (null, $"{tool.Name} produced no picture.") : (destination.ToArray(), null);
        }
        catch (TimeoutException)
        {
            return (null, $"{tool.Name} took longer than {Timeout.TotalSeconds:0} seconds and was abandoned.");
        }
        catch (Exception ex) when (ex is InvalidOperationException or NotSupportedException or IOException or UnauthorizedAccessException)
        {
            return (null, $"{tool.Name} failed: {ex.Message}");
        }
    }

    private static IEnumerable<string> DefaultSearchDirectories()
    {
        foreach (var directory in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty).Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            yield return directory;
        }

        foreach (var root in new[] { Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) }.Where(r => r.Length > 0))
        {
            foreach (var family in new[] { "gs", "gpcl6" })
            {
                var folder = Path.Combine(root, family);
                if (!Directory.Exists(folder))
                {
                    continue;
                }

                foreach (var version in Directory.EnumerateDirectories(folder).OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase))
                {
                    yield return Path.Combine(version, "bin");
                }
            }
        }
    }
}
