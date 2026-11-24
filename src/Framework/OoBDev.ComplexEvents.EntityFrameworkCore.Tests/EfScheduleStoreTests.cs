using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.ComplexEvents.EntityFrameworkCore.Tests;

[TestCategory("Simulate")]
[TestClass]
public sealed class EfScheduleStoreTests
{
    public TestContext TestContext { get; set; } = null!;

    private static readonly byte[] Payload = [7, 8];

    private SqliteScheduleDatabase database = null!;
    private FakeTimeProvider clock = null!;

    [TestInitialize]
    public void Setup()
    {
        database = new SqliteScheduleDatabase();
        clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 19, 0, 0, 0, TimeSpan.Zero));
    }

    [TestCleanup]
    public async Task Cleanup() => await database.DisposeAsync();

    private EfScheduleStore Store(int maxAttempts = 3) => new(database.Factory, clock, maxAttempts);

    [TestMethod]
    public async Task Register_SameKey_UpdatesTheSingleRow()
    {
        var store = Store();
        var first = await store.RegisterAsync("k", clock.GetUtcNow().AddHours(1), "ping", Payload, CancellationToken.None);
        var second = await store.RegisterAsync("k", clock.GetUtcNow().AddHours(5), "pong", Payload, CancellationToken.None);

        Assert.AreEqual(first.Id, second.Id);
        Assert.AreEqual("pong", second.EventType);
        CollectionAssert.AreEqual(Payload, second.Payload.ToArray());
        Assert.AreEqual(0, (await store.LeaseDueAsync(clock.GetUtcNow().AddHours(2), 10, TimeSpan.FromMinutes(1), CancellationToken.None)).Count);
    }

    [TestMethod]
    public async Task Lease_TwoStoresOnTheSameDatabase_NeverLeaseTheSameItem()
    {
        var a = Store();
        var b = Store();
        for (var i = 0; i < 10; i++) await a.RegisterAsync($"k{i}", clock.GetUtcNow(), "ping", Payload, CancellationToken.None);

        var results = await Task.WhenAll(
            a.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None),
            b.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None));
        var ids = results.SelectMany(r => r).Select(i => i.Id).ToList();
        TestContext.WriteLine($"a={results[0].Count} b={results[1].Count}");

        Assert.AreEqual(10, ids.Count);
        Assert.AreEqual(10, ids.Distinct().Count());
    }

    [TestMethod]
    public async Task Lease_ExpiresThenFailsAfterTheAttemptLimit()
    {
        var store = Store(maxAttempts: 2);
        await store.RegisterAsync("k", clock.GetUtcNow(), "ping", Payload, CancellationToken.None);

        var counts = new int[4];
        for (var i = 0; i < 4; i++)
        {
            counts[i] = (await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None)).Count;
            clock.Advance(TimeSpan.FromMinutes(2));
        }

        CollectionAssert.AreEqual(new[] { 1, 1, 0, 0 }, counts);
        var failed = await store.FailedItemsAsync(CancellationToken.None);
        Assert.AreEqual(1, failed.Count);
        Assert.AreEqual(2, failed[0].Attempts);
    }

    [TestMethod]
    public async Task Complete_RemovesTheItemAndRestartDoesNotRunItAgain()
    {
        var store = Store();
        await store.RegisterAsync("k", clock.GetUtcNow(), "ping", Payload, CancellationToken.None);
        var leased = await store.LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);
        await store.CompleteAsync(leased[0].Id, CancellationToken.None);
        clock.Advance(TimeSpan.FromHours(1));

        var again = await Store().LeaseDueAsync(clock.GetUtcNow(), 10, TimeSpan.FromMinutes(1), CancellationToken.None);

        Assert.AreEqual(0, again.Count);
        Assert.IsFalse(await store.ReleaseAsync("k", CancellationToken.None));
    }

    [TestMethod]
    public async Task Runner_WorksOverTheDatabaseStore()
    {
        var store = Store();
        var runs = 0;
        var runner = new ScheduleRunner(store, clock, (_, _) => { runs++; return Task.CompletedTask; }, TimeSpan.FromMinutes(1));
        await store.RegisterAsync("k", clock.GetUtcNow().AddMinutes(5), "ping", Payload, CancellationToken.None);

        Assert.AreEqual(0, await runner.RunDueAsync(10, CancellationToken.None));
        clock.Advance(TimeSpan.FromMinutes(5));
        Assert.AreEqual(1, await runner.RunDueAsync(10, CancellationToken.None));
        Assert.AreEqual(0, await runner.RunDueAsync(10, CancellationToken.None));
        Assert.AreEqual(1, runs);
    }
}
