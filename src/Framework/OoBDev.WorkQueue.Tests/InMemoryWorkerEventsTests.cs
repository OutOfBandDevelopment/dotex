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
public class InMemoryWorkerEventsTests
{
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 11, 17, 8, 0, 0, TimeSpan.Zero));
    private readonly WorkerEventsOptions _options = new() { MaxRecords = 3, MaxAge = TimeSpan.FromHours(1) };

    public required TestContext TestContext { get; set; }

    private InMemoryWorkerEvents Events() => new(Options.Create(_options), _clock);

    [TestMethod]
    public async Task Read_NewReader_StartsAtTheEndAsync()
    {
        var events = Events();
        await events.PublishAsync("hash", "old");

        var first = await events.ReadAsync("screen", 10);
        await events.PublishAsync("hash", "new");
        var second = await events.ReadAsync("screen", 10);

        TestContext.WriteLine("First read {0} events, second read {1}", first.Count, second.Count);
        Assert.AreEqual(0, first.Count);
        Assert.AreEqual("new", second.Single().Message);
    }

    [TestMethod]
    public async Task Read_FromStart_ReturnsEverythingOnceAsync()
    {
        var events = Events();
        await events.PublishAsync("hash", "a");
        await events.PublishAsync("hash", "b");

        var first = await events.ReadAsync("log", 10, fromStart: true);
        var again = await events.ReadAsync("log", 10, fromStart: true);

        Assert.AreEqual(2, first.Count);
        Assert.IsTrue(first[0].Id < first[1].Id);
        Assert.AreEqual(0, again.Count, "the mark moved to the last event returned");
    }

    [TestMethod]
    public async Task Read_Limit_ResumesFromTheMarkAsync()
    {
        var events = Events();
        foreach (var m in new[] { "a", "b", "c" })
            await events.PublishAsync("hash", m);

        var first = await events.ReadAsync("log", 2, fromStart: true);
        var second = await events.ReadAsync("log", 2, fromStart: true);

        CollectionAssert.AreEqual(new[] { "a", "b" }, first.Select(e => e.Message).ToArray());
        CollectionAssert.AreEqual(new[] { "c" }, second.Select(e => e.Message).ToArray());
    }

    [TestMethod]
    public async Task Trim_ByCount_KeepsTheNewestAsync()
    {
        var events = Events();
        foreach (var m in new[] { "a", "b", "c", "d", "e" })
            await events.PublishAsync("hash", m);

        var deleted = await events.TrimAsync();
        var kept = await events.ReadAsync("log", 10, fromStart: true);

        TestContext.WriteLine("Deleted {0}, kept {1}", deleted, kept.Count);
        Assert.AreEqual(2, deleted);
        CollectionAssert.AreEqual(new[] { "c", "d", "e" }, kept.Select(e => e.Message).ToArray());
    }

    [TestMethod]
    public async Task Trim_ByAge_DeletesOldEventsAndIdleReadersAsync()
    {
        var events = Events();
        await events.PublishAsync("hash", "old");
        await events.ReadAsync("tab", 10);
        _clock.Advance(TimeSpan.FromHours(2));
        await events.PublishAsync("hash", "fresh");

        var deleted = await events.TrimAsync();
        var returning = await events.ReadAsync("tab", 10);

        Assert.AreEqual(1, deleted);
        Assert.AreEqual(0, returning.Count, "an idle reader that returns restarts at the end");
    }
}
