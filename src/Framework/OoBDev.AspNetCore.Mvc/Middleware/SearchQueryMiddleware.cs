using OoBDev.AspNetCore.Mvc.Providers.SearchQuery;
using OoBDev.System.Accessors;
using OoBDev.System.Linq.Search;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// ASP.Net MVC Middleware to enable IQueryable{T} responses from  Controller Actions
/// </summary>
public class SearchQueryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SearchQueryMiddleware> _logger;
    private readonly IAccessor<ISearchQuery> _searchQuery;
    private readonly ISearchModelBuilder _builder;

    /// <summary>
    /// ASP.Net MVC Middleware to enable IQueryable{T} responses from  Controller Actions
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="log">The logger for logging information.</param>
    /// <param name="searchQuery">The accessor for the search query.</param>
    /// <param name="builder">The builder for search model.</param>
    public SearchQueryMiddleware(
        RequestDelegate next,
        ILogger<SearchQueryMiddleware> log,
        IAccessor<ISearchQuery> searchQuery,
        ISearchModelBuilder builder
        )
    {
        _next = next;
        _logger = log;
        _searchQuery = searchQuery;
        _builder = builder;
    }

    /// <summary>
    /// Invokes the middleware to handle the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            var (isSearch, searchModel) = await _builder.GetModelAsync<SearchQuery>(context);
            if (isSearch)
            {
                if (searchModel == null)
                {
                    _logger.LogWarning($"SearchQuery not bound");
                }
                else
                {
                    _logger.LogInformation($"Invoking: {{{nameof(searchModel)}}}", searchModel);
                }
                _searchQuery.Value = searchModel;
            }
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("Error");
            _logger.LogError($"{{{nameof(ex.Message)}}}", ex.Message);
            throw;
        }
    }
}
