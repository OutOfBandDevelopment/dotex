using OoBDev.System.PathSegments;
using OoBDev.System.Text.Json.JsonPath.Parser;
using OoBDev.System.Xml.XPath;
using System;
using System.Linq;
using System.Text.Json;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Text.Json;

/// <summary>
/// Provides factory methods for creating navigable structures from JSON documents and elements.
/// </summary>
public static class JsonNavigatorFactory
{
    /// <summary>
    /// Parses a JSON path string into a path segment structure.
    /// </summary>
    /// <param name="jsonPath">The JSON path string to parse.</param>
    /// <returns>A path segment representing the parsed JSON path.</returns>
    public static IPathSegment ParseAsJsonPath(this string jsonPath) => JsonPathFactory.Parse(jsonPath);

    /// <summary>
    /// Converts a JsonDocument to an XPath-navigable structure.
    /// </summary>
    /// <param name="json">The JSON document to convert.</param>
    /// <param name="rootName">Optional root element name for the navigable structure.</param>
    /// <param name="baseUri">Optional base URI for the navigable structure.</param>
    /// <returns>An XPath-navigable representation of the JSON document.</returns>
    public static IXPathNavigable ToNavigable(this JsonDocument json, XName? rootName = null, string? baseUri = null) =>
        json.RootElement.ToNavigable(rootName, baseUri);

    /// <summary>
    /// Converts a JsonElement to an XPath-navigable structure.
    /// </summary>
    /// <param name="json">The JSON element to convert.</param>
    /// <param name="rootName">Optional root element name for the navigable structure.</param>
    /// <param name="baseUri">Optional base URI for the navigable structure.</param>
    /// <returns>An XPath-navigable representation of the JSON element.</returns>
    public static IXPathNavigable ToNavigable(this JsonElement json, XName? rootName = null, string? baseUri = null) =>
        new ExtensibleNavigator(json.AsNode(rootName, baseUri));

    /// <summary>
    /// Converts a JsonDocument to a node structure.
    /// </summary>
    /// <param name="json">The JSON document to convert.</param>
    /// <param name="rootName">Optional root element name for the node structure.</param>
    /// <param name="baseUri">Optional base URI for the node structure.</param>
    /// <returns>A node representation of the JSON document.</returns>
    public static INode AsNode(this JsonDocument json, XName? rootName = null, string? baseUri = null) =>
        json.RootElement.AsNode(rootName, baseUri);

    /// <summary>
    /// Converts a JsonElement to a node structure with selectors for values, attributes, and children.
    /// </summary>
    /// <param name="json">The JSON element to convert.</param>
    /// <param name="rootName">Optional root element name for the node structure.</param>
    /// <param name="baseUri">Optional base URI for the node structure.</param>
    /// <returns>A node representation of the JSON element with type-specific value, attribute, and child selectors.</returns>
    public static INode AsNode(this JsonElement json, XName? rootName = null, string? baseUri = null)
    {
        if (rootName == null || string.IsNullOrWhiteSpace(rootName.LocalName))
            rootName = XName.Get(json.ValueKind.ToString(), baseUri ?? "");

        return new ExtensibleElementNode(
            rootName,
            json.Clone(),

            valueSelector: v => v switch
            {
                JsonElement element => element.ValueKind switch
                {
                    JsonValueKind.Array => null,
                    JsonValueKind.Object => null,

                    JsonValueKind.String => element.GetString(),
                    _ => element.GetRawText()
                },

                JsonProperty property => property.Value.ValueKind switch
                {
                    JsonValueKind.Array => null,
                    JsonValueKind.Object => null,

                    JsonValueKind.String => property.Value.GetString(),
                    _ => property.Value.GetRawText()
                },

                _ => throw new NotSupportedException(),
            },

             attributeSelector: a => a switch
             {
                 JsonElement element => new (XName, string?)[]
                 {
                    (XName.Get("kind", ""), element.ValueKind.ToString()),

                 }.Where(a => a.Item2 != null).AsEnumerable(),

                 JsonProperty property => null,

                 _ => throw new NotSupportedException(),
             },

             childSelector: c => c switch
             {
                 JsonElement element => element.ValueKind switch
                 {
                     JsonValueKind.Array => element.EnumerateArray().Select(i => (XName.Get("item", rootName.NamespaceName), (object)i)),
                     JsonValueKind.Object => element.EnumerateObject().Select(i => (XName.Get(i.Name, rootName.NamespaceName), (object)i.Value)),

                     _ => null
                 },

                 JsonProperty property => new[] { (XName.Get(property.Name, rootName.NamespaceName), (object)property.Value) }.AsEnumerable(),

                 _ => throw new NotSupportedException()
             }
        );
    }
}
