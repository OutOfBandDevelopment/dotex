namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents a simple XML element node with settable navigation properties.
/// </summary>
public interface ISimpleNode : IElementNode
{
    /// <summary>
    /// Sets the next sibling node.
    /// </summary>
    new INode? Next { set; }

    /// <summary>
    /// Sets the previous sibling node.
    /// </summary>
    new INode? Previous { set; }
}
