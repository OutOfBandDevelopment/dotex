using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.System.IO.Tcp.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class TcpPeerListenerTests
{
    public TestContext TestContext { get; set; } = null!;

    private static async Task<TcpPeer> NextAsync(IAsyncEnumerator<TcpPeer> peers, CancellationToken token)
    {
        Assert.IsTrue(await peers.MoveNextAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(5), token));
        return peers.Current;
    }

    private static async Task<string> ReadAsync(IDeviceAdapter adapter, int count, CancellationToken token)
    {
        var result = await adapter.Input.ReadAtLeastAsync(count, token).AsTask().WaitAsync(TimeSpan.FromSeconds(5), token);
        var text = Encoding.ASCII.GetString(result.Buffer.Slice(0, count));
        adapter.Input.AdvanceTo(result.Buffer.GetPosition(count));
        return text;
    }

    [TestMethod]
    public async Task AcceptPeersAsync_TwoPeers_GiveTwoIndependentOpenAdapters()
    {
        // Stage
        await using var listener = new TcpPeerListener("127.0.0.1", 0);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await using var peers = listener.AcceptPeersAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        using var first = new TcpClient();
        using var second = new TcpClient();

        // Test
        await first.ConnectAsync(IPAddress.Loopback, listener.Port, TestContext.CancellationToken);
        var peerOne = await NextAsync(peers, cts.Token);
        await second.ConnectAsync(IPAddress.Loopback, listener.Port, TestContext.CancellationToken);
        var peerTwo = await NextAsync(peers, cts.Token);
        await first.GetStream().WriteAsync("one"u8.ToArray(), TestContext.CancellationToken);
        await second.GetStream().WriteAsync("two"u8.ToArray(), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ConnectionState.Open, peerOne.Adapter.State);
        Assert.AreNotEqual(peerOne.RemoteEndPoint, peerTwo.RemoteEndPoint);
        Assert.AreEqual("one", await ReadAsync(peerOne.Adapter, 3, cts.Token));
        Assert.AreEqual("two", await ReadAsync(peerTwo.Adapter, 3, cts.Token));
        await peerOne.Adapter.DisposeAsync();
        await peerTwo.Adapter.DisposeAsync();
        await cts.CancelAsync();
    }

    [TestMethod]
    public async Task AcceptPeersAsync_PeerAdapter_WritesBackToItsOwnPeer()
    {
        // Stage
        await using var listener = new TcpPeerListener("127.0.0.1", 0);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await using var peers = listener.AcceptPeersAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, listener.Port, TestContext.CancellationToken);
        var peer = await NextAsync(peers, cts.Token);

        // Test
        await peer.Adapter.Output.WriteAsync("hi!"u8.ToArray(), TestContext.CancellationToken);
        var buffer = new byte[3];
        await client.GetStream().ReadExactlyAsync(buffer, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("hi!", Encoding.ASCII.GetString(buffer));
        await peer.Adapter.DisposeAsync();
        await cts.CancelAsync();
    }

    [TestMethod]
    public async Task AcceptPeersAsync_Cancelled_EndsTheSequence()
    {
        // Stage
        await using var listener = new TcpPeerListener("127.0.0.1", 0);
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        await using var peers = listener.AcceptPeersAsync(cts.Token).GetAsyncEnumerator(cts.Token);
        var next = peers.MoveNextAsync().AsTask();

        // Test
        await cts.CancelAsync();

        // Assert
        Assert.IsFalse(await next.WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken));
    }
}
