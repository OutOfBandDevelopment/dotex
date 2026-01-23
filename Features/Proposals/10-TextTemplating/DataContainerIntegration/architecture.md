# Data Container Integration - Architecture

**Epic:** 10 - Text Templating Extensions
**Feature:** Data Container Integration
**Last Updated:** 2026-01-22

---

## Architectural Overview

Data Container Integration provides a bridge between IDataContainer (Epic 11) and template providers, enabling lazy evaluation where templates only trigger provider execution for paths they actually access.

```
┌──────────────────────────────────────────────────────────────┐
│                    Template Engine                            │
│  - ApplyAsync(templateName, IDataContainer)                   │
│  - Detects IDataContainer vs. POCO                            │
└────────────────┬──────────────────────────────────────────────┘
                 ↓
      ┌──────────────────────────┐
      │  IDataContainerAdapter   │
      │  - Adapt(container)      │
      └──────────┬───────────────┘
                 ↓
      ┌──────────────────────────┐
      │  DataContainerProxy      │
      │  - DynamicObject         │
      │  - Lazy path evaluation  │
      └──────────┬───────────────┘
                 ↓
      ┌──────────────────────────┐
      │     IDataContainer       │
      │  - Evaluate(path)        │
      │  - Triggers providers    │
      └──────────────────────────┘
```

**Key Principle:** Transparent adapter that preserves lazy evaluation while making IDataContainer work with any template provider.

---

## Core Components

### 1. IDataContainerAdapter (Abstraction)

**Responsibilities:**
- Convert IDataContainer to template-provider-compatible object
- Preserve lazy evaluation semantics
- Support multiple adapter implementations (diagnostic, cached, etc.)

**Design Pattern:** Adapter Pattern

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Adapts IDataContainer to template-provider-compatible object.
/// </summary>
public interface IDataContainerAdapter
{
    /// <summary>
    /// Adapts container to object graph that template providers can consume.
    /// </summary>
    /// <param name="container">Data container with registered providers</param>
    /// <returns>Adapted object (typically DynamicObject or Dictionary)</returns>
    object Adapt(IDataContainer container);
}
```

---

### 2. DefaultDataContainerAdapter (Default Implementation)

**Responsibilities:**
- Create DataContainerProxy for dynamic member access
- Handle basic path-to-member resolution
- Support nested object navigation

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Default adapter using DynamicObject for lazy path evaluation.
/// </summary>
public class DefaultDataContainerAdapter : IDataContainerAdapter
{
    public object Adapt(IDataContainer container)
    {
        return new DataContainerProxy(container);
    }
}
```

---

### 3. DataContainerProxy (Dynamic Proxy)

**Responsibilities:**
- Implement DynamicObject for dynamic member access
- Resolve template paths to container paths
- Trigger lazy evaluation on path access
- Handle nested objects and arrays

**Design Pattern:** Proxy Pattern + Dynamic Object

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Dynamic proxy that evaluates IDataContainer paths on member access.
/// </summary>
internal class DataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly string _basePath;
    private readonly Dictionary<string, object?> _cache = new();

    public DataContainerProxy(IDataContainer container, string basePath = "")
    {
        _container = container;
        _basePath = basePath;
    }

    /// <summary>
    /// Handles member access (e.g., proxy.Customer.FirstName).
    /// </summary>
    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var memberName = binder.Name;
        var path = BuildPath(memberName);

        // Check cache first
        if (_cache.TryGetValue(path, out result))
        {
            return true;
        }

        // Evaluate path (triggers provider execution)
        var value = _container.Evaluate(path);

        // If value is complex, return nested proxy for further navigation
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
    /// Handles index access (e.g., proxy.Orders[0]).
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
    /// Handles conversion to string (for debugging).
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

        // Primitives and strings are NOT complex
        if (type.IsPrimitive || type.IsValueType || type == typeof(string))
            return false;

        // DateTime is NOT complex
        if (type == typeof(DateTime) || type == typeof(DateTimeOffset))
            return false;

        // Dictionaries and objects ARE complex
        return true;
    }
}
```

---

### 4. TemplateEngine Integration

**Responsibilities:**
- Detect IDataContainer vs. POCO objects
- Adapt IDataContainer before passing to provider
- Maintain backward compatibility with POCO objects

**Implementation:**

```csharp
namespace OoBDev.System.Text.Templating;

/// <summary>
/// Template engine with automatic IDataContainer support.
/// </summary>
public class TemplateEngine : ITemplateEngine
{
    private readonly IDataContainerAdapter _dataAdapter;
    private readonly IEnumerable<ITemplateProvider> _providers;
    private readonly IEnumerable<ITemplateSource> _sources;
    private readonly ILogger<TemplateEngine> _logger;

    public TemplateEngine(
        IDataContainerAdapter dataAdapter,
        IEnumerable<ITemplateProvider> providers,
        IEnumerable<ITemplateSource> sources,
        ILogger<TemplateEngine> logger)
    {
        _dataAdapter = dataAdapter;
        _providers = providers;
        _sources = sources;
        _logger = logger;
    }

    public async Task<string?> ApplyAsync(string templateName, object data)
    {
        using var stream = new MemoryStream();
        var context = await ApplyAsync(templateName, data, stream);

        if (context == null)
            return null;

        stream.Position = 0;
        using var reader = new StreamReader(stream);
        return await reader.ReadToEndAsync();
    }

    public async Task<ITemplateContext?> ApplyAsync(
        string templateName,
        object data,
        Stream target)
    {
        var context = GetTemplateContext(templateName);

        if (context == null)
        {
            _logger.LogWarning("Template {TemplateName} not found", templateName);
            return null;
        }

        var provider = GetProvider(context);

        if (provider == null)
        {
            _logger.LogWarning("No provider found for content type {ContentType}",
                context.ContentType);
            return null;
        }

        // Adapt IDataContainer if needed
        var adaptedData = data is IDataContainer container
            ? _dataAdapter.Adapt(container)
            : data;

        _logger.LogDebug("Applying template {TemplateName} with provider {ProviderType}",
            templateName, provider.GetType().Name);

        await provider.ApplyAsync(context, adaptedData, target);

        return context;
    }

    private ITemplateContext? GetTemplateContext(string templateName)
    {
        foreach (var source in _sources)
        {
            var context = source.GetTemplate(templateName);
            if (context != null)
                return context;
        }

        return null;
    }

    private ITemplateProvider? GetProvider(ITemplateContext context)
    {
        return _providers.FirstOrDefault(p => p.CanApply(context));
    }
}
```

---

## Data Flow

### Lazy Evaluation Flow

```
┌────────────────────────────────────────────────────────────┐
│  1. Template Rendering Started                             │
│     _templateEngine.ApplyAsync("welcome", container)       │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  2. Detect IDataContainer                                  │
│     if (data is IDataContainer container)                  │
│         adaptedData = _dataAdapter.Adapt(container)        │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  3. Create DataContainerProxy                              │
│     new DataContainerProxy(container)                      │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  4. Template Accesses Path                                 │
│     Template: "{{Customer/FirstName}}"                     │
│     Handlebars calls: proxy.Customer.FirstName             │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  5. TryGetMember("Customer")                               │
│     path = "Customer"                                      │
│     value = _container.Evaluate("Customer")                │
│     return new DataContainerProxy(_container, "Customer")  │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  6. IDataContainer.Evaluate("Customer")                    │
│     Provider executes NOW (lazy evaluation triggered)      │
│     customerData = await customerProvider.ProvideAsync()   │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  7. TryGetMember("FirstName")                              │
│     path = "Customer/FirstName"                            │
│     value = _container.Evaluate("Customer/FirstName")      │
│     return "John" (primitive value)                        │
└────────────────┬───────────────────────────────────────────┘
                 ↓
┌────────────────────────────────────────────────────────────┐
│  8. Template Rendering Complete                            │
│     Result: "Hello John!"                                  │
│     Providers executed: 1 (Customer only)                  │
│     Providers skipped: 2 (Order, Inventory never accessed) │
└────────────────────────────────────────────────────────────┘
```

---

## Design Patterns

### 1. Adapter Pattern
- `IDataContainerAdapter` abstracts adaptation logic
- Multiple implementations possible (default, diagnostic, cached)
- Template engine agnostic to adapter implementation

### 2. Proxy Pattern
- `DataContainerProxy` intercepts member access
- Delegates to IDataContainer for actual evaluation
- Transparent to template providers

### 3. Strategy Pattern
- Template engine uses adapter strategy
- Adapter can be swapped (default, diagnostic, optimized)
- No changes to template engine or providers

---

## Advanced Features

### 1. Diagnostic Adapter

**Purpose:** Track which paths templates access for optimization.

```csharp
namespace OoBDev.System.Text.Templating.Data;

/// <summary>
/// Diagnostic adapter that logs all path accesses.
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

internal class DiagnosticDataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly ILogger _logger;
    private readonly string _basePath;
    private readonly HashSet<string> _accessedPaths = new();
    private readonly Stopwatch _totalStopwatch = Stopwatch.StartNew();

    public DiagnosticDataContainerProxy(
        IDataContainer container,
        ILogger logger,
        string basePath = "")
    {
        _container = container;
        _logger = logger;
        _basePath = basePath;
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var memberName = binder.Name;
        var path = string.IsNullOrEmpty(_basePath)
            ? memberName
            : $"{_basePath}/{memberName}";

        // Log first access to path
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

        _logger.LogDebug("Path {Path} evaluated in {ElapsedMs}ms (value type: {ValueType})",
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

### 2. Cached Adapter

**Purpose:** Add caching layer for repeated template executions.

```csharp
/// <summary>
/// Cached adapter that stores evaluation results.
/// </summary>
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

internal class CachedDataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly IMemoryCache _cache;
    private readonly string _basePath;

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var path = string.IsNullOrEmpty(_basePath)
            ? binder.Name
            : $"{_basePath}/{binder.Name}";

        var cacheKey = $"path:{_container.GetHashCode()}:{path}";

        result = _cache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return _container.Evaluate(path);
        });

        if (IsComplexObject(result))
        {
            result = new CachedDataContainerProxy(_container, _cache, path);
        }

        return true;
    }

    private static bool IsComplexObject(object? obj)
    {
        if (obj == null) return false;
        var type = obj.GetType();
        return !type.IsPrimitive && !type.IsValueType && type != typeof(string);
    }
}
```

---

## Performance Optimizations

### 1. Path Caching
```csharp
// Cache evaluated paths within proxy instance
private readonly Dictionary<string, object?> _cache = new();

public override bool TryGetMember(GetMemberBinder binder, out object? result)
{
    var path = BuildPath(binder.Name);

    if (_cache.TryGetValue(path, out result))
    {
        return true;  // Cache hit - no provider execution
    }

    result = _container.Evaluate(path);  // Provider executes
    _cache[path] = result;  // Cache for future accesses
    return true;
}
```

### 2. Lazy Proxy Creation
```csharp
// Only create nested proxies when needed
if (IsComplexObject(value))
{
    result = new DataContainerProxy(_container, path);
    return true;
}

// Return primitive directly (no proxy overhead)
result = value;
return true;
```

---

## Error Handling

### Path Not Found
```csharp
public override bool TryGetMember(GetMemberBinder binder, out object? result)
{
    try
    {
        var path = BuildPath(binder.Name);
        result = _container.Evaluate(path);
        return true;
    }
    catch (DataProviderException ex)
    {
        _logger.LogWarning(ex, "Failed to evaluate path: {Path}", path);
        result = null;
        return false;  // Return false to signal path not found
    }
}
```

### Provider Execution Errors
```csharp
// Container handles provider errors
public object? Evaluate(string path)
{
    try
    {
        var provider = GetProvider(path);
        return provider.ProvideAsync(...).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Provider failed for path {Path}", path);
        throw new DataProviderException($"Failed to evaluate path: {path}", ex);
    }
}
```

---

## Thread Safety

### Proxy Thread Safety
```csharp
// Each proxy instance is NOT thread-safe
// Use separate container instances per request

// ✅ GOOD: Separate containers per request
public async Task<string> RenderEmailAsync(int orderId)
{
    var container = DataContainerFactory.Create();  // New instance
    container.RegisterProvider("Order", orderProvider);

    return await _templateEngine.ApplyAsync("order-email", container);
}

// ❌ BAD: Shared container across requests
private static readonly IDataContainer _sharedContainer = /* ... */;  // NOT SAFE
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 10 Overview](../README-REVISED.md)
- [Epic 11: Data Enhancement](../../11-DataEnhancement/README-REVISED.md)
