using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Hosting;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.Hosting;

[TestClass]
public class OutboxDispatcherHostTests
{
    private static Mock<IMessageQueueSender> Sender() => new();

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ExecuteAsync_PublishesPendingEntries()
    {
        var store = new InMemoryOutboxStore();
        await store.AddAsync(new OutboxEntry("a", "hello", DateTimeOffset.UnixEpoch));
        var sender = Sender();
        var sent = new TaskCompletionSource();
        sender.Setup(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .Callback(() => sent.TrySetResult())
            .ReturnsAsync("a");
        var host = new OutboxDispatcherHost(
            new OutboxDispatcher(store, sender.Object),
            Options.Create(new OutboxDispatcherHostOptions { PollInterval = TimeSpan.FromMilliseconds(10) }),
            NullLogger<OutboxDispatcherHost>.Instance);

        await host.StartAsync(CancellationToken.None);
        await sent.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        Assert.IsEmpty(await store.GetPendingAsync(10));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ExecuteAsync_SendFails_KeepsRunningAndRetries()
    {
        var store = new InMemoryOutboxStore();
        await store.AddAsync(new OutboxEntry("a", "hello", DateTimeOffset.UnixEpoch));
        var sender = Sender();
        var second = new TaskCompletionSource();
        sender.SetupSequence(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("down"))
            .Returns(() => { second.TrySetResult(); return Task.FromResult("a"); });
        var host = new OutboxDispatcherHost(
            new OutboxDispatcher(store, sender.Object),
            Options.Create(new OutboxDispatcherHostOptions { PollInterval = TimeSpan.FromMilliseconds(10) }),
            NullLogger<OutboxDispatcherHost>.Instance);

        await host.StartAsync(CancellationToken.None);
        await second.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await host.StopAsync(CancellationToken.None);

        sender.Verify(s => s.SendAsync(It.IsAny<object>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<int>()), Times.Exactly(2));
    }
}
