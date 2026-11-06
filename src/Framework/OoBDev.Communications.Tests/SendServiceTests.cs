using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Communications.Abstractions;
using OoBDev.TestUtilities;

namespace OoBDev.Communications.Tests;

[TestClass]
public class SendServiceTests
{
    private sealed record TestChannel(string Name, string Protocol, string Provider) : IChannel;

    private sealed class TestMessage(string id, string? correlation = null) : IMessage
    {
        public string MessageId { get; } = id;
        public string? CorrelationId { get; } = correlation;
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.UnixEpoch;
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    private sealed class Fixture
    {
        public FakeTimeProvider Time { get; } = new();
        public Mock<IChannelProvider> Provider { get; } = new();
        public Mock<IChannelRepository> Repository { get; } = new();
        public Mock<IChannelRegistry> Registry { get; } = new();
        public InMemoryDeliveryTracker Tracker { get; }
        public SendReceiveOptions Settings { get; } = new();
        public SendService Service { get; }

        public Fixture(bool channelExists = true, bool providerExists = true)
        {
            Tracker = new InMemoryDeliveryTracker(Time);
            var channel = new TestChannel("mail", "email", "fake");
            Repository.Setup(r => r.GetByNameAsync("mail", It.IsAny<CancellationToken>())).ReturnsAsync(channelExists ? channel : null);
            Registry.Setup(r => r.GetProvider("email", "fake")).Returns(providerExists ? Provider.Object : null);
            Service = new SendService(Repository.Object, Registry.Object, Tracker, Options.Create(Settings), Time);
        }

        public void Returns(params SendResult[] results)
        {
            var queue = new Queue<SendResult>(results);
            Provider.Setup(p => p.SendAsync(It.IsAny<IChannel>(), It.IsAny<IMessage>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => queue.Count > 1 ? queue.Dequeue() : queue.Peek());
        }

        public async Task<SendResult> SendAdvancing(string id, SendOptions? options = null)
        {
            var task = Service.SendAsync("mail", new TestMessage(id), options);
            while (!task.IsCompleted)
            {
                Time.Advance(TimeSpan.FromSeconds(10));
                await Task.Yield();
            }

            return await task;
        }
    }

    private static SendResult Ok(string id = "m1") => new(true, id, null, "p-" + id, DateTimeOffset.UnixEpoch);

    private static SendResult Fail(SendFailureKind kind, TimeSpan? retryAfter = null, string id = "m1")
        => new(false, id, null, null, DateTimeOffset.UnixEpoch, new SendFailure(kind, "boom", 500, retryAfter));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_ProviderSucceeds_ReturnsResultAndRecordsIt()
    {
        // Stage
        var f = new Fixture();
        f.Returns(Ok());

        // Test
        var result = await f.Service.SendAsync("mail", new TestMessage("m1"));

        // Assert
        Assert.IsTrue(result.Success);
        var status = await f.Tracker.GetStatusAsync("m1");
        Assert.AreEqual(DeliveryStatus.Sent, status?.Status);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_UnknownChannel_Throws()
    {
        var f = new Fixture(channelExists: false);

        var ex = await Assert.ThrowsExactlyAsync<ChannelNotFoundException>(() => f.Service.SendAsync("mail", new TestMessage("m1")));

        Assert.AreEqual("mail", ex.ChannelName);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_UnknownProvider_Throws()
    {
        var f = new Fixture(providerExists: false);

        var ex = await Assert.ThrowsExactlyAsync<ProviderNotFoundException>(() => f.Service.SendAsync("mail", new TestMessage("m1")));

        Assert.AreEqual("fake", ex.Provider);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_SameMessageTwice_SendsOnce()
    {
        var f = new Fixture();
        f.Returns(Ok());

        await f.Service.SendAsync("mail", new TestMessage("m1"));
        var second = await f.Service.SendAsync("mail", new TestMessage("m1"));

        Assert.IsTrue(second.Success);
        f.Provider.Verify(p => p.SendAsync(It.IsAny<IChannel>(), It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_TransientThenSuccess_Retries()
    {
        var f = new Fixture();
        f.Returns(Fail(SendFailureKind.Transient), Fail(SendFailureKind.RateLimited), Ok());

        var result = await f.SendAdvancing("m1");

        Assert.IsTrue(result.Success);
        f.Provider.Verify(p => p.SendAsync(It.IsAny<IChannel>(), It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow(SendFailureKind.Permanent)]
    [DataRow(SendFailureKind.Rejected)]
    public async Task SendAsync_NonRetryableFailure_DoesNotRetry(SendFailureKind kind)
    {
        var f = new Fixture();
        f.Returns(Fail(kind));

        var result = await f.Service.SendAsync("mail", new TestMessage("m1"));

        Assert.IsFalse(result.Success);
        f.Provider.Verify(p => p.SendAsync(It.IsAny<IChannel>(), It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.AreEqual(DeliveryStatus.Failed, (await f.Tracker.GetStatusAsync("m1"))?.Status);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_AlwaysTransient_StopsAfterRetryCount()
    {
        var f = new Fixture();
        f.Returns(Fail(SendFailureKind.Transient));

        var result = await f.SendAdvancing("m1", new SendOptions { RetryCount = 2 });

        Assert.IsFalse(result.Success);
        f.Provider.Verify(p => p.SendAsync(It.IsAny<IChannel>(), It.IsAny<IMessage>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_FailedEarlier_IsSentAgain()
    {
        var f = new Fixture();
        f.Returns(Fail(SendFailureKind.Permanent));
        await f.Service.SendAsync("mail", new TestMessage("m1"));
        f.Returns(Ok());

        var result = await f.Service.SendAsync("mail", new TestMessage("m1"));

        Assert.IsTrue(result.Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_Backoff_DoublesDelayAndHonoursRetryAfter()
    {
        var f = new Fixture();
        f.Returns(Fail(SendFailureKind.Transient), Fail(SendFailureKind.RateLimited, TimeSpan.FromSeconds(20)), Ok());
        var task = f.Service.SendAsync("mail", new TestMessage("m1"));

        await Task.Yield();
        f.Time.Advance(TimeSpan.FromMilliseconds(999));
        await Task.Yield();
        Assert.IsFalse(task.IsCompleted, "first delay is one second");
        f.Time.Advance(TimeSpan.FromMilliseconds(1));
        await Task.Yield();
        f.Time.Advance(TimeSpan.FromSeconds(19));
        await Task.Yield();
        Assert.IsFalse(task.IsCompleted, "RetryAfter of 20 s beats the 2 s backoff");
        f.Time.Advance(TimeSpan.FromSeconds(1));

        Assert.IsTrue((await task).Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_DelayAboveMax_IsCapped()
    {
        var f = new Fixture();
        f.Settings.RetryDelay = TimeSpan.FromMinutes(10);
        f.Settings.MaxRetryDelay = TimeSpan.FromSeconds(5);
        f.Returns(Fail(SendFailureKind.Transient), Ok());
        var task = f.Service.SendAsync("mail", new TestMessage("m1"));

        await Task.Yield();
        f.Time.Advance(TimeSpan.FromSeconds(5));

        Assert.IsTrue((await task).Success);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_Cancelled_Throws()
    {
        var f = new Fixture();
        f.Returns(Fail(SendFailureKind.Transient));
        using var cts = new CancellationTokenSource();
        var task = f.Service.SendAsync("mail", new TestMessage("m1"), null, cts.Token);
        await Task.Yield();

        await cts.CancelAsync();

        await Assert.ThrowsAsync<OperationCanceledException>(() => task);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendAsync_OptionsCorrelationId_FillsMissingOne()
    {
        var f = new Fixture();
        f.Returns(Ok());

        var result = await f.Service.SendAsync("mail", new TestMessage("m1"), new SendOptions { CorrelationId = "c1" });

        Assert.AreEqual("c1", result.CorrelationId);
        Assert.AreEqual(1, (await f.Tracker.GetByCorrelationIdAsync("c1")).Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task SendToMultipleChannelsAsync_ReturnsResultsInOrder()
    {
        var f = new Fixture();
        f.Returns(Ok());

        var results = await f.Service.SendToMultipleChannelsAsync(["mail", "mail"], new TestMessage("m1"));

        Assert.AreEqual(2, results.Count);
        Assert.IsTrue(results.All(r => r.Success));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task GetStatusAsync_Unknown_ReturnsNull()
    {
        var tracker = new InMemoryDeliveryTracker(new FakeTimeProvider());

        Assert.IsNull(await tracker.GetStatusAsync("nope"));
        Assert.IsNull(await tracker.GetSentResultAsync("nope", "mail"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryAddSendReceive_RegistersServicesOnce()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Mock.Of<IChannelRepository>());
        services.AddSingleton(Mock.Of<IChannelRegistry>());
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.TryAddSendReceive(o => o.RetryCount = 7).TryAddSendReceive();
        using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType<SendService>(provider.GetRequiredService<ISendService>());
        Assert.IsInstanceOfType<InMemoryDeliveryTracker>(provider.GetRequiredService<IDeliveryTracker>());
        Assert.AreEqual(7, provider.GetRequiredService<IOptions<SendReceiveOptions>>().Value.RetryCount);
        Assert.AreEqual(1, services.Count(s => s.ServiceType == typeof(ISendService)));
    }
}
