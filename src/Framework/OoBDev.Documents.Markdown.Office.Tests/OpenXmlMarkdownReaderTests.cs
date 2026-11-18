using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Markdown;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Office.Tests;

[TestClass]
public sealed class OpenXmlMarkdownReaderTests
{
    public required TestContext TestContext { get; set; }

    private const string Rel = "http://schemas.openxmlformats.org/package/2006/relationships";

    private static string Build(string extension, params (string Name, string Xml)[] parts)
    {
        var path = Path.Combine(Path.GetTempPath(), $"openxml_{Guid.NewGuid():N}{extension}");
        using var zip = ZipFile.Open(path, ZipArchiveMode.Create);
        foreach (var (name, xml) in parts)
        {
            using var writer = new StreamWriter(zip.CreateEntry(name).Open());
            writer.Write(xml);
        }

        return path;
    }

    private static string Workbook() => Build(
        ".xlsx",
        ("xl/workbook.xml", "<workbook xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><sheets><sheet name='People' sheetId='1' r:id='rId1'/><sheet name='Empty' sheetId='2' r:id='rId2'/></sheets></workbook>"),
        ("xl/_rels/workbook.xml.rels", $"<Relationships xmlns='{Rel}'><Relationship Id='rId1' Target='worksheets/sheet1.xml'/><Relationship Id='rId2' Target='worksheets/sheet2.xml'/></Relationships>"),
        ("xl/sharedStrings.xml", "<sst xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main'><si><t>Name</t></si><si><t>Note</t></si><si><r><t>Ada </t></r><r><t>L</t></r></si></sst>"),
        ("xl/worksheets/sheet1.xml", "<worksheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main'><sheetData><row r='1'><c r='A1' t='s'><v>0</v></c><c r='B1' t='s'><v>1</v></c><c r='C1' t='inlineStr'><is><t>Ok</t></is></c></row><row r='2'><c r='A2' t='s'><v>2</v></c><c r='C2' t='b'><v>1</v></c></row><row r='3'><c r='B3'><v>a|b</v></c></row></sheetData></worksheet>"),
        ("xl/worksheets/sheet2.xml", "<worksheet xmlns='http://schemas.openxmlformats.org/spreadsheetml/2006/main'><sheetData/></worksheet>"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_Xlsx_WritesATablePerSheetWithSharedStringsGapsAndEscapes()
    {
        var path = Workbook();
        try
        {
            var markdown = new XlsxFileMarkdownReader().Read(path, CancellationToken.None);

            TestContext.WriteLine("Markdown:{0}{1}", Environment.NewLine, markdown);
            StringAssert.Contains(markdown, "## People");
            StringAssert.Contains(markdown, "| Name | Note | Ok |");
            StringAssert.Contains(markdown, "| --- | --- | --- |");
            StringAssert.Contains(markdown, "| Ada L |  | TRUE |");
            StringAssert.Contains(markdown, "|  | a\\|b |  |");
            StringAssert.Contains(markdown, "## Empty");
            StringAssert.Contains(markdown, "_(empty)_");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_XlsxWithoutWorkbook_IsNotSupported()
    {
        var path = Build(".xlsx", ("other.xml", "<a/>"));
        try
        {
            Assert.ThrowsExactly<NotSupportedException>(() => new XlsxFileMarkdownReader().Read(path, CancellationToken.None));
        }
        finally
        {
            File.Delete(path);
        }
    }

    private static string Deck() => Build(
        ".pptx",
        ("ppt/presentation.xml", "<p:presentation xmlns:p='http://schemas.openxmlformats.org/presentationml/2006/main' xmlns:r='http://schemas.openxmlformats.org/officeDocument/2006/relationships'><p:sldIdLst><p:sldId id='256' r:id='rId2'/><p:sldId id='257' r:id='rId1'/></p:sldIdLst></p:presentation>"),
        ("ppt/_rels/presentation.xml.rels", $"<Relationships xmlns='{Rel}'><Relationship Id='rId1' Target='slides/slide1.xml'/><Relationship Id='rId2' Target='slides/slide2.xml'/></Relationships>"),
        ("ppt/slides/slide2.xml", "<p:sld xmlns:p='http://schemas.openxmlformats.org/presentationml/2006/main' xmlns:a='http://schemas.openxmlformats.org/drawingml/2006/main'><p:cSld><p:spTree><p:sp><p:nvSpPr><p:nvPr><p:ph type='title'/></p:nvPr></p:nvSpPr><p:txBody><a:p><a:r><a:t>Plan</a:t></a:r></a:p></p:txBody></p:sp><p:sp><p:txBody><a:p><a:r><a:t>First </a:t></a:r><a:r><a:t>point</a:t></a:r></a:p><a:p><a:r><a:t>Second</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>"),
        ("ppt/slides/_rels/slide2.xml.rels", $"<Relationships xmlns='{Rel}'><Relationship Id='n1' Target='../notesSlides/notesSlide2.xml'/></Relationships>"),
        ("ppt/notesSlides/notesSlide2.xml", "<p:notes xmlns:p='http://schemas.openxmlformats.org/presentationml/2006/main' xmlns:a='http://schemas.openxmlformats.org/drawingml/2006/main'><p:cSld><p:spTree><p:sp><p:nvSpPr><p:nvPr><p:ph type='body'/></p:nvPr></p:nvSpPr><p:txBody><a:p><a:r><a:t>Say hello</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:notes>"),
        ("ppt/slides/slide1.xml", "<p:sld xmlns:p='http://schemas.openxmlformats.org/presentationml/2006/main' xmlns:a='http://schemas.openxmlformats.org/drawingml/2006/main'><p:cSld><p:spTree><p:sp><p:txBody><a:p><a:r><a:t>Only text</a:t></a:r></a:p></p:txBody></p:sp></p:spTree></p:cSld></p:sld>"));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_Pptx_WritesSlidesInPresentationOrderWithTitleBulletsAndNotes()
    {
        var path = Deck();
        try
        {
            var markdown = new PptxFileMarkdownReader().Read(path, CancellationToken.None);

            TestContext.WriteLine("Markdown:{0}{1}", Environment.NewLine, markdown);
            Assert.IsTrue(markdown.IndexOf("## Slide 1: Plan", StringComparison.Ordinal) < markdown.IndexOf("## Slide 2", StringComparison.Ordinal), "slide order follows sldIdLst, not file names");
            StringAssert.Contains(markdown, "- First point");
            StringAssert.Contains(markdown, "- Second");
            StringAssert.Contains(markdown, "**Notes**");
            StringAssert.Contains(markdown, "Say hello");
            StringAssert.Contains(markdown, "- Only text");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void TryAddOpenXmlMarkdown_RegistersBothReaders()
    {
        var services = new ServiceCollection();

        services.TryAddOpenXmlMarkdown();
        services.TryAddOpenXmlMarkdown();

        using var provider = services.BuildServiceProvider();
        var readers = provider.GetServices<IFileMarkdownReader>().ToList();
        CollectionAssert.AreEquivalent(new[] { ".xlsx", ".xlsm", ".pptx", ".pptm" }, readers.SelectMany(r => r.Extensions).ToArray());
    }
}
