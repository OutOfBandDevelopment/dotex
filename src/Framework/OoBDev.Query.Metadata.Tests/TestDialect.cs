using System;
using System.Globalization;

namespace OoBDev.Query.Metadata.Tests;

internal sealed class TestDialect : IQueryDialect
{
    public string Quote(string name) => "\"" + name + "\"";

    public string Like(string expr, string parameter) => $"{expr} ILIKE {parameter}";

    public string NotLike(string expr, string parameter) => $"{expr} NOT ILIKE {parameter}";

    public string In(string expr, string parameter) => $"{expr} = ANY({parameter})";

    public string ArrayContains(string parameter, string expr) => $"{parameter} = ANY({expr})";

    public string Direction(bool descending) => descending ? " DESC NULLS LAST" : " ASC NULLS LAST";

    public string Limit(int rows) => string.Create(CultureInfo.InvariantCulture, $"LIMIT {rows}");

    public string? DatePart(string part, string expr) => part == "month" ? $"to_char({expr}, 'YYYY-MM')" : null;

    public string ContainsPattern(string text) => "%" + text.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("%", "\\%", StringComparison.Ordinal).Replace("_", "\\_", StringComparison.Ordinal) + "%";
}
