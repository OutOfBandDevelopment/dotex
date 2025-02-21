using OoBDev.Apache.Tika.Handlers;
using OoBDev.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika.Tests.Handlers;

[TestClass]
public class TikaDocxToHtmlConversionHandlerTests :
    TikaToHtmlConversionHandlerTestsBase<TikaDocxToHtmlConversionHandler>
{
    public required TestContext TestContext { get; set; }

    [TestCategory(TestCategories.DevLocal)]
    [DataTestMethod]
    [DataRow("sample1.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/html", ".html")]
    [DataRow("sample4.docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/html", ".html")]

    public async Task ConvertAsyncTest(string resourceName, string sourceType, string targetType, string ext) =>
        await ConvertAsyncTestHarness(resourceName, sourceType, targetType, ext, TestContext);

    [TestCategory(TestCategories.Unit)]
    [DataTestMethod]
    [DataRow("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/html")]
    [DataRow("application/x-tika-ooxml", "text/html")]
    [DataRow("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/xhtml")]
    [DataRow("application/x-tika-ooxml", "text/xhtml")]
    [DataRow("application/vnd.openxmlformats-officedocument.wordprocessingml.document", "text/xhtml+xml")]
    [DataRow("application/x-tika-ooxml", "text/xhtml+xml")]
    public async Task ConvertAsyncTest(string sourceContentType, string destinationContentType) =>
        await ConvertAsyncTestHarness(sourceContentType, destinationContentType);
}
