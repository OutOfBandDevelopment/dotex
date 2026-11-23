using System.ComponentModel.DataAnnotations;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Options of the metadata query engine.
/// </summary>
public sealed class QueryMetadataOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "Query:Metadata";

    /// <summary>The secret that signs paging cursors, at least 32 characters. A cursor signed with another key is rejected.</summary>
    [Required]
    [MinLength(32)]
    public string CursorKey { get; set; } = "";

    /// <summary>The largest page a request may ask for.</summary>
    [Range(1, 5000)]
    public int MaxTop { get; set; } = 500;

    /// <summary>The most filter entries a request may carry (the filter-size part of the cost guard).</summary>
    [Range(1, 1000)]
    public int MaxFilters { get; set; } = 50;

    /// <summary>The most values one filter entry may list.</summary>
    [Range(1, 10000)]
    public int MaxFilterValues { get; set; } = 100;

    /// <summary>The longest free text a request may carry, in characters.</summary>
    [Range(1, 100000)]
    public int MaxSearchLength { get; set; } = 1000;
}
