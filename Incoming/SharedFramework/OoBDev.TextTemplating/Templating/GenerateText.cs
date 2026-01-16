using HtmlAgilityPack;
using OoBDev.TextTemplating.Contracts;
using OoBDev.Toolkit.Common;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.TextTemplating.Templating
{
    public class GenerateText : IGenerateText
    {
        private const string UNWRAP = "__unwrap__";

        private readonly IObjectSerializer _serializer;
        private readonly ILogger<GenerateText> _log;

#pragma warning disable IDE1006 // Naming Styles
        internal static class nodes
        {
            public const string valueOf = "value-of";
            public const string repeater = "repeater";
            public const string valueAttribute = "value-attr";
            public const string condition = "condition";
            public static readonly string[] ignoredNodes = new[]
            {
                valueOf,
                repeater,
                valueAttribute,
                condition,
            };
        }

#pragma warning restore IDE1006 // Naming Styles

        public GenerateText(
            IObjectSerializer serializer,
            ILogger<GenerateText> log
            )
        {
            _serializer = serializer;
            _log = log;
        }

        //TODO: consider making async
        public Task<string?> GenerateAsync<TModel>(string? template, TModel model)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                _log.LogWarning($@"No template provided for model ""{model?.GetType()}""");
                return Task.FromResult<string?>(null);
            }

            var html = new HtmlDocument();
            html.LoadHtml(template);

            var json = _serializer.GetAsSerialized(model) switch//TODO: change this to object converter
            {
                null => null,
                string value => JToken.Parse(value)
            } ?? new JObject();

            var result = Visit(html.DocumentNode, json, new List<Scoped>());

            return Task.FromResult<string?>(result.WriteTo());
        }

        private class Scoped
        {
            public Scoped(string key, JToken value)
            {
                Key = key;
                Value = value;
            }
            public string Key { get; }
            public JToken Value { get; }
        }

        private static IEnumerable<JToken> SelectToken(JToken token, string? path)
        {
            if (token == null)
            {
                return Enumerable.Empty<JToken>();
            }
            if (string.IsNullOrWhiteSpace(path))
            {
                return token;
            }

            var firstChar = path?.ElementAtOrDefault(0);
            var source = firstChar == '$' ? token.Root : token;
            if (firstChar == '@')
            {
                path = "$" + (path?.Length == 1 ? "" : path?[1..]);
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                return source;
            }

            return source.SelectTokens(path).ToArray();
        }

        //TODO: this needs refactored into an actual visitor set
        private HtmlNode Visit(HtmlNode node, JToken source, IList<Scoped> scopes)
        {
            var replacements = new List<(HtmlNode?, HtmlNode)>();
            foreach (var child in node.ChildNodes)
            {
                var itemKey = child.Attributes["item"]?.Value;
                var sourceKey = child.Attributes["source"]?.Value;
                var bindingPath = child.Attributes["binding"]?.Value;
                var dataBindingPath = child.Attributes["data-binding"];

                var element = scopes.LastOrDefault(s => s.Key == sourceKey)?.Value ?? source;
                var data = SelectToken(element, bindingPath);
                var target = data.FirstOrDefault();
                var scope = string.IsNullOrWhiteSpace(itemKey) ? null : new Scoped(itemKey, target);
                if (scope != null)
                    scopes.Add(scope);

                try
                {
                    if (child.Name == nodes.valueOf)
                    {
                        var format = child.Attributes["format"]?.Value;
                        var type = child.Attributes["type"]?.Value;
                        var input = target?.ToString() ?? "";
                        var value = GetValue(input, format, type);

                        try
                        {
                            replacements.Add((HtmlNode.CreateNode(value), child));
                        }
                        catch
                        {
                            var nestedHtml = new HtmlDocument();
                            nestedHtml.LoadHtml(value);
                            var scrubed = nestedHtml.DocumentNode
                                                    .SelectNodes("//text()")
                                                    .OfType<HtmlNode>()
                                                    .Aggregate(
                                                        new StringBuilder(),
                                                        (sb, h) => sb.Append(h.InnerText),
                                                        sb => sb.ToString()
                                                        );

                            replacements.Add((HtmlNode.CreateNode(scrubed), child));
                        }
                        continue;
                    }
                    else if (child.Name == nodes.repeater)
                    {
                        var newNode = child.OwnerDocument.CreateElement("div");
                        newNode.Attributes.Add("class", UNWRAP);
                        if (data != null)
                        {
                            var set = data.Count() == 1 && target?.Type == JTokenType.Array ? target?.Children() : data;
                            foreach (var item in set ?? Enumerable.Empty<JToken>())
                            {
                                var innerTemplate = child.CloneNode(true);
                                var innerScope = string.IsNullOrWhiteSpace(itemKey) ? null : new Scoped(itemKey, item);
                                if (innerScope != null)
                                    scopes.Add(innerScope);
                                try
                                {
                                    var innerChildren = Visit(innerTemplate, item, scopes);
                                    newNode.AppendChildren(innerChildren.ChildNodes);
                                }
                                finally
                                {
                                    if (innerScope != null)
                                        scopes.Remove(innerScope);
                                }
                            }
                        }

                        replacements.Add((newNode, child));
                        continue;
                    }
                    else if (child.Name == nodes.valueAttribute)
                    {
                        var parent = GetParent(child);
                        if (parent != null)
                        {
                            string? value;
                            if (string.IsNullOrWhiteSpace(bindingPath))
                            {
                                value = child.Attributes["value"]?.Value;
                            }
                            else
                            {
                                var format = child.Attributes["format"]?.Value;
                                var type = child.Attributes["type"]?.Value;
                                var input = target?.ToString() ?? "";
                                value = GetValue(input, format, type);
                            }

                            var rule = child.Attributes["rule"]?.Value;
                            if (!string.IsNullOrWhiteSpace(rule))
                            {
                                var check = element.DeepClone().SelectTokens($"$..[?({rule})]").Any();
                                if (check)
                                {
                                    parent.SetAttributeValue(itemKey, value);
                                }
                            }
                            else
                            {
                                parent.SetAttributeValue(itemKey, value);
                            }
                        }
                        replacements.Add((HtmlNode.CreateNode(""), child));
                        continue;
                    }
                    else if (child.Name == nodes.condition)
                    {
                        var rule = child.Attributes["rule"]?.Value;
                        if (!string.IsNullOrWhiteSpace(rule))
                        {
                            var check = element.DeepClone().SelectTokens($"$..[?({rule})]").Any();
                            if (check)
                            {
                                var item = element;
                                var newNode = child.OwnerDocument.CreateElement("div");
                                newNode.Attributes.Add("class", UNWRAP);

                                var innerTemplate = child.CloneNode(true);
                                var innerScope = string.IsNullOrWhiteSpace(itemKey) ? null : new Scoped(itemKey, item);
                                if (innerScope != null)
                                    scopes.Add(innerScope);
                                try
                                {
                                    var innerChildren = Visit(innerTemplate, item, scopes);
                                    newNode.AppendChildren(innerChildren.ChildNodes);
                                }
                                finally
                                {
                                    if (innerScope != null)
                                        scopes.Remove(innerScope);
                                }

                                replacements.Add((newNode, child));
                                continue;
                            }
                            else
                            {
                                replacements.Add((null, child));
                            }
                        }
                    }
                    else if (dataBindingPath != null)
                    {
                        var value = string.Join(Environment.NewLine, SelectToken(element, dataBindingPath.Value)?.Select(s => s.ToString()) ?? Enumerable.Empty<string>());
                        child.Attributes.Remove(dataBindingPath);
                        child.InnerHtml = value;
                    }
                    var visited = Visit(child, element, scopes);
                }
                finally
                {
                    if (scope != null)
                        scopes.Remove(scope);
                }
            }
            foreach (var item in replacements)
            {
                if (item.Item1 == null)
                {

                    item.Item2.Remove();
                }
                else if (item.Item1.Attributes["class"]?.Value == UNWRAP)
                {
                    foreach (var ch in item.Item1.ChildNodes)
                    {
                        node.InsertBefore(ch, item.Item2);
                    }
                    item.Item2.Remove();
                }
                else
                {
                    node.ReplaceChild(item.Item1, item.Item2);
                }
            }
            return node;
        }

        private static HtmlNode? GetParent(HtmlNode? child)
        {
            var sibling = GetPreviousSibling(child);
            if (sibling != null)
            {
                return sibling;
            }

            while (child?.ParentNode != null)
            {
                var node = child.ParentNode;
                if (!nodes.ignoredNodes.Contains(node?.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    return node;
                }
                child = node;
            }
            return null;
        }

        private static HtmlNode? GetPreviousSibling(HtmlNode? child)
        {
            while (child?.PreviousSibling != null)
            {
                var node = child.PreviousSibling;
                if (node.NodeType == HtmlNodeType.Element)
                {
                    if (string.Compare(node?.Name, "input", true) == 0)
                    {
                        return node;
                    }
                    else if (string.Compare(node?.Name, "img", true) == 0)
                    {
                        return node;
                    }
                    return null;
                }
                child = node;
            }
            return null;
        }

        private static string? GetValue(string? input, string? format, string? type)
        {
            if (string.IsNullOrWhiteSpace(type) || string.Equals(type, "DATE", StringComparison.OrdinalIgnoreCase))
                return !DateTimeOffset.TryParse(input, out var date) ?
                    input :
                    string.IsNullOrWhiteSpace(format) ?
                        date.ToString() :
                        date.ToString(format);

            else if (string.Equals(type, "NUMBER", StringComparison.OrdinalIgnoreCase))
                return !decimal.TryParse(input, out var value) ?
                    input :
                    string.IsNullOrWhiteSpace(format) ?
                        value.ToString() :
                        value.ToString(format);

            return input;
        }

    }
}

