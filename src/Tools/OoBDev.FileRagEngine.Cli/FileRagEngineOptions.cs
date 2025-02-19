using OoBDev.System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.FileRagEngine.Cli;

/// <summary>
/// Represents options for configuring a template engine.
/// </summary>
public class FileRagEngineOptions
{
    /// <summary>
    /// Gets or sets the path to the input path.
    /// </summary>
    [CommandParameter(Value = "input")]
    public string? InputPath { get; set; }

    /// <summary>
    /// Gets or sets the template content.
    /// </summary>
    [Required]
    public required string Template { get; set; }

    /// <summary>
    /// Gets or sets the path to the output file.
    /// </summary>
    [CommandParameter(Value = "output")]
    public string? OutputPath { get; set; }

    /// <summary>
    /// Gets or sets the selected LanguageModelType
    /// </summary>
    [CommandParameter(Value = "llm")]
    public string? LanguageModelType { get; set; }

    /// <summary>
    /// Gets or sets the IncludeRawOutput
    /// </summary>
    [CommandParameter(Value = "raw")]
    public bool IncludeRawOutput { get; set; }

    /// <summary>
    /// Gets or sets the IncludePrompt
    /// </summary>
    [CommandParameter(Value = "include-prompt")]
    public bool IncludePrompt { get; set; }
}
