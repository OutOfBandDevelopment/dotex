# Caching Framework Architecture

**Version:** 1.0.0
**Last Updated:** 2026-01-20

---

## Table of Contents

1. [Architectural Overview](#architectural-overview)
2. [Component Design](#component-design)
3. [Design Patterns](#design-patterns)
4. [Data Flow](#data-flow)
5. [Thread Safety](#thread-safety)
6. [Performance Considerations](#performance-considerations)
7. [Extension Points](#extension-points)

---

## Architectural Overview

The OoBDev Caching Framework follows a **layered architecture** with clear separation of concerns:

```
┌────────────────────────────────────────────────────────────┐
│                    Application Layer                        │
│  - Business logic with [IsCacheable] attributes            │
│  - Dependency injection setup                               │
└────────────────────┬───────────────────────────────────────┘
                     │
┌────────────────────▼───────────────────────────────────────┐
│                   Factory Layer                             │
│  - CacheableFactory: Creates proxies                        │
│  - ICacheableFactory: Factory interface                     │
└────────────────────┬───────────────────────────────────────┘
                     │
┌────────────────────▼───────────────────────────────────────┐
│                    Proxy Layer                              │
│  - CachedProxy: Dynamic method interception                │
│  - ResultAwaiter: Task unwrapping utilities                │
└────────────────────┬───────────────────────────────────────┘
                     │
┌────────────────────▼───────────────────────────────────────┐
│                  Manager Layer                              │
│  - CachingManager: Orchestrates caching operations         │
│  - ICachingManager: Manager interface                      │
└────────────────────┬───────────────────────────────────────┘
                     │
┌────────────────────▼───────────────────────────────────────┐
│                  Provider Layer                             │
│  - ICachingProvider: Abstraction for storage               │
│  - Implementations: Redis, Microsoft Memory Cache          │
└────────────────────────────────────────────────────────────┘
```

### Design Principles

1. **Separation of Concerns** - Each layer has a single responsibility
2. **Dependency Inversion** - Depend on abstractions (interfaces), not implementations
3. **Open/Closed Principle** - Open for extension (new providers), closed for modification
4. **Interface Segregation** - Small, focused interfaces
5. **Provider Pattern** - Pluggable backends without changing application code

---

## Component Design

### 1. Abstractions Layer (`OoBDev.Caching.Abstractions`)

#### ICachingProvider

**Purpose:** Abstraction for cache storage backends

```csharp
public interface ICachingProvider
{
    /// <summary>
    /// Store data in cache with expiration
    /// </summary>
    Task StoreAsync(string key, object data, TimeSpan expiration);

    /// <summary>
    /// Retrieve data from cache
    /// </summary>
    /// <returns>Cached object or null if not found/expired</returns>
    Task<object?> RetreiveAsync(string key, Type targetType);

    /// <summary>
    /// Remove data from cache
    /// </summary>
    Task FlushAsync(string key);
}
```

**Implementation Requirements:**
- **Null-safe**: Handle null keys gracefully (no-op)
- **Type-safe**: Support object serialization/deserialization
- **Thread-safe**: Support concurrent access
- **Expiration**: Honor time-based expiration

#### ICachingManager

**Purpose:** High-level orchestration of caching operations

```csharp
public interface ICachingManager
{
    /// <summary>
    /// Build cache key from method metadata
    /// </summary>
    string BuildKey(MethodInfo method, object[] args);

    /// <summary>
    /// Store data in cache
    /// </summary>
    Task StoreAsync(string key, object data, TimeSpan expiration);

    /// <summary>
    /// Retrieve data from cache
    /// </summary>
    Task<object?> RetreiveAsync(string key, Type targetType);

    /// <summary>
    /// Flush data from cache
    /// </summary>
    Task FlushAsync(string key);
}
```

**Responsibilities:**
- Key generation from method signatures
- Delegation to configured provider
- Attribute parsing and parameter substitution

#### IsCacheableAttribute

**Purpose:** Declarative caching for methods

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class IsCacheableAttribute : Attribute
{
    /// <summary>
    /// Cache key template with parameter placeholders
    /// Example: "user:{userId}/profile"
    /// </summary>
    public string KeyTemplate { get; }

    /// <summary>
    /// Cache duration (TimeSpan format: "HH:MM:SS")
    /// Example: "01:30:00" for 1.5 hours
    /// </summary>
    public string Duration { get; }

    public IsCacheableAttribute(string keyTemplate, string duration)
    {
        KeyTemplate = keyTemplate;
        Duration = duration;
    }
}
```

**Usage:**
```csharp
[IsCacheable("product:{productId}", "00:30:00")]
public async Task<Product> GetProductAsync(int productId)
{
    return await _database.QueryAsync<Product>(productId);
}
```

#### FlushCacheAttribute

**Purpose:** Declarative cache invalidation

```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class FlushCacheAttribute : Attribute
{
    /// <summary>
    /// Cache key template to flush
    /// </summary>
    public string KeyTemplate { get; }

    /// <summary>
    /// Target method to flush (optional)
    /// </summary>
    public Type? TargetType { get; }
    public string? TargetMethodName { get; }

    public FlushCacheAttribute(string keyTemplate)
    {
        KeyTemplate = keyTemplate;
    }

    public FlushCacheAttribute(Type targetType, string targetMethodName)
    {
        TargetType = targetType;
        TargetMethodName = targetMethodName;
    }
}
```

**Usage:**
```csharp
[FlushCache("product:{productId}")]
public async Task UpdateProductAsync(int productId, Product product)
{
    await _database.UpdateAsync(product);
    // Cache flushed automatically
}

// OR flush by referencing another method
[FlushCache(typeof(ProductRepository), nameof(GetProductAsync))]
public async Task UpdateProductAsync(int productId, Product product)
{
    await _database.UpdateAsync(product);
}
```

---

### 2. Implementation Layer (`OoBDev.Caching`)

#### CacheableFactory

**Purpose:** Creates dynamic proxy instances with caching behavior

```csharp
public class CacheableFactory : ICacheableFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICachingManager _cachingManager;
    private readonly IConfiguration _configuration;

    public const string DisabledConfigurationKey = "OoBDev:Caching:Disabled";

    public TInterface Create<TInterface, TImplementation>()
        where TImplementation : TInterface
    {
        // Check if caching is disabled
        if (_configuration[DisabledConfigurationKey] == "true")
        {
            return ActivatorUtilities.CreateInstance<TImplementation>(_serviceProvider);
        }

        // Create implementation instance
        var implementation = ActivatorUtilities.CreateInstance<TImplementation>(_serviceProvider);

        // Create logger for proxy
        var logger = _serviceProvider.GetService<ILogger<TImplementation>>();

        // Wrap in caching proxy
        return CachedProxy<TInterface, TImplementation>.Create(
            implementation,
            _cachingManager,
            logger
        );
    }
}
```

**Design Decisions:**
- **Configuration-based disable**: Support `appsettings.json` toggle for caching
- **Lazy creation**: Only create instances when requested
- **Logger injection**: Provide diagnostic logging for cache hits/misses

#### CachedProxy<TInterface, TImplementation>

**Purpose:** Dynamic proxy that intercepts method calls and applies caching logic

```csharp
public class CachedProxy<TInterface, TImplementation> : DispatchProxy
    where TImplementation : TInterface
{
    private TImplementation _decorated;
    private ICachingManager _cachingManager;
    private ILogger _logger;

    public static TInterface Create(
        TImplementation decorated,
        ICachingManager cachingManager,
        ILogger logger)
    {
        var proxy = Create<TInterface, CachedProxy<TInterface, TImplementation>>();
        ((CachedProxy<TInterface, TImplementation>)proxy).Initialize(
            decorated, cachingManager, logger);
        return proxy;
    }

    protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
    {
        // 1. Check for [IsCacheable] attribute
        var isCacheableAttr = targetMethod.GetCustomAttribute<IsCacheableAttribute>();
        if (isCacheableAttr != null)
        {
            return HandleCacheableMethod(targetMethod, args, isCacheableAttr);
        }

        // 2. Check for [FlushCache] attribute
        var flushCacheAttr = targetMethod.GetCustomAttribute<FlushCacheAttribute>();
        if (flushCacheAttr != null)
        {
            return HandleFlushCacheMethod(targetMethod, args, flushCacheAttr);
        }

        // 3. No caching - invoke method directly
        return targetMethod.Invoke(_decorated, args);
    }
}
```

**Method Interception Logic:**

```
┌─────────────────────────────────────────────────────────┐
│  Method Call: repository.GetUserAsync(123)              │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────▼────────────┐
         │ Proxy.Invoke() called  │
         └───────────┬────────────┘
                     │
         ┌───────────▼────────────────────┐
         │ Check for [IsCacheable]?       │
         └─┬─────────────────────────┬────┘
           │ YES                      │ NO
           │                          │
    ┌──────▼──────────┐        ┌─────▼──────┐
    │ Build cache key │        │ Invoke     │
    │ from template   │        │ method     │
    └──────┬──────────┘        │ directly   │
           │                   └────────────┘
    ┌──────▼──────────┐
    │ Check cache     │
    └──────┬──────────┘
           │
     ┌─────┴─────┐
     │           │
  ┌──▼──┐   ┌───▼────┐
  │ HIT │   │ MISS   │
  └──┬──┘   └───┬────┘
     │          │
     │   ┌──────▼──────────┐
     │   │ Invoke method   │
     │   └──────┬──────────┘
     │          │
     │   ┌──────▼──────────┐
     │   │ Store in cache  │
     │   └──────┬──────────┘
     │          │
  ┌──▼──────────▼──┐
  │ Return result  │
  └────────────────┘
```

#### CachingManager

**Purpose:** Implements key building and delegates to provider

```csharp
public class CachingManager : ICachingManager
{
    private readonly IStringFormatter _stringFormatter;
    private readonly ISelectedService<ICachingProvider> _cacheProvider;

    public string BuildKey(MethodInfo method, object[] args)
    {
        // 1. Find attribute on method
        var isCacheableAttr = method.GetCustomAttribute<IsCacheableAttribute>();
        var flushCacheAttr = method.GetCustomAttribute<FlushCacheAttribute>();

        if (isCacheableAttr == null && flushCacheAttr == null)
            throw new ApplicationException("Method must have [IsCacheable] or [FlushCache] attribute");

        var keyTemplate = isCacheableAttr?.KeyTemplate ?? flushCacheAttr?.KeyTemplate;

        // 2. Handle FlushCache with target method reference
        if (flushCacheAttr?.TargetType != null && flushCacheAttr.TargetMethodName != null)
        {
            var targetMethod = flushCacheAttr.TargetType.GetMethod(flushCacheAttr.TargetMethodName);
            var targetAttr = targetMethod.GetCustomAttribute<IsCacheableAttribute>();
            keyTemplate = targetAttr.KeyTemplate;
            method = targetMethod; // Use target method for parameter binding
        }

        // 3. Format key template with parameters
        return _stringFormatter.Format(keyTemplate, method, args);
    }

    public Task StoreAsync(string key, object data, TimeSpan expiration)
        => _cacheProvider.Value.StoreAsync(key, data, expiration);

    public Task<object?> RetreiveAsync(string key, Type targetType)
        => _cacheProvider.Value.RetreiveAsync(key, targetType);

    public Task FlushAsync(string key)
        => _cacheProvider.Value.FlushAsync(key);
}
```

**Key Building Algorithm:**
1. Extract attribute from method
2. Get key template from attribute
3. Replace placeholders with parameter values using `IStringFormatter`
4. Example: `"user:{userId}"` + `args = [123]` → `"user:123"`

---

### 3. Provider Layer

#### RedisCachingProvider (`OoBDev.Redis.Caching`)

**Purpose:** Distributed caching using StackExchange.Redis

```csharp
public class RedisCachingProvider : ICachingProvider
{
    private readonly Lazy<IConnectionMultiplexer> _redis;
    private readonly IObjectConverter _converter;

    public async Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var db = _redis.Value.GetDatabase();
        var json = await _converter.ToJsonAsync(data);
        await db.StringSetAsync(key, json, expiration);
    }

    public async Task<object?> RetreiveAsync(string key, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        var db = _redis.Value.GetDatabase();
        var value = await db.StringGetAsync(key);

        if (value.IsNullOrEmpty) return null;

        return await _converter.ConvertAsync(value.ToString(), targetType);
    }

    public async Task FlushAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return;

        var db = _redis.Value.GetDatabase();
        await db.KeyDeleteAsync(key);
    }
}
```

**Features:**
- **JSON Serialization**: Uses `IObjectConverter` for type-safe serialization
- **Lazy Connection**: ConnectionMultiplexer created on first use
- **Distributed**: Shared cache across multiple application instances
- **Persistent**: Data survives application restarts (configurable)

#### MicrosoftMemoryCachingProvider (`OoBDev.Microsoft.Caching`)

**Purpose:** In-memory caching using IMemoryCache

```csharp
public class MicrosoftMemoryCachingProvider : ICachingProvider, IDisposable
{
    private readonly IMemoryCache _cache;

    public Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;

        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        _cache.Set(key, data, options);
        return Task.CompletedTask;
    }

    public Task<object?> RetreiveAsync(string key, Type targetType)
    {
        if (string.IsNullOrWhiteSpace(key)) return Task.FromResult<object?>(null);

        _cache.TryGetValue(key, out var value);
        return Task.FromResult(value);
    }

    public Task FlushAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;

        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
```

**Features:**
- **Fast**: No serialization, direct object references
- **Memory-efficient**: Automatic eviction based on memory pressure
- **Local**: Cache isolated to single application instance
- **Synchronous**: No network latency

---

## Design Patterns

### 1. Provider Pattern

**Definition:** Abstract a capability behind an interface, with multiple interchangeable implementations

**Benefits:**
- Swap providers without changing application code
- Easy testing with mock providers
- Add new providers without modifying existing code

**Example:**
```csharp
// Define abstraction
public interface ICachingProvider { ... }

// Implement providers
public class RedisCachingProvider : ICachingProvider { ... }
public class MicrosoftMemoryCachingProvider : ICachingProvider { ... }

// Use via abstraction
public class CachingManager
{
    private readonly ICachingProvider _provider;
    // Works with any provider
}
```

### 2. Factory Pattern

**Definition:** Encapsulate object creation logic

**Benefits:**
- Centralize complex creation logic
- Support conditional creation (caching enabled/disabled)
- Inject dependencies automatically

**Example:**
```csharp
public class CacheableFactory
{
    public TInterface Create<TInterface, TImplementation>()
    {
        if (cachingDisabled)
            return CreateDirect<TImplementation>();
        else
            return CreateProxy<TInterface, TImplementation>();
    }
}
```

### 3. Proxy Pattern (Dynamic Proxy)

**Definition:** Wrap an object to intercept method calls

**Benefits:**
- Transparent caching (no code changes to cached classes)
- Centralized cross-cutting concerns
- Attribute-driven behavior

**Example:**
```csharp
public class CachedProxy<TInterface, TImplementation> : DispatchProxy
{
    protected override object? Invoke(MethodInfo targetMethod, object[] args)
    {
        // Check cache, invoke method, store result
    }
}
```

### 4. Decorator Pattern

**Definition:** Enhance object behavior without modifying the object

**Benefits:**
- Original class remains unchanged
- Behavior can be added/removed dynamically
- Multiple decorators can be composed

**Example:**
```csharp
// Original implementation
public class UserRepository : IUserRepository { ... }

// Decorated with caching
var cached = CachedProxy.Create(new UserRepository(), ...);
```

---

## Data Flow

### Cacheable Method Call Flow

```
1. Client Code
   └─> repository.GetUserAsync(123)

2. Proxy Layer (CachedProxy)
   ├─> Extract [IsCacheable("user:{userId}", "01:00:00")]
   ├─> Build key via CachingManager
   │   └─> "user:123"
   ├─> Check cache via CachingManager
   │   └─> ICachingProvider.RetreiveAsync("user:123", typeof(User))
   │
   ├─> Cache HIT?
   │   ├─> YES: Return cached value
   │   └─> NO: Continue to step 3
   │
   └─> Invoke original method
       └─> userRepository.GetUserAsync(123)
           └─> Result: User object

3. Store in Cache
   └─> ICachingProvider.StoreAsync("user:123", userObject, TimeSpan.FromHours(1))

4. Return Result
   └─> User object
```

### FlushCache Method Call Flow

```
1. Client Code
   └─> repository.UpdateUserAsync(123, updatedUser)

2. Proxy Layer (CachedProxy)
   ├─> Extract [FlushCache("user:{userId}")]
   ├─> Build key via CachingManager
   │   └─> "user:123"
   │
   └─> Invoke original method
       └─> userRepository.UpdateUserAsync(123, updatedUser)
           └─> Update database

3. Flush Cache
   └─> ICachingProvider.FlushAsync("user:123")

4. Return Result
   └─> void (or Task)
```

---

## Thread Safety

### Provider Thread Safety

**RedisCachingProvider:**
- ✅ Thread-safe: StackExchange.Redis is thread-safe
- ✅ Connection multiplexing: Shared connection across threads
- ✅ Atomic operations: Redis commands are atomic

**MicrosoftMemoryCachingProvider:**
- ✅ Thread-safe: IMemoryCache is thread-safe
- ✅ Concurrent access: Multiple threads can read/write simultaneously
- ⚠️ **Race condition possible**: Check-then-act patterns need locking

### Proxy Thread Safety

**CachedProxy:**
- ✅ Stateless: No shared mutable state
- ✅ Concurrent calls: Multiple threads can invoke methods safely
- ⚠️ **Cache stampede**: Multiple threads may invoke expensive method simultaneously on cache miss

**Mitigation for Cache Stampede:**
```csharp
// Use SemaphoreSlim to ensure only one thread loads data
private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

public async Task<User> GetUserAsync(int userId)
{
    var key = $"user:{userId}";
    var cached = await _cachingManager.RetreiveAsync(key, typeof(User));
    if (cached != null) return (User)cached;

    await _semaphore.WaitAsync();
    try
    {
        // Double-check after acquiring lock
        cached = await _cachingManager.RetreiveAsync(key, typeof(User));
        if (cached != null) return (User)cached;

        // Load from database
        var user = await _database.GetUserAsync(userId);
        await _cachingManager.StoreAsync(key, user, TimeSpan.FromHours(1));
        return user;
    }
    finally
    {
        _semaphore.Release();
    }
}
```

---

## Performance Considerations

### Memory Cache Performance

**Characteristics:**
- **Read Latency:** ~0.05 ms (50 microseconds)
- **Write Latency:** ~0.05 ms
- **Throughput:** 10M+ operations/second
- **Memory:** Configurable size limits, automatic eviction

**When to Use:**
- Single-instance applications
- Low-latency requirements (< 1 ms)
- Frequently accessed data
- Data that can be lost on restart

### Redis Cache Performance

**Characteristics:**
- **Read Latency:** ~1-5 ms (local), ~5-15 ms (cloud)
- **Write Latency:** ~1-5 ms (local), ~5-15 ms (cloud)
- **Throughput:** 100K+ operations/second
- **Memory:** Server-side configuration

**When to Use:**
- Multi-instance applications
- Shared cache required
- Data persistence needed
- Moderate latency acceptable (< 10 ms)

### Optimization Strategies

**1. Minimize Serialization Overhead**
```csharp
// Use compact serialization formats
services.Configure<JsonSerializerOptions>(options =>
{
    options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});
```

**2. Batch Operations**
```csharp
// Batch cache operations to reduce network round-trips
var pipeline = _redis.CreateBatch();
var tasks = keys.Select(key => pipeline.StringGetAsync(key));
pipeline.Execute();
await Task.WhenAll(tasks);
```

**3. Short Expirations for Hot Data**
```csharp
// Use shorter expirations for frequently changing data
[IsCacheable("trending:{categoryId}", "00:05:00")] // 5 minutes
public async Task<Product[]> GetTrendingProductsAsync(int categoryId)
```

**4. Hybrid Caching**
```csharp
// Use memory cache as L1, Redis as L2
var cached = await _memoryCache.RetreiveAsync(key, type);
if (cached == null)
{
    cached = await _redisCache.RetreiveAsync(key, type);
    if (cached != null)
        await _memoryCache.StoreAsync(key, cached, TimeSpan.FromMinutes(5));
}
```

---

## Extension Points

### Custom Providers

Implement `ICachingProvider` for new backends:

```csharp
public class CosmosDbCachingProvider : ICachingProvider
{
    private readonly CosmosClient _client;

    public async Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        var container = _client.GetContainer("cache", "items");
        await container.UpsertItemAsync(new CacheItem
        {
            Id = key,
            Data = data,
            ExpiresAt = DateTimeOffset.UtcNow.Add(expiration)
        });
    }

    // ... implement other methods
}

// Register in DI
services.AddSingleton<ICachingProvider, CosmosDbCachingProvider>();
```

### Custom Key Formatters

Implement `IStringFormatter` for custom key logic:

```csharp
public class CustomStringFormatter : IStringFormatter
{
    public string Format(string template, MethodInfo method, object[] args)
    {
        // Custom placeholder logic
        // Example: Support complex object properties
        // "user:{user.Id}/profile" instead of "user:{userId}"
    }
}

// Register in DI
services.AddSingleton<IStringFormatter, CustomStringFormatter>();
```

### Custom Cache Eviction Policies

```csharp
public class LruCachingProvider : ICachingProvider
{
    private readonly Dictionary<string, CacheEntry> _cache = new();
    private readonly LinkedList<string> _accessOrder = new();

    public Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        // Implement LRU eviction logic
        if (_cache.Count >= _maxSize)
        {
            var oldest = _accessOrder.First.Value;
            _cache.Remove(oldest);
            _accessOrder.RemoveFirst();
        }

        _cache[key] = new CacheEntry { Data = data, ExpiresAt = DateTimeOffset.UtcNow.Add(expiration) };
        _accessOrder.AddLast(key);
    }

    // ... implement other methods with LRU tracking
}
```

---

## Summary

The OoBDev Caching Framework provides a **flexible, extensible architecture** for distributed and in-memory caching:

**Key Strengths:**
- ✅ **Separation of concerns** via layered architecture
- ✅ **Provider pattern** for pluggable backends
- ✅ **Dynamic proxies** for transparent caching
- ✅ **Attribute-driven** declarative configuration
- ✅ **Thread-safe** implementations
- ✅ **Performance-optimized** for common scenarios
- ✅ **Extensible** via interfaces and DI

**Design Tradeoffs:**
- **Complexity:** Dynamic proxies add complexity vs. manual caching
- **Performance:** Proxy overhead (~0.01 ms) vs. zero abstraction
- **Flexibility:** Provider pattern adds indirection vs. direct usage

**When to Use:**
- ✅ Multiple caching scenarios (development, staging, production)
- ✅ Need to swap providers without code changes
- ✅ Prefer declarative over imperative caching
- ✅ Want consistent caching patterns across projects

**When NOT to Use:**
- ❌ Maximum performance required (use direct caching)
- ❌ Simple single-provider scenario (use IMemoryCache directly)
- ❌ No need for attribute-based caching
