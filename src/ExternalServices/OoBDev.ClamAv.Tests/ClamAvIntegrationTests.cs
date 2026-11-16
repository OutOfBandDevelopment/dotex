using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Models;
using OoBDev.TestUtilities;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.ClamAv.Tests;

[TestClass]
[TestCategory(TestCategories.Integration)]
public class ClamAvIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private ClamAvContentScanner Scanner() => new(Options.Create(new ClamAvOptions
    {
        Host = TestContext.GetRequiredProperty<string>("CLAMAV_HOST"),
        Port = TestContext.GetRequiredProperty<int>("CLAMAV_PORT"),
    }));

    private static ContentReference Content(string text) =>
        new() { Content = new MemoryStream(Encoding.ASCII.GetBytes(text)), ContentType = "text/plain", FileName = "file.txt" };

    [TestMethod]
    public async Task Scan_PlainText_IsClean()
    {
        var result = await Scanner().ScanAsync(Content("hello"));

        Assert.IsTrue(result.IsClean);
    }

    [TestMethod]
    public async Task Scan_EicarTestString_IsInfected()
    {
        // The standard antivirus test string; it is not malware.
        var eicar = @"X5O!P%@AP[4\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";

        var result = await Scanner().ScanAsync(Content(eicar));

        Assert.IsFalse(result.IsClean);
        StringAssert.Contains(result.Detail, "Eicar");
    }
}
