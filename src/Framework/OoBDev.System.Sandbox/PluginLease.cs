using System;
using System.Threading;

namespace OoBDev.System.Sandbox;

/// <summary>A caller's hold on a plugin instance; disposing it releases the hold.</summary>
/// <typeparam name="T">The plugin type.</typeparam>
public sealed class PluginLease<T> : IDisposable
    where T : class, IDisposable
{
    private readonly Action _release;
    private int _released;

    internal PluginLease(T plugin, Action release)
    {
        Plugin = plugin;
        _release = release;
    }

    /// <summary>The leased plugin.</summary>
    public T Plugin { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Interlocked.Exchange(ref _released, 1) == 0)
        {
            _release();
        }
    }
}
