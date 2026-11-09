using OoBDev.System.IO.Ports.Bridge.Tests.TestKit;
using static OoBDev.System.IO.Ports.Bridge.Rfc2217Codes;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Ports.Bridge.Tests;

[TestClass]
[TestCategory(TestCategories.Simulate)]
[TestCategory(TestCategories.Tcp)]
[TestCategory(TestCategories.Rfc2217)]
public sealed class Rfc2217BridgeTests
{
    private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds(5);

    [TestMethod]
    public async Task ClientGetsGreetingAndSerialDataIsEscaped()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial, protocol: BridgeProtocol.Rfc2217);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var client = await harness.ConnectAsync();
        var stream = client.GetStream();
        using var cts = new CancellationTokenSource(IoTimeout);

        var greeting = new byte[3];
        await stream.ReadExactlyAsync(greeting, cts.Token);
        CollectionAssert.AreEqual(new byte[] { Iac, Will, OptionComPort }, greeting);

        // Remaining four option offers (BINARY x2, SGA x2) at three bytes each.
        await stream.ReadExactlyAsync(new byte[12], cts.Token);

        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");
        await serial.Latest!.DeviceSendAsync([0x41, Iac, 0x42]);

        var data = new byte[4];
        await stream.ReadExactlyAsync(data, cts.Token);
        CollectionAssert.AreEqual(new byte[] { 0x41, Iac, Iac, 0x42 }, data);
    }

    [TestMethod]
    public async Task ClientBaudRequestChangesPortAndIsRestoredOnDisconnect()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial, protocol: BridgeProtocol.Rfc2217);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");
        var link = serial.Latest!;

        var client = await harness.ConnectAsync();
        await Wait.UntilAsync(() => harness.Worker.IsClientConnected, "client registered");
        await client.GetStream().WriteAsync(new byte[] { Iac, Sb, OptionComPort, SetBaudRate, 0, 0, 0x25, 0x80, Iac, Se });
        await Wait.UntilAsync(() => link.BaudRate == 9600, "baud changed");

        client.Dispose();
        await Wait.UntilAsync(() => link.BaudRate == 115200, "baud restored");
    }

    [TestMethod]
    public async Task ClientPayloadReachesDeviceWithoutTelnetFraming()
    {
        var serial = new FakeSerialPortFactory();
        await using var harness = new BridgeHarness(serial, protocol: BridgeProtocol.Rfc2217);
        await Wait.UntilAsync(() => harness.Worker.IsSerialOpen, "serial open");

        using var client = await harness.ConnectAsync();
        await client.GetStream().WriteAsync(new byte[] { 1, Iac, Iac, Iac, Do, OptionBinary, 2 });

        var received = await serial.Latest!.DeviceReceiveAsync(3, IoTimeout);
        CollectionAssert.AreEqual(new byte[] { 1, Iac, 2 }, received);
    }

    [TestMethod]
    public async Task AdvertiserIsCalledWithBoundPortAndWithdrawnOnShutdown()
    {
        var advertiser = new RecordingAdvertiser();
        var harness = new BridgeHarness(new FakeSerialPortFactory(), advertiser: advertiser);

        Assert.HasCount(1, advertiser.Advertised);
        Assert.AreEqual(harness.Worker.BoundPort, advertiser.Advertised[0].Port);
        Assert.AreEqual("bench", advertiser.Advertised[0].Bridge.DisplayName);
        Assert.AreEqual(0, advertiser.Withdrawn);

        await harness.DisposeAsync();

        Assert.AreEqual(1, advertiser.Withdrawn);
    }

    private sealed class RecordingAdvertiser : IServiceAdvertiser
    {
        public List<(BridgeOptions Bridge, int Port)> Advertised { get; } = [];

        public int Withdrawn { get; private set; }

        public IDisposable Advertise(BridgeOptions bridge, int port)
        {
            Advertised.Add((bridge, port));
            return new Withdraw(this);
        }

        private sealed class Withdraw(RecordingAdvertiser owner) : IDisposable
        {
            public void Dispose() => owner.Withdrawn++;
        }
    }
}
