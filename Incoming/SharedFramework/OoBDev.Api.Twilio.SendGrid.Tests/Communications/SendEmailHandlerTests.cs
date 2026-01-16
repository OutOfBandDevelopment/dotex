using OoBDev.TestUtilities;
using OoBDev.Api.Twilio.SendGrid.Communications;
using OoBDev.Api.Twilio.SendGrid.Shared;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Models;
using OoBDev.MessageQueueing.Contracts;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using OoBDev.Communications.Contracts.Channels;
using SendGrid.Helpers.Mail;
using SendGrid;
using OoBDev.TestUtilities.Logging;

namespace OoBDev.Api.Twilio.SendGrid.Tests.Communications
{
    [TestClass]
    public class SendEmailHandlerTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;
        private Mock<IEmailMessageMapper> mockEmailMessageMapper;
        private Mock<IEmailClient> mockEmailClient;
        private Mock<IDeliveryPersitenceProvider> mockDeliveryPersitenceProvider;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockEmailMessageMapper = this.mockRepository.Create<IEmailMessageMapper>();
            this.mockEmailClient = this.mockRepository.Create<IEmailClient>();
            this.mockDeliveryPersitenceProvider = this.mockRepository.Create<IDeliveryPersitenceProvider>();
        }

        private SendGridEmailHandler CreateProvider() =>
            new SendGridEmailHandler(
                this.mockEmailMessageMapper.Object,
                this.mockEmailClient.Object,
                this.TestContext.GetTestLoggingServices<SendGridEmailHandler>(),
                this.mockDeliveryPersitenceProvider.Object);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest()
        {
            // Stage
            var sendGridMessage = new SendGridMessage();
            var response = new Response(default, default, default);
            var messageId = Guid.NewGuid();
            var data = new
            {
                ToAddresses = new[] { "test to", },
                Subject = "test subject",
            };

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(messageId);
            mockMessage.Setup(s => s.ToAddresses).Returns(data.ToAddresses);
            mockMessage.Setup(s => s.Subject).Returns(data.Subject);
            mockEmailMessageMapper.Setup(s => s.GetMessageAsync(mockMessage.Object)).ReturnsAsync(sendGridMessage);
            mockEmailClient.Setup(s => s.SendMessageAsync(sendGridMessage)).ReturnsAsync(response);
            mockDeliveryPersitenceProvider.Setup(s => s.SuccessAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<Response>())).ReturnsAsync(true);

            // Test
            var provider = this.CreateProvider();
            await provider.SendMessageAsync(mockMessage.Object);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(ApplicationException))]
        public async Task SendMessageAsyncTest_Throw()
        {
            // Stage
            var sendGridMessage = new SendGridMessage();
            var messageId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(messageId);
            mockEmailMessageMapper.Setup(s => s.GetMessageAsync(mockMessage.Object)).Throws(new ApplicationException());
            mockDeliveryPersitenceProvider.Setup(s => s.FailedAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<Exception>())).ReturnsAsync(false);

            // Test
            var provider = this.CreateProvider();
            await provider.SendMessageAsync(mockMessage.Object);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest_DontThrow()
        {
            // Stage
            var sendGridMessage = new SendGridMessage();
            var messageId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(messageId);
            mockEmailMessageMapper.Setup(s => s.GetMessageAsync(mockMessage.Object)).Throws(new ApplicationException());
            mockDeliveryPersitenceProvider.Setup(s => s.FailedAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<Exception>())).ReturnsAsync(true);

            // Test
            var provider = this.CreateProvider();
            await provider.SendMessageAsync(mockMessage.Object);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
