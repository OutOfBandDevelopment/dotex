using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WorkQueue;

/// <summary>
/// The work of one stage. Outputs must be keyed by the item and written by upsert, because delivery is at least once.
/// </summary>
public interface IWorkStageHandler
{
    /// <summary>Works one claimed item. Return normally to complete it; throw to record a failed attempt.</summary>
    /// <param name="item">The claim.</param>
    /// <param name="cancellationToken">Cancelled on shutdown and when the lease is lost.</param>
    /// <returns>A task that completes when the item is done.</returns>
    Task HandleAsync(WorkItem item, CancellationToken cancellationToken);
}
