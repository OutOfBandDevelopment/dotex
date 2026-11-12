using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Linq;
using WkHtmlToPdfDotNet;

namespace OoBDev.WkHtmlToPdf.Tests;

[TestClass]
public class HtmlToPdfConversionHandlerDocumentTests
{
    private static HtmlToPdfConversionHandler CreateHandler(HtmlToPdfOptions options) => new(null!, Options.Create(options));

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void CreateDocument_DefaultOptions_KeepsTheFormerFixedSettings()
    {
        // Test
        var doc = CreateHandler(new HtmlToPdfOptions()).CreateDocument("<p>x</p>");

        // Assert
        Assert.AreEqual(ColorMode.Color, doc.GlobalSettings.ColorMode);
        Assert.AreEqual(Orientation.Portrait, doc.GlobalSettings.Orientation);
        Assert.AreEqual(PaperKind.Letter, doc.GlobalSettings.PaperSize);
        var page = doc.Objects.Single();
        Assert.AreEqual("<p>x</p>", page.HtmlContent);
        Assert.AreEqual("utf-8", page.WebSettings.DefaultEncoding);
        Assert.AreEqual("Page [page] of [toPage]", page.HeaderSettings.Right);
        Assert.AreEqual(9, page.HeaderSettings.FontSize);
        Assert.IsTrue(page.HeaderSettings.Line);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void CreateDocument_ConfiguredOptions_AreApplied()
    {
        // Stage
        var options = new HtmlToPdfOptions
        {
            ColorMode = ColorMode.Grayscale,
            Orientation = Orientation.Landscape,
            PaperSize = PaperKind.A4,
            HeaderRight = string.Empty,
            HeaderFontSize = 12,
            HeaderLine = false,
        };

        // Test
        var doc = CreateHandler(options).CreateDocument("<p>x</p>");

        // Assert
        Assert.AreEqual(ColorMode.Grayscale, doc.GlobalSettings.ColorMode);
        Assert.AreEqual(Orientation.Landscape, doc.GlobalSettings.Orientation);
        Assert.AreEqual(PaperKind.A4, doc.GlobalSettings.PaperSize);
        var page = doc.Objects.Single();
        Assert.AreEqual(string.Empty, page.HeaderSettings.Right);
        Assert.AreEqual(12, page.HeaderSettings.FontSize);
        Assert.IsFalse(page.HeaderSettings.Line);
    }
}
