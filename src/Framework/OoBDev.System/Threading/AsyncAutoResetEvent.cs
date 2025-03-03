using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// Represents an asynchronous auto-reset event, allowing tasks to wait asynchronously until signaled.
/// </summary>
/// <remarks>
/// Based on the implementation from MSDN's Parallel Programming team:
/// <see href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266923.aspx"/>
/// </remarks>
public class AsyncAutoResetEvent
{
    //http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266923.aspx
    private readonly static Task s_completed = Task.FromResult(true);
    private readonly Queue<TaskCompletionSource<bool>> m_waits = new();
    private bool m_signaled;

    /// <summary>
    /// Asynchronously waits for the event to be set.
    /// </summary>
    /// <returns>A task that completes when the event is signaled.</returns>
    public Task WaitAsync()
    {
        lock (m_waits)
        {
            if (m_signaled)
            {
                m_signaled = false;
                return s_completed;
            }
            else
            {
                var tcs = new TaskCompletionSource<bool>();
                m_waits.Enqueue(tcs);
                return tcs.Task;
            }
        }
    }

    /// <summary>
    /// Sets the event, allowing one waiting task to proceed.
    /// If no tasks are waiting, the event remains signaled until the next wait call.
    /// </summary>
    public void Set()
    {
        TaskCompletionSource<bool>? toRelease = null;
        lock (m_waits)
        {
            if (m_waits.Count > 0)
                toRelease = m_waits.Dequeue();
            else if (!m_signaled)
                m_signaled = true;
        }
        toRelease?.SetResult(true);
    }
}
