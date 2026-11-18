using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.Query.Metadata;

/// <summary>
/// The text spelling of a query: an optional leading kind word, the search text, then the keyword clauses
/// <c>as facets by x</c>, <c>by -f,g</c>, <c>top n</c> and <c>count</c>. Everything before the first keyword is the search text.
/// </summary>
public static class QueryText
{
    private static readonly HashSet<string> Keywords = new(StringComparer.OrdinalIgnoreCase) { "as", "by", "top", "count" };

    private sealed record Token(string Text, string Raw, bool Quoted);

    /// <summary>Parses a query text into the same request the JSON form produces.</summary>
    /// <param name="text">The text.</param>
    /// <param name="registry">The kinds, to recognise a leading kind word.</param>
    /// <param name="defaultKind">The kind when the text does not start with one.</param>
    /// <returns>The request.</returns>
    /// <exception cref="QueryException">A clause is malformed.</exception>
    public static QueryRequest Parse(string text, KindRegistry registry, string defaultKind)
    {
        ArgumentNullException.ThrowIfNull(registry);
        var tokens = Tokenize(text ?? "");
        var kind = defaultKind;
        if (tokens.Count > 0 && !tokens[0].Quoted && registry.Kinds.Any(k => string.Equals(k.Name, tokens[0].Text, StringComparison.OrdinalIgnoreCase)))
        {
            kind = tokens[0].Text;
            tokens.RemoveAt(0);
        }

        var split = tokens.FindIndex(t => !t.Quoted && Keywords.Contains(t.Text));
        var words = split < 0 ? tokens : tokens[..split];
        var clauses = split < 0 ? [] : tokens[split..];
        var request = new QueryRequest { From = kind, Search = words.Count == 0 ? null : string.Join(' ', words.Select(t => t.Raw)) };
        return Clauses(request, clauses);
    }

    private static QueryRequest Clauses(QueryRequest request, List<Token> tokens)
    {
        var i = 0;
        while (i < tokens.Count)
        {
            switch (tokens[i].Text.ToLowerInvariant())
            {
                case "count":
                    request = request with { Count = true };
                    i++;
                    break;
                case "top":
                    if (i + 1 >= tokens.Count || !int.TryParse(tokens[i + 1].Text, NumberStyles.None, CultureInfo.InvariantCulture, out var n) || n < 1)
                        throw new QueryException("'top' needs a number, for example: top 50.");
                    request = request with { Top = n };
                    i += 2;
                    break;
                case "by":
                    if (i + 1 >= tokens.Count)
                        throw new QueryException("'by' needs facet names, for example: by -updated.");
                    var names = tokens[i + 1].Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    request = request.Shape is "facets" or "buckets" ? request with { By = names } : request with { Sort = names };
                    i += 2;
                    break;
                case "as":
                    const string usage = "'as' is followed by rows, facets by <facet> or buckets by <a>,<b> [cell <size>].";
                    if (i + 1 >= tokens.Count)
                        throw new QueryException(usage);
                    var shape = tokens[i + 1].Text.ToLowerInvariant();
                    if (shape == "rows")
                    {
                        request = request with { Shape = "rows" };
                        i += 2;
                    }
                    else if (shape == "facets" && i + 3 < tokens.Count && tokens[i + 2].Text.Equals("by", StringComparison.OrdinalIgnoreCase))
                    {
                        request = request with { Shape = "facets", By = tokens[i + 3].Text.Split(',', StringSplitOptions.RemoveEmptyEntries) };
                        i += 4;
                    }
                    else if (shape == "buckets" && i + 3 < tokens.Count && tokens[i + 2].Text.Equals("by", StringComparison.OrdinalIgnoreCase))
                    {
                        request = request with { Shape = "buckets", By = tokens[i + 3].Text.Split(',', StringSplitOptions.RemoveEmptyEntries) };
                        i += 4;
                        if (i < tokens.Count && tokens[i].Text.Equals("cell", StringComparison.OrdinalIgnoreCase))
                        {
                            if (i + 1 >= tokens.Count || !double.TryParse(tokens[i + 1].Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var cell) || cell <= 0)
                                throw new QueryException("'cell' needs a size, for example: cell 0.5.");
                            request = request with { Cell = cell };
                            i += 2;
                        }
                    }
                    else
                    {
                        throw new QueryException(usage);
                    }

                    break;
                default:
                    throw new QueryException($"Unexpected '{tokens[i].Text}' after the search text; expected as, by, top or count.");
            }
        }

        return request;
    }

    private static List<Token> Tokenize(string s)
    {
        var list = new List<Token>();
        var i = 0;
        while (i < s.Length)
        {
            if (char.IsWhiteSpace(s[i]))
            {
                i++;
                continue;
            }

            var start = i;
            var quoted = false;
            var open = false;
            while (i < s.Length && (open || !char.IsWhiteSpace(s[i])))
            {
                if (s[i] == '"')
                {
                    open = !open;
                    quoted = true;
                }

                i++;
            }

            var raw = s[start..i];
            list.Add(new Token(raw.Replace("\"", "", StringComparison.Ordinal), raw, quoted && raw.StartsWith('"') && raw.EndsWith('"')));
        }

        return list;
    }
}
