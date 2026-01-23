using HtmlAgilityPack;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;
using System.IO;
using System.Xml.XPath;

namespace OoBDev.System.Text.Html;

/// <summary>
/// Provides navigation capabilities for HTML documents, converting them to XPath-navigable structures using HtmlAgilityPack.
/// </summary>
[MediaType("text/html")]
[FileExtension(".html"), FileExtension(".htm")]
public class HtmlNavigator : IToXPathNavigable
{
    /// <summary>
    /// Converts an HTML file to an XPath-navigable structure.
    /// </summary>
    /// <param name="sourceFile">The path to the HTML file to convert.</param>
    /// <returns>An XPath-navigable representation of the HTML document.</returns>
    public IXPathNavigable ToNavigable(string sourceFile)
    {
        var html = new HtmlDocument()
        {
            DisableServerSideCode = true,

            OptionAutoCloseOnEnd = true,
            // OptionDefaultStreamEncoding = Encoding.UTF8,
            OptionEmptyCollection = true,
            OptionFixNestedTags = true,
            OptionOutputAsXml = true,
            OptionOutputOptimizeAttributeValues = true,
            // OptionPreserveXmlNamespaces = true,
            OptionReadEncoding = true,
            //OptionWriteEmptyNodes = true,

        };
        html.Load(sourceFile);
        var xpathNav = html.CreateNavigator();
        return xpathNav;
    }

    /// <summary>
    /// Converts an HTML stream to an XPath-navigable structure.
    /// </summary>
    /// <param name="stream">The stream containing HTML data to convert.</param>
    /// <returns>An XPath-navigable representation of the HTML document.</returns>
    public IXPathNavigable ToNavigable(Stream stream)
    {
        var html = new HtmlDocument()
        {
            DisableServerSideCode = true,

            OptionAutoCloseOnEnd = true,
            // OptionDefaultStreamEncoding = Encoding.UTF8,
            OptionEmptyCollection = true,
            OptionFixNestedTags = true,
            OptionOutputAsXml = true,
            OptionOutputOptimizeAttributeValues = true,
            // OptionPreserveXmlNamespaces = true,
            OptionReadEncoding = true,
            //OptionWriteEmptyNodes = true,

        };
        html.Load(stream);
        var xpathNav = html.CreateNavigator();
        return xpathNav;
    }
}