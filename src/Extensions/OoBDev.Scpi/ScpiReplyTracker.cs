using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Scpi;

/// <summary>
/// Pairs each query with its reply. An instrument answers queries in the order it received them, so the oldest
/// open query takes the next reply; a query that timed out keeps its place and discards its late reply, so the
/// answers behind it stay in step.
/// </summary>
public sealed class ScpiReplyTracker
{
    private readonly Func<ScpiRequest, Task<bool>> _send;
    private readonly TimeSpan _timeout;
    private readonly Queue<Pending> _open = new();
    private readonly Lock _gate = new();

    /// <summary>Initializes a new instance of the <see cref="ScpiReplyTracker"/> class.</summary>
    /// <param name="send">Queues a request on the device (for example <c>stream.Transmit</c>); false means it was refused.</param>
    /// <param name="timeout">How long a query waits for its reply, or <see langword="null"/> for five seconds.</param>
    public ScpiReplyTracker(Func<ScpiRequest, Task<bool>> send, TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(send);
        _send = send;
        _timeout = timeout ?? TimeSpan.FromSeconds(5);
    }

    /// <summary>Occurs for a reply that no query waits for (the instrument spoke first, or a reply arrived after its query gave up and was already discarded).</summary>
    public event EventHandler<ScpiReply>? Unsolicited;

    /// <summary>Gets how many queries still wait for a reply, including timed-out ones whose late reply is yet to be discarded.</summary>
    public int Open
    {
        get
        {
            lock (_gate)
            {
                return _open.Count;
            }
        }
    }

    /// <summary>Sends a command that has no reply.</summary>
    /// <param name="request">The command.</param>
    /// <returns><see langword="true"/> when the device accepted it.</returns>
    public Task<bool> SendAsync(ScpiRequest request) => _send(request);

    /// <summary>Sends a query and waits for its reply.</summary>
    /// <param name="request">A query (<see cref="ScpiRequest.IsQuery"/>).</param>
    /// <param name="cancellationToken">Gives up the wait; the reply is then discarded when it arrives.</param>
    /// <returns>The reply.</returns>
    /// <exception cref="ArgumentException">The request is not a query.</exception>
    /// <exception cref="InvalidOperationException">The device refused the request.</exception>
    /// <exception cref="TimeoutException">No reply came within the timeout.</exception>
    public async Task<ScpiReply> QueryAsync(ScpiRequest request, CancellationToken cancellationToken = default)
    {
        if (!request.IsQuery)
        {
            throw new ArgumentException("The request is not a query.", nameof(request));
        }

        var pending = new Pending();
        lock (_gate)
        {
            _open.Enqueue(pending);
        }

        if (!await _send(request).ConfigureAwait(false))
        {
            Abandon(pending, remove: true);
            throw new InvalidOperationException("The device did not accept the request.");
        }

        try
        {
            return await pending.Source.Task.WaitAsync(_timeout, cancellationToken).ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            Abandon(pending, remove: false);
            throw;
        }
        catch (OperationCanceledException)
        {
            Abandon(pending, remove: false);
            throw;
        }
    }

    /// <summary>Hands a received reply to the oldest open query.</summary>
    /// <param name="reply">The decoded reply.</param>
    public void OnReply(ScpiReply reply)
    {
        Pending? next = null;
        lock (_gate)
        {
            if (_open.Count > 0)
            {
                next = _open.Dequeue();
            }
        }

        if (next is null)
        {
            Unsolicited?.Invoke(this, reply);
        }
        else if (!next.Abandoned)
        {
            next.Source.TrySetResult(reply);
        }
    }

    private void Abandon(Pending pending, bool remove)
    {
        lock (_gate)
        {
            pending.Abandoned = true;
            if (remove)
            {
                var kept = new Queue<Pending>();
                foreach (var item in _open)
                {
                    if (!ReferenceEquals(item, pending))
                    {
                        kept.Enqueue(item);
                    }
                }

                _open.Clear();
                foreach (var item in kept)
                {
                    _open.Enqueue(item);
                }
            }
        }
    }

    private sealed class Pending
    {
        public TaskCompletionSource<ScpiReply> Source { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public bool Abandoned { get; set; }
    }
}
