using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Apache.Tika.Handlers;
using OoBDev.TestUtilities;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

[TestClass]
public class TikaPptToHtmlConversionHandlerTests :
    TikaToHtmlConversionHandlerTestsBase<TikaPptToHtmlConversionHandler>
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.Unit)]
    [TestMethod]
    [DataRow("application/vnd.ms-powerpoint", "text/html")]
    [DataRow("application/vnd.ms-powerpoint", "text/xhtml")]
    [DataRow("application/vnd.ms-powerpoint", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string sourceContentType, string destinationContentType) =>
        await ConvertAsyncTestHarness(sourceContentType, destinationContentType);
}
