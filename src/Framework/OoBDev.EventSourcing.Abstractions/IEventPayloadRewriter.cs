using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// The only way to change a stored payload (GDPR erasure). Kept apart from <see cref="IEventStore"/> so that ordinary code
/// cannot reach it; only the erasure compaction job uses it. Id, sequence, type and timestamp never change.
/// </summary>
public interface IEventPayloadRewriter
{
    /// <summary>
    /// Replaces the payloads of events of one stream in a single transaction.
    /// </summary>
    /// <param name="streamId">The stream the events belong to.</param>
    /// <param name="payloads">The new payload by sequence.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The number of events changed.</returns>
    Task<int> RewritePayloadsAsync(string streamId, IReadOnlyDictionary<long, string> payloads, CancellationToken cancellationToken = default);
}
