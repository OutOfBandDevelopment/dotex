# Caching Testing Guide

**Version:** 1.0.0
**Last Updated:** 2026-01-20

---

## Table of Contents

1. [Testing Strategy](#testing-strategy)
2. [Unit Testing](#unit-testing)
3. [Simulation Testing](#simulation-testing)
4. [Integration Testing](#integration-testing)
5. [Test Categories](#test-categories)
6. [Best Practices](#best-practices)
7. [Common Patterns](#common-patterns)

---

## Testing Strategy

OoBDev Caching uses a **layered testing strategy** aligned with the test pyramid:

```
           ┌─────────────┐
          ╱  Integration  ╲  ← Real Redis/Memory Cache (10%)
         ├─────────────────┤
        ╱    Simulation    ╲  ← Mocked providers, full stack (20%)
       ├───────────────────┤
      ╱       Unit          ╲  ← Mocked dependencies, isolated (70%)
     └───────────────────────┘
```

### Test Distribution Goal

- **70% Unit Tests** - Fast, isolated, test single components
- **20% Simulation Tests** - Mocked infrastructure, test full stack
- **10% Integration Tests** - Real providers, test against actual services

---

## Test Categories

OoBDev uses MSTest categories to organize tests:

```csharp
[TestCategory(TestCategories.Unit)]            // Fast, isolated, no external dependencies
[TestCategory(TestCategories.Simulate)]        // Full stack, mocked persistence
[TestCategory(TestCategories.Integration)]     // Docker-based external services
[TestCategory(TestCategories.DevLocal)]        // Manual/exploratory testing
[TestCategory(TestCategories.LiveIntegration)] // Cloud services (manual)
```

**Run Specific Categories:**
```bash
# Unit tests only (fastest)
dotnet test --filter "TestCategory=Unit"

# Unit + Simulation (CI/CD)
dotnet test --filter "TestCategory=Unit|TestCategory=Simulate"

# Integration tests (requires Docker)
dotnet test --filter "TestCategory=Integration"
```

---

## Unit Testing

Unit tests focus on **single components in isolation** with all dependencies mocked.

### Testing CachingManager

```csharp
using OoBDev.Caching.Managers;
using OoBDev.Caching.Abstractions;
using OoBDev.System.ComponentModel;
using OoBDev.System.Abstractions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace OoBDev.Caching.Tests.Managers
{
    [TestClass]
    public class CachingManagerTests
    {
        private MockRepository mockRepository;
        private Mock<IStringFormatter> mockStringFormatter;
        private Mock<ISelectedService<ICachingProvider>> mockCache;
        private Mock<ICachingProvider> mockCachingProvider;

        [TestInitialize]
        public void Setup()
        {
            mockRepository = new MockRepository(MockBehavior.Strict);
            mockStringFormatter = mockRepository.Create<IStringFormatter>();
            mockCache = mockRepository.Create<ISelectedService<ICachingProvider>>();
            mockCachingProvider = mockRepository.Create<ICachingProvider>();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public async Task StoreAsync_ValidKey_StoresInProvider()
        {
            // Arrange
            var key = "test:123";
            var data = new { Name = "Test" };
            var expiration = TimeSpan.FromMinutes(5);

            mockCachingProvider
                .Setup(p => p.StoreAsync(key, data, expiration))
                .Returns(Task.CompletedTask);
            mockCache
                .Setup(c => c.Value)
                .Returns(mockCachingProvider.Object);

            var manager = new CachingManager(mockStringFormatter.Object, mockCache.Object);

            // Act
            await manager.StoreAsync(key, data, expiration);

            // Assert
            mockRepository.VerifyAll();
        }

        [TestMethod]
        [TestCategory(TestCategories.Unit)]
        public void BuildKey_WithIsCacheableAttribute_ReturnsFormattedKey()
        {
            // Arrange
            var method = typeof(TestObject).GetMethod(nameof(TestObject.GetData));
            var args = new object[] { 123 };
            var expectedKey = "user:123";

            mockStringFormatter
                .Setup(f => f.Format("user:{userId}", method, args))
                .Returns(expectedKey);

            var manager = new CachingManager(mockStringFormatter.Object, mockCache.Object);

            // Act
            var result = manager.BuildKey(method, args);

            // Assert
            Assert.AreEqual(expectedKey, result);
            mockRepository.VerifyAll();
        }

        public abstract class TestObject
        {
            [IsCacheable("user:{userId}", "01:00:00")]
            public abstract Task<object> GetData(int userId);
        }
    }
}
```

### Testing CachedProxy

```csharp
using OoBDev.Caching.Factories;
using OoBDev.Caching.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Reflection;
using System.Threading.Tasks;

[TestClass]
public class CachedProxyTests
{
    private Mock<ICachingManager> mockCachingManager;

    [TestInitialize]
    public void Setup()
    {
        mockCachingManager = new Mock<ICachingManager>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Invoke_CacheableMethod_CacheHit_ReturnsCached()
    {
        // Arrange
        var cachedValue = 999;
        var cacheKey = "test:key";

        mockCachingManager
            .Setup(m => m.BuildKey(It.IsAny<MethodInfo>(), It.IsAny<object[]>()))
            .Returns(cacheKey);
        mockCachingManager
            .Setup(m => m.RetreiveAsync(cacheKey, typeof(int)))
            .ReturnsAsync(cachedValue);

        var decorated = new Mock<TestService>();
        var proxy = CachedProxy<ITestService, TestService>.Create(
            decorated.Object,
            mockCachingManager.Object,
            Mock.Of<ILogger>()
        );

        // Act
        var result = proxy.GetValue();

        // Assert
        Assert.AreEqual(cachedValue, result);
        decorated.Verify(s => s.GetValue(), Times.Never);  // Original not called
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public void Invoke_CacheableMethod_CacheMiss_InvokesOriginal()
    {
        // Arrange
        var realValue = 123;
        var cacheKey = "test:key";

        mockCachingManager
            .Setup(m => m.BuildKey(It.IsAny<MethodInfo>(), It.IsAny<object[]>()))
            .Returns(cacheKey);
        mockCachingManager
            .Setup(m => m.RetreiveAsync(cacheKey, typeof(int)))
            .ReturnsAsync(null);  // Cache miss
        mockCachingManager
            .Setup(m => m.StoreAsync(cacheKey, realValue, It.IsAny<TimeSpan>()))
            .Returns(Task.CompletedTask);

        var decorated = new Mock<TestService>();
        decorated.Setup(s => s.GetValue()).Returns(realValue);

        var proxy = CachedProxy<ITestService, TestService>.Create(
            decorated.Object,
            mockCachingManager.Object,
            Mock.Of<ILogger>()
        );

        // Act
        var result = proxy.GetValue();

        // Assert
        Assert.AreEqual(realValue, result);
        decorated.Verify(s => s.GetValue(), Times.Once);  // Original called
    }

    public interface ITestService
    {
        int GetValue();
    }

    public abstract class TestService : ITestService
    {
        [IsCacheable("test:key", "00:05:00")]
        public abstract int GetValue();
    }
}
```

### Testing Providers (Unit)

```csharp
using OoBDev.Redis.Caching.Providers;
using OoBDev.System.ComponentModel;
using StackExchange.Redis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Threading.Tasks;

[TestClass]
public class RedisCachingProviderTests
{
    private Mock<IObjectConverter> mockConverter;
    private Mock<IConnectionMultiplexerFactory> mockFactory;

    [TestInitialize]
    public void Setup()
    {
        mockConverter = new Mock<IObjectConverter>();
        mockFactory = new Mock<IConnectionMultiplexerFactory>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_NullKey_DoesNotThrow()
    {
        // Arrange
        var provider = new RedisCachingProvider(mockConverter.Object, mockFactory.Object);

        // Act & Assert (no exception)
        await provider.StoreAsync(null, "data", TimeSpan.FromMinutes(5));
    }

    [TestMethod]
    [TestCategory(TestCategories.Unit)]
    public async Task StoreAsync_ValidData_SerializesAndStores()
    {
        // Arrange
        var key = "test:123";
        var data = new { Name = "Test" };
        var json = "{\"Name\":\"Test\"}";
        var expiration = TimeSpan.FromMinutes(5);

        var mockRedis = new Mock<IConnectionMultiplexer>();
        var mockDb = new Mock<IDatabase>();

        mockFactory.Setup(f => f.Create()).Returns(mockRedis.Object);
        mockRedis.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>())).Returns(mockDb.Object);
        mockConverter.Setup(c => c.ToJsonAsync(data)).ReturnsAsync(json);
        mockDb.Setup(d => d.StringSetAsync(
            It.Is<RedisKey>(k => k == key),
            It.Is<RedisValue>(v => v == json),
            expiration,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()
        )).ReturnsAsync(true);

        var provider = new RedisCachingProvider(mockConverter.Object, mockFactory.Object);

        // Act
        await provider.StoreAsync(key, data, expiration);

        // Assert
        mockDb.Verify(d => d.StringSetAsync(
            It.IsAny<RedisKey>(),
            It.IsAny<RedisValue>(),
            expiration,
            It.IsAny<bool>(),
            It.IsAny<When>(),
            It.IsAny<CommandFlags>()
        ), Times.Once);
    }
}
```

---

## Simulation Testing

Simulation tests use **real implementations with mocked infrastructure** to test full stack without external dependencies.

### Testing with In-Memory Provider

```csharp
using OoBDev.Microsoft.Caching;
using OoBDev.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

[TestClass]
public class CachingSimulationTests
{
    private IServiceProvider serviceProvider;
    private ICachingProvider cachingProvider;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddMicrosoftCachingServices();

        serviceProvider = services.BuildServiceProvider();
        cachingProvider = serviceProvider.GetRequiredService<ICachingProvider>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task FullStack_StoreRetrieveFlush_WorksCorrectly()
    {
        // Arrange
        var key = $"test:{Guid.NewGuid()}";
        var data = new { Name = "Test", Value = 123 };
        var expiration = TimeSpan.FromMinutes(5);

        // Act - Store
        await cachingProvider.StoreAsync(key, data, expiration);

        // Act - Retrieve
        var retrieved = await cachingProvider.RetreiveAsync(key, data.GetType());

        // Assert - Retrieved successfully
        Assert.IsNotNull(retrieved);
        var typedResult = (dynamic)retrieved;
        Assert.AreEqual(data.Name, typedResult.Name);
        Assert.AreEqual(data.Value, typedResult.Value);

        // Act - Flush
        await cachingProvider.FlushAsync(key);

        // Act - Retrieve after flush
        var afterFlush = await cachingProvider.RetreiveAsync(key, data.GetType());

        // Assert - Cache empty
        Assert.IsNull(afterFlush);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task Expiration_AfterDuration_ReturnsNull()
    {
        // Arrange
        var key = $"test:{Guid.NewGuid()}";
        var data = "test value";
        var expiration = TimeSpan.FromMilliseconds(100);

        // Act - Store with short expiration
        await cachingProvider.StoreAsync(key, data, expiration);

        // Wait for expiration
        await Task.Delay(200);

        // Act - Retrieve after expiration
        var result = await cachingProvider.RetreiveAsync(key, typeof(string));

        // Assert
        Assert.IsNull(result);
    }
}
```

### Testing Attribute-Driven Caching (Full Stack)

```csharp
using OoBDev.Caching;
using OoBDev.Caching.Abstractions;
using OoBDev.Microsoft.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

[TestClass]
public class AttributeDrivenCachingTests
{
    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task IsCacheableAttribute_CachesResults()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddOoBDevCachingServices();
        services.AddMicrosoftCachingServices();
        services.AddTransient(sp => sp.Cacheable<IUserRepository, UserRepository>());

        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetRequiredService<IUserRepository>();

        // Act - First call (cache miss)
        var user1 = await repository.GetUserAsync(123);

        // Act - Second call (cache hit)
        var user2 = await repository.GetUserAsync(123);

        // Assert - Same instance (cached)
        Assert.AreSame(user1, user2);
    }

    [TestMethod]
    [TestCategory(TestCategories.Simulate)]
    public async Task FlushCacheAttribute_InvalidatesCache()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddMemoryCache();
        services.AddOoBDevCachingServices();
        services.AddMicrosoftCachingServices();
        services.AddTransient(sp => sp.Cacheable<IUserRepository, UserRepository>());

        var serviceProvider = services.BuildServiceProvider();
        var repository = serviceProvider.GetRequiredService<IUserRepository>();

        // Act - First call (cache miss)
        var user1 = await repository.GetUserAsync(123);

        // Act - Update (flushes cache)
        await repository.UpdateUserAsync(new User { Id = 123, Name = "Updated" });

        // Act - Second call (cache miss after flush)
        var user2 = await repository.GetUserAsync(123);

        // Assert - Different instances (cache flushed)
        Assert.AreNotSame(user1, user2);
    }

    public interface IUserRepository
    {
        Task<User> GetUserAsync(int userId);
        Task UpdateUserAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private static int _callCount = 0;

        [IsCacheable("user:{userId}", "01:00:00")]
        public Task<User> GetUserAsync(int userId)
        {
            _callCount++;
            return Task.FromResult(new User { Id = userId, Name = $"User {userId} (Call #{_callCount})" });
        }

        [FlushCache("user:{user.Id}")]
        public Task UpdateUserAsync(User user)
        {
            return Task.CompletedTask;
        }
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
```

---

## Integration Testing

Integration tests use **real providers** (Redis, Microsoft Memory Cache) running in Docker or locally.

### Setup for Redis Integration Tests

**Prerequisites:**
```bash
# Start Redis Docker container
cd /current/src/containers/testing
./scripts/integration-up.sh --wait

# Verify Redis is running
docker ps | grep redis
```

### Testing Redis Provider

```csharp
using OoBDev.Redis.Caching;
using OoBDev.Redis.Caching.Providers;
using OoBDev.Caching.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

[TestClass]
public class RedisCachingIntegrationTests
{
    private ICachingProvider provider;

    [TestInitialize]
    public void Setup()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                { "Redis:ConnectionString", "localhost:6379,ssl=false" }
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddRedisCachingServices();

        var serviceProvider = services.BuildServiceProvider();
        provider = serviceProvider.GetRequiredService<ICachingProvider>();
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreAndRetrieve_WithRedis_WorksCorrectly()
    {
        // Arrange
        var key = $"integration:test:{Guid.NewGuid()}";
        var data = new { Name = "Integration Test", Value = 999 };

        try
        {
            // Act - Store
            await provider.StoreAsync(key, data, TimeSpan.FromMinutes(5));

            // Act - Retrieve
            var result = await provider.RetreiveAsync(key, data.GetType());

            // Assert
            Assert.IsNotNull(result);
            var typedResult = (dynamic)result;
            Assert.AreEqual(data.Name, typedResult.Name);
            Assert.AreEqual(data.Value, (int)typedResult.Value);
        }
        finally
        {
            // Cleanup
            await provider.FlushAsync(key);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Flush_WithRedis_RemovesData()
    {
        // Arrange
        var key = $"integration:test:{Guid.NewGuid()}";
        var data = "test value";

        await provider.StoreAsync(key, data, TimeSpan.FromHours(1));

        // Act
        await provider.FlushAsync(key);

        // Assert
        var result = await provider.RetreiveAsync(key, typeof(string));
        Assert.IsNull(result);
    }
}
```

### Cleanup in Integration Tests

**ALWAYS clean up after integration tests** to prevent data leaks:

```csharp
[TestCleanup]
public async Task Cleanup()
{
    // Flush all test keys
    await provider.FlushAsync($"integration:test:*");
}
```

---

## Best Practices

### 1. Test Naming Convention

```csharp
[TestMethod]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Example:
    // StoreAsync_NullKey_DoesNotThrow
    // BuildKey_WithIsCacheableAttribute_ReturnsFormattedKey
}
```

### 2. Arrange-Act-Assert Pattern

```csharp
[TestMethod]
public async Task ExampleTest()
{
    // Arrange - Setup test data and mocks
    var key = "test:123";
    mockProvider.Setup(p => p.RetreiveAsync(key, typeof(string))).ReturnsAsync("cached");

    // Act - Execute the method under test
    var result = await manager.RetreiveAsync(key, typeof(string));

    // Assert - Verify results
    Assert.AreEqual("cached", result);
    mockProvider.VerifyAll();
}
```

### 3. Use Unique Keys for Integration Tests

```csharp
// BAD - Hardcoded keys can conflict
var key = "test:123";

// GOOD - Unique keys per test run
var key = $"test:{Guid.NewGuid()}";
```

### 4. Mock Strict Behavior

```csharp
// Use MockBehavior.Strict to catch unexpected calls
var mockRepository = new MockRepository(MockBehavior.Strict);
var mockProvider = mockRepository.Create<ICachingProvider>();

// Verify all setups were called
mockRepository.VerifyAll();
```

### 5. Test Negative Cases

```csharp
[TestMethod]
public async Task RetreiveAsync_NonExistentKey_ReturnsNull()
{
    var result = await provider.RetreiveAsync("does-not-exist", typeof(string));
    Assert.IsNull(result);
}

[TestMethod]
public async Task StoreAsync_NullKey_DoesNotThrow()
{
    await provider.StoreAsync(null, "data", TimeSpan.FromMinutes(5));
    // No exception = pass
}
```

### 6. Test Expiration

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task Expiration_AfterDuration_ReturnsNull()
{
    var key = $"test:{Guid.NewGuid()}";
    await provider.StoreAsync(key, "data", TimeSpan.FromMilliseconds(100));

    await Task.Delay(200);  // Wait for expiration

    var result = await provider.RetreiveAsync(key, typeof(string));
    Assert.IsNull(result);
}
```

---

## Common Patterns

### Pattern 1: Testing Cache Stampede Protection

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task ConcurrentAccess_CacheMiss_OnlyOneLoadOccurs()
{
    var key = $"test:{Guid.NewGuid()}";
    var loadCount = 0;

    async Task<string> LoadData()
    {
        Interlocked.Increment(ref loadCount);
        await Task.Delay(100);  // Simulate slow load
        return "loaded data";
    }

    // Simulate 10 concurrent requests
    var tasks = Enumerable.Range(0, 10).Select(async _ =>
    {
        var cached = await provider.RetreiveAsync(key, typeof(string));
        if (cached == null)
        {
            var data = await LoadData();
            await provider.StoreAsync(key, data, TimeSpan.FromMinutes(5));
            return data;
        }
        return (string)cached;
    });

    await Task.WhenAll(tasks);

    // Without locking: loadCount = 10 (all concurrent requests load)
    // With locking: loadCount = 1 (only first request loads)
    Assert.AreEqual(1, loadCount);
}
```

### Pattern 2: Testing Hybrid L1/L2 Caching

```csharp
[TestMethod]
[TestCategory(TestCategories.Simulate)]
public async Task HybridCache_L1Hit_DoesNotCheckL2()
{
    var mockL1 = new Mock<ICachingProvider>();
    var mockL2 = new Mock<ICachingProvider>();

    var hybrid = new HybridCachingProvider(mockL1.Object, mockL2.Object);

    var key = "test:123";
    var cachedValue = "l1 cached";

    mockL1.Setup(p => p.RetreiveAsync(key, typeof(string))).ReturnsAsync(cachedValue);

    var result = await hybrid.RetreiveAsync(key, typeof(string));

    Assert.AreEqual(cachedValue, result);
    mockL1.Verify(p => p.RetreiveAsync(key, typeof(string)), Times.Once);
    mockL2.Verify(p => p.RetreiveAsync(It.IsAny<string>(), It.IsAny<Type>()), Times.Never);
}
```

### Pattern 3: Testing Configuration-Based Disabling

```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]
public void CacheableFactory_CachingDisabled_ReturnsDirectInstance()
{
    var mockConfig = new Mock<IConfiguration>();
    mockConfig.Setup(c => c["OoBDev:Caching:Disabled"]).Returns("true");

    var factory = new CacheableFactory(
        Mock.Of<IServiceProvider>(),
        Mock.Of<ICachingManager>(),
        mockConfig.Object
    );

    var result = factory.Create<ITestService, TestService>();

    Assert.IsInstanceOfType(result, typeof(TestService));  // Direct, not proxy
}
```

---

## Summary

**Testing Pyramid for Caching:**
- ✅ **70% Unit Tests** - Fast, isolated, test components individually
- ✅ **20% Simulation Tests** - Mocked providers, test full stack
- ✅ **10% Integration Tests** - Real providers, test against services

**Key Principles:**
- ✅ Use strict mocks to catch unexpected behavior
- ✅ Test negative cases (null keys, expiration, cache misses)
- ✅ Clean up after integration tests (prevent data leaks)
- ✅ Use unique keys for integration tests (`Guid.NewGuid()`)
- ✅ Verify cache behavior (hit/miss, expiration, flush)

**Next Steps:**
1. Review existing tests in test projects
2. Add missing test coverage (target 80%+)
3. Run tests in CI/CD pipeline
4. Monitor test execution time and optimize slow tests
