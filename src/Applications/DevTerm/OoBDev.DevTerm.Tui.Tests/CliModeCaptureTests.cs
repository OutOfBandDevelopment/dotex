using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using OoBDev.System.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.DevTerm;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary><c>--capture &lt;dir&gt;</c>: the CLI saves detected streams as files and prints one line each.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class CliModeCaptureTests
{
    private TempDirectoryHandle _folder = null!;
    private string _directory = string.Empty;

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void Setup()
    {
        _folder = new TempDirectoryHandle();
        _directory = _folder.DirectoryPath;
    }

    [TestCleanup]
    public void Cleanup() => _folder.Dispose();

    private sealed class EndedStdin : TextReader
    {
        public TaskCompletionSource Release { get; } = new();

        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            await Release.Task.WaitAsync(cancellationToken);
            return null;
        }
    }

    [TestMethod]
    public async Task Capture_SavesADetectedImageIntoTheFolder_AndPrintsALine()
    {
        var transport = new FakeTransport();
        var ascii = new AsciiPresenter(Microsoft.Extensions.Options.Options.Create(new AsciiPresenterOptions()));
        var session = new Session(transport, new Pipeline([ascii]));
        var options = new CliOptions { Transport = "loopback", Presenter = ["ascii"], Capture = _directory };
        var stdin = new EndedStdin();
        var stdout = new StringWriter();
        var run = CliMode.RunAsync(session, new PresenterCatalog([ascii]), options, stdin, stdout, new StringWriter());

        await transport.PushIncomingAsync(StreamContentSamples.Bmp());
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(5);
        while ((!Directory.Exists(_directory) || Directory.GetFiles(_directory, "*.bmp").Length == 0) && DateTime.UtcNow < deadline)
        {
            await Task.Delay(20, TestContext.CancellationToken);
        }

        stdin.Release.SetResult();
        await run.WaitAsync(TimeSpan.FromSeconds(5), TestContext.CancellationToken);

        Assert.HasCount(1, Directory.GetFiles(_directory, "*.bmp"));
        StringAssert.Contains(stdout.ToString(), "Captured");
        StringAssert.Contains(stdout.ToString(), ".bmp");
    }
}
