using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.Services;

[TestClass]
public class OutboxTests
{
    private sealed class StepClock : TimeProvider
    {
        private DateTimeOffset _now = new(2026, 11, 16, 0, 0, 0, TimeSpan.Zero);

        public void Advance(TimeSpan by) => _now += by;

        public override DateTimeOffset GetUtcNow() => _now;
    }

    private static Mock<IMessageQueueSender> RealSender() => new();

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_StoresInsteadOfSending()
    {
        var store = new InMemoryOutboxStore();

        var id = await new OutboxMessageQueueSender(store, new StepClock()).SendAsync("hello", "m1");

        Assert.AreEqual("m1", id);
        var pending = await store.GetPendingAsync(10);
        Assert.HasCount(1, pending);
        Assert.AreEqual("hello", pending[0].Message);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_NoId_GeneratesOne()
    {
        var id = await new OutboxMessageQueueSender(new InMemoryOutboxStore(), new StepClock()).SendAsync("hello");

        Assert.IsFalse(string.IsNullOrEmpty(id));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task DispatchAsync_SendsPendingOldestFirstAndMarksThemSent()
    {
        var store = new InMemoryOutboxStore();
        var clock = new StepClock();
        var queue = new OutboxMessageQueueSender(store, clock);
        await queue.SendAsync("first", "a");
        clock.Advance(TimeSpan.FromSeconds(1));
        await queue.SendAsync("second", "b");
        var order = new List<string>();
        var real = RealSender();
        real.Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .Callback<object, string?, string?, string?, int>((m, id, _, _, _) => order.Add($"{id}:{m}"))
            .ReturnsAsync("x");

        var sent = await new OutboxDispatcher(store, real.Object).DispatchAsync();

        Assert.AreEqual(2, sent);
        CollectionAssert.AreEqual(new[] { "a:first", "b:second" }, order);
        Assert.IsEmpty(await store.GetPendingAsync(10));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task DispatchAsync_SendFails_KeepsTheEntryForTheNextRun()
    {
        var store = new InMemoryOutboxStore();
        await new OutboxMessageQueueSender(store, new StepClock()).SendAsync("hello", "a");
        var real = RealSender();
        real.Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("broker down"));

        await Assert.ThrowsAsync<InvalidOperationException>(() => new OutboxDispatcher(store, real.Object).DispatchAsync());

        Assert.HasCount(1, await store.GetPendingAsync(10));
    }
}
