using OoBDev.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.WkHtmlToPdf.Tests;

[TestClass]
public class HtmlToPdfConversionHandlerTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("HelloWorld.html", "text/html", "application/pdf")]
    [DataRow("HelloWorld.html", "text/xhtml", "application/pdf")]
    [DataRow("HelloWorld.html", "text/xhtml+xml", "application/pdf")]
    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType)
    {
        if (Environment.OSVersion.Platform != PlatformID.Win32NT)
        {
            //TODO: fix this better
            Assert.Inconclusive();
            return;
        }

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAddWkHtmlToPdfServices()
            .BuildServiceProvider()
            ;

        var converter = ActivatorUtilities.CreateInstance<HtmlToPdfConversionHandler>(serviceProvider);

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

        TestContext.AddResult(pdfStream, fileName: Path.ChangeExtension(resourceName, ".pdf"));

        Assert.IsTrue(pdfStream.Length > 0);
    }
}
