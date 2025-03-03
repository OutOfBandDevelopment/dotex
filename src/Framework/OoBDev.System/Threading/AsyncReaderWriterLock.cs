using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Threading;

/// <summary>
/// Provides an asynchronous reader-writer lock, allowing multiple readers or one writer to hold a lock.
/// </summary>
/// <remarks>
/// Based on the implementation from Microsoft's Parallel Programming team:
/// <see href="http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10267069.aspx"/>
/// </remarks>
public class AsyncReaderWriterLock
{
    // http://blogs.msdn.com/b/pfxteam/archive/2012/02/12/10267069.aspx
    private readonly Task<Releaser> m_readerReleaser;
    private readonly Task<Releaser> m_writerReleaser;

    private readonly Queue<TaskCompletionSource<Releaser>> m_waitingWriters = new();
    private TaskCompletionSource<Releaser> m_waitingReader = new();
    private int m_readersWaiting;
    private int m_status;

    /// <summary>
    /// Initializes a new instance of the <see cref="AsyncReaderWriterLock"/> class.
    /// </summary>
    public AsyncReaderWriterLock()
    {
        m_readerReleaser = Task.FromResult(new Releaser(this, false));
        m_writerReleaser = Task.FromResult(new Releaser(this, true));
    }

    /// <summary>
    /// Acquires a reader lock asynchronously.
    /// </summary>
    /// <returns>A task that represents the lock acquisition, and returns a <see cref="Releaser"/> when the lock is released.</returns>
    public Task<Releaser> ReaderLockAsync()
    {
        lock (m_waitingWriters)
        {
            if (m_status >= 0 && m_waitingWriters.Count == 0)
            {
                ++m_status;
                return m_readerReleaser;
            }
            else
            {
                ++m_readersWaiting;
                return m_waitingReader.Task.ContinueWith(t => t.Result);
            }
        }
    }

    private void ReaderRelease()
    {
        TaskCompletionSource<Releaser>? toWake = null;
        lock (m_waitingWriters)
        {
            --m_status;
            if (m_status == 0 && m_waitingWriters.Count > 0)
            {
                m_status = -1;
                toWake = m_waitingWriters.Dequeue();
            }
        }

        toWake?.SetResult(new Releaser(this, true));
    }

    /// <summary>
    /// Acquires a writer lock asynchronously.
    /// </summary>
    /// <returns>A task that represents the lock acquisition, and returns a <see cref="Releaser"/> when the lock is released.</returns>
    public Task<Releaser> WriterLockAsync()
    {
        lock (m_waitingWriters)
        {
            if (m_status == 0)
            {
                m_status = -1;
                return m_writerReleaser;
            }
            else
            {
                var waiter = new TaskCompletionSource<Releaser>();
                m_waitingWriters.Enqueue(waiter);
                return waiter.Task;
            }
        }
    }

    /// <summary>
    /// Releases a writer lock.
    /// </summary>
    private void WriterRelease()
    {
        TaskCompletionSource<Releaser>? toWake = null;
        bool toWakeIsWriter = false;

        lock (m_waitingWriters)
        {
            if (m_waitingWriters.Count > 0)
            {
                toWake = m_waitingWriters.Dequeue();
                toWakeIsWriter = true;
            }
            else if (m_readersWaiting > 0)
            {
                toWake = m_waitingReader;
                m_status = m_readersWaiting;
                m_readersWaiting = 0;
                m_waitingReader = new TaskCompletionSource<Releaser>();
            }
            else
                m_status = 0;
        }

        toWake?.SetResult(new Releaser(this, toWakeIsWriter));
    }

    /// <summary>
    /// A struct representing the releaser that can be used to release the lock.
    /// </summary>
    public readonly struct Releaser : IDisposable
    {
        private readonly AsyncReaderWriterLock m_toRelease;
        private readonly bool m_writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Releaser"/> struct.
        /// </summary>
        /// <param name="toRelease">The <see cref="AsyncReaderWriterLock"/> instance to release when disposed.</param>
        /// <param name="writer">Indicates if the releaser is for a writer lock.</param>
        internal Releaser(AsyncReaderWriterLock toRelease, bool writer)
        {
            m_toRelease = toRelease;
            m_writer = writer;
        }

        /// <summary>
        /// Releases the acquired lock.
        /// </summary>
        public void Dispose()
        {
            if (m_toRelease != null)
            {
                if (m_writer)
                    m_toRelease.WriterRelease();
                else
                    m_toRelease.ReaderRelease();
            }
        }
    }
}
