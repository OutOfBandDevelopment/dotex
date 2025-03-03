using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// Provides an asynchronous manual reset event, allowing tasks to wait for a signal.
/// </summary>
/// <remarks>
/// Based on the implementation from Microsoft's Parallel Programming team:
/// <see href="https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-1-asyncmanualresetevent/"/>
/// </remarks>
public class AsyncManualResetEvent
{
    // https://devblogs.microsoft.com/pfxteam/building-async-coordination-primitives-part-1-asyncmanualresetevent/
    private volatile TaskCompletionSource<bool> m_tcs = new();

    /// <summary>
    /// Asynchronously waits for the event to be set.
    /// </summary>
    /// <returns>A task that completes when the event is set.</returns>
    public Task WaitAsync() => m_tcs.Task;


    /// <summary>
    /// Sets the event, allowing all waiting tasks to proceed.
    /// </summary>
    public void Set()
    {
        var tcs = m_tcs;
        Task.Factory.StartNew(s => ((TaskCompletionSource<bool>)s).TrySetResult(true),
            tcs, CancellationToken.None, TaskCreationOptions.PreferFairness, TaskScheduler.Default);
        tcs.Task.Wait();
    }

    /// <summary>
    /// Resets the event, causing future waits to block until the event is set again.
    /// </summary>
    public void Reset()
    {
        while (true)
        {
            var tcs = m_tcs;
            if (!tcs.Task.IsCompleted ||
                Interlocked.CompareExchange(ref m_tcs, new TaskCompletionSource<bool>(), tcs) == tcs)
                return;
        }
    }
}
