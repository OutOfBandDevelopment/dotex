using System.Xml;
using System.Xml.Linq;

namespace OoBDev.System.Xml.Linq;

/// <summary>
/// Provides extension methods for XmlNode operations.
/// </summary>
public static class XmlNodeEx
{
    /// <summary>
    /// Converts an XmlNode to an XElement.
    /// </summary>
    /// <param name="node">The XML node to convert.</param>
    /// <returns>The converted XElement, or null if the node is null.</returns>
    public static XElement? ToXElement(this XmlNode node)
    {
        var xDoc = new XDocument();
        using (var xmlWriter = xDoc.CreateWriter())
            node.WriteTo(xmlWriter);
        return xDoc.Root;
    }
}
