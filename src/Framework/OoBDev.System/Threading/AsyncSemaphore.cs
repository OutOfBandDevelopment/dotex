using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// An asynchronous semaphore that limits access to a resource by a specified count of tasks.
/// </summary>
/// <remarks>
/// Based on the implementation from Microsoft's Parallel Programming team:
/// <see href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx"/>
/// </remarks>
public class AsyncSemaphore
{
    // http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10266983.aspx
    private readonly static Task s_completed = Task.FromResult(true);
    private readonly Queue<TaskCompletionSource<bool>> m_waiters = new();
    private int m_currentCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncSemaphore"/> class.
    /// </summary>
    /// <param name="initialCount">The initial number of available permits.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the initial count is negative.</exception>
    public AsyncSemaphore(int initialCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(initialCount, nameof(initialCount));
        m_currentCount = initialCount;
    }

    /// <summary>
    /// Asynchronously waits for a permit to become available.
    /// </summary>
    /// <returns>A task that represents the waiting operation, completing when a permit is acquired.</returns>
    public Task WaitAsync()
    {
        lock (m_waiters)
        {
            if (m_currentCount > 0)
            {
                --m_currentCount;
                return s_completed;
            }
            else
            {
                var waiter = new TaskCompletionSource<bool>();
                m_waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }
    }

    /// <summary>
    /// Releases a permit, allowing a waiting task to proceed if any are waiting.
    /// </summary>
    public void Release()
    {
        TaskCompletionSource<bool>? toRelease = default;
        lock (m_waiters)
        {
            if (m_waiters.Count > 0)
                toRelease = m_waiters.Dequeue();
            else
                ++m_currentCount;
        }
        toRelease?.SetResult(true);
    }
}
