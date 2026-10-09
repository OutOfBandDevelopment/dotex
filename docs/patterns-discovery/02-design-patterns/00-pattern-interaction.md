# Pattern Interaction Cheat-Sheet

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Index](./README.md) · [Pattern 1 — Abstractions + Implementation + Registration Extension →](./01-abstractions-implementation-registration.md)
<!-- nav -->

```plantuml
@startuml
skinparam shadowing false
participant "App.Program" as App
participant "TryAllCommonExtensions" as All
participant "TryAdd{Capability}Services" as Cap
participant "Adapter TryAdd{Vendor}Services" as Ad
participant "Consumer / IManager" as Mgr
participant "Selected keyed factory" as Sel
participant "MessageSender<TChannel>" as Snd
participant "MessagePropertyResolver" as Res

App -> All : cfg, Builder records
All -> Cap : per layer
Cap -> Cap : TryAddProviders()
All -> Ad : cfg, sectionName
Ad -> Ad : Configure<Options>(Bind(section))
Ad -> Ad : TryAdd<I, Impl>() (default)
Ad -> Ad : TryAddKeyed<I, Impl>("Key")

Mgr -> Sel : FromKeyedServices selected
Sel --> Mgr : keyed impl\n(config path)
Mgr -> Snd : Send(message)
Snd -> Res : resolve config (channel, message)
Res --> Snd : keyed IMessageSenderProvider
@enduml
```

*Figure 1 — Pattern interaction (registration and runtime resolution)*

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Index](./README.md) · [Pattern 1 — Abstractions + Implementation + Registration Extension →](./01-abstractions-implementation-registration.md)
<!-- nav -->
