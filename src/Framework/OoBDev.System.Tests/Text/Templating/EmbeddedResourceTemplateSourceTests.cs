using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.Text.Templating;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OoBDev.System.Tests.Text.Templating;

[TestClass]
public class EmbeddedResourceTemplateSourceTests
{
    public required TestContext TestContext { get; set; }

    private static readonly IFileType[] FileTypes =
    [
        new FileType { Extension = ".tpl", ContentType = "text/x-test-template", IsTemplateType = true },
        new FileType { Extension = ".md", ContentType = "text/markdown", IsTemplateType = false },
    ];

    private static EmbeddedResourceTemplateSource Create(IEnumerable<IFileType>? types = null) =>
        new(typeof(EmbeddedResourceTemplateSourceTests).Assembly, "TestTemplates.", types ?? FileTypes);

    [TestMethod]
    [TestCategory("Unit")]
    public void Get_EmbeddedTemplate_OpensResourceWithTargetTypeAndLowPrecedence()
    {
        var found = Create().Get("hello").ToList();
        TestContext.WriteLine($"Found {found.Count}: {string.Join(", ", found.Select(f => f.TemplateReference))}");

        Assert.AreEqual(1, found.Count);
        Assert.AreEqual(".md", found[0].TargetFileExtension);
        Assert.IsTrue(found[0].Priority > new FileTemplatingOptions().Priority, "a file on disk must win");
        using var reader = new StreamReader(found[0].OpenTemplate(found[0]));
        Assert.AreEqual("Hello embedded", reader.ReadToEnd().Trim());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void Get_UnknownTemplate_ReturnsNothing()
    {
        Assert.AreEqual(0, Create().Get("missing").Count());
    }
}
