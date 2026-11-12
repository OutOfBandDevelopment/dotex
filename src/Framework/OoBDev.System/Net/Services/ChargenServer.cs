using OoBDev.System.Net.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.Net.Services;

/// <summary>
/// Implements a Character Generator (chargen) server that sends random data to connected clients.
/// This is a simple TCP service that continuously generates and sends random byte sequences, commonly used for testing and network diagnostics.
/// </summary>
/// <param name="ipAddress">The IP address to bind to. If null, binds to all available network interfaces.</param>
/// <param name="port">The port number to listen on. Defaults to 19 (standard chargen port).</param>
/// <param name="maxBytesPerClient">Stop sending to a client after this many bytes (0 means no limit), so a forgotten connection cannot flood a session.</param>
public class ChargenServer(IPAddress? ipAddress = default, ushort port = 19, long maxBytesPerClient = ChargenServer.DefaultMaxBytesPerClient) : ServerBase(ipAddress, port)
{
    /// <summary>The default per-client byte limit (64 KiB).</summary>
    public const long DefaultMaxBytesPerClient = 64 * 1024;

    private readonly Dictionary<int, long> _sent = [];

    /// <summary>
    /// Starts the server and begins sending random data to connected clients at random intervals.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to stop the server.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    protected override async Task OnStartAsync(CancellationToken cancellationToken)
    {
        var rand = new Random();
        var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        while (!cancellationToken.IsCancellationRequested)
        {
            foreach (var client in Clients.ToArray())
            {
                try
                {
                    if (!client.Value.Connected)
                        continue;

                    var sent = _sent.GetValueOrDefault(client.Key);
                    if (maxBytesPerClient > 0 && sent >= maxBytesPerClient)
                        continue;

                    if (rand.NextDouble() > 0.5)
                        continue;
                    Memory<byte> buffer = Guid.NewGuid().ToByteArray();
                    await SendAsync(client.Value, buffer, cts.Token).ConfigureAwait(false);
                    _sent[client.Key] = sent + buffer.Length;
                }
                catch (OperationCanceledException ocex)
                {
                    Console.WriteLine($"{GetType()}::ChargenServer::Canceled: {Environment.CurrentManagedThreadId} ({ocex.Message})");
                }
            }

            await Task.Delay(rand.Next(1, 10) * 100, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Handles incoming messages from clients. The chargen server does not process received messages.
    /// </summary>
    /// <param name="clientId">The unique identifier for the client.</param>
    /// <param name="accepted">The TCP client connection.</param>
    /// <param name="message">The message received from the client.</param>
    /// <param name="cancellationToken">A cancellation token for the operation.</param>
    /// <returns>A completed task, as this server does not process incoming messages.</returns>
    protected override Task MessageReceivedAsync(int clientId, TcpClient accepted, Memory<byte> message, CancellationToken cancellationToken) =>
        Task.CompletedTask;
}
