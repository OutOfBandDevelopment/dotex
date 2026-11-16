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
    private sealed class FakeProvider(string[] filters, params SearchItem[] items) : ISearchProvider
    {
        public int Calls { get; private set; }

        public IReadOnlySet<string> SupportedFilters { get; } = filters.ToHashSet();

        public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(new SearchResult(items));
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
        Assert.IsNull(result.NextCursor);
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
    public async Task SearchAsync_Cursor_IsNotSupported()
    {
        var hybrid = new HybridSearchProvider(new Dictionary<string, ISearchProvider> { ["a"] = new FakeProvider([]) }, new MaxNormalisedFusion());

        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => hybrid.SearchAsync(new SearchRequest { Cursor = "x" }));
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
