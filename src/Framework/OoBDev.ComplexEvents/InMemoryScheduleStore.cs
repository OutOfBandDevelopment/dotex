using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents;

/// <summary>Process-local <see cref="IScheduleStore"/> for tests and single-instance use.</summary>
/// <param name="time">Clock for lease expiry.</param>
/// <param name="maxAttempts">Leases allowed before an item is marked failed.</param>
public sealed class InMemoryScheduleStore(TimeProvider time, int maxAttempts = 3) : IScheduleStore
{
    private sealed class Entry
    {
        public required Guid Id { get; init; }
        public required string Key { get; init; }
        public DateTimeOffset DueAt { get; set; }
        public required string EventType { get; set; }
        public ReadOnlyMemory<byte> Payload { get; set; }
        public int Attempts { get; set; }
        public bool Failed { get; set; }
        public DateTimeOffset? LeasedUntil { get; set; }
    }

    private readonly Lock _gate = new();
    private readonly List<Entry> _entries = [];

    /// <inheritdoc />
    public Task<ScheduleItem> RegisterAsync(string key, DateTimeOffset dueAt, string eventType, ReadOnlyMemory<byte> payload, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(key);
        lock (_gate)
        {
            var entry = _entries.Find(e => string.Equals(e.Key, key, StringComparison.Ordinal));
            if (entry is null)
            {
                entry = new Entry { Id = Guid.NewGuid(), Key = key, EventType = eventType };
                _entries.Add(entry);
            }

            entry.DueAt = dueAt;
            entry.EventType = eventType;
            entry.Payload = payload;
            return Task.FromResult(ToItem(entry));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<ScheduleItem>> LeaseDueAsync(DateTimeOffset now, int max, TimeSpan lease, CancellationToken cancellationToken)
    {
        lock (_gate)
        {
            var leased = new List<ScheduleItem>();
            foreach (var e in _entries.Where(e => !e.Failed && e.DueAt <= now && (e.LeasedUntil is null || e.LeasedUntil <= time.GetUtcNow())).OrderBy(e => e.DueAt).ToList())
            {
                if (leased.Count >= max) break;
                if (e.Attempts >= maxAttempts)
                {
                    e.Failed = true;
                    continue;
                }

                e.Attempts++;
                e.LeasedUntil = time.GetUtcNow() + lease;
                leased.Add(ToItem(e));
            }

            return Task.FromResult<IReadOnlyList<ScheduleItem>>(leased);
        }
    }

    /// <inheritdoc />
    public Task CompleteAsync(Guid id, CancellationToken cancellationToken)
    {
        lock (_gate) _entries.RemoveAll(e => e.Id == id);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<bool> ReleaseAsync(string key, CancellationToken cancellationToken)
    {
        lock (_gate) return Task.FromResult(_entries.RemoveAll(e => string.Equals(e.Key, key, StringComparison.Ordinal)) > 0);
    }

    /// <summary>Items marked failed.</summary>
    public IReadOnlyList<ScheduleItem> FailedItems()
    {
        lock (_gate) return _entries.Where(e => e.Failed).Select(ToItem).ToList();
    }

    private static ScheduleItem ToItem(Entry e) => new(e.Id, e.Key, e.DueAt, e.EventType, e.Payload, e.Attempts, e.Failed);
}
