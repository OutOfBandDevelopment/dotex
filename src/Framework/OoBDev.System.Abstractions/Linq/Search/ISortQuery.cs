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
