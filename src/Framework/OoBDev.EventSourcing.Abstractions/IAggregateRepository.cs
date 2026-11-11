using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Loads and saves one kind of aggregate through the event store.
/// </summary>
/// <typeparam name="T">The aggregate type.</typeparam>
public interface IAggregateRepository<T> where T : AggregateRoot, new()
{
    /// <summary>
    /// Rebuilds an aggregate from its events.
    /// </summary>
    /// <param name="id">The aggregate identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The aggregate, or <see langword="null"/> when it has no events.</returns>
    Task<T?> LoadAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends the uncommitted events of the aggregate.
    /// </summary>
    /// <param name="aggregate">The aggregate.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <exception cref="EventStreamConflictException">Another writer saved events to the same aggregate first.</exception>
    Task SaveAsync(T aggregate, CancellationToken cancellationToken = default);
}
