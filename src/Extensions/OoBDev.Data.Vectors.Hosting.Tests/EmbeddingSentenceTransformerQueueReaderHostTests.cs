using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Data.Vectors.Hosting;
using OoBDev.TestUtilities;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.Vectors.Hosting.Tests;

[TestClass]
public class EmbeddingSentenceTransformerQueueReaderHostTests
{
    public required TestContext TestContext { get; set; }

    private sealed class FailingReader : IEmbeddingSentenceTransformerQueueReader
    {
        private readonly TaskCompletionSource _second = new();
        private readonly Stopwatch _clock = Stopwatch.StartNew();
        private int _runs;

        public Task SecondRun => _second.Task;

        public TimeSpan SecondRunAt { get; private set; }

        public Task RunAsync(CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _runs) == 2)
            {
                SecondRunAt = _clock.Elapsed;
                _second.TrySetResult();
            }

            throw new InvalidOperationException("reader failed");
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task StartAsync_ReaderFails_RestartsAfterTheConfiguredDelay()
    {
        TestContext.WriteLine("Stage: a reader that always fails and a restart delay of 300 ms");
        var reader = new FailingReader();
        var options = Options.Create(new EmbeddingSentenceTransformerQueueReaderOptions { RestartDelay = TimeSpan.FromMilliseconds(300) });
        using var host = new EmbeddingSentenceTransformerQueueReaderHost(
            reader, TestContext.GetLogger<EmbeddingSentenceTransformerQueueReaderHost>(), options);

        TestContext.WriteLine("Test: start the host and wait for the second run");
        await host.StartAsync(CancellationToken.None).ConfigureAwait(false);
        await reader.SecondRun.WaitAsync(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
        await host.StopAsync(CancellationToken.None).ConfigureAwait(false);

        TestContext.WriteLine($"Assert: second run after {reader.SecondRunAt.TotalMilliseconds:F0} ms");
        Assert.IsGreaterThanOrEqualTo(250, reader.SecondRunAt.TotalMilliseconds);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Options_Default_RestartDelayIsTenSeconds()
    {
        Assert.AreEqual(TimeSpan.FromSeconds(10), new EmbeddingSentenceTransformerQueueReaderOptions().RestartDelay);
    }
}
