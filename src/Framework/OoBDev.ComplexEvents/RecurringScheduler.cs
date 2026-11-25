using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>
/// Drives a set of <see cref="RecurringSchedule"/>s: on start it registers what is due since each recorded last run, and after each
/// occurrence runs it records that run and registers the next one.
/// </summary>
/// <param name="lastRuns">Where last-run times are kept.</param>
/// <param name="schedules">The recurring schedules by name.</param>
public sealed class RecurringScheduler(ILastRunStore lastRuns, IReadOnlyDictionary<string, RecurringSchedule> schedules)
{
    /// <summary>Registers missed and upcoming occurrences of every schedule from its recorded last run.</summary>
    /// <param name="cancellationToken">Cancellation.</param>
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var (key, schedule) in schedules)
        {
            var last = await lastRuns.GetAsync(key, cancellationToken).ConfigureAwait(false);
            await schedule.EnsureAsync(last, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>Records a finished item when it belongs to a recurring schedule and registers the occurrence after it.</summary>
    /// <param name="item">The item that ran.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>True when the item belonged to a recurring schedule.</returns>
    public async Task<bool> CompletedAsync(ScheduleItem item, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(item);
        var match = schedules.FirstOrDefault(s => item.Key.StartsWith(s.Key + "@", StringComparison.Ordinal));
        if (match.Value is null)
        {
            return false;
        }

        await lastRuns.SetAsync(match.Key, item.DueAt, cancellationToken).ConfigureAwait(false);
        await match.Value.RegisterNextAsync(item.DueAt, cancellationToken).ConfigureAwait(false);
        return true;
    }
}
