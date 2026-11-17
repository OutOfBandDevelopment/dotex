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

    [TestMethod, TestCategory(TestCategories.Unit)]
    public async Task TransformAsync_InputFile_WritesOutputAndLeavesNoTempFile()
    {
        // Stage
        using var temp = new TempFileFactory(null).GetTempDirectory();
        var template = Path.Combine(temp.DirectoryPath, "t.xslt");
        File.WriteAllText(template, Stylesheet);
        var input = Path.Combine(temp.DirectoryPath, "in.xml");
        File.WriteAllText(input, "<root id=\"7\" />");
        var output = Path.Combine(temp.DirectoryPath, "out", "result.txt");
        TestContext.WriteLine($"template={template} input={input} output={output}");

        // Test
        await new XsltTransformer(temp.DirectoryPath).TransformAsync(template, input, output, TestContext.CancellationToken);

        // Assert
        Assert.AreEqual("7", File.ReadAllText(output));
        Assert.AreEqual(1, Directory.GetFiles(Path.GetDirectoryName(output)!).Length, "only the result stays");
    }

    public required TestContext TestContext { get; set; }
}
