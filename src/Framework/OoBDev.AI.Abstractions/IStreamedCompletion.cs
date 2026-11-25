using System.Collections.Generic;
using System.Threading;

namespace OoBDev.AI;

/// <summary>
/// A completion source that can return its answer as it is generated.
/// </summary>
public interface IStreamedCompletion
{
    /// <summary>
    /// Streams a response to the user input, constrained by the instructions.
    /// </summary>
    /// <param name="promptDetails">The instructions (system prompt).</param>
    /// <param name="userInput">The user's prompt.</param>
    /// <param name="cancellationToken">Cancels the stream.</param>
    /// <returns>The response in the pieces the model produces.</returns>
    IAsyncEnumerable<string> GetStreamedResponseAsync(string promptDetails, string userInput, CancellationToken cancellationToken = default);
}
