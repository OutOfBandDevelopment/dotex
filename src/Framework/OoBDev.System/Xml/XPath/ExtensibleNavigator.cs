using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Provides an XPathNavigator implementation for navigating extensible node structures.
/// </summary>
public sealed class ExtensibleNavigator : XPathNavigator
{
    private INode _current;
    private readonly IDictionary<string, string> _namespacePrefixes;

    /// <summary>
    /// Initializes a new instance of the ExtensibleNavigator class.
    /// </summary>
    /// <param name="current">The current node to start navigation from.</param>
    /// <param name="baseUri">Optional base URI for the navigator.</param>
    public ExtensibleNavigator(INode current, string? baseUri = null)
        : this(current, baseUri, null, null)
    {
    }
    private ExtensibleNavigator(
        INode current,
        string? baseUri,
        XmlNameTable? nameTable,
        IDictionary<string, string>? namespacePrefixes)
    {
        BaseURI = baseUri ?? "";
        _current = current;
        NameTable = nameTable ?? new ExtensibleNameTable();
        _namespacePrefixes = namespacePrefixes ?? new Dictionary<string, string>();
    }

    /// <summary>
    /// Gets the qualified name of the current node.
    /// </summary>
    public override string Name => LocalName;

    /// <summary>
    /// Gets the local name of the current node.
    /// </summary>
    public override string LocalName => _current.Name.LocalName;

    /// <summary>
    /// Gets the namespace URI of the current node.
    /// </summary>
    public override string NamespaceURI => _current switch
    {
        IRootNode _ => "",
        _ => _current.Name.NamespaceName
    };

    /// <summary>
    /// Gets the XPath node type of the current node.
    /// </summary>
    public override XPathNodeType NodeType =>
        _current.NodeType;

    /// <summary>
    /// Gets the namespace prefix of the current node.
    /// </summary>
    public override string Prefix => LookupPrefix(NamespaceURI);

    /// <summary>
    /// Looks up the prefix for the specified namespace URI.
    /// </summary>
    /// <param name="namespaceURI">The namespace URI to find the prefix for.</param>
    /// <returns>The namespace prefix, or an empty string if not found.</returns>
    public override string LookupPrefix(string namespaceURI)
    {
        if (_namespacePrefixes == null)
            return "";

        if (string.IsNullOrWhiteSpace(namespaceURI))
            return "";

        var uri = namespaceURI.Trim();
        if (!_namespacePrefixes.TryGetValue(uri, out var value))
        {
            value = $"n{_namespacePrefixes.Count + 1}";
            _namespacePrefixes.Add(uri, value);
        }
        return value;
    }

    /// <summary>
    /// Looks up the namespace URI for the specified prefix.
    /// </summary>
    /// <param name="prefix">The prefix to find the namespace URI for.</param>
    /// <returns>The namespace URI, or an empty string if not found.</returns>
    public override string LookupNamespace(string prefix) =>
        _namespacePrefixes.FirstOrDefault(v => v.Value == prefix).Key ?? base.LookupNamespace(prefix) ?? "";

    /// <summary>
    /// Gets the string value of the current node.
    /// </summary>
    public override string Value => _current.Value ?? "";

    /// <summary>
    /// Gets a value indicating whether the current node is an empty element.
    /// </summary>
    public override bool IsEmptyElement => string.IsNullOrEmpty(Value) && !HasChildren;

    /// <summary>
    /// Gets a value indicating whether the current node has attributes.
    /// </summary>
    public override bool HasAttributes => _current is IElementNode node && node.FirstAttribute != null;

    /// <summary>
    /// Gets a value indicating whether the current node has child nodes.
    /// </summary>
    public override bool HasChildren => _current is IElementNode node && node.FirstChild != null;

    /// <summary>
    /// Gets the base URI of the current node.
    /// </summary>
    public override string BaseURI { get; }

    /// <summary>
    /// Gets the XmlNameTable associated with this navigator.
    /// </summary>
    public override XmlNameTable NameTable { get; }

    /// <summary>
    /// Creates a copy of this navigator positioned at the same node.
    /// </summary>
    /// <returns>A new navigator with the same position.</returns>
    public override XPathNavigator Clone() => new ExtensibleNavigator(_current, BaseURI, NameTable, _namespacePrefixes);

    /// <summary>
    /// Moves to the node with the specified ID.
    /// </summary>
    /// <param name="id">The ID to search for.</param>
    /// <returns>Always returns false as ID navigation is not supported.</returns>
    public override bool MoveToId(string id) => false;

    /// <summary>
    /// Determines whether the current navigator is at the same position as the specified navigator.
    /// </summary>
    /// <param name="other">The navigator to compare with.</param>
    /// <returns>True if both navigators are at the same position, otherwise false.</returns>
    public override bool IsSamePosition(XPathNavigator other) =>
        other switch
        {
            ExtensibleNavigator openXPath => openXPath._current.Equals(_current),
            _ => false
        };

    /// <summary>
    /// Moves the navigator to the same position as the specified navigator.
    /// </summary>
    /// <param name="other">The navigator to move to.</param>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveTo(XPathNavigator other)
    {
        if (other is ExtensibleNavigator openXPath && openXPath._current != null)
        {
            _current = openXPath._current;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the first namespace declaration of the current element.
    /// </summary>
    /// <param name="namespaceScope">The scope of namespaces to include.</param>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToFirstNamespace(XPathNamespaceScope namespaceScope)
    {
        if (_current is IElementNode current && current.FirstNamespace != null)
        {
            _current = current.FirstNamespace;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the next namespace declaration.
    /// </summary>
    /// <param name="namespaceScope">The scope of namespaces to include.</param>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToNextNamespace(XPathNamespaceScope namespaceScope)
    {
        if (_current is INamespaceNode current && current.Next != null)
        {
            _current = current.Next;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the first attribute of the current element.
    /// </summary>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToFirstAttribute()
    {
        if (_current is IElementNode current && current.FirstAttribute != null)
        {
            _current = current.FirstAttribute;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the next attribute.
    /// </summary>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToNextAttribute()
    {
        if (_current is IAttributeNode current && current.Next != null)
        {
            _current = current.Next;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the parent node of the current node.
    /// </summary>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToParent()
    {
        if (_current.Parent != null)//&& !(_current.Parent is IRootNode)
        {
            _current = _current.Parent;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the first child node of the current node.
    /// </summary>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToFirstChild()
    {
        if (_current is IElementNode current && current.FirstChild != null)
        {
            _current = current.FirstChild;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the next sibling node of the current node.
    /// </summary>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToNext()
    {
        if (_current.Next != null)
        {
            _current = _current.Next;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Moves to the previous sibling node of the current node.
    /// </summary>
    /// <returns>True if the move was successful, otherwise false.</returns>
    public override bool MoveToPrevious()
    {
        if (_current.Previous != null)
        {
            _current = _current.Previous;
            return true;
        }
        return false;
    }
}
