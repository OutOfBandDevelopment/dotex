# OoBDev.Caching.Abstractions

Contracts for method caching. The implementation (manager, factory, proxy) is `OoBDev.Caching`; storage comes from a provider package (`OoBDev.Microsoft.Caching`, `OoBDev.Redis.Caching`).

## Types

**Table 1 — Caching contracts**

| Type | Role |
|------|------|
| `ICachingProvider` | Storage backend: `StoreAsync(key, data, expiration)`, `RetrieveAsync(key, targetType)`, `FlushAsync(key)`. |
| `ICachingManager` | `BuildKey(method, args)` from the attributes, plus `StoreAsync`, `RetrieveAsync` (typed and untyped) and `FlushAsync`. |
| `ICachingLock`, `NoLock` | Optional provider capability: `TryAcquireAsync(key, lifeTime)` returns a lock to dispose, or `null` when another process holds it. `ICachingManager.TryLockAsync` returns an always-granted `NoLock` when the provider has none. |
| `ICacheableFactory` | `Create<TInterface, TImplementation>()` returns the caching proxy. |
| `ServiceProviderExtensions.Cacheable<TInterface, TImplementation>()` | Shortcut for the factory; throws `NotSupportedException` when no `ICacheableFactory` is registered. |
| `[IsCacheable(keyFormatter, lifetime)]` | Marks a method whose result is cached under the formatted key for the lifetime (`TimeSpan` text, for example `"00:05:00"`). |
| `[FlushCache(keyFormatter)]`, `[FlushCache(type, methodName)]` | Flushes the formatted key, or the key of the named cacheable method for the same arguments; may be repeated on a method. |
| `CachingGlobals` | `ConfigurationPath` (`OoBDev:CachingProvider:Type`) and `SelectedKey` (`selected`). |

## Implementing a provider

```csharp
public class MyCachingProvider : ICachingProvider
{
    public Task FlushAsync(string? key) => ...;
    public Task StoreAsync(string? key, object? data, TimeSpan expiration) => ...;
    public Task<object?> RetrieveAsync(string? key, Type? targetType) => ...;
}
```

Register it under a kebab-case key and select it with the `OoBDev:CachingProvider:Type` setting (see the [Caching readme](../OoBDev.Caching/README.Caching.md)).
