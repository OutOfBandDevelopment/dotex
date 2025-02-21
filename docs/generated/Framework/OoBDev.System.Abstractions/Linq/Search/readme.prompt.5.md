Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```ISearchTermQuery.cs
namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents a query object that includes a search term for filtering data.
/// </summary>
public interface ISearchTermQuery
{
    /// <summary>
    /// Gets the search term used for filtering data.
    /// </summary>
    string? SearchTerm { get; }
}

```

```ISortBuilder.cs
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

```

```ISortQuery.cs
using System.Collections.Generic;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents an interface for specifying sorting criteria in a query.
/// </summary>
public interface ISortQuery
{
    /// <summary>
    /// Gets a dictionary containing sorting information, where the key is the column name and the value is the sort direction.
    /// </summary>
    IDictionary<string, OrderDirections> OrderBy { get; }
}

```

```OrderDirections.cs
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Enumeration to control sort order
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverterEx<OrderDirections>))]
public enum OrderDirections
{
    /// <summary>
    /// sort related items in ascending order
    /// </summary>
    [EnumValue(OrderDirectionsConstants.AscendingShort)]
    Ascending = 0,

    /// <summary>
    /// sort related items in descending order
    /// </summary>
    [EnumValue(OrderDirectionsConstants.DescendingShort)]
    Descending = 1,
}

```

```OrderDirectionsConstants.cs
namespace OoBDev.System.Linq.Search;

/// <summary>
/// Provides constants for order directions.
/// </summary>
public static class OrderDirectionsConstants
{
    /// <summary>
    /// Represents the ascending order direction.
    /// </summary>
    public const string Ascending = nameof(OrderDirections.Ascending);

    /// <summary>
    /// Represents a short form of the ascending order direction.
    /// </summary>
    public const string AscendingShort = "asc";

    /// <summary>
    /// Represents the descending order direction.
    /// </summary>
    public const string Descending = nameof(OrderDirections.Descending);

    /// <summary>
    /// Represents a short form of the descending order direction.
    /// </summary>
    public const string DescendingShort = "desc";
}

```

