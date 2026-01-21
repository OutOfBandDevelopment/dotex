using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics.CodeAnalysis;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Controller for handling search operations.
/// </summary>
[AllowAnonymous]
[Route("[Controller]/[Action]")]
[Obsolete]
[ExcludeFromCodeCoverage]
public class SearchController : Controller
{
    //private readonly ISearchProvider _search;

    ///// <summary>
    ///// Initializes a new instance of the <see cref="SearchController"/> class.
    ///// </summary>
    ///// <param name="search">The search provider.</param>
    //public SearchController(
    //    ISearchProvider search
    //    ) => _search = search;

    ///// <summary>
    ///// Retrieves a list of all available search results.
    ///// </summary>
    ///// <returns>An enumerable collection of search results.</returns>
    //[HttpGet]
    //public async Task<IEnumerable<SearchResultModel>> List() => await _search.ListAsync();

    ///// <summary>
    ///// Performs a semantic search with the given query.
    ///// </summary>
    ///// <param name="query">The search query.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <returns>An enumerable collection of search results.</returns>
    //[HttpGet]
    //public async Task<IEnumerable<SearchResultModel>> SemanticSearch(string? query = default, int limit = 10)
    //{
    //    Response.Headers[$"X-APP-{nameof(query)}"] = query;
    //    Response.Headers[$"X-APP-{nameof(limit)}"] = limit.ToString();
    //    return await _search.SemanticSearchAsync(query, limit);
    //}

    ///// <summary>
    ///// Performs a lexical search with the given query.
    ///// </summary>
    ///// <param name="query">The search query.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <returns>An enumerable collection of search results.</returns>
    //[HttpGet]
    //public async Task<IEnumerable<SearchResultModel>> LexicalSearch(string? query = default, int limit = 10)
    //{
    //    Response.Headers[$"X-APP-{nameof(query)}"] = query;
    //    Response.Headers[$"X-APP-{nameof(limit)}"] = limit.ToString();
    //    return await _search.LexicalSearchAsync(query, limit);
    //}

    ///// <summary>
    ///// Performs a hybrid search with the given query.
    ///// </summary>
    ///// <param name="query">The search query.</param>
    ///// <param name="limit">The maximum number of results to return.</param>
    ///// <returns>An enumerable collection of search results.</returns>
    //[HttpGet]
    //public async Task<IEnumerable<SearchResultModel>> HybridSearch(string? query = default, int limit = 10)
    //{
    //    Response.Headers[$"X-APP-{nameof(query)}"] = query;
    //    Response.Headers[$"X-APP-{nameof(limit)}"] = limit.ToString();
    //    return await _search.HybridSearchAsync(query, limit);
    //}

}
