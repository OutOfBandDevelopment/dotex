using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Provides extension methods for converting INode instances to XPath navigable objects and navigators.
/// </summary>
public static class NodeExtensions
{
    /// <summary>
    /// Converts an INode instance to an IXPathNavigable object that can be used for XPath queries.
    /// </summary>
    /// <param name="node">The node to convert.</param>
    /// <param name="baseUri">The base URI for the navigable object. Defaults to an empty string.</param>
    /// <returns>An IXPathNavigable object wrapping the node.</returns>
    public static IXPathNavigable ToNavigable(this INode node, string baseUri = "") =>
        new ExtensibleNavigator(node, baseUri);

    /// <summary>
    /// Converts an INode instance to an XPathNavigator for XPath query execution.
    /// </summary>
    /// <param name="node">The node to convert.</param>
    /// <param name="baseUri">The base URI for the navigator. Defaults to an empty string.</param>
    /// <returns>An XPathNavigator for the node, or null if the navigator could not be created.</returns>
    public static XPathNavigator? ToNavigator(this INode node, string baseUri = "") =>
        node.ToNavigable(baseUri).CreateNavigator();
}
