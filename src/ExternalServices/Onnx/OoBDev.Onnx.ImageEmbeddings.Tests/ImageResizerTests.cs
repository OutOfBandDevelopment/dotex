using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Onnx.ImageEmbeddings.Tests;

/// <summary>
/// Compares <see cref="ImageResizer"/> with Pillow output byte for byte
/// (scripts/embeddings/make-resize-golden.py).
/// </summary>
[TestClass]
public class ImageResizerTests
{
    private sealed record Case(int Width, int Height, int NewWidth, int NewHeight, int Filter, byte[] Source, byte[] Expected);

    private static Case[] Cases()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "TestData", "resize-golden.json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return [.. document.RootElement.GetProperty("cases").EnumerateArray().Select(c => new Case(
            c.GetProperty("width").GetInt32(),
            c.GetProperty("height").GetInt32(),
            c.GetProperty("newWidth").GetInt32(),
            c.GetProperty("newHeight").GetInt32(),
            c.GetProperty("filter").GetInt32(),
            Convert.FromBase64String(c.GetProperty("source").GetString()!),
            Convert.FromBase64String(c.GetProperty("expected").GetString()!)))];
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Resize_MatchesPillowExactly()
    {
        var failures = Cases()
            .Select(c =>
            {
                var actual = ImageResizer.Resize(c.Source, c.Width, c.Height, c.NewWidth, c.NewHeight, (ImageResample)c.Filter);
                var different = actual.Zip(c.Expected, (a, e) => a != e).Count(x => x);
                return (c, different);
            })
            .Where(x => x.different > 0)
            .Select(x => $"{x.c.Width}x{x.c.Height}->{x.c.NewWidth}x{x.c.NewHeight} filter {x.c.Filter}: {x.different} bytes differ")
            .ToArray();
        Assert.IsEmpty(failures, string.Join("; ", failures));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Resize_SameSize_ReturnsCopy()
    {
        var source = Enumerable.Range(0, 4 * 3 * 3).Select(i => (byte)i).ToArray();

        var result = ImageResizer.Resize(source, 4, 3, 4, 3, ImageResample.Bicubic);

        CollectionAssert.AreEqual(source, result);
        Assert.AreNotSame(source, result);
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Resize_SolidColor_StaysSolid()
    {
        var source = Enumerable.Repeat((byte)200, 30 * 20 * 3).ToArray();

        var result = ImageResizer.Resize(source, 30, 20, 7, 11, ImageResample.Bicubic);

        Assert.AreEqual(7 * 11 * 3, result.Length);
        Assert.IsTrue(result.All(b => b == 200));
    }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    public void Resize_BadArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ImageResizer.Resize(null!, 1, 1, 1, 1, ImageResample.Bicubic));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => ImageResizer.Resize(new byte[3], 1, 1, 0, 1, ImageResample.Bicubic));
        Assert.ThrowsExactly<ArgumentException>(() => ImageResizer.Resize(new byte[5], 1, 1, 2, 2, ImageResample.Bicubic));
    }
}
