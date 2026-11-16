using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
public class ExifLocatorTests
{
    private static byte[] Tiff(bool little = true) => new TiffBuilder(little).Ascii(271, "Canon").Build();

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(true)]
    [DataRow(false)]
    public void Find_PlainTiff_ReturnsTheFile(bool little)
    {
        var tiff = Tiff(little);

        Assert.AreEqual(tiff.Length, ExifLocator.Find(tiff).Length);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_Jpeg_ReturnsTheApp1Block()
    {
        var tiff = Tiff();

        var found = ExifLocator.Find(TiffBuilder.AsJpeg(tiff));

        CollectionAssert.AreEqual(tiff, found.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_Png_ReturnsTheExifChunk()
    {
        var tiff = Tiff();

        CollectionAssert.AreEqual(tiff, ExifLocator.Find(TiffBuilder.AsPng(tiff)).ToArray());
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(false)]
    [DataRow(true)]
    public void Find_WebP_ReturnsTheExifChunkWithOrWithoutHeader(bool withHeader)
    {
        var tiff = Tiff();

        CollectionAssert.AreEqual(tiff, ExifLocator.Find(TiffBuilder.AsWebP(tiff, withHeader)).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_JpegWithoutExif_IsEmpty()
    {
        byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x04, 0x01, 0x02, 0xFF, 0xD9];

        Assert.IsTrue(ExifLocator.Find(jpeg).IsEmpty);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_JpegWhoseSegmentRunsPastTheEnd_IsEmpty()
    {
        byte[] jpeg = [0xFF, 0xD8, 0xFF, 0xE1, 0x7F, 0xFF, (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0];

        Assert.IsTrue(ExifLocator.Find(jpeg).IsEmpty);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_PngWithoutExifOrOversizedChunk_IsEmpty()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0xFF, 0xFF, 0xFF, 0xFF, (byte)'e', (byte)'X', (byte)'I', (byte)'f', 0, 0, 0, 0, 0, 0, 0, 0];

        Assert.IsTrue(ExifLocator.Find(png).IsEmpty);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_PngExifChunkPastTheEnd_IsEmpty()
    {
        byte[] png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0x10, 0, (byte)'e', (byte)'X', (byte)'I', (byte)'f', 0, 0, 0, 0];

        Assert.IsTrue(ExifLocator.Find(png).IsEmpty);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Find_WebPWithoutExifOrTruncated_IsEmpty()
    {
        byte[] noExif = [.. "RIFF"u8.ToArray(), 12, 0, 0, 0, .. "WEBP"u8.ToArray(), .. "VP8 "u8.ToArray(), 0, 0, 0, 0];
        byte[] truncated = [.. "RIFF"u8.ToArray(), 12, 0, 0, 0, .. "WEBP"u8.ToArray(), .. "EXIF"u8.ToArray(), 0xFF, 0, 0, 0];

        Assert.IsTrue(ExifLocator.Find(noExif).IsEmpty);
        Assert.IsTrue(ExifLocator.Find(truncated).IsEmpty);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(new byte[0])]
    [DataRow(new byte[] { 1, 2, 3 })]
    [DataRow(new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61, 0, 0, 0, 0, 0, 0, 0, 0 })]
    public void Find_OtherOrTooShort_IsEmpty(byte[] file)
    {
        Assert.IsTrue(ExifLocator.Find(file).IsEmpty);
    }
}
