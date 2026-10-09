# OoBDev.Microsoft.ApplicationInsights

## Summary

This is a collection of extensions for Microsoft ApplicationsInsights to include contextual information

## Usage

Application Insights 3.x is built on OpenTelemetry, so the former telemetry processors are OpenTelemetry processors:

| Type | Signal | Adds |
|------|--------|------|
| `CorrelationInfoTelemetryProcessor` | spans (`OnStart`) | correlation id and request id tags |
| `CorrelationInfoLogProcessor` | log records (`OnEnd`) | correlation id and request id attributes |
| `UserTelemetryProcessor` | spans | `Claim-{ObjectId}` and `Claim-{UserId}` tags from the HTTP context |
| `UserLogProcessor` | log records | the same claims as attributes |

```csharp
services.TryAddApplicationInsightsExtensions();
```

This registers the processors and attaches them to the OpenTelemetry tracer and logger providers. It needs `IAccessor<CorrelationInfo>` and `IHttpContextAccessor` in the container.

## Notes 

* https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel
* https://learn.microsoft.com/en-us/azure/azure-monitor/app/api-filtering-sampling
* https://learn.microsoft.com/en-us/azure/azure-monitor/app/distributed-tracing-telemetry-correlation
