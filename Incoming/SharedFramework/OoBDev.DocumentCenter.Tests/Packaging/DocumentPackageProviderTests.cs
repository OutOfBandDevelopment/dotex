using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Contracts.Handlers;
using OoBDev.DocumentCenter.Contracts.Storage;
using OoBDev.DocumentCenter.Packaging;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.DocumentCenter.Tests.Packaging
{
    [TestClass]
    public class DocumentPackageProviderTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentPackageHandlerResolver> mockDocumentPackageHandlerResolver;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentPackageHandlerResolver = this.mockRepository.Create<IDocumentPackageHandlerResolver>();
        }

        private DocumentPackageProvider CreateProvider() => new DocumentPackageProvider(this.mockDocumentPackageHandlerResolver.Object);

        internal class TestRef : IDocumentRequestReference
        {
            public string Key { get; set; }
            public string Container { get; set; }
            public string FileName { get; set; }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackageAsyncTest()
        {
            // Stage
            var packageType = PackageTypes.ZipFile;
            var documents = new[] { new TestRef() };
            var content = Encoding.UTF8.GetBytes("hello world!");

            // Mock
            var mockHandler = mockRepository.Create<IDocumentPackageHandler>();
            mockHandler.Setup(s => s.PackageAsync(documents)).ReturnsAsync(content);
            mockDocumentPackageHandlerResolver.Setup(s => s.GetHandler(packageType)).Returns(mockHandler.Object);

            // Test
            var provider = this.CreateProvider();
            var result = await provider.PackageAsync(packageType, documents);

            // Assert
            Assert.AreEqual("aGVsbG8gd29ybGQh", Convert.ToBase64String(result));

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
