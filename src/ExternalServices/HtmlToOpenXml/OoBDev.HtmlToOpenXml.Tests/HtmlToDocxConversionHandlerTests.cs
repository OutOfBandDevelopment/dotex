using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.HtmlToOpenXml.Tests;

[TestClass]
public class HtmlToDocxConversionHandlerTests
{
    private const string Docx = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("text/html")]
    [DataRow("text/xhtml")]
    [DataRow("text/xhtml+xml")]
    [DataRow("TEXT/HTML")]
    public void SupportedSource_HtmlTypes_ReturnsTrue(string contentType)
    {
        // Stage
        var handler = new HtmlToDocxConversionHandler();

        // Test
        var supported = handler.SupportedSource(contentType);

        // Assert
        Assert.IsTrue(supported);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow(Docx)]
    [DataRow("APPLICATION/VND.OPENXMLFORMATS-OFFICEDOCUMENT.WORDPROCESSINGML.DOCUMENT")]
    public void SupportedDestination_Docx_ReturnsTrue(string contentType)
    {
        // Stage
        var handler = new HtmlToDocxConversionHandler();

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
        var handler = new HtmlToDocxConversionHandler();

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
        var handler = new HtmlToDocxConversionHandler();
        using var source = new MemoryStream();
        using var destination = new MemoryStream();

        // Test
        var action = () => handler.ConvertAsync(source, "text/markdown", destination, Docx);

        // Assert
        var ex = await Assert.ThrowsExactlyAsync<NotSupportedException>(action);
        StringAssert.Contains(ex.Message, "Source Content Type");
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public async Task ConvertAsync_UnsupportedDestination_ThrowsNotSupported()
    {
        // Stage
        var handler = new HtmlToDocxConversionHandler();
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
    public async Task ConvertAsync_Html_WritesRewoundDocxPackage()
    {
        // Stage
        var handler = new HtmlToDocxConversionHandler();
        using var source = new MemoryStream(Encoding.UTF8.GetBytes("<html><body><p>Hello</p></body></html>"));
        using var destination = new MemoryStream();

        // Test
        await handler.ConvertAsync(source, "text/html", destination, Docx);

        // Assert
        Assert.AreEqual(0, destination.Position);
        Assert.IsGreaterThan(0, destination.Length);
        Assert.AreEqual((byte)'P', destination.ReadByte());
        Assert.AreEqual((byte)'K', destination.ReadByte());
    }
}
