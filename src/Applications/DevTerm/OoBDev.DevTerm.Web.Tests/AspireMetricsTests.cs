using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;
using System.Net.Sockets;
using Microsoft.Playwright;
using OoBDev.TestUtilities;
using OoBDev.TestUtilities.Containers;
namespace OoBDev.DevTerm.Web.Tests;

/// <summary>
/// Reads the Aspire dashboard's Metrics page for the <c>devterm</c> resource in a real browser (its Blazor circuit needs one).
/// Starts the <c>oobd-test-otel</c> container when it is down; needs at least one <c>--otlp</c> run; skips otherwise.
/// </summary>
[TestClass]
[TestCategory("DevLocal")]
[TestCategory(TestCategories.Web)]
public sealed class AspireMetricsTests
{
    public required TestContext TestContext { get; set; }

    [IntegrationTestMethod("oobd-test-otel")]
    public async Task MetricsPage_ListsTheDevTermInstruments()
    {
        using (var probe = new TcpClient())
        {
            try
            {
                await probe.ConnectAsync("127.0.0.1", 18888);
            }
            catch (SocketException)
            {
                Assert.Inconclusive("The Aspire dashboard is not running on 127.0.0.1:18888.");
            }
        }

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("http://127.0.0.1:18888/metrics/resource/devterm");
        await page.WaitForTimeoutAsync(5000);
        var text = await page.InnerTextAsync("body");
        File.WriteAllText(WebTestTemp.File("aspire-metrics.txt"), text);
        TestContext.WriteLine("Aspire metrics page text: {0}", text.ReplaceLineEndings(" | "));
        if (text.Contains("Select a resource to view metrics", global::System.StringComparison.Ordinal))
        {
            Assert.Inconclusive("The dashboard has no devterm resource yet: run DevTerm once with --otlp so it reports metrics.");
        }
        StringAssert.Contains(text, "devterm.bytes.sent");
    }
}
