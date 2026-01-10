using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.Tests.MessageQueueing
{
    [TestClass]
    public class AmazonSqsMessageSenderTests
    {
        [MessageQueue(
            QueueType = QueueTypes.Default
            )]
        public class TestSender { }

        public class TestMessage { }

        public TestContext? TestContext { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private MockRepository mockRepository;

        private Mock<IAmazonSqsFactory> mockAmazonSqsFactory;
        private Mock<IQueueResolver<TestSender>> mockQueueResolver;
        private Mock<IObjectSerializer> mockObjectSerializer;
        private Mock<IQueueConfig<TestSender>> mockQueueConfig;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockAmazonSqsFactory = this.mockRepository.Create<IAmazonSqsFactory>();
            this.mockQueueResolver = this.mockRepository.Create<IQueueResolver<TestSender>>();
            this.mockObjectSerializer = this.mockRepository.Create<IObjectSerializer>();
            this.mockQueueConfig = this.mockRepository.Create<IQueueConfig<TestSender>>();
        }

        private AmazonSqsMessageSender<TestSender> CreateSqsMessageSender() => new AmazonSqsMessageSender<TestSender>(
                this.mockAmazonSqsFactory.Object,
                this.mockQueueResolver.Object,
                this.mockObjectSerializer.Object,
                this.mockQueueConfig.Object
            );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendAsyncTest()
        {
            // Stage
            var message = new TestMessage();
            var messageId = "input message id";

            var queueName = "test queue name";
            var accessKeyId = "test AccessKeyId";
            var secretAccessKey = "test SecretAccessKey";
            var region = RegionEndpoint.USEast1;
            var queueUrl = new GetQueueUrlResponse();
            var messageText = "test message";
            var messageBinary = Encoding.UTF8.GetBytes(messageText);
            var messageContent = ("test content type", messageBinary);
            var delaySeconds = 352;

            var newMessageId = "output message id";
            var sendMessage = new SendMessageResponse
            {
                MessageId = newMessageId,
            };

            var timeStamp = DateTimeOffset.UtcNow;

            var properties = new Dictionary<string, object>
            {
                { "test key", "test value"},
            };

            // Mock
            var mockAmazonSqs = mockRepository.Create<IAmazonSQS>();

            mockQueueResolver.Setup(s => s.GetQueueName()).Returns(queueName);
            mockQueueConfig.Setup(s => s.AccessKeyId).Returns(accessKeyId);
            mockQueueConfig.Setup(s => s.SecretAccessKey).Returns(secretAccessKey);
            mockQueueConfig.Setup(s => s.Region).Returns(region);
            mockQueueConfig.Setup(s => s.DelaySeconds).Returns(delaySeconds);
            mockAmazonSqsFactory.Setup(s => s.Create(accessKeyId, secretAccessKey, region)).Returns(mockAmazonSqs.Object);
            mockAmazonSqs.Setup(s => s.GetQueueUrlAsync(queueName, default)).ReturnsAsync(queueUrl);
            mockObjectSerializer.Setup(s => s.Serialize(message)).Returns(messageContent);
            mockAmazonSqs.Setup(s => s.SendMessageAsync(It.IsAny<SendMessageRequest>(), default))
                         .Callback<SendMessageRequest, CancellationToken>((req, can) => this.TestContext.AddResult(req))
                         .ReturnsAsync(sendMessage);

            // Test
            var sqsMessageSender = this.CreateSqsMessageSender();

            var result = await sqsMessageSender.SendAsync(message, messageId, properties);

            // Assert
            Assert.AreEqual(newMessageId, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
