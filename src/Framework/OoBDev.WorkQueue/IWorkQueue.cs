using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.WorkQueue;

/// <summary>
/// A queue whose unit of work is a visible row: status, attempts, lease and retry time can be read by a screen and by SQL.
/// Every call after a claim matches the attempt and the worker, so a worker that lost its lease cannot overwrite the new owner.
/// </summary>
public interface IWorkQueue
{
    /// <summary>Adds an item to a stage; nothing happens when the item already exists.</summary>
    /// <param name="stage">The stage name.</param>
    /// <param name="key">The item key.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when the item was added.</returns>
    Task<bool> EnqueueAsync(string stage, string key, CancellationToken cancellationToken = default);

    /// <summary>Claims up to <paramref name="batch"/> items for a worker; items that used up their attempts are dead-lettered first.</summary>
    /// <param name="stage">The stage name.</param>
    /// <param name="worker">The worker name.</param>
    /// <param name="batch">The most items to claim.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The claimed items, oldest first.</returns>
    Task<IReadOnlyList<WorkItem>> ClaimAsync(string stage, string worker, int batch, CancellationToken cancellationToken = default);

    /// <summary>Extends the lease of a claim.</summary>
    /// <param name="item">The claim.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>False when the lease was lost; the worker must stop.</returns>
    Task<bool> HeartbeatAsync(WorkItem item, CancellationToken cancellationToken = default);

    /// <summary>Finishes a claim with <see cref="WorkStatus.Done"/> or <see cref="WorkStatus.Skipped"/>.</summary>
    /// <param name="item">The claim.</param>
    /// <param name="status">The final status.</param>
    /// <param name="detail">An optional note kept on the row.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>False and no change when the lease was lost.</returns>
    Task<bool> CompleteAsync(WorkItem item, string status = WorkStatus.Done, string? detail = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Records a failed attempt. A retryable failure with attempts left returns the item to pending after a back-off;
    /// otherwise the item is dead-lettered.
    /// </summary>
    /// <param name="item">The claim.</param>
    /// <param name="error">The error text (kept to 500 characters).</param>
    /// <param name="retryable">False when retrying cannot help.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The resulting status (<see cref="WorkStatus.Pending"/> or <see cref="WorkStatus.Dead"/>), or null when the lease was lost.</returns>
    Task<string?> FailAsync(WorkItem item, string error, bool retryable = true, CancellationToken cancellationToken = default);

    /// <summary>Puts dead and parked items of a stage (or one item) back to pending with a fresh attempt budget.</summary>
    /// <param name="stage">The stage name.</param>
    /// <param name="key">One item, or null for the whole stage.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>How many items were returned.</returns>
    Task<int> RedriveAsync(string stage, string? key = null, CancellationToken cancellationToken = default);

    /// <summary>The counts of a stage.</summary>
    /// <param name="stage">The stage name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The counts.</returns>
    Task<StageStats> StatsAsync(string stage, CancellationToken cancellationToken = default);

    /// <summary>The dead-letter records of a stage, newest first.</summary>
    /// <param name="stage">The stage name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The records.</returns>
    Task<IReadOnlyList<DeadLetter>> DeadLettersAsync(string stage, CancellationToken cancellationToken = default);
}
