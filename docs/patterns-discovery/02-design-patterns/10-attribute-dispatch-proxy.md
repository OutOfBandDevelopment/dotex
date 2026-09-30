# Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 9 — `#if DEBUG` explicit-argument extension methods](./09-if-debug-explicit-arguments.md) · [Pattern 11 — Marker-generic channels & handlers →](./11-marker-generic-channels-handlers.md)
<!-- nav -->

**What:** Cross-cutting behavior (caching) is declared with attributes on the *implementation* method and applied by a `System.Reflection.DispatchProxy`:

```csharp
[IsCacheable("Users:{0}", "00:05:00")]  public Task<User?> GetAsync(Guid id) ...
[FlushCache(typeof(UserProvider), nameof(GetAsync))] public Task SaveAsync(User u) ...
```

```csharp
services.AddTransient<IUserProvider>(sp => sp.Cacheable<IUserProvider, UserProvider>());
```

Pieces: `IsCacheableAttribute(keyFormatter, lifetimeString)`, `ICachingManager.BuildKey(method, args)` (uses `IStringFormatter`), `ICacheableFactory.Create<TInterface,TImpl>()`, `CachedProxy<,>` (`DispatchProxy`), global kill-switch `OoBDev:Caching:Disabled`.

**Rules embedded in the design:** caching **never breaks the call** in Release (exceptions logged and swallowed); `void` and non-generic `Task` are rejected; nulls are not cached; the feature can be disabled by config without code change.

**Rough edges (under review, tracked in [`TODO.md`](../../../TODO.md)):** the proxy blocks on async (`.GetAwaiter().GetResult()`), uses reflection per call, and only works via interface. The interface-only limit is consistent with the standing preference that everything injected is consumed by interface, so it may not be a defect; the owner has not yet pinned down what the remaining concern was. `Retreive` is a spelling error baked into the public API (`RetreiveAsync`, should be `RetrieveAsync`) and is to be fixed.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 9 — `#if DEBUG` explicit-argument extension methods](./09-if-debug-explicit-arguments.md) · [Pattern 11 — Marker-generic channels & handlers →](./11-marker-generic-channels-handlers.md)
<!-- nav -->
