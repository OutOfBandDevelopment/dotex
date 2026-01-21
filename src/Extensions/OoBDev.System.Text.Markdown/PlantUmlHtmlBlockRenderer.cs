using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace OoBDev.System.Text.Markdown;

/// <summary>
/// Renders PlantUML blocks as HTML with embedded SVG diagrams.
/// </summary>
/// <param name="pipeline">The Markdown pipeline configuration.</param>
public class PlantUmlHtmlBlockRenderer(MarkdownPipeline pipeline) : HtmlObjectRenderer<PlantUmlBlock>
{
    private readonly PlantUmlRenderer _renderer = new(pipeline);

    /// <summary>
    /// Writes the PlantUML block as HTML with an embedded SVG diagram.
    /// </summary>
    /// <param name="renderer">The HTML renderer to write to.</param>
    /// <param name="obj">The PlantUML block to render.</param>
    protected override void Write(HtmlRenderer renderer, PlantUmlBlock obj) => _renderer.Write(renderer, obj.GetScript());
}
