# Pattern 2 — `TryAdd*` everywhere

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 1 — Abstractions + Implementation + Registration Extension](./01-abstractions-implementation-registration.md) · [Pattern 3 — Provider / Factory with keyed services →](./03-provider-factory-keyed.md)
<!-- nav -->

**What:** Library code uses `TryAddSingleton/Transient/…` so *the application always wins* and repeated calls are idempotent. Lifetimes seen: **Transient by default**, **Singleton** for stateless/expensive utilities (serializers, hashes, `TimeProvider`), **Singleton with a factory** for the selected-service wrapper.

**Exceptions that are deliberate:** collection-style registrations use plain `AddTransient` because *all* should be present (`ITemplateProvider`, `IFileType`, `IMessageSenderProvider`, `IMessageReceiverProvider`).

**Repeat:** default to `TryAdd*`; use `Add*` only when the interface is consumed as `IEnumerable<T>`.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 1 — Abstractions + Implementation + Registration Extension](./01-abstractions-implementation-registration.md) · [Pattern 3 — Provider / Factory with keyed services →](./03-provider-factory-keyed.md)
<!-- nav -->
