using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.Xslt.Cli.Tests;

[TestClass]
public class SandboxedTransformerTests
{
    private const string Template = """
        <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
          <xsl:output method="text"/>
          <xsl:template match="/"><xsl:value-of select="count(//item)"/></xsl:template>
        </xsl:stylesheet>
        """;

    private string _folder = string.Empty;

    [TestInitialize]
    public void Init()
    {
        _folder = Path.Combine(Path.GetTempPath(), $"Test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_folder);
    }

    [TestCleanup]
    public void Cleanup() => Directory.Delete(_folder, recursive: true);

    [TestMethod]
    [TestCategory("Unit")]
    public void TransformAll_XmlInput_WritesTemplateResult()
    {
        // Stage
        var input = Path.Combine(_folder, "in.xml");
        var template = Path.Combine(_folder, "t.xslt");
        var output = Path.Combine(_folder, "*.txt");
        File.WriteAllText(input, "<root><item/><item/><item/></root>");
        File.WriteAllText(template, Template);

        // Test
        new SandboxedTransformer(_folder).TransformAll(template, input, null, InputTypes.Xml, output, merge: false);

        // Assert
        Assert.AreEqual("3", File.ReadAllText(Path.Combine(_folder, "in.txt")));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TransformAll_JsonInputByExtension_NavigatesJson()
    {
        var input = Path.Combine(_folder, "in.json");
        var template = Path.Combine(_folder, "t.xslt");
        var output = Path.Combine(_folder, "*.txt");
        File.WriteAllText(input, """{"item": [1, 2]}""");
        File.WriteAllText(template, """
            <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
              <xsl:output method="text"/>
              <xsl:template match="/"><xsl:value-of select="count(//*)"/></xsl:template>
            </xsl:stylesheet>
            """);

        new SandboxedTransformer(_folder).TransformAll(template, input, null, InputTypes.ByExtension, output, merge: false);

        var result = Path.Combine(_folder, "in.txt");
        Assert.IsTrue(File.Exists(result));
        Assert.IsTrue(int.Parse(File.ReadAllText(result), global::System.Globalization.CultureInfo.InvariantCulture) > 0);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TransformAll_MergeInputs_WritesOneFile()
    {
        File.WriteAllText(Path.Combine(_folder, "a.xml"), "<root><item/></root>");
        File.WriteAllText(Path.Combine(_folder, "b.xml"), "<root><item/><item/></root>");
        var template = Path.Combine(_folder, "t.xslt");
        var output = Path.Combine(_folder, "merged.txt");
        File.WriteAllText(template, Template);

        new SandboxedTransformer(_folder).TransformAll(template, Path.Combine(_folder, "?.xml"), null, InputTypes.Xml, output, merge: true);

        Assert.IsTrue(File.Exists(output));
        Assert.IsTrue(int.Parse(File.ReadAllText(output), global::System.Globalization.CultureInfo.InvariantCulture) >= 3);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("a.xml")]
    [DataRow("a.html")]
    [DataRow("a.htm")]
    [DataRow("a.json")]
    [DataRow("a.yaml")]
    [DataRow("a.yml")]
    [DataRow("a.cs")]
    [DataRow("a.vb")]
    [DataRow("a.binlog")]
    [DataRow("a.dacpac")]
    public void GetNavigator_KnownExtension_ReturnsFactory(string file)
    {
        Assert.IsNotNull(new SandboxedTransformer(_folder).GetNavigator(InputTypes.ByExtension, file));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetNavigator_UnknownExtension_Throws()
    {
        Assert.ThrowsExactly<NotSupportedException>(() => new SandboxedTransformer(_folder).GetNavigator(InputTypes.ByExtension, "a.zzz"));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void GetNavigator_UnknownType_Throws()
    {
        Assert.ThrowsExactly<NotSupportedException>(() => new SandboxedTransformer(_folder).GetNavigator(InputTypes.Unknown, "a.xml"));
    }
}
