using OoBDev.System.Xml.XPath;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using YamlDotNet.RepresentationModel;

namespace OoBDev.System.Text.Yaml;

/// <summary>
/// Provides extension methods for converting YAML documents and nodes to XPath-navigable representations.
/// </summary>
public static class YamlNavigatorFactory
{
    /// <summary>
    /// Converts a YAML document to an XPath-navigable representation.
    /// </summary>
    /// <param name="yaml">The YAML document to convert.</param>
    /// <param name="rootName">Optional name for the root element. If null, uses the node type as the name.</param>
    /// <param name="baseUri">Optional base URI for the document.</param>
    /// <returns>An IXPathNavigable instance representing the YAML document.</returns>
    public static IXPathNavigable ToNavigable(this YamlDocument yaml, XName? rootName = null, string? baseUri = null) =>
        yaml.RootNode.ToNavigable(rootName, baseUri);

    /// <summary>
    /// Converts a YAML node to an XPath-navigable representation.
    /// </summary>
    /// <param name="yaml">The YAML node to convert.</param>
    /// <param name="rootName">Optional name for the root element. If null, uses the node type as the name.</param>
    /// <param name="baseUri">Optional base URI for the node.</param>
    /// <returns>An IXPathNavigable instance representing the YAML node.</returns>
    public static IXPathNavigable ToNavigable(this YamlNode yaml, XName? rootName = null, string? baseUri = null) =>
        new ExtensibleNavigator(yaml.AsNode(rootName, baseUri));

    /// <summary>
    /// Converts a YAML document to an INode representation.
    /// </summary>
    /// <param name="yaml">The YAML document to convert.</param>
    /// <param name="rootName">Optional name for the root element. If null, uses the node type as the name.</param>
    /// <param name="baseUri">Optional base URI for the document.</param>
    /// <returns>An INode instance representing the YAML document.</returns>
    public static INode AsNode(this YamlDocument yaml, XName? rootName = null, string? baseUri = null) =>
        yaml.RootNode.AsNode(rootName, baseUri);

    /// <summary>
    /// Converts a YAML node to an INode representation with configurable element name and child/attribute selectors.
    /// Handles YAML scalar nodes, mapping nodes (key-value pairs), and sequence nodes (lists).
    /// </summary>
    /// <param name="yaml">The YAML node to convert.</param>
    /// <param name="rootName">Optional name for the root element. If null, uses the node type as the name.</param>
    /// <param name="baseUri">Optional base URI for the node.</param>
    /// <returns>An INode instance representing the YAML node with appropriate structure for scalar, mapping, or sequence content.</returns>
    public static INode AsNode(this YamlNode yaml, XName? rootName = null, string? baseUri = null)
    {
        if (rootName == null || string.IsNullOrWhiteSpace(rootName.LocalName))
            rootName = XName.Get(yaml.NodeType.ToString());

        return new ExtensibleElementNode<YamlNode>(
            rootName,
            yaml,

             valueSelector: v => v switch
             {
                 YamlScalarNode scalar => scalar.Value,
                 _ => null,
             },

              attributeSelector: a => a switch
              {
                  _ when !a.Tag.IsEmpty && !string.IsNullOrWhiteSpace(a.Tag.Value) => new[] { ((XName)nameof(a.Tag), (string?)(a.Tag.Value ?? "")), },
                  _ => null,
              },

             childSelector: c => c switch
             {
                 YamlMappingNode mapping => mapping.Select(i => (XName.Get(i.Key switch
                 {
                     YamlScalarNode s when s.Value != null => s.Value,
                     _ => "item"
                 }, rootName.NamespaceName), i.Value)),
                 YamlSequenceNode mapping => mapping.Select(i => (XName.Get("item", rootName.NamespaceName), i)),
                 YamlScalarNode scalar => null,
                 _ => null,
             }
        );
    }
}
