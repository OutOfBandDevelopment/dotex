using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One entry in the conversion list: a mode, for <see cref="StreamConversionMode.Tool"/> which registered tool, and for the internal HP-GL conversion the picture format (null is SVG).</summary>
public sealed record StreamConversionChoice(StreamConversionMode Mode, string? ToolName, string DisplayName, string? Extension = null)
{
    /// <summary>The conversion list for <paramref name="options"/>: the fixed modes, plus Auto and one entry per registered tool when any are registered.</summary>
    public static IReadOnlyList<StreamConversionChoice> For(StreamCaptureConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var choices = new List<StreamConversionChoice>();
        foreach (var mode in StreamConversionModes.All)
        {
            choices.Add(new StreamConversionChoice(mode, null, StreamConversionModes.DisplayName(mode)));
            if (mode == StreamConversionMode.InternalHpglToSvg)
            {
                choices.Add(new StreamConversionChoice(mode, null, "HP-GL to PNG", "png"));
                choices.Add(new StreamConversionChoice(mode, null, "HP-GL to JPG", "jpg"));
            }

            if (mode == StreamConversionMode.InternalHpglToSvg && options.Tools.Count > 0)
            {
                choices.Add(new StreamConversionChoice(StreamConversionMode.Auto, null, StreamConversionModes.DisplayName(StreamConversionMode.Auto)));
                choices.AddRange(options.Tools.Select(t => new StreamConversionChoice(StreamConversionMode.Tool, t.Name, t.Name)));
            }
        }

        return choices;
    }

    /// <summary>Index of the choice matching <paramref name="options"/>' current mode and tool, or 0.</summary>
    public static int IndexOf(IReadOnlyList<StreamConversionChoice> choices, StreamCaptureConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(choices);
        ArgumentNullException.ThrowIfNull(options);
        var tool = options.Mode == StreamConversionMode.Tool ? options.ToolName : null;
        for (var i = 0; i < choices.Count; i++)
        {
            if (choices[i].Mode == options.Mode && string.Equals(choices[i].ToolName, tool, StringComparison.OrdinalIgnoreCase) && SameFormat(choices[i], options))
            {
                return i;
            }
        }

        return 0;
    }

    private static bool SameFormat(StreamConversionChoice choice, StreamCaptureConverterOptions options) =>
        choice.Mode != StreamConversionMode.InternalHpglToSvg || string.Equals(choice.Extension, options.OutputExtension is "jpeg" ? "jpg" : options.OutputExtension is "svg" ? null : options.OutputExtension, StringComparison.OrdinalIgnoreCase);

    /// <summary>Selects this choice on <paramref name="options"/>.</summary>
    public void ApplyTo(StreamCaptureConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Mode = Mode;
        options.ToolName = ToolName;
        if (Mode == StreamConversionMode.InternalHpglToSvg)
        {
            options.OutputExtension = Extension;
        }
    }
}
