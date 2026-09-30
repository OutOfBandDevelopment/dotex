# 05 — Industry Alternatives

The [current patterns](../02-design-patterns/README.md) and [practices](../03-practices-and-conventions/README.md) are the author's preferences and are documented as the standard to carry forward. This section is different: it lists **common industry approaches that differ** from them, with pros and cons, so each can be adopted, ignored or deferred on purpose.

## How to read it

Each topic has the same shape: what the code does today, the alternatives with pros and cons in a table, and a verdict.

**Table 1 — Verdict scale**

| Verdict | Meaning |
|---------|---------|
| **Keep** | The current approach is a sound match for industry practice or better suited to this codebase; carry it forward. |
| **Consider** | A worthwhile improvement that can be adopted incrementally or in new projects only. |
| **Change** | The current approach has a real defect or risk; fix or replace it in new work. |

Products change quickly. Licensing and feature statements below reflect what was generally known at the time of writing and should be re-checked before a decision.

## Summary of recommendations

**Table 2 — All topics and verdicts**

| # | Topic | Verdict | Where |
|---|-------|---------|-------|
| 1 | Provider selection (`ISelectedService` vs keyed DI vs named options) | Keep, consider simplification | [DI and composition](./01-di-and-composition.md) |
| 2 | Options binding and validation | Change (add validation) | [DI and composition](./01-di-and-composition.md) |
| 3 | `#if DEBUG` required parameters | Change | [DI and composition](./01-di-and-composition.md) |
| 4 | Builder records vs configure delegates | Keep | [DI and composition](./01-di-and-composition.md) |
| 5 | `IServiceProvider` injection | Keep, contained | [DI and composition](./01-di-and-composition.md) |
| 6 | Abstractions / implementation split | Keep | [Structure and build](./02-project-structure-and-build.md) |
| 7 | Glob-based Common aggregator | Consider | [Structure and build](./02-project-structure-and-build.md) |
| 8 | Layering style (Clean, Onion, Vertical Slice) | Keep | [Structure and build](./02-project-structure-and-build.md) |
| 9 | Central package management | Change | [Structure and build](./02-project-structure-and-build.md) |
| 10 | Analyzers and warnings as errors | Change | [Structure and build](./02-project-structure-and-build.md) |
| 11 | Versioning tool | Keep | [Structure and build](./02-project-structure-and-build.md) |
| 12 | `DispatchProxy` caching | Consider | [Cross-cutting](./03-cross-cutting-runtime.md) |
| 13 | Result envelope | Keep, align at HTTP edge | [Cross-cutting](./03-cross-cutting-runtime.md) |
| 14 | Observability and resilience | Consider (add) | [Cross-cutting](./03-cross-cutting-runtime.md) |
| 15 | Logging style | Consider | [Cross-cutting](./03-cross-cutting-runtime.md) |
| 16 | Default hash algorithm | Change | [Cross-cutting](./03-cross-cutting-runtime.md) |
| 17 | Custom messaging | Keep, consider bridge | [Messaging](./04-messaging.md) |
| 18 | Stored procedure mapper | Keep, consider Dapper | [Data and API](./05-data-and-api.md) |
| 19 | Swagger generation | Consider | [Data and API](./05-data-and-api.md) |
| 20 | Test framework and mocking | Consider | [Testing and quality](./06-testing-and-quality.md) |
| 21 | Docker test infrastructure | Consider | [Testing and quality](./06-testing-and-quality.md) |
| 22 | Documentation tooling | Consider | [Documentation](./07-documentation.md) |

*Figure 1 — effort and value of the recommended changes*

```plantuml
@startuml
skinparam shadowing false
rectangle "Low effort, high value" as Q1 {
  rectangle "Options validation" as a
  rectangle "Central package management" as b
  rectangle "Fix RetreiveAsync typo" as c
  rectangle "SHA-256 default hash" as d
}
rectangle "Higher effort, high value" as Q2 {
  rectangle "Enable analyzers" as e
  rectangle "OpenTelemetry and Polly" as f
  rectangle "Async-safe caching decorator" as g
}
rectangle "Optional" as Q3 {
  rectangle "Testcontainers" as h
  rectangle "Built-in OpenAPI" as i
  rectangle "Messaging bridge" as j
}
Q1 -[hidden]right- Q2
Q2 -[hidden]right- Q3
@enduml
```

<!-- toc:start -->
## Contents

1. [DI and Composition](./01-di-and-composition.md)
2. [Project Structure and Build](./02-project-structure-and-build.md)
3. [Cross-Cutting Runtime Concerns](./03-cross-cutting-runtime.md)
4. [Messaging](./04-messaging.md)
5. [Data Access and HTTP API](./05-data-and-api.md)
6. [Testing and Quality](./06-testing-and-quality.md)
7. [Documentation](./07-documentation.md)

### List of Figures

1. [Figure 1 — replacing the transport without changing callers](./04-messaging.md)

### List of Tables

1. [Table 1 — Provider selection options](./01-di-and-composition.md)
2. [Table 2 — Options approaches](./01-di-and-composition.md)
3. [Table 3 — Alternatives](./01-di-and-composition.md)
4. [Table 4 — Configuration objects](./01-di-and-composition.md)
5. [Table 5 — Service-locator concerns](./01-di-and-composition.md)
6. [Table 6 — Packaging of contracts](./02-project-structure-and-build.md)
7. [Table 7 — Aggregation approaches](./02-project-structure-and-build.md)
8. [Table 8 — Structural styles](./02-project-structure-and-build.md)
9. [Table 9 — Version management](./02-project-structure-and-build.md)
10. [Table 10 — Static analysis](./02-project-structure-and-build.md)
11. [Table 11 — Version derivation](./02-project-structure-and-build.md)
12. [Table 12 — Caching approaches](./03-cross-cutting-runtime.md)
13. [Table 13 — Error and result models](./03-cross-cutting-runtime.md)
14. [Table 14 — Operational libraries](./03-cross-cutting-runtime.md)
15. [Table 15 — Logging approaches](./03-cross-cutting-runtime.md)
16. [Table 16 — Hash choices](./03-cross-cutting-runtime.md)
17. [Table 17 — Messaging options](./04-messaging.md)
18. [Table 18 — Data access options](./05-data-and-api.md)
19. [Table 19 — OpenAPI generation](./05-data-and-api.md)
20. [Table 20 — Test tooling](./06-testing-and-quality.md)
21. [Table 21 — Container strategies](./06-testing-and-quality.md)
22. [Table 22 — Documentation options](./07-documentation.md)
23. [Table 23 — Suggested next documentation steps](./07-documentation.md)
<!-- toc:end -->
