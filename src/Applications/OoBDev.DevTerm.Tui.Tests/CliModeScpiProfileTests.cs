using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Devices.Scpi;
using OoBDev.DevTerm.Presenters.Text;

namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>
/// <c>--scpiprofile</c> applies the profile's terminator to the <c>scpi</c> presenter in the CLI, so a
/// terminatorless instrument (the Rigol DS1102E) prints its reply instead of buffering forever.
/// </summary>
[TestCategory("Unit")]
[TestClass]
public sealed class CliModeScpiProfileTests
{
    private IReadOnlyList<IInstrumentPanelProvider> _original = [];

    public required TestContext TestContext { get; set; }

    [TestInitialize]
    public void RegisterScpiProvider()
    {
        _original = InstrumentPanelProviders.All;
        InstrumentPanelProviders.Set([new ScpiInstrumentPanelProvider()]);
    }

    [TestCleanup]
    public void RestoreProviders() => InstrumentPanelProviders.Set(_original);

    private sealed class BlockingStdin : TextReader
    {
        public override async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return null;
        }
    }

    private async Task<string> RunAsync(string? scpiProfile)
    {
        var transport = new FakeTransport();
        var scpi = new ScpiReplyPresenter();
        var session = new Session(transport, new Pipeline([scpi]));
        var catalog = new PresenterCatalog([scpi, new AsciiPresenter(Microsoft.Extensions.Options.Options.Create(new AsciiPresenterOptions()))]);
        var options = new CliOptions { Transport = "loopback", Presenter = ["scpi"], Parser = "ascii", ScpiProfile = scpiProfile };
        var stdout = new StringWriter();
        using var cts = new CancellationTokenSource();
        var run = CliMode.RunAsync(session, catalog, options, new BlockingStdin(), stdout, new StringWriter(), cts);
        while (!stdout.ToString().Contains("Connected to", StringComparison.Ordinal))
        {
            await Task.Delay(10, TestContext.CancellationToken);
        }

        await transport.PushIncomingAsync(Encoding.ASCII.GetBytes("Rigol Technologies,DS1102E"));
        await Task.Delay(300, TestContext.CancellationToken);
        await cts.CancelAsync();
        await run;
        return stdout.ToString();
    }

    [TestMethod]
    public async Task TerminatorlessProfile_PrintsAReplyWithNoLineEnding()
    {
        var output = await RunAsync("Rigol DS1102E Oscilloscope");

        StringAssert.Contains(output, "[scpi] Rigol Technologies,DS1102E");
    }

    [TestMethod]
    public async Task NoProfile_StillWaitsForALineEnding()
    {
        var output = await RunAsync(null);

        Assert.DoesNotContain("[scpi]", output);
    }
}
