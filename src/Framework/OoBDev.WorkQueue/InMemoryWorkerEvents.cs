using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.WorkQueue;

/// <summary>
/// An in-process <see cref="IWorkerEvents"/> for tests and single-process hosts; the history is lost when the process ends.
/// </summary>
public sealed class InMemoryWorkerEvents(IOptions<WorkerEventsOptions> options, TimeProvider clock) : IWorkerEvents
{
    private readonly WorkerEventsOptions _options = options.Value;
    private readonly List<WorkerEvent> _events = [];
    private readonly Dictionary<string, (long LastId, DateTimeOffset Updated)> _readers = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();
    private long _nextId;

    /// <inheritdoc />
    public Task<bool> PublishAsync(string source, string message, CancellationToken cancellationToken = default)
    {
        lock (_gate)
            _events.Add(new WorkerEvent(++_nextId, clock.GetUtcNow(), source, message));
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<WorkerEvent>> ReadAsync(string reader, int limit, bool fromStart = false, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var now = clock.GetUtcNow();
            if (!_readers.TryGetValue(reader, out var mark))
                mark = (fromStart ? 0 : _nextId, now);
            var page = _events.Where(e => e.Id > mark.LastId).Take(limit).ToList();
            _readers[reader] = (page.Count > 0 ? page[^1].Id : mark.LastId, now);
            return Task.FromResult<IReadOnlyList<WorkerEvent>>(page);
        }
    }

    /// <inheritdoc />
    public Task<int> TrimAsync(CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var cut = clock.GetUtcNow() - _options.MaxAge;
            var before = _events.Count;
            _events.RemoveAll(e => e.At < cut);
            if (_events.Count > _options.MaxRecords)
                _events.RemoveRange(0, _events.Count - _options.MaxRecords);
            foreach (var idle in _readers.Where(r => r.Value.Updated < cut).Select(r => r.Key).ToList())
                _readers.Remove(idle);
            return Task.FromResult(before - _events.Count);
        }
    }
}
