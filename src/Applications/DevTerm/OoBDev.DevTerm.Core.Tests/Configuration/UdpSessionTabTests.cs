using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.IO.Tcp;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class UdpSessionTabTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Build_DatagramArrives_RaisesTheSenderAndDescribesIt()
    {
        // Stage
        var udp = UdpSessionTab.Build(new CliOptions { Presenter = ["ascii"] }, "127.0.0.1", 0);
        var seen = new TaskCompletionSource<UdpDatagramEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        udp.Adapter.DatagramReceived += (_, e) => seen.TrySetResult(e);
        await udp.Tab.Session.OpenAsync(TestContext.CancellationToken);
        using var sender = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));

        // Test
        await sender.SendAsync("abc"u8.ToArray(), new IPEndPoint(IPAddress.Loopback, udp.Adapter.LocalEndPoint!.Port), TestContext.CancellationToken);
        var datagram = await seen.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);

        // Assert
        Assert.AreEqual(ConnectionState.Open, udp.Tab.Session.State);
        StringAssert.Contains(UdpSessionTab.Describe(datagram), "(3 bytes)", StringComparison.Ordinal);
        await udp.Tab.Session.DisposeAsync();
    }
}
