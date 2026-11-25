using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;
using OoBDev.System.Net.Services;
using OoBDev.System.Net.Sockets;
using System;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DevTerm;

/// <summary>
/// Hosts one of the classic TCP services (echo, discard, daytime, time, chargen) as a device, so a session shows what
/// clients send it and the service doubles as a test peer for DevTerm's other channels. What clients send arrives as
/// the session's received data; the replies the service writes are reported on <see cref="Replied"/>. Anything written
/// to the session is dropped, because the service decides its own replies. Binds loopback unless told otherwise; the
/// server is started by <see cref="OpenAsync"/> and stopped by <see cref="CloseAsync"/>.
/// See docs/design/DevTermParity/generic-services-plan.md.
/// </summary>
public sealed class ServiceHostBridge : IDeviceAdapter
{
    private readonly ServiceKind _kind;
    private readonly IPAddress _address;
    private readonly ushort _port;
    private readonly long _chargenLimit;
    private readonly Lock _gate = new();
    private ServerBase? _server;
    private Pipe _pipe = NewPipe();
    private PipeWriter? _output;
    private ConnectionState _state = ConnectionState.Closed;

    /// <param name="kind">The service to host.</param>
    /// <param name="port">The TCP port to listen on (the service's standard port when 0).</param>
    /// <param name="address">The address to bind; loopback when null.</param>
    /// <param name="chargenLimit">How many bytes chargen sends each client before it stops (0 for no limit).</param>
    public ServiceHostBridge(ServiceKind kind, ushort port = 0, IPAddress? address = null, long chargenLimit = ChargenServer.DefaultMaxBytesPerClient)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(chargenLimit);
        _kind = kind;
        _address = address ?? IPAddress.Loopback;
        _port = port == 0 ? DefaultPort(kind) : port;
        _chargenLimit = chargenLimit;
    }

    /// <summary>The standard port of <paramref name="kind"/>.</summary>
    public static ushort DefaultPort(ServiceKind kind) => kind switch
    {
        ServiceKind.Echo => 7,
        ServiceKind.Discard => 9,
        ServiceKind.Daytime => 13,
        ServiceKind.Chargen => 19,
        ServiceKind.Time => 37,
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };

    /// <summary>Raised with each chunk the service wrote to a client.</summary>
    public event Action<ReadOnlyMemory<byte>>? Replied;

    /// <inheritdoc />
    public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged;

    /// <inheritdoc />
    public string Type => "Service";

    /// <inheritdoc />
    public string Path => $"{_kind.ToString().ToLowerInvariant()}://{_address}:{_port}";

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
    public PipeReader Input => _pipe.Reader;

    /// <inheritdoc />
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create((_, _) => Task.CompletedTask);

    /// <summary>Starts the server.</summary>
    /// <exception cref="IOException">The port could not be bound (in use, or a privileged port without the rights).</exception>
    public Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (_server is not null)
        {
            return Task.CompletedTask;
        }

        _pipe = NewPipe();
        var server = Create();
        server.Traffic += OnTraffic;
        try
        {
            server.Start();
        }
        catch (SocketException ex)
        {
            throw new IOException($"Cannot listen on {_address}:{_port}: {ex.Message}", ex);
        }

        _server = server;
        State = ConnectionState.Open;
        return Task.CompletedTask;
    }

    /// <summary>Stops the server and ends the received stream.</summary>
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        var server = _server;
        _server = null;
        if (server is not null)
        {
            server.Traffic -= OnTraffic;
            await server.StopAsync().ConfigureAwait(false);
            await server.DisposeAsync().ConfigureAwait(false);
        }

        await _pipe.Writer.CompleteAsync().ConfigureAwait(false);
        State = ConnectionState.Closed;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync() => await CloseAsync().ConfigureAwait(false);

    private static Pipe NewPipe() => new(new PipeOptions(pauseWriterThreshold: 0));

    private ServerBase Create() => _kind switch
    {
        ServiceKind.Echo => new EchoServer(_address, _port),
        ServiceKind.Discard => new DiscardServer(_address, _port),
        ServiceKind.Daytime => new DaytimeServer(_address, _port),
        ServiceKind.Time => new TimeServer(_address, _port),
        ServiceKind.Chargen => new ChargenServer(_address, _port, _chargenLimit),
        _ => throw new InvalidOperationException($"Unknown service {_kind}."),
    };

    private void OnTraffic(ReadOnlyMemory<byte> data, bool incoming)
    {
        if (!incoming)
        {
            Replied?.Invoke(data);
            return;
        }

        lock (_gate)
        {
            // The pipe never pauses (threshold 0), so this flush completes synchronously.
            _pipe.Writer.WriteAsync(data).AsTask().GetAwaiter().GetResult();
        }
    }
}
