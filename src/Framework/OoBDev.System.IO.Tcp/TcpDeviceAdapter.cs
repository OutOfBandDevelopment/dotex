using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.System.IO.Tcp;

/// <summary>A device reached over a TCP socket (a SCPI instrument on port 5025, a serial-to-Ethernet bridge, a raw socket); the stream is the connection.</summary>
/// <param name="host">The host name or address.</param>
/// <param name="port">The TCP port.</param>
/// <param name="connectTimeout">How long to wait for the connection; <see cref="TimeSpan.Zero"/> means five seconds.</param>
public sealed class TcpDeviceAdapter(string host, int port, TimeSpan connectTimeout = default) : StreamDeviceAdapter
{
    private readonly string _host = string.IsNullOrWhiteSpace(host) ? throw new ArgumentException("A host is required.", nameof(host)) : host;
    private readonly TimeSpan _connectTimeout = connectTimeout == default ? TimeSpan.FromSeconds(5) : connectTimeout;

    /// <inheritdoc />
    public override string Type => "Tcp";

    /// <inheritdoc />
    public override string Path => $"{_host}:{port}";

    /// <inheritdoc />
    protected override Task<Stream?> ConnectAsync(CancellationToken cancellationToken) =>
        ConnectStreamAsync(_host, port, _connectTimeout, cancellationToken);

    internal static async Task<Stream?> ConnectStreamAsync(string host, int port, TimeSpan connectTimeout, CancellationToken cancellationToken)
    {
        var client = new TcpClient { NoDelay = true };
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(connectTimeout);
            await client.ConnectAsync(host, port, timeout.Token).ConfigureAwait(false);
            return new NetworkStream(client.Client, ownsSocket: true);
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException or ObjectDisposedException && !cancellationToken.IsCancellationRequested)
        {
            client.Dispose();
            return null;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }
}
