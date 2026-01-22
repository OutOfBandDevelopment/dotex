# Data Container Integration - Requirements

**Epic:** 10 - Text Templating Extensions
**Feature:** Data Container Integration
**Priority:** HIGH
**Complexity:** MEDIUM
**Estimated LOC:** ~100

---

## Overview

Bridge between IDataContainer (Epic 11) and template providers, enabling lazy evaluation where templates only load data they actually use. Provides massive performance improvements for scenarios with multiple data sources.

---

## Business Requirements

### BR-1: Lazy Evaluation Support
**As a** developer
**I want** templates to only load data they actually use
**So that** unnecessary database queries and API calls are eliminated

**Acceptance Criteria:**
- Data providers registered with IDataContainer
- Providers execute ONLY when template accesses their path
- Providers that are never accessed never execute
- 50-70% reduction in data loading for typical scenarios

**Example:**
```csharp
// Register 3 providers
container.RegisterProvider("Customer", customerProvider);  // Loads from DB
container.RegisterProvider("Order", orderProvider);        // Loads from DB
container.RegisterProvider("Inventory", inventoryProvider); // Loads from API

// Template uses ONLY Customer data
var template = "Hello {{Customer/FirstName}}!";

// ONLY customerProvider executes (orderProvider and inventoryProvider never run)
var result = await _templateEngine.ApplyAsync("welcome", container);
```

---

### BR-2: Seamless ITemplateEngine Integration
**As a** developer
**I want** IDataContainer to work with existing ITemplateEngine interface
**So that** I don't need to change how I use templates

**Acceptance Criteria:**
- IDataContainer works with all template providers (Handlebars, XSLT, etc.)
- No changes to ITemplateEngine or ITemplateProvider interfaces
- Adapter automatically detects IDataContainer vs. POCO objects
- Existing templates continue to work with POCO objects

---

### BR-3: Path Resolution
**As a** template provider
**I want** to access data using XPath-like paths
**So that** lazy evaluation can determine which providers to execute

**Acceptance Criteria:**
- Template paths (e.g., `Customer/FirstName`) map to container paths
- Nested paths resolved correctly
- Array indexing supported (e.g., `Orders/0/Total`)
- Providers execute at first access, cached for subsequent accesses

---

### BR-4: Provider Execution Tracking
**As a** developer
**I want** to see which providers executed during template rendering
**So that** I can optimize data loading strategies

**Acceptance Criteria:**
- Optional diagnostic mode tracks provider executions
- Logging shows which providers were called
- Performance metrics available (execution time per provider)
- Debug information helps identify inefficient templates

---

## Technical Requirements

### TR-1: DataContainerAdapter Interface

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Adapts IDataContainer to template-provider-compatible object.
/// </summary>
public interface IDataContainerAdapter
{
    /// <summary>
    /// Adapts container to object that template providers can use.
    /// Preserves lazy evaluation semantics.
    /// </summary>
    /// <param name="container">Data container with registered providers</param>
    /// <returns>Object compatible with template providers</returns>
    object Adapt(IDataContainer container);
}
```

---

### TR-2: Default Implementation

```csharp
/// <summary>
/// Default adapter using DynamicObject for dynamic member access.
/// </summary>
public class DefaultDataContainerAdapter : IDataContainerAdapter
{
    public object Adapt(IDataContainer container)
    {
        return new DataContainerProxy(container);
    }
}

/// <summary>
/// Dynamic proxy that evaluates paths on member access.
/// </summary>
internal class DataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly string _basePath;

    public DataContainerProxy(IDataContainer container, string basePath = "")
    {
        _container = container;
        _basePath = basePath;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var path = string.IsNullOrEmpty(_basePath)
            ? binder.Name
            : $"{_basePath}/{binder.Name}";

        // Evaluate path (triggers provider execution)
        var value = _container.Evaluate(path);

        // If value is complex object, return nested proxy
        if (value is IDictionary<string, object> || IsComplexObject(value))
        {
            result = new DataContainerProxy(_container, path);
            return true;
        }

        result = value;
        return true;
    }

    public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object? result)
    {
        var index = indexes[0].ToString();
        var path = $"{_basePath}/{index}";

        result = _container.Evaluate(path);
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

### TR-3: TemplateEngine Integration

```csharp
namespace OoBDev.System.Text.Templating;

/// <summary>
/// Template engine with IDataContainer support.
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly IDataContainerAdapter _dataAdapter;
    private readonly IEnumerable<ITemplateProvider> _providers;
    private readonly IEnumerable<ITemplateSource> _sources;

    public TemplateEngine(
        IDataContainerAdapter dataAdapter,
        IEnumerable<ITemplateProvider> providers,
        IEnumerable<ITemplateSource> sources)
    {
        _dataAdapter = dataAdapter;
        _providers = providers;
        _sources = sources;
    }

    public async Task<string?> ApplyAsync(string templateName, object data)
    {
        // Adapt IDataContainer if needed
        var adaptedData = data is IDataContainer container
            ? _dataAdapter.Adapt(container)
            : data;

        var context = GetTemplateContext(templateName);
        var provider = GetProvider(context);

        using var stream = new MemoryStream();
        await provider.ApplyAsync(context, adaptedData, stream);

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }
}
```

---

### TR-4: Provider Execution Diagnostics

```csharp
/// <summary>
/// Diagnostic adapter that tracks provider executions.
/// </summary>
public class DiagnosticDataContainerAdapter : IDataContainerAdapter
{
    private readonly ILogger<DiagnosticDataContainerAdapter> _logger;

    public object Adapt(IDataContainer container)
    {
        return new DiagnosticDataContainerProxy(container, _logger);
    }
}

internal class DiagnosticDataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly ILogger _logger;
    private readonly HashSet<string> _accessedPaths = new();

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var path = binder.Name;

        if (!_accessedPaths.Contains(path))
        {
            _logger.LogDebug("Template accessed path: {Path}", path);
            _accessedPaths.Add(path);
        }

        var stopwatch = Stopwatch.StartNew();
        var value = _container.Evaluate(path);
        stopwatch.Stop();

        _logger.LogDebug("Path {Path} evaluated in {ElapsedMs}ms", path, stopwatch.ElapsedMilliseconds);

        result = value;
        return true;
    }
}
```

---

## Non-Functional Requirements

### NFR-1: Performance
- Adapter overhead: < 1ms per path access
- No performance degradation vs. direct POCO access
- Lazy evaluation reduces total execution time by 50-70%
- Provider caching prevents duplicate executions

### NFR-2: Compatibility
- Works with Handlebars templates
- Works with XSLT templates
- Works with Liquid templates (future)
- Works with Scriban templates (future)

### NFR-3: Developer Experience
- Transparent - developers don't need to know about adapter
- Works with existing template syntax
- Clear error messages when paths not found
- Diagnostic mode for troubleshooting

---

## Constraints

### C-1: Path Syntax Compatibility
- Template paths must use `/` separator (XPath-like)
- Handlebars paths must be converted (`.` → `/`)
- XSLT paths already use `/` (no conversion needed)

### C-2: Dynamic Object Limitations
- C# `dynamic` keyword not required but supported
- Some template engines may not support DynamicObject
- Fallback to Dictionary<string, object> if needed

### C-3: Provider Constraints
- Providers must be thread-safe
- Provider results cached per container instance
- Providers cannot access other providers directly

---

## Success Criteria

- ✅ IDataContainer works with ITemplateEngine
- ✅ Lazy evaluation reduces provider executions by 50-70%
- ✅ Adapter overhead < 1ms per path access
- ✅ Works with Handlebars and XSLT providers
- ✅ Diagnostic mode tracks provider executions
- ✅ 80%+ test coverage
- ✅ No breaking changes to existing APIs

---

## Out of Scope

- ❌ LINQ integration (future enhancement)
- ❌ Template compilation optimizations
- ❌ Provider dependency management
- ❌ Advanced path expressions (use IDataContainer capabilities)

---

## Performance Comparison

### Before (Eager Loading)

```csharp
// Load ALL data upfront
var customer = await _customerRepo.GetByIdAsync(customerId);
var order = await _orderRepo.GetByIdAsync(orderId);
var inventory = await _inventoryService.GetInventoryAsync(productIds);
var shipping = await _shippingService.GetShippingAsync(orderId);
var payment = await _paymentService.GetPaymentAsync(orderId);

var data = new
{
    Customer = customer,
    Order = order,
    Inventory = inventory,
    Shipping = shipping,
    Payment = payment
};

// Template uses ONLY Customer.FirstName
var result = await _templateEngine.ApplyAsync("welcome-email", data);

// Performance: 5 database/API calls (500ms+)
```

### After (Lazy Loading)

```csharp
// Register providers (NO execution)
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Inventory", inventoryProvider);
container.RegisterProvider("Shipping", shippingProvider);
container.RegisterProvider("Payment", paymentProvider);

// Template uses ONLY Customer.FirstName
var result = await _templateEngine.ApplyAsync("welcome-email", container);

// Performance: 1 database call (100ms)
// 80% faster, 5x fewer queries
```

---

## Dependencies

### Internal
- **Epic 11: Data Enhancement Pipeline** - IDataContainer interface
- **OoBDev.System.Abstractions** - ITemplateEngine, ITemplateProvider

### External
- System.Dynamic.Runtime (DynamicObject)
- Microsoft.Extensions.Logging

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 10 Overview](../README-REVISED.md)
- [Epic 11: Data Enhancement](../../11-DataEnhancement/README-REVISED.md)
