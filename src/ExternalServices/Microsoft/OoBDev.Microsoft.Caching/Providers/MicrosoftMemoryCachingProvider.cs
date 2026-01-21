using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using OoBDev.Caching;
using System;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Caching.Providers;

/// <summary>
/// In-memory caching provider using Microsoft.Extensions.Caching.Memory.
/// </summary>
public class MicrosoftMemoryCachingProvider : ICachingProvider, IDisposable
{
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Initializes a new instance of the <see cref="MicrosoftMemoryCachingProvider"/> class.
    /// </summary>
    /// <param name="optionsAccessor">The memory cache options.</param>
    public MicrosoftMemoryCachingProvider(
        IOptions<MemoryCacheOptions> optionsAccessor
        )
    {
        _cache = new MemoryCache(optionsAccessor);
    }

    /// <summary>
    /// Disposes the underlying memory cache.
    /// </summary>
    public void Dispose() => _cache.Dispose();

    /// <inheritdoc/>
    public Task FlushAsync(string? key)
    {
        if (key != null)
            _cache.Remove(key);
        return Task.FromResult(0);
    }

    /// <inheritdoc/>
    public Task<object?> RetreiveAsync(string? key, Type? targetType) =>
        Task.FromResult(
            key != null && _cache.TryGetValue(key, out var value) ?
                value :
                null
        );

    /// <inheritdoc/>
    public Task StoreAsync(string? key, object? data, TimeSpan expiration)
    {
        if (key != null && data != null)
            _cache.Set(key, data, expiration);
        return Task.FromResult(0);
    }
}
