using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.Tests;

public sealed record Ping(string Name);

public sealed record Other;

[ComplexEventHandler(typeof(Ping), Order = 2)]
public sealed class SecondHandler(List<string> log) : IComplexEventHandler<Ping>
{
    public Task HandleAsync(Ping @event, CancellationToken cancellationToken) { log.Add("second"); return Task.CompletedTask; }
}

[ComplexEventHandler(typeof(Ping), Order = 1)]
public sealed class FailingHandler(List<string> log) : IComplexEventHandler<Ping>
{
    public Task HandleAsync(Ping @event, CancellationToken cancellationToken) { log.Add("first"); throw new InvalidOperationException("boom"); }
}

[TestCategory("Unit")]
[TestClass]
public sealed class ComplexEventTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly byte[] Payload = [1];

    private static FakeTimeProvider NewClock() => new(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));

    [TestMethod]
    public async Task Dispatch_OrdersHandlersAndAggregatesFailures()
    {
        var log = new List<string>();
        var dispatcher = new ComplexEventDispatcher([new SecondHandler(log), new FailingHandler(log)]);

        var ex = await Assert.ThrowsExactlyAsync<AggregateException>(() => dispatcher.DispatchAsync(new Ping("a"), CancellationToken.None));
        TestContext.WriteLine(string.Join(",", log));

        CollectionAssert.AreEqual(new[] { "first", "second" }, log);
        Assert.AreEqual(1, ex.InnerExceptions.Count);
    }

    [TestMethod]
    public async Task Dispatch_NoMatchingHandler_DoesNothing()
    {
        var log = new List<string>();
        var dispatcher = new ComplexEventDispatcher([new SecondHandler(log)]);

        await dispatcher.DispatchAsync(new Other(), CancellationToken.None);

        Assert.AreEqual(0, log.Count);
    }

    [TestMethod]
    public async Task Runner_RunsOnceAtTheDueTimeAndReleasesIt()
    {
        var clock = NewClock();
        var store = new InMemoryScheduleStore(clock);
        var runs = 0;
        var runner = new ScheduleRunner(store, clock, (_, _) => { runs++; return Task.CompletedTask; }, TimeSpan.FromMinutes(1));
        await store.RegisterAsync("k", clock.GetUtcNow().AddHours(1), "ping", Payload, CancellationToken.None);

        Assert.AreEqual(0, await runner.RunDueAsync(10, CancellationToken.None));
        clock.Advance(TimeSpan.FromHours(1));
        Assert.AreEqual(1, await runner.RunDueAsync(10, CancellationToken.None));
        clock.Advance(TimeSpan.FromHours(1));
        Assert.AreEqual(0, await runner.RunDueAsync(10, CancellationToken.None));

        Assert.AreEqual(1, runs);
    }

    [TestMethod]
    public async Task Register_SameKey_MovesTheDueTimeWithoutDuplicate()
    {
        var clock = NewClock();
        var store = new InMemoryScheduleStore(clock);
        var first = await store.RegisterAsync("k", clock.GetUtcNow().AddHours(1), "ping", Payload, CancellationToken.None);
        var second = await store.RegisterAsync("k", clock.GetUtcNow().AddHours(5), "ping", Payload, CancellationToken.None);

        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual(0, (await store.LeaseDueAsync(clock.GetUtcNow().AddHours(2), 10, TimeSpan.FromMinutes(1), CancellationToken.None)).Count);
        Assert.IsTrue(await store.ReleaseAsync("k", CancellationToken.None));
        Assert.IsFalse(await store.ReleaseAsync("k", CancellationToken.None));
    }

    [TestMethod]
    public async Task Lease_HeldItemIsNotLeasedTwiceUntilItExpires()
    {
        var clock = NewClock();
        var store = new InMemoryScheduleStore(clock);
        await store.RegisterAsync("k", clock.GetUtcNow(), "ping", Payload, CancellationToken.None);

        var a = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);
        var b = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);
        clock.Advance(TimeSpan.FromMinutes(2));
        var c = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);

        Assert.AreEqual(1, a.Count);
        Assert.AreEqual(0, b.Count);
        Assert.AreEqual(1, c.Count);
        Assert.AreEqual(2, c[0].Attempts);
    }

    [TestMethod]
    public async Task Runner_FailingItem_RetriesThenMarksFailed()
    {
        var clock = NewClock();
        var store = new InMemoryScheduleStore(clock, maxAttempts: 2);
        var runner = new ScheduleRunner(store, clock, (_, _) => throw new InvalidOperationException("x"), TimeSpan.FromMinutes(1));
        await store.RegisterAsync("k", clock.GetUtcNow(), "ping", Payload, CancellationToken.None);

        for (var i = 0; i < 4; i++)
        {
            await runner.RunDueAsync(10, CancellationToken.None);
            clock.Advance(TimeSpan.FromMinutes(2));
        }

        Assert.AreEqual(1, store.FailedItems().Count);
        Assert.AreEqual(2, store.FailedItems()[0].Attempts);
    }
}
