using System;
using System.Collections.Generic;

namespace OoBDev.WebApi.Models;

/// <summary>A recent span and the dashboard link.</summary>
public class TelemetryRecentModel
{
    /// <summary>Gets or sets the dashboard address a browser can open.</summary>
    public required string DashboardUrl { get; set; }

    /// <summary>Gets or sets the recent traces, newest first.</summary>
    public required IList<TelemetryTraceModel> Traces { get; set; }
}

/// <summary>One recent trace.</summary>
public class TelemetryTraceModel
{
    /// <summary>Gets or sets the trace id.</summary>
    public required string TraceId { get; set; }

    /// <summary>Gets or sets the span name.</summary>
    public required string Name { get; set; }

    /// <summary>Gets or sets when the span started.</summary>
    public DateTimeOffset Started { get; set; }

    /// <summary>Gets or sets the span duration in milliseconds.</summary>
    public double DurationMs { get; set; }
}
