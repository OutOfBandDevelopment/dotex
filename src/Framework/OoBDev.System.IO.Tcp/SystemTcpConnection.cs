using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp;

/// <summary>
/// <see cref="ITcpConnection"/> backed by a real <see cref="TcpClient"/>. Deliberately thin —
/// logic worth unit testing belongs in <see cref="TcpEndpointDeviceAdapter"/>, which depends on the
/// interface instead of this class.
/// </summary>
public sealed class SystemTcpConnection : ITcpConnection
{
    private readonly TcpClient _client;

    public SystemTcpConnection(TcpClient client)
    {
        ArgumentNullException.ThrowIfNull(client);

        _client = client;
        Stream = client.GetStream();
    }

    public Stream Stream { get; }

    public Task WriteAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken) => Stream.WriteAsync(data, cancellationToken).AsTask();

    public void Dispose()
    {
        Stream.Dispose();
        _client.Dispose();
    }
}
