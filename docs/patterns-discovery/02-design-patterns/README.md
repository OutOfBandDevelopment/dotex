# 02 — Design Patterns (As Practiced in the Code)


Each pattern lists **what it is here**, **where to see it**, **how to repeat it**, and **known rough edges** (so they can be decided on deliberately rather than copied by accident). Alternatives are compared in [Industry Alternatives](../05-industry-alternatives/README.md); the overall structure is in [Architecture](../01-architecture/README.md).

---

<!-- toc:start -->
## Contents

1. [Pattern Interaction Cheat-Sheet](./00-pattern-interaction.md)
2. [Pattern 1 — Abstractions + Implementation + Registration Extension](./01-abstractions-implementation-registration.md)
3. [Pattern 2 — `TryAdd*` everywhere](./02-tryadd-everywhere.md)
4. [Pattern 3 — Provider / Factory with keyed services](./03-provider-factory-keyed.md)
5. [Pattern 4 — `ISelectedService<T>` — config-selected provider](./04-selected-service.md)
6. [Pattern 5 — Config-resolved provider per channel/message](./05-config-resolved-provider.md)
7. [Pattern 6 — Builder *records* carrying config section names](./06-builder-records.md)
8. [Pattern 7 — Options binding by section name](./07-options-binding-by-section-name.md)
9. [Pattern 8 — Config-gated registration](./08-config-gated-registration.md)
10. [Pattern 9 — `#if DEBUG` explicit-argument extension methods](./09-if-debug-explicit-arguments.md)
11. [Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration](./10-attribute-dispatch-proxy.md)
12. [Pattern 11 — Marker-generic channels & handlers](./11-marker-generic-channels-handlers.md)
13. [Pattern 12 — Message context object](./12-message-context-object.md)
14. [Pattern 13 — Supervised hosted service](./13-supervised-hosted-service.md)
15. [Pattern 14 — Strategy collections (engine → providers → sources)](./14-strategy-collections.md)
16. [Pattern 15 — Result envelope](./15-result-envelope.md)
17. [Pattern 16 — Injectable non-determinism](./16-injectable-non-determinism.md)
18. [Pattern 17 — Attribute + mapper data access](./17-attribute-mapper-data-access.md)
19. [Pattern 18 — Replace-to-override in host layers](./18-replace-to-override-in-host-layers.md)
20. [Pattern 19 — ASP.NET cross-cutting: middleware + request features](./19-aspnet-middleware-request-features.md)
21. [Pattern 20 — `IConfigureOptions<>` classes for third-party options](./20-configure-options-classes.md)

### List of Figures

1. [Figure 1 — Pattern interaction (registration and runtime resolution)](./00-pattern-interaction.md)

### List of Tables

1. [Table 1 — Provider registration conventions](./03-provider-factory-keyed.md)
<!-- toc:end -->
---
