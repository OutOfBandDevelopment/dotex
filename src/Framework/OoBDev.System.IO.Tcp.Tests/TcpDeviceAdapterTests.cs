using OoBDev.System.IO;
using System.Collections.Generic;
using System.IO;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.System.IO.Tcp.Tests;

/// <summary>The TCP adapter against an in-process loopback listener.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class TcpDeviceAdapterTests
{
    [TestMethod]
    public async Task OpenAsync_Listener_ExchangesBytes()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync();
        await using var adapter = new TcpDeviceAdapter("127.0.0.1", port);

        // Test
        await adapter.OpenAsync().ConfigureAwait(false);
        using var server = await accepted.ConfigureAwait(false);
        await adapter.Output.WriteAsync(Encoding.ASCII.GetBytes("*IDN?\n")).ConfigureAwait(false);
        var buffer = new byte[6];
        await server.GetStream().ReadExactlyAsync(buffer).ConfigureAwait(false);
        await server.GetStream().WriteAsync("OK\n"u8.ToArray()).ConfigureAwait(false);
        var reply = await adapter.Input.ReadAsync().ConfigureAwait(false);

        // Assert
        Assert.AreEqual(ConnectionState.Open, adapter.State);
        Assert.AreEqual("*IDN?\n", Encoding.ASCII.GetString(buffer));
        Assert.AreEqual("OK\n", Encoding.ASCII.GetString(reply.Buffer));
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
        await using var adapter = new TcpDeviceAdapter("127.0.0.1", port, TimeSpan.FromSeconds(2));

        // Test
        await Assert.ThrowsExactlyAsync<IOException>(() => adapter.OpenAsync()).ConfigureAwait(false);

        // Assert
        Assert.AreEqual(ConnectionState.Faulted, adapter.State);
    }

    [TestMethod]
    public async Task CloseAsync_AfterOpen_ReturnsToClosedAndRaisesEvents()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync();
        await using var adapter = new TcpDeviceAdapter("127.0.0.1", port);
        var states = new List<ConnectionState>();
        adapter.StateChanged += (_, e) => states.Add(e.Current);

        // Test
        await adapter.OpenAsync().ConfigureAwait(false);
        using var server = await accepted.ConfigureAwait(false);
        await adapter.CloseAsync().ConfigureAwait(false);

        // Assert
        CollectionAssert.AreEqual(new[] { ConnectionState.Opening, ConnectionState.Open, ConnectionState.Closing, ConnectionState.Closed }, states);
    }

    [TestMethod]
    public void Constructor_BlankHost_Throws()
    {
        // Assert
        Assert.ThrowsExactly<ArgumentException>(() => new TcpDeviceAdapter(" ", 5025));
    }
}
