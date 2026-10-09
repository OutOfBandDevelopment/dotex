using System.ComponentModel.DataAnnotations;

namespace OoBDev.Ollama;

/// <summary>
/// Represents the configuration options for the Ollama API client.
/// </summary>
public record OllamaApiClientOptions
{
    /// <summary>
    /// Gets or initializes the URL of the Ollama API.
    /// </summary>
    [Required]
    public required string Url { get; init; }

    /// <summary>
    /// Gets or initializes the default model to use with the Ollama API.
    /// </summary>
    [Required]
    public required string DefaultModel { get; init; }

    /// <summary>
    /// Gets or initializes an optional API key sent as a bearer token, for Ollama endpoints behind an authenticating gateway.
    /// </summary>
    public string? ApiKey { get; init; }
}
