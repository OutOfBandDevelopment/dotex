using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.DevTerm.Tests;

/// <summary>The hosted TCP services: clients on a loopback socket, traffic seen through the adapter.</summary>
[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class ServiceHostBridgeTests
{
    public TestContext TestContext { get; set; } = null!;

    private static ushort FreePort()
    {
        using var probe = new TcpListener(IPAddress.Loopback, 0);
        probe.Start();
        return (ushort)((IPEndPoint)probe.LocalEndpoint).Port;
    }

    private async Task<byte[]> ReadAsync(global::System.IO.Pipelines.PipeReader reader, int count)
    {
        while (true)
        {
            var result = await reader.ReadAtLeastAsync(count, TestContext.CancellationToken);
            var bytes = result.Buffer.Slice(0, count).ToArray();
            reader.AdvanceTo(result.Buffer.GetPosition(count));
            return bytes;
        }
    }

    [TestMethod]
    public async Task Echo_ReturnsTheBytes_AndTheAdapterSeesBothDirections()
    {
        var port = FreePort();
        await using var bridge = new ServiceHostBridge(ServiceKind.Echo, port);
        var replies = new ConcurrentQueue<byte[]>();
        bridge.Replied += data => replies.Enqueue(data.ToArray());
        await bridge.OpenAsync(TestContext.CancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, TestContext.CancellationToken);
        var stream = client.GetStream();
        await stream.WriteAsync("ping"u8.ToArray(), TestContext.CancellationToken);
        var echoed = new byte[4];
        await stream.ReadExactlyAsync(echoed, TestContext.CancellationToken);

        Assert.AreEqual("ping", Encoding.ASCII.GetString(echoed));
        Assert.AreEqual("ping", Encoding.ASCII.GetString(await ReadAsync(bridge.Input, 4)));
        Assert.AreEqual("ping", Encoding.ASCII.GetString(replies.SelectMany(r => r).ToArray()));
        Assert.AreEqual(ConnectionState.Open, bridge.State);
        Assert.AreEqual($"echo://127.0.0.1:{port}", bridge.Path);
    }

    [TestMethod]
    public async Task Discard_ShowsWhatClientsSend_ButNeverReplies()
    {
        var port = FreePort();
        await using var bridge = new ServiceHostBridge(ServiceKind.Discard, port);
        var replied = false;
        bridge.Replied += _ => replied = true;
        await bridge.OpenAsync(TestContext.CancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, TestContext.CancellationToken);
        await client.GetStream().WriteAsync("hello"u8.ToArray(), TestContext.CancellationToken);

        Assert.AreEqual("hello", Encoding.ASCII.GetString(await ReadAsync(bridge.Input, 5)));
        Assert.IsFalse(replied);
    }

    [TestMethod]
    public async Task Daytime_AnswersAnyInput()
    {
        var port = FreePort();
        await using var bridge = new ServiceHostBridge(ServiceKind.Daytime, port);
        await bridge.OpenAsync(TestContext.CancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, TestContext.CancellationToken);
        await client.GetStream().WriteAsync("?"u8.ToArray(), TestContext.CancellationToken);
        var reply = new byte[4];
        var read = await client.GetStream().ReadAsync(reply, TestContext.CancellationToken);

        Assert.IsGreaterThan(0, read);
    }

    [TestMethod]
    public async Task Chargen_StopsAtTheByteCap()
    {
        var port = FreePort();
        await using var bridge = new ServiceHostBridge(ServiceKind.Chargen, port, chargenLimit: 32);
        var sent = 0;
        bridge.Replied += data => Interlocked_Add(ref sent, data.Length);
        await bridge.OpenAsync(TestContext.CancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, port, TestContext.CancellationToken);
        var received = new byte[32];
        await client.GetStream().ReadExactlyAsync(received, TestContext.CancellationToken);
        await Task.Delay(1500, TestContext.CancellationToken);

        Assert.AreEqual(32, Volatile_Read(ref sent), "No more than the cap is sent.");
    }

    [TestMethod]
    public async Task Close_StopsTheListener_AndFreesThePort()
    {
        var port = FreePort();
        var bridge = new ServiceHostBridge(ServiceKind.Echo, port);
        await bridge.OpenAsync(TestContext.CancellationToken);

        await bridge.CloseAsync(TestContext.CancellationToken);

        Assert.AreEqual(ConnectionState.Closed, bridge.State);
        using var probe = new TcpListener(IPAddress.Loopback, port);
        probe.Start();
    }

    [TestMethod]
    public async Task Open_OnAPortInUse_ThrowsAnIOExceptionNamingTheAddress()
    {
        var port = FreePort();
        using var holder = new TcpListener(IPAddress.Loopback, port);
        holder.Start();
        await using var bridge = new ServiceHostBridge(ServiceKind.Echo, port);

        var ex = await Assert.ThrowsExactlyAsync<IOException>(() => bridge.OpenAsync(TestContext.CancellationToken));

        Assert.Contains($"127.0.0.1:{port}", ex.Message);
        Assert.AreEqual(ConnectionState.Closed, bridge.State);
    }

    [TestMethod]
    public void DefaultPort_IsTheStandardOne()
    {
        Assert.AreEqual((ushort)7, ServiceHostBridge.DefaultPort(ServiceKind.Echo));
        Assert.AreEqual((ushort)9, ServiceHostBridge.DefaultPort(ServiceKind.Discard));
        Assert.AreEqual((ushort)13, ServiceHostBridge.DefaultPort(ServiceKind.Daytime));
        Assert.AreEqual((ushort)19, ServiceHostBridge.DefaultPort(ServiceKind.Chargen));
        Assert.AreEqual((ushort)37, ServiceHostBridge.DefaultPort(ServiceKind.Time));
    }

    [TestMethod]
    public void Constructor_RejectsANegativeChargenLimit() =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => new ServiceHostBridge(ServiceKind.Chargen, 1, null, -1));

    private static void Interlocked_Add(ref int target, int value) => global::System.Threading.Interlocked.Add(ref target, value);

    private static int Volatile_Read(ref int target) => global::System.Threading.Volatile.Read(ref target);
}
