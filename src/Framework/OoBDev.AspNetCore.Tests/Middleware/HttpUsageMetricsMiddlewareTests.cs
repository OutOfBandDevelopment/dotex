using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.AspNetCore.Mvc.Middleware;
using OoBDev.TestUtilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Tests.Middleware;

[TestClass]
public class HttpUsageMetricsMiddlewareTests
{
    public required TestContext TestContext { get; set; }

    private sealed record Measure(string Instrument, double Value, Dictionary<string, object?> Tags);

    private async Task<List<Measure>> RunAsync(Func<HttpContext, Task> endpoint, int status)
    {
        var services = new ServiceCollection().AddMetrics().BuildServiceProvider();
        var factory = services.GetRequiredService<IMeterFactory>();
        var measures = new List<Measure>();
        using var listener = new MeterListener();
        listener.InstrumentPublished = (i, l) => { if (i.Meter.Name == HttpUsageMetricsMiddleware.MeterName) l.EnableMeasurementEvents(i); };
        listener.SetMeasurementEventCallback<long>((i, v, t, _) => measures.Add(new(i.Name, v, t.ToArray().ToDictionary(x => x.Key, x => x.Value))));
        listener.SetMeasurementEventCallback<double>((i, v, t, _) => measures.Add(new(i.Name, v, t.ToArray().ToDictionary(x => x.Key, x => x.Value))));
        listener.Start();

        var middleware = new HttpUsageMetricsMiddleware(async ctx =>
        {
            ctx.SetEndpoint(new RouteEndpoint(_ => Task.CompletedTask, RoutePatternFactory.Parse("/api/things/{id}"), 0, new EndpointMetadataCollection(), "things"));
            ctx.Response.StatusCode = status;
            await endpoint(ctx);
        }, factory);
        var context = new DefaultHttpContext();
        context.Request.Method = "GET";
        context.Request.Path = "/api/things/42";

        try { await middleware.Invoke(context); }
        catch (TimeoutException) { TestContext.WriteLine("timeout propagated"); }
        listener.RecordObservableInstruments();
        TestContext.WriteLine(string.Join("; ", measures.Select(m => $"{m.Instrument}={m.Value}")));
        return measures;
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Invoke_MatchedRoute_CountsByTemplateNotUrl()
    {
        var measures = await RunAsync(_ => Task.CompletedTask, 200);

        var requests = measures.Single(m => m.Instrument == "oobdev.http.requests");
        Assert.AreEqual("/api/things/{id}", requests.Tags["route"]);
        Assert.AreEqual("2xx", requests.Tags["status_class"]);
        Assert.IsTrue(measures.Any(m => m.Instrument == "oobdev.http.request.duration"));
        Assert.IsFalse(measures.Any(m => m.Instrument == "oobdev.http.timeouts"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Invoke_GatewayTimeout_CountsATimeout()
    {
        var measures = await RunAsync(_ => Task.CompletedTask, 504);

        Assert.AreEqual(1, measures.Count(m => m.Instrument == "oobdev.http.timeouts"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task Invoke_TimeoutException_CountsATimeoutAndRethrows()
    {
        var measures = await RunAsync(_ => throw new TimeoutException(), 500);

        Assert.AreEqual(1, measures.Count(m => m.Instrument == "oobdev.http.timeouts"));
        Assert.AreEqual("5xx", measures.Single(m => m.Instrument == "oobdev.http.requests").Tags["status_class"]);
    }
}
