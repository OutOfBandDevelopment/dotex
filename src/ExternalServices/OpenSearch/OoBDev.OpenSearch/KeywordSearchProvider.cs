using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Search;
using OoBDev.Search.Requests;
using SearchRequest = OoBDev.Search.Requests.SearchRequest;
using OpenSearch.Client;
using OpenSearch.Net;

namespace OoBDev.OpenSearch;

/// <summary>
/// Keyword search over an OpenSearch index: free text goes to a <c>simple_query_string</c>, filters to term, match and range clauses, a leading <c>-</c> in a sort key means descending.
/// The cursor is the offset of the next page.
/// </summary>
/// <param name="client">The low-level OpenSearch client.</param>
/// <param name="connection">The index to query.</param>
/// <param name="keyword">The keyword settings.</param>
public sealed class KeywordSearchProvider(IOpenSearchLowLevelClient client, IOptions<OpenSearchOptions> connection, IOptions<KeywordSearchOptions> keyword) : ISearchProvider
{
    /// <summary>The explanation field name of every hit.</summary>
    public const string Field = "keyword";

    private readonly KeywordSearchOptions settings = keyword.Value;

    /// <inheritdoc />
    public IReadOnlySet<string> SupportedFilters { get; } = keyword.Value.FilterFields.ToHashSet(StringComparer.Ordinal);

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        foreach (var filter in request.Filters.Where(f => !SupportedFilters.Contains(f.Name)))
        {
            throw new UnsupportedFilterException(filter.Name);
        }

        if (request.Anchor?.LikeItemId is not null || request.Anchor?.NearVector is not null)
        {
            throw new NotSupportedException("The keyword provider has no vectors; use the vector or hybrid provider.");
        }

        var offset = KeywordQueryBuilder.Offset(request.Cursor);
        var body = KeywordQueryBuilder.Build(request, settings.TextFields, offset);
        var response = await client
            .SearchAsync<StringResponse>(connection.Value.IndexName, PostData.String(body), ctx: cancellationToken)
            .ConfigureAwait(false);
        if (!response.Success)
        {
            throw new InvalidOperationException($"OpenSearch search failed: {response.DebugInformation}");
        }

        return Parse(response.Body, offset, request.PageSize);
    }

    private static SearchResult Parse(string body, int offset, int pageSize)
    {
        using var document = JsonDocument.Parse(body);
        var hits = document.RootElement.GetProperty("hits");
        var total = hits.GetProperty("total").GetProperty("value").GetInt64();
        var items = new List<SearchItem>();
        foreach (var hit in hits.GetProperty("hits").EnumerateArray())
        {
            var score = hit.TryGetProperty("_score", out var s) && s.ValueKind == JsonValueKind.Number ? s.GetDouble() : 0;
            items.Add(new SearchItem(hit.GetProperty("_id").GetString()!, score, new SearchExplanation(Field: Field), Source(hit)));
        }

        var next = offset + pageSize;
        return new SearchResult(items, next < total ? next.ToString(CultureInfo.InvariantCulture) : null);
    }

    private static Dictionary<string, object>? Source(JsonElement hit)
    {
        if (!hit.TryGetProperty("_source", out var source) || source.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        foreach (var property in source.EnumerateObject())
        {
            result[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString()!,
                JsonValueKind.Number => property.Value.TryGetInt64(out var whole) ? (object)whole : property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                _ => property.Value.GetRawText(),
            };
        }

        return result;
    }
}
