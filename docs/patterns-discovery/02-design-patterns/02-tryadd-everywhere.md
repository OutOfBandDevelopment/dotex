# Pattern 2 — `TryAdd*` everywhere

[↑ Design Patterns](./README.md) · [← 1. Abstractions + Implementation + Registration Extension](./01-abstractions-implementation-registration.md) · [3. Provider / Factory with keyed services →](./03-provider-factory-keyed.md)

**What:** Library code uses `TryAddSingleton/Transient/…` so *the application always wins* and repeated calls are idempotent. Lifetimes seen: **Transient by default**, **Singleton** for stateless/expensive utilities (serializers, hashes, `IDateTimeProvider`), **Singleton with a factory** for the selected-service wrapper.

**Exceptions that are deliberate:** collection-style registrations use plain `AddTransient` because *all* should be present (`ITemplateProvider`, `IFileType`, `IMessageSenderProvider`, `IMessageReceiverProvider`).

**Repeat:** default to `TryAdd*`; use `Add*` only when the interface is consumed as `IEnumerable<T>`.

---

[↑ Design Patterns](./README.md) · [← 1. Abstractions + Implementation + Registration Extension](./01-abstractions-implementation-registration.md) · [3. Provider / Factory with keyed services →](./03-provider-factory-keyed.md)
