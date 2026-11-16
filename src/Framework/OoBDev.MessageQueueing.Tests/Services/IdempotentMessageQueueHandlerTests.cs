using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using System;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.Services;

[TestClass]
public class IdempotentMessageQueueHandlerTests
{
    private static IMessageContext Context(string? sentId)
    {
        var context = new Mock<IMessageContext>();
        context.SetupGet(c => c.SentId).Returns(sentId);
        return context.Object;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_Redelivery_HandlesOnce()
    {
        var inner = new Mock<IMessageQueueHandler>();
        var handler = new IdempotentMessageQueueHandler("c", inner.Object, new InMemoryInboxStore());

        await handler.HandleAsync("x", Context("m1"));
        await handler.HandleAsync("x", Context("m1"));

        inner.Verify(h => h.HandleAsync("x", It.IsAny<IMessageContext>()), Times.Once);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_DifferentIds_HandlesEach()
    {
        var inner = new Mock<IMessageQueueHandler>();
        var handler = new IdempotentMessageQueueHandler("c", inner.Object, new InMemoryInboxStore());

        await handler.HandleAsync("x", Context("m1"));
        await handler.HandleAsync("x", Context("m2"));

        inner.Verify(h => h.HandleAsync("x", It.IsAny<IMessageContext>()), Times.Exactly(2));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_TwoConsumers_EachHandlesTheMessage()
    {
        var store = new InMemoryInboxStore();
        var a = new Mock<IMessageQueueHandler>();
        var b = new Mock<IMessageQueueHandler>();

        await new IdempotentMessageQueueHandler("a", a.Object, store).HandleAsync("x", Context("m1"));
        await new IdempotentMessageQueueHandler("b", b.Object, store).HandleAsync("x", Context("m1"));

        a.Verify(h => h.HandleAsync("x", It.IsAny<IMessageContext>()), Times.Once);
        b.Verify(h => h.HandleAsync("x", It.IsAny<IMessageContext>()), Times.Once);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_Failure_ReleasesSoTheRetryIsHandled()
    {
        var inner = new Mock<IMessageQueueHandler>();
        inner.SetupSequence(h => h.HandleAsync(It.IsAny<object>(), It.IsAny<IMessageContext>()))
            .ThrowsAsync(new InvalidOperationException("boom"))
            .Returns(Task.CompletedTask);
        var handler = new IdempotentMessageQueueHandler("c", inner.Object, new InMemoryInboxStore());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync("x", Context("m1")));
        await handler.HandleAsync("x", Context("m1"));

        inner.Verify(h => h.HandleAsync("x", It.IsAny<IMessageContext>()), Times.Exactly(2));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task HandleAsync_NoMessageId_AlwaysHandles()
    {
        var inner = new Mock<IMessageQueueHandler>();
        var handler = new IdempotentMessageQueueHandler("c", inner.Object, new InMemoryInboxStore());

        await handler.HandleAsync("x", Context(null));
        await handler.HandleAsync("x", Context(null));

        inner.Verify(h => h.HandleAsync("x", It.IsAny<IMessageContext>()), Times.Exactly(2));
    }
}
