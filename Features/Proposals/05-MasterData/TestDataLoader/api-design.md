# Test Data Loader - API Design

**Feature:** Test Data Loader
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## API Overview

The Test Data Loader API provides:
1. **ITestDataLoader** - Load and manage test scenarios
2. **ITestDataCleanup** - Automatic cleanup
3. **ITestDataBuilder<T>** - Fluent data building
4. **ITestDataGenerator** - Realistic data generation

---

## Core Interfaces

### ITestDataLoader

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions;

/// <summary>
/// Loads and manages test data for automated testing scenarios.
/// </summary>
public interface ITestDataLoader
{
    /// <summary>
    /// Loads a predefined test scenario by name.
    /// </summary>
    Task<TestDataSet> LoadScenarioAsync(string scenarioName, CancellationToken ct = default);

    /// <summary>
    /// Loads a test scenario with custom options.
    /// </summary>
    Task<TestDataSet> LoadScenarioAsync(string scenarioName, ScenarioOptions options, CancellationToken ct = default);

    /// <summary>
    /// Generates a test scenario using data generation libraries.
    /// </summary>
    Task<TestDataSet> GenerateScenarioAsync(string scenarioName, Action<GenerationOptions> configure, CancellationToken ct = default);

    /// <summary>
    /// Saves a single entity to the test database.
    /// </summary>
    Task<TestDataSet> SaveAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : class;

    /// <summary>
    /// Saves multiple entities to the test database.
    /// </summary>
    Task<TestDataSet> SaveAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default) where TEntity : class;

    /// <summary>
    /// Gets the current test data set for this test execution.
    /// </summary>
    TestDataSet CurrentDataSet { get; }
}
```

---

### ITestDataCleanup

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions;

/// <summary>
/// Manages cleanup of test data after test execution.
/// </summary>
public interface ITestDataCleanup
{
    /// <summary>
    /// Cleans up a specific test data set.
    /// </summary>
    Task CleanupAsync(TestDataSet dataSet, CancellationToken ct = default);

    /// <summary>
    /// Cleans up all test data sets for the current test.
    /// </summary>
    Task CleanupAllAsync(CancellationToken ct = default);

    /// <summary>
    /// Registers a custom cleanup action.
    /// </summary>
    void RegisterCleanupAction(Func<Task> cleanupAction);

    /// <summary>
    /// Executes all registered cleanup actions.
    /// </summary>
    Task ExecuteCleanupActionsAsync();
}
```

---

### ITestDataBuilder<T>

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions;

/// <summary>
/// Fluent builder for creating test entities.
/// </summary>
public interface ITestDataBuilder<T> where T : class
{
    /// <summary>
    /// Sets a property value.
    /// </summary>
    ITestDataBuilder<T> With(Expression<Func<T, object>> property, object value);

    /// <summary>
    /// Adds a collection of related entities.
    /// </summary>
    ITestDataBuilder<T> WithMany<TRelated>(
        Expression<Func<T, ICollection<TRelated>>> property,
        int count,
        Action<ITestDataBuilder<TRelated>>? configure = null) where TRelated : class, new();

    /// <summary>
    /// Adds a single related entity.
    /// </summary>
    ITestDataBuilder<T> WithOne<TRelated>(
        Expression<Func<T, TRelated>> property,
        Action<ITestDataBuilder<TRelated>>? configure = null) where TRelated : class, new();

    /// <summary>
    /// Builds the entity (does not save).
    /// </summary>
    T Build();

    /// <summary>
    /// Builds and saves the entity.
    /// </summary>
    Task<TestDataSet> SaveAsync();
}
```

---

###  ITestDataGenerator

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions;

/// <summary>
/// Generates realistic test data using data generation libraries.
/// </summary>
public interface ITestDataGenerator
{
    /// <summary>
    /// Generates a collection of entities with default rules.
    /// </summary>
    Task<IEnumerable<T>> GenerateAsync<T>(int count, int? seed = null) where T : class, new();

    /// <summary>
    /// Generates entities with custom configuration.
    /// </summary>
    Task<IEnumerable<T>> GenerateAsync<T>(int count, Action<T> configure, int? seed = null) where T : class, new();

    /// <summary>
    /// Generates entities with Bogus faker configuration.
    /// </summary>
    Task<IEnumerable<T>> GenerateAsync<T>(int count, Action<Faker<T>> configureFaker, int? seed = null) where T : class, new();
}
```

---

## Model Classes

### TestDataSet

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions.Models;

/// <summary>
/// Represents a collection of test data for a scenario.
/// </summary>
public class TestDataSet
{
    /// <summary>
    /// Gets or sets the unique database name for this test data.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Gets or sets the entities by ID.
    /// </summary>
    public Dictionary<string, object> Entities { get; set; } = new();

    /// <summary>
    /// Gets or sets cleanup actions to execute after the test.
    /// </summary>
    public List<Func<Task>> CleanupActions { get; set; } = new();

    /// <summary>
    /// Gets a strongly-typed entity by ID.
    /// </summary>
    public T GetEntity<T>(string id) where T : class
    {
        if (Entities.TryGetValue(id, out var entity) && entity is T typedEntity)
            return typedEntity;

        throw new KeyNotFoundException($"Entity '{id}' of type {typeof(T).Name} not found");
    }

    /// <summary>
    /// Gets all entities of a specific type.
    /// </summary>
    public IEnumerable<T> GetEntities<T>() where T : class
    {
        return Entities.Values.OfType<T>();
    }

    /// <summary>
    /// Adds an entity with a generated ID.
    /// </summary>
    public void AddEntity<T>(T entity) where T : class
    {
        var id = $"{typeof(T).Name}_{Guid.NewGuid():N}";
        Entities[id] = entity;
    }

    /// <summary>
    /// Adds an entity with a specific ID.
    /// </summary>
    public void AddEntity<T>(string id, T entity) where T : class
    {
        Entities[id] = entity;
    }
}
```

---

### ScenarioOptions

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions.Models;

/// <summary>
/// Options for loading a test scenario.
/// </summary>
public class ScenarioOptions
{
    /// <summary>
    /// Gets or sets whether to use an isolated database.
    /// </summary>
    public bool UseIsolatedDatabase { get; set; } = true;

    /// <summary>
    /// Gets or sets variable substitutions for the scenario template.
    /// </summary>
    public Dictionary<string, object> Variables { get; set; } = new();

    /// <summary>
    /// Gets or sets whether to auto-register cleanup.
    /// </summary>
    public bool AutoCleanup { get; set; } = true;
}
```

---

### GenerationOptions

```csharp
namespace OoBDev.Framework.Testing.TestData.Abstractions.Models;

/// <summary>
/// Options for generating test data.
/// </summary>
public class GenerationOptions
{
    /// <summary>
    /// Gets or sets the number of entities to generate.
    /// </summary>
    public int Count { get; set; } = 10;

    /// <summary>
    /// Gets or sets the seed for reproducible generation.
    /// </summary>
    public int? Seed { get; set; }

    /// <summary>
    /// Gets or sets the locale for generated data.
    /// </summary>
    public string Locale { get; set; } = "en_US";

    /// <summary>
    /// Gets or sets custom generators per entity type.
    /// </summary>
    public Dictionary<Type, Func<object>> CustomGenerators { get; set; } = new();

    /// <summary>
    /// Gets or sets Bogus faker configurations.
    /// </summary>
    public Dictionary<Type, object> FakerConfigurations { get; set; } = new();
}
```

---

## Usage Examples

### Example 1: Load Predefined Scenario

```csharp
[TestClass]
public class OrderProcessingTests
{
    private ITestDataLoader _loader = null!;
    private ITestDataCleanup _cleanup = null!;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        services.AddTestDataLoaderEntityFramework();

        var provider = services.BuildServiceProvider();
        _loader = provider.GetRequiredService<ITestDataLoader>();
        _cleanup = provider.GetRequiredService<ITestDataCleanup>();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _cleanup.CleanupAllAsync();
    }

    [TestMethod]
    public async Task ProcessOrder_ValidOrder_Success()
    {
        // Arrange - Load scenario
        var scenario = await _loader.LoadScenarioAsync("OrderProcessing");
        var customer = scenario.GetEntity<Customer>("Customer1");
        var order = scenario.GetEntity<Order>("Order1");

        // Act
        var result = await _orderService.ProcessAsync(order);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(OrderStatus.Completed, order.Status);
    }
}
```

---

### Example 2: Builder Pattern

```csharp
[TestMethod]
public async Task CreateCustomer_ValidData_Success()
{
    // Arrange - Build custom test data
    var scenario = await TestDataBuilder<Customer>
        .Create(_loader)
        .With(c => c.Name, "John Doe")
        .With(c => c.Email, "john@example.com")
        .WithMany(c => c.Orders, 3, order => order
            .With(o => o.Total, 99.99m)
            .WithMany(o => o.Items, 2))
        .SaveAsync();

    var customer = scenario.GetEntities<Customer>().First();

    // Act
    var retrieved = await _customerService.GetByIdAsync(customer.Id);

    // Assert
    Assert.IsNotNull(retrieved);
    Assert.AreEqual("John Doe", retrieved.Name);
    Assert.AreEqual(3, retrieved.Orders.Count);
}
```

---

### Example 3: Generate Realistic Data

```csharp
[TestMethod]
public async Task BulkImport_1000Customers_Success()
{
    // Arrange - Generate 1000 realistic customers
    var scenario = await _loader.GenerateScenarioAsync("Customers", options =>
    {
        options.Count = 1000;
        options.Seed = 12345;  // Reproducible
        options.Locale = "en_US";
    });

    var customers = scenario.GetEntities<Customer>();

    // Act
    var result = await _importService.BulkImportAsync(customers);

    // Assert
    Assert.AreEqual(1000, result.SuccessCount);
    Assert.IsTrue(customers.All(c => c.Email.Contains("@")));
    Assert.IsTrue(customers.All(c => c.Name.Length > 0));
}
```

---

### Example 4: Custom Cleanup

```csharp
[TestMethod]
public async Task TestWithExternalResources()
{
    // Arrange
    var tempFile = Path.GetTempFileName();

    _cleanup.RegisterCleanupAction(async () =>
    {
        if (File.Exists(tempFile))
        {
            File.Delete(tempFile);
        }
        await Task.CompletedTask;
    });

    // Write to temp file
    await File.WriteAllTextAsync(tempFile, "test data");

    // Test logic...

    // Cleanup happens automatically in [TestCleanup]
}
```

---

### Example 5: Parallel Tests with Isolation

```csharp
[TestClass]
public class ParallelTests
{
    [TestMethod]
    [DataRow("Scenario1")]
    [DataRow("Scenario2")]
    [DataRow("Scenario3")]
    public async Task LoadScenario_Parallel_IsolatesData(string scenarioName)
    {
        // Each test gets isolated database
        var scenario = await _loader.LoadScenarioAsync(scenarioName);

        Assert.IsNotNull(scenario.DatabaseName);
        Assert.IsTrue(scenario.DatabaseName.Contains("TestDb_"));

        // Verify isolation
        var entities = scenario.GetEntities<object>();
        Assert.IsTrue(entities.Any());
    }
}
```

---

### Example 6: Performance Testing Data

```csharp
[TestMethod]
[TestCategory(TestCategories.DevLocal)]
public async Task LoadTest_10KConcurrentUsers()
{
    // Generate large volume of test data
    var scenario = await _loader.GenerateScenarioAsync("LoadTest", options =>
    {
        options.Count = 10000;
        options.CustomGenerators[typeof(User)] = () => new User
        {
            Id = Guid.NewGuid(),
            ConcurrentSessionId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
    });

    var users = scenario.GetEntities<User>().ToList();
    Assert.AreEqual(10000, users.Count);

    // Run load test
    var tasks = users.Select(u => SimulateConcurrentUserAsync(u));
    await Task.WhenAll(tasks);
}
```

---

### Example 7: xUnit Integration

```csharp
public class XUnitTests : IDisposable
{
    private readonly ITestDataLoader _loader;
    private readonly ITestDataCleanup _cleanup;

    public XUnitTests()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        services.AddTestDataLoaderEntityFramework();

        var provider = services.BuildServiceProvider();
        _loader = provider.GetRequiredService<ITestDataLoader>();
        _cleanup = provider.GetRequiredService<ITestDataCleanup>();
    }

    [Fact]
    public async Task TestMethod()
    {
        var scenario = await _loader.LoadScenarioAsync("Test");

        // Test logic...

        Assert.NotNull(scenario);
    }

    public void Dispose()
    {
        _cleanup.CleanupAllAsync().GetAwaiter().GetResult();
    }
}
```

---

### Example 8: NUnit Integration

```csharp
[TestFixture]
public class NUnitTests
{
    private ITestDataLoader _loader = null!;
    private ITestDataCleanup _cleanup = null!;

    [SetUp]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddTestDataLoaderEntityFramework();
        var provider = services.BuildServiceProvider();

        _loader = provider.GetRequiredService<ITestDataLoader>();
        _cleanup = provider.GetRequiredService<ITestDataCleanup>();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _cleanup.CleanupAllAsync();
    }

    [Test]
    public async Task TestMethod()
    {
        var scenario = await _loader.LoadScenarioAsync("Test");
        Assert.That(scenario, Is.Not.Null);
    }
}
```

---

### Example 9: Custom Data Generation

```csharp
[TestMethod]
public async Task GenerateCustomers_CustomRules_RealisticData()
{
    // Arrange
    var generator = new BogusGenerator();

    var customers = await generator.GenerateAsync<Customer>(100, faker =>
    {
        faker.RuleFor(c => c.Name, f => f.Name.FullName());
        faker.RuleFor(c => c.Email, (f, c) => f.Internet.Email(c.Name));
        faker.RuleFor(c => c.Age, f => f.Random.Int(18, 65));
        faker.RuleFor(c => c.Balance, f => f.Finance.Amount(0, 10000));
    }, seed: 12345);

    var scenario = await _loader.SaveAsync(customers);

    // Assert
    var savedCustomers = scenario.GetEntities<Customer>().ToList();
    Assert.AreEqual(100, savedCustomers.Count);
    Assert.IsTrue(savedCustomers.All(c => c.Age >= 18 && c.Age <= 65));
    Assert.IsTrue(savedCustomers.All(c => c.Balance >= 0 && c.Balance <= 10000));
}
```

---

## Scenario Template Format

### JSON Format

```json
{
  "name": "OrderProcessing",
  "description": "Basic order processing test scenario",
  "entities": {
    "customers": [
      {
        "id": "Customer1",
        "name": "John Doe",
        "email": "john@example.com",
        "address": {
          "street": "123 Main St",
          "city": "Springfield",
          "state": "IL",
          "zip": "62701"
        }
      }
    ],
    "orders": [
      {
        "id": "Order1",
        "customerId": "Customer1",
        "orderDate": "2026-01-22",
        "total": 99.99,
        "status": "Pending",
        "items": [
          {
            "productId": "P1",
            "quantity": 2,
            "price": 49.99
          }
        ]
      }
    ]
  }
}
```

---

## Configuration

### appsettings.json

```json
{
  "TestData": {
    "DatabaseNamePrefix": "TestDb_",
    "UseInMemoryDatabase": false,
    "AutoCleanup": true,
    "CleanupTimeout": "00:01:00",
    "ScenarioPath": "TestData/Scenarios",
    "Scenarios": {
      "OrderProcessing": {
        "Name": "OrderProcessing",
        "Source": "Scenarios/OrderProcessing.json"
      },
      "CustomerManagement": {
        "Name": "CustomerManagement",
        "Source": "Scenarios/CustomerManagement.json"
      }
    }
  }
}
```

---

## Extension Methods

```csharp
public static class TestDataBuilderExtensions
{
    /// <summary>
    /// Creates a new test data builder.
    /// </summary>
    public static TestDataBuilder<T> Create<T>(ITestDataLoader loader) where T : class, new()
    {
        return new TestDataBuilder<T>(loader);
    }

    /// <summary>
    /// Applies default values based on entity type.
    /// </summary>
    public static ITestDataBuilder<T> WithDefaults<T>(this ITestDataBuilder<T> builder) where T : class
    {
        // Apply sensible defaults based on type
        return builder;
    }
}

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers test data loader services.
    /// </summary>
    public static IServiceCollection AddTestDataLoader(
        this IServiceCollection services,
        Action<TestDataOptions>? configure = null)
    {
        services.TryAddScoped<ITestDataLoader, TestDataLoader>();
        services.TryAddScoped<ITestDataCleanup, TestDataCleanup>();
        services.TryAddSingleton<IScenarioProvider, ScenarioProvider>();
        services.TryAddSingleton<ITestDataGenerator, BogusGenerator>();

        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Registers Entity Framework-specific test data loader services.
    /// </summary>
    public static IServiceCollection AddTestDataLoaderEntityFramework(
        this IServiceCollection services)
    {
        services.AddTestDataLoader();
        services.TryAddScoped<ITestDataLoader, EfCoreTestDataLoader>();
        services.TryAddScoped<ITestDataCleanup, EfCoreTestDataCleanup>();
        return services;
    }
}
```

---

## Performance Characteristics

| Operation | Time | Memory | Notes |
|-----------|------|--------|-------|
| Load Scenario (10 entities) | < 50ms | < 1 MB | Fast |
| Load Scenario (1,000 entities) | < 2s | < 50 MB | Good |
| Generate (10,000 entities) | < 15s | < 200 MB | Acceptable |
| Cleanup | < 1s | N/A | Always fast |
| Builder (1 entity) | < 10ms | < 100 KB | Instant |

---

## Thread Safety

All operations are thread-safe for concurrent test execution. Each test gets an isolated database with a unique name.

---

## Best Practices

1. **Always Use [TestCleanup]**: Ensure cleanup happens even if test fails
2. **Use Scenarios for Common Data**: Define reusable scenarios
3. **Use Builders for Custom Data**: Fluent API for one-off data
4. **Use Generators for Volume**: Bogus/AutoFixture for large datasets
5. **Isolate Tests**: Never share data between tests
6. **Seed for Reproducibility**: Use consistent seeds for deterministic generation

---

## References

- Epic 05: Master Data & Test Data Management
- Architecture: Test Data Loader Architecture
- Testing: Test Data Loader Testing Strategy
- Requirements: Test Data Loader Requirements
- Bogus Documentation: https://github.com/bchavez/Bogus
- AutoFixture Documentation: https://github.com/AutoFixture/AutoFixture
