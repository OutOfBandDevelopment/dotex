using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Fusion;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;

namespace OoBDev.Search.Tests;

[TestClass]
public class HybridSearchProviderTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class FakeProvider(string[] filters, params SearchItem[] items) : ISearchProvider
    {
        public int Calls { get; private set; }

        public IReadOnlySet<string> SupportedFilters { get; } = filters.ToHashSet();

        public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new SearchResult(items.Take(request.PageSize).ToArray()));
        }
    }

    private static SearchItem Item(string id, double score) => new(id, score, new SearchExplanation());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_TwoSources_FusesAndTrimsToPageSize()
    {
        var keyword = new FakeProvider(["ext"], Item("a", 10), Item("b", 5));
        var vector = new FakeProvider(["ext"], Item("b", 0.8), Item("c", 0.4));
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["keyword"] = keyword, ["vector"] = vector }, new MaxNormalisedFusion());

        var result = await hybrid.SearchAsync(SearchTextParser.Parse("ext:pdf top:2"));

        Assert.AreEqual(2, result.Items.Count);
        Assert.IsNotNull(result.NextCursor);
        Assert.AreEqual(1, keyword.Calls);
        Assert.AreEqual(1, vector.Calls);
        Assert.IsTrue(result.Items.All(i => i.Explanation.Fusion == FusionNames.MaxNormalised));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void SupportedFilters_IsIntersection()
    {
        var hybrid = new HybridSearchProvider(
            new Dictionary<string, ISearchProvider> { ["a"] = new FakeProvider(["x", "y"]), ["b"] = new FakeProvider(["y", "z"]) },
            new MaxNormalisedFusion());

        CollectionAssert.AreEquivalent(new[] { "y" }, hybrid.SupportedFilters.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_UnsupportedFilter_ThrowsNamingFilterNotValueAndCallsNoSource()
    {
        var source = new FakeProvider(["ext"]);
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["a"] = source }, new MaxNormalisedFusion());

        var ex = await Assert.ThrowsExactlyAsync<UnsupportedFilterException>(() => hybrid.SearchAsync(SearchTextParser.Parse("owner:secret-value")));

        Assert.AreEqual("owner", ex.Filter);
        Assert.IsFalse(ex.Message.Contains("secret-value", StringComparison.Ordinal));
        Assert.AreEqual(0, source.Calls);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_FollowingCursor_PagesThroughFusedResultsWithoutRepeats()
    {
        // Stage
        var keyword = new FakeProvider([], Item("a", 10), Item("b", 8), Item("c", 6), Item("d", 4), Item("e", 2));
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["keyword"] = keyword }, new MaxNormalisedFusion());
        var ids = new List<string>();

        // Test
        var request = new SearchRequest { PageSize = 2 };
        var pages = 0;
        while (true)
        {
            var page = await hybrid.SearchAsync(request);
            ids.AddRange(page.Items.Select(i => i.ItemId));
            pages++;
            TestContext.WriteLine($"page {pages}: {string.Join(",", page.Items.Select(i => i.ItemId))} next={page.NextCursor}");
            if (page.NextCursor is null)
                break;
            request = request with { Cursor = page.NextCursor };
        }

        // Assert
        CollectionAssert.AreEqual(new[] { "a", "b", "c", "d", "e" }, ids);
        Assert.AreEqual(3, pages);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_LastPageExactlyFull_HasNoCursor()
    {
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["a"] = new FakeProvider([], Item("a", 3), Item("b", 2)) }, new MaxNormalisedFusion());

        var result = await hybrid.SearchAsync(new SearchRequest { PageSize = 2 });

        Assert.AreEqual(2, result.Items.Count);
        Assert.IsNull(result.NextCursor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_PastMaxDepth_StopsPaging()
    {
        var items = Enumerable.Range(0, 10).Select(i => Item($"i{i}", 10 - i)).ToArray();
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["a"] = new FakeProvider([], items) }, new MaxNormalisedFusion(), maxDepth: 4);

        var first = await hybrid.SearchAsync(new SearchRequest { PageSize = 3 });
        var second = await hybrid.SearchAsync(new SearchRequest { PageSize = 3, Cursor = first.NextCursor });

        Assert.AreEqual(3, first.Items.Count);
        Assert.AreEqual(1, second.Items.Count);
        Assert.IsNull(second.NextCursor);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("x")]
    [DataRow("aGVsbG8=")]
    public async Task SearchAsync_ForeignCursor_Throws(string cursor)
    {
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["a"] = new FakeProvider([]) }, new MaxNormalisedFusion());

        await Assert.ThrowsExactlyAsync<ArgumentException>(() => hybrid.SearchAsync(new SearchRequest { Cursor = cursor }));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void SupportedFilters_NoSources_IsEmpty()
    {
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider>(), new MaxNormalisedFusion());

        Assert.AreEqual(0, hybrid.SupportedFilters.Count);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null, typeof(MaxNormalisedFusion))]
    [DataRow("reciprocal-rank", typeof(ReciprocalRankFusion))]
    public void TryAddSearchFusion_Strategy_PicksImplementation(string? strategy, Type expected)
    {
        var values = new Dictionary<string, string?>();
        if (strategy is not null)
            values["Search:Fusion:Strategy"] = strategy;
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(values).Build();
        var services = new ServiceCollection().TryAddSearchFusion(configuration);

        using var provider = services.BuildServiceProvider();

        Assert.IsInstanceOfType(provider.GetRequiredService<IScoreFusion>(), expected);
    }
}
