using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Marks a row type as a queryable kind. The SQL fragments are written by the application author; request text is never spliced into them.
/// </summary>
/// <param name="name">The kind name clients send in <c>from</c>.</param>
[AttributeUsage(AttributeTargets.Class)]
public sealed class QueryKindAttribute(string name) : Attribute
{
    /// <summary>The kind name.</summary>
    public string Name { get; } = name;

    /// <summary>A label for clients.</summary>
    public string Title { get; init; } = name;

    /// <summary>The table, view or sub-select the rows come from, including its alias (for example <c>photos r</c>).</summary>
    public required string Base { get; init; }

    /// <summary>Joins that supply the other facets.</summary>
    public string Joins { get; init; } = "";

    /// <summary>SQL expression of the stable, non-null, unique key of a row. It is the last sort column and the paging tie-breaker.</summary>
    public required string Key { get; init; }

    /// <summary>Comma separated facets used when the request has no sort, a leading <c>-</c> for descending.</summary>
    public string DefaultSort { get; init; } = "";

    /// <summary>The application right a caller needs to query the kind; empty means any caller.</summary>
    public string Right { get; init; } = "";

    /// <summary>Aggregate SQL expression that picks one representative per bucket (for example <c>min(r.sha)</c>); empty gives a null sample.</summary>
    public string Sample { get; init; } = "";
}
