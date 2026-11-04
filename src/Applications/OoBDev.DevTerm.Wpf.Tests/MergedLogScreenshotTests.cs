using System;
using System.IO.Pipelines;
using System.Threading;
using System.Threading.Tasks;
using DevTermApp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.TestUtilities;
using OoBDev.System.IO;
using OoBDev.System.IO.Pipelines;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The merged log window with two devices interleaved, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class MergedLogScreenshotTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class FakeAdapter(Pipe input) : IDeviceAdapter
    {
        public string Type => "loopback";
        public string Path => "loopback";
        public ConnectionState State => ConnectionState.Open;
        public event EventHandler<ConnectionStateChangedEventArgs>? StateChanged { add { } remove { } }
        public PipeReader Input => input.Reader;
        public PipeWriter Output { get; } = DeviceAdapterOutput.Create((_, _) => Task.CompletedTask);
        public Task OpenAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task CloseAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    /// <summary>Two interleaved devices in a merged log; the sessions are returned so the caller disposes them after use.</summary>
    internal static async Task<(MergedSessionLog Log, IAsyncDisposable Sessions)> BuildLogAsync(CancellationToken cancellationToken)
    {
        var log = new MergedSessionLog(new ManualTimeProvider());
        var scopePipe = new Pipe();
        var scope = new Session(new FakeAdapter(scopePipe), new Pipeline([]));
        var psu = new Session(new FakeAdapter(new Pipe()), new Pipeline([]));
        log.Track("scope", scope, "scope");
        log.Track("psu", psu, "psu");
        await scope.OpenAsync(cancellationToken);
        await psu.OpenAsync(cancellationToken);
        await scope.SendAsync("*IDN?\n"u8.ToArray(), cancellationToken);
        await psu.SendAsync("VOUT1?\n"u8.ToArray(), cancellationToken);
        await scopePipe.Writer.WriteAsync("TEKTRONIX,TDS2024C\n"u8.ToArray(), cancellationToken);
        SpinWait.SpinUntil(() => log.Entries.Count >= 5, TimeSpan.FromSeconds(5));
        return (log, new SessionPair(scope, psu));
    }

    internal sealed class SessionPair(Session first, Session second) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await first.DisposeAsync();
            await second.DisposeAsync();
        }
    }

    [TestMethod]
    public async Task MergedLogWindow_TwoDevices_IsCaptured()
    {
        var (log, sessions) = await BuildLogAsync(TestContext.CancellationToken);
        await using var _ = sessions;

        WpfScreenshot.Capture(() => new MergedLogWindow(new MergedLogDialogViewModel(log)), "wpf-merged-log.png", 700, 360);
    }
}
