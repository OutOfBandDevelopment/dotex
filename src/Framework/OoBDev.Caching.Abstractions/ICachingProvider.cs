using OoBDev.System.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace OoBDev.Caching;

/// <summary>
/// Provides the underlying cache storage implementation.
/// Implementations can use in-memory, distributed, or any other caching mechanism.
/// </summary>
[ContractConfig(
    AllowDefault = true,
    ConfigKey = "OoBDev:CachingProvider:Type"
    )]
public interface ICachingProvider
{
    /// <summary>
    /// Flushes (removes) a cache entry by key.
    /// </summary>
    /// <param name="key">The cache key to flush.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FlushAsync(string? key);

    /// <summary>
    /// Stores data in the cache with a specified expiration time.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The data to cache.</param>
    /// <param name="expiration">The cache entry expiration time.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAsync(string? key, object? data, TimeSpan expiration);

    /// <summary>
    /// Retrieves a cached value by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="targetType">The expected type of the cached value.</param>
    /// <returns>The cached value or null if not found.</returns>
    Task<object?> RetreiveAsync(string? key, Type? targetType);
}
