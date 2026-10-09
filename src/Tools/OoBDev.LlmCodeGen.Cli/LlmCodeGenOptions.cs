using System.ComponentModel.DataAnnotations;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Options of the LlmCodeGen tool. Provider endpoints and credentials stay in the adapters' own sections.
/// </summary>
public record LlmCodeGenOptions
{
    /// <summary>
    /// The configuration section the options bind from.
    /// </summary>
    public const string SectionName = "LlmCodeGen";

    /// <summary>
    /// Folder scanned recursively; every folder holding matching files becomes one prompt.
    /// </summary>
    [Required]
    public string? InputPath { get; init; }

    /// <summary>
    /// Folder the prompts, responses and extracted files are written to.
    /// </summary>
    [Required]
    public string? OutputPath { get; init; }

    /// <summary>
    /// Bundled template name (for example <c>documentation</c>) or the path of a Handlebars file.
    /// </summary>
    [Required]
    public string? Template { get; init; }

    /// <summary>
    /// Keyed <c>IMessageCompletion</c> provider: <c>ollama</c> or <c>groq-cloud</c> (alias <c>groq</c>).
    /// </summary>
    [Required]
    public string Provider { get; init; } = "ollama";

    /// <summary>
    /// Model name sent with each prompt.
    /// </summary>
    [Required]
    public string? Model { get; init; }

    /// <summary>
    /// File pattern selecting the files that go into a prompt.
    /// </summary>
    public string SearchPattern { get; init; } = "*.*";

    /// <summary>
    /// Reruns folders that already have a response.
    /// </summary>
    public bool Overwrite { get; init; }

    /// <summary>
    /// Writes the files found in fenced blocks of the response beside the response.
    /// </summary>
    public bool ExtractFiles { get; init; } = true;
}
