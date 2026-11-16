using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Media.Metadata.Raw;
using OoBDev.TestUtilities;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
public class RawTests
{
    private static byte[] FakeJpeg(int size)
    {
        var jpeg = new byte[size];
        jpeg[0] = 0xFF;
        jpeg[1] = 0xD8;
        jpeg[^2] = 0xFF;
        jpeg[^1] = 0xD9;
        return jpeg;
    }

    /// <summary>A NEF-like file: IFD0 with Make and a SubIFD that holds the previews, JPEG data appended after the TIFF block.</summary>
    private static byte[] Nef(byte[] small, byte[] large, bool little = true)
    {
        byte[] Build(uint smallAt, uint largeAt) => new TiffBuilder(little)
            .Ascii(271, "NIKON CORPORATION")
            .Longs(TagJpegOffset, smallAt).Longs(TagJpegLength, (uint)small.Length)
            .Child(330, new TiffBuilder(little).Longs(TagJpegOffset, largeAt).Longs(TagJpegLength, (uint)large.Length))
            .Build();

        var length = (uint)Build(0, 0).Length;
        return [.. Build(length, length + (uint)small.Length), .. small, .. large];
    }

    private const int TagJpegOffset = 513;
    private const int TagJpegLength = 514;

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(true)]
    [DataRow(false)]
    public void IsRaw_NikonStyleTiff_True(bool little)
    {
        var file = Nef(FakeJpeg(40), FakeJpeg(120), little);

        Assert.IsTrue(new EmbeddedPreviewDeveloper().IsRaw(new MemoryStream(file)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_SeekableStream_ReturnsToStart()
    {
        using var stream = new MemoryStream(Nef(FakeJpeg(40), FakeJpeg(120)));
        stream.Position = 0;

        new EmbeddedPreviewDeveloper().IsRaw(stream);

        Assert.AreEqual(0, stream.Position);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_DngVersionTag_True()
    {
        var dng = new TiffBuilder().Bytes(50706, [1, 4, 0, 0]).Build();

        Assert.IsTrue(RawDetector.IsRaw(dng));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_Cr2Marker_True()
    {
        byte[] cr2 = [.. "II*\0"u8.ToArray(), 16, 0, 0, 0, (byte)'C', (byte)'R', 2, 0, 0, 0, 0, 0];

        Assert.IsTrue(RawDetector.IsRaw(cr2));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_Cr3Fuji_Olympus_Panasonic_True()
    {
        var cr3 = new byte[24];
        cr3[3] = 24;
        Encoding.ASCII.GetBytes("ftypcrx ").CopyTo(cr3, 4);
        var fuji = Encoding.ASCII.GetBytes("FUJIFILMCCD-RAW 0201");
        var orf = Encoding.ASCII.GetBytes("IIRO\b\0\0\0\0\0\0\0\0\0");
        var rw2 = Encoding.ASCII.GetBytes("IIU\0\x18\0\0\0\0\0\0\0\0");

        Assert.IsTrue(RawDetector.IsRaw(cr3));
        Assert.IsTrue(RawDetector.IsRaw(fuji));
        Assert.IsTrue(RawDetector.IsRaw(orf));
        Assert.IsTrue(RawDetector.IsRaw(rw2));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_JpegPlainTiffAndShortInput_False()
    {
        var plainTiff = new TiffBuilder().Ascii(271, "Canon").Build();
        var jpeg = TiffBuilder.AsJpeg(plainTiff);

        Assert.IsFalse(RawDetector.IsRaw(plainTiff));
        Assert.IsFalse(RawDetector.IsRaw(jpeg));
        Assert.IsFalse(RawDetector.IsRaw(new byte[5]));
        Assert.IsFalse(RawDetector.IsRaw(new byte[64]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_MakeWithSubIfdsOfUnknownMaker_False()
    {
        var file = new TiffBuilder().Ascii(271, "Acme Scanner").Child(330, new TiffBuilder().Longs(256, 1u)).Build();

        Assert.IsFalse(RawDetector.IsRaw(file));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Develop_WritesTheLargestPreviewAndLeavesSourceUnchanged()
    {
        var small = FakeJpeg(40);
        var large = FakeJpeg(120);
        var file = Nef(small, large);
        var copy = (byte[])file.Clone();
        using var output = new MemoryStream();

        await new EmbeddedPreviewDeveloper().DevelopAsync(new MemoryStream(file), output, new RawDevelopOptions(), CancellationToken.None);

        CollectionAssert.AreEqual(large, output.ToArray());
        CollectionAssert.AreEqual(copy, file);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Develop_StripStylePreview_Found()
    {
        var jpeg = FakeJpeg(60);
        byte[] Build(uint at) => new TiffBuilder().Shorts(259, 7).Longs(273, at).Longs(279, (uint)jpeg.Length).Build();
        var file = (byte[])[.. Build((uint)Build(0).Length), .. jpeg];
        using var output = new MemoryStream();

        await new EmbeddedPreviewDeveloper().DevelopAsync(new MemoryStream(file), output, new RawDevelopOptions(), CancellationToken.None);

        CollectionAssert.AreEqual(jpeg, output.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public async Task Develop_OptionsBeyondThePreview_NotSupported(int which)
    {
        var options = which switch
        {
            1 => new RawDevelopOptions(Format: RawOutputFormat.Png),
            2 => new RawDevelopOptions(BitDepth: 16),
            3 => new RawDevelopOptions(RedScale: 1.2),
            _ => new RawDevelopOptions(BlueScale: 0.8),
        };
        var file = Nef(FakeJpeg(40), FakeJpeg(120));

        await Assert.ThrowsExactlyAsync<NotSupportedException>(() =>
            new EmbeddedPreviewDeveloper().DevelopAsync(new MemoryStream(file), new MemoryStream(), options, CancellationToken.None));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Develop_NoPreview_InvalidData()
    {
        var file = new TiffBuilder().Ascii(271, "NIKON").Build();

        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            new EmbeddedPreviewDeveloper().DevelopAsync(new MemoryStream(file), new MemoryStream(), new RawDevelopOptions(), CancellationToken.None));
        await Assert.ThrowsExactlyAsync<InvalidDataException>(() =>
            new EmbeddedPreviewDeveloper().DevelopAsync(new MemoryStream([1, 2, 3]), new MemoryStream(), new RawDevelopOptions(), CancellationToken.None));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void FindPreview_BadOffsetOrNoJpegMagic_Ignored()
    {
        var outside = new TiffBuilder().Longs(TagJpegOffset, 99999u).Longs(TagJpegLength, 50u).Build();
        var notJpeg = (byte[])[.. new TiffBuilder().Longs(TagJpegOffset, 0u).Longs(TagJpegLength, 8u).Build()];

        Assert.IsNull(EmbeddedPreviewDeveloper.FindPreview(outside));
        Assert.IsNull(EmbeddedPreviewDeveloper.FindPreview(notJpeg));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WhiteBalance_UnitScaleKeepsPixels_OneChannelChangesOnlyThatChannel()
    {
        byte[] rgb = [10, 100, 200, 0, 50, 250];

        CollectionAssert.AreEqual(rgb, WhiteBalance.Scale(rgb, 1, 1, 1));
        CollectionAssert.AreEqual(new byte[] { 10, 100, 255, 0, 50, 255 }, WhiteBalance.Scale(rgb, 1, 1, 2));
        CollectionAssert.AreEqual(new byte[] { 5, 100, 200, 0, 50, 250 }, WhiteBalance.Scale(rgb, 0.5, 1, 1));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WhiteBalance_LengthNotMultipleOfThree_Throws() =>
        Assert.ThrowsExactly<ArgumentException>(() => WhiteBalance.Scale(new byte[4], 1, 1, 1));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void IsRaw_NullStream_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new EmbeddedPreviewDeveloper().IsRaw(null!));
}
