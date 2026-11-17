using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.Documents.Markdown.Tests;

[TestClass]
public class ChunkAndPathTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Split_SmallParagraphs_PackedIntoOneChunk()
    {
        var text = new string('a', 100) + "\n\n" + new string('b', 100);

        var chunks = ChunkText.Split(text);

        Assert.AreEqual(1, chunks.Count);
        Assert.AreEqual(202, chunks[0].Length);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Split_PackFull_StartsNextChunk()
    {
        var text = new string('a', 1000) + "\n\n" + new string('b', 1000);

        Assert.AreEqual(2, ChunkText.Split(text).Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Split_LongParagraph_CutAtSpaceWithOverlap()
    {
        var text = string.Join(" ", Enumerable.Range(0, 600).Select(i => "word" + i));

        var chunks = ChunkText.Split(text);

        Assert.IsTrue(chunks.Count >= 3);
        Assert.IsTrue(chunks.All(c => c.Length <= ChunkText.MaxChars));
        Assert.IsTrue(chunks[0].Length >= ChunkText.MaxChars / 2);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Split_NoSpaces_CutsHard()
    {
        var chunks = ChunkText.Split(new string('x', 3200));

        Assert.AreEqual(ChunkText.MaxChars, chunks[0].Length);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Split_ShortChunksAndBlanks_Dropped()
    {
        Assert.AreEqual(0, ChunkText.Split("tiny\n\n   \n\nbits").Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Split_SurrogatePairs_CountOnce()
    {
        var text = string.Concat(Enumerable.Repeat("\U0001F600", 40));

        Assert.AreEqual(1, ChunkText.Split(text).Count);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Markdown_NestedPath_SlugsFoldersAndAddsHash()
    {
        var path = ExtractPaths.Markdown("x", "Manuals/Old Stuff/Radio_Guide v2.PDF", "0123456789abcdef");

        Assert.AreEqual(Path.Combine("x", "manuals", "old-stuff", "radio-guide-v2-01234567.md"), path);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Markdown_LooseFile_UsesRootFolder()
    {
        var path = ExtractPaths.Markdown("x", "note.txt", "abcdefghij");

        Assert.AreEqual(Path.Combine("x", "(root)", "note-abcdefgh.md"), path);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Markdown_MountPrefix_KeptInFolder()
    {
        var path = ExtractPaths.Markdown("x", "@nas/Docs/a.pdf", "abcdefghij");

        Assert.AreEqual(Path.Combine("x", "@nas", "docs", "a-abcdefgh.md"), path);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Slug_EmptyOrLong_Bounded()
    {
        Assert.AreEqual("x", ExtractPaths.Slug("***"));
        Assert.AreEqual(80, ExtractPaths.Slug(new string('a', 200)).Length);
    }
}
