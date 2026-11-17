using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.Caching;

/// <summary>
/// Manages cache operations including key generation, storage, retrieval, and flushing.
/// </summary>
public interface ICachingManager
{
    /// <summary>
    /// Builds a cache key from a method and its arguments.
    /// </summary>
    /// <param name="method">The method information.</param>
    /// <param name="args">The method arguments.</param>
    /// <returns>A unique cache key string.</returns>
    string BuildKey(MethodInfo method, params object?[]? args);

    /// <summary>
    /// Flushes (removes) a cache entry by key.
    /// </summary>
    /// <param name="key">The cache key to flush.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task FlushAsync(string key);

    /// <summary>
    /// Stores data in the cache with a specified lifetime.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="data">The data to cache.</param>
    /// <param name="lifeTime">The cache entry lifetime.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task StoreAsync(string key, object data, TimeSpan lifeTime);

    /// <summary>
    /// Retrieves a cached value by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="targetType">The expected type of the cached value.</param>
    /// <returns>The cached value or null if not found.</returns>
    Task<object?> RetrieveAsync(string key, Type targetType);

    /// <summary>
    /// Retrieves a strongly-typed cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value.</returns>
    Task<T?> RetrieveAsync<T>(string key);

    /// <summary>
    /// Takes a lock on <paramref name="key"/> shared by every process that uses the same cache, so one process computes a
    /// missing value while the others wait for it (cross-process single flight).
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="lifeTime">How long the lock holds if its owner dies before releasing it.</param>
    /// <returns>The lock to dispose when done; <see langword="null"/> when another process holds it. A provider with no lock support returns an always-granted no-op lock.</returns>
    Task<IAsyncDisposable?> TryLockAsync(string key, TimeSpan lifeTime) => Task.FromResult<IAsyncDisposable?>(NoLock.Instance);
}
