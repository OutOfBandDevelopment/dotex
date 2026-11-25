namespace OoBDev.Query.Metadata;

/// <summary>
/// A facet as the registry sees it.
/// </summary>
/// <param name="Name">The camel-cased property name.</param>
/// <param name="Expr">The SQL expression.</param>
/// <param name="Label">The label.</param>
/// <param name="Format">The format.</param>
/// <param name="Type">The wire type: string, number, boolean or array.</param>
/// <param name="Filterable">Whether it may be filtered.</param>
/// <param name="Sortable">Whether it may be sorted.</param>
/// <param name="Search">Whether plain words match it.</param>
/// <param name="Role">The role for generic renderers.</param>
/// <param name="Hidden">Whether clients hide it from column lists.</param>
/// <param name="Lazy">Whether it is returned only on request.</param>
public sealed record FacetInfo(string Name, string Expr, string Label, string Format, string Type, bool Filterable, bool Sortable, bool Search, string Role, bool Hidden, bool Lazy);
