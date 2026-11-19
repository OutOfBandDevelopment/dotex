using System;

namespace OoBDev.Imaging.People;

/// <summary>
/// Decides whether a clustering run of one kind is due.
/// </summary>
public static class ClusterSchedulePolicy
{
    /// <summary>Returns true when enough time has passed since the last run and enough unclustered items wait.</summary>
    /// <param name="now">The current time.</param>
    /// <param name="lastRun">When this kind last ran, or null if it never did.</param>
    /// <param name="unclustered">How many items of this kind have no cluster.</param>
    /// <param name="options">The interval and the minimum number of new items.</param>
    /// <returns>True when a run is due.</returns>
    public static bool ShouldRun(DateTimeOffset now, DateTimeOffset? lastRun, int unclustered, PeopleClusteringOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return unclustered >= options.MinNewItems && (lastRun is null || now - lastRun.Value >= options.MinInterval);
    }
}
