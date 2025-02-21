using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Markdig.Tests;

[TestClass]
public class MarkdownToHtmlConversionHandlerTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("HelloWorld.md", "text/markdown", "text/html")]
    [DataRow("HelloWorld.md", "text/plain", "text/html")]
    [DataRow("HelloWorld.md", "text/x-markdown", "text/html")]
    [DataRow("HelloWorld.md", "text/markdown", "text/xhtml")]
    [DataRow("HelloWorld.md", "text/plain", "text/xhtml")]
    [DataRow("HelloWorld.md", "text/x-markdown", "text/xhtml")]
    [DataRow("HelloWorld.md", "text/markdown", "text/xhtml+xml")]
    [DataRow("HelloWorld.md", "text/plain", "text/xhtml+xml")]
    [DataRow("HelloWorld.md", "text/x-markdown", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAddMarkdigServices()
            .BuildServiceProvider()
            ;

        var converter = ActivatorUtilities.CreateInstance<MarkdownToHtmlConversionHandler>(serviceProvider);

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

        TestContext.AddResult(pdfStream, fileName: Path.ChangeExtension(resourceName, ".html"));

        Assert.IsTrue(pdfStream.Length > 0);
    }
}
