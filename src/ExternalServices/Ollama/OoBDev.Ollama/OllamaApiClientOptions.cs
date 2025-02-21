namespace OoBDev.Ollama;

/// <summary>
/// Represents the configuration options for the Ollama API client.
/// </summary>
public record OllamaApiClientOptions
{
    /// <summary>
    /// Gets or initializes the URL of the Ollama API.
    /// </summary>
    public required string Url { get; init; }

    /// <summary>
    /// Gets or initializes the default model to use with the Ollama API.
    /// </summary>
    public required string DefaultModel { get; init; }
}
