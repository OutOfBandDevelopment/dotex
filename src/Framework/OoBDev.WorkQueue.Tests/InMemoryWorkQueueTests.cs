using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.WorkQueue.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class InMemoryWorkQueueTests
{
    private const string Stage = "hash";

    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 16, 8, 0, 0, TimeSpan.Zero));
    private readonly WorkQueueOptions _options = new()
    {
        BackoffBase = TimeSpan.FromSeconds(30),
        BackoffMax = TimeSpan.FromMinutes(10),
    };

    private InMemoryWorkQueue Queue() => new(Options.Create(_options), _clock);

    [TestMethod]
    public async Task Enqueue_IsIdempotent()
    {
        var queue = Queue();

        Assert.IsTrue(await queue.EnqueueAsync(Stage, "a"));
        Assert.IsFalse(await queue.EnqueueAsync(Stage, "a"));
        Assert.AreEqual(1, (await queue.StatsAsync(Stage)).Claimable);
    }

    [TestMethod]
    public async Task Claim_TwoWorkersRacing_EachItemGoesToOne()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");

        var first = await queue.ClaimAsync(Stage, "w1", 5);
        var second = await queue.ClaimAsync(Stage, "w2", 5);

        Assert.AreEqual(1, first.Count);
        Assert.AreEqual(1, first[0].Attempt);
        Assert.AreEqual(0, second.Count);
    }

    [TestMethod]
    public async Task Claim_TakesOldestFirstUpToBatch()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "b");
        _clock.Advance(TimeSpan.FromSeconds(1));
        await queue.EnqueueAsync(Stage, "a");
        _clock.Advance(TimeSpan.FromSeconds(1));
        await queue.EnqueueAsync(Stage, "c");

        var items = await queue.ClaimAsync(Stage, "w", 2);

        CollectionAssert.AreEqual(new[] { "b", "a" }, items.Select(i => i.Key).ToArray());
    }

    [TestMethod]
    public async Task Complete_FromWorkerWhoseLeaseExpired_IsRejected()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");
        var old = (await queue.ClaimAsync(Stage, "w1", 1))[0];

        _clock.Advance(_options.Default.Lease + TimeSpan.FromSeconds(1));
        var fresh = (await queue.ClaimAsync(Stage, "w2", 1))[0];

        Assert.AreEqual(2, fresh.Attempt);
        Assert.IsFalse(await queue.CompleteAsync(old));
        Assert.IsNull(await queue.FailAsync(old, "late"));
        Assert.IsFalse(await queue.HeartbeatAsync(old));
        Assert.IsTrue(await queue.CompleteAsync(fresh, WorkStatus.Skipped, "nothing to do"));
    }

    [TestMethod]
    public async Task Heartbeat_ExtendsTheLease_ButNotPastMaxRuntime()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");
        var item = (await queue.ClaimAsync(Stage, "w", 1))[0];

        _clock.Advance(TimeSpan.FromMinutes(10));
        Assert.IsTrue(await queue.HeartbeatAsync(item));
        _clock.Advance(TimeSpan.FromMinutes(10));
        Assert.AreEqual(0, (await queue.StatsAsync(Stage)).LeaseExpired);

        _clock.Advance(_options.Default.MaxRuntime);
        Assert.IsFalse(await queue.HeartbeatAsync(item));
        Assert.AreEqual(1, (await queue.StatsAsync(Stage)).LeaseExpired);
    }

    [TestMethod]
    public async Task Fail_Retryable_ReturnsToPendingAfterTheBackoffSchedule()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");

        var first = (await queue.ClaimAsync(Stage, "w", 1))[0];
        Assert.AreEqual(WorkStatus.Pending, await queue.FailAsync(first, "boom"));
        Assert.AreEqual(0, (await queue.StatsAsync(Stage)).Claimable);
        Assert.AreEqual(0, (await queue.ClaimAsync(Stage, "w", 1)).Count);

        _clock.Advance(TimeSpan.FromSeconds(30));
        var second = (await queue.ClaimAsync(Stage, "w", 1))[0];
        Assert.AreEqual(2, second.Attempt);
        Assert.AreEqual(WorkStatus.Pending, await queue.FailAsync(second, "boom"));

        _clock.Advance(TimeSpan.FromSeconds(59));
        Assert.AreEqual(0, (await queue.ClaimAsync(Stage, "w", 1)).Count);
        _clock.Advance(TimeSpan.FromSeconds(1));
        Assert.AreEqual(1, (await queue.ClaimAsync(Stage, "w", 1)).Count);
    }

    [TestMethod]
    public async Task Fail_OnTheLastAttempt_IsDeadWithADeadLetter()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");

        for (var attempt = 1; attempt <= _options.Default.MaxAttempts; attempt++)
        {
            _clock.Advance(TimeSpan.FromHours(1));
            var item = (await queue.ClaimAsync(Stage, "w", 1))[0];
            var status = await queue.FailAsync(item, new string('x', 600));
            Assert.AreEqual(attempt == _options.Default.MaxAttempts ? WorkStatus.Dead : WorkStatus.Pending, status);
        }

        var stats = await queue.StatsAsync(Stage);
        Assert.AreEqual(1, stats.Dead);
        Assert.AreEqual(0, stats.Claimable);
        var letter = (await queue.DeadLettersAsync(Stage)).Single();
        Assert.AreEqual("attempts exhausted", letter.Reason);
        Assert.AreEqual(500, letter.LastError!.Length);
        Assert.IsNull(letter.RedrivenAt);
    }

    [TestMethod]
    public async Task Fail_NonRetryable_IsDeadAtOnce()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");
        var item = (await queue.ClaimAsync(Stage, "w", 1))[0];

        Assert.AreEqual(WorkStatus.Dead, await queue.FailAsync(item, "bad input", retryable: false));

        Assert.AreEqual("non-retryable", (await queue.DeadLettersAsync(Stage)).Single().Reason);
    }

    [TestMethod]
    public async Task Claim_SweepsAnItemThatKeptHanging_ToParked()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");
        for (var attempt = 0; attempt < _options.Default.MaxAttempts; attempt++)
        {
            Assert.AreEqual(1, (await queue.ClaimAsync(Stage, "w", 1)).Count);
            _clock.Advance(_options.Default.Lease + TimeSpan.FromSeconds(1));
        }

        Assert.AreEqual(0, (await queue.ClaimAsync(Stage, "w", 1)).Count);

        var stats = await queue.StatsAsync(Stage);
        Assert.AreEqual(1, stats.Parked);
        Assert.AreEqual("lease expired", (await queue.DeadLettersAsync(Stage)).Single().Reason);
    }

    [TestMethod]
    public async Task Redrive_ReturnsDeadAndParkedWithAFreshBudget()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");
        await queue.EnqueueAsync(Stage, "b");
        foreach (var item in await queue.ClaimAsync(Stage, "w", 2))
            await queue.FailAsync(item, "x", retryable: false);

        Assert.AreEqual(1, await queue.RedriveAsync(Stage, "a"));
        Assert.AreEqual(1, await queue.RedriveAsync(Stage));
        Assert.AreEqual(0, await queue.RedriveAsync(Stage));

        var again = await queue.ClaimAsync(Stage, "w", 5);
        Assert.AreEqual(2, again.Count);
        Assert.IsTrue(again.All(i => i.Attempt == 1));
        Assert.IsTrue((await queue.DeadLettersAsync(Stage)).All(d => d.RedrivenAt is not null));
    }

    [TestMethod]
    public async Task Complete_RejectsAnUnknownFinalStatus()
    {
        var queue = Queue();
        await queue.EnqueueAsync(Stage, "a");
        var item = (await queue.ClaimAsync(Stage, "w", 1))[0];

        await Assert.ThrowsExactlyAsync<ArgumentOutOfRangeException>(() => queue.CompleteAsync(item, WorkStatus.Dead));
    }

    [TestMethod]
    public async Task Stats_AreScopedToTheirStage_AndStageOverridesApply()
    {
        _options.Stages["slow"] = new StageSla { MaxAttempts = 1, Lease = TimeSpan.FromHours(1) };
        var queue = Queue();
        await queue.EnqueueAsync("slow", "a");
        await queue.EnqueueAsync(Stage, "b");

        var item = (await queue.ClaimAsync("slow", "w", 1))[0];
        _clock.Advance(TimeSpan.FromMinutes(30));

        Assert.AreEqual(1, (await queue.StatsAsync("slow")).Running);
        Assert.AreEqual(0, (await queue.StatsAsync("slow")).LeaseExpired);
        Assert.AreEqual(1, (await queue.StatsAsync(Stage)).Claimable);
        Assert.AreEqual(WorkStatus.Dead, await queue.FailAsync(item, "x"));
    }
}
