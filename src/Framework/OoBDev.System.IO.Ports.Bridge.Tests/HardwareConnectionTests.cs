using System.IO.Ports;
using System.Text;
using OoBDev.System.IO.Ports.Bridge.Tests.TestKit;
using static OoBDev.System.IO.Ports.Bridge.Rfc2217Codes;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

/// <summary>
/// The connection-management behaviour from docs/bugs/single-client-silent-reject.md, run against a real
/// serial port (default COM3, override with SER2NET_TEST_PORT). Skipped as inconclusive when the port is absent
/// or busy. Nothing is written to the device except where a test says so.
/// </summary>
[TestClass]
[TestCategory(TestCategories.Hardware)]
[TestCategory(TestCategories.Serial)]
public sealed class HardwareConnectionTests
{
    private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(5);

    private static string PortName => Environment.GetEnvironmentVariable("SER2NET_TEST_PORT") ?? "COM3";

    [TestMethod]
    public async Task BusyPortIsExplainedAndOwnerKeepsIt()
    {
        await using var harness = await StartAsync(grace: TimeSpan.FromMilliseconds(100));

        using var owner = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "owner registered");

        using var second = await harness.ConnectAsync();
        using var cts = new CancellationTokenSource(IoTimeout);
        var received = new MemoryStream();
        await second.GetStream().CopyToAsync(received, cts.Token);

        StringAssert.StartsWith(Encoding.ASCII.GetString(received.ToArray()), "ser2net: port busy, held by ");
        Assert.IsTrue(harness.Worker.IsClientConnected);
        Assert.IsTrue(harness.Worker.IsSerialOpen, "serial port must stay open when a client is refused");
    }

    [TestMethod]
    public async Task ImmediateReconnectLoopNeverGetsRefused()
    {
        await using var harness = await StartAsync(grace: TimeSpan.FromSeconds(2));

        for (var i = 0; i < 25; i++)
        {
            var first = await harness.ConnectAsync();
            await Wait.UntilAsync(() => harness.Worker.IsClientConnected, $"first client, round {i}");
            first.Dispose();

            using var next = await harness.ConnectAsync();
            // A refused client is closed by the bridge with the "port busy" text; an accepted one stays quiet.
            await Task.Delay(30);
            Assert.IsTrue(next.Connected && !(next.Client.Poll(0, global::System.Net.Sockets.SelectMode.SelectRead) && next.Available > 0), $"round {i}: refused");
            await Wait.UntilAsync(() => harness.Worker.IsClientConnected, $"next client registered, round {i}");
        }
    }

    [TestMethod]
    public async Task ReplaceExistingClientHandsThePortToTheNewestClient()
    {
        await using var harness = await StartAsync(replaceExisting: true, grace: TimeSpan.FromSeconds(2));

        using var older = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "older registered");
        using var newer = await harness.ConnectAsync();

        using var cts = new CancellationTokenSource(IoTimeout);
        var read = await ReadOrResetAsync(older, cts.Token);
        Assert.AreEqual(0, read, "older client should be disconnected");
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "newer registered");
    }

    [TestMethod]
    public async Task Rfc2217SettingsReachTheRealPortAndAreRestoredOnDisconnect()
    {
        await using var harness = await StartAsync(protocol: BridgeProtocol.Rfc2217);

        var client = await harness.ConnectAsync();
        var stream = client.GetStream();
        using var cts = new CancellationTokenSource(IoTimeout);
        await stream.ReadExactlyAsync(new byte[15], cts.Token); // greeting: five 3-byte offers

        // Ask for 9600 baud, then 19200; the reply carries the value the real driver reports back.
        Assert.AreEqual(9600, await SetBaudAsync(stream, 9600, cts.Token));
        Assert.AreEqual(19200, await SetBaudAsync(stream, 19200, cts.Token));

        client.Dispose();
        await Wait.UntilAsync(() => !harness.Worker.IsClientConnected, "client released");

        // A fresh session should see the configured rate again (the previous client's change was reset).
        using var again = await harness.ConnectAsync();
        var againStream = again.GetStream();
        await againStream.ReadExactlyAsync(new byte[15], cts.Token);
        await againStream.WriteAsync(new byte[] { Iac, Sb, OptionComPort, SetBaudRate, 0, 0, 0, 0, Iac, Se }, cts.Token); // 0 = query
        Assert.AreEqual(115200, ReadBaudReply(await ReadSubnegotiationAsync(againStream, cts.Token)));
    }

    [TestMethod]
    public async Task LoopbackEchoWorksThroughTheBridgeWhenTxIsWiredToRx()
    {
        await using var harness = await StartAsync();

        using var client = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");
        var stream = client.GetStream();
        await stream.WriteAsync(new byte[] { 0x55, 0xAA, 0x01 });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(1));
        var echoed = new byte[3];
        try
        {
            await stream.ReadExactlyAsync(echoed, cts.Token);
        }
        catch (OperationCanceledException)
        {
            Assert.Inconclusive($"{PortName} has no TX-RX loopback wired, so there is nothing to echo.");
        }

        CollectionAssert.AreEqual(new byte[] { 0x55, 0xAA, 0x01 }, echoed);
    }

    private static async Task<BridgeHarness> StartAsync(
        BridgeProtocol protocol = BridgeProtocol.Raw,
        bool replaceExisting = false,
        TimeSpan? grace = null)
    {
        if (!SerialPort.GetPortNames().Contains(PortName, StringComparer.OrdinalIgnoreCase))
        {
            Assert.Inconclusive($"{PortName} is not present.");
        }

        try
        {
            using var probe = new SystemSerialPortFactory().Open(new BridgeOptions { ComPort = PortName });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            Assert.Inconclusive($"{PortName} cannot be opened (in use?): {ex.Message}");
        }

        var harness = new BridgeHarness(new SystemSerialPortFactory(), retry: TimeSpan.FromMilliseconds(200), protocol: protocol, replaceExisting: replaceExisting, grace: grace, comPort: PortName);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "real serial port open");
        return harness;
    }

    private static async Task<int> ReadOrResetAsync(global::System.Net.Sockets.TcpClient client, CancellationToken token)
    {
        try
        {
            return await client.GetStream().ReadAsync(new byte[1], token);
        }
        catch (IOException)
        {
            return 0;
        }
    }

    private static async Task<int> SetBaudAsync(Stream stream, int baud, CancellationToken token)
    {
        await stream.WriteAsync(new byte[] { Iac, Sb, OptionComPort, SetBaudRate, (byte)(baud >> 24), (byte)(baud >> 16), (byte)(baud >> 8), (byte)baud, Iac, Se }, token);
        return ReadBaudReply(await ReadSubnegotiationAsync(stream, token));
    }

    private static int ReadBaudReply(byte[] payload) =>
        (payload[0] << 24) | (payload[1] << 16) | (payload[2] << 8) | payload[3];

    /// <summary>Reads until a COM-PORT baud reply arrives and returns its 4 data bytes.</summary>
    private static async Task<byte[]> ReadSubnegotiationAsync(Stream stream, CancellationToken token)
    {
        var window = new List<byte>();
        var one = new byte[1];
        var header = new byte[] { Iac, Sb, OptionComPort, SetBaudRate + ServerOffset };
        while (true)
        {
            await stream.ReadExactlyAsync(one, token);
            window.Add(one[0]);
            var index = IndexOf(window, header);
            if (index >= 0 && window.Count >= index + header.Length + 6)
            {
                return window.Skip(index + header.Length).Take(4).ToArray();
            }
        }
    }

    private static int IndexOf(List<byte> data, byte[] pattern)
    {
        for (var i = 0; i + pattern.Length <= data.Count; i++)
        {
            if (data.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
            {
                return i;
            }
        }

        return -1;
    }
}
