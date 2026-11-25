using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class AggregateSnapshotTests
{
    public sealed record Counted(int By);

    public sealed class Counter : AggregateRoot, ISnapshotable
    {
        public int Total { get; private set; }

        public void Open(string id) => Id = id;

        public void Add(int by) => Raise(new Counted(by));

        public string CaptureState() => Total.ToString();

        public void RestoreState(string state) => Total = int.Parse(state);

        protected override void Apply(object @event)
        {
            if (@event is Counted counted)
                Total += counted.By;
        }
    }

    private static AggregateRepository<Counter> Create(IEventStore events, IEntityStore entities, int interval)
        => new(events, new EventTypeRegistry().Register<Counted>(), null, entities, Options.Create(new SnapshotOptions { Interval = interval }));

    private static async Task Save(AggregateRepository<Counter> repository, int times)
    {
        var counter = new Counter();
        counter.Open("c1");
        for (var i = 0; i < times; i++)
            counter.Add(1);
        await repository.SaveAsync(counter);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SaveAsync_PassingTheInterval_StoresSnapshot()
    {
        // Stage
        var entities = new InMemoryEntityStore();
        var repository = Create(new InMemoryEventStore(TimeProvider.System), entities, 2);

        // Test
        await Save(repository, 3);

        // Assert
        var snapshots = await entities.QueryAsync("aggregate-snapshot");
        Assert.AreEqual(1, snapshots.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task LoadAsync_WithSnapshot_ReplaysOnlyTheTail()
    {
        // Stage
        var events = new InMemoryEventStore(TimeProvider.System);
        var entities = new InMemoryEntityStore();
        var repository = Create(events, entities, 2);
        await Save(repository, 3);
        var key = AggregateRepository<Counter>.StreamIdOf("c1");
        var stored = (await entities.GetAsync("aggregate-snapshot", key))!;
        await entities.SaveAsync("aggregate-snapshot", key, "{\"Version\":2,\"State\":\"100\"}", stored.Version);

        // Test
        var loaded = await repository.LoadAsync("c1");

        // Assert (100 from the tampered snapshot plus the one event after version 2 proves the snapshot was used)
        Assert.AreEqual(101, loaded!.Total);
        Assert.AreEqual(3, loaded.Version);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task LoadAsync_IntervalZero_NeverSnapshotsAndReplaysAll()
    {
        // Stage
        var entities = new InMemoryEntityStore();
        var repository = Create(new InMemoryEventStore(TimeProvider.System), entities, 0);
        await Save(repository, 5);

        // Test
        var loaded = await repository.LoadAsync("c1");

        // Assert
        Assert.AreEqual(5, loaded!.Total);
        Assert.AreEqual(0, (await entities.QueryAsync("aggregate-snapshot")).Count);
    }
}
