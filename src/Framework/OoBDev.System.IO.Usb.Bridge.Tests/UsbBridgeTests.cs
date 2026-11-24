using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Pipes;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.IO.Usb.Bridge.Tests;

[TestClass]
public sealed class UsbBridgeTests
{
    private static readonly CancellationToken _ct = CancellationToken.None;
    private static readonly UsbBridgeOptions _tmcOptions = new() { Kind = UsbBridgeKind.UsbTmc, UsbVendorId = "1AB1", UsbProductId = "09C4" };

    public TestContext TestContext { get; set; } = null!;

    private static byte[] Size(uint size)
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, size);
        return bytes;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Codec_RoundTrip_KeepsOpIdAndPayload()
    {
        // Stage
        using var stream = new MemoryStream();
        var frame = new UsbFrame(UsbBridgeOp.WriteBulkOut, 7, new byte[] { 1, 2, 3 });
        // Test
        await UsbFrameCodec.WriteAsync(stream, frame, _ct);
        stream.Position = 0;
        var read = await UsbFrameCodec.ReadAsync(stream, _ct);
        // Assert
        Assert.IsNotNull(read);
        Assert.AreEqual(UsbBridgeOp.WriteBulkOut, read.Op);
        Assert.AreEqual(7u, read.Id);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, read.Payload.ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Codec_EmptyStream_ReturnsNull()
    {
        using var stream = new MemoryStream();
        Assert.IsNull(await UsbFrameCodec.ReadAsync(stream, _ct));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Codec_OversizeLength_Throws()
    {
        var bytes = new byte[4];
        BinaryPrimitives.WriteUInt32BigEndian(bytes, (uint)UsbFrameCodec.MaxFrameBytes + 1);
        using var stream = new MemoryStream(bytes);
        await Assert.ThrowsExactlyAsync<InvalidDataException>(() => UsbFrameCodec.ReadAsync(stream, _ct));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Codec_TruncatedFrame_Throws()
    {
        var bytes = UsbFrameCodec.Encode(new UsbFrame(UsbBridgeOp.Ok, 1, new byte[] { 1, 2, 3 }));
        using var stream = new MemoryStream(bytes[..^2]);
        await Assert.ThrowsExactlyAsync<EndOfStreamException>(() => UsbFrameCodec.ReadAsync(stream, _ct));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Tmc_HelloOpenWriteRead_ForwardsToDevice()
    {
        // Stage
        var device = new FakeUsbTmcDevice { Reply = Encoding.ASCII.GetBytes("RIGOL") };
        var factory = new FakeTmcFactory(device);
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions, factory, null);
        // Test
        var hello = await rig.CallAsync(UsbBridgeOp.Hello, [UsbFrameCodec.Version, (byte)UsbBridgeKind.UsbTmc]);
        var open = await rig.CallAsync(UsbBridgeOp.Open, []);
        var write = await rig.CallAsync(UsbBridgeOp.WriteBulkOut, [9, 9]);
        var read = await rig.CallAsync(UsbBridgeOp.ReadBulkIn, Size(64));
        // Assert
        Assert.AreEqual(UsbBridgeOp.Hello, hello.Op);
        Assert.AreEqual(UsbBridgeOp.Ok, open.Op);
        Assert.AreEqual(4096u, BinaryPrimitives.ReadUInt32BigEndian(open.Payload.Span));
        Assert.AreEqual(64u, BinaryPrimitives.ReadUInt32BigEndian(open.Payload.Span[4..]));
        Assert.AreEqual(UsbBridgeOp.Ok, write.Op);
        CollectionAssert.AreEqual(new byte[] { 9, 9 }, device.Written[0]);
        Assert.AreEqual(UsbBridgeOp.Data, read.Op);
        Assert.AreEqual(0, read.Payload.Span[0]);
        Assert.AreEqual("RIGOL", Encoding.ASCII.GetString(read.Payload.Span[1..]));
        Assert.AreEqual(0x1AB1, factory.Options!.VendorId);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Tmc_ControlOps_ReachDevice()
    {
        var device = new FakeUsbTmcDevice();
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions, new FakeTmcFactory(device), null);
        await rig.CallAsync(UsbBridgeOp.Open, []);
        await rig.CallAsync(UsbBridgeOp.Clear, []);
        await rig.CallAsync(UsbBridgeOp.SetRemote, [1]);
        await rig.CallAsync(UsbBridgeOp.AbortBulkIn, [5]);
        Assert.AreEqual(1, device.ClearCount);
        CollectionAssert.AreEqual(new[] { true }, device.RemoteCalls);
        CollectionAssert.AreEqual(new byte[] { 5 }, device.AbortedIn);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Tmc_ReadTimeout_IsReportedAsTimeoutError()
    {
        var device = new FakeUsbTmcDevice { TimeOutOnRead = true };
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions, new FakeTmcFactory(device), null);
        await rig.CallAsync(UsbBridgeOp.Open, []);
        var reply = await rig.CallAsync(UsbBridgeOp.ReadBulkIn, Size(16));
        Assert.AreEqual(UsbBridgeOp.Error, reply.Op);
        Assert.AreEqual((byte)UsbBridgeErrorCode.Timeout, reply.Payload.Span[0]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Open_FactoryFails_ReportsDeviceGone()
    {
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions, new FakeTmcFactory(null, new IOException("no device")), null);
        var reply = await rig.CallAsync(UsbBridgeOp.Open, []);
        Assert.AreEqual(UsbBridgeOp.Error, reply.Op);
        Assert.AreEqual((byte)UsbBridgeErrorCode.DeviceGone, reply.Payload.Span[0]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Hello_WrongKind_IsBadRequest()
    {
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions, new FakeTmcFactory(new FakeUsbTmcDevice()), null);
        var reply = await rig.CallAsync(UsbBridgeOp.Hello, [UsbFrameCodec.Version, (byte)UsbBridgeKind.Hid]);
        Assert.AreEqual((byte)UsbBridgeErrorCode.BadRequest, reply.Payload.Span[0]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SecondClient_IsRefused_WhenNotReplacing()
    {
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions, new FakeTmcFactory(new FakeUsbTmcDevice()), null);
        await rig.CallAsync(UsbBridgeOp.Hello, [UsbFrameCodec.Version, (byte)UsbBridgeKind.UsbTmc]);
        using var second = new TcpClient();
        await second.ConnectAsync("127.0.0.1", rig.Port);
        using var timeout = new CancellationTokenSource(3000);
        var read = await UsbFrameCodec.ReadAsync(second.GetStream(), timeout.Token);
        Assert.IsNull(read);
        var again = await rig.CallAsync(UsbBridgeOp.Clear, []);
        Assert.AreEqual(UsbBridgeOp.Error, again.Op);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SecondClient_Replaces_WhenAllowed()
    {
        var device = new FakeUsbTmcDevice();
        await using var rig = await UsbBridgeRig.StartAsync(_tmcOptions with { ReplaceExistingClient = true }, new FakeTmcFactory(device), null);
        await rig.CallAsync(UsbBridgeOp.Open, []);
        using var second = new TcpClient();
        await second.ConnectAsync("127.0.0.1", rig.Port);
        var stream = second.GetStream();
        await UsbFrameCodec.WriteAsync(stream, new UsbFrame(UsbBridgeOp.Hello, 1, new byte[] { UsbFrameCodec.Version, 1 }), _ct);
        using var timeout = new CancellationTokenSource(3000);
        var reply = await UsbFrameCodec.ReadAsync(stream, timeout.Token);
        Assert.AreEqual(UsbBridgeOp.Hello, reply!.Op);
        Assert.IsTrue(device.Disposed);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Hid_ReportsAreForwarded_AndWritesReachDevice()
    {
        // Stage
        using var pipe = new AnonymousPipeServerStream(PipeDirection.Out);
        using var input = new AnonymousPipeClientStream(PipeDirection.In, pipe.ClientSafePipeHandle);
        var hid = new FakeHidDevice(input);
        var options = new UsbBridgeOptions { Kind = UsbBridgeKind.Hid, UsbVendorId = "04D8", UsbProductId = "F848" };
        await using var rig = await UsbBridgeRig.StartAsync(options, null, new FakeHidFactory(hid));
        // Test
        var open = await rig.CallAsync(UsbBridgeOp.Open, []);
        pipe.Write([1, 2, 3]);
        pipe.Flush();
        using var timeout = new CancellationTokenSource(3000);
        var report = await UsbFrameCodec.ReadAsync(rig.Stream, timeout.Token);
        var write = await rig.CallAsync(UsbBridgeOp.HidWrite, [0, 4, 5]);
        // Assert
        Assert.AreEqual(UsbBridgeOp.Ok, open.Op);
        Assert.AreEqual(UsbBridgeOp.HidReport, report!.Op);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, report.Payload.ToArray());
        Assert.AreEqual(UsbBridgeOp.Ok, write.Op);
        CollectionAssert.AreEqual(new byte[] { 0, 4, 5 }, hid.Written);
    }
}
