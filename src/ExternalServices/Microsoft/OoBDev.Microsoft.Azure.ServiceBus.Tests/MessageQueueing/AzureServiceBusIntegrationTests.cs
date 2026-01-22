using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.MessageQueueing.Services;
using OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.Tests.MessageQueueing;

/// <summary>
/// Integration tests for Azure Service Bus message provider.
/// These tests require either the Azure Service Bus Emulator or real Azure Service Bus credentials.
/// </summary>
[TestClass]
public class AzureServiceBusIntegrationTests
{
    public required TestContext TestContext { get; set; }

    /// <summary>
    /// Lists all available Service Bus queues.
    /// Note: The emulator doesn't support management operations, so this requires real Azure.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task ListQueues_WithAzureServiceBus_ReturnsQueueNames()
    {
        // Arrange
        var connectionString = TestContext.GetProperty<string>("SERVICEBUS_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("localhost"))
        {
            Assert.Inconclusive("This test requires real Azure Service Bus (emulator doesn't support management API)");
            return;
        }

        var adminClient = new ServiceBusAdministrationClient(connectionString);

        // Act
        var queues = new List<string>();
        await foreach (var queue in adminClient.GetQueuesAsync())
        {
            queues.Add(queue.Name);
        }

        // Assert
        Assert.IsNotNull(queues);
        TestContext.WriteLine($"Found {queues.Count} queue(s):");
        foreach (var queueName in queues)
        {
            TestContext.WriteLine($"  - {queueName}");
        }
    }

    /// <summary>
    /// Creates a new test queue.
    /// Note: The emulator doesn't support management operations, so this requires real Azure.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task CreateQueue_WithAzureServiceBus_CreatesSuccessfully()
    {
        // Arrange
        var connectionString = TestContext.GetProperty<string>("SERVICEBUS_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("localhost"))
        {
            Assert.Inconclusive("This test requires real Azure Service Bus (emulator doesn't support management API)");
            return;
        }

        var adminClient = new ServiceBusAdministrationClient(connectionString);
        var queueName = $"test-queue-{Guid.NewGuid():N}";

        try
        {
            // Act
            var queueOptions = new CreateQueueOptions(queueName)
            {
                MaxDeliveryCount = 10,
                DefaultMessageTimeToLive = TimeSpan.FromMinutes(5)
            };

            var createdQueue = await adminClient.CreateQueueAsync(queueOptions);

            // Assert
            Assert.IsNotNull(createdQueue);
            Assert.AreEqual(queueName, createdQueue.Value.Name);
            TestContext.WriteLine($"Created queue: {createdQueue.Value.Name}");

            // Verify queue exists
            var queueExists = await adminClient.QueueExistsAsync(queueName);
            Assert.IsTrue(queueExists.Value);
        }
        finally
        {
            // Cleanup
            try
            {
                await adminClient.DeleteQueueAsync(queueName);
                TestContext.WriteLine($"Deleted test queue: {queueName}");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Sends a test message to a queue using the AzureServiceBusMessageProvider.
    /// Works with both emulator and real Azure Service Bus.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToServiceBusQueue_SendsSuccessfully()
    {
        // Arrange
        var connectionString = TestContext.GetRequiredProperty<string>("SERVICEBUS_CONNECTION_STRING");
        var queueName = TestContext.GetRequiredProperty<string>("SERVICEBUS_TEST_QUEUE");

        // Configure message provider
        var configBuilder = new ConfigurationBuilder();
        var configData = new Dictionary<string, string?>
        {
            { "MessageQueuing:TestQueue:ConnectionString", connectionString },
            { "MessageQueuing:TestQueue:QueueName", queueName }
        };
        configBuilder.AddInMemoryCollection(configData);
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAzureServiceBusServices();
        services.TryAddJsonSerializer();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act
        var context = contextFactory.Create("TestQueue", typeof(TestMessage).FullName);
        context.Headers["TestHeader"] = "TestValue";
        context.Headers["Priority"] = "High";
        context.Headers["Environment"] = "DevLocal";
        context.CorrelationId = Guid.NewGuid().ToString();

        var testMessage = new TestMessage
        {
            Id = 789,
            Content = $"Test message sent at {DateTime.UtcNow:O}",
            Timestamp = DateTime.UtcNow
        };

        var correlationId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(correlationId);
        Assert.IsFalse(string.IsNullOrEmpty(correlationId));
        TestContext.WriteLine($"Message sent with correlation ID: {correlationId}");

        // Verify message was sent by receiving it
        var client = new ServiceBusClient(connectionString);
        var receiver = client.CreateReceiver(queueName);

        try
        {
            var receivedMessage = await receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));

            if (receivedMessage != null)
            {
                TestContext.WriteLine($"Received message with correlation ID: {receivedMessage.CorrelationId}");
                Assert.AreEqual(correlationId, receivedMessage.CorrelationId);
                Assert.IsTrue(receivedMessage.ApplicationProperties.ContainsKey("TestHeader"));
                Assert.AreEqual("TestValue", receivedMessage.ApplicationProperties["TestHeader"].ToString());

                // Complete the message
                await receiver.CompleteMessageAsync(receivedMessage);
            }
            else
            {
                Assert.Fail("No message received from queue within timeout period");
            }
        }
        finally
        {
            await receiver.DisposeAsync();
            await client.DisposeAsync();
        }
    }

    /// <summary>
    /// Sends a test message to a topic using the AzureServiceBusMessageProvider.
    /// Works with both emulator and real Azure Service Bus.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToServiceBusTopic_SendsSuccessfully()
    {
        // Arrange
        var connectionString = TestContext.GetRequiredProperty<string>("SERVICEBUS_CONNECTION_STRING");
        var topicName = TestContext.GetRequiredProperty<string>("SERVICEBUS_TEST_TOPIC");

        // Configure message provider
        var configBuilder = new ConfigurationBuilder();
        var configData = new Dictionary<string, string?>
        {
            { "MessageQueuing:TestTopic:ConnectionString", connectionString },
            { "MessageQueuing:TestTopic:TopicName", topicName }
        };
        configBuilder.AddInMemoryCollection(configData);
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAzureServiceBusServices();
        services.TryAddJsonSerializer();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act
        var context = contextFactory.Create("TestTopic", typeof(TestMessage).FullName);
        context.Headers["MessageType"] = "TopicTest";
        context.CorrelationId = Guid.NewGuid().ToString();

        var testMessage = new TestMessage
        {
            Id = 101,
            Content = $"Topic test message sent at {DateTime.UtcNow:O}",
            Timestamp = DateTime.UtcNow
        };

        var correlationId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(correlationId);
        TestContext.WriteLine($"Message sent to topic with correlation ID: {correlationId}");
    }

    /// <summary>
    /// Tests sending to a session-enabled queue.
    /// Note: Requires a session-enabled queue to be created first.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task SendAsync_ToSessionQueue_WithSessionId_SendsSuccessfully()
    {
        // Arrange
        var connectionString = TestContext.GetProperty<string>("SERVICEBUS_CONNECTION_STRING");

        if (string.IsNullOrEmpty(connectionString) || connectionString.Contains("localhost"))
        {
            Assert.Inconclusive("Session-enabled queues require real Azure Service Bus");
            return;
        }

        var queueName = TestContext.GetProperty<string>("SERVICEBUS_SESSION_QUEUE");

        if (string.IsNullOrEmpty(queueName))
        {
            Assert.Inconclusive("SERVICEBUS_SESSION_QUEUE not configured in test settings");
            return;
        }

        var sessionId = $"session-{Guid.NewGuid():N}";

        // Configure message provider
        var configBuilder = new ConfigurationBuilder();
        var configData = new Dictionary<string, string?>
        {
            { "MessageQueuing:SessionQueue:ConnectionString", connectionString },
            { "MessageQueuing:SessionQueue:QueueName", queueName },
            { "MessageQueuing:SessionQueue:SessionId", sessionId }
        };
        configBuilder.AddInMemoryCollection(configData);
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAzureServiceBusServices();
        services.TryAddJsonSerializer();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IMessageContextFactory, MessageContextFactory>();

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<IMessageSenderProvider>();
        var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

        // Act
        var context = contextFactory.Create("SessionQueue", typeof(TestMessage).FullName);
        var testMessage = new TestMessage
        {
            Id = 202,
            Content = "Session test message",
            Timestamp = DateTime.UtcNow
        };

        var correlationId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(correlationId);
        TestContext.WriteLine($"Session message sent with correlation ID: {correlationId}");
        TestContext.WriteLine($"Session ID: {sessionId}");
    }

    private record TestMessage
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
    }
}
