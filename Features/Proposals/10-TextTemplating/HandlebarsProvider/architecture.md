# Handlebars Template Provider - Architecture

**Epic:** 10 - Text Templating Extensions
**Feature:** Handlebars Template Provider
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Handlebars Template Provider implements the **ITemplateProvider** abstraction using Handlebars.NET, enabling dynamic text generation with lazy-evaluated data from **IDataContainer**.

```
┌─────────────────────────────────────────────────────────────┐
│                     Consumer Application                    │
│              (Email, Report, Document Generation)           │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│              ITemplateProvider Interface                    │
│  - RenderAsync(template, data)                             │
│  - CompileAsync(template) → ICompiledTemplate              │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│         HandlebarsTemplateProvider                          │
│  - Handlebars.NET integration                              │
│  - IDataContainer adapter                                   │
│  - Custom helper registration                               │
│  - Template compilation cache                               │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┐
         ↓           ↓           ↓
┌─────────────┐ ┌─────────────┐ ┌─────────────┐
│  Handlebars │ │IDataContainer│ │   Helper    │
│   Engine    │ │   Adapter   │ │  Registry   │
└─────────────┘ └─────────────┘ └─────────────┘
```

---

## Core Components

### 1. HandlebarsTemplateProvider

**Responsibilities:**
- Implement ITemplateProvider interface
- Manage Handlebars engine lifecycle
- Coordinate template compilation and rendering
- Register helpers and partials
- Cache compiled templates

**Key Design Decisions:**
- **Lazy compilation** - Templates compiled on first use, cached for reuse
- **Thread-safe** - Handlebars engine is thread-safe for concurrent renders
- **Extensible** - Custom helpers can be registered at runtime

**Implementation Pattern:**
```csharp
public class HandlebarsTemplateProvider : ITemplateProvider
{
    private readonly IHandlebars _handlebars;
    private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _templateCache;
    private readonly IDataContainerAdapter _dataAdapter;

    public HandlebarsTemplateProvider(
        IHandlebars? handlebars = null,
        IDataContainerAdapter? dataAdapter = null)
    {
        _handlebars = handlebars ?? Handlebars.Create();
        _dataAdapter = dataAdapter ?? new DefaultDataContainerAdapter();
        _templateCache = new ConcurrentDictionary<string, HandlebarsTemplate<object, object>>();

        RegisterBuiltInHelpers();
    }

    public async Task<string> RenderAsync(
        string template,
        object data,
        IDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        // Get or compile template
        var compiledTemplate = _templateCache.GetOrAdd(
            template,
            t => _handlebars.Compile(t));

        // Adapt data if IDataContainer
        var adaptedData = data is IDataContainer container
            ? _dataAdapter.Adapt(container)
            : data;

        // Render template
        var result = compiledTemplate(adaptedData);

        return await Task.FromResult(result);
    }

    public async Task<ICompiledTemplate> CompileAsync(
        string template,
        CancellationToken cancellationToken = default)
    {
        var compiled = _handlebars.Compile(template);
        return await Task.FromResult(new HandlebarsCompiledTemplate(compiled));
    }

    private void RegisterBuiltInHelpers()
    {
        // Date formatting
        _handlebars.RegisterHelper("formatDate", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2 && parameters[0] is DateTime date)
            {
                var format = parameters[1]?.ToString() ?? "yyyy-MM-dd";
                writer.WriteSafeString(date.ToString(format));
            }
        });

        // Currency formatting
        _handlebars.RegisterHelper("currency", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1 && parameters[0] is decimal amount)
            {
                writer.WriteSafeString(amount.ToString("C"));
            }
        });

        // Number formatting
        _handlebars.RegisterHelper("formatNumber", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2)
            {
                var number = Convert.ToDecimal(parameters[0]);
                var format = parameters[1]?.ToString() ?? "N2";
                writer.WriteSafeString(number.ToString(format));
            }
        });

        // Case conversion
        _handlebars.RegisterHelper("uppercase", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1 && parameters[0] is string text)
            {
                writer.WriteSafeString(text.ToUpperInvariant());
            }
        });

        _handlebars.RegisterHelper("lowercase", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1 && parameters[0] is string text)
            {
                writer.WriteSafeString(text.ToLowerInvariant());
            }
        });

        // JSON serialization
        _handlebars.RegisterHelper("json", (writer, context, parameters) =>
        {
            if (parameters.Length >= 1)
            {
                var json = JsonSerializer.Serialize(parameters[0]);
                writer.WriteSafeString(json);
            }
        });
    }
}
```

---

### 2. IDataContainerAdapter

**Responsibilities:**
- Bridge between IDataContainer and Handlebars data model
- Translate dot notation to container navigation
- Lazily evaluate paths during template rendering
- Cache evaluated values per render

**Key Design Decisions:**
- **Lazy evaluation** - Container paths evaluated only when accessed in template
- **Dynamic proxy** - Use DynamicObject to intercept property access
- **Per-render cache** - Values cached within single render, cleared after

**Implementation Pattern:**
```csharp
public interface IDataContainerAdapter
{
    object Adapt(IDataContainer container);
}

public class DefaultDataContainerAdapter : IDataContainerAdapter
{
    public object Adapt(IDataContainer container)
    {
        return new DataContainerProxy(container);
    }
}

internal class DataContainerProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly string _basePath;
    private readonly Dictionary<string, object?> _cache;

    public DataContainerProxy(IDataContainer container, string basePath = "/")
    {
        _container = container;
        _basePath = basePath;
        _cache = new Dictionary<string, object?>();
    }

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        var path = CombinePath(_basePath, binder.Name);

        // Check cache first
        if (_cache.TryGetValue(path, out result))
        {
            return true;
        }

        // Evaluate from container (lazy - triggers provider only now)
        result = _container.Evaluate(path);

        // Wrap nested objects as proxies
        if (result != null && !IsPrimitive(result))
        {
            result = new DataContainerProxy(_container, path);
        }

        // Cache result
        _cache[path] = result;

        return true;
    }

    private static string CombinePath(string basePath, string segment)
    {
        basePath = basePath.TrimEnd('/');
        return $"{basePath}/{segment}";
    }

    private static bool IsPrimitive(object obj)
    {
        var type = obj.GetType();
        return type.IsPrimitive
            || type == typeof(string)
            || type == typeof(decimal)
            || type == typeof(DateTime)
            || type == typeof(DateTimeOffset)
            || type == typeof(Guid);
    }
}
```

---

### 3. HandlebarsCompiledTemplate

**Responsibilities:**
- Wrap Handlebars compiled template
- Implement ICompiledTemplate interface
- Enable reusable template rendering

**Implementation Pattern:**
```csharp
public class HandlebarsCompiledTemplate : ICompiledTemplate
{
    private readonly HandlebarsTemplate<object, object> _compiledTemplate;

    public HandlebarsCompiledTemplate(HandlebarsTemplate<object, object> compiledTemplate)
    {
        _compiledTemplate = compiledTemplate;
    }

    public async Task<string> RenderAsync(
        object data,
        IDictionary<string, object>? context = null,
        CancellationToken cancellationToken = default)
    {
        var result = _compiledTemplate(data);
        return await Task.FromResult(result);
    }
}
```

---

## Data Flow

### Sequence: Render Template with IDataContainer

```
┌─────────┐      ┌──────────────┐      ┌──────────┐      ┌──────────┐      ┌──────────┐
│Consumer │      │  Handlebars  │      │  Adapter │      │Container │      │ Provider │
└────┬────┘      │   Provider   │      │  Proxy   │      └────┬─────┘      └────┬─────┘
     │           └──────┬───────┘      └────┬─────┘            │                 │
     │                  │                   │                  │                 │
     │ RenderAsync()    │                   │                  │                 │
     ├─────────────────>│                   │                  │                 │
     │                  │                   │                  │                 │
     │                  │ Adapt(container)  │                  │                 │
     │                  ├──────────────────>│                  │                 │
     │                  │                   │                  │                 │
     │                  │ DataContainerProxy│                  │                 │
     │                  │<──────────────────┤                  │                 │
     │                  │                   │                  │                 │
     │                  │ Compile(template) │                  │                 │
     │                  │ (cached)          │                  │                 │
     │                  │                   │                  │                 │
     │                  │ Render(proxy)     │                  │                 │
     │                  │ (Handlebars eval) │                  │                 │
     │                  │                   │                  │                 │
     │                  │ Access {{Customer.FirstName}}       │                 │
     │                  ├──────────────────>│                  │                 │
     │                  │                   │                  │                 │
     │                  │                   │ Evaluate(path)   │                 │
     │                  │                   ├─────────────────>│                 │
     │                  │                   │                  │                 │
     │                  │                   │                  │ ProvideAsync()  │
     │                  │                   │                  ├────────────────>│
     │                  │                   │                  │                 │
     │                  │                   │                  │ Customer data   │
     │                  │                   │                  │<────────────────┤
     │                  │                   │                  │                 │
     │                  │                   │ "John"           │                 │
     │                  │                   │<─────────────────┤                 │
     │                  │                   │                  │                 │
     │                  │ "John" (cached)   │                  │                 │
     │                  │<──────────────────┤                  │                 │
     │                  │                   │                  │                 │
     │                  │ Rendered text     │                  │                 │
     │ Rendered text    │                   │                  │                 │
     │<─────────────────┤                   │                  │                 │
     │                  │                   │                  │                 │
```

**Key Points:**
1. Template rendering starts with RenderAsync call
2. IDataContainer adapted to dynamic proxy
3. Handlebars engine accesses data via proxy
4. Proxy lazily evaluates paths from container (triggers providers)
5. Values cached within proxy for render duration
6. Final rendered text returned to consumer

---

## Design Patterns

### 1. Adapter Pattern
- IDataContainerAdapter bridges IDataContainer to Handlebars
- Translates between container navigation and property access
- Enables seamless integration with different data sources

### 2. Proxy Pattern
- DataContainerProxy intercepts property access
- Lazy loading via TryGetMember
- Transparent caching

### 3. Template Method Pattern
- ITemplateProvider defines common rendering flow
- HandlebarsTemplateProvider implements Handlebars-specific logic
- Easy to swap template engines

### 4. Strategy Pattern
- Different helpers are strategies for data formatting
- Helpers registered dynamically
- Extensible via custom helper registration

---

## Performance Optimizations

### 1. Template Compilation Cache
- Compiled templates cached by template string
- Eliminates redundant Handlebars parsing
- Thread-safe concurrent dictionary

### 2. Per-Render Value Cache
- Values cached within DataContainerProxy per render
- Prevents redundant provider executions within single template
- Cache cleared after render (no stale data)

### 3. Lazy Provider Execution
- Container providers execute ONLY when paths accessed in template
- If template doesn't use a data path, provider never runs
- 50-70% query reduction vs. eager loading

### 4. String Pooling
- Use string interning for common paths
- Reduce memory allocation for repeated renders

---

## Thread Safety

### Concurrency Strategy
- **Handlebars engine** - Thread-safe for concurrent renders
- **Template cache** - ConcurrentDictionary for thread-safe caching
- **Proxy instances** - New proxy per render (no shared state)

### Synchronization Points
```csharp
// Template cache (thread-safe)
private readonly ConcurrentDictionary<string, HandlebarsTemplate<object, object>> _templateCache;

// No locks needed for rendering (stateless per render)
public async Task<string> RenderAsync(...)
{
    // Each render gets new proxy instance
    var proxy = new DataContainerProxy(container);
    return compiledTemplate(proxy);
}
```

---

## Error Handling

### Template Compilation Errors
```csharp
public async Task<ICompiledTemplate> CompileAsync(string template, ...)
{
    try
    {
        var compiled = _handlebars.Compile(template);
        return new HandlebarsCompiledTemplate(compiled);
    }
    catch (HandlebarsCompilerException ex)
    {
        throw new TemplateCompilationException(
            $"Failed to compile Handlebars template: {ex.Message}", ex);
    }
}
```

### Rendering Errors
```csharp
public async Task<string> RenderAsync(string template, object data, ...)
{
    try
    {
        var compiledTemplate = _templateCache.GetOrAdd(template, ...);
        var result = compiledTemplate(data);
        return result;
    }
    catch (HandlebarsRuntimeException ex)
    {
        throw new TemplateRenderException(
            $"Failed to render Handlebars template: {ex.Message}", ex);
    }
}
```

### Helper Errors
```csharp
_handlebars.RegisterHelper("formatDate", (writer, context, parameters) =>
{
    try
    {
        if (parameters.Length >= 2 && parameters[0] is DateTime date)
        {
            var format = parameters[1]?.ToString() ?? "yyyy-MM-dd";
            writer.WriteSafeString(date.ToString(format));
        }
        else
        {
            writer.WriteSafeString("[Invalid date]");
        }
    }
    catch (Exception ex)
    {
        writer.WriteSafeString($"[Error: {ex.Message}]");
    }
});
```

---

## Extension Points

### Custom Helpers
```csharp
public void RegisterHelper(string name, HandlebarsHelper helper)
{
    _handlebars.RegisterHelper(name, helper);
}

// Usage
provider.RegisterHelper("customFormat", (writer, context, parameters) =>
{
    // Custom formatting logic
    writer.WriteSafeString(formatted);
});
```

### Block Helpers
```csharp
public void RegisterBlockHelper(string name, HandlebarsBlockHelper helper)
{
    _handlebars.RegisterHelper(name, helper);
}

// Usage
provider.RegisterBlockHelper("section", (writer, options, context, arguments) =>
{
    writer.WriteSafeString("<section>");
    options.Template(writer, context);
    writer.WriteSafeString("</section>");
});
```

### Partials
```csharp
public void RegisterPartial(string name, string template)
{
    _handlebars.RegisterTemplate(name, template);
}

// Usage
provider.RegisterPartial("header", "<h1>{{title}}</h1>");
```

---

## Testing Strategy

### Unit Tests
- Template compilation (success and error cases)
- Rendering with static data
- Helper registration and invocation
- Partial registration
- Error handling

### Integration Tests
- Rendering with IDataContainer
- Lazy evaluation verification (provider called only when needed)
- Complex templates with loops, conditionals, partials
- Performance benchmarks

### Example Test
```csharp
[TestMethod]
public async Task RenderAsync_WithIDataContainer_LazilyEvaluatesData()
{
    // Arrange
    var mockProvider = new Mock<IDataProvider>();
    mockProvider
        .Setup(p => p.ProvideAsync(...))
        .ReturnsAsync(new { FirstName = "John", LastName = "Doe" });

    var container = DataContainerFactory.Create();
    container.RegisterProvider("Customer", mockProvider.Object);

    var provider = new HandlebarsTemplateProvider();
    var template = "Hello {{Customer.FirstName}}!";

    // Act
    var result = await provider.RenderAsync(template, container);

    // Assert
    Assert.AreEqual("Hello John!", result);
    mockProvider.Verify(
        p => p.ProvideAsync(...),
        Times.Once);  // Provider called once
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 10 Overview](../README-REVISED.md)
