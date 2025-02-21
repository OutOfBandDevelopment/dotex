using OoBDev.System.Net.Mime;
using OoBDev.System.Text.Templating;
using OoBDev.System.Text.Xml.Serialization;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace OoBDev.System.Tests.Text.Templating;

[TestClass]
public class XsltTemplateProviderTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void CanApplyTest()
    {
        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockContext = mockRepo.Create<ITemplateContext>();
        var mockXmlSerializer = mockRepo.Create<IXmlSerializer>();

        mockContext.SetupGet(s => s.TemplateContentType).Returns(ContentTypesExtensions.Application.XSLT);

        var provider = new XsltTemplateProvider(mockXmlSerializer.Object);
        var result = provider.CanApply(mockContext.Object);

        Assert.IsTrue(result);

        mockRepo.VerifyAll();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task ApplyAsyncTest()
    {
        var mockRepo = new MockRepository(MockBehavior.Strict);
        var mockContext = mockRepo.Create<ITemplateContext>();
        var mockXmlSerializer = mockRepo.Create<IXmlSerializer>();

        mockContext.Setup(s => s.OpenTemplate).Returns(c =>
        {
            var tms = new MemoryStream();
            var xslt = @"<?xml version=""1.0"" encoding=""UTF-8""?>
<xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">

  <xsl:template match=""/"">
    <html>
      <head>
        <title>XML to HTML Transformation</title>
      </head>
      <body>
        <h1>XML to HTML Transformation</h1>
        <table border=""1"">
          <tr>
            <th>Property</th>
            <th>Value</th>
          </tr>
          <xsl:apply-templates select=""root/prop""/>
        </table>
      </body>
    </html>
  </xsl:template>

  <xsl:template match=""prop"">
    <tr>
      <td><xsl:value-of select=""name()""/></td>
      <td><xsl:value-of select="".""/></td>
    </tr>
  </xsl:template>

</xsl:stylesheet>";
            using var writer = new StreamWriter(tms, leaveOpen: true) { AutoFlush = true };
            writer.Write(xslt);
            tms.Position = 0;
            return tms;
        });

        var provider = new XsltTemplateProvider(mockXmlSerializer.Object);

        var ms = new MemoryStream();
        var x = XDocument.Parse("<root><prop>value</prop></root>");

        var result = await provider.ApplyAsync(mockContext.Object, x, ms);

        ms.Position = 0;
        var reader = XmlReader.Create(ms);

        TestContext.AddResult(reader);

        Assert.IsTrue(result);
        mockRepo.VerifyAll();
    }
}
