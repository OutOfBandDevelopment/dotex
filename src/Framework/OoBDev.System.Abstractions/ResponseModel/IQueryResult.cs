using System.Collections;
using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a generic interface for a query result containing rows of data.
/// </summary>
public interface IQueryResult : IResult
{
    /// <summary>
    /// Gets the collection of rows in the query result.
    /// </summary>
    IEnumerable Rows { get; }
}

/// <summary>
/// Represents a generic interface for a query result containing model data of type <typeparamref name="TModel"/>.
/// </summary>
/// <typeparam name="TModel">The type of the model data.</typeparam>
public interface IQueryResult<TModel> : IQueryResult
{
    /// <summary>
    /// Gets the collection of rows in the query result, typed as <typeparamref name="TModel"/>.
    /// </summary>
    new IReadOnlyCollection<TModel> Rows { get; }
}
