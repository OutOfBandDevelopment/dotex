# Lazy Data Providers - API Design

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Lazy Data Providers
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM

---

## Overview

Complete API surface for pluggable, lazy-loading data providers. Includes built-in providers for common scenarios (database, API, configuration, files) and extensibility points for custom providers.

---

## Core Interfaces

### IDataProvider

```csharp
namespace OoBDev.Framework.Data
{
    /// <summary>
    /// Data provider abstraction for lazy loading from various sources.
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// Provides data for the given node and path.
        /// Called when node value is first accessed (lazy loading).
        /// </summary>
        /// <param name="node">Current data node (provides parent access, path)</param>
        /// <param name="path">Full navigation path</param>
        /// <param name="context">Optional context dictionary for passing parameters</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Data for this node, or null</returns>
        Task<object?> ProvideAsync(
            IDataNode node,
            string path,
            IDictionary<string, object?>? context,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Can this provider handle the given path?
        /// Used for filtering providers during selection.
        /// </summary>
        /// <param name="path">Navigation path</param>
        /// <returns>True if this provider can handle the path</returns>
        bool CanProvide(string path);

        /// <summary>
        /// Provider priority (higher wins if multiple providers match).
        /// Default: 0
        /// Range: -1000 to 1000
        /// </summary>
        int Priority { get; }

        /// <summary>
        /// Optional schema metadata for this provider's data.
        /// Used by schema discovery feature.
        /// </summary>
        ICanonicalSchema? SchemaMetadata { get; }
    }
}
```

---

### IDataProviderRegistry

```csharp
namespace OoBDev.Framework.Data
{
    /// <summary>
    /// Provider registry with path-based selection.
    /// </summary>
    public interface IDataProviderRegistry
    {
        /// <summary>
        /// Registers provider for specific path pattern.
        /// </summary>
        /// <param name="pathPattern">Path pattern (supports wildcards *, **)</param>
        /// <param name="provider">Data provider</param>
        /// <example>
        /// registry.Register("Customer/*", dbProvider);
        /// registry.Register("Config/**", configProvider);
        /// </example>
        void Register(string pathPattern, IDataProvider provider);

        /// <summary>
        /// Finds best matching provider for path.
        /// Returns null if no provider matches.
        /// </summary>
        /// <param name="path">Navigation path</param>
        /// <returns>Best matching provider, or null</returns>
        IDataProvider? FindProvider(string path);

        /// <summary>
        /// Gets all providers matching path (ordered by priority).
        /// </summary>
        /// <param name="path">Navigation path</param>
        /// <returns>All matching providers</returns>
        IEnumerable<IDataProvider> GetProviders(string path);

        /// <summary>
        /// Unregisters provider for path pattern.
        /// </summary>
        /// <param name="pathPattern">Path pattern to unregister</param>
        void Unregister(string pathPattern);

        /// <summary>
        /// Clears all providers.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets all registered path patterns.
        /// </summary>
        IReadOnlyList<string> RegisteredPatterns { get; }
    }
}
```

---

## Built-in Providers

### StaticDataProvider

```csharp
namespace OoBDev.Framework.Data.Providers
{
    /// <summary>
    /// Provider that returns static in-memory data.
    /// </summary>
    public class StaticDataProvider : IDataProvider
    {
        private readonly object? _data;

        /// <summary>
        /// Creates static provider with given data.
        /// </summary>
        /// <param name="data">Static data to return</param>
        public StaticDataProvider(object? data)
        {
            _data = data;
        }

        public Task<object?> ProvideAsync(
            IDataNode node,
            string path,
            IDictionary<string, object?>? context,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_data);
        }

        public bool CanProvide(string path) => true;

        public int Priority => 0;

        public ICanonicalSchema? SchemaMetadata => null;
    }
}
```

---

### DelegateDataProvider

```csharp
namespace OoBDev.Framework.Data.Providers
{
    /// <summary>
    /// Provider that uses a delegate function to provide data.
    /// </summary>
    public class DelegateDataProvider : IDataProvider
    {
        private readonly Func<IDataNode, string, IDictionary<string, object?>?, CancellationToken, Task<object?>> _provider;
        private readonly int _priority;

        /// <summary>
        /// Creates delegate provider with async function.
        /// </summary>
        public DelegateDataProvider(
            Func<IDataNode, string, IDictionary<string, object?>?, CancellationToken, Task<object?>> provider,
            int priority = 0)
        {
            _provider = provider ?? throw new ArgumentNullException(nameof(provider));
            _priority = priority;
        }

        /// <summary>
        /// Creates delegate provider with sync function.
        /// </summary>
        public DelegateDataProvider(
            Func<IDataNode, string, IDictionary<string, object?>?, object?> provider,
            int priority = 0)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            _provider = (node, path, context, ct) => Task.FromResult(provider(node, path, context));
            _priority = priority;
        }

        /// <summary>
        /// Creates simple delegate provider (no context needed).
        /// </summary>
        public DelegateDataProvider(Func<object?> provider, int priority = 0)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            _provider = (_, __, ___, ____) => Task.FromResult(provider());
            _priority = priority;
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

        public int Priority => _priority;

        public ICanonicalSchema? SchemaMetadata => null;
    }
}
```

---

### DatabaseDataProvider

```csharp
namespace OoBDev.Framework.Data.Providers
{
    /// <summary>
    /// Provider that loads data from Entity Framework DbContext.
    /// </summary>
    public class DatabaseDataProvider<TEntity> : IDataProvider
        where TEntity : class
    {
        private readonly DbContext _dbContext;
        private readonly Func<IQueryable<TEntity>, IDataNode, IQueryable<TEntity>>? _queryBuilder;
        private readonly int _priority;
        private readonly Lazy<ICanonicalSchema?> _schemaMetadata;

        /// <summary>
        /// Creates database provider for entity type.
        /// </summary>
        /// <param name="dbContext">EF DbContext</param>
        /// <param name="queryBuilder">Optional query builder (for filtering, includes, etc.)</param>
        /// <param name="priority">Provider priority</param>
        public DatabaseDataProvider(
            DbContext dbContext,
            Func<IQueryable<TEntity>, IDataNode, IQueryable<TEntity>>? queryBuilder = null,
            int priority = 0)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _queryBuilder = queryBuilder;
            _priority = priority;
            _schemaMetadata = new Lazy<ICanonicalSchema?>(() => InferSchemaFromEntityType());
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

        public int Priority => _priority;

        public ICanonicalSchema? SchemaMetadata => _schemaMetadata.Value;

        private ICanonicalSchema? InferSchemaFromEntityType()
        {
            // Use EF metadata to infer schema
            try
            {
                var entityType = _dbContext.Model.FindEntityType(typeof(TEntity));
                if (entityType == null) return null;

                // Build schema from EF metadata (properties, relationships, etc.)
                // ... implementation details
                return null;  // Simplified for example
            }
            catch
            {
                return null;
            }
        }
    }
}
```

---

### ApiDataProvider

```csharp
namespace OoBDev.Framework.Data.Providers
{
    /// <summary>
    /// Provider that loads data from HTTP REST API.
    /// </summary>
    public class ApiDataProvider : IDataProvider
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly Func<string, IDataNode, IDictionary<string, object?>?, string>? _urlBuilder;
        private readonly int _priority;

        /// <summary>
        /// Creates API provider.
        /// </summary>
        /// <param name="httpClient">HTTP client</param>
        /// <param name="baseUrl">Base URL for API</param>
        /// <param name="urlBuilder">Optional URL builder function</param>
        /// <param name="priority">Provider priority</param>
        public ApiDataProvider(
            HttpClient httpClient,
            string baseUrl,
            Func<string, IDataNode, IDictionary<string, object?>?, string>? urlBuilder = null,
            int priority = 0)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
            _urlBuilder = urlBuilder;
            _priority = priority;
        }

        public async Task<object?> ProvideAsync(
            IDataNode node,
            string path,
            IDictionary<string, object?>? context,
            CancellationToken cancellationToken = default)
        {
            string url;

            if (_urlBuilder != null)
            {
                url = _urlBuilder(_baseUrl, node, context);
            }
            else
            {
                // Default: append path to base URL
                var pathSegment = path.TrimStart('/').Replace("/", "");
                url = $"{_baseUrl.TrimEnd('/')}/{pathSegment}";
            }

            try
            {
                var response = await _httpClient.GetAsync(url, cancellationToken);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<object>(json);
            }
            catch (HttpRequestException ex)
            {
                throw new DataProviderException($"API request failed: {url}", path, this, ex);
            }
        }

        public bool CanProvide(string path) => true;

        public int Priority => _priority;

        public ICanonicalSchema? SchemaMetadata => null;
    }
}
```

---

### ConfigurationDataProvider

```csharp
namespace OoBDev.Framework.Data.Providers
{
    /// <summary>
    /// Provider that loads data from IConfiguration (appsettings.json, environment variables, etc.).
    /// </summary>
    public class ConfigurationDataProvider : IDataProvider
    {
        private readonly IConfiguration _configuration;
        private readonly string _sectionPrefix;
        private readonly int _priority;

        /// <summary>
        /// Creates configuration provider.
        /// </summary>
        /// <param name="configuration">Configuration instance</param>
        /// <param name="sectionPrefix">Optional section prefix (e.g., "Database")</param>
        /// <param name="priority">Provider priority</param>
        public ConfigurationDataProvider(
            IConfiguration configuration,
            string sectionPrefix = "",
            int priority = 0)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _sectionPrefix = sectionPrefix;
            _priority = priority;
        }

        public Task<object?> ProvideAsync(
            IDataNode node,
            string path,
            IDictionary<string, object?>? context,
            CancellationToken cancellationToken = default)
        {
            // Convert path: "/" → ":"
            var configPath = path.Replace("/", ":");

            if (!string.IsNullOrEmpty(_sectionPrefix))
                configPath = $"{_sectionPrefix}:{configPath}";

            configPath = configPath.TrimStart(':');

            var section = _configuration.GetSection(configPath);

            if (section == null || !section.Exists())
                return Task.FromResult<object?>(null);

            // If section has value, return it
            if (section.Value != null)
                return Task.FromResult<object?>(section.Value);

            // Otherwise, return children as dictionary
            var children = section.GetChildren().ToDictionary(
                s => s.Key,
                s => s.Value as object);

            return Task.FromResult<object?>(children);
        }

        public bool CanProvide(string path) => true;

        public int Priority => _priority;

        public ICanonicalSchema? SchemaMetadata => null;
    }
}
```

---

### FileDataProvider

```csharp
namespace OoBDev.Framework.Data.Providers
{
    /// <summary>
    /// Provider that loads data from files (JSON, XML, YAML).
    /// </summary>
    public class FileDataProvider : IDataProvider
    {
        private readonly string _filePath;
        private readonly FileFormat _format;
        private readonly int _priority;
        private object? _cachedData;
        private bool _loaded;
        private readonly object _loadLock = new object();

        /// <summary>
        /// Creates file provider.
        /// </summary>
        /// <param name="filePath">Path to file</param>
        /// <param name="format">File format (JSON, XML, YAML)</param>
        /// <param name="priority">Provider priority</param>
        public FileDataProvider(
            string filePath,
            FileFormat format = FileFormat.Json,
            int priority = 0)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            _format = format;
            _priority = priority;
        }

        public async Task<object?> ProvideAsync(
            IDataNode node,
            string path,
            IDictionary<string, object?>? context,
            CancellationToken cancellationToken = default)
        {
            if (!_loaded)
            {
                lock (_loadLock)
                {
                    if (!_loaded)
                    {
                        _cachedData = await LoadFileAsync(cancellationToken);
                        _loaded = true;
                    }
                }
            }

            return _cachedData;
        }

        private async Task<object?> LoadFileAsync(CancellationToken cancellationToken)
        {
            if (!File.Exists(_filePath))
                throw new FileNotFoundException($"File not found: {_filePath}");

            var content = await File.ReadAllTextAsync(_filePath, cancellationToken);

            return _format switch
            {
                FileFormat.Json => JsonSerializer.Deserialize<object>(content),
                FileFormat.Xml => XDocument.Parse(content),
                FileFormat.Yaml => YamlDeserializer.Deserialize(content),  // Requires YAML library
                _ => throw new NotSupportedException($"Format {_format} not supported")
            };
        }

        public bool CanProvide(string path) => true;

        public int Priority => _priority;

        public ICanonicalSchema? SchemaMetadata => null;
    }

    public enum FileFormat
    {
        Json,
        Xml,
        Yaml
    }
}
```

---

## Exception Types

```csharp
namespace OoBDev.Framework.Data
{
    /// <summary>
    /// Exception thrown when a data provider fails.
    /// </summary>
    public class DataProviderException : Exception
    {
        /// <summary>
        /// Path that caused the error.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Provider that threw the exception.
        /// </summary>
        public IDataProvider? Provider { get; }

        public DataProviderException(string message)
            : base(message)
        {
            Path = "";
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

    /// <summary>
    /// Exception thrown when provider registration fails.
    /// </summary>
    public class ProviderRegistrationException : Exception
    {
        public string PathPattern { get; }

        public ProviderRegistrationException(string message, string pathPattern)
            : base(message)
        {
            PathPattern = pathPattern;
        }
    }
}
```

---

## Usage Examples

### Example 1: Static Data Provider

```csharp
var data = new
{
    Customer = new
    {
        Id = 1,
        Name = "John Doe",
        Email = "john@example.com"
    }
};

var container = DataContainerFactory.Create();
container.RegisterProvider("/", new StaticDataProvider(data));

// Navigate and access
var customer = await container.Navigate("Customer").GetValueAsync();
var name = await container.Navigate("Customer/Name").GetValueAsync<string>();

Console.WriteLine(name);  // "John Doe"
```

---

### Example 2: Database Provider with Context-Aware Query

```csharp
public class MyDbContext : DbContext
{
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
}

var container = DataContainerFactory.Create();

// Register customer provider
container.RegisterProvider("Customer", new DatabaseDataProvider<Customer>(
    dbContext,
    (query, node) => query.Where(c => c.Id == 1)  // Filter for specific customer
));

// Register orders provider with context-aware filter
container.RegisterProvider("Customer/Orders", new DatabaseDataProvider<Order>(
    dbContext,
    (query, node) =>
    {
        // Access parent customer node to get customer ID
        var customerId = (int?)node.Parent?.Value?.GetType().GetProperty("Id")?.GetValue(node.Parent.Value);
        return query.Where(o => o.CustomerId == customerId);
    }
));

// Navigate
var customer = await container.Navigate("Customer").GetValueAsync<Customer>();
var orders = await container.Navigate("Customer/Orders").GetValueAsync<List<Order>>();

Console.WriteLine($"Customer: {customer.Name}");
Console.WriteLine($"Orders: {orders.Count}");
```

---

### Example 3: API Provider

```csharp
var httpClient = new HttpClient();

var container = DataContainerFactory.Create();

// Register weather API provider
container.RegisterProvider("Weather/*", new ApiDataProvider(
    httpClient,
    "https://api.weather.com/v1/forecast",
    (baseUrl, node, context) =>
    {
        // Extract city from path
        var city = node.Path.Split('/').Last();
        return $"{baseUrl}/{city}";
    }
));

// Navigate and fetch
var seattleWeather = await container.Navigate("Weather/Seattle").GetValueAsync();
var nycWeather = await container.Navigate("Weather/NYC").GetValueAsync();
```

---

### Example 4: Configuration Provider

```csharp
// appsettings.json:
// {
//   "Database": {
//     "ConnectionString": "Server=localhost;Database=MyDb",
//     "Timeout": 30,
//     "RetryCount": 3
//   },
//   "Logging": {
//     "LogLevel": "Information"
//   }
// }

var container = DataContainerFactory.Create();
container.RegisterProvider("Config/**", new ConfigurationDataProvider(configuration));

// Access configuration values
var connectionString = await container.Navigate("Config/Database/ConnectionString").GetValueAsync<string>();
var timeout = await container.Navigate("Config/Database/Timeout").GetValueAsync<int>();
var logLevel = await container.Navigate("Config/Logging/LogLevel").GetValueAsync<string>();

Console.WriteLine($"Connection: {connectionString}");
Console.WriteLine($"Timeout: {timeout}");
Console.WriteLine($"Log Level: {logLevel}");
```

---

### Example 5: File Provider

```csharp
// customers.json:
// {
//   "customers": [
//     { "id": 1, "name": "Alice" },
//     { "id": 2, "name": "Bob" }
//   ]
// }

var container = DataContainerFactory.Create();
container.RegisterProvider("Data/**", new FileDataProvider(
    "customers.json",
    FileFormat.Json
));

var data = await container.Navigate("Data").GetValueAsync();
Console.WriteLine(data);
```

---

### Example 6: Delegate Provider

```csharp
var container = DataContainerFactory.Create();

// Simple delegate (no context)
container.RegisterProvider("Timestamp", new DelegateDataProvider(
    () => DateTime.UtcNow
));

// Async delegate with context
container.RegisterProvider("User/*", new DelegateDataProvider(
    async (node, path, context, ct) =>
    {
        var userId = path.Split('/').Last();
        return await _userService.GetUserAsync(int.Parse(userId), ct);
    }
));

// Sync delegate with context
container.RegisterProvider("Random", new DelegateDataProvider(
    (node, path, context) => new Random().Next(1, 100)
));

var timestamp = await container.Navigate("Timestamp").GetValueAsync<DateTime>();
var user = await container.Navigate("User/123").GetValueAsync<User>();
var random = await container.Navigate("Random").GetValueAsync<int>();
```

---

### Example 7: Multiple Providers with Priority

```csharp
var container = DataContainerFactory.Create();

// Default provider (low priority)
container.RegisterProvider("**", new StaticDataProvider(
    new { message = "Default data" }
));

// Specific provider for customers (higher priority)
container.RegisterProvider("Customer/**", new DatabaseDataProvider<Customer>(
    dbContext,
    priority: 10
));

// Even more specific provider for customer orders (highest priority)
container.RegisterProvider("Customer/Orders/**", new ApiDataProvider(
    httpClient,
    "https://api.orders.com",
    priority: 20
));

// "Customer/123" → DatabaseDataProvider (priority 10)
var customer = await container.Navigate("Customer/123").GetValueAsync<Customer>();

// "Customer/Orders/456" → ApiDataProvider (priority 20, most specific)
var order = await container.Navigate("Customer/Orders/456").GetValueAsync();

// "Something/Else" → StaticDataProvider (fallback)
var fallback = await container.Navigate("Something/Else").GetValueAsync();
```

---

### Example 8: Custom Provider

```csharp
/// <summary>
/// Custom provider that loads data from Redis cache.
/// </summary>
public class RedisDataProvider : IDataProvider
{
    private readonly IConnectionMultiplexer _redis;
    private readonly string _keyPrefix;

    public RedisDataProvider(IConnectionMultiplexer redis, string keyPrefix = "")
    {
        _redis = redis;
        _keyPrefix = keyPrefix;
    }

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string path,
        IDictionary<string, object?>? context,
        CancellationToken cancellationToken = default)
    {
        var db = _redis.GetDatabase();
        var key = string.IsNullOrEmpty(_keyPrefix)
            ? path.Replace("/", ":")
            : $"{_keyPrefix}:{path.Replace("/", ":")}";

        var value = await db.StringGetAsync(key);

        if (!value.HasValue)
            return null;

        // Deserialize JSON
        return JsonSerializer.Deserialize<object>(value.ToString());
    }

    public bool CanProvide(string path) => true;

    public int Priority => 0;

    public ICanonicalSchema? SchemaMetadata => null;
}

// Usage
var container = DataContainerFactory.Create();
container.RegisterProvider("Cache/**", new RedisDataProvider(redis, "myapp"));

var cachedData = await container.Navigate("Cache/User/123").GetValueAsync();
```

---

## Extension Methods

```csharp
namespace OoBDev.Framework.Data
{
    public static class DataProviderExtensions
    {
        /// <summary>
        /// Registers provider for exact path (no wildcards).
        /// </summary>
        public static void RegisterExact(
            this IDataContainer container,
            string exactPath,
            IDataProvider provider)
        {
            container.RegisterProvider(exactPath, provider);
        }

        /// <summary>
        /// Registers provider for all paths under prefix.
        /// </summary>
        public static void RegisterUnder(
            this IDataContainer container,
            string prefix,
            IDataProvider provider)
        {
            var pattern = prefix.TrimEnd('/') + "/**";
            container.RegisterProvider(pattern, provider);
        }

        /// <summary>
        /// Registers static data provider.
        /// </summary>
        public static void RegisterStatic(
            this IDataContainer container,
            string path,
            object? data)
        {
            container.RegisterProvider(path, new StaticDataProvider(data));
        }

        /// <summary>
        /// Registers delegate provider.
        /// </summary>
        public static void RegisterDelegate(
            this IDataContainer container,
            string path,
            Func<object?> provider)
        {
            container.RegisterProvider(path, new DelegateDataProvider(provider));
        }

        /// <summary>
        /// Registers async delegate provider.
        /// </summary>
        public static void RegisterDelegateAsync(
            this IDataContainer container,
            string path,
            Func<IDataNode, string, IDictionary<string, object?>?, CancellationToken, Task<object?>> provider)
        {
            container.RegisterProvider(path, new DelegateDataProvider(provider));
        }
    }
}
```

**Extension Usage:**
```csharp
var container = DataContainerFactory.Create();

// Simpler registration APIs
container.RegisterStatic("Config", new { timeout = 30 });
container.RegisterDelegate("Timestamp", () => DateTime.UtcNow);
container.RegisterUnder("Customer", new DatabaseDataProvider<Customer>(dbContext));
```

---

## Best Practices

1. **Use Specific Paths**: Prefer specific paths over broad wildcards for better performance
2. **Set Priorities**: Use priority for explicit provider selection when multiple match
3. **Cache Expensive Operations**: Cache file loads, API responses in provider implementation
4. **Context-Aware Queries**: Use parent node values to build filtered queries
5. **Error Handling**: Catch and wrap provider exceptions with meaningful context
6. **Async All the Way**: Always use async methods to avoid blocking

---

## Performance Considerations

- **Provider Lookup**: O(n) where n = number of registered providers
- **Lazy Loading**: First access incurs provider cost + caching overhead (< 10ms)
- **Cached Access**: Subsequent access is < 1ms (in-memory)
- **Wildcard Matching**: Regex compilation cached, minimal overhead

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container API](../CoreContainer/api-design.md)
