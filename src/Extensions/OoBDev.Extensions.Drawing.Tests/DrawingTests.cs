using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

namespace OoBDev.Extensions.Drawing.Tests;

[TestClass]
public class Code39Tests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToPattern_Letter_AddsStartAndStopMarks()
    {
        var pattern = Code39.ToPattern("A");

        CollectionAssert.AreEqual(new byte[] { 0xBA, 0x22, 0x8A, 0xE8, 0xBA, 0x22 }, pattern);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToPattern_AlreadyMarked_DoesNotDuplicateMarks() =>
        CollectionAssert.AreEqual(Code39.ToPattern("A"), Code39.ToPattern("*A*"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("")]
    [DataRow("abc")]
    [DataRow("A_B")]
    public void ToPattern_InvalidCharacters_Throws(string message) =>
        Assert.ThrowsExactly<InvalidOperationException>(() => Code39.ToPattern(message));

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow("Hello World!", "H+E+L+L+O W+O+R+L+D/A")]
    [DataRow("*ABC*", "ABC")]
    [DataRow("\0\t", "%U$I")]
    [DataRow("a@`{~", "+A%V%W%P%S")]
    [DataRow(":;/-.", "/Z%F/O-.")]
    [DataRow("[\\]^_\u007f", "%K%L%M%N%O%T")]
    [DataRow("\u001b\u001f!,", "%A%E/A/L")]
    public void ToStandard_FullAscii_UsesEscapes(string message, string expected) =>
        Assert.AreEqual(expected, Code39.ToStandard(message));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToStandard_AboveAscii_Throws() =>
        Assert.ThrowsExactly<InvalidOperationException>(() => Code39.ToStandard("é"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EncodeStandard_Message_ProducesOneBitBitmapOfExpectedSize()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");

        using var image = Code39.EncodeStandard("HELLO");

        Assert.AreEqual(7 * 16, image.Width);
        Assert.AreEqual(Code39.BarHeight, image.Height);
        Assert.AreEqual(PixelFormat.Format1bppIndexed, image.PixelFormat);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void EncodeFullAscii_Message_EncodesEscapedText()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");

        using var image = Code39.EncodeFullAscii("a");

        Assert.AreEqual(4 * 16, image.Width);
    }
}

[TestClass]
public class ZOrderCurveTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(0, 0, 0u)]
    [DataRow(1, 0, 1u)]
    [DataRow(0, 1, 2u)]
    [DataRow(1, 1, 3u)]
    [DataRow(2, 0, 4u)]
    [DataRow(3, 3, 15u)]
    [DataRow(65535, 65535, uint.MaxValue)]
    public void GetValue_Point_InterleavesBits(int x, int y, uint expected) =>
        Assert.AreEqual(expected, new Point(x, y).GetValue());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetPoint_RoundTrip_ReturnsOriginal()
    {
        foreach (var point in new[] { new Point(0, 0), new Point(12345, 54321), new Point(65535, 0), new Point(1, 65535) })
            Assert.AreEqual(point, point.GetValue().GetPoint());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ExpandThenReduce_Value_ReturnsLow16Bits() =>
        Assert.AreEqual(0xBEEFu, 0x1BEEFu.Expand().Reduce());
}

[TestClass]
public class SizeExtensionsTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(1, 1, 0)]
    [DataRow(256, 100, 8)]
    [DataRow(257, 100, 9)]
    [DataRow(100, 4000, 12)]
    public void MaxLevel_Size_IsCeilingLog2OfLongerSide(int width, int height, int expected) =>
        Assert.AreEqual(expected, new Size(width, height).MaxLevel());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Scale_TinyFactor_NeverBelowOnePixel() =>
        Assert.AreEqual(new Size(1, 1), new Size(10, 20).Scale(0.0001));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Scale_Factor_RoundsUp() =>
        Assert.AreEqual(new Size(5, 8), new Size(9, 15).Scale(0.5));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void OffsetBy_CellSize_MultipliesPosition() =>
        Assert.AreEqual(new Point(30, 80), new Point(3, 4).OffsetBy(new Size(10, 20)));
}

[TestClass]
public class JpegSegmentReaderTests
{
    private static readonly byte[] _sample =
    [
        0xFF, 0xD8,
        0xFF, 0xE1, 0x00, 0x05, 0x01, 0x02, 0x03,
        0xFF, 0xDA, 0x00, 0x03, 0x09, 0x11, 0xFF, 0x00, 0x22, 0xFF, 0xD0, 0x33,
        0xFF, 0xD9,
    ];

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_Stream_SplitsMarkersAndPayloads()
    {
        using var stream = new MemoryStream(_sample);

        var segments = JpegSegmentReader.GetSegments(stream).ToList();

        CollectionAssert.AreEqual(new byte[] { 0xD8, 0xE1, 0xDA, 0xD9 }, segments.Select(s => s.Type).ToArray());
        CollectionAssert.AreEqual(new[] { 0, 1, 2, 3 }, segments.Select(s => s.Index).ToArray());
        CollectionAssert.AreEqual(new byte[] { 1, 2, 3 }, segments[1].Data);
        Assert.AreEqual(3, segments[1].Length);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_Scan_KeepsStuffedBytesAndRestartMarkers()
    {
        using var stream = new MemoryStream(_sample);

        var scan = JpegSegmentReader.GetSegments(stream).Single(s => s.Type == 0xDA);

        CollectionAssert.AreEqual(new byte[] { 0x09, 0x11, 0xFF, 0x00, 0x22, 0xFF, 0xD0, 0x33 }, scan.Data);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_File_ReadsFromPath()
    {
        var path = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(path, _sample);

            Assert.AreEqual(4, JpegSegmentReader.GetSegments(path).Count());
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_NotAMarker_Throws() =>
        Assert.ThrowsExactly<InvalidDataException>(() => JpegSegmentReader.GetSegments(new MemoryStream([0x00, 0x01])).ToList());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_TruncatedSegment_Throws() =>
        Assert.ThrowsExactly<EndOfStreamException>(() => JpegSegmentReader.GetSegments(new MemoryStream([0xFF, 0xE1, 0x00, 0x09, 0x01])).ToList());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_BadLength_Throws() =>
        Assert.ThrowsExactly<InvalidDataException>(() => JpegSegmentReader.GetSegments(new MemoryStream([0xFF, 0xE1, 0x00, 0x01])).ToList());

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetSegments_EmptyStream_ReturnsNothing() =>
        Assert.AreEqual(0, JpegSegmentReader.GetSegments(new MemoryStream()).Count());
}

[TestClass]
public class PngPackTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(3)]
    [DataRow(4)]
    [DataRow(1000)]
    public void Pack_Bytes_RoundTrips(int length)
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        var input = Enumerable.Range(0, length).Select(i => (byte)(i * 7 + 3)).ToArray();

        var png = PngPack.Pack(input);
        var output = PngPack.Unpack(png);

        CollectionAssert.AreEqual(input, output);
        CollectionAssert.AreEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, png.Take(4).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Unpack_OrdinaryImage_ThrowsInvalidData()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var bitmap = new Bitmap(2, 2, PixelFormat.Format32bppArgb);
        bitmap.SetPixel(0, 0, Color.FromArgb(255, 255, 255, 255));
        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);

        Assert.ThrowsExactly<InvalidDataException>(() => PngPack.Unpack(stream.ToArray()));
    }
}

[TestClass]
public class MultiScaleTileTests
{
    private static Bitmap CreateImage(int width, int height)
    {
        var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.CornflowerBlue);
        return bitmap;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetMaxLevel_Image_UsesLongerSide()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var image = CreateImage(600, 300);

        Assert.AreEqual(10, image.GetMaxLevel());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(10, 3, 2)]
    [DataRow(9, 2, 1)]
    [DataRow(8, 1, 1)]
    [DataRow(0, 1, 1)]
    public void GetTileCount_Level_CoversScaledImage(int level, int columns, int rows)
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var image = CreateImage(600, 300);

        Assert.AreEqual(new Size(columns, rows), image.GetTileCount(level));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTile_TopLevel_ReturnsTileSizedBlock()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var image = CreateImage(600, 300);

        using var tile = image.GetTile(10, 1, 0);

        Assert.AreEqual(new Size(256, 256), tile.Size);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTile_LowLevel_ReturnsWholeScaledImage()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var image = CreateImage(600, 300);

        using var tile = image.GetTile(7, 0, 0);

        Assert.AreEqual(new Size(75, 38), tile.Size);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTileAsBytes_Tile_IsJpeg()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var image = CreateImage(600, 300);

        var bytes = image.GetTileAsBytes(10, 0, 0);

        CollectionAssert.AreEqual(new byte[] { 0xFF, 0xD8 }, bytes.Take(2).ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetTileAsStream_Tile_StartsAtBeginning()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        using var image = CreateImage(300, 300);

        using var stream = image.GetTileAsStream(9, 0, 0);

        Assert.AreEqual(0, stream.Position);
        Assert.IsTrue(stream.Length > 0);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void StreamAndPathOverloads_Image_MatchImageOverloads()
    {
        if (!OperatingSystem.IsWindows()) Assert.Inconclusive("System.Drawing needs Windows.");
        var path = Path.ChangeExtension(Path.GetTempFileName(), ".png");
        try
        {
            using (var image = CreateImage(600, 300))
                image.Save(path, ImageFormat.Png);

            using var stream = File.OpenRead(path);
            Assert.AreEqual(10, path.GetMaxLevel());
            Assert.AreEqual(new Size(3, 2), path.GetTileCount(10));
            Assert.AreEqual(10, stream.GetMaxLevel());
            stream.Position = 0;
            Assert.AreEqual(new Size(3, 2), stream.GetTileCount(10));
            stream.Position = 0;
            Assert.IsTrue(stream.GetTileAsBytes(10, 0, 0).Length > 0);
            Assert.IsTrue(path.GetTileAsBytes(10, 0, 0).Length > 0);
        }
        finally
        {
            File.Delete(path);
        }
    }
}
