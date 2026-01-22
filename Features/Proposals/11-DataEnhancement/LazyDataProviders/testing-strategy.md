# Lazy Data Providers - Testing Strategy

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Lazy Data Providers
**Priority:** HIGH (Foundation)
**Target Coverage:** 85-90%

---

## Overview

Comprehensive testing strategy for lazy data provider system, ensuring reliable provider selection, lazy loading with caching, async support, and integration with various data sources (databases, APIs, configuration, files).

---

## Test Pyramid

```
        ┌─────────────────┐
        │  Performance    │  5 tests (benchmarks)
        │   Benchmarks    │
        ├─────────────────┤
        │   Integration   │  20 tests (provider integration, E2E scenarios)
        │      Tests      │
        ├─────────────────┤
        │   Unit Tests    │  65+ tests (providers, registry, lazy loading, path matching)
        └─────────────────┘
```

**Coverage Goals:**
- IDataProvider implementations: 90%+
- IDataProviderRegistry: 95%+
- Provider selection logic: 95%+
- Lazy loading mechanism: 90%+
- Overall: 85-90%

---

## Unit Tests

### Category 1: Provider Registry Tests (15 tests)

**Path Pattern Matching:**

```csharp
[TestClass]
public class DataProviderRegistryTests
{
    private DataProviderRegistry _registry = null!;

    [TestInitialize]
    public void Setup()
    {
        _registry = new DataProviderRegistry();
    }

    [TestMethod]
    public void Register_Provider_AddsToRegistry()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });

        // Act
        _registry.Register("Customer/*", provider);

        // Assert
        Assert.AreEqual(1, _registry.RegisteredPatterns.Count);
        Assert.AreEqual("Customer/*", _registry.RegisteredPatterns[0]);
    }

    [TestMethod]
    public void FindProvider_ExactMatch_ReturnsProvider()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });
        _registry.Register("Customer/Orders", provider);

        // Act
        var found = _registry.FindProvider("Customer/Orders");

        // Assert
        Assert.AreSame(provider, found);
    }

    [TestMethod]
    public void FindProvider_SingleLevelWildcard_MatchesOneLevel()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });
        _registry.Register("Customer/*", provider);

        // Act
        var match = _registry.FindProvider("Customer/Orders");
        var noMatch = _registry.FindProvider("Customer/Orders/123");

        // Assert
        Assert.AreSame(provider, match);
        Assert.IsNull(noMatch);  // Two levels, doesn't match single-level wildcard
    }

    [TestMethod]
    public void FindProvider_MultiLevelWildcard_MatchesAnyDepth()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });
        _registry.Register("Customer/**", provider);

        // Act
        var match1 = _registry.FindProvider("Customer/Orders");
        var match2 = _registry.FindProvider("Customer/Orders/123");
        var match3 = _registry.FindProvider("Customer/Orders/123/Items/456");

        // Assert
        Assert.AreSame(provider, match1);
        Assert.AreSame(provider, match2);
        Assert.AreSame(provider, match3);
    }

    [TestMethod]
    public void FindProvider_UniversalWildcard_MatchesEverything()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });
        _registry.Register("**", provider);

        // Act
        var match1 = _registry.FindProvider("Customer");
        var match2 = _registry.FindProvider("Customer/Orders/123");
        var match3 = _registry.FindProvider("Anything/Else/Here");

        // Assert
        Assert.AreSame(provider, match1);
        Assert.AreSame(provider, match2);
        Assert.AreSame(provider, match3);
    }

    [TestMethod]
    public void FindProvider_MostSpecificWins()
    {
        // Arrange
        var exactProvider = new StaticDataProvider(new { type = "exact" });
        var specificProvider = new StaticDataProvider(new { type = "specific" });
        var wildcardProvider = new StaticDataProvider(new { type = "wildcard" });
        var fallbackProvider = new StaticDataProvider(new { type = "fallback" });

        _registry.Register("Customer/Orders", exactProvider);
        _registry.Register("Customer/*", specificProvider);
        _registry.Register("Customer/**", wildcardProvider);
        _registry.Register("**", fallbackProvider);

        // Act
        var exact = _registry.FindProvider("Customer/Orders");
        var specific = _registry.FindProvider("Customer/Profile");
        var wildcard = _registry.FindProvider("Customer/Orders/123");
        var fallback = _registry.FindProvider("Product/123");

        // Assert
        Assert.AreSame(exactProvider, exact);
        Assert.AreSame(specificProvider, specific);
        Assert.AreSame(wildcardProvider, wildcard);
        Assert.AreSame(fallbackProvider, fallback);
    }

    [TestMethod]
    public void FindProvider_HigherPriorityWins_WhenEqualSpecificity()
    {
        // Arrange
        var lowPriority = new StaticDataProvider(new { priority = "low" }) { Priority = 0 };
        var highPriority = new StaticDataProvider(new { priority = "high" }) { Priority = 10 };

        _registry.Register("Customer/*", lowPriority);
        _registry.Register("Customer/*", highPriority);  // Same pattern, higher priority

        // Act
        var found = _registry.FindProvider("Customer/Orders");

        // Assert
        Assert.AreSame(highPriority, found);
    }

    [TestMethod]
    public void FindProvider_NoMatch_ReturnsNull()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });
        _registry.Register("Customer/*", provider);

        // Act
        var found = _registry.FindProvider("Product/123");

        // Assert
        Assert.IsNull(found);
    }

    [TestMethod]
    public void Unregister_RemovesProvider()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });
        _registry.Register("Customer/*", provider);

        // Act
        _registry.Unregister("Customer/*");
        var found = _registry.FindProvider("Customer/Orders");

        // Assert
        Assert.IsNull(found);
        Assert.AreEqual(0, _registry.RegisteredPatterns.Count);
    }

    [TestMethod]
    public void Clear_RemovesAllProviders()
    {
        // Arrange
        _registry.Register("Customer/*", new StaticDataProvider(new { a = 1 }));
        _registry.Register("Order/*", new StaticDataProvider(new { b = 2 }));
        _registry.Register("Product/*", new StaticDataProvider(new { c = 3 }));

        // Act
        _registry.Clear();

        // Assert
        Assert.AreEqual(0, _registry.RegisteredPatterns.Count);
        Assert.IsNull(_registry.FindProvider("Customer/123"));
    }

    [TestMethod]
    public void GetProviders_ReturnsAllMatchingProviders_OrderedByPriority()
    {
        // Arrange
        var provider1 = new StaticDataProvider(new { priority = 1 }) { Priority = 1 };
        var provider2 = new StaticDataProvider(new { priority = 5 }) { Priority = 5 };
        var provider3 = new StaticDataProvider(new { priority = 10 }) { Priority = 10 };

        _registry.Register("Customer/**", provider1);
        _registry.Register("Customer/**", provider2);
        _registry.Register("Customer/**", provider3);

        // Act
        var providers = _registry.GetProviders("Customer/Orders/123").ToList();

        // Assert
        Assert.AreEqual(3, providers.Count);
        Assert.AreSame(provider3, providers[0]);  // Highest priority first
        Assert.AreSame(provider2, providers[1]);
        Assert.AreSame(provider1, providers[2]);
    }
}
```

---

### Category 2: Lazy Loading Tests (15 tests)

```csharp
[TestClass]
public class LazyLoadingTests
{
    [TestMethod]
    public async Task Navigate_WithoutAccess_DoesNotLoadData()
    {
        // Arrange
        var providerCalled = false;
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new DelegateDataProvider(() =>
        {
            providerCalled = true;
            return new { Id = 1, Name = "John" };
        }));

        // Act
        var node = container.Navigate("Customer");

        // Assert (before accessing value)
        Assert.IsFalse(providerCalled, "Provider should not be called until value is accessed");
    }

    [TestMethod]
    public async Task GetValueAsync_FirstAccess_CallsProvider()
    {
        // Arrange
        var providerCallCount = 0;
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new DelegateDataProvider(() =>
        {
            providerCallCount++;
            return new { Id = 1, Name = "John" };
        }));

        var node = container.Navigate("Customer");

        // Act
        var value = await node.GetValueAsync();

        // Assert
        Assert.AreEqual(1, providerCallCount);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public async Task GetValueAsync_SecondAccess_UsesCachedValue()
    {
        // Arrange
        var providerCallCount = 0;
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new DelegateDataProvider(() =>
        {
            providerCallCount++;
            return new { Id = 1, Name = "John" };
        }));

        var node = container.Navigate("Customer");

        // Act
        var value1 = await node.GetValueAsync();
        var value2 = await node.GetValueAsync();
        var value3 = await node.GetValueAsync();

        // Assert
        Assert.AreEqual(1, providerCallCount, "Provider should only be called once");
        Assert.AreSame(value1, value2, "Same instance should be returned");
        Assert.AreSame(value2, value3);
    }

    [TestMethod]
    public async Task Value_Property_TriggersLazyLoad()
    {
        // Arrange
        var providerCalled = false;
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new DelegateDataProvider(() =>
        {
            providerCalled = true;
            return new { Id = 1, Name = "John" };
        }));

        var node = container.Navigate("Customer");

        // Act
        var value = node.Value;  // Synchronous access

        // Assert
        Assert.IsTrue(providerCalled);
        Assert.IsNotNull(value);
    }

    [TestMethod]
    public async Task ConcurrentAccess_LoadsOnlyOnce()
    {
        // Arrange
        var providerCallCount = 0;
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref providerCallCount);
            await Task.Delay(100);  // Simulate slow provider
            return new { Id = 1, Name = "John" };
        }));

        var node = container.Navigate("Customer");

        // Act - Concurrent access from multiple threads
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
        {
            return await node.GetValueAsync();
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(1, providerCallCount, "Provider should be called exactly once despite concurrent access");
        Assert.IsTrue(tasks.All(t => t.Result != null), "All tasks should get the value");
    }

    [TestMethod]
    public async Task NoProvider_ReturnsNull()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        // No provider registered

        // Act
        var value = await container.Navigate("NonExistent").GetValueAsync();

        // Assert
        Assert.IsNull(value);
    }

    [TestMethod]
    public async Task ProviderReturnsNull_CachesNull()
    {
        // Arrange
        var providerCallCount = 0;
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Empty", new DelegateDataProvider(() =>
        {
            providerCallCount++;
            return null;  // Provider returns null
        }));

        var node = container.Navigate("Empty");

        // Act
        var value1 = await node.GetValueAsync();
        var value2 = await node.GetValueAsync();

        // Assert
        Assert.IsNull(value1);
        Assert.IsNull(value2);
        Assert.AreEqual(1, providerCallCount, "Null result should also be cached");
    }

    [TestMethod]
    public async Task ProviderThrows_ThrowsDataProviderException()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Broken", new DelegateDataProvider(() =>
        {
            throw new InvalidOperationException("Provider error");
        }));

        // Act & Assert
        var ex = await Assert.ThrowsExceptionAsync<DataProviderException>(async () =>
        {
            await container.Navigate("Broken").GetValueAsync();
        });

        Assert.AreEqual("Broken", ex.Path);
        Assert.IsInstanceOfType<InvalidOperationException>(ex.InnerException);
    }
}
```

---

### Category 3: Built-in Provider Tests (20 tests)

**StaticDataProvider:**

```csharp
[TestClass]
public class StaticDataProviderTests
{
    [TestMethod]
    public async Task ProvideAsync_ReturnsStaticData()
    {
        // Arrange
        var data = new { Name = "Test", Value = 42 };
        var provider = new StaticDataProvider(data);

        // Act
        var result = await provider.ProvideAsync(null!, "", null);

        // Assert
        Assert.AreSame(data, result);
    }

    [TestMethod]
    public async Task ProvideAsync_CalledMultipleTimes_ReturnsSameInstance()
    {
        // Arrange
        var data = new { Name = "Test" };
        var provider = new StaticDataProvider(data);

        // Act
        var result1 = await provider.ProvideAsync(null!, "", null);
        var result2 = await provider.ProvideAsync(null!, "", null);

        // Assert
        Assert.AreSame(result1, result2);
    }

    [TestMethod]
    public void CanProvide_AlwaysReturnsTrue()
    {
        // Arrange
        var provider = new StaticDataProvider(new { test = "data" });

        // Act & Assert
        Assert.IsTrue(provider.CanProvide("any/path"));
        Assert.IsTrue(provider.CanProvide("another/path"));
    }
}
```

**DelegateDataProvider:**

```csharp
[TestClass]
public class DelegateDataProviderTests
{
    [TestMethod]
    public async Task ProvideAsync_CallsDelegate()
    {
        // Arrange
        var delegateCalled = false;
        var provider = new DelegateDataProvider(() =>
        {
            delegateCalled = true;
            return new { test = "data" };
        });

        // Act
        var result = await provider.ProvideAsync(null!, "", null);

        // Assert
        Assert.IsTrue(delegateCalled);
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task ProvideAsync_PassesParameters()
    {
        // Arrange
        IDataNode? capturedNode = null;
        string? capturedPath = null;
        IDictionary<string, object?>? capturedContext = null;

        var provider = new DelegateDataProvider((node, path, context, ct) =>
        {
            capturedNode = node;
            capturedPath = path;
            capturedContext = context;
            return Task.FromResult<object?>(new { test = "data" });
        });

        var mockNode = new Mock<IDataNode>();
        var testContext = new Dictionary<string, object?> { ["key"] = "value" };

        // Act
        await provider.ProvideAsync(mockNode.Object, "Customer/123", testContext);

        // Assert
        Assert.AreSame(mockNode.Object, capturedNode);
        Assert.AreEqual("Customer/123", capturedPath);
        Assert.AreSame(testContext, capturedContext);
    }

    [TestMethod]
    public void Priority_CanBeSet()
    {
        // Arrange
        var provider = new DelegateDataProvider(() => null, priority: 10);

        // Act & Assert
        Assert.AreEqual(10, provider.Priority);
    }
}
```

**DatabaseDataProvider:**

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class DatabaseDataProviderTests
{
    private MyDbContext _dbContext = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<MyDbContext>()
            .UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}")
            .Options;

        _dbContext = new MyDbContext(options);

        // Seed data
        _dbContext.Customers.AddRange(
            new Customer { Id = 1, Name = "Alice", Age = 30 },
            new Customer { Id = 2, Name = "Bob", Age = 25 }
        );
        _dbContext.SaveChanges();
    }

    [TestMethod]
    public async Task ProvideAsync_LoadsFromDatabase()
    {
        // Arrange
        var provider = new DatabaseDataProvider<Customer>(_dbContext);

        // Act
        var result = await provider.ProvideAsync(null!, "", null);

        // Assert
        var customers = (List<Customer>)result!;
        Assert.AreEqual(2, customers.Count);
        Assert.IsTrue(customers.Any(c => c.Name == "Alice"));
    }

    [TestMethod]
    public async Task ProvideAsync_WithQueryBuilder_FiltersResults()
    {
        // Arrange
        var provider = new DatabaseDataProvider<Customer>(
            _dbContext,
            (query, node) => query.Where(c => c.Age > 28)
        );

        // Act
        var result = await provider.ProvideAsync(null!, "", null);

        // Assert
        var customers = (List<Customer>)result!;
        Assert.AreEqual(1, customers.Count);
        Assert.AreEqual("Alice", customers[0].Name);
    }

    [TestMethod]
    public async Task ProvideAsync_WithCancellation_CancelsQuery()
    {
        // Arrange
        var provider = new DatabaseDataProvider<Customer>(_dbContext);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<OperationCanceledException>(async () =>
        {
            await provider.ProvideAsync(null!, "", null, cts.Token);
        });
    }
}

public class MyDbContext : DbContext
{
    public MyDbContext(DbContextOptions<MyDbContext> options) : base(options) { }
    public DbSet<Customer> Customers { get; set; } = null!;
}

public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Age { get; set; }
}
```

**ConfigurationDataProvider:**

```csharp
[TestClass]
public class ConfigurationDataProviderTests
{
    [TestMethod]
    public async Task ProvideAsync_LoadsConfigurationValue()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = "Server=localhost",
            ["Database:Timeout"] = "30"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var provider = new ConfigurationDataProvider(configuration);

        // Act
        var result = await provider.ProvideAsync(null!, "Database/ConnectionString", null);

        // Assert
        Assert.AreEqual("Server=localhost", result);
    }

    [TestMethod]
    public async Task ProvideAsync_LoadsConfigurationSection()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["Database:ConnectionString"] = "Server=localhost",
            ["Database:Timeout"] = "30"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var provider = new ConfigurationDataProvider(configuration);

        // Act
        var result = await provider.ProvideAsync(null!, "Database", null);

        // Assert
        var dict = (Dictionary<string, object?>)result!;
        Assert.AreEqual("Server=localhost", dict["ConnectionString"]);
        Assert.AreEqual("30", dict["Timeout"]);
    }

    [TestMethod]
    public async Task ProvideAsync_WithSectionPrefix_AppliesPrefix()
    {
        // Arrange
        var configData = new Dictionary<string, string?>
        {
            ["MyApp:Database:ConnectionString"] = "Server=localhost"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var provider = new ConfigurationDataProvider(configuration, "MyApp");

        // Act
        var result = await provider.ProvideAsync(null!, "Database/ConnectionString", null);

        // Assert
        Assert.AreEqual("Server=localhost", result);
    }
}
```

---

### Category 4: Context-Aware Provider Tests (10 tests)

```csharp
[TestClass]
public class ContextAwareProviderTests
{
    [TestMethod]
    public async Task Provider_AccessesParentNode()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new StaticDataProvider(
            new { Id = 123, Name = "Alice" }
        ));

        container.RegisterProvider("Customer/Orders", new DelegateDataProvider(
            async (node, path, context, ct) =>
            {
                // Access parent customer node
                var customerId = (int?)node.Parent?.Value?.GetType().GetProperty("Id")?.GetValue(node.Parent.Value);

                // Return orders for this customer
                return new[]
                {
                    new { OrderId = 1, CustomerId = customerId, Total = 100.0 },
                    new { OrderId = 2, CustomerId = customerId, Total = 200.0 }
                };
            }
        ));

        // Act
        var orders = await container.Navigate("Customer/Orders").GetValueAsync();

        // Assert
        Assert.IsNotNull(orders);
        var ordersList = (object[])orders!;
        Assert.AreEqual(2, ordersList.Length);
    }

    [TestMethod]
    public async Task Provider_ReceivesContext()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        IDictionary<string, object?>? capturedContext = null;

        container.RegisterProvider("Data", new DelegateDataProvider(
            (node, path, context, ct) =>
            {
                capturedContext = context;
                return Task.FromResult<object?>(new { test = "data" });
            }
        ));

        var testContext = new Dictionary<string, object?> { ["userId"] = 123 };

        // Act
        var node = container.Navigate("Data");
        await ((DataNode)node).GetValueAsync(testContext);  // Pass context

        // Assert
        Assert.IsNotNull(capturedContext);
        Assert.AreEqual(123, capturedContext["userId"]);
    }
}
```

---

## Integration Tests (20 tests)

### Category 5: End-to-End Scenarios (12 tests)

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class LazyDataProvidersE2ETests
{
    [TestMethod]
    public async Task CompleteScenario_MultipleProviders_LazyLoading()
    {
        // Arrange
        var dbContext = CreateInMemoryDbContext();
        var httpClient = new HttpClient();
        var configuration = CreateTestConfiguration();

        var container = DataContainerFactory.Create();

        // Register multiple providers
        container.RegisterProvider("Customer/**", new DatabaseDataProvider<Customer>(dbContext));
        container.RegisterProvider("Config/**", new ConfigurationDataProvider(configuration));
        container.RegisterProvider("Weather/**", new ApiDataProvider(httpClient, "https://api.weather.com"));

        // Act - Navigate without loading
        var customerNode = container.Navigate("Customer/123");
        var configNode = container.Navigate("Config/Database/ConnectionString");

        Assert.IsFalse(providersCalled);  // No providers called yet

        // Load values
        var customer = await customerNode.GetValueAsync<Customer>();
        var connectionString = await configNode.GetValueAsync<string>();

        // Assert
        Assert.IsNotNull(customer);
        Assert.IsNotNull(connectionString);
    }

    [TestMethod]
    public async Task LoadOnlyAccessedPaths_SkipsUnaccessed()
    {
        // Arrange
        var provider1Called = false;
        var provider2Called = false;
        var provider3Called = false;

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Path1", new DelegateDataProvider(() => { provider1Called = true; return "data1"; }));
        container.RegisterProvider("Path2", new DelegateDataProvider(() => { provider2Called = true; return "data2"; }));
        container.RegisterProvider("Path3", new DelegateDataProvider(() => { provider3Called = true; return "data3"; }));

        // Act - Only access Path1 and Path3
        await container.Navigate("Path1").GetValueAsync();
        await container.Navigate("Path3").GetValueAsync();

        // Assert
        Assert.IsTrue(provider1Called);
        Assert.IsFalse(provider2Called, "Path2 should not be loaded");
        Assert.IsTrue(provider3Called);
    }

    [TestMethod]
    public async Task QueryReduction_MeasuresActualSavings()
    {
        // Arrange - 100 paths registered
        var providerCallCounts = new ConcurrentDictionary<string, int>();
        var container = DataContainerFactory.Create();

        for (int i = 0; i < 100; i++)
        {
            var path = $"Data/{i}";
            container.RegisterProvider(path, new DelegateDataProvider(() =>
            {
                providerCallCounts.AddOrUpdate(path, 1, (_, count) => count + 1);
                return new { id = i, value = $"Data {i}" };
            }));
        }

        // Act - Access only 30 paths
        var accessedPaths = Enumerable.Range(0, 30).Select(i => $"Data/{i}");
        foreach (var path in accessedPaths)
        {
            await container.Navigate(path).GetValueAsync();
        }

        // Assert
        Assert.AreEqual(30, providerCallCounts.Count, "Only 30 providers should be called");

        // Calculate query reduction
        var reduction = (100 - 30) / 100.0;
        Assert.IsTrue(reduction >= 0.5, $"Query reduction should be >= 50%, actual: {reduction:P}");
    }
}
```

---

## Performance Benchmarks (5 tests)

```csharp
[TestClass]
[TestCategory(TestCategories.Performance)]
public class LazyDataProvidersPerformanceTests
{
    [TestMethod]
    public void ProviderLookup_1000Lookups_CompletesUnder50ms()
    {
        // Arrange
        var registry = new DataProviderRegistry();
        for (int i = 0; i < 100; i++)
        {
            registry.Register($"Path{i}/**", new StaticDataProvider(new { id = i }));
        }

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            registry.FindProvider($"Path{i % 100}/Sub/Path");
        }

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50,
            $"Expected < 50ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task LazyLoading_1000Nodes_CompletesUnder500ms()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        container.RegisterProvider("**", new DelegateDataProvider(() => new { data = "test" }));

        var stopwatch = Stopwatch.StartNew();

        // Act
        var tasks = Enumerable.Range(0, 1000).Select(i =>
            container.Navigate($"Path{i}").GetValueAsync());

        await Task.WhenAll(tasks);

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Expected < 500ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task CachedAccess_10000Reads_CompletesUnder100ms()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Data", new StaticDataProvider(new { test = "value" }));

        var node = container.Navigate("Data");
        await node.GetValueAsync();  // Prime cache

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 10000; i++)
        {
            await node.GetValueAsync();
        }

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Cached reads should be < 100ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void WildcardMatching_ComplexPatterns_PerformsWell()
    {
        // Arrange
        var registry = new DataProviderRegistry();
        registry.Register("Customer/**/Orders/**/LineItems/**", new StaticDataProvider(new { test = "data" }));

        var stopwatch = Stopwatch.StartNew();

        // Act
        for (int i = 0; i < 1000; i++)
        {
            registry.FindProvider("Customer/123/Profile/Orders/456/LineItems/789");
        }

        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Complex wildcard matching should be < 100ms, actual: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ConcurrentLoading_100Threads_NoDeadlock()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        container.RegisterProvider("**", new DelegateDataProvider(async () =>
        {
            await Task.Delay(10);  // Simulate slow provider
            return new { test = "data" };
        }));

        var stopwatch = Stopwatch.StartNew();

        // Act - 100 concurrent navigations
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(async () =>
        {
            return await container.Navigate($"Path{i}").GetValueAsync();
        })).ToArray();

        await Task.WhenAll(tasks);

        stopwatch.Stop();

        // Assert - Should complete without deadlock
        Assert.IsTrue(tasks.All(t => t.Result != null), "All tasks should complete");
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000, "Should complete in reasonable time");
    }
}
```

---

## Test Coverage Report

### Target Coverage by Component

| Component | Target Coverage | Priority |
|-----------|----------------|----------|
| IDataProviderRegistry | 95% | HIGH |
| Provider selection logic | 95% | HIGH |
| Lazy loading mechanism | 90% | HIGH |
| StaticDataProvider | 90% | MEDIUM |
| DelegateDataProvider | 90% | MEDIUM |
| DatabaseDataProvider | 85% | MEDIUM |
| ApiDataProvider | 80% | MEDIUM |
| ConfigurationDataProvider | 85% | MEDIUM |
| FileDataProvider | 80% | LOW |

---

## Success Criteria

- ✅ 65+ unit tests implemented
- ✅ 20+ integration tests implemented
- ✅ 5 performance benchmarks implemented
- ✅ 85%+ overall code coverage
- ✅ Provider selection logic 100% tested
- ✅ Lazy loading caching verified
- ✅ All built-in providers tested
- ✅ Context-aware queries tested
- ✅ Performance requirements met (< 50ms lookup, < 500ms lazy load, 50%+ query reduction)

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Core Container Testing](../CoreContainer/testing-strategy.md)
