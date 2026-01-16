using OoBDev.Communications.Composers;
using OoBDev.Communications.Contracts;
using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Composers;
using OoBDev.Communications.Contracts.DeliveryLog;
using OoBDev.Communications.Contracts.Models;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Composers
{
    [TestClass]
    public class EmailMessageComposerTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ComposeAndSendAsyncTest()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var culture = CultureInfo.GetCultureInfo("en-UK");
            var data = new JObject();
            var requestGroupId = Guid.NewGuid();

            var baseTemplates = new[] { "Subject", "Body", "Html", "From" };
            var channel = DeliveryChannels.Email;
            var channelType = DeliveryChannelType.Email;
            var now = DateTimeOffset.Now;
            var personEmail = "test email";
            var requestId = Guid.NewGuid();
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockEmail = mock.Create<ISendEmailProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<EmailMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockEmailMessageComposerConfig = mock.Create<IEmailMessageComposerConfig>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetEmailAsync(targetPersonId)).ReturnsAsync(personEmail);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                    i.PersonId == targetPersonId &&
                    i.Requested == now &&
                    i.RequestGroupId == requestGroupId &&
                    i.DeliveryChannel == channelType &&
                    i.MessageType == messageType &&
                    i.SentTo == personEmail &&
                    i.Subject == "Test Subject" &&
                    i.Text == "Test Body" &&
                    i.Html == "Test Html" &&
                    i.EnhancedData == data.ToString()
                )))
                .Callback<ICreateDeliveryRequest>(m => this.TestContext.AddResult(m))
                .ReturnsAsync(requestId);
            mockEmail.Setup(m => m.ScheduleSendMessageAsync(It.Is<EmailMessageModel>(i =>
                    i.ToAddresses.ElementAt(0) == personEmail &&
                    i.FromAddress == "Test From" &&
                    i.Subject == "Test Subject" &&
                    i.TextContent == "Test Body" &&
                    i.HtmlContent == "Test Html"
                 ), It.IsAny<string>()))
                .Callback<IEmailMessage, string>((m, i) => this.TestContext.AddResult(m))
                .Returns(Task.FromResult("")); 
            mockEmailMessageComposerConfig.SetupGet(s => s.EnableTracing).Returns(false);


            //Test
            var composer = new EmailMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockEmail.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockEmailMessageComposerConfig.Object
                );

            await composer.ComposeAndSendAsync(targetPersonId, messageType, culture, data, requestGroupId, headers);

            try
            {
                //Assert
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ComposeAndSendAsyncTest_Traced()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var culture = CultureInfo.GetCultureInfo("en-UK");
            var data = new JObject();
            var requestGroupId = Guid.NewGuid();

            var baseTemplates = new[] { "Subject", "Body", "Html", "From" };
            var channel = DeliveryChannels.Email;
            var channelType = DeliveryChannelType.Email;
            var now = DateTimeOffset.Now;
            var personEmail = "test email";
            var requestId = Guid.NewGuid();
            var headers = new Dictionary<string, object>();
            var tracing = "test trace";

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockEmail = mock.Create<ISendEmailProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<EmailMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockEmailMessageComposerConfig = mock.Create<IEmailMessageComposerConfig>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetEmailAsync(targetPersonId)).ReturnsAsync(personEmail);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                    i.PersonId == targetPersonId &&
                    i.Requested == now &&
                    i.RequestGroupId == requestGroupId &&
                    i.DeliveryChannel == channelType &&
                    i.MessageType == messageType &&
                    i.SentTo == personEmail &&
                    i.Subject == "Test Subject" &&
                    i.EnhancedData == data.ToString()
                )))
                .Callback<ICreateDeliveryRequest>(m => this.TestContext.AddResult(m))
                .ReturnsAsync(requestId);
            mockEmail.Setup(m => m.ScheduleSendMessageAsync(It.Is<EmailMessageModel>(i =>
                    i.ToAddresses.ElementAt(0) == personEmail &&
                    i.FromAddress == "Test From" &&
                    i.Subject == "Test Subject"
                 ), It.IsAny<string>()))
                .Callback<IEmailMessage, string>((m,i)=> this.TestContext.AddResult(m))
                .Returns(Task.FromResult(""));
            mockEmailMessageComposerConfig.SetupGet(s => s.EnableTracing).Returns(true);
            mockEmailMessageComposerConfig.Setup(s => s.TracingTemplateAsync(data)).ReturnsAsync(tracing);

            //Test
            var composer = new EmailMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockEmail.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockEmailMessageComposerConfig.Object
                );

            await composer.ComposeAndSendAsync(targetPersonId, messageType, culture, data, requestGroupId, headers);

            try
            {
                //Assert
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ComposeAndSendAsyncTest_ExceptionHandled()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var culture = CultureInfo.GetCultureInfo("en-UK");
            var data = new JObject();
            var requestGroupId = Guid.NewGuid();

            var baseTemplates = new[] { "Subject", "Body", "Html", "From" };
            var channel = DeliveryChannels.Email;
            var channelType = DeliveryChannelType.Email;
            var now = DateTimeOffset.Now;
            var personEmail = "test email";
            var requestId = Guid.NewGuid();
            var thrownException = new InvalidOperationException();
            var exceptionHandled = true;
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockEmail = mock.Create<ISendEmailProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<EmailMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockEmailMessageComposerConfig = mock.Create<IEmailMessageComposerConfig>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetEmailAsync(targetPersonId)).ReturnsAsync(personEmail);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                 i.PersonId == targetPersonId &&
                 i.Requested == now &&
                 i.RequestGroupId == requestGroupId &&
                 i.DeliveryChannel == channelType &&
                 i.MessageType == messageType &&
                 i.SentTo == personEmail &&
                 i.Subject == "Test Subject" &&
                 i.Text == "Test Body" &&
                 i.Html == "Test Html" &&
                 i.EnhancedData == data.ToString()
                ))).ReturnsAsync(requestId);
            mockEmail.Setup(m => m.ScheduleSendMessageAsync(It.Is<EmailMessageModel>(i =>
                   i.ToAddresses.ElementAt(0) == personEmail &&
                   i.FromAddress == "Test From" &&
                   i.Subject == "Test Subject" &&
                   i.TextContent == "Test Body" &&
                   i.HtmlContent == "Test Html"
                 ), It.IsAny<string>())).Throws(thrownException);
            mockDelivery.Setup(m => m.FailedAsync(requestId, now, thrownException)).ReturnsAsync(exceptionHandled);
            mockEmailMessageComposerConfig.SetupGet(s => s.EnableTracing).Returns(false);

            //Test
            var composer = new EmailMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockEmail.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockEmailMessageComposerConfig.Object
                );

            await composer.ComposeAndSendAsync(targetPersonId, messageType, culture, data, requestGroupId, headers);

            try
            {
                //Assert
            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task ComposeAndSendAsyncTest_ExceptionThrown()
        {
            //Stage
            var targetPersonId = Guid.NewGuid();
            var messageType = "test message type";
            var culture = CultureInfo.GetCultureInfo("en-UK");
            var data = new JObject();
            var requestGroupId = Guid.NewGuid();

            var baseTemplates = new[] { "Subject", "Body", "Html", "From" };
            var channel = DeliveryChannels.Email;
            var channelType = DeliveryChannelType.Email;
            var now = DateTimeOffset.Now;
            var personEmail = "test email";
            var requestId = Guid.NewGuid();
            var thrownException = new InvalidOperationException();
            var exceptionHandled = false;
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockEmail = mock.Create<ISendEmailProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<EmailMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();
            var mockEmailMessageComposerConfig = mock.Create<IEmailMessageComposerConfig>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetEmailAsync(targetPersonId)).ReturnsAsync(personEmail);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                 i.PersonId == targetPersonId &&
                 i.Requested == now &&
                 i.RequestGroupId == requestGroupId &&
                 i.DeliveryChannel == channelType &&
                 i.MessageType == messageType &&
                 i.SentTo == personEmail &&
                 i.Subject == "Test Subject" &&
                 i.Text == "Test Body" &&
                 i.Html == "Test Html" &&
                 i.EnhancedData == data.ToString()
                ))).ReturnsAsync(requestId);
            mockEmail.Setup(m => m.ScheduleSendMessageAsync(It.Is<EmailMessageModel>(i =>
                   i.ToAddresses.ElementAt(0) == personEmail &&
                   i.FromAddress == "Test From" &&
                   i.Subject == "Test Subject" &&
                   i.TextContent == "Test Body" &&
                   i.HtmlContent == "Test Html"
                 ), It.IsAny<string>())).Throws(thrownException);
            mockDelivery.Setup(m => m.FailedAsync(requestId, now, thrownException)).ReturnsAsync(exceptionHandled);
            mockEmailMessageComposerConfig.SetupGet(s => s.EnableTracing).Returns(false);

            //Test
            var composer = new EmailMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockEmail.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object,
                mockEmailMessageComposerConfig.Object
                );

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await composer.ComposeAndSendAsync(targetPersonId, messageType, culture, data, requestGroupId, headers);

                //Assert
                Assert.Fail("You shouldn't get here!");
            });

            //Verify
            mock.VerifyAll();
        }
    }
}