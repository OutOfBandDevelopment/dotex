using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using OpenTelemetry;
using OpenTelemetry.Logs;
using System.Collections.Generic;
using System.Diagnostics;

namespace OoBDev.OpenTelemetry;

/// <summary>
/// OpenTelemetry processor that adds correlation information to spans.
/// </summary>
/// <remarks>
/// The former Application Insights <c>ITelemetryProcessor</c> chain is replaced by
/// <see cref="BaseProcessor{T}"/> instances. The tag names are unchanged from the 2.x global properties.
/// </remarks>
public class CorrelationInfoTelemetryProcessor : BaseProcessor<Activity>
{
    private readonly IAccessor<CorrelationInfo> _correlationAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationInfoTelemetryProcessor"/> class.
    /// </summary>
    /// <param name="stringAccessor">The accessor for managing correlation information.</param>
    public CorrelationInfoTelemetryProcessor(IAccessor<CorrelationInfo> stringAccessor)
        => _correlationAccessor = stringAccessor;

    /// <summary>
    /// Adds correlation information to the span tags when the span starts.
    /// </summary>
    /// <param name="data">The span being started.</param>
    public override void OnStart(Activity data)
    {
        var info = _correlationAccessor.Value;
        if (!string.IsNullOrWhiteSpace(info?.CorrelationId))
            data.SetTag(DefinedHttpHeaders.CorrelationIdHeader, info.CorrelationId);
        if (!string.IsNullOrWhiteSpace(info?.RequestId))
            data.SetTag(DefinedHttpHeaders.RequestIdHeader, info.RequestId);
    }
}

/// <summary>
/// OpenTelemetry processor that adds correlation information to log records.
/// </summary>
public class CorrelationInfoLogProcessor : BaseProcessor<LogRecord>
{
    private readonly IAccessor<CorrelationInfo> _correlationAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationInfoLogProcessor"/> class.
    /// </summary>
    /// <param name="stringAccessor">The accessor for managing correlation information.</param>
    public CorrelationInfoLogProcessor(IAccessor<CorrelationInfo> stringAccessor)
        => _correlationAccessor = stringAccessor;

    /// <summary>
    /// Adds correlation information to the log record attributes.
    /// </summary>
    /// <param name="data">The log record being emitted.</param>
    public override void OnEnd(LogRecord data)
    {
        var info = _correlationAccessor.Value;
        var extra = new List<KeyValuePair<string, object?>>(2);
        if (!string.IsNullOrWhiteSpace(info?.CorrelationId))
            extra.Add(new(DefinedHttpHeaders.CorrelationIdHeader, info.CorrelationId));
        if (!string.IsNullOrWhiteSpace(info?.RequestId))
            extra.Add(new(DefinedHttpHeaders.RequestIdHeader, info.RequestId));
        if (extra.Count == 0) return;

        var attributes = new List<KeyValuePair<string, object?>>(data.Attributes ?? []);
        attributes.AddRange(extra);
        data.Attributes = attributes;
    }
}
