using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class ProjectionRunnerTests
{
    private sealed class ListProjection(string name = "list") : IProjection
    {
        public string Name { get; } = name;
        public List<string> Seen { get; } = [];

        public Task ApplyAsync(EventRecord record, CancellationToken cancellationToken = default)
        {
            Seen.Add(record.Type);
            return Task.CompletedTask;
        }
    }

    private static async Task<(InMemoryEventStore Events, InMemoryEntityStore Entities)> Stores(int count)
    {
        var events = new InMemoryEventStore(TimeProvider.System);
        await events.AppendAsync("s", 0, Enumerable.Range(1, count).Select(i => new NewEvent($"E{i}", "{}")).ToList());
        return (events, new InMemoryEntityStore());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CatchUpAsync_AppliesAllEventsInBatches()
    {
        // Stage
        var (events, entities) = await Stores(5);
        var projection = new ListProjection();

        // Test
        var applied = await new ProjectionRunner(events, entities, [projection]).CatchUpAsync(batchSize: 2);

        // Assert
        Assert.AreEqual(5, applied);
        CollectionAssert.AreEqual(new[] { "E1", "E2", "E3", "E4", "E5" }, projection.Seen);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CatchUpAsync_SecondRun_OnlyAppliesNewEvents()
    {
        // Stage
        var (events, entities) = await Stores(2);
        var projection = new ListProjection();
        var runner = new ProjectionRunner(events, entities, [projection]);
        await runner.CatchUpAsync();
        await events.AppendAsync("s", 2, [new NewEvent("E3", "{}")]);

        // Test
        var applied = await runner.CatchUpAsync();

        // Assert
        Assert.AreEqual(1, applied);
        CollectionAssert.AreEqual(new[] { "E1", "E2", "E3" }, projection.Seen);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task CatchUpAsync_ProjectionsKeepSeparateCheckpoints()
    {
        // Stage
        var (events, entities) = await Stores(2);
        var first = new ListProjection("first");
        await new ProjectionRunner(events, entities, [first]).CatchUpAsync();
        var second = new ListProjection("second");

        // Test
        await new ProjectionRunner(events, entities, [first, second]).CatchUpAsync();

        // Assert
        Assert.AreEqual(2, first.Seen.Count);
        Assert.AreEqual(2, second.Seen.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ResetAsync_ReplaysFromTheBeginning()
    {
        // Stage
        var (events, entities) = await Stores(2);
        var projection = new ListProjection();
        var runner = new ProjectionRunner(events, entities, [projection]);
        await runner.CatchUpAsync();

        // Test
        await runner.ResetAsync("list");
        var applied = await runner.CatchUpAsync();

        // Assert
        Assert.AreEqual(2, applied);
        Assert.AreEqual(4, projection.Seen.Count);
    }
}
