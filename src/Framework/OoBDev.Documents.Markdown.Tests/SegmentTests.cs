using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Ocr;
using OoBDev.Documents.Pdf;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Tests;

[TestClass]
public class SegmentTests
{
    private static string Show(IReadOnlyList<PageRange> r) => string.Join(",", r.Select(x => $"{x.From}-{x.To}"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Plan_ShortDocument_SingleRange() =>
        Assert.AreEqual("1-30", Show(new PdfSegmenter(new FakePdfTools(new PdfOutline(30, []))).Plan("f", 50)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Plan_NoSplitting_SingleRange() =>
        Assert.AreEqual("1-300", Show(new PdfSegmenter(new FakePdfTools(new PdfOutline(300, []))).Plan("f", 0)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Plan_Unreadable_WholeFile()
    {
        Assert.AreEqual("1-0", Show(new PdfSegmenter(new FakePdfTools(null)).Plan("f", 50)));
        Assert.AreEqual("1-0", Show(new PdfSegmenter(new FakePdfTools(null, throws: true)).Plan("f", 50)));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Plan_NoOutline_CutsEveryMaxPages() =>
        Assert.AreEqual("1-40,41-80,81-100", Show(new PdfSegmenter(new FakePdfTools(new PdfOutline(100, []))).Plan("f", 40)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Plan_ChaptersPackedAndOversizedChapterCut() =>
        Assert.AreEqual("1-30,31-60,61-90,91-120", Show(new PdfSegmenter(new FakePdfTools(new PdfOutline(120, [1, 10, 20, 31, 61]))).Plan("f", 30)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Pack_ChaptersJoinUntilFull() =>
        Assert.AreEqual("1-25,26-40", Show(PdfSegmenter.Pack([1, 11, 26], 40, 25)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Pack_OversizedChapterFlushesOpenRange() =>
        Assert.AreEqual("1-4,5-14,15-24,25-30", Show(PdfSegmenter.Pack([1, 5, 25], 30, 10)));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Recognize_ShortDocument_SendsWholeFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "seg-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "d.pdf");
            File.WriteAllText(file, "whole");
            var ocr = new FakeOcr(t => t);

            var text = await new SegmentedOcr(ocr, new FakePdfTools(new PdfOutline(3, []))).RecognizeAsync(file, Path.Combine(dir, "w"), 50, CancellationToken.None);

            Assert.AreEqual("whole", text);
            CollectionAssert.AreEqual(new[] { "d.pdf" }, ocr.Calls);
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Recognize_Pieces_KeepPageNumbersTrue()
    {
        var dir = Path.Combine(Path.GetTempPath(), "seg-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "d.pdf");
            File.WriteAllText(file, "x");
            var tools = new FakePdfTools(new PdfOutline(5, []));
            // 1-3 answers too many pages (4), 4-5 too few (1)
            var ocr = new FakeOcr(t => t == "1-3" ? $"a{IDocumentOcr.PageMark}b{IDocumentOcr.PageMark}c{IDocumentOcr.PageMark}d" : $"e{IDocumentOcr.PageMark}");

            var work = Path.Combine(dir, "w");
            var text = await new SegmentedOcr(ocr, tools).RecognizeAsync(file, work, 3, CancellationToken.None);

            Assert.AreEqual(string.Join(IDocumentOcr.PageMark, "a", "b", "c\n\nd", "e", ""), text);
            Assert.IsFalse(Directory.Exists(work));
        }
        finally { Directory.Delete(dir, true); }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Recognize_AfterFailure_ResumesFinishedPieces()
    {
        var dir = Path.Combine(Path.GetTempPath(), "seg-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
        try
        {
            var file = Path.Combine(dir, "d.pdf");
            File.WriteAllText(file, "x");
            var work = Path.Combine(dir, "w");
            var tools = new FakePdfTools(new PdfOutline(4, []));
            var fail = true;
            var ocr = new FakeOcr(t => t == "3-4" && fail ? throw new TimeoutException("slow") : $"p{IDocumentOcr.PageMark}q");
            var segmented = new SegmentedOcr(ocr, tools);

            await Assert.ThrowsExactlyAsync<TimeoutException>(() => segmented.RecognizeAsync(file, work, 2, CancellationToken.None));
            fail = false;
            var text = await segmented.RecognizeAsync(file, work, 2, CancellationToken.None);

            Assert.AreEqual(string.Join(IDocumentOcr.PageMark, "p", "q", "p", "q"), text);
            CollectionAssert.AreEqual(new[] { (1, 2), (3, 4), (3, 4) }, tools.Written);
        }
        finally { Directory.Delete(dir, true); }
    }
}
