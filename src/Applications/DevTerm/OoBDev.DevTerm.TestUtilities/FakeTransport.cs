using System;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.DevTerm.TestUtilities;

/// <summary>
/// An in-memory <see cref="IDeviceAdapter"/> for driving a real <c>Session</c> without hardware or a socket. It records
/// every write, pushes "device" bytes back on demand or after every write, fails on demand (every write, the next
/// write, the next open, the read side) and reopens with a fresh pipe after a close, so error and reconnect paths can be tested.
/// </summary>
public sealed class FakeTransport : IDeviceAdapter, ILineStatusSource
{
    private PipeWriter? _output;
    private Pipe _pipe = new();
    private bool _pipeCompleted;
    private ConnectionState _state = ConnectionState.Closed;
    private Exception? _nextWriteFailure;
    private Exception? _nextOpenFailure;

    public event EventHandler<string>? LineStatusChanged;

    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    public List<byte[]> WrittenPayloads { get; } = [];

    public IEnumerable<string> WrittenText => WrittenPayloads.Select(Encoding.ASCII.GetString);

    public int OpenCount { get; private set; }

    /// <summary>When set, every write throws this instead of being recorded.</summary>
    public Exception? FailWritesWith { get; set; }

    /// <summary>Text pushed back on the input (plus a newline) after every write; null leaves the device silent.</summary>
    public string? ReplyToEveryWrite { get; set; }

    public ConnectionState State
    {
        get => _state;
        private set
        {
            if (_state == value)
            {
                return;
            }

            var previous = _state;
            _state = value;
            StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, value));
        }
    }

    public PipeReader Input => _pipe.Reader;

    public string Type => "Fake";

    public string Path => "fake";

    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create(WriteAsync);

    public void RaiseLineStatus(string text) => LineStatusChanged?.Invoke(this, text);

    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_nextOpenFailure is { } failure)
        {
            _nextOpenFailure = null;
            State = ConnectionState.Faulted;
            return Task.FromException(failure);
        }

        if (_pipeCompleted)
        {
            _pipe = new Pipe();
            _pipeCompleted = false;
        }

        OpenCount++;
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _pipe.Writer.CompleteAsync();
        _pipeCompleted = true;
        State = ConnectionState.Closed;
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (FailWritesWith is { } always)
        {
            throw always;
        }

        if (_nextWriteFailure is { } failure)
        {
            _nextWriteFailure = null;
            throw failure;
        }

        WrittenPayloads.Add(data.ToArray());
        if (ReplyToEveryWrite is { } reply)
        {
            await _pipe.Writer.WriteAsync(Encoding.ASCII.GetBytes(reply + "\n"), CancellationToken.None);
        }
    }

    public async ValueTask DisposeAsync() => await CloseAsync();

    /// <summary>Simulates a device sending bytes back, as if they had arrived over the wire.</summary>
    public async Task PushIncomingAsync(byte[] data) => await _pipe.Writer.WriteAsync(data);

    /// <summary>Makes the next write fail with <paramref name="failure"/>, as a real device I/O error would.</summary>
    public void FailNextWrite(Exception failure) => _nextWriteFailure = failure;

    /// <summary>Makes the next open fail with <paramref name="failure"/>.</summary>
    public void FailNextOpen(Exception failure) => _nextOpenFailure = failure;

    /// <summary>Simulates the device or peer closing the connection cleanly.</summary>
    public async Task HangUpAsync()
    {
        await _pipe.Writer.CompleteAsync();
        _pipeCompleted = true;
    }

    /// <summary>Simulates the read side dying (an unplugged cable, a reset socket).</summary>
    public async Task FailReadAsync(Exception failure)
    {
        await _pipe.Writer.CompleteAsync(failure);
        _pipeCompleted = true;
    }
}
