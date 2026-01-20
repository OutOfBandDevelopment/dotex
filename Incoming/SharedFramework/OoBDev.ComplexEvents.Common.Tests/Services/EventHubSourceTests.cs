using OoBDev.Microsoft.Azure.EventHub;
using OoBDev.Microsoft.Azure.EventHub.ComplexEvents;
using OoBDev.Microsoft.Azure.ServiceBus;
using OoBDev.Microsoft.Azure.Storage;
using OoBDev.ComplexEvents.Common.Services;
using OoBDev.ComplexEvents.Contracts;
using OoBDev.ComplexEvents.Contracts.Services;
using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Contracts.Models;
using OoBDev.IdentityModel.Extensions;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using OoBDev.Toolkit.Contracts.Common;
using OoBDev.Toolkit.Contracts.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.ComplexEvents.Common.Tests.Services
{
    [TestClass]
    public class EventHubSourceTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<ISelectedService<IEventHubProvider<TestTarget>>> mockSelectedService;
        private Mock<IEventResolver> mockEventResolver;
        private Mock<IUserSessionAccessor> mockUserSessionAccessor;
        private Mock<IDateTools> mockDateTools;
        private Mock<IGuidTools> mockGuidTools;

        public class TestTarget { }
        public class TestEventTarget : IEventData { }

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockSelectedService = this.mockRepository.Create<ISelectedService<IEventHubProvider<TestTarget>>>();
            this.mockEventResolver = this.mockRepository.Create<IEventResolver>();
            this.mockUserSessionAccessor = this.mockRepository.Create<IUserSessionAccessor>();
            this.mockDateTools = this.mockRepository.Create<IDateTools>();
            this.mockGuidTools = this.mockRepository.Create<IGuidTools>();
        }

        private EventHubSource<TestTarget> CreateEventHubSource() =>
            new EventHubSource<TestTarget>(
                this.mockSelectedService.Object,
                this.mockEventResolver.Object,
                this.mockUserSessionAccessor.Object,
                this.mockDateTools.Object,
                this.mockGuidTools.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendAsyncTest()
        {
            // Stage
            var messageType = "test message type";
            var item = new TestEventTarget { };
            var targetUser = Guid.NewGuid();
            var userName = "test user name";
            var date = DateTimeOffset.Now;
            var userId = Guid.NewGuid();

            // Mock
            var mockEventHubProvider = mockRepository.Create<IEventHubProvider<TestTarget>>();
            var mockUserSession = mockRepository.Create<IUserSession>();

            mockEventResolver.Setup(s => s.GetMessageType(item)).Returns(messageType);
            mockUserSessionAccessor.Setup(s => s.Value).Returns(mockUserSession.Object);
            mockUserSession.Setup(s => s.Username).Returns(userName);
            mockUserSession.Setup(s => s.UserId).Returns(userId);
            mockDateTools.Setup(s => s.Now()).Returns(date);
            mockSelectedService.Setup(s => s.Value).Returns(mockEventHubProvider.Object);
            mockGuidTools.Setup(s => s.IsNullOrEmpty(userId)).Returns(false);

            mockEventHubProvider.Setup(s => s.SendAsync(
                It.IsAny<IEventData>(),
                It.IsAny<IDictionary<string, object>>()
                ))
                .Callback<IEventData, IDictionary<string, object>>((evnt, props) =>
                {
                    this.TestContext.AddResult(evnt);
                    foreach (var prop in props)
                        this.TestContext?.WriteLine($"{prop.Key} = {prop.Value}");
                })
                .Returns(Task.FromResult(0));

            // Test
            var eventHubSource = this.CreateEventHubSource();
            await eventHubSource.SendAsync(item, targetUser);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        public class TestUserSession : IUserSession
        {
            public TestUserSession()
            {
                UserId = Guid.NewGuid();
                PersonId = Guid.NewGuid();
            }

            public Guid UserId { get; }
            public string Username { get; }
            public Guid PersonId { get; }
            public string Culture { get; }
            public IUserRights Rights { get; }
            public IExtendedProperties ExtendedProperties { get; }
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        public async Task SendAsyncTest_Simulate()
        {
            // Stage
            var item = new TestEventTarget();
            var targetUser = Guid.NewGuid();

            var services = new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddDebugTestServices(this.TestContext)
                .AddToolkitServices()
                .AddComplexEventsServices()
                .AddMessageQueueingServices()
                .AddSingleton<IUserSession>(sp => new TestUserSession())
                .AddIdentityModelExtensions()
                ;

            // Mock

            // Test
            var sp = services.BuildServiceProvider();
            var eventHubSource = sp.GetService<IEventHubSource<TestTarget>>();
            await eventHubSource.SendAsync(item, targetUser);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public async Task SendAsyncTest_DevLocal()
        {
            // Stage
            var item = new TestEventTarget();
            var targetUser = Guid.NewGuid();

            var services = new ServiceCollection()
                .AddDebugTestConfigurations(
                    ("Azure:Storage:Default:ConnectionString", "UseDevelopmentStorage=true"),
                    ("Azure:Storage:Default:Container", "oobdev-access-files-filestore")
                )
                .AddTestLoggingServices(this.TestContext)
                .AddSharedFrameworkServices()
                .AddAzureStorageServices()
                ;

            // Test
            var sp = services.BuildServiceProvider();
            var eventHubSource = sp.GetService<IEventHubSource<TestTarget>>();
            await eventHubSource.SendAsync(item, targetUser);
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Simulation)]
        [DataRow("EnqueueEventHubProvider", typeof(EnqueueEventHubProvider<>))]
        [DataRow("AzureEventHubProvider", typeof(AzureEventHubProvider<>))]
        public void ProviderSelectionTest(string providerName, Type expected)
        {
            // Stage
            var item = new TestEventTarget();
            var targetUser = Guid.NewGuid();

            var services = new ServiceCollection()
                .AddSingleton<IUserSession>(sp => new TestUserSession())
                .AddDebugTestConfigurations(
                    ("OoBDev:EventHubProvider:Type", providerName),
                    ("Azure:EventHub:Default:ConnectionString", "")
                )
                .AddDebugTestServices(this.TestContext)
                .AddToolkitServices()
                .AddComplexEventsServices()
                .AddMessageQueueingServices()
                .AddAzureStorageServices()
                .AddAzureEventHubServices()
                .AddAzureServiceBusServices()
                .AddIdentityModelExtensions()
                ;

            // Mock

            // Test
            var sp = services.BuildServiceProvider();
            var eventHubSource = sp.GetService<IEventHubSource<TestTarget>>() as EventHubSource<TestTarget>; // ActivatorUtilities..CreateInstance<EventHubSource<TestTarget>>(sp);
            var provider = eventHubSource.Provider;

            // Assert
            Assert.AreEqual(expected, provider.GetType().GetGenericTypeDefinition());

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
