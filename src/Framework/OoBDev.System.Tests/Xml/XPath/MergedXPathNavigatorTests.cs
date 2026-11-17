using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.Xml.XPath;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Tests.Xml.XPath;

[TestClass]
public class MergedXPathNavigatorTests
{
        [TestMethod, TestCategory(TestCategories.Unit)]
    public void MergeMultiplePathsTest()
    {
        // Stage: two directories, the first one used twice under different source names
        using var factory = new TempFileFactory(null!).GetTempDirectory();
        var one = Directory.CreateDirectory(Path.Combine(factory.DirectoryPath, "one"));
        File.WriteAllText(Path.Combine(one.FullName, "a.txt"), "a");
        var two = Directory.CreateDirectory(Path.Combine(factory.DirectoryPath, "two"));
        File.WriteAllText(Path.Combine(two.FullName, "b.txt"), "bb");
        var navs = new (string, IXPathNavigable?)[] { ("f1", one.ToNavigable()), ("f2", two.ToNavigable()), ("f3", one.ToNavigable()) };

        // Test
        var navigator = navs.MergeNavigators().CreateNavigator()!;

        // Assert
        Assert.AreEqual(3, ((double)navigator.Evaluate("count(/Top/Node)")));
        Assert.AreEqual("f1", navigator.SelectSingleNode("/Top/Node[1]/@Source")?.Value);
        Assert.AreEqual("f3", navigator.SelectSingleNode("/Top/Node[3]/@Source")?.Value);
        Assert.AreEqual("a.txt", navigator.SelectSingleNode("/Top/Node[@Source='f1']/Directory/File/@Name")?.Value);
        Assert.AreEqual("a.txt", navigator.SelectSingleNode("/Top/Node[@Source='f3']/Directory/File/@Name")?.Value);
        Assert.AreEqual("b.txt", navigator.SelectSingleNode("/Top/Node[@Source='f2']/Directory/File/@Name")?.Value);
        Assert.AreEqual("2", navigator.SelectSingleNode("/Top/Node[@Source='f2']/Directory/File/@Length")?.Value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MergeNavigators_NullSource_IsSkipped()
    {
        var navs = new (string, IXPathNavigable?)[] { ("gone", null), ("kept", new XDocument(new XElement("r", "v")).ToXPathNavigable()) };

        var navigator = navs.MergeNavigators().CreateNavigator()!;

        Assert.AreEqual(1d, navigator.Evaluate("count(/Top/Node)"));
        Assert.AreEqual("kept", navigator.SelectSingleNode("/Top/Node/@Source")?.Value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void MergeNavigators_OnlyNullSources_Throws() =>
        Assert.ThrowsExactly<ArgumentNullException>(() => new (string, IXPathNavigable?)[] { ("gone", null) }.MergeNavigators());

    [TestMethod, TestCategory(TestCategories.Unit)]
    [TestTarget(typeof(XPathExtensions), Member = nameof(XPathExtensions.MergeWith))]
    public void MergeMultipleTest()
    {
        var x1 = ("x1", new XDocument(new XElement("top1", "test1")).CreateNavigator());
        var x2 = ("x2", new XDocument(new XElement("top2", "test2")).CreateNavigator());

        var merged = x1.MergeWith(x2);
        var mergedNav = merged.CreateNavigator();
        mergedNav?.MoveToRoot();

        var x = mergedNav?.Select("/Top/Node/top1");
        Assert.IsNotNull(x);
        Assert.IsTrue(x.MoveNext());
        Assert.AreEqual("test1", x.Current?.Value);

        var y = mergedNav?.Select("/Top/Node/top2");
        Assert.IsNotNull(y);
        Assert.IsTrue(y.MoveNext());
        Assert.AreEqual("test2", y.Current?.Value);
    }
}
