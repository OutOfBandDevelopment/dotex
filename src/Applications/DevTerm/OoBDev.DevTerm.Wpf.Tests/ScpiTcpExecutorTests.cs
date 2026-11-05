using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Scpi;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The raw-socket SCPI executor against a loopback instrument.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class ScpiTcpExecutorTests
{
    private static async Task<string> Serve(TcpListener listener, string reply)
    {
        using var client = await listener.AcceptTcpClientAsync().ConfigureAwait(false);
        var stream = client.GetStream();
        var buffer = new byte[64];
        var read = await stream.ReadAsync(buffer).ConfigureAwait(false);
        var received = Encoding.ASCII.GetString(buffer, 0, read);
        if (reply.Length > 0) await stream.WriteAsync(Encoding.ASCII.GetBytes(reply)).ConfigureAwait(false);
        return received;
    }

    [TestMethod]
    public async Task ExecuteAsync_Query_ReturnsReplyLine()
    {
        // Stage
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var server = Serve(listener, "ACME,PSU,1,2\r\n");
            using var executor = new ScpiTcpExecutor("127.0.0.1", ((IPEndPoint)listener.LocalEndpoint).Port);

            // Test
            var reply = await executor.ExecuteAsync(new ScpiRequest("*IDN?\n", true, ScpiResponseFormat.Text)).ConfigureAwait(false);

            // Assert
            Assert.AreEqual("ACME,PSU,1,2", reply);
            Assert.AreEqual("*IDN?\n", await server.ConfigureAwait(false));
        }
        finally
        {
            listener.Stop();
        }
    }

    [TestMethod]
    public async Task ExecuteAsync_Command_ReturnsNullWithoutWaiting()
    {
        // Stage
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var server = Serve(listener, string.Empty);
            using var executor = new ScpiTcpExecutor("127.0.0.1", ((IPEndPoint)listener.LocalEndpoint).Port);

            // Test
            var reply = await executor.ExecuteAsync(new ScpiRequest("*RST\n", false, ScpiResponseFormat.Text)).ConfigureAwait(false);

            // Assert
            Assert.IsNull(reply);
            Assert.AreEqual("*RST\n", await server.ConfigureAwait(false));
        }
        finally
        {
            listener.Stop();
        }
    }

    [TestMethod]
    public async Task ExecuteAsync_NothingListening_ThrowsIOException()
    {
        // Stage
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        using var executor = new ScpiTcpExecutor("127.0.0.1", port);

        // Test
        var failure = await Assert.ThrowsExactlyAsync<IOException>(() => executor.ExecuteAsync(new ScpiRequest("*IDN?\n", true, ScpiResponseFormat.Text))).ConfigureAwait(false);

        // Assert
        StringAssert.Contains(failure.Message, "127.0.0.1");
    }
}
