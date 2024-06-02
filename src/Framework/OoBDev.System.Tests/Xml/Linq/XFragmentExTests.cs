using OoBDev.System.Xml.Linq;
using OoBDev.TestUtilities;
using System.Xml.Linq;

namespace OoBDev.System.Tests.Xml.Linq;

[TestClass]
public class XFragmentExTests
{
    [TestMethod, TestCategory(TestCategories.Unit)]
    public void ToFragment_IEnumerable_XNode()
    {
        var nodes = new[]{
            new XElement("test"),
            new XElement("test2",
                new XElement("child",
                    new XAttribute("attr1", "attr1value")
                    )
                ),
        };
        var fragment = nodes.ToXFragment();

        var xml = @"<test /><test2><child attr1=""attr1value"" /></test2>";
        Assert.AreEqual((string)fragment, xml);
    }
}
