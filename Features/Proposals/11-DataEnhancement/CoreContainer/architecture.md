# Core Container & Navigation - Architecture

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Core Container & Navigation
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Core Container implements a lazy-evaluated, XPath-like data navigation system using the **Navigator Pattern** combined with **Lazy Loading** and **Provider Pattern**.

```
┌─────────────────────────────────────────────────────────────┐
│                        Consumer                             │
│              (Templates, Reports, Services)                 │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│                   IDataContainer                            │
│  - Navigate(path) → IDataNode                              │
│  - Evaluate(path) → object?                                │
│  - RegisterProvider(pattern, provider)                     │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┐
         ↓           ↓           ↓
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│  IDataNode  │ │  IDataNode  │ │  IDataNode  │
│   /Customer │ │    /Order   │ │   /System   │
│             │ │             │ │             │
│ Value       │ │ Value       │ │ Value       │
│ (lazy)  ────┼─┤ (lazy)  ────┼─┤ (lazy)  ────┤
└─────────────┘ └─────────────┘ └─────────────┘
      │               │               │
      ↓               ↓               ↓
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│IDataProvider│ │IDataProvider│ │IDataProvider│
│  Customer   │ │    Order    │ │   System    │
└─────────────┘ └─────────────┘ └─────────────┘
      │               │               │
      ↓               ↓               ↓
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│  Database   │ │     API     │ │   Config    │
└─────────────┘ └─────────────┘ └─────────────┘
```

---

## Core Components

### 1. DataContainer (Main Entry Point)

**Responsibilities:**
- Root node management
- Provider registration and lookup
- Path navigation coordination
- Lazy node creation

**Key Design Decisions:**
- **Singleton per context** - Each container represents one data context
- **Thread-safe** - Concurrent reads supported
- **Lazy node tree** - Nodes created on-demand, not upfront

**Implementation Pattern:**
```csharp
public class DataContainer : IDataContainer
{
    private readonly DataNode _root;
    private readonly Dictionary<string, IDataProvider> _providers;
    private readonly Dictionary<string, DataNode> _nodeCache;
    private readonly object _lock = new object();

    public DataContainer()
    {
        _root = new DataNode(this, "/", "root", null);
        _providers = new Dictionary<string, IDataProvider>();
        _nodeCache = new Dictionary<string, DataNode>();
    }

    public IDataNode Navigate(string path)
    {
        // 1. Normalize path
        var normalizedPath = NormalizePath(path);

        // 2. Check cache
        lock (_lock)
        {
            if (_nodeCache.TryGetValue(normalizedPath, out var cachedNode))
                return cachedNode;
        }

        // 3. Create node (lazy - does NOT execute provider)
        var node = CreateNode(normalizedPath);

        // 4. Cache node
        lock (_lock)
        {
            _nodeCache[normalizedPath] = node;
        }

        return node;
    }

    public void RegisterProvider(string pathPattern, IDataProvider provider)
    {
        // Provider registered but NOT executed
        lock (_lock)
        {
            _providers[pathPattern] = provider;
        }
    }

    internal IDataProvider? FindProvider(string path)
    {
        // Match path to provider pattern (exact → wildcard → recursive)
        lock (_lock)
        {
            // 1. Exact match
            if (_providers.TryGetValue(path, out var exactProvider))
                return exactProvider;

            // 2. Wildcard match (e.g., "Order/LineItems/*" matches "Order/LineItems/0")
            foreach (var (pattern, provider) in _providers)
            {
                if (IsWildcardMatch(path, pattern))
                    return provider;
            }

            // 3. Recursive match (e.g., "**/Address" matches "Customer/Address")
            foreach (var (pattern, provider) in _providers)
            {
                if (IsRecursiveMatch(path, pattern))
                    return provider;
            }

            return null;
        }
    }
}
```

---

### 2. DataNode (Navigator Pattern)

**Responsibilities:**
- Path-based navigation
- Lazy value loading
- Parent/child relationships
- Value caching

**Key Design Decisions:**
- **Value is lazy** - Provider executes ONLY when `Value` property accessed
- **Immutable path** - Path never changes after creation
- **Cached value** - Provider executes once, result cached

**Implementation Pattern:**
```csharp
public class DataNode : IDataNode
{
    private readonly IDataContainer _container;
    private readonly string _path;
    private readonly string _name;
    private readonly DataNode? _parent;

    private object? _value;
    private bool _valueLoaded;
    private readonly object _valueLock = new object();

    public string Path => _path;
    public string Name => _name;

    public object? Value
    {
        get
        {
            // Double-check locking pattern for lazy initialization
            if (!_valueLoaded)
            {
                lock (_valueLock)
                {
                    if (!_valueLoaded)
                    {
                        // Find and execute provider
                        var provider = _container.FindProvider(_path);
                        if (provider != null)
                        {
                            _value = provider.ProvideAsync(this, _path, null).GetAwaiter().GetResult();
                        }
                        _valueLoaded = true;
                    }
                }
            }
            return _value;
        }
    }

    public IDataNode? SelectSingleNode(string relativePath)
    {
        // Combine current path with relative path
        var absolutePath = CombinePaths(_path, relativePath);
        return _container.Navigate(absolutePath);
    }

    public IEnumerable<IDataNode> SelectNodes(string pattern)
    {
        // Pattern matching for wildcards
        // e.g., "LineItems/*" returns all LineItems children
        var nodes = new List<IDataNode>();

        if (pattern.Contains("*"))
        {
            // Wildcard pattern - enumerate children
            foreach (var child in Children)
            {
                if (MatchesPattern(child.Name, pattern))
                {
                    nodes.Add(child);
                }
            }
        }
        else
        {
            // Single node
            var node = SelectSingleNode(pattern);
            if (node != null)
            {
                nodes.Add(node);
            }
        }

        return nodes;
    }

    public IDataNode? Parent => _parent;

    public IEnumerable<IDataNode> Children
    {
        get
        {
            // Lazy children discovery
            // Check if value is enumerable/object with properties
            if (_value is IEnumerable enumerable and not string)
            {
                int index = 0;
                foreach (var item in enumerable)
                {
                    yield return _container.Navigate($"{_path}/{index}");
                    index++;
                }
            }
            else if (_value != null)
            {
                // Reflect properties as children
                var properties = _value.GetType().GetProperties();
                foreach (var prop in properties)
                {
                    yield return _container.Navigate($"{_path}/{prop.Name}");
                }
            }
        }
    }
}
```

---

### 3. IDataProvider (Provider Pattern)

**Responsibilities:**
- Fetch data from source (database, API, config, etc.)
- Execute ONLY when triggered by node value access
- Return data for specific path

**Key Design Decisions:**
- **Async execution** - Providers are async by default
- **Context-aware** - Receive node and metadata
- **Stateless** - Providers should be thread-safe and stateless

**Implementation Examples:**

**Database Provider:**
```csharp
public class CustomerDataProvider : IDataProvider
{
    private readonly ICustomerRepository _repository;

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata)
    {
        // Extract customer ID from parent path or metadata
        var customerId = metadata?["CustomerId"] as int? ?? 0;

        if (customerId == 0)
        {
            // Parse from path: "/Order/123/Customer" → get order 123, then customer
            customerId = ParseCustomerIdFromPath(node.Path);
        }

        // Fetch customer from database
        var customer = await _repository.GetByIdAsync(customerId);

        return customer;
    }
}
```

**API Provider:**
```csharp
public class OrderDataProvider : IDataProvider
{
    private readonly HttpClient _httpClient;

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata)
    {
        var orderId = metadata?["OrderId"] as int? ?? 0;

        // Fetch from API
        var response = await _httpClient.GetAsync($"/api/orders/{orderId}");
        response.EnsureSuccessStatusCode();

        var order = await response.Content.ReadFromJsonAsync<Order>();
        return order;
    }
}
```

**Configuration Provider:**
```csharp
public class SystemConfigProvider : IDataProvider
{
    private readonly IConfiguration _configuration;

    public Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata)
    {
        // Provide configuration values
        var config = new
        {
            ApplicationName = _configuration["ApplicationName"],
            Version = _configuration["Version"],
            Environment = _configuration["Environment"]
        };

        return Task.FromResult<object?>(config);
    }
}
```

---

## Data Flow

### Sequence: Navigate and Evaluate

```
┌─────────┐         ┌──────────────┐         ┌──────────┐         ┌──────────┐
│Consumer │         │DataContainer │         │ DataNode │         │ Provider │
└────┬────┘         └──────┬───────┘         └────┬─────┘         └────┬─────┘
     │                     │                      │                     │
     │ Navigate("Order")   │                      │                     │
     ├────────────────────>│                      │                     │
     │                     │                      │                     │
     │                     │ CheckCache("Order")  │                     │
     │                     ├─────────────────────>│                     │
     │                     │                      │                     │
     │                     │ Cache miss           │                     │
     │                     │<─────────────────────┤                     │
     │                     │                      │                     │
     │                     │ CreateNode("Order")  │                     │
     │                     ├─────────────────────>│                     │
     │                     │                      │                     │
     │                     │ CacheNode("Order")   │                     │
     │                     │<─────────────────────┤                     │
     │                     │                      │                     │
     │ IDataNode (Order)   │                      │                     │
     │<────────────────────┤                      │                     │
     │                     │                      │                     │
     │ node.Value          │                      │                     │
     ├──────────────────────────────────────────>│                     │
     │                     │                      │                     │
     │                     │                      │ FindProvider(path)  │
     │                     │                      ├────────────────────>│
     │                     │                      │                     │
     │                     │                      │ Provider found      │
     │                     │                      │<────────────────────┤
     │                     │                      │                     │
     │                     │                      │ ProvideAsync()      │
     │                     │                      ├────────────────────>│
     │                     │                      │                     │
     │                     │                      │ (DB/API call)       │
     │                     │                      │                     │
     │                     │                      │ object (Order data) │
     │                     │                      │<────────────────────┤
     │                     │                      │                     │
     │                     │                      │ CacheValue(Order)   │
     │                     │                      │                     │
     │ Order object        │                      │                     │
     │<──────────────────────────────────────────┤                     │
     │                     │                      │                     │
```

**Key Points:**
1. Navigation creates node but does NOT execute provider
2. Value access triggers provider execution
3. Provider result cached in node
4. Subsequent Value accesses return cached result (no provider execution)

---

## Design Patterns

### 1. Navigator Pattern
- Similar to `XPathNavigator` in .NET
- Tree-based navigation via path expressions
- Parent/child relationships

### 2. Lazy Loading Pattern
- Value loaded on-demand
- Double-checked locking for thread safety
- Cache result after first load

### 3. Provider Pattern
- Pluggable data sources
- Pattern matching for provider selection
- Async execution

### 4. Composite Pattern
- Nodes form tree structure
- Children enumerated lazily
- Recursive descent supported

---

## Performance Optimizations

### 1. Node Caching
- Nodes cached by path in container
- Prevents duplicate node creation
- Memory trade-off for speed

### 2. Value Caching
- Provider result cached in node
- Eliminates redundant provider executions
- 50-70% query reduction

### 3. Lazy Children
- Children enumerated on-demand
- Iterator pattern avoids upfront allocation
- Memory efficient for large collections

### 4. Path Normalization
- Paths normalized once at navigation
- Cache key based on normalized path
- Consistent cache hit rate

---

## Thread Safety

### Concurrency Strategy
- **Read-heavy workload** - Multiple threads reading concurrently
- **Lock-free reads** - Cached nodes/values accessed without locks
- **Locks for writes** - Provider registration and cache updates use locks
- **Double-checked locking** - Value loading uses fine-grained locking

### Synchronization Points
```csharp
// Container-level lock (coarse)
private readonly object _lock = new object();

// Node-level lock (fine-grained)
private readonly object _valueLock = new object();
```

### Concurrent Access Pattern
```csharp
// Thread 1: Navigate to "Order"
var node1 = container.Navigate("Order");  // Creates node, caches

// Thread 2: Navigate to "Order" (concurrent)
var node2 = container.Navigate("Order");  // Cache hit, returns same node

// Thread 1: Access Value
var value1 = node1.Value;  // First access, provider executes

// Thread 2: Access Value (concurrent)
var value2 = node2.Value;  // Waits for Thread 1, then returns cached value

// Both get same value, provider executes once
Assert.That(value1, Is.SameAs(value2));
```

---

## Error Handling

### Provider Errors
```csharp
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
                    try
                    {
                        var provider = _container.FindProvider(_path);
                        if (provider != null)
                        {
                            _value = provider.ProvideAsync(this, _path, null)
                                .GetAwaiter().GetResult();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Wrap provider exception with context
                        throw new DataProviderException(
                            $"Provider failed for path '{_path}'", ex);
                    }
                    finally
                    {
                        _valueLoaded = true;  // Mark loaded even on error
                    }
                }
            }
        }
        return _value;
    }
}
```

### Path Errors
```csharp
public IDataNode Navigate(string path)
{
    // Validate path syntax
    if (string.IsNullOrWhiteSpace(path))
        throw new ArgumentException("Path cannot be null or empty", nameof(path));

    if (path.Contains("//"))
        throw new ArgumentException("Path cannot contain consecutive slashes", nameof(path));

    // ... rest of navigation logic
}
```

---

## Testing Strategy

### Unit Tests
- Mock providers for deterministic behavior
- Test lazy loading (provider called once)
- Test navigation (path parsing, wildcards)
- Test caching (repeated access no provider call)

### Integration Tests
- Real providers (in-memory database)
- Concurrent access scenarios
- Performance benchmarks (query reduction)

### Example Test
```csharp
[TestMethod]
public async Task Value_FirstAccess_ExecutesProvider()
{
    // Arrange
    var mockProvider = new Mock<IDataProvider>();
    mockProvider
        .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
        .ReturnsAsync(new { Name = "John" });

    var container = new DataContainer();
    container.RegisterProvider("Customer", mockProvider.Object);

    // Act
    var node = container.Navigate("Customer");
    var value1 = node.Value;  // First access
    var value2 = node.Value;  // Second access

    // Assert
    mockProvider.Verify(
        p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
        Times.Once);  // Provider called ONCE, not twice

    Assert.That(value1, Is.SameAs(value2));  // Same cached instance
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Path Translation Feature](../PathTranslation/architecture.md)
