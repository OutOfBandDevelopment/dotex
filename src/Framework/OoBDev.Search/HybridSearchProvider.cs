using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// Runs a request against several providers and fuses their rankings with an <see cref="IScoreFusion"/>.
/// </summary>
/// <remarks>
/// A filter must be supported by every source (<see cref="SupportedFilters"/> is the intersection), otherwise <see cref="UnsupportedFilterException"/> is thrown before any source is called.
/// Hybrid results are one page: the cursor is not supported and <see cref="SearchResult.NextCursor"/> is always <see langword="null"/>.
/// </remarks>
/// <param name="sources">The named providers, for example <c>keyword</c> and <c>vector</c>.</param>
/// <param name="fusion">The fusion strategy.</param>
public sealed class HybridSearchProvider(IReadOnlyDictionary<string, ISearchProvider> sources, IScoreFusion fusion) : ISearchProvider
{
    /// <inheritdoc />
    public IReadOnlySet<string> SupportedFilters { get; } = sources.Values
        .Select(s => s.SupportedFilters)
        .Aggregate((IEnumerable<string>?)null, (all, next) => all is null ? next : all.Intersect(next, StringComparer.Ordinal))
        ?.ToHashSet(StringComparer.Ordinal) ?? [];

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        foreach (var filter in request.Filters)
            if (!SupportedFilters.Contains(filter.Name))
                throw new UnsupportedFilterException(filter.Name);
        if (request.Cursor is not null)
            throw new NotSupportedException("A hybrid search returns one page and has no cursor.");

        var calls = sources.Select(async pair => new FusionInput(pair.Key, (await pair.Value.SearchAsync(request, cancellationToken).ConfigureAwait(false)).Items)).ToArray();
        var inputs = await Task.WhenAll(calls).ConfigureAwait(false);
        return new SearchResult(fusion.Fuse(inputs).Take(request.PageSize).ToArray());
    }
}
