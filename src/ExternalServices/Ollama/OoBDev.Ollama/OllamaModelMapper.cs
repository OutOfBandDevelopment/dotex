using OoBDev.AI.Models;
using OllamaSharp;
using OllamaSharp.Models;

namespace OoBDev.Ollama;

/// <summary>
/// Represents a mapper interface for llama models.
/// </summary>
public class OllamaModelMapper : IOllamaModelMapper
{
    /// <summary>
    /// Maps a <see cref="CompletionRequest"/> model to a <see cref="GenerateCompletionRequest"/> object.
    /// </summary>
    /// <param name="request">The completion request model to map.</param>
    /// <returns>A generated completion request object.</returns>
    public GenerateCompletionRequest Map(CompletionRequest request)
    {
        return new()
        {
            Model = request.Model!,
            Context = request.Context!,

            Prompt = request.Prompt!,
            System = request.System!,
            Template = request.Template!,

            Images = request.Images ?? [],
        };
    }

    /// <summary>
    /// Maps a <see cref="ConversationContextWithResponse"/> object to a <see cref="CompletionResponse"/> object.
    /// </summary>
    /// <param name="response">The conversation context with response to map.</param>
    /// <returns>A completion response object.</returns>
    public CompletionResponse Map(ConversationContextWithResponse response) =>
        new()
        {
            Context = response.Context,
            Response = response.Response,
        };
}
