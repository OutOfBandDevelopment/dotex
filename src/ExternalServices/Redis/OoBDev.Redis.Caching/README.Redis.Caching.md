# OoBDev.Redis.Caching

Redis-based distributed caching provider for the OoBDev framework using StackExchange.Redis.

## Overview

This package provides a Redis implementation of the OoBDev caching abstractions, enabling distributed caching across multiple application instances using Redis as the backend store.

## Features

- **Distributed Caching** - Share cache across multiple application instances
- **High Performance** - Fast in-memory caching with Redis
- **Expiration Support** - Automatic cache expiration with configurable TTL
- **JSON Serialization** - Automatic serialization/deserialization of cached objects
- **Connection Management** - Factory pattern for Redis connection multiplexer

## Installation

```bash
dotnet add package OoBDev.Redis.Caching
dotnet add package StackExchange.Redis
```

## Configuration

### appsettings.json

```json
{
  "Redis": {
    "ConnectionMultiplexer": {
      "Config": "localhost:6379"
    }
  }
}
```

### Connection String Options

```json
{
  "Redis": {
    "ConnectionMultiplexer": {
      "Config": "server1:6379,server2:6379,password=yourpassword,ssl=true,abortConnect=false"
    }
  }
}
```

See [StackExchange.Redis Configuration](https://stackexchange.github.io/StackExchange.Redis/Configuration.html) for all options.

## Usage

### Register Services

```csharp
using OoBDev.Redis.Caching;
using Microsoft.Extensions.DependencyInjection;

// Option 1: Using extension method
services.AddRedisCachingServices();

// Option 2: Using registrar directly
new RedisCachingRegistrar().AddServices(services);
```

### Use Redis Caching

```csharp
using OoBDev.Caching.Abstractions;

public class ProductService
{
    private readonly ICachingProvider _cache;

    public ProductService(ICachingProvider cache)
    {
        _cache = cache;
    }

    public async Task<Product> GetProductAsync(int id)
    {
        var cacheKey = $"product:{id}";

        // Try to retrieve from Redis
        var cached = await _cache.RetreiveAsync(cacheKey, typeof(Product));
        if (cached != null) return (Product)cached;

        // Fetch from source
        var product = await FetchProductFromDatabaseAsync(id);

        // Store in Redis with 15-minute expiration
        await _cache.StoreAsync(cacheKey, product, TimeSpan.FromMinutes(15));

        return product;
    }

    public async Task InvalidateProductCacheAsync(int id)
    {
        var cacheKey = $"product:{id}";
        await _cache.FlushAsync(cacheKey);
    }
}
```

### Advanced Configuration

```csharp
using OoBDev.Redis.Caching.Providers;

// Custom connection multiplexer factory
services.AddSingleton<IConnectionMultiplexerFactory, CustomRedisFactory>();

public class CustomRedisFactory : IConnectionMultiplexerFactory
{
    public IConnectionMultiplexer Create()
    {
        var config = ConfigurationOptions.Parse("your-redis-config");
        config.ConnectRetry = 5;
        config.ConnectTimeout = 5000;
        return ConnectionMultiplexer.Connect(config);
    }
}
```

## Docker Compose Setup

For local development or testing:

```yaml
version: '3.8'
services:
  redis:
    image: redis:latest
    ports:
      - "6379:6379"
    volumes:
      - redis-data:/data
    command: redis-server --appendonly yes

volumes:
  redis-data:
```

Start with: `docker-compose up -d`

## Integration Testing

This package includes Integration tests that run against Docker containers. See the test project for examples.

### Run Integration Tests

```bash
# Start Redis container (see containers/testing/)
cd containers/testing
./scripts/integration-up.sh --wait

# Run tests
cd ../../src/src
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~Redis"

# Cleanup
cd ../../containers/testing
./scripts/integration-down.sh --clean
```

## Performance Considerations

- **Connection Pooling** - Uses singleton ConnectionMultiplexer for connection pooling
- **Async/Await** - All operations are asynchronous
- **JSON Serialization** - Objects are serialized to JSON (consider MessagePack for better performance)
- **Key Design** - Use meaningful, hierarchical key patterns (e.g., `product:123`, `user:456:cart`)

## Common Scenarios

### Cache Warming

```csharp
public async Task WarmCacheAsync(IEnumerable<Product> products)
{
    var tasks = products.Select(async product =>
    {
        var key = $"product:{product.Id}";
        await _cache.StoreAsync(key, product, TimeSpan.FromHours(1));
    });

    await Task.WhenAll(tasks);
}
```

### Cache Invalidation Pattern

```csharp
[FlushCache] // Using OoBDev.Caching.Abstractions attribute
public async Task UpdateProductAsync(Product product)
{
    await _repository.UpdateAsync(product);
    // Cache automatically flushed by attribute
}
```

### Multi-Tier Caching

```csharp
// Use in-memory cache as L1, Redis as L2
public class TwoTierCache
{
    private readonly IMemoryCache _l1;
    private readonly ICachingProvider _l2Redis;

    public async Task<T> GetAsync<T>(string key)
    {
        // Check L1 (in-memory)
        if (_l1.TryGetValue(key, out T value))
            return value;

        // Check L2 (Redis)
        var cached = await _l2Redis.RetreiveAsync(key, typeof(T));
        if (cached != null)
        {
            // Populate L1
            _l1.Set(key, cached, TimeSpan.FromMinutes(5));
            return (T)cached;
        }

        return default;
    }
}
```

## Troubleshooting

### Connection Issues

If you experience connection problems:

1. Verify Redis is running: `redis-cli ping` (should return `PONG`)
2. Check connection string in configuration
3. Verify network access (firewalls, security groups)
4. Enable connection logging in StackExchange.Redis

### Serialization Errors

If objects fail to serialize:

1. Ensure types are serializable (public properties, parameterless constructor)
2. Use `[JsonIgnore]` for properties that shouldn't be cached
3. Consider custom serializers for complex types

## Dependencies

- OoBDev.Caching.Abstractions - Caching interfaces
- OoBDev.System - Object converter for JSON serialization
- StackExchange.Redis - Redis client library
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.DependencyInjection.Abstractions

## Related Packages

- OoBDev.Caching - Core caching implementation
- OoBDev.Microsoft.Caching - Microsoft in-memory/distributed caching provider

## License

See the main OoBDev repository for license information.
