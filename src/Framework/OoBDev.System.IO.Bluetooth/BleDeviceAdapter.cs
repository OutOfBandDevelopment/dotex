using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.Bluetooth;

/// <summary>
/// <see cref="IDeviceAdapter"/> for a BLE peripheral connection. Unlike serial/HID/USBTMC there's no blocking <see cref="Stream"/> to pump on a
/// background thread: <see cref="IBleConnection"/> already delivers notifications asynchronously via
/// an event, so incoming bytes are written into <see cref="Input"/>'s pipe directly from that event.
/// </summary>
public sealed class BleDeviceAdapter : IDeviceAdapter
{
    private readonly IBleConnectionFactory _adapterFactory;
    private readonly IOptions<BleDeviceAdapterOptions> _options;
    private IBleConnection? _adapter;
    private ConnectionState _state = ConnectionState.Closed;
    private Pipe? _pipe;
    private PipeWriter? _output;

    // Serializes every PipeWriter access - each notification starts a fire-and-forget write, and
    // OnAdapterDisconnected/CloseAsync complete the writer from other threads. PipeWriter is
    // single-writer: a second WriteAsync while a first is still flushing (blocked on backpressure)
    // doesn't just throw - it corrupts the Pipe's internal state badly enough that even the reader
    private readonly SemaphoreSlim _writeGate = new(1, 1);

    /// <summary>Creates the adapter.</summary>
    public BleDeviceAdapter(IBleConnectionFactory adapterFactory, IOptions<BleDeviceAdapterOptions> options)
    {
        ArgumentNullException.ThrowIfNull(adapterFactory);
        ArgumentNullException.ThrowIfNull(options);

        _adapterFactory = adapterFactory;
        _options = options;
    }

    /// <inheritdoc />
    public string Type => "Ble";

    /// <inheritdoc />
    public string Path => _options.Value.DeviceId;

    /// <inheritdoc />
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create((data, ct) => WriteAsync(data, ct));

    /// <inheritdoc />
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

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public PipeReader Input => _pipe?.Reader ?? throw new InvalidOperationException("The BLE transport has not been opened.");

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (State is ConnectionState.Open or ConnectionState.Opening)
        {
            return;
        }

        State = ConnectionState.Opening;

        var adapter = _adapterFactory.Create(_options.Value);
        adapter.NotificationReceived += OnNotificationReceived;
        adapter.Disconnected += OnAdapterDisconnected;

        // Created before ConnectAsync, not after: some devices send a greeting the instant
        // notifications are enabled, which happens inside ConnectAsync - a notification arriving
        // that early needs somewhere to land rather than being silently dropped by
        // OnNotificationReceived's _pipe-is-null guard.
        _pipe = new Pipe();

        try
        {
            await RunWithTimeoutAsync(
                adapter.ConnectAsync,
                _options.Value.ConnectTimeoutMs,
                $"Connecting to the BLE peripheral timed out after {_options.Value.ConnectTimeoutMs} ms.",
                cancellationToken).ConfigureAwait(false);
        }
        catch
        {
            adapter.NotificationReceived -= OnNotificationReceived;
            adapter.Disconnected -= OnAdapterDisconnected;
            await adapter.DisposeAsync().ConfigureAwait(false);
            _pipe = null;
            State = ConnectionState.Faulted;
            throw;
        }

        _adapter = adapter;
        State = ConnectionState.Open;
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_adapter is null)
        {
            return;
        }

        State = ConnectionState.Closing;

        var adapter = _adapter;
        _adapter = null;
        adapter.NotificationReceived -= OnNotificationReceived;
        adapter.Disconnected -= OnAdapterDisconnected;

        await adapter.DisconnectAsync(cancellationToken).ConfigureAwait(false);
        await adapter.DisposeAsync().ConfigureAwait(false);

        CompleteWriter();
        _pipe = null;

        State = ConnectionState.Closed;
    }

    internal Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (_adapter is null || State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The BLE transport is not open.");
        }

        return RunWithTimeoutAsync(
            ct => _adapter.WriteAsync(data, ct),
            _options.Value.WriteTimeoutMs,
            $"Writing to the BLE peripheral timed out after {_options.Value.WriteTimeoutMs} ms.",
            cancellationToken);
    }

    /// <summary>
    /// Runs <paramref name="operation"/> under a linked token cancelled after <paramref name="timeoutMs"/>,
    /// converting the resulting <see cref="OperationCanceledException"/> into a <see cref="TimeoutException"/>
    /// only when the timeout (not the caller's own <paramref name="cancellationToken"/>) fired.
    /// </summary>
    private static async Task RunWithTimeoutAsync(
        Func<CancellationToken, Task> operation,
        int timeoutMs,
        string timeoutMessage,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await operation(linkedCts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException canceled) when (timeoutCts.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
        {
            throw new TimeoutException(timeoutMessage, canceled);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private void OnNotificationReceived(object? sender, ReadOnlyMemory<byte> data)
    {
        var pipe = _pipe;
        if (pipe is null)
        {
            return;
        }

        _ = WriteToPipeAsync(pipe.Writer, data);
    }

    private async Task WriteToPipeAsync(PipeWriter writer, ReadOnlyMemory<byte> data)
    {
        await _writeGate.WaitAsync().ConfigureAwait(false);
        try
        {
            await writer.WriteAsync(data).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ObjectDisposedException)
        {
            // The pipe was already completed (transport closing/closed) - the notification arrived
            // too late to matter.
        }
        finally
        {
            _writeGate.Release();
        }
    }

    private void OnAdapterDisconnected(object? sender, EventArgs e)
    {
        if (State != ConnectionState.Open)
        {
            return;
        }

        CompleteWriter();
        State = ConnectionState.Closed;
    }

    /// <summary>Completes <see cref="_pipe"/>'s writer under the same gate as every <see cref="WriteToPipeAsync"/> call, so completion can't race an in-flight write.</summary>
    private void CompleteWriter()
    {
        _writeGate.Wait();
        try
        {
            _pipe?.Writer.Complete();
        }
        finally
        {
            _writeGate.Release();
        }
    }
}
