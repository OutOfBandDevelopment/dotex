using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Middleware;
using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using OoBDev.TestUtilities;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Middleware;

[TestClass]
public class ProblemDetailsExceptionHandlerTests
{
    public required TestContext TestContext { get; set; }

    private sealed class TestAccessor : IAccessor<CorrelationInfo>
    {
        public CorrelationInfo? Value { get; set; }
    }

    private async Task<(bool handled, int status, string? contentType, JsonElement body)> RunAsync(bool includeLocation, Exception exception)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IAccessor<CorrelationInfo>>(new TestAccessor { Value = new CorrelationInfo { CorrelationId = "corr-1", RequestId = "req-1" } });
        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Path = "/api/things";
        context.TraceIdentifier = "trace-1";
        context.Response.Body = new MemoryStream();
        var handler = new ProblemDetailsExceptionHandler(
            NullLogger<ProblemDetailsExceptionHandler>.Instance,
            Options.Create(new ProblemDetailsExceptionHandlerOptions { IncludeErrorLocation = includeLocation }));

        var handled = await handler.TryHandleAsync(context, exception, TestContext.CancellationToken);

        context.Response.Body.Position = 0;
        var body = await JsonDocument.ParseAsync(context.Response.Body, cancellationToken: TestContext.CancellationToken);
        TestContext.WriteLine($"status {context.Response.StatusCode}, body {body.RootElement}");
        return (handled, context.Response.StatusCode, context.Response.ContentType, body.RootElement.Clone());
    }

    private static InvalidOperationException Thrown()
    {
        try { throw new InvalidOperationException("boom"); }
        catch (InvalidOperationException ex) { return ex; }
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task TryHandleAsync_UnhandledException_ReturnsProblemDetailsWithCorrelation()
    {
        var (handled, status, contentType, body) = await RunAsync(false, Thrown());

        Assert.IsTrue(handled);
        Assert.AreEqual(500, status);
        StringAssert.StartsWith(contentType, "application/problem+json");
        Assert.AreEqual("trace-1", body.GetProperty("traceId").GetString());
        Assert.AreEqual("corr-1", body.GetProperty("correlationId").GetString());
        Assert.AreEqual("req-1", body.GetProperty("requestId").GetString());
        Assert.AreEqual("/api/things", body.GetProperty("instance").GetString());
        Assert.IsFalse(body.TryGetProperty("errorLocation", out _), "location must stay hidden by default");
        Assert.IsFalse(body.ToString().Contains("boom", StringComparison.Ordinal), "the exception message must not leak");
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task TryHandleAsync_IncludeErrorLocation_NamesTheThrowingMethod()
    {
        var (_, _, _, body) = await RunAsync(true, Thrown());

        var location = body.GetProperty("errorLocation");
        Assert.AreEqual(typeof(InvalidOperationException).FullName, location.GetProperty("exception").GetString());
        StringAssert.Contains(location.GetProperty("method").GetString(), nameof(Thrown));
    }
}
