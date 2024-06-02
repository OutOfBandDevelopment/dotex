using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.XPath;
using System.Xml.Xsl;

namespace OoBDev.System.Text.Html.Tests;

[TestClass]
public class HtmlTemplateTransformTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod, TestCategory(TestCategories.DevLocal)]
    public async Task DeeperTest()
    {
        var xsltArgumentList = new XsltArgumentList();

        using var styleSheet = this.GetResourceStream("SimpleCopy.xslt");
        var template = await this.GetResourceAsStringAsync("TestTemplate.html").ConfigureAwait(false);

        var xslt = new XslCompiledTransform(false);
        using var xmlreader = XmlReader.Create(styleSheet, new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Parse,
            ConformanceLevel = ConformanceLevel.Document,
            NameTable = new NameTable(),
        });
        var xsltSettings = new XsltSettings(false, false);
        xslt.Load(xmlreader, xsltSettings, null);

        using var resultStream = new MemoryStream();

        XPathNavigator nav = new HtmlTemplateTransform(null, null).ToXPathNavigator(template);

        xslt.Transform(nav, xsltArgumentList, resultStream);

        TestContext.AddResult(resultStream, "TestResult.html");
    }
}
