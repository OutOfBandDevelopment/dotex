using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using OoBDev.DocumentCenter.Conversion;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConversionChainProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentConversionInputProvider> mockDocumentConversionInputProvider;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentConversionInputProvider = this.mockRepository.Create<IDocumentConversionInputProvider>();
        }

        private DocumentConversionChainProvider CreateProvider() =>
            new DocumentConversionChainProvider(
                this.mockDocumentConversionInputProvider.Object,
                this.TestContext.GetTestLoggingServices<DocumentConversionChainProvider>()
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertAsyncTest()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var html = Encoding.UTF8.GetBytes("<html />");
            var outputType = DocumentTypes.Jpeg;

            var markdown = Encoding.UTF8.GetBytes("*markdown");
            var jpeg = Encoding.UTF8.GetBytes("jpeg!");

            // Mock
            var mockIDocumentConversionHandler = mockRepository.Create<IDocumentConversionHandler>();

            mockIDocumentConversionHandler.Setup(s => s.ConvertAsync(
                DocumentTypes.Html,
                html,
                DocumentTypes.Markdown
                )).ReturnsAsync(markdown);

            mockIDocumentConversionHandler.Setup(s => s.ConvertAsync(
                DocumentTypes.Markdown,
                markdown,
                DocumentTypes.Jpeg
                )).ReturnsAsync(jpeg);

            mockDocumentConversionInputProvider
                .Setup(s => s.GetInputHandlers(DocumentTypes.Html))
                .Returns(new[]
                {
                    (mockIDocumentConversionHandler.Object, DocumentTypes.Markdown),
                });
            mockDocumentConversionInputProvider
                .Setup(s => s.GetInputHandlers(DocumentTypes.Markdown))
                .Returns(new[]
                {
                    (mockIDocumentConversionHandler.Object, DocumentTypes.Jpeg),
                });

            // Test
            var provider = this.CreateProvider();
            var result = await provider.ConvertAsync(inputType, html, outputType);

            // Assert
            Assert.AreEqual(jpeg, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(UnhandledConversionRequestedException))]
        public async Task ConvertAsyncTest_Empty()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var html = Encoding.UTF8.GetBytes("<html />");
            var outputType = DocumentTypes.Jpeg;

            // Mock
            mockDocumentConversionInputProvider
                .Setup(s => s.GetInputHandlers(It.IsAny<DocumentTypes>()))
                .Returns(System.Array.Empty<(IDocumentConversionHandler, DocumentTypes)>());

            // Test
            var provider = this.CreateProvider();
            var result = await provider.ConvertAsync(inputType, html, outputType);

            // Assert
            Assert.Fail("you shouldn't get here");

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
