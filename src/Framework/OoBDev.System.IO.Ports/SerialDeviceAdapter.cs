using OoBDev.System.IO.Pipelines;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;

namespace OoBDev.System.IO.Ports;


/// <summary>
/// <see cref="IDeviceAdapter"/> for a serial/UART connection. See docs/design/Transports/transports.md.
/// </summary>
public sealed class SerialDeviceAdapter : IDeviceAdapter, IComPortControl
{
    private PipeWriter? _output;
    private readonly ISerialPortFactory _portFactory;
    private readonly IOptions<SerialDeviceAdapterOptions> _options;
    private ISerialPort? _port;
    private Stream? _writeStream;
    private ConnectionState _state = ConnectionState.Closed;
    private Pipe? _pipe;
    private CancellationTokenSource? _pumpCts;
    private Task? _pumpTask;

    public SerialDeviceAdapter(ISerialPortFactory portFactory, IOptions<SerialDeviceAdapterOptions> options)
    {
        ArgumentNullException.ThrowIfNull(portFactory);
        ArgumentNullException.ThrowIfNull(options);

        _portFactory = portFactory;
        _options = options;
    }

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

    public PipeReader Input => _pipe?.Reader ?? throw new InvalidOperationException("The serial transport has not been opened.");

    /// <inheritdoc />
    public string Type => "Serial";

    /// <inheritdoc />
    public string Path => _options.Value.PortName;

    /// <inheritdoc />
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create((data, ct) => WriteAsync(data, ct));

    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (State is ConnectionState.Open or ConnectionState.Opening)
        {
            return;
        }

        // A device-side close or unplug moves the state to Closed but leaves the port and pump behind; release them before opening again.
        if (_port is not null)
        {
            await CloseAsync(cancellationToken).ConfigureAwait(false);
        }

        State = ConnectionState.Opening;

        var port = _portFactory.Create(_options.Value);

        try
        {
            port.Open();
        }
        catch
        {
            port.Dispose();
            State = ConnectionState.Faulted;
            throw;
        }

        _port = port;
        _writeStream = _options.Value.WriteByteDelayMs >= 0
            ? new WriteDelayStream(new SerialPortWriteStream(port), _options.Value.WriteByteDelayMs)
            : null;
        _pipe = new Pipe();
        _pumpCts = new CancellationTokenSource();

        var pipe = _pipe;
        _pumpTask = Task.Run(() => StreamToPipePump.RunAsync(port.BaseStream, pipe.Writer, _pumpCts.Token), CancellationToken.None);
        _ = _pumpTask.ContinueWith(
            _ =>
            {
                // The pump ends either because we're deliberately closing (State already moved
                // past Open by then) or because the port faulted/disconnected underneath us.
                if (State == ConnectionState.Open)
                {
                    State = ConnectionState.Closed;
                }
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);

        State = ConnectionState.Open;
    }

    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_port is null)
        {
            return;
        }

        State = ConnectionState.Closing;

        _pumpCts?.Cancel();
        try
        {
            if (_pumpTask is not null)
            {
                await _pumpTask.ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected: cancelling the pump while it's blocked flushing into a paused pipe can
            // surface as the pump task itself completing Canceled rather than completing normally.
        }
        finally
        {
            _pumpCts?.Dispose();
            _pumpCts = null;
            _pumpTask = null;
            _pipe = null;
            _writeStream = null;

            _port.Close();
            _port.Dispose();
            _port = null;

            State = ConnectionState.Closed;
        }
    }

    public async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        if (_port is null || State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The serial transport is not open.");
        }

        if (_writeStream is not null)
        {
            await _writeStream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
            return;
        }

        var buffer = data.ToArray();
        _port.Write(buffer, 0, buffer.Length);
    }

    public IComPortControl SetBaudRate(int baudRate)
    {
        _options.Value.BaudRate = baudRate;
        _port?.SetBaudRate(baudRate);
        return this;
    }

    public IComPortControl SetDataBits(int dataBits)
    {
        _options.Value.DataBits = dataBits;
        _port?.SetDataBits(dataBits);
        return this;
    }

    public IComPortControl SetParity(ComParity parity)
    {
        var mapped = parity switch
        {
            ComParity.Odd => global::System.IO.Ports.Parity.Odd,
            ComParity.Even => global::System.IO.Ports.Parity.Even,
            ComParity.Mark => global::System.IO.Ports.Parity.Mark,
            ComParity.Space => global::System.IO.Ports.Parity.Space,
            _ => global::System.IO.Ports.Parity.None,
        };
        _options.Value.Parity = mapped;
        _port?.SetParity(mapped);
        return this;
    }

    public IComPortControl SetStopBits(ComStopBits stopBits)
    {
        var mapped = stopBits switch
        {
            ComStopBits.Two => global::System.IO.Ports.StopBits.Two,
            ComStopBits.OnePointFive => global::System.IO.Ports.StopBits.OnePointFive,
            _ => global::System.IO.Ports.StopBits.One,
        };
        _options.Value.StopBits = mapped;
        _port?.SetStopBits(mapped);
        return this;
    }

    public IComPortControl SetDtr(bool enabled)
    {
        _options.Value.DtrEnable = enabled;
        _port?.SetDtr(enabled);
        return this;
    }

    public IComPortControl SetRts(bool enabled)
    {
        _options.Value.RtsEnable = enabled;

        // RTS belongs to the handshake under hardware flow control; only remember the value then.
        if (_options.Value.Handshake is not (global::System.IO.Ports.Handshake.RequestToSend or global::System.IO.Ports.Handshake.RequestToSendXOnXOff))
        {
            _port?.SetRts(enabled);
        }

        return this;
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }
}
