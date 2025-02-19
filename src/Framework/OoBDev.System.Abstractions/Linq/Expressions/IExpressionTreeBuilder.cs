using OoBDev.System.Linq.Search;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace OoBDev.System.Linq.Expressions;

/// <summary>
/// Represents a builder for constructing expression trees used in querying and filtering.
/// </summary>
public interface IExpressionTreeBuilder
{
    /// <summary>
    /// Gets the names of properties that can be used for searching.
    /// </summary>
    /// <returns>The collection of searchable property names.</returns>
    IReadOnlyCollection<string> GetSearchablePropertyNames();

    /// <summary>
    /// Gets the names of properties that can be used for sorting.
    /// </summary>
    /// <returns>The collection of sortable property names.</returns>
    IReadOnlyCollection<string> GetSortablePropertyNames();

    /// <summary>
    /// Gets the names of properties that can be used for filtering.
    /// </summary>
    /// <returns>The collection of filterable property names.</returns>
    IReadOnlyCollection<string> GetFilterablePropertyNames();

    /// <summary>
    /// Gets the default sort order defined by the expression tree builder.
    /// </summary>
    /// <returns>The collection of default sort order information.</returns>
    IReadOnlyCollection<(string column, OrderDirections direction)> DefaultSortOrder();
}

/// <summary>
/// Represents a typed builder for constructing expression trees used in querying and filtering.
/// </summary>
/// <typeparam name="TModel">The type of the model for which expression trees are built.</typeparam>
public interface IExpressionTreeBuilder<TModel> : IExpressionTreeBuilder
{
    /// <summary>
    /// Gets the predicate expression based on the provided filter parameters.
    /// </summary>
    /// <param name="name">The name of the property to filter on.</param>
    /// <param name="value">The filter parameter value.</param>
    /// <param name="stringComparison">The string comparison option for string-based filtering.</param>
    /// <param name="isSearchTerm">A flag indicating whether the filter is a search term.</param>
    /// <returns>The predicate expression based on the filter parameters.</returns>
    Expression<Func<TModel, bool>>? GetPredicateExpression(string name, FilterParameter value, StringComparison stringComparison, bool isSearchTerm);

    /// <summary>
    /// Builds the expression tree based on the provided query parameter.
    /// </summary>
    /// <param name="queryParameter">The query parameter used for building the expression tree.</param>
    /// <param name="stringComparison">The string comparison option for string-based filtering.</param>
    /// <param name="isSearchTerm">A flag indicating whether the query parameter is a search term.</param>
    /// <returns>The expression tree representing the filtering condition.</returns>
    Expression<Func<TModel, bool>>? BuildExpression(object? queryParameter, StringComparison stringComparison, bool isSearchTerm);

    /// <summary>
    /// Gets a dictionary of property names and their corresponding expression trees.
    /// </summary>
    /// <returns>The dictionary of property names and expression trees.</returns>
    IReadOnlyDictionary<string, Expression<Func<TModel, object>>> PropertyExpressions();
}
