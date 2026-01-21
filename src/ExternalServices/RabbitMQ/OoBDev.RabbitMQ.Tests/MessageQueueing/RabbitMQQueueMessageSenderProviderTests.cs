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
}
