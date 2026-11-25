using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// The durable, append-only history of events. Selected by the configured key <c>OoBDev:EventSourcing:EventStore:Type</c>.
/// </summary>
public interface IEventStore
{
    /// <summary>
    /// Appends events to a stream if the stream's last sequence equals <paramref name="expectedSequence"/>.
    /// </summary>
    /// <param name="streamId">The stream to append to.</param>
    /// <param name="expectedSequence">The last sequence the caller has seen; zero for a new stream.</param>
    /// <param name="events">The events to append, in order.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result that reports a conflict instead of throwing.</returns>
    Task<AppendResult> AppendAsync(string streamId, long expectedSequence, IReadOnlyList<NewEvent> events, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads one stream in sequence order.
    /// </summary>
    /// <param name="streamId">The stream to read.</param>
    /// <param name="fromSequence">The first sequence to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IReadOnlyList<EventRecord>> ReadAsync(string streamId, long fromSequence = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads the global feed in position order.
    /// </summary>
    /// <param name="afterPosition">Only events with a greater position are returned.</param>
    /// <param name="maxCount">The most events to return.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task<IReadOnlyList<EventRecord>> ReadAllAsync(long afterPosition, int maxCount = 500, CancellationToken cancellationToken = default);
}
