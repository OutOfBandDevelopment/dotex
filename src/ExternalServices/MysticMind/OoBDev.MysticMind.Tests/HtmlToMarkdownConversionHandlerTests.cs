using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.MysticMind.Tests;

[TestClass]
public class HtmlToMarkdownConversionHandlerTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("HelloWorld.html", "text/html", "text/markdown")]
    [DataRow("HelloWorld.html", "text/html", "text/plain")]
    [DataRow("HelloWorld.html", "text/html", "text/x-markdown")]
    [DataRow("HelloWorld.html", "text/xhtml", "text/markdown")]
    [DataRow("HelloWorld.html", "text/xhtml", "text/plain")]
    [DataRow("HelloWorld.html", "text/xhtml", "text/x-markdown")]
    [DataRow("HelloWorld.html", "text/xhtml+xml", "text/markdown")]
    [DataRow("HelloWorld.html", "text/xhtml+xml", "text/plain")]
    [DataRow("HelloWorld.html", "text/xhtml+xml", "text/x-markdown")]
    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAddMysticMindServices()
            .BuildServiceProvider()
            ;

        var converter = ActivatorUtilities.CreateInstance<HtmlToMarkdownConversionHandler>(serviceProvider);

        var resource = GetType().Assembly.GetManifestResourceNames().FirstOrDefault(l => l.EndsWith($".{resourceName}"))
            ?? throw new ApplicationException($"missing .{resourceName} resource");
        using var stream = GetType().Assembly.GetManifestResourceStream(resource)
            ?? throw new ApplicationException($"missing .{resourceName} resource");

        using var htmlStream = new MemoryStream();
        stream.CopyTo(htmlStream);
        htmlStream.Position = 0;
        using var pdfStream = new MemoryStream();

        await converter.ConvertAsync(htmlStream, sourceType, pdfStream, targetType);
        pdfStream.Position = 0;

        TestContext.WriteLine($"Content Length: {pdfStream.Length}");

        TestContext.AddResult(pdfStream, fileName: Path.ChangeExtension(resourceName, ".md"));

        Assert.IsTrue(pdfStream.Length > 0);
    }
}
