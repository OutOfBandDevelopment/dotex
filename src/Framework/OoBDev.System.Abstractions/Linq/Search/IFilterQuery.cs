using System.Collections.Generic;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents a query with filtering options.
/// </summary>
public interface IFilterQuery
{
    /// <summary>
    /// Gets the collection of filter parameters.
    /// </summary>
    IDictionary<string, FilterParameter>? Filter { get; }
}
