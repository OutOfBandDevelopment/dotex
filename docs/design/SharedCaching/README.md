# Shared Caching

Design documents for the caching features of the SharedFramework (Epic 4). Each feature has four documents: requirements, architecture, API design and testing strategy.

## Contents

- [Features](#features)
- [Implementation status](#implementation-status)

## Features

**Table 1 — Shared Caching documents**

| Feature | Requirements | Architecture | API design | Testing strategy |
|---------|--------------|--------------|------------|------------------|
| Attribute-based caching | [requirements](AttributeBasedCaching/requirements.md) | [architecture](AttributeBasedCaching/architecture.md) | [api-design](AttributeBasedCaching/api-design.md) | [testing-strategy](AttributeBasedCaching/testing-strategy.md) |
| Background tasks | [requirements](BackgroundTasks/requirements.md) | [architecture](BackgroundTasks/architecture.md) | [api-design](BackgroundTasks/api-design.md) | [testing-strategy](BackgroundTasks/testing-strategy.md) |
| Transparent AOP caching | [requirements](TransparentAopCaching/requirements.md) | [architecture](TransparentAopCaching/architecture.md) | [api-design](TransparentAopCaching/api-design.md) | [testing-strategy](TransparentAopCaching/testing-strategy.md) |

## Implementation status

Reconciled with `src/Framework/OoBDev.Caching` and `OoBDev.Caching.Abstractions` on 2026-11-06. The designs are proposals; the code is the current truth where they differ.

**Table 2 — Design names against the code**

| Design | Code | State |
|--------|------|-------|
| `ICachedProxyFactory`, `CachedProxyFactory`, `AddCachedProxy` | `ICacheableFactory.Create<TInterface, TImplementation>()`, `CacheableFactory`, `CachedProxy` (a `DispatchProxy`) | built under other names; the code names win |
| `CacheableAttribute` | `IsCacheableAttribute(keyFormatter, lifetimeSpan)` | built under another name |
| `CacheInvalidateAttribute` | `FlushCacheAttribute(keyFormatter)` and `FlushCacheAttribute(targetClass, methodName)`, repeatable | built under another name |
| `ICacheService`, `CacheProvider` | `ICachingManager` (`BuildKey`, store, retrieve, `FlushAsync`) over `ICachingProvider` (provider selection by configured key) | built under other names |
| `IBackgroundTask`, `IBackgroundTaskScheduler`, `CacheWarmingTask`, `CacheEvictionTask` | none | not built |

The Castle, Quartz and Hangfire variants named in the designs are not used: the code uses the platform `DispatchProxy`. Background tasks stay a design only until a consumer needs cache warming (hosted services are the platform option).
