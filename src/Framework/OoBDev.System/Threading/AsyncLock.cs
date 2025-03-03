using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// Provides an asynchronous mutual exclusion mechanism similar to a lock statement.
/// </summary>
/// <remarks>
/// Based on the implementation from MSDN's Parallel Programming team:
/// <see href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx"/>
/// </remarks>
public class AsyncLock
{
    // http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266988.aspx
    private readonly AsyncSemaphore m_semaphore;
    private readonly Task<Releaser> m_releaser;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncLock"/> class.
    /// </summary>
    public AsyncLock()
    {
        m_semaphore = new AsyncSemaphore(1);
        m_releaser = Task.FromResult(new Releaser(this));
    }

    /// <summary>
    /// Asynchronously acquires the lock.
    /// </summary>
    /// <returns>A task that completes when the lock is acquired, returning a <see cref="Releaser"/> that releases the lock when disposed.</returns>
    public Task<Releaser> LockAsync()
    {
        var wait = m_semaphore.WaitAsync();
        return wait.IsCompleted ?
            m_releaser :
            wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                this, CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    /// <summary>
    /// Represents a handle to an acquired <see cref="AsyncLock"/>, which releases the lock when disposed.
    /// </summary>
    public readonly struct Releaser : IDisposable
    {
        private readonly AsyncLock m_toRelease;

        /// <summary>
        /// Initializes a new instance of the <see cref="Releaser"/> struct.
        /// </summary>
        /// <param name="toRelease">The <see cref="AsyncLock"/> instance to release when disposed.</param>
        internal Releaser(AsyncLock toRelease) { m_toRelease = toRelease; }

        /// <summary>
        /// Releases the acquired lock.
        /// </summary>
        public void Dispose() => m_toRelease?.m_semaphore.Release();
    }
}
