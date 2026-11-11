using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Builds one read model from the events. It must be idempotent, because a crash between applying an event and saving the
/// checkpoint delivers that event again.
/// </summary>
public interface IProjection
{
    /// <summary>
    /// Gets the projection name (also its checkpoint key).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Updates the read model for one event.
    /// </summary>
    /// <param name="record">The event.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ApplyAsync(EventRecord record, CancellationToken cancellationToken = default);
}
