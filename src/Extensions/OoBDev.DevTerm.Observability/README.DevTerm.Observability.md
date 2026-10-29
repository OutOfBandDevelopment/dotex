# OoBDev.DevTerm.Observability

Opt-in OTLP (gRPC) export of `DevTermTelemetry` traces and metrics. `TelemetryExporter.ParseEndpoint` reads the option (`true` means `http://localhost:4317`); `TelemetryExporter.Start` begins best-effort export and never throws for an unreachable collector.

Tests: `OoBDev.DevTerm.Observability.Tests`.
