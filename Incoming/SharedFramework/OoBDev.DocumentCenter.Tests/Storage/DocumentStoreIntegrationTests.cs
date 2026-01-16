using OoBDev.DocumentCenter.Contracts;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using static OoBDev.DocumentCenter.Contracts.DocumentTypes;

namespace OoBDev.DocumentCenter.Tests.Storage
{
    [TestClass]
    public class DocumentStoreIntegrationTests
    {
        public TestContext TestContext { get; set; }

        private static byte[] TestData => Encoding.UTF8.GetBytes("Hello World!");
        private static Stream TestStream => new MemoryStream(TestData) { Position = 0 };

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_ByteArray_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(TestData, Text);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }
        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_Stream_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(TestStream, Text);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_ByteArray_string()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(TestData, "text/plain; charset=utf-8");
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }
        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_Stream_string()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(TestStream, "text/plain; charset=utf-8");
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_string_ByteArray()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(this.TestContext.TestName + ".bin", TestData);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }
        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_string_Stream()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(this.TestContext.TestName + ".bin", TestStream);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_string_ByteArray_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(this.TestContext.TestName, TestData, Text);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }
        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_string_Stream_DocumentTypes()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(this.TestContext.TestName, TestStream, Text);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_string_ByteArray_string()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(this.TestContext.TestName, TestData, "text/plain; charset=utf-8");
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }
        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task StoreAsyncTest_string_Stream_string()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(this.TestContext.TestName, TestStream, "text/plain; charset=utf-8");
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));
        }

        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task GetAsyncTest_Key()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(TestData, Text).ConfigureAwait(false);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));

            this.TestContext?.WriteLine(new string('-', 10));
            var file = await store.GetAsync(result.Key).ConfigureAwait(false);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(file));

            var resultFile = Path.Combine(this.TestContext.TestRunResultsDirectory, file.FileName);
            await File.WriteAllBytesAsync(resultFile, file.Content);
            this.TestContext.AddResultFile(resultFile);

            this.TestContext?.WriteLine(new string('-', 10));
            this.TestContext?.WriteLine(Encoding.UTF8.GetString(file.Content));
        }
        [TestMethod, TestCategory(TestCategories.Integration)]
        public async Task GetAsyncTest_Key_Container()
        {
            var store = this.TestContext.GetService<IDocumentStore>();
            var result = await store.StoreAsync(TestData, Text).ConfigureAwait(false);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(result));

            this.TestContext?.WriteLine(new string('-', 10));
            var file = await store.GetAsync(result.Key, result.Container).ConfigureAwait(false);
            this.TestContext?.WriteLine(JsonConvert.SerializeObject(file));

            var resultFile = Path.Combine(this.TestContext.TestRunResultsDirectory, file.FileName);
            await File.WriteAllBytesAsync(resultFile, file.Content);
            this.TestContext.AddResultFile(resultFile);

            this.TestContext?.WriteLine(new string('-', 10));
            this.TestContext?.WriteLine(Encoding.UTF8.GetString(file.Content));
        }
    }
}
