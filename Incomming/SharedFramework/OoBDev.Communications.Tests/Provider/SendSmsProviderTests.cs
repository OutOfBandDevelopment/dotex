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
    public class SendSmsProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;


        private Mock<IMessageSender<SmsMessageModel>> mockMessageSender;
        private Mock<ISelectedService<ISendSmsHandler>> mockServiceFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);


            this.mockMessageSender = this.mockRepository.Create<IMessageSender<SmsMessageModel>>();
            this.mockServiceFactory = this.mockRepository.Create<ISelectedService<ISendSmsHandler>>();
        }

        private SendSmsProvider CreateProvider() => new SendSmsProvider(
            this.mockMessageSender.Object,
            this.mockServiceFactory.Object,
            this.TestContext.GetTestLoggingServices<SendSmsProvider>()
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
            var mockMessage = mockRepository.Create<ISmsMessage>();
            var mockHandler = mockRepository.Create<ISendSmsHandler>();

            mockMessageSender.Setup(s => s.SendAsync(
                It.Is<ISmsMessage>(i => i == mockMessage.Object),
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
        public async Task ScheduleSendMessageAsyncTest_Error()
        {
            // Stage
            var messageId = Guid.NewGuid().ToString();
            var expected = Guid.NewGuid().ToString();
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<ISmsMessage>();
            var mockHandler = mockRepository.Create<ISendSmsHandler>();

            mockMessage.Setup(s => s.RequestId).Returns(requestId);
            mockMessageSender.Setup(s => s.SendAsync(
                It.Is<ISmsMessage>(i => i == mockMessage.Object),
                It.Is<string>(i => i == messageId),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>()
                )).ThrowsAsync(new ApplicationException());
			mockServiceFactory.Setup(s => s.Value).Returns(mockHandler.Object);

            // Test
            var provider = this.CreateProvider();
            await Assert.ThrowsExceptionAsync<ApplicationException>(async () =>
            {
                var result = await provider.ScheduleSendMessageAsync(mockMessage.Object, messageId);

                // Assert
                Assert.AreEqual(expected, result);

                // Verify
                this.mockRepository.VerifyAll();
            });
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest()
        {
            // Stage
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<ISmsMessage>();
            var mockHandler = mockRepository.Create<ISendSmsHandler>();

            mockMessage.Setup(s => s.RequestId).Returns(requestId);
            mockHandler.Setup(s => s.SendMessageAsync(mockMessage.Object)).Returns(Task.FromResult(0));
            mockServiceFactory.Setup(s => s.Value).Returns(mockHandler.Object);

            // Test
            var provider = this.CreateProvider();

            await provider.SendMessageAsync(mockMessage.Object);

            // Assert

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task SendMessageAsyncTest_Error()
        {
            // Stage
            var requestId = Guid.NewGuid();

            // Mock
            var mockMessage = mockRepository.Create<ISmsMessage>();
            var mockHandler = mockRepository.Create<ISendSmsHandler>();

            mockMessage.Setup(s => s.RequestId).Returns(requestId);
            mockHandler.Setup(s => s.SendMessageAsync(mockMessage.Object)).ThrowsAsync(new ApplicationException());
            mockServiceFactory.Setup(s => s.Value).Returns(mockHandler.Object);

            // Test
            var provider = this.CreateProvider();

            await Assert.ThrowsExceptionAsync<ApplicationException>(async () =>
            {
                await provider.SendMessageAsync(mockMessage.Object);

                // Assert

                // Verify
                this.mockRepository.VerifyAll();
            });
        }
    }
}
