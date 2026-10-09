# OoBDev.Microsoft.Caching

Microsoft in-memory caching provider for the OoBDev framework using Microsoft.Extensions.Caching.Memory.

## Overview

This package provides a Microsoft MemoryCache implementation of the OoBDev caching abstractions, enabling fast in-memory caching within a single application instance.

## Features

- **In-Memory Caching** - Fast, local caching using Microsoft.Extensions.Caching.Memory
- **Automatic Expiration** - Configurable time-based expiration
- **Memory Pressure Handling** - Automatic eviction when memory is low
- **Simple Setup** - Minimal configuration required
- **Thread-Safe** - Safe for concurrent access

## Installation

```bash
dotnet add package OoBDev.Microsoft.Caching
dotnet add package Microsoft.Extensions.Caching.Memory
```

## Usage

### Register Services

```csharp
using OoBDev.Microsoft.Caching;
using Microsoft.Extensions.DependencyInjection;

// Option 1: Using extension method
services.AddMicrosoftCachingServices();

// Option 2: Using registrar directly
new MicrosoftCachingRegistrar().AddServices(services);
```

### Configuration (Optional)

Configure MemoryCache options in `appsettings.json`:

```json
{
  "MemoryCache": {
    "SizeLimit": null,
    "CompactionPercentage": 0.05,
    "ExpirationScanFrequency": "00:02:00"
  }
}
```

Or configure programmatically:

```csharp
services.Configure<MemoryCacheOptions>(options =>
{
    options.SizeLimit = 1024; // Limit number of entries
    options.CompactionPercentage = 0.25; // Remove 25% when limit reached
    options.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
});
```

### Use In-Memory Caching

```csharp
using OoBDev.Caching.Abstractions;

public class UserService
{
    private readonly ICachingProvider _cache;

    public UserService(ICachingProvider cache)
    {
        _cache = cache;
    }

    public async Task<User> GetUserAsync(int id)
    {
        var cacheKey = $"user:{id}";

        // Try to retrieve from memory cache
        var cached = await _cache.RetreiveAsync(cacheKey, typeof(User));
        if (cached != null) return (User)cached;

        // Fetch from source
        var user = await FetchUserFromDatabaseAsync(id);

        // Store in memory cache with 5-minute expiration
        await _cache.StoreAsync(cacheKey, user, TimeSpan.FromMinutes(5));

        return user;
    }

    public async Task InvalidateUserCacheAsync(int id)
    {
        var cacheKey = $"user:{id}";
        await _cache.FlushAsync(cacheKey);
    }
}
```

## When to Use

### In-Memory Caching (This Package)

Use Microsoft.Caching when:
- ✅ Single application instance or load balancer with sticky sessions
- ✅ Fast access to frequently used data is critical
- ✅ Cached data is small enough to fit in memory
- ✅ Cache invalidation across instances is not needed
- ✅ Simplified deployment (no external dependencies)

### Distributed Caching (OoBDev.Redis.Caching)

Use Redis.Caching when:
- ✅ Multiple application instances need shared cache
- ✅ Load balanced without sticky sessions
- ✅ Cache must survive application restarts
- ✅ Large cache datasets
- ✅ Cross-service cache sharing needed

### Hybrid Approach

Combine both for optimal performance:

```csharp
public class HybridCache
{
    private readonly IMemoryCache _l1; // Fast, local (this package)
    private readonly ICachingProvider _l2; // Shared, distributed (Redis)

    public async Task<T> GetAsync<T>(string key)
    {
        // Check L1 (in-memory) first
        if (_l1.TryGetValue(key, out T value))
            return value;

        // Check L2 (Redis) if L1 miss
        var cached = await _l2.RetreiveAsync(key, typeof(T));
        if (cached != null)
        {
            // Populate L1 for next access
            _l1.Set(key, cached, TimeSpan.FromMinutes(1));
            return (T)cached;
        }

        return default;
    }
}
```

## Memory Management

The MemoryCache automatically manages memory pressure:

```csharp
services.Configure<MemoryCacheOptions>(options =>
{
    // Limit to 1000 entries
    options.SizeLimit = 1000;

    // When limit reached, remove 20% of entries
    options.CompactionPercentage = 0.20;
});

// When storing, specify entry size
public async Task StoreWithSizeAsync(string key, object data)
{
    var cacheEntryOptions = new MemoryCacheEntryOptions
    {
        Size = 1, // Each entry counts as 1 towards SizeLimit
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
    };

    // Note: Standard ICachingProvider doesn't expose MemoryCacheEntryOptions
    // For advanced options, inject IMemoryCache directly
}
```

## Advanced Scenarios

### Sliding Expiration

For sliding expiration, inject `IMemoryCache` directly:

```csharp
public class SlidingExpirationCache
{
    private readonly IMemoryCache _cache;

    public SlidingExpirationCache(IMemoryCache cache)
    {
        _cache = cache;
    }

    public T GetOrCreate<T>(string key, Func<T> factory)
    {
        return _cache.GetOrCreate(key, entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return factory();
        });
    }
}
```

### Cache Priority

```csharp
_cache.Set(key, value, new MemoryCacheEntryOptions
{
    Priority = CacheItemPriority.High, // Less likely to be evicted
    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
});
```

### Eviction Callbacks

```csharp
_cache.Set(key, value, new MemoryCacheEntryOptions
{
    PostEvictionCallbacks =
    {
        new PostEvictionCallbackRegistration
        {
            EvictionCallback = (key, value, reason, state) =>
            {
                // Handle eviction
                Console.WriteLine($"Cache entry {key} evicted: {reason}");
            }
        }
    }
});
```

## Performance Characteristics

| Operation | Complexity | Notes |
|-----------|-----------|-------|
| Get | O(1) | Fast dictionary lookup |
| Set | O(1) | Fast dictionary insert |
| Remove | O(1) | Fast dictionary delete |
| Scan | O(n) | Periodic background scan |

**Benchmarks** (typical):
- Get: < 1 μs
- Set: < 1 μs
- Memory overhead: ~100 bytes per entry

## Testing

### Unit Tests

```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public async Task CachingProvider_ShouldStoreAndRetrieve()
{
    // Arrange
    var services = new ServiceCollection();
    new MicrosoftCachingRegistrar().AddServices(services);
    var provider = services.BuildServiceProvider();
    var cache = provider.GetRequiredService<ICachingProvider>();

    var key = "test-key";
    var data = new { Name = "Test", Value = 123 };

    // Act
    await cache.StoreAsync(key, data, TimeSpan.FromMinutes(5));
    var result = await cache.RetreiveAsync(key, data.GetType());

    // Assert
    Assert.IsNotNull(result);
    Assert.AreEqual(data.Name, ((dynamic)result).Name);
}
```

## Troubleshooting

### Memory Leaks

If memory usage grows unbounded:

1. Set `SizeLimit` in MemoryCacheOptions
2. Enable compaction: `CompactionPercentage > 0`
3. Use shorter expiration times
4. Specify entry sizes when using SizeLimit

### Cache Misses

If cache hit rate is low:

1. Increase expiration times
2. Use sliding expiration for frequently accessed items
3. Pre-warm cache on application startup
4. Monitor eviction reasons (memory pressure, expiration, etc.)

## Dependencies

- OoBDev.Caching.Abstractions - Caching interfaces
- Microsoft.Extensions.Caching.Memory - In-memory cache implementation
- Microsoft.Extensions.Caching.Abstractions - Caching abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## Related Packages

- OoBDev.Caching - Core caching implementation
- OoBDev.Redis.Caching - Redis distributed caching provider

## License

See the main OoBDev repository for license information.
