using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;
using OoBDev.System.Text.Templating;
using OoBDev.TestUtilities;
using OoBDev.WebApi.Services;
using System.IO;
using System.Linq;

namespace OoBDev.Example.WebApi.Tests;

[TestClass]
public class TemplateCatalogTests
{
    private static readonly IFileType[] FileTypes =
    [
        new FileType { Extension = ".hbs", ContentType = "text/x-handlebars-template", IsTemplateType = true },
        new FileType { Extension = ".html", ContentType = "text/html" },
        new FileType { Extension = ".txt", ContentType = "text/plain" },
    ];

    private static readonly TempFileFactory Temp = new(new ServiceCollection().BuildServiceProvider());

    private static TemplateCatalog Catalog(string folder)
        => new(Options.Create(new FileTemplatingOptions { TemplatePath = folder }), FileTypes);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_TemplateFiles_SplitsNameTargetAndEngineAndSkipsOthers()
    {
        using var dir = Temp.GetTempDirectory();
        File.WriteAllText(Path.Combine(dir.DirectoryPath, "b-mail.html.hbs"), "<p>{{x}}</p>");
        File.WriteAllText(Path.Combine(dir.DirectoryPath, "a-note.txt.hbs"), "{{x}}");
        File.WriteAllText(Path.Combine(dir.DirectoryPath, "readme.md"), "not a template");

        var list = Catalog(dir.DirectoryPath).List();

        CollectionAssert.AreEqual(new[] { "a-note", "b-mail" }, list.Select(t => t.Name).ToArray());
        Assert.AreEqual(".html", list[1].TargetExtension);
        Assert.AreEqual("text/x-handlebars-template", list[1].TemplateContentType);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void List_FolderMissing_ReturnsEmpty()
        => Assert.AreEqual(0, Catalog(Path.Combine(Path.GetTempPath(), "missing-template-folder")).List().Count);

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Read_ListedFile_ReturnsText()
    {
        using var dir = Temp.GetTempDirectory();
        File.WriteAllText(Path.Combine(dir.DirectoryPath, "a-note.txt.hbs"), "Hello {{name}}");

        var detail = Catalog(dir.DirectoryPath).Read("a-note.txt.hbs");

        Assert.IsNotNull(detail);
        Assert.AreEqual("Hello {{name}}", detail.Text);
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Read_PathOutsideTheList_ReturnsNull()
    {
        using var dir = Temp.GetTempDirectory();
        File.WriteAllText(Path.Combine(dir.DirectoryPath, "a-note.txt.hbs"), "x");

        Assert.IsNull(Catalog(dir.DirectoryPath).Read("../a-note.txt.hbs"));
    }
}
