using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using OoBDev.Search.Requests;
using SearchRequest = OoBDev.Search.Requests.SearchRequest;

namespace OoBDev.OpenSearch;

/// <summary>Turns a <see cref="SearchRequest"/> into an OpenSearch query body; pure, so it is tested without a server.</summary>
internal static class KeywordQueryBuilder
{
    /// <summary>The offset a cursor encodes, 0 for none; a damaged cursor is an error.</summary>
    public static int Offset(string? cursor)
    {
        if (cursor is null)
        {
            return 0;
        }

        if (!int.TryParse(cursor, NumberStyles.None, CultureInfo.InvariantCulture, out var offset))
        {
            throw new ArgumentException("The cursor is not valid.", nameof(cursor));
        }

        return offset;
    }

    public static string Build(SearchRequest request, IEnumerable<string> textFields, int offset)
    {
        using var stream = new MemoryStream();
        using (var json = new Utf8JsonWriter(stream))
        {
            json.WriteStartObject();
            json.WriteNumber("from", offset);
            json.WriteNumber("size", request.PageSize);
            json.WriteBoolean("track_total_hits", true);
            WriteQuery(json, request, textFields);
            WriteSort(json, request.Sort);
            json.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteQuery(Utf8JsonWriter json, SearchRequest request, IEnumerable<string> textFields)
    {
        json.WriteStartObject("query");
        json.WriteStartObject("bool");
        var must = new List<FieldFilter>();
        var mustNot = request.Filters.Where(f => f.Negated).ToList();
        must.AddRange(request.Filters.Where(f => !f.Negated));

        json.WriteStartArray("must");
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            json.WriteStartObject();
            json.WriteStartObject("match_all");
            json.WriteEndObject();
            json.WriteEndObject();
        }
        else
        {
            json.WriteStartObject();
            json.WriteStartObject("simple_query_string");
            json.WriteString("query", request.Text);
            json.WriteStartArray("fields");
            foreach (var field in textFields)
            {
                json.WriteStringValue(field);
            }

            json.WriteEndArray();
            json.WriteString("default_operator", "and");
            json.WriteEndObject();
            json.WriteEndObject();
        }

        json.WriteEndArray();
        WriteFilters(json, "filter", must);
        WriteFilters(json, "must_not", mustNot);
        json.WriteEndObject();
        json.WriteEndObject();
    }

    private static void WriteFilters(Utf8JsonWriter json, string name, List<FieldFilter> filters)
    {
        if (filters.Count == 0)
        {
            return;
        }

        json.WriteStartArray(name);
        foreach (var filter in filters)
        {
            json.WriteStartObject();
            switch (filter.Comparator)
            {
                case FilterComparator.Equals:
                    json.WriteStartObject("term");
                    json.WriteString(filter.Name, filter.Value);
                    json.WriteEndObject();
                    break;
                case FilterComparator.Contains:
                    json.WriteStartObject("match");
                    json.WriteString(filter.Name, filter.Value);
                    json.WriteEndObject();
                    break;
                default:
                    json.WriteStartObject("range");
                    json.WriteStartObject(filter.Name);
                    json.WriteString(RangeKey(filter.Comparator), filter.Value);
                    json.WriteEndObject();
                    json.WriteEndObject();
                    break;
            }

            json.WriteEndObject();
        }

        json.WriteEndArray();
    }

    private static string RangeKey(FilterComparator comparator) => comparator switch
    {
        FilterComparator.Greater => "gt",
        FilterComparator.GreaterOrEqual => "gte",
        FilterComparator.Less => "lt",
        FilterComparator.LessOrEqual => "lte",
        _ => throw new ArgumentOutOfRangeException(nameof(comparator)),
    };

    private static void WriteSort(Utf8JsonWriter json, IReadOnlyList<string> sort)
    {
        if (sort.Count == 0)
        {
            return;
        }

        json.WriteStartArray("sort");
        foreach (var key in sort)
        {
            var descending = key.StartsWith('-');
            json.WriteStartObject();
            json.WriteString(descending ? key[1..] : key, descending ? "desc" : "asc");
            json.WriteEndObject();
        }

        json.WriteEndArray();
    }
}
