using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>The rows a query returned.</summary>
public class DataQueryResultModel
{
    /// <summary>Gets or sets the column names.</summary>
    public IReadOnlyList<string> Columns { get; set; } = [];

    /// <summary>Gets or sets the rows, each as text per column (null stays null).</summary>
    public IReadOnlyList<IReadOnlyList<string?>> Rows { get; set; } = [];

    /// <summary>Gets or sets a value indicating whether more rows existed than the cap allowed.</summary>
    public bool Truncated { get; set; }

    /// <summary>Gets or sets the elapsed milliseconds.</summary>
    public long ElapsedMs { get; set; }
}
