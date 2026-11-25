using HtmlAgilityPack;
using OoBDev.System.Text.Templating;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Html;

/// <summary>
/// Defines a visitor for processing HTML document nodes with data binding support.
/// </summary>
public interface IHtmlDocumentVisitor
{
    /// <summary>
    /// Visits an HTML node and processes data bindings recursively.
    /// </summary>
    /// <param name="node">The HTML node to visit.</param>
    /// <param name="root">The root path resolver for absolute path references.</param>
    /// <param name="current">The current path resolver for relative path references.</param>
    /// <param name="scoped">Array of scoped data resolvers with their scope names.</param>
    /// <returns>The processed HTML node with bindings resolved.</returns>
    Task<HtmlNode> VisitAsync(HtmlNode node, IPathResolver root, IPathResolver current, (string scope, IPathResolver data)[] scoped);
}
