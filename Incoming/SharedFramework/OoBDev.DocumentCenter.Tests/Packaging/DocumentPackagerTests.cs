using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Providers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Packaging;
using OoBDev.TestUtilities;
using OoBDev.Toolkit.Contracts.Common;
using OoBDev.Toolkit.Contracts.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Tests.Packaging
{
    [TestClass]
    public class DocumentPackagerTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentTypeResolver> mockDocumentTypeResolver;
        private Mock<IDocumentPackageProvider> mockDocumentPackageProvider;
        private Mock<IDateTools> mockDateTools;
        private Mock<ITempFileFactory> mockTempFileFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentTypeResolver = this.mockRepository.Create<IDocumentTypeResolver>();
            this.mockDocumentPackageProvider = this.mockRepository.Create<IDocumentPackageProvider>();
            this.mockDateTools = this.mockRepository.Create<IDateTools>();
            this.mockTempFileFactory = this.mockRepository.Create<ITempFileFactory>();
        }

        private DocumentPackager CreateDocumentPackager()
        {
            return new DocumentPackager(
                this.mockDocumentTypeResolver.Object,
                this.mockDocumentPackageProvider.Object,
                this.mockDateTools.Object,
                this.mockTempFileFactory.Object);
        }

        internal class TestDocument : IDocumentRequestReference
        {
            public string Key { get; set; }
            public string Container { get; set; }
            public string FileName { get; set; }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackToAsyncTest_SetOf_IDocumentRequestReference()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var documentType = DocumentTypes.Zip;
            var request = new[]
            {
                new TestDocument{ }
            };

            var content = Encoding.UTF8.GetBytes("Hello World!");
            var ext = ".text";
            var contentType = "app/thing";
            var date = new DateTime(1, 2, 3, 4, 5, 6);

            // Mock
            mockDocumentTypeResolver.Setup(s => s.GetByPackageType(packageType)).Returns(documentType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(documentType)).Returns(ext);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(documentType)).Returns(contentType);
            mockDateTools.Setup(s => s.Now()).Returns(date);
            mockDocumentPackageProvider.Setup(s => s.PackageAsync(
                It.Is<PackageTypes>(i => i == packageType),
                It.Is<IEnumerable<IDocumentRequestReference>>(i => i == request)
                 )).ReturnsAsync(content);

            // Test
            var documentPackager = this.CreateDocumentPackager();
            var result = await documentPackager.PackToAsync(packageType, request);

            // Assert
            Assert.AreEqual("SGVsbG8gV29ybGQh", Convert.ToBase64String(result.Content));
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(documentType, result.DocumentType);
            Assert.AreEqual("00010203040506.text", result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackToAsyncTest_SetOf_Keys()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var documentType = DocumentTypes.Zip;
            var request = new[]
            {
                "Key1"
            };

            var content = Encoding.UTF8.GetBytes("Hello World!");
            var ext = ".text";
            var contentType = "app/thing";
            var date = new DateTime(1, 2, 3, 4, 5, 6);

            // Mock
            mockDocumentTypeResolver.Setup(s => s.GetByPackageType(packageType)).Returns(documentType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(documentType)).Returns(ext);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(documentType)).Returns(contentType);
            mockDateTools.Setup(s => s.Now()).Returns(date);
            mockDocumentPackageProvider.Setup(s => s.PackageAsync(
                It.Is<PackageTypes>(i => i == packageType),
                It.Is<IEnumerable<IDocumentRequestReference>>(i => i.All(e => e.Key == request[0] && e.Container == null && e.FileName == null))
                 )).ReturnsAsync(content);

            // Test
            var documentPackager = this.CreateDocumentPackager();
            var result = await documentPackager.PackToAsync(packageType, request);

            // Assert
            Assert.AreEqual("SGVsbG8gV29ybGQh", Convert.ToBase64String(result.Content));
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(documentType, result.DocumentType);
            Assert.AreEqual("00010203040506.text", result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackToAsyncTest_Container_And_SetOf_Keys()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var documentType = DocumentTypes.Zip;
            var request = new[]
            {
                "Key1"
            };
            var container = "test container";

            var content = Encoding.UTF8.GetBytes("Hello World!");
            var ext = ".text";
            var contentType = "app/thing";
            var date = new DateTime(1, 2, 3, 4, 5, 6);

            // Mock
            mockDocumentTypeResolver.Setup(s => s.GetByPackageType(packageType)).Returns(documentType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(documentType)).Returns(ext);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(documentType)).Returns(contentType);
            mockDateTools.Setup(s => s.Now()).Returns(date);
            mockDocumentPackageProvider.Setup(s => s.PackageAsync(
                It.Is<PackageTypes>(i => i == packageType),
                It.Is<IEnumerable<IDocumentRequestReference>>(i => i.All(e => e.Key == request[0] && e.Container == container && e.FileName == null))
                 )).ReturnsAsync(content);

            // Test
            var documentPackager = this.CreateDocumentPackager();
            var result = await documentPackager.PackToAsync(packageType, container, request);

            // Assert
            Assert.AreEqual("SGVsbG8gV29ybGQh", Convert.ToBase64String(result.Content));
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(documentType, result.DocumentType);
            Assert.AreEqual("00010203040506.text", result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackToAsyncTest_SetOf_Key_Container()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var documentType = DocumentTypes.Zip;
            var request = new[]
            {
                (key: "Key1", container: "test container")
            };

            var content = Encoding.UTF8.GetBytes("Hello World!");
            var ext = ".text";
            var contentType = "app/thing";
            var date = new DateTime(1, 2, 3, 4, 5, 6);

            // Mock
            mockDocumentTypeResolver.Setup(s => s.GetByPackageType(packageType)).Returns(documentType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(documentType)).Returns(ext);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(documentType)).Returns(contentType);
            mockDateTools.Setup(s => s.Now()).Returns(date);
            mockDocumentPackageProvider.Setup(s => s.PackageAsync(
                It.Is<PackageTypes>(i => i == packageType),
                It.Is<IEnumerable<IDocumentRequestReference>>(i => i.All(e => e.Key == request[0].key && e.Container == request[0].container && e.FileName == null))
                 )).ReturnsAsync(content);

            // Test
            var documentPackager = this.CreateDocumentPackager();
            var result = await documentPackager.PackToAsync(packageType, request);

            // Assert
            Assert.AreEqual("SGVsbG8gV29ybGQh", Convert.ToBase64String(result.Content));
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(documentType, result.DocumentType);
            Assert.AreEqual("00010203040506.text", result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }


        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackToAsyncTest_SetOf_Key_Container_File()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var documentType = DocumentTypes.Zip;
            var request = new[]
            {
                (key: "Key1", container: "test container", file: "test file")
            };

            var content = Encoding.UTF8.GetBytes("Hello World!");
            var ext = ".text";
            var contentType = "app/thing";
            var date = new DateTime(1, 2, 3, 4, 5, 6);

            // Mock
            mockDocumentTypeResolver.Setup(s => s.GetByPackageType(packageType)).Returns(documentType);
            mockDocumentTypeResolver.Setup(s => s.GetExtension(documentType)).Returns(ext);
            mockDocumentTypeResolver.Setup(s => s.GetMimeType(documentType)).Returns(contentType);
            mockDateTools.Setup(s => s.Now()).Returns(date);
            mockDocumentPackageProvider.Setup(s => s.PackageAsync(
                It.Is<PackageTypes>(i => i == packageType),
                It.Is<IEnumerable<IDocumentRequestReference>>(i => i.All(e => e.Key == request[0].key && e.Container == request[0].container && e.FileName == request[0].file))
                 )).ReturnsAsync(content);

            // Test
            var documentPackager = this.CreateDocumentPackager();
            var result = await documentPackager.PackToAsync(packageType, request);

            // Assert
            Assert.AreEqual("SGVsbG8gV29ybGQh", Convert.ToBase64String(result.Content));
            Assert.AreEqual(contentType, result.ContentType);
            Assert.AreEqual(documentType, result.DocumentType);
            Assert.AreEqual("00010203040506.text", result.FileName);

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
