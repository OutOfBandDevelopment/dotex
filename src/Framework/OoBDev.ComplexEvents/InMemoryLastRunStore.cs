using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Keeps last-run times in memory; lost on restart, so use a durable store when catch-up must survive one.</summary>
public sealed class InMemoryLastRunStore : ILastRunStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _runs = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task<DateTimeOffset?> GetAsync(string key, CancellationToken cancellationToken)
        => Task.FromResult<DateTimeOffset?>(_runs.TryGetValue(key, out var value) ? value : null);

    /// <inheritdoc />
    public Task SetAsync(string key, DateTimeOffset lastRun, CancellationToken cancellationToken)
    {
        _runs.AddOrUpdate(key, lastRun, (_, current) => lastRun > current ? lastRun : current);
        return Task.CompletedTask;
    }
}
