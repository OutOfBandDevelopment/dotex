using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using OoBDev.Search.Requests;

namespace OoBDev.Search;

/// <summary>
/// Parses the text form of a search into a <see cref="SearchRequest"/>: plain words stay free text, <c>name:value</c> tokens become filters.
/// </summary>
/// <remarks>
/// <c>-name:value</c> negates; a value may start with <c>&gt;</c>, <c>&gt;=</c>, <c>&lt;</c>, <c>&lt;=</c> or <c>~</c> (contains); double quotes group words
/// (<c>tag:"two words"</c>, <c>"exact phrase"</c> stays quoted in the text). Reserved names: <c>like</c> and <c>near</c> (anchor), <c>sort</c> (comma list), <c>top</c> and <c>after</c>.
/// </remarks>
public static class SearchTextParser
{
    /// <summary>Parses the text.</summary>
    /// <param name="text">The search text.</param>
    /// <returns>The request.</returns>
    /// <exception cref="FormatException"><c>top:</c> is not a positive whole number.</exception>
    public static SearchRequest Parse(string text)
    {
        var words = new List<string>();
        var filters = new List<FieldFilter>();
        var sort = new List<string>();
        string? like = null, near = null, after = null;
        var top = new SearchRequest().PageSize;

        foreach (var token in Tokenize(text))
        {
            var colon = token.StartsWith('"') ? -1 : token.IndexOf(':', StringComparison.Ordinal);
            if (colon <= 0 || colon == token.Length - 1)
            {
                words.Add(token);
                continue;
            }

            var negated = token[0] == '-';
            var name = token[(negated ? 1 : 0)..colon];
            var value = Unquote(token[(colon + 1)..]);
            if (name.Length == 0 || !IsName(name))
            {
                words.Add(token);
                continue;
            }

            if (!negated && Reserved(name, value, ref like, ref near, ref after, ref top, sort))
                continue;

            filters.Add(Filter(name, value, negated));
        }

        var anchor = like is null && near is null ? null : new SearchAnchor(LikeItemId: like, NearText: near);
        return new SearchRequest { Text = string.Join(' ', words), Filters = filters, Anchor = anchor, Sort = sort, PageSize = top, Cursor = after };
    }

    private static bool Reserved(string name, string value, ref string? like, ref string? near, ref string? after, ref int top, List<string> sort)
    {
        switch (name.ToLowerInvariant())
        {
            case "like": like = value; return true;
            case "near": near = value; return true;
            case "after": after = value; return true;
            case "sort": sort.AddRange(value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)); return true;
            case "top":
                if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out top) || top < 1)
                    throw new FormatException("top: must be a positive whole number.");
                return true;
            default: return false;
        }
    }

    private static FieldFilter Filter(string name, string value, bool negated)
    {
        foreach (var (prefix, comparator) in Prefixes)
            if (value.StartsWith(prefix, StringComparison.Ordinal) && value.Length > prefix.Length)
                return new FieldFilter(name, comparator, value[prefix.Length..], negated);
        return new FieldFilter(name, FilterComparator.Equals, value, negated);
    }

    private static readonly (string Prefix, FilterComparator Comparator)[] Prefixes =
    [
        (">=", FilterComparator.GreaterOrEqual), ("<=", FilterComparator.LessOrEqual), (">", FilterComparator.Greater), ("<", FilterComparator.Less), ("~", FilterComparator.Contains),
    ];

    private static bool IsName(string name)
    {
        foreach (var c in name)
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '.')
                return false;
        return true;
    }

    private static string Unquote(string value) => value.Length >= 2 && value[0] == '"' && value[^1] == '"' ? value[1..^1] : value;

    private static IEnumerable<string> Tokenize(string text)
    {
        var current = new StringBuilder();
        var quoted = false;
        foreach (var c in text)
        {
            if (c == '"')
                quoted = !quoted;
            if (char.IsWhiteSpace(c) && !quoted)
            {
                if (current.Length > 0)
                    yield return current.ToString();
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
            yield return current.ToString();
    }
}
