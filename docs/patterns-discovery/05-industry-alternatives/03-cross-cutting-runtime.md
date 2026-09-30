# Cross-Cutting Runtime Concerns

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Project Structure and Build](./02-project-structure-and-build.md) · [Messaging →](./04-messaging.md)
<!-- nav -->

## 12. Attribute-driven caching with `DispatchProxy`

**Today:** `[IsCacheable]` and `[FlushCache]` applied by a reflection-based proxy ([pattern 10](../02-design-patterns/10-attribute-dispatch-proxy.md)).

**Table 12 — Caching approaches**

| Option | Pros | Cons |
|--------|------|------|
| Current `DispatchProxy` | No extra dependency; attribute is declarative | Blocks on async (sync over async); reflection per call; interface only; misspelled API |
| Scrutor decorators | Explicit, testable, no reflection; async friendly | One decorator class per interface |
| Castle DynamicProxy | Mature interception, async support via libraries | Extra dependency; runtime code generation |
| `HybridCache` (Microsoft.Extensions.Caching.Hybrid) | First-party; L1 plus L2, stampede protection, tags | Call-site API rather than attributes; newer |
| Source-generated decorators | No reflection; AOT friendly | Generator to build and maintain |

**Verdict: Consider.** Keep the attribute vocabulary but reimplement dispatch asynchronously; evaluate `HybridCache` as the storage and stampede layer behind `ICachingProvider`.

**Owner decision:** converting to the source-generator version is welcome; it was not an option when the feature was written.

## 13. Result envelope

**Today:** `IResult`, `IModelResult<T>`, `IQueryResult<T>`, `ResultMessage` ([pattern 15](../02-design-patterns/15-result-envelope.md)).

**Table 13 — Error and result models**

| Option | Pros | Cons |
|--------|------|------|
| Current envelope | Carries multiple messages, codes and context; uniform | Not an HTTP standard; clients must know the shape |
| `ProblemDetails` (RFC 9457) | Standard for HTTP errors; built into ASP.NET Core | Single problem per response by default |
| `Result<T>` or `OneOf` in code (ErrorOr, FluentResults) | Explicit success/failure in the type system | Another library; mapping needed at the edge |
| Exceptions for failures | Simple | Costly for expected failures |

**Verdict: Keep,** and map to `ProblemDetails` at the HTTP boundary so external clients get the standard.

**Owner decision:** no general result envelope on the HTTP surface. Failures should be `ProblemDetails`, produced by middleware, automatic through OpenAPI and transparent to the developer.

## 14. Observability and resilience

**Today:** logging through `ILogger`; no OpenTelemetry or resilience library found in the framework layer.

**Table 14 — Operational libraries**

| Option | Pros | Cons |
|--------|------|------|
| Current | Minimal | No traces or metrics; retry logic is hand-rolled (fixed 10 second delay in the message host) |
| OpenTelemetry (traces, metrics, logs) | Vendor-neutral standard; auto-instrumentation | Configuration surface |
| Polly or `Microsoft.Extensions.Resilience` | Retries, circuit breakers, timeouts as policy | **Polly is avoided by the owner (license change)**; `Microsoft.Extensions.Resilience` builds on Polly, so check before use |
| Health checks (`AddHealthChecks`) | Standard readiness and liveness | Needs per-adapter checks |

**Verdict: Consider (add).** Add an `OoBDev.Telemetry` capability and per-adapter health checks; use resilience pipelines for adapter calls.

**Owner decision:** add OpenTelemetry. **Avoid Polly** because of its license change; because `Microsoft.Extensions.Resilience` is built on Polly, its use needs a license and dependency check before adoption, otherwise use small in-house or platform primitives.

## 15. Logging style

**Table 15 — Logging approaches**

| Option | Pros | Cons |
|--------|------|------|
| Current: interpolated strings with hand-built templates | Readable | Interpolation defeats structured logging and allocates even when disabled |
| `LoggerMessage` source generator (`[LoggerMessage]`) | Zero allocation when disabled; compile-time template checks | Extra partial methods |
| `Serilog` message templates | Rich sinks | Another dependency |

**Verdict: Consider.** Use `[LoggerMessage]` in hot paths and templates with arguments everywhere else.

**Owner decision:** migrate to the attribute (`[LoggerMessage]`) form, which did not exist when the framework was written. No third-party logging such as Serilog.

## 16. Default hash algorithm

**Today:** `OoBDev.System` registers MD5 as a default hash.

**Table 16 — Hash choices**

| Option | Pros | Cons |
|--------|------|------|
| MD5 | Fast; ubiquitous for checksums | Collision-broken; flagged by security scanners |
| SHA-256 | Standard strong hash | Slightly slower |
| SHA-512 | Strong hash; the owner's chosen default; often fast on 64-bit CPUs | Longer digests |
| xxHash / non-crypto hashes | Very fast for cache keys | Not for security |

**Verdict: Change.** Default to SHA-256 and keep MD5 only where a legacy format requires it, clearly named.

**Owner decision:** agreed to move the default, and the target is **SHA-512** (not SHA-256).

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Project Structure and Build](./02-project-structure-and-build.md) · [Messaging →](./04-messaging.md)
<!-- nav -->
