# Observability Approaches

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Security Approaches](./12-security.md) · [Resilience Approaches →](./14-resilience.md)
<!-- nav -->

## 28. Metrics, tracing and health checks

**Today:** per-adapter `IHealthCheck` classes and a health endpoint options factory; structured logging with `[LoggerMessage]`; no traces or metrics ([observability practices](../03-practices-and-conventions/13-observability-practices.md)). The owner decision is to add OpenTelemetry.

**Table 35 — Telemetry approaches**

| Approach | Pros | Cons |
|----------|------|------|
| `System.Diagnostics` (`ActivitySource`, `Meter`) with the OpenTelemetry SDK in the host | Platform primitives in libraries; vendor-neutral export; standard names and propagation | SDK and exporter packages to keep current; sampling and cardinality need care |
| Vendor SDK directly (Application Insights, Datadog and others) | Rich vendor features in one package | Locks libraries to a vendor; conflicts with the adapter rule |
| Metrics only (`EventCounters`, Prometheus endpoint) | Cheap and simple | No request-level causality |
| Logs only, with correlation identifiers | Nothing new to learn | Hard to see latency breakdown across services |
| .NET Aspire service defaults | One-line wiring of telemetry, health and discovery; dashboard for local development | Opinionated; conventions may not match the layering; owner has asked for a spike, not adoption |

**Table 36 — Health check styles**

| Style | Pros | Cons |
|-------|------|------|
| Liveness and readiness endpoints with tiers (current direction) | Orchestrator-friendly; hides detail from anonymous callers | Needs the tier rules implemented |
| Single health endpoint | Trivial | Dependency outage can restart healthy processes |
| Synthetic probes from outside | Tests the real user path | Extra infrastructure |

Package capabilities reflect general knowledge and must be re-checked before a decision.

**Verdict: Adopt (owner decision).** Instrument libraries with `ActivitySource` and `Meter`, export with OpenTelemetry from the host, keep vendor exporters out of the framework, split liveness from readiness, and run the Aspire spike before deciding on service defaults.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Security Approaches](./12-security.md) · [Resilience Approaches →](./14-resilience.md)
<!-- nav -->
