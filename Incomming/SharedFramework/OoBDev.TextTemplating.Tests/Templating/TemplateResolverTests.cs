using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.TextTemplating.Contracts;
using OoBDev.TextTemplating.Templating;
using OoBDev.Toolkit.Templating.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Tests.Templating
{
    [TestClass]
    public class TemplateResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<ITextTemplateProvider> mockTextTemplateProvider;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockTextTemplateProvider = this.mockRepository.Create<ITextTemplateProvider>();
        }

        private TemplateResolver CreateTemplateResolver() =>
            new TemplateResolver(
                this.mockTextTemplateProvider.Object,
                this.TestContext.GetTestLoggingServices<TemplateResolver>()
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.TextGeneration)]
        public async Task GetTemplateAsyncTest()
        {
            // Stage
            var templateName = "test template name";
            var culture = new CultureInfo("es-MX");
            var templateContent = "test template";

            // Mock
            mockTextTemplateProvider.Setup(m => m.GetAsync(templateName, "es", "MX")).ReturnsAsync(templateContent);

            // Test
            var templateResolver = this.CreateTemplateResolver();

            var result = await templateResolver.GetTemplateAsync(
                templateName,
                culture);

            // Assert
            Assert.AreEqual(templateContent, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [TestCategory(TestCategories.Feature.TextGeneration)]
        public async Task GetTemplateSummariesAsyncTest()
        {
            // Stage
            var inputs = new[]
            {
                new TemplateSummaryModel{ },
            };

            // Mock
            mockTextTemplateProvider.Setup(m => m.GetSummaryAsync()).ReturnsAsync(inputs.AsEnumerable());

            // Test
            var templateResolver = this.CreateTemplateResolver();


            var result = await templateResolver.GetTemplateSummariesAsync();

            // Assert
            CollectionAssert.AreEqual(inputs, result.ToList());

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
