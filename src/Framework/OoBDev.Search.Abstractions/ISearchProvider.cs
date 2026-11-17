using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// A search backend (keyword, vector, hybrid or a SQL kind) behind one request model.
/// </summary>
public interface ISearchProvider
{
    /// <summary>The filter names the provider supports; a request naming another throws <see cref="UnsupportedFilterException"/>.</summary>
    IReadOnlySet<string> SupportedFilters { get; }

    /// <summary>Runs the request.</summary>
    /// <param name="request">The request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>One page of results, each with its explanation.</returns>
    Task<SearchResult> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
}
