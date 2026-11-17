using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace OoBDev.AI;

/// <summary>
/// Decorates an <see cref="IStreamedCompletion"/>; the call is recorded when the stream ends, with the estimate taken over the chunks received.
/// A stream the caller abandons records what arrived so far.
/// </summary>
/// <param name="inner">The streaming service that does the work.</param>
/// <param name="meter">Where the usage is added up.</param>
public sealed class EstimatingStreamedCompletion(IStreamedCompletion inner, LlmUsageMeter meter) : IStreamedCompletion
{
    /// <inheritdoc />
    public async IAsyncEnumerable<string> GetStreamedResponseAsync(string promptDetails, string userInput, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var received = new StringBuilder();
        try
        {
            await foreach (var chunk in inner.GetStreamedResponseAsync(promptDetails, userInput, cancellationToken))
            {
                received.Append(chunk);
                yield return chunk;
            }
        }
        finally
        {
            meter.Record(null, string.Concat(promptDetails, userInput), received.ToString());
        }
    }
}
