using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Pdf;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Tests;

[TestClass]
public class ConverterTests
{
    private string dir = "";

    [TestInitialize]
    public void Init()
    {
        dir = Path.Combine(Path.GetTempPath(), "mdtest-" + Path.GetRandomFileName());
        Directory.CreateDirectory(dir);
    }

    [TestCleanup]
    public void Clean() => Directory.Delete(dir, true);

    private string Write(string name, string content)
    {
        var path = Path.Combine(dir, name);
        File.WriteAllText(path, content);
        return path;
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_Pdf_WritesAnchorsAndFrontMatter()
    {
        var pdf = Write("manual.pdf", "%PDF-1.4 body");
        var md = Path.Combine(dir, "out", "manual.md");
        var page = new string('a', 60);
        var converter = new DocumentMarkdownConverter(new FakePdfText(new PdfText([page, page], "My \"Title\"")));

        var result = converter.Convert(pdf, md, "abc123");

        Assert.AreEqual(ConversionStatus.Converted, result.Status);
        var text = File.ReadAllText(md);
        StringAssert.Contains(text, "title: \"My 'Title'\"");
        StringAssert.Contains(text, "kind: pdf");
        StringAssert.Contains(text, "pages: 2");
        StringAssert.Contains(text, "<a id=\"p2\"></a>\n## Page 2");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_PdfWithoutTitle_UsesFileName()
    {
        var pdf = Write("manual.pdf", "%PDF-1.4");
        var md = Path.Combine(dir, "m.md");

        new DocumentMarkdownConverter(new FakePdfText(new PdfText([new string('a', 80)], null))).Convert(pdf, md, "h");

        StringAssert.Contains(File.ReadAllText(md), "title: \"manual\"");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_ThinPdf_NeedsOcr()
    {
        var pdf = Write("scan.pdf", "%PDF-1.4");
        var converter = new DocumentMarkdownConverter(new FakePdfText(new PdfText(["x", "y"], null)));

        var result = converter.Convert(pdf, Path.Combine(dir, "s.md"), "h");

        Assert.AreEqual(ConversionStatus.NeedsOcr, result.Status);
        Assert.IsFalse(File.Exists(Path.Combine(dir, "s.md")));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_PdfWithNoPages_NeedsOcr()
    {
        var pdf = Write("none.pdf", "%PDF-1.4");

        var result = new DocumentMarkdownConverter(new FakePdfText(new PdfText([], null))).Convert(pdf, Path.Combine(dir, "n.md"), "h");

        Assert.AreEqual(ConversionStatus.NeedsOcr, result.Status);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_NamedPdfWithoutHeader_NotPdf()
    {
        var pdf = Write("fake.pdf", "<html>metadata</html>");

        var result = new DocumentMarkdownConverter(new FakePdfText(new PdfText([], null))).Convert(pdf, Path.Combine(dir, "f.md"), "h");

        Assert.AreEqual(ConversionStatus.NotPdf, result.Status);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_Text_NormalisesLineEndings()
    {
        var txt = Write("notes.txt", "one\r\ntwo\rthree");
        var md = Path.Combine(dir, "notes.md");

        var result = new DocumentMarkdownConverter(new FakePdfText(new PdfText([], null))).Convert(txt, md, "h");

        Assert.AreEqual(ConversionStatus.Converted, result.Status);
        StringAssert.EndsWith(File.ReadAllText(md), "one\ntwo\nthree");
        StringAssert.Contains(File.ReadAllText(md), "kind: text");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_EmptyText_Empty()
    {
        var txt = Write("blank.txt", "  \n ");

        var result = new DocumentMarkdownConverter(new FakePdfText(new PdfText([], null))).Convert(txt, Path.Combine(dir, "b.md"), "h");

        Assert.AreEqual(ConversionStatus.Empty, result.Status);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Convert_ExistingMarkdown_Unchanged()
    {
        var txt = Write("a.txt", "hello");
        var md = Write("a.md", "kept");

        var result = new DocumentMarkdownConverter(new FakePdfText(new PdfText([], null))).Convert(txt, md, "h");

        Assert.AreEqual(ConversionStatus.Unchanged, result.Status);
        Assert.AreEqual("kept", File.ReadAllText(md));
    }
}
