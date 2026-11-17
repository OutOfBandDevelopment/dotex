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
}
