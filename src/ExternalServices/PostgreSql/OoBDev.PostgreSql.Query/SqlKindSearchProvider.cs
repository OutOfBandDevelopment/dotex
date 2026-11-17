using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using OoBDev.Query.Metadata;
using OoBDev.Search;
using OoBDev.Search.Requests;

namespace OoBDev.PostgreSql.Query;

/// <summary>
/// Search over one metadata query kind: free text becomes the kind's search words, filters become facet operators,
/// the cursor is the engine's keyset cursor. Hits are unranked (score 0), so they fuse by rank.
/// </summary>
/// <param name="engine">The metadata query engine.</param>
/// <param name="options">The kind and id facet.</param>
public sealed class SqlKindSearchProvider(QueryEngine engine, IOptions<SqlKindSearchOptions> options) : ISearchProvider
{
    /// <summary>The explanation field name of every hit.</summary>
    public const string Field = "sql";

    private readonly SqlKindSearchOptions settings = options.Value;

    /// <inheritdoc />
    public IReadOnlySet<string> SupportedFilters { get; } = options.Value.Kind.Length == 0
        ? new HashSet<string>()
        : engine.Registry.Get(options.Value.Kind).Facets.Where(f => f.Filterable).Select(f => f.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public async Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Anchor?.LikeItemId is not null || request.Anchor?.NearVector is not null || request.Anchor?.NearText is not null)
        {
            throw new NotSupportedException("The SQL provider has no vectors; use the vector or hybrid provider.");
        }

        foreach (var filter in request.Filters.Where(f => !SupportedFilters.Contains(f.Name)))
        {
            throw new UnsupportedFilterException(filter.Name);
        }

        var kind = engine.Registry.Get(settings.Kind);
        var query = new QueryRequest
        {
            From = settings.Kind,
            Search = request.Text.Length == 0 ? null : request.Text,
            Filters = Filters(kind, request.Filters),
            Sort = request.Sort.Count == 0 ? null : [.. request.Sort],
            Top = request.PageSize,
            After = request.Cursor,
        };

        var result = await engine.QueryAsync(query, cancellationToken).ConfigureAwait(false);
        var items = result.Rows.Select(row => new SearchItem(
            Convert.ToString(row[settings.IdFacet], CultureInfo.InvariantCulture) ?? "",
            0,
            new SearchExplanation(Field: Field),
            row.Where(kv => kv.Value is not null).ToDictionary(kv => kv.Key, kv => kv.Value!))).ToList();
        return new SearchResult(items, result.Next);
    }

    private static Dictionary<string, JsonElement>? Filters(KindInfo kind, IReadOnlyList<FieldFilter> filters)
    {
        if (filters.Count == 0)
        {
            return null;
        }

        var result = new Dictionary<string, Dictionary<string, JsonElement>>(StringComparer.OrdinalIgnoreCase);
        foreach (var filter in filters)
        {
            var facet = kind.Facet(filter.Name)!;
            if (!result.TryGetValue(facet.Name, out var operators))
            {
                result[facet.Name] = operators = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
            }

            var op = Operator(filter);
            if (!operators.TryAdd(op, Value(facet, filter.Value)))
            {
                throw new NotSupportedException($"Two '{op}' filters on '{facet.Name}' cannot be combined.");
            }
        }

        return result.ToDictionary(kv => kv.Key, kv => JsonSerializer.SerializeToElement(kv.Value));
    }

    private static string Operator(FieldFilter filter) => (filter.Comparator, filter.Negated) switch
    {
        (FilterComparator.Equals, false) => "eq",
        (FilterComparator.Equals, true) => "ne",
        (FilterComparator.Contains, false) => "like",
        (FilterComparator.Contains, true) => "nlike",
        (FilterComparator.Greater, false) => "gt",
        (FilterComparator.GreaterOrEqual, false) => "gte",
        (FilterComparator.Less, false) => "lt",
        (FilterComparator.LessOrEqual, false) => "lte",
        _ => throw new NotSupportedException($"A negated {filter.Comparator} filter is not supported."),
    };

    private static JsonElement Value(FacetInfo facet, string value) =>
        facet.Type == "number" && double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            ? JsonSerializer.SerializeToElement(number)
            : JsonSerializer.SerializeToElement(value);
}
