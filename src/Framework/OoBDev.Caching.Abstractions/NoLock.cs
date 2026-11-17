using System;
using System.Threading.Tasks;

namespace OoBDev.Caching;

/// <summary>A lock that is always granted and releases nothing, for a cache that is not shared between processes.</summary>
public sealed class NoLock : IAsyncDisposable
{
    /// <summary>The shared instance.</summary>
    public static NoLock Instance { get; } = new();

    private NoLock()
    {
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
