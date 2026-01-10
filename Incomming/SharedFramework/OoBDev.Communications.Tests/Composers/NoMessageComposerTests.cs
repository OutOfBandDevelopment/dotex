using OoBDev.Communications.Composers;
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
    public class NoMessageComposerTests
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

            var channelType = DeliveryChannelType.None;
            var now = DateTimeOffset.Now;
            var sentTo = "None";
            var requestId = Guid.NewGuid();
            var headers = new Dictionary<string, object>();

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockDelivery = mock.Create<IDeliveryPersitenceProvider>();
            var mockLog = mock.Create<ILogger<NoMessageComposer>>(MockBehavior.Loose);
            var mockDate = mock.Create<IDateTools>();

            mockDate.Setup(m => m.Now()).Returns(now);
            mockDelivery.Setup(m => m.RequestedAsync(It.Is<CreateDeliveryRequestModel>(i =>
                 i.PersonId == targetPersonId &&
                 i.Requested == now &&
                 i.RequestGroupId == requestGroupId &&
                 i.DeliveryChannel == channelType &&
                 i.MessageType == messageType &&
                i.SentTo == sentTo &&
                i.Subject == null &&
                i.Text == null &&
                i.Html == null &&
                i.EnhancedData == data.ToString()
                ))).ReturnsAsync(requestId);
            mockDelivery.Setup(m => m.SuccessAsync(requestId, now, "Not Sent")).ReturnsAsync(true);

            //Test
            var composer = new NoMessageComposer(
                mockDelivery.Object,
                mockLog.Object,
                mockDate.Object
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
    }
}