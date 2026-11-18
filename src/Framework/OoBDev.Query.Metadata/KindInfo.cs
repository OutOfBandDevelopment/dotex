using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Query.Metadata;

/// <summary>
/// A kind as the registry sees it, built once from the attributes of its row type.
/// </summary>
/// <param name="Name">The kind name.</param>
/// <param name="RowType">The row class.</param>
/// <param name="Title">The label.</param>
/// <param name="Base">The source and its alias.</param>
/// <param name="Joins">The joins.</param>
/// <param name="Key">The key expression.</param>
/// <param name="DefaultSort">The default sort.</param>
/// <param name="Right">The required application right, or empty.</param>
/// <param name="Facets">The facets.</param>
/// <param name="Predicates">Predicate name to allowed value to condition.</param>
/// <param name="Operators">Search operator name to condition.</param>
/// <param name="Sample">A sample expression for the buckets shape.</param>
/// <param name="Hints">Hint name to boolean expression.</param>
/// <param name="Relations">Relation name to condition.</param>
public sealed record KindInfo(
    string Name,
    Type RowType,
    string Title,
    string Base,
    string Joins,
    string Key,
    string DefaultSort,
    string Right,
    IReadOnlyList<FacetInfo> Facets,
    IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> Predicates,
    IReadOnlyDictionary<string, string> Operators,
    string Sample = "",
    IReadOnlyDictionary<string, string>? Hints = null,
    IReadOnlyDictionary<string, string>? Relations = null)
{
    /// <summary>Finds a facet by name, ignoring case.</summary>
    /// <param name="name">The facet name.</param>
    /// <returns>The facet, or null.</returns>
    public FacetInfo? Facet(string name) => Facets.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
}
