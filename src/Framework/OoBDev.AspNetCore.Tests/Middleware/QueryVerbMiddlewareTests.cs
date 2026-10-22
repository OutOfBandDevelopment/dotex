using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Middleware;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Middleware;

[TestClass]
public class QueryVerbMiddlewareTests
{
    private static (QueryVerbMiddleware middleware, Probe probe) Create()
    {
        var probe = new Probe();
        return (new QueryVerbMiddleware(c => { probe.Method = c.Request.Method; probe.Called = true; return Task.CompletedTask; }), probe);
    }

    private static DefaultHttpContext Request(string method, string? contentType = "application/json", string? overrideHeader = null)
    {
        var context = new DefaultHttpContext();
        context.Request.Method = method;
        context.Request.ContentType = contentType;
        if (overrideHeader != null)
            context.Request.Headers[QueryVerbMiddleware.OverrideHeader] = overrideHeader;
        context.Response.Body = new global::System.IO.MemoryStream();
        return context;
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_QueryVerb_RoutesAsGetAndSetsFeatureAsync()
    {
        // Stage
        var (middleware, probe) = Create();
        var context = Request("QUERY");

        // Test
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual("GET", probe.Method);
        Assert.AreEqual("QUERY", context.Features.Get<IQueryVerbFeature>()?.OriginalMethod);
        Assert.AreEqual("application/json", context.Request.ContentType);
    }

    [TestMethod]
    [TestCategory("Unit")]
    [DataRow("QUERY", true)]
    [DataRow("query", true)]
    [DataRow("GET", false)]
    public async Task Invoke_PostWithOverride_RewritesOnlyQueryAsync(string value, bool rewritten)
    {
        // Stage
        var (middleware, probe) = Create();
        var context = Request("POST", overrideHeader: value);

        // Test
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(rewritten ? "GET" : "POST", probe.Method);
        Assert.AreEqual(rewritten ? "POST" : null, context.Features.Get<IQueryVerbFeature>()?.OriginalMethod);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_PutWithOverride_IsNotRewrittenAsync()
    {
        // Stage
        var (middleware, probe) = Create();

        // Test
        await middleware.InvokeAsync(Request("PUT", overrideHeader: "QUERY"));

        // Assert
        Assert.AreEqual("PUT", probe.Method);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_QueryWithoutBodyType_Returns415Async()
    {
        // Stage
        var (middleware, probe) = Create();
        var context = Request("QUERY", contentType: null);

        // Test
        await middleware.InvokeAsync(context);

        // Assert
        Assert.AreEqual(415, context.Response.StatusCode);
        Assert.IsFalse(probe.Called);
    }

    private sealed class Probe
    {
        public string? Method { get; set; }
        public bool Called { get; set; }
    }
}
