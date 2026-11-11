# OoBDev.Microsoft.Caching

In-process caching provider for `OoBDev.Caching`, backed by `Microsoft.Extensions.Caching.Memory`.

## Registration

```csharp
services.TryAddMicrosoftCachingServices();
```

This calls `AddMemoryCache()` and registers `MicrosoftMemoryCachingProvider` as the singleton `ICachingProvider` and under the key `memory-cache` (`MicrosoftCachingGlobals.ProviderKey`). To select it for the attribute-driven caching of `OoBDev.Caching`, set `OoBDev:CachingProvider:Type` to `memory-cache` (see the [Caching readme](../../../Framework/OoBDev.Caching/README.Caching.md)).

## Behaviour

- The provider owns a `MemoryCache` built from the `IOptions<MemoryCacheOptions>` in the container, so `SizeLimit`, `CompactionPercentage` and `ExpirationScanFrequency` are set with `services.Configure<MemoryCacheOptions>(...)`.
- `StoreAsync` keeps the object reference for the absolute lifetime given; nothing is serialized, and a null key or null data is ignored.
- `RetrieveAsync` returns the stored object (the target type is not used) or `null`; `FlushAsync` removes one key.
- The cache is per process. Use `OoBDev.Redis.Caching` when several instances must share entries or entries must survive a restart.

Sliding expiration and entry priorities are not exposed by `ICachingProvider`; inject `IMemoryCache` (a separate cache from the provider's own) for those.

## Verification

`OoBDev.Microsoft.Caching.Tests` (Simulate) holds a worked example of `[IsCacheable]` and `[FlushCache]` through `ICacheableFactory`.
