using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.IO.Inventory;

/// <summary>Keeps entries in memory; for tests and one-off runs.</summary>
public sealed class InMemoryInventoryStore : IInventoryStore
{
    private readonly ConcurrentDictionary<string, InventoryEntry> _entries = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public ValueTask<InventoryEntry?> FindAsync(string path, CancellationToken cancellationToken)
        => ValueTask.FromResult(_entries.TryGetValue(path, out var e) ? e : null);

    /// <inheritdoc />
    public ValueTask SaveAsync(InventoryEntry entry, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _entries[entry.Path] = entry;
        return ValueTask.CompletedTask;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<InventoryEntry> ListAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        foreach (var e in _entries.Values) yield return e;
        await Task.CompletedTask.ConfigureAwait(false);
    }
}
