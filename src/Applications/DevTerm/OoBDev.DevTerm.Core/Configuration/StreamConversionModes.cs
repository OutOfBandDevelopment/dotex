using System.Collections.Generic;

namespace OoBDev.DevTerm.Configuration;

/// <summary>The names the Stream Monitor windows show for each <see cref="StreamConversionMode"/>, in the order they are offered.</summary>
public static class StreamConversionModes
{
    public static IReadOnlyList<StreamConversionMode> All { get; } =
        [StreamConversionMode.None, StreamConversionMode.InternalHpglToSvg, StreamConversionMode.ExternalTool];

    public static string DisplayName(StreamConversionMode mode) => mode switch
    {
        StreamConversionMode.InternalHpglToSvg => "HP-GL to SVG",
        StreamConversionMode.ExternalTool => "External tool",
        StreamConversionMode.Auto => "Auto (by format)",
        _ => "None",
    };
}
