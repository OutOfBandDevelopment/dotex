using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.Tests.TestItems;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.MessageQueueing.Tests.Services;

[TestClass]
public class InProcessMessageProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task InOutTest()
    {
        var message = new TestMessage();
        var context = new MessageContext();
        var cancellationTokenSource = new CancellationTokenSource();

        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockHandler = mockRepo.Create<IMessageHandlerProvider>();
        mockHandler.Setup(s => s.HandleAsync(It.IsAny<IQueueMessage>(), It.IsAny<string>()))
                   .Returns(Task.CompletedTask)
                   .Callback(() =>
                   {
                       TestContext.WriteLine("Handler Called!");
                       cancellationTokenSource.Cancel();
                   });

        var provider = new InProcessMessageProvider(
            TestLogger.CreateLogger<InProcessMessageProvider>()
            );

        provider.SetHandlerProvider(mockHandler.Object);

        var correlationId = await provider.SendAsync(message, context);
        Assert.IsNotNull(correlationId);
        context.CorrelationId = correlationId;

        await provider.RunAsync(cancellationTokenSource.Token);

        mockRepo.VerifyAll();
    }

}
