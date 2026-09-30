# Pattern 9 — `#if DEBUG` explicit-argument extension methods

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 8 — Config-gated registration](./08-config-gated-registration.md) · [Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration →](./10-attribute-dispatch-proxy.md)
<!-- nav -->

**What:** Optional parameters are optional in **Release** and **required in Debug**, so developers see every knob while building but consumers get one-liners:

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

**Rough edge:** it makes the *compiled API differ between configurations* – a library built in Debug and consumed in Release code (or vice-versa via project reference) will not compile the same call. Prefer overloads if the goal is discoverability.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 8 — Config-gated registration](./08-config-gated-registration.md) · [Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration →](./10-attribute-dispatch-proxy.md)
<!-- nav -->
