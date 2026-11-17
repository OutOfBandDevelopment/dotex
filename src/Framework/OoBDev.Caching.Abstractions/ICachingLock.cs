using System;
using System.Threading.Tasks;

namespace OoBDev.Caching;

/// <summary>
/// Optional capability of an <see cref="ICachingProvider"/> whose store is shared by several processes (for example Redis):
/// a lock that outlives one process, used so a cold key is computed once across all of them.
/// </summary>
public interface ICachingLock
{
    /// <summary>Tries to take the lock for <paramref name="key"/> without waiting.</summary>
    /// <param name="key">The cache key.</param>
    /// <param name="lifeTime">How long the lock holds if its owner dies before releasing it.</param>
    /// <returns>The lock to dispose when done, or <see langword="null"/> when another owner holds it.</returns>
    Task<IAsyncDisposable?> TryAcquireAsync(string key, TimeSpan lifeTime);
}
