using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Pipelines;

/// <summary>
/// Base for adapters whose device is a <see cref="Stream"/>: owns the state machine and pumps the stream into
/// <see cref="Input"/> while <see cref="Output"/> writes straight to the stream.
/// </summary>
public abstract class StreamDeviceAdapter : IDeviceAdapter
{
    private static readonly Pipe _closedPipe = CreateCompletedPipe();

    private readonly SemaphoreSlim _gate = new(1, 1);
    private Pipe _pipe = new();
    private PipeWriter _output = _closedPipe.Writer;
    private Stream? _stream;
    private CancellationTokenSource? _pumpCancel;
    private Task? _pump;
    private ConnectionState _state;

    /// <inheritdoc />
    public abstract string Type { get; }

    /// <inheritdoc />
    public abstract string Path { get; }

    /// <inheritdoc />
    public ConnectionState State => _state;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc />
    public PipeWriter Output => _output;

    /// <summary>Opens the underlying stream.</summary>
    /// <param name="cancellationToken">A token to cancel the open.</param>
    /// <returns>The open stream, or <see langword="null"/> when the device could not be opened.</returns>
    protected abstract Task<Stream?> ConnectAsync(CancellationToken cancellationToken);

    /// <summary>Releases what <see cref="ConnectAsync"/> acquired beyond the stream, after the stream is disposed.</summary>
    protected virtual void Disconnect()
    {
    }

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (_state == ConnectionState.Open) return;
            if (_state != ConnectionState.Closed) throw new InvalidOperationException($"Cannot open an adapter that is {_state}.");

            SetState(ConnectionState.Opening);
            Stream? stream;
            try
            {
                stream = await ConnectAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (Exception)
            {
                SetState(ConnectionState.Faulted);
                throw;
            }

            if (stream is null)
            {
                SetState(ConnectionState.Faulted);
                throw new IOException($"Could not open {Type} device '{Path}'.");
            }

            _stream = stream;
            _pipe = new Pipe();
            _output = PipeWriter.Create(stream, new StreamPipeWriterOptions(leaveOpen: true));
            _pumpCancel = new CancellationTokenSource();
            var writer = _pipe.Writer;
            var token = _pumpCancel.Token;
            _pump = Task.Run(() => StreamToPipePump.RunAsync(stream, writer, token), CancellationToken.None);
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
            if (_stream is not null)
            {
                await _stream.DisposeAsync().ConfigureAwait(false);
                Disconnect();
            }
            if (_pump is not null)
            {
                try { await _pump.ConfigureAwait(false); }
                catch (Exception) { }
            }

            _pumpCancel?.Dispose();
            _pumpCancel = null;
            _pump = null;
            _stream = null;
            _output = _closedPipe.Writer;
            SetState(ConnectionState.Closed);
        }
        finally
        {
            _gate.Release();
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        _gate.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Moves to a new state and raises <see cref="StateChanged"/>.</summary>
    /// <param name="next">The new state.</param>
    protected void SetState(ConnectionState next)
    {
        var previous = _state;
        if (previous == next) return;
        _state = next;
        StateChanged?.Invoke(this, new ConnectionStateChangedEventArgs(previous, next));
    }

    private static Pipe CreateCompletedPipe()
    {
        var pipe = new Pipe();
        pipe.Writer.Complete();
        return pipe;
    }
}
