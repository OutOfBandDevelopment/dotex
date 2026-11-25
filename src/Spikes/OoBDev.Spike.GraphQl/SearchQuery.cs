using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HotChocolate;
using OoBDev.Search;
using OoBDev.Search.Requests;

namespace OoBDev.Spike.GraphQl;

/// <summary>One hit as the GraphQL schema shows it.</summary>
public sealed record HitDto(string Id, double Score);

/// <summary>A Relay-style page over the search result.</summary>
public sealed record SearchPage(IReadOnlyList<HitDto> Nodes, string? EndCursor, bool HasNextPage);

/// <summary>The query root: the custom search text syntax is one string argument, the cursor is the provider's.</summary>
public sealed class SearchQuery
{
    [GraphQlApplicationRight("search.read")]
    public async Task<SearchPage> Search(string? text, int? first, string? after, [Service] ISearchProvider provider, CancellationToken cancellationToken)
    {
        var request = SearchTextParser.Parse(text ?? "") with { PageSize = first ?? 20, Cursor = after };
        var result = await provider.SearchAsync(request, cancellationToken).ConfigureAwait(false);
        return new SearchPage(result.Items.Select(i => new HitDto(i.ItemId, i.Score)).ToArray(), result.NextCursor, result.NextCursor is not null);
    }

    /// <summary>Typed filters instead of the text syntax: the schema, not a string, lists what can be filtered.</summary>
    [GraphQlApplicationRight("search.read")]
    public async Task<SearchPage> SearchTyped(string? text, string? ext, int? first, string? after, [Service] ISearchProvider provider, CancellationToken cancellationToken)
    {
        var filters = new List<FieldFilter>();
        if (ext is not null)
            filters.Add(new FieldFilter("ext", FilterComparator.Equals, ext));
        var request = new SearchRequest { Text = text ?? "", Filters = filters, PageSize = first ?? 20, Cursor = after };
        var result = await provider.SearchAsync(request, cancellationToken).ConfigureAwait(false);
        return new SearchPage(result.Items.Select(i => new HitDto(i.ItemId, i.Score)).ToArray(), result.NextCursor, result.NextCursor is not null);
    }
}
