using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Handler;
using OoBDev.Communications.Contracts.Models;
using OoBDev.Communications.Provider;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit.Contracts.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Provider
{
    [TestClass]
    public class SendEmailProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IMessageSender<EmailMessageModel>> mockMessageSender;
        private Mock<ISelectedService<ISendEmailHandler>> mockHandler;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockMessageSender = this.mockRepository.Create<IMessageSender<EmailMessageModel>>();
            this.mockHandler = this.mockRepository.Create<ISelectedService<ISendEmailHandler>>();
        }

        private SendEmailProvider CreateProvider() => new SendEmailProvider(
            this.mockMessageSender.Object,
            this.mockHandler.Object,
            this.TestContext.GetTestLoggingServices<SendEmailProvider>()
            );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ScheduleSendMessageAsyncTest()
        {
            // Stage
            var messageId = Guid.NewGuid().ToString();
            var expected = Guid.NewGuid().ToString();
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();

            mockMessageSender.Setup(s => s.SendAsync(
                It.Is<IEmailMessage>(i => i == mockMessage.Object),
                It.Is<string>(i => i == messageId),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).ReturnsAsync(expected);

            // Test
            var provider = this.CreateProvider();


            var result = await provider.ScheduleSendMessageAsync(mockMessage.Object, messageId);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(ApplicationException))]
        public async Task ScheduleSendMessageAsyncTest_Error()
        {
            // Stage
            var messageId = Guid.NewGuid().ToString();
            var expected = Guid.NewGuid().ToString();
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.RequestId).Returns(requestId);
            var mockHandler = mockRepository.Create<ISendEmailHandler>();

            mockMessageSender.Setup(s => s.SendAsync(
                It.Is<IEmailMessage>(i => i == mockMessage.Object),
                It.Is<string>(i => i == messageId),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).ThrowsAsync(new ApplicationException());
            this.mockHandler.Setup(s => s.Value).Returns(mockHandler.Object);

            // Test
            var provider = this.CreateProvider();


            var result = await provider.ScheduleSendMessageAsync(mockMessage.Object, messageId);

            // Assert
            Assert.Fail("you shouldnt get here");

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest()
        {
            // Stage
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            var mockHandler = mockRepository.Create<ISendEmailHandler>();

            mockMessage.Setup(s => s.RequestId).Returns(requestId);
            mockHandler.Setup(s => s.SendMessageAsync(mockMessage.Object)).Returns(Task.FromResult(0));
            this.mockHandler.Setup(s => s.Value).Returns(mockHandler.Object);

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
        public async Task SendMessageAsyncTest_Error()
        {
            // Stage
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            var mockHandler = mockRepository.Create<ISendEmailHandler>();

            mockMessage.Setup(s => s.RequestId).Returns(requestId);
            mockHandler.Setup(s => s.SendMessageAsync(mockMessage.Object)).ThrowsAsync(new ApplicationException());
            this.mockHandler.Setup(s => s.Value).Returns(mockHandler.Object);

            // Test
            var provider = this.CreateProvider();

            await provider.SendMessageAsync(mockMessage.Object);

            // Assert
            Assert.Fail("you shouldnt get here");

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
