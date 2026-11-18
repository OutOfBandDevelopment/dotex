using System;
using System.Globalization;
using OoBDev.Query.Metadata;

namespace OoBDev.PostgreSql.Query;

/// <summary>
/// The PostgreSQL spelling of the metadata query SQL: <c>ILIKE</c>, <c>= ANY(array)</c>, <c>NULLS LAST</c> and <c>to_char</c> date parts.
/// </summary>
public sealed class PostgresQueryDialect : IQueryDialect
{
    /// <inheritdoc />
    public string Quote(string name) => "\"" + name.Replace("\"", "\"\"", StringComparison.Ordinal) + "\"";

    /// <inheritdoc />
    public string Like(string expr, string parameter) => $"{expr} ILIKE {parameter} ESCAPE '\\'";

    /// <inheritdoc />
    public string NotLike(string expr, string parameter) => $"{expr} NOT ILIKE {parameter} ESCAPE '\\'";

    /// <inheritdoc />
    public string In(string expr, string parameter) => $"{expr} = ANY({parameter})";

    /// <inheritdoc />
    public string ArrayContains(string parameter, string expr) => $"{parameter} = ANY({expr})";

    /// <inheritdoc />
    public string Direction(bool descending) => descending ? " DESC NULLS LAST" : " ASC NULLS LAST";

    /// <inheritdoc />
    public string Limit(int rows) => string.Create(CultureInfo.InvariantCulture, $"LIMIT {rows}");

    /// <inheritdoc />
    public string? DatePart(string part, string expr) => part switch
    {
        "year" => $"to_char({expr}, 'YYYY')",
        "month" => $"to_char({expr}, 'YYYY-MM')",
        "day" => $"to_char({expr}, 'YYYY-MM-DD')",
        _ => null,
    };

    /// <inheritdoc />
    public string GridCell(string expr, string cellParameter) => $"floor(({expr}) / {cellParameter})";

    /// <inheritdoc />
    public string Haversine(string lat, string lon, string latParameter, string lonParameter) =>
        $"(6371.0 * acos(least(1.0, greatest(-1.0, cos(radians({lat})) * cos(radians({latParameter})) * cos(radians({lonParameter}) - radians({lon})) + sin(radians({lat})) * sin(radians({latParameter}))))))";

    /// <inheritdoc />
    public string VectorDistance(string column, string anchor) => $"(({column}) <=> ({anchor}))";

    /// <inheritdoc />
    public string ContainsPattern(string text) =>
        "%" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%";
}
