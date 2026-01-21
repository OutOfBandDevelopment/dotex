using System.Xml.Linq;
using static OoBDev.System.ToolkitConstants;
using System.Xml.Serialization;
using System.Xml.XPath;

namespace OoBDev.System.Xml.Xsl.Extensions;

/// <summary>
/// A wrapper around string functions intended for use with XslCompiledTransform
/// </summary>
[XmlRoot(Namespace = XmlNamespaces.Base + nameof(XmlExtensions))]
public class XmlExtensions
{
    private readonly XNamespace _ns;

    /// <summary>
    /// Create instance of XmlExtensions
    /// </summary>
    public XmlExtensions()
    {
        _ns = this.GetXmlNamespace() + XmlNamespaces.OutputSuffix;
    }

    /// <summary>
    /// Returns the XPathNodeIterator unchanged. Used for type compatibility in XSLT transformations.
    /// </summary>
    /// <param name="xPathNavigator">The XPathNodeIterator to return.</param>
    /// <returns>The same XPathNodeIterator that was passed in.</returns>
    public XPathNodeIterator Fixup(XPathNodeIterator xPathNavigator) => xPathNavigator;

    /// <summary>
    /// Evaluates an XPath expression against an XPathNavigator and returns the matching nodes.
    /// </summary>
    /// <param name="xPathNavigator">The navigator to evaluate the XPath expression against.</param>
    /// <param name="xpath">The XPath expression to evaluate.</param>
    /// <returns>An XPathNodeIterator containing the nodes that match the XPath expression.</returns>
    public XPathNodeIterator Evaluate(XPathNavigator xPathNavigator, string xpath) => xPathNavigator.Select(xpath);
}
