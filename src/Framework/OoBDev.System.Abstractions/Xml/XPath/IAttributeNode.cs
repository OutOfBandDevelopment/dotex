namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents an XML attribute node in an XPath navigation tree.
/// </summary>
public interface IAttributeNode : INode
{
    /// <summary>
    /// Gets the next attribute node in the tree.
    /// </summary>
    new IAttributeNode? Next { get; }

    /// <summary>
    /// Gets the previous attribute node in the tree.
    /// </summary>
    new IAttributeNode? Previous { get; }
}
