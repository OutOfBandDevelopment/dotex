namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a generic interface for a paged query result containing model data and additional paging information.
/// </summary>
public interface IPagedQueryResult : IQueryResult
{
    /// <summary>
    /// Gets the current page number.
    /// </summary>
    int CurrentPage { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    int TotalPageCount { get; }

    /// <summary>
    /// Gets the total number of rows across all pages.
    /// </summary>
    int TotalRowCount { get; }
}

/// <summary>
/// Represents a generic interface for a paged query result containing model data of type <typeparamref name="TModel"/>
/// and additional paging information.
/// </summary>
/// <typeparam name="TModel">The type of the model data.</typeparam>
public interface IPagedQueryResult<TModel> : IPagedQueryResult, IQueryResult<TModel>
{
}
