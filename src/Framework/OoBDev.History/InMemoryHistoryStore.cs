using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.History;

/// <summary>Thread-safe in-memory provider for tests and single-process use.</summary>
public sealed class InMemoryHistoryStore(HistoryOptions options) : IHistoryStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<HistoryPoint>> _raw = new(StringComparer.Ordinal);
    private readonly Dictionary<(string Series, string Tier, DateTimeOffset Start), Bucket> _buckets = [];

    public Task AppendAsync(string series, HistoryPoint point, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(series);
        lock (_gate)
        {
            if (!_raw.TryGetValue(series, out var points))
            {
                _raw[series] = points = [];
            }

            points.Add(point);
            foreach (var tier in options.Tiers)
            {
                var key = (series, tier.Name, tier.StartOf(point.Timestamp));
                _buckets[key] = _buckets.TryGetValue(key, out var bucket) ? bucket.Add(point) : Bucket.First(point);
            }
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<HistoryPoint>> ReadRawAsync(string series, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<HistoryPoint> result = _raw.TryGetValue(series, out var points)
                ? points.Where(p => p.Timestamp >= from && p.Timestamp < to).OrderBy(p => p.Timestamp).ToList()
                : [];
            return Task.FromResult(result);
        }
    }

    public Task<IReadOnlyList<HistoryBucket>> ReadBucketsAsync(string series, string tier, DateTimeOffset from, DateTimeOffset to, CancellationToken cancellationToken = default)
    {
        var name = options.GetTier(tier).Name;
        lock (_gate)
        {
            IReadOnlyList<HistoryBucket> result = _buckets
                .Where(b => b.Key.Series == series && b.Key.Tier == name && b.Key.Start >= from && b.Key.Start < to)
                .OrderBy(b => b.Key.Start)
                .Select(b => b.Value.ToBucket(b.Key.Start))
                .ToList();
            return Task.FromResult(result);
        }
    }

    public Task<int> PruneAsync(DateTimeOffset now, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var removed = 0;
            var rawCutoff = now - options.RawRetention;
            foreach (var points in _raw.Values)
            {
                removed += points.RemoveAll(p => p.Timestamp < rawCutoff);
            }

            foreach (var key in _buckets.Keys.Where(k => k.Start < now - options.GetTier(k.Tier).Retention).ToList())
            {
                _buckets.Remove(key);
                removed++;
            }

            return Task.FromResult(removed);
        }
    }

    private readonly record struct Bucket(long Count, double Minimum, double Maximum, double Sum, double Last, DateTimeOffset LastAt)
    {
        public static Bucket First(HistoryPoint p) => new(1, p.Value, p.Value, p.Value, p.Value, p.Timestamp);

        public Bucket Add(HistoryPoint p) => new(
            Count + 1,
            Math.Min(Minimum, p.Value),
            Math.Max(Maximum, p.Value),
            Sum + p.Value,
            p.Timestamp >= LastAt ? p.Value : Last,
            p.Timestamp >= LastAt ? p.Timestamp : LastAt);

        public HistoryBucket ToBucket(DateTimeOffset start) => new(start, Count, Minimum, Maximum, Sum, Last);
    }
}
