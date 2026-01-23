# Path Syntax Translation - API Design

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Path Syntax Translation
**Last Updated:** 2026-01-22

---

## API Overview

Path Translation API provides:
1. **IPathNavigator** - Extensibility point for custom syntaxes
2. **ICanonicalPath** - Syntax-agnostic path representation
3. **IPathTranslationService** - Translation orchestration
4. **Built-in Navigators** - XPath, JSONPath, Dot Notation

---

## Core Interfaces

### IPathNavigator

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Path navigator for specific syntax (XPath, JSONPath, Dot Notation, etc.).
/// </summary>
public interface IPathNavigator
{
    /// <summary>
    /// Navigator type identifier (e.g., "xpath", "jsonpath", "dotnotation").
    /// </summary>
    string NavigatorType { get; }

    /// <summary>
    /// Parses path string into canonical representation.
    /// </summary>
    /// <param name="path">Path in this navigator's syntax</param>
    /// <returns>Canonical path</returns>
    /// <exception cref="PathParseException">Invalid syntax</exception>
    ICanonicalPath Parse(string path);

    /// <summary>
    /// Formats canonical path to this navigator's syntax.
    /// </summary>
    /// <param name="canonical">Canonical path</param>
    /// <returns>Formatted path string</returns>
    string Format(ICanonicalPath canonical);

    /// <summary>
    /// Checks if path string can be parsed by this navigator.
    /// </summary>
    /// <param name="path">Path to check</param>
    /// <returns>True if this navigator can parse the path</returns>
    bool CanParse(string path);
}
```

---

### ICanonicalPath

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Canonical (syntax-agnostic) path representation.
/// </summary>
public interface ICanonicalPath : IEquatable<ICanonicalPath>
{
    /// <summary>
    /// Path segments (properties, array indices, wildcards, etc.).
    /// </summary>
    IReadOnlyList<IPathSegment> Segments { get; }

    /// <summary>
    /// Is this an absolute path?
    /// </summary>
    bool IsAbsolute { get; }

    /// <summary>
    /// Combines this path with another path.
    /// </summary>
    /// <param name="other">Path to append</param>
    /// <returns>Combined path</returns>
    ICanonicalPath Combine(ICanonicalPath other);

    /// <summary>
    /// Gets parent path (removes last segment).
    /// </summary>
    /// <returns>Parent path or null if root</returns>
    ICanonicalPath? GetParent();

    /// <summary>
    /// Converts to canonical string format.
    /// </summary>
    string ToString();
}
```

---

### IPathSegment

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Single segment in a canonical path.
/// </summary>
public interface IPathSegment : IEquatable<IPathSegment>
{
    /// <summary>
    /// Segment type.
    /// </summary>
    PathSegmentType Type { get; }

    /// <summary>
    /// Property name or wildcard pattern (for property/wildcard segments).
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// Array index (for array index segments).
    /// </summary>
    int? Index { get; }
}

/// <summary>
/// Path segment types.
/// </summary>
public enum PathSegmentType
{
    /// <summary>
    /// Property access (e.g., "Customer", "Address").
    /// </summary>
    Property,

    /// <summary>
    /// Array element access by index (e.g., [0], [1]).
    /// </summary>
    ArrayIndex,

    /// <summary>
    /// Wildcard (matches any single element, e.g., *).
    /// </summary>
    Wildcard,

    /// <summary>
    /// Recursive descent (matches at any depth, e.g., **).
    /// </summary>
    RecursiveDescent
}
```

---

### IPathTranslationService

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Service for translating paths between different syntaxes.
/// </summary>
public interface IPathTranslationService
{
    /// <summary>
    /// Registers a path navigator.
    /// </summary>
    /// <param name="navigator">Navigator to register</param>
    void RegisterNavigator(IPathNavigator navigator);

    /// <summary>
    /// Translates path from one syntax to another.
    /// </summary>
    /// <param name="path">Path in source syntax</param>
    /// <param name="sourceSyntax">Source navigator type</param>
    /// <param name="targetSyntax">Target navigator type</param>
    /// <returns>Path in target syntax</returns>
    string Translate(string path, string sourceSyntax, string targetSyntax);

    /// <summary>
    /// Parses path using any registered navigator (auto-detects syntax).
    /// </summary>
    /// <param name="path">Path string</param>
    /// <returns>Canonical path</returns>
    ICanonicalPath ParseAny(string path);

    /// <summary>
    /// Formats canonical path using specified syntax.
    /// </summary>
    /// <param name="canonical">Canonical path</param>
    /// <param name="targetSyntax">Target navigator type</param>
    /// <returns>Formatted path string</returns>
    string Format(ICanonicalPath canonical, string targetSyntax);

    /// <summary>
    /// Gets navigator by type.
    /// </summary>
    /// <param name="navigatorType">Navigator type</param>
    /// <returns>Navigator or null if not found</returns>
    IPathNavigator? GetNavigator(string navigatorType);

    /// <summary>
    /// Gets all registered navigator types.
    /// </summary>
    IEnumerable<string> GetRegisteredNavigatorTypes();
}
```

---

## Factory & Builder

### PathSegment Factory

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Factory for creating path segments.
/// </summary>
public static class PathSegment
{
    /// <summary>
    /// Creates a property segment.
    /// </summary>
    /// <param name="propertyName">Property name</param>
    public static IPathSegment Property(string propertyName)
    {
        if (string.IsNullOrWhiteSpace(propertyName))
            throw new ArgumentException("Property name cannot be empty", nameof(propertyName));

        return new PropertySegment(propertyName);
    }

    /// <summary>
    /// Creates an array index segment.
    /// </summary>
    /// <param name="index">Array index (0-based)</param>
    public static IPathSegment ArrayIndex(int index)
    {
        if (index < 0)
            throw new ArgumentOutOfRangeException(nameof(index), "Index cannot be negative");

        return new ArrayIndexSegment(index);
    }

    /// <summary>
    /// Creates a wildcard segment (*).
    /// </summary>
    public static IPathSegment Wildcard() => new WildcardSegment();

    /// <summary>
    /// Creates a recursive descent segment (**).
    /// </summary>
    public static IPathSegment RecursiveDescent() => new RecursiveDescentSegment();
}
```

### CanonicalPath Builder

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Builder for constructing canonical paths.
/// </summary>
public class CanonicalPathBuilder
{
    private readonly List<IPathSegment> _segments = new();
    private bool _isAbsolute;

    /// <summary>
    /// Sets whether path is absolute.
    /// </summary>
    public CanonicalPathBuilder Absolute(bool isAbsolute = true)
    {
        _isAbsolute = isAbsolute;
        return this;
    }

    /// <summary>
    /// Adds property segment.
    /// </summary>
    public CanonicalPathBuilder Property(string name)
    {
        _segments.Add(PathSegment.Property(name));
        return this;
    }

    /// <summary>
    /// Adds array index segment.
    /// </summary>
    public CanonicalPathBuilder Index(int index)
    {
        _segments.Add(PathSegment.ArrayIndex(index));
        return this;
    }

    /// <summary>
    /// Adds wildcard segment.
    /// </summary>
    public CanonicalPathBuilder Wildcard()
    {
        _segments.Add(PathSegment.Wildcard());
        return this;
    }

    /// <summary>
    /// Adds recursive descent segment.
    /// </summary>
    public CanonicalPathBuilder RecursiveDescent()
    {
        _segments.Add(PathSegment.RecursiveDescent());
        return this;
    }

    /// <summary>
    /// Builds canonical path.
    /// </summary>
    public ICanonicalPath Build()
    {
        return new CanonicalPath(_isAbsolute, _segments.ToArray());
    }
}
```

---

## Built-in Navigators

### XPathNavigator

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation.Navigators;

/// <summary>
/// Navigator for XPath syntax.
/// Syntax: Customer/Orders/0/Total
/// </summary>
public class XPathNavigator : IPathNavigator
{
    public string NavigatorType => "xpath";

    public ICanonicalPath Parse(string path)
    {
        // Implementation in architecture.md
    }

    public string Format(ICanonicalPath canonical)
    {
        // Implementation in architecture.md
    }

    public bool CanParse(string path)
    {
        // XPath: contains / or is simple property name
        return path.Contains("/") || !path.Contains(".");
    }
}
```

### JSONPathNavigator

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation.Navigators;

/// <summary>
/// Navigator for JSONPath syntax.
/// Syntax: $.Customer.Orders[0].Total
/// </summary>
public class JSONPathNavigator : IPathNavigator
{
    public string NavigatorType => "jsonpath";

    public ICanonicalPath Parse(string path)
    {
        // Implementation in architecture.md
    }

    public string Format(ICanonicalPath canonical)
    {
        // Implementation in architecture.md
    }

    public bool CanParse(string path)
    {
        // JSONPath: starts with $
        return path.StartsWith("$");
    }
}
```

### DotNotationNavigator

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation.Navigators;

/// <summary>
/// Navigator for Dot Notation syntax.
/// Syntax: Customer.Orders.0.Total
/// </summary>
public class DotNotationNavigator : IPathNavigator
{
    public string NavigatorType => "dotnotation";

    public ICanonicalPath Parse(string path)
    {
        // Implementation in architecture.md
    }

    public string Format(ICanonicalPath canonical)
    {
        // Implementation in architecture.md
    }

    public bool CanParse(string path)
    {
        // Dot notation: contains . but not $ or /
        return path.Contains(".") && !path.StartsWith("$") && !path.Contains("/");
    }
}
```

---

## Usage Examples

### Example 1: Basic Translation

```csharp
using OoBDev.System.Data.Enhancement.PathTranslation;

// Create translation service (has built-in navigators registered)
var translation = new PathTranslationService();

// Translate XPath → JSONPath
var xpath = "Customer/Orders/0/Total";
var jsonpath = translation.Translate(xpath, "xpath", "jsonpath");
Console.WriteLine(jsonpath);  // $.Customer.Orders[0].Total

// Translate JSONPath → Dot Notation
var dotNotation = translation.Translate(jsonpath, "jsonpath", "dotnotation");
Console.WriteLine(dotNotation);  // Customer.Orders.0.Total

// Translate Dot Notation → XPath (round-trip)
var xpathAgain = translation.Translate(dotNotation, "dotnotation", "xpath");
Console.WriteLine(xpathAgain);  // Customer/Orders/0/Total
```

---

### Example 2: Auto-Detection

```csharp
// ParseAny() auto-detects syntax
var canonical1 = translation.ParseAny("$.Customer.Name");      // Detects JSONPath
var canonical2 = translation.ParseAny("Customer/Name");        // Detects XPath
var canonical3 = translation.ParseAny("Customer.Name");        // Detects Dot Notation

// All produce same canonical path
Assert.AreEqual(canonical1, canonical2);
Assert.AreEqual(canonical2, canonical3);

// Format to any syntax
var xpath = translation.Format(canonical1, "xpath");
var jsonpath = translation.Format(canonical1, "jsonpath");
var dotNotation = translation.Format(canonical1, "dotnotation");

Console.WriteLine(xpath);        // Customer/Name
Console.WriteLine(jsonpath);     // $.Customer.Name
Console.WriteLine(dotNotation);  // Customer.Name
```

---

### Example 3: Builder Pattern

```csharp
// Build canonical path programmatically
var canonical = new CanonicalPathBuilder()
    .Absolute()
    .Property("Customer")
    .Property("Orders")
    .Index(0)
    .Property("Total")
    .Build();

// Format to different syntaxes
var xpath = translation.Format(canonical, "xpath");          // /Customer/Orders/0/Total
var jsonpath = translation.Format(canonical, "jsonpath");    // $.Customer.Orders[0].Total
var dotNotation = translation.Format(canonical, "dotnotation"); // Customer.Orders.0.Total
```

---

### Example 4: Wildcards and Recursive Descent

```csharp
// Wildcard: all line items
var wildcardPath = new CanonicalPathBuilder()
    .Property("Order")
    .Property("LineItems")
    .Wildcard()
    .Property("Price")
    .Build();

translation.Format(wildcardPath, "xpath");       // Order/LineItems/*/Price
translation.Format(wildcardPath, "jsonpath");    // $.Order.LineItems[*].Price
translation.Format(wildcardPath, "dotnotation"); // Order.LineItems.*.Price

// Recursive descent: LineItems at any depth
var recursivePath = new CanonicalPathBuilder()
    .RecursiveDescent()
    .Property("LineItems")
    .Build();

translation.Format(recursivePath, "xpath");      // **/LineItems
translation.Format(recursivePath, "jsonpath");   // $..LineItems
translation.Format(recursivePath, "dotnotation"); // **.LineItems
```

---

### Example 5: Custom Navigator

```csharp
/// <summary>
/// Custom navigator for MongoDB-style paths.
/// Syntax: customer.orders.0.total
/// </summary>
public class MongoDBPathNavigator : IPathNavigator
{
    public string NavigatorType => "mongodb";

    public ICanonicalPath Parse(string path)
    {
        // MongoDB uses dot notation with lowercase
        var segments = path.Split('.')
            .Select(part =>
            {
                if (int.TryParse(part, out var index))
                    return PathSegment.ArrayIndex(index);
                return PathSegment.Property(part);
            })
            .ToArray();

        return new CanonicalPath(isAbsolute: false, segments);
    }

    public string Format(ICanonicalPath canonical)
    {
        var parts = canonical.Segments.Select(s =>
            s.Type == PathSegmentType.Property ? s.Value.ToLowerInvariant() :
            s.Type == PathSegmentType.ArrayIndex ? s.Index.ToString() :
            s.ToString());

        return string.Join(".", parts);
    }

    public bool CanParse(string path)
    {
        // MongoDB: lowercase with dots
        return path.Contains(".") && path == path.ToLowerInvariant();
    }
}

// Register custom navigator
translation.RegisterNavigator(new MongoDBPathNavigator());

// Use custom navigator
var mongodb = translation.Translate("Customer/Orders/0/Total", "xpath", "mongodb");
Console.WriteLine(mongodb);  // customer.orders.0.total
```

---

### Example 6: Integration with DataContainer

```csharp
// DataContainer uses translation service internally
var container = new DataContainer(translation);

// Navigate using any syntax - all work
var node1 = container.Navigate("$.Customer.FirstName");    // JSONPath
var node2 = container.Navigate("Customer/FirstName");      // XPath
var node3 = container.Navigate("Customer.FirstName");      // Dot Notation

// All return same node (cached internally as canonical path)
Assert.AreSame(node1, node2);
Assert.AreSame(node2, node3);

// Register provider using any syntax
container.RegisterProvider("$.Customer", customerProvider);     // JSONPath
container.RegisterProvider("Order/LineItems/*", lineItemProvider); // XPath
container.RegisterProvider("System.Config", configProvider);    // Dot Notation

// All work - internally converted to canonical paths
```

---

## Extension Methods

### PathTranslation Extensions

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

public static class PathTranslationExtensions
{
    /// <summary>
    /// Translates path to XPath syntax.
    /// </summary>
    public static string ToXPath(this IPathTranslationService service, string path)
    {
        var canonical = service.ParseAny(path);
        return service.Format(canonical, "xpath");
    }

    /// <summary>
    /// Translates path to JSONPath syntax.
    /// </summary>
    public static string ToJSONPath(this IPathTranslationService service, string path)
    {
        var canonical = service.ParseAny(path);
        return service.Format(canonical, "jsonpath");
    }

    /// <summary>
    /// Translates path to Dot Notation syntax.
    /// </summary>
    public static string ToDotNotation(this IPathTranslationService service, string path)
    {
        var canonical = service.ParseAny(path);
        return service.Format(canonical, "dotnotation");
    }

    /// <summary>
    /// Checks if two paths are equivalent (same canonical representation).
    /// </summary>
    public static bool AreEquivalent(this IPathTranslationService service, string path1, string path2)
    {
        var canonical1 = service.ParseAny(path1);
        var canonical2 = service.ParseAny(path2);
        return canonical1.Equals(canonical2);
    }
}
```

### CanonicalPath Extensions

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

public static class CanonicalPathExtensions
{
    /// <summary>
    /// Gets last segment of path.
    /// </summary>
    public static IPathSegment? GetLastSegment(this ICanonicalPath path)
    {
        return path.Segments.LastOrDefault();
    }

    /// <summary>
    /// Gets first segment of path.
    /// </summary>
    public static IPathSegment? GetFirstSegment(this ICanonicalPath path)
    {
        return path.Segments.FirstOrDefault();
    }

    /// <summary>
    /// Checks if path is a property path (all property segments).
    /// </summary>
    public static bool IsPropertyPath(this ICanonicalPath path)
    {
        return path.Segments.All(s => s.Type == PathSegmentType.Property);
    }

    /// <summary>
    /// Checks if path contains wildcards.
    /// </summary>
    public static bool HasWildcards(this ICanonicalPath path)
    {
        return path.Segments.Any(s =>
            s.Type == PathSegmentType.Wildcard ||
            s.Type == PathSegmentType.RecursiveDescent);
    }

    /// <summary>
    /// Gets depth of path (number of segments).
    /// </summary>
    public static int GetDepth(this ICanonicalPath path)
    {
        return path.Segments.Count;
    }
}
```

---

## Error Handling

### Exception Types

```csharp
namespace OoBDev.System.Data.Enhancement.PathTranslation;

/// <summary>
/// Base exception for path translation errors.
/// </summary>
public class PathTranslationException : Exception
{
    public string? Path { get; }

    public PathTranslationException(string message, string? path = null)
        : base(message)
    {
        Path = path;
    }

    public PathTranslationException(string message, Exception innerException, string? path = null)
        : base(message, innerException)
    {
        Path = path;
    }
}

/// <summary>
/// Exception thrown when path parsing fails.
/// </summary>
public class PathParseException : PathTranslationException
{
    public string NavigatorType { get; }
    public int? Position { get; }

    public PathParseException(
        string message,
        string path,
        string navigatorType,
        int? position = null)
        : base(message, path)
    {
        NavigatorType = navigatorType;
        Position = position;
    }
}

/// <summary>
/// Exception thrown when navigator not found.
/// </summary>
public class NavigatorNotFoundException : PathTranslationException
{
    public string NavigatorType { get; }

    public NavigatorNotFoundException(string navigatorType)
        : base($"Navigator '{navigatorType}' not found")
    {
        NavigatorType = navigatorType;
    }
}
```

### Error Handling Examples

```csharp
try
{
    var canonical = translation.ParseAny("$.Customer..Name");  // Invalid JSONPath syntax
}
catch (PathParseException ex)
{
    Console.WriteLine($"Parse error in {ex.Path} at position {ex.Position}");
    Console.WriteLine($"Navigator: {ex.NavigatorType}");
}

try
{
    var result = translation.Translate("Customer/Name", "xpath", "unknown-syntax");
}
catch (NavigatorNotFoundException ex)
{
    Console.WriteLine($"Navigator '{ex.NavigatorType}' not registered");

    // Show available navigators
    var available = translation.GetRegisteredNavigatorTypes();
    Console.WriteLine($"Available: {string.Join(", ", available)}");
}
```

---

## Best Practices

### 1. Use Auto-Detection When Possible
```csharp
// ✅ GOOD: Let service detect syntax
var canonical = translation.ParseAny(userInput);

// ❌ BAD: Hardcoding syntax assumption
var canonical = translation.GetNavigator("xpath").Parse(userInput); // What if it's JSONPath?
```

### 2. Cache Translation Service Instance
```csharp
// ✅ GOOD: Singleton service
services.AddSingleton<IPathTranslationService, PathTranslationService>();

// ❌ BAD: Creating new instances
var translation1 = new PathTranslationService();
var translation2 = new PathTranslationService(); // Wasted registration
```

### 3. Use Canonical Paths Internally
```csharp
// ✅ GOOD: Store canonical paths
private readonly Dictionary<ICanonicalPath, object> _cache;

// ❌ BAD: Store string paths (syntax-dependent)
private readonly Dictionary<string, object> _cache; // Won't match across syntaxes
```

### 4. Format Only at Boundaries
```csharp
// ✅ GOOD: Parse at input, format at output
var canonical = translation.ParseAny(userInput);
// ... work with canonical internally ...
var output = translation.Format(canonical, targetSyntax);

// ❌ BAD: Converting back and forth
var xpath = translation.ToXPath(jsonpath);
var canonical = translation.ParseAny(xpath);
var jsonpathAgain = translation.ToJSONPath(xpath); // Unnecessary
```

---

## Performance Considerations

### Translation Caching
```csharp
// Cache frequently translated paths
private readonly ConcurrentDictionary<(string path, string source, string target), string> _translationCache;

public string Translate(string path, string sourceSyntax, string targetSyntax)
{
    var key = (path, sourceSyntax, targetSyntax);
    return _translationCache.GetOrAdd(key, k =>
    {
        var canonical = _navigators[k.source].Parse(k.path);
        return _navigators[k.target].Format(canonical);
    });
}
```

### Parse Caching
```csharp
// Cache parsed canonical paths
private readonly ConcurrentDictionary<string, ICanonicalPath> _parseCache;

public ICanonicalPath ParseAny(string path)
{
    return _parseCache.GetOrAdd(path, p =>
    {
        foreach (var navigator in _navigators.Values)
        {
            if (navigator.CanParse(p))
                return navigator.Parse(p);
        }
        return _defaultNavigator.Parse(p);
    });
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container API](../CoreContainer/api-design.md)
