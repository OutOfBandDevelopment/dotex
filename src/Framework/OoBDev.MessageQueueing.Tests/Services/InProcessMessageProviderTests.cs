using OoBDev.System.Providers;
using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.Tests.TestItems;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Options;
using Moq;
using System;
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
            TestLogger.CreateLogger<InProcessMessageProvider>(),
            Options.Create(new InProcessMessageProviderOptions { PollInterval = TimeSpan.FromMilliseconds(10) })
            );

        provider.SetHandlerProvider(mockHandler.Object);

        var correlationId = await provider.SendAsync(message, context);
        Assert.IsNotNull(correlationId);
        context.CorrelationId = correlationId;

        await provider.RunAsync(cancellationTokenSource.Token);

        mockRepo.VerifyAll();
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_GuidProviderRegistered_UsesItsIdsAsCorrelationId()
    {
        TestContext.WriteLine("Stage: a guid provider that returns a known id");
        var known = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var guids = new Mock<IGuidProvider>(MockBehavior.Strict);
        guids.Setup(g => g.NewGuid()).Returns(known);
        var provider = new InProcessMessageProvider(
            TestLogger.CreateLogger<InProcessMessageProvider>(),
            Options.Create(new InProcessMessageProviderOptions()),
            guids.Object);
        var context = new MessageContext();

        var id = await provider.SendAsync(new object(), context);

        TestContext.WriteLine($"correlation id {id}");
        Assert.AreEqual(known.ToString(), id);
        guids.VerifyAll();
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SendAsync_NoGuidProvider_GeneratesAGuidCorrelationId()
    {
        var provider = new InProcessMessageProvider(
            TestLogger.CreateLogger<InProcessMessageProvider>(),
            Options.Create(new InProcessMessageProviderOptions()));

        var id = await provider.SendAsync(new object(), new MessageContext());

        Assert.IsTrue(Guid.TryParse(id, out _), id);
    }
}
