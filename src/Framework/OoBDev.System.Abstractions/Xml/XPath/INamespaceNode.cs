namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents an XML namespace node in an XPath navigation tree.
/// </summary>
public interface INamespaceNode : INode
{
    /// <summary>
    /// Gets the next namespace node in the tree.
    /// </summary>
    new INamespaceNode? Next { get; }

    /// <summary>
    /// Gets the previous namespace node in the tree.
    /// </summary>
    new INamespaceNode? Previous { get; }
}
