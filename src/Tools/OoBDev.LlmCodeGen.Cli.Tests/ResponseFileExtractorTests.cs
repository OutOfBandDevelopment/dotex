using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.LlmCodeGen.Cli.Tests;

[TestClass]
public class ResponseFileExtractorTests
{
    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_NoFences_ReturnsEmpty()
    {
        // Test
        var files = ResponseFileExtractor.Extract("just some prose");

        // Assert
        Assert.AreEqual(0, files.Count);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_BoldNameThenFence_UsesName()
    {
        // Test
        var files = ResponseFileExtractor.Extract("**src/Foo.cs**\n```csharp\nclass Foo {}\n```\n");

        // Assert
        Assert.AreEqual(1, files.Count);
        Assert.AreEqual("src/Foo.cs", files[0].RelativePath);
        Assert.AreEqual("class Foo {}", files[0].Content);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_BackTickedNameWithColon_UsesName()
    {
        // Test
        var files = ResponseFileExtractor.Extract("File: `a.txt`:\n```\nhello\n```");

        // Assert
        Assert.AreEqual("a.txt", files[0].RelativePath);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_CommentPathOnFirstLine_UsesNameAndDropsLine()
    {
        // Test
        var files = ResponseFileExtractor.Extract("```ts\n// app/list.tsx\nexport {};\n```");

        // Assert
        Assert.AreEqual("app/list.tsx", files[0].RelativePath);
        Assert.AreEqual("export {};", files[0].Content);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_UnnamedBlocks_NumberedUnknown()
    {
        // Test
        var files = ResponseFileExtractor.Extract("```\none\n```\ntext\n```\ntwo\n```");

        // Assert
        Assert.AreEqual("unknown1.txt", files[0].RelativePath);
        Assert.AreEqual("unknown2.txt", files[1].RelativePath);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    [DataRow("../evil.txt")]
    [DataRow("a/../../evil.txt")]
    [DataRow("/etc/passwd")]
    [DataRow("C:/Windows/x.txt")]
    [DataRow("bad|name.txt")]
    [DataRow("a//b.txt")]
    public void Extract_UnsafeName_BecomesErrorFileWithOriginalNameKept(string name)
    {
        // Test
        var files = ResponseFileExtractor.Extract($"**{name}**\n```\nbody\n```");

        // Assert
        Assert.AreEqual("error1.txt", files[0].RelativePath);
        StringAssert.Contains(files[0].Content, name);
        StringAssert.Contains(files[0].Content, "body");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_MultipleFiles_KeepsOrder()
    {
        // Test
        var files = ResponseFileExtractor.Extract("**a.txt**\n```\n1\n```\n\n**b.txt**\n```\n2\n```");

        // Assert
        Assert.AreEqual(2, files.Count);
        Assert.AreEqual("a.txt", files[0].RelativePath);
        Assert.AreEqual("b.txt", files[1].RelativePath);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Extract_NameSeparatedByProse_DoesNotCarryOver()
    {
        // Test
        var files = ResponseFileExtractor.Extract("**a.txt**\nsome prose\n```\n1\n```");

        // Assert
        Assert.AreEqual("unknown1.txt", files[0].RelativePath);
    }
}
