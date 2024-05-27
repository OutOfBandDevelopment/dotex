using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Xml.XPath;

internal class ExtensibleTextNode<T>(
     INode parent,
     XName name,
     T item,
     string value
        ) : ExtensibleSimpleNodeBase<T>(parent, name, item, value, XPathNodeType.Text)
{
}
