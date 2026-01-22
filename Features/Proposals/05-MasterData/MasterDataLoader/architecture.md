# Master Data Loader - Architecture Design

**Feature:** Master Data Loader
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

The Master Data Loader provides a comprehensive infrastructure for loading, versioning, and managing reference data in applications. It uses a provider-based architecture to support multiple data sources, dependency resolution, idempotent operations, and production-grade reliability.

---

## Architectural Principles

### Core Principles

1. **Provider Pattern**: Pluggable data sources via IDataSourceProvider
2. **Strategy Pattern**: Multiple loading strategies (insert-only, upsert, merge)
3. **Repository Pattern**: Abstract data persistence concerns
4. **Factory Pattern**: Data source provider selection and instantiation
5. **Single Responsibility**: Each component has one clear purpose
6. **Dependency Injection**: All dependencies injected via constructor
7. **Idempotency**: Safe to execute multiple times without side effects

### Design Goals

- **Reliability**: Transaction support, rollback, audit trail
- **Performance**: Parallel loading, streaming, batching
- **Extensibility**: Easy to add new data sources and strategies
- **Observability**: Comprehensive logging and metrics
- **Testability**: All components unit testable

---

## System Context

```
┌─────────────────────────────────────────────────────────────────┐
│                        Application Layer                         │
│  (Web APIs, Background Services, CLI Tools, EF Migrations)      │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            │ Uses
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Master Data Loader                           │
│  ┌──────────────────┐  ┌─────────────────┐  ┌────────────────┐ │
│  │  IMasterData     │  │  IDataSource    │  │  ILoading      │ │
│  │  Loader          │  │  Provider       │  │  Strategy      │ │
│  └──────────────────┘  └─────────────────┘  └────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
                            │ Persists to
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      Data Storage Layer                          │
│  (SQL Server, PostgreSQL, MongoDB, Azure Table Storage)         │
└─────────────────────────────────────────────────────────────────┘

External Data Sources:
├── JSON Files (file system, blob storage)
├── CSV Files (file system, network shares)
├── XML Files (file system, configuration)
├── SQL Databases (legacy systems, data warehouses)
└── REST APIs (external services, microservices)
```

---

## Component Architecture

### Layer Structure

```
OoBDev.Framework.Data.MasterData/
├── Abstractions/
│   ├── IMasterDataLoader.cs
│   ├── IDataSourceProvider.cs
│   ├── ILoadingStrategy.cs
│   ├── IVersionManager.cs
│   ├── IDependencyResolver.cs
│   ├── IChangeTracker.cs
│   └── Models/
│       ├── DataSet.cs
│       ├── LoadStatus.cs
│       ├── LoadOptions.cs
│       ├── DataSetMetadata.cs
│       ├── VersionInfo.cs
│       └── ChangeRecord.cs
├── Implementations/
│   ├── MasterDataLoader.cs
│   ├── VersionManager.cs
│   ├── DependencyResolver.cs
│   ├── ChangeTracker.cs
│   └── LoadingStrategies/
│       ├── InsertOnlyStrategy.cs
│       ├── UpsertStrategy.cs
│       ├── MergeStrategy.cs
│       └── StreamingStrategy.cs
├── Providers/
│   ├── JsonDataSourceProvider.cs
│   ├── CsvDataSourceProvider.cs
│   ├── XmlDataSourceProvider.cs
│   ├── SqlDataSourceProvider.cs
│   └── ApiDataSourceProvider.cs
├── Extensions/
│   ├── ServiceCollectionExtensions.cs
│   ├── DbContextExtensions.cs
│   └── ModelBuilderExtensions.cs
└── Configuration/
    ├── MasterDataLoaderOptions.cs
    ├── DataSourceOptions.cs
    └── LoadingStrategyOptions.cs

OoBDev.Framework.Data.MasterData.EntityFramework/
├── Implementations/
│   ├── EfCoreMasterDataLoader.cs
│   ├── EfCoreChangeTracker.cs
│   └── EfCoreVersionManager.cs
└── Extensions/
    └── MasterDataSeeding.cs

OoBDev.Framework.Data.MasterData.Tests/
├── Unit/
│   ├── MasterDataLoaderTests.cs
│   ├── DependencyResolverTests.cs
│   ├── VersionManagerTests.cs
│   └── LoadingStrategies/
├── Integration/
│   ├── JsonSourceTests.cs
│   ├── CsvSourceTests.cs
│   ├── SqlSourceTests.cs
│   └── EndToEndTests.cs
└── TestData/
    ├── countries.json
    ├── states.csv
    └── metadata/
```

---

## Core Components

### 1. IMasterDataLoader (Facade)

**Responsibility:** Orchestrates the entire loading process

```csharp
public interface IMasterDataLoader
{
    // Primary operations
    Task LoadAsync(string dataSetName, CancellationToken ct = default);
    Task LoadAsync(string dataSetName, LoadOptions options, CancellationToken ct = default);
    Task<IReadOnlyList<LoadResult>> LoadBatchAsync(
        IEnumerable<string> dataSetNames,
        CancellationToken ct = default);

    // Status and monitoring
    Task<LoadStatus> GetStatusAsync(string dataSetName);
    Task<IEnumerable<string>> GetAvailableDataSetsAsync();
    Task<IEnumerable<string>> GetLoadedDataSetsAsync();

    // Validation
    Task<ValidationResult> ValidateAsync(string dataSetName, CancellationToken ct = default);

    // Version management
    Task<VersionInfo> GetVersionAsync(string dataSetName);
    Task MigrateToVersionAsync(string dataSetName, string targetVersion, CancellationToken ct = default);

    // Change tracking
    Task<IEnumerable<ChangeRecord>> GetChangeHistoryAsync(
        string dataSetName,
        DateTime? from = null,
        DateTime? to = null);
}
```

**Key Behaviors:**
- Validates data set configuration
- Resolves dependencies automatically
- Selects appropriate loading strategy
- Manages transactions and rollback
- Tracks changes and versions
- Emits telemetry events

---

### 2. IDataSourceProvider (Strategy)

**Responsibility:** Read data from external sources

```csharp
public interface IDataSourceProvider
{
    string SourceType { get; }

    bool CanHandle(string source);

    Task<DataSet> LoadAsync(string source, CancellationToken ct = default);
    Task<DataSet> LoadAsync(string source, DataSourceOptions options, CancellationToken ct = default);

    IAsyncEnumerable<DataRow> StreamAsync(string source, CancellationToken ct = default);
}

public abstract class DataSourceProviderBase : IDataSourceProvider
{
    protected ILogger Logger { get; }

    public abstract string SourceType { get; }
    public abstract bool CanHandle(string source);

    public virtual async Task<DataSet> LoadAsync(string source, CancellationToken ct = default)
    {
        var rows = new List<DataRow>();
        await foreach (var row in StreamAsync(source, ct))
        {
            rows.Add(row);
        }
        return new DataSet(rows);
    }

    public abstract IAsyncEnumerable<DataRow> StreamAsync(
        string source,
        CancellationToken ct = default);
}
```

**Implementations:**
- JsonDataSourceProvider: JSON file/string parsing
- CsvDataSourceProvider: CSV file parsing with delimiter options
- XmlDataSourceProvider: XML file/XPath parsing
- SqlDataSourceProvider: SQL query execution
- ApiDataSourceProvider: REST API calls with pagination

---

### 3. ILoadingStrategy (Strategy)

**Responsibility:** Determine how to persist data

```csharp
public interface ILoadingStrategy
{
    string Name { get; }

    Task<LoadResult> LoadAsync(
        DataSet dataSet,
        DataSetMetadata metadata,
        CancellationToken ct = default);
}

public interface ILoadingStrategyFactory
{
    ILoadingStrategy GetStrategy(string strategyName);
    ILoadingStrategy GetDefaultStrategy();
}
```

**Implementations:**

1. **InsertOnlyStrategy**: Insert new records, skip existing
   - Use case: Initial seeding, append-only data
   - Performance: Fastest (bulk insert)
   - Idempotency: Detects duplicates by natural key

2. **UpsertStrategy**: Insert new, update existing
   - Use case: Regular updates, changing reference data
   - Performance: Moderate (detect + merge)
   - Idempotency: Full upsert semantics

3. **MergeStrategy**: Insert, update, optionally delete
   - Use case: Complete synchronization
   - Performance: Slowest (full comparison)
   - Idempotency: Maintains exact match with source

4. **StreamingStrategy**: Process large data sets incrementally
   - Use case: Very large data sets (> 1M records)
   - Performance: Memory-efficient
   - Idempotency: Batched operations

---

### 4. IDependencyResolver

**Responsibility:** Resolve and order data set dependencies

```csharp
public interface IDependencyResolver
{
    Task<IReadOnlyList<string>> ResolveAsync(
        string dataSetName,
        CancellationToken ct = default);

    Task<IReadOnlyList<string>> ResolveAsync(
        IEnumerable<string> dataSetNames,
        CancellationToken ct = default);

    Task<bool> HasCircularDependenciesAsync(string dataSetName);
}

public class DependencyResolver : IDependencyResolver
{
    private readonly IMasterDataMetadataRepository _metadataRepository;
    private readonly ILogger<DependencyResolver> _logger;

    public async Task<IReadOnlyList<string>> ResolveAsync(
        string dataSetName,
        CancellationToken ct = default)
    {
        var metadata = await _metadataRepository.GetAsync(dataSetName);
        var graph = await BuildDependencyGraphAsync(metadata.Dependencies, ct);
        return TopologicalSort(graph);
    }
}
```

**Algorithm:**
1. Build dependency graph from metadata
2. Detect circular dependencies
3. Topologically sort nodes
4. Return load order

---

### 5. IVersionManager

**Responsibility:** Manage data set versions and migrations

```csharp
public interface IVersionManager
{
    Task<VersionInfo> GetCurrentVersionAsync(string dataSetName);

    Task<IEnumerable<VersionInfo>> GetAvailableVersionsAsync(string dataSetName);

    Task ApplyVersionAsync(
        string dataSetName,
        string version,
        CancellationToken ct = default);

    Task<MigrationPlan> PlanMigrationAsync(
        string dataSetName,
        string fromVersion,
        string toVersion);

    Task ExecuteMigrationAsync(
        MigrationPlan plan,
        CancellationToken ct = default);
}

public class VersionInfo
{
    public string Number { get; set; } = string.Empty;
    public DateTime AppliedDate { get; set; }
    public string AppliedBy { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int RecordsAffected { get; set; }
}
```

**Versioning Strategy:**
- Semantic versioning (major.minor.patch)
- Version metadata stored in database
- Migration scripts for version transitions
- Rollback capability

---

### 6. IChangeTracker

**Responsibility:** Track and audit data changes

```csharp
public interface IChangeTracker
{
    Task TrackChangeAsync(
        ChangeRecord change,
        CancellationToken ct = default);

    Task<IEnumerable<ChangeRecord>> GetChangesAsync(
        string dataSetName,
        DateTime? from = null,
        DateTime? to = null);

    Task<IEnumerable<ChangeRecord>> GetEntityChangesAsync(
        string dataSetName,
        object entityKey);
}

public class ChangeRecord
{
    public Guid Id { get; set; }
    public string DataSetName { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public object EntityKey { get; set; } = null!;
    public ChangeOperation Operation { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public DateTime Timestamp { get; set; }
    public string User { get; set; } = string.Empty;
}

public enum ChangeOperation
{
    Insert,
    Update,
    Delete
}
```

---

## Data Flow

### Loading Sequence Diagram

```
┌─────────┐          ┌──────────────┐          ┌────────────────┐
│ Client  │          │ MasterData   │          │  Dependency    │
│         │          │ Loader       │          │  Resolver      │
└────┬────┘          └──────┬───────┘          └────────┬───────┘
     │                      │                           │
     │ LoadAsync("States")  │                           │
     ├─────────────────────>│                           │
     │                      │                           │
     │                      │ ResolveAsync("States")    │
     │                      ├──────────────────────────>│
     │                      │                           │
     │                      │  ["Countries", "States"]  │
     │                      │<──────────────────────────┤
     │                      │                           │
     │                      ▼                           │
     │           ┌──────────────────┐                  │
     │           │ Load "Countries" │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │                    ▼                            │
     │           ┌──────────────────┐                  │
     │           │  DataSource      │                  │
     │           │  Provider        │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │                    │ LoadAsync("countries.json")│
     │                    ▼                            │
     │           ┌──────────────────┐                  │
     │           │   DataSet        │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │                    ▼                            │
     │           ┌──────────────────┐                  │
     │           │  Loading         │                  │
     │           │  Strategy        │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │                    │ UpsertAsync()              │
     │                    ▼                            │
     │           ┌──────────────────┐                  │
     │           │   Database       │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │                    ▼                            │
     │           ┌──────────────────┐                  │
     │           │  Change Tracker  │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │                    ▼                            │
     │           ┌──────────────────┐                  │
     │           │ Load "States"    │                  │
     │           │ (repeat)         │                  │
     │           └────────┬─────────┘                  │
     │                    │                            │
     │   LoadResult       │                            │
     │<───────────────────┤                            │
     │                    │                            │
```

---

## Entity Framework Integration

### DbContext Integration

```csharp
public static class DbContextExtensions
{
    public static async Task LoadMasterDataAsync(
        this DbContext context,
        string dataSetName,
        CancellationToken ct = default)
    {
        var loader = context.GetService<IMasterDataLoader>();
        await loader.LoadAsync(dataSetName, ct);
    }

    public static async Task LoadMasterDataAsync(
        this DbContext context,
        params string[] dataSetNames)
    {
        var loader = context.GetService<IMasterDataLoader>();
        await loader.LoadBatchAsync(dataSetNames);
    }
}
```

### Migration Seeding

```csharp
public class SeedMasterData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Schema changes first
        migrationBuilder.CreateTable(name: "Countries", /* ... */);

        // Then seed data via extension point
        migrationBuilder.SeedMasterData("Countries", "States", "Cities");
    }
}

public static class MigrationBuilderExtensions
{
    public static void SeedMasterData(
        this MigrationBuilder builder,
        params string[] dataSetNames)
    {
        builder.Operations.Add(
            new MasterDataLoadOperation(dataSetNames));
    }
}
```

### OnModelCreating Seeding

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);

    // Use master data loader for seeding
    modelBuilder.SeedFromMasterData("Countries");
    modelBuilder.SeedFromMasterData("States");
}

public static class ModelBuilderExtensions
{
    public static ModelBuilder SeedFromMasterData(
        this ModelBuilder builder,
        string dataSetName)
    {
        var loader = builder.GetInfrastructure()
            .GetService<IMasterDataLoader>();

        // Load data synchronously during model creation
        var dataSet = loader.LoadAsync(dataSetName).GetAwaiter().GetResult();

        // Convert to HasData calls
        var entityType = builder.Model.FindEntityType(dataSet.EntityType);
        builder.Entity(entityType.ClrType).HasData(dataSet.Entities);

        return builder;
    }
}
```

---

## Configuration

### Options Pattern

```csharp
public class MasterDataLoaderOptions
{
    public string DefaultStrategy { get; set; } = "Upsert";
    public int BatchSize { get; set; } = 1000;
    public bool EnableChangeTracking { get; set; } = true;
    public bool EnableVersioning { get; set; } = true;
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(5);

    public Dictionary<string, DataSetConfiguration> DataSets { get; set; } = new();
}

public class DataSetConfiguration
{
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0.0";
    public string SourceType { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<string> Dependencies { get; set; } = new();
    public string? LoadingStrategy { get; set; }
    public Dictionary<string, object> SourceOptions { get; set; } = new();
}
```

### Configuration File

```json
{
  "MasterDataLoader": {
    "DefaultStrategy": "Upsert",
    "BatchSize": 1000,
    "EnableChangeTracking": true,
    "EnableVersioning": true,
    "DataSets": {
      "Countries": {
        "Name": "Countries",
        "Version": "1.0.0",
        "SourceType": "json",
        "Source": "Data/countries.json",
        "Dependencies": [],
        "LoadingStrategy": "Upsert"
      },
      "States": {
        "Name": "States",
        "Version": "1.0.0",
        "SourceType": "csv",
        "Source": "Data/states.csv",
        "Dependencies": ["Countries"],
        "SourceOptions": {
          "Delimiter": ",",
          "HasHeader": true,
          "Encoding": "utf-8"
        }
      }
    }
  }
}
```

---

## Dependency Injection

### Service Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMasterDataLoader(
        this IServiceCollection services,
        Action<MasterDataLoaderOptions>? configure = null)
    {
        // Core services
        services.TryAddSingleton<IMasterDataLoader, MasterDataLoader>();
        services.TryAddSingleton<IDependencyResolver, DependencyResolver>();
        services.TryAddSingleton<IVersionManager, VersionManager>();
        services.TryAddSingleton<IChangeTracker, ChangeTracker>();

        // Factories
        services.TryAddSingleton<IDataSourceProviderFactory, DataSourceProviderFactory>();
        services.TryAddSingleton<ILoadingStrategyFactory, LoadingStrategyFactory>();

        // Data source providers
        services.TryAddSingleton<IDataSourceProvider, JsonDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, CsvDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, XmlDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, SqlDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, ApiDataSourceProvider>();

        // Loading strategies
        services.TryAddSingleton<ILoadingStrategy, InsertOnlyStrategy>();
        services.TryAddSingleton<ILoadingStrategy, UpsertStrategy>();
        services.TryAddSingleton<ILoadingStrategy, MergeStrategy>();
        services.TryAddSingleton<ILoadingStrategy, StreamingStrategy>();

        // Configuration
        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }

    public static IServiceCollection AddMasterDataLoaderEntityFramework(
        this IServiceCollection services)
    {
        services.AddMasterDataLoader();
        services.TryAddScoped<IMasterDataLoader, EfCoreMasterDataLoader>();
        return services;
    }
}
```

---

## Error Handling

### Exception Hierarchy

```csharp
public class MasterDataException : Exception
{
    public string? DataSetName { get; set; }
}

public class DataSourceException : MasterDataException
{
    public string? Source { get; set; }
}

public class DependencyResolutionException : MasterDataException
{
    public IReadOnlyList<string> CircularDependencies { get; set; } = Array.Empty<string>();
}

public class ValidationException : MasterDataException
{
    public IReadOnlyList<ValidationError> Errors { get; set; } = Array.Empty<ValidationError>();
}

public class VersionException : MasterDataException
{
    public string? CurrentVersion { get; set; }
    public string? RequestedVersion { get; set; }
}
```

### Error Recovery

```csharp
public class MasterDataLoader : IMasterDataLoader
{
    public async Task LoadAsync(
        string dataSetName,
        LoadOptions options,
        CancellationToken ct = default)
    {
        using var transaction = await BeginTransactionAsync(ct);
        try
        {
            // Resolve dependencies
            var loadOrder = await _dependencyResolver.ResolveAsync(dataSetName, ct);

            // Load each data set
            foreach (var dataSet in loadOrder)
            {
                await LoadSingleAsync(dataSet, options, ct);
            }

            // Commit transaction
            await transaction.CommitAsync(ct);
        }
        catch (Exception ex)
        {
            // Rollback transaction
            await transaction.RollbackAsync(ct);

            _logger.LogError(ex, "Failed to load data set {DataSetName}", dataSetName);
            throw new MasterDataException($"Failed to load data set '{dataSetName}'", ex)
            {
                DataSetName = dataSetName
            };
        }
    }
}
```

---

## Performance Optimization

### Batching Strategy

```csharp
public class UpsertStrategy : ILoadingStrategy
{
    private readonly int _batchSize;

    public async Task<LoadResult> LoadAsync(
        DataSet dataSet,
        DataSetMetadata metadata,
        CancellationToken ct = default)
    {
        var result = new LoadResult();
        var batch = new List<DataRow>(_batchSize);

        await foreach (var row in dataSet.StreamRowsAsync(ct))
        {
            batch.Add(row);

            if (batch.Count >= _batchSize)
            {
                await UpsertBatchAsync(batch, metadata, ct);
                result.RecordsProcessed += batch.Count;
                batch.Clear();
            }
        }

        // Process remaining
        if (batch.Count > 0)
        {
            await UpsertBatchAsync(batch, metadata, ct);
            result.RecordsProcessed += batch.Count;
        }

        return result;
    }
}
```

### Parallel Loading

```csharp
public async Task<IReadOnlyList<LoadResult>> LoadBatchAsync(
    IEnumerable<string> dataSetNames,
    CancellationToken ct = default)
{
    // Resolve dependencies for all data sets
    var loadOrder = await _dependencyResolver.ResolveAsync(dataSetNames, ct);

    // Group by dependency level
    var levels = GroupByDependencyLevel(loadOrder);

    var results = new List<LoadResult>();

    // Load each level in parallel
    foreach (var level in levels)
    {
        var levelResults = await Task.WhenAll(
            level.Select(dataSet => LoadAsync(dataSet, ct)));

        results.AddRange(levelResults);
    }

    return results;
}
```

### Caching

```csharp
public class CachedMetadataRepository : IMasterDataMetadataRepository
{
    private readonly IMasterDataMetadataRepository _inner;
    private readonly IMemoryCache _cache;

    public async Task<DataSetMetadata> GetAsync(string dataSetName)
    {
        return await _cache.GetOrCreateAsync(
            $"metadata:{dataSetName}",
            async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromHours(1);
                return await _inner.GetAsync(dataSetName);
            });
    }
}
```

---

## Security Considerations

### Authentication and Authorization

```csharp
public interface IMasterDataAuthorizationService
{
    Task<bool> CanLoadAsync(string dataSetName, ClaimsPrincipal user);
    Task<bool> CanModifyAsync(string dataSetName, ClaimsPrincipal user);
}

public class MasterDataLoader : IMasterDataLoader
{
    private readonly IMasterDataAuthorizationService _authService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public async Task LoadAsync(
        string dataSetName,
        CancellationToken ct = default)
    {
        var user = _httpContextAccessor.HttpContext?.User;
        if (user != null && !await _authService.CanLoadAsync(dataSetName, user))
        {
            throw new UnauthorizedAccessException(
                $"User not authorized to load data set '{dataSetName}'");
        }

        // Continue with loading...
    }
}
```

### Sensitive Data Handling

```csharp
public class SecureDataSourceProvider : IDataSourceProvider
{
    private readonly IDataProtectionProvider _dataProtection;

    public async Task<DataSet> LoadAsync(
        string source,
        DataSourceOptions options,
        CancellationToken ct = default)
    {
        var dataSet = await LoadInternalAsync(source, options, ct);

        // Encrypt sensitive fields
        if (options.SensitiveFields?.Any() == true)
        {
            var protector = _dataProtection.CreateProtector("MasterData");

            foreach (var row in dataSet.Rows)
            {
                foreach (var field in options.SensitiveFields)
                {
                    if (row.TryGetValue(field, out var value) && value != null)
                    {
                        row[field] = protector.Protect(value.ToString()!);
                    }
                }
            }
        }

        return dataSet;
    }
}
```

---

## Observability

### Logging

```csharp
public partial class MasterDataLoader
{
    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Information,
        Message = "Loading data set {DataSetName} from {Source} using {Strategy} strategy")]
    private partial void LogLoadingStarted(
        string dataSetName,
        string source,
        string strategy);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Information,
        Message = "Loaded {RecordCount} records for {DataSetName} in {Duration}ms")]
    private partial void LogLoadingCompleted(
        string dataSetName,
        int recordCount,
        long duration);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Error,
        Message = "Failed to load data set {DataSetName}: {Error}")]
    private partial void LogLoadingFailed(
        string dataSetName,
        string error,
        Exception ex);
}
```

### Metrics

```csharp
public class MasterDataLoaderMetrics
{
    private readonly IMeterFactory _meterFactory;
    private readonly Meter _meter;

    private readonly Counter<long> _recordsLoaded;
    private readonly Histogram<double> _loadDuration;
    private readonly Counter<long> _loadFailures;

    public MasterDataLoaderMetrics(IMeterFactory meterFactory)
    {
        _meterFactory = meterFactory;
        _meter = _meterFactory.Create("OoBDev.Framework.Data.MasterData");

        _recordsLoaded = _meter.CreateCounter<long>(
            "master_data.records_loaded",
            description: "Number of records loaded");

        _loadDuration = _meter.CreateHistogram<double>(
            "master_data.load_duration",
            unit: "ms",
            description: "Duration of load operations");

        _loadFailures = _meter.CreateCounter<long>(
            "master_data.load_failures",
            description: "Number of failed load operations");
    }

    public void RecordLoad(string dataSetName, int recordCount, TimeSpan duration)
    {
        var tags = new TagList { { "data_set", dataSetName } };
        _recordsLoaded.Add(recordCount, tags);
        _loadDuration.Record(duration.TotalMilliseconds, tags);
    }
}
```

---

## Testing Strategy

### Unit Testing

```csharp
[TestClass]
public class DependencyResolverTests
{
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

        // Act & Assert
        await Assert.ThrowsExceptionAsync<DependencyResolutionException>(
            () => resolver.ResolveAsync("A"));
    }
}
```

### Integration Testing

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class MasterDataLoaderIntegrationTests
{
    private ServiceProvider _services = null!;
    private IMasterDataLoader _loader = null!;

    [TestInitialize]
    public void Initialize()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<TestDbContext>(options =>
            options.UseSqlServer(
                TestContext.GetRequiredProperty<string>("SQL_CONNECTION_STRING")));
        services.AddMasterDataLoaderEntityFramework();

        _services = services.BuildServiceProvider();
        _loader = _services.GetRequiredService<IMasterDataLoader>();
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
    }
}
```

---

## Migration Strategy

### Phase 1: Core Infrastructure
1. Implement core interfaces and models
2. Implement DependencyResolver
3. Implement basic MasterDataLoader
4. Add JSON and CSV providers
5. Implement InsertOnlyStrategy and UpsertStrategy

### Phase 2: Advanced Features
1. Implement VersionManager
2. Implement ChangeTracker
3. Add XML, SQL, and API providers
4. Implement MergeStrategy and StreamingStrategy
5. Add Entity Framework integration

### Phase 3: Production Features
1. Add authentication and authorization
2. Implement metrics and observability
3. Add performance optimizations
4. Create migration tooling
5. Comprehensive documentation

---

## Future Enhancements

1. **Real-time Synchronization**: Watch files/APIs for changes
2. **Distributed Locking**: Coordinate loads across multiple instances
3. **Schema Evolution**: Automatic schema migration support
4. **Data Quality**: Built-in data quality checks and reporting
5. **GraphQL Support**: Load from GraphQL endpoints
6. **Excel Support**: Native Excel file reading
7. **Compression**: Support compressed data sources
8. **Encryption**: Encrypt data at rest
9. **Multi-tenancy**: Tenant-specific data sets
10. **Cloud Storage**: Azure Blob, AWS S3, GCS integration

---

## References

- Epic 05: Master Data & Test Data Management
- Feature: Data Source Providers
- Feature: Test Data Loader
- Entity Framework Core Data Seeding: https://learn.microsoft.com/ef/core/modeling/data-seeding
- Topological Sort Algorithm: https://en.wikipedia.org/wiki/Topological_sorting
