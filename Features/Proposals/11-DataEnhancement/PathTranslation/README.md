# Feature: Path Syntax Translation

**Epic:** Data Enhancement Pipeline (Epic 11)
**Priority:** HIGH
**LOC:** ~300
**Purpose:** Modular translation between different path syntax types (XPath, JSONPath, dot notation, etc.)

---

## Overview

Different template engines and data consumers use different path syntaxes. This feature provides a **pluggable translation layer** that allows:

1. **Template engines to use their native syntax**
2. **Data providers to register with preferred syntax**
3. **Automatic translation between syntaxes**
4. **Extensible for new path syntaxes**

---

## Supported Path Syntaxes

### XPath Syntax
```
Customer/Address/City
Customer/Address/*
Customer/Orders/0/Total
**/LineItems
```

**Characteristics:**
- `/` separators
- Array indexing with `[0]` or `/0`
- Wildcards: `*` (single level), `**` (multiple levels)
- Absolute paths start with `/`

### JSONPath Syntax
```
$.Customer.Address.City
$.Customer.Address.*
$.Customer.Orders[0].Total
$..LineItems
```

**Characteristics:**
- `$.` prefix for root
- `.` separators
- Array indexing with `[0]`
- Recursive descent: `..` (like XPath `**`)

### Property Dot Notation
```
Customer.Address.City
Customer.Address.*
Customer.Orders.0.Total
**.LineItems
```

**Characteristics:**
- `.` separators (like C# properties)
- Array indexing with `.0` notation
- Wildcards: `*`, `**`
- No root prefix

### Custom Syntax (Extensible)
Template engines can define their own syntax (e.g., Handlebars `{{Customer/Name}}` vs Liquid `{{customer.name}}`).

---

## Architecture

### IPathTranslator - Main Abstraction

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Translates between different path syntax types.
/// </summary>
public interface IPathTranslator
{
    /// <summary>
    /// Path syntax type this translator handles (e.g., "xpath", "jsonpath", "dotnotation").
    /// </summary>
    string SyntaxType { get; }

    /// <summary>
    /// Parses a path in this syntax into a canonical form.
    /// </summary>
    /// <param name="path">Path in this syntax (e.g., "$.Customer.Address.City")</param>
    /// <returns>Canonical path representation</returns>
    ICanonicalPath Parse(string path);

    /// <summary>
    /// Formats a canonical path into this syntax.
    /// </summary>
    /// <param name="canonicalPath">Canonical path</param>
    /// <returns>Path in this syntax</returns>
    string Format(ICanonicalPath canonicalPath);

    /// <summary>
    /// Checks if a path string matches this syntax.
    /// </summary>
    bool CanParse(string path);
}
```

### ICanonicalPath - Internal Representation

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Canonical (internal) representation of a path, independent of syntax.
/// </summary>
public interface ICanonicalPath
{
    /// <summary>
    /// Path segments (e.g., ["Customer", "Address", "City"]).
    /// </summary>
    IReadOnlyList<IPathSegment> Segments { get; }

    /// <summary>
    /// Whether this is an absolute path (starts at root).
    /// </summary>
    bool IsAbsolute { get; }

    /// <summary>
    /// Appends a segment to this path.
    /// </summary>
    ICanonicalPath Append(IPathSegment segment);

    /// <summary>
    /// Gets parent path (removes last segment).
    /// </summary>
    ICanonicalPath? GetParent();
}
```

### IPathSegment - Path Component

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Represents a single segment in a path.
/// </summary>
public interface IPathSegment
{
    /// <summary>
    /// Segment type (Property, Index, Wildcard, RecursiveDescent).
    /// </summary>
    PathSegmentType Type { get; }

    /// <summary>
    /// Segment value (property name, array index, or wildcard pattern).
    /// </summary>
    string Value { get; }

    /// <summary>
    /// For Index type, the numeric index.
    /// </summary>
    int? Index { get; }

    /// <summary>
    /// For Wildcard type, whether it's recursive (**).
    /// </summary>
    bool IsRecursive { get; }
}

public enum PathSegmentType
{
    Property,           // "Customer", "Address"
    Index,              // "[0]", "0"
    Wildcard,           // "*"
    RecursiveDescent    // "**", ".."
}
```

---

## Path Translators

### XPathTranslator

```csharp
public class XPathTranslator : IPathTranslator
{
    public string SyntaxType => "xpath";

    public ICanonicalPath Parse(string path)
    {
        // Parse: "Customer/Address/City" → ["Customer", "Address", "City"]
        // Parse: "Customer/Orders/0/Total" → ["Customer", "Orders", [0], "Total"]
        // Parse: "**/LineItems" → [**, "LineItems"]

        var segments = new List<IPathSegment>();
        var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            if (part == "**")
            {
                segments.Add(new PathSegment { Type = PathSegmentType.RecursiveDescent, Value = "**", IsRecursive = true });
            }
            else if (part == "*")
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Wildcard, Value = "*" });
            }
            else if (int.TryParse(part, out var index))
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Index, Value = part, Index = index });
            }
            else
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Property, Value = part });
            }
        }

        return new CanonicalPath
        {
            Segments = segments,
            IsAbsolute = path.StartsWith('/')
        };
    }

    public string Format(ICanonicalPath canonicalPath)
    {
        // ["Customer", "Address", "City"] → "Customer/Address/City"
        var parts = canonicalPath.Segments.Select(s => s.Type switch
        {
            PathSegmentType.RecursiveDescent => "**",
            PathSegmentType.Wildcard => "*",
            PathSegmentType.Index => s.Index.ToString(),
            _ => s.Value
        });

        var formatted = string.Join("/", parts);
        return canonicalPath.IsAbsolute ? "/" + formatted : formatted;
    }

    public bool CanParse(string path)
    {
        // Heuristic: Contains '/' and doesn't start with '$.'
        return path.Contains('/') && !path.StartsWith("$.");
    }
}
```

### JsonPathTranslator

```csharp
public class JsonPathTranslator : IPathTranslator
{
    public string SyntaxType => "jsonpath";

    public ICanonicalPath Parse(string path)
    {
        // Parse: "$.Customer.Address.City" → ["Customer", "Address", "City"]
        // Parse: "$.Customer.Orders[0].Total" → ["Customer", "Orders", [0], "Total"]
        // Parse: "$..LineItems" → [**, "LineItems"]

        var normalized = path.TrimStart('$', '.');
        var segments = new List<IPathSegment>();

        // Handle recursive descent
        if (normalized.StartsWith(".."))
        {
            segments.Add(new PathSegment { Type = PathSegmentType.RecursiveDescent, Value = "**", IsRecursive = true });
            normalized = normalized.Substring(2).TrimStart('.');
        }

        // Split by '.' and parse segments
        var parts = Regex.Split(normalized, @"(?<!\[)\.");

        foreach (var part in parts)
        {
            if (part == "*")
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Wildcard, Value = "*" });
            }
            else if (part.Contains('['))
            {
                // Handle "Orders[0]"
                var match = Regex.Match(part, @"^(\w+)\[(\d+)\]$");
                if (match.Success)
                {
                    segments.Add(new PathSegment { Type = PathSegmentType.Property, Value = match.Groups[1].Value });
                    segments.Add(new PathSegment { Type = PathSegmentType.Index, Value = match.Groups[2].Value, Index = int.Parse(match.Groups[2].Value) });
                }
            }
            else
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Property, Value = part });
            }
        }

        return new CanonicalPath
        {
            Segments = segments,
            IsAbsolute = path.StartsWith("$.")
        };
    }

    public string Format(ICanonicalPath canonicalPath)
    {
        // ["Customer", "Orders", [0], "Total"] → "$.Customer.Orders[0].Total"
        var parts = new List<string>();
        var lastWasProperty = false;

        foreach (var segment in canonicalPath.Segments)
        {
            switch (segment.Type)
            {
                case PathSegmentType.RecursiveDescent:
                    parts.Add("..");
                    lastWasProperty = false;
                    break;

                case PathSegmentType.Wildcard:
                    parts.Add("*");
                    lastWasProperty = false;
                    break;

                case PathSegmentType.Index:
                    // Append to previous property: "Orders" + "[0]"
                    if (lastWasProperty && parts.Count > 0)
                    {
                        parts[parts.Count - 1] += $"[{segment.Index}]";
                    }
                    lastWasProperty = false;
                    break;

                case PathSegmentType.Property:
                    parts.Add(segment.Value);
                    lastWasProperty = true;
                    break;
            }
        }

        var formatted = string.Join(".", parts);
        return canonicalPath.IsAbsolute ? "$." + formatted : formatted;
    }

    public bool CanParse(string path)
    {
        return path.StartsWith("$.");
    }
}
```

### DotNotationTranslator

```csharp
public class DotNotationTranslator : IPathTranslator
{
    public string SyntaxType => "dotnotation";

    public ICanonicalPath Parse(string path)
    {
        // Parse: "Customer.Address.City" → ["Customer", "Address", "City"]
        // Parse: "Customer.Orders.0.Total" → ["Customer", "Orders", [0], "Total"]
        // Parse: "**.LineItems" → [**, "LineItems"]

        var segments = new List<IPathSegment>();
        var parts = path.Split('.');

        foreach (var part in parts)
        {
            if (part == "**")
            {
                segments.Add(new PathSegment { Type = PathSegmentType.RecursiveDescent, Value = "**", IsRecursive = true });
            }
            else if (part == "*")
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Wildcard, Value = "*" });
            }
            else if (int.TryParse(part, out var index))
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Index, Value = part, Index = index });
            }
            else
            {
                segments.Add(new PathSegment { Type = PathSegmentType.Property, Value = part });
            }
        }

        return new CanonicalPath { Segments = segments, IsAbsolute = false };
    }

    public string Format(ICanonicalPath canonicalPath)
    {
        // ["Customer", "Address", "City"] → "Customer.Address.City"
        var parts = canonicalPath.Segments.Select(s => s.Type switch
        {
            PathSegmentType.RecursiveDescent => "**",
            PathSegmentType.Wildcard => "*",
            PathSegmentType.Index => s.Index.ToString(),
            _ => s.Value
        });

        return string.Join(".", parts);
    }

    public bool CanParse(string path)
    {
        // Default fallback (contains dots, not starting with $.)
        return path.Contains('.') && !path.StartsWith("$.");
    }
}
```

---

## Path Translation Service

### IPathTranslationService

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Service for translating between different path syntaxes.
/// </summary>
public interface IPathTranslationService
{
    /// <summary>
    /// Registers a path translator.
    /// </summary>
    void RegisterTranslator(IPathTranslator translator);

    /// <summary>
    /// Translates a path from one syntax to another.
    /// </summary>
    /// <param name="path">Path in source syntax</param>
    /// <param name="sourceSyntax">Source syntax type (e.g., "jsonpath")</param>
    /// <param name="targetSyntax">Target syntax type (e.g., "xpath")</param>
    /// <returns>Path in target syntax</returns>
    string Translate(string path, string sourceSyntax, string targetSyntax);

    /// <summary>
    /// Parses a path in any registered syntax to canonical form.
    /// </summary>
    ICanonicalPath ParseAny(string path);

    /// <summary>
    /// Formats a canonical path into the specified syntax.
    /// </summary>
    string Format(ICanonicalPath canonicalPath, string targetSyntax);
}
```

### Implementation

```csharp
public class PathTranslationService : IPathTranslationService
{
    private readonly Dictionary<string, IPathTranslator> _translators = new();

    public void RegisterTranslator(IPathTranslator translator)
    {
        _translators[translator.SyntaxType] = translator;
    }

    public string Translate(string path, string sourceSyntax, string targetSyntax)
    {
        if (!_translators.TryGetValue(sourceSyntax, out var sourceTranslator))
            throw new InvalidOperationException($"Source syntax '{sourceSyntax}' not registered");

        if (!_translators.TryGetValue(targetSyntax, out var targetTranslator))
            throw new InvalidOperationException($"Target syntax '{targetSyntax}' not registered");

        // Parse → Canonical → Format
        var canonical = sourceTranslator.Parse(path);
        return targetTranslator.Format(canonical);
    }

    public ICanonicalPath ParseAny(string path)
    {
        // Try each translator's CanParse() in order
        foreach (var translator in _translators.Values)
        {
            if (translator.CanParse(path))
            {
                return translator.Parse(path);
            }
        }

        // Default to dot notation if no match
        if (_translators.TryGetValue("dotnotation", out var dotTranslator))
        {
            return dotTranslator.Parse(path);
        }

        throw new InvalidOperationException($"No translator can parse path: {path}");
    }

    public string Format(ICanonicalPath canonicalPath, string targetSyntax)
    {
        if (!_translators.TryGetValue(targetSyntax, out var translator))
            throw new InvalidOperationException($"Target syntax '{targetSyntax}' not registered");

        return translator.Format(canonicalPath);
    }
}
```

---

## Integration with IDataContainer

### Enhanced IDataContainer

```csharp
public interface IDataContainer
{
    /// <summary>
    /// Path translation service for this container.
    /// </summary>
    IPathTranslationService PathTranslation { get; }

    /// <summary>
    /// Navigates to a node using any registered path syntax.
    /// </summary>
    /// <param name="path">Path in any syntax (auto-detected)</param>
    IDataNode Navigate(string path);

    /// <summary>
    /// Navigates to a node using explicit syntax type.
    /// </summary>
    IDataNode Navigate(string path, string syntaxType);

    /// <summary>
    /// Evaluates a path in any syntax.
    /// </summary>
    object? Evaluate(string path);

    /// <summary>
    /// Evaluates a path using explicit syntax type.
    /// </summary>
    object? Evaluate(string path, string syntaxType);
}
```

### Provider Registration with Syntax

```csharp
public interface IDataContainer
{
    /// <summary>
    /// Registers a provider for a path pattern in default syntax (XPath).
    /// </summary>
    void RegisterProvider(string pathPattern, IDataProvider provider);

    /// <summary>
    /// Registers a provider for a path pattern in explicit syntax.
    /// </summary>
    void RegisterProvider(string pathPattern, string syntaxType, IDataProvider provider);
}

// Usage examples
container.RegisterProvider("Customer/Address", customerProvider);                        // XPath (default)
container.RegisterProvider("$.Customer.Address", "jsonpath", customerProvider);          // JSONPath
container.RegisterProvider("Customer.Address", "dotnotation", customerProvider);         // Dot notation
```

---

## Template Engine Integration

### Handlebars (uses dot notation)

```csharp
public class HandlebarsTemplateAdapter
{
    private readonly IPathTranslationService _pathTranslation;

    public object AdaptDataContainer(IDataContainer container)
    {
        // Handlebars uses dot notation: {{Customer.Name}}
        // Translate to container's internal representation

        return new DynamicDataProxy(container, "dotnotation");
    }
}

public class DynamicDataProxy : DynamicObject
{
    private readonly IDataContainer _container;
    private readonly string _syntaxType;

    public override bool TryGetMember(GetMemberBinder binder, out object? result)
    {
        // Handlebars accesses "Customer.Name"
        // Translate to container's path syntax and evaluate

        var path = binder.Name;  // "Customer"
        result = _container.Evaluate(path, _syntaxType);
        return true;
    }
}
```

### XSLT (uses XPath)

```csharp
public class XsltTemplateAdapter
{
    public XmlDocument AdaptDataContainer(IDataContainer container)
    {
        // XSLT uses XPath: /Customer/Address/City
        // Container already supports XPath, so direct conversion to XML

        var xml = new XmlDocument();
        BuildXmlFromContainer(container.Root, xml);
        return xml;
    }
}
```

### Liquid (uses dot notation)

```csharp
public class LiquidTemplateAdapter
{
    public object AdaptDataContainer(IDataContainer container)
    {
        // Liquid uses dot notation: {{ customer.name }}
        // Same as Handlebars

        return new DynamicDataProxy(container, "dotnotation");
    }
}
```

---

## Usage Examples

### Example 1: Translation Between Syntaxes

```csharp
var translation = new PathTranslationService();
translation.RegisterTranslator(new XPathTranslator());
translation.RegisterTranslator(new JsonPathTranslator());
translation.RegisterTranslator(new DotNotationTranslator());

// Translate JSONPath to XPath
var jsonPath = "$.Customer.Orders[0].Total";
var xPath = translation.Translate(jsonPath, "jsonpath", "xpath");
// Result: "Customer/Orders/0/Total"

// Translate XPath to Dot Notation
var xPath2 = "Customer/Address/City";
var dotNotation = translation.Translate(xPath2, "xpath", "dotnotation");
// Result: "Customer.Address.City"
```

### Example 2: Provider Registration with Different Syntaxes

```csharp
var container = DataContainerFactory.Create();

// Register provider using XPath (default)
container.RegisterProvider("Customer/Orders", ordersProvider);

// Register provider using JSONPath
container.RegisterProvider("$.Customer.Orders", "jsonpath", ordersProvider);

// Register provider using dot notation
container.RegisterProvider("Customer.Orders", "dotnotation", ordersProvider);

// All three register the SAME provider - syntax translated automatically
```

### Example 3: Template Engine Uses Native Syntax

```csharp
// Handlebars template (uses dot notation)
var handlebarsTemplate = "Hello {{Customer.FirstName}}!";

// Data container (internal XPath)
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);

// Adapter translates dot notation to XPath
var adapter = new HandlebarsTemplateAdapter(_pathTranslation);
var adaptedData = adapter.AdaptDataContainer(container);

// Handlebars accesses "Customer.FirstName"
// → Adapter translates to "Customer/FirstName" (XPath)
// → Container evaluates XPath
// → Returns value
var result = Handlebars.Compile(handlebarsTemplate)(adaptedData);
```

### Example 4: Auto-Detection

```csharp
// Container auto-detects syntax
var value1 = container.Evaluate("$.Customer.Address.City");          // JSONPath (auto-detected)
var value2 = container.Evaluate("Customer/Address/City");            // XPath (auto-detected)
var value3 = container.Evaluate("Customer.Address.City");            // Dot notation (auto-detected)

// All three return the SAME value
```

---

## Translation Table

| XPath | JSONPath | Dot Notation | Description |
|-------|----------|--------------|-------------|
| `Customer/Address/City` | `$.Customer.Address.City` | `Customer.Address.City` | Simple property path |
| `Customer/Orders/0/Total` | `$.Customer.Orders[0].Total` | `Customer.Orders.0.Total` | Array index |
| `Customer/*/Address` | `$.Customer.*.Address` | `Customer.*.Address` | Wildcard (single level) |
| `**/LineItems` | `$..LineItems` | `**.LineItems` | Recursive descent |
| `/Customer` | `$.Customer` | `Customer` | Absolute vs relative |

---

## Benefits

### 1. Template Engine Flexibility
✅ Handlebars uses dot notation natively
✅ XSLT uses XPath natively
✅ Liquid uses dot notation natively
✅ No forcing template engines to use unfamiliar syntax

### 2. Provider Flexibility
✅ Providers register with preferred syntax
✅ Automatic translation to container's internal format
✅ Same provider works with any template engine

### 3. Extensibility
✅ Add new syntaxes via `IPathTranslator` implementation
✅ Custom template engines can define custom syntax
✅ No changes to core container

### 4. Interoperability
✅ Data from JSONPath providers works with XPath templates
✅ Cross-syntax compatibility
✅ Canonical representation ensures consistency

---

## Success Metrics

- ✅ Support XPath, JSONPath, and Dot Notation out of box
- ✅ 100% accurate translation between syntaxes
- ✅ Template engines use native syntax (no forced conversions)
- ✅ Providers register with any syntax
- ✅ < 10ms translation overhead per path
- ✅ Extensible for new syntaxes
- ✅ 80%+ test coverage

---

## Dependencies

### OoBDev Framework
- None (this IS a framework component)

### External
- System.Text.RegularExpressions (built-in)
- System.Dynamic (built-in, for DynamicObject)

---

## Related Documentation

- [Epic 11: Data Enhancement Pipeline](../README-REVISED.md)
- [Epic 10: Text Templating](../../10-TextTemplating/README-REVISED.md)
