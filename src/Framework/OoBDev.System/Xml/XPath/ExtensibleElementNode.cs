using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

/// <summary>
/// Represents an extensible element node with configurable value, attribute, child, and namespace selectors.
/// </summary>
/// <param name="name">The name of the element.</param>
/// <param name="item">The underlying data item for this element.</param>
/// <param name="valueSelector">Optional function to extract the text value from the item.</param>
/// <param name="attributeSelector">Optional function to extract attributes from the item.</param>
/// <param name="childSelector">Optional function to extract child elements from the item.</param>
/// <param name="namespacesSelector">Optional function to extract namespaces from the item.</param>
/// <param name="preserveWhitespace">Optional predicate to determine if whitespace should be preserved for this item.</param>
[DebuggerDisplay("E:>{Name}")]
public class ExtensibleElementNode(
    XName name,
    object item,
    Func<object, string?>? valueSelector = null,
    Func<object, IEnumerable<(XName name, string? value)>?>? attributeSelector = null,
    Func<object, IEnumerable<(XName name, object child)>?>? childSelector = null,
    Func<object, IEnumerable<XName>?>? namespacesSelector = null,
    Predicate<object>? preserveWhitespace = null
        ) : ExtensibleElementNode<object>(null, name, item, valueSelector, attributeSelector, childSelector, namespacesSelector, preserveWhitespace)
{
}

/// <summary>
/// Represents a strongly-typed extensible element node with configurable value, attribute, child, and namespace selectors.
/// </summary>
/// <typeparam name="T">The type of the underlying data item.</typeparam>
[DebuggerDisplay("E:>{Name}")]
public class ExtensibleElementNode<T> : IElementNode, ISimpleNode
{
    private readonly T _item;

    private readonly Func<T, string?>? _valueSelector;
    private readonly Predicate<T>? _preserveWhitespace;
    private readonly Func<T, IEnumerable<(XName name, string? value)>?>? _attributeSelector;
    private readonly Func<T, IEnumerable<(XName name, T child)>?>? _childSelector;
    private readonly Func<T, IEnumerable<XName>?>? _namespacesSelector;

    private readonly Lazy<INode?> _value;
    private readonly Lazy<INode?> _children;
    private readonly Lazy<IAttributeNode?> _attributes;
    private readonly Lazy<INamespaceNode?> _namespaces;

    /// <summary>
    /// Initializes a new instance of the ExtensibleElementNode class with configurable selectors.
    /// </summary>
    /// <param name="name">The name of the element.</param>
    /// <param name="item">The underlying data item for this element.</param>
    /// <param name="valueSelector">Optional function to extract the text value from the item.</param>
    /// <param name="attributeSelector">Optional function to extract attributes from the item.</param>
    /// <param name="childSelector">Optional function to extract child elements from the item.</param>
    /// <param name="namespacesSelector">Optional function to extract namespaces from the item.</param>
    /// <param name="preserveWhitespace">Optional predicate to determine if whitespace should be preserved for this item.</param>
    public ExtensibleElementNode(
        XName name,
        T item,
        Func<T, string?>? valueSelector = null,
        Func<T, IEnumerable<(XName name, string? value)>?>? attributeSelector = null,
        Func<T, IEnumerable<(XName name, T child)>?>? childSelector = null,
        Func<T, IEnumerable<XName>?>? namespacesSelector = null,
        Predicate<T>? preserveWhitespace = null
        )
        : this(null, name, item, valueSelector, attributeSelector, childSelector, namespacesSelector, preserveWhitespace)
    {
    }

    /// <summary>
    /// Initializes a new instance of the ExtensibleElementNode class with a parent node and configurable selectors.
    /// </summary>
    /// <param name="parent">The parent node of this element.</param>
    /// <param name="name">The name of the element.</param>
    /// <param name="item">The underlying data item for this element.</param>
    /// <param name="valueSelector">Optional function to extract the text value from the item.</param>
    /// <param name="attributeSelector">Optional function to extract attributes from the item.</param>
    /// <param name="childSelector">Optional function to extract child elements from the item.</param>
    /// <param name="namespacesSelector">Optional function to extract namespaces from the item.</param>
    /// <param name="preserveWhitespace">Optional predicate to determine if whitespace should be preserved for this item.</param>
    protected ExtensibleElementNode(
        INode? parent,
        XName name,
        T item,
        Func<T, string?>? valueSelector,
        Func<T, IEnumerable<(XName name, string? value)>?>? attributeSelector,
        Func<T, IEnumerable<(XName name, T child)>?>? childSelector,
        Func<T, IEnumerable<XName>?>? namespacesSelector,
        Predicate<T>? preserveWhitespace = null
        )
    {
        Parent = parent ?? new ExtensibleRootNode<T>(this);
        Name = name;
        _item = item;

        _valueSelector = valueSelector;
        _attributeSelector = attributeSelector;
        _childSelector = childSelector;
        _namespacesSelector = namespacesSelector;
        _preserveWhitespace = preserveWhitespace;

        _value = new Lazy<INode?>(() =>
            _valueSelector?.Invoke(_item) switch
            {
                null => (INode?)null,
                string value => string.IsNullOrWhiteSpace(value) switch
                {
                    true => new ExtensibleWhitespaceNode<T>(this, Name, _item, value),
                    false => (_preserveWhitespace?.Invoke(_item) ?? false) switch
                    {
                        true => new ExtensibleSignificantWhitespaceNode<T>(this, Name, _item, value),
                        false => new ExtensibleTextNode<T>(this, Name, _item, value)
                    }
                },
            });

        _attributes = new Lazy<IAttributeNode?>(() =>
        {
            var query = (_attributeSelector?.Invoke(_item) ?? []).GetEnumerator();
            IAttributeNode? first = null;
            IAttributeNode? previous = null;

            while (query.MoveNext())
            {
                if (query.Current.value == null) continue;

                var newItem = new ExtensibleAttributeNode<T>(
                    this,
                    query.Current.name,
                    _item,
                    query.Current.value
                    )
                {
                    Previous = previous,
                };
                if (previous is ExtensibleAttributeNode<T> node) node.Next = newItem;
                first ??= newItem;
                previous = newItem;
            }

            return first;
        });

        _children = new Lazy<INode?>(() =>
        {
            var query = (_childSelector?.Invoke(_item) ?? []).GetEnumerator();
            INode? first = null;
            INode? previous = null;

            while (query.MoveNext())
            {
                if (query.Current.child == null) continue;
                var newItem = new ExtensibleElementNode<T>(
                    this,
                    query.Current.name,
                    query.Current.child,
                    _valueSelector,
                    _attributeSelector,
                    _childSelector,
                    _namespacesSelector
                    )
                {
                    Previous = previous,
                };
                // Console.WriteLine($"\t\t==={newItem.Name} +++ {newItem.NodeType}");
                if (previous is ISimpleNode node) node.Next = newItem;
                first ??= newItem;
                previous = newItem;
            }

            if (_value.Value is ISimpleNode next && previous is ISimpleNode last)
            {
                last.Next = next;
                next.Previous = last;
            }

            return first ?? _value.Value;
        });

        _namespaces = new Lazy<INamespaceNode?>(() =>
        {
            var query = (_namespacesSelector?.Invoke(_item) ?? []).GetEnumerator();
            INamespaceNode? first = null;
            INamespaceNode? previous = null;

            while (query.MoveNext())
            {
                var newItem = new ExtensibleNamespaceNode<T>(
                    this,
                    query.Current,
                    _item
                    )
                {
                    Previous = previous,
                };
                if (previous is ExtensibleNamespaceNode<T> node) node.Next = newItem;
                first ??= newItem;
                previous = newItem;
            }

            return first;
        });
    }

    /// <summary>
    /// Gets the first child node of this element.
    /// </summary>
    public INode? FirstChild => _children.Value;

    /// <summary>
    /// Gets the first attribute of this element.
    /// </summary>
    public IAttributeNode? FirstAttribute => _attributes.Value;

    /// <summary>
    /// Gets the first namespace declaration of this element.
    /// </summary>
    public INamespaceNode? FirstNamespace => _namespaces.Value;

    /// <summary>
    /// Gets the next sibling node.
    /// </summary>
    public INode? Next { get; private set; }

    /// <summary>
    /// Gets the previous sibling node.
    /// </summary>
    public INode? Previous { get; private set; }

    /// <summary>
    /// Gets the parent node of this element.
    /// </summary>
    public INode? Parent { get; }

    /// <summary>
    /// Gets the name of this element.
    /// </summary>
    public XName Name { get; }

    /// <summary>
    /// Gets the text value of this element.
    /// </summary>
    public string? Value => _value.Value?.Value;

    /// <summary>
    /// Gets the XPath node type, which is always Element for this node.
    /// </summary>
    public XPathNodeType NodeType { get; } = XPathNodeType.Element;

    INode? ISimpleNode.Next { set => Next = value; }
    INode? ISimpleNode.Previous { set => Previous = value; }
}
