using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Scpi.Tests;

/// <summary>A bench instrument behind a serial-to-Ethernet bridge, written one byte at a time because the instrument UART drops bursts.</summary>
internal sealed class BenchTcpInstrument : IDisposable
{
    private readonly TcpClient _client;
    private readonly NetworkStream _stream;
    private readonly TimeSpan _byteDelay;

    private BenchTcpInstrument(TcpClient client, TimeSpan byteDelay)
    {
        _client = client;
        _stream = client.GetStream();
        _byteDelay = byteDelay;
    }

    /// <summary>Connects to the host named by <paramref name="hostParameter"/>, or ends the test Inconclusive when it is blank or unreachable.</summary>
    public static async Task<BenchTcpInstrument> RequireAsync(TestContext testContext, string hostParameter, TimeSpan byteDelay)
    {
        ArgumentNullException.ThrowIfNull(testContext);

        var host = testContext.GetPropertyOrDefault(hostParameter, string.Empty);
        if (string.IsNullOrWhiteSpace(host))
            Assert.Inconclusive($"No '{hostParameter}' run parameter: set it in .runsettings to the bridge address of the instrument.");

        var port = testContext.GetPropertyOrDefault("TCP_DEVICE_PORT", 23);
        var client = new TcpClient();
        try
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            await client.ConnectAsync(host, port, timeout.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is SocketException or OperationCanceledException)
        {
            client.Dispose();
            Assert.Inconclusive($"{host}:{port} is not reachable: is the instrument online?");
        }

        return new BenchTcpInstrument(client, byteDelay);
    }

    /// <summary>Sends <paramref name="command"/> plus <paramref name="terminator"/> and returns everything received until the line goes quiet.</summary>
    public async Task<string> QueryAsync(string command, string terminator, TimeSpan quiet)
    {
        await DrainAsync(TimeSpan.FromMilliseconds(300)).ConfigureAwait(false);

        foreach (var ch in command + terminator)
        {
            await _stream.WriteAsync(new[] { (byte)ch }).ConfigureAwait(false);
            await Task.Delay(_byteDelay).ConfigureAwait(false);
        }

        return await DrainAsync(quiet).ConfigureAwait(false);
    }

    private async Task<string> DrainAsync(TimeSpan quiet)
    {
        var text = new StringBuilder();
        var buffer = new byte[256];
        while (true)
        {
            using var timeout = new CancellationTokenSource(quiet);
            try
            {
                var read = await _stream.ReadAsync(buffer, timeout.Token).ConfigureAwait(false);
                if (read == 0)
                    break;
                text.Append(Encoding.Latin1.GetString(buffer, 0, read));
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        return text.ToString();
    }

    public void Dispose()
    {
        _stream.Dispose();
        _client.Dispose();
    }
}
