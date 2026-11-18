namespace OoBDev.Query.Metadata;

/// <summary>A space as the registry holds it.</summary>
/// <param name="Name">The name.</param>
/// <param name="Metric">The metric.</param>
/// <param name="A">Geo latitude facet.</param>
/// <param name="B">Geo longitude facet.</param>
/// <param name="Table">Vector table.</param>
/// <param name="IdColumn">Vector key column.</param>
/// <param name="IdType">Anchor key SQL type.</param>
/// <param name="VectorColumn">Vector column.</param>
/// <param name="DefaultMax">The default farthest distance, or NaN.</param>
public sealed record SpaceInfo(string Name, SpaceMetric Metric, string A, string B, string Table, string IdColumn, string IdType, string VectorColumn, double DefaultMax);
