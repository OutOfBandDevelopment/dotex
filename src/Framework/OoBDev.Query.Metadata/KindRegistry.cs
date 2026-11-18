using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Reads the attributes of every row type in the given assemblies. This is the only place kinds are described; the compiler, the describe call and the OpenAPI document all read it.
/// </summary>
public sealed class KindRegistry
{
    private readonly Dictionary<string, KindInfo> _kinds = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Scans the assemblies for types marked with <see cref="QueryKindAttribute"/>.</summary>
    /// <param name="assemblies">The assemblies to scan.</param>
    public KindRegistry(params Assembly[] assemblies)
    {
        foreach (var type in assemblies.SelectMany(a => a.GetTypes()))
        {
            var attribute = type.GetCustomAttribute<QueryKindAttribute>();
            if (attribute is not null)
                _kinds[attribute.Name] = Build(type, attribute);
        }
    }

    /// <summary>All kinds, by name.</summary>
    public IEnumerable<KindInfo> Kinds => _kinds.Values.OrderBy(k => k.Name, StringComparer.Ordinal);

    /// <summary>Finds a kind.</summary>
    /// <param name="name">The kind name.</param>
    /// <returns>The kind.</returns>
    /// <exception cref="QueryException">The kind is unknown; the message lists the known kinds.</exception>
    public KindInfo Get(string name) => _kinds.TryGetValue(name, out var kind)
        ? kind
        : throw new QueryException($"Unknown kind '{name}'. Known kinds: {string.Join(", ", _kinds.Keys.Order(StringComparer.Ordinal))}.");

    private static KindInfo Build(Type type, QueryKindAttribute attribute)
    {
        var facets = new List<FacetInfo>();
        foreach (var property in type.GetProperties())
        {
            var facet = property.GetCustomAttribute<FacetAttribute>();
            if (facet is null)
                continue;
            var name = char.ToLowerInvariant(property.Name[0]) + property.Name[1..];
            facets.Add(new FacetInfo(name, facet.Expr, facet.Label.Length > 0 ? facet.Label : name, facet.Format, TypeName(property.PropertyType),
                facet.Filterable, facet.Sortable, facet.Search, facet.Role, facet.Hidden, facet.Lazy));
        }

        var predicates = type.GetCustomAttributes<PredicateAttribute>().GroupBy(p => p.Name)
            .ToDictionary(g => g.Key, g => (IReadOnlyDictionary<string, string>)g.ToDictionary(p => p.Value, p => p.Sql, StringComparer.OrdinalIgnoreCase), StringComparer.OrdinalIgnoreCase);
        var operators = type.GetCustomAttributes<SearchOperatorAttribute>().ToDictionary(o => o.Name, o => o.Sql, StringComparer.OrdinalIgnoreCase);
        var hints = type.GetCustomAttributes<HintAttribute>().ToDictionary(h => h.Name, h => h.Sql, StringComparer.OrdinalIgnoreCase);
        var relations = type.GetCustomAttributes<RelationAttribute>().ToDictionary(r => r.Name, r => r.Sql, StringComparer.OrdinalIgnoreCase);
        return new KindInfo(attribute.Name, type, attribute.Title, attribute.Base, attribute.Joins, attribute.Key, attribute.DefaultSort, attribute.Right, facets, predicates, operators, attribute.Sample, hints, relations);
    }

    private static string TypeName(Type type)
    {
        type = Nullable.GetUnderlyingType(type) ?? type;
        if (type == typeof(bool))
            return "boolean";
        if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(float) || type == typeof(decimal) || type == typeof(short))
            return "number";
        return type.IsArray ? "array" : "string";
    }
}
