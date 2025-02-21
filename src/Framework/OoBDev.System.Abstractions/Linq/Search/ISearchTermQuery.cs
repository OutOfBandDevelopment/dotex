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
