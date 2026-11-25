using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities.Data;

namespace OoBDev.TestUtilities.Tests.Data;

[TestClass]
public class CsvDataAttributeTests
{
    public enum Mode { Fast, Slow }

    private readonly ITempDirectory _temp = new TempFileFactory(null).GetTempDirectory();


    [TestCleanup]
    public void Cleanup() => _temp.Dispose();

    [TestMethod, TestCategory(TestCategories.Unit)]
    [CsvData("Data/sample.csv")]
    public void CsvData_SampleFile_RunsOncePerRow(string note, int count, Mode? mode)
    {
        Assert.IsNotNull(note);
        Assert.IsGreaterThanOrEqualTo(0, count);
        Assert.IsTrue(mode is null || Enum.IsDefined(mode.Value));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetData_SampleFile_MapsColumnsByNameAndConvertsTypes()
    {
        var method = typeof(CsvDataAttributeTests).GetMethod(nameof(Typed));

        var rows = new CsvDataAttribute("Data/sample.csv").GetData(method).ToArray();

        Assert.HasCount(3, rows);
        CollectionAssert.AreEqual(new byte[] { 1, 2 }, (byte[])rows[0][0]);
        Assert.AreEqual(Mode.Fast, rows[0][1]);
        Assert.AreEqual("has \"quotes\", and comma", rows[1][2]);
        Assert.IsNull(rows[2][1]);
        Assert.IsEmpty((byte[])rows[2][0]);
        Assert.AreEqual(0, rows[2][3]);
    }

    public void Typed(byte[] hex, Mode? mode, string note, int count) { }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetDisplayName_RowFromGetData_NamesTheFileAndLine()
    {
        var attribute = new CsvDataAttribute("Data/sample.csv");
        var method = typeof(CsvDataAttributeTests).GetMethod(nameof(Typed));

        var rows = attribute.GetData(method).ToArray();

        Assert.AreEqual("Typed (sample.csv:3)", attribute.GetDisplayName(method, rows[1]));
        Assert.AreEqual("Typed", attribute.GetDisplayName(method, [1]));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetData_BadValueMissingColumnOrFile_FailsWithFileAndLine()
    {
        var method = typeof(CsvDataAttributeTests).GetMethod(nameof(Typed));
        var other = typeof(CsvDataAttributeTests).GetMethod(nameof(Missing));
        var badNumber = typeof(CsvDataAttributeTests).GetMethod(nameof(Typed));
        var bad = TempCsv("count,hex,mode,note\r\n1,00,Fast,a\r\nx,00,Fast,b\r\n");

        var wrongType = Assert.ThrowsExactly<InvalidDataException>(() => new CsvDataAttribute(bad).GetData(badNumber).ToArray());
        var noColumn = Assert.ThrowsExactly<InvalidDataException>(() => new CsvDataAttribute("Data/sample.csv").GetData(other).ToArray());
        var noFile = Assert.ThrowsExactly<FileNotFoundException>(() => new CsvDataAttribute("Data/none.csv").GetData(method).ToArray());

        StringAssert.Contains(wrongType.Message, ":3:");
        StringAssert.Contains(wrongType.Message, "'count'");
        StringAssert.Contains(noColumn.Message, "'absent'");
        StringAssert.Contains(noFile.Message, "none.csv");
    }

    public void Missing(string absent) { }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void GetData_ShortRowOrOpenQuote_FailsWithTheLine()
    {
        var method = typeof(CsvDataAttributeTests).GetMethod(nameof(Typed));
        var shortRow = TempCsv("hex,mode,note,count\r\n00,Fast,a\r\n");
        var openQuote = TempCsv("hex,mode,note,count\r\n00,Fast,\"a,1\r\n");

        var tooShort = Assert.ThrowsExactly<InvalidDataException>(() => new CsvDataAttribute(shortRow).GetData(method).ToArray());
        var unclosed = Assert.ThrowsExactly<InvalidDataException>(() => new CsvDataAttribute(openQuote).GetData(method).ToArray());

        StringAssert.Contains(tooShort.Message, ":2:");
        StringAssert.Contains(unclosed.Message, ":2:");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CsvRows_QuotedLineBreakAndBlankLines_KeepTheStartLine()
    {
        var rows = CsvRows.Read("a,b\n\n\"x\ny\",2\n", "t.csv");

        Assert.HasCount(2, rows);
        Assert.AreEqual(3, rows[1].Line);
        Assert.AreEqual("x\ny", rows[1].Fields[0]);
    }

    private string TempCsv(string content)
    {
        var path = Path.Combine(_temp.DirectoryPath, $"csv_{Guid.NewGuid():N}.csv");
        File.WriteAllText(path, content);
        return path;
    }
}
