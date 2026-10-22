using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Threading.Tasks;

/// <summary>
/// Provides extension methods for executing asynchronous Task methods synchronously.
/// </summary>
public static class TaskEx
{
    // http://stackoverflow.com/questions/5095183/how-would-i-run-an-async-taskt-method-synchronously
    // http://stackoverflow.com/a/5097066/89586 

    /// <summary>
    /// Execute's an async Task{T} method which has a void return value synchronously
    /// </summary>
    /// <param name="task">Task{T} method to execute</param>
    public static void RunSync(this Func<Task> task)
    {
        var oldContext = SynchronizationContext.Current;
        using var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        synch.Post(_ => _ = RunAsync(), null);
        synch.BeginMessageLoop();

        SynchronizationContext.SetSynchronizationContext(oldContext);

        async Task RunAsync()
        {
            try
            {
                await task().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                synch.InnerException = e;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }
    }

    /// <summary>
    /// Execute's an async Task{T} method which has a T return type synchronously
    /// </summary>
    /// <typeparam name="T">Return Type</typeparam>
    /// <param name="task">Task{T} method to execute</param>
    /// <returns></returns>
    public static T? RunSync<T>(this Func<Task<T>> task)
    {
        var oldContext = SynchronizationContext.Current;
        using var synch = new ExclusiveSynchronizationContext();
        SynchronizationContext.SetSynchronizationContext(synch);
        T? ret = default;
        synch.Post(_ => _ = RunAsync(), null);
        synch.BeginMessageLoop();
        SynchronizationContext.SetSynchronizationContext(oldContext);
        return ret;

        async Task RunAsync()
        {
            try
            {
                ret = await task().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                synch.InnerException = e;
            }
            finally
            {
                synch.EndMessageLoop();
            }
        }
    }

    private sealed class ExclusiveSynchronizationContext : SynchronizationContext, IDisposable
    {
        private bool done;
        private volatile bool _disposed;
        public Exception? InnerException { get; set; }
        private readonly AutoResetEvent workItemsWaiting = new(false);
        private readonly Queue<Tuple<SendOrPostCallback, object?>> items =
            new();

        public override void Send(SendOrPostCallback d, object? state) => throw new NotSupportedException("We cannot send to our same thread");

        public override void Post(SendOrPostCallback d, object? state)
        {
            if (_disposed)
                return;
            lock (items)
            {
                items.Enqueue(Tuple.Create(d, state));
            }
            workItemsWaiting.Set();
        }

        public void EndMessageLoop() => Post(_ => done = true, null);

        public void BeginMessageLoop()
        {
            while (!done)
            {
                Tuple<SendOrPostCallback, object?>? task = null;
                lock (items)
                {
                    if (items.Count > 0)
                    {
                        task = items.Dequeue();
                    }
                }
                if (task != null)
                {
                    task.Item1(task.Item2);
                    if (InnerException != null) // the method threw an exception
                    {
                        throw new AggregateException("AsyncHelpers.Run method threw an exception.", InnerException);
                    }
                }
                else
                {
                    workItemsWaiting.WaitOne();
                }
            }
        }

        public override SynchronizationContext CreateCopy() => this;

        public void Dispose()
        {
            _disposed = true;
            workItemsWaiting.Dispose();
        }
    }
}
