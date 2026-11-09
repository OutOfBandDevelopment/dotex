# Master Data Loader - API Design

**Feature:** Master Data Loader
**Epic:** 05 - Master Data & Test Data Management
**Status:** Proposed
**Last Updated:** 2026-01-22

---

## Overview

This document defines the complete public API for the Master Data Loader, including interfaces, classes, configuration options, and extension methods. All examples demonstrate real-world usage patterns.

---

## Core Interfaces

### IMasterDataLoader

Primary interface for loading and managing master data.

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions;

/// <summary>
/// Provides functionality for loading and managing master data from various sources.
/// </summary>
public interface IMasterDataLoader
{
    /// <summary>
    /// Loads a data set by name using default options.
    /// </summary>
    /// <param name="dataSetName">The name of the data set to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentNullException">Thrown when dataSetName is null.</exception>
    /// <exception cref="MasterDataException">Thrown when loading fails.</exception>
    Task LoadAsync(string dataSetName, CancellationToken ct = default);

    /// <summary>
    /// Loads a data set by name with custom options.
    /// </summary>
    /// <param name="dataSetName">The name of the data set to load.</param>
    /// <param name="options">Loading options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task LoadAsync(string dataSetName, LoadOptions options, CancellationToken ct = default);

    /// <summary>
    /// Loads multiple data sets in dependency order.
    /// </summary>
    /// <param name="dataSetNames">The names of the data sets to load.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A collection of load results for each data set.</returns>
    Task<IReadOnlyList<LoadResult>> LoadBatchAsync(
        IEnumerable<string> dataSetNames,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the current status of a data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <returns>The current load status.</returns>
    Task<LoadStatus> GetStatusAsync(string dataSetName);

    /// <summary>
    /// Gets all available data sets that can be loaded.
    /// </summary>
    /// <returns>A collection of data set names.</returns>
    Task<IEnumerable<string>> GetAvailableDataSetsAsync();

    /// <summary>
    /// Gets all data sets that have been loaded.
    /// </summary>
    /// <returns>A collection of loaded data set names.</returns>
    Task<IEnumerable<string>> GetLoadedDataSetsAsync();

    /// <summary>
    /// Validates a data set without loading it.
    /// </summary>
    /// <param name="dataSetName">The name of the data set to validate.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Validation result.</returns>
    Task<ValidationResult> ValidateAsync(string dataSetName, CancellationToken ct = default);

    /// <summary>
    /// Gets the current version of a data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <returns>Version information.</returns>
    Task<VersionInfo> GetVersionAsync(string dataSetName);

    /// <summary>
    /// Migrates a data set to a specific version.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="targetVersion">The target version (e.g., "1.2.0").</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task MigrateToVersionAsync(
        string dataSetName,
        string targetVersion,
        CancellationToken ct = default);

    /// <summary>
    /// Gets the change history for a data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="from">Start date for change history (optional).</param>
    /// <param name="to">End date for change history (optional).</param>
    /// <returns>A collection of change records.</returns>
    Task<IEnumerable<ChangeRecord>> GetChangeHistoryAsync(
        string dataSetName,
        DateTime? from = null,
        DateTime? to = null);
}
```

---

### IDataSourceProvider

Interface for reading data from external sources.

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions;

/// <summary>
/// Provides functionality for loading data from external sources.
/// </summary>
public interface IDataSourceProvider
{
    /// <summary>
    /// Gets the type of source this provider handles (e.g., "json", "csv", "xml").
    /// </summary>
    string SourceType { get; }

    /// <summary>
    /// Determines if this provider can handle the specified source.
    /// </summary>
    /// <param name="source">The source identifier (path, URL, connection string, etc.).</param>
    /// <returns>True if this provider can handle the source; otherwise, false.</returns>
    bool CanHandle(string source);

    /// <summary>
    /// Loads data from the specified source.
    /// </summary>
    /// <param name="source">The source identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The loaded data set.</returns>
    Task<DataSet> LoadAsync(string source, CancellationToken ct = default);

    /// <summary>
    /// Loads data from the specified source with custom options.
    /// </summary>
    /// <param name="source">The source identifier.</param>
    /// <param name="options">Source-specific options.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The loaded data set.</returns>
    Task<DataSet> LoadAsync(
        string source,
        DataSourceOptions options,
        CancellationToken ct = default);

    /// <summary>
    /// Streams data from the specified source for memory-efficient processing.
    /// </summary>
    /// <param name="source">The source identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An async enumerable of data rows.</returns>
    IAsyncEnumerable<DataRow> StreamAsync(string source, CancellationToken ct = default);
}
```

---

### ILoadingStrategy

Interface for data persistence strategies.

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions;

/// <summary>
/// Defines a strategy for loading data into the target store.
/// </summary>
public interface ILoadingStrategy
{
    /// <summary>
    /// Gets the name of this loading strategy.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Loads data using this strategy.
    /// </summary>
    /// <param name="dataSet">The data set to load.</param>
    /// <param name="metadata">Metadata about the data set.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The result of the load operation.</returns>
    Task<LoadResult> LoadAsync(
        DataSet dataSet,
        DataSetMetadata metadata,
        CancellationToken ct = default);
}
```

---

### IDependencyResolver

Interface for resolving data set dependencies.

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions;

/// <summary>
/// Resolves dependencies between data sets and determines load order.
/// </summary>
public interface IDependencyResolver
{
    /// <summary>
    /// Resolves the dependencies for a single data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ordered list of data sets to load (dependencies first).</returns>
    /// <exception cref="DependencyResolutionException">Thrown when circular dependencies are detected.</exception>
    Task<IReadOnlyList<string>> ResolveAsync(
        string dataSetName,
        CancellationToken ct = default);

    /// <summary>
    /// Resolves the dependencies for multiple data sets.
    /// </summary>
    /// <param name="dataSetNames">The names of the data sets.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>An ordered list of data sets to load (dependencies first).</returns>
    Task<IReadOnlyList<string>> ResolveAsync(
        IEnumerable<string> dataSetNames,
        CancellationToken ct = default);

    /// <summary>
    /// Checks if a data set has circular dependencies.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <returns>True if circular dependencies exist; otherwise, false.</returns>
    Task<bool> HasCircularDependenciesAsync(string dataSetName);
}
```

---

### IVersionManager

Interface for managing data set versions.

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions;

/// <summary>
/// Manages versioning of master data sets.
/// </summary>
public interface IVersionManager
{
    /// <summary>
    /// Gets the current version of a data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <returns>Version information.</returns>
    Task<VersionInfo> GetCurrentVersionAsync(string dataSetName);

    /// <summary>
    /// Gets all available versions of a data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <returns>A collection of available versions.</returns>
    Task<IEnumerable<VersionInfo>> GetAvailableVersionsAsync(string dataSetName);

    /// <summary>
    /// Applies a specific version to a data set.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="version">The version to apply.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ApplyVersionAsync(
        string dataSetName,
        string version,
        CancellationToken ct = default);

    /// <summary>
    /// Plans a migration from one version to another.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="fromVersion">The current version.</param>
    /// <param name="toVersion">The target version.</param>
    /// <returns>A migration plan.</returns>
    Task<MigrationPlan> PlanMigrationAsync(
        string dataSetName,
        string fromVersion,
        string toVersion);

    /// <summary>
    /// Executes a migration plan.
    /// </summary>
    /// <param name="plan">The migration plan to execute.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task ExecuteMigrationAsync(MigrationPlan plan, CancellationToken ct = default);
}
```

---

### IChangeTracker

Interface for tracking data changes.

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions;

/// <summary>
/// Tracks changes to master data for auditing purposes.
/// </summary>
public interface IChangeTracker
{
    /// <summary>
    /// Records a change to master data.
    /// </summary>
    /// <param name="change">The change record to track.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task TrackChangeAsync(ChangeRecord change, CancellationToken ct = default);

    /// <summary>
    /// Gets changes for a data set within a date range.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="from">Start date (optional).</param>
    /// <param name="to">End date (optional).</param>
    /// <returns>A collection of change records.</returns>
    Task<IEnumerable<ChangeRecord>> GetChangesAsync(
        string dataSetName,
        DateTime? from = null,
        DateTime? to = null);

    /// <summary>
    /// Gets changes for a specific entity.
    /// </summary>
    /// <param name="dataSetName">The name of the data set.</param>
    /// <param name="entityKey">The entity's natural key.</param>
    /// <returns>A collection of change records for the entity.</returns>
    Task<IEnumerable<ChangeRecord>> GetEntityChangesAsync(
        string dataSetName,
        object entityKey);
}
```

---

## Model Classes

### LoadOptions

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Options for loading master data.
/// </summary>
public class LoadOptions
{
    /// <summary>
    /// Gets or sets the loading strategy to use. If null, uses the default strategy.
    /// </summary>
    public string? Strategy { get; set; }

    /// <summary>
    /// Gets or sets whether to skip dependency resolution.
    /// </summary>
    public bool SkipDependencies { get; set; }

    /// <summary>
    /// Gets or sets whether to validate data before loading.
    /// </summary>
    public bool ValidateBeforeLoad { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to track changes during loading.
    /// </summary>
    public bool TrackChanges { get; set; } = true;

    /// <summary>
    /// Gets or sets the batch size for bulk operations.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets the timeout for the operation.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    /// Gets or sets custom metadata to associate with this load operation.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

---

### LoadResult

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Result of a data set load operation.
/// </summary>
public class LoadResult
{
    /// <summary>
    /// Gets or sets the name of the data set.
    /// </summary>
    public string DataSetName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status of the load operation.
    /// </summary>
    public LoadStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the number of records processed.
    /// </summary>
    public int RecordsProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of records inserted.
    /// </summary>
    public int RecordsInserted { get; set; }

    /// <summary>
    /// Gets or sets the number of records updated.
    /// </summary>
    public int RecordsUpdated { get; set; }

    /// <summary>
    /// Gets or sets the number of records deleted.
    /// </summary>
    public int RecordsDeleted { get; set; }

    /// <summary>
    /// Gets or sets the number of records skipped (unchanged).
    /// </summary>
    public int RecordsSkipped { get; set; }

    /// <summary>
    /// Gets or sets the duration of the load operation.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the load started.
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the load completed.
    /// </summary>
    public DateTime? EndTime { get; set; }

    /// <summary>
    /// Gets or sets any errors that occurred during loading.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Gets or sets the dependencies that were resolved.
    /// </summary>
    public List<string> DependenciesResolved { get; set; } = new();
}
```

---

### LoadStatus

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Status of a data set load operation.
/// </summary>
public enum LoadStatus
{
    /// <summary>
    /// Data set has not been loaded yet.
    /// </summary>
    NotLoaded,

    /// <summary>
    /// Load operation is currently in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Load operation completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Load operation failed.
    /// </summary>
    Failed,

    /// <summary>
    /// Load operation was cancelled.
    /// </summary>
    Cancelled
}
```

---

### DataSet

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Represents a collection of data rows from a source.
/// </summary>
public class DataSet
{
    /// <summary>
    /// Gets or sets the name of the data set.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type name.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the data rows.
    /// </summary>
    public List<DataRow> Rows { get; set; } = new();

    /// <summary>
    /// Gets or sets metadata about the data set.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();

    /// <summary>
    /// Creates a new data set with the specified rows.
    /// </summary>
    public DataSet(IEnumerable<DataRow> rows)
    {
        Rows = new List<DataRow>(rows);
    }

    /// <summary>
    /// Creates an empty data set.
    /// </summary>
    public DataSet()
    {
    }

    /// <summary>
    /// Streams the rows asynchronously.
    /// </summary>
    public async IAsyncEnumerable<DataRow> StreamRowsAsync(
        [EnumeratorCancellation] CancellationToken ct = default)
    {
        foreach (var row in Rows)
        {
            ct.ThrowIfCancellationRequested();
            yield return row;
            await Task.Yield(); // Allow interleaving
        }
    }
}
```

---

### DataRow

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Represents a single row of data.
/// </summary>
public class DataRow : Dictionary<string, object?>
{
    /// <summary>
    /// Gets a strongly-typed value from the row.
    /// </summary>
    public T? GetValue<T>(string key)
    {
        if (TryGetValue(key, out var value) && value != null)
        {
            if (value is T typedValue)
                return typedValue;

            return (T)Convert.ChangeType(value, typeof(T));
        }

        return default;
    }

    /// <summary>
    /// Gets a required value from the row.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the key is not found.</exception>
    /// <exception cref="InvalidCastException">Thrown when the value cannot be converted.</exception>
    public T GetRequiredValue<T>(string key)
    {
        if (!TryGetValue(key, out var value))
            throw new KeyNotFoundException($"Key '{key}' not found in data row");

        if (value == null)
            throw new InvalidOperationException($"Value for key '{key}' is null");

        if (value is T typedValue)
            return typedValue;

        return (T)Convert.ChangeType(value, typeof(T));
    }
}
```

---

### DataSetMetadata

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Metadata about a data set configuration.
/// </summary>
public class DataSetMetadata
{
    /// <summary>
    /// Gets or sets the name of the data set.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version of the data set.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the source type (json, csv, xml, sql, api).
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source location.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type that this data set populates.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the names of data sets that must be loaded first.
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Gets or sets the loading strategy to use (InsertOnly, Upsert, Merge, Streaming).
    /// </summary>
    public string? LoadingStrategy { get; set; }

    /// <summary>
    /// Gets or sets the natural key fields used for idempotency.
    /// </summary>
    public List<string> NaturalKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets source-specific options.
    /// </summary>
    public Dictionary<string, object> SourceOptions { get; set; } = new();

    /// <summary>
    /// Gets or sets validation rules.
    /// </summary>
    public ValidationRules? ValidationRules { get; set; }
}
```

---

### VersionInfo

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Information about a data set version.
/// </summary>
public class VersionInfo
{
    /// <summary>
    /// Gets or sets the version number (e.g., "1.2.0").
    /// </summary>
    public string Number { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date this version was applied.
    /// </summary>
    public DateTime AppliedDate { get; set; }

    /// <summary>
    /// Gets or sets who applied this version.
    /// </summary>
    public string AppliedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a description of the changes in this version.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of records affected by this version.
    /// </summary>
    public int RecordsAffected { get; set; }

    /// <summary>
    /// Gets or sets additional metadata.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
```

---

### ChangeRecord

```csharp
namespace OoBDev.Framework.Data.MasterData.Abstractions.Models;

/// <summary>
/// Records a change to master data.
/// </summary>
public class ChangeRecord
{
    /// <summary>
    /// Gets or sets the unique identifier for this change.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the name of the data set.
    /// </summary>
    public string DataSetName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity type.
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the entity's natural key.
    /// </summary>
    public object EntityKey { get; set; } = null!;

    /// <summary>
    /// Gets or sets the type of operation performed.
    /// </summary>
    public ChangeOperation Operation { get; set; }

    /// <summary>
    /// Gets or sets the old values (JSON).
    /// </summary>
    public string? OldValues { get; set; }

    /// <summary>
    /// Gets or sets the new values (JSON).
    /// </summary>
    public string? NewValues { get; set; }

    /// <summary>
    /// Gets or sets when the change occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets who made the change.
    /// </summary>
    public string User { get; set; } = string.Empty;
}

/// <summary>
/// Type of change operation.
/// </summary>
public enum ChangeOperation
{
    Insert,
    Update,
    Delete
}
```

---

## Configuration

### MasterDataLoaderOptions

```csharp
namespace OoBDev.Framework.Data.MasterData.Configuration;

/// <summary>
/// Configuration options for the master data loader.
/// </summary>
public class MasterDataLoaderOptions
{
    /// <summary>
    /// Gets or sets the default loading strategy.
    /// </summary>
    public string DefaultStrategy { get; set; } = "Upsert";

    /// <summary>
    /// Gets or sets the default batch size for bulk operations.
    /// </summary>
    public int BatchSize { get; set; } = 1000;

    /// <summary>
    /// Gets or sets whether to enable change tracking by default.
    /// </summary>
    public bool EnableChangeTracking { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to enable versioning by default.
    /// </summary>
    public bool EnableVersioning { get; set; } = true;

    /// <summary>
    /// Gets or sets the command timeout for operations.
    /// </summary>
    public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets or sets the data set configurations.
    /// </summary>
    public Dictionary<string, DataSetConfiguration> DataSets { get; set; } = new();
}

/// <summary>
/// Configuration for a single data set.
/// </summary>
public class DataSetConfiguration
{
    /// <summary>
    /// Gets or sets the name of the data set.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the version.
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Gets or sets the source type.
    /// </summary>
    public string SourceType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the source location.
    /// </summary>
    public string Source { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets dependencies.
    /// </summary>
    public List<string> Dependencies { get; set; } = new();

    /// <summary>
    /// Gets or sets the loading strategy.
    /// </summary>
    public string? LoadingStrategy { get; set; }

    /// <summary>
    /// Gets or sets source-specific options.
    /// </summary>
    public Dictionary<string, object> SourceOptions { get; set; } = new();
}
```

---

## Extension Methods

### ServiceCollectionExtensions

```csharp
namespace OoBDev.Framework.Data.MasterData.Extensions;

/// <summary>
/// Extension methods for registering master data loader services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers master data loader services with default configuration.
    /// </summary>
    public static IServiceCollection AddMasterDataLoader(
        this IServiceCollection services)
    {
        return services.AddMasterDataLoader(configure: null);
    }

    /// <summary>
    /// Registers master data loader services with custom configuration.
    /// </summary>
    public static IServiceCollection AddMasterDataLoader(
        this IServiceCollection services,
        Action<MasterDataLoaderOptions>? configure)
    {
        services.TryAddSingleton<IMasterDataLoader, MasterDataLoader>();
        services.TryAddSingleton<IDependencyResolver, DependencyResolver>();
        services.TryAddSingleton<IVersionManager, VersionManager>();
        services.TryAddSingleton<IChangeTracker, ChangeTracker>();

        // Factories
        services.TryAddSingleton<IDataSourceProviderFactory, DataSourceProviderFactory>();
        services.TryAddSingleton<ILoadingStrategyFactory, LoadingStrategyFactory>();

        // Providers
        services.TryAddSingleton<IDataSourceProvider, JsonDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, CsvDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, XmlDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, SqlDataSourceProvider>();
        services.TryAddSingleton<IDataSourceProvider, ApiDataSourceProvider>();

        // Strategies
        services.TryAddSingleton<ILoadingStrategy, InsertOnlyStrategy>();
        services.TryAddSingleton<ILoadingStrategy, UpsertStrategy>();
        services.TryAddSingleton<ILoadingStrategy, MergeStrategy>();
        services.TryAddSingleton<ILoadingStrategy, StreamingStrategy>();

        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Registers Entity Framework-specific master data loader services.
    /// </summary>
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

### DbContextExtensions

```csharp
namespace OoBDev.Framework.Data.MasterData.Extensions;

/// <summary>
/// Extension methods for DbContext integration.
/// </summary>
public static class DbContextExtensions
{
    /// <summary>
    /// Loads master data into the database.
    /// </summary>
    public static async Task LoadMasterDataAsync(
        this DbContext context,
        string dataSetName,
        CancellationToken ct = default)
    {
        var loader = context.GetService<IMasterDataLoader>();
        if (loader == null)
            throw new InvalidOperationException(
                "IMasterDataLoader not registered. Call AddMasterDataLoaderEntityFramework().");

        await loader.LoadAsync(dataSetName, ct);
    }

    /// <summary>
    /// Loads multiple master data sets into the database.
    /// </summary>
    public static async Task LoadMasterDataAsync(
        this DbContext context,
        params string[] dataSetNames)
    {
        var loader = context.GetService<IMasterDataLoader>();
        if (loader == null)
            throw new InvalidOperationException(
                "IMasterDataLoader not registered. Call AddMasterDataLoaderEntityFramework().");

        await loader.LoadBatchAsync(dataSetNames);
    }

    /// <summary>
    /// Validates master data without loading it.
    /// </summary>
    public static async Task<ValidationResult> ValidateMasterDataAsync(
        this DbContext context,
        string dataSetName,
        CancellationToken ct = default)
    {
        var loader = context.GetService<IMasterDataLoader>();
        if (loader == null)
            throw new InvalidOperationException(
                "IMasterDataLoader not registered. Call AddMasterDataLoaderEntityFramework().");

        return await loader.ValidateAsync(dataSetName, ct);
    }
}
```

---

## Usage Examples

### Example 1: Basic Data Set Loading

```csharp
using OoBDev.Framework.Data.MasterData.Abstractions;
using Microsoft.Extensions.DependencyInjection;

// Configure services
var services = new ServiceCollection();
services.AddLogging();
services.AddMasterDataLoader(options =>
{
    options.DataSets["Countries"] = new DataSetConfiguration
    {
        Name = "Countries",
        SourceType = "json",
        Source = "Data/countries.json",
        Version = "1.0.0"
    };
});

var provider = services.BuildServiceProvider();
var loader = provider.GetRequiredService<IMasterDataLoader>();

// Load countries data
await loader.LoadAsync("Countries");

// Check status
var status = await loader.GetStatusAsync("Countries");
Console.WriteLine($"Loaded {status.RecordsProcessed} countries");
```

---

### Example 2: Loading with Dependencies

```csharp
// Configure multiple data sets with dependencies
services.AddMasterDataLoader(options =>
{
    options.DataSets["Countries"] = new DataSetConfiguration
    {
        Name = "Countries",
        SourceType = "json",
        Source = "Data/countries.json",
        Dependencies = []
    };

    options.DataSets["States"] = new DataSetConfiguration
    {
        Name = "States",
        SourceType = "csv",
        Source = "Data/states.csv",
        Dependencies = ["Countries"],  // Must load Countries first
        SourceOptions = new Dictionary<string, object>
        {
            ["Delimiter"] = ",",
            ["HasHeader"] = true
        }
    };

    options.DataSets["Cities"] = new DataSetConfiguration
    {
        Name = "Cities",
        SourceType = "json",
        Source = "Data/cities.json",
        Dependencies = ["States"]  // Must load States first
    };
});

// Load States - automatically loads Countries first
await loader.LoadAsync("States");

// Or load all at once
await loader.LoadBatchAsync(new[] { "Countries", "States", "Cities" });
```

---

### Example 3: Custom Loading Strategy

```csharp
// Load with custom options
var options = new LoadOptions
{
    Strategy = "Merge",  // Use merge strategy instead of default upsert
    BatchSize = 5000,    // Larger batch size
    ValidateBeforeLoad = true,
    TrackChanges = true
};

await loader.LoadAsync("Products", options);

// Check what changed
var changes = await loader.GetChangeHistoryAsync(
    "Products",
    from: DateTime.UtcNow.AddHours(-1));

foreach (var change in changes)
{
    Console.WriteLine($"{change.Operation}: {change.EntityKey}");
}
```

---

### Example 4: Entity Framework Integration

```csharp
using Microsoft.EntityFrameworkCore;
using OoBDev.Framework.Data.MasterData.Extensions;

public class ApplicationDbContext : DbContext
{
    public DbSet<Country> Countries { get; set; } = null!;
    public DbSet<State> States { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(connectionString);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Seed data during model creation
        modelBuilder.SeedFromMasterData("Countries");
        modelBuilder.SeedFromMasterData("States");
    }
}

// Or load at runtime
using var context = new ApplicationDbContext();
await context.Database.EnsureCreatedAsync();
await context.LoadMasterDataAsync("Countries", "States");
```

---

### Example 5: Version Migration

```csharp
// Check current version
var currentVersion = await loader.GetVersionAsync("Countries");
Console.WriteLine($"Current version: {currentVersion.Number}");

// Get available versions
var versions = await loader.GetAvailableVersionsAsync("Countries");
foreach (var version in versions)
{
    Console.WriteLine($"  {version.Number}: {version.Description}");
}

// Migrate to new version
await loader.MigrateToVersionAsync("Countries", "2.0.0");

// Verify migration
var newVersion = await loader.GetVersionAsync("Countries");
Console.WriteLine($"Migrated to version: {newVersion.Number}");
Console.WriteLine($"Records affected: {newVersion.RecordsAffected}");
```

---

### Example 6: Data Validation

```csharp
// Validate before loading
var validationResult = await loader.ValidateAsync("Countries");

if (!validationResult.IsValid)
{
    Console.WriteLine("Validation errors:");
    foreach (var error in validationResult.Errors)
    {
        Console.WriteLine($"  Row {error.RowNumber}: {error.Message}");
        Console.WriteLine($"    Field: {error.FieldName}");
        Console.WriteLine($"    Value: {error.Value}");
    }
    return;
}

// Safe to load
await loader.LoadAsync("Countries");
```

---

### Example 7: Background Service Loading

```csharp
using Microsoft.Extensions.Hosting;

public class MasterDataLoaderService : BackgroundService
{
    private readonly IMasterDataLoader _loader;
    private readonly ILogger<MasterDataLoaderService> _logger;

    public MasterDataLoaderService(
        IMasterDataLoader loader,
        ILogger<MasterDataLoaderService> logger)
    {
        _loader = loader;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Loading master data on startup...");

        try
        {
            // Load all reference data on startup
            var results = await _loader.LoadBatchAsync(
                new[] { "Countries", "States", "Cities", "Currencies" },
                stoppingToken);

            foreach (var result in results)
            {
                _logger.LogInformation(
                    "Loaded {DataSet}: {RecordsLoaded} records in {Duration}",
                    result.DataSetName,
                    result.RecordsProcessed,
                    result.Duration);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load master data");
            throw;
        }
    }
}

// Register the service
services.AddHostedService<MasterDataLoaderService>();
```

---

### Example 8: Multi-Source Configuration

```csharp
services.AddMasterDataLoader(options =>
{
    // JSON file source
    options.DataSets["Countries"] = new DataSetConfiguration
    {
        Name = "Countries",
        SourceType = "json",
        Source = "Data/countries.json"
    };

    // CSV file source
    options.DataSets["Products"] = new DataSetConfiguration
    {
        Name = "Products",
        SourceType = "csv",
        Source = "Data/products.csv",
        SourceOptions = new Dictionary<string, object>
        {
            ["Delimiter"] = "|",
            ["HasHeader"] = true,
            ["Encoding"] = "utf-8"
        }
    };

    // SQL query source
    options.DataSets["LegacyCustomers"] = new DataSetConfiguration
    {
        Name = "LegacyCustomers",
        SourceType = "sql",
        Source = "SELECT * FROM Customers WHERE Active = 1",
        SourceOptions = new Dictionary<string, object>
        {
            ["ConnectionString"] = "Server=legacy;Database=CRM;..."
        }
    };

    // REST API source
    options.DataSets["ExchangeRates"] = new DataSetConfiguration
    {
        Name = "ExchangeRates",
        SourceType = "api",
        Source = "https://api.example.com/v1/rates",
        SourceOptions = new Dictionary<string, object>
        {
            ["Method"] = "GET",
            ["Headers"] = new Dictionary<string, string>
            {
                ["Authorization"] = "Bearer {token}",
                ["Accept"] = "application/json"
            }
        }
    };
});
```

---

### Example 9: Change Tracking and Auditing

```csharp
// Load data with change tracking enabled
await loader.LoadAsync("Countries");

// Query change history
var changes = await loader.GetChangeHistoryAsync(
    "Countries",
    from: DateTime.UtcNow.AddDays(-7));

// Generate audit report
Console.WriteLine("Changes in the last 7 days:");
foreach (var change in changes.OrderBy(c => c.Timestamp))
{
    Console.WriteLine($"[{change.Timestamp:yyyy-MM-dd HH:mm:ss}] " +
                      $"{change.Operation} by {change.User}");
    Console.WriteLine($"  Entity: {change.EntityType} (Key: {change.EntityKey})");

    if (change.Operation == ChangeOperation.Update)
    {
        Console.WriteLine($"  Old: {change.OldValues}");
        Console.WriteLine($"  New: {change.NewValues}");
    }
}

// Get changes for specific entity
var entityChanges = await loader.GetEntityChangesAsync("Countries", "US");
Console.WriteLine($"\nHistory for Country 'US':");
foreach (var change in entityChanges)
{
    Console.WriteLine($"  {change.Timestamp}: {change.Operation}");
}
```

---

## Exception Handling

### Common Exceptions

```csharp
try
{
    await loader.LoadAsync("Countries");
}
catch (DataSourceException ex)
{
    // Problem reading from source
    _logger.LogError(ex, "Failed to read from source {Source}", ex.Source);
    // Retry with different source or alert administrators
}
catch (DependencyResolutionException ex)
{
    // Circular dependencies detected
    _logger.LogError(ex, "Circular dependencies: {Dependencies}",
        string.Join(" -> ", ex.CircularDependencies));
    // Fix configuration
}
catch (ValidationException ex)
{
    // Data failed validation
    _logger.LogError(ex, "Validation failed with {ErrorCount} errors", ex.Errors.Count);
    foreach (var error in ex.Errors)
    {
        _logger.LogError("  Row {Row}, Field {Field}: {Message}",
            error.RowNumber, error.FieldName, error.Message);
    }
    // Fix source data
}
catch (VersionException ex)
{
    // Version mismatch or migration error
    _logger.LogError(ex, "Version error: {Current} -> {Requested}",
        ex.CurrentVersion, ex.RequestedVersion);
    // Check migration scripts
}
catch (MasterDataException ex)
{
    // General master data error
    _logger.LogError(ex, "Failed to load data set {DataSet}", ex.DataSetName);
    // Generic error handling
}
```

---

## Best Practices

### 1. Configuration Organization

```csharp
// Store data set configurations in appsettings.json
{
  "MasterDataLoader": {
    "DefaultStrategy": "Upsert",
    "BatchSize": 1000,
    "DataSets": {
      "Countries": {
        "Name": "Countries",
        "SourceType": "json",
        "Source": "Data/countries.json",
        "Version": "1.0.0"
      }
    }
  }
}

// Load from configuration
services.AddMasterDataLoader(options =>
{
    configuration.GetSection("MasterDataLoader").Bind(options);
});
```

### 2. Dependency Management

```csharp
// Use explicit dependencies in configuration
options.DataSets["Cities"] = new DataSetConfiguration
{
    Dependencies = ["Countries", "States"],  // Clear dependencies
    // ...
};

// Validate dependencies before loading
var resolver = provider.GetRequiredService<IDependencyResolver>();
var hasCircular = await resolver.HasCircularDependenciesAsync("Cities");
if (hasCircular)
{
    throw new InvalidOperationException("Circular dependency detected!");
}
```

### 3. Idempotent Loading

```csharp
// Define natural keys for idempotency
options.DataSets["Countries"] = new DataSetConfiguration
{
    Name = "Countries",
    NaturalKeys = ["Code"],  // Use country code as natural key
    LoadingStrategy = "Upsert"  // Safe to run multiple times
};
```

### 4. Performance Optimization

```csharp
// Use streaming for large data sets
var streamingOptions = new LoadOptions
{
    Strategy = "Streaming",
    BatchSize = 10000  // Larger batches for bulk insert
};

await loader.LoadAsync("LargeDataSet", streamingOptions);
```

### 5. Error Recovery

```csharp
// Implement retry logic for transient failures
var retryPolicy = Policy
    .Handle<DataSourceException>()
    .WaitAndRetryAsync(3, retryAttempt =>
        TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

await retryPolicy.ExecuteAsync(() => loader.LoadAsync("Countries"));
```

---

## Performance Characteristics

| Operation | Time Complexity | Space Complexity | Notes |
|-----------|----------------|------------------|-------|
| LoadAsync (small) | O(n) | O(n) | n = record count, < 10K records |
| LoadAsync (large) | O(n) | O(1) | Streaming mode, > 100K records |
| ResolveAsync | O(v + e) | O(v) | v = data sets, e = dependencies |
| ValidateAsync | O(n) | O(1) | Streaming validation |
| GetChangeHistoryAsync | O(log n + k) | O(k) | k = matching changes, indexed query |
| GetVersionAsync | O(1) | O(1) | Cached metadata |

**Benchmarks (typical hardware):**
- 10,000 records: < 5 seconds
- 100,000 records: < 30 seconds
- 1,000,000 records: < 5 minutes (streaming)

---

## Thread Safety

All implementations are thread-safe for concurrent reads. Write operations (LoadAsync, MigrateToVersionAsync) use optimistic concurrency or pessimistic locking depending on the underlying data store.

```csharp
// Safe to call from multiple threads
var tasks = Enumerable.Range(0, 10)
    .Select(_ => loader.GetStatusAsync("Countries"))
    .ToArray();

await Task.WhenAll(tasks);

// Concurrent loads are serialized automatically
var loadTasks = new[]
{
    loader.LoadAsync("Countries"),
    loader.LoadAsync("States"),
    loader.LoadAsync("Cities")
};

await Task.WhenAll(loadTasks);  // Dependencies handled correctly
```

---

## References

- Epic 05: Master Data & Test Data Management
- Architecture: Master Data Loader Architecture
- Testing: Master Data Loader Testing Strategy
- Feature: Data Source Providers
- Feature: Test Data Loader
