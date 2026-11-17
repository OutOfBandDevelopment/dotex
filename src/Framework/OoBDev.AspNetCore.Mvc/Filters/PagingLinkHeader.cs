using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>Builds the RFC 8288 <c>Link</c> header (first, prev, next, last) for a paged result.</summary>
public static class PagingLinkHeader
{
    /// <summary>The query parameter that carries the page number.</summary>
    public const string PageParameter = "currentPage";

    /// <summary>Builds the header value, or null when there is a single page or none.</summary>
    /// <param name="request">The current request; its path and query are kept, only the page number changes.</param>
    /// <param name="currentPage">The current page, starting at 1.</param>
    /// <param name="totalPageCount">The total number of pages.</param>
    /// <returns>The header value or null.</returns>
    public static string? Build(HttpRequest request, int currentPage, int totalPageCount)
    {
        if (totalPageCount <= 1)
        {
            return null;
        }

        var links = new List<string> { Link(request, 1, "first") };
        if (currentPage > 1)
        {
            links.Add(Link(request, currentPage - 1, "prev"));
        }
        if (currentPage < totalPageCount)
        {
            links.Add(Link(request, currentPage + 1, "next"));
        }
        links.Add(Link(request, totalPageCount, "last"));
        return string.Join(", ", links);
    }

    private static string Link(HttpRequest request, int page, string rel)
    {
        var query = request.Query
            .Where(q => !string.Equals(q.Key, PageParameter, global::System.StringComparison.OrdinalIgnoreCase))
            .SelectMany(q => q.Value.Select(v => KeyValuePair.Create(q.Key, (string?)v)))
            .ToList();
        query.Add(KeyValuePair.Create(PageParameter, (string?)page.ToString(CultureInfo.InvariantCulture)));

        var url = request.PathBase.Add(request.Path).ToUriComponent();
        foreach (var pair in query)
        {
            url = QueryHelpers.AddQueryString(url, pair.Key, pair.Value);
        }
        return $"<{url}>; rel=\"{rel}\"";
    }
}
