using System;
using System.Collections.Generic;
using System.Linq;
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
public class ReceivePollTests
{
    private sealed record TestChannel(string Name, string Protocol, string Provider) : IChannel;

    private sealed class TestMessage(string id) : IMessage
    {
        public string MessageId { get; } = id;
        public string? CorrelationId => null;
        public DateTimeOffset Timestamp => DateTimeOffset.UnixEpoch;
        public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>(StringComparer.Ordinal);
    }

    private sealed class Handler(List<string> log, Func<IMessage, bool>? fails = null) : IMessageReceivedHandler
    {
        public Task HandleAsync(IMessage message, CancellationToken cancellationToken = default)
        {
            log.Add(message.MessageId);
            return fails?.Invoke(message) == true ? throw new InvalidOperationException("boom") : Task.CompletedTask;
        }
    }

    private sealed class Fixture
    {
        public Mock<IPollableChannelProvider> Provider { get; } = new();
        public List<string> Log { get; } = [];
        public ReceiveService Service { get; }

        public Fixture(Func<IMessage, bool>? handlerFails = null, bool channelExists = true, object? providerOverride = null, params string[] ids)
        {
            var channel = new TestChannel("inbox", "email", "imap");
            var repository = new Mock<IChannelRepository>();
            repository.Setup(r => r.GetByNameAsync("inbox", It.IsAny<CancellationToken>())).ReturnsAsync(channelExists ? channel : null);
            Provider.Setup(p => p.PollAsync(channel, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(ids.Select(i => new PolledMessage(new TestMessage(i), "r-" + i)).ToList());
            var registry = new Mock<IChannelRegistry>();
            registry.Setup(r => r.GetProvider("email", "imap")).Returns(providerOverride as IChannelProvider ?? Provider.Object);
            var time = new FakeTimeProvider();
            Service = new ReceiveService(
                repository.Object,
                registry.Object,
                Mock.Of<IWebhookProcessorFactory>(),
                new InMemoryDeliveryTracker(time),
                [new Handler(Log, handlerFails)],
                [],
                Options.Create(new SendReceiveOptions()),
                time,
                NullLogger<ReceiveService>.Instance);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_HandledMessages_AreAcknowledgedAfterTheHandlers()
    {
        var f = new Fixture(ids: ["a", "b"]);

        var result = await f.Service.PollAsync("inbox");

        CollectionAssert.AreEqual(new[] { "a", "b" }, result.Select(m => m.MessageId).ToArray());
        CollectionAssert.AreEqual(new[] { "a", "b" }, f.Log);
        f.Provider.Verify(p => p.AcknowledgeAsync(It.IsAny<IChannel>(), It.Is<IReadOnlyCollection<string>>(r => r.SequenceEqual(new[] { "r-a", "r-b" })), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_HandlerThrows_MessageIsNotAcknowledged()
    {
        var f = new Fixture(handlerFails: m => m.MessageId == "a", ids: ["a", "b"]);

        var result = await f.Service.PollAsync("inbox");

        CollectionAssert.AreEqual(new[] { "b" }, result.Select(m => m.MessageId).ToArray());
        f.Provider.Verify(p => p.AcknowledgeAsync(It.IsAny<IChannel>(), It.Is<IReadOnlyCollection<string>>(r => r.SequenceEqual(new[] { "r-b" })), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_AllHandlersFail_AcknowledgesNothing()
    {
        var f = new Fixture(handlerFails: _ => true, ids: ["a"]);

        var result = await f.Service.PollAsync("inbox");

        Assert.AreEqual(0, result.Count);
        f.Provider.Verify(p => p.AcknowledgeAsync(It.IsAny<IChannel>(), It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_ProviderReturnsTooMany_HandlesOnlyMaxMessages()
    {
        var f = new Fixture(ids: ["a", "b", "c"]);

        var result = await f.Service.PollAsync("inbox", 2);

        Assert.AreEqual(2, result.Count);
        f.Provider.Verify(p => p.PollAsync(It.IsAny<IChannel>(), 2, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_UnknownChannel_Throws()
        => await Assert.ThrowsAsync<ChannelNotFoundException>(() => new Fixture(channelExists: false).Service.PollAsync("inbox"));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_ProviderCannotPoll_Throws()
    {
        var plain = new Mock<IChannelProvider>().Object;

        await Assert.ThrowsAsync<NotSupportedException>(() => new Fixture(providerOverride: plain).Service.PollAsync("inbox"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task PollAsync_NonPositiveMax_Throws()
        => await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => new Fixture().Service.PollAsync("inbox", 0));
}
