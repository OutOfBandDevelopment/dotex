using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// Wraps a provider and removes the hits the caller may not read (<see cref="IItemAccessPolicy"/>). The inner provider is asked for
/// <c>PageSize * overFetch</c> hits so a page can still be filled after hiding some; a page can be short when most hits are hidden.
/// Like the hybrid provider, the result is one page: a cursor is not supported and <see cref="SearchResult.NextCursor"/> is <see langword="null"/>.
/// </summary>
/// <param name="inner">The provider to filter.</param>
/// <param name="policy">The access policy.</param>
/// <param name="overFetch">How many times the page size to ask the inner provider for (at least 1).</param>
public sealed class AccessFilteredSearchProvider(ISearchProvider inner, IItemAccessPolicy policy, int overFetch = 3) : ISearchProvider
{
    /// <inheritdoc />
    public global::System.Collections.Generic.IReadOnlySet<string> SupportedFilters => inner.SupportedFilters;

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Cursor is not null)
            throw new NotSupportedException("An access-filtered search returns one page and has no cursor.");

        var found = await inner.SearchAsync(request with { PageSize = request.PageSize * Math.Max(1, overFetch) }, cancellationToken).ConfigureAwait(false);
        var readable = await policy.FilterReadableAsync(found.Items.Select(i => i.ItemId).Distinct(StringComparer.Ordinal).ToArray(), cancellationToken).ConfigureAwait(false);
        return new SearchResult(found.Items.Where(i => readable.Contains(i.ItemId)).Take(request.PageSize).ToArray());
    }
}
