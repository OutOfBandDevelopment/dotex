using System.Collections.Generic;

namespace OoBDev.Query.Metadata;

/// <summary>
/// One page of results.
/// </summary>
/// <typeparam name="T">The row type: a dictionary, the kind's row class or <see cref="FacetCount"/>.</typeparam>
public sealed record QueryResult<T>
{
    /// <summary>The kind that was queried.</summary>
    public required string Kind { get; init; }

    /// <summary>The shape of the rows.</summary>
    public required string Shape { get; init; }

    /// <summary>The rows.</summary>
    public required IReadOnlyList<T> Rows { get; init; }

    /// <summary>The signed cursor of the next page, or null on the last page.</summary>
    public string? Next { get; init; }

    /// <summary>The number of matching rows when the request asked for it.</summary>
    public long? Count { get; init; }

    /// <summary>The time the query took.</summary>
    public long ElapsedMs { get; init; }

    /// <summary>For shape <c>graph</c>: the distinct node ids the edges touch.</summary>
    public IReadOnlyList<string>? Nodes { get; init; }
}
