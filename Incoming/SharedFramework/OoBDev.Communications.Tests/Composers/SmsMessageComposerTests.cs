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
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Composers
{
    [TestClass]
    public class SmsMessageComposerTests
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

            var baseTemplates = new[] { "Body", "From" };
            var channel = DeliveryChannels.Sms;
            var channelType = DeliveryChannelType.Sms;
            var now = DateTimeOffset.Now;
            var personSms = "test sms";
            var requestId = Guid.NewGuid();
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockSms = mock.Create<ISendSmsProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<SmsMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetSmsAsync(targetPersonId)).ReturnsAsync(personSms);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                 i.PersonId == targetPersonId &&
                 i.Requested == now &&
                 i.RequestGroupId == requestGroupId &&
                 i.DeliveryChannel == channelType &&
                 i.MessageType == messageType &&
                 i.SentTo == personSms &&
                 i.Subject == null &&
                 i.Text == "Test Body" &&
                 i.Html == null &&
                 i.EnhancedData == data.ToString()
                ))).ReturnsAsync(requestId);
            mockSms.Setup(m => m.ScheduleSendMessageAsync(It.Is<SmsMessageModel>(i =>
                   i.To == personSms &&
                   i.From == "Test From" &&
                   i.Body == "Test Body"
                 ), It.IsAny<string>())).Returns(Task.FromResult(""));

            //Test
            var composer = new SmsMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockSms.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object
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

            var baseTemplates = new[] { "Body", "From" };
            var channel = DeliveryChannels.Sms;
            var channelType = DeliveryChannelType.Sms;
            var now = DateTimeOffset.Now;
            var personSms = "test sms";
            var requestId = Guid.NewGuid();
            var thrownException = new InvalidOperationException();
            var exceptionHandled = true;
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockSms = mock.Create<ISendSmsProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<SmsMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetSmsAsync(targetPersonId)).ReturnsAsync(personSms);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                 i.PersonId == targetPersonId &&
                 i.Requested == now &&
                 i.RequestGroupId == requestGroupId &&
                 i.DeliveryChannel == channelType &&
                 i.MessageType == messageType &&
                 i.SentTo == personSms &&
                 i.Subject == null &&
                 i.Text == "Test Body" &&
                 i.Html == null &&
                 i.EnhancedData == data.ToString()
                ))).ReturnsAsync(requestId);
            mockSms.Setup(m => m.ScheduleSendMessageAsync(It.Is<SmsMessageModel>(i =>
                   i.To == personSms &&
                   i.From == "Test From" &&
                   i.Body == "Test Body"
                 ), It.IsAny<string>())).Throws(thrownException);
            mockDelivery.Setup(m => m.FailedAsync(requestId, now, thrownException)).ReturnsAsync(exceptionHandled);

            //Test
            var composer = new SmsMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockSms.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object
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

            var baseTemplates = new[] { "Body", "From" };
            var channel = DeliveryChannels.Sms;
            var channelType = DeliveryChannelType.Sms;
            var now = DateTimeOffset.Now;
            var personSms = "test sms";
            var requestId = Guid.NewGuid();
            var thrownException = new InvalidOperationException();
            var exceptionHandled = false;
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockTemplate = mock.Create<ITemplateProvider>();
            var mockPerson = mock.Create<IPersonContactProvider>();
            var mockSms = mock.Create<ISendSmsProvider>();
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<SmsMessageComposer>>(MockBehavior.Loose);
            var mockDateTools = mock.Create<IDateTools>();

            foreach (var baseTemplate in baseTemplates)
            {
                mockTemplate.Setup(m => m.GetTemplateAsync(messageType, channel, baseTemplate, culture, data)).ReturnsAsync($"Test {baseTemplate}");
            }
            mockPerson.Setup(m => m.GetSmsAsync(targetPersonId)).ReturnsAsync(personSms);
            mockDateTools.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                 i.PersonId == targetPersonId &&
                 i.Requested == now &&
                 i.RequestGroupId == requestGroupId &&
                 i.DeliveryChannel == channelType &&
                 i.MessageType == messageType &&
                 i.SentTo == personSms &&
                 i.Subject == null &&
                 i.Text == "Test Body" &&
                 i.Html == null &&
                 i.EnhancedData == data.ToString()
                ))).ReturnsAsync(requestId);
            mockSms.Setup(m => m.ScheduleSendMessageAsync(It.Is<SmsMessageModel>(i =>
                   i.To == personSms &&
                   i.From == "Test From" &&
                   i.Body == "Test Body"
                 ), It.IsAny<string>())).Throws(thrownException);
            mockDelivery.Setup(m => m.FailedAsync(requestId, now, thrownException)).ReturnsAsync(exceptionHandled);

            //Test
            var composer = new SmsMessageComposer(
                mockTemplate.Object,
                mockPerson.Object,
                mockSms.Object,
                mockDelivery.Object,
                mockLog.Object,
                mockDateTools.Object
                );

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(async () =>
            {
                await composer.ComposeAndSendAsync(targetPersonId, messageType, culture, data, requestGroupId, headers);

                //Assert
            });

            //Verify
            mock.VerifyAll();
        }
    }
}