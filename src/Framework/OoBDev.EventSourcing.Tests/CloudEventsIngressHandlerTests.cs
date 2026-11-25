using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.EventSourcing.Tests;

[TestClass]
public class CloudEventsIngressHandlerTests
{
    private static string Event(long sequence) => CloudEventMapper.ToCloudEvent(
        new EventRecord("s1", sequence, "Created", DateTimeOffset.UnixEpoch, "{\"n\":1}", null, 0), "test");

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_NextInSequence_AppendsToTheStore()
    {
        // Stage
        var store = new InMemoryEventStore(TimeProvider.System);
        var handler = new CloudEventsIngressHandler(store);

        // Test
        await handler.HandleAsync(Event(1), Mock.Of<IMessageContext>());
        await ((IMessageQueueHandler)handler).HandleAsync(Event(2), Mock.Of<IMessageContext>());

        // Assert
        CollectionAssert.AreEqual(new long[] { 1, 2 }, (await store.ReadAsync("s1")).Select(e => e.Sequence).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_GapInSequence_Throws()
    {
        // Stage
        var handler = new CloudEventsIngressHandler(new InMemoryEventStore(TimeProvider.System));

        // Test / Assert
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => handler.HandleAsync(Event(3), Mock.Of<IMessageContext>()));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_NonStringObject_Throws()
    {
        // Stage
        var handler = new CloudEventsIngressHandler(new InMemoryEventStore(TimeProvider.System));

        // Test / Assert
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => ((IMessageQueueHandler)handler).HandleAsync(42, Mock.Of<IMessageContext>()));
    }
}
