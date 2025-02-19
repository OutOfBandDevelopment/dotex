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
    [TestCategory(TestCategories.DevLocal)]
    public async Task ConvertAsyncTest()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApacheTikaClientOptions:Url"] = "http://127.0.0.1:9998",
            })
            .Build()
            ;

        var services = new ServiceCollection()
            .AddLogging()
            .TryCommonExtensions(config, new())
            .TryCommonExternalExtensions(config, new(), new())
            .BuildServiceProvider();

        var documentConversion = services.GetRequiredService<IDocumentConversion>();
        var fileTypes = services.GetServices<IDocumentType>();

        var sourceFile = @"C:\Users\MWhited\OneDrive - OoBDev Group, LLC\Desktop\Matthew Whited.txt";
        var sourceFileType = fileTypes
            .FirstOrDefault(ft => ft.FileExtensions.Any(e => string.Equals(e, Path.GetExtension(sourceFile), StringComparison.OrdinalIgnoreCase)))
            ?.ContentTypes[0]
            ?? throw new ApplicationException($"unable to identify source file type")
            ;

        foreach (var ext in fileTypes.SelectMany(e => e.FileExtensions).Distinct().OrderBy(s => s))
        {
            try
            {
                await using var source = File.OpenRead(sourceFile);
                var targetFile = Path.ChangeExtension(sourceFile, ext);

                if (Path.GetFullPath(sourceFile) == Path.GetFullPath(targetFile)) continue;

                //this.TestContext.WriteLine(ext);
                var targetFileType = fileTypes
                    .FirstOrDefault(ft => ft.FileExtensions.Any(e => string.Equals(e, Path.GetExtension(targetFile), StringComparison.OrdinalIgnoreCase)))
                    ?.ContentTypes[0]
                    ?? throw new ApplicationException($"unable to identify source file type")
                    ;

                using var target = new MemoryStream();
                if (await documentConversion.ConvertAsync(source, sourceFileType, target, targetFileType))
                {
                    await using var targetOut = File.Create(targetFile);
                    this.TestContext.WriteLine($"out({ext}):{targetFile}");
                    await target.CopyToAsync(targetOut);
                }
                else
                {
                    //  this.TestContext.WriteLine($"nope({ext}):{targetFile}");
                }
            }
            catch (Exception ex)
            {
                this.TestContext.WriteLine($"ERR({ext}):{ex.Message}");
            }
        }
    }
}
