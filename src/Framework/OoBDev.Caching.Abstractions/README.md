# OoBDev.Caching.Abstractions

Distributed caching abstractions and interfaces for the OoBDev framework.

## Overview

This package provides the core abstractions for implementing distributed caching in OoBDev applications. It defines interfaces and attributes for caching providers, managers, and factories.

## Features

- **ICachingProvider** - Interface for caching provider implementations
- **ICachingManager** - Interface for cache management operations
- **ICacheableFactory** - Factory pattern for cache operations
- **[IsCacheable]** - Attribute to mark methods as cacheable
- **[FlushCache]** - Attribute to trigger cache invalidation

## Usage

### Mark Methods as Cacheable

```csharp
public class DataService
{
    [IsCacheable]
    public async Task<Data> GetDataAsync(string id)
    {
        // This method's results will be cached
        return await _repository.GetByIdAsync(id);
    }

    [FlushCache]
    public async Task UpdateDataAsync(Data data)
    {
        // This will flush the cache when called
        await _repository.UpdateAsync(data);
    }
}
```

### Implement a Caching Provider

```csharp
[ContractConfig(AllowDefault = true, ConfigKey = "OoBDev:CachingProvider:Type")]
public class MyCachingProvider : ICachingProvider
{
    public async Task FlushAsync(string key)
    {
        // Implementation
    }

    public async Task StoreAsync(string key, object data, TimeSpan expiration)
    {
        // Implementation
    }

    public async Task<object?> RetreiveAsync(string key, Type targetType)
    {
        // Implementation
    }
}
```

## Providers

See provider packages for concrete implementations:

- **OoBDev.Redis.Caching** - Redis-based distributed caching
- **OoBDev.Microsoft.Caching** - Microsoft in-memory and distributed caching

## Installation

```bash
dotnet add package OoBDev.Caching.Abstractions
```

## Dependencies

- OoBDev.System.Abstractions - Core system abstractions

## Related Packages

- OoBDev.Caching - Common caching implementation
- OoBDev.Redis.Caching - Redis provider
- OoBDev.Microsoft.Caching - Microsoft provider

## License

See the main OoBDev repository for license information.
