using System;

namespace OoBDev.History;

/// <summary>A rollup level: points are summarised into buckets of <see cref="Bucket"/> and kept for <see cref="Retention"/>.</summary>
public sealed record HistoryTier(string Name, TimeSpan Bucket, TimeSpan Retention)
{
    /// <summary>Start of the bucket that holds <paramref name="timestamp"/>.</summary>
    public DateTimeOffset StartOf(DateTimeOffset timestamp)
    {
        var ticks = timestamp.UtcTicks;
        return new DateTimeOffset(ticks - (ticks % Bucket.Ticks), TimeSpan.Zero);
    }
}
