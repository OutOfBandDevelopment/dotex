using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// Represents an asynchronous barrier that allows multiple tasks to wait until a predefined number of participants have reached the barrier.
/// </summary>
/// <remarks>
/// Based on the implementation from MSDN's Parallel Programming team:
/// <see href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266932.aspx"/>
/// </remarks>
public class AsyncBarrier
{
    private readonly int m_participantCount;
    private int m_remainingParticipants;
    private ConcurrentStack<TaskCompletionSource<bool>> m_waiters;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncBarrier"/> class with the specified number of participants.
    /// </summary>
    /// <param name="participantCount">The number of participants required to release the barrier.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="participantCount"/> is less than or equal to zero.</exception>
    public AsyncBarrier(int participantCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(participantCount);
        m_remainingParticipants = m_participantCount = participantCount;
        m_waiters = new ConcurrentStack<TaskCompletionSource<bool>>();
    }

    /// <summary>
    /// Signals that a participant has reached the barrier and waits for all participants.
    /// The last participant to arrive releases all waiting participants.
    /// </summary>
    /// <returns>A task that completes when all participants have reached the barrier.</returns>
    public Task SignalAndWait()
    {
        var tcs = new TaskCompletionSource<bool>();
        m_waiters.Push(tcs);
        if (Interlocked.Decrement(ref m_remainingParticipants) == 0)
        {
            m_remainingParticipants = m_participantCount;
            var waiters = m_waiters;
            m_waiters = new ConcurrentStack<TaskCompletionSource<bool>>();
            Parallel.ForEach(waiters, w => w.SetResult(true));
        }
        return tcs.Task;
    }
}
