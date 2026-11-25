namespace OoBDev.Documents.ExternalTools;

/// <summary>One registered external conversion tool (Ghostscript, GhostPCL and similar).</summary>
public sealed class ExternalToolOptions
{
    /// <summary>Shown in lists and used to pick the tool by name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>The executable.</summary>
    public string? Path { get; set; }

    /// <summary>Argument template; <c>{input}</c>, <c>{output}</c> and <c>{dpi}</c> are substituted per token.</summary>
    public string Arguments { get; set; } = string.Empty;

    /// <summary>Comma-separated formats it accepts (<c>ps</c>, <c>pcl</c>, <c>hpgl</c>, <c>image</c> or an extension such as <c>bmp</c>); empty means any.</summary>
    public string Formats { get; set; } = string.Empty;

    /// <summary>Extension (no dot) of the file the tool writes.</summary>
    public string OutputExtension { get; set; } = "png";

    /// <summary>The value substituted for <c>{dpi}</c>.</summary>
    public int Dpi { get; set; } = 150;

    /// <summary>A copy, so an editor can change a tool without touching the options it was loaded from.</summary>
    /// <param name="tool">The tool to copy.</param>
    /// <returns>The copy.</returns>
    public static ExternalToolOptions Clone(ExternalToolOptions tool)
    {
        global::System.ArgumentNullException.ThrowIfNull(tool);
        return new()
        {
            Name = tool.Name,
            Path = tool.Path,
            Arguments = tool.Arguments,
            Formats = tool.Formats,
            OutputExtension = tool.OutputExtension,
            Dpi = tool.Dpi,
        };
    }
}
