using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Linq;
using System.Text;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class HeicContainerTests
{
    public required TestContext TestContext { get; set; }

    private static byte[] Box(string type, params byte[][] body)
    {
        var data = body.SelectMany(b => b).ToArray();
        var size = data.Length + 8;
        return [(byte)(size >> 24), (byte)(size >> 16), (byte)(size >> 8), (byte)size, .. Encoding.ASCII.GetBytes(type), .. data];
    }

    private static byte[] U16(int v) => [(byte)(v >> 8), (byte)v];

    private static byte[] U32(int v) => [(byte)(v >> 24), (byte)(v >> 16), (byte)(v >> 8), (byte)v];

    private static byte[] Infe(int id, string type)
        => Box("infe", [2, 0, 0, 0], U16(id), U16(0), Encoding.ASCII.GetBytes(type), [0]);

    private static byte[] Heic(byte[]? tiff, int width, int height, byte? rotation)
    {
        var ftyp = Box("ftyp", "heic"u8.ToArray(), U32(0));
        byte[] Meta(int offset, int length)
        {
            var props = rotation is null
                ? Box("ipco", Box("ispe", U32(0), U32(width), U32(height)))
                : Box("ipco", Box("ispe", U32(0), U32(width), U32(height)), Box("irot", [rotation.Value]));
            var ipma = Box("ipma", U32(0), U32(1), U16(1), [(byte)(rotation is null ? 1 : 2)], rotation is null ? [1] : [1, 2]);
            return Box(
                "meta",
                U32(0),
                Box("pitm", U32(0), U16(1)),
                Box("iinf", U32(0), U16(tiff is null ? 1 : 2), Infe(1, "hvc1"), tiff is null ? [] : Infe(2, "Exif")),
                Box("iloc", U32(0), [0x44, 0x00], U16(tiff is null ? 0 : 1), tiff is null ? [] : [.. U16(2), .. U16(0), .. U16(1), .. U32(offset), .. U32(length)]),
                Box("iprp", props, ipma));
        }

        var payload = tiff is null ? [] : U32(0).Concat(tiff).ToArray();
        var metaLength = Meta(0, payload.Length).Length;
        var file = ftyp.Concat(Meta(ftyp.Length + metaLength + 8, payload.Length)).Concat(Box("mdat", payload)).ToArray();
        return file;
    }

    [TestMethod]
    public void Read_ExifItem_ReturnsSizeAndTiff()
    {
        var tiff = new TiffBuilder(true).Ascii(271, "Canon").Build();
        var file = Heic(tiff, 4032, 3024, null);

        var info = HeicContainer.Read(file);

        TestContext.WriteLine($"{info?.Width}x{info?.Height}");
        Assert.IsNotNull(info);
        Assert.AreEqual(4032, info.Width);
        Assert.AreEqual(3024, info.Height);
        CollectionAssert.AreEqual(tiff, info.Tiff);
        CollectionAssert.AreEqual(tiff, ExifLocator.Find(file).ToArray());
    }

    [TestMethod]
    public void Read_QuarterTurn_SwapsSize()
    {
        var info = HeicContainer.Read(Heic(null, 4032, 3024, 1));

        Assert.IsNotNull(info);
        Assert.AreEqual(3024, info.Width);
        Assert.AreEqual(4032, info.Height);
        Assert.IsNull(info.Tiff);
    }

    [TestMethod]
    public void Read_HalfTurn_KeepsSize()
    {
        var info = HeicContainer.Read(Heic(null, 4032, 3024, 2));

        Assert.AreEqual(4032, info!.Width);
    }

    [TestMethod]
    public void Read_NoExifItem_FindsNothing()
    {
        Assert.AreEqual(0, ExifLocator.Find(Heic(null, 10, 10, null)).Length);
    }

    [TestMethod]
    public void Read_Garbage_ReturnsNull()
    {
        var file = Heic(null, 10, 10, null);

        Assert.IsNull(HeicContainer.Read(file[..40]));
        Assert.IsNull(HeicContainer.Read(new byte[50]));
        Assert.IsFalse(HeicContainer.IsHeic(new byte[50]));
        Assert.IsTrue(HeicContainer.IsHeic(file));
    }
}
