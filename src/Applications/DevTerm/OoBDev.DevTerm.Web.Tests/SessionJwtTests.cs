using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.TestUtilities;

namespace OoBDev.DevTerm.Web.Tests;

[TestClass]
[TestCategory(TestCategories.Unit)]
[TestCategory(TestCategories.Web)]
public class SessionJwtTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TryValidate_FreshToken_IsValidAndKeepsTheReadOnlyFlag()
    {
        // Stage
        var jwt = new SessionJwt();

        // Test
        var (full, _) = jwt.Issue(readOnly: false);
        var (watch, _) = jwt.Issue(readOnly: true);

        // Assert
        Assert.IsTrue(jwt.TryValidate(full, out var fullReadOnly));
        Assert.IsFalse(fullReadOnly);
        Assert.IsTrue(jwt.TryValidate(watch, out var watchReadOnly));
        Assert.IsTrue(watchReadOnly);
    }

    [TestMethod]
    public void TryValidate_AfterTheLifetime_IsRejected()
    {
        // Stage
        var clock = new FakeTimeProvider(new DateTimeOffset(2026, 11, 25, 12, 0, 0, TimeSpan.Zero));
        var jwt = new SessionJwt(clock, TimeSpan.FromMinutes(5));
        var (token, _) = jwt.Issue(false);

        // Test
        clock.Advance(TimeSpan.FromMinutes(4));
        var before = jwt.TryValidate(token, out _);
        clock.Advance(TimeSpan.FromMinutes(2));
        var after = jwt.TryValidate(token, out _);
        TestContext.WriteLine($"before={before} after={after}");

        // Assert
        Assert.IsTrue(before);
        Assert.IsFalse(after);
    }

    [TestMethod]
    public void TryValidate_TokenFromAnotherHostOrGarbage_IsRejected()
    {
        // Stage
        var (foreign, _) = new SessionJwt().Issue(false);
        var jwt = new SessionJwt();

        // Test and Assert
        Assert.IsFalse(jwt.TryValidate(foreign, out _));
        Assert.IsFalse(jwt.TryValidate("secret", out _));
        Assert.IsFalse(jwt.TryValidate(null, out _));
        Assert.IsFalse(jwt.TryValidate("a.b.c", out _));
    }

    [TestMethod]
    public async Task TokenEndpoint_SharedKeyIsExchanged_JwtWorksAndCannotBeExchangedAgain()
    {
        // Stage
        int port;
        using (var listener = new TcpListener(IPAddress.Loopback, 0))
        {
            listener.Start();
            port = ((IPEndPoint)listener.LocalEndpoint).Port;
        }

        var built = WebHost.Build(new CliOptions { Transport = "loopback", Presenter = ["ascii"], Tui = false, Cli = true },
            new WebOptions { Urls = $"http://127.0.0.1:{port}", Token = "secret", ReadOnlyToken = "watch" }, []);
        await built.Hub.StartAsync();
        await built.App.StartAsync();
        await using (built.Hub)
        {
            var url = $"http://127.0.0.1:{port}";
            using var key = new HttpClient { DefaultRequestHeaders = { Authorization = new("Bearer", "secret") } };
            using var viewer = new HttpClient { DefaultRequestHeaders = { Authorization = new("Bearer", "watch") } };

            // Test
            var issued = await (await key.PostAsync(url + "/api/token", null, TestContext.CancellationToken)).Content.ReadFromJsonAsync<JsonElement>(TestContext.CancellationToken);
            var viewerIssued = await (await viewer.PostAsync(url + "/api/token", null, TestContext.CancellationToken)).Content.ReadFromJsonAsync<JsonElement>(TestContext.CancellationToken);
            using var withJwt = new HttpClient { DefaultRequestHeaders = { Authorization = new("Bearer", issued.GetProperty("access_token").GetString()) } };
            using var withViewerJwt = new HttpClient { DefaultRequestHeaders = { Authorization = new("Bearer", viewerIssued.GetProperty("access_token").GetString()) } };
            var status = await withJwt.GetStringAsync(url + "/api/status", TestContext.CancellationToken);
            var viewerStatus = await withViewerJwt.GetStringAsync(url + "/api/status", TestContext.CancellationToken);
            var again = await withJwt.PostAsync(url + "/api/token", null, TestContext.CancellationToken);
            using var anonymous = new HttpClient();
            var denied = await anonymous.PostAsync(url + "/api/token", null, TestContext.CancellationToken);
            TestContext.WriteLine($"expires_in={issued.GetProperty("expires_in")} again={(int)again.StatusCode} anonymous={(int)denied.StatusCode}");

            // Assert
            Assert.AreEqual("Bearer", issued.GetProperty("token_type").GetString());
            StringAssert.Contains(status, "\"readOnly\":false");
            StringAssert.Contains(viewerStatus, "\"readOnly\":true");
            Assert.AreEqual(HttpStatusCode.Forbidden, again.StatusCode);
            Assert.AreEqual(HttpStatusCode.Unauthorized, denied.StatusCode);
            await built.App.StopAsync();
        }
    }
}
