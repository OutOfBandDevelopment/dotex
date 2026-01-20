using OoBDev.IdentityModel.Contracts;
using OoBDev.IdentityModel.Extensions.Services;
using OoBDev.MessageQueueing;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.ServiceBus.Tests.MessageQueueing
{
    [TestClass]
    public class AzureServiceBusQueueMessageSenderIntegrationTests
    {
        public TestContext TestContext { get; set; }

        private TestSource GetService()
        {
            var services = new ServiceCollection()
                .AddDebugTestConfigurations(
                    ("Azure:ServiceBus:Default:ConnectionString", "Endpoint=sb://lw-dev-whited.servicebus.windows.net/;SharedAccessKeyName=MwDevLocal;SharedAccessKey=lXP3YWm+swgHnZJSwNQYGrDKKOT1pHDCk4AqRLZMNiU=")
                )

                .AddToolkitServices()
                .AddMessageQueueingServices()

                .AddAzureServiceBusServices()

                .AddTestLoggingServices(TestContext, LogLevel.Trace)
                .AddLogging()
                .AddSingleton<IUserSessionAccessor>(sp => new UserSessionAccessor(sp))
                ;

            var serviceProvider = services.BuildServiceProvider();
            var sender = ActivatorUtilities.CreateInstance<TestSource>(serviceProvider);
            return sender;
        }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public async Task SendAsyncTest_Queue()
        {
            // Stage
            var message = new TestMessage();
            var messageId = Guid.NewGuid().ToString();

            // Test
            var sender = GetService();
            var result = await sender.SendQueueAsync(message, messageId);

            this.TestContext.Write(result);

            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(result));
        }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public async Task SendAsyncTest_Topic()
        {
            // Stage
            var message = new TestMessage();
            var messageId = Guid.NewGuid().ToString();

            // Test
            var sender = GetService();
            var result = await sender.SendTopicAsync(message, messageId);

            this.TestContext.Write(result);

            // Assert
            Assert.IsTrue(!string.IsNullOrEmpty(result));
        }
    }
}
