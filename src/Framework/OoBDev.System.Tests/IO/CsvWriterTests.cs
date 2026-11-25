using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class CsvWriterTests
{
    private sealed record Row(int Col_One, string? Text);

    private static string Run(Action<Stream> write)
    {
        using var stream = new MemoryStream();
        write(stream);
        Assert.IsTrue(stream.CanWrite, "the stream must stay open");
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_WithHeader_QuotesEveryField()
    {
        var rows = Enumerable.Range(0, 3).Select(r => new { Col0 = r, Col1 = r * 2 });
        var result = Run(s => rows.WriteAsCsvTo(s));
        Assert.AreEqual("\"Col0\",\"Col1\"\r\n\"0\",\"0\"\r\n\"1\",\"2\"\r\n\"2\",\"4\"\r\n".Replace("\r\n", Environment.NewLine), result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_NoHeader_SkipsHeaderLine()
    {
        var result = Run(s => new[] { new Row(1, "a") }.WriteAsCsvTo(s, includeHeader: false));
        Assert.AreEqual("\"1\",\"a\"" + Environment.NewLine, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_UnderscoreInName_BecomesSpaceInHeader()
    {
        var result = Run(s => Array.Empty<Row>().WriteAsCsvTo(s));
        Assert.AreEqual("\"Col One\",\"Text\"" + Environment.NewLine, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_QuotesAndNulls_AreEscaped()
    {
        var result = Run(s => new[] { new Row(2, "say \"hi\", ok"), new Row(3, null) }.WriteAsCsvTo(s, includeHeader: false));
        Assert.AreEqual("\"2\",\"say \"\"hi\"\", ok\"" + Environment.NewLine + "\"3\",\"\"" + Environment.NewLine, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_NullItems_AreSkipped()
    {
        var result = Run(s => new Row?[] { null, new Row(1, "x") }.WriteAsCsvTo(s, includeHeader: false));
        Assert.AreEqual("\"1\",\"x\"" + Environment.NewLine, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_Decimal_UsesInvariantCulture()
    {
        var result = Run(s => new[] { new { Value = 1.5m } }.WriteAsCsvTo(s, includeHeader: false));
        Assert.AreEqual("\"1.5\"" + Environment.NewLine, result);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task WriteAsCsvToAsync_MatchesSync()
    {
        var rows = Enumerable.Range(0, 3).Select(r => new { Col0 = r, Col1 = r * 2 });
        var sync = Run(s => rows.WriteAsCsvTo(s));
        using var stream = new MemoryStream();
        await rows.WriteAsCsvToAsync(stream);
        Assert.IsTrue(stream.CanWrite);
        Assert.AreEqual(sync, Encoding.UTF8.GetString(stream.ToArray()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task WriteAsCsvToAsync_NoHeader_SkipsHeaderLine()
    {
        using var stream = new MemoryStream();
        await new[] { new Row(1, "a") }.WriteAsCsvToAsync(stream, includeHeader: false);
        Assert.AreEqual("\"1\",\"a\"" + Environment.NewLine, Encoding.UTF8.GetString(stream.ToArray()));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task WriteAsCsvToAsync_Cancelled_Throws()
    {
        using var stream = new MemoryStream();
        using var cts = new global::System.Threading.CancellationTokenSource();
        cts.Cancel();
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => new[] { new Row(1, "a") }.WriteAsCsvToAsync(stream, cancellationToken: cts.Token));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void WriteAsCsvTo_NullArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => ((Row[])null!).WriteAsCsvTo(new MemoryStream()));
        Assert.ThrowsExactly<ArgumentNullException>(() => new[] { new Row(1, "a") }.WriteAsCsvTo(null!));
    }
}
