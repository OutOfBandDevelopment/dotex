# Attribute-Based Declarative Caching - API Design

**Epic:** 04 - Distributed Caching
**Feature:** Attribute-Based Declarative Caching
**Last Updated:** 2026-01-22

---

## API Overview

Declarative caching via C# attributes. Two primary attributes:
1. **[Cacheable]** - Mark methods for caching
2. **[CacheInvalidate]** - Mark methods to invalidate cache

---

## Core Attributes

### CacheableAttribute

```csharp
namespace OoBDev.Framework.Caching.Attributes;

/// <summary>
/// Marks method for automatic caching.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheableAttribute : Attribute
{
    /// <summary>
    /// Cache duration in seconds (default: 300 = 5 minutes).
    /// </summary>
    public int Duration { get; set; } = 300;

    /// <summary>
    /// Custom key prefix.
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Include parameters in cache key (default: true).
    /// </summary>
    public bool VaryByParameters { get; set; } = true;

    /// <summary>
    /// Include user in cache key.
    /// </summary>
    public bool VaryByUser { get; set; } = false;

    /// <summary>
    /// Include culture in cache key.
    /// </summary>
    public bool VaryByCulture { get; set; } = false;

    /// <summary>
    /// Cache region for partitioning.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Use sliding expiration (refreshes on access).
    /// </summary>
    public bool SlidingExpiration { get; set; } = false;

    /// <summary>
    /// Cache provider (override default).
    /// </summary>
    public CacheProvider? Provider { get; set; }

    /// <summary>
    /// Condition expression (e.g., "result.IsPublished == true").
    /// </summary>
    public string? Condition { get; set; }
}
```

---

### CacheInvalidateAttribute

```csharp
namespace OoBDev.Framework.Caching.Attributes;

/// <summary>
/// Marks method to invalidate cache entries.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CacheInvalidateAttribute : Attribute
{
    /// <summary>
    /// Wildcard pattern (e.g., "orders:*").
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Specific keys to invalidate (supports templates like "order:{orderId}").
    /// </summary>
    public string[]? Keys { get; set; }

    /// <summary>
    /// Region to invalidate.
    /// </summary>
    public string? Region { get; set; }
}
```

---

## Supporting Interfaces

### ICacheConditionEvaluator

```csharp
namespace OoBDev.Framework.Caching.Attributes;

/// <summary>
/// Evaluates cache condition expressions.
/// </summary>
public interface ICacheConditionEvaluator
{
    /// <summary>
    /// Evaluates condition against result.
    /// </summary>
    /// <param name="result">Method result</param>
    /// <param name="condition">Condition expression</param>
    /// <returns>True if condition passes (should cache)</returns>
    bool Evaluate(object? result, string condition);
}
```

---

## Usage Examples

### Example 1: Basic Caching with Duration

```csharp
using OoBDev.Framework.Caching.Attributes;

public interface IOrderService
{
    // Cache for 5 minutes (default)
    [Cacheable]
    Task<Order> GetOrderAsync(int orderId);

    // Cache for 10 minutes
    [Cacheable(Duration = 600)]
    Task<Order> GetOrderDetailsAsync(int orderId);

    // Cache for 1 hour
    [Cacheable(Duration = 3600)]
    Task<IEnumerable<OrderStatus>> GetOrderStatusesAsync();
}
```

---

### Example 2: Cache Key Variations

```csharp
public interface IProductService
{
    // Default: Varies by parameters only
    [Cacheable(Duration = 300)]
    Task<Product> GetProductAsync(int productId);

    // Vary by user and parameters
    [Cacheable(Duration = 300, VaryByUser = true)]
    Task<decimal> GetProductPriceAsync(int productId);

    // Vary by user, culture, and parameters
    [Cacheable(
        Duration = 300,
        VaryByUser = true,
        VaryByCulture = true)]
    Task<ProductDetails> GetLocalizedProductAsync(int productId);

    // Custom key prefix
    [Cacheable(
        Duration = 600,
        KeyPrefix = "featured")]
    Task<Product> GetFeaturedProductAsync();
}
```

---

### Example 3: Regions and Partitioning

```csharp
public interface ICatalogService
{
    // Catalog region
    [Cacheable(Duration = 1800, Region = "catalog")]
    Task<IEnumerable<Category>> GetCategoriesAsync();

    // Catalog region
    [Cacheable(Duration = 1800, Region = "catalog")]
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

    // Inventory region (separate cache partition)
    [Cacheable(Duration = 60, Region = "inventory")]
    Task<int> GetStockLevelAsync(int productId);
}
```

---

### Example 4: Sliding Expiration

```csharp
public interface IUserService
{
    // Sliding expiration - refreshes on each access
    [Cacheable(
        Duration = 900,
        SlidingExpiration = true,
        VaryByUser = true)]
    Task<UserPreferences> GetUserPreferencesAsync(int userId);

    // Absolute expiration - expires after 10 minutes regardless of access
    [Cacheable(Duration = 600)]
    Task<User> GetUserAsync(int userId);
}
```

---

### Example 5: Conditional Caching

```csharp
public interface IProductService
{
    // Only cache if product is published
    [Cacheable(
        Duration = 600,
        Condition = "result.IsPublished == true")]
    Task<Product> GetProductAsync(int productId);

    // Only cache if result is not null
    [Cacheable(
        Duration = 300,
        Condition = "result != null")]
    Task<Product?> FindProductBySkuAsync(string sku);

    // Only cache if price is greater than zero
    [Cacheable(
        Duration = 300,
        Condition = "result.Price > 0")]
    Task<ProductPrice> GetProductPriceAsync(int productId);
}
```

---

### Example 6: Cache Invalidation by Pattern

```csharp
public interface IOrderService
{
    [Cacheable(Duration = 300, KeyPrefix = "orders")]
    Task<Order> GetOrderAsync(int orderId);

    [Cacheable(Duration = 300, KeyPrefix = "orders")]
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);

    // Invalidate all order-related caches
    [CacheInvalidate(Pattern = "orders:*")]
    Task CreateOrderAsync(Order order);

    // Invalidate all order-related caches
    [CacheInvalidate(Pattern = "orders:*")]
    Task UpdateOrderAsync(Order order);

    // Invalidate all order-related caches
    [CacheInvalidate(Pattern = "orders:*")]
    Task DeleteOrderAsync(int orderId);
}
```

---

### Example 7: Cache Invalidation by Specific Keys

```csharp
public interface IOrderService
{
    [Cacheable(Duration = 300)]
    Task<Order> GetOrderAsync(int orderId);

    // Invalidate specific order cache
    [CacheInvalidate(Keys = new[] { "order:{orderId}" })]
    Task UpdateOrderStatusAsync(int orderId, OrderStatus status);

    // Invalidate multiple keys
    [CacheInvalidate(Keys = new[]
    {
        "order:{orderId}",
        "order:{orderId}:details"
    })]
    Task UpdateOrderAsync(int orderId, Order order);
}
```

---

### Example 8: Cache Invalidation by Region

```csharp
public interface ICatalogService
{
    [Cacheable(Duration = 1800, Region = "catalog")]
    Task<IEnumerable<Category>> GetCategoriesAsync();

    [Cacheable(Duration = 1800, Region = "catalog")]
    Task<IEnumerable<Product>> GetProductsAsync();

    // Invalidate entire catalog region
    [CacheInvalidate(Region = "catalog")]
    Task RefreshCatalogAsync();

    // Invalidate entire catalog region
    [CacheInvalidate(Region = "catalog")]
    Task ImportProductsAsync(IEnumerable<Product> products);
}
```

---

### Example 9: Multiple Invalidation Strategies

```csharp
public interface IProductService
{
    [Cacheable(Duration = 600, Region = "catalog")]
    Task<Product> GetProductAsync(int productId);

    [Cacheable(Duration = 600, Region = "catalog")]
    Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

    // Invalidate specific product + all category caches
    [CacheInvalidate(Keys = new[] { "product:{productId}" })]
    [CacheInvalidate(Pattern = "*:GetProductsByCategoryAsync*")]
    Task UpdateProductAsync(int productId, Product product);
}
```

---

## Registration and Configuration

### Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Framework.Caching.Attributes;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register cache service
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();

        // Register attribute-based caching infrastructure
        services.AddAttributeBasedCaching();

        // Register services with attribute-based caching
        services.AddCachedProxyWithAttributes<IOrderService, OrderService>();
        services.AddCachedProxyWithAttributes<IProductService, ProductService>();
    }
}
```

### Extension Method

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class AttributeBasedCachingExtensions
{
    /// <summary>
    /// Adds attribute-based caching infrastructure.
    /// </summary>
    public static IServiceCollection AddAttributeBasedCaching(
        this IServiceCollection services)
    {
        services.TryAddSingleton<ICacheConditionEvaluator, CacheConditionEvaluator>();
        services.TryAddSingleton<ICachedProxyFactory, CachedProxyFactory>();
        services.TryAddSingleton<ICacheKeyBuilder, CacheKeyBuilder>();

        return services;
    }

    /// <summary>
    /// Registers service with attribute-based caching.
    /// </summary>
    public static IServiceCollection AddCachedProxyWithAttributes<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddSingleton<TImplementation>();

        services.AddSingleton<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var factory = provider.GetRequiredService<ICachedProxyFactory>();

            var options = new CacheProxyOptions
            {
                InterceptorType = typeof(AttributeBasedCacheInterceptor)
            };

            return factory.CreateProxy<TInterface, TImplementation>(target, options);
        });

        return services;
    }
}
```

---

## Best Practices

### 1. Attribute Placement
```csharp
// ✅ GOOD: Attributes on interface methods
public interface IOrderService
{
    [Cacheable(Duration = 300)]
    Task<Order> GetOrderAsync(int orderId);
}

// ✅ ALSO GOOD: Attributes on implementation methods
public class OrderService : IOrderService
{
    [Cacheable(Duration = 300)]
    public async Task<Order> GetOrderAsync(int orderId)
    {
        return await _repository.GetByIdAsync(orderId);
    }
}
```

### 2. Cache Duration
```csharp
// ✅ GOOD: Appropriate durations
[Cacheable(Duration = 3600)]  // 1 hour - rarely changes
Task<IEnumerable<Country>> GetCountriesAsync();

[Cacheable(Duration = 60)]  // 1 minute - frequently changes
Task<StockLevel> GetStockLevelAsync(int productId);
```

### 3. Invalidation Patterns
```csharp
// ✅ GOOD: Specific patterns
[CacheInvalidate(Pattern = "orders:customer:*")]
Task UpdateCustomerAsync(Customer customer);

// ❌ BAD: Overly broad pattern
[CacheInvalidate(Pattern = "*")]  // Invalidates EVERYTHING!
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
