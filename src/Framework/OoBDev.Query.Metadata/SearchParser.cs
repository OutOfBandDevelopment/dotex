using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.Query.Metadata;

/// <summary>
/// Splits a search string into operators (<c>ext:pdf</c>, <c>-tag:x</c>, <c>size:&gt;10mb</c>) and plain words.
/// </summary>
public static partial class SearchParser
{
    [GeneratedRegex("""(?:-?[A-Za-z]+:(?:"[^"]*"|\S+))|"[^"]*"|\S+""")]
    private static partial Regex Token();

    [GeneratedRegex("""^(-)?([A-Za-z]{2,}):(.*)$""")]
    private static partial Regex Op();

    [GeneratedRegex("^(>=|<=|>|<|=)?(.*)$")]
    private static partial Regex Comparison();

    /// <summary>Parses a search string.</summary>
    /// <param name="search">The text; null is empty.</param>
    /// <returns>The operators and words.</returns>
    public static ParsedSearch Parse(string? search)
    {
        var operators = new List<SearchOperator>();
        var words = new List<string>();
        foreach (Match match in Token().Matches(search ?? ""))
        {
            var token = match.Value;
            var op = Op().Match(token);
            if (op.Success)
            {
                var raw = op.Groups[3].Value.Trim('"');
                var comparison = Comparison().Match(raw);
                operators.Add(new SearchOperator(op.Groups[2].Value.ToLowerInvariant(), comparison.Groups[1].Value, comparison.Groups[2].Value, op.Groups[1].Success));
            }
            else
            {
                words.Add(token.Trim('"'));
            }
        }

        return new ParsedSearch(operators, words.Where(w => w.Length > 0).ToList());
    }
}
