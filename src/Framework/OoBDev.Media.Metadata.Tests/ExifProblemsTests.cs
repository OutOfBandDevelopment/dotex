using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class ExifProblemsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void ReadWithProblems_SoundFile_HasNoProblems()
    {
        var file = TiffBuilder.AsJpeg(new TiffBuilder().Ascii(271, "Canon").Shorts(274, 6).Build());

        var result = ExifReader.ReadWithProblems(file);

        Assert.AreEqual("Canon", result.Fields.Make);
        Assert.AreEqual(0, result.Problems.Count);
    }

    [TestMethod]
    public void ReadWithProblems_NoExifBlock_HasNoProblems()
    {
        var result = ExifReader.ReadWithProblems(new byte[20]);

        Assert.AreEqual(ExifFields.Empty, result.Fields);
        Assert.AreEqual(0, result.Problems.Count);
    }

    [TestMethod]
    public void ReadWithProblems_WrongTypesAndBadDate_ReportsEachAndKeepsTheValidOnes()
    {
        var tiff = new TiffBuilder().Shorts(271, 5).Ascii(272, "EOS").Shorts(274, 1, 2).Ascii(306, "not a date").Build();

        var result = ExifReader.ReadWithProblems(tiff);

        foreach (var p in result.Problems)
            TestContext.WriteLine(p);
        Assert.AreEqual("EOS", result.Fields.Model);
        Assert.IsNull(result.Fields.Make);
        Assert.AreEqual(1, result.Fields.Orientation);
        Assert.AreEqual(2, result.Problems.Count);
        Assert.IsTrue(result.Problems[0].Contains("Tag 271", global::System.StringComparison.Ordinal));
        Assert.IsTrue(result.Problems[1].Contains("not a date", global::System.StringComparison.Ordinal));
    }

    [TestMethod]
    public void ReadWithProblems_GpsOutOfRange_IsReported()
    {
        var gps = new TiffBuilder().Ascii(1, "N").Rationals(2, (95, 1), (0, 1), (0, 1)).Ascii(3, "E").Rationals(4, (10, 1), (0, 1), (0, 1));
        var tiff = new TiffBuilder().Child(0x8825, gps).Build();

        var result = ExifReader.ReadWithProblems(tiff);

        Assert.IsNull(result.Fields.Latitude);
        Assert.AreEqual(1, result.Problems.Count);
    }

    [TestMethod]
    public void ReadWithProblems_TruncatedDirectory_CountsCutOffEntries()
    {
        var tiff = new TiffBuilder().Ascii(271, "Canon").Ascii(272, "EOS").Build();
        var cut = tiff[..(tiff.Length - 16)];

        var result = ExifReader.ReadWithProblems(cut);

        TestContext.WriteLine(string.Join(" | ", result.Problems));
        Assert.IsTrue(result.Problems.Count >= 1);
    }
}
