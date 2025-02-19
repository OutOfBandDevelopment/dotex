using OoBDev.Apache.Tika.Handlers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

[TestClass]
public class TikaEpubToHtmlConversionHandlerTests :
    TikaToHtmlConversionHandlerTestsBase<TikaEpubToHtmlConversionHandler>
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("accessible_epub_3.epub", "application/epub+zip", "text/html", ".html")]
    [DataRow("childrens-literature.epub", "application/epub+zip", "text/html", ".html")]
    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType, string ext) =>
        await ConvertAsyncTestHarness(resourceName, sourceType, targetType, ext, TestContext);

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("application/epub+zip", "text/html")]
    [DataRow("application/epub+zip", "text/xhtml")]
    [DataRow("application/epub+zip", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string sourceContentType, string destinationContentType) =>
        await ConvertAsyncTestHarness(sourceContentType, destinationContentType);
}
