using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Sandbox;

/// <summary>
/// Hands out plugin instances by lifetime mode. A shared instance is keyed by plugin id plus a hash of its grants, so two
/// applications with different grants never share a process. The host calls <see cref="Sweep"/> on a timer to end idle shared instances.
/// </summary>
/// <typeparam name="T">The plugin type.</typeparam>
public sealed class PluginPool<T>(TimeProvider time, TimeSpan idleTimeout) : IDisposable
    where T : class, IDisposable
{
    private sealed class Entry(T plugin)
    {
        public T Plugin { get; } = plugin;

        public int Leases { get; set; }

        public DateTimeOffset IdleSince { get; set; }
    }

    private readonly Dictionary<string, Entry> _shared = new(StringComparer.Ordinal);
    private readonly object _gate = new();

    /// <summary>Number of shared instances currently running.</summary>
    public int SharedCount
    {
        get
        {
            lock (_gate)
            {
                return _shared.Count;
            }
        }
    }

    /// <summary>Leases an instance. <paramref name="create"/> runs only when no usable shared instance exists.</summary>
    public PluginLease<T> Acquire(string pluginId, string grantsHash, PluginLifetime lifetime, Func<T> create)
    {
        if (lifetime == PluginLifetime.Bound)
        {
            var own = create();
            return new PluginLease<T>(own, own.Dispose);
        }

        var key = pluginId + "|" + grantsHash;
        lock (_gate)
        {
            if (!_shared.TryGetValue(key, out var entry))
            {
                entry = new Entry(create());
                _shared[key] = entry;
            }

            entry.Leases++;
            return new PluginLease<T>(entry.Plugin, () => Release(key));
        }
    }

    /// <summary>Ends shared instances that have had no lease for longer than the idle timeout; returns how many.</summary>
    public int Sweep()
    {
        var now = time.GetUtcNow();
        lock (_gate)
        {
            var idle = _shared.Where(p => p.Value.Leases == 0 && now - p.Value.IdleSince >= idleTimeout).ToList();
            foreach (var pair in idle)
            {
                _shared.Remove(pair.Key);
                pair.Value.Plugin.Dispose();
            }

            return idle.Count;
        }
    }

    /// <summary>
    /// Ends idle instances as <see cref="Sweep()"/> does, and asks every other idle shared instance whether it is healthy; one that is not is ended and removed,
    /// so the next <see cref="Acquire"/> starts a fresh process (the restart policy). Leased instances are never probed: their stream belongs to the caller.
    /// The pool is locked while it asks.
    /// </summary>
    public int Sweep(Func<T, bool> healthy)
    {
        ArgumentNullException.ThrowIfNull(healthy);
        var ended = Sweep();
        lock (_gate)
        {
            var hung = _shared.Where(p => p.Value.Leases == 0 && !healthy(p.Value.Plugin)).ToList();
            foreach (var pair in hung)
            {
                _shared.Remove(pair.Key);
                pair.Value.Plugin.Dispose();
            }

            return ended + hung.Count;
        }
    }

    /// <summary>Ends every shared instance.</summary>
    public void Dispose()
    {
        lock (_gate)
        {
            foreach (var entry in _shared.Values)
            {
                entry.Plugin.Dispose();
            }

            _shared.Clear();
        }
    }

    private void Release(string key)
    {
        lock (_gate)
        {
            if (_shared.TryGetValue(key, out var entry) && --entry.Leases == 0)
            {
                entry.IdleSince = time.GetUtcNow();
            }
        }
    }
}
