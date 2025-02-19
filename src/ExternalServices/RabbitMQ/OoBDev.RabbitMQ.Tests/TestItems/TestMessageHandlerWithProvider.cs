using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.RabbitMQ.Tests.MessageQueueing;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.RabbitMQ.Tests.TestItems;

public class TestMessageHandlerWithProvider(
    ILogger<TestMessageHandlerWithProvider> logger,
    TestContext testContext
        ) : IMessageQueueHandler<RabbitMQQueueMessageSenderProviderTests>
{
    private readonly ILogger _logger = logger;

    public Task HandleAsync(object message, IMessageContext context)
    {
        _logger.LogInformation("HandleAsync: {message}", message);
        testContext.AddResult(message, fileName: $"TestMessageHandlerWithProvider-Message-{context.Config.Path}");
        return Task.CompletedTask;
    }
}
