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

**Today:** Swashbuckle with small `IConfigureOptions` classes for filters and OAuth ([pattern 20](../02-design-patterns/20-configure-options-classes.md)).

**Table 19 — OpenAPI generation**

| Option | Pros | Cons |
|--------|------|------|
| Current Swashbuckle | Mature; rich filters; UI included | No longer the template default; upgrades have needed .NET 10 fixes |
| `Microsoft.AspNetCore.OpenApi` (built-in) | First-party; document transformers; less reflection | Needs a separate UI package; different extension model |
| NSwag | Code generation for clients | Different configuration style |
| Scalar or other UIs on top of built-in documents | Modern UI | Additional package |

**Verdict: Consider.** Keep Swashbuckle for existing apps; prefer the built-in generator in new products and port the filter classes to document transformers when there is a reason to touch them.

**Owner decision:** migrate to Scalar and the "async-ui" viewer (as written in the answers; confirm whether this means an AsyncAPI viewer or Swagger UI).

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Messaging](./04-messaging.md) · [Testing and Quality →](./06-testing-and-quality.md)
<!-- nav -->
