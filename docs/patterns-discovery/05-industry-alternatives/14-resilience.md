# Resilience Approaches

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Observability Approaches](./13-observability.md) · [AI and RAG Approaches →](./15-ai-and-rag.md)
<!-- nav -->

## 29. Retries, timeouts and idempotency

**Today:** no shared resilience code; hosts poll with fixed delays; message handlers have no documented idempotency rule ([resilience practices](../03-practices-and-conventions/14-resilience-practices.md)). The owner has said to avoid Polly because of its license change and to check `Microsoft.Extensions.Resilience`, which builds on it.

**Table 37 — Resilience implementation options**

| Option | Pros | Cons |
|--------|------|------|
| Polly directly | Mature and widely known | License change; owner prefers to avoid |
| `Microsoft.Extensions.Resilience` / `Microsoft.Extensions.Http.Resilience` | Official, integrates with `HttpClientFactory` and telemetry | Built on Polly, so the license question must be resolved first; check what is bundled |
| Built-in `HttpClient` handlers and vendor SDK retry settings | No dependency; vendors know their own errors | Inconsistent options per vendor; no shared breaker |
| Small in-house decorators (timeout, retry with jitter, breaker) over interfaces | No dependency; matches the decorator and caching proxy style; testable with `TimeProvider` | Code to write and maintain; easy to get subtly wrong |
| Service mesh or gateway retries | Outside application code | Only for HTTP between services; hides retries from the code that owns idempotency |

**Table 38 — Messaging failure handling**

| Option | Pros | Cons |
|--------|------|------|
| Vendor native retry and dead-letter queues | Least code; well tested | Different semantics per vendor; needs one options shape |
| Framework retry decorator over `IMessageProvider` | Same behavior on every vendor | Duplicates vendor features; more code |
| Outbox and inbox tables | Reliable publish and duplicate detection | Database dependency; extra moving parts |

Licensing and package contents reflect general knowledge and must be verified before a decision.

**Verdict: Consider in-house decorators or `Microsoft.Extensions.Resilience`, pending the license check; Reject Polly directly (owner decision).** Map vendor dead-letter and retry features onto one options shape, make handlers idempotent, and record the library choice as an ADR.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Observability Approaches](./13-observability.md) · [AI and RAG Approaches →](./15-ai-and-rag.md)
<!-- nav -->
