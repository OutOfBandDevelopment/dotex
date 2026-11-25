using OoBDev.System.Configuration;
using OoBDev.System.Text.Templating.Xslt;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.TemplateEngine.Cli;

/// <summary>
/// Represents options for configuring a template engine.
/// </summary>
public class TemplateEngineOptions
{
    /// <summary>
    /// Gets or sets the path to the input file; a wildcard in the file name selects several files.
    /// </summary>
    [CommandParameter(Value = "input")]
    public string? InputFile { get; set; }

    /// <summary>
    /// Gets or sets the template: a template name for the template engine, or the path of an XSLT file (.xslt or .xsl).
    /// </summary>
    [Required]
    public required string Template { get; set; }

    /// <summary>
    /// Gets or sets the type of the input file. Handlebars templates read <see cref="InputTypes.Json"/> and
    /// <see cref="InputTypes.Xml"/>; XSLT templates read every type of <see cref="InputTypes"/>, and
    /// <see cref="InputTypes.ByExtension"/> is the default.
    /// </summary>
    [CommandParameter(Value = "input-type")]
    public InputTypes? InputFileType { get; set; }

    /// <summary>
    /// Gets or sets the path to the output file; <c>[file]</c> stands for the input file name without extension.
    /// </summary>
    [CommandParameter(Value = "output")]
    public string? OutputFile { get; set; }

    /// <summary>
    /// Gets or sets a wildcard of inputs to skip (XSLT templates only).
    /// </summary>
    [CommandParameter(Value = "exclude")]
    public string? Exclude { get; set; }

    /// <summary>
    /// Gets or sets the folder an XSLT template may read and write; defaults to the folder of the output.
    /// </summary>
    [CommandParameter(Value = "sandbox")]
    public string? Sandbox { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether all inputs are merged into one document before an XSLT transform.
    /// </summary>
    [CommandParameter(Value = "merge")]
    public bool Merge { get; set; }
}
