using OoBDev.Api.Twilio.SmsMessaging.Communications;
using OoBDev.Api.Twilio.SmsMessaging.Shared;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Models;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace OoBDev.Api.Twilio.SmsMessaging.Tests.Communications
{
    [TestClass]
    public class SendTwilioSmsHandlerTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;
        private Mock<ISmsClient> mockSmsClient;
        private Mock<IDeliveryPersitenceProvider> mockDeliveryPersitenceProvider;
        private Mock<IDateTools> mockIDateTools;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockSmsClient = this.mockRepository.Create<ISmsClient>();
            this.mockDeliveryPersitenceProvider = this.mockRepository.Create<IDeliveryPersitenceProvider>();
            this.mockIDateTools = this.mockRepository.Create<IDateTools>();
        }

        private SendTwilioSmsHandler CreateProvider() =>
            new SendTwilioSmsHandler(
                this.mockSmsClient.Object,
                this.TestContext.GetTestLoggingServices<SendTwilioSmsHandler>(),
                this.mockDeliveryPersitenceProvider.Object,
                mockIDateTools.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest()
        {
            // Stage
            var date = DateTimeOffset.Now;
            var response = FormatterServices.GetUninitializedObject(typeof(MessageResource)) as MessageResource;
            var id = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<ISmsMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(id);
            mockMessage.Setup(s => s.To).Returns("test to");
            mockMessage.Setup(s => s.Body).Returns("test body");

            mockIDateTools.Setup(s => s.Now()).Returns(date);
            mockSmsClient.Setup(s => s.SendMessageAsync(mockMessage.Object)).ReturnsAsync(response);
            mockDeliveryPersitenceProvider.Setup(s => s.SuccessAsync(id, date, response)).Returns(Task.FromResult(true));

            // Test
            var sendTwilioSmsHandler = this.CreateProvider();
            await sendTwilioSmsHandler.SendMessageAsync(mockMessage.Object);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest_ErrorThrow()
        {
            // Stage
            var date = DateTimeOffset.Now;
            var id = Guid.NewGuid();
            var exception = new ApplicationException();
            var @throw = false;

            // Mock
            var mockMessage = mockRepository.Create<ISmsMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(id);
            mockMessage.Setup(s => s.To).Returns("test to");
            mockMessage.Setup(s => s.Body).Returns("test body");

            mockIDateTools.Setup(s => s.Now()).Returns(date);
            mockSmsClient.Setup(s => s.SendMessageAsync(mockMessage.Object)).ThrowsAsync(exception);
            mockDeliveryPersitenceProvider.Setup(s => s.FailedAsync(id, date, exception)).ReturnsAsync(@throw);

            // Test
            var sendTwilioSmsHandler = this.CreateProvider();
            await Assert.ThrowsExceptionAsync<ApplicationException>(async () =>
            {
                await sendTwilioSmsHandler.SendMessageAsync(mockMessage.Object);

                // Assert

                // Verify
                this.mockRepository.VerifyAll();
            });
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest_ErrorDontThrow()
        {
            // Stage
            var date = DateTimeOffset.Now;
            var id = Guid.NewGuid();
            var exception = new ApplicationException();
            var @throw = true;

            // Mock
            var mockMessage = mockRepository.Create<ISmsMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(id);

            mockIDateTools.Setup(s => s.Now()).Returns(date);
            mockSmsClient.Setup(s => s.SendMessageAsync(mockMessage.Object)).ThrowsAsync(exception);
            mockDeliveryPersitenceProvider.Setup(s => s.FailedAsync(id, date, exception)).ReturnsAsync(@throw);

            // Test
            var sendTwilioSmsHandler = this.CreateProvider();
            await sendTwilioSmsHandler.SendMessageAsync(mockMessage.Object);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
