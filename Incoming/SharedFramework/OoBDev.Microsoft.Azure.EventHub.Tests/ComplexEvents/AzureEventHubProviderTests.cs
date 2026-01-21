using Azure.Messaging.EventHubs;
using OoBDev.Microsoft.Azure.EventHub.ComplexEvents;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;


namespace OoBDev.Microsoft.Azure.EventHub.Tests.ComplexEvents
{
    [TestClass]
    public class AzureEventHubProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IEventResolver> mockEventResolver;
        private Mock<IObjectSerializer> mockObjectSerializer;
        private Mock<IEventHubClientFactory> mockEventHubClientFactory;

        public class TestEvent : IEventData { }
        public class TestTarget { }

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockEventResolver = this.mockRepository.Create<IEventResolver>();
            this.mockObjectSerializer = this.mockRepository.Create<IObjectSerializer>();
            this.mockEventHubClientFactory = this.mockRepository.Create<IEventHubClientFactory>();
        }

        private AzureEventHubProvider<TestTarget> CreateProvider() =>
            new AzureEventHubProvider<TestTarget>(
                this.mockEventResolver.Object,
                this.mockObjectSerializer.Object,
                this.mockEventHubClientFactory.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendAsyncTest()
        {
            // Stage
            var item = new TestEvent { };
            var properties = new Dictionary<string, object>()
            {
                { "test prop1", "value 1"},
            };
            var result = ("test type", Encoding.UTF8.GetBytes("test value"));
            var eventHub = "test event hub";
            var partitionKey = "test partition key";

            // Mock
            var mockEventHubClient = mockRepository.Create<IEventHubClient>();

            mockEventResolver.Setup(s => s.GetEventHubName<TestTarget>()).Returns(eventHub);
            mockEventResolver.Setup(s => s.GetPartitionKey<TestTarget>()).Returns(partitionKey);
            mockEventHubClientFactory.Setup(s => s.Create(eventHub, partitionKey)).Returns(mockEventHubClient.Object);

            mockObjectSerializer.Setup(s => s.Serialize(item)).Returns(result);

            mockEventHubClient.Setup(s => s.SendAsync(
                It.IsAny<EventData>(),
                It.IsAny<string>()
                ))
                .Callback<EventData, string>((evnt, key) =>
                {
                    this.TestContext.AddResult(new
                    {
                        evnt.ContentType,
                        evnt.MessageId,
                        evnt.PartitionKey,
                        evnt.CorrelationId,
                        EventBody = evnt.EventBody.ToArray(),
                        evnt.Properties,
                        evnt.SystemProperties,
                    }, fileName: nameof(EventData));
                    this.TestContext?.WriteLine($"PartitionKey:{key}");
                })
                .Returns(Task.FromResult(0));

            // Test
            var provider = this.CreateProvider();
            await provider.SendAsync(item, properties);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
