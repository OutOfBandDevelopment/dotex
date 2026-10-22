
using Microsoft.AspNetCore.Mvc;
using OoBDev.System.Text.Html;
using OoBDev.System.Text.Json;
using OoBDev.System.Text.Yaml;
using OoBDev.System.Xml.XPath;
using OoBDev.WebApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.XPath;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// Text utilities: query JSON, YAML, HTML and XML with one XPath syntax, and render Markdown.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TextToolsController : ControllerBase
{
    private const int MaxLength = 200_000;

    private static readonly global::Markdig.MarkdownPipeline Pipeline = BuildPipeline();

    /// <summary>Runs an XPath query over a JSON, YAML, HTML or XML document.</summary>
    /// <param name="model">The format, document and query.</param>
    /// <returns>The matches and the XML form of the document.</returns>
    [HttpPost("xpath")]
    public ActionResult<XPathQueryResponseModel> Query([FromBody] XPathQueryRequestModel model)
    {
        if (model.Content.Length > MaxLength)
            return Problem(title: "Document too large", detail: $"Keep it under {MaxLength} characters.", statusCode: 413);
        IToXPathNavigable navigator = model.Format.ToLowerInvariant() switch
        {
            "json" => new JsonNavigator(),
            "yaml" => new YamlNavigator(),
            "html" => new HtmlNavigator(),
            "xml" => new XmlNavigable(),
            _ => throw new ArgumentOutOfRangeException(nameof(model), "Use json, yaml, html or xml."),
        };
        try
        {
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(model.Content));
            var navigable = navigator.ToNavigable(stream) ?? throw new InvalidDataException("The document is empty.");
            var nav = navigable.CreateNavigator() ?? throw new InvalidDataException("The document has no navigator.");
            var matches = new global::System.Collections.Generic.List<string>();
            var evaluated = nav.Evaluate(model.XPath);
            if (evaluated is XPathNodeIterator iterator)
            {
                foreach (XPathNavigator node in iterator)
                    matches.Add(node.NodeType == XPathNodeType.Element ? node.OuterXml : node.Value);
            }
            else
            {
                matches.Add(Convert.ToString(evaluated, global::System.Globalization.CultureInfo.InvariantCulture) ?? "");
            }
            return new XPathQueryResponseModel { Count = matches.Count, Matches = matches, Xml = nav.OuterXml };
        }
        catch (Exception ex) when (ex is XPathException or XmlException or InvalidDataException or ArgumentException or InvalidOperationException
            or global::System.Text.Json.JsonException or YamlDotNet.Core.YamlException)
        {
            return Problem(title: "Could not query the document", detail: ex.Message, statusCode: 400);
        }
    }

    /// <summary>Renders Markdown to HTML with the advanced extensions on. Raw HTML is passed through, so show the result in a sandbox.</summary>
    /// <param name="model">The Markdown.</param>
    /// <returns>The HTML.</returns>
    [HttpPost("markdown")]
    public MarkdownResponseModel Markdown([FromBody] MarkdownRequestModel model) =>
        new() { Html = global::Markdig.Markdown.ToHtml(model.Markdown, Pipeline) };

    private static global::Markdig.MarkdownPipeline BuildPipeline()
    {
        var builder = new global::Markdig.MarkdownPipelineBuilder();
        global::Markdig.MarkdownExtensions.UseAdvancedExtensions(builder);
        return builder.Build();
    }

    private sealed class XmlNavigable : IToXPathNavigable
    {
        public IXPathNavigable? ToNavigable(string filePath) => ToNavigable(global::System.IO.File.OpenRead(filePath));

        public IXPathNavigable? ToNavigable(Stream stream)
        {
            var doc = new XmlDocument { XmlResolver = null };
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null });
            doc.Load(reader);
            return doc;
        }
    }
}
