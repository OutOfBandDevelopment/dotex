# Architecture § 6 — Runtime Composition Model (How a Call Finds Its Implementation)

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 5 — Composition Root](./05-composition-root.md) · [Architecture § 7 — Cross-Cutting Building Blocks (all in `OoBDev.System*`) →](./07-cross-cutting-building-blocks.md)
<!-- nav -->

There are three selection mechanisms, used for three different situations. They are described in detail in [Design Patterns](../02-design-patterns/README.md): [pattern 3 – provider/factory](../02-design-patterns/03-provider-factory-keyed.md), [pattern 4 – config-selected keyed service](../02-design-patterns/04-selected-service.md), [pattern 5 – config-resolved provider](../02-design-patterns/05-config-resolved-provider.md) and [pattern 14 – strategy collections](../02-design-patterns/14-strategy-collections.md).

**Table 3 — Runtime composition mechanisms**

| Situation | Mechanism | Example |
|-----------|-----------|---------|
| Exactly one implementation should win, chosen by deployment | **Default registration + keyed registration + `TryAddConfiguredKeyedService<T>`** (config path such as `OoBDev:CachingProvider:Type`) | `ICachingProvider` → Redis / Microsoft memory cache |
| Choice is made *per message type / per channel* at call time | **Config-resolved keyed provider** (`MessageQueue:{Channel}:{Message}:Provider`) | `IMessageSenderProviderFactory.Sender(channel, message)` |
| *All* implementations participate | **Multiple registrations** (`AddTransient`, then resolve `IEnumerable<T>`) | `ITemplateProvider`, `IFileType`, `IMessageReceiverProvider` |

---

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Architecture § 5 — Composition Root](./05-composition-root.md) · [Architecture § 7 — Cross-Cutting Building Blocks (all in `OoBDev.System*`) →](./07-cross-cutting-building-blocks.md)
<!-- nav -->
