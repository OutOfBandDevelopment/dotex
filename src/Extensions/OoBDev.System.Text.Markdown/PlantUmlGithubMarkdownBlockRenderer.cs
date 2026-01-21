using Markdig;
using Markdig.Renderers.Normalize;

namespace OoBDev.System.Text.Markdown;

/// <summary>
/// Renders PlantUML blocks as GitHub-flavored Markdown with embedded diagram images.
/// </summary>
/// <param name="pipeline">The Markdown pipeline configuration.</param>
public class PlantUmlGithubMarkdownBlockRenderer(MarkdownPipeline pipeline) : NormalizeObjectRenderer<PlantUmlBlock>
{
    private readonly PlantUmlRenderer _renderer = new(pipeline);

    /// <summary>
    /// Writes the PlantUML block as GitHub-flavored Markdown with an embedded diagram image.
    /// </summary>
    /// <param name="renderer">The normalize renderer to write to.</param>
    /// <param name="obj">The PlantUML block to render.</param>
    protected override void Write(NormalizeRenderer renderer, PlantUmlBlock obj) => _renderer.Write(renderer, obj.GetScript());
}
