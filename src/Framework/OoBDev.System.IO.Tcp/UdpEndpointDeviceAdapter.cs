using Microsoft.Extensions.Options;
using OoBDev.System.IO.Pipelines;
using System;
using System.Globalization;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>
/// <see cref="IDeviceAdapter"/> for a bound UDP port. One session receives datagrams from any sender
/// (each also raised through <see cref="DatagramReceived"/> with its sender, so a front end can show it);
/// sent bytes go to <see cref="ReplyTarget"/>, which starts as the configured target or the last sender.
/// See docs/design/Transports/transports.md (UDP).
/// </summary>
public sealed class UdpEndpointDeviceAdapter(IOptions<UdpEndpointOptions> options) : IDeviceAdapter
{
    private readonly UdpEndpointOptions _options = options.Value;
    private PipeWriter? _output;
    private UdpClient? _client;
    private Pipe? _pipe;
    private CancellationTokenSource? _pumpCts;
    private Task? _pumpTask;
    private IPEndPoint? _replyTarget;
    private ConnectionState _state = ConnectionState.Closed;

    /// <inheritdoc />
    public string Type => "Udp";

    /// <inheritdoc />
    public string Path => LocalEndPoint?.ToString() ?? $"{_options.Host}:{_options.Port}";

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

    /// <summary>Raised for each datagram with its sender and payload, before the bytes reach <see cref="Input"/>.</summary>
    public event EventHandler<UdpDatagramEventArgs>? DatagramReceived;

    /// <inheritdoc />
    public PipeReader Input => _pipe?.Reader ?? throw new InvalidOperationException("The UDP transport has not been opened.");

    /// <inheritdoc />
    public PipeWriter Output => _output ??= DeviceAdapterOutput.Create((data, ct) => WriteAsync(data, ct));

    /// <summary>The bound local endpoint, once open (shows the real port when 0 was requested).</summary>
    public IPEndPoint? LocalEndPoint => _client?.Client.LocalEndPoint as IPEndPoint;

    /// <summary>Where <see cref="Output"/> goes; set it to choose a peer, or leave the last sender.</summary>
    public IPEndPoint? ReplyTarget
    {
        get => _replyTarget;
        set => _replyTarget = value;
    }

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken = default)
    {
        if (State is ConnectionState.Open or ConnectionState.Opening)
        {
            return;
        }

        State = ConnectionState.Opening;
        try
        {
            var bind = string.IsNullOrWhiteSpace(_options.Host)
                ? IPAddress.IPv6Any
                : IPAddress.TryParse(_options.Host, out var literal)
                    ? literal
                    : (await Dns.GetHostAddressesAsync(_options.Host, cancellationToken).ConfigureAwait(false))[0];
            _client = new UdpClient(AddressFamilyOf(bind));
            if (bind.Equals(IPAddress.IPv6Any))
            {
                _client.Client.DualMode = true;
            }

            _client.Client.Bind(new IPEndPoint(bind, _options.Port));
            if (!string.IsNullOrWhiteSpace(_options.Target))
            {
                _replyTarget = await ParseTargetAsync(_options.Target, cancellationToken).ConfigureAwait(false);
            }
        }
        catch
        {
            _client?.Dispose();
            _client = null;
            State = ConnectionState.Faulted;
            throw;
        }

        _pipe = new Pipe();
        _pumpCts = new CancellationTokenSource();
        var client = _client;
        var pipe = _pipe;
        var token = _pumpCts.Token;
        _pumpTask = Task.Run(() => PumpAsync(client, pipe.Writer, token), CancellationToken.None);
        State = ConnectionState.Open;
    }

    /// <inheritdoc />
    public async Task CloseAsync(CancellationToken cancellationToken = default)
    {
        if (_client is null)
        {
            return;
        }

        State = ConnectionState.Closing;
        _pumpCts?.Cancel();
        _client.Dispose();
        try
        {
            if (_pumpTask is not null)
            {
                await _pumpTask.ConfigureAwait(false);
            }
        }
        finally
        {
            _pumpCts?.Dispose();
            _pumpCts = null;
            _pumpTask = null;
            _pipe = null;
            _client = null;
            State = ConnectionState.Closed;
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync().ConfigureAwait(false);
        GC.SuppressFinalize(this);
    }

    private async Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        if (_client is null || State != ConnectionState.Open)
        {
            throw new InvalidOperationException("The UDP transport is not open.");
        }

        var target = _replyTarget ?? throw new InvalidOperationException("No UDP peer to send to yet: set a target or wait for a datagram.");
        await _client.SendAsync(data, target, cancellationToken).ConfigureAwait(false);
    }

    private async Task PumpAsync(UdpClient client, PipeWriter writer, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var result = await client.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(_options.Target))
                {
                    _replyTarget = result.RemoteEndPoint;
                }

                DatagramReceived?.Invoke(this, new UdpDatagramEventArgs(result.RemoteEndPoint, result.Buffer));
                await writer.WriteAsync(result.Buffer, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or ObjectDisposedException or SocketException)
        {
            // Closing the socket is how the loop ends.
        }
        finally
        {
            await writer.CompleteAsync().ConfigureAwait(false);
            if (State == ConnectionState.Open)
            {
                State = ConnectionState.Closed;
            }
        }
    }

    private static AddressFamily AddressFamilyOf(IPAddress address) => address.AddressFamily;

    private static async Task<IPEndPoint> ParseTargetAsync(string target, CancellationToken cancellationToken)
    {
        var separator = target.LastIndexOf(':');
        if (separator <= 0 || !int.TryParse(target.AsSpan(separator + 1), NumberStyles.None, CultureInfo.InvariantCulture, out var port))
        {
            throw new FormatException($"The UDP target '{target}' is not host:port.");
        }

        var host = target[..separator].Trim('[', ']');
        var address = IPAddress.TryParse(host, out var literal)
            ? literal
            : (await Dns.GetHostAddressesAsync(host, cancellationToken).ConfigureAwait(false))[0];
        return new IPEndPoint(address, port);
    }
}
