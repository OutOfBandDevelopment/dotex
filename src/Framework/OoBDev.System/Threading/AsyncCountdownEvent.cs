using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// Represents an asynchronous countdown event that signals when a specified number of signals have been received.
/// </summary>
/// <remarks>
/// Based on the implementation from MSDN's Parallel Programming team:
/// <see href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx"/>
/// </remarks>
public class AsyncCountdownEvent
{
    // http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx
    private readonly AsyncManualResetEvent m_amre = new();
    private int m_count;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncCountdownEvent"/> class with the specified initial count.
    /// </summary>
    /// <param name="initialCount">The number of signals required before the event is set.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="initialCount"/> is less than or equal to zero.</exception>
    public AsyncCountdownEvent(int initialCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCount, nameof(initialCount));
        m_count = initialCount;
    }

    /// <summary>
    /// Asynchronously waits for the countdown event to be set.
    /// </summary>
    /// <returns>A task that completes when the event is signaled.</returns>
    public Task WaitAsync() => m_amre.WaitAsync();

    /// <summary>
    /// Signals the event, decrementing the remaining count.
    /// When the count reaches zero, the event is set.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown if the method is called when the count is already zero or negative.</exception>
    public void Signal()
    {
        if (m_count <= 0)
            throw new InvalidOperationException();

        int newCount = Interlocked.Decrement(ref m_count);
        if (newCount == 0)
            m_amre.Set();
        else if (newCount < 0)
            throw new InvalidOperationException();
    }

    /// <summary>
    /// Signals the event and waits asynchronously until the countdown reaches zero.
    /// </summary>
    /// <returns>A task that completes when the countdown reaches zero.</returns>
    public Task SignalAndWait()
    {
        Signal();
        return WaitAsync();
    }
}
