using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class RecurringSchedulerTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly DateTimeOffset Start = new(2026, 11, 23, 10, 0, 0, TimeSpan.Zero);

    private static RecurringScheduler Create(IScheduleStore store, FakeTimeProvider clock, ILastRunStore lastRuns, CatchUpPolicy policy)
        => new(lastRuns, new Dictionary<string, RecurringSchedule> { ["hb"] = new(store, clock, "hb", new CronRecurrence("*/15 * * * *"), "Heartbeat", new byte[] { 1 }, policy) });

    [TestMethod]
    public async Task LastRunStore_OlderValue_NeverReplacesNewer()
    {
        var store = new InMemoryLastRunStore();

        await store.SetAsync("k", Start, CancellationToken.None);
        await store.SetAsync("k", Start.AddHours(-1), CancellationToken.None);

        Assert.AreEqual(Start, await store.GetAsync("k", CancellationToken.None));
        Assert.IsNull(await store.GetAsync("other", CancellationToken.None));
    }

    [TestMethod]
    public async Task Completed_RecordsTheRunAndRegistersTheNextOccurrence()
    {
        var clock = new FakeTimeProvider(Start);
        var store = new InMemoryScheduleStore(clock);
        var lastRuns = new InMemoryLastRunStore();
        var scheduler = Create(store, clock, lastRuns, CatchUpPolicy.Skip);
        var item = new ScheduleItem(Guid.NewGuid(), $"hb@{Start.AddMinutes(15).UtcTicks}", Start.AddMinutes(15), "Heartbeat", new byte[] { 1 });

        var handled = await scheduler.CompletedAsync(item, CancellationToken.None);
        var unrelated = await scheduler.CompletedAsync(item with { Key = "other" }, CancellationToken.None);

        Assert.IsTrue(handled);
        Assert.IsFalse(unrelated);
        Assert.AreEqual(item.DueAt, await lastRuns.GetAsync("hb", CancellationToken.None));
        clock.Advance(TimeSpan.FromMinutes(40));
        var due = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);
        TestContext.WriteLine(string.Join(",", due.Select(d => d.Key)));
        Assert.AreEqual(1, due.Count);
        Assert.AreEqual(Start.AddMinutes(30), due[0].DueAt);
    }

    [TestMethod]
    public async Task Start_AfterDowntime_CatchesUpFromTheRecordedLastRun()
    {
        var clock = new FakeTimeProvider(Start);
        var store = new InMemoryScheduleStore(clock);
        var lastRuns = new InMemoryLastRunStore();
        await lastRuns.SetAsync("hb", Start.AddMinutes(-30), CancellationToken.None);

        await Create(store, clock, lastRuns, CatchUpPolicy.RunAll).StartAsync(CancellationToken.None);

        var due = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);
        CollectionAssert.AreEqual(new[] { Start.AddMinutes(-15), Start }, due.Select(d => d.DueAt).ToArray());
    }
}
