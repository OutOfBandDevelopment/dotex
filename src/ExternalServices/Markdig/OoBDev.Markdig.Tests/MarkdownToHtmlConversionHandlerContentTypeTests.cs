using global::Markdig;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Markdig.Tests;

[TestClass]
public class MarkdownToHtmlConversionHandlerContentTypeTests
{
    private static MarkdownToHtmlConversionHandler CreateHandler() => new(new MarkdownPipelineBuilder().Build());

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("text/markdown")]
    [DataRow("TEXT/Markdown")]
    [DataRow("text/x-markdown")]
    [DataRow("text/plain")]
    public void SupportedSource_MarkdownTypes_ReturnsTrue(string contentType)
    {
        // Stage
        var handler = CreateHandler();

        // Test
        var supported = handler.SupportedSource(contentType);

        // Assert
        Assert.IsTrue(supported);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("text/html")]
    [DataRow("application/pdf")]
    [DataRow("")]
    public void SupportedSource_OtherTypes_ReturnsFalse(string contentType)
    {
        // Stage
        var handler = CreateHandler();

        // Test
        var supported = handler.SupportedSource(contentType);

        // Assert
        Assert.IsFalse(supported);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("text/html")]
    [DataRow("text/xhtml")]
    [DataRow("text/xhtml+xml")]
    [DataRow("TEXT/HTML")]
    public void SupportedDestination_HtmlTypes_ReturnsTrue(string contentType)
    {
        // Stage
        var handler = CreateHandler();

        // Test
        var supported = handler.SupportedDestination(contentType);

        // Assert
        Assert.IsTrue(supported);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("text/markdown")]
    [DataRow("application/pdf")]
    public void SupportedDestination_OtherTypes_ReturnsFalse(string contentType)
    {
        // Stage
        var handler = CreateHandler();

        // Test
        var supported = handler.SupportedDestination(contentType);

        // Assert
        Assert.IsFalse(supported);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task ConvertAsync_UnsupportedSource_ThrowsNotSupported()
    {
        // Stage
        var handler = CreateHandler();
        using var source = new MemoryStream();
        using var destination = new MemoryStream();

        // Test
        var action = () => handler.ConvertAsync(source, "application/pdf", destination, "text/html");

        // Assert
        await Assert.ThrowsExactlyAsync<NotSupportedException>(action);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task ConvertAsync_UnsupportedDestination_ThrowsNotSupported()
    {
        // Stage
        var handler = CreateHandler();
        using var source = new MemoryStream();
        using var destination = new MemoryStream();

        // Test
        var action = () => handler.ConvertAsync(source, "text/markdown", destination, "application/pdf");

        // Assert
        var ex = await Assert.ThrowsExactlyAsync<NotSupportedException>(action);
        StringAssert.Contains(ex.Message, "Destination Content Type");
    }
}
