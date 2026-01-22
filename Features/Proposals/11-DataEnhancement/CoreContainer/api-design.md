# Core Container & Navigation - API Design

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Core Container & Navigation
**Last Updated:** 2026-01-22

---

## API Overview

The Core Container API provides three primary interfaces:
1. **IDataContainer** - Main entry point for data access
2. **IDataNode** - Navigator for tree traversal
3. **IDataProvider** - Extensibility point for data sources

---

## Core Interfaces

### IDataContainer

**Purpose:** Main entry point for creating and navigating data containers.

```csharp
namespace OoBDev.System.Data.Enhancement;

/// <summary>
/// Generic, lazy-evaluated data container with XPath-like navigation.
/// </summary>
public interface IDataContainer
{
    /// <summary>
    /// Gets the root node of the data tree.
    /// </summary>
    IDataNode Root { get; }

    /// <summary>
    /// Navigates to a node using XPath-like syntax.
    /// </summary>
    /// <param name="path">Path with / separators (e.g., "Customer/Address/City")</param>
    /// <returns>Data node at path (lazy - does NOT execute provider)</returns>
    /// <exception cref="ArgumentException">Invalid path syntax</exception>
    IDataNode Navigate(string path);

    /// <summary>
    /// Evaluates path and returns value.
    /// </summary>
    /// <param name="path">Path to evaluate</param>
    /// <returns>Value at path (triggers provider execution)</returns>
    object? Evaluate(string path);

    /// <summary>
    /// Evaluates path and returns strongly-typed value.
    /// </summary>
    /// <typeparam name="T">Expected type</typeparam>
    /// <param name="path">Path to evaluate</param>
    /// <returns>Typed value at path</returns>
    /// <exception cref="InvalidCastException">Value not convertible to T</exception>
    T? Evaluate<T>(string path);

    /// <summary>
    /// Registers data provider for path pattern.
    /// </summary>
    /// <param name="pathPattern">
    /// Path pattern (exact, wildcard *, or recursive **)
    /// Examples: "Customer", "Order/LineItems/*", "**/Address"
    /// </param>
    /// <param name="provider">Provider that fetches data for this pattern</param>
    void RegisterProvider(string pathPattern, IDataProvider provider);

    /// <summary>
    /// Removes all registered providers and cached nodes.
    /// </summary>
    void Clear();
}
```

---

### IDataNode

**Purpose:** Navigator pattern for tree traversal, similar to XPathNavigator.

```csharp
namespace OoBDev.System.Data.Enhancement;

/// <summary>
/// Represents a node in the data tree with lazy value loading.
/// </summary>
public interface IDataNode
{
    /// <summary>
    /// Gets the absolute path of this node (e.g., "/Customer/Address/City").
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Gets the name of this node (e.g., "City" for "/Customer/Address/City").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets the value at this node.
    /// TRIGGERS lazy loading if not already loaded.
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Gets the value as strongly-typed instance.
    /// </summary>
    /// <typeparam name="T">Expected type</typeparam>
    /// <returns>Typed value</returns>
    /// <exception cref="InvalidCastException">Value not convertible to T</exception>
    T? GetValue<T>();

    /// <summary>
    /// Selects single child node by relative path.
    /// </summary>
    /// <param name="relativePath">Path relative to this node (e.g., "Address/City")</param>
    /// <returns>Child node or null if not found</returns>
    IDataNode? SelectSingleNode(string relativePath);

    /// <summary>
    /// Selects multiple nodes matching pattern.
    /// </summary>
    /// <param name="pattern">
    /// Pattern with wildcards (e.g., "LineItems/*" for all line items)
    /// </param>
    /// <returns>Matching child nodes</returns>
    IEnumerable<IDataNode> SelectNodes(string pattern);

    /// <summary>
    /// Gets the parent node.
    /// </summary>
    IDataNode? Parent { get; }

    /// <summary>
    /// Gets all child nodes (lazy enumeration).
    /// </summary>
    IEnumerable<IDataNode> Children { get; }

    /// <summary>
    /// Checks if this node has child nodes.
    /// Does NOT trigger value loading.
    /// </summary>
    bool HasChildren { get; }

    /// <summary>
    /// Gets the depth of this node in the tree (root = 0).
    /// </summary>
    int Depth { get; }

    /// <summary>
    /// Checks if value has been loaded.
    /// </summary>
    bool IsLoaded { get; }
}
```

---

### IDataProvider

**Purpose:** Extensibility point for pluggable data sources.

```csharp
namespace OoBDev.System.Data.Enhancement;

/// <summary>
/// Provides data for a specific path in the data tree.
/// Implementations fetch from database, API, configuration, etc.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Provides data for the given node.
    /// Called lazily when node value is accessed.
    /// </summary>
    /// <param name="node">Node requesting data</param>
    /// <param name="context">Context path (same as node.Path)</param>
    /// <param name="metadata">Optional metadata for provider (e.g., IDs, filters)</param>
    /// <returns>Data object for this node</returns>
    Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata);
}
```

---

## Factory & Builder

### DataContainerFactory

**Purpose:** Fluent API for creating and configuring containers.

```csharp
namespace OoBDev.System.Data.Enhancement;

/// <summary>
/// Factory for creating IDataContainer instances.
/// </summary>
public static class DataContainerFactory
{
    /// <summary>
    /// Creates empty data container.
    /// </summary>
    public static IDataContainer Create()
    {
        return new DataContainer();
    }

    /// <summary>
    /// Creates container with initial data at root.
    /// </summary>
    /// <param name="initialData">Initial data object</param>
    public static IDataContainer Create(object initialData)
    {
        var container = new DataContainer();
        container.RegisterProvider("/", new StaticDataProvider(initialData));
        return container;
    }

    /// <summary>
    /// Creates container with configuration action.
    /// </summary>
    /// <param name="configure">Configuration delegate</param>
    public static IDataContainer Create(Action<IDataContainer> configure)
    {
        var container = new DataContainer();
        configure(container);
        return container;
    }
}

/// <summary>
/// Builder for fluent container configuration.
/// </summary>
public class DataContainerBuilder
{
    private readonly IDataContainer _container;

    public DataContainerBuilder()
    {
        _container = DataContainerFactory.Create();
    }

    /// <summary>
    /// Adds provider for path pattern.
    /// </summary>
    public DataContainerBuilder WithProvider(string pathPattern, IDataProvider provider)
    {
        _container.RegisterProvider(pathPattern, provider);
        return this;
    }

    /// <summary>
    /// Adds static data provider.
    /// </summary>
    public DataContainerBuilder WithData(string path, object data)
    {
        _container.RegisterProvider(path, new StaticDataProvider(data));
        return this;
    }

    /// <summary>
    /// Builds and returns configured container.
    /// </summary>
    public IDataContainer Build()
    {
        return _container;
    }
}
```

---

## Built-in Providers

### StaticDataProvider

**Purpose:** Provider for static in-memory data.

```csharp
namespace OoBDev.System.Data.Enhancement.Providers;

/// <summary>
/// Provides static data (no external calls).
/// </summary>
public class StaticDataProvider : IDataProvider
{
    private readonly object? _data;

    public StaticDataProvider(object? data)
    {
        _data = data;
    }

    public Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata)
    {
        return Task.FromResult(_data);
    }
}
```

### DelegateDataProvider

**Purpose:** Provider using delegate for lazy computation.

```csharp
namespace OoBDev.System.Data.Enhancement.Providers;

/// <summary>
/// Provides data using delegate.
/// </summary>
public class DelegateDataProvider : IDataProvider
{
    private readonly Func<IDataNode, string, IDictionary<string, object?>?, Task<object?>> _provider;

    public DelegateDataProvider(Func<IDataNode, string, IDictionary<string, object?>?, Task<object?>> provider)
    {
        _provider = provider;
    }

    public DelegateDataProvider(Func<Task<object?>> provider)
    {
        _provider = (_, _, _) => provider();
    }

    public Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata)
    {
        return _provider(node, context, metadata);
    }
}
```

---

## Usage Examples

### Example 1: Basic Navigation

```csharp
using OoBDev.System.Data.Enhancement;

// Create container with initial data
var data = new
{
    Customer = new
    {
        FirstName = "John",
        LastName = "Doe",
        Address = new
        {
            Street = "123 Main St",
            City = "Springfield",
            State = "IL",
            Zip = "62701"
        }
    }
};

var container = DataContainerFactory.Create(data);

// Navigate to city
var cityNode = container.Navigate("Customer/Address/City");
Console.WriteLine(cityNode.Value);  // "Springfield"

// Evaluate path directly
var state = container.Evaluate<string>("Customer/Address/State");
Console.WriteLine(state);  // "IL"
```

---

### Example 2: Provider Registration

```csharp
// Create empty container
var container = DataContainerFactory.Create();

// Register customer provider
container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
{
    // Fetch from database
    var customer = await _customerRepo.GetByIdAsync(customerId);
    return customer;
}));

// Register order provider
container.RegisterProvider("Order", new DelegateDataProvider(async () =>
{
    var order = await _orderRepo.GetByIdAsync(orderId);
    return order;
}));

// Navigate (providers NOT executed yet)
var customerNode = container.Navigate("Customer");
var orderNode = container.Navigate("Order");

// Access values (triggers providers NOW)
var customerName = customerNode.SelectSingleNode("FirstName")?.Value;
var orderTotal = orderNode.SelectSingleNode("Total")?.Value;
```

---

### Example 3: Lazy Evaluation

```csharp
// Register providers
container.RegisterProvider("Customer", expensiveCustomerProvider);
container.RegisterProvider("Order", expensiveOrderProvider);
container.RegisterProvider("Inventory", expensiveInventoryProvider);

// Template uses ONLY Customer data
var template = "Hello {{Customer/FirstName}}!";

// Apply template
var result = await _templateEngine.ApplyAsync(template, container);

// Result: ONLY expensiveCustomerProvider executed
// Order and Inventory providers NEVER called (50-70% query reduction)
```

---

### Example 4: Wildcard Selection

```csharp
var data = new
{
    Order = new
    {
        OrderNumber = "12345",
        LineItems = new[]
        {
            new { ProductName = "Widget", Quantity = 2, Price = 19.99m },
            new { ProductName = "Gadget", Quantity = 1, Price = 29.99m },
            new { ProductName = "Doohickey", Quantity = 3, Price = 9.99m }
        }
    }
};

var container = DataContainerFactory.Create(data);

// Select all line items using wildcard
var lineItemNodes = container.Navigate("Order/LineItems").SelectNodes("*");

foreach (var item in lineItemNodes)
{
    var productName = item.SelectSingleNode("ProductName")?.Value;
    var price = item.SelectSingleNode("Price")?.GetValue<decimal>();
    Console.WriteLine($"{productName}: ${price}");
}

// Output:
// Widget: $19.99
// Gadget: $29.99
// Doohickey: $9.99
```

---

### Example 5: Builder Pattern

```csharp
var container = new DataContainerBuilder()
    .WithData("Customer", new
    {
        FirstName = "John",
        LastName = "Doe"
    })
    .WithProvider("Order", new DelegateDataProvider(async () =>
        await _orderRepo.GetByIdAsync(orderId)))
    .WithProvider("**/Address", new AddressProvider(_addressService))
    .Build();

// Use container
var firstName = container.Evaluate<string>("Customer/FirstName");
var order = container.Evaluate("Order");
```

---

### Example 6: Custom Provider

```csharp
/// <summary>
/// Provider that fetches customer from database.
/// </summary>
public class CustomerDatabaseProvider : IDataProvider
{
    private readonly ICustomerRepository _repository;
    private readonly ILogger<CustomerDatabaseProvider> _logger;

    public CustomerDatabaseProvider(
        ICustomerRepository repository,
        ILogger<CustomerDatabaseProvider> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<object?> ProvideAsync(
        IDataNode node,
        string context,
        IDictionary<string, object?>? metadata)
    {
        // Get customer ID from metadata
        if (metadata == null || !metadata.TryGetValue("CustomerId", out var customerIdObj))
        {
            _logger.LogWarning("CustomerId not found in metadata for path {Path}", context);
            return null;
        }

        var customerId = Convert.ToInt32(customerIdObj);

        _logger.LogDebug("Fetching customer {CustomerId} for path {Path}", customerId, context);

        // Fetch from database
        var customer = await _repository.GetByIdAsync(customerId);

        if (customer == null)
        {
            _logger.LogWarning("Customer {CustomerId} not found", customerId);
            return null;
        }

        return customer;
    }
}

// Usage with metadata
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", new CustomerDatabaseProvider(_repo, _logger));

var metadata = new Dictionary<string, object?>
{
    ["CustomerId"] = 123
};

var customerNode = container.Navigate("Customer");
// Pass metadata when accessing value
// (Note: This requires provider to support metadata - implementation detail)
var customer = customerNode.Value;
```

---

### Example 7: Recursive Descent

```csharp
var data = new
{
    Order = new
    {
        Customer = new
        {
            Name = "John Doe",
            Address = new
            {
                Street = "123 Main St",
                City = "Springfield"
            }
        },
        ShippingAddress = new
        {
            Street = "456 Oak Ave",
            City = "Shelbyville"
        }
    }
};

var container = DataContainerFactory.Create(data);

// Find all "Address" nodes at any depth using recursive descent
container.RegisterProvider("**/Address", new AddressValidationProvider());

// Access triggers provider for ALL matching paths:
// - Order/Customer/Address
// - Order/ShippingAddress (if named "Address")
```

---

## Extension Methods

### Evaluation Extensions

```csharp
namespace OoBDev.System.Data.Enhancement;

public static class DataContainerExtensions
{
    /// <summary>
    /// Evaluates multiple paths and returns dictionary.
    /// </summary>
    public static IDictionary<string, object?> EvaluateMany(
        this IDataContainer container,
        params string[] paths)
    {
        var results = new Dictionary<string, object?>();
        foreach (var path in paths)
        {
            results[path] = container.Evaluate(path);
        }
        return results;
    }

    /// <summary>
    /// Checks if path exists (has value).
    /// </summary>
    public static bool Exists(this IDataContainer container, string path)
    {
        try
        {
            var node = container.Navigate(path);
            return node.Value != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Gets value with fallback default.
    /// </summary>
    public static T? GetValueOrDefault<T>(
        this IDataContainer container,
        string path,
        T? defaultValue = default)
    {
        try
        {
            return container.Evaluate<T>(path) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }
}

public static class DataNodeExtensions
{
    /// <summary>
    /// Converts node value to dictionary.
    /// </summary>
    public static IDictionary<string, object?> ToDictionary(this IDataNode node)
    {
        var dict = new Dictionary<string, object?>();

        foreach (var child in node.Children)
        {
            dict[child.Name] = child.Value;
        }

        return dict;
    }

    /// <summary>
    /// Checks if node has value (triggers loading).
    /// </summary>
    public static bool HasValue(this IDataNode node)
    {
        return node.Value != null;
    }
}
```

---

## Error Handling

### Exception Types

```csharp
namespace OoBDev.System.Data.Enhancement;

/// <summary>
/// Base exception for data container errors.
/// </summary>
public class DataContainerException : Exception
{
    public string? Path { get; }

    public DataContainerException(string message, string? path = null)
        : base(message)
    {
        Path = path;
    }

    public DataContainerException(string message, Exception innerException, string? path = null)
        : base(message, innerException)
    {
        Path = path;
    }
}

/// <summary>
/// Exception thrown when data provider fails.
/// </summary>
public class DataProviderException : DataContainerException
{
    public DataProviderException(string message, Exception innerException, string? path = null)
        : base(message, innerException, path)
    {
    }
}

/// <summary>
/// Exception thrown for invalid path syntax.
/// </summary>
public class InvalidPathException : DataContainerException
{
    public InvalidPathException(string message, string path)
        : base(message, path)
    {
    }
}
```

### Error Handling Example

```csharp
try
{
    var container = DataContainerFactory.Create();
    container.RegisterProvider("Customer", new CustomerDatabaseProvider(_repo));

    var customer = container.Evaluate("Customer");
}
catch (DataProviderException ex)
{
    _logger.LogError(ex, "Failed to load customer from path {Path}", ex.Path);
    // Handle provider failure
}
catch (InvalidPathException ex)
{
    _logger.LogError(ex, "Invalid path syntax: {Path}", ex.Path);
    // Handle invalid path
}
catch (DataContainerException ex)
{
    _logger.LogError(ex, "Data container error at path {Path}", ex.Path);
    // Handle general error
}
```

---

## Best Practices

### 1. Provider Design
```csharp
// ✅ GOOD: Provider is stateless and thread-safe
public class GoodProvider : IDataProvider
{
    private readonly IRepository _repository;  // Injected dependency

    public async Task<object?> ProvideAsync(...)
    {
        // Fetch data using injected service
        return await _repository.GetByIdAsync(id);
    }
}

// ❌ BAD: Provider has mutable state
public class BadProvider : IDataProvider
{
    private object? _cachedData;  // Don't cache in provider!

    public async Task<object?> ProvideAsync(...)
    {
        if (_cachedData == null)  // Race condition!
        {
            _cachedData = await FetchData();
        }
        return _cachedData;
    }
}
```

### 2. Path Patterns
```csharp
// ✅ GOOD: Specific path patterns
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order/LineItems/*", lineItemProvider);

// ❌ BAD: Overly broad patterns (performance impact)
container.RegisterProvider("**/*", everythingProvider);  // Matches EVERYTHING!
```

### 3. Lazy Evaluation
```csharp
// ✅ GOOD: Navigate without triggering load
var node = container.Navigate("ExpensiveData");
if (needsData)
{
    var value = node.Value;  // Load only when needed
}

// ❌ BAD: Always loading data even if not needed
var value = container.Evaluate("ExpensiveData");  // Always loads!
if (needsData)
{
    UseData(value);
}
```

### 4. Container Lifetime
```csharp
// ✅ GOOD: Container per request/context
public async Task ProcessOrder(int orderId)
{
    var container = DataContainerFactory.Create();
    container.RegisterProvider("Order", new OrderProvider(orderId));
    // Use container for this request only
}

// ❌ BAD: Shared container with stale cache
private static readonly IDataContainer _sharedContainer = DataContainerFactory.Create();
```

---

## Performance Considerations

### Query Reduction
```csharp
// Without lazy evaluation: 3 provider executions
var customer = await _customerRepo.GetByIdAsync(customerId);  // Query 1
var order = await _orderRepo.GetByIdAsync(orderId);          // Query 2
var inventory = await _inventoryService.GetStockAsync();     // Query 3

// With lazy evaluation: 1 provider execution (template uses ONLY Customer)
container.RegisterProvider("Customer", customerProvider);    // Registered
container.RegisterProvider("Order", orderProvider);          // Registered
container.RegisterProvider("Inventory", inventoryProvider);  // Registered

var template = "Hello {{Customer/FirstName}}!";
var result = await _templateEngine.ApplyAsync(template, container);
// Only customerProvider executed (66% query reduction)
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Path Translation API](../PathTranslation/api-design.md)
