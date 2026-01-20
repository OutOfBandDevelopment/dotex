using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json.Serialization;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.Tests.MessageQueueing;

/// <summary>
/// Integration tests for AWS SQS message provider.
/// These tests require either LocalStack or real AWS credentials.
/// </summary>
[TestClass]
public class AmazonSqsIntegrationTests
{
    public required TestContext TestContext { get; set; }

    /// <summary>
    /// Lists all available SQS queues.
    /// Useful for verifying LocalStack setup or AWS credentials.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task ListQueues_WithLocalStack_ReturnsQueueUrls()
    {
        // Arrange
        var endpoint = TestContext.GetProperty<string>("SQS_ENDPOINT") ?? "http://localhost:4566";
        var region = TestContext.GetProperty<string>("AWS_REGION") ?? "us-east-1";
        var accessKeyId = TestContext.GetProperty<string>("AWS_ACCESS_KEY_ID") ?? "test";
        var secretAccessKey = TestContext.GetProperty<string>("AWS_SECRET_ACCESS_KEY") ?? "test";

        var config = new AmazonSQSConfig
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region
        };

        var client = new AmazonSQSClient(accessKeyId, secretAccessKey, config);

        // Act
        var response = await client.ListQueuesAsync(new ListQueuesRequest());

        // Assert
        Assert.IsNotNull(response);
        Assert.IsNotNull(response.QueueUrls);

        TestContext.WriteLine($"Found {response.QueueUrls.Count} queue(s):");
        foreach (var queueUrl in response.QueueUrls)
        {
            TestContext.WriteLine($"  - {queueUrl}");
        }
    }

    /// <summary>
    /// Creates a new test queue.
    /// Queue name will include timestamp to ensure uniqueness.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task CreateQueue_WithLocalStack_CreatesSuccessfully()
    {
        // Arrange
        var endpoint = TestContext.GetProperty<string>("SQS_ENDPOINT") ?? "http://localhost:4566";
        var region = TestContext.GetProperty<string>("AWS_REGION") ?? "us-east-1";
        var accessKeyId = TestContext.GetProperty<string>("AWS_ACCESS_KEY_ID") ?? "test";
        var secretAccessKey = TestContext.GetProperty<string>("AWS_SECRET_ACCESS_KEY") ?? "test";

        var config = new AmazonSQSConfig
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region
        };

        var client = new AmazonSQSClient(accessKeyId, secretAccessKey, config);
        var queueName = $"test-queue-{DateTime.UtcNow:yyyyMMddHHmmss}";

        try
        {
            // Act
            var createResponse = await client.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = new Dictionary<string, string>
                {
                    { "MessageRetentionPeriod", "300" }, // 5 minutes
                    { "VisibilityTimeout", "30" }
                }
            });

            // Assert
            Assert.IsNotNull(createResponse);
            Assert.IsNotNull(createResponse.QueueUrl);
            Assert.IsTrue(createResponse.QueueUrl.Contains(queueName));

            TestContext.WriteLine($"Created queue: {createResponse.QueueUrl}");

            // Verify queue exists
            var getUrlResponse = await client.GetQueueUrlAsync(queueName);
            Assert.AreEqual(createResponse.QueueUrl, getUrlResponse.QueueUrl);
        }
        finally
        {
            // Cleanup - delete the test queue
            try
            {
                var queueUrl = await client.GetQueueUrlAsync(queueName);
                await client.DeleteQueueAsync(queueUrl.QueueUrl);
                TestContext.WriteLine($"Deleted test queue: {queueName}");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    /// <summary>
    /// Sends a test message to a queue using the AmazonSqsMessageProvider.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendAsync_ToLocalStackQueue_SendsSuccessfully()
    {
        // Arrange
        var endpoint = TestContext.GetProperty<string>("SQS_ENDPOINT") ?? "http://localhost:4566";
        var region = TestContext.GetProperty<string>("AWS_REGION") ?? "us-east-1";
        var accessKeyId = TestContext.GetProperty<string>("AWS_ACCESS_KEY_ID") ?? "test";
        var secretAccessKey = TestContext.GetProperty<string>("AWS_SECRET_ACCESS_KEY") ?? "test";
        var queueName = TestContext.GetProperty<string>("SQS_TEST_QUEUE") ?? "integration-test-queue";

        // Ensure queue exists
        var sqsConfig = new AmazonSQSConfig
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region
        };
        var sqsClient = new AmazonSQSClient(accessKeyId, secretAccessKey, sqsConfig);

        string queueUrl;
        try
        {
            var getUrlResponse = await sqsClient.GetQueueUrlAsync(queueName);
            queueUrl = getUrlResponse.QueueUrl;
        }
        catch (QueueDoesNotExistException)
        {
            var createResponse = await sqsClient.CreateQueueAsync(queueName);
            queueUrl = createResponse.QueueUrl;
            TestContext.WriteLine($"Created test queue: {queueUrl}");
        }

        // Configure message provider
        var configBuilder = new ConfigurationBuilder();
        var configData = new Dictionary<string, string?>
        {
            { "MessageQueuing:TestQueue:QueueUrl", queueUrl },
            { "MessageQueuing:TestQueue:Region", region },
            { "MessageQueuing:TestQueue:AccessKeyId", accessKeyId },
            { "MessageQueuing:TestQueue:SecretAccessKey", secretAccessKey },
            { "MessageQueuing:TestQueue:ServiceUrl", endpoint }
        };
        configBuilder.AddInMemoryCollection(configData);
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();
        services.TryAddAmazonSqsServices();
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
        context.CorrelationId = Guid.NewGuid().ToString();

        var testMessage = new TestMessage
        {
            Id = 123,
            Content = $"Test message sent at {DateTime.UtcNow:O}",
            Timestamp = DateTime.UtcNow
        };

        var messageId = await sender.SendAsync(testMessage, context);

        // Assert
        Assert.IsNotNull(messageId);
        Assert.IsFalse(string.IsNullOrEmpty(messageId));
        TestContext.WriteLine($"Message sent with ID: {messageId}");
        TestContext.WriteLine($"Correlation ID: {context.CorrelationId}");

        // Verify message was sent by receiving it
        var receiveResponse = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
        {
            QueueUrl = queueUrl,
            MaxNumberOfMessages = 1,
            WaitTimeSeconds = 5,
            MessageAttributeNames = new List<string> { "All" }
        });

        Assert.IsTrue(receiveResponse.Messages.Count > 0, "No messages received from queue");
        var receivedMessage = receiveResponse.Messages.First();

        TestContext.WriteLine($"Received message: {receivedMessage.Body}");
        Assert.IsTrue(receivedMessage.MessageAttributes.ContainsKey("TestHeader"));
        Assert.AreEqual("TestValue", receivedMessage.MessageAttributes["TestHeader"].StringValue);
    }

    /// <summary>
    /// Tests sending to a FIFO queue with MessageGroupId.
    /// </summary>
    [TestMethod]
    [TestCategory(TestCategories.DevLocal)]
    public async Task SendAsync_ToFifoQueue_WithMessageGroupId_SendsSuccessfully()
    {
        // Arrange
        var endpoint = TestContext.GetProperty<string>("SQS_ENDPOINT") ?? "http://localhost:4566";
        var region = TestContext.GetProperty<string>("AWS_REGION") ?? "us-east-1";
        var accessKeyId = TestContext.GetProperty<string>("AWS_ACCESS_KEY_ID") ?? "test";
        var secretAccessKey = TestContext.GetProperty<string>("AWS_SECRET_ACCESS_KEY") ?? "test";
        var queueName = $"test-fifo-{DateTime.UtcNow:yyyyMMddHHmmss}.fifo";

        var sqsConfig = new AmazonSQSConfig
        {
            ServiceURL = endpoint,
            AuthenticationRegion = region
        };
        var sqsClient = new AmazonSQSClient(accessKeyId, secretAccessKey, sqsConfig);

        string queueUrl;
        try
        {
            // Create FIFO queue
            var createResponse = await sqsClient.CreateQueueAsync(new CreateQueueRequest
            {
                QueueName = queueName,
                Attributes = new Dictionary<string, string>
                {
                    { "FifoQueue", "true" },
                    { "ContentBasedDeduplication", "true" }
                }
            });
            queueUrl = createResponse.QueueUrl;
            TestContext.WriteLine($"Created FIFO queue: {queueUrl}");

            // Configure message provider
            var configBuilder = new ConfigurationBuilder();
            var configData = new Dictionary<string, string?>
            {
                { "MessageQueuing:FifoQueue:QueueUrl", queueUrl },
                { "MessageQueuing:FifoQueue:Region", region },
                { "MessageQueuing:FifoQueue:AccessKeyId", accessKeyId },
                { "MessageQueuing:FifoQueue:SecretAccessKey", secretAccessKey },
                { "MessageQueuing:FifoQueue:ServiceUrl", endpoint },
                { "MessageQueuing:FifoQueue:MessageGroupId", "test-group" }
            };
            configBuilder.AddInMemoryCollection(configData);
            var configuration = configBuilder.Build();

            var services = new ServiceCollection();
            services.TryAddAmazonSqsServices();
            services.TryAddJsonSerializer();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddSingleton<IMessageContextFactory, MessageContextFactory>();

            var provider = services.BuildServiceProvider();
            var sender = provider.GetRequiredService<IMessageSenderProvider>();
            var contextFactory = provider.GetRequiredService<IMessageContextFactory>();

            // Act
            var context = contextFactory.Create("FifoQueue", typeof(TestMessage).FullName);
            var testMessage = new TestMessage
            {
                Id = 456,
                Content = "FIFO test message",
                Timestamp = DateTime.UtcNow
            };

            var messageId = await sender.SendAsync(testMessage, context);

            // Assert
            Assert.IsNotNull(messageId);
            TestContext.WriteLine($"FIFO message sent with ID: {messageId}");
        }
        finally
        {
            // Cleanup
            try
            {
                await sqsClient.DeleteQueueAsync(queueUrl);
                TestContext.WriteLine($"Deleted FIFO queue: {queueName}");
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    private record TestMessage
    {
        public int Id { get; init; }
        public string Content { get; init; } = string.Empty;
        public DateTime Timestamp { get; init; }
    }
}
