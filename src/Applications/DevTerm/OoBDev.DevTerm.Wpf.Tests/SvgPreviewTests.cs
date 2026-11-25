using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The SVG subset drawn for the stream monitor preview.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class SvgPreviewTests
{
    [TestMethod]
    public void TryRender_ViewBox_SizesTheDrawingToIt()
    {
        // Stage
        const string svg = "<svg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 200 100'><line x1='0' y1='0' x2='200' y2='100' stroke='black'/></svg>";

        // Test
        var image = SvgPreview.TryRender(svg, out var error);

        // Assert
        Assert.IsNotNull(image, error);
        Assert.AreEqual(new Rect(0, 0, 200, 100), image.Drawing.Bounds);
        Assert.IsNull(error);
    }

    [TestMethod]
    public void TryRender_NoViewBox_UsesWidthAndHeight()
    {
        // Stage
        const string svg = "<svg width='50px' height='30'><rect x='1' y='1' width='10' height='10'/></svg>";

        // Test
        var image = SvgPreview.TryRender(svg, out _);

        // Assert
        Assert.IsNotNull(image);
        Assert.AreEqual(50, image.Drawing.Bounds.Width);
        Assert.AreEqual(30, image.Drawing.Bounds.Height);
    }

    [TestMethod]
    public void TryRender_Shapes_AreDrawnOverTheBackground()
    {
        // Stage
        const string svg = "<svg viewBox='0 0 10 10'><circle cx='5' cy='5' r='2' fill='red'/><polygon points='0,0 5,0 5,5'/><path d='M0,0 L10,10' stroke='blue'/></svg>";

        // Test
        var image = SvgPreview.TryRender(svg, out _);

        // Assert
        Assert.IsNotNull(image);
        Assert.AreEqual(4, ((DrawingGroup)image.Drawing).Children.Count);
    }

    [TestMethod]
    public void TryRender_NotAnSvgRoot_ReportsWhy()
    {
        // Stage / Test
        var image = SvgPreview.TryRender("<html/>", out var error);

        // Assert
        Assert.IsNull(image);
        Assert.AreEqual("it isn't an SVG document.", error);
    }

    [TestMethod]
    public void TryRender_BadXml_ReportsWhy()
    {
        // Stage / Test
        var image = SvgPreview.TryRender("<svg", out var error);

        // Assert
        Assert.IsNull(image);
        Assert.IsFalse(string.IsNullOrEmpty(error));
    }

    [TestMethod]
    public void TryRender_WideDrawing_KeepsHairlinesVisible()
    {
        // Stage
        const string svg = "<svg viewBox='0 0 5000 5000'><line x1='0' y1='0' x2='5000' y2='5000' stroke='black' stroke-width='1'/></svg>";

        // Test
        var image = SvgPreview.TryRender(svg, out _);

        // Assert
        var line = (GeometryDrawing)((DrawingGroup)image!.Drawing).Children[1];
        Assert.IsTrue(line.Pen!.Thickness >= 20);
    }
}
