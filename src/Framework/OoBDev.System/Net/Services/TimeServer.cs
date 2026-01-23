using OoBDev.System.Net.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Services;

/// <summary>
/// Implements an RFC 868 Time Protocol server that provides the current time as seconds since 1900-01-01.
/// Listens on port 37 by default and responds to client requests with a 32-bit integer representing the current time.
/// </summary>
/// <param name="ipAddress">The IP address to bind to. Defaults to all available interfaces.</param>
/// <param name="port">The port to listen on. Defaults to 37 (standard Time Protocol port).</param>
public class TimeServer(IPAddress? ipAddress = default, ushort port = 37) : ServerBase(ipAddress, port)
{
    /// <summary>
    /// Handles incoming client connections by sending the current time as seconds since 1900-01-01 00:00:00 UTC.
    /// </summary>
    /// <param name="clientId">The unique identifier for this client connection.</param>
    /// <param name="accepted">The TCP client that connected.</param>
    /// <param name="message">The message received from the client (not used in Time Protocol).</param>
    /// <param name="cancellationToken">Cancellation token for aborting the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken)
    {
        var timeDiff = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - new DateTimeOffset(1900, 1, 1, 0, 0, 0, new TimeSpan(0, 0, 0)).ToUnixTimeSeconds();
        Memory<byte> buffer = BitConverter.GetBytes((int)timeDiff);
        await accepted.GetStream().WriteAsync(buffer);
    }
}
