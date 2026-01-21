using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;

namespace OoBDev.System.Text.Markdown;

/// <summary>
/// Markdig extension for processing PlantUML diagrams in Markdown documents.
/// Adds support for rendering PlantUML code blocks as diagrams.
/// </summary>
public class PlantUmlExtension : IMarkdownExtension
{
    /// <summary>
    /// Configures the Markdown pipeline to recognize PlantUML code blocks.
    /// </summary>
    /// <param name="pipeline">The pipeline builder to configure.</param>
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.BlockParsers.Contains<PlantUmlBlockParser>())
            pipeline.BlockParsers.Insert(0, new PlantUmlBlockParser());
    }

    /// <summary>
    /// Configures the renderer to handle PlantUML blocks.
    /// Adds HTML and GitHub Markdown renderers for PlantUML diagrams.
    /// </summary>
    /// <param name="pipeline">The configured Markdown pipeline.</param>
    /// <param name="renderer">The renderer to configure.</param>
    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        if (renderer is HtmlRenderer html && !html.ObjectRenderers.Contains<PlantUmlHtmlBlockRenderer>())
            html.ObjectRenderers.Insert(0, new PlantUmlHtmlBlockRenderer(pipeline));

        if (renderer is NormalizeRenderer github && !github.ObjectRenderers.Contains<PlantUmlGithubMarkdownBlockRenderer>())
            github.ObjectRenderers.Insert(0, new PlantUmlGithubMarkdownBlockRenderer(pipeline));
    }
}
