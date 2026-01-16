using Amazon;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Contracts.Services;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace OoBDev.Amazon.Sqs.Tests.MessageQueueing
{
    [TestClass]
    public class AmazonSqsFactoryTests
    {
        public TestContext? TestContext { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private MockRepository mockRepository;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private AmazonSqsFactory CreateFactory() => new AmazonSqsFactory();

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void CreateTest()
        {
            // Stage

            // Mock
            var factory = this.CreateFactory();
            var mockConnection = mockRepository.Create<IQueueConnectionString>();
            mockConnection.Setup(s => s["AccessKeyId"]).Returns("Test AccessKeyId");
            mockConnection.Setup(s => s["SecretAccessKey"]).Returns("Test SecretAccessKey");
            mockConnection.Setup(s => s["Region"]).Returns("us-east-1");

            // Test
            var result = factory.Create(mockConnection.Object);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
