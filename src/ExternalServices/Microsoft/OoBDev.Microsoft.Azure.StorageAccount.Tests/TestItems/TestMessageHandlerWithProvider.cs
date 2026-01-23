//using OoBDev.Azure.StorageAccount.Tests.MessageQueueing;
//using OoBDev.MessageQueueing;
//using OoBDev.MessageQueueing.Services;
//using OoBDev.TestUtilities;
//using Microsoft.Extensions.Logging;
//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System.Threading.Tasks;

//namespace OoBDev.Azure.StorageAccount.Tests.TestItems;

//public class TestMessageHandlerWithProvider(
//    ILogger<TestMessageHandlerWithProvider> logger,
//    TestContext testContext
//        ) : IMessageQueueHandler<AzureStorageQueueMessageSenderProviderTests>
//{
//    private readonly ILogger _logger = logger;

//    public Task HandleAsync(object message, IMessageContext context)
//    {
//        _logger.LogInformation("HandleAsync: {message}", message);
//        testContext.AddResult(message, fileName: $"TestMessageHandlerWithProvider-Message-{context.Config.Path}");
//        return Task.CompletedTask;
//    }
//}
