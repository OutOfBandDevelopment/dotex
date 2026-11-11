using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class EventRelayTests
{
    private readonly FakeTimeProvider _time = new(new DateTimeOffset(2026, 11, 11, 12, 0, 0, TimeSpan.Zero));
    private readonly List<(object Message, string? Id)> _sent = [];
    private InMemoryEventStore _events = null!;
    private InMemoryEntityStore _entities = null!;

    private EventRelay Create(TimeSpan window, int batch = 500)
    {
        _events = new InMemoryEventStore(_time);
        _entities = new InMemoryEntityStore();
        var sender = new Mock<IMessageQueueSender<EventsChannel>>();
        sender
            .Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .Returns((object m, string? id, string? _, string? _, int _) =>
            {
                _sent.Add((m, id));
                return Task.FromResult(id ?? string.Empty);
            });
        var options = Options.Create(new EventRelayOptions { SafetyWindow = window, BatchSize = batch });
        return new EventRelay(_events, _entities, sender.Object, options, _time);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RunOnceAsync_SendsEventsOnceAndAdvancesCheckpoint()
    {
        var relay = Create(TimeSpan.Zero);
        await _events.AppendAsync("a", 0, [new NewEvent("A1", "{}"), new NewEvent("A2", "{}")]);

        var first = await relay.RunOnceAsync();
        var second = await relay.RunOnceAsync();

        Assert.AreEqual(2, first);
        Assert.AreEqual(0, second);
        Assert.AreEqual("a:1", _sent[0].Id);
        Assert.AreEqual("a:2", _sent[1].Id);
        Assert.AreEqual("2", (await _entities.GetAsync("checkpoint", "event-relay"))!.Value);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RunOnceAsync_YoungEventsWaitForTheSafetyWindow()
    {
        var relay = Create(TimeSpan.FromSeconds(5));
        await _events.AppendAsync("a", 0, [new NewEvent("A1", "{}")]);

        Assert.AreEqual(0, await relay.RunOnceAsync());

        _time.Advance(TimeSpan.FromSeconds(6));

        Assert.AreEqual(1, await relay.RunOnceAsync());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RunOnceAsync_BatchSizeLimitsEachPass()
    {
        var relay = Create(TimeSpan.Zero, batch: 2);
        await _events.AppendAsync("a", 0, [new NewEvent("1", "{}"), new NewEvent("2", "{}"), new NewEvent("3", "{}")]);

        Assert.AreEqual(2, await relay.RunOnceAsync());
        Assert.AreEqual(1, await relay.RunOnceAsync());
        Assert.AreEqual(3, _sent.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task RunOnceAsync_LostCheckpoint_Throws()
    {
        var relay = Create(TimeSpan.Zero);
        await _events.AppendAsync("a", 0, [new NewEvent("A1", "{}")]);
        var sender = new Mock<IMessageQueueSender<EventsChannel>>();
        sender
            .Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .Returns(async (object _, string? _, string? _, string? _, int _) =>
            {
                await _entities.SaveAsync("checkpoint", "event-relay", "99", 0);
                return "id";
            });
        var racing = new EventRelay(_events, _entities, sender.Object, Options.Create(new EventRelayOptions { SafetyWindow = TimeSpan.Zero }), _time);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => racing.RunOnceAsync());
    }
}
