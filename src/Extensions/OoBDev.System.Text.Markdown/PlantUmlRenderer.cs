using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Normalize;
using PlantUml.Net;
using System;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace OoBDev.System.Text.Markdown;

/// <summary>
/// Renders PlantUML diagrams using the PlantUML.Net library.
/// Supports both HTML/SVG output and GitHub-flavored Markdown output.
/// </summary>
public class PlantUmlRenderer
{
    private readonly MarkdownPipeline _pipeline;
    private readonly IPlantUmlRenderer _render;

    /// <summary>
    /// Initializes a new instance of the PlantUmlRenderer class.
    /// </summary>
    /// <param name="pipeline">The Markdown pipeline configuration.</param>
    public PlantUmlRenderer(MarkdownPipeline pipeline)
    {
        var renderFactory = new RendererFactory();
        _render = renderFactory.CreateRenderer(new PlantUmlSettings
        {
            RemoteUrl = "https://www.plantuml.com/plantuml/", //TODO: expose these are configurable
            RenderingMode = RenderingMode.Remote,
        });

        _pipeline = pipeline;
    }

    /// <summary>
    /// Writes a PlantUML script as HTML with an embedded SVG diagram.
    /// Includes a collapsible details section with the PlantUML source code.
    /// </summary>
    /// <param name="renderer">The HTML renderer to write to.</param>
    /// <param name="script">The PlantUML script to render.</param>
    public void Write(HtmlRenderer renderer, string script)
    {
        if (string.IsNullOrWhiteSpace(script))
        {
            return;
        }
        else if (!renderer.EnableHtmlForInline)
        {
            renderer.Write(script);
            return;
        }

        //image
        //Details

        var svg = _render.Render(script, OutputFormat.Svg);
        var xml = XElement.Parse(Encoding.UTF8.GetString(svg));
        renderer.Write(xml.ToString() + Environment.NewLine + Environment.NewLine);

        renderer.Write("<details>" + Environment.NewLine);
        renderer.Write("\t<summary>PlantUML - Details</summary>" + Environment.NewLine + Environment.NewLine);
        renderer.Write("<pre><code type=\"plantuml\">" + Environment.NewLine);
        renderer.Write(script);
        renderer.Write("</code></pre></details>" + Environment.NewLine + Environment.NewLine);
    }

    /// <summary>
    /// Writes a PlantUML script as GitHub-flavored Markdown with an embedded diagram image.
    /// Includes a collapsible details section with the PlantUML source code.
    /// </summary>
    /// <param name="renderer">The normalize renderer to write to.</param>
    /// <param name="script">The PlantUML script to render.</param>
    public void Write(NormalizeRenderer renderer, string script)
    {
        script = script.Trim();
        if (script.StartsWith("@startuml", StringComparison.InvariantCultureIgnoreCase))
        {
            script = script[9..];
        }
        if (script.EndsWith("@enduml", StringComparison.InvariantCultureIgnoreCase))
        {
            script = script[..^7];
        }
        script = script.Trim();

        renderer.Write($"![PlantUML Diagram]({_render.RenderAsUri(script, OutputFormat.Svg)})" + Environment.NewLine + Environment.NewLine);

        renderer.Write("<details>" + Environment.NewLine);
        renderer.Write("\t<summary>PlantUML - Details</summary>" + Environment.NewLine + Environment.NewLine);
        renderer.Write("```plantuml" + Environment.NewLine);
        renderer.Write(script + Environment.NewLine);
        renderer.Write("```" + Environment.NewLine + Environment.NewLine);
        renderer.Write("</details>" + Environment.NewLine + Environment.NewLine);
    }

    /// <summary>
    /// Builds a Markdown-formatted exception message for PlantUML rendering errors.
    /// </summary>
    /// <param name="exception">The exception that occurred during rendering.</param>
    /// <param name="stackTrace">If true, includes the exception stack trace in the message.</param>
    /// <returns>A Markdown-formatted error message in a code block.</returns>
    public string BuildMarkdownExceptionMessage(Exception exception, bool stackTrace)
    {
        var message = "```" + Environment.NewLine + "PlantUML exception:" + Environment.NewLine + exception.Message;
        if (exception is FileNotFoundException)
        {
            message += " (" + ((FileNotFoundException)exception).FileName + ")";
        }
        if (stackTrace)
        {
            message += Environment.NewLine;
            message += exception.StackTrace;
        }
        message = message + Environment.NewLine + "```" + Environment.NewLine;
        return message;
    }
}
