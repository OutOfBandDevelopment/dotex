using OoBDev.Documents.ExternalTools;
using System;
using System.Collections.Generic;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// <see cref="StreamCaptureConverter"/>'s configuration - bound from <see cref="CliOptions"/>'s
/// <c>StreamConvert*</c> properties via <see cref="FromCliOptions"/>/<see cref="CopyFrom"/> so a
/// saved connection profile carries its Stream Monitor conversion settings the same way every other
/// per-connection setting does.
/// </summary>
public sealed class StreamCaptureConverterOptions
{
    public StreamConversionMode Mode { get; set; } = StreamConversionMode.None;

    /// <summary>The registered tool run when <see cref="Mode"/> is <see cref="StreamConversionMode.Tool"/>.</summary>
    public string? ToolName { get; set; }

    /// <summary>Registered converter tools - see docs/design/StreamTools/stream-converter-tools.md.</summary>
    public List<ExternalToolOptions> Tools { get; set; } = [];

    public string? ExternalToolPath { get; set; }

    public string ExternalToolArguments { get; set; } = string.Empty;

    public int ExternalToolDpi { get; set; } = 150;

    public string? OutputExtension { get; set; }

    /// <summary>Builds a fresh <see cref="StreamCaptureConverterOptions"/> from <paramref name="cliOptions"/> (or all-defaults/<see cref="StreamConversionMode.None"/> when null) plus <paramref name="globalTools"/>, the app-wide list the caller loaded from <see cref="ConverterToolsStore"/> (not read here, so tests never touch the user's own file) - for ad hoc construction outside DI (TUI/WPF, which build <see cref="StreamMonitor"/> the same ad hoc way).</summary>
    public static StreamCaptureConverterOptions FromCliOptions(CliOptions? cliOptions, IEnumerable<ExternalToolOptions>? globalTools = null)
    {
        var options = new StreamCaptureConverterOptions();
        if (cliOptions is not null)
        {
            CopyFrom(cliOptions, options);
        }

        // The app-wide tools (ConverterToolsStore) come first; a profile's own, older ones fill in any names they lack.
        options.Tools = ConverterToolsStore.Merge(globalTools, options.Tools);
        return options;
    }

    /// <summary>Copies <paramref name="cliOptions"/>'s <c>StreamConvert*</c> properties onto <paramref name="target"/> - what <see cref="ServiceCollectionExtensions.AddDevTermFrontEnd"/> configures the DI-registered options instance with.</summary>
    public static void CopyFrom(CliOptions cliOptions, StreamCaptureConverterOptions target)
    {
        ArgumentNullException.ThrowIfNull(cliOptions);
        ArgumentNullException.ThrowIfNull(target);

        target.Mode = ParseMode(cliOptions.StreamConvertMode);
        target.ToolName = ToolNameOf(cliOptions.StreamConvertMode);
        target.Tools = [.. cliOptions.StreamConvertTools];
        target.ExternalToolPath = cliOptions.StreamConvertExternalToolPath;
        target.ExternalToolArguments = cliOptions.StreamConvertExternalToolArguments ?? string.Empty;
        target.ExternalToolDpi = cliOptions.StreamConvertDpi;
        target.OutputExtension = cliOptions.StreamConvertOutputExtension;
    }

    private static string? ToolNameOf(string? raw) =>
        raw?.Trim() is { } text && text.StartsWith("tool:", StringComparison.OrdinalIgnoreCase) ? text["tool:".Length..].Trim() : null;

    private static StreamConversionMode ParseMode(string? raw) => raw?.Trim().ToLowerInvariant() switch
    {
        "auto" => StreamConversionMode.Auto,
        { } text when text.StartsWith("tool:", StringComparison.Ordinal) => StreamConversionMode.Tool,
        "externaltool" => StreamConversionMode.ExternalTool,
        "internalhpgltosvg" => StreamConversionMode.InternalHpglToSvg,
        _ => StreamConversionMode.None,
    };
}
