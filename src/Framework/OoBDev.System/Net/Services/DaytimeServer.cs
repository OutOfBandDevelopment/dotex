using OoBDev.System.Net.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Services;

/// <summary>
/// Implements a Daytime Protocol server that returns the current date and time to connected clients.
/// This is a simple TCP service that responds to any client message with the current system date and time.
/// </summary>
/// <param name="ipAddress">The IP address to bind to. If null, binds to all available network interfaces.</param>
/// <param name="port">The port number to listen on. Defaults to 13 (standard daytime port).</param>
public class DaytimeServer(IPAddress? ipAddress = default, ushort port = 13) : ServerBase(ipAddress, port)
{
    /// <summary>
    /// Handles incoming messages by responding with the current date and time in UTF-8 encoded format.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <param name="accepted">The TCP client connection.</param>
    /// <param name="message">The message received from the client (not used in this implementation).</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken)
    {
        Memory<byte> buffer = Encoding.UTF8.GetBytes(DateTimeOffset.Now.ToString());
        await accepted.GetStream().WriteAsync(buffer, cancellationToken);
    }
}
