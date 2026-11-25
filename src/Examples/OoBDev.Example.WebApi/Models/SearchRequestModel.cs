using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A query plus the passages to search.</summary>
public class SearchRequestModel
{
    /// <summary>Gets or sets the query.</summary>
    public required string Query { get; set; }

    /// <summary>Gets or sets the passages to search.</summary>
    public required IList<RagPassage> Passages { get; set; }

    /// <summary>Gets or sets how many hits each mode returns.</summary>
    public int Limit { get; set; } = 5;
}
