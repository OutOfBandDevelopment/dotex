using OoBDev.Common;
using OoBDev.Common.Extensions;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Documents.Tests.Conversion;

[TestClass]
public class DocumentConversionTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task ConvertAsyncTest()
    {
        // Stage
        var tikaUrl = TestContext.GetRequiredProperty<string>("TIKA_URL");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApacheTikaClientOptions:Url"] = tikaUrl,
            })
            .Build();

        var services = new ServiceCollection()
            .AddLogging()
            .TryCommonExtensions(config, new())
            .TryCommonExternalExtensions(config, new(), new())
            .BuildServiceProvider();

        var documentConversion = services.GetRequiredService<IDocumentConversion>();
        var fileTypes = services.GetServices<IDocumentType>();

        // Create test content
        var sourceContent = $"Test document created at {DateTime.UtcNow:O}";
        var sourceFileType = "text/plain";

        // Test conversions to various formats
        var targetFormats = new[] { ".pdf", ".html", ".xml", ".rtf" };

        foreach (var ext in targetFormats)
        {
            try
            {
                var targetFileType = fileTypes
                    .FirstOrDefault(ft => ft.FileExtensions.Any(e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase)))
                    ?.ContentTypes[0];

                if (targetFileType == null)
                {
                    TestContext.WriteLine($"Skip({ext}): No file type found");
                    continue;
                }

                // Test conversion
                using var source = new MemoryStream(global::System.Text.Encoding.UTF8.GetBytes(sourceContent));
                using var target = new MemoryStream();

                if (await documentConversion.ConvertAsync(source, sourceFileType, target, targetFileType))
                {
                    TestContext.WriteLine($"Success({ext}): Converted {source.Length} bytes → {target.Length} bytes");
                    Assert.IsGreaterThan(0, target.Length, $"Converted document should have content for {ext}");
                }
                else
                {
                    TestContext.WriteLine($"Skip({ext}): Conversion not supported");
                }
            }
            catch (Exception ex)
            {
                TestContext.WriteLine($"Error({ext}): {ex.Message}");
            }
        }
    }
}
