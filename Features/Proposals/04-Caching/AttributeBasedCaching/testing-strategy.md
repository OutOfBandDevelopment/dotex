# Attribute-Based Declarative Caching - Testing Strategy

**Epic:** 04 - Distributed Caching
**Feature:** Attribute-Based Declarative Caching
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage focusing on attribute processing, condition evaluation, and cache invalidation.

**Test Categories:**
- **Unit Tests** - Attribute reading, condition evaluation, key generation
- **Integration Tests** - End-to-end attribute-based caching
- **Performance Tests** - Attribute lookup overhead

---

## Test Pyramid

```
            ┌───────────────────┐
            │  Performance Tests│  (5 tests)
            └───────────────────┘
          ┌───────────────────────┐
          │  Integration Tests    │  (15 tests)
          └───────────────────────┘
      ┌─────────────────────────────┐
      │       Unit Tests            │  (40+ tests)
      └─────────────────────────────┘
```

---

## Unit Tests

### 1. Attribute Tests

```csharp
[TestClass]
public class CacheableAttributeTests
{
    [TestMethod]
    public void CacheableAttribute_DefaultValues_AreCorrect()
    {
        // Arrange & Act
        var attr = new CacheableAttribute();

        // Assert
        Assert.AreEqual(300, attr.Duration);
        Assert.IsTrue(attr.VaryByParameters);
        Assert.IsFalse(attr.VaryByUser);
        Assert.IsFalse(attr.VaryByCulture);
        Assert.IsFalse(attr.SlidingExpiration);
    }

    [TestMethod]
    public void CacheableAttribute_CustomValues_AreSet()
    {
        // Arrange & Act
        var attr = new CacheableAttribute
        {
            Duration = 600,
            KeyPrefix = "test",
            VaryByUser = true,
            Region = "catalog"
        };

        // Assert
        Assert.AreEqual(600, attr.Duration);
        Assert.AreEqual("test", attr.KeyPrefix);
        Assert.IsTrue(attr.VaryByUser);
        Assert.AreEqual("catalog", attr.Region);
    }
}

[TestClass]
public class CacheInvalidateAttributeTests
{
    [TestMethod]
    public void CacheInvalidateAttribute_Pattern_IsSet()
    {
        // Arrange & Act
        var attr = new CacheInvalidateAttribute
        {
            Pattern = "orders:*"
        };

        // Assert
        Assert.AreEqual("orders:*", attr.Pattern);
    }

    [TestMethod]
    public void CacheInvalidateAttribute_Keys_AreSet()
    {
        // Arrange & Act
        var attr = new CacheInvalidateAttribute
        {
            Keys = new[] { "order:123", "order:456" }
        };

        // Assert
        CollectionAssert.AreEqual(new[] { "order:123", "order:456" }, attr.Keys);
    }
}
```

---

### 2. AttributeBasedCacheInterceptor Tests

```csharp
[TestClass]
public class AttributeBasedCacheInterceptorTests
{
    [TestMethod]
    public async Task InterceptAsync_CacheableAttribute_CachesResult()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        mockCache
            .Setup(c => c.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync((object?)null);

        var interceptor = new AttributeBasedCacheInterceptor(mockCache.Object, ...);

        var method = typeof(IOrderService).GetMethod(nameof(IOrderService.GetOrderAsync))!;
        // Method has [Cacheable(Duration = 300)]

        var result = new Order { Id = 123 };
        Func<Task<object?>> proceed = async () => await Task.FromResult<object?>(result);

        // Act
        await interceptor.InterceptAsync(method, new object[] { 123 }, proceed);

        // Assert
        mockCache.Verify(
            c => c.SetAsync(It.IsAny<string>(), result, TimeSpan.FromSeconds(300)),
            Times.Once);
    }

    [TestMethod]
    public async Task InterceptAsync_CacheableWithCondition_EvaluatesCondition()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();
        mockCache
            .Setup(c => c.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync((object?)null);

        var mockEvaluator = new Mock<ICacheConditionEvaluator>();
        mockEvaluator
            .Setup(e => e.Evaluate(It.IsAny<object>(), It.IsAny<string>()))
            .Returns(false);  // Condition fails

        var interceptor = new AttributeBasedCacheInterceptor(
            mockCache.Object,
            ...,
            mockEvaluator.Object);

        var method = typeof(IProductService).GetMethod("GetProductAsync")!;
        // Method has [Cacheable(Condition = "result.IsPublished == true")]

        var result = new Product { Id = 123, IsPublished = false };
        Func<Task<object?>> proceed = async () => await Task.FromResult<object?>(result);

        // Act
        await interceptor.InterceptAsync(method, new object[] { 123 }, proceed);

        // Assert
        mockEvaluator.Verify(e => e.Evaluate(result, "result.IsPublished == true"), Times.Once);
        mockCache.Verify(
            c => c.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()),
            Times.Never);  // Not cached because condition failed
    }

    [TestMethod]
    public async Task InterceptAsync_CacheInvalidatePattern_InvalidatesCacheByPattern()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();

        var interceptor = new AttributeBasedCacheInterceptor(mockCache.Object, ...);

        var method = typeof(IOrderService).GetMethod("UpdateOrderAsync")!;
        // Method has [CacheInvalidate(Pattern = "orders:*")]

        Func<Task<object?>> proceed = async () => await Task.FromResult<object?>(null);

        // Act
        await interceptor.InterceptAsync(method, new object[] { new Order() }, proceed);

        // Assert
        mockCache.Verify(c => c.RemoveByPatternAsync("orders:*"), Times.Once);
    }

    [TestMethod]
    public async Task InterceptAsync_CacheInvalidateKeys_InvalidatesSpecificKeys()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();

        var interceptor = new AttributeBasedCacheInterceptor(mockCache.Object, ...);

        var method = typeof(IOrderService).GetMethod("DeleteOrderAsync")!;
        // Method has [CacheInvalidate(Keys = new[] { "order:{orderId}" })]

        Func<Task<object?>> proceed = async () => await Task.FromResult<object?>(null);

        // Act
        await interceptor.InterceptAsync(method, new object[] { 123 }, proceed);

        // Assert
        mockCache.Verify(c => c.RemoveAsync("order:123"), Times.Once);
    }

    [TestMethod]
    public async Task InterceptAsync_CacheInvalidateRegion_InvalidatesRegion()
    {
        // Arrange
        var mockCache = new Mock<ICacheService>();

        var interceptor = new AttributeBasedCacheInterceptor(mockCache.Object, ...);

        var method = typeof(ICatalogService).GetMethod("RefreshCatalogAsync")!;
        // Method has [CacheInvalidate(Region = "catalog")]

        Func<Task<object?>> proceed = async () => await Task.FromResult<object?>(null);

        // Act
        await interceptor.InterceptAsync(method, Array.Empty<object>(), proceed);

        // Assert
        mockCache.Verify(c => c.RemoveByPatternAsync("catalog:*"), Times.Once);
    }
}
```

---

### 3. CacheConditionEvaluator Tests

```csharp
[TestClass]
public class CacheConditionEvaluatorTests
{
    private CacheConditionEvaluator _evaluator;

    [TestInitialize]
    public void Setup()
    {
        _evaluator = new CacheConditionEvaluator();
    }

    [TestMethod]
    public void Evaluate_PropertyEquality_ReturnsTrue()
    {
        // Arrange
        var product = new Product { IsPublished = true };
        var condition = "result.IsPublished == true";

        // Act
        var result = _evaluator.Evaluate(product, condition);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_PropertyEquality_ReturnsFalse()
    {
        // Arrange
        var product = new Product { IsPublished = false };
        var condition = "result.IsPublished == true";

        // Act
        var result = _evaluator.Evaluate(product, condition);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Evaluate_PropertyInequality_ReturnsTrue()
    {
        // Arrange
        var order = new Order { Status = "Completed" };
        var condition = "result.Status != \"Draft\"";

        // Act
        var result = _evaluator.Evaluate(order, condition);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_NullCheck_ReturnsTrue()
    {
        // Arrange
        var order = new Order { Id = 123 };
        var condition = "result != null";

        // Act
        var result = _evaluator.Evaluate(order, condition);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_NullCheck_ReturnsFalse()
    {
        // Arrange
        object? order = null;
        var condition = "result != null";

        // Act
        var result = _evaluator.Evaluate(order, condition);

        // Assert
        Assert.IsFalse(result);
    }

    [TestMethod]
    public void Evaluate_NumericComparison_ReturnsTrue()
    {
        // Arrange
        var product = new Product { Price = 99.99m };
        var condition = "result.Price > 0";

        // Act
        var result = _evaluator.Evaluate(product, condition);

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void Evaluate_EmptyCondition_ReturnsTrue()
    {
        // Arrange
        var product = new Product();
        var condition = "";

        // Act
        var result = _evaluator.Evaluate(product, condition);

        // Assert
        Assert.IsTrue(result);  // Empty condition = always cache
    }
}
```

---

## Integration Tests

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class AttributeBasedCachingIntegrationTests
{
    private IServiceProvider _serviceProvider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddMemoryCache();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddAttributeBasedCaching();
        services.AddCachedProxyWithAttributes<IOrderService, OrderService>();

        _serviceProvider = services.BuildServiceProvider();
    }

    [TestMethod]
    public async Task GetOrderAsync_WithCacheableAttribute_CachesResult()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IOrderService>();

        // Act
        var order1 = await service.GetOrderAsync(123);  // First call
        var order2 = await service.GetOrderAsync(123);  // Second call (cached)

        // Assert
        Assert.AreSame(order1, order2);  // Same instance from cache
    }

    [TestMethod]
    public async Task UpdateOrderAsync_WithCacheInvalidate_InvalidatesCache()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IOrderService>();
        var cache = _serviceProvider.GetRequiredService<ICacheService>();

        // Prime cache
        await service.GetOrderAsync(123);

        // Act
        await service.UpdateOrderAsync(new Order { Id = 123, Total = 199.99m });

        // Assert
        var cachedOrder = await cache.GetAsync<Order>("order:123");
        Assert.IsNull(cachedOrder);  // Cache invalidated
    }

    [TestMethod]
    public async Task GetProductAsync_WithCondition_OnlyCachesWhenConditionTrue()
    {
        // Arrange
        var service = _serviceProvider.GetRequiredService<IProductService>();
        var repository = _serviceProvider.GetRequiredService<IProductRepository>();

        repository.Add(new Product { Id = 123, IsPublished = false });
        repository.Add(new Product { Id = 456, IsPublished = true });

        // Act
        var product1 = await service.GetProductAsync(123);  // Not published, not cached
        var product2 = await service.GetProductAsync(456);  // Published, cached

        repository.Update(123, new Product { Id = 123, IsPublished = false, Name = "Changed" });

        var product1Again = await service.GetProductAsync(123);  // Refetched (not cached)
        var product2Again = await service.GetProductAsync(456);  // From cache

        // Assert
        Assert.AreEqual("Changed", product1Again.Name);  // Updated from repository
        Assert.AreSame(product2, product2Again);  // Same cached instance
    }
}
```

---

## Performance Tests

```csharp
[TestClass]
[TestCategory(TestCategories.Unit)]
public class AttributeBasedCachingPerformanceTests
{
    [TestMethod]
    public async Task AttributeLookup_Overhead_LessThan1Millisecond()
    {
        // Arrange
        var service = CreateServiceWithAttributes();
        var method = typeof(IOrderService).GetMethod(nameof(IOrderService.GetOrderAsync))!;

        // Act - Measure attribute lookup time
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var attr = method.GetCustomAttribute<CacheableAttribute>();
        }
        stopwatch.Stop();

        var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;

        // Assert
        Assert.IsTrue(avgTime < 1, $"Average attribute lookup: {avgTime}ms (expected < 1ms)");
    }

    [TestMethod]
    public void ConditionEvaluation_Performance_LessThan5Milliseconds()
    {
        // Arrange
        var evaluator = new CacheConditionEvaluator();
        var product = new Product { IsPublished = true, Price = 99.99m };
        var condition = "result.IsPublished == true && result.Price > 0";

        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            evaluator.Evaluate(product, condition);
        }
        stopwatch.Stop();

        var avgTime = stopwatch.ElapsedMilliseconds / 1000.0;

        // Assert
        Assert.IsTrue(avgTime < 5, $"Average condition evaluation: {avgTime}ms (expected < 5ms)");
    }
}
```

---

## Coverage Goals

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| CacheableAttribute | 90% | Property setting |
| CacheInvalidateAttribute | 90% | Property setting |
| AttributeBasedCacheInterceptor | 85% | Attribute reading, caching, invalidation |
| CacheConditionEvaluator | 85% | Expression parsing, evaluation |
| Extension Methods | 75% | DI registration |

**Total Tests:** 40 unit + 15 integration + 5 performance = **60 tests**

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
