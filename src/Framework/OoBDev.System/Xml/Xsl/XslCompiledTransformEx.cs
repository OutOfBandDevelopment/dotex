using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace OoBDev.System.Xml.Xsl;

/// <summary>
/// Provides extension methods for performing XSLT transformations with XslCompiledTransform, supporting various input formats and parameter passing mechanisms.
/// </summary>
public static class XslCompiledTransformEx
{
    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet, passing XElement parameters.
    /// </summary>
    /// <param name="xmlStylesheet">The XSLT stylesheet as an XElement.</param>
    /// <param name="xmlDocument">The XML document to transform as an XElement.</param>
    /// <param name="arguments">Optional XElement parameters to pass to the stylesheet. The element name determines the parameter name.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(XElement xmlStylesheet, XElement xmlDocument, params XElement[] arguments) =>
        Transform(xmlStylesheet, xmlDocument, arguments.AsEnumerable());

    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet, passing a collection of XElement parameters.
    /// </summary>
    /// <param name="xmlStylesheet">The XSLT stylesheet as an XElement.</param>
    /// <param name="xmlDocument">The XML document to transform as an XElement.</param>
    /// <param name="arguments">A collection of XElement parameters to pass to the stylesheet. The element name determines the parameter name.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(XElement xmlStylesheet, XElement xmlDocument, IEnumerable<XElement> arguments)
    {
        var query = arguments.Select(x => new KeyValuePair<XName, XElement>(x.Name, x));
        return Transform(xmlStylesheet, xmlDocument, query);
    }

    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet, passing named XElement parameters.
    /// </summary>
    /// <param name="xmlStylesheet">The XSLT stylesheet as an XElement.</param>
    /// <param name="xmlDocument">The XML document to transform as an XElement.</param>
    /// <param name="arguments">Named parameters (name-value pairs) to pass to the stylesheet.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(XElement xmlStylesheet, XElement xmlDocument, params KeyValuePair<XName, XElement>[] arguments) =>
        Transform(xmlStylesheet, xmlDocument, arguments.AsEnumerable());

    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet, passing a collection of named XElement parameters.
    /// </summary>
    /// <param name="xmlStylesheet">The XSLT stylesheet as an XElement.</param>
    /// <param name="xmlDocument">The XML document to transform as an XElement.</param>
    /// <param name="arguments">A collection of named parameters (name-value pairs) to pass to the stylesheet.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(XElement xmlStylesheet, XElement xmlDocument, IEnumerable<KeyValuePair<XName, XElement>> arguments)
    {
        var xsltArgumentList = new XsltArgumentList();

        foreach (var argument in arguments)
        {
            var navigator = argument.Value.CreateNavigator();
            xsltArgumentList.AddParam(argument.Key.LocalName, argument.Key.NamespaceName, navigator);
        }

        var transform = new XslCompiledTransform(false);

        using var stylesheetReader = xmlStylesheet.CreateReader();
        using var xmlDocumentReader = xmlDocument.CreateReader();
        transform.Load(stylesheetReader);

        using var outStream = new MemoryStream();
        using var writer = new StreamWriter(outStream);
        transform.Transform(xmlDocumentReader, xsltArgumentList, writer);
        var result = Encoding.UTF8.GetString(outStream.ToArray());
        return result;
    }

    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet loaded from file paths, passing XElement parameters.
    /// </summary>
    /// <param name="xmlStylesheetPath">The file path to the XSLT stylesheet.</param>
    /// <param name="xmlDocumentPath">The file path to the XML document to transform.</param>
    /// <param name="arguments">Optional XElement parameters to pass to the stylesheet.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(string xmlStylesheetPath, string xmlDocumentPath, params XElement[] arguments) =>
        Transform(xmlStylesheetPath, xmlDocumentPath, arguments.OfType<object>());

    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet loaded from file paths, passing object parameters or extension objects.
    /// </summary>
    /// <param name="xmlStylesheetPath">The file path to the XSLT stylesheet.</param>
    /// <param name="xmlDocumentPath">The file path to the XML document to transform.</param>
    /// <param name="arguments">Parameters or extension objects to pass to the stylesheet. Supports XElement, XDocument, XPathNavigator, KeyValuePair&lt;string, object&gt; (extension objects), or other objects with XML namespace.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(string xmlStylesheetPath, string xmlDocumentPath, params object[] arguments) =>
        Transform(xmlStylesheetPath, xmlDocumentPath, arguments.AsEnumerable());

    /// <summary>
    /// Transforms an XML document using an XSLT stylesheet loaded from file paths, passing a collection of object parameters or extension objects.
    /// </summary>
    /// <param name="xmlStylesheetPath">The file path to the XSLT stylesheet.</param>
    /// <param name="xmlDocumentPath">The file path to the XML document to transform.</param>
    /// <param name="arguments">A collection of parameters or extension objects to pass to the stylesheet. Supports XElement, XDocument, XPathNavigator, KeyValuePair&lt;string, object&gt; (extension objects), or other objects with XML namespace.</param>
    /// <returns>The transformation result as a string.</returns>
    public static string Transform(string xmlStylesheetPath, string xmlDocumentPath, IEnumerable<object> arguments)
    {
        var xsltArgumentList = new XsltArgumentList();

        foreach (var argument in arguments.Where(a => a != null))
        {
            var element = argument is XDocument ? (argument as XDocument)?.Root : argument as XElement;
            if (element != null)
            {
                var navigator = element.CreateNavigator();
                xsltArgumentList.AddParam(element.Name.LocalName, element.Name.NamespaceName, navigator);
            }
            else if (argument is XPathNavigator navigator)
            {
                xsltArgumentList.AddParam(navigator.Name, navigator.NamespaceURI, navigator);
            }
            else if (argument is KeyValuePair<string, object> kvp)
            {
                xsltArgumentList.AddExtensionObject(kvp.Key, kvp.Value);
            }
            else
            {
                xsltArgumentList.AddExtensionObject(argument.GetXmlNamespace(), argument);
            }
        }

        var transform = new XslCompiledTransform(true);
        transform.Load(xmlStylesheetPath);

        using var outStream = new MemoryStream();
        using var writer = new StreamWriter(outStream);
        transform.Transform(xmlDocumentPath, xsltArgumentList, writer);
        var result = Encoding.UTF8.GetString(outStream.ToArray());
        return result;
    }
}
