using OoBDev.Apache.Tika.Handlers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

[TestClass]
public class TikaOdtToHtmlConversionHandlerTests :
    TikaToHtmlConversionHandlerTestsBase<TikaOdtToHtmlConversionHandler>
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("sample2.odt", "application/vnd.oasis.opendocument.text", "text/html", ".html")]
    [DataRow("sample3.odt", "application/vnd.oasis.opendocument.text", "text/html", ".html")]
    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType, string ext) =>
        await ConvertAsyncTestHarness(resourceName, sourceType, targetType, ext, TestContext);

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("application/vnd.oasis.opendocument.text", "text/html")]
    [DataRow("application/vnd.oasis.opendocument.text", "text/xhtml")]
    [DataRow("application/vnd.oasis.opendocument.text", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string sourceContentType, string destinationContentType) =>
        await ConvertAsyncTestHarness(sourceContentType, destinationContentType);
}
