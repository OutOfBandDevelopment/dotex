using OoBDev.Apache.Tika.Handlers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

[TestClass]
public class TikaDocToHtmlConversionHandlerTests :
    TikaToHtmlConversionHandlerTestsBase<TikaDocToHtmlConversionHandler>
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("sample2.doc", "application/msword", "text/html", ".html")]
    public async Task ExternalConvertAsyncTest(string resourceName, string sourceType, string targetType, string ext) =>
        await ConvertAsyncTestHarness(resourceName, sourceType, targetType, ext, TestContext);

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("application/msword", "text/html")]
    [DataRow("application/x-tika-msoffice", "text/html")]
    [DataRow("application/msword", "text/xhtml")]
    [DataRow("application/x-tika-msoffice", "text/xhtml")]
    [DataRow("application/msword", "text/xhtml+xml")]
    [DataRow("application/x-tika-msoffice", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string sourceContentType, string destinationContentType) =>
        await ConvertAsyncTestHarness(sourceContentType, destinationContentType);
}
