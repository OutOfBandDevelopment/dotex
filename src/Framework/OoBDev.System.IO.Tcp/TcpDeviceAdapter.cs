using System;
using System.IO;
using System.Net.Sockets;

namespace OoBDev.System.IO.Tcp;

/// <summary>A device reached over a TCP socket (a SCPI instrument on port 5025, a serial-to-Ethernet bridge, a raw socket); the stream is the connection.</summary>
/// <param name="host">The host name or address.</param>
/// <param name="port">The TCP port.</param>
/// <param name="connectTimeout">How long to wait for the connection; <see cref="TimeSpan.Zero"/> means five seconds.</param>
public sealed class TcpDeviceAdapter(string host, int port, TimeSpan connectTimeout = default) : IDeviceAdapter, IDisposable
{
    private readonly string _host = string.IsNullOrWhiteSpace(host) ? throw new ArgumentException("A host is required.", nameof(host)) : host;
    private readonly TimeSpan _connectTimeout = connectTimeout == default ? TimeSpan.FromSeconds(5) : connectTimeout;
    private NetworkStream? _stream;

    /// <inheritdoc />
    public string Type => "Tcp";

    /// <inheritdoc />
    public string Path => $"{_host}:{port}";

    /// <inheritdoc />
    public Stream Stream => _stream ?? throw new InvalidOperationException($"{Path} is not open; call {nameof(TryOpen)} first.");

    /// <inheritdoc />
    public bool TryOpen(out Stream? stream)
    {
        if (_stream is { } open)
        {
            stream = open;
            return true;
        }

        var client = new TcpClient { NoDelay = true };
        try
        {
            if (!client.ConnectAsync(_host, port).Wait(_connectTimeout))
            {
                client.Dispose();
                stream = null;
                return false;
            }

            _stream = new NetworkStream(client.Client, ownsSocket: true);
            stream = _stream;
            return true;
        }
        catch (Exception ex) when (ex is SocketException or AggregateException or ObjectDisposedException)
        {
            client.Dispose();
            stream = null;
            return false;
        }
    }

    /// <summary>Closes the connection if it is open.</summary>
    public void Dispose() => _stream?.Dispose();
}
