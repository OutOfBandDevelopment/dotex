using System.Collections.Generic;

namespace OoBDev.Query.Metadata;

/// <summary>A kind as a client may see it.</summary>
/// <param name="Name">The kind name.</param>
/// <param name="Title">The label.</param>
/// <param name="DefaultSort">The default sort.</param>
/// <param name="Facets">The facets.</param>
/// <param name="Predicates">Predicate names with their allowed values.</param>
/// <param name="Operators">Search operator names.</param>
public sealed record KindDescription(string Name, string Title, string DefaultSort, IReadOnlyList<FacetDescription> Facets, IReadOnlyDictionary<string, string[]> Predicates, string[] Operators, string[] Hints);
