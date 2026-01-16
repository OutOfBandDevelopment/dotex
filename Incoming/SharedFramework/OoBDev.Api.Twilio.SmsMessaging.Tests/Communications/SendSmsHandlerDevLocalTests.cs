using OoBDev.Communications.Contracts.Channels;
using OoBDev.Communications.Contracts.Models;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Api.Twilio.SmsMessaging.Tests.Communications
{
    [TestClass]
    public class SendSmsHandlerDevLocalTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public async Task ScheduleSendMessageAsyncTest()
        {
            var service = this.TestContext.GetService<ISendSmsProvider>();
            var corrId = await service.ScheduleSendMessageAsync(new SmsMessageModel
            {
                To = "+16149891748",
                Body = "Test Message",
            });
            this.TestContext?.WriteLine($"Correlation ID: {corrId}");
        }

        [TestMethod]
        [TestCategory(TestCategories.DevLocal)]
        public async Task SendMessageAsyncTest()
        {
            var service = this.TestContext.GetService<ISendSmsProvider>();
            await service.SendMessageAsync(new SmsMessageModel
            {
               To = "+16149891748", //Matt Whited 1
                Body = "Test Message",
            });
        }
    }
}
