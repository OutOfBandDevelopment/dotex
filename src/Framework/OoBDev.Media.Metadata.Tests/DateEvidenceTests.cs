using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Media.Metadata.Dates;
using OoBDev.TestUtilities;
using System;

namespace OoBDev.Media.Metadata.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
public class DateEvidenceTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void Parse_FileNameWithFullDate_DayPrecisionWithFileBonus()
    {
        var (date, place) = PathDateParser.Parse("D:/Photos/Trips/Lake_House_20190504_101010.jpg", 2026);

        TestContext.WriteLine($"{date} | {place}");
        Assert.IsNotNull(date);
        Assert.AreEqual(new DateOnly(2019, 5, 4), date.Date);
        Assert.AreEqual(DatePrecision.Day, date.Precision);
        Assert.AreEqual(0.80, date.Confidence);
        Assert.AreEqual("'20190504' in file name (ymd)", date.Detail);
        Assert.AreEqual("Lake House", place?.Text);
    }

    [TestMethod]
    public void Parse_FolderYearAndMonth_UsesTheMostSpecific()
    {
        var (date, _) = PathDateParser.Parse("/pics/2018/2018-07 Italy/IMG_0001.JPG", 2026);

        Assert.IsNotNull(date);
        Assert.AreEqual(new DateOnly(2018, 7, 1), date.Date);
        Assert.AreEqual(DatePrecision.Month, date.Precision);
        Assert.AreEqual(0.60, date.Confidence);
    }

    [TestMethod]
    public void Parse_UsDateAndTieGoesToTheDeeperFolder()
    {
        var (date, _) = PathDateParser.Parse("/a/2015/06152016/b.jpg", 2026);

        Assert.AreEqual(new DateOnly(2016, 6, 15), date!.Date);
    }

    [TestMethod]
    public void Parse_NoDateOrImplausibleYear_ReturnsNothing()
    {
        Assert.IsNull(PathDateParser.Parse("/a/b/c.jpg", 2026).Date);
        Assert.IsNull(PathDateParser.Parse("/a/20990101/c.jpg", 2026).Date);
        Assert.IsNull(PathDateParser.Parse("/a/20190231/c.jpg", 2026).Date);
    }

    [TestMethod]
    public void Parse_ArchivePath_ReadsBothSides()
    {
        var (date, _) = PathDateParser.Parse("/x/backup.zip!/2012/Summer Camp/p.jpg", 2026);

        Assert.AreEqual(2012, date!.Date.Year);
    }

    [TestMethod]
    public void Words_GenericFolder_IsDropped()
    {
        Assert.IsNull(PathDateParser.Words("Photos", null));
        Assert.AreEqual("Summer Camp", PathDateParser.Words("SummerCamp_2012", "2012"));
    }

    [TestMethod]
    public void Combine_AgreeingSources_RaiseConfidence()
    {
        var best = DateCombiner.Combine(
        [
            new("fs", new DateOnly(2019, 5, 4), DatePrecision.Day, DateCombiner.FileTimeConfidence),
            new("exif", new DateOnly(2019, 5, 4), DatePrecision.Day, DateCombiner.ExifConfidence),
            new("path", new DateOnly(2019, 5, 1), DatePrecision.Month, 0.60),
        ]);

        TestContext.WriteLine(best.ToString());
        Assert.AreEqual("exif", best.Source);
        Assert.AreEqual(2, best.Agreeing);
        Assert.AreEqual("exif,fs,path", best.Sources);
        Assert.AreEqual(0.99, best.Confidence);
        Assert.IsFalse(best.Conflict);
    }

    [TestMethod]
    public void Combine_TwoConfidentSourcesFarApart_FlagsConflict()
    {
        var best = DateCombiner.Combine(
        [
            new("exif", new DateOnly(2019, 5, 4), DatePrecision.Day, 0.95),
            new("path", new DateOnly(2012, 1, 1), DatePrecision.Year, 0.45),
        ]);

        Assert.IsTrue(best.Conflict);
        Assert.AreEqual(0.95, best.Confidence);
    }

    [TestMethod]
    public void Combine_TieOnConfidence_FinerPrecisionWins()
    {
        var best = DateCombiner.Combine(
        [
            new("a", new DateOnly(2019, 1, 1), DatePrecision.Year, 0.5),
            new("b", new DateOnly(2019, 3, 4), DatePrecision.Day, 0.5),
        ]);

        Assert.AreEqual("b", best.Source);
    }

    [TestMethod]
    public void Combine_NoEvidence_Throws()
        => Assert.ThrowsExactly<ArgumentException>(() => DateCombiner.Combine([]));

    [TestMethod]
    [DataRow("2019-05-04", DatePrecision.Day, "2019-05-05", DatePrecision.Day, true)]
    [DataRow("2019-05-04", DatePrecision.Day, "2019-05-06", DatePrecision.Day, false)]
    [DataRow("2019-05-04", DatePrecision.Day, "2019-05-30", DatePrecision.Month, true)]
    [DataRow("2019-05-04", DatePrecision.Day, "2019-12-30", DatePrecision.Year, true)]
    [DataRow("2019-05-04", DatePrecision.Day, "2020-05-04", DatePrecision.Year, false)]
    public void Agree_UsesTheCoarserPrecision(string a, DatePrecision pa, string b, DatePrecision pb, bool expected)
        => Assert.AreEqual(expected, DateCombiner.Agree(DateOnly.Parse(a, global::System.Globalization.CultureInfo.InvariantCulture), pa, DateOnly.Parse(b, global::System.Globalization.CultureInfo.InvariantCulture), pb));
}
