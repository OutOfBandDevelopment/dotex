using OoBDev.System.Net.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Services;

/// <summary>
/// Implements an Echo Protocol server that echoes back any data received from clients.
/// This is a simple TCP service that responds to clients by sending back exactly what was received, commonly used for testing and network diagnostics.
/// </summary>
/// <param name="ipAddress">The IP address to bind to. If null, binds to all available network interfaces.</param>
/// <param name="port">The port number to listen on. Defaults to 7 (standard echo port).</param>
public class EchoServer(IPAddress? ipAddress = default, ushort port = 7) : ServerBase(ipAddress, port)
{
    /// <summary>
    /// Handles incoming messages by echoing them back to the client unchanged.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <param name="accepted">The TCP client connection.</param>
    /// <param name="message">The message received from the client, which will be echoed back.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken) => await accepted.GetStream().WriteAsync(message, cancellationToken);
}
