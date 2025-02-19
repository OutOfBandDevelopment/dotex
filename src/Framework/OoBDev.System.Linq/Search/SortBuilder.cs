using OoBDev.Extensions.Linq;
using OoBDev.System.Linq.Expressions;
using OoBDev.System.ResponseModel;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Builder class for sorting IQueryable instances based on a provided sort query.
/// </summary>
/// <typeparam name="TModel">The type of the model being sorted.</typeparam>
/// <param name="logger">The logger used for logging sorting-related messages. If not provided, a default console logger will be used.</param>
/// <param name="messages">The result message capturer for publishing sorting-related messages.</param>
public class SortBuilder<TModel>(
    ILogger<SortBuilder<TModel>>? logger = null,
    ICaptureResultMessage? messages = null
        ) : ISortBuilder<TModel>
{
    private readonly ILogger _logger = logger ?? new ConsoleLogger<SortBuilder<TModel>>();
    private readonly ICaptureResultMessage _messages = messages ?? CaptureResultMessage.Default;

    /// <summary>
    /// Sorts an IQueryable instance based on the provided sort query.
    /// </summary>
    /// <param name="query">The IQueryable instance to be sorted.</param>
    /// <param name="searchRequest">The sort query parameters.</param>
    /// <param name="treeBuilder">The expression tree builder for building property expressions.</param>
    /// <param name="stringComparison">The string comparison type to use when sorting string values.</param>
    /// <returns>An IOrderedQueryable instance representing the sorted result.</returns>
    public IOrderedQueryable<TModel> SortBy(
        IQueryable<TModel> query,
        ISortQuery searchRequest,
        IExpressionTreeBuilder<TModel> treeBuilder,
        StringComparison stringComparison
        )
    {
        var stringComparer = StringComparer.FromComparison(stringComparison);
        var sortLookup = treeBuilder.PropertyExpressions();

        var orderBys = searchRequest.OrderBy;

        var compositeSortMap =
              sortLookup.Select(kvp => (kvp.Key, Expression: kvp.Value, Weight: 2))
              .GroupBy(k => k.Key).Select(i => (i.Key, i.OrderBy(x => x.Weight).First().Expression))
              .ToDictionary(k => k.Key, v => v.Expression, stringComparer)
              ;

        var unmatchedKeys = searchRequest.OrderBy.Keys.Except(compositeSortMap.Keys, stringComparer);
        var matchedKeys = searchRequest.OrderBy.Keys.Intersect(compositeSortMap.Keys, stringComparer);

        if (unmatchedKeys.Any())
        {
            _messages.Publish(new ResultMessage
            {
                Level = MessageLevels.Warning,
                Message = SearchQuery.Messages.SortPropertiesNotFound,
                MessageCode = SearchQuery.Messages.SortPropertiesNotFoundCode,
                MetaData = new
                {
                    unmatchedKeys
                }
            });
            _logger.LogWarning(
                $"Could not use properties: {{{nameof(unmatchedKeys)}}} as they are not on the model",
                string.Join("; ", unmatchedKeys)
                );
        }

        if (!matchedKeys.Any() && treeBuilder.DefaultSortOrder().Count != 0)
        {
            orderBys = treeBuilder.DefaultSortOrder()
              .ToDictionary(k => k.column, v => v.direction, stringComparer);

            _messages.Publish(new ResultMessage
            {
                Level = MessageLevels.Information,
                Message = SearchQuery.Messages.ApplySortDefault,
                MessageCode = SearchQuery.Messages.ApplySortDefaultCode,
                MetaData = new
                {
                    defaultSort = treeBuilder.DefaultSortOrder().Select(t => new { t.column, t.direction }),
                }
            });
            _logger.LogInformation(
                $"Applying default sort for {{type}}: {{{nameof(orderBys)}}}",
                typeof(TModel),
                string.Join("; ", treeBuilder.DefaultSortOrder())
                );
        }

        IOrderedQueryable<TModel>? ordered = null;
        foreach (var orderBy in orderBys)
        {
            if (!compositeSortMap.TryGetValue(orderBy.Key, out var keySelector, stringComparer) ||
                keySelector == null
                ) continue;

            ordered = (ordered, orderBy.Value) switch
            {
                (null, OrderDirections.Ascending) => query.OrderBy(keySelector),
                (null, OrderDirections.Descending) => query.OrderByDescending(keySelector),
                (_, OrderDirections.Ascending) => ordered.ThenBy(keySelector),
                (_, OrderDirections.Descending) => ordered.ThenByDescending(keySelector),
                _ => ordered
            };
        }

        if (ordered == null)
        {
            _messages.Publish(new ResultMessage
            {
                Level = MessageLevels.Information,
                Message = SearchQuery.Messages.ForceSortDefault,
                MessageCode = SearchQuery.Messages.ForceSortDefaultCode,
                MetaData = new
                {
                    defaultSort = new[] { new { column = "*", direction = OrderDirections.Ascending } },
                }
            });
            _logger.LogWarning($"Force sort by 0 applied for {{type}}", typeof(TModel));
            ordered ??= query.OrderBy(_ => 0);
        }

        return ordered;
    }
}
