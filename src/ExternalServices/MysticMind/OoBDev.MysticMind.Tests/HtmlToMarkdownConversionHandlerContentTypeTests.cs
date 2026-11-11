using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using ReverseMarkdown;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.MysticMind.Tests;

[TestClass]
public class HtmlToMarkdownConversionHandlerContentTypeTests
{
    private static HtmlToMarkdownConversionHandler CreateHandler() => new(new Converter());

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("text/html")]
    [DataRow("text/xhtml")]
    [DataRow("text/xhtml+xml")]
    [DataRow("TEXT/HTML")]
    public void SupportedSource_HtmlTypes_ReturnsTrue(string contentType)
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
    [DataRow("text/markdown")]
    [DataRow("text/x-markdown")]
    [DataRow("text/plain")]
    public void SupportedDestination_MarkdownTypes_ReturnsTrue(string contentType)
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
    public void SupportedDestination_Html_ReturnsFalse()
    {
        // Stage
        var handler = CreateHandler();

        // Test
        var supported = handler.SupportedDestination("text/html");

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
        var action = () => handler.ConvertAsync(source, "text/markdown", destination, "text/markdown");

        // Assert
        var ex = await Assert.ThrowsExactlyAsync<NotSupportedException>(action);
        StringAssert.Contains(ex.Message, "Source Content Type");
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
        var action = () => handler.ConvertAsync(source, "text/html", destination, "application/pdf");

        // Assert
        var ex = await Assert.ThrowsExactlyAsync<NotSupportedException>(action);
        StringAssert.Contains(ex.Message, "Destination Content Type");
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task ConvertAsync_Html_WritesMarkdownAndRewindsDestination()
    {
        // Stage
        var handler = CreateHandler();
        using var source = new MemoryStream(Encoding.UTF8.GetBytes("<h1>Title</h1>"));
        using var destination = new MemoryStream();

        // Test
        await handler.ConvertAsync(source, "text/html", destination, "text/markdown");

        // Assert
        Assert.AreEqual(0, destination.Position);
        using var reader = new StreamReader(destination, leaveOpen: true);
        StringAssert.Contains(await reader.ReadToEndAsync(), "# Title");
    }
}
