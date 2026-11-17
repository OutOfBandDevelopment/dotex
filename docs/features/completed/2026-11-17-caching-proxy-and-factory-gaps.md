# Feature request: caching proxy and factory gaps

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#cached-proxy-passes-void-and-task-methods-through))
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.Caching` (`CachedProxy`, `CacheableFactory`)

## Need

The attribute-based caching proxy has no defined behavior for `void` methods and non-generic `Task` returns (a cache entry makes no sense there), and the factory builds a typed `ILogger<T>` where an `ILoggerFactory` may be the better dependency.

## Proposed behavior

Calls to `void` and non-generic `Task` methods on a cached interface are passed through and never cached, or rejected when the proxy is built, with a clear message. Decide between `ILogger<T>` and `ILoggerFactory` for the factory and document it.

## Acceptance criteria

- Given a `[Cacheable]` void method, when called, then it is not cached and does not throw an unexpected exception (or the build fails with a clear message).
- Given a non-generic `Task` method, then the same applies.

## Existing code checked

`docs/design/SharedCaching/AttributeBasedCaching` (api-design and testing-strategy name `CachedProxy`) does not state the rule for these return types.

Source comments (replaced by a reference to this request):

- `src/Framework/OoBDev.Caching/Factories/CachedProxy.cs`: "add exception for void and non generic tasks".
- `src/Framework/OoBDev.Caching/Factories/CacheableFactory.cs`: "what if this took ILoggerFactory instead".

## Security and operations

None.

## Priority

- **Impact:** surprising behavior on unsupported signatures.
- **Needed by:** no date.
- **Workaround today:** do not mark such methods cacheable.

## Outcome

- A `[IsCacheable]` void or non-generic `Task` method is called through, never cached, with a warning log; it no longer throws in DEBUG builds. Rule recorded in [api-design.md](../../design/SharedCaching/AttributeBasedCaching/api-design.md).
- `CacheableFactory` keeps `ILogger<T>` (declined `ILoggerFactory`: the category is already the decorated type).
