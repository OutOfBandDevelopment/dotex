using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.Tests;
using OoBDev.RabbitMQ.MessageQueueing;
using OoBDev.RabbitMQ.Tests.TestItems;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace OoBDev.RabbitMQ.Tests.MessageQueueing;

#warning is this needed

[TestClass]
[MessageQueue(QueueConfig)]
public class RabbitMQQueueMessageSenderProviderTests
{
    public const string QueueConfig = "test-config";

    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsyncTest_ByFullType()
    {
        var rabbitMQHost = TestContext.GetRequiredProperty<string>("RABBITMQ_HOST");
        var rabbitMQPort = TestContext.GetPropertyOrDefault("RABBITMQ_PORT", 5672);

        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {$"MessageQueue:{QueueConfig}:Provider", typeof(RabbitMQQueueMessageProvider).AssemblyQualifiedName },

            {$"MessageQueue:{QueueConfig}:Config:HostName", rabbitMQHost },
            {$"MessageQueue:{QueueConfig}:Config:Port", rabbitMQPort.ToString() },
            {$"MessageQueue:{QueueConfig}:Config:QueueName", "test-queue" },
            {$"MessageQueue:{QueueConfig}:Config:RequestedConnectionTimeout", "2000" }, // 2 second timeout
        });

        var config = configBuilder.Build();

        var service = MessageSenderTests.GetServiceProvider(TestContext, config, services => services.TryAddRabbitMQServices());

        // ---------------

        var sender = service.GetRequiredService<IMessageQueueSender<RabbitMQQueueMessageSenderProviderTests>>();
        var correlationId = await sender.SendAsync(new
        {
            hello = "world",
        });

        TestContext.Write($"correlationId: {correlationId}");
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsyncTest_ByKeyed()
    {
        var rabbitMQHost = TestContext.GetRequiredProperty<string>("RABBITMQ_HOST");
        var rabbitMQPort = TestContext.GetPropertyOrDefault("RABBITMQ_PORT", 5672);

        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {$"MessageQueue:{QueueConfig}:Provider", RabbitMQGlobals.MessageProviderKey },

            {$"MessageQueue:{QueueConfig}:Config:HostName", rabbitMQHost },
            {$"MessageQueue:{QueueConfig}:Config:Port", rabbitMQPort.ToString() },
            {$"MessageQueue:{QueueConfig}:Config:QueueName", "test-queue" },
            {$"MessageQueue:{QueueConfig}:Config:RequestedConnectionTimeout", "2000" }, // 2 second timeout

        });

        var config = configBuilder.Build();

        var service = MessageSenderTests.GetServiceProvider(TestContext, config, services => services.TryAddRabbitMQServices());

        // ---------------

        var sender = service.GetRequiredService<IMessageQueueSender<RabbitMQQueueMessageSenderProviderTests>>();
        var correlationId = await sender.SendAsync(new
        {
            hello = "world",
        });

        TestContext.Write($"correlationId: {correlationId}");
    }

    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task FindProviderTests()
    {
        var rabbitMQHost = TestContext.GetRequiredProperty<string>("RABBITMQ_HOST");
        var rabbitMQPort = TestContext.GetPropertyOrDefault("RABBITMQ_PORT", 5672);

        var configBuilder = new ConfigurationBuilder();

        configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
        {
            {$"MessageQueue:{QueueConfig}:Provider", RabbitMQGlobals.MessageProviderKey },

            {$"MessageQueue:{QueueConfig}:Config:HostName", rabbitMQHost },
            {$"MessageQueue:{QueueConfig}:Config:Port", rabbitMQPort.ToString() },
            {$"MessageQueue:{QueueConfig}:Config:QueueName", "test-queue" },
            {$"MessageQueue:{QueueConfig}:Config:RequestedConnectionTimeout", "2000" }, // 2 second timeout

            {$"MessageQueue:Default:Provider", InProcessMessageProvider.MessageProviderKey },
        });

        var config = configBuilder.Build();

        var service = MessageSenderTests.GetServiceProvider(TestContext, config, services =>
        {
            services.TryAddRabbitMQServices();

            services.AddTransient<IMessageQueueHandler, TestMessageHandler>();
            services.AddTransient<IMessageQueueHandler, TestMessageHandlerWithProvider>();
            services.AddTransient<IMessageQueueHandler, TestMessageHandlerWithProviderAndMessage>();
        });

        // ---------------

        var configurationSection = config.GetSection($"MessageQueue:{QueueConfig}:Config");

        var sender = service.GetRequiredService<IMessageQueueSender<RabbitMQQueueMessageSenderProviderTests>>();
        var sender2 = service.GetRequiredService<IMessageQueueSender>();

        var factory = service.GetRequiredService<IMessageReceiverProviderFactory>();
        var providers = factory.Create().ToArray();

        var tasks = new List<Task>();
        var tokenSource = new CancellationTokenSource();
        var token = tokenSource.Token;

        foreach (var provider in providers)
        {
            tasks.Add(Task.Run(() => provider.RunAsync(token)));
        }

        tasks.Add(Task.Run(async () =>
        {
            for (var x = 0; x < 10; x++)
            {
                for (var y = 0; y < x; y++)
                {
                    object message = y % 2 == 0 ? new TestQueueMessage() : new { Hello = "There" };
                    Debug.WriteLine($"----------: Send {DateTimeOffset.Now} :---------- [{message}]");
                    var id = await sender.SendAsync(message);
                    var id2 = await sender2.SendAsync(message);
                    Debug.WriteLine($"----------: Sent {DateTimeOffset.Now} :---------- [{id}]"); ///{id2}
                }

                Debug.WriteLine($"----------: Waiting {DateTimeOffset.Now} :---------- ");

                await Task.Delay(1000);
            }

            tokenSource.Cancel();
        }));

        await Task.WhenAll(tasks);
    }
}
