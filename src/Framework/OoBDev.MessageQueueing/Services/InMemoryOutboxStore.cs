using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Services;

/// <summary>An in-process <see cref="IOutboxStore"/>; it is not durable and not transactional, so it suits tests and single-process hosts.</summary>
public class InMemoryOutboxStore : IOutboxStore
{
    private readonly ConcurrentDictionary<string, OutboxEntry> _pending = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public Task AddAsync(OutboxEntry entry)
    {
        _pending[entry.Id] = entry;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<OutboxEntry>> GetPendingAsync(int max) =>
        Task.FromResult<IReadOnlyList<OutboxEntry>>(_pending.Values.OrderBy(e => e.CreatedAt).Take(max).ToList());

    /// <inheritdoc />
    public Task MarkSentAsync(string id)
    {
        _pending.TryRemove(id, out _);
        return Task.CompletedTask;
    }
}
