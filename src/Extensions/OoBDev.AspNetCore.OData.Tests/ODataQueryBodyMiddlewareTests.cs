using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Middleware;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.OData.Tests;

[TestClass]
public class ODataQueryBodyMiddlewareTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_NotARewrittenRequest_LeavesTheQueryAlone()
    {
        // Stage
        var context = new DefaultHttpContext();
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("%24top=5"));
        var called = false;

        // Test
        await new ODataQueryBodyMiddleware(_ => { called = true; return Task.CompletedTask; }).InvokeAsync(context);

        // Assert
        Assert.IsTrue(called);
        Assert.AreEqual(string.Empty, context.Request.QueryString.Value ?? string.Empty);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_JsonBody_IsNotTouched()
    {
        // Stage
        var context = new DefaultHttpContext();
        context.Features.Set<IQueryVerbFeature>(new FakeFeature());
        context.Request.ContentType = "application/json";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        // Test
        await new ODataQueryBodyMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

        // Assert
        Assert.AreEqual(string.Empty, context.Request.QueryString.Value ?? string.Empty);
        Assert.AreEqual(0, context.Request.Body.Position);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_FormWithOnlyPlainFields_AddsNothing()
    {
        // Stage
        var context = new DefaultHttpContext();
        context.Features.Set<IQueryVerbFeature>(new FakeFeature());
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("name=x"));

        // Test
        await new ODataQueryBodyMiddleware(_ => Task.CompletedTask).InvokeAsync(context);

        // Assert
        Assert.AreEqual(string.Empty, context.Request.QueryString.Value ?? string.Empty);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public void TryAddODataQueryServices_InvalidLimits_FailValidation()
    {
        // Stage
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["OoBDev:OData:MaxTop"] = "0" }).Build();
        var services = new ServiceCollection().AddLogging();
        services.TryAddODataQueryServices(configuration, ServiceCollectionExtensions.DefaultSection);
        using var provider = services.BuildServiceProvider();

        // Test
        var error = Assert.ThrowsExactly<OptionsValidationException>(() => _ = provider.GetRequiredService<IOptions<ODataQueryLimits>>().Value);

        // Assert
        StringAssert.Contains(error.Message, "MaxTop", global::System.StringComparison.Ordinal);
    }

    private sealed class FakeFeature : IQueryVerbFeature
    {
        public string OriginalMethod => "QUERY";
    }
}
