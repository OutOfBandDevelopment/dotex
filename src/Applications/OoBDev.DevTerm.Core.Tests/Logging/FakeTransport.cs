using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Logging;
using System.IO.Pipelines;

namespace OoBDev.DevTerm.Logging.Tests;

/// <summary>
/// A minimal, in-memory <see cref="IDeviceAdapter"/> for driving a real <c>Session</c> under the session
/// logger without any real hardware/socket — a copy of <c>DevTerm.Console.Tests.FakeTransport</c>
/// (test projects don't reference each other). Can also fail on demand (the next write,
/// the read side, or the next open), and reopens with a fresh pipe after a close, so error/reconnect
/// paths can be tested.
/// </summary>
internal sealed class FakeTransport : IDeviceAdapter, ILineStatusSource
{
    private PipeWriter? _output;

    public event EventHandler<string>? LineStatusChanged;

    public void RaiseLineStatus(string text) => LineStatusChanged?.Invoke(this, text);

    private Pipe _pipe = new();
    private bool _pipeCompleted;
    private ConnectionState _state = ConnectionState.Closed;
    private Exception? _nextWriteFailure;
    private Exception? _nextOpenFailure;

    public List<byte[]> WrittenPayloads { get; } = [];

    public int OpenCount { get; private set; }

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

    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    public PipeReader Input => _pipe.Reader;

    public string Type => "Fake";

    public string Path => "fake";

    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create(WriteAsync);

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

    public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (_nextWriteFailure is { } failure)
        {
            _nextWriteFailure = null;
            return Task.FromException(failure);
        }

        WrittenPayloads.Add(data.ToArray());
        return Task.CompletedTask;
    }

    public async ValueTask DisposeAsync() => await CloseAsync();

    /// <summary>Simulates a device sending bytes back, as if they'd arrived over the wire.</summary>
    public async Task PushIncomingAsync(byte[] data) => await _pipe.Writer.WriteAsync(data);

    /// <summary>Makes the next <see cref="WriteAsync"/> fail with <paramref name="failure"/>, as a real device I/O error would.</summary>
    public void FailNextWrite(Exception failure) => _nextWriteFailure = failure;

    /// <summary>Makes the next <see cref="OpenAsync"/> fail with <paramref name="failure"/>.</summary>
    public void FailNextOpen(Exception failure) => _nextOpenFailure = failure;

    /// <summary>Simulates the device/peer closing the connection cleanly.</summary>
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
