using Markdig;

namespace OoBDev.System.Text.Markdown;

/// <summary>
/// Provides extension methods for adding PlantUML support to Markdig pipelines.
/// </summary>
public static class PlantumlExtensionFunctions
{
    /// <summary>
    /// Adds PlantUML extension to the Markdown pipeline.
    /// </summary>
    /// <param name="pipeline">The pipeline builder to extend.</param>
    /// <returns>The pipeline builder for method chaining.</returns>
    public static MarkdownPipelineBuilder UsePlantuml(this MarkdownPipelineBuilder pipeline)
    {
        if (!pipeline.Extensions.Contains<PlantUmlExtension>())
            pipeline.Extensions.Add(new PlantUmlExtension());
        return pipeline;
    }
}
