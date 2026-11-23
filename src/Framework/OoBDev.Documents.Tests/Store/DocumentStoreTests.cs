using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Models;
using OoBDev.Documents.Store;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Tests.Store;

[TestClass]
public class DocumentStoreTests
{
    public required TestContext TestContext { get; set; }

    private static ContentReference Doc(string text, string name = "Report.PDF") =>
        new() { Content = new MemoryStream(Encoding.UTF8.GetBytes(text)), ContentType = "application/pdf", FileName = name };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_ThenRead_ReturnsTheSameContent()
    {
        // Stage
        var container = new MemoryContainer();
        var store = new DocumentStore(container, new DocumentKeyGenerator(new FakeTimeProvider(new DateTimeOffset(2026, 11, 23, 0, 0, 0, TimeSpan.Zero))), new BypassContentValidation());

        // Test
        var key = await store.StoreAsync(Doc("hello"));
        TestContext.WriteLine("Key: {0}", key);
        var read = await store.ReadAsync(key);

        // Assert
        StringAssert.StartsWith(key, "2026/11/");
        StringAssert.EndsWith(key, ".pdf");
        using var reader = new StreamReader(read!.Content);
        Assert.AreEqual("hello", await reader.ReadToEndAsync());
        Assert.IsNull(await store.ReadAsync("2026/11/missing.pdf"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_ValidationFails_StoresNothing()
    {
        var container = new MemoryContainer();
        var store = new DocumentStore(container, new DocumentKeyGenerator(TimeProvider.System), new Refuse());
        await Assert.ThrowsExactlyAsync<InvalidContentException>(() => store.StoreAsync(Doc("bad")));
        Assert.AreEqual(0, container.Items.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_ValidatorReadsTheStream_StillStoresTheWholeContent()
    {
        var container = new MemoryContainer();
        var store = new DocumentStore(container, new DocumentKeyGenerator(TimeProvider.System), new ReadAll());
        var key = await store.StoreAsync(Doc("whole"));
        using var reader = new StreamReader((await store.ReadAsync(key))!.Content);
        Assert.AreEqual("whole", await reader.ReadToEndAsync());
    }

    private sealed class Refuse : IValidateContent
    {
        public Task ValidateAsync(ContentReference content) => throw new InvalidContentException("no");
    }

    private sealed class ReadAll : IValidateContent
    {
        public async Task ValidateAsync(ContentReference content) => await new StreamReader(content.Content, leaveOpen: true).ReadToEndAsync();
    }

    private sealed class MemoryContainer : IBlobContainer
    {
        public Dictionary<string, byte[]> Items { get; } = [];

        public Task<ContentReference?> GetContentAsync(string path) => Task.FromResult<ContentReference?>(Items.TryGetValue(path, out var b)
            ? new ContentReference { Content = new MemoryStream(b), ContentType = "application/octet-stream", FileName = path }
            : null);

        public Task StoreContentAsync(ContentReference reference, IDictionary<string, string>? metadata = null, bool overwrite = false)
        {
            using var copy = new MemoryStream();
            reference.Content.CopyTo(copy);
            Items[reference.FileName] = copy.ToArray();
            return Task.CompletedTask;
        }

        public Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path) => throw new NotSupportedException();

        public Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference) => throw new NotSupportedException();

        public IQueryable<ContentMetaDataReference> QueryContent() => throw new NotSupportedException();

        public Task DeleteContentAsync(string path) => Task.FromResult(Items.Remove(path));
    }
}
