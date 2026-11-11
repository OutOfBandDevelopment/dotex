using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.TestUtilities;
using System;
using System.IO;

namespace OoBDev.System.Tests.IO;

[TestClass]
public class IniFileTests
{
    private TempDirectoryHandle? _temp;
    private string _dir = string.Empty;

    [TestInitialize]
    public void Setup()
    {
        _temp = new TempDirectoryHandle();
        _dir = _temp.DirectoryPath;
    }

    [TestCleanup]
    public void Cleanup() => _temp?.Dispose();

    private string Path(string name = "a.ini") => global::System.IO.Path.Combine(_dir, name);

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_MissingFile_ReturnsDefault()
    {
        var ini = new IniFile(Path());
        Assert.AreEqual(string.Empty, ini.Read("S", "k"));
        Assert.AreEqual("d", ini.Read("S", "k", "d"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Write_NewFile_CreatesSectionAndKey()
    {
        var ini = new IniFile(Path());
        ini.Write("Net", "port", "8080");
        Assert.AreEqual("8080", ini.Read("Net", "port"));
        Assert.AreEqual("8080", ini.Read("net", "PORT"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Write_ExistingKey_ReplacesValueAndKeepsOtherLines()
    {
        File.WriteAllText(Path(), "; top comment\n[A]\nx=1\n# note\ny=2\n\n[B]\nx=9\n");
        var ini = new IniFile(Path());
        ini.Write("A", "y", "3");
        Assert.AreEqual("3", ini.Read("A", "y"));
        Assert.AreEqual("1", ini.Read("A", "x"));
        Assert.AreEqual("9", ini.Read("B", "x"));
        StringAssert.Contains(File.ReadAllText(Path()), "; top comment");
        StringAssert.Contains(File.ReadAllText(Path()), "# note");
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Write_NewKeyInExistingSection_InsertsAfterLastEntry()
    {
        File.WriteAllText(Path(), "[A]\nx=1\n\n[B]\nz=2\n");
        var ini = new IniFile(Path());
        ini.Write("A", "n", "5");
        Assert.AreEqual("5", ini.Read("A", "n"));
        Assert.AreEqual("", ini.Read("B", "n"));
        Assert.AreEqual("2", ini.Read("B", "z"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Write_NewSection_IsAppended()
    {
        File.WriteAllText(Path(), "[A]\nx=1\n");
        var ini = new IniFile(Path());
        ini.Write("C", "k", "v");
        Assert.AreEqual("v", ini.Read("C", "k"));
        Assert.AreEqual("1", ini.Read("A", "x"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Indexers_UseDefaultSectionAndExplicitSection()
    {
        var ini = new IniFile(Path(), "Main");
        ini["name"] = "box";
        ini["Other", "name"] = "crate";
        Assert.AreEqual("box", ini["name"]);
        Assert.AreEqual("crate", ini["Other", "name"]);
        Assert.AreEqual("box", ini.Read("Main", "name"));
        Assert.AreEqual("Main", ini.DefaultSection);
        Assert.AreEqual(Path(), ini.Path);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_ValueWithEqualsAndSpaces_IsTrimmedAtTheFirstEquals()
    {
        File.WriteAllText(Path(), "[A]\n  key = a=b \n");
        Assert.AreEqual("a=b", new IniFile(Path()).Read("A", "key"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_CommentedKey_IsIgnored()
    {
        File.WriteAllText(Path(), "[A]\n;k=1\n#k=2\n");
        Assert.AreEqual("none", new IniFile(Path()).Read("A", "k", "none"));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Constructor_BlankArguments_Throw()
    {
        Assert.ThrowsExactly<ArgumentException>(() => new IniFile(" "));
        Assert.ThrowsExactly<ArgumentException>(() => new IniFile(Path(), " "));
    }
}
