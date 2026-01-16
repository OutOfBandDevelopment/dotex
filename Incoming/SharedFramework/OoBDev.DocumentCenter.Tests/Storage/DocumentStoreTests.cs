using OoBDev.ComplexEvents.Contracts;
using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Storage;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Logging;
using OoBDev.Toolkit.Contracts.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Tests.Storage
{
    [TestClass]
    public class DocumentStoreTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentKeyGenerator> mockDocumentKeyGenerator;
        private Mock<IBlobContainerClient<DocumentStore>> mockBlobContainerClient;
        private Mock<IDocumentTypeResolver> mockDocumentTypeResolver;
        private Mock<IDateTools> mockDateTools;
        private Mock<IValidateContent> mockValidateContent;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentKeyGenerator = this.mockRepository.Create<IDocumentKeyGenerator>();
            this.mockBlobContainerClient = this.mockRepository.Create<IBlobContainerClient<DocumentStore>>();
            this.mockDocumentTypeResolver = this.mockRepository.Create<IDocumentTypeResolver>();
            this.mockDateTools = this.mockRepository.Create<IDateTools>();
            this.mockValidateContent = this.mockRepository.Create<IValidateContent>();
        }

        private DocumentStore CreateDocumentStore() =>
            new DocumentStore(
                this.mockDocumentKeyGenerator.Object,
                this.mockBlobContainerClient.Object,
                this.TestContext.GetTestLoggingServices<DocumentStore>(),
                this.mockDocumentTypeResolver.Object,
                this.mockDateTools.Object,
                this.mockValidateContent.Object
                );

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task GetAsyncTest_Key()
        {
            // Stage
            string key = "test key";

            var testData = new
            {
                ContainerName = "test ContainerName data",
                docType = DocumentTypes.Jpeg,
                contentType = "test content type",
                data = new byte[] { 1, 2, 3 },
            };

            // Mock
            var mockContent = mockRepository.Create<IBlobContentResult>();
            mockContent.Setup(s => s.ContentType).Returns(testData.contentType);
            mockContent.Setup(s => s.Content).Returns(testData.data);
            mockBlobContainerClient.Setup(s => s.ContainerName).Returns(testData.ContainerName);
            mockBlobContainerClient.Setup(s => s.DownloadAsync(key)).ReturnsAsync(mockContent.Object);
            mockDocumentTypeResolver.Setup(s => s.GetByMimeOrFileName(mockContent.Object.ContentType, key)).Returns(testData.docType);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.GetAsync(key);

            // Assert
            CollectionAssert.AreEqual(testData.data, result.Content);
            Assert.AreEqual(testData.contentType, result.ContentType);
            Assert.AreEqual(testData.docType, result.DocumentType);
            Assert.AreEqual(key, result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(FileNotFoundException))]
        public async Task GetAsyncTest_Key_NotFound()
        {
            // Stage
            string key = "test key";
            IBlobContentResult content = null;

            var testData = new
            {
                ContainerName = "test ContainerName data",
            };

            // Mock
            mockBlobContainerClient.Setup(s => s.ContainerName).Returns(testData.ContainerName);
            mockBlobContainerClient.Setup(s => s.DownloadAsync(key)).ReturnsAsync(content);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.GetAsync(key);

            // Assert
            Assert.Fail("you shouldn't get here");

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task GetAsyncTest_KeyContainer()
        {
            // Stage
            string key = "test key";
            string container = "test container";

            var testData = new
            {
                ContainerName = "test ContainerName data",
                docType = DocumentTypes.Jpeg,
                contentType = "test content type",
                data = new byte[] { 1, 2, 3 },
            };

            // Mock
            var mockContent = mockRepository.Create<IBlobContentResult>();
            mockContent.Setup(s => s.ContentType).Returns(testData.contentType);
            mockContent.Setup(s => s.Content).Returns(testData.data);
            mockBlobContainerClient.Setup(s => s.ContainerName).Returns(testData.ContainerName);
            mockBlobContainerClient.Setup(s => s.DownloadAsync(key)).ReturnsAsync(mockContent.Object);
            mockDocumentTypeResolver.Setup(s => s.GetByMimeOrFileName(mockContent.Object.ContentType, key)).Returns(testData.docType);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.GetAsync(key, container);

            // Assert
            CollectionAssert.AreEqual(testData.data, result.Content);
            Assert.AreEqual(testData.contentType, result.ContentType);
            Assert.AreEqual(testData.docType, result.DocumentType);
            Assert.AreEqual(key, result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }

        /*
   private async Task<IDocumentStoreResult> StoreAsyncInternal(string fileName, Stream content, string contentType)
            var timeStamp = DateTimeOffset.Now;

            if (string.IsNullOrWhiteSpace(fileName))
            {
                fileName = _resolver.GenerateFileName(contentType);
            }
            if (string.IsNullOrWhiteSpace(contentType))
            {
                contentType = _resolver.GetMimeType(_resolver.GetByFileName(fileName));
            }

            catch (Exception ex)
            {
                await _event.SendAsync("Error Storing File", new
                {
                    fileName,
                    TimeStamp = timeStamp,
                    timeStamp.Ticks,
                    Error = ex.Message,
                }).ConfigureAwait(false);
                throw;
            }
        }
    }
        */


        private (string key, string container) MockSuccessTest(
            DocumentTypes? documentType,
            string contentType,
            string fileName,
            Exception throwOnGenerateKey = null
            )
        {
            var date = DateTimeOffset.Now;

            var container = "test container";

            if (documentType.HasValue)
            {
                mockDocumentTypeResolver.Setup(s => s.GetMimeType(documentType.Value)).Returns(contentType = documentType.ToString());
            }
            else if (contentType == null)
            {
                contentType = "test content type";
                mockDocumentTypeResolver.Setup(s => s.GetByFileName(fileName)).Returns(DocumentTypes.Png);
                mockDocumentTypeResolver.Setup(s => s.GetMimeType(DocumentTypes.Png)).Returns(contentType);
            }
            if (fileName == null)
            {
                fileName = "test file";
                mockDocumentTypeResolver.Setup(s => s.GenerateFileName(contentType)).Returns(fileName);
            }

            mockValidateContent.Setup(s => s.EnsureValidContentAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>())).Returns(Task.CompletedTask);
            mockDateTools.Setup(s => s.Now()).Returns(date);

            var key = Guid.NewGuid().ToString();

            if (throwOnGenerateKey == null)
            {
                mockDocumentKeyGenerator.Setup(s => s.Generate(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Returns(key);

                mockBlobContainerClient.Setup(s => s.UploadAsync(It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>())).Returns(Task.FromResult(0));

                mockBlobContainerClient.Setup(s => s.ContainerName).Returns(container);
            }
            else
            {
                mockDocumentKeyGenerator.Setup(s => s.Generate(It.IsAny<string>(), It.IsAny<DateTimeOffset>())).Throws(throwOnGenerateKey);
            }

            return (key, container);
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_NullContent()
        {
            // Stage
            byte[] content = null;
            var contentType = DocumentTypes.Pdf;

            // Mock
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(contentType)).Returns(contentType.ToString());

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(content, contentType);

            // Assert
            Assert.IsNull(result);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_BytesDocumentType()
        {
            // Stage
            var content = new byte[] { 1, 2, 3 };
            var contentType = DocumentTypes.Pdf;

            // Mock
            var mocked = MockSuccessTest(contentType, null, null);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_StreamDocumentType()
        {
            // Stage
            var content = new MemoryStream(new byte[] { 1, 2, 3 });
            var contentType = DocumentTypes.Pdf;

            // Mock
            var mocked = MockSuccessTest(contentType, null, null);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_BytesContentType()
        {
            // Stage
            var content = new byte[] { 1, 2, 3 };
            string contentType = "test content type";

            // Mock
            var mocked = MockSuccessTest(null, contentType, null);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_StreamContentTypes()
        {
            // Stage
            var content = new MemoryStream(new byte[] { 1, 2, 3 });
            string contentType = "test content type";

            // Mock
            var mocked = MockSuccessTest(null, contentType, null);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_FileNameBytes()
        {
            // Stage
            string fileName = "test file name";
            var content = new byte[] { 1, 2, 3 };

            // Mock
            var mocked = MockSuccessTest(null, null, fileName);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(fileName, content);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_FileNameStream()
        {
            // Stage
            string fileName = "test file name";
            var content = new MemoryStream(new byte[] { 1, 2, 3 });

            // Mock
            var mocked = MockSuccessTest(null, null, fileName);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(fileName, content);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_FilenameBytesContentType()
        {
            // Stage
            string fileName = "test file";
            var content = new byte[] { 1, 2, 3 };
            string contentType = "test content type";

            // Mock
            var mocked = MockSuccessTest(null, contentType, fileName);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(fileName, content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_FilenameStreamContentType()
        {
            // Stage
            string fileName = "test file name";
            var content = new MemoryStream(new byte[] { 1, 2, 3 });
            string contentType = "test content type";

            // Mock
            var mocked = MockSuccessTest(null, contentType, fileName);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(fileName, content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_FileNameBytesDocType()
        {
            // Stage
            string fileName = "test file";
            var content = new byte[] { 1, 2, 3 };
            var contentType = DocumentTypes.Pdf;

            // Mock
            var mocked = MockSuccessTest(contentType, null, fileName);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(fileName, content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsyncTest_FileNameStreamDocType()
        {
            // Stage
            string fileName = "test file";
            var content = new MemoryStream(new byte[] { 1, 2, 3 });
            var contentType = DocumentTypes.Pdf;

            // Mock
            var mocked = MockSuccessTest(contentType, null, fileName);

            // Test
            var documentStore = this.CreateDocumentStore();
            var result = await documentStore.StoreAsync(fileName, content, contentType);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(mocked.container, result.Container);
            Assert.AreEqual(mocked.key, result.Key);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        [ExpectedException(typeof(ApplicationException))]
        public async Task StoreAsyncTest_Error()
        {
            // Stage
            string fileName = "test file";
            var content = new MemoryStream(new byte[] { 1, 2, 3 });
            var contentType = DocumentTypes.Pdf;

            // Mock
            _ = MockSuccessTest(contentType, null, fileName, new ApplicationException());

            // Test
            var documentStore = this.CreateDocumentStore();
            _ = await documentStore.StoreAsync(fileName, content, contentType);

            // Assert
            Assert.Fail("you shouldn't get here");

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
