using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Common.Extensions;
using OoBDev.Documents.Conversion;
using OoBDev.TestUtilities;
using System.Linq;

namespace OoBDev.Documents.Tests.Conversion;

[TestClass]
public class ConverterRegistrationTests
{
    public required TestContext TestContext { get; set; }

    private static string[] Handlers(ExternalExtensionBuilder builder)
    {
        var services = new ServiceCollection();
        services.TryCommonExternalExtensions(new ConfigurationBuilder().Build(), new(), builder);
        return [.. services
            .Where(d => d.ServiceType == typeof(IDocumentConversionHandler))
            .Select(d => d.ImplementationType?.Name ?? "")];
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryCommonExternalExtensions_Default_RegistersNeitherHtmlConverter()
    {
        var handlers = Handlers(new());
        TestContext.WriteLine("Handlers: {0}", string.Join(", ", handlers));
        CollectionAssert.DoesNotContain(handlers, "HtmlToPdfConversionHandler");
        CollectionAssert.DoesNotContain(handlers, "HtmlToDocxConversionHandler");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void TryCommonExternalExtensions_OptedIn_RegistersBothHtmlConverters()
    {
        var handlers = Handlers(new() { IncludeWkHtmlToPdf = true, IncludeHtmlToOpenXml = true });
        TestContext.WriteLine("Handlers: {0}", string.Join(", ", handlers));
        CollectionAssert.Contains(handlers, "HtmlToPdfConversionHandler");
        CollectionAssert.Contains(handlers, "HtmlToDocxConversionHandler");
    }
}
