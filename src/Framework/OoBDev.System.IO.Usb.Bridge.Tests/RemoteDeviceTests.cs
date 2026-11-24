using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

[TestClass]
public sealed class RemoteDeviceTests
{
    public TestContext TestContext { get; set; } = null!;

    private static UsbBridgeServer Start(UsbBridgeOptions options, FakeTmcFactory? tmc, FakeHidFactory? hid, CancellationToken token, out Task run)
    {
        var server = new UsbBridgeServer(options with { BindAddress = "127.0.0.1", TcpPort = 0 }, tmc, hid, NullLogger.Instance);
        server.Start();
        run = server.RunAsync(token);
        return server;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RemoteTmc_WriteAndRead_GoThroughTheBridge()
    {
        // Stage
        using var cts = new CancellationTokenSource();
        var device = new FakeUsbTmcDevice { Reply = Encoding.ASCII.GetBytes("RIGOL,DM3058E") };
        using var server = Start(new UsbBridgeOptions { UsbVendorId = "1AB1", UsbProductId = "09C4" }, new FakeTmcFactory(device), null, cts.Token, out var run);
        using var remote = new RemoteUsbTmcDevice("127.0.0.1", server.BoundPort);
        // Test
        remote.Open();
        remote.WriteBulkOut([1, 2]);
        var buffer = new byte[64];
        var count = remote.ReadBulkIn(buffer, out var stalled);
        remote.SetRemote(true);
        // Assert
        Assert.IsTrue(remote.IsOpen);
        Assert.AreEqual(4096, remote.MaxTransferSize);
        Assert.AreEqual(64, remote.MaxPacketSize);
        Assert.AreEqual("RIGOL,DM3058E", Encoding.ASCII.GetString(buffer, 0, count));
        Assert.IsFalse(stalled);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, device.Written[0]);
        CollectionAssert.AreEqual(new[] { true }, device.RemoteCalls);
        remote.Close();
        await cts.CancelAsync();
        await run;
        Assert.IsTrue(device.Disposed);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RemoteTmc_DeviceTimeout_BecomesTimeoutException()
    {
        using var cts = new CancellationTokenSource();
        var device = new FakeUsbTmcDevice { TimeOutOnRead = true };
        using var server = Start(new UsbBridgeOptions { UsbVendorId = "1", UsbProductId = "2" }, new FakeTmcFactory(device), null, cts.Token, out var run);
        using var remote = new RemoteUsbTmcDevice("127.0.0.1", server.BoundPort);
        remote.Open();
        Assert.ThrowsExactly<TimeoutException>(() => remote.ReadBulkIn(new byte[8], out _));
        await cts.CancelAsync();
        await run;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RemoteTmc_OpenFailure_ThrowsIOException()
    {
        using var cts = new CancellationTokenSource();
        using var server = Start(new UsbBridgeOptions { UsbVendorId = "1", UsbProductId = "2" }, new FakeTmcFactory(null, new IOException("gone")), null, cts.Token, out var run);
        using var remote = new RemoteUsbTmcDevice("127.0.0.1", server.BoundPort);
        Assert.ThrowsExactly<IOException>(remote.Open);
        Assert.IsFalse(remote.IsOpen);
        await cts.CancelAsync();
        await run;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RemoteHid_ReportsAndWrites_GoThroughTheBridge()
    {
        // Stage
        using var cts = new CancellationTokenSource();
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var input = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);
        var hid = new FakeHidDevice(input);
        var options = new UsbBridgeOptions { Kind = UsbBridgeKind.Hid, UsbVendorId = "04D8", UsbProductId = "F848" };
        using var server = Start(options, null, new FakeHidFactory(hid), cts.Token, out var run);
        using var remote = new RemoteUsbHidDevice("127.0.0.1", server.BoundPort);
        // Test
        remote.Open();
        pipe.Write([7, 8, 9]);
        pipe.Flush();
        var buffer = new byte[8];
        using var timeout = new CancellationTokenSource(3000);
        var count = await remote.BaseStream.ReadAsync(buffer, timeout.Token);
        remote.Write([0, 1, 2], 0, 3);
        // Assert
        CollectionAssert.AreEqual(new byte[] { 7, 8, 9 }, buffer[..count]);
        CollectionAssert.AreEqual(new byte[] { 0, 1, 2 }, hid.Written);
        remote.Close();
        Assert.AreEqual(0, await remote.BaseStream.ReadAsync(buffer, timeout.Token));
        await cts.CancelAsync();
        await run;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void RemoteTmc_NobodyListening_ThrowsIOException()
    {
        using var remote = new RemoteUsbTmcDevice("127.0.0.1", 1);
        Assert.ThrowsExactly<IOException>(remote.Open);
    }
}
