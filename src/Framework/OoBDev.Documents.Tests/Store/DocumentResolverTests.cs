using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using OoBDev.Documents.Store;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Tests.Store;

[TestClass]
public class DocumentResolverTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_ThenResolve_UsesTheNamedContainer()
    {
        // Stage
        var containers = new Containers();
        var resolver = new DocumentResolver(containers, new DocumentKeyGenerator(TimeProvider.System), new BypassContentValidation());
        var doc = new ContentReference { Content = new MemoryStream(Encoding.UTF8.GetBytes("hi")), ContentType = "text/plain", FileName = "a.txt" };

        // Test
        var reference = await resolver.StoreAsync("invoices", doc);
        TestContext.WriteLine("Reference: {0}/{1}", reference.Container, reference.Key);
        var read = await resolver.ResolveAsync(reference);

        // Assert
        Assert.AreEqual("invoices", reference.Container);
        Assert.AreEqual(1, containers.Items["invoices"].Count);
        Assert.AreEqual(0, containers.Items.GetValueOrDefault("other")?.Count ?? 0);
        using var reader = new StreamReader(read!.Content);
        Assert.AreEqual("hi", await reader.ReadToEndAsync());
        Assert.IsNull(await resolver.ResolveAsync(new DocumentReference("other", reference.Key)));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_BlankContainer_Throws()
    {
        var resolver = new DocumentResolver(new Containers(), new DocumentKeyGenerator(TimeProvider.System), new BypassContentValidation());
        var doc = new ContentReference { Content = new MemoryStream(), ContentType = "text/plain", FileName = "a.txt" };
        await Assert.ThrowsExactlyAsync<ArgumentException>(() => resolver.StoreAsync(" ", doc));
    }

    private sealed class Containers : IBlobContainerFactory
    {
        public Dictionary<string, Dictionary<string, byte[]>> Items { get; } = [];

        public IBlobContainer Create(string containerName) => new Container(Items.TryGetValue(containerName, out var d) ? d : Items[containerName] = []);

        public IBlobContainer<T> Create<T>() => throw new NotSupportedException();
    }

    private sealed class Container(Dictionary<string, byte[]> items) : IBlobContainer
    {
        public Task<ContentReference?> GetContentAsync(string path) => Task.FromResult<ContentReference?>(items.TryGetValue(path, out var b)
            ? new ContentReference { Content = new MemoryStream(b), ContentType = "application/octet-stream", FileName = path }
            : null);

        public Task StoreContentAsync(ContentReference reference, IDictionary<string, string>? metadata = null, bool overwrite = false)
        {
            using var copy = new MemoryStream();
            reference.Content.CopyTo(copy);
            items[reference.FileName] = copy.ToArray();
            return Task.CompletedTask;
        }

        public Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path) => throw new NotSupportedException();

        public Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference) => throw new NotSupportedException();

        public IQueryable<ContentMetaDataReference> QueryContent() => throw new NotSupportedException();

        public Task DeleteContentAsync(string path) => Task.FromResult(items.Remove(path));
    }
}
