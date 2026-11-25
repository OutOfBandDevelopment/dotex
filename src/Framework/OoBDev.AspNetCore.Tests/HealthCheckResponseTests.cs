using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.AspNetCore.Authorization;
using OoBDev.AspNetCore.Mvc.Diagnostics.HealthChecks;
using OoBDev.AspNetCore.Mvc.OpenApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests;

[TestClass]
public class HealthCheckResponseTests
{
    public required TestContext TestContext { get; set; }

    private static HealthReport Report() => new(
        new Dictionary<string, HealthReportEntry>
        {
            ["db"] = new(HealthStatus.Unhealthy, "connection refused", TimeSpan.Zero, new InvalidOperationException("boom"),
                new Dictionary<string, object> { ["server"] = "sql01" }),
        },
        TimeSpan.Zero);

    private async Task<JsonElement> CallAsync(bool authenticated, RightCheckResult? right)
    {
        var services = new ServiceCollection();
        if (right is not null)
        {
            var checker = new Mock<IApplicationRightChecker>();
            checker.Setup(c => c.Check(It.IsAny<ClaimsPrincipal>(), It.Is<IReadOnlyCollection<string>>(r => global::System.Linq.Enumerable.Contains(r, "health.details")))).Returns(right.Value);
            services.AddSingleton(checker.Object);
        }

        var context = new DefaultHttpContext
        {
            RequestServices = services.BuildServiceProvider(),
            User = new ClaimsPrincipal(authenticated ? new ClaimsIdentity([new Claim("sub", "1")], "test") : new ClaimsIdentity()),
        };
        context.Response.Body = new MemoryStream();

        await HealthCheckOptionsFactory.Create().ResponseWriter(context, Report());

        context.Response.Body.Position = 0;
        var body = new StreamReader(context.Response.Body).ReadToEnd();
        TestContext.WriteLine($"authenticated={authenticated}, right={right}: {body.ReplaceLineEndings(" ")}");
        return JsonDocument.Parse(body).RootElement.Clone();
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ResponseWriter_Anonymous_ReturnsStatusOnlyAsync()
    {
        var json = await CallAsync(false, RightCheckResult.Allowed);

        Assert.AreEqual("Unhealthy", json.GetProperty("status").GetString());
        Assert.IsFalse(json.TryGetProperty("results", out _));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ResponseWriter_AuthenticatedWithoutRight_ReturnsChecksWithoutDetailsAsync()
    {
        var json = await CallAsync(true, RightCheckResult.Forbidden);
        var db = json.GetProperty("results").GetProperty("db");

        Assert.AreEqual("Unhealthy", db.GetProperty("status").GetString());
        Assert.IsFalse(db.TryGetProperty("description", out _));
        Assert.IsFalse(db.TryGetProperty("data", out _));
        Assert.IsFalse(db.TryGetProperty("exception", out _));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ResponseWriter_AuthenticatedWithoutChecker_ReturnsChecksWithoutDetailsAsync()
    {
        var json = await CallAsync(true, null);

        Assert.IsFalse(json.GetProperty("results").GetProperty("db").TryGetProperty("description", out _));
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task ResponseWriter_WithDetailRight_ReturnsDescriptionDataAndErrorAsync()
    {
        var json = await CallAsync(true, RightCheckResult.Allowed);
        var db = json.GetProperty("results").GetProperty("db");

        Assert.AreEqual("connection refused", db.GetProperty("description").GetString());
        Assert.AreEqual("sql01", db.GetProperty("data").GetProperty("server").GetString());
        Assert.AreEqual("boom", db.GetProperty("exception").GetString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Transformer_MappedPath_AppearsInDocumentAsync()
    {
        var services = new ServiceCollection().AddSingleton(new HealthCheckPath { Value = "/status/health" }).BuildServiceProvider();
        var document = new global::Microsoft.OpenApi.OpenApiDocument();
        var context = new global::Microsoft.AspNetCore.OpenApi.OpenApiDocumentTransformerContext
        {
            DocumentName = "v1",
            DescriptionGroups = [],
            ApplicationServices = services,
        };

        await new HealthChecksDocumentTransformer().TransformAsync(document, context, CancellationToken.None);

        Assert.IsTrue(document.Paths.ContainsKey("/status/health"));
        Assert.IsFalse(document.Paths.ContainsKey("/health"));
    }
}
