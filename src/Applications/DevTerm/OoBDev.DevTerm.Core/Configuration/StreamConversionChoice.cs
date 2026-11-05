using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One entry in the conversion list: a mode, and for <see cref="StreamConversionMode.Tool"/> which registered tool.</summary>
public sealed record StreamConversionChoice(StreamConversionMode Mode, string? ToolName, string DisplayName)
{
    /// <summary>The conversion list for <paramref name="options"/>: the fixed modes, plus Auto and one entry per registered tool when any are registered.</summary>
    public static IReadOnlyList<StreamConversionChoice> For(StreamCaptureConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        var choices = new List<StreamConversionChoice>();
        foreach (var mode in StreamConversionModes.All)
        {
            choices.Add(new StreamConversionChoice(mode, null, StreamConversionModes.DisplayName(mode)));
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
            if (choices[i].Mode == options.Mode && string.Equals(choices[i].ToolName, tool, StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return 0;
    }

    /// <summary>Selects this choice on <paramref name="options"/>.</summary>
    public void ApplyTo(StreamCaptureConverterOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Mode = Mode;
        options.ToolName = ToolName;
    }
}
