using OoBDev.System.Configuration;

namespace OoBDev.DocumentConverter.Cli;

/// <summary>
/// Represents options for configuring a template engine.
/// </summary>
public class DocumentConverterOptions
{
    /// <summary>
    /// Gets or sets the path to the input path.
    /// </summary>
    [CommandParameter(Value = "input")]
    public string? InputPath { get; set; }

    /// <summary>
    /// Gets or sets the path to the output file.
    /// </summary>
    [CommandParameter(Value = "output")]
    public string? OutputPath { get; set; }
}
