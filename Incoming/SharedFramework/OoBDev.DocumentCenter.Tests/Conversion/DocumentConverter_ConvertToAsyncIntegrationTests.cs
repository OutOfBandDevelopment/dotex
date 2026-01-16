using OoBDev.DocumentCenter.Contracts;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static OoBDev.DocumentCenter.Contracts.DocumentTypes;
using static OoBDev.TestUtilities.TestCategories;

namespace OoBDev.DocumentCenter.Tests.Conversion
{
    [TestClass]
    public class DocumentConverter_ConvertToAsyncIntegrationTests
    {
        public TestContext TestContext { get; set; }
        private static byte[] TestData => Encoding.UTF8.GetBytes("Hello World!");
        private static Stream TestStream => new MemoryStream(TestData) { Position = 0 };

        [TestMethod, TestCategory(TestCategories.Integration), Ignore]
        public async Task ConvertToAsync_Test_string_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync("dPdNIZJyFkKDrxxb2-grvQ_637286349959569083.txt", Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }
        [TestMethod, TestCategory(TestCategories.Integration), Ignore]
        public async Task ConvertToAsync_Test_string_string_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync("dPdNIZJyFkKDrxxb2-grvQ_637286349959569083.txt", "test-container", Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAsync_Test_DocumentTypes_byteArray_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync(Text, TestData, Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAsync_Test_DocumentTypes_Stream_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync(Text, TestStream, Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAsync_Test_string_byteArray_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync("test.txt", TestData, Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAsync_Test_string_Stream_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync("test.txt", TestStream, Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAsync_Test_string_byteArray_string_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync("test.txt", TestData, "text/plain", Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task ConvertToAsync_Test_string_Stream_string_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentConverter>();
            var result = await store.ConvertToAsync("test.txt", TestStream, "text/plain", Pdf);
            this.TestContext?.WriteLine($"ContentType: {result.ContentType}");
            this.TestContext?.WriteLine($"DocumentType: {result.DocumentType}");
            this.TestContext?.WriteLine($"FileName: {result.FileName}");
            this.TestContext?.WriteLine($"ContentLength: {result.Content.Length}");
            this.TestContext.AddResultFile(result.FileName, result.Content);
        }
    }
}
