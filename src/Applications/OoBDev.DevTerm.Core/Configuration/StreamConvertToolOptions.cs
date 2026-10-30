using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm;

namespace OoBDev.DevTerm.Configuration;

/// <summary>One registered Stream Monitor converter tool - see <see cref="CliOptions.StreamConvertTools"/>.</summary>
public sealed class StreamConvertToolOptions
{
    /// <summary>Shown in the conversion list and used by <c>tool:Name</c>.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The executable.</summary>
    public string? Path { get; set; }

    /// <summary>Argument template; <c>{input}</c>, <c>{output}</c>, <c>{dpi}</c> are substituted per token.</summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>Comma-separated capture formats it accepts (<c>ps</c>, <c>pcl</c>, <c>hpgl</c>, <c>image</c> or an extension such as <c>bmp</c>); empty means any.</summary>
    public string Formats { get; set; } = string.Empty;

    /// <summary>Extension (no dot) of the file the tool writes.</summary>
    public string OutputExtension { get; set; } = "png";

    /// <summary>The value substituted for <c>{dpi}</c>.</summary>
    public int Dpi { get; set; } = 150;

    /// <summary>A copy, so an editor can change a tool without touching the options it was loaded from.</summary>
    public static StreamConvertToolOptions Clone(StreamConvertToolOptions tool) => new()
    {
        Name = tool.Name,
        Path = tool.Path,
        Arguments = tool.Arguments,
        Formats = tool.Formats,
        OutputExtension = tool.OutputExtension,
        Dpi = tool.Dpi,
    };
}
