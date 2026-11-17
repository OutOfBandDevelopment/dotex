using System;
using System.Buffers.Binary;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.ClamAv.Tests;

/// <summary>A loopback stand-in for clamd that records the INSTREAM payload and sends a fixed reply.</summary>
internal sealed class FakeClamd : IAsyncDisposable
{
    private readonly TcpListener listener = new(IPAddress.Loopback, 0);
    private readonly Task serving;

    public FakeClamd(string reply)
    {
        listener.Start();
        serving = Serve(reply);
    }

    public int Port => ((IPEndPoint)listener.LocalEndpoint).Port;

    public string Command { get; private set; } = string.Empty;

    public int Chunks { get; private set; }

    public byte[] Payload { get; private set; } = [];

    private async Task Serve(string reply)
    {
        using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
        var stream = client.GetStream();
        var command = new StringBuilder();
        var one = new byte[1];
        while (await stream.ReadAsync(one).ConfigureAwait(false) == 1 && one[0] != 0)
        {
            command.Append((char)one[0]);
        }

        Command = command.ToString();
        using var payload = new MemoryStream();
        var header = new byte[4];
        while (true)
        {
            await stream.ReadExactlyAsync(header).ConfigureAwait(false);
            var length = BinaryPrimitives.ReadInt32BigEndian(header);
            if (length == 0)
            {
                break;
            }

            var chunk = new byte[length];
            await stream.ReadExactlyAsync(chunk).ConfigureAwait(false);
            payload.Write(chunk);
            Chunks++;
        }

        Payload = payload.ToArray();
        await stream.WriteAsync(Encoding.ASCII.GetBytes(reply + "\0")).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        listener.Stop();
        try
        {
            await serving.ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is SocketException or ObjectDisposedException or EndOfStreamException or IOException)
        {
            // The scanner under test may have disconnected first.
        }
    }
}
