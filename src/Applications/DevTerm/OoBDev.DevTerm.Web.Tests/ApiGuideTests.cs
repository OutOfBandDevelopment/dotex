using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.Documents.Guides;

namespace OoBDev.DevTerm.Web.Tests;

/// <summary>Generates the web host's API guides into <c>docs/applications/devterm/api-guide</c> from the documents the host serves (design: docs/design/GeneratedGuides).</summary>
[TestClass]
[TestCategory("Unit")]
public class ApiGuideTests
{
    public required TestContext TestContext { get; set; }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "GitVersion.yml")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new InvalidOperationException("repo root not found");
    }

    private static int FreePort()
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        return ((IPEndPoint)listener.LocalEndpoint).Port;
    }

    [TestMethod]
    public async Task Guides_FromTheServedOpenApiAndAsyncApiDocuments_ListEveryOperationAndChannel()
    {
        // Stage
        var port = FreePort();
        var built = WebHost.Build(new CliOptions { Transport = "loopback", Presenter = ["ascii"], Tui = false, Cli = true },
            new WebOptions { Urls = $"http://127.0.0.1:{port}", Token = "secret" }, []);
        await built.Hub.StartAsync();
        await built.App.StartAsync(TestContext.CancellationToken);
        string openApi, asyncApi;
        try
        {
            using var client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "secret");
            openApi = await client.GetStringAsync("/openapi/v1.json", TestContext.CancellationToken);
            asyncApi = await client.GetStringAsync("/asyncapi.json", TestContext.CancellationToken);
        }
        finally
        {
            await built.App.StopAsync(TestContext.CancellationToken);
            await built.Hub.DisposeAsync();
        }

        // Test
        var rest = ApiGuideRenderer.OpenApi(openApi, "DevTerm web host: REST API");
        var messaging = ApiGuideRenderer.AsyncApi(asyncApi, "DevTerm web host: streaming channels");
        var folder = Path.Combine(RepoRoot(), "docs", "applications", "devterm", "api-guide");
        ApiGuideRenderer.WriteIfChanged(Path.Combine(folder, "rest.md"), rest);
        ApiGuideRenderer.WriteIfChanged(Path.Combine(folder, "streaming.md"), messaging);

        // Assert
        StringAssert.Contains(rest, "### GET `/api/status`");
        StringAssert.Contains(rest, "### POST `/api/session/connect`");
        StringAssert.Contains(rest, "### DELETE `/api/connections/{id}`");
        StringAssert.Contains(messaging, "### events `/api/events`");
        StringAssert.Contains(messaging, "- `sendLine`: send on `mainSession`");
        Assert.AreEqual(rest, ApiGuideRenderer.OpenApi(openApi, "DevTerm web host: REST API"), "same document, same guide");
    }
}
