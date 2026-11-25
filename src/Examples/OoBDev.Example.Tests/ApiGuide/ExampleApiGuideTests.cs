using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Documents.Guides;
using OoBDev.TestUtilities;

namespace OoBDev.Example.Tests.ApiGuide;

/// <summary>
/// Generates the Example web API guides into <c>docs/applications/example/api-guide</c> from the OpenAPI and AsyncAPI documents the
/// built service serves in Development (design: docs/design/GeneratedGuides). The service runs as a child process on a free
/// loopback port; no model is downloaded and no external service is called because the documents are built from the endpoints.
/// </summary>
[TestClass]
[TestCategory(TestCategories.Simulate)]
public sealed class ExampleApiGuideTests
{
    public TestContext TestContext { get; set; } = null!;

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
    public async Task Guides_FromTheServedDocuments_ListTheEndpointsAndChannels()
    {
        // Stage
        var root = RepoRoot();
        var configuration = new DirectoryInfo(AppContext.BaseDirectory).Parent!.Name;
        var dll = Path.Combine(root, "src", "Examples", "OoBDev.Example.WebApi", "bin", configuration, "net10.0", "OoBDev.Example.WebApi.dll");
        if (!File.Exists(dll))
        {
            Assert.Inconclusive($"Build OoBDev.Example.WebApi first ({dll}).");
        }

        var port = FreePort();
        var startInfo = new ProcessStartInfo("dotnet", $"\"{dll}\"")
        {
            WorkingDirectory = Path.GetDirectoryName(dll)!,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        startInfo.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        startInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";

        using var process = Process.Start(startInfo)!;
        _ = process.StandardOutput.ReadToEndAsync(TestContext.CancellationToken);
        _ = process.StandardError.ReadToEndAsync(TestContext.CancellationToken);

        try
        {
        using var client = new HttpClient { BaseAddress = new Uri($"http://127.0.0.1:{port}") };
        string? openApi = null;
        var deadline = DateTime.UtcNow.AddSeconds(90);
        while (openApi is null && DateTime.UtcNow < deadline && !process.HasExited)
        {
            try
            {
                openApi = await client.GetStringAsync("/openapi/all.json", TestContext.CancellationToken);
            }
            catch (HttpRequestException)
            {
                await Task.Delay(500, TestContext.CancellationToken);
            }
        }

        Assert.IsNotNull(openApi, "The Example web API did not serve its OpenAPI document.");
        var asyncApi = await client.GetStringAsync("/asyncapi/all.json", TestContext.CancellationToken);
        Generate(root, openApi, asyncApi);
        }
        finally
        {
            process.Kill(entireProcessTree: true);
        }
    }

    private static void Generate(string root, string openApi, string asyncApi)
    {
        // The service version comes from GitVersion and changes with every commit; the guide must not.
        openApi = Regex.Replace(openApi, """("info"\s*:\s*\{[^}]*?"version"\s*:\s*")[^"]*(")""", "${1}current${2}", RegexOptions.None, TimeSpan.FromSeconds(5));


        // Test
        var rest = ApiGuideRenderer.OpenApi(openApi, "Example web API: REST endpoints");
        var messaging = ApiGuideRenderer.AsyncApi(asyncApi, "Example web API: message channels");
        var folder = Path.Combine(root, "docs", "applications", "example", "api-guide");
        ApiGuideRenderer.WriteIfChanged(Path.Combine(folder, "rest.md"), rest);
        ApiGuideRenderer.WriteIfChanged(Path.Combine(folder, "messaging.md"), messaging);

        // Assert
        StringAssert.Contains(rest, "`/api/Toolbox/hash`");
        StringAssert.Contains(rest, "`/api/VectorStore/query`");
        Assert.IsTrue(messaging.Contains("###", StringComparison.Ordinal), "the messaging guide lists a channel");
        Assert.AreEqual(rest, ApiGuideRenderer.OpenApi(openApi, "Example web API: REST endpoints"), "same document, same guide");
        Assert.IsFalse(rest.Contains("127.0.0.1", StringComparison.Ordinal), "no machine-specific port in the guide");
    }
}
