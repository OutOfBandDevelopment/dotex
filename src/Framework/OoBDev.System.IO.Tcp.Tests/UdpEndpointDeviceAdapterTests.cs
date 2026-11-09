using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class UdpEndpointDeviceAdapterTests
{
    public TestContext TestContext { get; set; } = null!;

    private static UdpEndpointDeviceAdapter Create(string? target = null) =>
        new(Options.Create(new UdpEndpointOptions { Host = "127.0.0.1", Port = 0, Target = target }));

    private async Task<string> ReadAsync(UdpEndpointDeviceAdapter adapter, int count)
    {
        var result = await adapter.Input.ReadAtLeastAsync(count, TestContext.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);
        var text = Encoding.ASCII.GetString(result.Buffer.Slice(0, count));
        adapter.Input.AdvanceTo(result.Buffer.GetPosition(count));
        return text;
    }

    [TestMethod]
    public async Task Open_Datagram_ArrivesOnInputAndRaisesEventWithSender()
    {
        // Stage
        await using var adapter = Create();
        await adapter.OpenAsync(TestContext.CancellationToken);
        UdpDatagramEventArgs? seen = null;
        adapter.DatagramReceived += (_, e) => seen = e;
        using var sender = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

        // Test
        await sender.SendAsync("ping"u8.ToArray(), new IPEndPoint(IPAddress.Loopback, adapter.LocalEndPoint!.Port), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("ping", await ReadAsync(adapter, 4));
        Assert.IsNotNull(seen);
        Assert.AreEqual(((IPEndPoint)sender.Client.LocalEndPoint!).Port, seen.Sender.Port);
    }

    [TestMethod]
    public async Task Write_AfterADatagram_RepliesToTheLastSender()
    {
        // Stage
        await using var adapter = Create();
        await adapter.OpenAsync(TestContext.CancellationToken);
        using var peer = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        await peer.SendAsync("hi"u8.ToArray(), new IPEndPoint(IPAddress.Loopback, adapter.LocalEndPoint!.Port), TestContext.CancellationToken);
        await ReadAsync(adapter, 2);

        // Test
        await adapter.Output.WriteAsync("ok"u8.ToArray(), TestContext.CancellationToken);
        var reply = await peer.ReceiveAsync(TestContext.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("ok", Encoding.ASCII.GetString(reply.Buffer));
    }

    [TestMethod]
    public async Task Write_ConfiguredTarget_WinsOverTheLastSender()
    {
        // Stage
        using var chosen = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        await using var adapter = Create($"127.0.0.1:{((IPEndPoint)chosen.Client.LocalEndPoint!).Port}");
        await adapter.OpenAsync(TestContext.CancellationToken);
        using var other = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        await other.SendAsync("x"u8.ToArray(), new IPEndPoint(IPAddress.Loopback, adapter.LocalEndPoint!.Port), TestContext.CancellationToken);
        await ReadAsync(adapter, 1);

        // Test
        await adapter.Output.WriteAsync("to"u8.ToArray(), TestContext.CancellationToken);
        var reply = await chosen.ReceiveAsync(TestContext.CancellationToken).AsTask().WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("to", Encoding.ASCII.GetString(reply.Buffer));
    }

    [TestMethod]
    public async Task Write_NoPeerYet_Throws()
    {
        // Stage
        await using var adapter = Create();
        await adapter.OpenAsync(TestContext.CancellationToken);

        // Test + Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await adapter.Output.WriteAsync("x"u8.ToArray(), TestContext.CancellationToken));
    }

    [TestMethod]
    public async Task Close_AfterOpen_EndsInputAndClosesState()
    {
        // Stage
        var adapter = Create();
        await adapter.OpenAsync(TestContext.CancellationToken);

        // Test
        await adapter.CloseAsync(TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ConnectionState.Closed, adapter.State);
    }
}
