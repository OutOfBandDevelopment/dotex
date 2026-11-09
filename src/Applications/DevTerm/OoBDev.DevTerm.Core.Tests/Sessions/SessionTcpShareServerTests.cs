using OoBDev.System.IO.Pipelines;
using OoBDev.System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using Moq;

namespace OoBDev.DevTerm.Tests;

/// <summary>A TCP client shares a live session over a real loopback socket.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class SessionTcpShareServerTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public async Task Client_ReceivesDeviceBytes_AndItsBytesReachTheDevice()
    {
        var pipe = new Pipe();
        var written = new List<byte>();
        var transport = new Mock<IDeviceAdapter>();
        transport.SetupGet(t => t.Input).Returns(pipe.Reader);
        transport.SetupGet(t => t.State).Returns(ConnectionState.Open);
        transport.SetupGet(t => t.Output).Returns(DeviceAdapterOutput.Create((data, _) =>
        {
            lock (written)
            {
                written.AddRange(data.ToArray());
            }

            return Task.CompletedTask;
        }));
        await using var session = new Session(transport.Object, new Pipeline([]));
        await using var server = new SessionTcpShareServer(session, IPAddress.Loopback, 0);
        using var registration = session.AddObserver(server);
        await session.OpenAsync(TestContext.CancellationToken);

        using var client = new TcpClient();
        await client.ConnectAsync(IPAddress.Loopback, server.Port, TestContext.CancellationToken);
        var stream = client.GetStream();
        await Task.Delay(200, TestContext.CancellationToken);

        await pipe.Writer.WriteAsync(new byte[] { 1, 2, 3 }, TestContext.CancellationToken);
        var buffer = new byte[3];
        await stream.ReadExactlyAsync(buffer, TestContext.CancellationToken);
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, buffer);

        await stream.WriteAsync(new byte[] { 9, 8 }, TestContext.CancellationToken);
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (DateTime.UtcNow < deadline)
        {
            lock (written)
            {
                if (written.Count == 2)
                {
                    break;
                }
            }

            await Task.Delay(20, TestContext.CancellationToken);
        }

        CollectionAssert.AreEqual(new byte[] { 9, 8 }, written.ToArray());
    }

    [TestMethod]
    public async Task SecondClient_IsClosedWhileOneIsConnected()
    {
        var transport = new Mock<IDeviceAdapter>();
        transport.SetupGet(t => t.Input).Returns(new Pipe().Reader);
        transport.SetupGet(t => t.State).Returns(ConnectionState.Open);
        await using var session = new Session(transport.Object, new Pipeline([]));
        await using var server = new SessionTcpShareServer(session, IPAddress.Loopback, 0);

        using var first = new TcpClient();
        await first.ConnectAsync(IPAddress.Loopback, server.Port, TestContext.CancellationToken);
        await Task.Delay(200, TestContext.CancellationToken);
        using var second = new TcpClient();
        await second.ConnectAsync(IPAddress.Loopback, server.Port, TestContext.CancellationToken);

        var read = await second.GetStream().ReadAsync(new byte[1], TestContext.CancellationToken);
        Assert.AreEqual(0, read, "the rejected client sees an immediate close.");
    }
}
