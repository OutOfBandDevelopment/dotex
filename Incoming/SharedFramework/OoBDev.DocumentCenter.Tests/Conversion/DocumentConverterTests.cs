using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Conversion;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConverterTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentStore> mockDocumentStore;
        private Mock<IDocumentTypeResolver> mockDocumentTypeResolver;
        private Mock<IDocumentConversionProvider> mockDocumentConversionProvider;
        private Mock<IStreamTools> mockStreamTools;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentStore = this.mockRepository.Create<IDocumentStore>();
            this.mockDocumentTypeResolver = this.mockRepository.Create<IDocumentTypeResolver>();
            this.mockDocumentConversionProvider = this.mockRepository.Create<IDocumentConversionProvider>();
            this.mockStreamTools = this.mockRepository.Create<IStreamTools>();
        }

        private DocumentConverter CreateDocumentConverter() =>
            new DocumentConverter(
                this.mockDocumentStore.Object,
                this.mockDocumentTypeResolver.Object,
                this.mockDocumentConversionProvider.Object,
                this.mockStreamTools.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_1_KeyType()
        {
            // Stage
            var key = "test file";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentContentResult = mockRepository.Create<IDocumentContentResult>();
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentStore.Setup(s => s.GetAsync(key)).ReturnsAsync(mockIDocumentContentResult.Object);
            mockIDocumentContentResult.Setup(s => s.DocumentType).Returns(documentData.type);
            mockIDocumentContentResult.Setup(s => s.Content).Returns(documentData.data);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockDocumentStore.Setup(s => s.StoreAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DocumentTypes>()
                ))
                .Callback<string, byte[], DocumentTypes>((file, content, type) =>
                 {
                     this.TestContext.AddResult(content, fileName: file);
                     this.TestContext?.WriteLine($"Type: {type}");
                 })
                .ReturnsAsync(mockIDocumentStoreResult.Object);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(key, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_2_KeyContainerType()
        {
            // Stage
            var key = "test key";
            var container = "test container";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentContentResult = mockRepository.Create<IDocumentContentResult>();
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentStore.Setup(s => s.GetAsync(key, container)).ReturnsAsync(mockIDocumentContentResult.Object);
            mockIDocumentContentResult.Setup(s => s.DocumentType).Returns(documentData.type);
            mockIDocumentContentResult.Setup(s => s.Content).Returns(documentData.data);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockDocumentStore.Setup(s => s.StoreAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DocumentTypes>()
                ))
                .Callback<string, byte[], DocumentTypes>((file, content, type) =>
                {
                    this.TestContext.AddResult(content, fileName: file);
                    this.TestContext?.WriteLine($"Type: {type}");
                })
                .ReturnsAsync(mockIDocumentStoreResult.Object);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(key, container, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_3_TypeBytesType()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var content = Encoding.UTF8.GetBytes("test content");
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",

                file = "test file",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(inputType, content, outputType))
                .ReturnsAsync(documentData.data);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(outputType)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GenerateFileName(outputType)).Returns(documentData.file);
            mockDocumentStore
                .Setup(s => s.StoreAsync(documentData.file, content, outputType))
                .ReturnsAsync(mockIDocumentStoreResult.Object);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(inputType, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_4_TypeStreamType()
        {
            // Stage
            var data = Encoding.UTF8.GetBytes("test content");

            var inputType = DocumentTypes.Html;
            var content = new MemoryStream(data);
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",

                file = "test file",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(inputType, data, outputType))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(outputType)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GenerateFileName(outputType)).Returns(documentData.file);
            mockDocumentStore
                .Setup(s => s.StoreAsync(documentData.file, documentData.output, outputType))
                .ReturnsAsync(mockIDocumentStoreResult.Object);
            mockStreamTools.Setup(s => s.ToArrayAsync(content)).ReturnsAsync(content.ToArray());

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(inputType, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_5_FileBytesType()
        {
            // Stage
            var fileName = "test file";
            var content = Encoding.UTF8.GetBytes("test content");
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByFileName(fileName)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockDocumentStore.Setup(s => s.StoreAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DocumentTypes>()
                ))
                .Callback<string, byte[], DocumentTypes>((file, content, type) =>
                {
                    this.TestContext.AddResult(content, fileName: file);
                    this.TestContext?.WriteLine($"Type: {type}");
                })
                .ReturnsAsync(mockIDocumentStoreResult.Object);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(fileName, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_6_FileStreamType()
        {
            // Stage
            var fileName = "test file";
            var content = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByFileName(fileName)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockDocumentStore.Setup(s => s.StoreAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DocumentTypes>()
                ))
                .Callback<string, byte[], DocumentTypes>((file, content, type) =>
                {
                    this.TestContext.AddResult(content, fileName: file);
                    this.TestContext?.WriteLine($"Type: {type}");
                })
                .ReturnsAsync(mockIDocumentStoreResult.Object);
            mockStreamTools.Setup(s => s.ToArrayAsync(content)).ReturnsAsync(content.ToArray());

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(fileName, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_7_FileBytesMimeType()
        {
            // Stage
            var fileName = "test file";
            var content = Encoding.UTF8.GetBytes("test content");
            var contentType = "text content type";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByMime(contentType)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockDocumentStore.Setup(s => s.StoreAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<DocumentTypes>()
                ))
                .Callback<string, byte[], DocumentTypes>((file, content, type) =>
                {
                    this.TestContext.AddResult(content, fileName: file);
                    this.TestContext?.WriteLine($"Type: {type}");
                })
                .ReturnsAsync(mockIDocumentStoreResult.Object);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(fileName, content, contentType, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAndStoreAsyncTest_8_FileStreamMimeType()
        {
            // Stage
            var fileName = "test file";
            var content = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            var contentType = "text content type";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByMime(contentType)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockDocumentStore.Setup(s => s.StoreAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>()
                ))
                .Callback<string, byte[]>((file, content) =>
                {
                    this.TestContext.AddResult(content, fileName: file);
                })
                .ReturnsAsync(mockIDocumentStoreResult.Object);
            mockStreamTools.Setup(s => s.ToArrayAsync(content)).ReturnsAsync(content.ToArray());

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAndStoreAsync(fileName, content, contentType, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_1_KeyType()
        {
            // Stage
            var key = "test key";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentContentResult = mockRepository.Create<IDocumentContentResult>();
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentStore.Setup(s => s.GetAsync(key)).ReturnsAsync(mockIDocumentContentResult.Object);
            mockIDocumentContentResult.Setup(s => s.DocumentType).Returns(documentData.type);
            mockIDocumentContentResult.Setup(s => s.Content).Returns(documentData.data);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(key, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_2_KeyContainerType()
        {
            // Stage
            var key = "test key";
            var container = "test container";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentContentResult = mockRepository.Create<IDocumentContentResult>();
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentStore.Setup(s => s.GetAsync(key, container)).ReturnsAsync(mockIDocumentContentResult.Object);
            mockIDocumentContentResult.Setup(s => s.DocumentType).Returns(documentData.type);
            mockIDocumentContentResult.Setup(s => s.Content).Returns(documentData.data);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(key, container, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_3_TypeBytesType()
        {
            // Stage
            var inputType = DocumentTypes.Html;
            var content = Encoding.UTF8.GetBytes("test content");
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",

                file = "test file",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(inputType, content, outputType))
                .ReturnsAsync(documentData.data);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(outputType)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GenerateFileName(outputType)).Returns(documentData.file);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(inputType, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_4_TypeStreamType()
        {
            // Stage
            var data = Encoding.UTF8.GetBytes("test content");

            var inputType = DocumentTypes.Html;
            var content = new MemoryStream(data);
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",

                file = "test file",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(inputType, data, outputType))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(outputType)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GenerateFileName(outputType)).Returns(documentData.file);
            mockStreamTools.Setup(s => s.ToArrayAsync(content)).ReturnsAsync(content.ToArray());

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(inputType, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_5_FileBytesType()
        {
            // Stage
            var fileName = "test file";
            var content = Encoding.UTF8.GetBytes("test content");
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByFileName(fileName)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(fileName, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_6_FileStreamType()
        {
            // Stage
            var fileName = "test file";
            var content = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByFileName(fileName)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockStreamTools.Setup(s => s.ToArrayAsync(content)).ReturnsAsync(content.ToArray());

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(fileName, content, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_7_FileBytesMimeType()
        {
            // Stage
            var fileName = "test file";
            var content = Encoding.UTF8.GetBytes("test content");
            var contentType = "text content type";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByMime(contentType)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(fileName, content, contentType, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task ConvertToAsyncTest_8_FileStreamMimeType()
        {
            // Stage
            var fileName = "test file";
            var content = new MemoryStream(Encoding.UTF8.GetBytes("test content"));
            var contentType = "text content type";
            var outputType = DocumentTypes.Jpeg;

            var documentData = new
            {
                type = DocumentTypes.Html,
                data = Encoding.UTF8.GetBytes("test content"),
                output = Encoding.UTF8.GetBytes("test output"),

                mimeType = "test mime",
                ext = ".ext",
            };

            // Mock
            var mockIDocumentStoreResult = mockRepository.Create<IDocumentStoreResult>();

            mockDocumentTypeResolver.Setup(s => s.GetByMime(contentType)).Returns(documentData.type);
            mockDocumentConversionProvider
                .Setup(s => s.ConvertAsync(DocumentTypes.Html, documentData.data, DocumentTypes.Jpeg))
                .ReturnsAsync(documentData.output);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Jpeg)).Returns(documentData.mimeType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(DocumentTypes.Jpeg)).Returns(documentData.ext);
            mockStreamTools.Setup(s => s.ToArrayAsync(content)).ReturnsAsync(content.ToArray());

            // Test
            var documentConverter = this.CreateDocumentConverter();
            var result = await documentConverter.ConvertToAsync(fileName, content, contentType, outputType);

            // Assert
            Assert.IsNotNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
