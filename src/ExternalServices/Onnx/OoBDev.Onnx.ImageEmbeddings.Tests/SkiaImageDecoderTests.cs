using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Onnx.ImageEmbeddings.Skia;
using OoBDev.TestUtilities;
using System;
using System.IO;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

[TestClass]
public class SkiaImageDecoderTests
{
    private static byte[] Image(string name) => File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "TestData", "images", name));

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("scene-224.png", 224, 224)]
    [DataRow("scene-640x480.png", 640, 480)]
    [DataRow("tiny-1x1.png", 1, 1)]
    [DataRow("gray-300.png", 300, 300)]
    [DataRow("palette-200.png", 200, 200)]
    [DataRow("alpha-256.png", 256, 256)]
    [DataRow("scene-640x480-q85.jpg", 640, 480)]
    [DataRow("scene-333x517-q40.jpg", 333, 517)]
    public void Decode_ReturnsRgbOfExpectedSize(string file, int width, int height)
    {
        var decoded = new SkiaImageDecoder().Decode(Image(file), 100_000_000);

        Assert.AreEqual(width, decoded.Width);
        Assert.AreEqual(height, decoded.Height);
        Assert.AreEqual(width * height * 3, decoded.Rgb.Length);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Decode_Png_IsLossless()
    {
        // red-square.png: the center is pure red.
        var decoded = new SkiaImageDecoder().Decode(Image("red-square.png"), 100_000_000);
        var center = ((decoded.Height / 2 * decoded.Width) + (decoded.Width / 2)) * 3;

        Assert.AreEqual(255, decoded.Rgb[center]);
        Assert.AreEqual(0, decoded.Rgb[center + 1]);
        Assert.AreEqual(0, decoded.Rgb[center + 2]);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Decode_TooManyPixels_Throws()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => new SkiaImageDecoder().Decode(Image("scene-640x480.png"), 1000));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Decode_NotAnImage_Throws()
    {
        Assert.ThrowsExactly<InvalidDataException>(() => new SkiaImageDecoder().Decode(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 1000));
    }
}
