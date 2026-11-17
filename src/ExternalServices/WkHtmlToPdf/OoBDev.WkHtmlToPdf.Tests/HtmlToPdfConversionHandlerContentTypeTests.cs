using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.WkHtmlToPdf.Tests;

[TestClass]
public class HtmlToPdfConversionHandlerContentTypeTests
{
    // The converter is only used after the content type checks, so these tests need no native library.
    private static HtmlToPdfConversionHandler CreateHandler() => new(null!, Options.Create(new HtmlToPdfOptions()));

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
    [DataRow("application/pdf")]
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
    [DataRow("application/pdf")]
    [DataRow("APPLICATION/PDF")]
    public void SupportedDestination_Pdf_ReturnsTrue(string contentType)
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
        var action = () => handler.ConvertAsync(source, "text/markdown", destination, "application/pdf");

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
        var action = () => handler.ConvertAsync(source, "text/html", destination, "text/html");

        // Assert
        var ex = await Assert.ThrowsExactlyAsync<NotSupportedException>(action);
        StringAssert.Contains(ex.Message, "Destination Content Type");
    }
}
