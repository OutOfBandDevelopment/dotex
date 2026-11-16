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
            _ => throw new QueryException("shape is rows or facets."),
        };
    }

    private static CompiledQuery Rows(KindInfo kind, QueryRequest request, IQueryDialect dialect, SqlParameters parameters, int top, CursorCodec? cursors)
    {
        var sorts = ResolveSort(kind, request);
        var spec = string.Join(',', sorts.Take(sorts.Count - 1).Select(s => (s.Descending ? "-" : "") + s.Name));

        var columns = kind.Facets
            .Where(f => !f.Lazy || request.Select?.Contains(f.Name, StringComparer.OrdinalIgnoreCase) == true)
            .Select(f => $"{f.Expr} AS {dialect.Quote(f.Name)}")
            .Concat(sorts.Select((s, i) => $"{s.Expr} AS {dialect.Quote("__s" + i)}"));

        var where = FilterCompiler.Compile(kind, request, parameters, dialect).ToList();
        var from = $"FROM {kind.Base} {kind.Joins}".TrimEnd();
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

    private static string Where(List<string> terms) => terms.Count == 0 ? "" : " WHERE " + string.Join(" AND ", terms);

    private static List<SortColumn> ResolveSort(KindInfo kind, QueryRequest request)
    {
        var requested = request.Sort is { Length: > 0 }
            ? request.Sort
            : kind.DefaultSort.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        var sorts = new List<SortColumn>();
        foreach (var raw in requested)
        {
            var descending = raw.StartsWith('-');
            var name = raw.TrimStart('-', '+');
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
