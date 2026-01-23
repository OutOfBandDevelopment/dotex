using System.Diagnostics;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

[DebuggerDisplay("A:>{Name}= {Value}")]
internal class ExtensibleAttributeNode<T>(
     INode parent,
     XName name,
     T item,
     string value
        ) : IAttributeNode
{
    public INode? Parent => parent;
    public XName Name => name;
    public string? Value => value;
    public T Item => item;

    public IAttributeNode? Next { get; internal set; }
    public IAttributeNode? Previous { get; internal set; }

    public XPathNodeType NodeType => XPathNodeType.Attribute;

    INode? INode.Next => Next;
    INode? INode.Previous => Previous;
}
