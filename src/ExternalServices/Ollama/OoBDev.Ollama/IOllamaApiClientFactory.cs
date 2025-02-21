using OllamaSharp;

namespace OoBDev.Ollama;

/// <summary>
/// Represents a factory for creating instances of the OllamaApiClient.
/// </summary>
public interface IOllamaApiClientFactory
{
    /// <summary>
    /// Builds an instance of the OllamaApiClient for the specified host.
    /// </summary>
    /// <returns>An instance of the OllamaApiClient.</returns>
    IOllamaApiClient Build();
}
