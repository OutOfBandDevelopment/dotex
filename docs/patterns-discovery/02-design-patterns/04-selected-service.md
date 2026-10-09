# Pattern 4 — Config-selected keyed service

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 3 — Provider / Factory with keyed services](./03-provider-factory-keyed.md) · [Pattern 5 — Config-resolved provider per channel/message →](./05-config-resolved-provider.md)
<!-- nav -->

**What:** When exactly one implementation should be active, a container-registered factory picks a keyed service from a configuration path. `TryAddConfiguredKeyedService<TService>(configurationPath, selectedKey)` registers the factory under `selectedKey`; consumers inject `[FromKeyedServices(selectedKey)] TService`. With no configured value the **default un-keyed registration** is used, so registration order does not matter; a configured key that names nothing throws `InvalidOperationException` instead of falling back silently.

```csharp
// OoBDev.Caching: path "OoBDev:CachingProvider:Type", selected key "selected"
services.TryAddConfiguredKeyedService<ICachingProvider>(CachingGlobals.ConfigurationPath, CachingGlobals.SelectedKey);

public CachingManager(IStringFormatter formatter,
    [FromKeyedServices(CachingGlobals.SelectedKey)] ICachingProvider cache) { ... }
```

```json
{ "OoBDev": { "CachingProvider": { "Type": "redis" } } }
```

It replaces the earlier `ISelectedService<T>` wrapper (deleted 2026-10-09): that type read a hard-coded `OoBDev::ServiceKeys::{FullTypeName}` key in its constructor, wrapped every service in `.Value`, and an unused `[ContractConfig]` attribute duplicated the path.

**Repeat:** put the path and selected key in a `{Capability}Globals` class next to the abstraction, register the factory in the capability's `TryAdd{Capability}Services`, inject the keyed service in the manager/orchestrator, and keep adapters unaware of selection.

**Rough edges:** selection is per resolution (transient), so changing configuration at run time only affects newly created consumers; `IServiceProvider` and `IConfiguration` are used inside the factory only, which is the right place.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 3 — Provider / Factory with keyed services](./03-provider-factory-keyed.md) · [Pattern 5 — Config-resolved provider per channel/message →](./05-config-resolved-provider.md)
<!-- nav -->
