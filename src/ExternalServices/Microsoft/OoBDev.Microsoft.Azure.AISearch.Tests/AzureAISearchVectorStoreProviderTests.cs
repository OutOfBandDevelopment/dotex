using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.AISearch.Tests;

[TestClass]
public class AzureAISearchVectorStoreProviderTests
{
    private static IOptions<AzureAISearchOptions> Options(bool ensure = false) =>
        global::Microsoft.Extensions.Options.Options.Create(new AzureAISearchOptions { Endpoint = "https://x.search.windows.net", IndexName = "docs", EnsureIndexExists = ensure, Top = 5 });

    private static Response<SearchResults<SearchDocument>> Results(params (SearchDocument Doc, double Score)[] hits) =>
        Response.FromValue(
            SearchModelFactory.SearchResults(
                hits.Select(h => SearchModelFactory.SearchResult(h.Doc, h.Score, null)).ToList(),
                hits.Length, null, null, Mock.Of<Response>()),
            Mock.Of<Response>());

    private static SearchDocument Doc(string id, string payload) =>
        new() { [AzureAISearchGlobals.IdField] = id, [AzureAISearchGlobals.PayloadField] = payload };

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreVectorsAsync_CreatesIndexAndUploadsDocuments()
    {
        // Stage
        var index = new Mock<SearchIndexClient>();
        var search = new Mock<SearchClient>();
        IReadOnlyList<SearchDocument>? uploaded = null;
        search.Setup(s => s.UploadDocumentsAsync(It.IsAny<IEnumerable<SearchDocument>>(), It.IsAny<IndexDocumentsOptions>(), It.IsAny<CancellationToken>()))
            .Callback((IEnumerable<SearchDocument> d, IndexDocumentsOptions? _, CancellationToken _) => uploaded = d.ToList())
            .ReturnsAsync(Mock.Of<Response<IndexDocumentsResult>>());
        var provider = new AzureAISearchVectorStoreProvider(index.Object, search.Object, Options(ensure: true), "docs");

        // Test
        var ids = await provider.StoreVectorsAsync(
            [(new float[] { 1, 2, 3 }, (IDictionary<string, object>)new Dictionary<string, object> { ["page"] = 1 })],
            new Dictionary<string, object> { ["file"] = "a.txt" });

        // Assert
        Assert.AreEqual(1, ids.Length);
        Assert.AreEqual(ids[0], (string)uploaded!.Single()[AzureAISearchGlobals.IdField]);
        index.Verify(i => i.CreateOrUpdateIndexAsync(It.Is<SearchIndex>(x => x.Fields.Any(f => f.VectorSearchDimensions == 3)), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreVectorsAsync_NoItems_DoesNothing()
    {
        // Stage
        var search = new Mock<SearchClient>(MockBehavior.Strict);
        var provider = new AzureAISearchVectorStoreProvider(new Mock<SearchIndexClient>().Object, search.Object, Options(), "docs");

        // Test
        var ids = await provider.StoreVectorsAsync(Array.Empty<ReadOnlyMemory<float>>(), new Dictionary<string, object>());

        // Assert
        Assert.AreEqual(0, ids.Length);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FindNeighborsAsync_MapsScoreAndMetadata()
    {
        // Stage
        var search = new Mock<SearchClient>();
        search.Setup(s => s.SearchAsync<SearchDocument>(null, It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Results((Doc("1", """{"file":"a"}"""), 0.9)));
        var provider = new AzureAISearchVectorStoreProvider(new Mock<SearchIndexClient>().Object, search.Object, Options(), "docs");

        // Test
        var found = await provider.FindNeighborsAsync(new float[] { 1 }).ToListAsync();

        // Assert
        Assert.AreEqual("1", found.Single().ItemId);
        Assert.AreEqual(0.9f, found.Single().Score);
        Assert.AreEqual("a", found.Single().MetaData!["file"]);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task FindNeighborsAsync_GroupBy_KeepsFirstPerGroup()
    {
        // Stage
        var search = new Mock<SearchClient>();
        search.Setup(s => s.SearchAsync<SearchDocument>(null, It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Results((Doc("1", """{"file":"a"}"""), 0.9), (Doc("2", """{"file":"a"}"""), 0.8), (Doc("3", """{"file":"b"}"""), 0.7)));
        var provider = new AzureAISearchVectorStoreProvider(new Mock<SearchIndexClient>().Object, search.Object, Options(), "docs");

        // Test
        var found = await provider.FindNeighborsAsync(new float[] { 1 }, "file").ToListAsync();

        // Assert
        CollectionAssert.AreEqual(new[] { "1", "3" }, found.Select(f => f.ItemId).ToArray());
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ListAsync_ReturnsAllDocuments()
    {
        // Stage
        var search = new Mock<SearchClient>();
        search.Setup(s => s.SearchAsync<SearchDocument>("*", It.IsAny<SearchOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Results((Doc("1", "{}"), 1), (Doc("2", "{}"), 1)));
        var provider = new AzureAISearchVectorStoreProvider(new Mock<SearchIndexClient>().Object, search.Object, Options(), "docs");

        // Test
        var all = await provider.ListAsync().ToListAsync();

        // Assert
        Assert.AreEqual(2, all.Count);
        Assert.AreEqual(0f, all[0].Score);
    }
}
