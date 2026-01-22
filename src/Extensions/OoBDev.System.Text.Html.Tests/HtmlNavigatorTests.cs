using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Xml.XPath;
using OoBDev.TestUtilities;
using System.Linq;
using System.Xml;

namespace OoBDev.System.Text.Html.Tests;

[TestClass]
public class HtmlNavigatorTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void QueryTest()
    {
        using var htmlStream = this.GetResourceStream("ComplexTemplate.html");
        var html = new HtmlNavigator();
        var htmlNav = html.ToNavigable(htmlStream).CreateNavigator().Clone();

        // Test selecting value-of elements (13 in ComplexTemplate.html)
        var valueOf = htmlNav.Select("//value-of");
        var valueOfNodes = valueOf.AsNodeSet().ToList();
        TestContext.WriteLine($"value-of: {valueOfNodes.Count}");
        Assert.HasCount(13, valueOfNodes, "Should find 13 value-of elements");

        // Test selecting value-attr attributes (3 in ComplexTemplate.html)
        var valueAttr = htmlNav.Select("//@value-attr");
        var valueAttrNodes = valueAttr.AsNodeSet().ToList();
        TestContext.WriteLine($"value-attr: {valueAttrNodes.Count}");
        Assert.HasCount(3, valueAttrNodes, "Should find 3 value-attr attributes");

        // Test selecting repeater elements (6 in ComplexTemplate.html)
        var repeater = htmlNav.Select("//repeater");
        var repeaterNodes = repeater.AsNodeSet().ToList();
        TestContext.WriteLine($"repeater: {repeaterNodes.Count}");
        Assert.HasCount(6, repeaterNodes, "Should find 6 repeater elements");

        // Test selecting condition elements (5 in ComplexTemplate.html)
        var condition = htmlNav.Select("//condition");
        var conditionNodes = condition.AsNodeSet().ToList();
        TestContext.WriteLine($"condition: {conditionNodes.Count}");
        Assert.HasCount(5, conditionNodes, "Should find 5 condition elements");

        // Test selecting data-binding attributes (8 in ComplexTemplate.html)
        var dataBinding = htmlNav.Select("//@data-binding");
        var dataBindingNodes = dataBinding.AsNodeSet().ToList();
        TestContext.WriteLine($"data-binding: {dataBindingNodes.Count}");
        Assert.HasCount(8, dataBindingNodes, "Should find 8 data-binding attributes");
    }
}
