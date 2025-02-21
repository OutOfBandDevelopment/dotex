using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Azure.StorageAccount.Tests.TestItems;

public class TestMessageHandler(
    ILogger<TestMessageHandler> logger,
    TestContext testContext
        ) : IMessageQueueHandler
{
    private readonly ILogger _logger = logger;

    public Task HandleAsync(object message, IMessageContext context)
    {
        _logger.LogInformation("HandleAsync: {message}", message);
        testContext.AddResult(message, fileName: $"TestMessageHandler-Message-{context.Config.Path}");
        return Task.CompletedTask;
    }
}
