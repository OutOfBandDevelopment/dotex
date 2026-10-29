using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Tcp.Tests;

/// <summary>Telnet escaping and negotiation over an in-memory stream.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class TelnetStreamTests
{
    private sealed class Duplex(byte[] incoming) : MemoryStream
    {
        private readonly MemoryStream _in = new(incoming);

        public MemoryStream Sent { get; } = new();

        public override int Read(byte[] buffer, int offset, int count) => _in.Read(buffer, offset, count);

        public override async ValueTask<int> ReadAsync(global::System.Memory<byte> buffer, global::System.Threading.CancellationToken cancellationToken = default) =>
            await _in.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);

        public override void Write(byte[] buffer, int offset, int count) => Sent.Write(buffer, offset, count);

        public override async ValueTask WriteAsync(global::System.ReadOnlyMemory<byte> buffer, global::System.Threading.CancellationToken cancellationToken = default) =>
            await Sent.WriteAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    [TestMethod]
    public async Task ReadAsync_EscapedIacAndCommands_ReturnsDataOnly()
    {
        // Stage
        var wire = new Duplex([0x41, 255, 255, 255, 251, 1, 0x42, 255, 250, 44, 101, 255, 240, 0x43]);
        using var telnet = new TelnetStream(wire);
        var buffer = new byte[16];

        // Test
        var total = 0;
        int read;
        while ((read = await telnet.ReadAsync(buffer.AsMemory(total)).ConfigureAwait(false)) > 0) total += read;

        // Assert
        CollectionAssert.AreEqual(new byte[] { 0x41, 255, 0x42, 0x43 }, buffer[..total]);
        CollectionAssert.AreEqual(new byte[] { 255, 254, 1 }, wire.Sent.ToArray());
    }

    [TestMethod]
    public async Task ReadAsync_DoComPort_AcceptsWithWill()
    {
        // Stage
        var wire = new Duplex([255, 253, 44, 0x31]);
        using var telnet = new TelnetStream(wire);

        // Test
        var buffer = new byte[4];
        var read = await telnet.ReadAsync(buffer.AsMemory()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(1, read);
        CollectionAssert.AreEqual(new byte[] { 255, 251, 44 }, wire.Sent.ToArray());
    }

    [TestMethod]
    public async Task WriteAsync_IacInData_IsDoubled()
    {
        // Stage
        var wire = new Duplex([]);
        using var telnet = new TelnetStream(wire);

        // Test
        await telnet.WriteAsync(new byte[] { 1, 255, 2 }).ConfigureAwait(false);

        // Assert
        CollectionAssert.AreEqual(new byte[] { 1, 255, 255, 2 }, wire.Sent.ToArray());
    }

    [TestMethod]
    public void Handshake_Settings_EncodesSubNegotiations()
    {
        // Stage
        using var adapter = new Rfc2217DeviceAdapter("h", 1, 115200, 8, ComPortParity.Even, ComPortStopBits.Two);

        // Test
        var bytes = adapter.Handshake();

        // Assert
        byte[] expected =
        [
            255, 251, 44,
            255, 250, 44, 1, 0, 1, 194, 0, 255, 240,
            255, 250, 44, 2, 8, 255, 240,
            255, 250, 44, 3, 3, 255, 240,
            255, 250, 44, 4, 2, 255, 240,
        ];
        CollectionAssert.AreEqual(expected, bytes);
    }

    [TestMethod]
    public void Handshake_ByteEqualToIac_IsDoubled()
    {
        // Stage
        using var adapter = new Rfc2217DeviceAdapter("h", 1, 0x000000FF);

        // Test
        var bytes = adapter.Handshake();

        // Assert
        CollectionAssert.AreEqual(new byte[] { 255, 250, 44, 1, 0, 0, 0, 255, 255, 255, 240 }, bytes[3..14]);
    }
}
