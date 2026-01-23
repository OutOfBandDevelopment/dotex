using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

/// <summary>
/// Represents a collection of XML nodes that can be treated as a fragment without a single root element.
/// Provides parsing, serialization, and list operations for XML fragments.
/// </summary>
public class XFragment : IList<XNode>
{
    // https://github.com/OutOfBandDevelopment/Samples/blob/master/HandyClasses/XFragment.cs
    private IList<XNode> Nodes { get; } = [];

    /// <summary>
    /// Initializes a new instance of the XFragment class from a collection of XML nodes.
    /// </summary>
    /// <param name="nodes">The collection of nodes to include in the fragment.</param>
    public XFragment(IEnumerable<XNode> nodes)
    {
        foreach (var node in (nodes ?? []).Where(n => n != null))
            Nodes.Add(node);
    }

    /// <summary>
    /// Initializes a new instance of the XFragment class from one or more XML nodes.
    /// </summary>
    /// <param name="node">The first node to include in the fragment.</param>
    /// <param name="nodes">Additional nodes to include in the fragment.</param>
    public XFragment(XNode node, params XNode[] nodes)
        : this(new[] { node }.Concat(nodes ?? Enumerable.Empty<XNode>()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the XFragment class by parsing an XML string.
    /// </summary>
    /// <param name="xml">The XML string to parse as a fragment.</param>
    public XFragment(string? xml)
        : this(Parser(xml).ToArray())
    {
    }

    /// <summary>
    /// Initializes a new instance of the XFragment class by reading from an XmlReader.
    /// </summary>
    /// <param name="xmlReader">The XmlReader to read the fragment from.</param>
    public XFragment(XmlReader xmlReader)
        : this(Parser(xmlReader).ToArray())
    {
    }

    private static IEnumerable<XNode> Parser(string? xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
            yield break;

        var settings = new XmlReaderSettings
        {
            ConformanceLevel = ConformanceLevel.Fragment,
            IgnoreWhitespace = true
        };

        using var stringReader = new StringReader(xml);
        using var xmlReader = XmlReader.Create(stringReader, settings);
        foreach (var node in Parser(xmlReader))
            yield return node;
    }

    private static IEnumerable<XNode> Parser(XmlReader xmlReader)
    {
        if (xmlReader == null)
            yield break;

        xmlReader.MoveToContent();
        while (xmlReader.ReadState != ReadState.EndOfFile)
            yield return XNode.ReadFrom(xmlReader);
    }

    /// <summary>
    /// Returns a string representation of the XML fragment.
    /// </summary>
    /// <returns>The XML fragment as a string.</returns>
    public override string? ToString() => this;

    /// <summary>
    /// Creates an XmlReader for reading the fragment.
    /// </summary>
    /// <returns>An XmlReader configured to read the fragment.</returns>
    public XmlReader CreateReader() => XmlReader.Create(new StringReader(this!), new XmlReaderSettings
    {
        ConformanceLevel = ConformanceLevel.Fragment,
    });

    /// <summary>
    /// Parses an XML string into an XFragment.
    /// </summary>
    /// <param name="xml">The XML string to parse.</param>
    /// <returns>An XFragment containing the parsed nodes.</returns>
    public static XFragment Parse(string xml) => new(xml);

    /// <summary>
    /// Parses XML from an XmlReader into an XFragment.
    /// </summary>
    /// <param name="xmlReader">The XmlReader to read from.</param>
    /// <returns>An XFragment containing the parsed nodes.</returns>
    public static XFragment Parse(XmlReader xmlReader) => new(xmlReader);

    #region IEnumerable

    /// <summary>
    /// Returns an enumerator that iterates through the fragment's nodes.
    /// </summary>
    /// <returns>An enumerator for the nodes in the fragment.</returns>
    public IEnumerator<XNode> GetEnumerator() => (Nodes ?? Enumerable.Empty<XNode>()).Where(n => n != null).GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion

    #region IList

    /// <summary>
    /// Gets the number of nodes in the fragment.
    /// </summary>
    public int Count => Nodes.Count;

    /// <summary>
    /// Gets a value indicating whether the fragment is read-only.
    /// </summary>
    public bool IsReadOnly => Nodes.IsReadOnly;

    /// <summary>
    /// Gets or sets the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the node to get or set.</param>
    /// <returns>The node at the specified index.</returns>
    public XNode this[int index]
    {
        get => Nodes[index];
        set => Nodes[index] = value;
    }

    /// <summary>
    /// Determines the index of a specific node in the fragment.
    /// </summary>
    /// <param name="item">The node to locate.</param>
    /// <returns>The index of the node if found, otherwise -1.</returns>
    public int IndexOf(XNode item) => Nodes.IndexOf(item);

    /// <summary>
    /// Inserts a node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the node should be inserted.</param>
    /// <param name="item">The node to insert.</param>
    public void Insert(int index, XNode item) => Nodes.Insert(index, item);

    /// <summary>
    /// Removes the node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the node to remove.</param>
    public void RemoveAt(int index) => Nodes.RemoveAt(index);

    /// <summary>
    /// Adds a node to the fragment.
    /// </summary>
    /// <param name="item">The node to add.</param>
    public void Add(XNode item) => Nodes.Add(item);

    /// <summary>
    /// Removes all nodes from the fragment.
    /// </summary>
    public void Clear() => Nodes.Clear();

    /// <summary>
    /// Determines whether the fragment contains a specific node.
    /// </summary>
    /// <param name="item">The node to locate.</param>
    /// <returns>True if the node is found, otherwise false.</returns>
    public bool Contains(XNode item) => Nodes.Contains(item);

    /// <summary>
    /// Copies the nodes of the fragment to an array, starting at a particular array index.
    /// </summary>
    /// <param name="array">The destination array.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(XNode[] array, int arrayIndex) => Nodes.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes the first occurrence of a specific node from the fragment.
    /// </summary>
    /// <param name="item">The node to remove.</param>
    /// <returns>True if the node was successfully removed, otherwise false.</returns>
    public bool Remove(XNode item) => Nodes.Remove(item);

    #endregion

    #region Conversions

    /// <summary>
    /// Implicitly converts an XML string to an XFragment.
    /// </summary>
    /// <param name="xml">The XML string to convert.</param>
    public static implicit operator XFragment(string? xml) => new(xml);

    /// <summary>
    /// Implicitly converts an XFragment to an XML string.
    /// </summary>
    /// <param name="fragment">The fragment to convert.</param>
    public static implicit operator string?(XFragment fragment)
    {
        if (fragment == null)
            return null;

        var settings = new XmlWriterSettings
        {
            OmitXmlDeclaration = true,
            ConformanceLevel = ConformanceLevel.Fragment,
        };
        var sb = new StringBuilder();
        using (var xmlwriter = XmlWriter.Create(sb, settings))
        {
            foreach (var node in fragment)
            {
                xmlwriter.WriteNode(node.CreateReader(), false);
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Implicitly converts an array of nodes to an XFragment.
    /// </summary>
    /// <param name="nodes">The nodes to convert.</param>
    public static implicit operator XFragment(XNode[] nodes) => new(nodes);

    /// <summary>
    /// Implicitly converts a single node to an XFragment.
    /// </summary>
    /// <param name="node">The node to convert.</param>
    public static implicit operator XFragment(XNode node) => new(node);

    #endregion
}
