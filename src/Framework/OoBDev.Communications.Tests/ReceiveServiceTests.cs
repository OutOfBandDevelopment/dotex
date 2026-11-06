using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class ReceiveServiceTests
{
    private sealed record TestChannel(string Name, string Protocol, string Provider) : IChannel;

    private sealed class TestMessage(string id) : IMessage
    {
        public string MessageId { get; } = id;
        public string? CorrelationId => null;
        public DateTimeOffset Timestamp => DateTimeOffset.UnixEpoch;
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    private sealed class RecordingMessageHandler(List<string> log, string name, bool throws = false) : IMessageReceivedHandler
    {
        public Task HandleAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            log.Add(name + ":" + message.MessageId);
            return throws ? throw new InvalidOperationException("boom") : Task.CompletedTask;
        }
    }

    private sealed class RecordingStatusHandler(List<string> log) : IDeliveryStatusHandler
    {
        public Task HandleAsync(DeliveryStatusUpdate update, CancellationToken cancellationToken = default)
        {
            log.Add("status:" + update.NewStatus);
            return Task.CompletedTask;
        }
    }

    private sealed class Fixture
    {
        public FakeTimeProvider Time { get; } = new();
        public Mock<IWebhookProcessor> Processor { get; } = new();
        public InMemoryDeliveryTracker Tracker { get; }
        public List<string> Log { get; } = [];
        public ReceiveService Service { get; }

        public Fixture(bool channelExists = true, bool processorExists = true, bool validSignature = true, params ChannelEvent[] events)
        {
            Tracker = new InMemoryDeliveryTracker(Time);
            var repository = new Mock<IChannelRepository>();
            repository.Setup(r => r.GetByNameAsync("hook", It.IsAny<CancellationToken>()))
                .ReturnsAsync(channelExists ? new TestChannel("hook", "email", "fake") : null);
            Processor.Setup(p => p.Provider).Returns("fake");
            Processor.Setup(p => p.ValidateSignature(It.IsAny<IChannel>(), It.IsAny<WebhookRequest>())).Returns(validSignature);
            Processor.Setup(p => p.ProcessAsync(It.IsAny<IChannel>(), It.IsAny<WebhookRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(events);
            var factory = new Mock<IWebhookProcessorFactory>();
            factory.Setup(f => f.GetProcessor("fake")).Returns(processorExists ? Processor.Object : null);
            Service = new ReceiveService(
                repository.Object,
                factory.Object,
                Tracker,
                [new RecordingMessageHandler(Log, "first", throws: true), new RecordingMessageHandler(Log, "second")],
                [new RecordingStatusHandler(Log)],
                Options.Create(new SendReceiveOptions()),
                Time,
                NullLogger<ReceiveService>.Instance);
        }

        public WebhookRequest Request(TimeSpan? age = null) => new(
            new byte[] { 1 },
            new Dictionary<string, string>(StringComparer.Ordinal),
            new Dictionary<string, string>(StringComparer.Ordinal),
            Time.GetUtcNow() - (age ?? TimeSpan.Zero));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_UnknownChannel_ReturnsFailureWithoutThrowing()
    {
        var f = new Fixture(channelExists: false);

        var result = await f.Service.HandleWebhookAsync("hook", f.Request());

        Assert.IsFalse(result.Success);
        Assert.Contains("not found", result.ErrorMessage!, StringComparison.Ordinal);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_UnknownProcessor_ReturnsFailure()
    {
        var f = new Fixture(processorExists: false);

        var result = await f.Service.HandleWebhookAsync("hook", f.Request());

        Assert.IsFalse(result.Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_InvalidSignature_RunsNoHandler()
    {
        var f = new Fixture(validSignature: false, events: new MessageReceivedEvent("m1", new TestMessage("m1")));

        var result = await f.Service.HandleWebhookAsync("hook", f.Request());

        Assert.IsFalse(result.Success);
        Assert.AreEqual(0, f.Log.Count);
        f.Processor.Verify(p => p.ProcessAsync(It.IsAny<IChannel>(), It.IsAny<WebhookRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_StaleTimestamp_Rejected()
    {
        var f = new Fixture(events: new MessageReceivedEvent("m1", new TestMessage("m1")));

        var result = await f.Service.HandleWebhookAsync("hook", f.Request(TimeSpan.FromMinutes(6)));

        Assert.IsFalse(result.Success);
        Assert.AreEqual(0, f.Log.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_MessageAndStatusEvents_DispatchedAndCounted()
    {
        var f = new Fixture(events:
        [
            new MessageReceivedEvent("m1", new TestMessage("m1")),
            new DeliveryStatusEvent("m2", new DeliveryStatusUpdate("m2", DeliveryStatus.Delivered, null, DateTimeOffset.UnixEpoch)),
        ]);

        var result = await f.Service.HandleWebhookAsync("hook", f.Request());

        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.MessagesProcessed);
        CollectionAssert.AreEqual(new[] { "m1", "m2" }, new List<string>(result.MessageIds));
        CollectionAssert.AreEqual(new[] { "first:m1", "second:m1", "status:Delivered" }, f.Log);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_HandlerThrows_OthersStillRun()
    {
        var f = new Fixture(events: new MessageReceivedEvent("m1", new TestMessage("m1")));

        var result = await f.Service.HandleWebhookAsync("hook", f.Request());

        Assert.IsTrue(result.Success);
        CollectionAssert.Contains(f.Log, "second:m1");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleWebhookAsync_StatusEvent_MovesTrackerForwardOnly()
    {
        var f = new Fixture(events: new DeliveryStatusEvent("m1", new DeliveryStatusUpdate("m1", DeliveryStatus.Bounced, "no mailbox", DateTimeOffset.UnixEpoch)));
        await f.Tracker.RecordSentAsync("m1", "hook", new SendResult(true, "m1", null, "p", DateTimeOffset.UnixEpoch));

        await f.Service.HandleWebhookAsync("hook", f.Request());
        var late = await f.Tracker.UpdateStatusAsync(new DeliveryStatusUpdate("m1", DeliveryStatus.Sent, null, DateTimeOffset.UnixEpoch));

        Assert.IsFalse(late);
        var record = await f.Tracker.GetStatusAsync("m1");
        Assert.AreEqual(DeliveryStatus.Bounced, record?.Status);
        Assert.AreEqual("no mailbox", record?.StatusDetails);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task UpdateStatusAsync_UnknownMessage_IsIgnored()
    {
        var tracker = new InMemoryDeliveryTracker(new FakeTimeProvider());

        Assert.IsFalse(await tracker.UpdateStatusAsync(new DeliveryStatusUpdate("nope", DeliveryStatus.Delivered, null, DateTimeOffset.UnixEpoch)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void GetProcessor_ByProviderKey_IsCaseInsensitiveAndNullWhenUnknown()
    {
        var processor = new Mock<IWebhookProcessor>();
        processor.Setup(p => p.Provider).Returns("SendGrid");
        var factory = new WebhookProcessorFactory([processor.Object]);

        Assert.AreSame(processor.Object, factory.GetProcessor("sendgrid"));
        Assert.IsNull(factory.GetProcessor("twilio"));
    }
}
