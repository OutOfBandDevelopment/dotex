# Lazy Data Providers - Requirements

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Lazy Data Providers
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~300

---

## Overview

Extensible provider system that lazily loads data from various sources (databases, APIs, configuration, files) only when accessed through IDataContainer navigation. Enables efficient data access with pluggable provider implementations.

---

## Business Requirements

### BR-1: Pluggable Data Sources
**As a** developer
**I want** to plug in different data sources for my data container
**So that** data can be loaded from databases, APIs, config files, or any custom source

**Acceptance Criteria:**
- Support database providers (Entity Framework, Dapper, raw ADO.NET)
- Support API providers (REST, GraphQL)
- Support configuration providers (appsettings.json, environment variables)
- Support file providers (JSON, XML, CSV)
- Extensible to custom providers
- Provider registration by path prefix

**Example:**
```csharp
container.RegisterProvider("Customer/*", new DatabaseProvider(dbContext));
container.RegisterProvider("Config/*", new ConfigurationProvider(configuration));
container.RegisterProvider("Weather/*", new ApiProvider("https://api.weather.com"));
```

---

### BR-2: Lazy Loading
**As a** framework
**I want** to defer data loading until navigation
**So that** unnecessary queries are avoided and performance is improved

**Acceptance Criteria:**
- Data loaded only when `IDataNode.Value` or `GetValueAsync()` accessed
- Provider called once per path (cached result)
- No data loaded if path never accessed
- 50-70% reduction in query count for typical scenarios

**Example:**
```csharp
var container = CreateContainer();
container.RegisterProvider("Customer/Orders", new DatabaseProvider());

// No query executed yet
var node = container.Navigate("Customer/Orders");

// Query executes HERE on first access
var orders = await node.GetValueAsync<List<Order>>();

// No query on subsequent access (cached)
var ordersAgain = await node.GetValueAsync<List<Order>>();
```

---

### BR-3: Provider Selection by Path
**As a** framework
**I want** to select the appropriate provider based on the navigation path
**So that** different parts of the data model can come from different sources

**Acceptance Criteria:**
- Wildcard path matching (`Customer/*` matches `Customer/Orders`, `Customer/Profile`)
- Recursive matching (`Customer/**` matches any depth under `Customer`)
- Most specific path wins (longest match)
- Provider fallback if no exact match

**Example:**
```csharp
container.RegisterProvider("Customer/*", new DatabaseProvider());
container.RegisterProvider("Customer/Orders/*", new OrderServiceProvider());  // More specific
container.RegisterProvider("**", new DefaultProvider());  // Fallback

// Uses OrderServiceProvider (most specific)
var orders = container.Navigate("Customer/Orders/123");

// Uses DatabaseProvider
var profile = container.Navigate("Customer/Profile");

// Uses DefaultProvider (fallback)
var unknown = container.Navigate("Something/Else");
```

---

### BR-4: Async Provider Support
**As a** provider implementer
**I want** async data loading support
**So that** I can make async database/API calls without blocking

**Acceptance Criteria:**
- IDataProvider.ProvideAsync() method
- Support for CancellationToken
- Async/await pattern throughout
- No synchronous blocking

**Example:**
```csharp
public class DatabaseProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        // Async database query
        return await _dbContext.Customers
            .Where(c => c.Id == node.Parent.Value.Id)
            .ToListAsync(cancellationToken);
    }
}
```

---

### BR-5: Provider Context
**As a** provider
**I want** access to navigation context (parent nodes, path segments, metadata)
**So that** I can make context-aware queries (e.g., load orders for specific customer)

**Acceptance Criteria:**
- IDataNode provides access to parent node
- IDataNode provides current path
- Context dictionary for passing query parameters
- Provider can navigate to sibling/parent data

**Example:**
```csharp
public class OrdersProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        // Access parent customer node
        var customerId = node.Parent?.Value?.Id;

        // Context-aware query
        return await _dbContext.Orders
            .Where(o => o.CustomerId == customerId)
            .ToListAsync(cancellationToken);
    }
}
```

---

## Technical Requirements

### TR-1: IDataProvider Interface

```csharp
/// <summary>
/// Data provider abstraction for lazy loading.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Provides data for the given node and path.
    /// </summary>
    /// <param name="node">Current data node</param>
    /// <param name="path">Full navigation path</param>
    /// <param name="context">Optional context dictionary</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Data for this node</returns>
    Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Can this provider handle the given path?
    /// </summary>
    /// <param name="path">Navigation path</param>
    /// <returns>True if this provider can handle the path</returns>
    bool CanProvide(string path);

    /// <summary>
    /// Provider priority (higher wins if multiple providers match).
    /// Default: 0
    /// </summary>
    int Priority { get; }

    /// <summary>
    /// Optional schema metadata for this provider's data.
    /// </summary>
    ICanonicalSchema? SchemaMetadata { get; }
}
```

---

### TR-2: Built-in Provider Implementations

**StaticDataProvider** (already implemented):
```csharp
public class StaticDataProvider : IDataProvider
{
    private readonly object _data;

    public StaticDataProvider(object data)
    {
        _data = data;
    }

    public Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<object?>(_data);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}
```

**DelegateDataProvider** (already implemented):
```csharp
public class DelegateDataProvider : IDataProvider
{
    private readonly Func<IDataNode, string, IDictionary<string, object?>?, CancellationToken, Task<object?>> _provider;

    public DelegateDataProvider(
        Func<IDataNode, string, IDictionary<string, object?>?, CancellationToken, Task<object?>> provider)
    {
        _provider = provider;
    }

    public Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        return _provider(node, path, context, cancellationToken);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}
```

**DatabaseDataProvider** (new):
```csharp
/// <summary>
/// Loads data from Entity Framework DbContext.
/// </summary>
public class DatabaseDataProvider<TEntity> : IDataProvider
    where TEntity : class
{
    private readonly DbContext _dbContext;
    private readonly Func<DbSet<TEntity>, IDataNode, IQueryable<TEntity>>? _queryBuilder;

    public DatabaseDataProvider(
        DbContext dbContext,
        Func<DbSet<TEntity>, IDataNode, IQueryable<TEntity>>? queryBuilder = null)
    {
        _dbContext = dbContext;
        _queryBuilder = queryBuilder;
    }

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Set<TEntity>().AsQueryable();

        if (_queryBuilder != null)
            query = _queryBuilder(query.AsDbSet(), node);

        return await query.ToListAsync(cancellationToken);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;  // Could infer from EF metadata
}
```

**ApiDataProvider** (new):
```csharp
/// <summary>
/// Loads data from REST API.
/// </summary>
public class ApiDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly Func<string, IDataNode, string>? _urlBuilder;

    public ApiDataProvider(
        HttpClient httpClient,
        string baseUrl,
        Func<string, IDataNode, string>? urlBuilder = null)
    {
        _httpClient = httpClient;
        _baseUrl = baseUrl;
        _urlBuilder = urlBuilder;
    }

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        var url = _urlBuilder != null
            ? _urlBuilder(_baseUrl, node)
            : $"{_baseUrl}/{path}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}
```

**ConfigurationDataProvider** (new):
```csharp
/// <summary>
/// Loads data from IConfiguration.
/// </summary>
public class ConfigurationDataProvider : IDataProvider
{
    private readonly IConfiguration _configuration;
    private readonly string _sectionPrefix;

    public ConfigurationDataProvider(
        IConfiguration configuration,
        string sectionPrefix = "")
    {
        _configuration = configuration;
        _sectionPrefix = sectionPrefix;
    }

    public Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        var configPath = string.IsNullOrEmpty(_sectionPrefix)
            ? path.Replace("/", ":")
            : $"{_sectionPrefix}:{path.Replace("/", ":")}";

        var section = _configuration.GetSection(configPath);
        return Task.FromResult<object?>(section.Value ?? section.GetChildren().ToList());
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}
```

**FileDataProvider** (new):
```csharp
/// <summary>
/// Loads data from JSON/XML files.
/// </summary>
public class FileDataProvider : IDataProvider
{
    private readonly string _filePath;
    private readonly FileFormat _format;

    public FileDataProvider(string filePath, FileFormat format = FileFormat.Json)
    {
        _filePath = filePath;
        _format = format;
    }

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        var content = await File.ReadAllTextAsync(_filePath, cancellationToken);

        return _format switch
        {
            FileFormat.Json => JsonSerializer.Deserialize<object>(content),
            FileFormat.Xml => XDocument.Parse(content),
            _ => throw new NotSupportedException($"Format {_format} not supported")
        };
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}

public enum FileFormat
{
    Json,
    Xml,
    Yaml
}
```

---

### TR-3: Provider Registry

```csharp
/// <summary>
/// Provider registry with path matching.
/// </summary>
public interface IDataProviderRegistry
{
    /// <summary>
    /// Registers provider for specific path pattern.
    /// </summary>
    void Register(string pathPattern, IDataProvider provider);

    /// <summary>
    /// Finds best matching provider for path.
    /// </summary>
    IDataProvider? FindProvider(string path);

    /// <summary>
    /// Gets all providers matching path (ordered by priority).
    /// </summary>
    IEnumerable<IDataProvider> GetProviders(string path);

    /// <summary>
    /// Unregisters provider for path pattern.
    /// </summary>
    void Unregister(string pathPattern);

    /// <summary>
    /// Clears all providers.
    /// </summary>
    void Clear();
}
```

---

### TR-4: Path Pattern Matching

**Pattern Syntax:**
- `Customer` - Exact match
- `Customer/*` - Single-level wildcard (matches `Customer/Orders`, not `Customer/Orders/123`)
- `Customer/**` - Multi-level wildcard (matches any depth under `Customer`)
- `**` - Universal wildcard (matches everything)

**Matching Priority:**
1. Exact path match
2. Longest wildcard match
3. Provider priority property
4. Registration order (last registered wins)

**Example:**
```csharp
container.RegisterProvider("Customer/Orders", exactProvider);         // Priority 1
container.RegisterProvider("Customer/*", singleLevelProvider);        // Priority 2
container.RegisterProvider("Customer/**", multiLevelProvider);        // Priority 3
container.RegisterProvider("**", universalProvider);                  // Priority 4

// "Customer/Orders" → exactProvider
// "Customer/Profile" → singleLevelProvider
// "Customer/Orders/123" → multiLevelProvider
// "Product/123" → universalProvider
```

---

## Non-Functional Requirements

### NFR-1: Performance
- Provider lookup: < 5ms
- Lazy loading overhead: < 10ms
- Cache hit: < 1ms
- 50-70% query reduction vs eager loading

### NFR-2: Compatibility
- Works with .NET 10.0
- Compatible with Entity Framework Core 10+
- Compatible with ASP.NET Core IConfiguration
- No breaking changes to IDataContainer

### NFR-3: Extensibility
- Easy to implement custom providers
- Provider composition (chain multiple providers)
- Provider decorators for caching, logging, etc.

### NFR-4: Thread Safety
- Provider registry is thread-safe
- Concurrent provider access supported
- No race conditions in lazy loading

---

## Constraints

### C-1: Provider Limitations
- Providers are stateless (no internal caching beyond framework)
- Providers cannot modify container structure
- Providers responsible for their own error handling

### C-2: Path Matching Complexity
- Wildcard matching has O(n) complexity (n = number of registered providers)
- Deep path hierarchies may have performance implications
- Recommend using specific paths over broad wildcards

---

## Success Criteria

- ✅ 5+ built-in providers (Static, Delegate, Database, API, Configuration, File)
- ✅ Path pattern matching with wildcards
- ✅ Provider priority and selection
- ✅ Lazy loading with caching
- ✅ Async support throughout
- ✅ 80%+ test coverage

---

## Out of Scope

- ❌ Provider composition/chaining (future enhancement)
- ❌ Provider decorators (caching, logging, etc.) (future enhancement)
- ❌ GraphQL provider (future enhancement)
- ❌ gRPC provider (future enhancement)
- ❌ Provider health checks (future enhancement)

---

## Dependencies

### Internal
- Core Container & Navigation (Epic 11)

### External
- .NET 10.0 BCL
- Microsoft.Extensions.Configuration (for ConfigurationDataProvider)
- System.Net.Http (for ApiDataProvider)
- Entity Framework Core (optional, for DatabaseDataProvider)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container Requirements](../CoreContainer/requirements.md)
