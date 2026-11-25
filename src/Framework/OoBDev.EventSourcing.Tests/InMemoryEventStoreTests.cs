using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class InMemoryEventStoreTests
{
    private static InMemoryEventStore Create() => new(TimeProvider.System);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AppendAsync_NewStream_AssignsSequenceAndPosition()
    {
        var store = Create();

        var result = await store.AppendAsync("a", 0, [new NewEvent("Created", "{}"), new NewEvent("Renamed", "{}")]);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(2, result.LastSequence);
        CollectionAssert.AreEqual(new long[] { 1, 2 }, result.Events.Select(e => e.Sequence).ToArray());
        CollectionAssert.AreEqual(new long[] { 1, 2 }, result.Events.Select(e => e.Position).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AppendAsync_WrongExpectedSequence_ReportsConflictAndStoresNothing()
    {
        var store = Create();
        await store.AppendAsync("a", 0, [new NewEvent("Created", "{}")]);

        var result = await store.AppendAsync("a", 0, [new NewEvent("Again", "{}")]);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(1, result.LastSequence);
        Assert.AreEqual(0, result.Events.Count);
        Assert.AreEqual(1, (await store.ReadAsync("a")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AppendAsync_ConcurrentWriters_ExactlyOneWins()
    {
        var store = Create();

        var results = await Task.WhenAll(Enumerable.Range(0, 20)
            .Select(i => Task.Run(() => store.AppendAsync("a", 0, [new NewEvent("E" + i, "{}")]))));

        Assert.AreEqual(1, results.Count(r => r.Succeeded));
        Assert.AreEqual(1, (await store.ReadAsync("a")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ReadAsync_FromSequence_ReturnsTail()
    {
        var store = Create();
        await store.AppendAsync("a", 0, [new NewEvent("1", "{}"), new NewEvent("2", "{}"), new NewEvent("3", "{}")]);

        var tail = await store.ReadAsync("a", 2);

        CollectionAssert.AreEqual(new[] { "2", "3" }, tail.Select(e => e.Type).ToArray());
        Assert.AreEqual(0, (await store.ReadAsync("missing")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ReadAllAsync_FollowsGlobalPositionAcrossStreams()
    {
        var store = Create();
        await store.AppendAsync("a", 0, [new NewEvent("a1", "{}")]);
        await store.AppendAsync("b", 0, [new NewEvent("b1", "{}")]);
        await store.AppendAsync("a", 1, [new NewEvent("a2", "{}")]);

        var all = await store.ReadAllAsync(0);
        var after = await store.ReadAllAsync(1, 1);

        CollectionAssert.AreEqual(new[] { "a1", "b1", "a2" }, all.Select(e => e.Type).ToArray());
        CollectionAssert.AreEqual(new[] { "b1" }, after.Select(e => e.Type).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task AppendAsync_BlankStreamId_Throws()
    {
        var store = Create();

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => store.AppendAsync(" ", 0, [new NewEvent("x", "{}")]));
    }
}
