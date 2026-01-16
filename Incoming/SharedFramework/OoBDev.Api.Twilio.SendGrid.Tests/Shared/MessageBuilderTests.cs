using OoBDev.Api.Twilio.SendGrid.Shared;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Linq;

namespace OoBDev.Api.Twilio.SendGrid.Tests.Shared
{
    [TestClass]
    public class MessageBuilderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;
        private Mock<IConfiguration> mockConfig;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
            this.mockConfig = this.mockRepository.Create<IConfiguration>();
        }

        private MessageBuilder CreateMessageBuilder() =>
            new MessageBuilder(this.mockConfig.Object);

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddFromAddressTest()
        {
            // Stage
            var email = new SendGridMessage();
            string emailAddress = "read@email.com";
            string defaultEmailAddress = "default@email.com";

            // Mock
            mockConfig.Setup(s => s["Twilio:SendGrid:Default:From:Email"]).Returns(defaultEmailAddress);
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.FromAddress).Returns(emailAddress);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddFromAddress(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(emailAddress, email.From.Email);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddFromAddressTest_Default()
        {
            // Stage
            var email = new SendGridMessage();
            string emailAddress = null;
            string defaultEmailAddress = "default@email.com";

            // Mock
            mockConfig.Setup(s => s["Twilio:SendGrid:Default:From:Email"]).Returns(defaultEmailAddress);
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.FromAddress).Returns(emailAddress);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddFromAddress(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(defaultEmailAddress, email.From.Email);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddToAddressesTest()
        {
            // Stage
            var email = new SendGridMessage();
            var data = new[]
            {
                "test@address.com",
            };

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.ToAddresses).Returns(data);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddToAddresses(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(data[0], email.Personalizations[0].Tos[0].Email);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddToAddressesTest_Default()
        {
            // Stage
            var email = new SendGridMessage();
            string[] data = null;

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.ToAddresses).Returns(data);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddToAddresses(mockMessage.Object, email);

            // Assert
            Assert.IsNull(email.Personalizations);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddCcAddressesTest()
        {
            // Stage
            var email = new SendGridMessage();
            var data = new[]
            {
                "test@address.com",
            };

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.CcAddresses).Returns(data);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddCcAddresses(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(data[0], email.Personalizations[0].Ccs[0].Email);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddCcAddressesTest_Default()
        {
            // Stage
            var email = new SendGridMessage();
            string[] data = null;

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.CcAddresses).Returns(data);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddCcAddresses(mockMessage.Object, email);

            // Assert
            Assert.IsNull(email.Personalizations);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddBccAddressesTest()
        {
            // Stage
            var email = new SendGridMessage();
            var data = new[]
            {
                "test@address.com",
            };

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.BccAddresses).Returns(data);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddBccAddresses(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(data[0], email.Personalizations[0].Bccs[0].Email);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddBccAddressesTest_Default()
        {
            // Stage
            var email = new SendGridMessage();
            string[] data = null;

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.BccAddresses).Returns(data);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddBccAddresses(mockMessage.Object, email);

            // Assert
            Assert.IsNull(email.Personalizations);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddSubjectAddressTest()
        {
            // Stage
            var email = new SendGridMessage();
            string subject = "test subject";
            string defaultSubject = "default subject";

            // Mock
            mockConfig.Setup(s => s["Twilio:SendGrid:Default:Subject"]).Returns(defaultSubject);
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.Subject).Returns(subject);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddSubject(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(subject, email.Subject);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddSubjectAddressTest_Default()
        {
            // Stage
            var email = new SendGridMessage();
            string subject = null;
            string defaultSubject = "default subject";


            // Mock
            mockConfig.Setup(s => s["Twilio:SendGrid:Default:Subject"]).Returns(defaultSubject);
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.Subject).Returns(subject);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddSubject(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(defaultSubject, email.Subject);

            // Verify
            this.mockRepository.VerifyAll();
        }
        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddSubjectAddressTest_Null()
        {
            // Stage
            var email = new SendGridMessage();
            string subject = null;
            string defaultSubject = null;

            // Mock
            mockConfig.Setup(s => s["Twilio:SendGrid:Default:Subject"]).Returns(defaultSubject);
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.Subject).Returns(subject);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddSubject(mockMessage.Object, email);

            // Assert
            Assert.IsNotNull(email.Subject);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddBodyTest_Both()
        {
            // Stage
            var email = new SendGridMessage();
            string text = "test text body";
            string html = "test html body";

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.TextContent).Returns(text);
            mockMessage.Setup(s => s.HtmlContent).Returns(html);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddBody(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(text, email.Contents.FirstOrDefault(i => i.Type == MimeType.Text)?.Value);
            Assert.AreEqual(html, email.Contents.FirstOrDefault(i => i.Type == MimeType.Html)?.Value);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(NotSupportedException))]
        public void AddBodyTest_Neither()
        {
            // Stage
            var email = new SendGridMessage();
            string text = null;
            string html = null;

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.TextContent).Returns(text);
            mockMessage.Setup(s => s.HtmlContent).Returns(html);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddBody(mockMessage.Object, email);

            // Assert
            Assert.Fail("you shouldnt get here");

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddBodyTest_Text()
        {
            // Stage
            var email = new SendGridMessage();
            string text = "test text body";
            string html = null;

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.TextContent).Returns(text);
            mockMessage.Setup(s => s.HtmlContent).Returns(html);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddBody(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(text, email.Contents.FirstOrDefault(i => i.Type == MimeType.Text)?.Value);
            Assert.AreEqual(html, email.Contents.FirstOrDefault(i => i.Type == MimeType.Html)?.Value);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void AddBodyTest_Html()
        {
            // Stage
            var email = new SendGridMessage();
            string text = null;
            string html = "test html body";

            // Mock
            var mockMessage = mockRepository.Create<IEmailMessage>();
            mockMessage.Setup(s => s.TextContent).Returns(text);
            mockMessage.Setup(s => s.HtmlContent).Returns(html);

            // Test
            var messageBuilder = this.CreateMessageBuilder();
            messageBuilder.AddBody(mockMessage.Object, email);

            // Assert
            Assert.AreEqual(text, email.Contents.FirstOrDefault(i => i.Type == MimeType.Text)?.Value);
            Assert.AreEqual(html, email.Contents.FirstOrDefault(i => i.Type == MimeType.Html)?.Value);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
