using OoBDev.System.Configuration;

namespace OoBDev.System.Text.Templating;

/// <summary>
/// Configuration settings for file templating engine
/// </summary>
public class FileTemplatingOptions
{
    /// <summary>
    /// template source path
    /// </summary>
    [CommandParameter(Value = "file-template-path")]
    public string TemplatePath { get; set; } = "./";

    /// <summary>
    /// sandbox root path
    /// </summary>
    [CommandParameter(Value = "file-template-sandbox")]
    public string? SandboxPath { get; set; } = null;

    /// <summary>
    /// template priority
    /// </summary>
    [CommandParameter(Value = "file-template-priority")]
    public int Priority { get; set; } = 100;
}
