using OllamaSharp.Models;
using OoBDev.AI.Models;

namespace OoBDev.Ollama;

/// <summary>
/// Maps framework completion models to Ollama requests.
/// </summary>
public interface IOllamaModelMapper
{
    /// <summary>
    /// Maps a framework completion request to an Ollama generate request.
    /// </summary>
    /// <param name="request">The framework request.</param>
    /// <param name="defaultModel">The model to use when the request names none.</param>
    /// <returns>The Ollama request.</returns>
    GenerateRequest Map(CompletionRequest request, string defaultModel);
}
