# Transparent AOP Caching - Requirements

**Epic:** 04 - Distributed Caching
**Feature:** Transparent AOP Caching
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~400

---

## Overview

Transparent caching via dynamic proxy and aspect-oriented programming (AOP). Developers write zero cache logic - caching intercepted automatically at method boundaries using Castle DynamicProxy or DispatchProxy.

---

## Business Requirements

### BR-1: Zero Cache Logic in Business Code
**As a** developer
**I want** caching to be completely transparent to my business logic
**So that** I can focus on business requirements without cache boilerplate

**Acceptance Criteria:**
- Service implementations contain ONLY business logic
- No cache-check code in service methods
- No cache-set code in service methods
- Cache behavior defined via registration/attributes
- Proxies intercept method calls automatically

**Example:**
```csharp
// ✅ GOOD: Zero cache logic
public class OrderService : IOrderService
{
    public async Task<Order> GetOrderAsync(int orderId)
    {
        // ONLY business logic - no caching code
        return await _repository.GetOrderAsync(orderId);
    }
}

// ❌ BAD: Manual cache logic scattered everywhere
public class OrderService : IOrderService
{
    public async Task<Order> GetOrderAsync(int orderId)
    {
        var cacheKey = $"order:{orderId}";
        var cached = await _cache.GetAsync<Order>(cacheKey);
        if (cached != null) return cached;

        var order = await _repository.GetOrderAsync(orderId);
        await _cache.SetAsync(cacheKey, order, TimeSpan.FromMinutes(5));
        return order;
    }
}
```

---

### BR-2: Dynamic Proxy Interception
**As a** framework developer
**I want** method calls intercepted via dynamic proxy
**So that** caching is transparent and non-invasive

**Acceptance Criteria:**
- Service registered with caching proxy wrapper
- Proxy intercepts ALL interface method calls
- Cache logic executed BEFORE target method
- Target method executed ONLY on cache miss
- Return value cached for subsequent calls

---

### BR-3: Interface-Based Registration
**As a** developer
**I want** to register services with caching via simple DI extension
**So that** enabling caching is a one-line configuration change

**Acceptance Criteria:**
- Extension method: `AddCachedProxy<TInterface, TImplementation>()`
- Works with ASP.NET Core DI
- Supports options configuration
- No changes to service implementation required
- Can switch between cached/uncached via configuration

**Example:**
```csharp
// Without caching
services.AddSingleton<IOrderService, OrderService>();

// With caching (one-line change)
services.AddCachedProxy<IOrderService, OrderService>(options =>
{
    options.DefaultDuration = TimeSpan.FromMinutes(5);
    options.CacheProvider = CacheProvider.Redis;
});
```

---

### BR-4: Automatic Cache Key Generation
**As a** framework
**I want** cache keys generated automatically from method signature
**So that** developers don't manually construct cache keys

**Acceptance Criteria:**
- Cache key format: `{TypeName}.{MethodName}({Parameters})`
- Parameters serialized deterministically
- Complex objects hashed consistently
- Collision-free keys for different methods
- Configurable key prefix per service

**Example Cache Keys:**
```
OrderService.GetOrderAsync(123)
OrderService.GetOrdersByCustomerAsync(456, 2024-01-01)
ProductService.GetProductAsync(789)
```

---

### BR-5: Cache Invalidation Support
**As a** developer
**I want** mutation methods to invalidate related cache entries
**So that** stale data is never served after updates

**Acceptance Criteria:**
- Invalidation patterns: exact key, wildcard, regex
- Automatic invalidation on service methods marked for invalidation
- Manual invalidation via `ICacheInvalidationService`
- Invalidation across distributed cache nodes
- Atomic invalidation (no partial failures)

---

## Technical Requirements

### TR-1: Interface Design
```csharp
/// <summary>
/// Cache interceptor for method calls.
/// </summary>
public interface ICacheInterceptor
{
    /// <summary>
    /// Intercepts method call and applies caching logic.
    /// </summary>
    /// <param name="method">Method being called</param>
    /// <param name="args">Method arguments</param>
    /// <param name="proceed">Delegate to execute target method</param>
    /// <returns>Cached or freshly-computed result</returns>
    Task<object?> InterceptAsync(MethodInfo method, object[] args, Func<Task<object?>> proceed);
}

/// <summary>
/// Cache key builder for method calls.
/// </summary>
public interface ICacheKeyBuilder
{
    /// <summary>
    /// Builds cache key from method and arguments.
    /// </summary>
    string BuildKey(MethodInfo method, object[] args, CacheKeyOptions? options = null);

    /// <summary>
    /// Builds invalidation pattern for method.
    /// </summary>
    string BuildInvalidationPattern(MethodInfo method);
}

/// <summary>
/// Proxy factory for creating cached service proxies.
/// </summary>
public interface ICachedProxyFactory
{
    /// <summary>
    /// Creates proxy that intercepts method calls for caching.
    /// </summary>
    TInterface CreateProxy<TInterface, TImplementation>(
        TImplementation target,
        CacheProxyOptions options)
        where TInterface : class
        where TImplementation : class, TInterface;
}
```

---

### TR-2: Proxy Implementation Options

**Option 1: Castle DynamicProxy (Recommended)**
- ✅ Mature, battle-tested library
- ✅ Supports interface and class proxies
- ✅ Async-aware interception
- ✅ Comprehensive features
- ❌ External dependency

**Option 2: DispatchProxy (Built-in)**
- ✅ Built into .NET (no external dependency)
- ✅ Interface proxies only
- ✅ Lightweight
- ❌ Less feature-rich
- ❌ More manual async handling

**Decision:** Support BOTH, Castle DynamicProxy as default.

---

### TR-3: Cache Key Generation Algorithm

**Format:**
```
{KeyPrefix}:{TypeName}.{MethodName}({SerializedArgs})

Example:
app:OrderService.GetOrderAsync(123)
app:ProductService.SearchAsync("widget",10,0)
```

**Algorithm:**
1. Extract type name (short name, not fully-qualified)
2. Extract method name
3. Serialize arguments:
   - Primitives: ToString()
   - Strings: As-is
   - Complex objects: JSON serialize (deterministic)
   - Collections: Serialize items, join with `,`
4. Hash if key exceeds max length (200 chars)
5. Prepend key prefix

---

### TR-4: Interception Flow

**Sequence:**
```
1. Consumer calls method on proxy
   ↓
2. Proxy intercepts call
   ↓
3. Build cache key from method + args
   ↓
4. Check cache
   ↓
   ├─ Cache HIT → Return cached value
   │
   └─ Cache MISS
      ↓
      5. Execute target method
      ↓
      6. Cache result
      ↓
      7. Return result
```

**Async Support:**
```csharp
public async Task<object?> InterceptAsync(MethodInfo method, object[] args, Func<Task<object?>> proceed)
{
    var cacheKey = _keyBuilder.BuildKey(method, args);

    // Check cache
    var cached = await _cache.GetAsync<object>(cacheKey);
    if (cached != null)
    {
        return cached;  // Cache hit
    }

    // Execute target method
    var result = await proceed();

    // Cache result
    if (result != null)
    {
        var duration = _options.DefaultDuration;
        await _cache.SetAsync(cacheKey, result, duration);
    }

    return result;
}
```

---

### TR-5: Cache Configuration Options

```csharp
public class CacheProxyOptions
{
    /// <summary>
    /// Default cache duration (default: 5 minutes).
    /// </summary>
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Cache provider (Memory, Redis, SQL).
    /// </summary>
    public CacheProvider Provider { get; set; } = CacheProvider.Memory;

    /// <summary>
    /// Key prefix for all cached entries.
    /// </summary>
    public string KeyPrefix { get; set; } = "app";

    /// <summary>
    /// Whether to cache null results.
    /// </summary>
    public bool CacheNullValues { get; set; } = false;

    /// <summary>
    /// Maximum key length before hashing (default: 200).
    /// </summary>
    public int MaxKeyLength { get; set; } = 200;

    /// <summary>
    /// Serializer for complex objects.
    /// </summary>
    public ISerializer Serializer { get; set; } = new JsonSerializer();

    /// <summary>
    /// Whether to enable distributed locking for cache misses.
    /// </summary>
    public bool UseDistributedLock { get; set; } = true;
}
```

---

### TR-6: Performance Requirements

- **Cache overhead:** < 5ms per cached method call
- **Cache hit latency:** < 1ms for in-memory, < 10ms for Redis
- **Key generation:** < 1ms
- **Proxy creation:** < 50ms at startup (one-time cost)
- **Throughput:** 10,000+ cached calls/sec per service

---

### TR-7: Thread Safety

- Proxy instances are thread-safe
- Concurrent cache access synchronized
- Distributed lock prevents cache stampede
- Lock timeout: 30 seconds default

**Cache Stampede Prevention:**
```csharp
public async Task<object?> InterceptAsync(MethodInfo method, object[] args, Func<Task<object?>> proceed)
{
    var cacheKey = _keyBuilder.BuildKey(method, args);

    // Check cache
    var cached = await _cache.GetAsync<object>(cacheKey);
    if (cached != null) return cached;

    // Distributed lock for cache miss
    var lockKey = $"{cacheKey}:lock";
    using (var @lock = await _distributedLock.AcquireAsync(lockKey, TimeSpan.FromSeconds(30)))
    {
        // Double-check cache (another thread may have populated it)
        cached = await _cache.GetAsync<object>(cacheKey);
        if (cached != null) return cached;

        // Execute target method
        var result = await proceed();

        // Cache result
        await _cache.SetAsync(cacheKey, result, _options.DefaultDuration);
        return result;
    }
}
```

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with ASP.NET Core DI
- Compatible with .NET 10.0
- Supports async/await patterns
- Works with both interface and class proxies (if using Castle)

### NFR-2: Debuggability
- Proxy behavior is transparent in stack traces
- Logging support for cache hits/misses
- Metrics collection for cache performance
- Exception stack traces show original method

### NFR-3: Testability
- Can inject real implementation (bypass proxy)
- Mock cache for testing
- Verify cache behavior in integration tests
- Performance benchmarks for cache overhead

---

## Constraints

### C-1: Interface Requirement
- Service MUST have interface for proxy creation
- Implementation MUST implement interface
- Virtual methods required for class proxies

### C-2: Serialization Constraints
- Cached objects must be serializable
- Circular references may cause serialization failures
- Large objects (> 1MB) may impact performance

### C-3: Cache Provider Limitations
- In-memory cache does NOT scale across servers
- Redis requires network round-trip
- SQL cache slowest but most durable

---

## Success Criteria

- ✅ Zero cache logic in business code
- ✅ One-line DI registration for caching
- ✅ Automatic cache key generation
- ✅ Transparent interception via dynamic proxy
- ✅ Cache hits < 1ms (in-memory), < 10ms (Redis)
- ✅ Cache stampede prevention
- ✅ 80%+ test coverage
- ✅ Performance: < 5ms cache overhead

---

## Out of Scope

- ❌ Attribute-based caching (see AttributeBasedCaching feature)
- ❌ Background cache warming (see BackgroundTasks feature)
- ❌ Cache eviction policies (use provider-specific configs)
- ❌ Cache statistics dashboard (future enhancement)

---

## Dependencies

### Internal
- OoBDev.Framework.Caching (existing cache abstraction)
- OoBDev.System.Serialization (JSON serialization)
- OoBDev.System.Threading.DistributedLock (cache stampede prevention)

### External
- Castle.Core (DynamicProxy) - Optional
- System.Reflection.DispatchProxy (built-in)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 04 Overview](../README.md)
