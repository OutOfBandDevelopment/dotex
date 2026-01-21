using OoBDev.System.Net.Sockets;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Services;

/// <summary>
/// Implements a Discard Protocol server that accepts and discards all incoming data without response.
/// This is a simple TCP service that receives data from clients but does not send any response, commonly used for testing and network diagnostics.
/// </summary>
/// <param name="ipAddress">The IP address to bind to. If null, binds to all available network interfaces.</param>
/// <param name="port">The port number to listen on. Defaults to 9 (standard discard port).</param>
public class DiscardServer(IPAddress? ipAddress = default, ushort port = 9) : ServerBase(ipAddress, port)
{
    /// <summary>
    /// Handles incoming messages by silently discarding them without any response or processing.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <param name="accepted">The TCP client connection.</param>
    /// <param name="message">The message received from the client (discarded).</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A completed task, as this server does not process or respond to messages.</returns>
    protected override Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
