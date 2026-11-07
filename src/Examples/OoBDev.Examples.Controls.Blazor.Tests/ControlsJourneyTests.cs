using System;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Playwright;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Examples.Controls.Blazor.Components;
using OoBDev.Examples.TestUtilities;

namespace OoBDev.Examples.Controls.Blazor.Tests;

/// <summary>The user journey in a real browser against the running example: click the controls, watch the preview and the bound-model table change, with a screenshot of each step for the controls guide. Runs Microsoft Edge through Playwright and is inconclusive when Edge cannot be launched.</summary>
[TestCategory("DevLocal")]
[TestClass]
public sealed class ControlsJourneyTests
{
    [TestMethod]
    public async Task Journey_ClickAndType_PreviewAndModelFollowAndBothStepsAreCaptured()
    {
        // Stage
        var port = FreePort();
        var url = $"http://127.0.0.1:{port}";
        using var host = StartExample(url);
        await WaitUntilServingAsync(url, host);
        using var playwright = await Playwright.CreateAsync();
        IBrowser browser;
        try
        {
            browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Channel = "msedge", Headless = true });
        }
        catch (PlaywrightException ex)
        {
            Assert.Inconclusive("Microsoft Edge could not be launched by Playwright: " + ex.Message);
            return;
        }

        await using (browser)
        {
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 1000, Height = 760 }, ColorScheme = ColorScheme.Light });
            await page.GotoAsync(url);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            await Assertions.Expect(page.Locator("[data-preview=name]")).ToHaveTextAsync("sample");
            await Capture(page, "blazor-1-initial.png");

            // Test
            await ClickUntilCircuitAnswersAsync(page);
            await Assertions.Expect(page.Locator("[data-readout=Mode] td")).ToHaveTextAsync("High");
            await page.Locator("[data-id=ShowAdvanced] input[type=checkbox]").CheckAsync();
            await Assertions.Expect(page.Locator("[data-id=Notes]")).ToBeVisibleAsync();
            await page.Locator("[data-id=Notes] input[type=text]").FillAsync("bench 2");
            await page.Locator("[data-id=Notes] input[type=text]").PressAsync("Tab");
            await page.Locator("[data-id=Enabled] input[type=checkbox]").UncheckAsync();

            // Assert
            await Assertions.Expect(page.Locator("[data-preview=notes]")).ToHaveTextAsync("bench 2");
            await Assertions.Expect(page.Locator("[data-preview=state]")).ToHaveTextAsync("disabled");
            await Assertions.Expect(page.Locator("[data-preview=mode]")).ToHaveClassAsync("badge mode-high");
            await Capture(page, "blazor-2-changed.png");
        }
    }

    /// <summary>The server-rendered page is visible before the SignalR circuit connects, and an event sent before that is lost, so click until the first change comes back.</summary>
    private static async Task ClickUntilCircuitAnswersAsync(IPage page)
    {
        var high = page.Locator("[data-id=Mode] input[type=radio]").Nth(2);
        var readout = page.Locator("[data-readout=Mode] td");
        for (var attempt = 0; attempt < 20; attempt++)
        {
            await high.ClickAsync();
            await Task.Delay(250);
            if (string.Equals(await readout.InnerTextAsync(), "High", StringComparison.Ordinal))
            {
                return;
            }
        }
    }

    private static async Task Capture(IPage page, string imageName)
    {
        using var temp = new OoBDev.System.IO.TempFileHandle();
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = temp.FilePath, Type = ScreenshotType.Png });
        ExampleGuide.Sync(temp.FilePath, imageName);
    }

    private static ExampleProcess StartExample(string url)
    {
        var assembly = typeof(ControlsPage).Assembly.Location;
        var start = new ProcessStartInfo("dotnet", $"\"{assembly}\" --urls {url}")
        {
            WorkingDirectory = global::System.IO.Path.GetDirectoryName(assembly)!,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        return new ExampleProcess(Process.Start(start) ?? throw new InvalidOperationException("The example did not start."));
    }

    private static async Task WaitUntilServingAsync(string url, ExampleProcess host)
    {
        using var http = new HttpClient();
        for (var attempt = 0; attempt < 120; attempt++)
        {
            if (host.Process.HasExited)
            {
                Assert.Fail("The example exited: " + await host.Process.StandardError.ReadToEndAsync());
            }

            try
            {
                using var response = await http.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // Not listening yet.
            }

            await Task.Delay(250);
        }

        Assert.Fail("The example did not answer on " + url);
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    private sealed class ExampleProcess(Process process) : IDisposable
    {
        public Process Process { get; } = process;

        public void Dispose()
        {
            if (!Process.HasExited)
            {
                Process.Kill(entireProcessTree: true);
            }

            Process.Dispose();
        }
    }
}
