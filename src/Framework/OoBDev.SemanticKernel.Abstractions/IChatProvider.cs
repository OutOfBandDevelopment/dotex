using System.Threading.Tasks;

namespace OoBDev.SemanticKernel;

/// <summary>
/// Provides chat functionality for single-turn conversational interactions with AI models.
/// </summary>
public interface IChatProvider
{
    /// <summary>
    /// Executes a single-turn chat interaction asynchronously, sending a prompt and receiving a response.
    /// </summary>
    /// <param name="prompt">The user prompt to send to the AI model.</param>
    /// <returns>
    /// A task representing the asynchronous operation, containing the AI's response text,
    /// or <c>null</c> if no response is generated.
    /// </returns>
    Task<string?> OneShotAsync(string prompt);
}
