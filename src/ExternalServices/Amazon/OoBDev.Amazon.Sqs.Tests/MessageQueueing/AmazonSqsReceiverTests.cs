using System;
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
[TestCategory(TestCategories.Unit)]
public sealed class AmazonSqsReceiverTests
{
    private const string _queueUrl = "http://sqs.test/queue";

    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    public async Task RunAsync_HandlerSucceeds_DeletesTheMessage()
    {
        // Stage
        using var cts = new CancellationTokenSource();
        var (provider, client, handler) = Create(cts, Body("hello"));
        var handled = new List<string>();
        handler.Setup(h => h.HandleAsync(It.IsAny<IQueueMessage>(), It.IsAny<string>()))
            .Callback<IQueueMessage, string>((_, id) => handled.Add(id))
            .Returns(Task.CompletedTask);

        // Test
        await provider.RunAsync(cts.Token);

        // Assert
        CollectionAssert.AreEqual(new[] { "m-1" }, handled);
        client.Verify(c => c.DeleteMessageAsync(_queueUrl, "r-1", It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task RunAsync_HandlerThrows_LeavesTheMessageOnTheQueue()
    {
        // Stage
        using var cts = new CancellationTokenSource();
        var (provider, client, handler) = Create(cts, Body("boom"));
        handler.Setup(h => h.HandleAsync(It.IsAny<IQueueMessage>(), It.IsAny<string>())).ThrowsAsync(new InvalidOperationException("no"));

        // Test
        await provider.RunAsync(cts.Token);

        // Assert
        client.Verify(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task RunAsync_BodyIsNotAWrappedMessage_StaysOnTheQueue()
    {
        // Stage
        using var cts = new CancellationTokenSource();
        var (provider, client, handler) = Create(cts, "not json");

        // Test
        await provider.RunAsync(cts.Token);

        // Assert
        handler.Verify(h => h.HandleAsync(It.IsAny<IQueueMessage>(), It.IsAny<string>()), Times.Never);
        client.Verify(c => c.DeleteMessageAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod]
    public async Task RunAsync_ConfigWaitAndMax_AreClampedIntoTheRequest()
    {
        // Stage
        using var cts = new CancellationTokenSource();
        var (provider, client, _) = Create(cts, null, ("WaitSeconds", "99"), ("MaxMessages", "0"));

        // Test
        await provider.RunAsync(cts.Token);

        // Assert
        client.Verify(c => c.ReceiveMessageAsync(
            It.Is<ReceiveMessageRequest>(r => r.QueueUrl == _queueUrl && r.WaitTimeSeconds == 20 && r.MaxNumberOfMessages == 1),
            It.IsAny<CancellationToken>()), Times.AtLeastOnce);
    }

    [TestMethod]
    public async Task RunAsync_WithoutHandlerProvider_Throws()
    {
        var provider = new AmazonSqsMessageProvider(Serializer(), Mock.Of<ISqsClientFactory>(), NullLogger<AmazonSqsMessageProvider>.Instance);

        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => provider.RunAsync(TestContext.CancellationToken));
    }

    [TestMethod]
    public void TryAddAmazonSqsServices_RegistersTheReceiverUnderTheProviderKey()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.TrySerializerExtensions(SerializerTypes.Json);
        services.AddSingleton(Mock.Of<ISqsClientFactory>());
        services.TryAddAmazonSqsServices();

        using var provider = services.BuildServiceProvider();

        Assert.IsNotNull(provider.GetKeyedService<IMessageReceiverProvider>(AwsSqsGlobals.MessageProviderKey));
    }

    private static string Body(string text) =>
        global::System.Text.Json.JsonSerializer.Serialize(new { ContentType = "application/json", PayloadType = typeof(string).AssemblyQualifiedName, CorrelationId = "c-1", Payload = text, Properties = new Dictionary<string, object?>() });

    private static IJsonSerializer Serializer()
    {
        var services = new ServiceCollection();
        services.TrySerializerExtensions(SerializerTypes.Json);
        return services.BuildServiceProvider().GetRequiredService<IJsonSerializer>();
    }

    private static (AmazonSqsMessageProvider Provider, Mock<IAmazonSQS> Client, Mock<IMessageHandlerProvider> Handler) Create(
        CancellationTokenSource cts, string? body, params (string Key, string Value)[] settings)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(Array.ConvertAll(settings, s => KeyValuePair.Create<string, string?>($"Q:{s.Key}", s.Value)))
            .Build().GetSection("Q");

        var client = new Mock<IAmazonSQS>();
        var calls = 0;
        client.Setup(c => c.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                if (calls++ > 0)
                {
                    cts.Cancel();
                    return new ReceiveMessageResponse { Messages = [] };
                }

                return new ReceiveMessageResponse { Messages = body is null ? [] : [new Message { MessageId = "m-1", ReceiptHandle = "r-1", Body = body }] };
            });

        var factory = new Mock<ISqsClientFactory>();
        factory.Setup(f => f.CreateAsync(It.IsAny<IConfigurationSection>())).ReturnsAsync((client.Object, _queueUrl));

        var handler = new Mock<IMessageHandlerProvider>();
        handler.SetupGet(h => h.Config).Returns(config);

        var provider = new AmazonSqsMessageProvider(Serializer(), factory.Object, NullLogger<AmazonSqsMessageProvider>.Instance);
        provider.SetHandlerProvider(handler.Object);
        return (provider, client, handler);
    }
}
