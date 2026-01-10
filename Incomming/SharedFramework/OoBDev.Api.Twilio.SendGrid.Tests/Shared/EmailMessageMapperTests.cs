using OoBDev.Api.Twilio.SendGrid.Shared;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SendGrid.Helpers.Mail;
using System.Threading.Tasks;

namespace OoBDev.Api.Twilio.SendGrid.Tests.Shared
{
    [TestClass]
    public class EmailMessageMapperTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IMessageBuilder> mockMessageBuilder;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockMessageBuilder = this.mockRepository.Create<IMessageBuilder>();
        }

        private EmailMessageMapper CreateEmailMessageMapper() =>
            new EmailMessageMapper(this.mockMessageBuilder.Object);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task GetMessageAsyncTest()
        {
            // Stage

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessageBuilder.Setup(s => s.AddFromAddress(It.Is<IEmailMessage>(m => m == mockMessage.Object), It.IsAny<SendGridMessage>()));
            mockMessageBuilder.Setup(s => s.AddToAddresses(It.Is<IEmailMessage>(m => m == mockMessage.Object), It.IsAny<SendGridMessage>()));
            mockMessageBuilder.Setup(s => s.AddCcAddresses(It.Is<IEmailMessage>(m => m == mockMessage.Object), It.IsAny<SendGridMessage>()));
            mockMessageBuilder.Setup(s => s.AddBccAddresses(It.Is<IEmailMessage>(m => m == mockMessage.Object), It.IsAny<SendGridMessage>()));
            mockMessageBuilder.Setup(s => s.AddSubject(It.Is<IEmailMessage>(m => m == mockMessage.Object), It.IsAny<SendGridMessage>()));
            mockMessageBuilder.Setup(s => s.AddBody(It.Is<IEmailMessage>(m => m == mockMessage.Object), It.IsAny<SendGridMessage>()));

            // Test
            var emailMessageMapper = this.CreateEmailMessageMapper();
            var result = await emailMessageMapper.GetMessageAsync(mockMessage.Object);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
