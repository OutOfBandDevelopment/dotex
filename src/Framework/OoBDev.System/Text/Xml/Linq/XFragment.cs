using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Text.Xml.Linq;

/// <summary>
/// Represents a fragment of XML nodes with additional functionality for manipulation and conversion.
/// </summary>
public class XFragment : IList<XNode>, IXPathNavigable
{
    // https://github.com/OutOfBandDevelopment/Samples/blob/master/HandyClasses/XFragment.cs
    private readonly List<XNode> _nodes = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="XFragment"/> class with the specified fragment.
    /// </summary>
    /// <param name="fragment">The fragment to initialize the instance with.</param>
    public XFragment(XFragment fragment) : this(fragment._nodes) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="XFragment"/> class with the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to initialize the instance with.</param>
    public XFragment(XNode[] nodes) : this(nodes.AsEnumerable()) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="XFragment"/> class with the specified nodes.
    /// </summary>
    /// <param name="nodes">The nodes to initialize the instance with.</param>
    public XFragment(IEnumerable<XNode> nodes)
    {
        foreach (var node in (nodes ?? []).Where(n => n != null))
            _nodes.Add(node);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XFragment"/> class with the specified nodes.
    /// </summary>
    /// <param name="node">The nodes to initialize the instance with.</param>
    /// <param name="nodes">The nodes to initialize the instance with.</param>
    public XFragment(XNode node, params XNode[] nodes)
        : this(new[] { node }.Concat(nodes ?? Enumerable.Empty<XNode>()))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XFragment"/> class with the specified XML string.
    /// </summary>
    /// <param name="xml">The XML string to initialize the instance with.</param>
    public XFragment(string? xml)
        : this(Parser(xml).ToArray())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="XFragment"/> class with the specified XML reader.
    /// </summary>
    /// <param name="xmlReader">The XML reader to initialize the instance with.</param>
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
    /// Returns the XML string representation of the fragment.
    /// </summary>
    /// <returns>The XML string representation of the fragment.</returns>
    public override string ToString() => this!;

    /// <summary>
    /// Creates an XML reader for the fragment.
    /// </summary>
    /// <returns>An XML reader for the fragment.</returns>
    public XmlReader CreateReader() => XmlReader.Create(new StringReader(this!), new XmlReaderSettings
    {
        ConformanceLevel = ConformanceLevel.Fragment,
    });

    /// <summary>
    /// Parses the specified XML string into an <see cref="XFragment"/> instance.
    /// </summary>
    /// <param name="xml">The XML string to parse.</param>
    /// <returns>An <see cref="XFragment"/> instance representing the parsed XML string.</returns>
    public static XFragment Parse(string xml) => new(xml);

    /// <summary>
    /// Parses the XML content from the specified reader into an <see cref="XFragment"/> instance.
    /// </summary>
    /// <param name="xmlReader">The XML reader containing the XML content to parse.</param>
    /// <returns>An <see cref="XFragment"/> instance representing the parsed XML content.</returns>
    public static XFragment Parse(XmlReader xmlReader) => new(xmlReader);

    #region IEnumerable 

    /// <summary>
    /// Returns an enumerator that iterates through the collection of XML nodes.
    /// </summary>
    /// <returns>An enumerator that can be used to iterate through the collection of XML nodes.</returns>
    public IEnumerator<XNode> GetEnumerator() =>
        (_nodes ?? Enumerable.Empty<XNode>()).Where(n => n != null).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Creates an XPath navigator for navigating the fragment.
    /// </summary>
    /// <returns>An XPath navigator for navigating the fragment.</returns>
    public XPathNavigator? CreateNavigator()
    {
        using var reader = CreateReader();
        var document = XDocument.Load(reader);
        return document.CreateNavigator();
    }

    #endregion

    #region IList

    /// <summary>
    /// Gets the number of elements contained in the collection of XML nodes.
    /// </summary>
    public int Count => _nodes.Count;

    /// <summary>
    /// Gets a value indicating whether the collection of XML nodes is read-only.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Gets or sets the XML node at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index of the XML node to get or set.</param>
    /// <returns>The XML node at the specified index.</returns>
    public XNode this[int index]
    {
        get => _nodes[index];
        set => _nodes[index] = value;
    }

    /// <summary>
    /// Determines the index of a specific XML node in the collection.
    /// </summary>
    /// <param name="item">The XML node to locate in the collection.</param>
    /// <returns>The zero-based index of the XML node within the collection, if found; otherwise, -1.</returns>
    public int IndexOf(XNode item) => _nodes.IndexOf(item);

    /// <summary>
    /// Inserts an XML node into the collection at the specified index.
    /// </summary>
    /// <param name="index">The zero-based index at which the XML node should be inserted.</param>
    /// <param name="item">The XML node to insert into the collection.</param>
    public void Insert(int index, XNode item) => _nodes.Insert(index, item);

    /// <summary>
    /// Removes the XML node at the specified index from the collection.
    /// </summary>
    /// <param name="index">The zero-based index of the XML node to remove.</param>
    public void RemoveAt(int index) => _nodes.RemoveAt(index);

    /// <summary>
    /// Adds an XML node to the end of the collection.
    /// </summary>
    /// <param name="item">The XML node to add to the collection.</param>
    public void Add(XNode item) => _nodes.Add(item);

    /// <summary>
    /// Removes all XML nodes from the collection.
    /// </summary>
    public void Clear() => _nodes.Clear();

    /// <summary>
    /// Determines whether the collection contains a specific XML node.
    /// </summary>
    /// <param name="item">The XML node to locate in the collection.</param>
    /// <returns>true if the XML node is found in the collection; otherwise, false.</returns>
    public bool Contains(XNode item) => _nodes.Contains(item);

    /// <summary>
    /// Copies the elements of the collection to an array, starting at a particular index.
    /// </summary>
    /// <param name="array">The one-dimensional array that is the destination of the elements copied from the collection.</param>
    /// <param name="arrayIndex">The zero-based index in the array at which copying begins.</param>
    public void CopyTo(XNode[] array, int arrayIndex) => _nodes.CopyTo(array, arrayIndex);

    /// <summary>
    /// Removes the first occurrence of a specific XML node from the collection.
    /// </summary>
    /// <param name="item">The XML node to remove from the collection.</param>
    /// <returns>true if the XML node was successfully removed from the collection; otherwise, false.</returns>
    public bool Remove(XNode item) => _nodes.Remove(item);

    #endregion

    #region Conversions 
    /// <summary>
    /// Implicitly converts a string to an <see cref="XFragment"/> instance.
    /// </summary>
    /// <param name="xml">The XML string to convert.</param>
    public static implicit operator XFragment(string? xml) => new(xml);

    /// <summary>
    /// Implicitly converts an <see cref="XFragment"/> instance to a string.
    /// </summary>
    /// <param name="fragment">The <see cref="XFragment"/> instance to convert.</param>
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
    /// Implicitly converts an array of XML nodes to an <see cref="XFragment"/> instance.
    /// </summary>
    /// <param name="nodes">The array of XML nodes to convert.</param>
    public static implicit operator XFragment(XNode[] nodes) => new(nodes);

    #endregion
}
