# Master Data Loader - Testing Strategy

**Feature:** Master Data Loader
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (50+ tests)
- **Integration Tests** - End-to-end scenarios with real databases (20+ tests)
- **Performance Tests** - Load time and throughput benchmarks (8 tests)
- **Concurrency Tests** - Thread-safety and parallel loading (5 tests)

**Coverage Targets:**
- Unit Tests: 90%+ coverage
- Integration Tests: 80%+ coverage
- Critical paths: 100% coverage

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (8 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │Concurrency Tests  │  (5 tests)
                  │                   │
                  └───────────────────┘
            ┌───────────────────────────────┐
            │   Integration Tests           │  (20+ tests)
            │                               │
            └───────────────────────────────┘
      ┌─────────────────────────────────────────┐
      │          Unit Tests                     │  (50+ tests)
      │                                         │
      └─────────────────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. MasterDataLoader Tests

**File:** `MasterDataLoaderTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.Framework.Data.MasterData.Abstractions;
using OoBDev.Framework.Data.MasterData.Abstractions.Models;

namespace OoBDev.Framework.Data.MasterData.Tests.Unit;

[TestClass]
public class MasterDataLoaderTests
{
    private Mock<IDataSourceProvider> _mockProvider = null!;
    private Mock<IDependencyResolver> _mockResolver = null!;
    private Mock<ILoadingStrategy> _mockStrategy = null!;
    private MasterDataLoader _loader = null!;

    [TestInitialize]
    public void Initialize()
    {
        _mockProvider = new Mock<IDataSourceProvider>();
        _mockResolver = new Mock<IDependencyResolver>();
        _mockStrategy = new Mock<ILoadingStrategy>();

        _loader = new MasterDataLoader(
            _mockProvider.Object,
            _mockResolver.Object,
            _mockStrategy.Object,
            Mock.Of<IVersionManager>(),
            Mock.Of<IChangeTracker>(),
            NullLogger<MasterDataLoader>.Instance);
    }

    [TestMethod]
    public async Task LoadAsync_ValidDataSet_LoadsSuccessfully()
    {
        // Arrange
        var dataSet = new DataSet
        {
            Name = "Countries",
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" }
            }
        };

        _mockResolver
            .Setup(r => r.ResolveAsync("Countries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Countries" });

        _mockProvider
            .Setup(p => p.LoadAsync("Data/countries.json", It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockStrategy
            .Setup(s => s.LoadAsync(dataSet, It.IsAny<DataSetMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoadResult { RecordsProcessed = 1 });

        // Act
        await _loader.LoadAsync("Countries");

        // Assert
        _mockProvider.Verify(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockStrategy.Verify(s => s.LoadAsync(It.IsAny<DataSet>(), It.IsAny<DataSetMetadata>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public async Task LoadAsync_NullDataSetName_ThrowsArgumentNullException()
    {
        // Act
        await _loader.LoadAsync(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public async Task LoadAsync_EmptyDataSetName_ThrowsArgumentException()
    {
        // Act
        await _loader.LoadAsync("");
    }

    [TestMethod]
    public async Task LoadAsync_WithDependencies_LoadsInCorrectOrder()
    {
        // Arrange
        _mockResolver
            .Setup(r => r.ResolveAsync("States", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Countries", "States" });

        var loadOrder = new List<string>();

        _mockProvider
            .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((string source, CancellationToken ct) =>
            {
                var name = source.Contains("countries") ? "Countries" : "States";
                loadOrder.Add(name);
                return new DataSet { Name = name };
            });

        // Act
        await _loader.LoadAsync("States");

        // Assert
        CollectionAssert.AreEqual(new[] { "Countries", "States" }, loadOrder);
    }

    [TestMethod]
    public async Task LoadAsync_WithCustomOptions_UsesSpecifiedStrategy()
    {
        // Arrange
        var options = new LoadOptions { Strategy = "Merge" };

        _mockResolver
            .Setup(r => r.ResolveAsync("Countries", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<string> { "Countries" });

        // Act
        await _loader.LoadAsync("Countries", options);

        // Assert
        // Verify custom strategy was used
        _mockStrategy.Verify(s => s.LoadAsync(
            It.IsAny<DataSet>(),
            It.IsAny<DataSetMetadata>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task LoadBatchAsync_MultipleDataSets_LoadsAll()
    {
        // Arrange
        var dataSetNames = new[] { "Countries", "States", "Cities" };

        _mockResolver
            .Setup(r => r.ResolveAsync(dataSetNames, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSetNames.ToList());

        _mockProvider
            .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataSet());

        _mockStrategy
            .Setup(s => s.LoadAsync(It.IsAny<DataSet>(), It.IsAny<DataSetMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoadResult { RecordsProcessed = 1 });

        // Act
        var results = await _loader.LoadBatchAsync(dataSetNames);

        // Assert
        Assert.AreEqual(3, results.Count);
        _mockProvider.Verify(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [TestMethod]
    public async Task GetStatusAsync_LoadedDataSet_ReturnsCompletedStatus()
    {
        // Arrange
        await _loader.LoadAsync("Countries");

        // Act
        var status = await _loader.GetStatusAsync("Countries");

        // Assert
        Assert.AreEqual(LoadStatus.Completed, status.Status);
    }

    [TestMethod]
    public async Task GetStatusAsync_NotLoadedDataSet_ReturnsNotLoadedStatus()
    {
        // Act
        var status = await _loader.GetStatusAsync("Countries");

        // Assert
        Assert.AreEqual(LoadStatus.NotLoaded, status.Status);
    }

    [TestMethod]
    public async Task ValidateAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataSet
            {
                Rows = new List<DataRow>
                {
                    new() { ["Code"] = "US", ["Name"] = "United States" }
                }
            });

        // Act
        var result = await _loader.ValidateAsync("Countries");

        // Assert
        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public async Task ValidateAsync_InvalidData_ReturnsErrors()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new DataSet
            {
                Rows = new List<DataRow>
                {
                    new() { ["Code"] = "", ["Name"] = "United States" }  // Invalid: empty code
                }
            });

        // Act
        var result = await _loader.ValidateAsync("Countries");

        // Assert
        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors.Count > 0);
    }

    [TestMethod]
    public async Task LoadAsync_DataSourceException_RollsBackTransaction()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DataSourceException("Failed to read source"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<MasterDataException>(
            () => _loader.LoadAsync("Countries"));
    }

    [TestMethod]
    public async Task LoadAsync_IdempotentLoad_SkipsUnchangedRecords()
    {
        // Arrange
        var dataSet = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" }
            }
        };

        _mockProvider
            .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dataSet);

        _mockStrategy
            .Setup(s => s.LoadAsync(It.IsAny<DataSet>(), It.IsAny<DataSetMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new LoadResult
            {
                RecordsProcessed = 1,
                RecordsSkipped = 1  // Record already exists
            });

        // Act - Load twice
        await _loader.LoadAsync("Countries");
        var result = await _loader.LoadAsync("Countries");

        // Assert
        Assert.AreEqual(1, result.RecordsSkipped);
        Assert.AreEqual(0, result.RecordsInserted);
    }
}
```

---

#### 2. DependencyResolver Tests

**File:** `DependencyResolverTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class DependencyResolverTests
{
    [TestMethod]
    public async Task ResolveAsync_NoDependencies_ReturnsSingleDataSet()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["Countries"] = new() { Name = "Countries", Dependencies = [] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var order = await resolver.ResolveAsync("Countries");

        // Assert
        Assert.AreEqual(1, order.Count);
        Assert.AreEqual("Countries", order[0]);
    }

    [TestMethod]
    public async Task ResolveAsync_SimpleDependency_ReturnsCorrectOrder()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["Countries"] = new() { Name = "Countries", Dependencies = [] },
            ["States"] = new() { Name = "States", Dependencies = ["Countries"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var order = await resolver.ResolveAsync("States");

        // Assert
        CollectionAssert.AreEqual(new[] { "Countries", "States" }, order.ToArray());
    }

    [TestMethod]
    public async Task ResolveAsync_MultipleDependencies_ReturnsTopologicalOrder()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["Countries"] = new() { Name = "Countries", Dependencies = [] },
            ["States"] = new() { Name = "States", Dependencies = ["Countries"] },
            ["Cities"] = new() { Name = "Cities", Dependencies = ["States"] },
            ["PostalCodes"] = new() { Name = "PostalCodes", Dependencies = ["Cities"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var order = await resolver.ResolveAsync("PostalCodes");

        // Assert
        Assert.AreEqual(4, order.Count);
        Assert.AreEqual("Countries", order[0]);
        Assert.AreEqual("States", order[1]);
        Assert.AreEqual("Cities", order[2]);
        Assert.AreEqual("PostalCodes", order[3]);
    }

    [TestMethod]
    public async Task ResolveAsync_DiamondDependency_ReturnsCorrectOrder()
    {
        // Arrange
        //     A
        //    / \
        //   B   C
        //    \ /
        //     D
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["A"] = new() { Name = "A", Dependencies = [] },
            ["B"] = new() { Name = "B", Dependencies = ["A"] },
            ["C"] = new() { Name = "C", Dependencies = ["A"] },
            ["D"] = new() { Name = "D", Dependencies = ["B", "C"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var order = await resolver.ResolveAsync("D");

        // Assert
        Assert.AreEqual(4, order.Count);
        Assert.AreEqual("A", order[0]);  // A must be first
        Assert.IsTrue(order.IndexOf("B") < order.IndexOf("D"));  // B before D
        Assert.IsTrue(order.IndexOf("C") < order.IndexOf("D"));  // C before D
    }

    [TestMethod]
    [ExpectedException(typeof(DependencyResolutionException))]
    public async Task ResolveAsync_CircularDependency_ThrowsException()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["A"] = new() { Name = "A", Dependencies = ["B"] },
            ["B"] = new() { Name = "B", Dependencies = ["A"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        await resolver.ResolveAsync("A");  // Should throw
    }

    [TestMethod]
    [ExpectedException(typeof(DependencyResolutionException))]
    public async Task ResolveAsync_CircularDependencyThreeNodes_ThrowsException()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["A"] = new() { Name = "A", Dependencies = ["B"] },
            ["B"] = new() { Name = "B", Dependencies = ["C"] },
            ["C"] = new() { Name = "C", Dependencies = ["A"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        await resolver.ResolveAsync("A");  // Should throw
    }

    [TestMethod]
    public async Task HasCircularDependenciesAsync_NoDependencies_ReturnsFalse()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["Countries"] = new() { Name = "Countries", Dependencies = [] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var hasCircular = await resolver.HasCircularDependenciesAsync("Countries");

        // Assert
        Assert.IsFalse(hasCircular);
    }

    [TestMethod]
    public async Task HasCircularDependenciesAsync_CircularDependency_ReturnsTrue()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["A"] = new() { Name = "A", Dependencies = ["B"] },
            ["B"] = new() { Name = "B", Dependencies = ["A"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var hasCircular = await resolver.HasCircularDependenciesAsync("A");

        // Assert
        Assert.IsTrue(hasCircular);
    }

    [TestMethod]
    public async Task ResolveAsync_BatchWithSharedDependencies_OptimizesOrder()
    {
        // Arrange
        var metadata = new Dictionary<string, DataSetMetadata>
        {
            ["Countries"] = new() { Name = "Countries", Dependencies = [] },
            ["States"] = new() { Name = "States", Dependencies = ["Countries"] },
            ["Cities"] = new() { Name = "Cities", Dependencies = ["States"] },
            ["Currencies"] = new() { Name = "Currencies", Dependencies = ["Countries"] }
        };

        var repository = new InMemoryMetadataRepository(metadata);
        var resolver = new DependencyResolver(repository, NullLogger<DependencyResolver>.Instance);

        // Act
        var order = await resolver.ResolveAsync(new[] { "Cities", "Currencies" });

        // Assert
        Assert.AreEqual("Countries", order[0]);  // Shared dependency loaded once
        Assert.IsTrue(order.Contains("States"));
        Assert.IsTrue(order.Contains("Cities"));
        Assert.IsTrue(order.Contains("Currencies"));
    }
}
```

---

#### 3. LoadingStrategy Tests

**File:** `LoadingStrategyTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class InsertOnlyStrategyTests
{
    [TestMethod]
    public async Task LoadAsync_NewRecords_InsertsAll()
    {
        // Arrange
        var strategy = new InsertOnlyStrategy();
        var dataSet = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" },
                new() { ["Code"] = "CA", ["Name"] = "Canada" }
            }
        };

        var metadata = new DataSetMetadata
        {
            NaturalKeys = ["Code"]
        };

        // Act
        var result = await strategy.LoadAsync(dataSet, metadata);

        // Assert
        Assert.AreEqual(2, result.RecordsInserted);
        Assert.AreEqual(0, result.RecordsUpdated);
        Assert.AreEqual(0, result.RecordsSkipped);
    }

    [TestMethod]
    public async Task LoadAsync_DuplicateRecords_SkipsExisting()
    {
        // Arrange
        var strategy = new InsertOnlyStrategy();
        var dataSet = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" }
            }
        };

        var metadata = new DataSetMetadata { NaturalKeys = ["Code"] };

        // Act - Load twice
        await strategy.LoadAsync(dataSet, metadata);
        var result = await strategy.LoadAsync(dataSet, metadata);

        // Assert
        Assert.AreEqual(0, result.RecordsInserted);
        Assert.AreEqual(1, result.RecordsSkipped);
    }
}

[TestClass]
public class UpsertStrategyTests
{
    [TestMethod]
    public async Task LoadAsync_NewAndExistingRecords_InsertsAndUpdates()
    {
        // Arrange
        var strategy = new UpsertStrategy();

        // First load
        var initialData = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" }
            }
        };

        var metadata = new DataSetMetadata { NaturalKeys = ["Code"] };
        await strategy.LoadAsync(initialData, metadata);

        // Second load with updates
        var updatedData = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "USA" },  // Updated
                new() { ["Code"] = "CA", ["Name"] = "Canada" }  // New
            }
        };

        // Act
        var result = await strategy.LoadAsync(updatedData, metadata);

        // Assert
        Assert.AreEqual(1, result.RecordsInserted);  // CA
        Assert.AreEqual(1, result.RecordsUpdated);   // US
    }

    [TestMethod]
    public async Task LoadAsync_UnchangedRecords_Skips()
    {
        // Arrange
        var strategy = new UpsertStrategy();
        var dataSet = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" }
            }
        };

        var metadata = new DataSetMetadata { NaturalKeys = ["Code"] };

        // Act - Load identical data twice
        await strategy.LoadAsync(dataSet, metadata);
        var result = await strategy.LoadAsync(dataSet, metadata);

        // Assert
        Assert.AreEqual(0, result.RecordsInserted);
        Assert.AreEqual(0, result.RecordsUpdated);
        Assert.AreEqual(1, result.RecordsSkipped);
    }
}

[TestClass]
public class MergeStrategyTests
{
    [TestMethod]
    public async Task LoadAsync_WithDeletes_RemovesMissingRecords()
    {
        // Arrange
        var strategy = new MergeStrategy();
        var metadata = new DataSetMetadata { NaturalKeys = ["Code"] };

        // Initial load
        var initialData = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" },
                new() { ["Code"] = "CA", ["Name"] = "Canada" },
                new() { ["Code"] = "MX", ["Name"] = "Mexico" }
            }
        };
        await strategy.LoadAsync(initialData, metadata);

        // Merge with fewer records
        var mergedData = new DataSet
        {
            Rows = new List<DataRow>
            {
                new() { ["Code"] = "US", ["Name"] = "United States" },
                new() { ["Code"] = "CA", ["Name"] = "Canada" }
                // MX missing - should be deleted
            }
        };

        // Act
        var result = await strategy.LoadAsync(mergedData, metadata);

        // Assert
        Assert.AreEqual(1, result.RecordsDeleted);  // MX deleted
    }
}

[TestClass]
public class StreamingStrategyTests
{
    [TestMethod]
    public async Task LoadAsync_LargeDataSet_ProcessesInBatches()
    {
        // Arrange
        var strategy = new StreamingStrategy(batchSize: 1000);
        var rows = Enumerable.Range(0, 10000)
            .Select(i => new DataRow { ["Id"] = i, ["Name"] = $"Item {i}" })
            .ToList();

        var dataSet = new DataSet { Rows = rows };
        var metadata = new DataSetMetadata { NaturalKeys = ["Id"] };

        // Act
        var result = await strategy.LoadAsync(dataSet, metadata);

        // Assert
        Assert.AreEqual(10000, result.RecordsProcessed);
        Assert.IsTrue(result.Duration.TotalSeconds < 30);  // Performance check
    }
}
```

---

#### 4. VersionManager Tests

**File:** `VersionManagerTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class VersionManagerTests
{
    [TestMethod]
    public async Task GetCurrentVersionAsync_NewDataSet_ReturnsDefaultVersion()
    {
        // Arrange
        var manager = new VersionManager();

        // Act
        var version = await manager.GetCurrentVersionAsync("Countries");

        // Assert
        Assert.AreEqual("1.0.0", version.Number);
    }

    [TestMethod]
    public async Task ApplyVersionAsync_ValidVersion_UpdatesVersion()
    {
        // Arrange
        var manager = new VersionManager();

        // Act
        await manager.ApplyVersionAsync("Countries", "1.1.0");
        var version = await manager.GetCurrentVersionAsync("Countries");

        // Assert
        Assert.AreEqual("1.1.0", version.Number);
    }

    [TestMethod]
    public async Task GetAvailableVersionsAsync_MultipleVersions_ReturnsAll()
    {
        // Arrange
        var manager = new VersionManager();
        await manager.ApplyVersionAsync("Countries", "1.0.0");
        await manager.ApplyVersionAsync("Countries", "1.1.0");
        await manager.ApplyVersionAsync("Countries", "2.0.0");

        // Act
        var versions = await manager.GetAvailableVersionsAsync("Countries");

        // Assert
        Assert.AreEqual(3, versions.Count());
    }

    [TestMethod]
    public async Task PlanMigrationAsync_ValidVersions_ReturnsPlan()
    {
        // Arrange
        var manager = new VersionManager();

        // Act
        var plan = await manager.PlanMigrationAsync("Countries", "1.0.0", "2.0.0");

        // Assert
        Assert.IsNotNull(plan);
        Assert.AreEqual("1.0.0", plan.FromVersion);
        Assert.AreEqual("2.0.0", plan.ToVersion);
        Assert.IsTrue(plan.Steps.Count > 0);
    }
}
```

---

#### 5. ChangeTracker Tests

**File:** `ChangeTrackerTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class ChangeTrackerTests
{
    [TestMethod]
    public async Task TrackChangeAsync_InsertOperation_RecordsChange()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var change = new ChangeRecord
        {
            DataSetName = "Countries",
            EntityType = "Country",
            EntityKey = "US",
            Operation = ChangeOperation.Insert,
            NewValues = "{\"Code\":\"US\",\"Name\":\"United States\"}",
            User = "system"
        };

        // Act
        await tracker.TrackChangeAsync(change);

        // Assert
        var changes = await tracker.GetChangesAsync("Countries");
        Assert.AreEqual(1, changes.Count());
    }

    [TestMethod]
    public async Task GetChangesAsync_WithDateRange_ReturnsFilteredChanges()
    {
        // Arrange
        var tracker = new ChangeTracker();
        var now = DateTime.UtcNow;

        await tracker.TrackChangeAsync(new ChangeRecord
        {
            DataSetName = "Countries",
            Timestamp = now.AddDays(-10),
            Operation = ChangeOperation.Insert
        });

        await tracker.TrackChangeAsync(new ChangeRecord
        {
            DataSetName = "Countries",
            Timestamp = now.AddDays(-5),
            Operation = ChangeOperation.Update
        });

        // Act
        var changes = await tracker.GetChangesAsync(
            "Countries",
            from: now.AddDays(-7));

        // Assert
        Assert.AreEqual(1, changes.Count());  // Only the recent change
    }

    [TestMethod]
    public async Task GetEntityChangesAsync_SpecificEntity_ReturnsEntityHistory()
    {
        // Arrange
        var tracker = new ChangeTracker();

        await tracker.TrackChangeAsync(new ChangeRecord
        {
            DataSetName = "Countries",
            EntityKey = "US",
            Operation = ChangeOperation.Insert
        });

        await tracker.TrackChangeAsync(new ChangeRecord
        {
            DataSetName = "Countries",
            EntityKey = "US",
            Operation = ChangeOperation.Update
        });

        await tracker.TrackChangeAsync(new ChangeRecord
        {
            DataSetName = "Countries",
            EntityKey = "CA",
            Operation = ChangeOperation.Insert
        });

        // Act
        var changes = await tracker.GetEntityChangesAsync("Countries", "US");

        // Assert
        Assert.AreEqual(2, changes.Count());  // Only US changes
    }
}
```

---

## Integration Tests

### Test Coverage Areas

#### 1. End-to-End Loading Tests

**File:** `EndToEndLoadingTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class EndToEndLoadingTests
{
    private ServiceProvider _services = null!;
    private IMasterDataLoader _loader = null!;
    private string _connectionString = null!;

    [TestInitialize]
    public void Initialize()
    {
        _connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer(_connectionString));
        services.AddMasterDataLoaderEntityFramework();

        _services = services.BuildServiceProvider();
        _loader = _services.GetRequiredService<IMasterDataLoader>();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureDeletedAsync();
        _services.Dispose();
    }

    [TestMethod]
    public async Task LoadAsync_CountriesFromJson_LoadsSuccessfully()
    {
        // Act
        await _loader.LoadAsync("Countries");

        // Assert
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var count = await context.Countries.CountAsync();
        Assert.IsTrue(count > 0);
        Assert.IsTrue(count >= 195);  // Expect at least 195 countries
    }

    [TestMethod]
    public async Task LoadAsync_StatesWithDependencies_LoadsBothDataSets()
    {
        // Act
        await _loader.LoadAsync("States");

        // Assert
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        var countryCount = await context.Countries.CountAsync();
        var stateCount = await context.States.CountAsync();

        Assert.IsTrue(countryCount > 0, "Countries should be loaded");
        Assert.IsTrue(stateCount > 0, "States should be loaded");
    }

    [TestMethod]
    public async Task LoadAsync_IdempotentLoad_DoesNotDuplicate()
    {
        // Act - Load twice
        await _loader.LoadAsync("Countries");
        await _loader.LoadAsync("Countries");

        // Assert
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        var count = await context.Countries.CountAsync();
        var usCount = await context.Countries.CountAsync(c => c.Code == "US");

        Assert.AreEqual(1, usCount, "US should only appear once");
    }

    [TestMethod]
    public async Task LoadBatchAsync_MultipleDataSets_LoadsAllInOrder()
    {
        // Act
        var results = await _loader.LoadBatchAsync(new[] { "Countries", "States", "Cities" });

        // Assert
        Assert.AreEqual(3, results.Count);
        Assert.IsTrue(results.All(r => r.Status == LoadStatus.Completed));

        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        Assert.IsTrue(await context.Countries.AnyAsync());
        Assert.IsTrue(await context.States.AnyAsync());
        Assert.IsTrue(await context.Cities.AnyAsync());
    }
}
```

---

#### 2. Data Source Provider Tests

**File:** `DataSourceProviderIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class DataSourceProviderIntegrationTests
{
    [TestMethod]
    public async Task JsonProvider_LoadRealFile_ReturnsValidData()
    {
        // Arrange
        var provider = new JsonDataSourceProvider();
        var path = Path.Combine(TestContext.TestDeploymentDir, "Data", "countries.json");

        // Act
        var dataSet = await provider.LoadAsync(path);

        // Assert
        Assert.IsNotNull(dataSet);
        Assert.IsTrue(dataSet.Rows.Count > 0);
        Assert.IsTrue(dataSet.Rows.All(r => r.ContainsKey("Code")));
        Assert.IsTrue(dataSet.Rows.All(r => r.ContainsKey("Name")));
    }

    [TestMethod]
    public async Task CsvProvider_LoadRealFile_ReturnsValidData()
    {
        // Arrange
        var provider = new CsvDataSourceProvider();
        var path = Path.Combine(TestContext.TestDeploymentDir, "Data", "states.csv");

        // Act
        var dataSet = await provider.LoadAsync(path);

        // Assert
        Assert.IsNotNull(dataSet);
        Assert.IsTrue(dataSet.Rows.Count > 0);
    }

    [TestMethod]
    public async Task SqlProvider_LoadFromDatabase_ReturnsValidData()
    {
        // Arrange
        var connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");
        var provider = new SqlDataSourceProvider();

        var options = new DataSourceOptions
        {
            ["ConnectionString"] = connectionString
        };

        // Act
        var dataSet = await provider.LoadAsync(
            "SELECT Code, Name FROM Countries WHERE Population > 1000000",
            options);

        // Assert
        Assert.IsNotNull(dataSet);
        Assert.IsTrue(dataSet.Rows.Count > 0);
    }

    [TestMethod]
    public async Task ApiProvider_LoadFromRestApi_ReturnsValidData()
    {
        // Arrange
        var provider = new ApiDataSourceProvider();
        var url = "https://restcountries.com/v3.1/all";

        // Act
        var dataSet = await provider.LoadAsync(url);

        // Assert
        Assert.IsNotNull(dataSet);
        Assert.IsTrue(dataSet.Rows.Count > 0);
    }
}
```

---

#### 3. Entity Framework Integration Tests

**File:** `EntityFrameworkIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class EntityFrameworkIntegrationTests
{
    [TestMethod]
    public async Task DbContext_LoadMasterData_InsertsRecords()
    {
        // Arrange
        var connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer(connectionString));
        services.AddMasterDataLoaderEntityFramework();

        var provider = services.BuildServiceProvider();

        // Act
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TestDbContext>();
        await context.Database.EnsureCreatedAsync();
        await context.LoadMasterDataAsync("Countries");

        // Assert
        var count = await context.Countries.CountAsync();
        Assert.IsTrue(count > 0);
    }

    [TestMethod]
    public async Task ModelBuilder_SeedFromMasterData_HasData()
    {
        // This test verifies that OnModelCreating seeding works
        // The data is seeded during migration, not runtime

        var connectionString = TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING");

        var services = new ServiceCollection();
        services.AddDbContext<SeededDbContext>(options =>
            options.UseSqlServer(connectionString));

        var provider = services.BuildServiceProvider();

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<SeededDbContext>();
        await context.Database.EnsureCreatedAsync();

        // Assert - Data should be seeded automatically
        var count = await context.Countries.CountAsync();
        Assert.IsTrue(count > 0, "Countries should be seeded during database creation");
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
    public async Task LoadAsync_10KRecords_CompletesInUnder5Seconds()
    {
        // Arrange
        var loader = CreateLoader();
        var dataSet = GenerateDataSet(10000);

        // Act
        var sw = Stopwatch.StartNew();
        await loader.LoadAsync(dataSet);
        sw.Stop();

        // Assert
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 5,
            $"Expected < 5s, actual: {sw.Elapsed.TotalSeconds:F2}s");
    }

    [TestMethod]
    public async Task LoadAsync_100KRecords_CompletesInUnder30Seconds()
    {
        // Arrange
        var loader = CreateLoader();
        var dataSet = GenerateDataSet(100000);

        // Act
        var sw = Stopwatch.StartNew();
        await loader.LoadAsync(dataSet);
        sw.Stop();

        // Assert
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 30,
            $"Expected < 30s, actual: {sw.Elapsed.TotalSeconds:F2}s");
    }

    [TestMethod]
    public async Task LoadAsync_1MRecords_UsesConstantMemory()
    {
        // Arrange
        var loader = CreateLoader(strategy: "Streaming");
        var initialMemory = GC.GetTotalMemory(forceFullCollection: true);

        // Act
        await loader.LoadAsync(GenerateLargeDataSet(1000000));

        var finalMemory = GC.GetTotalMemory(forceFullCollection: true);
        var memoryUsed = (finalMemory - initialMemory) / (1024 * 1024);  // MB

        // Assert
        Assert.IsTrue(memoryUsed < 500,
            $"Expected < 500 MB, actual: {memoryUsed} MB");
    }

    [TestMethod]
    public async Task DependencyResolver_1000DataSets_ResolvesQuickly()
    {
        // Arrange
        var resolver = CreateResolverWith1000DataSets();

        // Act
        var sw = Stopwatch.StartNew();
        await resolver.ResolveAsync("DataSet999");
        sw.Stop();

        // Assert
        Assert.IsTrue(sw.Elapsed.TotalMilliseconds < 100,
            $"Expected < 100ms, actual: {sw.Elapsed.TotalMilliseconds:F2}ms");
    }

    [TestMethod]
    public async Task LoadBatchAsync_ParallelLoading_FasterThanSequential()
    {
        // Arrange
        var loader = CreateLoader();
        var dataSetNames = new[] { "DS1", "DS2", "DS3", "DS4", "DS5" };

        // Sequential baseline
        var seqSw = Stopwatch.StartNew();
        foreach (var name in dataSetNames)
        {
            await loader.LoadAsync(name);
        }
        seqSw.Stop();

        // Act - Parallel
        var parSw = Stopwatch.StartNew();
        await loader.LoadBatchAsync(dataSetNames);
        parSw.Stop();

        // Assert
        Assert.IsTrue(parSw.Elapsed < seqSw.Elapsed,
            $"Parallel ({parSw.ElapsedMilliseconds}ms) should be faster than sequential ({seqSw.ElapsedMilliseconds}ms)");
    }

    [TestMethod]
    public async Task ChangeTracker_100KChanges_QueryPerformance()
    {
        // Arrange
        var tracker = CreateChangeTracker();

        // Insert 100K changes
        for (int i = 0; i < 100000; i++)
        {
            await tracker.TrackChangeAsync(new ChangeRecord
            {
                DataSetName = "TestData",
                EntityKey = $"Entity{i}",
                Operation = ChangeOperation.Insert
            });
        }

        // Act
        var sw = Stopwatch.StartNew();
        var changes = await tracker.GetChangesAsync("TestData",
            from: DateTime.UtcNow.AddDays(-1));
        sw.Stop();

        // Assert
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 1,
            $"Expected < 1s for query, actual: {sw.Elapsed.TotalSeconds:F2}s");
    }

    [TestMethod]
    public async Task UpsertStrategy_DetectChanges_EfficientComparison()
    {
        // Arrange
        var strategy = new UpsertStrategy();
        var dataSet = GenerateDataSet(50000);  // 50K records

        // Initial load
        await strategy.LoadAsync(dataSet, new DataSetMetadata());

        // Modify 1% of records
        for (int i = 0; i < 500; i++)
        {
            dataSet.Rows[i * 100]["Name"] = "Modified";
        }

        // Act
        var sw = Stopwatch.StartNew();
        var result = await strategy.LoadAsync(dataSet, new DataSetMetadata());
        sw.Stop();

        // Assert
        Assert.AreEqual(500, result.RecordsUpdated);
        Assert.IsTrue(sw.Elapsed.TotalSeconds < 10,
            $"Expected < 10s, actual: {sw.Elapsed.TotalSeconds:F2}s");
    }

    [TestMethod]
    public async Task JsonProvider_StreamParsing_LowMemoryFootprint()
    {
        // Arrange
        var provider = new JsonDataSourceProvider();
        var largeFile = GenerateLargeJsonFile(100000);  // 100K records
        var initialMemory = GC.GetTotalMemory(forceFullCollection: true);

        // Act
        await foreach (var row in provider.StreamAsync(largeFile))
        {
            // Process each row
            _ = row.GetValue<string>("Name");
        }

        var finalMemory = GC.GetTotalMemory(forceFullCollection: true);
        var memoryUsed = (finalMemory - initialMemory) / (1024 * 1024);  // MB

        // Assert
        Assert.IsTrue(memoryUsed < 100,
            $"Expected < 100 MB for streaming, actual: {memoryUsed} MB");
    }
}
```

---

## Concurrency Tests

**File:** `ConcurrencyTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class ConcurrencyTests
{
    [TestMethod]
    public async Task LoadAsync_ConcurrentReads_ThreadSafe()
    {
        // Arrange
        var loader = CreateLoader();
        await loader.LoadAsync("Countries");  // Initial load

        // Act - 100 concurrent reads
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => loader.GetStatusAsync("Countries"))
            .ToArray();

        // Assert - Should not throw
        var results = await Task.WhenAll(tasks);
        Assert.AreEqual(100, results.Length);
        Assert.IsTrue(results.All(r => r.Status == LoadStatus.Completed));
    }

    [TestMethod]
    public async Task LoadAsync_ConcurrentLoadsWithDependencies_Serialized()
    {
        // Arrange
        var loader = CreateLoader();

        // Act - Try to load States and Cities concurrently
        // Both depend on Countries, which should be loaded only once
        var task1 = loader.LoadAsync("States");
        var task2 = loader.LoadAsync("Cities");

        await Task.WhenAll(task1, task2);

        // Assert
        var countriesStatus = await loader.GetStatusAsync("Countries");
        Assert.AreEqual(LoadStatus.Completed, countriesStatus.Status);
        // Verify Countries was loaded exactly once (check logs or metrics)
    }

    [TestMethod]
    public async Task ChangeTracker_ConcurrentWrites_AllChangesRecorded()
    {
        // Arrange
        var tracker = CreateChangeTracker();

        // Act - 50 concurrent writes
        var tasks = Enumerable.Range(0, 50)
            .Select(i => tracker.TrackChangeAsync(new ChangeRecord
            {
                DataSetName = "TestData",
                EntityKey = $"Entity{i}",
                Operation = ChangeOperation.Insert
            }))
            .ToArray();

        await Task.WhenAll(tasks);

        // Assert
        var changes = await tracker.GetChangesAsync("TestData");
        Assert.AreEqual(50, changes.Count());
    }

    [TestMethod]
    public async Task LoadBatchAsync_ParallelIndependentDataSets_LoadsConcurrently()
    {
        // Arrange
        var loader = CreateLoader();

        // Three independent data sets (no dependencies)
        var dataSetNames = new[] { "Countries", "Currencies", "Languages" };

        // Act
        var sw = Stopwatch.StartNew();
        await loader.LoadBatchAsync(dataSetNames);
        sw.Stop();

        // Assert
        // Parallel loading should be faster than sequential
        // (Hard to assert exact time, but verify all loaded successfully)
        var results = await Task.WhenAll(
            dataSetNames.Select(name => loader.GetStatusAsync(name)));

        Assert.IsTrue(results.All(r => r.Status == LoadStatus.Completed));
        Console.WriteLine($"Parallel load time: {sw.Elapsed.TotalSeconds:F2}s");
    }

    [TestMethod]
    public async Task VersionManager_ConcurrentVersionChecks_Consistent()
    {
        // Arrange
        var manager = CreateVersionManager();
        await manager.ApplyVersionAsync("Countries", "1.0.0");

        // Act - 100 concurrent version checks
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => manager.GetCurrentVersionAsync("Countries"))
            .ToArray();

        var versions = await Task.WhenAll(tasks);

        // Assert
        Assert.IsTrue(versions.All(v => v.Number == "1.0.0"));
    }
}
```

---

## Test Data

### Test Data Files

**Data/countries.json:**
```json
[
  {
    "Code": "US",
    "Name": "United States",
    "ISO3": "USA",
    "Population": 331449281,
    "Capital": "Washington, D.C."
  },
  {
    "Code": "CA",
    "Name": "Canada",
    "ISO3": "CAN",
    "Population": 38005238,
    "Capital": "Ottawa"
  },
  {
    "Code": "MX",
    "Name": "Mexico",
    "ISO3": "MEX",
    "Population": 128932753,
    "Capital": "Mexico City"
  }
]
```

**Data/states.csv:**
```csv
Code,Name,CountryCode,Population
CA,California,US,39512223
TX,Texas,US,28995881
NY,New York,US,19453561
ON,Ontario,CA,14734014
QC,Quebec,CA,8574571
```

---

## Coverage Goals

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| MasterDataLoader | 90% | 100% |
| DependencyResolver | 95% | 100% |
| LoadingStrategies | 90% | 100% |
| DataSourceProviders | 85% | 95% |
| VersionManager | 85% | 95% |
| ChangeTracker | 85% | 90% |

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

# Specific test class
dotnet test --filter "FullyQualifiedName~MasterDataLoaderTests"

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

### CI/CD Integration

```yaml
# .github/workflows/master-data-loader.yml
name: Master Data Loader Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest

    services:
      sqlserver:
        image: mcr.microsoft.com/mssql/server:2022-latest
        env:
          ACCEPT_EULA: Y
          SA_PASSWORD: YourStrong@Passw0rd
        ports:
          - 1433:1433

    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'

      - name: Run unit tests
        run: dotnet test --filter "TestCategory=Unit" --logger trx

      - name: Run integration tests
        run: dotnet test --filter "TestCategory=Integration" --logger trx
        env:
          SQL_CONNECTION_STRING: "Server=localhost;Database=TestDb;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True;"

      - name: Generate coverage report
        run: dotnet test --collect:"XPlat Code Coverage"
```

---

## Mocking Strategy

### Common Mock Setups

```csharp
// Mock IDataSourceProvider
var mockProvider = new Mock<IDataSourceProvider>();
mockProvider
    .Setup(p => p.CanHandle(It.IsAny<string>()))
    .Returns(true);
mockProvider
    .Setup(p => p.LoadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new DataSet { Rows = new List<DataRow>() });

// Mock IDependencyResolver
var mockResolver = new Mock<IDependencyResolver>();
mockResolver
    .Setup(r => r.ResolveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync((string name, CancellationToken ct) => new List<string> { name });

// Mock ILoadingStrategy
var mockStrategy = new Mock<ILoadingStrategy>();
mockStrategy
    .Setup(s => s.LoadAsync(It.IsAny<DataSet>(), It.IsAny<DataSetMetadata>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new LoadResult { RecordsProcessed = 1 });

// Mock IVersionManager
var mockVersionManager = new Mock<IVersionManager>();
mockVersionManager
    .Setup(v => v.GetCurrentVersionAsync(It.IsAny<string>()))
    .ReturnsAsync(new VersionInfo { Number = "1.0.0" });

// Mock IChangeTracker
var mockChangeTracker = new Mock<IChangeTracker>();
mockChangeTracker
    .Setup(c => c.TrackChangeAsync(It.IsAny<ChangeRecord>(), It.IsAny<CancellationToken>()))
    .Returns(Task.CompletedTask);
```

---

## Test Utilities

**File:** `TestHelpers.cs`

```csharp
public static class TestHelpers
{
    public static DataSet GenerateDataSet(int recordCount)
    {
        var rows = Enumerable.Range(0, recordCount)
            .Select(i => new DataRow
            {
                ["Id"] = i,
                ["Code"] = $"CODE{i:D6}",
                ["Name"] = $"Item {i}",
                ["Value"] = i * 100.0
            })
            .ToList();

        return new DataSet { Rows = rows };
    }

    public static DataSetMetadata CreateMetadata(
        string name,
        params string[] dependencies)
    {
        return new DataSetMetadata
        {
            Name = name,
            Dependencies = dependencies.ToList(),
            NaturalKeys = ["Code"],
            Version = "1.0.0"
        };
    }

    public static IMasterDataLoader CreateTestLoader(
        IDataSourceProvider? provider = null,
        IDependencyResolver? resolver = null,
        ILoadingStrategy? strategy = null)
    {
        provider ??= Mock.Of<IDataSourceProvider>();
        resolver ??= Mock.Of<IDependencyResolver>();
        strategy ??= Mock.Of<ILoadingStrategy>();

        return new MasterDataLoader(
            provider,
            resolver,
            strategy,
            Mock.Of<IVersionManager>(),
            Mock.Of<IChangeTracker>(),
            NullLogger<MasterDataLoader>.Instance);
    }
}
```

---

## References

- Epic 05: Master Data & Test Data Management
- Architecture: Master Data Loader Architecture
- API Design: Master Data Loader API Design
- MSTest Documentation: https://learn.microsoft.com/visualstudio/test/using-microsoft-visualstudio-testtools-unittesting-members-in-unit-tests
- Moq Framework: https://github.com/moq/moq4
