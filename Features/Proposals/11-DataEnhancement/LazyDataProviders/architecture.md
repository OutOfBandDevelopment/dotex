# Lazy Data Providers - Architecture

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Lazy Data Providers
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM

---

## Overview

Pluggable provider architecture for lazy-loading data from diverse sources (databases, APIs, configuration, files). Uses path-based provider selection with wildcard matching and priority-based resolution.

---

## Architectural Goals

1. **Pluggability**: Easy to add new data sources via IDataProvider
2. **Lazy Loading**: Data loaded only when accessed, with automatic caching
3. **Path-Based Selection**: Providers selected based on navigation path
4. **Performance**: Minimal overhead for provider lookup and lazy evaluation
5. **Composability**: Providers work seamlessly with path translation and schema discovery

---

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────────┐
│                         IDataContainer                                   │
│                                                                           │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                    IDataProviderRegistry                          │  │
│  │                                                                    │  │
│  │  Registered Providers:                                            │  │
│  │  ┌──────────────────────────────────────────────────────────────┐│  │
│  │  │ "Customer/*"         → DatabaseProvider                       ││  │
│  │  │ "Customer/Orders/**" → OrderServiceProvider (higher priority) ││  │
│  │  │ "Config/**"          → ConfigurationProvider                  ││  │
│  │  │ "Weather/**"         → ApiProvider                            ││  │
│  │  │ "**"                 → DefaultProvider (fallback)             ││  │
│  │  └──────────────────────────────────────────────────────────────┘│  │
│  │                                                                    │  │
│  │  FindProvider(path) → IDataProvider                               │  │
│  └──────────────────────────────────────────────────────────────────┘  │
│                                ↓                                         │
│  ┌──────────────────────────────────────────────────────────────────┐  │
│  │                    IDataNode (Lazy Loading)                       │  │
│  │                                                                    │  │
│  │  - Value (lazy, cached)                                           │  │
│  │  - GetValueAsync<T>()                                             │  │
│  │  - Provider selection on first access                             │  │
│  │  - Result caching after first load                                │  │
│  └──────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
                                 ↓
         ┌───────────────────────┴───────────────────────┐
         │                                               │
         ▼                                               ▼
┌─────────────────────┐                    ┌─────────────────────────┐
│   IDataProvider     │                    │   IDataProvider         │
│   Implementations   │                    │   Implementations       │
│                     │                    │                         │
│ - DatabaseProvider  │                    │ - ApiProvider           │
│ - ConfigProvider    │                    │ - FileProvider          │
│ - StaticProvider    │                    │ - CustomProvider        │
└─────────────────────┘                    └─────────────────────────┘
         │                                               │
         ▼                                               ▼
┌─────────────────────┐                    ┌─────────────────────────┐
│  Data Source        │                    │  Data Source            │
│  (Database, EF)     │                    │  (HTTP API, Files)      │
└─────────────────────┘                    └─────────────────────────┘
```

---

## Core Components

### 1. IDataProvider (Already Defined in Requirements)

```csharp
public interface IDataProvider
{
    Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default);

    bool CanProvide(string path);
    int Priority { get; }
    ICanonicalSchema? SchemaMetadata { get; }
}
```

---

### 2. IDataProviderRegistry

```csharp
/// <summary>
/// Provider registry with path-based selection.
/// </summary>
public interface IDataProviderRegistry
{
    void Register(string pathPattern, IDataProvider provider);
    IDataProvider? FindProvider(string path);
    IEnumerable<IDataProvider> GetProviders(string path);
    void Unregister(string pathPattern);
    void Clear();
}

/// <summary>
/// Default implementation using pattern matching.
/// </summary>
public class DataProviderRegistry : IDataProviderRegistry
{
    private readonly List<ProviderRegistration> _registrations = new();
    private readonly object _lock = new object();

    public void Register(string pathPattern, IDataProvider provider)
    {
        lock (_lock)
        {
            _registrations.Add(new ProviderRegistration
            {
                Pattern = pathPattern,
                Provider = provider,
                Matcher = CreateMatcher(pathPattern)
            });
        }
    }

    public IDataProvider? FindProvider(string path)
    {
        lock (_lock)
        {
            // Find all matching providers
            var matches = _registrations
                .Where(r => r.Matcher(path))
                .OrderByDescending(r => GetMatchScore(r.Pattern, path))  // Longest match
                .ThenByDescending(r => r.Provider.Priority)                // Highest priority
                .ThenBy(r => r.RegistrationOrder);                         // Last registered

            return matches.FirstOrDefault()?.Provider;
        }
    }

    public IEnumerable<IDataProvider> GetProviders(string path)
    {
        lock (_lock)
        {
            return _registrations
                .Where(r => r.Matcher(path))
                .OrderByDescending(r => GetMatchScore(r.Pattern, path))
                .ThenByDescending(r => r.Provider.Priority)
                .Select(r => r.Provider)
                .ToList();
        }
    }

    public void Unregister(string pathPattern)
    {
        lock (_lock)
        {
            _registrations.RemoveAll(r => r.Pattern == pathPattern);
        }
    }

    public void Clear()
    {
        lock (_lock)
        {
            _registrations.Clear();
        }
    }

    private Func<string, bool> CreateMatcher(string pattern)
    {
        // Exact match
        if (!pattern.Contains("*"))
            return path => path == pattern;

        // Universal wildcard
        if (pattern == "**")
            return _ => true;

        // Convert pattern to regex
        var regexPattern = "^" + Regex.Escape(pattern)
            .Replace("\\*\\*/", ".*")     // Multi-level wildcard
            .Replace("\\*\\*", ".*")      // Multi-level wildcard (end)
            .Replace("\\*/", "[^/]+/")    // Single-level wildcard
            .Replace("\\*", "[^/]+")      // Single-level wildcard (end)
            + "$";

        var regex = new Regex(regexPattern, RegexOptions.Compiled);
        return path => regex.IsMatch(path);
    }

    private int GetMatchScore(string pattern, string path)
    {
        // Exact match: highest score
        if (pattern == path)
            return int.MaxValue;

        // Specific path (no wildcards): score = length
        if (!pattern.Contains("*"))
            return pattern.Length;

        // Wildcard patterns: score based on specificity
        var wildcardIndex = pattern.IndexOf('*');
        return wildcardIndex;  // Longer prefix before wildcard = higher score
    }

    private class ProviderRegistration
    {
        public string Pattern { get; init; } = "";
        public IDataProvider Provider { get; init; } = null!;
        public Func<string, bool> Matcher { get; init; } = null!;
        public int RegistrationOrder { get; init; }
    }
}
```

---

### 3. DataContainer Integration

The DataContainer (from Core Container feature) integrates the provider registry:

```csharp
public class DataContainer : IDataContainer
{
    private readonly IDataProviderRegistry _providerRegistry;
    private readonly DataNode _rootNode;

    public DataContainer()
    {
        _providerRegistry = new DataProviderRegistry();
        _rootNode = new DataNode(this, "/", null);
    }

    public void RegisterProvider(string pathPattern, IDataProvider provider)
    {
        _providerRegistry.Register(pathPattern, provider);
    }

    public IDataNode Navigate(string path)
    {
        // Navigate to path, creating nodes as needed
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var currentNode = _rootNode;

        foreach (var segment in segments)
        {
            currentNode = currentNode.GetOrCreateChild(segment);
        }

        return currentNode;
    }

    internal IDataProvider? FindProvider(string path)
    {
        return _providerRegistry.FindProvider(path);
    }
}
```

---

### 4. DataNode Lazy Loading (Enhanced)

```csharp
public class DataNode : IDataNode
{
    private readonly IDataContainer _container;
    private readonly string _path;
    private readonly DataNode? _parent;
    private readonly Dictionary<string, DataNode> _children = new();

    private object? _value;
    private bool _valueLoaded;
    private readonly object _valueLock = new object();

    public DataNode(IDataContainer container, string path, DataNode? parent)
    {
        _container = container;
        _path = path;
        _parent = parent;
    }

    public object? Value
    {
        get
        {
            if (!_valueLoaded)
            {
                lock (_valueLock)
                {
                    if (!_valueLoaded)
                    {
                        LoadValueAsync().GetAwaiter().GetResult();
                        _valueLoaded = true;
                    }
                }
            }
            return _value;
        }
    }

    public async Task<T?> GetValueAsync<T>(CancellationToken cancellationToken = default)
    {
        if (!_valueLoaded)
        {
            await LoadValueAsync(cancellationToken);
            _valueLoaded = true;
        }

        return _value is T typedValue ? typedValue : default;
    }

    private async Task LoadValueAsync(CancellationToken cancellationToken = default)
    {
        var provider = ((DataContainer)_container).FindProvider(_path);

        if (provider == null)
        {
            _value = null;
            return;
        }

        try
        {
            _value = await provider.ProvideAsync(this, _path, null, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new DataProviderException($"Provider failed for path '{_path}'", ex);
        }
    }

    public IDataNode? Parent => _parent;
    public string Path => _path;

    internal DataNode GetOrCreateChild(string segment)
    {
        lock (_children)
        {
            if (!_children.TryGetValue(segment, out var child))
            {
                var childPath = _path == "/" ? $"/{segment}" : $"{_path}/{segment}";
                child = new DataNode(_container, childPath, this);
                _children[segment] = child;
            }
            return child;
        }
    }
}
```

---

## Design Patterns

### 1. Provider Pattern (Strategy)

Each IDataProvider implementation is a strategy for loading data.

```
┌──────────────────┐
│  IDataProvider   │
└────────┬─────────┘
         │
    ┌────┴────┬──────────┬────────────┬──────────┐
    │         │          │            │          │
┌───▼───┐ ┌──▼───┐ ┌────▼─────┐ ┌────▼────┐ ┌──▼──────┐
│Static │ │ DB   │ │   API    │ │ Config  │ │ Custom  │
│Provider│ │Provider│ │ Provider │ │Provider │ │Provider │
└────────┘ └──────┘ └──────────┘ └─────────┘ └─────────┘
```

### 2. Registry Pattern

IDataProviderRegistry maintains a collection of providers with pattern-based lookup.

### 3. Lazy Loading Pattern

Value loaded on-demand with double-checked locking for thread safety.

### 4. Template Method Pattern (in DataNode)

```csharp
// Template method for value loading
public async Task<T?> GetValueAsync<T>()
{
    if (!_valueLoaded)          // Check
    {
        await LoadValueAsync();  // Load (virtual/extensible)
        _valueLoaded = true;     // Mark loaded
    }
    return (T?)_value;          // Return
}
```

---

## Data Flow

### Provider Registration Flow

```
┌─────────────────────────┐
│ Application Code        │
│ container.Register(     │
│   "Customer/*",         │
│   dbProvider)           │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ IDataProviderRegistry   │
│ - Store pattern         │
│ - Store provider        │
│ - Create matcher        │
└─────────────────────────┘
```

### Lazy Loading Flow

```
┌─────────────────────────┐
│ Application Code        │
│ var orders = await      │
│   node.GetValueAsync()  │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ DataNode                │
│ - Check if loaded       │
│ - If not: find provider │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ IDataProviderRegistry   │
│ - Match path pattern    │
│ - Return best provider  │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ IDataProvider           │
│ - ProvideAsync()        │
│ - Load from source      │
│ - Return data           │
└────────┬────────────────┘
         │
         ▼
┌─────────────────────────┐
│ DataNode                │
│ - Cache result          │
│ - Mark as loaded        │
│ - Return to caller      │
└─────────────────────────┘
```

### Provider Selection Algorithm

```
Input: path = "Customer/Orders/123"

Registered Providers:
1. "Customer/Orders/123" (exact)      → DatabaseProvider    (Priority: 10)
2. "Customer/Orders/*"   (specific)   → OrderProvider       (Priority: 5)
3. "Customer/**"         (wildcard)   → CustomerProvider    (Priority: 0)
4. "**"                  (universal)  → DefaultProvider     (Priority: 0)

Matching:
1. ✅ "Customer/Orders/123" matches (exact)
2. ✅ "Customer/Orders/*" matches (specific wildcard)
3. ✅ "Customer/**" matches (multi-level wildcard)
4. ✅ "**" matches (universal wildcard)

Scoring:
1. Exact: score = MAX_INT
2. Specific: score = 16 (length before *)
3. Wildcard: score = 9 (length before **)
4. Universal: score = 0

Ordering:
1. DatabaseProvider    (score = MAX_INT, priority = 10)
2. OrderProvider       (score = 16, priority = 5)
3. CustomerProvider    (score = 9, priority = 0)
4. DefaultProvider     (score = 0, priority = 0)

Selected: DatabaseProvider (highest score)
```

---

## Built-in Provider Implementations

### DatabaseDataProvider (Entity Framework)

```csharp
public class DatabaseDataProvider<TEntity> : IDataProvider
    where TEntity : class
{
    private readonly DbContext _dbContext;
    private readonly Func<IQueryable<TEntity>, IDataNode, IQueryable<TEntity>>? _queryBuilder;

    public DatabaseDataProvider(
        DbContext dbContext,
        Func<IQueryable<TEntity>, IDataNode, IQueryable<TEntity>>? queryBuilder = null)
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
            query = _queryBuilder(query, node);

        return await query.ToListAsync(cancellationToken);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => InferFromEntityType();

    private ICanonicalSchema InferFromEntityType()
    {
        // Use EF metadata to infer schema
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
        // ... build ICanonicalSchema from EF metadata
        return null;  // Simplified
    }
}
```

**Usage:**
```csharp
container.RegisterProvider("Customer/Orders", new DatabaseDataProvider<Order>(
    dbContext,
    (query, node) =>
    {
        // Context-aware query (filter by customer ID from parent node)
        var customerId = node.Parent?.Value?.Id;
        return query.Where(o => o.CustomerId == customerId);
    }));
```

---

### ApiDataProvider (REST API)

```csharp
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
            : $"{_baseUrl}/{path.Replace("/", "")}";

        var response = await _httpClient.GetAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<object>(json);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}
```

**Usage:**
```csharp
container.RegisterProvider("Weather/**", new ApiDataProvider(
    httpClient,
    "https://api.weather.com/v1",
    (baseUrl, node) => $"{baseUrl}/forecast/{node.Path.Split('/').Last()}"
));
```

---

### ConfigurationDataProvider

```csharp
public class ConfigurationDataProvider : IDataProvider
{
    private readonly IConfiguration _configuration;
    private readonly string _sectionPrefix;

    public ConfigurationDataProvider(IConfiguration configuration, string sectionPrefix = "")
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
        // Convert path separator: "/" → ":"
        var configPath = string.IsNullOrEmpty(_sectionPrefix)
            ? path.Replace("/", ":")
            : $"{_sectionPrefix}:{path.Replace("/", ":")}";

        var section = _configuration.GetSection(configPath);

        // Return value or children
        object? result = section.Value ?? section.GetChildren().ToDictionary(
            s => s.Key,
            s => s.Value as object);

        return Task.FromResult(result);
    }

    public bool CanProvide(string path) => true;
    public int Priority => 0;
    public ICanonicalSchema? SchemaMetadata => null;
}
```

**Usage:**
```csharp
// appsettings.json:
// {
//   "Database": {
//     "ConnectionString": "...",
//     "Timeout": 30
//   }
// }

container.RegisterProvider("Config/**", new ConfigurationDataProvider(configuration));

// Access:
var connectionString = await container.Navigate("Config/Database/ConnectionString").GetValueAsync<string>();
```

---

## Error Handling

### Provider Exceptions

```csharp
public class DataProviderException : Exception
{
    public string Path { get; }
    public IDataProvider? Provider { get; }

    public DataProviderException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }

    public DataProviderException(
        string message,
        string path,
        IDataProvider? provider = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        Path = path;
        Provider = provider;
    }
}
```

### Error Handling Strategy

```csharp
private async Task LoadValueAsync(CancellationToken cancellationToken = default)
{
    var provider = ((DataContainer)_container).FindProvider(_path);

    if (provider == null)
    {
        // No provider: return null
        _value = null;
        return;
    }

    try
    {
        _value = await provider.ProvideAsync(this, _path, null, cancellationToken);
    }
    catch (OperationCanceledException)
    {
        // Cancellation: rethrow
        throw;
    }
    catch (Exception ex)
    {
        // Provider error: wrap and rethrow
        throw new DataProviderException(
            $"Provider failed for path '{_path}'",
            _path,
            provider,
            ex);
    }
}
```

---

## Performance Considerations

### Provider Lookup Caching

```csharp
public class CachingProviderRegistry : IDataProviderRegistry
{
    private readonly IDataProviderRegistry _inner;
    private readonly ConcurrentDictionary<string, IDataProvider?> _lookupCache = new();

    public IDataProvider? FindProvider(string path)
    {
        return _lookupCache.GetOrAdd(path, p => _inner.FindProvider(p));
    }

    public void Register(string pathPattern, IDataProvider provider)
    {
        _inner.Register(pathPattern, provider);
        _lookupCache.Clear();  // Invalidate cache
    }
}
```

### Lazy Value Caching

Double-checked locking ensures single provider call:

```csharp
if (!_valueLoaded)
{
    lock (_valueLock)
    {
        if (!_valueLoaded)  // Double-check
        {
            await LoadValueAsync();
            _valueLoaded = true;
        }
    }
}
```

---

## Thread Safety

- **IDataProviderRegistry**: Thread-safe with locking
- **DataNode lazy loading**: Thread-safe with double-checked locking
- **Providers**: Stateless, inherently thread-safe
- **Concurrent access**: Multiple threads can navigate and load simultaneously

---

## Integration with Other Features

### Schema Discovery Integration

```csharp
public class DatabaseDataProvider<TEntity> : IDataProvider
{
    public ICanonicalSchema? SchemaMetadata => InferSchemaFromEntityType();

    private ICanonicalSchema InferSchemaFromEntityType()
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
        // Convert EF metadata to ICanonicalSchema
        return schemaService.InferSchemaFromType(typeof(TEntity));
    }
}
```

### Path Translation Integration

```csharp
// Providers work with canonical paths
var translationService = new PathTranslationService();
var canonical = translationService.ParseAny("$.Customer.Orders");

// Navigate using canonical path
var node = container.Navigate(canonical.ToString());
var orders = await node.GetValueAsync<List<Order>>();
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container Architecture](../CoreContainer/architecture.md)
