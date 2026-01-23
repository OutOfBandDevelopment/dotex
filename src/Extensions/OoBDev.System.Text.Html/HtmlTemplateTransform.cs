using HtmlAgilityPack;
using OoBDev.System.Text.Templating;
using System.Threading.Tasks;
using System.Xml.XPath;

namespace OoBDev.System.Text.Html;

/// <summary>
/// Transforms HTML templates by applying data bindings and processing HTML content.
/// Implements ITemplateTransform for HTML media type.
/// </summary>
/// <param name="instanceFactory">Factory for creating path resolver instances from source objects.</param>
/// <param name="htmlVisitor">Visitor for processing HTML nodes with data bindings.</param>
[TemplateTransform(MediaTypes.Html)]
public class HtmlTemplateTransform(
    IInstanceFactory instanceFactory,
    IHtmlDocumentVistor htmlVisitor
        ) : ITemplateTransform
{
    /// <summary>
    /// Converts HTML content to an XPath navigator for query operations.
    /// </summary>
    /// <param name="content">The HTML content to convert.</param>
    /// <returns>An XPathNavigator for querying the HTML document.</returns>
    public XPathNavigator ToXPathNavigator(string content)
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
        html.LoadHtml(content);
        var xpathNav = html.CreateNavigator();
        return xpathNav;
    }

    /// <summary>
    /// Transforms an HTML template by applying data from the source object.
    /// Processes data bindings, repeaters, and other template directives.
    /// </summary>
    /// <param name="source">The source object containing data for the template.</param>
    /// <param name="template">The HTML template string with data binding expressions.</param>
    /// <returns>The transformed HTML string with data bindings resolved.</returns>
    public async Task<string> Transform(object source, string template)
    {
        var pathResolver = await instanceFactory.GetPathResolver(source);

        var html = new HtmlDocument()
        {
            DisableServerSideCode = true,
        };
        html.LoadHtml(template);

        var result = await htmlVisitor.VisitAsync(
            node: html.DocumentNode,
            root: pathResolver,
            current: pathResolver,
            scoped: []
            );

        return result.WriteTo();
    }
}