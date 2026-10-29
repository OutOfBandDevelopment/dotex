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
    public async Task TryOpen_Listener_ExchangesBytes()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        var accepted = listener.AcceptTcpClientAsync();
        var adapter = new TcpDeviceAdapter("127.0.0.1", port);

        // Test
        var opened = adapter.TryOpen(out var stream);
        using var server = await accepted.ConfigureAwait(false);
        await stream!.WriteAsync(Encoding.ASCII.GetBytes("*IDN?\n")).ConfigureAwait(false);
        var buffer = new byte[6];
        await server.GetStream().ReadExactlyAsync(buffer).ConfigureAwait(false);

        // Assert
        Assert.IsTrue(opened);
        Assert.AreEqual("*IDN?\n", Encoding.ASCII.GetString(buffer));
        Assert.AreEqual($"127.0.0.1:{port}", adapter.Path);
        Assert.AreSame(stream, adapter.Stream);
        stream.Dispose();
    }

    [TestMethod]
    public void TryOpen_NothingListening_ReturnsFalse()
    {
        // Stage
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        var adapter = new TcpDeviceAdapter("127.0.0.1", port, TimeSpan.FromSeconds(2));

        // Test
        var opened = adapter.TryOpen(out var stream);

        // Assert
        Assert.IsFalse(opened);
        Assert.IsNull(stream);
    }

    [TestMethod]
    public void Stream_NotOpened_Throws()
    {
        // Stage
        var adapter = new TcpDeviceAdapter("127.0.0.1", 1);

        // Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => adapter.Stream);
    }

    [TestMethod]
    public void Constructor_BlankHost_Throws()
    {
        // Assert
        Assert.ThrowsExactly<ArgumentException>(() => new TcpDeviceAdapter(" ", 5025));
    }
}
