using OoBDev.Common;
using OoBDev.Documents;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Examples.Tests.Documents;

[TestClass]
public class IDocumentConversionTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("HelloWorld.html", "text/html", "text/x-markdown", ".md")]
    //TODO: these require more configuration on linux at this time. 
    //[DataRow("HelloWorld.html", "text/html", "application/pdf", ".pdf")]
    //[DataRow("HelloWorld.txt", "text/plain", "application/pdf", ".pdf")]
    [DataRow("HelloWorld.txt", "text/plain", "text/html", ".html")]
    [DataRow("HelloWorld.md", "text/markdown", "text/html", ".html")]
    [DataRow("HelloWorld.txt", "text/plain", "text/plain", ".txt")]
    public Task ConvertAsyncTest(string resourceName, string sourceType, string targetType, string extension) =>
        InternalConvertAsyncTest(resourceName, sourceType, targetType, extension, false);

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("HelloWorld.txt", "unknown/unknown", "text/plain", ".txt")]
    [DataRow("sample1.docx", "unknown/unknown", "application/pdf", ".pdf")]
    [DataRow("sample1.docx", "unknown/unknown", "text/markdown", ".md")]
    [DataRow("sample1.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/plain", ".txt")]
    [DataRow("sample1.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/html", ".html")]
    [DataRow("sample1.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "application/pdf", ".pdf")]

    [DataRow("sample2.doc", "application/msword", "application/pdf", ".pdf")]
    [DataRow("sample2.odt", "application/vnd.oasis.opendocument.text", "application/pdf", ".pdf")]
    [DataRow("sample3.odt", "application/vnd.oasis.opendocument.text", "application/pdf", ".pdf")]

    [DataRow("sample-2.rtf", "application/rtf", "application/pdf", ".pdf")]
    [DataRow("accessible_epub_3.epub", "application/epub+zip", "application/pdf", ".pdf")]
    [DataRow("accessible_epub_3.epub", "unknown/unknown", "application/pdf", ".pdf")]
    public Task ExternalConvertAsyncTest(string resourceName, string sourceType, string targetType, string extension) =>
        InternalConvertAsyncTest(resourceName, sourceType, targetType, extension, true);

    private async Task InternalConvertAsyncTest(string resourceName, string sourceType, string targetType, string extension, bool includeExternal)
    {
        var configBuilder = new ConfigurationBuilder();

        if (includeExternal)
        {
            configBuilder.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    { "ApacheTikaClientOptions:Url","http://127.0.0.1:9998"}

                });
        }
        var config = configBuilder.Build();
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder
                .AddConsole()
                .AddDebug()
                .SetMinimumLevel(LogLevel.Trace)
                )
            .TryAllCommonExtensions(config,
                systemBuilder: new(),
                aspNetBuilder: new(),
                jwtBuilder: new(),
                identityBuilder: new(),
                externalBuilder: new(),
                hostingBuilder: new()
                )
            .BuildServiceProvider()
            ;

        var converter = serviceProvider.GetRequiredService<IDocumentConversion>();

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

        TestContext.AddResult(pdfStream, fileName: Path.ChangeExtension(resourceName, extension));

        Assert.IsTrue(pdfStream.Length > 0);
    }
}
