# Transparent AOP Caching - API Design

**Epic:** 04 - Distributed Caching
**Feature:** Transparent AOP Caching
**Last Updated:** 2026-01-22

---

## API Overview

The Transparent AOP Caching API provides dynamic proxy-based caching with zero code changes to business logic. Three primary interfaces:
1. **ICachedProxyFactory** - Create cached proxies
2. **ICacheInterceptor** - Intercept method calls
3. **ICacheKeyBuilder** - Generate cache keys

---

## Core Interfaces

### ICachedProxyFactory

**Purpose:** Factory for creating dynamic proxies with caching interceptors.

```csharp
namespace OoBDev.Framework.Caching.Proxy;

/// <summary>
/// Factory for creating cached service proxies.
/// </summary>
public interface ICachedProxyFactory
{
    /// <summary>
    /// Creates interface proxy with caching interceptor.
    /// </summary>
    /// <typeparam name="TInterface">Service interface</typeparam>
    /// <typeparam name="TImplementation">Service implementation</typeparam>
    /// <param name="target">Target service instance</param>
    /// <param name="options">Caching options</param>
    /// <returns>Proxy that intercepts calls and applies caching</returns>
    TInterface CreateProxy<TInterface, TImplementation>(
        TImplementation target,
        CacheProxyOptions options)
        where TInterface : class
        where TImplementation : class, TInterface;
}
```

---

### ICacheInterceptor

**Purpose:** Intercepts method calls and applies caching logic.

```csharp
namespace OoBDev.Framework.Caching.Proxy;

/// <summary>
/// Interceptor for method call caching.
/// </summary>
public interface ICacheInterceptor
{
    /// <summary>
    /// Intercepts method call and applies caching.
    /// </summary>
    /// <param name="method">Method being called</param>
    /// <param name="args">Method arguments</param>
    /// <param name="proceed">Delegate to execute target method</param>
    /// <returns>Cached or freshly-computed result</returns>
    Task<object?> InterceptAsync(
        MethodInfo method,
        object[] args,
        Func<Task<object?>> proceed);
}
```

---

### ICacheKeyBuilder

**Purpose:** Generates deterministic cache keys from method signatures.

```csharp
namespace OoBDev.Framework.Caching.Proxy;

/// <summary>
/// Builds cache keys from method calls.
/// </summary>
public interface ICacheKeyBuilder
{
    /// <summary>
    /// Builds cache key from method and arguments.
    /// </summary>
    /// <param name="method">Method being cached</param>
    /// <param name="args">Method arguments</param>
    /// <param name="options">Key generation options</param>
    /// <returns>Deterministic cache key</returns>
    string BuildKey(MethodInfo method, object[] args, CacheKeyOptions? options = null);

    /// <summary>
    /// Builds invalidation pattern for method.
    /// </summary>
    /// <param name="method">Method whose cache to invalidate</param>
    /// <returns>Wildcard pattern matching all keys for method</returns>
    string BuildInvalidationPattern(MethodInfo method);
}
```

---

## Configuration Classes

### CacheProxyOptions

**Purpose:** Configuration for cached proxies.

```csharp
namespace OoBDev.Framework.Caching.Proxy;

/// <summary>
/// Options for cached proxy creation.
/// </summary>
public class CacheProxyOptions
{
    /// <summary>
    /// Default cache duration (default: 5 minutes).
    /// </summary>
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Cache provider to use.
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

    /// <summary>
    /// Distributed lock timeout (default: 30 seconds).
    /// </summary>
    public TimeSpan LockTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Proxy implementation strategy.
    /// </summary>
    public ProxyStrategy Strategy { get; set; } = ProxyStrategy.CastleDynamicProxy;
}

/// <summary>
/// Proxy implementation strategies.
/// </summary>
public enum ProxyStrategy
{
    /// <summary>
    /// Castle DynamicProxy (recommended, requires Castle.Core).
    /// </summary>
    CastleDynamicProxy,

    /// <summary>
    /// Built-in DispatchProxy (no external dependencies).
    /// </summary>
    DispatchProxy
}
```

---

### CacheKeyOptions

**Purpose:** Options for cache key generation.

```csharp
namespace OoBDev.Framework.Caching.Proxy;

/// <summary>
/// Options for cache key generation.
/// </summary>
public class CacheKeyOptions
{
    /// <summary>
    /// Custom key prefix (overrides default).
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Whether to include parameter names in key.
    /// </summary>
    public bool IncludeParameterNames { get; set; } = false;

    /// <summary>
    /// Custom key template.
    /// Format: {TypeName}.{MethodName}({Args})
    /// </summary>
    public string? KeyTemplate { get; set; }
}
```

---

## Extension Methods

### Dependency Injection Extensions

**Purpose:** Fluent API for registering cached services.

```csharp
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for cached proxy registration.
/// </summary>
public static class CachedProxyServiceExtensions
{
    /// <summary>
    /// Registers service with caching proxy.
    /// </summary>
    /// <typeparam name="TInterface">Service interface</typeparam>
    /// <typeparam name="TImplementation">Service implementation</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="configure">Configuration delegate</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddCachedProxy<TInterface, TImplementation>(
        this IServiceCollection services,
        Action<CacheProxyOptions>? configure = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        // Register implementation (non-public)
        services.TryAddSingleton<TImplementation>();

        // Register proxy factory
        services.TryAddSingleton<ICachedProxyFactory, CachedProxyFactory>();

        // Register proxy as interface
        services.AddSingleton<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var factory = provider.GetRequiredService<ICachedProxyFactory>();

            var options = new CacheProxyOptions();
            configure?.Invoke(options);

            return factory.CreateProxy<TInterface, TImplementation>(target, options);
        });

        return services;
    }

    /// <summary>
    /// Registers scoped service with caching proxy.
    /// </summary>
    public static IServiceCollection AddScopedCachedProxy<TInterface, TImplementation>(
        this IServiceCollection services,
        Action<CacheProxyOptions>? configure = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddScoped<TImplementation>();
        services.TryAddSingleton<ICachedProxyFactory, CachedProxyFactory>();

        services.AddScoped<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var factory = provider.GetRequiredService<ICachedProxyFactory>();

            var options = new CacheProxyOptions();
            configure?.Invoke(options);

            return factory.CreateProxy<TInterface, TImplementation>(target, options);
        });

        return services;
    }

    /// <summary>
    /// Registers transient service with caching proxy.
    /// </summary>
    public static IServiceCollection AddTransientCachedProxy<TInterface, TImplementation>(
        this IServiceCollection services,
        Action<CacheProxyOptions>? configure = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        services.TryAddTransient<TImplementation>();
        services.TryAddSingleton<ICachedProxyFactory, CachedProxyFactory>();

        services.AddTransient<TInterface>(provider =>
        {
            var target = provider.GetRequiredService<TImplementation>();
            var factory = provider.GetRequiredService<ICachedProxyFactory>();

            var options = new CacheProxyOptions();
            configure?.Invoke(options);

            return factory.CreateProxy<TInterface, TImplementation>(target, options);
        });

        return services;
    }
}
```

---

## Usage Examples

### Example 1: Basic Cached Service

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Framework.Caching.Proxy;

// Service interface
public interface IOrderService
{
    Task<Order> GetOrderAsync(int orderId);
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
    Task UpdateOrderAsync(Order order);
}

// Service implementation (NO cache logic)
public class OrderService : IOrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task<Order> GetOrderAsync(int orderId)
    {
        // ONLY business logic - caching is transparent
        return await _repository.GetByIdAsync(orderId);
    }

    public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
    {
        return await _repository.GetByCustomerAsync(customerId);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await _repository.UpdateAsync(order);
    }
}

// Startup - Register with caching
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register repository
        services.AddSingleton<IOrderRepository, OrderRepository>();

        // Register service with caching proxy
        services.AddCachedProxy<IOrderService, OrderService>(options =>
        {
            options.DefaultDuration = TimeSpan.FromMinutes(5);
            options.Provider = CacheProvider.Memory;
            options.KeyPrefix = "orders";
        });
    }
}

// Consumer - Transparent caching
public class CheckoutController : ControllerBase
{
    private readonly IOrderService _orderService;

    public CheckoutController(IOrderService orderService)
    {
        _orderService = orderService;  // Injected proxy
    }

    [HttpGet("orders/{orderId}")]
    public async Task<Order> GetOrder(int orderId)
    {
        // First call: Executes method, caches result
        // Second call: Returns cached result (< 1ms)
        return await _orderService.GetOrderAsync(orderId);
    }
}
```

---

### Example 2: Redis-Backed Caching

```csharp
// Startup
public void ConfigureServices(IServiceCollection services)
{
    // Configure Redis cache
    services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = "localhost:6379";
    });

    // Register service with Redis caching
    services.AddCachedProxy<IProductService, ProductService>(options =>
    {
        options.Provider = CacheProvider.Redis;
        options.DefaultDuration = TimeSpan.FromMinutes(10);
        options.KeyPrefix = "products";
        options.UseDistributedLock = true;  // Prevent cache stampede
    });
}
```

---

### Example 3: Multiple Services with Different Strategies

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Fast in-memory cache for frequently-accessed data
    services.AddCachedProxy<ICategoryService, CategoryService>(options =>
    {
        options.Provider = CacheProvider.Memory;
        options.DefaultDuration = TimeSpan.FromMinutes(30);
    });

    // Distributed Redis cache for shared data
    services.AddCachedProxy<IProductService, ProductService>(options =>
    {
        options.Provider = CacheProvider.Redis;
        options.DefaultDuration = TimeSpan.FromMinutes(10);
    });

    // SQL cache for durable caching
    services.AddCachedProxy<IOrderService, OrderService>(options =>
    {
        options.Provider = CacheProvider.SqlServer;
        options.DefaultDuration = TimeSpan.FromHours(1);
    });
}
```

---

### Example 4: Scoped Services with Caching

```csharp
// Scoped service (per-request lifetime)
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Scoped cached proxy
    services.AddScopedCachedProxy<ICartService, CartService>(options =>
    {
        options.DefaultDuration = TimeSpan.FromMinutes(15);
        options.KeyPrefix = "carts";
    });
}

// Service uses scoped DbContext
public class CartService : ICartService
{
    private readonly AppDbContext _context;

    public CartService(AppDbContext context)
    {
        _context = context;  // Scoped per request
    }

    public async Task<Cart> GetCartAsync(Guid cartId)
    {
        return await _context.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.Id == cartId);
    }
}
```

---

### Example 5: Custom Key Builder

```csharp
public class CustomKeyBuilder : ICacheKeyBuilder
{
    public string BuildKey(MethodInfo method, object[] args, CacheKeyOptions? options = null)
    {
        var prefix = options?.KeyPrefix ?? "app";
        var typeName = method.DeclaringType?.Name ?? "Unknown";
        var methodName = method.Name;

        // Custom key format: {prefix}:{type}:{method}:{arg1}:{arg2}...
        var argsPart = string.Join(":", args.Select(a => a?.ToString() ?? "null"));

        return $"{prefix}:{typeName}:{methodName}:{argsPart}";
    }

    public string BuildInvalidationPattern(MethodInfo method)
    {
        var typeName = method.DeclaringType?.Name ?? "Unknown";
        var methodName = method.Name;

        return $"*:{typeName}:{methodName}:*";
    }
}

// Register custom key builder
services.AddSingleton<ICacheKeyBuilder, CustomKeyBuilder>();
```

---

### Example 6: Conditional Caching

```csharp
public class ConditionalCacheInterceptor : ICacheInterceptor
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyBuilder _keyBuilder;

    public async Task<object?> InterceptAsync(
        MethodInfo method,
        object[] args,
        Func<Task<object?>> proceed)
    {
        // Build key
        var cacheKey = _keyBuilder.BuildKey(method, args);

        // Check cache
        var cached = await _cache.GetAsync<object>(cacheKey);
        if (cached != null) return cached;

        // Execute method
        var result = await proceed();

        // Conditional caching (only cache if result meets criteria)
        if (ShouldCache(result))
        {
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        }

        return result;
    }

    private bool ShouldCache(object? result)
    {
        // Example: Only cache if result is not empty
        if (result is IEnumerable enumerable)
        {
            return enumerable.Cast<object>().Any();
        }

        // Example: Only cache if product is published
        if (result is Product product)
        {
            return product.IsPublished;
        }

        return result != null;
    }
}
```

---

### Example 7: Cache Invalidation

```csharp
public interface IOrderService
{
    Task<Order> GetOrderAsync(int orderId);
    Task UpdateOrderAsync(Order order);
}

public class OrderServiceWithInvalidation : IOrderService
{
    private readonly IOrderRepository _repository;
    private readonly ICacheService _cache;
    private readonly ICacheKeyBuilder _keyBuilder;

    public async Task<Order> GetOrderAsync(int orderId)
    {
        return await _repository.GetByIdAsync(orderId);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        await _repository.UpdateAsync(order);

        // Invalidate cache after update
        var getMethod = typeof(IOrderService).GetMethod(nameof(GetOrderAsync))!;
        var cacheKey = _keyBuilder.BuildKey(getMethod, new object[] { order.Id });
        await _cache.RemoveAsync(cacheKey);
    }
}
```

---

### Example 8: Performance Monitoring

```csharp
public class MonitoredCacheInterceptor : ICacheInterceptor
{
    private readonly ICacheInterceptor _innerInterceptor;
    private readonly IMetricsService _metrics;

    public async Task<object?> InterceptAsync(
        MethodInfo method,
        object[] args,
        Func<Task<object?>> proceed)
    {
        var stopwatch = Stopwatch.StartNew();
        var cacheKey = _keyBuilder.BuildKey(method, args);

        try
        {
            var result = await _innerInterceptor.InterceptAsync(method, args, proceed);
            stopwatch.Stop();

            // Record cache hit
            _metrics.RecordCacheHit(cacheKey, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Record cache miss or error
            _metrics.RecordCacheMiss(cacheKey, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
```

---

### Example 9: Testing with Bypassed Cache

```csharp
[TestClass]
public class OrderServiceTests
{
    [TestMethod]
    public async Task GetOrderAsync_ValidId_ReturnsOrder()
    {
        // Arrange - Use real implementation (bypass proxy)
        var mockRepository = new Mock<IOrderRepository>();
        mockRepository
            .Setup(r => r.GetByIdAsync(123))
            .ReturnsAsync(new Order { Id = 123, Total = 99.99m });

        var service = new OrderService(mockRepository.Object);

        // Act
        var order = await service.GetOrderAsync(123);

        // Assert
        Assert.IsNotNull(order);
        Assert.AreEqual(123, order.Id);
        Assert.AreEqual(99.99m, order.Total);

        // Verify repository called directly (no caching)
        mockRepository.Verify(r => r.GetByIdAsync(123), Times.Once);
    }
}
```

---

## Error Handling

### Exception Types

```csharp
namespace OoBDev.Framework.Caching.Proxy;

/// <summary>
/// Base exception for proxy caching errors.
/// </summary>
public class CacheProxyException : Exception
{
    public string? CacheKey { get; }

    public CacheProxyException(string message, string? cacheKey = null)
        : base(message)
    {
        CacheKey = cacheKey;
    }

    public CacheProxyException(string message, Exception innerException, string? cacheKey = null)
        : base(message, innerException)
    {
        CacheKey = cacheKey;
    }
}

/// <summary>
/// Exception thrown when proxy creation fails.
/// </summary>
public class ProxyCreationException : CacheProxyException
{
    public Type InterfaceType { get; }
    public Type ImplementationType { get; }

    public ProxyCreationException(string message, Type interfaceType, Type implementationType)
        : base(message)
    {
        InterfaceType = interfaceType;
        ImplementationType = implementationType;
    }
}

/// <summary>
/// Exception thrown when cache key generation fails.
/// </summary>
public class CacheKeyGenerationException : CacheProxyException
{
    public MethodInfo Method { get; }
    public object[] Arguments { get; }

    public CacheKeyGenerationException(string message, MethodInfo method, object[] arguments, Exception innerException)
        : base(message, innerException)
    {
        Method = method;
        Arguments = arguments;
    }
}
```

---

## Best Practices

### 1. Service Design
```csharp
// ✅ GOOD: Interface-based service
public interface IOrderService
{
    Task<Order> GetOrderAsync(int orderId);
}

public class OrderService : IOrderService
{
    public async Task<Order> GetOrderAsync(int orderId)
    {
        // Business logic only
    }
}

// ❌ BAD: No interface (cannot create proxy)
public class OrderService
{
    public async Task<Order> GetOrderAsync(int orderId) { }
}
```

### 2. Serialization
```csharp
// ✅ GOOD: Simple, serializable types
public class Order
{
    public int Id { get; set; }
    public decimal Total { get; set; }
    public DateTime OrderDate { get; set; }
}

// ❌ BAD: Circular references, non-serializable
public class Order
{
    public Customer Customer { get; set; }  // Customer has Orders collection
    public Stream Document { get; set; }  // Stream not serializable
}
```

### 3. Cache Duration
```csharp
// ✅ GOOD: Appropriate durations
services.AddCachedProxy<ICategoryService, CategoryService>(options =>
{
    options.DefaultDuration = TimeSpan.FromHours(1);  // Rarely changes
});

services.AddCachedProxy<IStockService, StockService>(options =>
{
    options.DefaultDuration = TimeSpan.FromSeconds(30);  // Frequently changes
});

// ❌ BAD: Too long or too short
options.DefaultDuration = TimeSpan.FromDays(7);  // Too long - stale data
options.DefaultDuration = TimeSpan.FromMilliseconds(100);  // Too short - cache overhead
```

---

## Performance Considerations

### Cache Overhead
```csharp
// Without caching: 100ms database query
var order = await _repository.GetByIdAsync(orderId);  // 100ms

// With caching:
// First call: 100ms (query) + 5ms (cache overhead) = 105ms
// Second call: 1ms (cache hit) = 1ms

// Improvement: 100x faster for cached calls
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Attribute-Based Caching API](../AttributeBasedCaching/api-design.md)
