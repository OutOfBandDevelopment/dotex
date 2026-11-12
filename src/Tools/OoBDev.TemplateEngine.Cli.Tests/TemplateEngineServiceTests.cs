using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.IO;
using OoBDev.System.Text.Templating;
using OoBDev.System.Text.Templating.Xslt;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.TemplateEngine.Cli.Tests;

/// <summary>The service sends XSLT templates through the sandboxed transformer and everything else to the engine.</summary>
[TestClass]
[DoNotParallelize]
public sealed class TemplateEngineServiceTests
{
    private const string CountItems = """
        <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:output method="text"/>
          <xsl:template match="/"><xsl:value-of select="count(//item)"/></xsl:template>
        </xsl:stylesheet>
        """;

    private ITempDirectory? _temp;
    private string _folder = string.Empty;

    [TestInitialize]
    public void Init()
    {
        _temp = new TempFileFactory(new ServiceCollection().BuildServiceProvider()).GetTempDirectory();
        _folder = _temp.DirectoryPath;
    }

    [TestCleanup]
    public void Cleanup() => _temp?.Dispose();

    [TestMethod]
    [TestCategory("Unit")]
    public async Task StartAsync_XsltTemplateXmlInput_WritesResultWithoutTheEngine()
    {
        // Stage
        File.WriteAllText(Path.Combine(_folder, "doc.xml"), "<root><item/><item/></root>");
        File.WriteAllText(Path.Combine(_folder, "t.xslt"), CountItems);
        var engine = new Mock<ITemplateEngine>(MockBehavior.Strict);

        // Test
        await Create(engine, new TemplateEngineOptions
        {
            InputFile = Path.Combine(_folder, "doc.xml"),
            Template = Path.Combine(_folder, "t.xslt"),
            OutputFile = Path.Combine(_folder, "[file].txt"),
        }).StartAsync(CancellationToken.None);

        // Assert
        Assert.AreEqual("2", File.ReadAllText(Path.Combine(_folder, "doc.txt")));
        engine.VerifyNoOtherCalls();
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task StartAsync_XsltTemplateMergeJsonInputs_WritesOneFile()
    {
        // Stage
        File.WriteAllText(Path.Combine(_folder, "a.json"), """{"item": [1]}""");
        File.WriteAllText(Path.Combine(_folder, "b.json"), """{"item": [1, 2]}""");
        File.WriteAllText(Path.Combine(_folder, "t.xsl"), """
            <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="text"/>
              <xsl:template match="/"><xsl:value-of select="count(//*)"/></xsl:template>
            </xsl:stylesheet>
            """);

        // Test
        await Create(new Mock<ITemplateEngine>(MockBehavior.Strict), new TemplateEngineOptions
        {
            InputFile = Path.Combine(_folder, "?.json"),
            InputFileType = InputTypes.Json,
            Template = Path.Combine(_folder, "t.xsl"),
            OutputFile = Path.Combine(_folder, "merged.txt"),
            Merge = true,
        }).StartAsync(CancellationToken.None);

        // Assert
        Assert.IsTrue(int.Parse(File.ReadAllText(Path.Combine(_folder, "merged.txt")), global::System.Globalization.CultureInfo.InvariantCulture) > 0);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("a.xslt", true)]
    [DataRow("A.XSL", true)]
    [DataRow("a.hbs", false)]
    [DataRow("report", false)]
    public void IsXslt_ByExtension(string template, bool expected) => Assert.AreEqual(expected, XsltRun.IsXslt(template));

    [TestMethod]
    [TestCategory("Unit")]
    public async Task StartAsync_HandlebarsTemplate_UsesTheEngine()
    {
        // Stage
        File.WriteAllText(Path.Combine(_folder, "doc.json"), """{"a":1}""");
        var engine = new Mock<ITemplateEngine>();
        engine.Setup(e => e.ApplyAsync("report", It.IsAny<object>(), It.IsAny<Stream>())).ReturnsAsync((ITemplateContext?)null);

        // Test
        await Create(engine, new TemplateEngineOptions
        {
            InputFile = Path.Combine(_folder, "doc.json"),
            Template = "report",
            OutputFile = Path.Combine(_folder, "out", "[file].md"),
        }).StartAsync(CancellationToken.None);

        // Verify
        engine.Verify(e => e.ApplyAsync("report", It.IsAny<object>(), It.IsAny<Stream>()), Times.Once);
    }

    private static TemplateEngineService Create(Mock<ITemplateEngine> engine, TemplateEngineOptions options) =>
        new(NullLogger<TemplateEngineService>.Instance, Options.Create(options), engine.Object);
}
