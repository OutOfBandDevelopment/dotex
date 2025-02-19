Here is the documentation for the source code in Markdown format:

**Overview**
==========

This documentation provides an overview of the source code for the OoBDev System Response Model library. The library provides a set of interfaces and classes that can be used to represent the results of operations and queries.

**IResult.cs**
-------------

```IResult.cs
using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a result, which may include a collection of messages.
/// </summary>
public interface IResult
{
    /// <summary>
    /// Gets a collection of result messages.
    /// </summary>
    IReadOnlyCollection<ResultMessage>? Messages { get; }
}
```

**MessageLevels.cs**
--------------------

```MessageLevels.cs
using OoBDev.System.ComponentModel;
using OoBDev.System.Text.Json.Serialization;
using System.Text.Json.Serialization;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// response message level
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverterEx<MessageLevels>))]
public enum MessageLevels
{
    /// <summary>
    /// extra detailed information 
    /// </summary>
    [EnumValue("trace")]
    Trace,

    /// <summary>
    /// information to assist in troubleshooting
    /// </summary>
    [EnumValue("debug")]
    Debug,

    /// <summary>
    /// extra information about process
    /// </summary>
    [EnumValue("info")]
    Information,

    /// <summary>
    /// notifications and assumptions about processing that did not effect output
    /// </summary>
    [EnumValue("warning")]
    Warning,

    /// <summary>
    /// errors that caused the system to not complete your request as you may have expected
    /// </summary>
    [EnumValue("error")]
    Error,

    /// <summary>
    /// information about processing that failed
    /// </summary>
    [EnumValue("fatal")]
    Critical,

    /// <summary>
    /// unknown value
    /// </summary>
    [EnumValue("unknown")]
    None,
}
```

**ModelResult.cs**
-----------------

```ModelResult.cs
using System.Collections.Generic;

namespace OoBDev.System.ResponseModel;

/// <summary>
/// Represents a result containing a single model.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public record ModelResult<TModel> : IModelResult<TModel>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModelResult{TModel}"/> class with the specified data.
    /// </summary>
    /// <param name="data">The model data.</param>
    public ModelResult(TModel data) => Data = data;

    /// <summary>
    /// Gets the model data.
    /// </summary>
    public TModel? Data { get; }

    /// <summary>
    /// Gets the model data as an object.
    /// </summary>
    object? IModelResult.Data => Data;

    /// <summary>
    /// Gets or sets the collection of result messages.
    /// </summary>
    public IReadOnlyCollection<ResultMessage>? Messages { get; init; }
}
```

**PagedQueryResult.cs**
----------------------

```PagedQueryResult.cs
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
        )
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
        )
    {
        CurrentPage = toWrap.Current