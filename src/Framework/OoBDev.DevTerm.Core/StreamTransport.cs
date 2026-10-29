using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO;

namespace OoBDev.DevTerm;

/// <summary>
/// An <see cref="ITransport"/> over any <see cref="IDeviceAdapter"/>, so the serial, HID and TCP adapters are written once
/// and serve both the device libraries and the sessions.
/// </summary>
public sealed class StreamTransport(IDeviceAdapter adapter) : ITransport
{
    private readonly IDeviceAdapter _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
    private readonly SemaphoreSlim _gate = new(1, 1);
    private Pipe _pipe = new();
    private Stream? _stream;
    private CancellationTokenSource? _pumpCancel;
    private Task? _pump;
    private ConnectionState _state;

    /// <inheritdoc />
    public ConnectionState State => _state;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_state == ConnectionState.Open) return;
            if (_state != ConnectionState.Closed) throw new InvalidOperationException($"Cannot open a transport that is {_state}.");

            SetState(ConnectionState.Opening);
            if (!_adapter.TryOpen(out var stream) || stream is null)
            {
                SetState(ConnectionState.Faulted);
                throw new IOException($"Could not open {_adapter.Type} device '{_adapter.Path}'.");
            }

            _stream = stream;
            _pipe = new Pipe();
            _pumpCancel = new CancellationTokenSource();
            _pump = Task.Run(() => StreamToPipePump.RunAsync(stream, _pipe.Writer, _pumpCancel.Token), CancellationToken.None);
            SetState(ConnectionState.Open);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_state == ConnectionState.Closed) return;

            SetState(ConnectionState.Closing);
            _pumpCancel?.Cancel();
            if (_stream is not null) await _stream.DisposeAsync().ConfigureAwait(false);
            if (_pump is not null)
            {
                try { await _pump.ConfigureAwait(false); }
                catch (Exception) { }
            }

            _pumpCancel?.Dispose();
            _pumpCancel = null;
            _pump = null;
            _stream = null;
            SetState(ConnectionState.Closed);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var stream = _stream;
        if (_state != ConnectionState.Open || stream is null) throw new InvalidOperationException("The transport is not open.");

        try
        {
            await stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        catch (Exception) when (!cancellationToken.IsCancellationRequested)
        {
            SetState(ConnectionState.Faulted);
            throw;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        _gate.Dispose();
    }

    private void SetState(ConnectionState next)
    {
        var previous = _state;
        if (previous == next) return;
        _state = next;
        StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, next));
    }
}
