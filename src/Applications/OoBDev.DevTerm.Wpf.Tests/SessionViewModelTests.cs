using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Loopback;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The session view model over the loopback adapter.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class SessionViewModelTests
{
    [TestMethod]
    public async Task Connect_Closed_OpensAndEnablesSend()
    {
        // Stage
        await using var viewModel = Create();

        // Test
        viewModel.Connect.Execute(null);
        await WaitAsync(() => viewModel.State == ConnectionState.Open);

        // Assert
        Assert.IsFalse(viewModel.Connect.CanExecute(null));
        Assert.IsTrue(viewModel.Disconnect.CanExecute(null));
        Assert.IsFalse(viewModel.Send.CanExecute(null), "Nothing typed yet.");
    }

    [TestMethod]
    public async Task Send_Hello_ShowsTheLoopbackReply()
    {
        // Stage
        await using var viewModel = Create();
        viewModel.Connect.Execute(null);
        await WaitAsync(() => viewModel.State == ConnectionState.Open);
        viewModel.SendText = "hello";

        // Test
        viewModel.Send.Execute(null);
        await WaitAsync(() => viewModel.OutputLines.Any(l => l.Contains("From Loopback test", StringComparison.Ordinal)));

        // Assert
        Assert.AreEqual(string.Empty, viewModel.SendText);
        Assert.IsNull(viewModel.ErrorMessage);
    }

    [TestMethod]
    public async Task Disconnect_Open_ClosesTheAdapter()
    {
        // Stage
        await using var viewModel = Create();
        viewModel.Connect.Execute(null);
        await WaitAsync(() => viewModel.State == ConnectionState.Open);

        // Test
        viewModel.Disconnect.Execute(null);
        await WaitAsync(() => viewModel.State == ConnectionState.Closed);

        // Assert
        Assert.IsTrue(viewModel.Connect.CanExecute(null));
    }

    [TestMethod]
    public void Constructor_BlankTitle_Throws()
    {
        // Test / Assert
        Assert.ThrowsExactly<ArgumentException>(() => new SessionViewModel(new Session(new LoopbackTransport(Options.Create(new LoopbackTransportOptions())), new Pipeline([])), " "));
    }

    private static SessionViewModel Create()
    {
        var transport = new LoopbackTransport(Options.Create(new LoopbackTransportOptions()));
        var ascii = new AsciiPresenter(Options.Create(new AsciiPresenterOptions()));
        return new SessionViewModel(new Session(transport, new Pipeline([ascii])), "Loopback");
    }

    private static async Task WaitAsync(Func<bool> condition)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            Assert.IsLessThan(5000, watch.ElapsedMilliseconds, "Timed out waiting for the condition.");
            await Task.Delay(10);
        }
    }
}
