using System;
using System.IO;
using System.Text;
using Markdig;
using Markdig.Renderers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.TestUtilities;
using PlantUml.Net;

namespace OoBDev.System.Text.Markdown.Tests;

[TestCategory(TestCategories.Unit)]
[TestClass]
public class PlantUmlRendererTests
{
    [TestMethod]
    public void Write_Html_EscapesTheScriptInTheDetailsBlock()
    {
        // Stage
        var backend = new Mock<IPlantUmlRenderer>();
        backend.Setup(r => r.Render(It.IsAny<string>(), OutputFormat.Svg)).Returns(Encoding.UTF8.GetBytes("<svg xmlns=\"http://www.w3.org/2000/svg\"/>"));
        var pipeline = new MarkdownPipelineBuilder().Build();
        var renderer = new PlantUmlRenderer(pipeline, backend.Object);
        using var writer = new StringWriter();
        var html = new HtmlRenderer(writer);

        // Test
        renderer.Write(html, "@startuml\nA -> B : <script>alert(1)</script>\n@enduml");

        // Assert
        var result = writer.ToString();
        Assert.DoesNotContain("<script>", result, StringComparison.Ordinal);
        Assert.Contains("&lt;script&gt;", result, StringComparison.Ordinal);
        Assert.Contains("<svg", result, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Constructor_BlankRemoteUrl_ThrowsWithAClearMessage()
    {
        // Stage
        var options = new PlantUmlRendererOptions { RemoteUrl = " " };

        // Test
        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => new PlantUmlRenderer(new MarkdownPipelineBuilder().Build(), options));

        // Assert
        Assert.Contains("RemoteUrl", ex.Message, StringComparison.Ordinal);
    }

    [TestMethod]
    public void Options_Default_PointsAtTheLocalContainer() =>
        Assert.AreEqual("http://localhost:8086/", new PlantUmlRendererOptions().RemoteUrl);

    [TestMethod]
    public void UsePlantuml_BlankRemoteUrl_FailsWhenTheRendererIsSetUp()
    {
        // Stage
        var pipeline = new MarkdownPipelineBuilder().UsePlantuml(new PlantUmlRendererOptions { RemoteUrl = null }).Build();
        using var writer = new StringWriter();

        // Test and Assert
        Assert.ThrowsExactly<InvalidOperationException>(() => pipeline.Setup(new HtmlRenderer(writer)));
    }

    [TestMethod]
    public void Write_Html_BlankScript_WritesNothing()
    {
        // Stage
        var backend = new Mock<IPlantUmlRenderer>(MockBehavior.Strict);
        var renderer = new PlantUmlRenderer(new MarkdownPipelineBuilder().Build(), backend.Object);
        using var writer = new StringWriter();

        // Test
        renderer.Write(new HtmlRenderer(writer), "  ");

        // Assert
        Assert.AreEqual(string.Empty, writer.ToString());
    }
}
