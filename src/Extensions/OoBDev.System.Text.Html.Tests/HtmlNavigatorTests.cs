using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;

namespace OoBDev.System.Text.Html.Tests;

[TestClass]
public class HtmlNavigatorTests
{

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public void QueryTest()
    {
        using var styleSheet = this.GetResourceStream("ComplexTemplate.html");
        var html = new HtmlNavigator();
        var xpath = html.ToNavigable(styleSheet).CreateNavigator().Clone();


        var valueOf = xpath.Select("//value-of");
        var valueAttr = xpath.Select("//value-attr");
        var repeater = xpath.Select("//repeater");
        var condition = xpath.Select("//condition");
        var dataBinding = xpath.Select("//@data-binding");
    }
}
