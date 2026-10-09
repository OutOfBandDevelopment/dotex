namespace OoBDev.GroqCloud;

/// <summary>
/// Configuration options for the Groq Cloud API client.
/// </summary>
public record GroqCloudApiClientOptions
{
    /// <summary>
    /// The API key used to authenticate with the Groq Cloud service.
    /// </summary>
    public string? ApiKey { get; init; }

    /// <summary>
    /// The model identifier for the AI model to be used. Defaults to "openai/gpt-oss-20b".
    /// </summary>
    public string Model { get; init; } = "openai/gpt-oss-20b";
}
