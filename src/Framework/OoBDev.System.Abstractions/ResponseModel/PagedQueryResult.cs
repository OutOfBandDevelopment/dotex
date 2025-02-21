using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents the result of a paged query, including information about the current page, total page count,
/// total row count, and a collection of items.
/// </summary>
/// <typeparam name="TModel">The type of the items in the result.</typeparam>
public class PagedQueryResult<TModel> : QueryResult<TModel>, IPagedQueryResult<TModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PagedQueryResult{TModel}"/> class.
    /// </summary>
    /// <param name="currentPage">The current page number.</param>
    /// <param name="totalPageCount">The total number of pages.</param>
    /// <param name="totalRowCount">The total number of rows.</param>
    /// <param name="items">The collection of items in the result.</param>
    public PagedQueryResult(
        int currentPage,
        int totalPageCount,
        int totalRowCount,
        IEnumerable<TModel> items
        ) : base(items)
    {
        CurrentPage = currentPage;
        TotalPageCount = totalPageCount;
        TotalRowCount = totalRowCount;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PagedQueryResult{TModel}"/> class by wrapping an existing
    /// <see cref="IPagedQueryResult{TModel}"/> instance.
    /// </summary>
    /// <param name="toWrap">The <see cref="IPagedQueryResult{TModel}"/> instance to wrap.</param>
    public PagedQueryResult(
        IPagedQueryResult<TModel> toWrap
        ) : this(
            currentPage: toWrap.CurrentPage,
            totalPageCount: toWrap.TotalPageCount,
            totalRowCount: toWrap.TotalRowCount,
            items: toWrap.Rows
            )
    {
    }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public PagedQueryResult()
    {
    }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int CurrentPage { get; }

    /// <summary>
    /// Gets the total number of pages.
    /// </summary>
    public int TotalPageCount { get; }

    /// <summary>
    /// Gets the total number of rows.
    /// </summary>
    public int TotalRowCount { get; }
}
