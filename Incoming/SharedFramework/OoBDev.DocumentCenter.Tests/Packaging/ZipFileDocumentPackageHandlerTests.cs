using OoBDev.TestUtilities;
using OoBDev.DocumentCenter.Contracts;
using OoBDev.DocumentCenter.Packaging;
using OoBDev.Toolkit.Contracts.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;
using OoBDev.DocumentCenter.Contracts.Storage;
using System.Text;
using OoBDev.Toolkit.IO;
using System.IO.Compression;
using System.Linq;

namespace OoBDev.DocumentCenter.Tests.Packaging
{
    [TestClass]
    public class ZipFileDocumentPackageHandlerTests
    {
        public TestContext TestContext { get; set; }

        private MockRepository mockRepository;

        private Mock<IDocumentStore> mockDocumentStore;
        private Mock<ITempFileFactory> mockTempFileFactory;

        [TestInitialize]
        public void TestInitialize()
        {
            this.mockRepository = new MockRepository(MockBehavior.Strict);

            this.mockDocumentStore = this.mockRepository.Create<IDocumentStore>();
            this.mockTempFileFactory = this.mockRepository.Create<ITempFileFactory>();
        }

        private ZipFileDocumentPackageHandler CreateZipFileDocumentPackageHandler()
        {
            return new ZipFileDocumentPackageHandler(
                this.mockDocumentStore.Object,
                this.mockTempFileFactory.Object
                );
        }

        internal class TestDocument : IDocumentRequestReference
        {
            public string Key { get; set; }
            public string Container { get; set; }
            public string FileName { get; set; }
            public bool Content { get; set; }
        }
        internal class TestContent : IDocumentContentResult
        {
            public byte[] Content { get; set; }
            public string ContentType { get; set; }
            public string FileName { get; set; }
            public DocumentTypes DocumentType { get; set; }
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task PackageAsyncTest()
        {
            // Stage
            var references = new[]
            {
                new TestDocument() { Key= "1", Container = "abc", FileName=null, Content = true },
                new TestDocument() { Key= "2", Container = "abc", FileName=null, Content = true },
                new TestDocument() { Key= "3", Container = "abc", FileName="root.txt", Content = true },
                new TestDocument() { Key= "4", Container = "abc", FileName="path/file.txt", Content = true },
                new TestDocument() { Key= "5", Container = "abc", FileName="path/file2.txt", Content = true },
                new TestDocument() { Key= "6", Container = "abc", FileName="path\\file3.txt", Content = true },
                new TestDocument() { Key= "7", Container = "abc", FileName="path\\file4.txt", Content = false },
            };
            var content = "hello world!";

            // Mock
            mockTempFileFactory.Setup(s => s.GetTempFile()).Returns(new TempFileHandle());
            foreach (var docref in references)
            {
                var contentResult = docref.Content ? new TestContent
                {
                    Content = Encoding.UTF8.GetBytes(content),
                    ContentType = "text/plain",
                    DocumentType = DocumentTypes.Text,
                    FileName = docref.FileName,
                } : null;
                mockDocumentStore.SetupSequence(s => s.GetAsync(docref.Key, docref.Container)).ReturnsAsync(contentResult);
            }

            // Test
            var zipFileDocumentPackageHandler = this.CreateZipFileDocumentPackageHandler();
            var result = await zipFileDocumentPackageHandler.PackageAsync(references);

            this.TestContext.AddResultFile("Result.zip", result, out var resultPath);

            // Assert
            using (var zip = ZipFile.OpenRead(resultPath))
            {
                Assert.AreEqual(references.Count(i=>i.Content), zip.Entries.Count);
                foreach (var e in zip.Entries)
                {
                    this.TestContext?.WriteLine($"{e.FullName}: {e.CompressedLength}/{e.Length}");
                }
            }

            // Verify
            this.mockRepository.VerifyAll();
        }
    }
}
