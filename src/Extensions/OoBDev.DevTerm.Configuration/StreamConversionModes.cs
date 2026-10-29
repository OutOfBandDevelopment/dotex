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
