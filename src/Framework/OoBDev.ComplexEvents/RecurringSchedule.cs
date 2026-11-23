using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>
/// Keeps one recurring event registered in an <see cref="IScheduleStore"/>. Each occurrence is its own item
/// (key <c>{key}@{due ticks}</c>), so registering the same occurrence twice never duplicates it.
/// </summary>
/// <param name="store">The store.</param>
/// <param name="time">The clock.</param>
/// <param name="key">The recurring schedule's name.</param>
/// <param name="recurrence">When it recurs.</param>
/// <param name="eventType">Caller-defined event type name.</param>
/// <param name="payload">Serialized event.</param>
/// <param name="catchUp">What to do with occurrences missed since the last run.</param>
public sealed class RecurringSchedule(IScheduleStore store, TimeProvider time, string key, IRecurrence recurrence, string eventType, ReadOnlyMemory<byte> payload, CatchUpPolicy catchUp = CatchUpPolicy.Skip)
{
    private const int MaxMissed = 1000;

    /// <summary>
    /// Registers what is due since <paramref name="lastRun"/> (per the catch-up policy) and the next future occurrence.
    /// Call it on start with the recorded last run time, and after each run with that run's due time.
    /// </summary>
    /// <param name="lastRun">The due time of the last occurrence that ran, or null for a schedule that never ran (it starts at the next occurrence).</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The due times registered, oldest first.</returns>
    public async Task<IReadOnlyList<DateTimeOffset>> EnsureAsync(DateTimeOffset? lastRun, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow();
        var registered = new List<DateTimeOffset>();
        var missed = new List<DateTimeOffset>();
        var cursor = lastRun ?? now;
        while (recurrence.Next(cursor) is { } due && due <= now && missed.Count < MaxMissed)
        {
            missed.Add(due);
            cursor = due;
        }

        var toRun = catchUp switch
        {
            CatchUpPolicy.RunAll => missed,
            CatchUpPolicy.RunLatest => missed.Count > 0 ? [missed[^1]] : missed,
            _ => [],
        };
        foreach (var due in toRun)
        {
            await RegisterAsync(due, registered, cancellationToken).ConfigureAwait(false);
        }

        if (recurrence.Next(cursor) is { } next && next > now)
        {
            await RegisterAsync(next, registered, cancellationToken).ConfigureAwait(false);
        }

        return registered;
    }

    private async Task RegisterAsync(DateTimeOffset due, List<DateTimeOffset> registered, CancellationToken cancellationToken)
    {
        await store.RegisterAsync($"{key}@{due.UtcTicks.ToString(CultureInfo.InvariantCulture)}", due, eventType, payload, cancellationToken).ConfigureAwait(false);
        registered.Add(due);
    }
}
