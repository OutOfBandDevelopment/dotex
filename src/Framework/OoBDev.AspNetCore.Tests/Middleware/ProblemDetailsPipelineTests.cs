using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Middleware;

[TestClass]
public class ProblemDetailsPipelineTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task UseProblemDetailsExceptionHandler_ThrowingEndpoint_ReturnsProblemJsonWithLocationWhenConfigured()
    {
        // Stage
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["OoBDev:ProblemDetails:IncludeErrorLocation"] = "true" })
            .Build();
        var services = new ServiceCollection();
        services.AddLogging(b => b.SetMinimumLevel(LogLevel.None));
        services.AddMetrics();
        services.AddSingleton(new global::System.Diagnostics.DiagnosticListener("test"));
        services.TryAddProblemDetailsExceptionHandler(configuration);
        using var provider = services.BuildServiceProvider();
        var app = new ApplicationBuilder(provider);
        app.UseProblemDetailsExceptionHandler();
        app.Run(_ => throw new InvalidOperationException("boom"));
        var pipeline = app.Build();
        var context = new DefaultHttpContext { RequestServices = provider };
        context.Request.Path = "/fail";
        context.Response.Body = new MemoryStream();

        // Test
        await pipeline(context);

        // Assert
        context.Response.Body.Position = 0;
        using var body = await JsonDocument.ParseAsync(context.Response.Body, cancellationToken: TestContext.CancellationToken);
        TestContext.WriteLine($"status {context.Response.StatusCode}, body {body.RootElement}");
        Assert.AreEqual(500, context.Response.StatusCode);
        Assert.AreEqual(typeof(InvalidOperationException).FullName, body.RootElement.GetProperty("errorLocation").GetProperty("exception").GetString());
        Assert.IsTrue(body.RootElement.TryGetProperty("traceId", out _));
    }
}
