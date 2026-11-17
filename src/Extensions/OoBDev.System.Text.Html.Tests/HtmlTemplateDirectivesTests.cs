using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Threading.Tasks;

namespace OoBDev.System.Text.Html.Tests;

[TestClass]
public class HtmlTemplateDirectivesTests
{
    private const string Data = """
        {
          "Parent": { "FirstName": "Chris" },
          "Marker": ")",
          "Class": { "Date": "5/12/2020", "Cost": "1234.5" },
          "Link": { "Image": "http://example.test/img.png" },
          "Periods": [
            { "Number": 1, "Things": ["Math", "English"] },
            { "Number": 2, "Things": ["History"] }
          ],
          "FullSchedule": [
            { "Name": "Science", "Period": 1 },
            { "Name": "Math", "Period": 2 },
            { "Name": "<b>Art</b>", "Period": 3 }
          ]
        }
        """;

    private static async Task<string> Run(string template)
    {
        var factory = new JsonInstanceFactory();
        var transform = new HtmlTemplateTransform(factory, new HtmlDocumentVisitor(factory));
        return await transform.Transform(Data, template).ConfigureAwait(false);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueOf_ReplacesTheElementWithTheValue() =>
        Assert.AreEqual("<p>Chris</p>", await Run("""<p><value-of binding="$.Parent.FirstName" /></p>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueOf_EncodesMarkupInData() =>
        Assert.AreEqual("&lt;b&gt;Art&lt;/b&gt;", await Run("""<value-of binding="FullSchedule[2].Name" />"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueOf_FormatsDatesAndNumbers()
    {
        Assert.AreEqual("2020-05-12", await Run("""<value-of binding="Class.Date" format="yyyy-MM-dd" />"""));
        Assert.AreEqual("1234.50", await Run("""<value-of binding="Class.Cost" format="0.00" type="number" />"""));
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueOf_MissingBindingGivesEmptyText() =>
        Assert.AreEqual("[]", await Run("""[<value-of binding="Nope.Nothing" />]"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Repeater_RepeatsOverAnArray() =>
        Assert.AreEqual("Science,Math,&lt;b&gt;Art&lt;/b&gt;,", await Run("""<repeater binding="FullSchedule"><value-of binding="Name" />,</repeater>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Repeater_FilterSelectsItems() =>
        Assert.AreEqual("Math;&lt;b&gt;Art&lt;/b&gt;;", await Run("""<repeater binding="FullSchedule[?(@.Period >= 2)]"><value-of binding="Name" />;</repeater>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Repeater_NestedScopesAndRootPaths() =>
        Assert.AreEqual("1:Math) 1:English) 2:History) ", await Run(
            """<repeater binding="Periods" item="period"><repeater binding="Things" item="thing"><value-of source="period" binding="Number" />:<value-of source="thing" binding="@" /><value-of binding="$.Marker" /> </repeater></repeater>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Condition_MarksOnlyMatchingItems() =>
        Assert.AreEqual("Science Math* ", await Run(
            """<repeater binding="FullSchedule[?(@.Period < 3)]"><value-of binding="Name" /><condition rule="@.Period == 2">*</condition> </repeater>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueAttr_SetsTheAttributeOnThePrecedingInput() =>
        Assert.AreEqual("""<li><input type="checkbox" checked="checked"></li>""", await Run(
            """<li><input type="checkbox" /><value-attr value="checked" item="checked" rule="$.FullSchedule" /></li>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueAttr_RuleThatFailsLeavesTheAttributeOff() =>
        Assert.AreEqual("""<li><input type="checkbox"></li>""", await Run(
            """<li><input type="checkbox" /><value-attr value="checked" item="checked" rule="@.Missing == 1" /></li>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task ValueAttr_BindingSetsTheImageSource() =>
        Assert.AreEqual("""<p><img src="http://example.test/img.png">Text</p>""", await Run(
            """<p><img /><value-attr binding="$.Link.Image" item="src" />Text</p>"""));

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task DataBinding_ReplacesTheContentAndDropsTheAttribute() =>
        Assert.AreEqual("<div>Chris</div>", await Run("""<div data-binding="$.Parent.FirstName">Other Stuff</div>"""));
}
