# Attribute-Based Declarative Caching - Architecture

**Epic:** 04 - Distributed Caching
**Feature:** Attribute-Based Declarative Caching
**Last Updated:** 2026-01-22

---

## Architectural Overview

Attribute-based caching builds on Transparent AOP Caching, adding declarative configuration via C# attributes. Interceptor reads `[Cacheable]` and `[CacheInvalidate]` attributes at runtime to configure caching behavior.

```
┌────────────────────────────────────────────────────────────┐
│                    IOrderService                           │
│                                                            │
│  [Cacheable(Duration=300)]                                │
│  Task<Order> GetOrderAsync(int orderId);                  │
│                                                            │
│  [CacheInvalidate(Pattern="orders:*")]                    │
│  Task UpdateOrderAsync(Order order);                      │
└────────────────────┬───────────────────────────────────────┘
                     │
                     ↓
┌────────────────────────────────────────────────────────────┐
│      AttributeBasedCacheInterceptor                        │
│   (extends CacheInterceptor)                              │
└────────────────────┬───────────────────────────────────────┘
                     │
          ┌──────────┼──────────┐
          ↓          ↓          ↓
   ┌──────────┐ ┌──────────┐ ┌──────────┐
   │Attribute │ │Condition │ │  Cache   │
   │ Reader   │ │Evaluator │ │Invalidate│
   └──────────┘ └──────────┘ └──────────┘
```

---

## Core Components

### 1. CacheableAttribute

**Purpose:** Declarative caching configuration on methods.

**Design:**
```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheableAttribute : Attribute
{
    public int Duration { get; set; } = 300;
    public string? KeyPrefix { get; set; }
    public bool VaryByParameters { get; set; } = true;
    public bool VaryByUser { get; set; } = false;
    public bool VaryByCulture { get; set; } = false;
    public string? Region { get; set; }
    public bool SlidingExpiration { get; set; } = false;
    public CacheProvider? Provider { get; set; }
    public string? Condition { get; set; }
}
```

---

### 2. CacheInvalidateAttribute

**Purpose:** Declarative cache invalidation on mutation methods.

**Design:**
```csharp
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CacheInvalidateAttribute : Attribute
{
    public string? Pattern { get; set; }
    public string[]? Keys { get; set; }
    public string? Region { get; set; }
}
```

---

### 3. AttributeBasedCacheInterceptor

**Purpose:** Interceptor that reads attributes and applies caching logic.

**Implementation:**
```csharp
public class AttributeBasedCacheInterceptor : ICacheInterceptor
{
    private readonly ICacheService _cache;
    private readonly ICacheKeyBuilder _keyBuilder;
    private readonly ICacheConditionEvaluator _conditionEvaluator;
    private readonly IUserContext _userContext;
    private readonly ILogger _logger;

    public async Task<object?> InterceptAsync(
        MethodInfo method,
        object[] args,
        Func<Task<object?>> proceed)
    {
        // 1. Check for [Cacheable] attribute
        var cacheableAttr = method.GetCustomAttribute<CacheableAttribute>();
        if (cacheableAttr != null)
        {
            return await HandleCacheableAsync(method, args, proceed, cacheableAttr);
        }

        // 2. Check for [CacheInvalidate] attribute
        var invalidateAttrs = method.GetCustomAttributes<CacheInvalidateAttribute>();
        if (invalidateAttrs.Any())
        {
            await HandleCacheInvalidateAsync(method, args, invalidateAttrs);
        }

        // 3. Execute method
        return await proceed();
    }

    private async Task<object?> HandleCacheableAsync(
        MethodInfo method,
        object[] args,
        Func<Task<object?>> proceed,
        CacheableAttribute attr)
    {
        // Build cache key with attribute options
        var keyOptions = new CacheKeyOptions
        {
            KeyPrefix = attr.KeyPrefix,
            VaryByUser = attr.VaryByUser,
            VaryByCulture = attr.VaryByCulture
        };

        var cacheKey = _keyBuilder.BuildKey(method, args, keyOptions);

        // Add region to key
        if (!string.IsNullOrEmpty(attr.Region))
        {
            cacheKey = $"{attr.Region}:{cacheKey}";
        }

        // Check cache
        var cached = await _cache.GetAsync<object>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache HIT: {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache MISS: {CacheKey}", cacheKey);

        // Execute method
        var result = await proceed();

        // Evaluate condition
        if (!string.IsNullOrEmpty(attr.Condition))
        {
            if (!_conditionEvaluator.Evaluate(result, attr.Condition))
            {
                _logger.LogDebug("Condition failed, not caching: {CacheKey}", cacheKey);
                return result;
            }
        }

        // Cache result
        var duration = TimeSpan.FromSeconds(attr.Duration);
        if (attr.SlidingExpiration)
        {
            await _cache.SetAsync(cacheKey, result, duration, sliding: true);
        }
        else
        {
            await _cache.SetAsync(cacheKey, result, duration);
        }

        return result;
    }

    private async Task HandleCacheInvalidateAsync(
        MethodInfo method,
        object[] args,
        IEnumerable<CacheInvalidateAttribute> attrs)
    {
        foreach (var attr in attrs)
        {
            // Invalidate by pattern
            if (!string.IsNullOrEmpty(attr.Pattern))
            {
                await _cache.RemoveByPatternAsync(attr.Pattern);
            }

            // Invalidate specific keys
            if (attr.Keys != null && attr.Keys.Length > 0)
            {
                foreach (var keyTemplate in attr.Keys)
                {
                    // Resolve template (e.g., "order:{orderId}" → "order:123")
                    var key = ResolveKeyTemplate(keyTemplate, method, args);
                    await _cache.RemoveAsync(key);
                }
            }

            // Invalidate region
            if (!string.IsNullOrEmpty(attr.Region))
            {
                await _cache.RemoveByPatternAsync($"{attr.Region}:*");
            }
        }
    }

    private string ResolveKeyTemplate(string template, MethodInfo method, object[] args)
    {
        // Simple template resolution
        // "{orderId}" → args[0]
        // "{customerId}" → args[1]
        var parameters = method.GetParameters();

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramName = parameters[i].Name;
            template = template.Replace($"{{{paramName}}}", args[i]?.ToString() ?? "null");
        }

        return template;
    }
}
```

---

## Data Flow

### Sequence: Cached Method with Attribute

```
┌──────────┐    ┌─────────┐    ┌──────────────┐    ┌─────────┐
│ Consumer │    │  Proxy  │    │ Interceptor  │    │  Cache  │
└────┬─────┘    └────┬────┘    └──────┬───────┘    └────┬────┘
     │               │                │                 │
     │ GetOrderAsync(123)             │                 │
     ├──────────────>│                │                 │
     │               │ Intercept()    │                 │
     │               ├───────────────>│                 │
     │               │                │                 │
     │               │                │ Read [Cacheable]│
     │               │                │ Duration=300    │
     │               │                │                 │
     │               │                │ BuildKey()      │
     │               │                │                 │
     │               │                │ GetAsync(key)   │
     │               │                ├────────────────>│
     │               │                │                 │
     │               │                │ Cache MISS      │
     │               │                │<────────────────┤
     │               │                │                 │
     │               │                │ Proceed()       │
     │               │                │                 │
     │               │                │ Evaluate        │
     │               │                │ Condition       │
     │               │                │ (if specified)  │
     │               │                │                 │
     │               │                │ SetAsync(       │
     │               │                │   key, result,  │
     │               │                │   300 seconds)  │
     │               │                ├────────────────>│
     │               │                │                 │
```

---

## Design Patterns

### 1. Decorator Pattern
- Attributes decorate methods with caching metadata
- Interceptor applies decoration at runtime

### 2. Strategy Pattern
- Different caching strategies per attribute configuration
- Condition evaluator strategy

### 3. Template Method Pattern
- Key template resolution (e.g., `{orderId}`)

---

## Performance Optimizations

### 1. Attribute Caching
- Attributes read once per method and cached
- No reflection overhead on subsequent calls

### 2. Condition Compilation
- Simple conditions compiled to delegates
- Fast evaluation (< 5ms)

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
