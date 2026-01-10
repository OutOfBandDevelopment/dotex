using OoBDev.ComplexEvents.Common.Services;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.IdentityModel.Extensions;
using OoBDev.MessageQueueing;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.TestUtilities;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Services
{
    [TestClass]
    public class EnqueueEventHubProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IEventResolver> mockEventResolver;
        private Mock<IMessageSender<EnqueueEventHubProvider<TestTarget>>> mockMessageSender;

        public class TestTarget { }
        public class TestEventTarget : IEventData { }

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockEventResolver = this.mockRepository.Create<IEventResolver>();
            this.mockMessageSender = this.mockRepository.Create<IMessageSender<EnqueueEventHubProvider<TestTarget>>>();
        }

        private EnqueueEventHubProvider<TestTarget> CreateProvider() =>
            new EnqueueEventHubProvider<TestTarget>(
                this.mockEventResolver.Object,
                this.mockMessageSender.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendAsyncTest()
        {
            // Stage
            var messageType = "test message type";
            var item = new TestEventTarget();
            var properties = new Dictionary<string, object>
            {
            };
            var messageIdOut = "test message id out";

            // Mock
            mockEventResolver.Setup(s => s.GetMessageType(item)).Returns(messageType);
            mockMessageSender.Setup(s => s.SendAsync(
                It.IsAny<ComplexEventData>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                ))
                .Callback<ComplexEventData, string, string, int, string>(
                (evnt, mesgId, caller, line, callerPath) =>
                {
                    this.TestContext.AddResult(evnt);

                    this.TestContext?.WriteLine($"MessageId:   {mesgId}");
                    this.TestContext?.WriteLine($"Caller:      {caller}");
                    this.TestContext?.WriteLine($"Line Number: {line}");
                    this.TestContext?.WriteLine($"Caller Path: {callerPath}");
                })
                .ReturnsAsync(messageIdOut);

            // Test
            var provider = this.CreateProvider();
            await provider.SendAsync(item, properties);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        public async Task ResolveAndInvokeTest()
        {
            // Stage
            var services = new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddDebugTestServices(this.TestContext)
                .AddToolkitServices()
                .AddIdentityModelExtensions()
                .AddMessageQueueingServices()
                .AddComplexEventsServices()
                ;
            var serviceProvider = services.BuildServiceProvider();
            var callback = serviceProvider.GetRequiredService<ITestCallbackWrapper>();

            var messageReceived = false;
            callback.ServiceRequest = (t,sender, o, message) =>
            {
                this.TestContext?.WriteLine(new string('>', 20));
                this.TestContext?.WriteLine($"{t}:{o}");
                this.TestContext?.WriteLine($"{nameof(sender)}:{sender}");
                this.TestContext?.WriteLine($"{nameof(message)}:{message}");
                this.TestContext?.WriteLine(new string('<', 20));
                messageReceived = true;
                return null;
            };

            // Mock

            // Test
            var service = ActivatorUtilities.CreateInstance<TestService>(serviceProvider);
            await service.SendAsync(new TestEvent
            {
                Hello = "World",
            });

            // Assert
            Assert.IsTrue(messageReceived);

            // Verify
            this.mockRepository.VerifyAll();
        }

        public class TestEvent : IEventData
        {
            public string Hello { get; internal set; }
        }

        public class TestService
        {
            private readonly IEventHubSource<TestService> _sender;
            public TestService(IEventHubSource<TestService> sender) => _sender = sender;

            public Task SendAsync<T>(T message) where T : IEventData =>
                _sender.SendAsync(message);
        }
    }
}
