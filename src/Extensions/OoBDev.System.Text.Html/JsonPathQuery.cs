using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

namespace OoBDev.System.Text.Html;

/// <summary>
/// A small JSONPath subset over <see cref="JsonNode"/>: <c>$</c> (root), <c>@</c> (current), <c>.name</c>,
/// <c>name</c>, <c>['name']</c>, <c>[n]</c>, <c>[*]</c> and <c>[?(expression)]</c> filters.
/// </summary>
/// <remarks>
/// A filter or rule expression is either <c>left op right</c> (<c>==  !=  &gt;=  &lt;=  &gt;  &lt;</c>) or a bare path that must
/// select a truthy value. The right side is a quoted string, a number, <c>true</c>, <c>false</c>, <c>null</c> or a path.
/// A missing property yields no match; a JSON null yields a <see langword="null"/> match.
/// </remarks>
internal static class JsonPathQuery
{
    private static readonly string[] Operators = ["==", "!=", ">=", "<=", ">", "<"];

    public static IReadOnlyList<JsonNode?> Select(JsonNode? root, JsonNode? current, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return [current];

        var text = path.Trim();
        var i = 0;
        JsonNode? start = current;
        if (text[0] == '$') { start = root; i = 1; }
        else if (text[0] == '@') { start = current; i = 1; }

        IReadOnlyList<JsonNode?> nodes = [start];
        while (i < text.Length && nodes.Count > 0)
        {
            var c = text[i];
            if (c == '.')
            {
                i++;
                continue;
            }

            if (c == '[')
            {
                var end = FindClosingBracket(text, i);
                var inner = text[(i + 1)..end].Trim();
                i = end + 1;
                nodes = ApplyBracket(root, nodes, inner);
            }
            else
            {
                var begin = i;
                while (i < text.Length && text[i] != '.' && text[i] != '[') i++;
                nodes = Child(nodes, text[begin..i]);
            }
        }

        return nodes;
    }

    public static bool Test(JsonNode? root, JsonNode? node, string? rule)
    {
        if (string.IsNullOrWhiteSpace(rule))
            return true;

        return Evaluate(root, node, rule) || Descendants(node).Any(d => Evaluate(root, d, rule));
    }

    private static bool Evaluate(JsonNode? root, JsonNode? node, string expression)
    {
        var (index, op) = FindOperator(expression);
        if (index < 0)
            return Select(root, node, expression).Any(Truthy);

        var left = Select(root, node, expression[..index]);
        var right = Operand(root, node, expression[(index + op!.Length)..].Trim());
        if (left.Count == 0)
            return op == "!=";

        return Compare(left[0], right, op);
    }

    private static (int Index, string? Op) FindOperator(string expression)
    {
        var quote = '\0';
        for (var i = 0; i < expression.Length; i++)
        {
            var c = expression[i];
            if (quote != '\0') { if (c == quote) quote = '\0'; continue; }
            if (c is '\'' or '"') { quote = c; continue; }
            foreach (var op in Operators)
            {
                if (string.CompareOrdinal(expression, i, op, 0, op.Length) == 0)
                    return (i, op);
            }
        }

        return (-1, null);
    }

    private static object? Operand(JsonNode? root, JsonNode? node, string text)
    {
        if (text.Length >= 2 && (text[0] is '\'' or '"') && text[^1] == text[0])
            return text[1..^1];
        if (text is "true") return true;
        if (text is "false") return false;
        if (text is "null") return null;
        if (decimal.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            return number;

        var selected = Select(root, node, text);
        return selected.Count == 0 ? null : Plain(selected[0]);
    }

    private static bool Compare(JsonNode? left, object? right, string op)
    {
        var l = Plain(left);
        if (l is null || right is null)
            return op switch { "==" => l is null && right is null, "!=" => !(l is null && right is null), _ => false };

        int order;
        if (l is decimal ld && right is decimal rd) order = ld.CompareTo(rd);
        else if (l is bool lb && right is bool rb) order = lb.CompareTo(rb);
        else order = string.CompareOrdinal(Convert.ToString(l, CultureInfo.InvariantCulture), Convert.ToString(right, CultureInfo.InvariantCulture));

        return op switch
        {
            "==" => order == 0,
            "!=" => order != 0,
            ">=" => order >= 0,
            "<=" => order <= 0,
            ">" => order > 0,
            _ => order < 0,
        };
    }

    private static object? Plain(JsonNode? node)
    {
        if (node is not JsonValue value)
            return node?.ToJsonString();
        if (value.TryGetValue<bool>(out var b)) return b;
        if (value.TryGetValue<decimal>(out var d)) return d;
        return value.TryGetValue<string>(out var s) ? s : value.ToJsonString();
    }

    private static bool Truthy(JsonNode? node) => node switch
    {
        null => false,
        JsonValue v when v.TryGetValue<bool>(out var b) => b,
        JsonValue v when v.TryGetValue<string>(out var s) => s.Length > 0,
        _ => true,
    };

    private static IReadOnlyList<JsonNode?> Child(IEnumerable<JsonNode?> nodes, string name)
    {
        var result = new List<JsonNode?>();
        foreach (var node in nodes)
        {
            if (name == "*")
                result.AddRange(Children(node));
            else if (node is JsonObject obj && obj.TryGetPropertyValue(name, out var value))
                result.Add(value);
        }

        return result;
    }

    private static IReadOnlyList<JsonNode?> ApplyBracket(JsonNode? root, IEnumerable<JsonNode?> nodes, string inner)
    {
        if (inner == "*")
            return nodes.SelectMany(Children).ToList();

        if (inner.StartsWith("?(", StringComparison.Ordinal) && inner.EndsWith(')'))
        {
            var expression = inner[2..^1];
            return nodes.SelectMany(Children).Where(c => Evaluate(root, c, expression)).ToList();
        }

        if (inner.Length >= 2 && (inner[0] is '\'' or '"') && inner[^1] == inner[0])
            return Child(nodes, inner[1..^1]);

        if (int.TryParse(inner, NumberStyles.Integer, CultureInfo.InvariantCulture, out var index))
        {
            var result = new List<JsonNode?>();
            foreach (var node in nodes)
            {
                if (node is JsonArray array && index >= 0 && index < array.Count)
                    result.Add(array[index]);
            }

            return result;
        }

        throw new FormatException($"Unsupported path segment [{inner}].");
    }

    private static IEnumerable<JsonNode?> Children(JsonNode? node) => node switch
    {
        JsonArray array => array.ToList(),
        JsonObject obj => obj.Select(p => p.Value).ToList(),
        _ => [],
    };

    private static IEnumerable<JsonNode?> Descendants(JsonNode? node)
    {
        foreach (var child in Children(node))
        {
            yield return child;
            foreach (var deeper in Descendants(child))
                yield return deeper;
        }
    }

    private static int FindClosingBracket(string text, int open)
    {
        var depth = 0;
        var quote = '\0';
        for (var i = open; i < text.Length; i++)
        {
            var c = text[i];
            if (quote != '\0') { if (c == quote) quote = '\0'; continue; }
            if (c is '\'' or '"') quote = c;
            else if (c == '[') depth++;
            else if (c == ']' && --depth == 0) return i;
        }

        throw new FormatException($"Unclosed '[' in path '{text}'.");
    }
}
