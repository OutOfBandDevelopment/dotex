# OoBDev.Caching

Common caching implementation for the OoBDev framework with factory and manager support.

## Overview

This package provides the core implementation of distributed caching functionality, including cache factories, proxies, and managers.

## Features

- **CacheableFactory** - Factory for creating cached proxies
- **CachedProxy** - Dynamic proxy for automatic caching
- **CachingManager** - Cache management and operations
- **ResultAwaiter** - Asynchronous result handling

## Usage

### Register Caching Services

```csharp
using OoBDev.Caching;
using Microsoft.Extensions.DependencyInjection;

services.AddOoBDevCaching();
```

### Use Caching Manager

```csharp
public class DataService
{
    private readonly ICachingManager _cache;

    public DataService(ICachingManager cache)
    {
        _cache = cache;
    }

    public async Task<Data> GetDataAsync(string id)
    {
        var cacheKey = $"data:{id}";

        // Try to retrieve from cache
        var cached = await _cache.GetAsync<Data>(cacheKey);
        if (cached != null) return cached;

        // Fetch from source
        var data = await FetchDataFromSourceAsync(id);

        // Store in cache
        await _cache.SetAsync(cacheKey, data, TimeSpan.FromMinutes(15));

        return data;
    }
}
```

### Automatic Caching with Attributes

```csharp
using OoBDev.Caching.Abstractions;

public class ProductService
{
    [IsCacheable]
    public async Task<Product> GetProductAsync(int id)
    {
        // This method's results will be automatically cached
        // Cache key generated from method name and parameters
        return await _repository.GetByIdAsync(id);
    }

    [FlushCache]
    public async Task UpdateProductAsync(Product product)
    {
        // This will flush relevant cache entries when called
        await _repository.UpdateAsync(product);
    }
}
```

## Configuration

Configure caching in `appsettings.json`:

```json
{
  "OoBDev": {
    "CachingProvider": {
      "Type": "Redis",
      "DefaultExpiration": "00:15:00"
    }
  }
}
```

## Providers

This package requires a caching provider implementation. Available providers:

- **OoBDev.Redis.Caching** - Redis-based distributed caching
- **OoBDev.Microsoft.Caching** - Microsoft in-memory and distributed caching

## Installation

```bash
dotnet add package OoBDev.Caching
dotnet add package OoBDev.Redis.Caching  # or OoBDev.Microsoft.Caching
```

## Dependencies

- OoBDev.Caching.Abstractions - Caching interfaces
- OoBDev.System.Abstractions - Core abstractions
- Microsoft.Extensions.Configuration.Abstractions
- Microsoft.Extensions.Logging.Abstractions

## Architecture

The caching system uses a factory pattern with dynamic proxies for transparent caching:

1. **Factory** creates cached proxies for services
2. **Proxy** intercepts method calls and applies caching logic
3. **Manager** handles cache storage and retrieval
4. **Provider** implements actual cache backend (Redis, Memory, etc.)

## Related Packages

- OoBDev.Caching.Abstractions - Interfaces and attributes
- OoBDev.Redis.Caching - Redis provider
- OoBDev.Microsoft.Caching - Microsoft provider

## License

See the main OoBDev repository for license information.
