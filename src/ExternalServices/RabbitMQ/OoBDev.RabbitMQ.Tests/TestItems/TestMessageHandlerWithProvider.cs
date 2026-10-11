using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.RabbitMQ.Tests.MessageQueueing;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.RabbitMQ.Tests.TestItems;

public partial class TestMessageHandlerWithProvider(
    ILogger<TestMessageHandlerWithProvider> logger,
    TestContext testContext
        ) : IMessageQueueHandler<RabbitMQQueueMessageSenderProviderTests>
{
    private readonly ILogger _logger = logger;

    public Task HandleAsync(object message, IMessageContext context)
    {
        LogHandle(_logger, message);
        testContext.AddResult(message, fileName: $"TestMessageHandlerWithProvider-Message-{context.Config.Path}");
        return Task.CompletedTask;
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "HandleAsync: {message}")]
    private static partial void LogHandle(ILogger logger, object message);
}
