# OoBDev.OpenTelemetry

## Summary

OpenTelemetry registration for OoBDev hosts: tracer and logger providers, OTLP export to any collector, and processors that add correlation and user information to spans and log records. It replaces the former `OoBDev.Microsoft.ApplicationInsights` project; the vendor SDK is gone, so any OTLP backend works (Grafana, Jaeger, Azure Monitor through a collector, and others).

## Usage

```csharp
services.TryAddOpenTelemetryExtensions(configuration);   // section "OpenTelemetryOptions"
```

Nothing is added unless the configuration section exists or `OTEL_EXPORTER_OTLP_ENDPOINT` is set, so hosts without telemetry settings are unchanged. The processors need `IAccessor<CorrelationInfo>` and `IHttpContextAccessor` in the container. `TryAddCommonExternalExtensions` calls this with `ExternalExtensionBuilder.OpenTelemetryOptionSection`.

**Table 1 — OpenTelemetryOptions**

| Setting | Default | Purpose |
|---------|---------|---------|
| `OtlpEndpoint` | none (then `OTEL_EXPORTER_OTLP_ENDPOINT`) | Collector base address, for example `http://localhost:4318`. For `HttpProtobuf` the signal path (`v1/traces`, `v1/logs`) is added for you |
| `Protocol` | `HttpProtobuf` | `HttpProtobuf` or `Grpc` |
| `ServiceName` | exporter default | `service.name` resource attribute |
| `Sources` | none | `ActivitySource` names to collect spans from |

```json
{
  "OpenTelemetryOptions": {
    "OtlpEndpoint": "http://otel-collector:4318",
    "ServiceName": "my-api",
    "Sources": [ "MyCompany.MyApi" ]
  }
}
```

The standard `OTEL_*` variables (headers, protocol, resource attributes) are honored by the exporter in addition to the options.

**Table 2 — Processors**

| Type | Signal | Adds |
|------|--------|------|
| `CorrelationInfoTelemetryProcessor` | spans (`OnStart`) | correlation id and request id tags |
| `CorrelationInfoLogProcessor` | log records (`OnEnd`) | correlation id and request id attributes |
| `UserTelemetryProcessor` | spans | `Claim-{ObjectId}` and `Claim-{UserId}` tags from the HTTP context |
| `UserLogProcessor` | log records | the same claims as attributes |

## Testing

Unit tests cover the processors and the registration. The Integration tests send spans and logs over OTLP to the `otel-lgtm` container (`grafana/otel-lgtm`, started by `containers/testing`) and read them back from Tempo and Loki through the Grafana proxy. They use the `OTEL_EXPORTER_OTLP_ENDPOINT` and `OTEL_GRAFANA_URL` test properties.

## Notes

* https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel
* https://opentelemetry.io/docs/specs/otel/protocol/exporter/
* [Change document](../../../docs/changes/migration-opentelemetry-2026-10-09.md)
