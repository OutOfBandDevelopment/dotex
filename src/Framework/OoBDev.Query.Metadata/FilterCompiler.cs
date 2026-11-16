using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Turns the <c>filters</c> object and the <c>search</c> text into WHERE terms. Facet expressions come from the registry; request values are always bound.
/// </summary>
internal static partial class FilterCompiler
{
    [GeneratedRegex(@"^(\d+(?:\.\d+)?)\s*(b|kb|mb|gb|tb)?$", RegexOptions.IgnoreCase)]
    private static partial Regex ByteSize();

    public static IEnumerable<string> Compile(KindInfo kind, QueryRequest request, SqlParameters parameters, IQueryDialect dialect)
    {
        foreach (var (name, value) in request.Filters ?? [])
        {
            if (kind.Predicates.TryGetValue(name, out var allowed))
            {
                yield return Predicate(name, value, allowed, kind.Key);
                continue;
            }

            var facet = kind.Facet(name) ?? throw new QueryException($"Kind '{kind.Name}' has no filter '{name}'. Facets: {Names(kind.Facets.Where(f => f.Filterable))}.");
            if (!facet.Filterable)
                throw new QueryException($"Facet '{facet.Name}' cannot be filtered.");
            yield return Facet(facet, value, parameters, dialect);
        }

        var parsed = SearchParser.Parse(request.Search);
        foreach (var op in parsed.Operators)
            yield return OperatorTerm(kind, op, parameters, dialect);

        foreach (var word in parsed.Words.Take(8))
        {
            var searchable = kind.Facets.Where(f => f.Search).ToArray();
            if (searchable.Length == 0)
                throw new QueryException($"Kind '{kind.Name}' has no searchable facets.");
            var pattern = parameters.Add(dialect.ContainsPattern(word));
            yield return "(" + string.Join(" OR ", searchable.Select(f => dialect.Like(f.Expr, pattern))) + ")";
        }
    }

    public static object? Scalar(FacetInfo? facet, JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Number when facet is null || facet.Type == "number" => value.TryGetInt64(out var l) ? (object)l : value.GetDouble(),
        JsonValueKind.Number => value.GetRawText(),
        JsonValueKind.String when facet?.Format == "date" => DateTime.TryParse(value.GetString(), CultureInfo.InvariantCulture,
            DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out var d)
            ? DateTime.SpecifyKind(d, DateTimeKind.Unspecified)
            : throw new QueryException($"'{value.GetString()}' is not a date for '{facet.Name}'."),
        JsonValueKind.String => value.GetString(),
        _ => throw new QueryException($"A filter on '{facet?.Name ?? "the key"}' needs a simple value."),
    };

    private static string Names(IEnumerable<FacetInfo> facets) => string.Join(", ", facets.Select(f => f.Name));

    private static string Predicate(string name, JsonElement value, IReadOnlyDictionary<string, string> allowed, string key)
    {
        var text = value.ValueKind == JsonValueKind.String ? value.GetString() ?? "" : "";
        if (text.Length == 0)
            return "TRUE";
        return allowed.TryGetValue(text, out var sql)
            ? "(" + sql.Replace("{k}", key, StringComparison.Ordinal) + ")"
            : throw new QueryException($"Filter '{name}' takes {string.Join(" or ", allowed.Keys)}.");
    }

    private static string Facet(FacetInfo facet, JsonElement value, SqlParameters parameters, IQueryDialect dialect)
    {
        if (value.ValueKind != JsonValueKind.Object)
            return Operator(facet, value.ValueKind == JsonValueKind.Array ? "in" : "eq", value, parameters, dialect);

        var parts = value.EnumerateObject().Select(o => Operator(facet, o.Name, o.Value, parameters, dialect)).ToList();
        return parts.Count == 0 ? "TRUE" : "(" + string.Join(" AND ", parts) + ")";
    }

    private static string Operator(FacetInfo f, string op, JsonElement v, SqlParameters p, IQueryDialect d)
    {
        switch (op)
        {
            case "eq" when v.ValueKind == JsonValueKind.Null: return $"{f.Expr} IS NULL";
            case "eq": return f.Type == "array" ? d.ArrayContains(p.Add(Scalar(f, v)), f.Expr) : $"{f.Expr} = {p.Add(Scalar(f, v))}";
            case "ne" when v.ValueKind == JsonValueKind.Null: return $"{f.Expr} IS NOT NULL";
            case "ne": return $"{f.Expr} IS DISTINCT FROM {p.Add(Scalar(f, v))}";
            case "gt": return $"{f.Expr} > {p.Add(Scalar(f, v))}";
            case "gte": return $"{f.Expr} >= {p.Add(Scalar(f, v))}";
            case "lt": return $"{f.Expr} < {p.Add(Scalar(f, v))}";
            case "lte": return $"{f.Expr} <= {p.Add(Scalar(f, v))}";
            case "like": return d.Like(f.Expr, p.Add(d.ContainsPattern(v.GetString() ?? "")));
            case "nlike": return $"({f.Expr} IS NULL OR {d.NotLike(f.Expr, p.Add(d.ContainsPattern(v.GetString() ?? "")))})";
            case "exists": return v.ValueKind == JsonValueKind.True ? $"{f.Expr} IS NOT NULL" : $"{f.Expr} IS NULL";
            case "in": return d.In(f.Expr, p.Add(List(f, v)));
            case "between":
                var items = v.ValueKind == JsonValueKind.Array ? v.EnumerateArray().ToArray() : [];
                if (items.Length != 2)
                    throw new QueryException("between takes [low, high].");
                return $"({f.Expr} BETWEEN {p.Add(Scalar(f, items[0]))} AND {p.Add(Scalar(f, items[1]))})";
            default:
                throw new QueryException($"Unknown operator '{op}' on '{f.Name}'. Use eq, ne, gt, gte, lt, lte, like, nlike, in, between or exists.");
        }
    }

    private static object List(FacetInfo f, JsonElement v)
    {
        if (v.ValueKind != JsonValueKind.Array)
            throw new QueryException($"'in' on '{f.Name}' takes an array.");
        var items = v.EnumerateArray().Select(e => Scalar(f, e)).ToArray();
        if (items.All(i => i is long))
            return items.Select(i => (long)i!).ToArray();
        if (items.All(i => i is long or double))
            return items.Select(Convert.ToDouble).ToArray();
        return items.Select(i => i?.ToString() ?? "").ToArray();
    }

    private static string OperatorTerm(KindInfo kind, SearchOperator op, SqlParameters p, IQueryDialect d)
    {
        string term;
        if (kind.Predicates.TryGetValue(op.Name, out var values))
        {
            term = Predicate(op.Name, JsonSerializer.SerializeToElement(op.Value), values, kind.Key);
        }
        else if (kind.Operators.TryGetValue(op.Name, out var sql))
        {
            term = "(" + sql.Replace("{k}", kind.Key, StringComparison.Ordinal)
                .Replace("{v}", sql.Contains("{v}", StringComparison.Ordinal) ? p.Add(d.ContainsPattern(op.Value)) : "", StringComparison.Ordinal)
                .Replace("{x}", sql.Contains("{x}", StringComparison.Ordinal) ? p.Add(op.Value.ToLowerInvariant()) : "", StringComparison.Ordinal) + ")";
        }
        else
        {
            var facet = kind.Facet(op.Name) ?? throw new QueryException(
                $"Kind '{kind.Name}' has no search operator '{op.Name}'. Use {string.Join(", ", kind.Facets.Where(x => x.Filterable).Select(x => x.Name).Concat(kind.Predicates.Keys).Concat(kind.Operators.Keys))}; quote the text to search for it as words.");
            if (!facet.Filterable)
                throw new QueryException($"Facet '{facet.Name}' cannot be filtered.");
            term = FacetTerm(facet, op, p, d);
        }

        return op.Negated ? $"({term}) IS NOT TRUE" : term;
    }

    private static string FacetTerm(FacetInfo f, SearchOperator o, SqlParameters p, IQueryDialect d)
    {
        var comparator = o.Comparator;
        if (f.Format == "date")
        {
            var (from, to) = Period(f, o.Value);
            return comparator switch
            {
                ">" => $"{f.Expr} >= {p.Add(to)}",
                ">=" => $"{f.Expr} >= {p.Add(from)}",
                "<" => $"{f.Expr} < {p.Add(from)}",
                "<=" => $"{f.Expr} < {p.Add(to)}",
                _ => $"({f.Expr} >= {p.Add(from)} AND {f.Expr} < {p.Add(to)})",
            };
        }

        object value;
        if (f.Format == "bytes")
            value = Bytes(f, o.Value);
        else if (f.Type == "number")
            value = double.TryParse(o.Value, CultureInfo.InvariantCulture, out var n)
                ? (n == Math.Floor(n) && Math.Abs(n) < 1e15 ? (object)(long)n : n)
                : throw new QueryException($"'{o.Value}' is not a number for '{f.Name}'.");
        else if (f.Type == "boolean")
            value = o.Value.Equals("true", StringComparison.OrdinalIgnoreCase) ? true
                : o.Value.Equals("false", StringComparison.OrdinalIgnoreCase) ? false
                : throw new QueryException($"'{o.Value}' is not true or false for '{f.Name}'.");
        else if (comparator.Length == 0)
            return f.Search ? d.Like(f.Expr, p.Add(d.ContainsPattern(o.Value))) : $"lower({f.Expr}) = {p.Add(o.Value.ToLowerInvariant())}";
        else
            value = o.Value;

        var sign = comparator switch { ">" => ">", ">=" => ">=", "<" => "<", "<=" => "<=", _ => "=" };
        return $"{f.Expr} {sign} {p.Add(value)}";
    }

    private static long Bytes(FacetInfo f, string text)
    {
        var match = ByteSize().Match(text.Trim());
        if (!match.Success)
            throw new QueryException($"'{text}' is not a size for '{f.Name}'. Use a number with b, kb, mb, gb or tb.");
        var unit = match.Groups[2].Value.ToLowerInvariant() switch { "kb" => 1L << 10, "mb" => 1L << 20, "gb" => 1L << 30, "tb" => 1L << 40, _ => 1L };
        return (long)(double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture) * unit);
    }

    private static (DateTime From, DateTime To) Period(FacetInfo f, string text)
    {
        var s = text.Trim();
        var ci = CultureInfo.InvariantCulture;
        if (s.Length == 4 && int.TryParse(s, CultureInfo.InvariantCulture, out var year) && year is > 0 and < 10000)
            return (new DateTime(year, 1, 1), new DateTime(year, 1, 1).AddYears(1));
        if (DateTime.TryParseExact(s, "yyyy-MM", ci, DateTimeStyles.None, out var month))
            return (month, month.AddMonths(1));
        if (DateTime.TryParseExact(s, "yyyy-MM-dd", ci, DateTimeStyles.None, out var day))
            return (day, day.AddDays(1));
        throw new QueryException($"'{text}' is not a date for '{f.Name}'. Use 2024, 2024-05 or 2024-05-17.");
    }
}
