using System;
using System.Linq;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Projects the registry to the description clients may see. No SQL fragment leaves through it.
/// </summary>
public static class Describer
{
    /// <summary>Describes every kind.</summary>
    /// <param name="registry">The registry.</param>
    /// <returns>The description.</returns>
    public static Description Describe(KindRegistry registry) => new(registry.Kinds.Select(Describe).ToArray());

    /// <summary>Describes one kind.</summary>
    /// <param name="kind">The kind.</param>
    /// <returns>The description.</returns>
    public static KindDescription Describe(KindInfo kind) => new(
        kind.Name,
        kind.Title,
        kind.DefaultSort,
        kind.Facets.Select(f => new FacetDescription(f.Name, f.Label, f.Format, f.Type, f.Filterable, f.Sortable, f.Search, f.Role, f.Hidden)).ToArray(),
        kind.Predicates.ToDictionary(p => p.Key, p => p.Value.Keys.ToArray(), StringComparer.OrdinalIgnoreCase),
        kind.Operators.Keys.Order(StringComparer.Ordinal).ToArray(),
        (kind.Hints?.Keys ?? []).Order(StringComparer.Ordinal).ToArray());
}
