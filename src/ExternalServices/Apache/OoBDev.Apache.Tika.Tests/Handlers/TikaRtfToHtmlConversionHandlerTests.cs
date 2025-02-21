using OoBDev.Apache.Tika.Handlers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

[TestClass]
public class TikaRtfToHtmlConversionHandlerTests :
    TikaToHtmlConversionHandlerTestsBase<TikaRtfToHtmlConversionHandler>
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("sample-1.rtf", "application/rtf", "text/html", ".html")]
    [DataRow("sample-2.rtf", "application/rtf", "text/html", ".html")]
    [DataRow("sample-3.rtf", "application/rtf", "text/html", ".html")]
    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType, string ext) =>
        await ConvertAsyncTestHarness(resourceName, sourceType, targetType, ext, TestContext);

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("application/rtf", "text/html")]
    [DataRow("application/rtf", "text/xhtml")]
    [DataRow("application/rtf", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string sourceContentType, string destinationContentType) =>
        await ConvertAsyncTestHarness(sourceContentType, destinationContentType);
}
