namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents an XML element node in an XPath navigation tree.
/// </summary>
public interface IElementNode : INode
{
    /// <summary>
    /// Gets the first attribute of this element.
    /// </summary>
    IAttributeNode? FirstAttribute { get; }

    /// <summary>
    /// Gets the first child node of this element.
    /// </summary>
    INode? FirstChild { get; }

    /// <summary>
    /// Gets the first namespace declaration of this element.
    /// </summary>
    INamespaceNode? FirstNamespace { get; }
}
