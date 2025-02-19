using OoBDev.System.Accessors;
using OoBDev.System.Linq.Search;
using OoBDev.System.ResponseModel;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace OoBDev.AspNetCore.Mvc.Filters;

/// <summary>
/// Search Query Results filter is an extension for ASP.Net Core to enable a common
/// pattern to query data endpoints with from Controller Actions.  This detects 
/// actions that return IQueryable{TModel} and intercepts the web request to complete the 
/// query based on user requested inputs.
/// <see cref="SearchQuery{TModel}"/>
/// <see cref="IQueryBuilder{TModel}"/>
/// <see cref="IQueryable{TModel}"/>
/// <see cref="IQueryResult{TModel}"/>
/// <see cref="IPagedQueryResult{TModel}"/>
/// </summary>
public class SearchQueryResultFilter(
    IAccessor<ISearchQuery> searchQuery,
    ILogger<SearchQueryResultFilter> logger,
    IServiceProvider serviceProvider
        ) : IResultFilter
{
    private readonly ILogger _logger = logger;

    /// <summary>
    /// Called after the action result executes.
    /// </summary>
    /// <param name="context">The <see cref="ResultExecutedContext"/>.</param>
    public void OnResultExecuted(ResultExecutedContext context)
    {
    }

    /// <summary>
    /// Called before the action result executes.
    /// </summary>
    /// <param name="context">The <see cref="ResultExecutingContext"/>.</param>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is Microsoft.AspNetCore.Mvc.ObjectResult objectResult &&
            objectResult.Value is IQueryable query)
        {
            var elementType = query.ElementType;

            _logger.LogInformation($"Base Query: {{{nameof(query)}}} ({{{nameof(query.ElementType)}}})", query.ToString(), elementType);

            if (elementType != null && searchQuery.Value != null)
            {
                var queryBuilder = (IQueryBuilder)serviceProvider.GetRequiredService(typeof(IQueryBuilder<>).MakeGenericType(elementType));
                objectResult.Value = queryBuilder.ExecuteBy(query, searchQuery.Value);
            }
            else
            {
                _logger.LogWarning(
                    $"No {nameof(SearchQuery)} ({{{nameof(elementType)}}}) found, results will not be filtered",
                    elementType
                    );
            }
        }
    }
}
