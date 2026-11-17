using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Markdown;
using OoBDev.System;
using OoBDev.System.IO;
using OoBDev.TestUtilities;

namespace OoBDev.MysticMind.Tests;

[TestClass]
public class HtmlFileMarkdownReaderTests
{
    private static ServiceProvider Build() => new ServiceCollection()
        .AddLogging(b => b.SetMinimumLevel(LogLevel.Warning))
        .TryAddProviders()
        .TryAddMysticMindServices()
        .BuildServiceProvider();

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Registration_AddsTheReaderForHtmlExtensions()
    {
        using var services = Build();

        var reader = services.GetServices<IFileMarkdownReader>().OfType<HtmlFileMarkdownReader>().Single();

        CollectionAssert.AreEquivalent(new[] { ".html", ".htm", ".xhtml" }, reader.Extensions.ToArray());
        Assert.AreEqual("reversemarkdown html", reader.Extractor);
    }

    [TestMethod, TestCategory(TestCategories.Unit)]
    public void Read_HtmlFile_ReturnsMarkdown()
    {
        using var services = Build();
        using var temp = services.GetRequiredService<ITempFileFactory>().GetTempDirectory();
        var file = Path.Combine(temp.DirectoryPath, "page.html");
        File.WriteAllText(file, "<html><body><h1>Title</h1><p>Some <b>bold</b> text</p></body></html>");

        var markdown = services.GetServices<IFileMarkdownReader>().OfType<HtmlFileMarkdownReader>().Single().Read(file, CancellationToken.None);

        StringAssert.Contains(markdown, "# Title");
        StringAssert.Contains(markdown, "**bold**");
    }
}
