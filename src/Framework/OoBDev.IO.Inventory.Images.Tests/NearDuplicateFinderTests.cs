using System;
using System.IO;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using SkiaSharp;

namespace OoBDev.IO.Inventory.Images.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class NearDuplicateFinderTests
{
    public TestContext TestContext { get; set; } = null!;

    private static byte[] Png(int width, int height, Action<SKCanvas, int, int> draw)
    {
        using var bitmap = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            draw(canvas, width, height);
        }

        using var image = SKImage.FromBitmap(bitmap);
        return image.Encode(SKEncodedImageFormat.Png, 100).ToArray();
    }

    private static void Scene(SKCanvas c, int w, int h)
    {
        using var dark = new SKPaint { Color = SKColors.Black };
        using var grey = new SKPaint { Color = new SKColor(120, 120, 120) };
        c.DrawRect(0, 0, w * 0.3f, h, dark);
        c.DrawRect(w * 0.5f, 0, w * 0.2f, h * 0.5f, grey);
        c.DrawRect(w * 0.7f, h * 0.5f, w * 0.3f, h * 0.5f, dark);
    }

    private static void Other(SKCanvas c, int w, int h)
    {
        using var dark = new SKPaint { Color = SKColors.Black };
        c.DrawRect(0, h * 0.4f, w, h * 0.2f, dark);
        c.DrawRect(w * 0.1f, 0, w * 0.1f, h, dark);
        c.DrawRect(w * 0.6f, 0, w * 0.1f, h, dark);
    }

    [TestMethod]
    public void Scan_ImageAndResizedCopy_GroupedUnrelatedImageIsNot()
    {
        using var dir = new TempFileFactory(null!).GetTempDirectory();
        File.WriteAllBytes(Path.Combine(dir.DirectoryPath, "a.png"), Png(400, 300, Scene));
        File.WriteAllBytes(Path.Combine(dir.DirectoryPath, "a-small.png"), Png(160, 120, Scene));
        File.WriteAllBytes(Path.Combine(dir.DirectoryPath, "b.png"), Png(400, 300, Other));
        File.WriteAllText(Path.Combine(dir.DirectoryPath, "broken.png"), "not an image");

        var (groups, skipped) = new NearDuplicateFinder().Scan(dir.DirectoryPath, 5, CancellationToken.None);
        TestContext.WriteLine($"groups={groups.Count} skipped={skipped.Count}");

        Assert.AreEqual(1, groups.Count);
        CollectionAssert.AreEqual(new[] { Path.Combine(dir.DirectoryPath, "a-small.png"), Path.Combine(dir.DirectoryPath, "a.png") }, (global::System.Collections.ICollection)groups[0]);
        Assert.AreEqual(1, skipped.Count);
    }

    [TestMethod]
    public void DHash_SameImage_ZeroDistanceDifferentImage_Far()
    {
        using var a1 = new MemoryStream(Png(200, 200, Scene));
        using var a2 = new MemoryStream(Png(200, 200, Scene));
        using var b = new MemoryStream(Png(200, 200, Other));

        Assert.AreEqual(0, ImageHasher.Distance(ImageHasher.DHash(a1), ImageHasher.DHash(a2)));
        Assert.IsTrue(ImageHasher.Distance(ImageHasher.DHash(new MemoryStream(Png(200, 200, Scene))), ImageHasher.DHash(b)) > 5);
    }

    [TestMethod]
    public void Group_SingleLinkage_ChainsNearHashes()
    {
        var groups = new NearDuplicateFinder().Group([("a", 0L), ("b", 0b11L), ("c", 0b1111L), ("z", -1L)], 2);

        Assert.AreEqual(1, groups.Count);
        CollectionAssert.AreEqual(new[] { "a", "b", "c" }, (global::System.Collections.ICollection)groups[0]);
    }
}
