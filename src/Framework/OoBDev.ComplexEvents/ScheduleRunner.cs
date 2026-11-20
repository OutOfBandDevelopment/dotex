using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Leases due items and runs a callback for each; a throwing callback leaves the lease to expire (an attempt is counted).</summary>
/// <param name="store">The store.</param>
/// <param name="time">The clock.</param>
/// <param name="run">Runs one item (deserialize by <see cref="ScheduleItem.EventType"/> and dispatch).</param>
/// <param name="lease">Lease length.</param>
public sealed class ScheduleRunner(IScheduleStore store, TimeProvider time, Func<ScheduleItem, CancellationToken, Task> run, TimeSpan lease)
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
                continue;
            }

            await store.CompleteAsync(item.Id, cancellationToken).ConfigureAwait(false);
            done++;
        }

        return done;
    }
}
