using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents a wrapped node that provides navigation capabilities around an XPath navigator.
/// </summary>
public interface IWrappedNode
{
    /// <summary>
    /// Gets the previous wrapped node in the sequence.
    /// </summary>
    IWrappedNode? Previous { get; }

    /// <summary>
    /// Gets the underlying XPath navigator for this node.
    /// </summary>
    XPathNavigator Current { get; }

    /// <summary>
    /// Gets the next wrapped node in the sequence.
    /// </summary>
    IWrappedNode? Next { get; }

    /// <summary>
    /// Gets the first wrapped node in the sequence.
    /// </summary>
    IWrappedNode First { get; }

    /// <summary>
    /// Gets the last wrapped node in the sequence.
    /// </summary>
    IWrappedNode Last { get; }

    /// <summary>
    /// Gets the source identifier for this wrapped node.
    /// </summary>
    string Source { get; }
}
