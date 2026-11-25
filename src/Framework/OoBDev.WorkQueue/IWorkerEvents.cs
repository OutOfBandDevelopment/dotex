using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WorkQueue;

/// <summary>
/// The history of worker progress events: workers publish, each reader (a screen, a log tail) keeps its own high-water mark,
/// and the retention trims by count and age.
/// </summary>
public interface IWorkerEvents
{
    /// <summary>Appends an event. A failure to publish never fails the worker, so callers may ignore the result.</summary>
    /// <param name="source">Who publishes (a stage or worker name).</param>
    /// <param name="message">The event text.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the event was stored.</returns>
    Task<bool> PublishAsync(string source, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the events after the reader's mark, oldest first, and moves the mark to the last one returned.
    /// A new reader starts at the current end unless <paramref name="fromStart"/> is set.
    /// </summary>
    /// <param name="reader">The reader name.</param>
    /// <param name="limit">The most events to return.</param>
    /// <param name="fromStart">True to start a new reader at the oldest kept event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The events, possibly none.</returns>
    Task<IReadOnlyList<WorkerEvent>> ReadAsync(string reader, int limit, bool fromStart = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes events beyond <see cref="WorkerEventsOptions.MaxRecords"/> or older than <see cref="WorkerEventsOptions.MaxAge"/>,
    /// and readers idle for longer than the age (a returning reader restarts at the end).
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How many events were deleted.</returns>
    Task<int> TrimAsync(CancellationToken cancellationToken = default);
}
