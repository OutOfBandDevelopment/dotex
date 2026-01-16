using OoBDev.DocumentCenter.Contracts;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static OoBDev.DocumentCenter.Contracts.DocumentTypes;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConverter_ConvertToAndStoreAsyncIntegrationTests
    {
        public TestContext TestContext { get; set; }
        private static byte[] TestData => Encoding.UTF8.GetBytes("Hello World!");
        private static Stream TestStream => new MemoryStream(TestData) { Position = 0 };

        [TestMethod, TestCategory(TestCategories.Integration), Ignore]
        public async Task ConvertToAndStoreAsync_Test_string_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync("dPdNIZJyFkKDrxxb2-grvQ_637286349959569083.txt", Pdf);
            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }
        [TestMethod, TestCategory(TestCategories.Integration), Ignore]
        public async Task ConvertToAndStoreAsync_Test_string_string_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync("dPdNIZJyFkKDrxxb2-grvQ_637286349959569083.txt", "test-container", Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAndStoreAsync_Test_DocumentTypes_byteArray_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync(Text, TestData, Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAndStoreAsync_Test_DocumentTypes_Stream_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync(Text, TestStream, Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAndStoreAsync_Test_string_byteArray_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync("test.txt", TestData, Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAndStoreAsync_Test_string_Stream_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync("test.txt", TestStream, Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAndStoreAsync_Test_string_byteArray_string_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync("test.txt", TestData, "text/plain", Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAndStoreAsync_Test_string_Stream_string_DocumentTypes()
        {
            var converter = this.TestContext.GetService<IDocumentConverter>();
            var result = await converter.ConvertToAndStoreAsync("test.txt", TestStream, "text/plain", Pdf);

            this.TestContext?.WriteLine($"Container: {result.Container}");
            this.TestContext?.WriteLine($"Key: {result.Key}");
            this.TestContext?.WriteLine(new string('-', 10));

            var store = this.TestContext.GetService<IDocumentStore>();
            var getback = await store.GetAsync(result.Key, result.Container);
            this.TestContext?.WriteLine($"ContentType: {getback.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {getback.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {getback.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {getback.Content.Length}");
            this.TestContext.AddResultFile(getback.FileName, getback.Content);
        }
    }
}
