# Data Container Integration - API Design

**Epic:** 10 - Text Templating Extensions
**Feature:** Data Container Integration
**Last Updated:** 2026-01-22

---

## API Overview

The Data Container Integration API provides:
1. **IDataContainerAdapter** - Adapter interface for converting IDataContainer to template-compatible objects
2. **DefaultDataContainerAdapter** - Default implementation using DynamicObject
3. **DiagnosticDataContainerAdapter** - Diagnostic implementation with logging
4. **TemplateEngine Extensions** - Automatic IDataContainer detection

---

## Core Interfaces

### IDataContainerAdapter

**Purpose:** Adapter abstraction for converting IDataContainer to template provider-compatible objects.

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Adapts IDataContainer to object graph compatible with template providers.
/// Preserves lazy evaluation semantics.
/// </summary>
public interface IDataContainerAdapter
{
    /// <summary>
    /// Adapts container to template-provider-compatible object.
    /// </summary>
    /// <param name="container">Data container with registered providers</param>
    /// <returns>
    /// Object that template providers can consume (typically DynamicObject or Dictionary)
    /// </returns>
    object Adapt(IDataContainer container);
}
```

---

### DefaultDataContainerAdapter

**Purpose:** Default adapter implementation using DynamicObject for lazy path evaluation.

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Default adapter using DynamicObject for dynamic member access.
/// </summary>
public class DefaultDataContainerAdapter : IDataContainerAdapter
{
    /// <summary>
    /// Adapts container to DataContainerProxy.
    /// </summary>
    public object Adapt(IDataContainer container)
    {
        return new DataContainerProxy(container);
    }
}
```

---

### DataContainerProxy (Internal)

**Purpose:** Dynamic proxy that intercepts member access and delegates to IDataContainer.

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Dynamic proxy that evaluates IDataContainer paths on member access.
/// INTERNAL - not exposed to consumers.
/// </summary>
internal class DataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly string _basePath;
    private readonly Dictionary<string, object?> _cache;

    public DataContainerProxy(IDataContainer container, string basePath = "")
    {
        _container = container;
        _basePath = basePath;
        _cache = new Dictionary<string, object?>();
    }

    /// <summary>
    /// Intercepts member access (e.g., proxy.Customer).
    /// </summary>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var memberName = binder.Name;
        var path = BuildPath(memberName);

        // Check cache
        if (_cache.TryGetValue(path, out result))
        {
            return true;
        }

        // Evaluate path (triggers provider)
        var value = _container.Evaluate(path);

        // Return nested proxy for complex objects
        if (IsComplexObject(value))
        {
            result = new DataContainerProxy(_container, path);
            _cache[path] = result;
            return true;
        }

        // Cache and return primitive value
        result = value;
        _cache[path] = result;
        return true;
    }

    /// <summary>
    /// Intercepts index access (e.g., proxy.Orders[0]).
    /// </summary>
    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        var index = indexes[0].ToString();
        var path = BuildPath(index);

        var value = _container.Evaluate(path);

        if (IsComplexObject(value))
        {
            result = new DataContainerProxy(_container, path);
            return true;
        }

        result = value;
        return true;
    }

    /// <summary>
    /// Intercepts conversion operations (e.g., (string)proxy).
    /// </summary>
    public override bool TryConvert(ConvertBinder binder, out object? result)
    {
        if (binder.Type == typeof(string))
        {
            result = _container.Evaluate(_basePath)?.ToString();
            return true;
        }

        result = null;
        return false;
    }

    private string BuildPath(string segment)
    {
        return string.IsNullOrEmpty(_basePath)
            ? segment
            : $"{_basePath}/{segment}";
    }

    private static bool IsComplexObject(object? obj)
    {
        if (obj == null) return false;

        var type = obj.GetType();

        // Primitives are NOT complex
        if (type.IsPrimitive || type.IsValueType)
            return false;

        // Strings and dates are NOT complex
        if (type == typeof(string) || type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return false;

        // Everything else IS complex
        return true;
    }
}
```

---

## Advanced Adapters

### DiagnosticDataContainerAdapter

**Purpose:** Diagnostic adapter that logs all path accesses and timing.

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Diagnostic adapter that tracks and logs all path accesses.
/// Use for debugging and optimization.
/// </summary>
public class DiagnosticDataContainerAdapter : IDataContainerAdapter
{
    private readonly ILogger<DiagnosticDataContainerAdapter> _logger;

    public DiagnosticDataContainerAdapter(ILogger<DiagnosticDataContainerAdapter> logger)
    {
        _logger = logger;
    }

    public object Adapt(IDataContainer container)
    {
        return new DiagnosticDataContainerProxy(container, _logger);
    }
}

/// <summary>
/// Diagnostic proxy that logs path access and timing.
/// </summary>
internal class DiagnosticDataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly ILogger _logger;
    private readonly string _basePath;
    private readonly HashSet<string> _accessedPaths;
    private readonly Stopwatch _totalStopwatch;

    public DiagnosticDataContainerProxy(
        IDataContainer container,
        ILogger logger,
        string basePath = "")
    {
        _container = container;
        _logger = logger;
        _basePath = basePath;
        _accessedPaths = new HashSet<string>();
        _totalStopwatch = Stopwatch.StartNew();
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var memberName = binder.Name;
        var path = string.IsNullOrEmpty(_basePath)
            ? memberName
            : $"{_basePath}/{memberName}";

        // Log first access
        if (!_accessedPaths.Contains(path))
        {
            _logger.LogDebug("Template accessed path: {Path} at {ElapsedMs}ms",
                path, _totalStopwatch.ElapsedMilliseconds);
            _accessedPaths.Add(path);
        }

        // Evaluate with timing
        var stopwatch = Stopwatch.StartNew();
        var value = _container.Evaluate(path);
        stopwatch.Stop();

        _logger.LogDebug("Path {Path} evaluated in {ElapsedMs}ms (type: {ValueType})",
            path, stopwatch.ElapsedMilliseconds, value?.GetType().Name ?? "null");

        // Return nested proxy for complex objects
        if (IsComplexObject(value))
        {
            result = new DiagnosticDataContainerProxy(_container, _logger, path);
            return true;
        }

        result = value;
        return true;
    }

    private static bool IsComplexObject(object? obj)
    {
        if (obj == null) return false;
        var type = obj.GetType();
        return !type.IsPrimitive
            && !type.IsValueType
            && type != typeof(string)
            && type != typeof(DateTime);
    }
}
```

---

## Dependency Injection Extensions

```csharp
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Dependency injection extensions for data container integration.
/// </summary>
public static class DataContainerTemplatingServiceCollectionExtensions
{
    /// <summary>
    /// Adds default data container adapter.
    /// </summary>
    public static IServiceCollection AddDataContainerTemplating(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IDataContainerAdapter, DefaultDataContainerAdapter>();
        return services;
    }

    /// <summary>
    /// Adds diagnostic data container adapter (logs all path accesses).
    /// </summary>
    public static IServiceCollection AddDiagnosticDataContainerTemplating(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IDataContainerAdapter, DiagnosticDataContainerAdapter>();
        return services;
    }

    /// <summary>
    /// Adds custom data container adapter.
    /// </summary>
    public static IServiceCollection AddDataContainerTemplating<TAdapter>(
        this IServiceCollection services)
        where TAdapter : class, IDataContainerAdapter
    {
        services.TryAddSingleton<IDataContainerAdapter, TAdapter>();
        return services;
    }
}
```

---

## Usage Examples

### Example 1: Basic Integration

```csharp
using OoBDev.System.Data.Enhancement;
using OoBDev.System.Text.Templating;

// Create data container
var container = DataContainerFactory.Create();

// Register providers (NOT executed yet)
container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
{
    return await _customerRepo.GetByIdAsync(customerId);
}));

container.RegisterProvider("Order", new DelegateDataProvider(async () =>
{
    return await _orderRepo.GetByIdAsync(orderId);
}));

// Handlebars template
var template = @"
Hello {{Customer/FirstName}} {{Customer/LastName}}!

Your order #{{Order/OrderNumber}} is confirmed.
";

// Render (ONLY customerProvider and orderProvider execute)
var result = await _templateEngine.ApplyAsync("order-confirmation", container);

// Output:
// Hello John Doe!
// Your order #12345 is confirmed.
```

---

### Example 2: Dependency Injection Setup

```csharp
// Startup.cs / Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Add template engine
    services.AddTemplateEngine();

    // Add Handlebars provider
    services.AddHandlebarsTemplateProvider();

    // Add data container integration
    services.AddDataContainerTemplating();  // Default adapter

    // Or use diagnostic adapter
    // services.AddDiagnosticDataContainerTemplating();
}

// Controller / Service
public class EmailService
{
    private readonly ITemplateEngine _templateEngine;
    private readonly ICustomerRepository _customerRepo;

    public EmailService(
        ITemplateEngine templateEngine,
        ICustomerRepository customerRepo)
    {
        _templateEngine = templateEngine;
        _customerRepo = customerRepo;
    }

    public async Task<string> GenerateWelcomeEmailAsync(int customerId)
    {
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            return await _customerRepo.GetByIdAsync(customerId);
        }));

        return await _templateEngine.ApplyAsync("welcome-email", container);
    }
}
```

---

### Example 3: Diagnostic Mode

```csharp
// Enable diagnostic logging
services.AddDiagnosticDataContainerTemplating();

// Use template engine (logs all path accesses)
var container = DataContainerFactory.Create();

container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Inventory", inventoryProvider);

var result = await _templateEngine.ApplyAsync("complex-template", container);

// Logs output:
// [Debug] Template accessed path: Customer at 5ms
// [Debug] Path Customer evaluated in 120ms (type: CustomerDto)
// [Debug] Template accessed path: Customer/FirstName at 125ms
// [Debug] Path Customer/FirstName evaluated in 0ms (type: String)
// [Debug] Template accessed path: Order at 125ms
// [Debug] Path Order evaluated in 85ms (type: OrderDto)
// [Debug] Template accessed path: Order/OrderNumber at 210ms
// [Debug] Path Order/OrderNumber evaluated in 0ms (type: String)
// NOTE: Inventory provider NEVER accessed (not used by template)
```

---

### Example 4: Performance Comparison

```csharp
// BEFORE: Eager loading (load ALL data)
var customer = await _customerRepo.GetByIdAsync(customerId);
var order = await _orderRepo.GetByIdAsync(orderId);
var inventory = await _inventoryService.GetInventoryAsync(productIds);
var shipping = await _shippingService.GetShippingAsync(orderId);

var data = new
{
    Customer = customer,
    Order = order,
    Inventory = inventory,
    Shipping = shipping
};

var result = await _templateEngine.ApplyAsync("welcome", data);
// Time: ~500ms (4 queries, template uses 2)

// AFTER: Lazy loading (load ONLY what template uses)
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Inventory", inventoryProvider);
container.RegisterProvider("Shipping", shippingProvider);

var result = await _templateEngine.ApplyAsync("welcome", container);
// Time: ~200ms (2 queries, template uses 2)
// 60% faster, 50% fewer queries
```

---

### Example 5: XSLT Integration

```csharp
// IDataContainer works with XSLT templates too!

var container = DataContainerFactory.Create();
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Customer", customerProvider);

// XSLT template with XPath syntax (already uses /)
var xsltTemplate = @"
<xsl:stylesheet version='1.0'>
  <xsl:template match='/'>
    <invoice>
      <customer><xsl:value-of select='Customer/FirstName'/></customer>
      <order><xsl:value-of select='Order/OrderNumber'/></order>
    </invoice>
  </xsl:template>
</xsl:stylesheet>
";

// Render (adapter converts container to XML-compatible format)
var result = await _templateEngine.ApplyAsync("invoice", container);
```

---

### Example 6: Nested Object Navigation

```csharp
// Data structure:
// Customer
//   └─ Address
//        ├─ Street
//        ├─ City
//        └─ State

var container = DataContainerFactory.Create();

container.RegisterProvider("Customer", new StaticDataProvider(new
{
    FirstName = "John",
    LastName = "Doe",
    Address = new
    {
        Street = "123 Main St",
        City = "Springfield",
        State = "IL"
    }
}));

// Template accesses nested paths
var template = @"
{{Customer/FirstName}} {{Customer/LastName}}
{{Customer/Address/Street}}
{{Customer/Address/City}}, {{Customer/Address/State}}
";

var result = await _templateEngine.ApplyAsync("address-label", container);

// Output:
// John Doe
// 123 Main St
// Springfield, IL
```

---

### Example 7: Array Access

```csharp
// Data structure:
// Order
//   └─ LineItems (array)
//        ├─ [0] { ProductName, Quantity, Price }
//        ├─ [1] { ProductName, Quantity, Price }
//        └─ [2] { ProductName, Quantity, Price }

var container = DataContainerFactory.Create();

container.RegisterProvider("Order", new StaticDataProvider(new
{
    OrderNumber = "12345",
    LineItems = new[]
    {
        new { ProductName = "Widget", Quantity = 2, Price = 19.99m },
        new { ProductName = "Gadget", Quantity = 1, Price = 29.99m },
        new { ProductName = "Doohickey", Quantity = 3, Price = 9.99m }
    }
}));

// Handlebars template with array iteration
var template = @"
Order #{{Order/OrderNumber}}

Line Items:
{{#each Order/LineItems}}
- {{ProductName}}: {{Quantity}} x ${{Price}}
{{/each}}
";

var result = await _templateEngine.ApplyAsync("order-summary", container);

// Output:
// Order #12345
//
// Line Items:
// - Widget: 2 x $19.99
// - Gadget: 1 x $29.99
// - Doohickey: 3 x $9.99
```

---

### Example 8: Conditional Paths

```csharp
var container = DataContainerFactory.Create();

container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Premium", premiumDataProvider);  // Expensive query

// Template conditionally accesses Premium data
var template = @"
Hello {{Customer/FirstName}}!

{{#if Customer/IsPremium}}
Welcome back, premium member!
Your benefits: {{Premium/Benefits}}
{{else}}
Upgrade to premium for exclusive benefits!
{{/if}}
";

// If Customer/IsPremium is false, Premium provider NEVER executes
var result = await _templateEngine.ApplyAsync("welcome", container);
```

---

### Example 9: Custom Adapter

```csharp
// Custom adapter with additional features
public class CachedDataContainerAdapter : IDataContainerAdapter
{
    private readonly IMemoryCache _cache;

    public CachedDataContainerAdapter(IMemoryCache cache)
    {
        _cache = cache;
    }

    public object Adapt(IDataContainer container)
    {
        return new CachedDataContainerProxy(container, _cache);
    }
}

// Register custom adapter
services.AddDataContainerTemplating<CachedDataContainerAdapter>();
```

---

## Error Handling

### Exception Types

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Base exception for data container adapter errors.
/// </summary>
public class DataContainerAdapterException : TemplateException
{
    public string? Path { get; }

    public DataContainerAdapterException(string message, string? path = null)
        : base(message)
    {
        Path = path;
    }

    public DataContainerAdapterException(string message, Exception innerException, string? path = null)
        : base(message, innerException)
    {
        Path = path;
    }
}

/// <summary>
/// Exception thrown when path evaluation fails.
/// </summary>
public class PathEvaluationException : DataContainerAdapterException
{
    public PathEvaluationException(string path, Exception innerException)
        : base($"Failed to evaluate path: {path}", innerException, path)
    {
    }
}
```

### Error Handling Example

```csharp
try
{
    var result = await _templateEngine.ApplyAsync("template", container);
}
catch (PathEvaluationException ex)
{
    _logger.LogError(ex, "Failed to evaluate path {Path}", ex.Path);
    // Handle missing data
}
catch (DataContainerAdapterException ex)
{
    _logger.LogError(ex, "Data container adapter error");
    // Handle adapter issues
}
```

---

## Best Practices

### 1. Provider Registration
```csharp
// ✅ GOOD: Register providers before passing to template
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);

await _templateEngine.ApplyAsync("template", container);

// ❌ BAD: Pass empty container
var container = DataContainerFactory.Create();
await _templateEngine.ApplyAsync("template", container);  // No data!
```

### 2. Path Syntax
```csharp
// ✅ GOOD: Use forward slashes
var template = "{{Customer/Address/City}}";

// ❌ BAD: Use dots (won't work with container paths)
var template = "{{Customer.Address.City}}";  // Provider won't match
```

### 3. Provider Scope
```csharp
// ✅ GOOD: Create new container per request
public async Task<string> RenderAsync()
{
    var container = DataContainerFactory.Create();
    container.RegisterProvider("Customer", customerProvider);
    return await _templateEngine.ApplyAsync("template", container);
}

// ❌ BAD: Reuse container across requests (not thread-safe)
private IDataContainer _container = DataContainerFactory.Create();  // NOT SAFE!
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 10 Overview](../README-REVISED.md)
- [Epic 11: Core Container API](../../11-DataEnhancement/CoreContainer/api-design.md)
