using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>
/// One serial device bridged to one TCP listener. The serial port stays open while no
/// client is connected (and is reopened after failures); one TCP client at a time is served.
/// </summary>
public sealed class BridgeWorker : IDisposable
{
    private const int BufferSize = 4096;

    private readonly BridgeOptions _options;
    private readonly ISerialPortFactory _serialFactory;
    private readonly ILogger _logger;
    private readonly TimeSpan _retryInterval;
    private readonly Lock _gate = new();
    private readonly IServiceAdvertiser? _advertiser;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    private TcpListener? _listener;
    private ISerialLink? _link;
    private TcpClient? _client;
    private Task _clientTask = Task.CompletedTask;
    private Rfc2217Session? _session;
    private IDisposable? _advertisement;
    private EndPoint? _clientRemote;
    private DateTimeOffset _clientSince;

    public BridgeWorker(BridgeOptions options, ISerialPortFactory serialFactory, ILogger logger, TimeSpan retryInterval, IServiceAdvertiser? advertiser = null)
    {
        _advertiser = advertiser;
        _options = options;
        _serialFactory = serialFactory;
        _logger = logger;
        _retryInterval = retryInterval;
    }

    public BridgeOptions Options => _options;

    /// <summary>The clock for client timing; replaced in tests.</summary>
    public TimeProvider TimeProvider { get; init; } = TimeProvider.System;

    /// <inheritdoc />
    public void Dispose() => _writeLock.Dispose();

    /// <summary>How long a new connection waits for the previous client to finish releasing the port before it is refused.</summary>
    public TimeSpan ClientReleaseGrace { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>A write to the client that makes no progress for this long drops the client, so a stalled peer cannot block the serial reader.</summary>
    public TimeSpan ClientWriteTimeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>The port the listener is actually bound to (useful when TcpPort is 0).</summary>
    public int BoundPort => ((IPEndPoint)(_listener ?? throw new InvalidOperationException("Not started.")).LocalEndpoint).Port;

    public bool IsSerialOpen
    {
        get { lock (_gate) { return _link is not null; } }
    }

    public bool IsClientConnected
    {
        get { lock (_gate) { return _client is not null; } }
    }

    /// <summary>Binds the TCP listener. Throws if the port is unavailable.</summary>
    public void Start()
    {
        if (_listener is not null)
        {
            return;
        }

        var listener = new TcpListener(IPAddress.Parse(_options.BindAddress), _options.TcpPort);
        listener.Start();
        _listener = listener;
        _logger.Listening(_options.ComPort, listener.LocalEndpoint, _options.Protocol);

        if (_advertiser is not null)
        {
            try
            {
                _advertisement = _advertiser.Advertise(_options, ((IPEndPoint)listener.LocalEndpoint).Port);
            }
            catch (Exception ex)
            {
                _logger.AdvertisementFailed(ex, _options.ComPort);
            }
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        Start();
        var listener = _listener!;
        var serialTask = RunSerialAsync(cancellationToken);

        try
        {
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                await AcceptAsync(client, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Normal shutdown.
        }
        finally
        {
            listener.Stop();
            _advertisement?.Dispose();
            await serialTask.ConfigureAwait(false);
            await _clientTask.ConfigureAwait(false);
        }
    }

    /// <summary>Applies keep-alive (a vanished peer is noticed in about a minute) and disables Nagle.</summary>
    public static void ConfigureClientSocket(Socket socket)
    {
        socket.NoDelay = true;
        try
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
        }
        catch (Exception ex) when (ex is SocketException or PlatformNotSupportedException)
        {
            // Keep-alive tuning is best effort; the default OS keep-alive still applies where enabled.
        }
    }

    private async Task AcceptAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            ConfigureClientSocket(client.Client);
            if (await TryClaimAsync(client, cancellationToken).ConfigureAwait(false))
            {
                return;
            }

            await RejectAsync(client, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or InvalidOperationException)
        {
            _logger.AcceptDropped(ex, _options.ComPort);
            client.Dispose();
        }
    }

    /// <summary>
    /// Makes the client the active one. If another client holds the port, either disconnects it
    /// (ReplaceExistingClient) or gives it a short grace period to finish releasing, which covers
    /// a client that closed and reconnected immediately.
    /// </summary>
    private async Task<bool> TryClaimAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var deadline = TimeProvider.GetUtcNow() + ClientReleaseGrace;
        var kicked = false;

        while (true)
        {
            TcpClient? holder = null;
            lock (_gate)
            {
                if (_client is null)
                {
                    _client = client;
                    _clientRemote = client.Client.RemoteEndPoint;
                    _clientSince = TimeProvider.GetUtcNow();
                    _clientTask = RunClientAsync(client, cancellationToken);
                    return true;
                }

                if (_options.ReplaceExistingClient && !kicked)
                {
                    holder = _client;
                    kicked = true;
                    _logger.ReplacingClient(_options.ComPort, _clientRemote, client.Client.RemoteEndPoint);
                }
            }

            holder?.Dispose();

            if (TimeProvider.GetUtcNow() >= deadline)
            {
                return false;
            }

            await Task.Delay(10, cancellationToken).ConfigureAwait(false);
        }
    }

    private async Task RejectAsync(TcpClient client, CancellationToken cancellationToken)
    {
        string holder;
        lock (_gate)
        {
            holder = _clientRemote is null
                ? "another client"
                : $"{_clientRemote} for {(TimeProvider.GetUtcNow() - _clientSince).TotalSeconds:0}s";
        }

        _logger.RejectedClient(_options.ComPort, client.Client.RemoteEndPoint, holder);

        // Raw bridges can carry a plain-text reason; RFC 2217 clients expect telnet framing, so they just see the close.
        if (_options.Protocol == BridgeProtocol.Raw)
        {
            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(2));
                var message = global::System.Text.Encoding.ASCII.GetBytes($"ser2net: port busy, held by {holder}\r\n");
                await client.GetStream().WriteAsync(message, timeout.Token).ConfigureAwait(false);
                client.Client.Shutdown(SocketShutdown.Send);
            }
            catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
            {
                // The peer is already gone; closing is all that is left.
            }
        }

        client.Dispose();
    }

    private async Task RunClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        var remote = client.Client.RemoteEndPoint;
        _logger.ClientConnected(_options.ComPort, remote);
        Rfc2217Session? session = null;
        using var clientCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Task modemTask = Task.CompletedTask;

        try
        {
            if (_options.Protocol == BridgeProtocol.Rfc2217)
            {
                session = new Rfc2217Session(_options, GetControl);
                lock (_gate)
                {
                    _session = session;
                }

                await WriteToClientAsync(client, session.GetGreeting(), cancellationToken).ConfigureAwait(false);
                modemTask = PollModemAsync(client, session, clientCts.Token);
            }

            await PumpClientAsync(client, session, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            // Shutdown.
        }
        catch (Exception ex) when (ex is IOException or SocketException or ObjectDisposedException or InvalidOperationException)
        {
            _logger.ClientError(ex, _options.ComPort);
        }
        finally
        {
            await clientCts.CancelAsync().ConfigureAwait(false);
            await modemTask.ConfigureAwait(false);

            session?.Reset();
            client.Dispose();
            ReleaseClient(client);

            _logger.ClientDisconnected(_options.ComPort, remote);
        }
    }

    private async Task PumpClientAsync(TcpClient client, Rfc2217Session? session, CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];
        var stream = client.GetStream();

        while (true)
        {
            var read = await stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                return;
            }

            if (session is null)
            {
                await WriteToSerialAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                continue;
            }

            var result = session.Process(buffer.AsSpan(0, read));
            if (result.Reply.Length > 0)
            {
                await WriteToClientAsync(client, result.Reply, cancellationToken).ConfigureAwait(false);
            }

            if (result.SerialData.Length > 0)
            {
                await WriteToSerialAsync(result.SerialData, cancellationToken).ConfigureAwait(false);
            }
        }
    }

    private void ReleaseClient(TcpClient client)
    {
        lock (_gate)
        {
            if (ReferenceEquals(_client, client))
            {
                _client = null;
                _clientRemote = null;
                _session = null;
            }
        }
    }
    private ISerialControl? GetControl()
    {
        lock (_gate)
        {
            return _link;
        }
    }

    private async Task WriteToClientAsync(TcpClient client, ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(ClientWriteTimeout);
        try
        {
            await _writeLock.WaitAsync(timeout.Token).ConfigureAwait(false);
            try
            {
                await client.GetStream().WriteAsync(data, timeout.Token).ConfigureAwait(false);
            }
            finally
            {
                _writeLock.Release();
            }
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            throw new IOException($"Client did not accept data within {ClientWriteTimeout.TotalSeconds:0.#}s.", ex);
        }
    }

    private async Task PollModemAsync(TcpClient client, Rfc2217Session session, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(250), TimeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                if (session.PollModemState() is { } frame)
                {
                    await WriteToClientAsync(client, frame, cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (Exception ex) when (ex is OperationCanceledException or IOException or SocketException or ObjectDisposedException or InvalidOperationException)
        {
            // Client left or shutdown; the read loop handles cleanup.
        }
    }

    private async Task WriteToSerialAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        ISerialLink? link;
        lock (_gate)
        {
            link = _link;
        }

        if (link is null)
        {
            _logger.SerialClosedDropped(_options.ComPort, data.Length);
            return;
        }

        try
        {
            await link.Stream.WriteAsync(data, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or InvalidOperationException or ObjectDisposedException or UnauthorizedAccessException)
        {
            // The serial loop notices the dead port on its next read and reopens it.
            _logger.SerialWriteFailed(ex, _options.ComPort, data.Length);
        }
    }

    private async Task RunSerialAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[BufferSize];

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var link = _serialFactory.Open(_options);
                using var registration = cancellationToken.Register(link.Dispose);

                lock (_gate)
                {
                    _link = link;
                }

                _logger.SerialOpen(_options.ComPort, _options);

                while (true)
                {
                    var read = await link.Stream.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                    if (read == 0)
                    {
                        throw new EndOfStreamException("Serial stream ended.");
                    }

                    await ForwardToClientAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.SerialUnavailable(_options.ComPort, ex.Message, _retryInterval.TotalSeconds);
            }
            finally
            {
                lock (_gate)
                {
                    _link = null;
                }
            }

            try
            {
                await Task.Delay(_retryInterval, cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task ForwardToClientAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken)
    {
        TcpClient? client;
        Rfc2217Session? session;
        lock (_gate)
        {
            client = _client;
            session = _session;
        }

        if (client is null)
        {
            return;
        }

        try
        {
            var payload = session is null ? data : Rfc2217Session.Escape(data);
            await WriteToClientAsync(client, payload, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is IOException or SocketException or InvalidOperationException or ObjectDisposedException)
        {
            _logger.ClientDropped(ex, _options.ComPort);
            client.Dispose();
        }
    }
}
