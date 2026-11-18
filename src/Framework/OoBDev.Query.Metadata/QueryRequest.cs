using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.Query.Metadata;

/// <summary>
/// One query body for every kind. The JSON form and the text form (<see cref="QueryText"/>) produce the same request.
/// </summary>
public sealed record QueryRequest
{
    /// <summary>The kind to query.</summary>
    public required string From { get; init; }

    /// <summary>Operators (<c>ext:pdf</c>, <c>-tag:x</c>, <c>size:&gt;10mb</c>) and plain words matched against the searchable facets.</summary>
    public string? Search { get; init; }

    /// <summary>Facet to value (equals), to an array (in) or to an object of operators (eq, ne, gt, gte, lt, lte, like, nlike, in, between, exists); predicate names take their allowed value.</summary>
    public Dictionary<string, JsonElement>? Filters { get; init; }

    /// <summary>Facet names, a leading <c>-</c> for descending.</summary>
    public string[]? Sort { get; init; }

    /// <summary>Names of lazy facets to include.</summary>
    public string[]? Select { get; init; }

    /// <summary>Page size, clamped to the configured maximum.</summary>
    public int Top { get; init; } = 60;

    /// <summary>The <see cref="QueryResult{T}.Next"/> of the previous page.</summary>
    public string? After { get; init; }

    /// <summary><c>rows</c> or <c>facets</c> (a count per value of <see cref="By"/>[0]).</summary>
    public string Shape { get; init; } = "rows";

    /// <summary>The facet to group by for the facets shape; <c>taken:month</c> groups a date facet by a part.</summary>
    public string[]? By { get; init; }

    /// <summary>Also return the number of rows that match the filters.</summary>
    public bool Count { get; init; }

    /// <summary>Grid cell size in the units of the two <see cref="By"/> facets for shape <c>buckets</c> (for example degrees); default 1.</summary>
    public double? Cell { get; init; }

    /// <summary>Only rows related to an anchor row through a relation the kind declares.</summary>
    public RelatedClause? Related { get; init; }
}
