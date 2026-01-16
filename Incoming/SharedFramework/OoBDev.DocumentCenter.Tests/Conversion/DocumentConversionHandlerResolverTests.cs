using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using Microsoft.Extensions.Logging;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConversionHandlerResolverTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IServiceProvider> mockServiceProvider;
        private Mock<IDocumentConversionInputProvider> mockDocumentConversionInputProvider;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockServiceProvider = this.mockRepository.Create<IServiceProvider>();
            this.mockDocumentConversionInputProvider = this.mockRepository.Create<IDocumentConversionInputProvider>();
        }

        private DocumentConversionHandlerResolver CreateDocumentConversionHandlerResolver() =>
            new DocumentConversionHandlerResolver(
                this.mockServiceProvider.Object,
                this.mockDocumentConversionInputProvider.Object
                );
        /*
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
         */

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetHandlerTest_Direct()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var outputType = DocumentTypes.Jpeg;

            // Mock
            var mockIDocumentConversionHandler = mockRepository.Create<IDocumentConversionHandler>();
            mockDocumentConversionInputProvider
                .Setup(s => s.GetInputHandlers(DocumentTypes.Html))
                .Returns(new[]
                {
                    (mockIDocumentConversionHandler.Object, DocumentTypes.Jpeg),
                });

            // Test
            var documentConversionHandlerResolver = this.CreateDocumentConversionHandlerResolver();
            var result = documentConversionHandlerResolver.GetHandler(inputType, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetHandlerTest_Chain()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var outputType = DocumentTypes.Jpeg;

            // Mock
            var mockIDocumentConversionHandler = mockRepository.Create<IDocumentConversionHandler>();
            var mockIDocumentConversionInputProvider = mockRepository.Create<IDocumentConversionInputProvider>();
            var mockILoggerDocumentConversionChainProvider = mockRepository.Create<ILogger<DocumentConversionChainProvider>>(MockBehavior.Loose);

            mockDocumentConversionInputProvider
                .Setup(s => s.GetInputHandlers(DocumentTypes.Html))
                .Returns(new[]
                {
                    (mockIDocumentConversionHandler.Object, DocumentTypes.Markdown),
                });

            mockServiceProvider
                .Setup(s => s.GetService(typeof(IDocumentConversionInputProvider)))
                .Returns(mockIDocumentConversionInputProvider.Object);
            mockServiceProvider
                .Setup(s => s.GetService(typeof(ILogger<DocumentConversionChainProvider>)))
                .Returns(mockILoggerDocumentConversionChainProvider.Object);

            // Test
            var documentConversionHandlerResolver = this.CreateDocumentConversionHandlerResolver();
            var result = documentConversionHandlerResolver.GetHandler(inputType, outputType);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(DocumentConversionChainProvider));

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
