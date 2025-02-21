using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.System.ResponseModel;
/// <summary>
/// Represents the result of a query operation, containing a collection of items and optional result messages.
/// </summary>
/// <typeparam name="TModel">The type of items in the result.</typeparam>
public class QueryResult<TModel> : IQueryResult<TModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{TModel}"/> class.
    /// </summary>
    /// <param name="items">The collection of items in the result.</param>
    public QueryResult(IEnumerable<TModel> items) => Rows = items as List<TModel> ?? items.ToList();

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{TModel}"/> class by wrapping another <see cref="IQueryResult{TModel}"/>.
    /// </summary>
    /// <param name="toWrap">The query result to wrap.</param>
    public QueryResult(IQueryResult<TModel> toWrap) : this(items: toWrap.Rows)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="QueryResult{TModel}"/> class with an empty collection of items.
    /// </summary>
    public QueryResult()
    {
    }

    /// <summary>
    /// Gets the collection of items in the result.
    /// </summary>
    public IReadOnlyCollection<TModel> Rows { get; } = new List<TModel>();

    /// <summary>
    /// Gets the collection of items in the result as a non-generic enumerable.
    /// </summary>
    IEnumerable IQueryResult.Rows => Rows;

    /// <summary>
    /// Gets or sets the collection of result messages associated with the query result.
    /// </summary>
    public IReadOnlyCollection<ResultMessage>? Messages { get; init; }
}
