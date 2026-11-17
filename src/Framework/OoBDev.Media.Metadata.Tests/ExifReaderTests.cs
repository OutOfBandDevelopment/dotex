using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
public class ExifReaderTests
{
    private static TiffBuilder Gps(string latRef, string lonRef, (uint, uint)[] lat, (uint, uint)[] lon, bool little = true) =>
        new TiffBuilder(little).Ascii(1, latRef).Rationals(2, lat).Ascii(3, lonRef).Rationals(4, lon);

    private static readonly (uint, uint)[] Lat = [(52, 1), (30, 1), (0, 1)];
    private static readonly (uint, uint)[] Lon = [(13, 1), (24, 1), (36, 1)];

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(true)]
    [DataRow(false)]
    public void Read_AllFields_ReturnsThem(bool little)
    {
        var tiff = new TiffBuilder(little)
            .Ascii(271, "Canon\0 ")
            .Ascii(272, "EOS 5D")
            .Shorts(274, 6)
            .Child(0x8769, new TiffBuilder(little).Ascii(36867, "2024:05:06 07:08:09"))
            .Child(0x8825, Gps("N", "E", Lat, Lon, little))
            .Build();

        var fields = ExifReader.Read(TiffBuilder.AsJpeg(tiff));

        Assert.AreEqual("Canon", fields.Make);
        Assert.AreEqual("EOS 5D", fields.Model);
        Assert.AreEqual(6, fields.Orientation);
        Assert.AreEqual(new DateTime(2024, 5, 6, 7, 8, 9), fields.Taken);
        Assert.AreEqual(52.5, fields.Latitude!.Value, 1e-9);
        Assert.AreEqual(13.41, fields.Longitude!.Value, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_SouthAndWest_AreNegative()
    {
        var tiff = new TiffBuilder().Child(0x8825, Gps("S", "W", Lat, Lon)).Build();

        var fields = ExifReader.Read(TiffBuilder.AsPng(tiff));

        Assert.AreEqual(-52.5, fields.Latitude!.Value, 1e-9);
        Assert.AreEqual(-13.41, fields.Longitude!.Value, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_OutOfRangeOrNullIslandGps_IsDropped()
    {
        var tooHigh = new TiffBuilder().Child(0x8825, Gps("N", "E", [(95, 1), (0, 1), (0, 1)], Lon)).Build();
        var nullIsland = new TiffBuilder().Child(0x8825, Gps("N", "E", [(0, 1), (0, 1), (0, 1)], [(0, 1), (0, 1), (0, 1)])).Build();

        Assert.IsNull(ExifReader.Read(tooHigh).Latitude);
        Assert.IsNull(ExifReader.Read(nullIsland).Longitude);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_ZeroDenominator_CountsAsZero()
    {
        var tiff = new TiffBuilder().Child(0x8825, Gps("N", "E", [(52, 0), (30, 1), (0, 1)], Lon)).Build();

        Assert.AreEqual(0.5, ExifReader.Read(tiff).Latitude!.Value, 1e-9);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_GpsWithWrongTypeOrFewValues_IsIgnored()
    {
        var wrongType = new TiffBuilder().Child(0x8825, new TiffBuilder().Shorts(2, 1, 2, 3).Shorts(4, 1, 2, 3)).Build();
        var fewValues = new TiffBuilder().Child(0x8825, new TiffBuilder().Rationals(2, (1, 1)).Rationals(4, (1, 1))).Build();

        Assert.IsNull(ExifReader.Read(wrongType).Latitude);
        Assert.IsNull(ExifReader.Read(fewValues).Latitude);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_NoOriginalDate_FallsBackToDateTime()
    {
        var tiff = new TiffBuilder().Ascii(306, "2020:01:02 03:04:05").Build();

        Assert.AreEqual(new DateTime(2020, 1, 2, 3, 4, 5), ExifReader.Read(tiff).Taken);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_LongMakeAndEmptyModel_AreCutAndNull()
    {
        var tiff = new TiffBuilder().Ascii(271, new string('x', 150)).Ascii(272, "  ").Build();

        var fields = ExifReader.Read(tiff);

        Assert.AreEqual(100, fields.Make!.Length);
        Assert.AreEqual("", fields.Model);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_OrientationOfWrongType_IsNull()
    {
        var tiff = new TiffBuilder().Ascii(274, "6").Build();

        Assert.IsNull(ExifReader.Read(tiff).Orientation);
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(null, null)]
    [DataRow("", null)]
    [DataRow("  ", null)]
    [DataRow("not a date", null)]
    [DataRow("2024:13:40 00:00:00", null)]
    public void ParseTaken_Invalid_IsNull(string? raw, object? expected)
    {
        Assert.AreEqual(expected, ExifReader.ParseTaken(raw));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ParseTaken_ExtraTrailingText_IsIgnored()
    {
        Assert.AreEqual(new DateTime(2024, 1, 2, 3, 4, 5), ExifReader.ParseTaken(" 2024:01:02 03:04:05.123+01:00"));
    }

    [DataTestMethod, TestCategory(TestCategories.Unit)]
    [DataRow(new byte[0])]
    [DataRow(new byte[] { 0xFF, 0xD8, 0xFF, 0xD9, 0, 0 })]
    public void Read_NoExif_IsEmpty(byte[] file)
    {
        Assert.AreEqual(ExifFields.Empty, ExifReader.Read(file));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_TiffBlockWithoutByteOrder_IsEmpty()
    {
        var bad = new byte[] { 0xFF, 0xD8, 0xFF, 0xE1, 0, 0x14, (byte)'E', (byte)'x', (byte)'i', (byte)'f', 0, 0, (byte)'X', (byte)'X', 42, 0, 8, 0, 0, 0, 0, 0 };

        Assert.AreEqual(ExifFields.Empty, ExifReader.Read(bad));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_TruncatedAtEveryLength_NeverThrows()
    {
        var tiff = new TiffBuilder()
            .Ascii(271, "Canon").Shorts(274, 3)
            .Child(0x8769, new TiffBuilder().Ascii(36867, "2024:05:06 07:08:09"))
            .Child(0x8825, Gps("N", "E", Lat, Lon))
            .Build();
        var file = TiffBuilder.AsJpeg(tiff);

        for (var length = 0; length <= file.Length; length++)
        {
            var part = file.AsSpan(0, length);
            ExifReader.Read(part);
            ExifDump.Read(part);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_PointerOutsideTheBlock_IsEmptyOrPartial()
    {
        var tiff = new TiffBuilder().Ascii(271, "Canon").Longs(0x8769, 0x7FFFFFF0).Build();

        Assert.AreEqual("Canon", ExifReader.Read(tiff).Make);
    }
}
