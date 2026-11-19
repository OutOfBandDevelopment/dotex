using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Middleware;
using OoBDev.System.Accessors;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Middleware;

[TestClass]
public class CultureInfoMiddlewareTests
{
    private sealed class TestAccessor : IAccessor<CultureInfo>
    {
        public CultureInfo? Value { get; set; }
    }

    private sealed class FiringResponseFeature : HttpResponseFeature
    {
        private readonly List<(Func<object, Task> callback, object state)> _starting = [];

        public override void OnStarting(Func<object, Task> callback, object state) => _starting.Add((callback, state));

        public async Task FireAsync()
        {
            foreach (var (callback, state) in _starting)
                await callback(state);
        }
    }

    private static async Task<(CultureInfo? culture, string? contentLanguage)> RunAsync(string? acceptLanguage, CultureInfoMiddlewareOptions? options = null)
    {
        var accessor = new TestAccessor();
        var middleware = new CultureInfoMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        var feature = new FiringResponseFeature();
        context.Features.Set<IHttpResponseFeature>(feature);
        if (acceptLanguage != null)
            context.Request.Headers.AcceptLanguage = acceptLanguage;
        await middleware.Invoke(context, NullLogger<CultureInfoMiddleware>.Instance, accessor, Options.Create(options ?? new CultureInfoMiddlewareOptions()));
        await feature.FireAsync();
        return (accessor.Value, context.Response.Headers.ContentLanguage.ToString());
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_FrenchCanadian_SetsAccessorAndContentLanguageAsync()
    {
        var (culture, contentLanguage) = await RunAsync("fr-CA");

        Assert.AreEqual("fr-CA", culture?.Name);
        Assert.AreEqual("fr-CA", contentLanguage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_UnsupportedCulture_FallsBackToDefaultAsync()
    {
        var (culture, contentLanguage) = await RunAsync("de-DE", new() { DefaultCulture = "en-US", SupportedCultures = ["en-US", "fr"] });

        Assert.AreEqual("en-US", culture?.Name);
        Assert.AreEqual("en-US", contentLanguage);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_QualityOrderAndLanguageMatch_PicksBestSupportedAsync()
    {
        var (culture, _) = await RunAsync("de;q=0.9, fr-CA;q=0.8, en-US;q=0.1", new() { DefaultCulture = "en-US", SupportedCultures = ["en-US", "fr"] });

        Assert.AreEqual("fr", culture?.Name);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_NoHeader_UsesDefaultAsync()
    {
        var (culture, _) = await RunAsync(null, new() { DefaultCulture = "fr" });

        Assert.AreEqual("fr", culture?.Name);
    }

    [TestMethod]
    [TestCategory("Unit")]
    public async Task Invoke_NoHeaderNoDefault_LeavesAccessorEmptyAsync()
    {
        var (culture, _) = await RunAsync(null);

        Assert.IsNull(culture);
    }
}
