# Test Data Loader - Architecture Design

**Feature:** Test Data Loader
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

The Test Data Loader provides infrastructure for creating, loading, and managing test data in automated testing scenarios. It emphasizes data isolation, automatic cleanup, scenario-based loading, and realistic data generation using a builder pattern and integration with popular data generation libraries.

---

## Architectural Principles

### Core Principles

1. **Isolation First**: Each test gets isolated data
2. **Automatic Cleanup**: No manual cleanup required
3. **Builder Pattern**: Fluent API for data construction
4. **Scenario-Based**: Predefined test scenarios
5. **Realistic Data**: Integration with Bogus/AutoFixture
6. **MSTest Integration**: Seamless integration with MSTest framework

### Design Goals

- **Simplicity**: < 5 methods for common scenarios
- **Performance**: Load 1,000 records in < 2 seconds
- **Reliability**: 100% cleanup success rate
- **Testability**: All components unit testable
- **Extensibility**: Easy to add custom scenarios and builders

---

## System Context

```
┌─────────────────────────────────────────────────────────────┐
│                    Test Execution Layer                      │
│  (MSTest, xUnit, NUnit)                                     │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           │ Uses
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  Test Data Loader                            │
│  ┌──────────────┐  ┌────────────┐  ┌────────────────────┐  │
│  │  ITestData   │  │  ITestData │  │  ITestDataBuilder  │  │
│  │  Loader      │  │  Cleanup   │  │                    │  │
│  └──────────────┘  └────────────┘  └────────────────────┘  │
└──────────────────────────┬──────────────────────────────────┘
                           │
                           │ Persists to
                           ▼
┌─────────────────────────────────────────────────────────────┐
│                  Isolated Test Database                      │
│  (SQL Server, PostgreSQL, In-Memory, MongoDB)               │
└─────────────────────────────────────────────────────────────┘

Data Sources:
├── Scenario Templates (JSON/YAML)
├── Builder API (C# fluent)
├── Bogus (realistic generation)
└── AutoFixture (auto-generation)
```

---

## Component Architecture

### Layer Structure

```
OoBDev.Framework.Testing.TestData/
├── Abstractions/
│   ├── ITestDataLoader.cs
│   ├── ITestDataCleanup.cs
│   ├── ITestDataBuilder.cs
│   ├── ITestDataGenerator.cs
│   ├── IScenarioProvider.cs
│   └── Models/
│       ├── TestDataSet.cs
│       ├── TestScenario.cs
│       ├── CleanupHandle.cs
│       └── GenerationOptions.cs
├── Implementations/
│   ├── TestDataLoader.cs
│   ├── TestDataCleanup.cs
│   ├── ScenarioProvider.cs
│   └── Generators/
│       ├── BogusGenerator.cs
│       └── AutoFixtureGenerator.cs
├── Builders/
│   ├── TestDataBuilder.cs
│   ├── EntityBuilder.cs
│   └── ScenarioBuilder.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   ├── TestContextExtensions.cs
│   └── DbContextExtensions.cs
└── Scenarios/
    ├── ScenarioTemplates/
    │   ├── OrderProcessing.json
    │   ├── CustomerManagement.json
    │   └── InventoryManagement.json
    └── ScenarioLoader.cs

OoBDev.Framework.Testing.TestData.EntityFramework/
├── Implementations/
│   ├── EfCoreTestDataLoader.cs
│   └── EfCoreTestDataCleanup.cs
└── Extensions/
    └── TestDataSeeding.cs

OoBDev.Framework.Testing.TestData.Tests/
├── Unit/
│   ├── TestDataLoaderTests.cs
│   ├── TestDataBuilderTests.cs
│   ├── ScenarioProviderTests.cs
│   └── GeneratorTests.cs
├── Integration/
│   ├── EndToEndScenarioTests.cs
│   ├── EntityFrameworkTests.cs
│   └── CleanupTests.cs
└── TestData/
    └── Scenarios/
```

---

## Core Components

### 1. ITestDataLoader (Facade)

**Responsibility:** Load and manage test data

```csharp
public interface ITestDataLoader
{
    Task<TestDataSet> LoadScenarioAsync(string scenarioName, CancellationToken ct = default);
    Task<TestDataSet> LoadScenarioAsync(string scenarioName, ScenarioOptions options, CancellationToken ct = default);
    Task<TestDataSet> GenerateScenarioAsync(string scenarioName, Action<GenerationOptions> configure, CancellationToken ct = default);
    Task<TestDataSet> SaveAsync<TEntity>(TEntity entity, CancellationToken ct = default) where TEntity : class;
    Task<TestDataSet> SaveAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken ct = default) where TEntity : class;
    TestDataSet CurrentDataSet { get; }
}
```

**Key Behaviors:**
- Creates isolated database per test
- Loads scenario from templates
- Registers cleanup actions
- Provides entity access by ID or type

---

### 2. ITestDataCleanup

**Responsibility:** Clean up test data after tests

```csharp
public interface ITestDataCleanup
{
    Task CleanupAsync(TestDataSet dataSet, CancellationToken ct = default);
    Task CleanupAllAsync(CancellationToken ct = default);
    void RegisterCleanupAction(Func<Task> cleanupAction);
    Task ExecuteCleanupActionsAsync();
}

public class TestDataCleanup : ITestDataCleanup
{
    private readonly ConcurrentBag<TestDataSet> _dataSets = new();
    private readonly ConcurrentBag<Func<Task>> _cleanupActions = new();

    public async Task CleanupAsync(TestDataSet dataSet, CancellationToken ct = default)
    {
        if (dataSet.DatabaseName != null)
        {
            await DropDatabaseAsync(dataSet.DatabaseName, ct);
        }

        foreach (var action in dataSet.CleanupActions)
        {
            await action();
        }

        _dataSets.TryTake(out _);
    }

    public async Task CleanupAllAsync(CancellationToken ct = default)
    {
        var tasks = _dataSets.Select(ds => CleanupAsync(ds, ct));
        await Task.WhenAll(tasks);

        await ExecuteCleanupActionsAsync();
    }
}
```

---

### 3. ITestDataBuilder<T>

**Responsibility:** Fluent API for building test entities

```csharp
public interface ITestDataBuilder<T> where T : class
{
    ITestDataBuilder<T> With(Expression<Func<T, object>> property, object value);
    ITestDataBuilder<T> WithMany<TRelated>(
        Expression<Func<T, ICollection<TRelated>>> property,
        int count,
        Action<ITestDataBuilder<TRelated>>? configure = null) where TRelated : class, new();
    ITestDataBuilder<T> WithOne<TRelated>(
        Expression<Func<T, TRelated>> property,
        Action<ITestDataBuilder<TRelated>>? configure = null) where TRelated : class, new();
    T Build();
    Task<TestDataSet> SaveAsync();
}

public class TestDataBuilder<T> : ITestDataBuilder<T> where T : class, new()
{
    private readonly T _entity;
    private readonly ITestDataLoader _loader;
    private readonly List<Action<T>> _configurations = new();

    public TestDataBuilder(ITestDataLoader loader)
    {
        _entity = new T();
        _loader = loader;
    }

    public ITestDataBuilder<T> With(Expression<Func<T, object>> property, object value)
    {
        _configurations.Add(entity =>
        {
            var memberExpression = (MemberExpression)property.Body;
            var propertyInfo = (PropertyInfo)memberExpression.Member;
            propertyInfo.SetValue(entity, value);
        });

        return this;
    }

    public T Build()
    {
        foreach (var config in _configurations)
        {
            config(_entity);
        }

        return _entity;
    }

    public async Task<TestDataSet> SaveAsync()
    {
        var entity = Build();
        return await _loader.SaveAsync(entity);
    }
}
```

---

### 4. IScenarioProvider

**Responsibility:** Load scenario templates

```csharp
public interface IScenarioProvider
{
    Task<TestScenario> GetScenarioAsync(string scenarioName, CancellationToken ct = default);
    Task<IEnumerable<string>> GetAvailableScenariosAsync();
}

public class ScenarioProvider : IScenarioProvider
{
    private readonly TestDataOptions _options;
    private readonly ILogger<ScenarioProvider> _logger;

    public async Task<TestScenario> GetScenarioAsync(string scenarioName, CancellationToken ct = default)
    {
        var scenarioPath = Path.Combine(_options.ScenarioPath, $"{scenarioName}.json");

        if (!File.Exists(scenarioPath))
            throw new FileNotFoundException($"Scenario '{scenarioName}' not found at {scenarioPath}");

        var json = await File.ReadAllTextAsync(scenarioPath, ct);
        var scenario = JsonSerializer.Deserialize<TestScenario>(json);

        return scenario ?? throw new InvalidOperationException($"Failed to deserialize scenario '{scenarioName}'");
    }
}
```

---

### 5. ITestDataGenerator

**Responsibility:** Generate realistic test data

```csharp
public interface ITestDataGenerator
{
    Task<IEnumerable<T>> GenerateAsync<T>(int count, int? seed = null) where T : class, new();
    Task<IEnumerable<T>> GenerateAsync<T>(int count, Action<T> configure, int? seed = null) where T : class, new();
}

public class BogusGenerator : ITestDataGenerator
{
    public async Task<IEnumerable<T>> GenerateAsync<T>(int count, int? seed = null) where T : class, new()
    {
        var faker = new Faker<T>();

        if (seed.HasValue)
        {
            faker.UseSeed(seed.Value);
        }

        return await Task.Run(() => faker.Generate(count));
    }

    public async Task<IEnumerable<T>> GenerateAsync<T>(
        int count,
        Action<T> configure,
        int? seed = null) where T : class, new()
    {
        var entities = await GenerateAsync<T>(count, seed);

        foreach (var entity in entities)
        {
            configure(entity);
        }

        return entities;
    }
}
```

---

## Data Flow

### Scenario Loading Sequence

```
┌─────────┐          ┌──────────────┐          ┌────────────┐
│  Test   │          │  TestData    │          │ Scenario   │
│ Method  │          │  Loader      │          │ Provider   │
└────┬────┘          └──────┬───────┘          └─────┬──────┘
     │                      │                        │
     │ LoadScenarioAsync    │                        │
     │ ("OrderProcessing")  │                        │
     ├─────────────────────>│                        │
     │                      │                        │
     │                      │ GetScenario            │
     │                      ├───────────────────────>│
     │                      │                        │
     │                      │  Scenario JSON         │
     │                      │<───────────────────────┤
     │                      │                        │
     │                      ▼                        │
     │           ┌─────────────────┐                 │
     │           │ Create Unique   │                 │
     │           │ Database        │                 │
     │           └────────┬────────┘                 │
     │                    │                          │
     │                    ▼                          │
     │           ┌─────────────────┐                 │
     │           │ Parse Scenario  │                 │
     │           └────────┬────────┘                 │
     │                    │                          │
     │                    ▼                          │
     │           ┌─────────────────┐                 │
     │           │ Load Entities   │                 │
     │           └────────┬────────┘                 │
     │                    │                          │
     │                    ▼                          │
     │           ┌─────────────────┐                 │
     │           │ Register        │                 │
     │           │ Cleanup         │                 │
     │           └────────┬────────┘                 │
     │                    │                          │
     │   TestDataSet      │                          │
     │<───────────────────┤                          │
     │                    │                          │
     │  [Test executes]   │                          │
     │                    │                          │
     │  CleanupAsync      │                          │
     ├───────────────────>│                          │
     │                    │                          │
     │                    ▼                          │
     │           ┌─────────────────┐                 │
     │           │ Drop Database   │                 │
     │           └─────────────────┘                 │
```

---

## Configuration

### Options Pattern

```csharp
public class TestDataOptions
{
    public string DatabaseNamePrefix { get; set; } = "TestDb_";
    public bool UseInMemoryDatabase { get; set; } = false;
    public bool AutoCleanup { get; set; } = true;
    public TimeSpan CleanupTimeout { get; set; } = TimeSpan.FromMinutes(1);
    public string ScenarioPath { get; set; } = "Scenarios";
    public Dictionary<string, ScenarioConfiguration> Scenarios { get; set; } = new();
}

public class ScenarioConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public Dictionary<string, object> Variables { get; set; } = new();
}

public class GenerationOptions
{
    public int Count { get; set; } = 10;
    public int? Seed { get; set; }
    public string Locale { get; set; } = "en_US";
    public Dictionary<Type, Func<object>> CustomGenerators { get; set; } = new();
}
```

---

## Dependency Injection

```csharp
public static class ServiceCollectionExtensions
{
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

## MSTest Integration

```csharp
[TestClass]
public class OrderProcessingTests
{
    private ITestDataLoader _testDataLoader = null!;
    private ITestDataCleanup _cleanup = null!;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
        services.AddTestDataLoaderEntityFramework();

        var provider = services.BuildServiceProvider();
        _testDataLoader = provider.GetRequiredService<ITestDataLoader>();
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
        // Arrange
        var scenario = await _testDataLoader.LoadScenarioAsync("OrderProcessing");
        var order = scenario.GetEntity<Order>("Order1");

        // Act
        var result = await ProcessOrderAsync(order);

        // Assert
        Assert.IsTrue(result.Success);
    }
}
```

---

## Performance Optimization

### Database Pooling

```csharp
public class TestDataLoader : ITestDataLoader
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> _databaseLocks = new();

    public async Task<TestDataSet> LoadScenarioAsync(string scenarioName, CancellationToken ct = default)
    {
        var databaseName = $"TestDb_{Guid.NewGuid():N}";
        var lockObject = _databaseLocks.GetOrAdd(databaseName, _ => new SemaphoreSlim(1, 1));

        await lockObject.WaitAsync(ct);
        try
        {
            return await LoadScenarioInternalAsync(scenarioName, databaseName, ct);
        }
        finally
        {
            lockObject.Release();
        }
    }
}
```

### Parallel Test Support

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
        var scenario = await _testDataLoader.LoadScenarioAsync(scenarioName);

        // Each test gets its own isolated database
        Assert.IsNotNull(scenario.DatabaseName);
        Assert.IsTrue(scenario.DatabaseName.Contains(Guid.NewGuid().ToString("N").Substring(0, 8)));
    }
}
```

---

## Error Handling

### Exception Hierarchy

```csharp
public class TestDataException : Exception
{
    public string? ScenarioName { get; set; }
}

public class ScenarioNotFoundException : TestDataException
{
    public ScenarioNotFoundException(string scenarioName)
        : base($"Scenario '{scenarioName}' not found")
    {
        ScenarioName = scenarioName;
    }
}

public class CleanupFailedException : TestDataException
{
    public CleanupFailedException(string message, Exception inner)
        : base(message, inner)
    {
    }
}
```

---

## Security Considerations

### Isolated Databases

```csharp
public class TestDataLoader : ITestDataLoader
{
    private string GenerateIsolatedDatabaseName()
    {
        // Use GUID + timestamp for uniqueness
        var guid = Guid.NewGuid().ToString("N").Substring(0, 8);
        var timestamp = DateTime.UtcNow.Ticks;
        return $"{_options.DatabaseNamePrefix}{guid}_{timestamp}";
    }
}
```

---

## Future Enhancements

1. **Snapshot Support**: Save/restore database snapshots
2. **Data Pools**: Shared test data pools for performance
3. **Cloud Support**: Azure SQL, AWS RDS integration
4. **Docker Integration**: Spin up containers per test
5. **Data Versioning**: Version control for test scenarios
6. **Visual Studio Integration**: Test Explorer integration
7. **Performance Profiling**: Track test data loading performance
8. **Smart Cleanup**: Detect and clean orphaned databases

---

## References

- Epic 05: Master Data & Test Data Management
- Feature: Master Data Loader
- MSTest Documentation
- Bogus Documentation: https://github.com/bchavez/Bogus
- AutoFixture Documentation: https://github.com/AutoFixture/AutoFixture
