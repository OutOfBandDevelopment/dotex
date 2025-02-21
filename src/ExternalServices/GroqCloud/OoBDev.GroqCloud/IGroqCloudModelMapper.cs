using OoBDev.AI.Models;
using GroqNet.ChatCompletions;

namespace OoBDev.GroqCloud;

/// <summary>
/// Represents a mapper interface for llama models.
/// </summary>
public interface IGroqCloudModelMapper
{
    /// <summary>
    /// Maps a <see cref="CompletionRequest"/> model to a <see cref="GroqChatHistory"/> object.
    /// </summary>
    /// <param name="request">The completion request model to map.</param>
    /// <returns>A generated completion request object.</returns>
    GroqChatHistory Map(CompletionRequest request);

    /// <summary>
    /// Maps a <see cref="GroqChatCompletions"/> object to a <see cref="CompletionResponse"/> object.
    /// </summary>
    /// <param name="response">The conversation context with response to map.</param>
    /// <returns>A completion response object.</returns>
    CompletionResponse Map(GroqChatCompletions response);
}
