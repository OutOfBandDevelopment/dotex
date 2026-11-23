using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Remembers the due time of the last occurrence that ran for each recurring schedule, so a restarted scheduler can catch up.</summary>
public interface ILastRunStore
{
    /// <summary>Reads the last run of a recurring schedule.</summary>
    /// <param name="key">The recurring schedule's name.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The due time of the last run, or null when it never ran.</returns>
    Task<DateTimeOffset?> GetAsync(string key, CancellationToken cancellationToken);

    /// <summary>Records the last run; an older value never replaces a newer one.</summary>
    /// <param name="key">The recurring schedule's name.</param>
    /// <param name="lastRun">The due time of the occurrence that ran.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    Task SetAsync(string key, DateTimeOffset lastRun, CancellationToken cancellationToken);
}
