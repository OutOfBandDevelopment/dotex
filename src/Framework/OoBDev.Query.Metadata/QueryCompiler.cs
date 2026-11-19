using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Compiles a <see cref="QueryRequest"/> against a kind into one SQL statement. Only fragments declared on the kind are written into the text; every request value is bound.
/// </summary>
public static class QueryCompiler
{
    private const int BucketLimit = 1000;

    private sealed record SortColumn(string Name, string Expr, bool Descending, FacetInfo? Facet);

    /// <summary>Compiles the request.</summary>
    /// <param name="kind">The kind from the registry.</param>
    /// <param name="request">The request.</param>
    /// <param name="dialect">The SQL dialect.</param>
    /// <param name="maxTop">The largest page.</param>
    /// <param name="cursors">Reads the <c>after</c> cursor; required when the request has one.</param>
    /// <returns>The statement and its bound values.</returns>
    /// <exception cref="QueryException">The request asks for something the kind does not declare.</exception>
    public static CompiledQuery Compile(KindInfo kind, QueryRequest request, IQueryDialect dialect, int maxTop, CursorCodec? cursors)
    {
        ArgumentNullException.ThrowIfNull(kind);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(dialect);

        var parameters = new SqlParameters();
        var top = Math.Clamp(request.Top, 1, maxTop);
        return request.Shape switch
        {
            "rows" => Rows(kind, request, dialect, parameters, top, cursors),
            "facets" => Facets(kind, request, dialect, parameters, top),
            "buckets" => Buckets(kind, request, dialect, parameters),
            "graph" => Graph(kind, request, dialect, parameters, top, null),
            _ => throw new QueryException("shape is rows, facets, buckets or graph."),
        };
    }

    private static CompiledQuery Rows(KindInfo kind, QueryRequest request, IQueryDialect dialect, SqlParameters parameters, int top, CursorCodec? cursors)
    {
        var from = $"FROM {kind.Base} {kind.Joins}".TrimEnd();
        var where = new List<string>();
        var distance = request.Near is { } near ? Near(kind, near, dialect, parameters, ref from, where) : null;
        var sorts = ResolveSort(kind, request, distance);
        var spec = string.Join(',', sorts.Take(sorts.Count - 1).Select(s => (s.Descending ? "-" : "") + s.Name));

        var columns = kind.Facets
            .Where(f => !f.Lazy || request.Select?.Contains(f.Name, StringComparer.OrdinalIgnoreCase) == true)
            .Select(f => $"{f.Expr} AS {dialect.Quote(f.Name)}")
            .Concat(distance is null ? [] : [$"{distance} AS {dialect.Quote("distance")}"])
            .Concat(sorts.Select((s, i) => $"{s.Expr} AS {dialect.Quote("__s" + i)}"));

        where.AddRange(FilterCompiler.Compile(kind, request, parameters, dialect));
        if (request.Related is { } related)
            where.Add(Relation(kind, related, parameters));
        var countSql = request.Count ? $"SELECT count(*) {from}{Where(where)}" : null;

        if (!string.IsNullOrEmpty(request.After))
        {
            var state = (cursors ?? throw new QueryException("The after cursor is not valid.")).Decode(request.After, kind.Name, spec)!;
            if (state.Values.Length != sorts.Count)
                throw new QueryException("The after cursor is not valid.");
            where.Add(After(sorts, state.Values, parameters));
        }

        var sql = new StringBuilder("SELECT ").AppendJoin(", ", columns).Append(' ').Append(from).Append(Where(where))
            .Append(" ORDER BY ").AppendJoin(", ", sorts.Select(s => s.Expr + dialect.Direction(s.Descending)))
            .Append(' ').Append(dialect.Limit(top + 1)).ToString();
        return new CompiledQuery(sql, parameters.Values, countSql, top, spec, sorts.Count);
    }

    private static string Near(KindInfo kind, NearClause near, IQueryDialect dialect, SqlParameters parameters, ref string from, List<string> where)
    {
        if (kind.Spaces is null || !kind.Spaces.TryGetValue(near.Space, out var space))
            throw new QueryException($"Kind '{kind.Name}' has no space '{near.Space}'. Spaces: {string.Join(", ", kind.Spaces?.Keys ?? [])}.");

        string distance;
        if (space.Metric == SpaceMetric.Haversine)
        {
            if (near.Lat is not { } lat || near.Lon is not { } lon)
                throw new QueryException($"Space '{space.Name}' needs an anchor position: lat and lon.");
            var a = Numeric(kind, space.A);
            var b = Numeric(kind, space.B);
            where.Add($"{a.Expr} IS NOT NULL AND {b.Expr} IS NOT NULL");
            distance = dialect.Haversine(a.Expr, b.Expr, parameters.Add(lat), parameters.Add(lon));
        }
        else
        {
            if (string.IsNullOrEmpty(near.Ref))
                throw new QueryException($"Space '{space.Name}' needs an anchor: ref, the key of a row.");
            var anchor = $"SELECT a.{space.VectorColumn} FROM {space.Table} a WHERE a.{space.IdColumn} = CAST({parameters.Add(near.Ref)} AS {space.IdType})";
            if (space.Candidates > 0)
            {
                var pool = dialect.VectorDistance("x." + space.VectorColumn, anchor);
                from += $" JOIN (SELECT x.{space.IdColumn} AS id, {pool} AS d FROM {space.Table} x ORDER BY {pool} {dialect.Limit(space.Candidates)}) sv ON sv.id = {kind.Key}";
                distance = "sv.d";
            }
            else
            {
                from += $" JOIN {space.Table} sv ON sv.{space.IdColumn} = {kind.Key}";
                distance = dialect.VectorDistance("sv." + space.VectorColumn, anchor);
            }
        }

        var max = near.Max ?? (double.IsNaN(space.DefaultMax) ? (double?)null : space.DefaultMax);
        if (max is { } limit)
            where.Add($"{distance} <= {parameters.Add(limit)}");
        return distance;
    }

    private static string Relation(KindInfo kind, RelatedClause related, SqlParameters parameters)
    {
        if (kind.Relations is null || !kind.Relations.TryGetValue(related.Via, out var sql))
            throw new QueryException($"Kind '{kind.Name}' has no relation '{related.Via}'. Relations: {string.Join(", ", kind.Relations?.Keys ?? [])}.");
        return "(" + sql.Replace("{k}", kind.Key, StringComparison.Ordinal).Replace("{anchor}", parameters.Add(related.Of), StringComparison.Ordinal) + ")";
    }

    private static CompiledQuery Facets(KindInfo kind, QueryRequest request, IQueryDialect dialect, SqlParameters parameters, int top)
    {
        if (!string.IsNullOrEmpty(request.After))
            throw new QueryException("shape facets is not paged.");

        var by = request.By?.FirstOrDefault() ?? "";
        var part = "";
        var colon = by.IndexOf(':', StringComparison.Ordinal);
        if (colon >= 0)
        {
            part = by[(colon + 1)..].ToLowerInvariant();
            by = by[..colon];
        }

        var facet = kind.Facet(by) ?? throw new QueryException("shape facets needs by: [facet].");
        var expr = facet.Expr;
        if (part.Length > 0)
        {
            if (facet.Format != "date")
                throw new QueryException($"Facet '{facet.Name}' is not a date, so it has no '{part}' part.");
            expr = dialect.DatePart(part, facet.Expr) ?? throw new QueryException($"Date part '{part}' is not supported.");
        }

        var where = FilterCompiler.Compile(kind, request, parameters, dialect).ToList();
        if (part.Length > 0)
            where.Add($"{facet.Expr} IS NOT NULL");
        var order = part.Length > 0 ? expr : $"count(*) DESC, {expr}";
        var sql = $"SELECT {expr} AS {dialect.Quote("value")}, count(*) AS {dialect.Quote("count")} FROM {kind.Base} {kind.Joins}".TrimEnd()
            + $"{Where(where)} GROUP BY {expr} ORDER BY {order} {dialect.Limit(top)}";
        return new CompiledQuery(sql, parameters.Values, null, top, "", 0);
    }

    private static CompiledQuery Buckets(KindInfo kind, QueryRequest request, IQueryDialect dialect, SqlParameters parameters)
    {
        if (!string.IsNullOrEmpty(request.After))
            throw new QueryException("shape buckets is not paged.");
        if (request.By is not { Length: 2 })
            throw new QueryException("shape buckets needs by: [facetA, facetB].");

        var a = Numeric(kind, request.By[0]);
        var b = Numeric(kind, request.By[1]);
        var where = FilterCompiler.Compile(kind, request, parameters, dialect).ToList();
        where.Add($"{a.Expr} IS NOT NULL AND {b.Expr} IS NOT NULL");
        var cell = parameters.Add(Math.Clamp(request.Cell ?? 1.0, 0.0001, 90));
        var sample = kind.Sample.Length > 0 ? kind.Sample : "NULL";
        var sql = $"SELECT avg({a.Expr}) AS {dialect.Quote("a")}, avg({b.Expr}) AS {dialect.Quote("b")}, count(*) AS {dialect.Quote("count")}, {sample} AS {dialect.Quote("sample")} "
            + $"FROM {kind.Base} {kind.Joins}".TrimEnd()
            + $"{Where(where)} GROUP BY {dialect.GridCell(a.Expr, cell)}, {dialect.GridCell(b.Expr, cell)} ORDER BY count(*) DESC {dialect.Limit(BucketLimit)}";
        return new CompiledQuery(sql, parameters.Values, null, BucketLimit, "", 0);
    }

    private static FacetInfo Numeric(KindInfo kind, string name)
    {
        var facet = kind.Facet(name) ?? throw new QueryException($"Kind '{kind.Name}' has no facet '{name}'.");
        if (facet.Type != "number")
            throw new QueryException($"Facet '{facet.Name}' is not numeric, so it cannot be a bucket axis.");
        return facet;
    }

    /// <summary>The strongest links of an edge kind that pass the filters; with a frontier, only links that touch it (one step of a walk).</summary>
    internal static CompiledQuery Graph(KindInfo kind, QueryRequest request, IQueryDialect dialect, SqlParameters parameters, int top, string[]? frontier)
    {
        var edge = kind.Edge ?? throw new QueryException($"Kind '{kind.Name}' is not made of links, so it has no graph.");
        var where = FilterCompiler.Compile(kind, request, parameters, dialect).ToList();
        var source = $"CAST({edge.Source} AS text)";
        var target = $"CAST({edge.Target} AS text)";
        if (frontier is not null)
        {
            var parameter = parameters.Add(frontier);
            where.Add($"({dialect.In(source, parameter)} OR {dialect.In(target, parameter)})");
        }

        var sql = $"SELECT {source} AS {dialect.Quote("source")}, {target} AS {dialect.Quote("target")}, ({edge.Weight}) AS {dialect.Quote("weight")} "
            + $"FROM {kind.Base} {kind.Joins}".TrimEnd()
            + $"{Where(where)} ORDER BY ({edge.Weight}) DESC, {source}, {target} {dialect.Limit(top)}";
        return new CompiledQuery(sql, parameters.Values, null, top, "", 0);
    }

    private static string Where(List<string> terms) => terms.Count == 0 ? "" : " WHERE " + string.Join(" AND ", terms);

    private static List<SortColumn> ResolveSort(KindInfo kind, QueryRequest request, string? distance)
    {
        var requested = request.Sort is { Length: > 0 }
            ? request.Sort
            : distance is not null ? ["distance"] : kind.DefaultSort.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var sorts = new List<SortColumn>();
        foreach (var raw in requested)
        {
            var descending = raw.StartsWith('-');
            var name = raw.TrimStart('-', '+');
            if (name.Equals("distance", StringComparison.OrdinalIgnoreCase))
            {
                if (distance is null)
                    throw new QueryException("Sort by distance needs a near clause.");
                if (sorts.All(s => s.Name != "distance"))
                    sorts.Add(new SortColumn("distance", distance, descending, null));
                continue;
            }

            var facet = kind.Facet(name) ?? throw new QueryException($"Kind '{kind.Name}' has no facet '{name}' to sort by.");
            if (!facet.Sortable)
                throw new QueryException($"Facet '{facet.Name}' cannot be sorted.");
            if (sorts.Any(s => s.Facet == facet))
                continue;
            sorts.Add(new SortColumn(facet.Name, facet.Expr, descending, facet));
        }

        sorts.Add(new SortColumn("", kind.Key, false, null));
        return sorts;
    }

    private static string After(List<SortColumn> sorts, JsonElement[] values, SqlParameters parameters)
    {
        var alternatives = new List<string>();
        for (var i = 0; i < sorts.Count; i++)
        {
            var parts = new List<string>();
            for (var j = 0; j < i; j++)
                parts.Add(values[j].ValueKind == JsonValueKind.Null
                    ? $"{sorts[j].Expr} IS NULL"
                    : $"{sorts[j].Expr} = {parameters.Add(FilterCompiler.Scalar(sorts[j].Facet, values[j]))}");

            var last = sorts[i];
            if (values[i].ValueKind == JsonValueKind.Null)
            {
                if (last.Facet is null)
                    throw new QueryException("The after cursor is not valid.");
                continue;
            }

            var bound = parameters.Add(FilterCompiler.Scalar(last.Facet, values[i]));
            var sign = last.Descending ? "<" : ">";
            parts.Add(last.Facet is null ? $"{last.Expr} {sign} {bound}" : $"({last.Expr} {sign} {bound} OR {last.Expr} IS NULL)");
            alternatives.Add("(" + string.Join(" AND ", parts) + ")");
        }

        return alternatives.Count == 0 ? "FALSE" : "(" + string.Join(" OR ", alternatives) + ")";
    }
}
