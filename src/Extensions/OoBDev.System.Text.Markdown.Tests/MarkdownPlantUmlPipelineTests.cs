using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Markdig;
using Markdig.Renderers.Normalize;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using _Markdown = Markdig.Markdown;

namespace OoBDev.System.Text.Markdown.Tests;

/// <summary>The whole Markdig pipeline with the PlantUML extension, against a local stub of the PlantUML server.</summary>
[TestCategory(TestCategories.Unit)]
[TestClass]
public class MarkdownPlantUmlPipelineTests
{
    private const string Document = "# Title\n\nSome *text* here.\n\n```plantuml\n@startuml\nA -> B : hello\n@enduml\n```\n\n```csharp\nvar x = 1;\n```\n\n- one\n- two\n";

    private HttpListener? _listener;
    private string _url = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        for (var port = 18600; port < 18700; port++)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://127.0.0.1:{port}/");
            try
            {
                listener.Start();
            }
            catch (HttpListenerException)
            {
                listener.Close();
                continue;
            }

            _listener = listener;
            _url = $"http://127.0.0.1:{port}/";
            _ = Task.Run(Serve);
            return;
        }

        Assert.Inconclusive("No free local port for the PlantUML stub.");
    }

    [TestCleanup]
    public void Cleanup() => _listener?.Close();

    private async Task Serve()
    {
        while (_listener is { IsListening: true })
        {
            try
            {
                var context = await _listener.GetContextAsync();
                var body = Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"><title>stub</title></svg>");
                context.Response.ContentType = "image/svg+xml";
                await context.Response.OutputStream.WriteAsync(body);
                context.Response.Close();
            }
            catch (Exception ex) when (ex is HttpListenerException or ObjectDisposedException)
            {
                return;
            }
        }
    }

    private MarkdownPipeline Pipeline() =>
        new MarkdownPipelineBuilder().UseAdvancedExtensions().UsePlantuml(new PlantUmlRendererOptions { RemoteUrl = _url }).Build();

    [TestMethod]
    public void ToHtml_PlantUmlFence_RendersTheSvgAndKeepsTheSourceInDetails()
    {
        // Test
        var html = _Markdown.ToHtml(Document, Pipeline());

        // Assert
        StringAssert.Contains(html, "<h1");
        StringAssert.Contains(html, "<svg");
        StringAssert.Contains(html, "stub");
        StringAssert.Contains(html, "<summary>PlantUML - Details</summary>");
        StringAssert.Contains(html, "A -&gt; B : hello");
        StringAssert.Contains(html, "<li>one</li>");
    }

    [TestMethod]
    public void ToHtml_OtherFence_IsNotTreatedAsPlantUml()
    {
        var html = _Markdown.ToHtml("```csharp\nvar x = 1;\n```\n", Pipeline());

        StringAssert.Contains(html, "language-csharp");
        Assert.IsFalse(html.Contains("<svg", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ToHtml_PlantUmlFenceWithArguments_IsStillDiagram()
    {
        var html = _Markdown.ToHtml("```plantuml width=100\n@startuml\nA -> B\n@enduml\n```\n", Pipeline());

        StringAssert.Contains(html, "<svg");
    }

    [TestMethod]
    public void Normalize_PlantUmlFence_BecomesAnImageWithTheSourceInDetails()
    {
        // Test
        var markdown = _Markdown.Normalize(Document, new NormalizeOptions
        {
            EmptyLineAfterCodeBlock = true,
            EmptyLineAfterHeading = true,
            EmptyLineAfterThematicBreak = true,
            ExpandAutoLinks = true,
            ListItemCharacter = '-',
            SpaceAfterQuoteBlock = true,
        }, Pipeline());

        // Assert
        StringAssert.Contains(markdown, "![PlantUML Diagram](");
        StringAssert.Contains(markdown, "<summary>PlantUML - Details</summary>");
        StringAssert.Contains(markdown, "```plantuml");
        StringAssert.Contains(markdown, "A -> B : hello");
        Assert.IsFalse(markdown.Contains("@startuml", StringComparison.Ordinal), "The @startuml wrapper is stripped from the details source.");
        StringAssert.Contains(markdown, "- one");
        StringAssert.Contains(markdown, "```csharp");
    }

    [TestMethod]
    public void ToPlainText_Document_KeepsTheText()
    {
        var text = _Markdown.ToPlainText(Document, Pipeline());

        StringAssert.Contains(text, "Title");
        StringAssert.Contains(text, "Some text here.");
        StringAssert.Contains(text, "one");
    }

    [TestMethod]
    public void UsePlantuml_CalledTwice_AddsTheExtensionOnce()
    {
        var builder = new MarkdownPipelineBuilder().UsePlantuml(new PlantUmlRendererOptions { RemoteUrl = _url }).UsePlantuml();

        Assert.AreEqual(1, builder.Extensions.Count);
    }

    [TestMethod]
    public void BuildMarkdownExceptionMessage_WithAndWithoutStackTrace_FormatsACodeBlock()
    {
        // Stage
        var renderer = new PlantUmlRenderer(new MarkdownPipelineBuilder().Build(), new PlantUmlRendererOptions { RemoteUrl = _url });
        Exception thrown;
        try
        {
            throw new FileNotFoundException("missing", "diagram.puml");
        }
        catch (FileNotFoundException ex)
        {
            thrown = ex;
        }

        // Test
        var plain = renderer.BuildMarkdownExceptionMessage(thrown, false);
        var detailed = renderer.BuildMarkdownExceptionMessage(thrown, true);

        // Assert
        StringAssert.StartsWith(plain, "```");
        StringAssert.Contains(plain, "missing (diagram.puml)");
        Assert.IsFalse(plain.Contains("   at ", StringComparison.Ordinal));
        StringAssert.Contains(detailed, "   at ");
    }
}
