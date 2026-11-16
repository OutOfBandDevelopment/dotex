using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AI;
using OoBDev.Search.Models;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;

namespace OoBDev.Search.Tests;

[TestClass]
public class VectorSearchProviderTests
{
    private static async IAsyncEnumerable<SearchResultModel> Hits(params (string Id, float Score)[] hits)
    {
        foreach (var (id, score) in hits)
        {
            await Task.Yield();
            yield return new SearchResultModel { ItemId = id, Score = score, MetaData = new Dictionary<string, object> { ["k"] = id } };
        }
    }

    private static (VectorSearchProvider Provider, Mock<IVectorStore> Store, Mock<IEmbeddingProvider> Embeddings) Create(params (string, float)[] hits)
    {
        var store = new Mock<IVectorStore>();
        store.Setup(s => s.FindNeighborsAsync(It.IsAny<ReadOnlyMemory<float>>())).Returns(() => Hits(hits));
        var embeddings = new Mock<IEmbeddingProvider>();
        embeddings.Setup(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ReadOnlyMemory<float>([1f, 2f]));
        return (new VectorSearchProvider(store.Object, embeddings.Object), store, embeddings);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_Text_EmbedsAndReturnsNeighbours()
    {
        var (provider, store, embeddings) = Create(("a", 0.9f), ("b", 0.5f));

        var result = await provider.SearchAsync(new SearchRequest { Text = "red car" });

        CollectionAssert.AreEqual(new[] { "a", "b" }, result.Items.Select(i => i.ItemId).ToArray());
        Assert.AreEqual("vector", result.Items[0].Explanation.Field);
        Assert.AreEqual("a", result.Items[0].MetaData!["k"]);
        embeddings.Verify(e => e.GenerateEmbeddingAsync("red car", null, It.IsAny<CancellationToken>()), Times.Once);
        store.Verify(s => s.FindNeighborsAsync(It.IsAny<ReadOnlyMemory<float>>()), Times.Once);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_PageSize_StopsAfterThatManyHits()
    {
        var (provider, _, _) = Create(("a", 0.9f), ("b", 0.5f), ("c", 0.1f));

        var result = await provider.SearchAsync(new SearchRequest { Text = "x", PageSize = 2 });

        Assert.AreEqual(2, result.Items.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_NearVector_SkipsEmbedding()
    {
        var (provider, _, embeddings) = Create(("a", 0.9f));
        var anchor = new SearchAnchor(NearVector: new ReadOnlyMemory<float>([3f]));

        var result = await provider.SearchAsync(new SearchRequest { Text = "ignored", Anchor = anchor });

        Assert.AreEqual(1, result.Items.Count);
        embeddings.Verify(e => e.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_NearText_IsEmbeddedInsteadOfText()
    {
        var (provider, _, embeddings) = Create(("a", 0.9f));

        await provider.SearchAsync(new SearchRequest { Text = "plain", Anchor = new SearchAnchor(NearText: "sunset") });

        embeddings.Verify(e => e.GenerateEmbeddingAsync("sunset", null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_NoText_ReturnsEmptyWithoutCallingTheStore()
    {
        var (provider, store, _) = Create(("a", 0.9f));

        var result = await provider.SearchAsync(new SearchRequest { Text = "  " });

        Assert.AreEqual(0, result.Items.Count);
        store.Verify(s => s.FindNeighborsAsync(It.IsAny<ReadOnlyMemory<float>>()), Times.Never);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_FilterCursorOrLikeAnchor_Refused()
    {
        var (provider, _, _) = Create();

        var filtered = new SearchRequest { Text = "x", Filters = [new FieldFilter("ext", FilterComparator.Equals, "pdf")] };
        var thrown = await Assert.ThrowsExactlyAsync<UnsupportedFilterException>(() => provider.SearchAsync(filtered));
        Assert.AreEqual("ext", thrown.Filter);
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(new SearchRequest { Text = "x", Cursor = "c" }));
        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(new SearchRequest { Text = "x", Anchor = new SearchAnchor(LikeItemId: "i") }));
        Assert.AreEqual(0, provider.SupportedFilters.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_NullRequest_Throws()
    {
        var (provider, _, _) = Create();

        await Assert.ThrowsExactlyAsync<ArgumentNullException>(() => provider.SearchAsync(null!));
    }
}
