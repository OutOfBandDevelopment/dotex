using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>
/// The client end of a bridge connection: sends one request at a time, matches the reply by id and hands the
/// unsolicited HID reports to <see cref="ReportReceived"/>. Calls are synchronous for the caller.
/// </summary>
public sealed class RemoteConnection : IDisposable
{
    private readonly UsbBridgeKind _kind;
    private readonly TimeSpan _requestTimeout;
    private readonly ConcurrentDictionary<uint, TaskCompletionSource<UsbFrame>> _pending = new();
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    private readonly CancellationTokenSource _cts = new();
    private TcpClient? _client;
    private Task? _reader;
    private uint _id;

    /// <summary>Creates a connection for one kind of device; call <see cref="Connect"/> next.</summary>
    public RemoteConnection(UsbBridgeKind kind, TimeSpan? requestTimeout = null)
    {
        _kind = kind;
        _requestTimeout = requestTimeout ?? TimeSpan.FromSeconds(10);
    }

    /// <summary>Raised on the reader thread for each HID input report.</summary>
    public event Action<byte[]>? ReportReceived;

    /// <summary>Raised once when the connection ends (peer closed or failed).</summary>
    public event Action? Disconnected;

    /// <summary>True between <see cref="Connect"/> and the end of the connection.</summary>
    public bool IsConnected => _client is { Connected: true } && _reader is { IsCompleted: false };

    /// <summary>The device description the server returned in its Hello reply.</summary>
    public string? Description { get; private set; }

    /// <summary>Connects and says Hello; throws <see cref="IOException"/> when the server refuses.</summary>
    public void Connect(string host, int port)
    {
        var client = new TcpClient { NoDelay = true };
        try
        {
            client.Connect(host, port);
        }
        catch (SocketException ex)
        {
            client.Dispose();
            throw new IOException($"Cannot reach the USB bridge at {host}:{port}: {ex.Message}", ex);
        }

        _client = client;
        _reader = Task.Run(ReadLoopAsync, CancellationToken.None);
        var hello = Call(UsbBridgeOp.Hello, [UsbFrameCodec.Version, (byte)_kind]);
        Description = Encoding.UTF8.GetString(hello.Payload.Span[2..]);
    }

    /// <summary>Sends a request and waits for its reply; an Error reply becomes an exception.</summary>
    public UsbFrame Call(UsbBridgeOp op, byte[] payload)
    {
        var client = _client ?? throw new InvalidOperationException("Not connected.");
        var id = Interlocked.Increment(ref _id);
        var completion = new TaskCompletionSource<UsbFrame>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = completion;
        try
        {
            _sendLock.Wait();
            try
            {
                UsbFrameCodec.WriteAsync(client.GetStream(), new UsbFrame(op, id, payload), _cts.Token).GetAwaiter().GetResult();
            }
            finally
            {
                _sendLock.Release();
            }

            if (!completion.Task.Wait(_requestTimeout))
                throw new TimeoutException($"The USB bridge did not answer {op} within {_requestTimeout.TotalSeconds:0} s.");
            var reply = completion.Task.GetAwaiter().GetResult();
            if (reply.Op != UsbBridgeOp.Error)
                return reply;
            var span = reply.Payload.Span;
            var text = Encoding.UTF8.GetString(span[1..]);
            throw (UsbBridgeErrorCode)span[0] switch
            {
                UsbBridgeErrorCode.Timeout => new TimeoutException(text),
                UsbBridgeErrorCode.BadRequest => new InvalidOperationException(text),
                _ => new IOException(text),
            };
        }
        catch (AggregateException ex) when (ex.InnerException is IOException or ObjectDisposedException)
        {
            throw new IOException("The USB bridge connection ended.", ex.InnerException);
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _cts.Cancel();
        _client?.Dispose();
        FailPending();
        _cts.Dispose();
        _sendLock.Dispose();
    }

    private async Task ReadLoopAsync()
    {
        try
        {
            var stream = _client!.GetStream();
            while (!_cts.IsCancellationRequested)
            {
                var frame = await UsbFrameCodec.ReadAsync(stream, _cts.Token).ConfigureAwait(false);
                if (frame is null)
                    break;
                if (frame.Op == UsbBridgeOp.HidReport)
                    ReportReceived?.Invoke(frame.Payload.ToArray());
                else if (_pending.TryGetValue(frame.Id, out var completion))
                    completion.TrySetResult(frame);
            }
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or InvalidDataException or OperationCanceledException)
        {
            // The connection ended; pending calls are failed below.
        }

        FailPending();
        Disconnected?.Invoke();
    }

    private void FailPending()
    {
        foreach (var completion in _pending.Values)
            completion.TrySetException(new IOException("The USB bridge connection ended."));
    }
}
