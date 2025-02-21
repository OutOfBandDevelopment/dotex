using System.Collections.Generic;

namespace OoBDev.Search.Models;

/// <summary>
/// Represents a search result model containing information about a search result.
/// </summary>
public record SearchResultModel
{
    /// <summary>
    /// Gets or initializes the score of the search result.
    /// </summary>
    public required float Score { get; init; }

    /// <summary>
    /// Gets or initializes the item's id value.
    /// </summary>
    public required string ItemId { get; init; }

    /// <summary>
    /// Gets or initializes the item's metadata.
    /// </summary>
    public Dictionary<string, object>? MetaData { get; init; }
}
