using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.Xslt.Cli.Tests;

[TestClass]
public class XsltCliOptionsTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_ShortSwitches_FillsEveryOption()
    {
        var o = XsltCliOptions.Parse(["-i", "in/*.xml", "-t", "t.xslt", "-o", "out/a.txt", "-e", "in/skip.xml", "-x", "json", "-s", "box", "-m"]);

        Assert.AreEqual(Path.Combine("in", "*.xml"), o.Input);
        Assert.AreEqual("t.xslt", o.Template);
        Assert.AreEqual(Path.Combine("out", "a.txt"), o.Output);
        Assert.AreEqual(Path.Combine("in", "skip.xml"), o.Exclude);
        Assert.AreEqual(InputTypes.Json, o.InputType);
        Assert.AreEqual("box", o.Sandbox);
        Assert.IsTrue(o.Merge);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_LongSwitches_MatchShortOnes()
    {
        var o = XsltCliOptions.Parse(["--input", "a.xml", "--template", "t.xslt", "--output", "o.txt", "--merge", "false"]);

        Assert.AreEqual("a.xml", o.Input);
        Assert.IsFalse(o.Merge);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_Defaults_UseXmlAndTheOutputFolderAsSandbox()
    {
        var o = XsltCliOptions.Parse(["-i", "a.xml", "-t", "t.xslt", "-o", Path.Combine(Path.GetTempPath(), "x", "o.txt")]);

        Assert.AreEqual(InputTypes.Xml, o.InputType);
        Assert.IsFalse(o.Merge);
        Assert.IsNull(o.Exclude);
        Assert.AreEqual(Path.Combine(Path.GetTempPath(), "x"), o.Sandbox);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_MergeBeforeAnotherSwitch_IsTrue()
    {
        var o = XsltCliOptions.Parse(["-m", "-i", "a.xml", "-t", "t.xslt", "-o", "o.txt"]);

        Assert.IsTrue(o.Merge);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("-t", "t.xslt", "-o", "o.txt")]
    [DataRow("-i", "a.xml", "-o", "o.txt")]
    [DataRow("-i", "a.xml", "-t", "t.xslt")]
    public void Parse_MissingRequiredOption_Throws(params string[] args)
    {
        Assert.ThrowsExactly<ArgumentException>(() => XsltCliOptions.Parse(args));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_UnknownInputType_Throws()
    {
        Assert.ThrowsExactly<ArgumentException>(() => XsltCliOptions.Parse(["-i", "a", "-t", "t", "-o", "o", "-x", "nope"]));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Parse_NullArgs_Throws()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => XsltCliOptions.Parse(null!));
    }
}
