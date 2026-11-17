using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
public class ExifDumpTests
{
    private static IReadOnlyDictionary<string, string> Dump(TiffBuilder tiff) => ExifDump.Read(TiffBuilder.AsJpeg(tiff.Build()));

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(true)]
    [DataRow(false)]
    public void Read_AllDirectories_AreNamedAndPrefixed(bool little)
    {
        var tiff = new TiffBuilder(little)
            .Ascii(271, "Canon")
            .Shorts(274, 1)
            .Child(0x8769, new TiffBuilder(little).Ascii(36867, "2024:05:06 07:08:09").Child(0xA005, new TiffBuilder(little).Ascii(1, "R98")))
            .Child(0x8825, new TiffBuilder(little).Ascii(1, "N").Rationals(2, (52, 1), (30, 1), (0, 1)));

        var dump = Dump(tiff);

        Assert.AreEqual("Canon", dump["Make"]);
        Assert.AreEqual("1", dump["Orientation"]);
        Assert.AreEqual("2024:05:06 07:08:09", dump["DateTimeOriginal"]);
        Assert.AreEqual("R98", dump["Interop.InteropIndex"]);
        Assert.AreEqual("N", dump["GPS.GPSLatitudeRef"]);
        Assert.AreEqual("(52.0, 30.0, 0.0)", dump["GPS.GPSLatitude"]);
        Assert.IsFalse(dump.Keys.Any(k => k.Contains("ExifOffset", StringComparison.Ordinal)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_NumericTypes_PrintLikePython()
    {
        var tiff = new TiffBuilder()
            .Shorts(0x0100, 5)
            .Longs(0x0101, 4000000000)
            .SignedShorts(0x0102, -2, 3)
            .SignedLongs(0x0103, -70000)
            .Floats(0x0106, 1.5f)
            .Doubles(0x0107, 2.0)
            .Rationals(0x0108, (1, 3))
            .SignedRationals(0x0109, (-1, 2))
            .Rationals(0x010A, (1, 0))
            .Bytes(0x010D, [0xFE, 1], type: 6);

        var dump = Dump(tiff);

        Assert.AreEqual("5", dump["ImageWidth"]);
        Assert.AreEqual("4000000000", dump["ImageLength"]);
        Assert.AreEqual("(-2, 3)", dump["BitsPerSample"]);
        Assert.AreEqual("-70000", dump["Compression"]);
        Assert.AreEqual("1.5", dump["PhotometricInterpretation"]);
        Assert.AreEqual("2.0", dump["Thresholding"]);
        Assert.AreEqual("0.3333333333333333", dump["CellWidth"]);
        Assert.AreEqual("-0.5", dump["CellLength"]);
        Assert.AreEqual("nan", dump["FillOrder"]);
        Assert.AreEqual("(-2, 1)", dump["DocumentName"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_Limits_ApplyToBytesValuesAndText()
    {
        var tiff = new TiffBuilder()
            .Bytes(0x8773, new byte[65], type: 7)
            .Shorts(0x0111, Enumerable.Range(0, 25).Select(i => (ushort)i).ToArray())
            .Shorts(0x0112, Enumerable.Range(0, 3).Select(i => (ushort)i).ToArray())
            .Ascii(0x010E, new string('a', 400))
            .Ascii(0x010F, "  ")
            .Bytes(0x0113, "ab"u8.ToArray(), type: 1);

        var dump = Dump(tiff);

        Assert.AreEqual("<65 bytes>", dump["InterColorProfile"]);
        Assert.AreEqual("<25 values>", dump["StripOffsets"]);
        Assert.AreEqual("(0, 1, 2)", dump["Orientation"]);
        Assert.AreEqual(300, dump["ImageDescription"].Length);
        Assert.IsFalse(dump.ContainsKey("Make"));
        Assert.AreEqual("ab", dump["275"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_UnknownTag_UsesTheNumber()
    {
        var dump = Dump(new TiffBuilder().Shorts(0x7777, 9));

        Assert.AreEqual("9", dump["30583"]);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_NoExifOrBadBlock_IsEmpty()
    {
        Assert.AreEqual(0, ExifDump.Read(new byte[] { 1, 2, 3 }).Count);
        Assert.AreEqual(0, ExifDump.Read(new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0, 0x14, (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0, (byte)'X', (byte)'X', 42, 0, 8, 0, 0, 0, 0, 0 }).Count);
    }
}
