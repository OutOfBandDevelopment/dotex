using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents a node in an XML XPath navigation tree.
/// </summary>
public interface INode
{
    /// <summary>
    /// Gets the qualified name of the node.
    /// </summary>
    XName Name { get; }

    /// <summary>
    /// Gets the parent node of this node.
    /// </summary>
    INode? Parent { get; }

    /// <summary>
    /// Gets the next sibling node.
    /// </summary>
    INode? Next { get; }

    /// <summary>
    /// Gets the previous sibling node.
    /// </summary>
    INode? Previous { get; }

    /// <summary>
    /// Gets the text value of the node.
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// Gets the XPath node type.
    /// </summary>
    XPathNodeType NodeType { get; }
}
