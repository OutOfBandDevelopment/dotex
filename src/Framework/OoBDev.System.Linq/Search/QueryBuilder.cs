using OoBDev.Extensions.Reflection;
using OoBDev.System.ComponentModel.Search;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.ResponseModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Provides a base class for building and executing queries based on search, filter, sort, and page criteria.
/// </summary>
public abstract class QueryBuilder
{
    /// <summary>
    /// Default page size when not defined on request
    /// </summary>
    public const int DefaultPageSize = 10;

    /// <summary>
    /// Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery.
    /// </summary>
    /// <seealso cref="ISearchTermQuery"/>
    /// <seealso cref="IFilterQuery"/>
    /// <seealso cref="ISortQuery"/>
    /// <seealso cref="IPageQuery"/>
    /// <param name="query">The queryable data source.</param>
    /// <param name="searchQuery">The search query parameters.</param>
    /// <param name="postBuildVisitors">Optional post-build expression visitors.</param>
    /// <param name="logger">Optional logger for logging messages.</param>
    /// <param name="capture">Optional message capture for result messages.</param>
    /// <returns>The result of the query execution.</returns>
    public static IQueryResult Execute(
        IQueryable query,
        ISearchQuery searchQuery,
#if DEBUG
        IEnumerable<IPostBuildExpressionVisitor>? postBuildVisitors,
        ILogger<QueryBuilder>? logger,
        ICaptureResultMessage? capture
#else
        IEnumerable<IPostBuildExpressionVisitor>? postBuildVisitors = null,
        ILogger<QueryBuilder>? logger = null,
        ICaptureResultMessage? capture = null
#endif
        )
    {
        var queryType = Type.MakeGenericSignatureType(typeof(IQueryable<>), Type.MakeGenericMethodParameter(0)) ??
            throw new NotSupportedException($"{query.GetType()} is not supported");

        var methodSignature = typeof(QueryBuilder).GetMethod(
            nameof(Execute),
            1,
            [
                queryType,
                typeof(ISearchQuery),
                typeof(IEnumerable<IPostBuildExpressionVisitor>),
                typeof(ILogger<QueryBuilder>),
                typeof(ICaptureResultMessage)
            ]
            ) ??
            throw new NotSupportedException($"{query.GetType()} is not supported");

        var method = methodSignature.MakeGenericMethod(query.ElementType);
        var result = method.Invoke(null, [query, searchQuery, postBuildVisitors, logger, capture]) ??
            throw new NotSupportedException($"{query.GetType()} is not supported");
        var paged = (IQueryResult)result;
        return paged;
    }

    /// <summary>
    /// Composes and executes a typed query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery.
    /// </summary>
    /// <seealso cref="ISearchTermQuery"/>
    /// <seealso cref="IFilterQuery"/>
    /// <seealso cref="ISortQuery"/>
    /// <seealso cref="IPageQuery"/>
    /// <typeparam name="TModel">The type of the model in the query.</typeparam>
    /// <param name="query">The typed queryable data source.</param>
    /// <param name="searchQuery">The search query parameters.</param>
    /// <param name="postBuildVisitors">Optional post-build expression visitors.</param>
    /// <param name="logger">Optional logger for logging messages.</param>
    /// <param name="capture">Optional message capture for result messages.</param>
    /// <returns>The result of the typed query execution.</returns>
    public static IQueryResult<TModel> Execute<TModel>(
        IQueryable<TModel> query,
        ISearchQuery searchQuery,
#if DEBUG
        IEnumerable<IPostBuildExpressionVisitor>? postBuildVisitors,
        ILogger<QueryBuilder>? logger,
        ICaptureResultMessage? capture
#else
        IEnumerable<IPostBuildExpressionVisitor>? postBuildVisitors = null,
        ILogger<QueryBuilder>? logger = null,
        ICaptureResultMessage? capture = null
#endif
        ) =>
        new QueryBuilder<TModel>(
            new SortBuilder<TModel>(),
            new ExpressionTreeBuilder<TModel>(),
            postBuildVisitors,
            logger,
            capture
            ).ExecuteBy(query, searchQuery);

    /// <summary>
    /// Composes and executes a typed query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery.
    /// </summary>
    /// <seealso cref="ISearchTermQuery"/>
    /// <seealso cref="IFilterQuery"/>
    /// <seealso cref="ISortQuery"/>
    /// <seealso cref="IPageQuery"/>
    /// <typeparam name="TModel">The type of the model in the query.</typeparam>
    /// <param name="query">The typed queryable data source.</param>
    /// <param name="searchQuery">The search query parameters.</param>
    /// <param name="postBuildVisitor">A single post-build expression visitor.</param>
    /// <param name="postBuildVisitors">Additional post-build expression visitors.</param>
    /// <returns>The result of the typed query execution.</returns>
    public static IQueryResult<TModel> Execute<TModel>(
        IQueryable<TModel> query,
        ISearchQuery searchQuery,
        IPostBuildExpressionVisitor postBuildVisitor,
        params IPostBuildExpressionVisitor[] postBuildVisitors) =>
        new QueryBuilder<TModel>(
            new SortBuilder<TModel>(),
            new ExpressionTreeBuilder<TModel>(),
            new[] { postBuildVisitor }.Concat(postBuildVisitors),
            default,
            default
            ).ExecuteBy(query, searchQuery);
}

/// <summary>
/// Provides a typed implementation for building and executing queries based on search, filter, sort, and page criteria.
/// </summary>
/// <typeparam name="TModel">The type of the model in the query.</typeparam>
public class QueryBuilder<TModel>(
    ISortBuilder<TModel> sortBuilder,
    IExpressionTreeBuilder<TModel> expressionBuilder,
#if DEBUG
        IEnumerable<IPostBuildExpressionVisitor>? postBuildVisitors,
        ILogger<QueryBuilder>? logger,
        ICaptureResultMessage? capture
#else
        IEnumerable<IPostBuildExpressionVisitor>? postBuildVisitors = null,
        ILogger<QueryBuilder>? logger = null,
        ICaptureResultMessage? capture = null
#endif
        ) : QueryBuilder, IQueryBuilder<TModel>
{
    private readonly IEnumerable<IPostBuildExpressionVisitor> _postBuildVisitors = postBuildVisitors ?? [];
    private readonly ILogger _logger = logger ?? new ConsoleLogger<QueryBuilder>();
    private readonly ICaptureResultMessage _capture = capture ?? CaptureResultMessage.Default;

    /// <summary>
    /// Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery.
    /// </summary>
    /// <seealso cref="ISearchTermQuery"/>
    /// <seealso cref="IFilterQuery"/>
    /// <seealso cref="ISortQuery"/>
    /// <seealso cref="IPageQuery"/>
    /// <param name="query">The queryable data source.</param>
    /// <param name="searchQuery">The search query parameters.</param>
    /// <returns>The result of the query execution.</returns>
    public IQueryResult<TModel> ExecuteBy(
        IQueryable<TModel> query,
        ISearchQuery searchQuery
        )
    {
        var ordered = BuildFrom(query, searchQuery, StringComparison.InvariantCultureIgnoreCase);

        if (searchQuery.PageSize >= 0)
        {
            return PageBy(ordered, searchQuery);
        }
        else
        {
            _logger.LogInformation($"Execute : {{{nameof(query)}}}", ordered.ToString());
            return new QueryResult<TModel>(ordered)
            {
                Messages = _capture.Capture(),
            };
        }
    }

    /// <summary>
    /// Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery.
    /// </summary>
    /// <seealso cref="ISearchTermQuery"/>
    /// <seealso cref="IFilterQuery"/>
    /// <seealso cref="ISortQuery"/>
    /// <seealso cref="IPageQuery"/>
    /// <param name="query">The queryable data source.</param>
    /// <param name="searchQuery">The search query parameters.</param>
    /// <returns>The result of the query execution.</returns>
    public IQueryResult ExecuteBy(IQueryable query, ISearchQuery searchQuery) =>
        ExecuteBy((query as IQueryable<TModel>) ?? throw new NotSupportedException(), searchQuery);

    internal IOrderedQueryable<TModel> BuildFrom(
        IQueryable<TModel> query,
        ISearchQuery searchQuery,
        StringComparison stringComparison
        )
    {
        var queryIntercepts = query.ElementType.GetAttributes<ISearchQueryIntercept>();
        foreach (var interceptor in queryIntercepts)
        {
            _logger.LogDebug($"Intercepted by: {{{nameof(interceptor)}}}", interceptor);
            searchQuery = interceptor.Intercept(searchQuery);
        }

        _logger.LogInformation($"Build query for {{{nameof(searchQuery)}}}", searchQuery);

        var searched = SearchBy(query, searchQuery, stringComparison, true);
        var filtered = FilterBy(searched, searchQuery, stringComparison);

        if (query.ToString() == filtered.ToString())
        {
            _capture.Publish(new ResultMessage
            {
                Level = MessageLevels.Warning,
                Message = SearchQuery.Messages.NoSearchQueryFilter,
                MessageCode = SearchQuery.Messages.NoSearchQueryFilterCode,
            });
            _logger.LogWarning(
                $"No filtering detected: {{{nameof(query)}}}: {{{nameof(searchQuery)}}}",
                query.ToString(),
                searchQuery.ToString()
                );
        }

        var sorted = SortBy(filtered, searchQuery, stringComparison);

        if (_postBuildVisitors.Any())
        {
            var toVisit = sorted.Expression;
            foreach (var visitor in _postBuildVisitors)
            {
                _logger.LogDebug($"Visited by: {{{nameof(visitor)}}}", visitor);
                toVisit = visitor.Visit(toVisit) ?? toVisit;
            }
            sorted = (IOrderedQueryable<TModel>)query.Provider.CreateQuery<TModel>(toVisit);
        }

        return sorted;
    }

    internal PagedQueryResult<TModel> PageBy(
        IOrderedQueryable<TModel> query,
        IPageQuery? pager
        )
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        var pageLength = pager?.PageSize ?? DefaultPageSize;
        if (pageLength <= 0) pageLength = DefaultPageSize;
        var page = (pager?.CurrentPage ?? 0) < 0 ? 0 : pager?.CurrentPage ?? 0;
        var excludePageCount = pager?.ExcludePageCount ?? false;

        var totalRows = excludePageCount ? -1 : query.Count();

        var pagedQuery = query.Skip(page * pageLength).Take(pageLength);
        var rows = (pageLength, page) switch
        {
            _ when page < 0 => Array.Empty<TModel>(),
            (int.MaxValue, 0) => [.. query],
            _ => [.. pagedQuery]
        };

        _logger.LogInformation($"Execute (paged): {{{nameof(query)}}}", pagedQuery.ToString());

        var result = new PagedQueryResult<TModel>(
            currentPage: page,
            totalPageCount: totalRows == -1 ? totalRows : (int)Math.Ceiling((decimal)totalRows / pageLength),
            totalRowCount: totalRows,
            items: rows
            )
        {
            Messages = _capture.Capture(),
        };
        return result;
    }

    internal IQueryable<TModel> SearchBy(
        IQueryable<TModel> query,
        ISearchTermQuery? search,
        StringComparison stringComparison,
        bool isSearchTerm
        )
    {

        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (!string.IsNullOrWhiteSpace(search?.SearchTerm))
        {
            var searchTermExpression = expressionBuilder.BuildExpression(search.SearchTerm, stringComparison, isSearchTerm);
            if (searchTermExpression != null)
                return query.Where(searchTermExpression);
        }
        return query;
    }

    internal IQueryable<TModel> FilterBy(
        IQueryable<TModel> query,
        IFilterQuery? filter,
        StringComparison stringComparison
        )
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));
        if (filter?.Filter != null)
        {
            foreach (var item in filter.Filter)
            {
                var filterExpression = expressionBuilder.GetPredicateExpression(item.Key, item.Value, stringComparison, false);
                if (filterExpression != null)
                {
                    query = query.Where(filterExpression);
                }
                else
                {
                    _capture.Publish(new ResultMessage
                    {
                        Message = SearchQuery.Messages.NoPropertyFilter,
                        MessageCode = SearchQuery.Messages.NoPropertyFilterCode,
                        Context = item.Key,
                        MetaData = item.Value,
                    });
                    _logger.LogWarning($"No filter mapped for property: {{name}} => {{filter}}", item.Key, item.Value);
                }
            }
        }
        return query;
    }

    internal IOrderedQueryable<TModel> SortBy(
        IQueryable<TModel> query,
        ISortQuery? sortBy,
        StringComparison keyStringComparer
        )
    {
        ArgumentNullException.ThrowIfNull(query, nameof(query));

        query = sortBy != null ? sortBuilder.SortBy(query, sortBy, expressionBuilder, keyStringComparer) : query;
        return query is IOrderedQueryable<TModel> sorted ? sorted : query.OrderBy(_ => 0);
    }
}
