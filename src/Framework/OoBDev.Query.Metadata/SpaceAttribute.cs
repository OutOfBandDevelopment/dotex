using System;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A proximity space of a kind: how far a row is from an anchor. A geo space computes from two numeric facets; a vector space joins an embedding table.
/// Requests use it with <c>near</c>, which adds a <c>distance</c> column and the <c>distance</c> sort.
/// </summary>
/// <param name="name">The space name.</param>
/// <param name="metric">How distance is measured.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class SpaceAttribute(string name, SpaceMetric metric) : Attribute
{
    /// <summary>The space name.</summary>
    public string Name { get; } = name;

    /// <summary>The metric.</summary>
    public SpaceMetric Metric { get; } = metric;

    /// <summary>Geo: the numeric facet holding the latitude.</summary>
    public string A { get; init; } = "";

    /// <summary>Geo: the numeric facet holding the longitude.</summary>
    public string B { get; init; } = "";

    /// <summary>Vector: the embedding table; it is joined to the kind as <c>sv</c>.</summary>
    public string Table { get; init; } = "";

    /// <summary>Vector: the table column that holds the kind's key.</summary>
    public string IdColumn { get; init; } = "";

    /// <summary>Vector: the SQL type the anchor key is cast to.</summary>
    public string IdType { get; init; } = "bigint";

    /// <summary>Vector: the vector column.</summary>
    public string VectorColumn { get; init; } = "";

    /// <summary>
    /// Vector: when above zero, the nearest this many rows are taken from the embedding table first (<c>ORDER BY distance LIMIT n</c> on the table alone, which lets a vector index such as HNSW answer it)
    /// and the kind's filters apply to that pool. Paging then covers the pool only, and a filter can leave fewer rows than asked for. Zero joins the whole table.
    /// </summary>
    public int Candidates { get; init; }

    /// <summary>A farthest distance applied when the request gives none; unset means no limit.</summary>
    public double DefaultMax { get; init; } = double.NaN;
}
