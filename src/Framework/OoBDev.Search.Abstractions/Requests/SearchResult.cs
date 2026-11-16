using System.Collections.Generic;

namespace OoBDev.Search.Requests;

/// <summary>
/// One page of results.
/// </summary>
/// <param name="Items">The items, best first unless a sort was requested.</param>
/// <param name="NextCursor">The cursor of the next page, or <see langword="null"/> on the last page.</param>
public sealed record SearchResult(IReadOnlyList<SearchItem> Items, string? NextCursor = null);
