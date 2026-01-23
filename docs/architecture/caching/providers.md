# Caching Provider Pattern Guide

**Version:** 1.0.0
**Last Updated:** 2026-01-20

---

## Table of Contents

1. [Provider Pattern Overview](#provider-pattern-overview)
2. [Implementing a Custom Provider](#implementing-a-custom-provider)
3. [Built-in Providers](#built-in-providers)
4. [Provider Selection](#provider-selection)
5. [Advanced Patterns](#advanced-patterns)
6. [Testing Providers](#testing-providers)

---

## Provider Pattern Overview

The **Provider Pattern** enables pluggable caching backends without changing application code. All providers implement the `ICachingProvider` interface, allowing seamless swapping via dependency injection.

### Interface Contract

```csharp
namespace OoBDev.Caching.Abstractions
{
    public interface ICachingProvider
    {
        /// <summary>
        /// Store data in cache with expiration
        /// </summary>
        /// <param name="key">Unique cache key</param>
        /// <param name="data">Data to cache</param>
        /// <param name="expiration">Time until expiration</param>
        Task StoreAsync(string key, object data, TimeSpan expiration);

        /// <summary>
        /// Retrieve data from cache
        /// </summary>
        /// <param name="key">Unique cache key</param>
        /// <param name="targetType">Expected return type</param>
        /// <returns>Cached object or null if not found/expired</returns>
        Task<object?> RetreiveAsync(string key, Type targetType);

        /// <summary>
        /// Remove data from cache
        /// </summary>
        /// <param name="key">Unique cache key</param>
        Task FlushAsync(string key);
    }
}
```

### Implementation Requirements

**All providers MUST:**
1. ✅ **Handle null keys gracefully** - Return/no-op, don't throw exceptions
2. ✅ **Be thread-safe** - Support concurrent access from multiple threads
3. ✅ **Honor expiration** - Automatically remove/reject expired entries
4. ✅ **Return null on miss** - `RetreiveAsync` returns null if key not found or expired
5. ✅ **Support serialization** - Convert objects to storable format (if needed)
6. ✅ **Be disposable** (if needed) - Implement `IDisposable` for cleanup

---

## Implementing a Custom Provider

### Step 1: Create Provider Class

```csharp
using OoBDev.Caching.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace MyApp.Caching
{
    public class InMemoryDictionaryCachingProvider : ICachingProvider, IDisposable
    {
        private readonly ConcurrentDictionary<string, CacheEntry> _cache = new();
        private readonly System.Threading.Timer _cleanupTimer;

        private class CacheEntry
        {
            public object Data { get; set; }
            public DateTimeOffset ExpiresAt { get; set; }

            public bool IsExpired => DateTimeOffset.UtcNow >= ExpiresAt;
        }

        public InMemoryDictionaryCachingProvider()
        {
            // Cleanup expired entries every 60 seconds
            _cleanupTimer = new System.Threading.Timer(
                _ => CleanupExpiredEntries(),
                null,
                TimeSpan.FromSeconds(60),
                TimeSpan.FromSeconds(60)
            );
        }

        public Task StoreAsync(string key, object data, TimeSpan expiration)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;

            _cache[key] = new CacheEntry
            {
                Data = data,
                ExpiresAt = DateTimeOffset.UtcNow.Add(expiration)
            };

            return Task.CompletedTask;
        }

        public Task<object?> RetreiveAsync(string key, Type targetType)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.FromResult<object?>(null);

            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.IsExpired)
                {
                    _cache.TryRemove(key, out _);
                    return Task.FromResult<object?>(null);
                }

                return Task.FromResult<object?>(entry.Data);
            }

            return Task.FromResult<object?>(null);
        }

        public Task FlushAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key)) return Task.CompletedTask;

            _cache.TryRemove(key, out _);
            return Task.CompletedTask;
        }

        private void CleanupExpiredEntries()
        {
            var expiredKeys = _cache
                .Where(kvp => kvp.Value.IsExpired)
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                _cache.TryRemove(key, out _);
            }
        }

        public void Dispose()
        {
            _cleanupTimer?.Dispose();
            _cache.Clear();
        }
    }
}
```

### Step 2: Create Registrar

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Caching.Abstractions;

namespace MyApp.Caching
{
    /// <summary>
    /// Service registration for in-memory dictionary caching provider.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class InMemoryDictionaryCachingRegistrar
    {
        public IServiceCollection AddServices(IServiceCollection services)
        {
            services.AddSingleton<ICachingProvider, InMemoryDictionaryCachingProvider>();
            return services;
        }
    }
}
```

### Step 3: Create Extension Method

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace MyApp.Caching
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInMemoryDictionaryCachingServices(
            this IServiceCollection services)
        {
            return new InMemoryDictionaryCachingRegistrar().AddServices(services);
        }
    }
}
```

### Step 4: Register in Application

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Caching;
using MyApp.Caching;

var services = new ServiceCollection();

// Core caching services
services.AddOoBDevCachingServices();

// Register custom provider
services.AddInMemoryDictionaryCachingServices();

var serviceProvider = services.BuildServiceProvider();
```

---

## Built-in Providers

### 1. Redis Caching Provider

**Package:** `OoBDev.Redis.Caching`

**Backend:** StackExchange.Redis

**Use Case:** Distributed caching across multiple application instances

**Features:**
- ✅ Distributed cache shared across instances
- ✅ Persistent storage (optional)
- ✅ High throughput (100K+ ops/sec)
- ✅ JSON serialization via `IObjectConverter`
- ✅ Connection pooling via `IConnectionMultiplexer`

**Configuration:**
```csharp
services.AddRedisCachingServices();

// appsettings.json
{
  "Redis": {
    "ConnectionString": "localhost:6379,ssl=false"
  }
}
```

**Pros:**
- Shared cache across instances
- Survives application restarts
- Supports complex data structures

**Cons:**
- Network latency (~1-5 ms locally, ~5-15 ms cloud)
- Requires Redis server
- Serialization overhead

**Full Documentation:** [OoBDev.Redis.Caching/README.md](../../../ExternalServices/Redis/OoBDev.Redis.Caching/README.md)

---

### 2. Microsoft Memory Cache Provider

**Package:** `OoBDev.Microsoft.Caching`

**Backend:** Microsoft.Extensions.Caching.Memory.IMemoryCache

**Use Case:** Fast in-memory caching for single-instance applications

**Features:**
- ✅ In-memory storage (no serialization)
- ✅ Ultra-fast access (< 1 ms)
- ✅ Automatic memory pressure handling
- ✅ Eviction policies (LRU, size limits)
- ✅ Zero external dependencies

**Configuration:**
```csharp
services.AddMicrosoftCachingServices();

// Optional: Configure memory limits
services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 1024; // Max 1024 entries
    options.CompactionPercentage = 0.25; // Remove 25% when full
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(5);
});
```

**Pros:**
- Fastest performance (< 1 ms)
- No external dependencies
- Direct object references (no serialization)

**Cons:**
- Lost on application restart
- Not shared across instances
- Memory consumption on single server

**Full Documentation:** [OoBDev.Microsoft.Caching/README.md](../../../ExternalServices/Microsoft/OoBDev.Microsoft.Caching/README.md)

---

## Provider Selection

### Decision Matrix

| Scenario | Provider | Rationale |
|----------|----------|-----------|
| **Single web server** | Microsoft.Caching | No need for distributed cache |
| **Multiple web servers** | Redis.Caching | Shared cache required |
| **Serverless (Azure Functions)** | Redis.Caching | Stateless execution |
| **Desktop application** | Microsoft.Caching | No external dependencies |
| **Microservices** | Redis.Caching | Centralized cache |
| **High read throughput (> 1M/sec)** | Microsoft.Caching | Fastest reads |
| **High availability** | Redis.Caching (clustered) | Redundancy |
| **Development/Testing** | Microsoft.Caching | Zero configuration |
| **Cache persistence required** | Redis.Caching | Survives restarts |

### Performance Comparison

| Operation | Microsoft.Caching | Redis (localhost) | Redis (Azure) |
|-----------|-------------------|-------------------|---------------|
| **Read (hit)** | 0.05 ms | 1.2 ms | 5-10 ms |
| **Write** | 0.05 ms | 1.5 ms | 5-12 ms |
| **Throughput** | 10M+ ops/sec | 100K+ ops/sec | 50K+ ops/sec |
| **Memory** | Local RAM | Server RAM | Cloud managed |
| **Persistence** | No | Yes (configurable) | Yes |
| **Shared** | No | Yes | Yes |

### Cost Comparison

| Provider | Infrastructure Cost | Latency | Complexity |
|----------|---------------------|---------|------------|
| **Microsoft.Caching** | $0 (uses app memory) | < 1 ms | Low |
| **Redis (self-hosted)** | VM cost (~$20-100/month) | 1-5 ms | Medium |
| **Redis (Azure Cache)** | $15-500+/month | 5-15 ms | Low |

---

## Advanced Patterns

### 1. Multi-Provider Strategy

Use different providers for different scenarios:

```csharp
// Startup.cs
services.AddMicrosoftCachingServices();  // Default provider
services.AddRedisCachingServices();       // Named provider

// Register both with names
services.AddSingleton<INamedProvider<ICachingProvider>>(sp =>
{
    var providers = new Dictionary<string, ICachingProvider>
    {
        ["memory"] = sp.GetRequiredService<MicrosoftMemoryCachingProvider>(),
        ["redis"] = sp.GetRequiredService<RedisCachingProvider>()
    };
    return new NamedProvider<ICachingProvider>(providers);
});

// Use in application
public class UserService
{
    private readonly INamedProvider<ICachingProvider> _providers;

    public async Task<User> GetUserAsync(int userId)
    {
        // Use memory for frequently accessed users
        var memoryCache = _providers.GetProvider("memory");
        var cached = await memoryCache.RetreiveAsync($"user:{userId}", typeof(User));
        if (cached != null) return (User)cached;

        // Fallback to Redis
        var redisCache = _providers.GetProvider("redis");
        cached = await redisCache.RetreiveAsync($"user:{userId}", typeof(User));
        if (cached != null)
        {
            // Promote to memory cache
            await memoryCache.StoreAsync($"user:{userId}", cached, TimeSpan.FromMinutes(5));
            return (User)cached;
        }

        // Load from database...
    }
}
```

### 2. Hybrid L1/L2 Caching

Combine memory (L1) and Redis (L2) for optimal performance:

```csharp
public class HybridCachingProvider : ICachingProvider
{
    private readonly MicrosoftMemoryCachingProvider _l1Cache;
    private readonly RedisCachingProvider _l2Cache;

    public async Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        // Store in both L1 and L2
        await _l1Cache.StoreAsync(key, data, expiration);
        await _l2Cache.StoreAsync(key, data, expiration);
    }

    public async Task<object?> RetreiveAsync(string key, Type targetType)
    {
        // Check L1 first (fast)
        var cached = await _l1Cache.RetreiveAsync(key, targetType);
        if (cached != null) return cached;

        // Check L2 (slower)
        cached = await _l2Cache.RetreiveAsync(key, targetType);
        if (cached != null)
        {
            // Promote to L1
            await _l1Cache.StoreAsync(key, cached, TimeSpan.FromMinutes(5));
        }

        return cached;
    }

    public async Task FlushAsync(string key)
    {
        // Flush from both L1 and L2
        await _l1Cache.FlushAsync(key);
        await _l2Cache.FlushAsync(key);
    }
}

// Register
services.AddSingleton<ICachingProvider, HybridCachingProvider>();
```

### 3. Write-Through Caching

Ensure cache consistency by updating cache and database together:

```csharp
public class ProductRepository : IProductRepository
{
    private readonly ICachingProvider _cache;
    private readonly IDatabase _database;

    [IsCacheable("product:{productId}", "01:00:00")]
    public async Task<Product> GetProductAsync(int productId)
    {
        return await _database.GetProductAsync(productId);
    }

    public async Task UpdateProductAsync(Product product)
    {
        // Update database first
        await _database.UpdateAsync(product);

        // Write through to cache
        await _cache.StoreAsync(
            $"product:{product.Id}",
            product,
            TimeSpan.FromHours(1)
        );
    }
}
```

### 4. Cache-Aside Pattern with Locking

Prevent cache stampede with locking:

```csharp
public class UserService
{
    private readonly ICachingProvider _cache;
    private readonly IDatabase _database;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

    public async Task<User> GetUserAsync(int userId)
    {
        var key = $"user:{userId}";

        // Fast path: Check cache without lock
        var cached = await _cache.RetreiveAsync(key, typeof(User));
        if (cached != null) return (User)cached;

        // Slow path: Acquire lock, double-check, load
        await _semaphore.WaitAsync();
        try
        {
            // Double-check after acquiring lock
            cached = await _cache.RetreiveAsync(key, typeof(User));
            if (cached != null) return (User)cached;

            // Load from database
            var user = await _database.GetUserAsync(userId);

            // Store in cache
            await _cache.StoreAsync(key, user, TimeSpan.FromHours(1));

            return user;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

---

## Testing Providers

### Unit Testing with Mock Provider

```csharp
using Moq;
using OoBDev.Caching.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class UserServiceTests
{
    private Mock<ICachingProvider> _mockCache;
    private UserService _userService;

    [TestInitialize]
    public void Setup()
    {
        _mockCache = new Mock<ICachingProvider>();
        _userService = new UserService(_mockCache.Object);
    }

    [TestMethod]
    public async Task GetUserAsync_CacheHit_ReturnsCachedUser()
    {
        // Arrange
        var userId = 123;
        var expectedUser = new User { Id = userId, Name = "John" };
        _mockCache
            .Setup(c => c.RetreiveAsync($"user:{userId}", typeof(User)))
            .ReturnsAsync(expectedUser);

        // Act
        var result = await _userService.GetUserAsync(userId);

        // Assert
        Assert.AreEqual(expectedUser, result);
        _mockCache.Verify(
            c => c.RetreiveAsync($"user:{userId}", typeof(User)),
            Times.Once
        );
    }
}
```

### Integration Testing with Real Provider

```csharp
using OoBDev.Microsoft.Caching;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;

[TestClass]
public class MicrosoftCachingProviderTests
{
    private ICachingProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMicrosoftCachingServices();
        var serviceProvider = services.BuildServiceProvider();
        _provider = serviceProvider.GetRequiredService<ICachingProvider>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreAsync_AndRetreive_WorksCorrectly()
    {
        // Arrange
        var key = $"test:{Guid.NewGuid()}";
        var data = new { Name = "Test", Value = 123 };

        // Act
        await _provider.StoreAsync(key, data, TimeSpan.FromMinutes(5));
        var result = await _provider.RetreiveAsync(key, data.GetType());

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(data.Name, ((dynamic)result).Name);
        Assert.AreEqual(data.Value, ((dynamic)result).Value);

        // Cleanup
        await _provider.FlushAsync(key);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RetreiveAsync_ExpiredEntry_ReturnsNull()
    {
        // Arrange
        var key = $"test:{Guid.NewGuid()}";
        var data = "test";

        // Act
        await _provider.StoreAsync(key, data, TimeSpan.FromMilliseconds(100));
        await Task.Delay(200); // Wait for expiration
        var result = await _provider.RetreiveAsync(key, typeof(string));

        // Assert
        Assert.IsNull(result);
    }
}
```

### Testing Provider Registrar

```csharp
[TestClass]
public class RedisCachingRegistrarTests
{
    [TestMethod]
    public void AddServices_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(Mock.Of<IConfiguration>());

        // Act
        var registrar = new RedisCachingRegistrar();
        registrar.AddServices(services);
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        var provider = serviceProvider.GetService<ICachingProvider>();
        Assert.IsNotNull(provider);
        Assert.IsInstanceOfType(provider, typeof(RedisCachingProvider));
    }
}
```

---

## Best Practices

### 1. Provider Implementation

✅ **DO:**
- Handle null keys gracefully (return/no-op, don't throw)
- Implement thread-safe operations
- Use async/await properly (avoid `Task.Run` unless necessary)
- Log errors and cache operations (optional)
- Dispose resources properly (`IDisposable`)

❌ **DON'T:**
- Throw exceptions on cache miss (return null instead)
- Block threads (use async all the way)
- Ignore expiration (always check and honor)
- Store sensitive data without encryption

### 2. Provider Selection

✅ **DO:**
- Choose based on deployment model (single vs multi-instance)
- Consider latency requirements (< 1 ms vs < 10 ms)
- Evaluate persistence needs (restart tolerance)
- Test under realistic load

❌ **DON'T:**
- Use distributed cache for single-instance apps
- Use memory cache for multi-instance apps
- Over-provision (e.g., Redis cluster for small app)

### 3. Testing

✅ **DO:**
- Unit test with mock providers
- Integration test with real providers
- Test expiration behavior
- Test concurrency scenarios
- Test error handling

❌ **DON'T:**
- Skip integration tests
- Test with production cache servers
- Ignore cleanup in tests (causes flaky tests)

---

## Summary

The **Provider Pattern** in OoBDev Caching enables:

✅ **Flexibility** - Swap providers without changing code
✅ **Testability** - Mock providers in unit tests
✅ **Extensibility** - Add custom providers easily
✅ **Performance** - Choose optimal backend per scenario
✅ **Separation of Concerns** - Application logic independent of storage

**Next Steps:**
1. Choose provider based on [Decision Matrix](#decision-matrix)
2. Implement custom provider if needed
3. Follow [Best Practices](#best-practices)
4. Test thoroughly with [Testing Patterns](#testing-providers)
