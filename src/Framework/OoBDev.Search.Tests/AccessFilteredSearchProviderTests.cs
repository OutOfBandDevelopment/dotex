using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Search.Requests;
using OoBDev.TestUtilities;

namespace OoBDev.Search.Tests;

[TestClass]
public class AccessFilteredSearchProviderTests
{
    public TestContext TestContext { get; set; } = null!;

    private sealed class Inner(params string[] ids) : ISearchProvider
    {
        public int AskedPageSize { get; private set; }

        public IReadOnlySet<string> SupportedFilters { get; } = new HashSet<string> { "ext" };

        public Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
        {
            AskedPageSize = request.PageSize;
            return Task.FromResult(new SearchResult(ids.Select((id, i) => new SearchItem(id, 10 - i, new SearchExplanation())).ToArray(), "inner-cursor"));
        }
    }

    private sealed class Policy(params string[] readable) : IItemAccessPolicy
    {
        public Task<IReadOnlySet<string>> FilterReadableAsync(IReadOnlyCollection<string> itemIds, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlySet<string>>(itemIds.Where(readable.Contains).ToHashSet());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_HidesUnreadableHitsAndKeepsRank()
    {
        var inner = new Inner("a", "secret", "b", "c");
        var provider = new AccessFilteredSearchProvider(inner, new Policy("a", "b", "c"));

        var result = await provider.SearchAsync(new SearchRequest { Text = "x", PageSize = 2 }, TestContext.CancellationToken);
        TestContext.WriteLine(string.Join(",", result.Items.Select(i => i.ItemId)));

        CollectionAssert.AreEqual(new[] { "a", "b" }, result.Items.Select(i => i.ItemId).ToArray());
        Assert.AreEqual(6, inner.AskedPageSize);
        Assert.IsNull(result.NextCursor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_NothingReadable_ReturnsEmpty()
    {
        var provider = new AccessFilteredSearchProvider(new Inner("a", "b"), new Policy());

        var result = await provider.SearchAsync(new SearchRequest { Text = "x" }, TestContext.CancellationToken);

        Assert.AreEqual(0, result.Items.Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task SearchAsync_Cursor_IsNotSupported()
    {
        var provider = new AccessFilteredSearchProvider(new Inner("a"), new Policy("a"));

        await Assert.ThrowsExactlyAsync<NotSupportedException>(() => provider.SearchAsync(new SearchRequest { Cursor = "c" }, TestContext.CancellationToken));
    }
}
