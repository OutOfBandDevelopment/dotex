using HtmlAgilityPack;
using OoBDev.System.MetaData;
using OoBDev.System.Xml.XPath;
using System.IO;
using System.Xml.XPath;

namespace OoBDev.System.Text.Html;

[MediaType("text/html")]
[FileExtension(".html"), FileExtension(".htm")]
public class HtmlNavigator : IToXPathNavigable
{
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