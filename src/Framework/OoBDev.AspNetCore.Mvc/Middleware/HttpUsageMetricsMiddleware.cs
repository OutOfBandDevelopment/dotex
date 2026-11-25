using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Threading.Tasks;

namespace OoBDev.AspNetCore.Mvc.Middleware;

/// <summary>
/// Counts API calls and timeouts by route template (never the URL with identifiers) and records request duration.
/// Instruments live on the meter <see cref="MeterName"/>; without a listener they cost almost nothing.
/// </summary>
public class HttpUsageMetricsMiddleware
{
    /// <summary>The meter name.</summary>
    public const string MeterName = "OoBDev.AspNetCore.Mvc";

    private readonly RequestDelegate _next;
    private readonly Counter<long> _requests;
    private readonly Counter<long> _timeouts;
    private readonly Histogram<double> _duration;

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpUsageMetricsMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next delegate.</param>
    /// <param name="meterFactory">The meter factory.</param>
    public HttpUsageMetricsMiddleware(RequestDelegate next, IMeterFactory meterFactory)
    {
        _next = next;
        var meter = meterFactory.Create(MeterName);
        _requests = meter.CreateCounter<long>("oobdev.http.requests", description: "API calls by route template, method and status class");
        _timeouts = meter.CreateCounter<long>("oobdev.http.timeouts", description: "Calls that ended in a timeout (408, 504 or a timeout exception)");
        _duration = meter.CreateHistogram<double>("oobdev.http.request.duration", "ms", "Request duration");
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task representing the operation.</returns>
    public async Task Invoke(HttpContext context)
    {
        var start = Stopwatch.GetTimestamp();
        var timedOut = false;
        try
        {
            await _next(context);
        }
        catch (TimeoutException)
        {
            timedOut = true;
            throw;
        }
        finally
        {
            var status = context.Response.StatusCode;
            var route = (context.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText ?? "unmatched";
            var method = context.Request.Method;
            var statusClass = $"{status / 100}xx";
            _requests.Add(1, new("route", route), new("method", method), new("status_class", statusClass));
            _duration.Record(Stopwatch.GetElapsedTime(start).TotalMilliseconds, new KeyValuePair<string, object?>("route", route));
            if (timedOut || status is StatusCodes.Status408RequestTimeout or StatusCodes.Status504GatewayTimeout)
                _timeouts.Add(1, new KeyValuePair<string, object?>("route", route));
        }
    }
}
