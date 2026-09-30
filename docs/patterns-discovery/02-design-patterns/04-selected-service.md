# Pattern 4 — `ISelectedService<T>` — config-selected provider

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 3 — Provider / Factory with keyed services](./03-provider-factory-keyed.md) · [Pattern 5 — Config-resolved provider per channel/message →](./05-config-resolved-provider.md)
<!-- nav -->

**What:** When exactly one implementation should be active, consumers inject `ISelectedService<TService>`; its constructor reads
`configuration["OoBDev::ServiceKeys::{typeof(TService).FullName}"]` and resolves the keyed service, **falling back to the default un-keyed registration**.

```csharp
public class SelectedService<TService> : ISelectedService<TService> where TService : notnull
{
    public SelectedService(IConfiguration configuration, IServiceProvider serviceProvider)
    {
        var key = configuration[$"OoBDev::ServiceKeys::{typeof(TService).FullName}"];
        Value = serviceProvider.GetKeyedService<TService>(key)
                ?? serviceProvider.GetRequiredService<TService>();
    }
    public TService Value { get; }
}
```

Registered once as an open generic in `TryAddProviders()`; used by `CachingManager` for `ICachingProvider`.

**Repeat:** inject `ISelectedService<IThing>` in the *manager/orchestrator*, keep adapters unaware of selection.

**Rough edges (important):**

* `[ContractConfig(AllowDefault, ConfigKey)]` is declared on `ICachingProvider` and documented in the README (`OoBDev:CachingProvider:Type`) but **nothing reads it**; the runtime key is the hard-coded `OoBDev::ServiceKeys::…` path (double-colon). Either wire the attribute into `SelectedService<T>` or delete it.
* Resolution happens in the constructor, so selection is fixed for the lifetime of the wrapper (singleton ⇒ for the process).
* `IServiceProvider` injection is a service-locator; it is contained inside the wrapper, which is the right place.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 3 — Provider / Factory with keyed services](./03-provider-factory-keyed.md) · [Pattern 5 — Config-resolved provider per channel/message →](./05-config-resolved-provider.md)
<!-- nav -->
