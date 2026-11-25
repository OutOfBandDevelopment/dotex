using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Configuration.Tests;

[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class PeerTabSourceTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task Start_TwoPeersConnect_RaiseTwoTabsThatOpenOnTheAcceptedConnection()
    {
        // Stage
        await using var source = new PeerTabSource(new CliOptions { Transport = "loopback", Presenter = ["ascii"] }, "127.0.0.1", 0);
        var ready = new TaskCompletionSource<PeerTabEventArgs>(TaskCreationOptions.RunContinuationsAsynchronously);
        var seen = 0;
        var tabs = new global::System.Collections.Concurrent.ConcurrentBag<PeerTabEventArgs>();
        source.TabReady += (_, e) =>
        {
            tabs.Add(e);
            if (Interlocked.Increment(ref seen) == 2)
            {
                ready.TrySetResult(e);
            }
        };
        source.Start();
        using var first = new TcpClient();
        using var second = new TcpClient();

        // Test
        await first.ConnectAsync(IPAddress.Loopback, source.Port, TestContext.CancellationToken);
        await second.ConnectAsync(IPAddress.Loopback, source.Port, TestContext.CancellationToken);
        await ready.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);

        // Assert
        Assert.HasCount(2, tabs);
        foreach (var e in tabs)
        {
            await e.Tab.Session.OpenAsync(TestContext.CancellationToken);
            Assert.AreEqual(ConnectionState.Open, e.Tab.Session.State);
            Assert.AreEqual("ascii", e.Tab.Parser);
            await e.Tab.Session.DisposeAsync();
        }
    }
}
