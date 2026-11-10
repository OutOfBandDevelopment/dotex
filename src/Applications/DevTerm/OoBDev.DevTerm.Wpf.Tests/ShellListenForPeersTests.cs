using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>File &gt; Listen for peers: one session per connecting TCP peer.</summary>
[TestCategory("Unit")]
[TestClass]
[TestCategory(TestCategories.Tcp)]
public sealed class ShellListenForPeersTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class FixedPortDialog(int? port) : IListenPeersDialog
    {
        public int? AskPort() => port;
    }

    [TestMethod]
    public void ListenForPeers_NoDialog_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.ListenForPeers.CanExecute(null));
    }

    [TestMethod]
    public async Task ListenForPeers_DialogCancelled_StaysIdle()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IListenPeersDialog>(new FixedPortDialog(null));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Test
        shell.ListenForPeers.Execute(null);

        // Assert
        Assert.IsNull(shell.ListeningPort);
        Assert.IsTrue(shell.ListenForPeers.CanExecute(null));
    }

    [TestMethod]
    public async Task ListenForPeers_PeerConnects_AddsAConnectedSessionTitledByItsEndpoint()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IListenPeersDialog>(new FixedPortDialog(0));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.ListenForPeers.Execute(null);
        using var client = new TcpClient();

        // Test
        await client.ConnectAsync(IPAddress.Loopback, shell.ListeningPort!.Value, TestContext.CancellationToken);
        var added = SpinWait.SpinUntil(() => shell.Sessions.Count == 1, TimeSpan.FromSeconds(5));

        // Assert
        Assert.IsTrue(added);
        Assert.IsFalse(shell.ListenForPeers.CanExecute(null));
        Assert.Contains("127.0.0.1", shell.Sessions[0].Title, StringComparison.Ordinal);
        shell.Dispose();
    }
}
