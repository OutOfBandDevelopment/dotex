# Test Data Loader - Testing Strategy

**Feature:** Test Data Loader
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and usability tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing (40+ tests)
- **Integration Tests** - End-to-end scenarios (15+ tests)
- **Usability Tests** - Developer experience validation (10 tests)
- **Performance Tests** - Load time and cleanup benchmarks (5 tests)

**Coverage Targets:**
- Unit Tests: 90%+ coverage
- Integration Tests: 80%+ coverage
- Critical paths: 100% coverage

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (5 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Usability Tests  │  (10 tests)
                  │                   │
                  └───────────────────┘
            ┌───────────────────────────────┐
            │   Integration Tests           │  (15+ tests)
            │                               │
            └───────────────────────────────┘
      ┌─────────────────────────────────────────┐
      │          Unit Tests                     │  (40+ tests)
      │                                         │
      └─────────────────────────────────────────┘
```

---

## Unit Tests

### 1. TestDataLoader Tests

**File:** `TestDataLoaderTests.cs`

```csharp
[TestClass]
public class TestDataLoaderTests
{
    [TestMethod]
    public async Task LoadScenarioAsync_ValidScenario_ReturnsDataSet()
    {
        var mockProvider = new Mock<IScenarioProvider>();
        mockProvider.Setup(p => p.GetScenarioAsync("Test", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestScenario { Name = "Test" });

        var loader = new TestDataLoader(mockProvider.Object, /* ... */);
        var dataSet = await loader.LoadScenarioAsync("Test");

        Assert.IsNotNull(dataSet);
        Assert.IsNotNull(dataSet.DatabaseName);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task LoadScenarioAsync_NullScenarioName_ThrowsException()
    {
        var loader = CreateTestLoader();
        await loader.LoadScenarioAsync(null!);
    }

    [TestMethod]
    public async Task LoadScenarioAsync_IsolatedDatabase_UniqueNames()
    {
        var loader = CreateTestLoader();

        var dataSet1 = await loader.LoadScenarioAsync("Test");
        var dataSet2 = await loader.LoadScenarioAsync("Test");

        Assert.AreNotEqual(dataSet1.DatabaseName, dataSet2.DatabaseName);
    }

    [TestMethod]
    public async Task GenerateScenarioAsync_ValidOptions_GeneratesData()
    {
        var loader = CreateTestLoader();

        var dataSet = await loader.GenerateScenarioAsync("Test", options =>
        {
            options.Count = 10;
            options.Seed = 12345;
        });

        Assert.IsNotNull(dataSet);
        Assert.AreEqual(10, dataSet.GetEntities<object>().Count());
    }
}
```

### 2. TestDataBuilder Tests

**File:** `TestDataBuilderTests.cs`

```csharp
[TestClass]
public class TestDataBuilderTests
{
    [TestMethod]
    public void With_SetProperty_AppliesValue()
    {
        var builder = new TestDataBuilder<Customer>(Mock.Of<ITestDataLoader>());
        var customer = builder.With(c => c.Name, "John Doe").Build();
        Assert.AreEqual("John Doe", customer.Name);
    }

    [TestMethod]
    public void WithMany_GeneratesCollection()
    {
        var builder = new TestDataBuilder<Customer>(Mock.Of<ITestDataLoader>());
        var customer = builder.WithMany(c => c.Orders, 3).Build();
        Assert.AreEqual(3, customer.Orders.Count);
    }

    [TestMethod]
    public async Task SaveAsync_ValidEntity_SavesAndReturnsDataSet()
    {
        var mockLoader = new Mock<ITestDataLoader>();
        mockLoader.Setup(l => l.SaveAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TestDataSet());

        var builder = new TestDataBuilder<Customer>(mockLoader.Object);
        var dataSet = await builder.With(c => c.Name, "John").SaveAsync();

        Assert.IsNotNull(dataSet);
        mockLoader.Verify(l => l.SaveAsync(It.IsAny<Customer>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

### 3. TestDataCleanup Tests

**File:** `TestDataCleanupTests.cs`

```csharp
[TestClass]
public class TestDataCleanupTests
{
    [TestMethod]
    public async Task CleanupAsync_ValidDataSet_DropsDatabase()
    {
        var cleanup = new TestDataCleanup(/* ... */);
        var dataSet = new TestDataSet { DatabaseName = "TestDb_12345" };

        await cleanup.CleanupAsync(dataSet);
        // Verify database was dropped
    }

    [TestMethod]
    public async Task CleanupAllAsync_MultipleDataSets_CleansAll()
    {
        var cleanup = new TestDataCleanup();
        var dataSet1 = new TestDataSet { DatabaseName = "TestDb_1" };
        var dataSet2 = new TestDataSet { DatabaseName = "TestDb_2" };

        await cleanup.CleanupAllAsync();
        // Verify both databases dropped
    }

    [TestMethod]
    public async Task RegisterCleanupAction_CustomAction_ExecutesOnCleanup()
    {
        var cleanup = new TestDataCleanup();
        var actionExecuted = false;
        cleanup.RegisterCleanupAction(async () =>
        {
            actionExecuted = true;
            await Task.CompletedTask;
        });

        await cleanup.ExecuteCleanupActionsAsync();
        Assert.IsTrue(actionExecuted);
    }
}
```

---

## Integration Tests

### End-to-End Scenario Tests

**File:** `EndToEndScenarioTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class EndToEndScenarioTests
{
    private ITestDataLoader _loader = null!;
    private ITestDataCleanup _cleanup = null!;

    [TestInitialize]
    public void Initialize()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");

        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer(connectionString));
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
    public async Task LoadScenario_OrderProcessing_LoadsAllEntities()
    {
        var dataSet = await _loader.LoadScenarioAsync("OrderProcessing");

        Assert.IsNotNull(dataSet);
        var customer = dataSet.GetEntity<Customer>("Customer1");
        Assert.IsNotNull(customer);
        Assert.AreEqual("John Doe", customer.Name);

        var order = dataSet.GetEntity<Order>("Order1");
        Assert.IsNotNull(order);
        Assert.AreEqual(customer.Id, order.CustomerId);
    }

    [TestMethod]
    public async Task GenerateScenario_1000Customers_PerformsWell()
    {
        var sw = Stopwatch.StartNew();
        var dataSet = await _loader.GenerateScenarioAsync("Customers", options =>
        {
            options.Count = 1000;
        });
        sw.Stop();

        var customers = dataSet.GetEntities<Customer>();
        Assert.AreEqual(1000, customers.Count());
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 15, $"Took {sw.Elapsed.TotalSeconds:F2}s");
    }
}
```

---

## Performance Tests

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class PerformanceTests
{
    [TestMethod]
    public async Task LoadScenario_SmallDataSet_Under50ms()
    {
        var loader = CreateTestLoader();

        var sw = Stopwatch.StartNew();
        await loader.LoadScenarioAsync("SmallScenario");
        sw.Stop();

        Assert.IsTrue(sw.Elapsed.TotalMilliseconds < 50);
    }

    [TestMethod]
    public async Task GenerateScenario_10KEntities_Under15Seconds()
    {
        var loader = CreateTestLoader();

        var sw = Stopwatch.StartNew();
        await loader.GenerateScenarioAsync("Large", options =>
        {
            options.Count = 10000;
        });
        sw.Stop();

        Assert.IsTrue(sw.Elapsed.TotalSeconds < 15);
    }

    [TestMethod]
    public async Task Cleanup_SingleDatabase_Under1Second()
    {
        var loader = CreateTestLoader();
        var cleanup = CreateTestCleanup();

        var dataSet = await loader.LoadScenarioAsync("Test");

        var sw = Stopwatch.StartNew();
        await cleanup.CleanupAsync(dataSet);
        sw.Stop();

        Assert.IsTrue(sw.Elapsed.TotalSeconds < 1);
    }

    [TestMethod]
    public async Task Builder_100Entities_FastConstruction()
    {
        var loader = CreateTestLoader();

        var sw = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            var customer = await TestDataBuilder<Customer>
                .Create(loader)
                .With(c => c.Name, $"Customer{i}")
                .Build();
        }
        sw.Stop();

        Assert.IsTrue(sw.Elapsed.TotalSeconds < 1);
    }
}
```

---

## Coverage Goals

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| TestDataLoader | 90% | 100% |
| TestDataBuilder | 90% | 100% |
| TestDataCleanup | 95% | 100% |
| BogusGenerator | 85% | 95% |
| ScenarioProvider | 85% | 90% |

**Overall Target: 85%+**

---

## Test Execution

### Running Tests

```bash
# All unit tests
dotnet test --filter "TestCategory=Unit"

# All integration tests
dotnet test --filter "TestCategory=Integration"

# Performance tests (local only)
dotnet test --filter "TestCategory=DevLocal"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

---

## References

- Epic 05: Master Data & Test Data Management
- Architecture: Test Data Loader Architecture
- API Design: Test Data Loader API Design
- Requirements: Test Data Loader Requirements
