using OoBDev.System.Linq.Expressions;
using System;
using System.Linq;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents an interface for building sorting expressions and applying sorting to a query.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public interface ISortBuilder<TModel>
{
    /// <summary>
    /// Sorts the specified query based on the provided sort criteria.
    /// </summary>
    /// <param name="query">The query to be sorted.</param>
    /// <param name="searchRequest">The sort criteria specified in the search request.</param>
    /// <param name="treeBuilder">The expression tree builder for the model.</param>
    /// <param name="stringComparison">The string comparison method used for sorting.</param>
    /// <returns>An <see cref="IOrderedQueryable{TModel}"/> representing the sorted query.</returns>
    IOrderedQueryable<TModel> SortBy(
        IQueryable<TModel> query,
        ISortQuery searchRequest,
        IExpressionTreeBuilder<TModel> treeBuilder,
        StringComparison stringComparison
    );
}
