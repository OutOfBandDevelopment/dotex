using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class StageWorkerTests
{
    private const string Stage = "hash";

    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.Zero));
    private readonly WorkQueueOptions _options = new() { BatchSize = 5 };

    private (InMemoryWorkQueue Queue, StageWorker Worker, Handler Handler) Create(Func<WorkItem, Task> work, WorkQueueOptions? options = null)
    {
        var settings = options ?? _options;
        var queue = new InMemoryWorkQueue(Options.Create(settings), _clock);
        var handler = new Handler(work);
        var worker = new StageWorker(Stage, queue, handler, Options.Create(settings), _clock, NullLogger<StageWorker>.Instance);
        return (queue, worker, handler);
    }

    [TestMethod]
    public async Task RunOnce_CompletesEveryClaimedItem()
    {
        var (queue, worker, handler) = Create(_ => Task.CompletedTask);
        await queue.EnqueueAsync(Stage, "a");
        await queue.EnqueueAsync(Stage, "b");

        var claimed = await worker.RunOnceAsync(CancellationToken.None);

        Assert.AreEqual(2, claimed);
        CollectionAssert.AreEquivalent(new[] { "a", "b" }, handler.Seen.ToArray());
        Assert.AreEqual(0, (await queue.StatsAsync(Stage)).Claimable);
        Assert.AreEqual(0, (await queue.StatsAsync(Stage)).Running);
    }

    [TestMethod]
    public async Task RunOnce_AFailingHandler_RecordsARetry()
    {
        var (queue, worker, _) = Create(_ => throw new InvalidOperationException("boom"));
        await queue.EnqueueAsync(Stage, "a");

        await worker.RunOnceAsync(CancellationToken.None);

        var stats = await queue.StatsAsync(Stage);
        Assert.AreEqual(0, stats.Running);
        Assert.AreEqual(0, stats.Dead);
        _clock.Advance(TimeSpan.FromMinutes(1));
        Assert.AreEqual(1, (await queue.StatsAsync(Stage)).Claimable);
    }

    [TestMethod]
    public async Task RunOnce_ANonRetryableFailure_IsDead()
    {
        var (queue, worker, _) = Create(_ => throw new NonRetryableWorkException("bad input"));
        await queue.EnqueueAsync(Stage, "a");

        await worker.RunOnceAsync(CancellationToken.None);

        Assert.AreEqual(1, (await queue.StatsAsync(Stage)).Dead);
    }

    [TestMethod]
    public async Task RunOnce_WithNothingToDo_ClaimsNothing()
    {
        var (_, worker, handler) = Create(_ => Task.CompletedTask);

        Assert.AreEqual(0, await worker.RunOnceAsync(CancellationToken.None));
        Assert.AreEqual(0, handler.Seen.Count);
    }

    [TestMethod]
    public async Task Heartbeat_KeepsALongRunningItemClaimed()
    {
        var options = new WorkQueueOptions { HeartbeatInterval = TimeSpan.FromMinutes(5) };
        options.Default.Lease = TimeSpan.FromMinutes(10);
        var release = new TaskCompletionSource();
        var started = new TaskCompletionSource();
        var (queue, worker, _) = Create(async _ =>
        {
            started.SetResult();
            await release.Task;
        }, options);
        await queue.EnqueueAsync(Stage, "a");

        var run = worker.RunOnceAsync(CancellationToken.None);
        await started.Task;
        for (var i = 0; i < 4; i++)
        {
            _clock.Advance(TimeSpan.FromMinutes(5));
            await Task.Delay(50);
        }

        Assert.AreEqual(0, (await queue.StatsAsync(Stage)).LeaseExpired);
        release.SetResult();
        await run;
        Assert.AreEqual(0, (await queue.StatsAsync(Stage)).Running);
    }

    [TestMethod]
    public async Task ExecuteAsync_ADisabledStage_DoesNothing()
    {
        var options = new WorkQueueOptions();
        options.Stages[Stage] = new StageSla { Enabled = false };
        var (queue, worker, handler) = Create(_ => Task.CompletedTask, options);
        await queue.EnqueueAsync(Stage, "a");

        await worker.StartAsync(CancellationToken.None);
        await worker.StopAsync(CancellationToken.None);

        Assert.AreEqual(0, handler.Seen.Count);
        Assert.AreEqual(1, (await queue.StatsAsync(Stage)).Claimable);
    }

    [TestMethod]
    public async Task ExecuteAsync_PollsUntilStopped()
    {
        var (queue, worker, handler) = Create(_ => Task.CompletedTask);
        await worker.StartAsync(CancellationToken.None);

        await queue.EnqueueAsync(Stage, "a");
        for (var i = 0; i < 50 && handler.Seen.Count == 0; i++)
        {
            _clock.Advance(_options.PollInterval);
            await Task.Delay(20);
        }
        await worker.StopAsync(CancellationToken.None);

        CollectionAssert.AreEqual(new[] { "a" }, handler.Seen.ToArray());
    }

    [TestMethod]
    public async Task Registration_AddsAHostedWorkerPerStage_AndUsesTheConfiguredHandler()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<TimeProvider>(_clock);
        services.TryAddInMemoryWorkQueue();
        services.AddWorkStage<CountingHandler>(Stage);
        services.AddWorkStage<CountingHandler>("other");
        await using var provider = services.BuildServiceProvider();

        var hosted = provider.GetServices<IHostedService>().OfType<StageWorker>().ToList();
        var queue = provider.GetRequiredService<IWorkQueue>();
        await queue.EnqueueAsync(Stage, "a");
        var claimed = await hosted[0].RunOnceAsync(CancellationToken.None);

        Assert.AreEqual(2, hosted.Count);
        Assert.AreEqual(1, claimed);
        Assert.IsInstanceOfType<InMemoryWorkQueue>(queue);
        Assert.AreSame(queue, provider.GetRequiredService<IWorkQueue>());
    }

    [TestMethod]
    public void Registration_RejectsABlankStageName()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new ServiceCollection().AddWorkStage<CountingHandler>(" "));
    }

    private sealed class Handler(Func<WorkItem, Task> work) : IWorkStageHandler
    {
        public List<string> Seen { get; } = [];

        public async Task HandleAsync(WorkItem item, CancellationToken cancellationToken)
        {
            lock (Seen)
                Seen.Add(item.Key);
            await work(item);
        }
    }

    private sealed class CountingHandler : IWorkStageHandler
    {
        public Task HandleAsync(WorkItem item, CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
