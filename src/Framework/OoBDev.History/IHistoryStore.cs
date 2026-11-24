using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.History;

/// <summary>Time-series points per named series, with rollup tiers kept up to date as points arrive.</summary>
public interface IHistoryStore
{
    /// <summary>Stores the point and updates the bucket of every tier it falls in.</summary>
    public Task AppendAsync(string series, HistoryPoint point, CancellationToken cancellationToken = default);

    /// <summary>Raw points from <paramref name="from"/> (inclusive) to <paramref name="to"/> (exclusive), oldest first.</summary>
    public Task<IReadOnlyList<HistoryPoint>> ReadRawAsync(string series, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);

    /// <summary>Buckets of a tier whose start is from <paramref name="from"/> (inclusive) to <paramref name="to"/> (exclusive), oldest first.</summary>
    public Task<IReadOnlyList<HistoryBucket>> ReadBucketsAsync(string series, string tier, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default);

    /// <summary>Deletes raw points and buckets older than their retention, measured from <paramref name="now"/>. Returns the number of rows removed.</summary>
    public Task<int> PruneAsync(DateTimeOffset now, CancellationToken cancellationToken = default);
}
