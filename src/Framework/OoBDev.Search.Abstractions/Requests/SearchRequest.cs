using System.Collections.Generic;

namespace OoBDev.Search.Requests;

/// <summary>
/// The request every <see cref="ISearchProvider"/> understands. The text form (<c>SearchTextParser</c>) and the JSON form are the same record.
/// </summary>
public sealed record SearchRequest
{
    /// <summary>Free text.</summary>
    public string Text { get; init; } = "";

    /// <summary>Field conditions, all of which must hold.</summary>
    public IReadOnlyList<FieldFilter> Filters { get; init; } = [];

    /// <summary>An optional closeness anchor.</summary>
    public SearchAnchor? Anchor { get; init; }

    /// <summary>Sort fields, a leading <c>-</c> meaning descending; empty means by score.</summary>
    public IReadOnlyList<string> Sort { get; init; } = [];

    /// <summary>The number of items a page holds.</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>The opaque cursor of the page to read, from a previous <see cref="SearchResult.NextCursor"/>.</summary>
    public string? Cursor { get; init; }
}
