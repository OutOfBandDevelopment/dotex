using OoBDev.System.Accessors;
using OoBDev.System.Net.Http;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;

namespace OoBDev.Microsoft.ApplicationInsights.Extensibility;

/// <summary>
/// Implements an <see cref="ITelemetryProcessor"/> that adds correlation information to telemetry items.
/// </summary>
public class CorrelationInfoTelemetryProcessor : ITelemetryProcessor
{
    private readonly IAccessor<CorrelationInfo> _correlationAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorrelationInfoTelemetryProcessor"/> class.
    /// </summary>
    /// <param name="stringAccessor">The accessor for managing correlation information.</param>
    public CorrelationInfoTelemetryProcessor(
        IAccessor<CorrelationInfo> stringAccessor
        ) => _correlationAccessor = stringAccessor;

    /// <summary>
    /// Processes telemetry items by adding correlation information to the global properties.
    /// </summary>
    /// <param name="item">The telemetry item to process.</param>
    public void Process(ITelemetry item)
    {
        var correlationId = _correlationAccessor.Value?.CorrelationId;
        if (!string.IsNullOrWhiteSpace(correlationId))
            item.Context.GlobalProperties[DefinedHttpHeaders.CorrelationIdHeader] = correlationId;
        var requestId = _correlationAccessor.Value?.RequestId;
        if (!string.IsNullOrWhiteSpace(requestId))
            item.Context.GlobalProperties[DefinedHttpHeaders.RequestIdHeader] = requestId;
    }
}
