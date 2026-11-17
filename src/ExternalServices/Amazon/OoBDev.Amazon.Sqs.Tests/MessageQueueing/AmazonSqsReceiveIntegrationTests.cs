using System;
using OoBDev.TestUtilities.Containers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.System;
using OoBDev.System.Text;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;

namespace OoBDev.Amazon.Sqs.Tests.MessageQueueing;

[TestClass]
[TestCategory(TestCategories.Integration)]
public sealed class AmazonSqsReceiveIntegrationTests
{
    public TestContext TestContext { get; set; } = null!;

    [IntegrationTestMethod("oobd-test-moto")]
    public async Task RunAsync_MessageSentToMoto_IsHandledAndDeleted()
    {
        // Stage
        var endpoint = TestInstanceGuard.Require(TestContext.GetRequiredProperty<string>("SQS_ENDPOINT"));
        var region = TestContext.GetRequiredProperty<string>("AWS_REGION");
        var accessKeyId = TestContext.GetRequiredProperty<string>("AWS_ACCESS_KEY_ID");
        var secretAccessKey = TestContext.GetRequiredProperty<string>("AWS_SECRET_ACCESS_KEY");
        var queueName = $"Test_{Guid.NewGuid():N}";
        var settings = new Dictionary<string, string?>
        {
            ["Q:QueueName"] = queueName,
            ["Q:Region"] = region,
            ["Q:AccessKeyId"] = accessKeyId,
            ["Q:SecretAccessKey"] = secretAccessKey,
            ["Q:ServiceUrl"] = endpoint,
            ["Q:WaitSeconds"] = "1",
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build().GetSection("Q");
        using var admin = new AmazonSQSClient(accessKeyId, secretAccessKey, new AmazonSQSConfig { ServiceURL = endpoint, AuthenticationRegion = region });
        var queueUrl = (await admin.CreateQueueAsync(queueName, TestContext.CancellationToken)).QueueUrl;

        var services = new ServiceCollection();
        services.TrySerializerExtensions(SerializerTypes.Json);
        var serializer = services.BuildServiceProvider().GetRequiredService<IJsonSerializer>();
        var provider = new AmazonSqsMessageProvider(serializer, new SqsClientFactory(), NullLogger<AmazonSqsMessageProvider>.Instance);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.CancellationToken);
        var handled = new TaskCompletionSource<IQueueMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = new Mock<IMessageHandlerProvider>();
        handler.SetupGet(h => h.Config).Returns(config);
        handler.Setup(h => h.HandleAsync(It.IsAny<IQueueMessage>(), It.IsAny<string>()))
            .Callback<IQueueMessage, string>((m, _) => handled.TrySetResult(m))
            .Returns(Task.CompletedTask);
        provider.SetHandlerProvider(handler.Object);

        try
        {
            await admin.SendMessageAsync(queueUrl, """{"ContentType":"application/json","PayloadType":"System.String","CorrelationId":"c-42","Payload":"hello","Properties":{}}""", TestContext.CancellationToken);

            // Test
            var run = provider.RunAsync(cts.Token);
            var message = await handled.Task.WaitAsync(TimeSpan.FromSeconds(30), TestContext.CancellationToken);
            await cts.CancelAsync();
            await run;

            // Assert
            Assert.AreEqual("c-42", message.CorrelationId);
            var remaining = await admin.ReceiveMessageAsync(new ReceiveMessageRequest { QueueUrl = queueUrl, WaitTimeSeconds = 1, VisibilityTimeout = 0 }, TestContext.CancellationToken);
            Assert.IsEmpty(remaining.Messages ?? []);
        }
        finally
        {
            await admin.DeleteQueueAsync(queueUrl, CancellationToken.None);
        }
    }
}
