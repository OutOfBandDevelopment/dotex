# OoBDev - Caching Framework

## Configuration

| Key                                                    | Notes                       | Options                                              | Default      |
| ------------------------------------------------------ | --------------------------- | ---------------------------------------------------- | ------------ |
| OoBDev:Caching:Disabled                                | Disable caching globally    | true, false                                          | false        |
| OoBDev::ServiceKeys::OoBDev.Caching.ICachingProvider   | Select specific provider    | Redis, MemoryCache (or custom keyed registration)    | (first registered) |

### Provider Selection

When multiple caching providers are registered, the framework uses `ISelectedService<ICachingProvider>` to select the active provider:

1. **Configuration-based selection:** Set `OoBDev::ServiceKeys::OoBDev.Caching.ICachingProvider` to the keyed service name
2. **Fallback:** If no key is configured, uses the first registered `ICachingProvider`

Providers register themselves with specific keys:
- Microsoft Memory Cache: `MemoryCache`
- Redis Cache: `Redis`

## Setup

Register caching services in your IOC container:

```csharp
var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)  // Required for provider selection
    .AddOptions()                                  // Required for configuration binding
    .TryAddCachingServices()                       // Core caching framework
    .TryAddMicrosoftCachingServices()              // Option 1: In-memory caching
    // OR
    .TryAddRedisCachingServices()                  // Option 2: Distributed Redis caching
;
```

**Requirements:**
- `IConfiguration` must be registered for provider selection to work
- `.AddOptions()` is required for configuration binding
- At least one caching provider must be registered (Microsoft, Redis, or custom)
- If using Redis, configure connection string: `ConnectionMultiplexerFactory:Source`

**Multiple Providers:**
When registering multiple providers, set the configuration key `OoBDev::ServiceKeys::OoBDev.Caching.ICachingProvider` to select which one to use (see Configuration section above).

## Usage

### Registering Cacheable Classes

For any interface that you want cached, use the `.Cacheable<TInterface, TImplementation>()` extension method during IOC registration:

```csharp
using OoBDev.Caching;

// In your IOC registration
services.AddTransient(sp => sp.Cacheable<IExampleRepository, ExampleRepository>());
```

**Note:** The caching attributes (`[IsCacheable]`, `[FlushCache]`) are in the `OoBDev.Caching` namespace.

### Caching Methods with `[IsCacheable]`

Methods that you want cached must be tagged with the `[IsCacheable]` attribute:

```csharp
[IsCacheable("cacheKey", "lifetime")]
```

**Parameters:**
1. **Cache Key Pattern:** String with parameter placeholders (e.g., `"bucket1/data/{param1}/{param2}"`)
2. **Lifetime:** TimeSpan formatted string (e.g., `"01:00:00"` for 1 hour, `"00:05:00"` for 5 minutes)

**Key Formatting:**
- Use `{parameterName}` to include method parameters in the cache key
- Use `{model.PropertyName}` to access properties of complex parameters
- Use `{model.Property.SubProperty}` for nested property chains (unlimited depth)
- Multiple methods with the same cache key will share cached results

**Examples:**

```csharp
// Simple parameter substitution
[IsCacheable("bucket1/set/{param1}/{param2}", "00:05:00")]
public Task<ReturnModel[]> GetDataSet(string param1, string param2)
{
    // Returns: cached for 5 minutes with key "bucket1/set/value1/value2"
}

// Property-based key formatting
[IsCacheable("bucket1/data/{model.Param1}/{model.Param2}", "00:00:30")]
public Task GetByModel(ReturnModel model)
{
    // Returns: cached for 30 seconds with key "bucket1/data/modelValue1/modelValue2"
}

// Nested property chains
[IsCacheable("companies/{company.Address.City}/users", "01:00:00")]
public Task<User[]> GetUsersByCompanyCity(Company company)
{
    // Returns: cached for 1 hour with key "companies/Seattle/users"
    // if company.Address.City = "Seattle"
}

// Mixed parameters and property chains
[IsCacheable("users/{userId}/orders/{order.Product.Category}", "00:15:00")]
public Task<OrderDetails> GetOrderDetails(int userId, Order order)
{
    // Returns: cached for 15 minutes with complex key
}
```

**Supported Return Types:**
- `Task<T>` - Async methods (recommended)
- `T` - Synchronous methods (returns are cached)

### Flushing Cache with `[FlushCache]`

Use the `[FlushCache]` attribute to automatically clear cache entries before method execution:

**Option 1: Key Pattern (same as IsCacheable)**

```csharp
[FlushCache("bucket1/data/{param1}/{param2}")]
public Task UpdateData(string param1, string param2)
{
    // Clears cache key "bucket1/data/value1/value2" before executing
}

[FlushCache("bucket1/data/{model.Param1}/{model.Param2}")]
public Task UpdateData(ReturnModel model)
{
    // Clears cache using model properties
}

[FlushCache("companies/{company.Address.City}/users")]
public Task UpdateCompanyUsers(Company company)
{
    // Clears cache using nested property chain
}
```

**Option 2: Method Reference**

```csharp
[FlushCache(typeof(ExampleRepository), nameof(GetData))]
public Task UpdateData2(string param1, string param2)
{
    // Automatically uses the cache key pattern from GetData method
    // Useful when update methods share the same parameters as read methods
}
```

**Note:** Cache is flushed **before** the method executes, ensuring fresh data on next access.

## Complete Example

```csharp
// Startup/Program.cs
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var services = new ServiceCollection()
    .AddSingleton<IConfiguration>(configuration)
    .AddOptions()
    .TryAddCachingServices()
    .TryAddMicrosoftCachingServices()  // or .TryAddRedisCachingServices()

    // Register your cacheable services
    .AddTransient(sp => sp.Cacheable<IProductRepository, ProductRepository>())
    ;

// Repository Implementation
public interface IProductRepository
{
    Task<Product> GetProductAsync(int id);
    Task<Product[]> SearchProductsAsync(string category);
    Task UpdateProductAsync(Product product);
}

public class ProductRepository : IProductRepository
{
    [IsCacheable("products/{id}", "01:00:00")]
    public async Task<Product> GetProductAsync(int id)
    {
        // Cached for 1 hour
        return await _database.GetProductAsync(id);
    }

    [IsCacheable("products/search/{category}", "00:15:00")]
    public async Task<Product[]> SearchProductsAsync(string category)
    {
        // Cached for 15 minutes
        return await _database.SearchAsync(category);
    }

    [FlushCache("products/{product.Id}")]
    public async Task UpdateProductAsync(Product product)
    {
        // Flushes cache for this product before updating
        await _database.UpdateAsync(product);
    }
}
```

## Provider-Specific Configuration

### Microsoft Memory Cache (In-Memory)

**Installation:** Already included in .NET runtime

**Configuration:**
```csharp
services.TryAddMicrosoftCachingServices();
```

**Characteristics:**
- In-process memory storage
- Fast access (microseconds)
- Not shared across instances
- Lost on application restart
- Best for: Single-instance applications, development, testing

### Redis Cache (Distributed)

**Installation:** Requires Redis server

**Configuration:**
```json
// appsettings.json
{
  "ConnectionMultiplexerFactory": {
    "Source": "localhost:6379"
  }
}
```

```csharp
services.TryAddRedisCachingServices();
```

**Characteristics:**
- Distributed storage
- Shared across all application instances
- Survives application restarts
- Slower than in-memory (network latency)
- Best for: Multi-instance applications, microservices, production

### Disabling Caching

To disable caching globally (useful for debugging):

```json
// appsettings.json
{
  "OoBDev": {
    "Caching": {
      "Disabled": true
    }
  }
}
```

When disabled, cached methods execute normally without caching overhead.

## Testing

For unit tests, use the `NullCachingProvider` (available in test projects only):

```csharp
// In test setup
services
    .TryAddCachingServices()
    .AddSingleton<ICachingProvider, NullCachingProvider>();  // No-op provider
```

For integration tests, use the actual provider:

```csharp
// Test with Microsoft Memory Cache
services
    .TryAddCachingServices()
    .TryAddMicrosoftCachingServices();

// Test with Redis (requires Redis running)
services
    .TryAddCachingServices()
    .TryAddRedisCachingServices();
```

## Architecture

The caching framework consists of:

- **`ICachingProvider`** - Storage abstraction (Redis, Memory, Custom)
- **`ICachingManager`** - Cache key generation and coordination
- **`ICacheableFactory`** - Creates dynamic proxies for cached interfaces
- **`IStringFormatter`** - Formats cache keys with parameter substitution
- **`ISelectedService<T>`** - Configuration-based service selection

See `/docs/architecture/caching/` for detailed architecture documentation.