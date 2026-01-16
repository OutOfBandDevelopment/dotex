using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Conversion;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConversionInputProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);
        }

        private DocumentConversionInputProvider CreateProvider(params IDocumentConversionHandler[] handlers) =>
            new DocumentConversionInputProvider(handlers);

        /*

            from handler in _handlers
            from attribute in TypeDescriptor.GetAttributes(handler).OfType<DocumentHandlerAttribute>()
            where attribute.InputType == inputType
            orderby attribute.Priority descending
            select (handler, attribute.OutputType);
        */

        public abstract class ConvertBase : IDocumentConversionHandler
        {
            public Task<byte[]> ConvertAsync(DocumentTypes inputType, byte[] input, DocumentTypes outputType) =>
            throw new NotImplementedException();
        }
        [DocumentHandler(InputType = DocumentTypes.Html, OutputType = DocumentTypes.Markdown, Priority = 5)]
        public class Html2Markdown : ConvertBase { }
        [DocumentHandler(InputType = DocumentTypes.Html, OutputType = DocumentTypes.Text, Priority = 10)]
        public class Html2Text : ConvertBase { }
        [DocumentHandler(InputType = DocumentTypes.Pdf, OutputType = DocumentTypes.Jpeg, Priority = 1)]
        public class Pdf2Jpeg : ConvertBase { }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void GetInputHandlersTest()
        {
            // Stage
            var inputType = DocumentTypes.Html;

            // Mock

            // Test
            var provider = this.CreateProvider(
                new Html2Markdown(),
                new Html2Text(),
                new Pdf2Jpeg()
                );
            var result = provider.GetInputHandlers(inputType).ToArray();

            // Assert
            Assert.AreEqual(2, result.Count());
            Assert.IsInstanceOfType(result.ElementAt(0).handler, typeof(Html2Text));
            Assert.IsInstanceOfType(result.ElementAt(1).handler, typeof(Html2Markdown));
            Assert.AreEqual(DocumentTypes.Text, result.ElementAt(0).output);
            Assert.AreEqual(DocumentTypes.Markdown, result.ElementAt(1).output);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
