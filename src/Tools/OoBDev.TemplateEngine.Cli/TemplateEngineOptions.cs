using OoBDev.System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.TemplateEngine.Cli;

/// <summary>
/// Represents options for configuring a template engine.
/// </summary>
public class TemplateEngineOptions
{
    /// <summary>
    /// Gets or sets the path to the input file.
    /// </summary>
    [CommandParameter(Value = "input")]
    public string? InputFile { get; set; }

    /// <summary>
    /// Gets or sets the template content.
    /// </summary>
    [Required]
    public required string Template { get; set; }

    /// <summary>
    /// Gets or sets the type of the input file.
    /// </summary>
    [CommandParameter(Value = "input-type")]
    public FileTypes? InputFileType { get; set; }

    /// <summary>
    /// Gets or sets the path to the output file.
    /// </summary>
    [CommandParameter(Value = "output")]
    public string? OutputFile { get; set; }
}
