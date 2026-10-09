# Pattern 1 — Abstractions + Implementation + Registration Extension

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern Interaction Cheat-Sheet](./00-pattern-interaction.md) · [Pattern 2 — `TryAdd*` everywhere →](./02-tryadd-everywhere.md)
<!-- nav -->

**What:** A capability = `X.Abstractions` (contracts) + `X` (defaults) + `ServiceCollectionExtensions` in the implementation (one public `TryAddXServices`).

**See:** `Framework/OoBDev.Caching.Abstractions`, `Framework/OoBDev.Caching/ServiceCollectionExtensions.cs`.

```csharp
public static IServiceCollection TryAddCachingServices(this IServiceCollection services)
{
    services.TryAddProviders();                                   // shared dependencies first
    services.TryAddTransient<ICachingManager, CachingManager>();
    services.TryAddTransient<ICacheableFactory, CacheableFactory>();
    return services;                                              // always fluent
}
```

**Repeat:** name the method `TryAdd{Capability}Services` (or `…Extensions` for cross-cutting); call the dependencies' `TryAdd*` at the top; return `services`.

**Rough edges:** the static class used to be named `ServiceCollectionEx` in six projects. **Done 2026-10-09:** `ServiceCollectionExtensions` everywhere. Extension-method call sites were unaffected because only the class name changed.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern Interaction Cheat-Sheet](./00-pattern-interaction.md) · [Pattern 2 — `TryAdd*` everywhere →](./02-tryadd-everywhere.md)
<!-- nav -->
