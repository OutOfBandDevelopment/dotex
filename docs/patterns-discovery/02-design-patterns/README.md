# 02 — Design Patterns (As Practiced in the Code)


Each pattern lists **what it is here**, **where to see it**, **how to repeat it**, and **known rough edges** (so they can be decided on deliberately rather than copied by accident). Alternatives are compared in [Industry Alternatives](../05-industry-alternatives/README.md); the overall structure is in [Architecture](../01-architecture/README.md).

---

## Contents

- [1. Abstractions + Implementation + Registration Extension](./01-abstractions-implementation-registration.md)
- [2. `TryAdd*` everywhere](./02-tryadd-everywhere.md)
- [3. Provider / Factory with keyed services](./03-provider-factory-keyed.md)
- [4. `ISelectedService<T>` — config-selected provider](./04-selected-service.md)
- [5. Config-resolved provider per channel/message](./05-config-resolved-provider.md)
- [6. Builder *records* carrying config section names](./06-builder-records.md)
- [7. Options binding by section name](./07-options-binding-by-section-name.md)
- [8. Config-gated registration](./08-config-gated-registration.md)
- [9. `#if DEBUG` explicit-argument extension methods](./09-if-debug-explicit-arguments.md)
- [10. Attribute-declared behavior + dispatch-proxy decoration](./10-attribute-dispatch-proxy.md)
- [11. Marker-generic channels & handlers](./11-marker-generic-channels-handlers.md)
- [12. Message context object](./12-message-context-object.md)
- [13. Supervised hosted service](./13-supervised-hosted-service.md)
- [14. Strategy collections (engine → providers → sources)](./14-strategy-collections.md)
- [15. Result envelope](./15-result-envelope.md)
- [16. Injectable non-determinism](./16-injectable-non-determinism.md)
- [17. Attribute + mapper data access](./17-attribute-mapper-data-access.md)
- [18. Replace-to-override in host layers](./18-replace-to-override-in-host-layers.md)
- [19. ASP.NET cross-cutting: middleware + request features](./19-aspnet-middleware-request-features.md)
- [20. `IConfigureOptions<>` classes for third-party options](./20-configure-options-classes.md)
- [Pattern Interaction Cheat-Sheet](#pattern-interaction-cheat-sheet)

### List of Figures

1. [Figure 1 — Pattern interaction (registration and runtime resolution)](#pattern-interaction-cheat-sheet)

### List of Tables

1. [Table 1 — Provider registration conventions](./03-provider-factory-keyed.md)

---

## Pattern Interaction Cheat-Sheet

```plantuml
@startuml
skinparam shadowing false
participant "App.Program" as App
participant "TryAllCommonExtensions" as All
participant "TryAdd{Capability}Services" as Cap
participant "Adapter TryAdd{Vendor}Services" as Ad
participant "Consumer / IManager" as Mgr
participant "ISelectedService<IProvider>" as Sel
participant "MessageSender<TChannel>" as Snd
participant "MessagePropertyResolver" as Res

App -> All : cfg, Builder records
All -> Cap : per layer
Cap -> Cap : TryAddProviders()
All -> Ad : cfg, sectionName
Ad -> Ad : Configure<Options>(Bind(section))
Ad -> Ad : TryAdd<I, Impl>() (default)
Ad -> Ad : TryAddKeyed<I, Impl>("Key")

Mgr -> Sel : .Value
Sel --> Mgr : keyed impl
(OoBDev::ServiceKeys::<Type>)
Mgr -> Snd : Send(message)
Snd -> Res : resolve config (channel, message)
Res --> Snd : keyed IMessageSenderProvider
@enduml
```

*Figure 1 — Pattern interaction (registration and runtime resolution)*
