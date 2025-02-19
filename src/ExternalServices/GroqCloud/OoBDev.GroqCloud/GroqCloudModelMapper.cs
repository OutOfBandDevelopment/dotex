using OoBDev.AI.Models;
using GroqNet.ChatCompletions;
using System.Linq;

namespace OoBDev.GroqCloud;

/// <summary>
/// Represents a mapper interface for llama models.
/// </summary>
public class GroqCloudModelMapper : IGroqCloudModelMapper
{
    /// <summary>
    /// Maps a <see cref="CompletionRequest"/> model to a <see cref="GroqChatHistory"/> object.
    /// </summary>
    /// <param name="request">The completion request model to map.</param>
    /// <returns>A generated completion request object.</returns>
    public GroqChatHistory Map(CompletionRequest request) => request.System switch
    {
        string system when !string.IsNullOrWhiteSpace(system) => new(system) { new(request.Prompt) },
        _ => new() { new(request.Prompt) },
    };

    /// <summary>
    /// Maps a <see cref="GroqChatCompletions"/> object to a <see cref="CompletionResponse"/> object.
    /// </summary>
    /// <param name="response">The conversation context with response to map.</param>
    /// <returns>A completion response object.</returns>
    public CompletionResponse Map(GroqChatCompletions response) => new()
    {
        Context = [], //TODO: I need a way to map this more generic,
        Response = response.Choices.FirstOrDefault()?.Message?.Content ?? "",
    };
}
