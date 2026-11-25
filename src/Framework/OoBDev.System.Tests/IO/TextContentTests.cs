using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System.IO;
using System.Text;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class TextContentTests
{
    public TestContext TestContext { get; set; } = null!;

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("plain text\r\nwith lines\tand tabs", true)]
    [DataRow("", true)]
    public void LooksLikeText_Utf8_Matches(string text, bool expected)
    {
        // Test
        var result = TextContent.LooksLikeText(Encoding.UTF8.GetBytes(text));

        // Assert
        TestContext.WriteLine("\"{0}\" -> {1}", text, result);
        Assert.AreEqual(expected, result);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LooksLikeText_NulBytes_IsBinary() =>
        Assert.IsFalse(TextContent.LooksLikeText(new byte[] { 0x4D, 0x5A, 0x00, 0x03, 0x00 }));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LooksLikeText_ManyControlCharacters_IsBinary() =>
        Assert.IsFalse(TextContent.LooksLikeText(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 0x41, 0x42 }));

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void LooksLikeText_Utf16WithByteOrderMark_IsText()
    {
        // Stage
        byte[] bytes = [.. Encoding.Unicode.GetPreamble(), .. Encoding.Unicode.GetBytes("hello")];

        // Test and Assert
        Assert.IsTrue(TextContent.LooksLikeText(bytes));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void FileLooksLikeText_TextAndBinaryFiles_AreToldApart()
    {
        // Stage
        using var temp = new TempDirectoryHandle();
        var dir = temp.DirectoryPath;
        var text = Path.Combine(dir, "a.unknown");
        var binary = Path.Combine(dir, "b.unknown");
        File.WriteAllText(text, "just words");
        File.WriteAllBytes(binary, [0x89, 0x50, 0x4E, 0x47, 0x00, 0x00, 0x1A]);

        // Test and Assert
        TestContext.WriteLine("Files in {0}", dir);
        Assert.IsTrue(TextContent.FileLooksLikeText(text));
        Assert.IsFalse(TextContent.FileLooksLikeText(binary));
    }
}
