using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using OoBDev.System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Cgi.Tests;

// Fixtures: .cmd batch files on Windows, .sh scripts (run by /bin/sh) on Linux, see scripts/ci/run-linux-tests.sh.
[TestClass]
#pragma warning disable CA1001 // disposed in TestCleanup
public class CgiMiddlewareTests
{
    private static string Ext => OperatingSystem.IsWindows() ? ".cmd" : ".sh";

    private WebApplication? _app;
    private HttpClient? _client;

    [TestCleanup]
    public async Task Cleanup()
    {
        _client?.Dispose();
        if (_app != null)
            await _app.DisposeAsync();
    }

    private async Task StartAsync(Dictionary<string, string?>? overrides = null, bool withUserHeaders = false, Action<IServiceCollection>? configure = null)
    {
        var settings = new Dictionary<string, string?>
        {
            ["OoBDev:Cgi:ScriptRoot"] = Path.Combine(AppContext.BaseDirectory, "Fixtures"),
            ["OoBDev:Cgi:AllowBatchFiles"] = "true",
            [$"OoBDev:Cgi:Handlers:{Ext}"] = OperatingSystem.IsWindows() ? "" : "/bin/sh",
            ["OoBDev:Cgi:AllowAnonymous"] = "true",
        };
        foreach (var (key, value) in overrides ?? [])
            settings[key] = value;

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(settings);
        builder.Services.TryAddCgiServices(builder.Configuration, "OoBDev:Cgi");
        configure?.Invoke(builder.Services);

        _app = builder.Build();
        if (withUserHeaders)
        {
            // Stage: stand-in for real authentication; X-Test-Rights lists the rights, X-Test-User signs in
            _app.Use((context, next) =>
            {
                if (context.Request.Headers.TryGetValue("X-Test-User", out var user))
                {
                    var claims = context.Request.Headers["X-Test-Rights"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(r => new Claim(CommonClaims.ApplicationRight, r)).Append(new Claim(ClaimTypes.Name, user.ToString()));
                    context.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "test"));
                }
                return next(context);
            });
        }
        _app.UseCgi();
        await _app.StartAsync();

        var address = _app.Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!.Addresses.First();
        _client = new HttpClient { BaseAddress = new Uri(address) };
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_EnvScript_ReceivesMetaVariablesAndNoProxy()
    {
        // Stage
        await StartAsync();
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/cgi-bin/env{Ext}/extra/path?a=1&b=2");
        request.Headers.Add("Proxy", "http://evil");

        // Test
        using var response = await _client!.SendAsync(request);
        var text = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(text, "METHOD=GET");
        StringAssert.Contains(text, "QUERY_STRING=a=1&b=2");
        StringAssert.Contains(text, "PATHINFO=/extra/path");
        StringAssert.Contains(text, "TRANSLATED=");
        StringAssert.Contains(text, "extra" + global::System.IO.Path.DirectorySeparatorChar + "path");
        StringAssert.Contains(text, $"SCRIPT=/cgi-bin/env{Ext}");
        StringAssert.Contains(text, "GATEWAY=CGI/1.1");
        StringAssert.Contains(text, "PROXY=" + Environment.NewLine);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_StatusScript_UsesStatusAndHeaders()
    {
        // Stage
        await StartAsync();

        // Test
        using var response = await _client!.GetAsync($"/cgi-bin/status{Ext}");

        // Assert
        Assert.AreEqual((HttpStatusCode)418, response.StatusCode);
        Assert.IsTrue(response.Headers.Contains("X-Fixture"));
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "short and stout");
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_NoContentType_ReturnsBadGateway()
    {
        await StartAsync();
        using var response = await _client!.GetAsync($"/cgi-bin/no-content-type{Ext}");
        Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_ExitWithoutOutput_ReturnsServerError()
    {
        await StartAsync();
        using var response = await _client!.GetAsync($"/cgi-bin/exit-1{Ext}");
        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Post_Body_IsEchoedByScript()
    {
        // Stage
        await StartAsync();
        using var content = new StringContent("hello body", Encoding.UTF8, "text/plain");

        // Test
        using var response = await _client!.PostAsync($"/cgi-bin/echo{Ext}", content);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "hello body");
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_LocationScript_ReturnsRedirectWithLocation()
    {
        // Stage
        await StartAsync();
        using var client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = _client!.BaseAddress };

        // Test
        using var response = await client.GetAsync($"/cgi-bin/location{Ext}");

        // Assert
        Assert.AreEqual(HttpStatusCode.Found, response.StatusCode);
        Assert.AreEqual("/elsewhere", response.Headers.Location?.ToString());
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_NoBlankLineAfterHeaders_ReturnsBadGateway()
    {
        // Stage
        await StartAsync();

        // Test
        using var response = await _client!.GetAsync($"/cgi-bin/no-blank-line{Ext}");

        // Assert
        Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_QueryWithShellMetacharacters_ReachesScriptAsData()
    {
        // Stage
        await StartAsync();

        // Test
        using var response = await _client!.GetAsync($"/cgi-bin/env{Ext}?x=%24(whoami)%3Bdir");

        // Assert
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "QUERY_STRING=x=%24(whoami)%3Bdir");
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_MissingScript_ReturnsNotFound()
    {
        await StartAsync();
        using var response = await _client!.GetAsync($"/cgi-bin/nope{Ext}");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_UnlistedExtension_ReturnsForbidden()
    {
        await StartAsync();
        using var response = await _client!.GetAsync("/cgi-bin/env.txt");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_BatchFileWithoutOptIn_ReturnsForbidden()
    {
        if (!OperatingSystem.IsWindows())
            Assert.Inconclusive("Batch files are Windows only.");
        await StartAsync(new() { ["OoBDev:Cgi:AllowBatchFiles"] = "false" });
        using var response = await _client!.GetAsync($"/cgi-bin/env{Ext}");
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    [DataRow("/cgi-bin/..%2fenv")]
    [DataRow("/cgi-bin/%2e%2e/env")]
    [DataRow("/cgi-bin/sub%5c..%5cenv")]
    public async Task Get_Traversal_IsNotServed(string path)
    {
        await StartAsync();
        using var response = await _client!.GetAsync(path + Ext);
        Assert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Post_BodyOverLimit_ReturnsContentTooLarge()
    {
        await StartAsync(new() { ["OoBDev:Cgi:MaxRequestBodyBytes"] = "10" });
        using var content = new StringContent(new string('x', 100), Encoding.UTF8, "text/plain");
        using var response = await _client!.PostAsync($"/cgi-bin/env{Ext}", content);
        Assert.AreEqual(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_OutsidePrefix_FallsThrough()
    {
        await StartAsync();
        using var response = await _client!.GetAsync("/other");
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_AnonymousByDefault_ReturnsUnauthorized()
    {
        await StartAsync(new() { ["OoBDev:Cgi:AllowAnonymous"] = "false" }, withUserHeaders: true);
        using var response = await _client!.GetAsync($"/cgi-bin/env{Ext}");
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    [DataRow("", HttpStatusCode.Forbidden)]
    [DataRow("other", HttpStatusCode.Forbidden)]
    [DataRow("other,cgi.run", HttpStatusCode.OK)]
    public async Task Get_RequiredRights_NeedsOneMatch(string rights, HttpStatusCode expected)
    {
        await StartAsync(new() { ["OoBDev:Cgi:AllowAnonymous"] = "false", ["OoBDev:Cgi:RequiredRights:0"] = "cgi.run" }, withUserHeaders: true);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/cgi-bin/env{Ext}");
        request.Headers.Add("X-Test-User", "alice");
        request.Headers.Add("X-Test-Rights", rights);
        using var response = await _client!.SendAsync(request);
        Assert.AreEqual(expected, response.StatusCode);
    }

    private sealed class DenyingChecker : IApplicationRightChecker
    {
        public RightCheckResult Check(ClaimsPrincipal principal, IReadOnlyCollection<string> requiredRights) => RightCheckResult.Forbidden;
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_RegisteredChecker_DecidesTheOutcome()
    {
        // Stage
        await StartAsync(new() { ["OoBDev:Cgi:AllowAnonymous"] = "false", ["OoBDev:Cgi:RequiredRights:0"] = "cgi.run" }, withUserHeaders: true, configure: s => s.AddSingleton<IApplicationRightChecker, DenyingChecker>());
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/cgi-bin/env{Ext}");
        request.Headers.Add("X-Test-User", "alice");
        request.Headers.Add("X-Test-Rights", "cgi.run");

        // Test
        using var response = await _client!.SendAsync(request);

        // Assert
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_ScriptRights_AreCheckedPerScript()
    {
        await StartAsync(new() { ["OoBDev:Cgi:AllowAnonymous"] = "false", [$"OoBDev:Cgi:ScriptRights:status{Ext}:0"] = "teapot" }, withUserHeaders: true);
        using var allowed = new HttpRequestMessage(HttpMethod.Get, $"/cgi-bin/env{Ext}");
        allowed.Headers.Add("X-Test-User", "alice");
        using var denied = new HttpRequestMessage(HttpMethod.Get, $"/cgi-bin/status{Ext}");
        denied.Headers.Add("X-Test-User", "alice");

        using var first = await _client!.SendAsync(allowed);
        using var second = await _client.SendAsync(denied);

        Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
        Assert.AreEqual(HttpStatusCode.Forbidden, second.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_ProgramOverTimeout_ReturnsGatewayTimeout()
    {
        await StartAsync(new() { ["OoBDev:Cgi:Timeout"] = "00:00:01" });
        using var response = await _client!.GetAsync($"/cgi-bin/sleep-first{Ext}");
        Assert.AreEqual(HttpStatusCode.GatewayTimeout, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_AllSlotsBusy_ReturnsServiceUnavailableWithRetryAfter()
    {
        // Stage
        await StartAsync(new() { ["OoBDev:Cgi:MaxConcurrentProcesses"] = "1" });
        var first = _client!.GetAsync($"/cgi-bin/sleep{Ext}");
        await Task.Delay(TimeSpan.FromMilliseconds(750));

        // Test
        using var second = await _client.GetAsync($"/cgi-bin/env{Ext}");
        using var firstResponse = await first;

        // Assert
        Assert.AreEqual(HttpStatusCode.ServiceUnavailable, second.StatusCode);
        Assert.IsTrue(second.Headers.Contains("Retry-After"));
        Assert.AreEqual(HttpStatusCode.OK, firstResponse.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_ResponseOverLimit_ReturnsBadGateway()
    {
        await StartAsync(new() { ["OoBDev:Cgi:MaxResponseBytes"] = "10" });
        using var response = await _client!.GetAsync($"/cgi-bin/env{Ext}");
        Assert.AreEqual(HttpStatusCode.BadGateway, response.StatusCode);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_StderrFlood_StillAnswersAndKeepsLimit()
    {
        // Stage
        var logs = new CapturingLoggerProvider();
        await StartAsync(new() { ["OoBDev:Cgi:MaxStderrBytes"] = "100" }, configure: s => s.AddLogging(b => b.AddProvider(logs)));

        // Test
        using var response = await _client!.GetAsync($"/cgi-bin/stderr-flood{Ext}");

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        StringAssert.Contains(await response.Content.ReadAsStringAsync(), "ok");
        var entry = logs.Entries.Single(e => e.Message.StartsWith("CGI stderr:", StringComparison.Ordinal));
        Assert.AreEqual(LogLevel.Warning, entry.Level);
        Assert.IsTrue(entry.Message.Length <= "CGI stderr: ".Length + 100);
    }

    [TestMethod]
    [TestCategory("Simulate")]
    public async Task Get_ClientAbort_FreesTheProcessSlot()
    {
        // Stage
        await StartAsync(new() { ["OoBDev:Cgi:MaxConcurrentProcesses"] = "1" });
        using var cancel = new CancellationTokenSource();
        var aborted = _client!.GetAsync($"/cgi-bin/sleep{Ext}", HttpCompletionOption.ResponseContentRead, cancel.Token);
        await Task.Delay(TimeSpan.FromMilliseconds(750));

        // Test
        await cancel.CancelAsync();
        await Assert.ThrowsAsync<TaskCanceledException>(() => aborted);
        var status = HttpStatusCode.ServiceUnavailable;
        for (var attempt = 0; attempt < 20 && status == HttpStatusCode.ServiceUnavailable; attempt++)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(250));
            using var next = await _client.GetAsync($"/cgi-bin/env{Ext}");
            status = next.StatusCode;
        }

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, status);
    }

    [TestMethod]
    [TestCategory("DevLocal")]
    [DataRow(1)]
    [DataRow(4)]
    [DataRow(16)]
    public async Task Benchmark_TrivialScript_ReportsLatencyAndThroughput(int concurrency)
    {
        // Stage
        const int requests = 100;
        await StartAsync(new() { ["OoBDev:Cgi:MaxConcurrentProcesses"] = "64" });
        using var warmup = await _client!.GetAsync($"/cgi-bin/env{Ext}");
        var latencies = new List<double>();
        using var gate = new SemaphoreSlim(concurrency);
        var clock = Stopwatch.StartNew();

        // Test
        await Task.WhenAll(Enumerable.Range(0, requests).Select(async _ =>
        {
            await gate.WaitAsync();
            try
            {
                var started = Stopwatch.GetTimestamp();
                using var response = await _client.GetAsync($"/cgi-bin/env{Ext}");
                var elapsed = Stopwatch.GetElapsedTime(started).TotalMilliseconds;
                Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
                lock (latencies)
                    latencies.Add(elapsed);
            }
            finally
            {
                gate.Release();
            }
        }));
        clock.Stop();

        // Assert
        latencies.Sort();
        var p50 = latencies[latencies.Count / 2];
        var p95 = latencies[(int)(latencies.Count * 0.95)];
        Console.WriteLine($"{Environment.OSVersion} concurrency={concurrency} requests={requests} p50={p50:F1}ms p95={p95:F1}ms throughput={requests / clock.Elapsed.TotalSeconds:F1}/s");
        Assert.HasCount(requests, latencies);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Start_MissingScriptRoot_Fails()
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls("http://127.0.0.1:0");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?> { ["OoBDev:Cgi:ScriptRoot"] = Path.Combine(AppContext.BaseDirectory, "does-not-exist") });
        builder.Services.TryAddCgiServices(builder.Configuration, "OoBDev:Cgi");
        await using var app = builder.Build();

        await Assert.ThrowsExactlyAsync<OptionsValidationException>(() => app.StartAsync());
    }
}
