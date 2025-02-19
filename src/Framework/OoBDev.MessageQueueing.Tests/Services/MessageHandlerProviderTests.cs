using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.Tests.TestItems;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.Services;

[TestClass]
public class MessageHandlerProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task HandleAsyncTest()
    {
        var messageId = "test message id";
        var payload = new TestMessage();

        var configBuilder = new ConfigurationBuilder().Build();
        var configSection = configBuilder.GetSection("TestSection");

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockSerializer = mockRepo.Create<IJsonSerializer>();
        var mockContextFactory = mockRepo.Create<IMessageContextFactory>();
        var mockMessage = mockRepo.Create<IQueueMessage>();
        var mockMessageQueueHandler = mockRepo.Create<IMessageQueueHandler>();
        var mockContext = mockRepo.Create<IMessageContext>();

        mockContextFactory.Setup(s => s.Create(GetType(), mockMessage.Object, configSection)).Returns(mockContext.Object);
        mockMessage.Setup(s => s.PayloadType).Returns((string?)null);
        mockMessage.Setup(s => s.Payload).Returns(payload);
        mockContext.SetupSet(s => s.SentId = messageId);
        mockMessageQueueHandler.Setup(s => s.HandleAsync(payload, mockContext.Object)).Returns(Task.CompletedTask);

        var provider = new MessageHandlerProvider(
            mockSerializer.Object,
            mockContextFactory.Object,
            TestLogger.CreateLogger<MessageHandlerProvider>()
            );

        var @internal = provider as IMessageHandlerProviderWrapped;

        @internal.SetConfig(configSection);
        @internal.SetChannelType(GetType());
        @internal.SetHandlers([mockMessageQueueHandler.Object]);

        await provider.HandleAsync(mockMessage.Object, messageId);

        mockRepo.VerifyAll();
    }
}
