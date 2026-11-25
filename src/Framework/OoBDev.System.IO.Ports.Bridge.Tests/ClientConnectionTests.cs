using System.Net.Sockets;
using System.Text;
using OoBDev.System.IO.Ports.Bridge.Tests.TestKit;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Simulate)]
[TestCategory(TestCategories.Tcp)]
public sealed class ClientConnectionTests
{
    private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(5);

    [TestMethod]
    public async Task RejectedClientIsToldWhyAndThenClosed()
    {
        await using var harness = new BridgeHarness(new FakeSerialPortFactory(), grace: TimeSpan.FromMilliseconds(50));
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var holder = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "holder registered");

        using var second = await harness.ConnectAsync();
        using var cts = new CancellationTokenSource(IoTimeout);
        var received = new MemoryStream();
        await second.GetStream().CopyToAsync(received, cts.Token); // ends when the bridge closes

        var text = Encoding.ASCII.GetString(received.ToArray());
        StringAssert.StartsWith(text, "ser2net: port busy, held by ");
        Assert.IsTrue(harness.Worker.IsClientConnected, "the original client must keep the port");
    }

    [TestMethod]
    public async Task ReconnectingImmediatelyAfterCloseIsAccepted()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial, grace: TimeSpan.FromSeconds(2));
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        for (var i = 0; i < 20; i++)
        {
            var first = await harness.ConnectAsync();
            first.Dispose();

            using var next = await harness.ConnectAsync();
            await next.GetStream().WriteAsync(new byte[] { (byte)i });
            var received = await serial.Latest!.DeviceReceiveAsync(1, IoTimeout);
            CollectionAssert.AreEqual(new[] { (byte)i }, received, $"round {i}");
        }
    }

    [TestMethod]
    public async Task ReplaceExistingClientDisconnectsTheOldOneAndServesTheNewOne()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial, replaceExisting: true, grace: TimeSpan.FromSeconds(2));
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var older = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "older registered");

        using var newer = await harness.ConnectAsync();
        using var cts = new CancellationTokenSource(IoTimeout);
        var probe = new byte[1];
        var read = await ReadOrResetAsync(older, probe, cts.Token);
        Assert.AreEqual(0, read, "the older client should be disconnected");

        await newer.GetStream().WriteAsync(new byte[] { 7, 8 });
        var received = await serial.Latest!.DeviceReceiveAsync(2, IoTimeout);
        CollectionAssert.AreEqual(new byte[] { 7, 8 }, received);
    }

    [TestMethod]
    public async Task StalledClientIsDroppedAndSerialReadingContinues()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial, writeTimeout: TimeSpan.FromMilliseconds(300));
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var stalled = await harness.ConnectAsync(); // never reads
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");

        // The Pipe behind the fake serial port pauses writers when nobody reads it, so every send below
        // only completes while the bridge keeps draining the serial side.
        var chunk = new byte[64 * 1024];
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        for (var i = 0; i < 400 && !cts.IsCancellationRequested; i++)
        {
            await serial.Latest!.DeviceSendAsync(chunk).WaitAsync(cts.Token);
        }

        await Wait.UntilAsync(() => !harness.Worker.IsClientConnected, "stalled client dropped");
    }

    [TestMethod]
    public void AcceptedSocketsGetKeepAlive()
    {
        using var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        BridgeWorker.ConfigureClientSocket(socket);

        Assert.AreNotEqual(0, (int)socket.GetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive)!);
        Assert.IsTrue(socket.NoDelay);
    }

    private static async Task<int> ReadOrResetAsync(TcpClient client, byte[] buffer, CancellationToken token)
    {
        try
        {
            return await client.GetStream().ReadAsync(buffer, token);
        }
        catch (IOException)
        {
            return 0; // a reset also means the bridge dropped us
        }
    }
}
