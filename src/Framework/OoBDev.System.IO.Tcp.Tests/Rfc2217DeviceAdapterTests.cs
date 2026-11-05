using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Tcp.Tests;

/// <summary>The RFC 2217 adapter opening against an in-process loopback listener.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class Rfc2217DeviceAdapterTests
{
    [TestMethod]
    public async Task OpenAsync_Listener_SendsTheHandshakeAndPassesDataBytes()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync();
        await using var adapter = new Rfc2217DeviceAdapter("127.0.0.1", port, 19200);
        var expected = adapter.Handshake();

        // Test
        await adapter.OpenAsync().ConfigureAwait(false);
        using var server = await accepted.ConfigureAwait(false);
        var received = new byte[expected.Length];
        await server.GetStream().ReadExactlyAsync(received).ConfigureAwait(false);
        await server.GetStream().WriteAsync("OK"u8.ToArray()).ConfigureAwait(false);
        var reply = await adapter.Input.ReadAsync().ConfigureAwait(false);

        // Assert
        Assert.AreEqual(ConnectionState.Open, adapter.State);
        CollectionAssert.AreEqual(expected, received);
        Assert.AreEqual("OK", Encoding.ASCII.GetString(reply.Buffer));
        Assert.AreEqual($"127.0.0.1:{port}", adapter.Path);
    }

    [TestMethod]
    public async Task OpenAsync_NothingListening_FaultsAndThrows()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        await using var adapter = new Rfc2217DeviceAdapter("127.0.0.1", port);

        // Test
        await Assert.ThrowsExactlyAsync<IOException>(() => adapter.OpenAsync()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
    }

    [TestMethod]
    public async Task CloseAsync_AfterOpen_ReturnsToClosed()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync();
        await using var adapter = new Rfc2217DeviceAdapter("127.0.0.1", port);
        await adapter.OpenAsync().ConfigureAwait(false);
        using var server = await accepted.ConfigureAwait(false);

        // Test
        await adapter.CloseAsync().ConfigureAwait(false);

        // Assert
        Assert.AreEqual(ConnectionState.Closed, adapter.State);
    }
}
