using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using OoBDev.DocumentCenter.Contracts;
using System.Text;
using OoBDev.DocumentCenter.Contracts.Handlers;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConversionProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentConversionHandlerResolver> mockDocumentConversionHandlerResolver;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentConversionHandlerResolver = this.mockRepository.Create<IDocumentConversionHandlerResolver>();
        }

        private DocumentConversionProvider CreateProvider() =>
            new DocumentConversionProvider(
                this.mockDocumentConversionHandlerResolver.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertAsyncTest_Same()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var input = Encoding.UTF8.GetBytes("test data");
            var outputType = DocumentTypes.Html;

            // Mock

            // Test
            var provider = this.CreateProvider();
            var result = await provider.ConvertAsync(inputType, input, outputType);

            // Assert
            Assert.AreEqual(input, result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertAsyncTest_Convert()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var input = Encoding.UTF8.GetBytes("test data");
            var outputType = DocumentTypes.Jpeg;
            var output = Encoding.UTF8.GetBytes("result data");

            // Mock
            var mockIDocumentConversionHandler = mockRepository.Create<IDocumentConversionHandler>();

            mockDocumentConversionHandlerResolver
                .Setup(s => s.GetHandler(DocumentTypes.Html, DocumentTypes.Jpeg))
                .Returns(mockIDocumentConversionHandler.Object);

            mockIDocumentConversionHandler
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, input, DocumentTypes.Jpeg))
                .ReturnsAsync(output);

            // Test
            var provider = this.CreateProvider();
            var result = await provider.ConvertAsync(inputType, input, outputType);

            // Assert
            Assert.AreEqual(output, result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
