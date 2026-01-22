# Attribute-Based Declarative Caching - Requirements

**Epic:** 04 - Distributed Caching
**Feature:** Attribute-Based Declarative Caching
**Priority:** HIGH (Developer Experience)
**Complexity:** MEDIUM
**Estimated LOC:** ~350

---

## Overview

Declarative caching using C# attributes to mark methods for caching and cache invalidation. Built on top of Transparent AOP Caching infrastructure. Developers use `[Cacheable]` and `[CacheInvalidate]` attributes instead of manual configuration.

---

## Business Requirements

### BR-1: Declarative Caching via Attributes
**As a** developer
**I want** to declare caching behavior using attributes on methods
**So that** caching configuration is visible in the code where it's used

**Acceptance Criteria:**
- `[Cacheable]` attribute marks methods for caching
- Attribute properties configure duration, key generation, etc.
- No manual DI registration required per method
- Works with interface and implementation methods
- Compile-time safety (attributes validated)

**Example:**
```csharp
public interface IOrderService
{
    [Cacheable(Duration = 300)]  // 5 minutes
    Task<Order> GetOrderAsync(int orderId);

    [Cacheable(Duration = 60, VaryByParameters = true)]
    Task<IEnumerable<Order>> SearchOrdersAsync(string query, int page);

    [CacheInvalidate(Pattern = "orders:*")]
    Task UpdateOrderAsync(Order order);
}
```

---

### BR-2: Flexible Cache Duration
**As a** developer
**I want** to specify cache duration per method
**So that** different data has appropriate cache lifetimes

**Acceptance Criteria:**
- Duration property in seconds (default: 300)
- Sliding expiration support
- Absolute expiration support
- Per-method override of default duration

---

### BR-3: Cache Key Customization
**As a** developer
**I want** to customize how cache keys are generated
**So that** I can control cache partitioning and invalidation

**Acceptance Criteria:**
- `KeyPrefix` property for custom key prefixes
- `VaryByParameters` includes method arguments (default: true)
- `VaryByUser` includes user ID/name
- `VaryByCulture` includes current culture
- `Region` for cache partitioning

**Example:**
```csharp
[Cacheable(
    Duration = 600,
    KeyPrefix = "products",
    VaryByUser = true,
    VaryByCulture = true,
    Region = "catalog")]
Task<Product> GetProductAsync(int productId);
```

---

### BR-4: Cache Invalidation Attributes
**As a** developer
**I want** to automatically invalidate cache entries when data changes
**So that** stale data is never served

**Acceptance Criteria:**
- `[CacheInvalidate]` attribute on mutation methods
- Pattern-based invalidation (wildcard support)
- Specific key invalidation
- Region-based invalidation
- Atomic invalidation (all-or-nothing)

**Example:**
```csharp
// Invalidate specific keys
[CacheInvalidate(Keys = new[] { "order:{orderId}" })]
Task DeleteOrderAsync(int orderId);

// Invalidate by pattern
[CacheInvalidate(Pattern = "orders:customer:*")]
Task UpdateCustomerAsync(Customer customer);

// Invalidate entire region
[CacheInvalidate(Region = "catalog")]
Task RefreshCatalogAsync();
```

---

### BR-5: Conditional Caching
**As a** developer
**I want** to cache only when certain conditions are met
**So that** I can avoid caching invalid or temporary data

**Acceptance Criteria:**
- `Condition` property for cache condition expressions
- Evaluated against return value
- Supports simple expressions (e.g., `result.IsPublished == true`)
- Skip caching if condition fails

**Example:**
```csharp
[Cacheable(
    Duration = 600,
    Condition = "result.IsPublished == true")]
Task<Product> GetProductAsync(int productId);
```

---

## Technical Requirements

### TR-1: Attribute Design
```csharp
/// <summary>
/// Marks method for caching.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class CacheableAttribute : Attribute
{
    /// <summary>
    /// Cache duration in seconds (default: 300 = 5 minutes).
    /// </summary>
    public int Duration { get; set; } = 300;

    /// <summary>
    /// Custom key prefix (default: type name).
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Include method parameters in cache key (default: true).
    /// </summary>
    public bool VaryByParameters { get; set; } = true;

    /// <summary>
    /// Include user ID/name in cache key.
    /// </summary>
    public bool VaryByUser { get; set; } = false;

    /// <summary>
    /// Include current culture in cache key.
    /// </summary>
    public bool VaryByCulture { get; set; } = false;

    /// <summary>
    /// Cache region/partition.
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Sliding expiration (refreshes on access).
    /// </summary>
    public bool SlidingExpiration { get; set; } = false;

    /// <summary>
    /// Cache provider (override default).
    /// </summary>
    public CacheProvider? Provider { get; set; }

    /// <summary>
    /// Condition expression for caching (e.g., "result.IsPublished == true").
    /// </summary>
    public string? Condition { get; set; }
}

/// <summary>
/// Marks method to invalidate cache entries.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class CacheInvalidateAttribute : Attribute
{
    /// <summary>
    /// Wildcard pattern for keys to invalidate (e.g., "orders:*").
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

### TR-2: Interceptor Integration

**Requirement:** Interceptor must read attributes at runtime and apply caching configuration.

**Flow:**
```csharp
public class AttributeBasedCacheInterceptor : ICacheInterceptor
{
    public async Task<object?> InterceptAsync(MethodInfo method, object[] args, Func<Task<object?>> proceed)
    {
        // 1. Check for [Cacheable] attribute
        var cacheableAttr = method.GetCustomAttribute<CacheableAttribute>();
        if (cacheableAttr != null)
        {
            // Apply caching based on attribute properties
            return await ApplyCachingAsync(method, args, proceed, cacheableAttr);
        }

        // 2. Check for [CacheInvalidate] attribute
        var invalidateAttr = method.GetCustomAttribute<CacheInvalidateAttribute>();
        if (invalidateAttr != null)
        {
            // Invalidate cache entries
            await InvalidateCacheAsync(method, args, invalidateAttr);
        }

        // 3. Execute method
        return await proceed();
    }
}
```

---

### TR-3: Key Generation with Attributes

**Requirement:** Cache keys must incorporate attribute properties.

**Key Format:**
```
{KeyPrefix}:{TypeName}.{MethodName}({Parameters})[:{User}][:{Culture}]

Examples:
products:ProductService.GetProductAsync(123)
products:ProductService.GetProductAsync(123):user:john
products:ProductService.GetProductAsync(123):user:john:culture:en-US
```

---

### TR-4: Condition Expression Evaluation

**Requirement:** Evaluate condition expressions against return values.

**Supported Expressions:**
- Property equality: `result.IsPublished == true`
- Property inequality: `result.Status != "Draft"`
- Null checks: `result != null`
- Simple comparisons: `result.Price > 0`

**Evaluation Engine:**
```csharp
public class CacheConditionEvaluator
{
    public bool EvaluateCondition(object? result, string condition)
    {
        if (string.IsNullOrWhiteSpace(condition))
            return true;

        // Parse and evaluate condition expression
        // Use simple expression evaluator (not full LINQ)
        return SimpleExpressionEvaluator.Evaluate(result, condition);
    }
}
```

---

### TR-5: Performance Requirements

- **Attribute lookup:** < 1ms per method call (cached)
- **Key generation with attributes:** < 2ms
- **Condition evaluation:** < 5ms
- **No overhead if no attributes present**

---

### TR-6: Thread Safety

- Attribute instances are thread-safe (read-only after creation)
- Interceptor reads attributes without locks
- Cache invalidation is atomic per key

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Compatible with ASP.NET Core DI
- Supports async/await patterns
- Works with both interface and class attributes

### NFR-2: Debuggability
- Attribute values visible in debugger
- Logging shows attribute-derived cache keys
- Condition evaluation failures logged

### NFR-3: Testability
- Attributes can be inspected in unit tests
- Mock condition evaluator for testing
- Verify cache behavior via attributes

---

## Constraints

### C-1: Attribute Limitations
- Attributes must have compile-time constants
- Cannot use dynamic values in attribute properties
- Condition expressions limited to simple comparisons
- No support for complex LINQ expressions

### C-2: Inheritance
- Attributes NOT inherited by default
- Derived classes must re-apply attributes
- Interface attributes DO apply to implementations

---

## Success Criteria

- ✅ `[Cacheable]` and `[CacheInvalidate]` attributes implemented
- ✅ Flexible duration, key customization, conditions
- ✅ Automatic invalidation on mutations
- ✅ < 5ms overhead for attribute-based caching
- ✅ 80%+ test coverage
- ✅ Works with Transparent AOP Caching infrastructure

---

## Out of Scope

- ❌ Complex LINQ expressions in conditions
- ❌ Attribute code generation (use manual attributes)
- ❌ Dynamic attribute values (use runtime configuration)
- ❌ Custom attribute extensibility (future enhancement)

---

## Dependencies

### Internal
- OoBDev.Framework.Caching.Proxy (Transparent AOP Caching)
- OoBDev.System.Expressions (Simple expression evaluator)

### External
- System.Reflection (attribute lookup)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 04 Overview](../README.md)
