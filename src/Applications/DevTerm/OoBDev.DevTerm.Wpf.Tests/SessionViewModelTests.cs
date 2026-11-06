using System;
using OoBDev.System.IO;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Loopback;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.System.IO.Tcp;

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
    public async Task Send_EchoSentOn_ListsTheSentTextInTheOutput()
    {
        // Stage
        await using var viewModel = Create();
        viewModel.Connect.Execute(null);
        await WaitAsync(() => viewModel.State == ConnectionState.Open);
        viewModel.EchoSent = true;
        viewModel.SendText = "hello";

        // Test
        viewModel.Send.Execute(null);
        await WaitAsync(() => viewModel.SendText.Length == 0);

        // Assert
        CollectionAssert.Contains(viewModel.OutputLines, "> hello");
    }

    [TestMethod]
    public async Task ToggleLogging_NoTab_CannotExecute()
    {
        // Stage
        await using var viewModel = Create();

        // Assert
        Assert.IsFalse(viewModel.ToggleLogging.CanExecute(null));
    }

    [TestMethod]
    public async Task ToggleLogging_WithTab_StartsAndStopsALogFile()
    {
        // Stage
        using var homeFolder = new TempDirectoryHandle();
        var home = homeFolder.DirectoryPath;
        var saved = Environment.GetEnvironmentVariable(DevTermHome.EnvironmentVariable);
        Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, home);
        try
        {
            var transport = new LoopbackTransport(Options.Create(new LoopbackTransportOptions()));
            var session = new Session(transport, new Pipeline([]));
            var tab = new SessionTab(session, new PresenterCatalog([]), new CliOptions());
            await using var viewModel = new SessionViewModel(session, "Loopback", tab);

            // Test
            viewModel.ToggleLogging.Execute(null);
            var started = viewModel.IsLogging;
            var path = viewModel.LogPath;
            viewModel.ToggleLogging.Execute(null);

            // Assert
            Assert.IsTrue(started);
            Assert.IsTrue(File.Exists(path));
            Assert.IsFalse(viewModel.IsLogging);
        }
        finally
        {
            Environment.SetEnvironmentVariable(DevTermHome.EnvironmentVariable, saved);
        }
    }

    [TestMethod]
    public async Task ClearOutput_LinesListed_EmptiesTheOutput()
    {
        // Stage
        await using var viewModel = Create();
        viewModel.OutputLines.Add("one");

        // Test
        viewModel.ClearOutput.Execute(null);

        // Assert
        Assert.AreEqual(0, viewModel.OutputLines.Count);
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

    [TestMethod]
    public async Task SoftwareFlowControl_LoopbackTransport_IsUnavailableAndStaysOff()
    {
        // Stage
        await using var viewModel = Create();

        // Test
        viewModel.SoftwareFlowControl = true;

        // Assert
        Assert.IsFalse(viewModel.HasSoftwareFlowControl);
        Assert.IsFalse(viewModel.SoftwareFlowControl);
    }

    [TestMethod]
    public async Task SoftwareFlowControl_TcpTransport_TogglesTheAdapterAndRaisesChange()
    {
        // Stage
        var transport = new TcpEndpointDeviceAdapter(new SystemTcpConnectionSource(), Options.Create(new TcpEndpointOptions()));
        var ascii = new AsciiPresenter(Options.Create(new AsciiPresenterOptions()));
        await using var viewModel = new SessionViewModel(new Session(transport, new Pipeline([ascii])), "Tcp");
        var raised = 0;
        viewModel.PropertyChanged += (_, e) => raised += e.PropertyName == nameof(SessionViewModel.SoftwareFlowControl) ? 1 : 0;

        // Test
        viewModel.SoftwareFlowControl = true;

        // Assert
        Assert.IsTrue(viewModel.HasSoftwareFlowControl);
        Assert.IsTrue(transport.SoftwareFlowControl);
        Assert.AreEqual(1, raised);
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
