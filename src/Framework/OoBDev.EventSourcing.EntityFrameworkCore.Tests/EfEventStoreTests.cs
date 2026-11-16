using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.EntityFrameworkCore.Tests;

[TestClass]
public class EfEventStoreTests
{
    private SqliteDatabase _database = null!;
    private EfEventStore _store = null!;

    [TestInitialize]
    public void Initialize()
    {
        _database = new SqliteDatabase();
        _store = new EfEventStore(_database.Factory, TimeProvider.System);
    }

    [TestCleanup]
    public async Task Cleanup() => await _database.DisposeAsync();

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task AppendAsync_NewStream_AssignsSequenceAndPosition()
    {
        var result = await _store.AppendAsync("a", 0, [new NewEvent("Created", "{}"), new NewEvent("Renamed", "{\"n\":1}", "{\"by\":\"x\"}")]);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(2, result.LastSequence);
        CollectionAssert.AreEqual(new long[] { 1, 2 }, result.Events.Select(e => e.Sequence).ToArray());
        CollectionAssert.AreEqual(new long[] { 1, 2 }, result.Events.Select(e => e.Position).ToArray());
        Assert.AreEqual("{\"by\":\"x\"}", (await _store.ReadAsync("a"))[1].Metadata);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task AppendAsync_WrongExpectedSequence_ReportsConflictAndStoresNothing()
    {
        await _store.AppendAsync("a", 0, [new NewEvent("Created", "{}")]);

        var result = await _store.AppendAsync("a", 0, [new NewEvent("Again", "{}")]);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(1, result.LastSequence);
        Assert.AreEqual(1, (await _store.ReadAsync("a")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task AppendAsync_ConcurrentWriters_ExactlyOneWins()
    {
        var results = await Task.WhenAll(Enumerable.Range(0, 8)
            .Select(i => Task.Run(() => _store.AppendAsync("a", 0, [new NewEvent("E" + i, "{}")]))));

        Assert.AreEqual(1, results.Count(r => r.Succeeded));
        Assert.AreEqual(1, (await _store.ReadAsync("a")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task AppendAsync_InvalidJsonPayload_IsRejectedByTheDatabase()
    {
        await Assert.ThrowsAsync<Microsoft.EntityFrameworkCore.DbUpdateException>(() => _store.AppendAsync("a", 0, [new NewEvent("Bad", "not json")]));
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task ReadAsync_FromSequence_ReturnsTail()
    {
        await _store.AppendAsync("a", 0, [new NewEvent("1", "{}"), new NewEvent("2", "{}"), new NewEvent("3", "{}")]);

        var tail = await _store.ReadAsync("a", 2);

        CollectionAssert.AreEqual(new[] { "2", "3" }, tail.Select(e => e.Type).ToArray());
        Assert.AreEqual(0, (await _store.ReadAsync("missing")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task ReadAllAsync_FollowsGlobalPositionAcrossStreams()
    {
        await _store.AppendAsync("a", 0, [new NewEvent("a1", "{}")]);
        await _store.AppendAsync("b", 0, [new NewEvent("b1", "{}")]);
        await _store.AppendAsync("a", 1, [new NewEvent("a2", "{}")]);

        var all = await _store.ReadAllAsync(0);
        var after = await _store.ReadAllAsync(1, 1);

        CollectionAssert.AreEqual(new[] { "a1", "b1", "a2" }, all.Select(e => e.Type).ToArray());
        CollectionAssert.AreEqual(new[] { "b1" }, after.Select(e => e.Type).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task RewritePayloadsAsync_ChangesOnlyNamedSequences_AndIsIdempotent()
    {
        await _store.AppendAsync("s", 0, [new NewEvent("A", "{\"v\":1}"), new NewEvent("A", "{\"v\":2}")]);
        var changes = new Dictionary<long, string> { [2] = "{\"v\":0}" };

        var first = await _store.RewritePayloadsAsync("s", changes);
        var second = await _store.RewritePayloadsAsync("s", changes);

        Assert.AreEqual(1, first);
        Assert.AreEqual(0, second);
        var read = await _store.ReadAsync("s");
        Assert.AreEqual("{\"v\":1}", read[0].Payload);
        Assert.AreEqual("{\"v\":0}", read[1].Payload);
        Assert.AreEqual(2, read[1].Position);
    }
}
