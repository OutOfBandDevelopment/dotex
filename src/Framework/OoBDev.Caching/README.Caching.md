# OoBDev.Caching

Attribute-driven method caching: a manager that builds cache keys and a factory that wraps a service in a caching proxy. The contracts and attributes (`ICachingProvider`, `ICachingManager`, `ICacheableFactory`, `[IsCacheable]`, `[FlushCache]`) are in `OoBDev.Caching.Abstractions`; the storage is a provider package.

## Contents

- [Registration](#registration)
- [Provider selection](#provider-selection)
- [Caching a service](#caching-a-service)
- [Failure behaviour](#failure-behaviour)

## Registration

```csharp
services.TryAddCachingServices();
```

This adds the configured `ICachingProvider` selection, `ICachingManager` (`CachingManager`) and `ICacheableFactory` (`CacheableFactory`), all transient, plus the `IStringFormatter` used for keys.

## Provider selection

The provider is chosen by configuration at `OoBDev:CachingProvider:Type` (`CachingGlobals.ConfigurationPath`) and resolved under the key `selected`; when the value is empty the default `ICachingProvider` registration is used. Provider packages and their keys:

**Table 1 — Caching providers**

| Package | Key |
|---------|-----|
| `OoBDev.Microsoft.Caching` | `memory-cache` |
| `OoBDev.Redis.Caching` | `redis` |

```json
{ "OoBDev": { "CachingProvider": { "Type": "redis" } } }
```

## Caching a service

Put the attributes on the implementation's methods, then ask the service provider for the cached proxy:

```csharp
public class ProductRepository : IProductRepository
{
    [IsCacheable("products/{id}", "00:05:00")]
    public Task<Product> GetAsync(int id) => ...;

    [FlushCache("products/{product.Id}")]
    public Task UpdateAsync(Product product) => ...;

    [FlushCache(typeof(ProductRepository), nameof(GetAsync))]
    public Task<Product> RefreshAsync(int id) => ...;
}

IProductRepository repository = serviceProvider.Cacheable<IProductRepository, ProductRepository>();
```

- `[IsCacheable(keyFormatter, lifetime)]` stores the result under the formatted key for the lifetime (`TimeSpan` text).
- `[FlushCache(keyFormatter)]` flushes the formatted key; `[FlushCache(type, methodName)]` flushes the key the named cacheable method would build for the same arguments.
- Key formatters use `{parameter}` and `{parameter.Property}` placeholders over the method arguments.
- `ICachingManager` can also be used directly: `BuildKey`, `StoreAsync`, `RetrieveAsync` and `FlushAsync`.

## Failure behaviour

A caching failure is logged and swallowed so the call still runs, except in DEBUG builds, which rethrow so a broken key or provider shows up locally.

Tests: `OoBDev.Caching.Tests`, plus the provider test projects (`OoBDev.Microsoft.Caching.Tests` holds the worked example).
