using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.Xml.Xsl.Extensions;
using OoBDev.TestUtilities;
using System.IO;
using System.Xml;

namespace OoBDev.System.Tests.Xml.Xsl.Extensions;

[TestClass]
public class FileExtensionsTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReadXml_MissingFile_ThrowsByDefaultAndReturnsEmptyWhenAsked()
    {
        using var temp = new TempFileFactory(null).GetTempDirectory();
        var missing = Path.Combine(temp.DirectoryPath, "missing.xml");
        TestContext.WriteLine($"missing={missing}");

        Assert.ThrowsExactly<FileNotFoundException>(() => new FileExtensions(temp.DirectoryPath).ReadXml(missing));
        var empty = new FileExtensions(temp.DirectoryPath, emptyOnUnreadable: true).ReadXml(missing);

        Assert.IsFalse(empty.MoveToFirstChild());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReadXml_NotXml_ThrowsByDefaultAndReturnsEmptyWhenAsked()
    {
        using var temp = new TempFileFactory(null).GetTempDirectory();
        var file = Path.Combine(temp.DirectoryPath, "plain.txt");
        File.WriteAllText(file, "this is not xml");

        Assert.ThrowsExactly<XmlException>(() => new FileExtensions(temp.DirectoryPath).ReadXml(file));
        Assert.IsFalse(new FileExtensions(temp.DirectoryPath, emptyOnUnreadable: true).ReadXml(file).MoveToFirstChild());
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ReadXml_ValidFile_ReturnsDocument()
    {
        using var temp = new TempFileFactory(null).GetTempDirectory();
        var file = Path.Combine(temp.DirectoryPath, "ok.xml");
        File.WriteAllText(file, "<a><b/></a>");

        var nav = new FileExtensions(temp.DirectoryPath).ReadXml(file);

        Assert.IsTrue(nav.MoveToFirstChild());
        Assert.AreEqual("a", nav.Name);
    }
}
