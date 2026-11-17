namespace OoBDev.Query.Metadata;

/// <summary>A facet as a client may see it: labels and flags, never SQL.</summary>
/// <param name="Name">The facet name.</param>
/// <param name="Label">The label.</param>
/// <param name="Format">The format.</param>
/// <param name="Type">The wire type.</param>
/// <param name="Filterable">Whether it may be filtered.</param>
/// <param name="Sortable">Whether it may be sorted.</param>
/// <param name="Search">Whether plain words match it.</param>
/// <param name="Role">The role.</param>
/// <param name="Hidden">Whether it is hidden from column lists.</param>
public sealed record FacetDescription(string Name, string Label, string Format, string Type, bool Filterable, bool Sortable, bool Search, string Role, bool Hidden);
