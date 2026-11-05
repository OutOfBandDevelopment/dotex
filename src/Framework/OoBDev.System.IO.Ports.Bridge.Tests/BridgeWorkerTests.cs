using OoBDev.System.IO.Ports.Bridge.Tests.TestKit;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Simulate)]
[TestCategory(TestCategories.Tcp)]
public sealed class BridgeWorkerTests
{
    private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(5);

    [TestMethod]
    public async Task ClientBytesReachSerialDevice()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var client = await harness.ConnectAsync();
        await client.GetStream().WriteAsync(new byte[] { 1, 2, 3 });

        var received = await serial.Latest!.DeviceReceiveAsync(3, IoTimeout);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, received);
    }

    [TestMethod]
    public async Task SerialBytesReachClient()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var client = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");
        await serial.Latest!.DeviceSendAsync(new byte[] { 9, 8, 7 });

        var buffer = new byte[3];
        using var cts = new CancellationTokenSource(IoTimeout);
        await client.GetStream().ReadExactlyAsync(buffer, cts.Token);
        CollectionAssert.AreEqual(new byte[] { 9, 8, 7 }, buffer);
    }

    [TestMethod]
    public async Task SerialPortStaysOpenAcrossClientDisconnectAndNextClientWorks()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        var first = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "first client");
        first.Dispose();
        await Wait.UntilAsync(() => !harness.Worker.IsClientConnected, "first client gone");

        Assert.IsTrue(harness.Worker.IsSerialOpen);
        Assert.HasCount(1, serial.Links, "serial port should not have been reopened");

        using var second = await harness.ConnectAsync();
        await second.GetStream().WriteAsync(new byte[] { 5 });
        CollectionAssert.AreEqual(new byte[] { 5 }, await serial.Latest!.DeviceReceiveAsync(1, IoTimeout));
    }

    [TestMethod]
    public async Task SecondConcurrentClientIsRejectedWithoutDisturbingFirst()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var first = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "first client");

        using var second = await harness.ConnectAsync();
        using var cts = new CancellationTokenSource(IoTimeout);
        await second.GetStream().CopyToAsync(Stream.Null, cts.Token); // completes only when the bridge closes the connection

        await first.GetStream().WriteAsync(new byte[] { 42 });
        CollectionAssert.AreEqual(new byte[] { 42 }, await serial.Latest!.DeviceReceiveAsync(1, IoTimeout));
    }

    [TestMethod]
    public async Task SerialUnplugReopensPortAndKeepsClientConnected()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var client = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");

        serial.Latest!.Unplug();
        await Wait.UntilAsync(() => serial.Links.Count == 2, "serial reopened");
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open again");
        Assert.IsTrue(harness.Worker.IsClientConnected);

        await serial.Latest!.DeviceSendAsync(new byte[] { 7 });
        var buffer = new byte[1];
        using var cts = new CancellationTokenSource(IoTimeout);
        await client.GetStream().ReadExactlyAsync(buffer, cts.Token);
        Assert.AreEqual(7, buffer[0]);
    }

    [TestMethod]
    public async Task RetriesWhileDeviceIsAbsent()
    {
        var serial = new FakeSerialPortFactory(failFirstOpens: 3);
        await using var harness = new BridgeHarness(serial);

        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open after retries");
        Assert.IsGreaterThanOrEqualTo(4, serial.OpenAttempts);
    }

    [TestMethod]
    public async Task StopClosesSerialPortAndSocket()
    {
        var serial = new FakeSerialPortFactory();
        var harness = new BridgeHarness(serial);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var client = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");

        await harness.DisposeAsync();

        Assert.IsTrue(serial.Latest!.IsDisposed);
        var read = await client.GetStream().ReadAsync(new byte[1]);
        Assert.AreEqual(0, read);
    }
}
