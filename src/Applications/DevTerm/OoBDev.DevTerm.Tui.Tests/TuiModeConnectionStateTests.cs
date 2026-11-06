using OoBDev.System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.DevTerm;
using Microsoft.Extensions.Options;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// The TUI main window's connection-state UI: the colored status line, the title's disconnected
/// suffix, which Device menu items are enabled, and tagged status/error lines in the output pane.
/// </summary>
[TestCategory("Unit")]
[TestClass]
[DoNotParallelize]
public sealed class TuiModeConnectionStateTests
{
    public required TestContext TestContext { get; set; }

    private static CliOptions Tcp() => new() { Transport = "tcp", Host = "127.0.0.1", Port = "1", Presenter = ["ascii"] };

    private static (Session Session, FakeTransport Transport, AsciiPresenter Ascii) CreateSession()
    {
        var transport = new FakeTransport();
        var ascii = new AsciiPresenter(Options.Create(new AsciiPresenterOptions()));
        return (new Session(transport, new Pipeline([ascii])), transport, ascii);
    }

    [TestMethod]
    public async Task LostConnection_FlipsTheStatusLineAndTitle()
    {
        var (session, transport, ascii) = CreateSession();
        await session.OpenAsync(TestContext.CancellationToken);

        TuiTestRunner.RunWithLoop(session, ascii, Tcp(), parts =>
        {
            transport.FailReadAsync(new IOException("socket reset")).GetAwaiter().GetResult();

            (string Status, string Title, string Output) Snapshot()
            {
                (string, string, string) result = default;
                using var done = new ManualResetEventSlim(false);
                TuiTestRunner.CurrentApp.Invoke(() =>
                {
                    result = (parts.StatusLabel.Text, parts.Window.Title, parts.Output.Text);
                    done.Set();
                });
                done.Wait(TimeSpan.FromSeconds(5));
                return result;
            }

            var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
            var state = Snapshot();
            while (!state.Status.Contains("Disconnected", StringComparison.Ordinal) && DateTime.UtcNow < deadline)
            {
                Thread.Sleep(20);
                state = Snapshot();
            }

            StringAssert.Contains(state.Status, "Disconnected");
            Assert.EndsWith(" — disconnected", state.Title);
            StringAssert.Contains(state.Output, "[error] Connection lost: socket reset");
        });
    }

    [TestMethod]
    public void StatusAndErrorLines_AreTagged() =>
        Assert.AreEqual(("[dev-term] Connected.", "[error] Nope."), (TuiMode.StatusLine("Connected."), TuiMode.ErrorLine("Nope.")));
}
