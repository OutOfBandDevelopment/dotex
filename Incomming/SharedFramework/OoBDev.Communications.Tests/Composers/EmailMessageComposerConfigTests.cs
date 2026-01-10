using OoBDev.Communications.Composers;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Configuration;
using OoBDev.TestUtilities.Logging;
using OoBDev.TextTemplating;
using OoBDev.TextTemplating.Contracts;
using OoBDev.Toolkit;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Communications.Tests.Composers
{
    [TestClass]
    public class EmailMessageComposerConfigTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IConfiguration> mockConfiguration;
        private Mock<IGenerateText> mockGenerateText;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockConfiguration = this.mockRepository.Create<IConfiguration>();
            this.mockGenerateText = this.mockRepository.Create<IGenerateText>();
        }

        private EmailMessageComposerConfig CreateEmailMessageComposerConfig()
        {
            return new EmailMessageComposerConfig(
                this.mockConfiguration.Object,
                this.mockGenerateText.Object
                );
        }

        [DataTestMethod]
        [TestCategory(TestCategories.Unit)]
        [DataRow("true", true)]
        [DataRow("True", true)]
        [DataRow("1", true)]
        [DataRow("false", false)]
        [DataRow("False", false)]
        [DataRow("0", false)]
        public void EnableTracingTest(string value, bool expected)
        {
            // Stage

            // Mock
            mockConfiguration.SetupGet(s => s[EmailMessageComposerConfig.TraceConfig]).Returns(value);

            // Test
            var emailMessageComposerConfig = this.CreateEmailMessageComposerConfig();
            var result = emailMessageComposerConfig.EnableTracing;

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task TracingTemplateAsyncTest()
        {
            // Stage
            JObject data = await this.TestContext.GetTestDataAsync<JObject>("Data");
            var expected = "test";

            // Mock
            mockGenerateText.Setup(s => s.GenerateAsync(It.IsAny<string>(), It.IsAny<JObject>()))
                            .Callback<string, JObject>((t, d) => this.TestContext.AddResult(t))
                            .ReturnsAsync(expected);

            // Test
            var emailMessageComposerConfig = this.CreateEmailMessageComposerConfig();
            var result = await emailMessageComposerConfig.TracingTemplateAsync(data);

            // Assert
            Assert.AreEqual(expected, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Simulation)]
        public async Task TracingTemplateAsyncTest_Simulation()
        {
            // Stage
            JObject data = await this.TestContext.GetTestDataAsync<JObject>("Data");
            string expected = await this.TestContext.GetTestDataAsync<string>("ExpectedResult.txt");

            var services = new ServiceCollection()
                .AddDebugTestConfigurations()
                .AddToolkitServices()
                .AddTextTemplatingServices()
                .AddTestLoggingServices(this.TestContext)
                ;

            // Mock

            // Test
            var emailMessageComposerConfig = ActivatorUtilities.CreateInstance<EmailMessageComposerConfig>(services.BuildServiceProvider());
            var result = await emailMessageComposerConfig.TracingTemplateAsync(data);
            this.TestContext.AddResult(result);

            this.TestContext.AddResult(expected);
            this.TestContext.AddResult(result);

            // Assert
            //TODO: fix this up once timezone support is added
            CollectionAssert.AreEquivalent(
                expected.Split(Environment.NewLine.ToCharArray()).Where(l=>!l.StartsWith("SentAt:")).ToArray(), 
                result.Split(Environment.NewLine.ToCharArray()).Where(l => !l.StartsWith("SentAt:")).ToArray()
                );

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
