# Data Access and HTTP API

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Messaging](./04-messaging.md) · [Testing and Quality →](./06-testing-and-quality.md)
<!-- nav -->

## 18. Stored procedure mapper

**Today:** `IDatabaseQuery<TDbOptions>` executes stored procedures described by attributes on a query type, mapping rows to results ([pattern 17](../02-design-patterns/17-attribute-mapper-data-access.md)).

**Table 18 — Data access options**

| Option | Pros | Cons |
|--------|------|------|
| Current attribute mapper | Small; stored-procedure first; streams with `IAsyncEnumerable`; connection chosen by type | Custom code to maintain; reflection and lazy mapper creation |
| Dapper | Tiny, fast, well known; async streaming with buffered options | Still SQL strings; no attribute model |
| Entity Framework Core | Migrations, change tracking, LINQ | Heavy for procedure-centric designs; different mental model |
| Source-generated mappers | Fast, AOT friendly | Generator effort |
| Typed API over `DbDataReader` only | No dependencies | Boilerplate |

**Verdict: Keep** for existing procedure-based databases. **Consider** Dapper as an alternate provider behind the same interface for teams that prefer it, and add unit tests that pin the attribute contract.

**Owner decision:** the stored procedure mapper is a feature to offer, not a requirement of the framework.

## 19. Swagger and OpenAPI

**Today:** `Microsoft.AspNetCore.OpenApi` with document, operation and schema transformers registered by small `IConfigureOptions` classes, and Scalar as the reference ([pattern 20](../02-design-patterns/20-configure-options-classes.md), [design](../../design/OpenApiScalar/README.md)). Swashbuckle was removed on 2026-10-09.

**Table 19 — OpenAPI generation**

| Option | Pros | Cons |
|--------|------|------|
| Swashbuckle (previous) | Mature; rich filters; UI included | Removed; upgrades had needed .NET 10 fixes |
| `Microsoft.AspNetCore.OpenApi` (built-in) | First-party; document transformers; less reflection | Needs a separate UI package; different extension model |
| NSwag | Code generation for clients | Different configuration style |
| Scalar or other UIs on top of built-in documents | Modern UI | Additional package |

**Verdict: Adopted.** The built-in generator with Scalar replaced Swashbuckle; the custom filters became transformers.

**Owner decision:** migrate to Scalar and an AsyncAPI viewer (answered 2026-10-09). Scalar is done; the AsyncAPI document and viewer for the SQS, Service Bus and RabbitMQ surfaces is phase 2 of the [design](../../design/OpenApiScalar/architecture.md#phase-2--asyncapi).

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Messaging](./04-messaging.md) · [Testing and Quality →](./06-testing-and-quality.md)
<!-- nav -->
