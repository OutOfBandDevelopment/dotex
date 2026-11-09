using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>
/// A TCP listener that keeps accepting: every inbound peer becomes its own, already open
/// <see cref="IDeviceAdapter"/> (one tab per peer in a front end), unlike
/// <see cref="TcpEndpointMode.Listener"/> on <see cref="TcpEndpointDeviceAdapter"/>, which takes one peer
/// and stops. See docs/design/Transports/transports.md (listener mode).
/// </summary>
public sealed class TcpPeerListener : IAsyncDisposable
{
    private readonly TcpListener _listener;

    /// <summary>Binds <paramref name="host"/> (any address when empty) on <paramref name="port"/> (0 picks a free port) and starts listening.</summary>
    /// <param name="host">The local address or host name to bind, or empty for any.</param>
    /// <param name="port">The local port, or 0 for a free one.</param>
    public TcpPeerListener(string? host, int port)
    {
        var address = string.IsNullOrWhiteSpace(host) ? IPAddress.IPv6Any : ResolveBindAddress(host);
        _listener = new TcpListener(address, port);
        if (address.Equals(IPAddress.IPv6Any))
        {
            _listener.Server.DualMode = true;
        }

        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
    }

    /// <summary>The bound local port (the chosen one when 0 was requested).</summary>
    public int Port { get; }

    /// <summary>Yields one open adapter per accepted peer until cancelled or disposed.</summary>
    /// <param name="cancellationToken">Stops accepting.</param>
    public async IAsyncEnumerable<TcpPeer> AcceptPeersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                yield break;
            }
            catch (ObjectDisposedException)
            {
                yield break;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                yield break;
            }

            var remote = (IPEndPoint)client.Client.RemoteEndPoint!;
            var connection = new SystemTcpConnection(client);
            var options = new TcpEndpointOptions { Mode = TcpEndpointMode.Listener, Host = remote.Address.ToString(), Port = remote.Port };
            var adapter = new TcpEndpointDeviceAdapter(new AcceptedConnectionSource(connection), Options.Create(options));
            await adapter.OpenAsync(cancellationToken).ConfigureAwait(false);
            yield return new TcpPeer(remote.ToString(), adapter);
        }
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        _listener.Stop();
        return ValueTask.CompletedTask;
    }

    private static IPAddress ResolveBindAddress(string host) =>
        IPAddress.TryParse(host, out var literal)
            ? literal
            : Dns.GetHostAddresses(host) is { Length: > 0 } found ? found[0] : throw new SocketException((int)SocketError.HostNotFound);

    private sealed class AcceptedConnectionSource(ITcpConnection connection) : ITcpConnectionSource
    {
        public Task<ITcpConnection> ConnectAsync(TcpEndpointOptions options, CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<ITcpConnection> AcceptAsync(TcpEndpointOptions options, CancellationToken cancellationToken) => Task.FromResult(connection);
    }
}
