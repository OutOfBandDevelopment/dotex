using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Ocr;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Tests;

[TestClass]
public class OcrMarkdownTests
{
    private string dir = "";

    [TestInitialize]
    public void Init()
    {
        dir = Path.Combine(Path.GetTempPath(), "ocrtest-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
    }

    [TestCleanup]
    public void Clean() => Directory.Delete(dir, true);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Write_SplitsPagesAndDropsTrailingEmpty()
    {
        var pdf = Path.Combine(dir, "scan.pdf");
        File.WriteAllText(pdf, "x");
        var md = Path.Combine(dir, "o", "scan.md");

        OcrMarkdown.Write(pdf, md, "hash", $" first {IDocumentOcr.PageMark} second {IDocumentOcr.PageMark}  ", "fake");

        var text = File.ReadAllText(md);
        StringAssert.Contains(text, "kind: pdf-ocr");
        StringAssert.Contains(text, "pages: 2");
        StringAssert.Contains(text, "## Page 2\n\nsecond");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CopyFrom_OcrDonor_RewritesSource()
    {
        var a = Path.Combine(dir, "a.pdf");
        var b = Path.Combine(dir, "b.pdf");
        File.WriteAllText(a, "x");
        File.WriteAllText(b, "x");
        var donor = Path.Combine(dir, "a.md");
        OcrMarkdown.Write(a, donor, "h", "text", "fake");
        var copy = Path.Combine(dir, "c", "b.md");

        Assert.IsTrue(OcrMarkdown.CopyFrom(donor, copy, b));

        var bPath = b.Replace('\\', '/');
        StringAssert.Contains(File.ReadAllText(copy), $"source: \"{bPath}\"");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void CopyFrom_MissingOrNonOcrDonor_ReturnsFalse()
    {
        var plain = Path.Combine(dir, "p.md");
        File.WriteAllText(plain, "---\nkind: pdf\n---\n");

        Assert.IsFalse(OcrMarkdown.CopyFrom(Path.Combine(dir, "none.md"), Path.Combine(dir, "x.md"), "f.pdf"));
        Assert.IsFalse(OcrMarkdown.CopyFrom(plain, Path.Combine(dir, "x.md"), "f.pdf"));
    }
}
