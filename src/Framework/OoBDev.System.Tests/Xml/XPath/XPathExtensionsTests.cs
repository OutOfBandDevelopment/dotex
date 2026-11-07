using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Xml.XPath;
using OoBDev.TestUtilities;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace OoBDev.System.Tests.Xml.XPath;

[TestClass]
public class XPathExtensionsTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsNodeSet_MixedSequence_UnwrapsNavigablesAndTextsTheRest()
    {
        // Stage
        var document = new XDocument(new XElement("root", new XElement("a"), new XElement("b")));
        object[] input = [document.ToXPathNavigable(), 42, new XDocument(new XElement("c")).ToXPathNavigable()];

        // Test
        var nodes = input.AsNodeSet().ToList();

        // Assert
        Assert.AreEqual(3, nodes.Count);
        Assert.AreEqual("42", nodes[1]?.Value);
        Assert.AreEqual("c", nodes[2]?.SelectSingleNode("/c")?.Name);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsNodeSet_SingleValue_BecomesOneTextNode()
    {
        // Test
        var nodes = ((object)"hello").AsNodeSet().ToList();

        // Assert
        Assert.AreEqual(1, nodes.Count);
        Assert.AreEqual("hello", nodes[0]?.Value);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void AsNodeSet_NodeIteratorBoxed_YieldsEachSelectedNode()
    {
        // Stage
        var navigator = new XDocument(new XElement("r", new XElement("i"), new XElement("i"))).CreateNavigator();
        object iterators = new object[] { navigator.Select("/r/i") };

        // Test
        var nodes = iterators.AsNodeSet().ToList();

        // Assert
        Assert.AreEqual(2, nodes.Count);
    }
}
