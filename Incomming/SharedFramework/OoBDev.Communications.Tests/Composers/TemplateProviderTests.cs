using OoBDev.Communications.Composers;
using OoBDev.TestUtilities;
using OoBDev.TextTemplating.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Composers
{
    [TestClass]
    public class TemplateProviderTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod, TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.CommunicationCenter)]
        public async Task GetTemplateAsyncTest()
        {
            //Stage
            string messageType = "test message";
            string deliveryChannel = "test channel";
            string section = "test section";
            CultureInfo culture = CultureInfo.GetCultureInfo("en-UK");
            JObject data = new JObject();

            var template = "test template";
            var expected = "test result";

            //Mock
            var mock = new MockRepository(MockBehavior.Strict);
            var mockResolver = mock.Create<ITemplateResolver>();
            var mockGenerate = mock.Create<IGenerateText>();
            mockResolver.Setup(m => m.GetTemplateAsync(string.Join("-", messageType, deliveryChannel, section), culture)).ReturnsAsync(template);
            mockGenerate.Setup(m => m.GenerateAsync(template, data)).ReturnsAsync(expected);

            //Test
            var provider = new TemplateProvider(
                mockResolver.Object,
                mockGenerate.Object
                );
            var result = await provider.GetTemplateAsync(messageType, deliveryChannel, section, culture, data);

            try
            {
                //Assert
                Assert.AreEqual(expected, result);

            }
            finally
            {
                //Verify
                mock.VerifyAll();
            }
        }

    }
}
