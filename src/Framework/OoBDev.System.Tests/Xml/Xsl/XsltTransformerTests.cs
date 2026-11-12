using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.Xml.Xsl;
using OoBDev.TestUtilities;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.System.Tests.Xml.Xsl;

[DoNotParallelize]
[TestClass]
public class XsltTransformerTests
{
    private const string Stylesheet = """
        <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:output method="text" />
          <xsl:template match="/"><xsl:value-of select="/root/@id" /></xsl:template>
        </xsl:stylesheet>
        """;

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task Transform_ConcurrentCalls_EachWritesItsOwnOutputAndKeepsTheWorkingDirectory()
    {
        // Stage
        using var temp = new TempFileFactory(null).GetTempDirectory();
        var template = Path.Combine(temp.DirectoryPath, "t.xslt");
        File.WriteAllText(template, Stylesheet);
        var inputs = Enumerable.Range(0, 16).Select(i =>
        {
            var path = Path.Combine(temp.DirectoryPath, $"in{i}.xml");
            File.WriteAllText(path, $"<root id=\"{i}\" />");
            return (Input: path, Output: Path.Combine(temp.DirectoryPath, $"out{i}", "result.txt"), Id: i);
        }).ToArray();
        var before = Directory.GetCurrentDirectory();
        var transformer = new XsltTransformer(temp.DirectoryPath);

        // Test
        await Task.WhenAll(inputs.Select(i => Task.Run(() => transformer.Transform(template, i.Input, i.Output), TestContext.CancellationToken)));

        // Assert
        Assert.AreEqual(before, Directory.GetCurrentDirectory());
        foreach (var i in inputs)
        {
            Assert.AreEqual(i.Id.ToString(), File.ReadAllText(i.Output));
        }
    }

    public required TestContext TestContext { get; set; }
}
