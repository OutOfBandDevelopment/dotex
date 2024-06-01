using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

public interface IWrappedNode
{
    IWrappedNode? Previous { get; }
    XPathNavigator Current { get; }
    IWrappedNode? Next { get; }

    IWrappedNode First { get; }
    IWrappedNode Last { get; }

    string Source { get; }
}
