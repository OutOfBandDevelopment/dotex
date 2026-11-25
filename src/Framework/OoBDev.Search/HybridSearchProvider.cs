using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// Runs a request against several providers and fuses their rankings with an <see cref="IScoreFusion"/>.
/// </summary>
/// <remarks>
/// A filter must be supported by every source (<see cref="SupportedFilters"/> is the intersection), otherwise <see cref="UnsupportedFilterException"/> is thrown before any source is called.
/// Paging is stateless: the cursor holds the offset into the fused ranking, and each page asks every source for the top
/// <c>offset + page size + 1</c> hits and fuses them again. The ranking is stable only while the sources are, and a page never reaches
/// past <c>maxDepth</c> fused hits (then <see cref="SearchResult.NextCursor"/> is <see langword="null"/>).
/// </remarks>
/// <param name="sources">The named providers, for example <c>keyword</c> and <c>vector</c>.</param>
/// <param name="fusion">The fusion strategy.</param>
/// <param name="maxDepth">The most fused hits a caller can page through; bounds the work each page asks of the sources.</param>
public sealed class HybridSearchProvider(IReadOnlyDictionary<string, ISearchProvider> sources, IScoreFusion fusion, int maxDepth = 200) : ISearchProvider
{
    private const string CursorPrefix = "hybrid:";

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

        var offset = ReadOffset(request.Cursor);
        var wanted = Math.Min(offset + request.PageSize, maxDepth);
        if (offset >= wanted)
            return new SearchResult([]);

        var sourceRequest = request with { Cursor = null, PageSize = wanted + 1 };
        var calls = sources.Select(async pair => new FusionInput(pair.Key, (await pair.Value.SearchAsync(sourceRequest, cancellationToken).ConfigureAwait(false)).Items)).ToArray();
        var inputs = await Task.WhenAll(calls).ConfigureAwait(false);
        var fused = fusion.Fuse(inputs).ToArray();
        var page = fused.Skip(offset).Take(wanted - offset).ToArray();
        var next = fused.Length > wanted && wanted < maxDepth ? WriteCursor(wanted) : null;
        return new SearchResult(page, next);
    }

    private static int ReadOffset(string? cursor)
    {
        if (cursor is null)
            return 0;
        try
        {
            var text = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            if (text.StartsWith(CursorPrefix, StringComparison.Ordinal) && int.TryParse(text.AsSpan(CursorPrefix.Length), out var offset) && offset >= 0)
                return offset;
        }
        catch (FormatException)
        {
        }

        throw new ArgumentException("The cursor was not made by a hybrid search.", nameof(cursor));
    }

    private static string WriteCursor(int offset) => Convert.ToBase64String(Encoding.UTF8.GetBytes(CursorPrefix + offset));
}
