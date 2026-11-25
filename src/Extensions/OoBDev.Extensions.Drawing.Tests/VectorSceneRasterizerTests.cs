using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Hpgl;
using OoBDev.TestUtilities;
using SkiaSharp;

namespace OoBDev.Extensions.Drawing.Tests;

[TestClass]
public class VectorSceneRasterizerTests
{
    private static readonly VectorScene _line = new(100, 50, [new VectorPolyline(1, "#000000", [new VectorPoint(0, 25), new VectorPoint(100, 25)])]);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rasterize_Png_HasTheRequestedWidthAndKeepsTheAspectRatio()
    {
        var bytes = VectorSceneRasterizer.Rasterize(_line, 200);

        using var bitmap = SKBitmap.Decode(bytes);
        Assert.AreEqual(200, bitmap.Width);
        Assert.AreEqual(100, bitmap.Height);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rasterize_KnownLine_DarkensItsRowAndLeavesTheCornerWhite()
    {
        using var bitmap = SKBitmap.Decode(VectorSceneRasterizer.Rasterize(_line, 200));

        Assert.IsLessThan(128, bitmap.GetPixel(100, 50).Red, "The line runs across the middle row.");
        Assert.AreEqual(SKColors.White, bitmap.GetPixel(100, 5));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rasterize_EmptyScene_IsAllWhite()
    {
        using var bitmap = SKBitmap.Decode(VectorSceneRasterizer.Rasterize(VectorScene.Empty, 50));

        Assert.AreEqual(SKColors.White, bitmap.GetPixel(10, 10));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rasterize_Jpeg_StartsWithTheJpegMarker()
    {
        var bytes = VectorSceneRasterizer.Rasterize(_line, 100, SKEncodedImageFormat.Jpeg);

        Assert.AreEqual(0xFF, bytes[0]);
        Assert.AreEqual(0xD8, bytes[1]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(0)]
    [DataRow(-5)]
    [DataRow(VectorSceneRasterizer.MaxWidthPixels + 1)]
    public void Rasterize_WidthOutOfRange_Throws(int width) =>
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => VectorSceneRasterizer.Rasterize(_line, width));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Rasterize_UnsupportedFormat_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => VectorSceneRasterizer.Rasterize(_line, 50, SKEncodedImageFormat.Gif));
}
