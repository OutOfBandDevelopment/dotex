namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents a page query for paginating results.
/// </summary>
public interface IPageQuery
{
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Gets the size of each page.
    /// </summary>
    int PageSize { get; }

    /// <summary>
    /// Gets a value indicating whether to exclude the total page count from the result.
    /// </summary>
    bool ExcludePageCount { get; }
}
