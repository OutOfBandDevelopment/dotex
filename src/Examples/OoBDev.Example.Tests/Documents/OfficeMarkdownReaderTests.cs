using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Common;
using OoBDev.Documents;
using OoBDev.System.IO;
using OoBDev.Documents.Markdown.Office;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Examples.Tests.Documents;

[TestClass]
public class OfficeMarkdownReaderTests
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    [DataRow("sample1.docx")]
    [DataRow("sample2.doc")]
    [DataRow("sample2.odt")]
    [DataRow("sample-2.rtf")]
    public async Task Read_OfficeSample_ReturnsMarkdownThroughTheTikaChain(string resourceName)
    {
        // Stage
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApacheTikaClientOptions:Url"] = TestContext.GetPropertyOrDefault("TIKA_URL", "http://127.0.0.1:9998"),
            })
            .Build();
        await using var provider = new ServiceCollection()
            .AddLogging()
            .TryAllCommonExtensions(config, systemBuilder: new(), aspNetBuilder: new(), jwtBuilder: new(), identityBuilder: new(), externalBuilder: new(), hostingBuilder: new())
            .BuildServiceProvider();
        using var directory = provider.GetRequiredService<ITempFileFactory>().GetTempDirectory();
        var path = Path.Combine(directory.DirectoryPath, resourceName);
        var resource = GetType().Assembly.GetManifestResourceNames().FirstOrDefault(n => n.EndsWith($".{resourceName}", StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"missing .{resourceName} resource");
        using (var source = GetType().Assembly.GetManifestResourceStream(resource)!)
        using (var target = File.Create(path))
        {
            source.CopyTo(target);
        }

        // Test
        var markdown = new OfficeFileMarkdownReader(provider.GetRequiredService<IDocumentConversion>()).Read(path, CancellationToken.None);

        // Assert
        TestContext.WriteLine($"{resourceName}: {markdown.Length} characters");
        Assert.IsFalse(string.IsNullOrWhiteSpace(markdown));
    }
}
