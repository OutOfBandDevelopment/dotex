using HtmlAgilityPack;
using OoBDev.System.Text.Templating;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Html;

/// <summary>
/// Applies the HTML template directives: <c>&lt;value-of&gt;</c>, <c>&lt;repeater&gt;</c>, <c>&lt;condition&gt;</c>,
/// <c>&lt;value-attr&gt;</c> and the <c>data-binding</c> attribute. See the TextTemplating design for the syntax.
/// </summary>
/// <param name="instanceFactory">Creates the scoped path resolvers for repeater items.</param>
public class HtmlDocumentVisitor(IInstanceFactory instanceFactory) : IHtmlDocumentVisitor
{
    private const string ValueOf = "value-of";
    private const string Repeater = "repeater";
    private const string ValueAttribute = "value-attr";
    private const string Condition = "condition";
    private static readonly string[] Directives = [ValueOf, Repeater, ValueAttribute, Condition];

    /// <inheritdoc />
    public async Task<HtmlNode> VisitAsync(HtmlNode node, IPathResolver root, IPathResolver current, (string scope, IPathResolver data)[] scoped)
    {
        foreach (var child in node.ChildNodes.ToList())
        {
            if (child.NodeType != HtmlNodeType.Element)
                continue;

            var binding = child.Attributes["binding"]?.Value;
            var itemKey = child.Attributes["item"]?.Value;
            var sourceKey = child.Attributes["source"]?.Value;
            var source = string.IsNullOrWhiteSpace(sourceKey)
                ? current
                : scoped.LastOrDefault(s => s.scope == sourceKey).data ?? current;

            switch (child.Name.ToLowerInvariant())
            {
                case ValueOf:
                    {
                        var target = (await Select(root, source, binding).ConfigureAwait(false)).FirstOrDefault();
                        var text = Format(Text(target), child.Attributes["format"]?.Value, child.Attributes["type"]?.Value);
                        node.ReplaceChild(Encoded(node, text), child);
                        break;
                    }
                case Repeater:
                    {
                        var items = await Items(root, source, binding).ConfigureAwait(false);
                        foreach (var item in items)
                        {
                            var itemResolver = await instanceFactory.GetPathResolver(item!).ConfigureAwait(false);
                            var clone = child.CloneNode(true);
                            await VisitAsync(clone, root, itemResolver, Scope(scoped, itemKey, itemResolver)).ConfigureAwait(false);
                            foreach (var inner in clone.ChildNodes.ToList())
                                node.InsertBefore(inner, child);
                        }

                        node.RemoveChild(child);
                        break;
                    }
                case Condition:
                    {
                        var rule = child.Attributes["rule"]?.Value;
                        var passes = string.IsNullOrWhiteSpace(rule) || await Rule(root, source, rule).ConfigureAwait(false);
                        if (passes)
                        {
                            var clone = child.CloneNode(true);
                            await VisitAsync(clone, root, source, Scope(scoped, itemKey, source)).ConfigureAwait(false);
                            foreach (var inner in clone.ChildNodes.ToList())
                                node.InsertBefore(inner, child);
                        }

                        node.RemoveChild(child);
                        break;
                    }
                case ValueAttribute:
                    {
                        await ApplyAttribute(child, root, source, binding, itemKey).ConfigureAwait(false);
                        node.RemoveChild(child);
                        break;
                    }
                default:
                    {
                        var dataBinding = child.Attributes["data-binding"];
                        if (dataBinding != null)
                        {
                            var values = await Select(root, source, dataBinding.Value).ConfigureAwait(false);
                            child.Attributes.Remove(dataBinding);
                            child.RemoveAllChildren();
                            child.AppendChild(Encoded(node, string.Join(Environment.NewLine, values.Select(Text))));
                        }

                        await VisitAsync(child, root, source, scoped).ConfigureAwait(false);
                        break;
                    }
            }
        }

        return node;
    }

    private async Task ApplyAttribute(HtmlNode directive, IPathResolver root, IPathResolver source, string? binding, string? attributeName)
    {
        var parent = ParentOf(directive);
        if (parent == null || string.IsNullOrWhiteSpace(attributeName))
            return;

        var rule = directive.Attributes["rule"]?.Value;
        if (!string.IsNullOrWhiteSpace(rule) && !await Rule(root, source, rule).ConfigureAwait(false))
            return;

        string? value;
        if (string.IsNullOrWhiteSpace(binding))
        {
            value = directive.Attributes["value"]?.Value;
        }
        else
        {
            var target = (await Select(root, source, binding).ConfigureAwait(false)).FirstOrDefault();
            value = Format(Text(target), directive.Attributes["format"]?.Value, directive.Attributes["type"]?.Value);
        }

        parent.SetAttributeValue(attributeName, value ?? string.Empty);
    }

    private static (string scope, IPathResolver data)[] Scope((string scope, IPathResolver data)[] scoped, string? key, IPathResolver data) =>
        string.IsNullOrWhiteSpace(key) ? scoped : [.. scoped, (key, data)];

    private static IPathResolver ResolverFor(IPathResolver root, IPathResolver source, string? path) =>
        path != null && path.TrimStart().StartsWith('$') ? root : source;

    private static async Task<List<object?>> Select(IPathResolver root, IPathResolver source, string? path)
    {
        var result = await ResolverFor(root, source, path).ItemSelector(path ?? string.Empty).ConfigureAwait(false);
        return result switch
        {
            null => [],
            string => [result],
            IEnumerable many => many.Cast<object?>().ToList(),
            _ => [result],
        };
    }

    private static async Task<List<object?>> Items(IPathResolver root, IPathResolver source, string? path)
    {
        var matches = await Select(root, source, path).ConfigureAwait(false);
        if (matches.Count == 1 && matches[0] is JsonArray array)
            return [.. array];

        return matches;
    }

    private static Task<bool> Rule(IPathResolver root, IPathResolver source, string rule) =>
        ResolverFor(root, source, rule).Test(rule);

    private static string Text(object? value) => value switch
    {
        null => string.Empty,
        JsonValue v when v.TryGetValue<string>(out var s) => s,
        JsonNode n => n.ToJsonString(),
        _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
    };

    private static string Format(string input, string? format, string? type)
    {
        var date = string.IsNullOrWhiteSpace(type) || type.Equals("DATE", StringComparison.OrdinalIgnoreCase);
        var number = string.IsNullOrWhiteSpace(type) || type.Equals("NUMBER", StringComparison.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(format) && string.IsNullOrWhiteSpace(type))
            return input;

        if (date && DateTimeOffset.TryParse(input, CultureInfo.InvariantCulture, out var when))
            return string.IsNullOrWhiteSpace(format) ? when.ToString(CultureInfo.InvariantCulture) : when.ToString(format, CultureInfo.InvariantCulture);
        if (number && decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
            return string.IsNullOrWhiteSpace(format) ? value.ToString(CultureInfo.InvariantCulture) : value.ToString(format, CultureInfo.InvariantCulture);

        return input;
    }

    private static HtmlNode Encoded(HtmlNode owner, string text) =>
        owner.OwnerDocument.CreateTextNode(HtmlEntity.Entitize(text, true, true));

    private static HtmlNode? ParentOf(HtmlNode directive)
    {
        for (var sibling = directive.PreviousSibling; sibling != null; sibling = sibling.PreviousSibling)
        {
            if (sibling.NodeType != HtmlNodeType.Element)
                continue;

            return sibling.Name is "input" or "img" ? sibling : null;
        }

        for (var node = directive.ParentNode; node != null; node = node.ParentNode)
        {
            if (!Directives.Contains(node.Name, StringComparer.OrdinalIgnoreCase))
                return node;
        }

        return null;
    }
}
