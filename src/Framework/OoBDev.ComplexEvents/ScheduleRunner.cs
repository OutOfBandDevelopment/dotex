using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Leases due items and runs a callback for each; a throwing callback leaves the lease to expire (an attempt is counted) and is reported to the optional failure handler.</summary>
/// <param name="store">The store.</param>
/// <param name="time">The clock.</param>
/// <param name="run">Runs one item (deserialize by <see cref="ScheduleItem.EventType"/> and dispatch).</param>
/// <param name="lease">Lease length.</param>
/// <param name="onFailure">Told about each failed attempt, for example to dead-letter the item.</param>
public sealed class ScheduleRunner(IScheduleStore store, TimeProvider time, Func<ScheduleItem, CancellationToken, Task> run, TimeSpan lease, IScheduleFailureHandler? onFailure = null)
{
    /// <summary>Runs everything due now; returns how many completed.</summary>
    /// <param name="max">Most items to lease.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    public async Task<int> RunDueAsync(int max, CancellationToken cancellationToken)
    {
        var done = 0;
        foreach (var item in await store.LeaseDueAsync(time.GetUtcNow(), max, lease, cancellationToken).ConfigureAwait(false))
        {
            try
            {
                await run(item, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                await ReportAsync(item, ex, cancellationToken).ConfigureAwait(false);
                continue;
            }

            await store.CompleteAsync(item.Id, cancellationToken).ConfigureAwait(false);
            done++;
        }

        return done;
    }

    private async Task ReportAsync(ScheduleItem item, Exception exception, CancellationToken cancellationToken)
    {
        if (onFailure is null)
        {
            return;
        }

        try
        {
            await onFailure.OnFailedAsync(item, exception, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
        }
    }
}
