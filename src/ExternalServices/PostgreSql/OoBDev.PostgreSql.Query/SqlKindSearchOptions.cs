using System.ComponentModel.DataAnnotations;

namespace OoBDev.PostgreSql.Query;

/// <summary>Settings of <see cref="SqlKindSearchProvider"/>, bound from <see cref="SectionName"/>.</summary>
public sealed class SqlKindSearchOptions
{
    /// <summary>The configuration section.</summary>
    public const string SectionName = "OoBDev:Search:SqlKind";

    /// <summary>The query kind (<c>[QueryKind]</c> name) the provider searches.</summary>
    [Required]
    public string Kind { get; set; } = "";

    /// <summary>The facet whose value is the item id of a hit.</summary>
    [Required]
    public string IdFacet { get; set; } = "id";
}
