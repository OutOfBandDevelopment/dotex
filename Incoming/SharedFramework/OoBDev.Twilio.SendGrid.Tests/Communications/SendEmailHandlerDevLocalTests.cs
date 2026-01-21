using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Models;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.Twilio.SendGrid.Tests.Communications
{
    [TestClass]
    public class SendEmailHandlerDevLocalTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, Ignore]
        [TestCategory(TestCategories.DevLocal)]
        public async Task ScheduleSendMessageAsyncTest()
        {
            var services = ForIntegration();

            var messageId = await services.ScheduleSendMessageAsync(new EmailMessageModel
            {
                ToAddresses = new List<string>
                {
                    "mwwhited@hotmail.com",
                },

                TextContent = "Hello World!",
                HtmlContent = "<b>Hello</b> <i>World</i>!",
            });

            this.TestContext?.WriteLine($@"MessageId: {messageId}");
        }

        [TestMethod, Ignore]
        [TestCategory(TestCategories.DevLocal)]
        public async Task SendMessageAsyncTest()
        {
            var services = ForIntegration();

            await services.SendMessageAsync(new EmailMessageModel
            {
                ToAddresses = new List<string>
                {
                    "mwwhited@hotmail.com",
                },

                TextContent = "Hello World!",
                HtmlContent = "<b>Hello</b> <i>World</i>!",
            });
        }

        private ISendEmailProvider ForIntegration()
        {
            return IntegrationServices.GetService<ISendEmailProvider>(context: this.TestContext);
        }
    }
}