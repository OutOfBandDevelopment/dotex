# Pattern 9 — `#if DEBUG` explicit-argument extension methods

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 8 — Config-gated registration](./08-config-gated-registration.md) · [Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration →](./10-attribute-dispatch-proxy.md)
<!-- nav -->

**What:** Optional parameters are optional in **Release** and **required in Debug**. The purpose is to make sure child builders are passed through: a roll-up such as `TryAllCommonExtensions` must hand each layer's builder down to the layer beneath it, and if the parameter had a default in dev builds a caller could silently forget to forward it and get default behavior. Making it required turns that omission into a compile error while developing; Release consumers still get one-liners:

```csharp
public static IServiceCollection TryAddSystemExtensions(
    this IServiceCollection services,
    IConfiguration config,
#if DEBUG
    SystemExtensionBuilder? builder
#else
    SystemExtensionBuilder? builder = default
#endif
)
```

Found in ~37 files (every `Try*` entry point, `IEmbeddingProvider`, etc.). Same trick for `catch { throw; }` inside `CachedProxy` ("blows up locally, swallowed in production").

**Rough edge:** it makes the *compiled API differ between configurations* – a library built in Debug and consumed in Release code (or vice-versa via project reference) will not compile the same call. Build the whole solution in one configuration. This is an accepted trade-off for the safety it buys; see the [industry alternatives](../05-industry-alternatives/01-di-and-composition.md) for other ways to get the same guarantee.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 8 — Config-gated registration](./08-config-gated-registration.md) · [Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration →](./10-attribute-dispatch-proxy.md)
<!-- nav -->
