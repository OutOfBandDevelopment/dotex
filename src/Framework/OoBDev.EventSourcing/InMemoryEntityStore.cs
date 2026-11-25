using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing;

/// <summary>
/// Entity store that keeps entities in memory; for tests and single-process use.
/// </summary>
public sealed class InMemoryEntityStore : IEntityStore
{
    private readonly Lock _gate = new();
    private readonly Dictionary<(string Type, string Key), EntityRecord> _items = [];

    /// <inheritdoc />
    public Task<EntityRecord?> GetAsync(string type, string key, CancellationToken cancellationToken = default)
    {
        lock (_gate)
            return Task.FromResult(_items.GetValueOrDefault((type, key)));
    }

    /// <inheritdoc />
    public Task<EntityRecord?> SaveAsync(string type, string key, string value, long expectedVersion, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            var current = _items.GetValueOrDefault((type, key));
            if ((current?.Version ?? 0) != expectedVersion)
                return Task.FromResult<EntityRecord?>(null);

            var saved = new EntityRecord(type, key, expectedVersion + 1, value);
            _items[(type, key)] = saved;
            return Task.FromResult<EntityRecord?>(saved);
        }
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(string type, string key, long? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            if (!_items.TryGetValue((type, key), out var current))
                return Task.FromResult(false);
            if (expectedVersion is not null && current.Version != expectedVersion)
                return Task.FromResult(false);
            return Task.FromResult(_items.Remove((type, key)));
        }
    }

    /// <inheritdoc />
    public Task<IReadOnlyList<EntityRecord>> QueryAsync(string type, string keyPrefix = "", CancellationToken cancellationToken = default)
    {
        lock (_gate)
        {
            IReadOnlyList<EntityRecord> result =
            [
                .. _items.Values
                    .Where(e => e.Type == type && e.Key.StartsWith(keyPrefix, StringComparison.Ordinal))
                    .OrderBy(e => e.Key, StringComparer.Ordinal)
            ];
            return Task.FromResult(result);
        }
    }
}
