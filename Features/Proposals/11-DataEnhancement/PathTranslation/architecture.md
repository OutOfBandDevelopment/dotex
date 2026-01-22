# Path Syntax Translation - Architecture

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Path Syntax Translation
**Last Updated:** 2026-01-22

---

## Architectural Overview

Path Translation provides a modular, provider-based system for converting between different path syntaxes. **XPath is one of many navigation providers**, not THE navigation system. All syntaxes translate through a canonical internal representation.

```
┌──────────────────────────────────────────────────────────────┐
│                    Consumer Applications                      │
│         (Templates, Services, Data Queries)                   │
└────────────────┬──────────────┬──────────────┬────────────────┘
                 │              │              │
        XPath    │    JSONPath  │    Dot       │    Custom
       Syntax    │    Syntax    │  Notation    │    Syntax
                 ↓              ↓              ↓
┌─────────────────────────────────────────────────────────────┐
│            IPathTranslationService                          │
│  - RegisterNavigator()                                      │
│  - Translate(source, target)                                │
│  - ParseAny() - auto-detect syntax                          │
└────────────────┬──────────────┬──────────────┬──────────────┘
                 ↓              ↓              ↓
      ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
      │XPathNavigator│ │JSONNavigator │ │ DotNavigator │
      │              │ │              │ │              │
      │Parse()       │ │Parse()       │ │Parse()       │
      │Format()      │ │Format()      │ │Format()      │
      └──────┬───────┘ └──────┬───────┘ └──────┬───────┘
             │                │                │
             └────────────────┼────────────────┘
                              ↓
                   ┌────────────────────┐
                   │  ICanonicalPath    │
                   │                    │
                   │ Segments[]         │
                   │ IsAbsolute         │
                   └────────────────────┘
                              ↓
                   ┌────────────────────┐
                   │  DataContainer     │
                   │  (Uses canonical)  │
                   └────────────────────┘
```

**Key Principle:** Syntax translation happens at the edges (parse/format), core container uses canonical representation internally.

---

## Core Components

### 1. ICanonicalPath (Internal Representation)

**Responsibilities:**
- Syntax-agnostic path representation
- Segment-based structure
- Path manipulation (combine, parent, etc.)

**Design Pattern:** Value Object

**Implementation:**

```csharp
public class CanonicalPath : ICanonicalPath
{
    private readonly List<IPathSegment> _segments;
    private readonly bool _isAbsolute;

    public IReadOnlyList<IPathSegment> Segments => _segments;
    public bool IsAbsolute => _isAbsolute;

    public CanonicalPath(bool isAbsolute, params IPathSegment[] segments)
    {
        _isAbsolute = isAbsolute;
        _segments = new List<IPathSegment>(segments);
    }

    public ICanonicalPath Combine(ICanonicalPath other)
    {
        var combined = new List<IPathSegment>(_segments);
        combined.AddRange(other.Segments);
        return new CanonicalPath(_isAbsolute, combined.ToArray());
    }

    public ICanonicalPath? GetParent()
    {
        if (_segments.Count == 0)
            return null;

        var parentSegments = _segments.Take(_segments.Count - 1).ToArray();
        return new CanonicalPath(_isAbsolute, parentSegments);
    }

    public override string ToString()
    {
        // Canonical string format: "Customer/Address/City"
        var prefix = _isAbsolute ? "/" : "";
        return prefix + string.Join("/", _segments.Select(s => s.ToString()));
    }

    public override bool Equals(object? obj)
    {
        if (obj is not CanonicalPath other)
            return false;

        if (_isAbsolute != other._isAbsolute)
            return false;

        if (_segments.Count != other._segments.Count)
            return false;

        for (int i = 0; i < _segments.Count; i++)
        {
            if (!_segments[i].Equals(other._segments[i]))
                return false;
        }

        return true;
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(_isAbsolute);
        foreach (var segment in _segments)
        {
            hash.Add(segment);
        }
        return hash.ToHashCode();
    }
}
```

---

### 2. PathSegment (Building Blocks)

**Segment Types:**

```csharp
public class PathSegment : IPathSegment
{
    public PathSegmentType Type { get; }
    public string? Value { get; }
    public int? Index { get; }

    private PathSegment(PathSegmentType type, string? value = null, int? index = null)
    {
        Type = type;
        Value = value;
        Index = index;
    }

    // Factory methods
    public static IPathSegment Property(string name) =>
        new PathSegment(PathSegmentType.Property, name);

    public static IPathSegment ArrayIndex(int index) =>
        new PathSegment(PathSegmentType.ArrayIndex, index: index);

    public static IPathSegment Wildcard() =>
        new PathSegment(PathSegmentType.Wildcard, "*");

    public static IPathSegment RecursiveDescent() =>
        new PathSegment(PathSegmentType.RecursiveDescent, "**");

    public override string ToString()
    {
        return Type switch
        {
            PathSegmentType.Property => Value ?? "",
            PathSegmentType.ArrayIndex => Index?.ToString() ?? "",
            PathSegmentType.Wildcard => "*",
            PathSegmentType.RecursiveDescent => "**",
            _ => ""
        };
    }

    public override bool Equals(object? obj)
    {
        if (obj is not PathSegment other)
            return false;

        return Type == other.Type &&
               Value == other.Value &&
               Index == other.Index;
    }

    public override int GetHashCode() =>
        HashCode.Combine(Type, Value, Index);
}
```

---

### 3. XPathNavigator (One of Many Navigators)

**Responsibilities:**
- Parse XPath syntax to canonical
- Format canonical to XPath syntax

**Syntax Rules:**
- Separator: `/`
- Array index: `Orders/0`, `Orders/1`
- Wildcard: `Orders/*`
- Recursive: `**/LineItems`

**Implementation:**

```csharp
public class XPathNavigator : IPathNavigator
{
    public string NavigatorType => "xpath";

    public ICanonicalPath Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        var isAbsolute = path.StartsWith("/");
        var pathToParse = isAbsolute ? path.Substring(1) : path;

        var segments = new List<IPathSegment>();

        foreach (var part in pathToParse.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            // Recursive descent
            if (part == "**")
            {
                segments.Add(PathSegment.RecursiveDescent());
                continue;
            }

            // Wildcard
            if (part == "*")
            {
                segments.Add(PathSegment.Wildcard());
                continue;
            }

            // Array index (numeric)
            if (int.TryParse(part, out var index))
            {
                segments.Add(PathSegment.ArrayIndex(index));
                continue;
            }

            // Property
            segments.Add(PathSegment.Property(part));
        }

        return new CanonicalPath(isAbsolute, segments.ToArray());
    }

    public string Format(ICanonicalPath canonical)
    {
        var prefix = canonical.IsAbsolute ? "/" : "";
        var parts = canonical.Segments.Select(segment =>
        {
            return segment.Type switch
            {
                PathSegmentType.Property => segment.Value,
                PathSegmentType.ArrayIndex => segment.Index?.ToString(),
                PathSegmentType.Wildcard => "*",
                PathSegmentType.RecursiveDescent => "**",
                _ => ""
            };
        });

        return prefix + string.Join("/", parts);
    }

    public bool CanParse(string path)
    {
        // XPath: contains / but not $ or . (unless numeric like 0.5)
        if (path.StartsWith("$"))
            return false;

        if (path.Contains("/"))
            return true;

        // Could be single property name
        return !path.Contains(".");
    }
}
```

---

### 4. JSONPathNavigator (Another Navigator)

**Syntax Rules:**
- Root: `$`
- Property: `.Customer` or `["Customer"]`
- Array index: `[0]`, `[1]`
- Wildcard: `[*]`
- Recursive: `..` (double dot)

**Implementation:**

```csharp
public class JSONPathNavigator : IPathNavigator
{
    public string NavigatorType => "jsonpath";

    public ICanonicalPath Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        // Must start with $ for absolute paths
        var isAbsolute = path.StartsWith("$");
        var pathToParse = isAbsolute ? path.Substring(1) : path;

        var segments = new List<IPathSegment>();
        var i = 0;

        while (i < pathToParse.Length)
        {
            // Recursive descent: ..
            if (pathToParse.Substring(i).StartsWith(".."))
            {
                segments.Add(PathSegment.RecursiveDescent());
                i += 2;
                continue;
            }

            // Bracket notation: [0], [*], ["property"]
            if (pathToParse[i] == '[')
            {
                var endBracket = pathToParse.IndexOf(']', i);
                if (endBracket == -1)
                    throw new ArgumentException($"Unclosed bracket at position {i}");

                var bracketContent = pathToParse.Substring(i + 1, endBracket - i - 1);

                // Wildcard
                if (bracketContent == "*")
                {
                    segments.Add(PathSegment.Wildcard());
                }
                // Array index
                else if (int.TryParse(bracketContent, out var index))
                {
                    segments.Add(PathSegment.ArrayIndex(index));
                }
                // Property (quoted)
                else if (bracketContent.StartsWith("\"") && bracketContent.EndsWith("\""))
                {
                    var propName = bracketContent.Trim('"');
                    segments.Add(PathSegment.Property(propName));
                }

                i = endBracket + 1;
                continue;
            }

            // Dot notation: .Customer
            if (pathToParse[i] == '.')
            {
                i++; // Skip dot
                var nextDot = pathToParse.IndexOfAny(new[] { '.', '[' }, i);
                var propName = nextDot == -1
                    ? pathToParse.Substring(i)
                    : pathToParse.Substring(i, nextDot - i);

                if (!string.IsNullOrEmpty(propName))
                {
                    segments.Add(PathSegment.Property(propName));
                }

                i += propName.Length;
                continue;
            }

            i++;
        }

        return new CanonicalPath(isAbsolute, segments.ToArray());
    }

    public string Format(ICanonicalPath canonical)
    {
        var sb = new StringBuilder();

        if (canonical.IsAbsolute)
        {
            sb.Append('$');
        }

        foreach (var segment in canonical.Segments)
        {
            switch (segment.Type)
            {
                case PathSegmentType.Property:
                    sb.Append('.').Append(segment.Value);
                    break;

                case PathSegmentType.ArrayIndex:
                    sb.Append('[').Append(segment.Index).Append(']');
                    break;

                case PathSegmentType.Wildcard:
                    sb.Append("[*]");
                    break;

                case PathSegmentType.RecursiveDescent:
                    sb.Append("..");
                    break;
            }
        }

        return sb.ToString();
    }

    public bool CanParse(string path)
    {
        // JSONPath starts with $
        return path.StartsWith("$");
    }
}
```

---

### 5. DotNotationNavigator (Yet Another Navigator)

**Syntax Rules:**
- Property: `Customer.Address.City`
- Array index: `Orders.0.Total` (numeric segment)
- Wildcard: `Orders.*.Total`
- Recursive: `Customer.**.LineItems`

**Implementation:**

```csharp
public class DotNotationNavigator : IPathNavigator
{
    public string NavigatorType => "dotnotation";

    public ICanonicalPath Parse(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be empty", nameof(path));

        // Dot notation is always relative
        var isAbsolute = false;
        var segments = new List<IPathSegment>();

        var parts = path.Split('.');

        for (int i = 0; i < parts.Length; i++)
        {
            var part = parts[i];

            // Recursive descent (**. )
            if (part == "**")
            {
                segments.Add(PathSegment.RecursiveDescent());
                continue;
            }

            // Wildcard
            if (part == "*")
            {
                segments.Add(PathSegment.Wildcard());
                continue;
            }

            // Array index (numeric)
            if (int.TryParse(part, out var index))
            {
                segments.Add(PathSegment.ArrayIndex(index));
                continue;
            }

            // Property
            segments.Add(PathSegment.Property(part));
        }

        return new CanonicalPath(isAbsolute, segments.ToArray());
    }

    public string Format(ICanonicalPath canonical)
    {
        var parts = canonical.Segments.Select(segment =>
        {
            return segment.Type switch
            {
                PathSegmentType.Property => segment.Value,
                PathSegmentType.ArrayIndex => segment.Index?.ToString(),
                PathSegmentType.Wildcard => "*",
                PathSegmentType.RecursiveDescent => "**",
                _ => ""
            };
        });

        return string.Join(".", parts);
    }

    public bool CanParse(string path)
    {
        // Dot notation: contains . but not $ or /
        return path.Contains(".") && !path.StartsWith("$") && !path.Contains("/");
    }
}
```

---

### 6. PathTranslationService (Orchestrator)

**Responsibilities:**
- Register navigators
- Auto-detect syntax
- Translate between syntaxes

**Implementation:**

```csharp
public class PathTranslationService : IPathTranslationService
{
    private readonly Dictionary<string, IPathNavigator> _navigators;

    public PathTranslationService()
    {
        _navigators = new Dictionary<string, IPathNavigator>(StringComparer.OrdinalIgnoreCase);

        // Register built-in navigators
        RegisterNavigator(new XPathNavigator());
        RegisterNavigator(new JSONPathNavigator());
        RegisterNavigator(new DotNotationNavigator());
    }

    public void RegisterNavigator(IPathNavigator navigator)
    {
        _navigators[navigator.NavigatorType] = navigator;
    }

    public string Translate(string path, string sourceSyntax, string targetSyntax)
    {
        // 1. Get source navigator
        if (!_navigators.TryGetValue(sourceSyntax, out var sourceNavigator))
            throw new ArgumentException($"Unknown source syntax: {sourceSyntax}");

        // 2. Get target navigator
        if (!_navigators.TryGetValue(targetSyntax, out var targetNavigator))
            throw new ArgumentException($"Unknown target syntax: {targetSyntax}");

        // 3. Parse source to canonical
        var canonical = sourceNavigator.Parse(path);

        // 4. Format canonical to target
        return targetNavigator.Format(canonical);
    }

    public ICanonicalPath ParseAny(string path)
    {
        // Try each navigator until one can parse it
        foreach (var navigator in _navigators.Values)
        {
            if (navigator.CanParse(path))
            {
                return navigator.Parse(path);
            }
        }

        // Default: try dot notation
        return _navigators["dotnotation"].Parse(path);
    }

    public string Format(ICanonicalPath canonical, string targetSyntax)
    {
        if (!_navigators.TryGetValue(targetSyntax, out var navigator))
            throw new ArgumentException($"Unknown syntax: {targetSyntax}");

        return navigator.Format(canonical);
    }

    public IPathNavigator? GetNavigator(string navigatorType)
    {
        return _navigators.TryGetValue(navigatorType, out var navigator)
            ? navigator
            : null;
    }
}
```

---

## Data Flow

### Translation Flow

```
┌──────────┐
│  XPath   │   "Customer/Orders/0/Total"
│  String  │
└────┬─────┘
     │ Parse(xpath)
     ↓
┌──────────────────────────┐
│   ICanonicalPath         │
│                          │
│ Segments:                │
│  [0] Property: Customer  │
│  [1] Property: Orders    │
│  [2] ArrayIndex: 0       │
│  [3] Property: Total     │
└────┬─────────────────────┘
     │ Format(jsonpath)
     ↓
┌──────────┐
│JSONPath  │   "$.Customer.Orders[0].Total"
│ String   │
└──────────┘
```

**Key Points:**
1. Source syntax parsed to canonical (syntax-agnostic)
2. Canonical format stored/manipulated internally
3. Canonical formatted to target syntax on output
4. No direct source→target translation (always through canonical)

---

## Design Patterns

### 1. Strategy Pattern
- IPathNavigator is the strategy interface
- XPathNavigator, JSONPathNavigator, DotNotationNavigator are concrete strategies
- PathTranslationService is the context

### 2. Value Object Pattern
- CanonicalPath is immutable
- Equality based on value, not identity
- Segments are immutable once created

### 3. Factory Pattern
- PathSegment factory methods (Property, ArrayIndex, etc.)
- Encapsulates segment creation logic

---

## Integration with Data Container

### Container Uses Canonical Paths Internally

```csharp
public class DataContainer : IDataContainer
{
    private readonly PathTranslationService _translation;
    private readonly Dictionary<ICanonicalPath, DataNode> _nodeCache;

    public IDataNode Navigate(string path)
    {
        // 1. Auto-detect syntax and parse to canonical
        var canonicalPath = _translation.ParseAny(path);

        // 2. Check cache using canonical path
        if (_nodeCache.TryGetValue(canonicalPath, out var cachedNode))
            return cachedNode;

        // 3. Create node with canonical path
        var node = new DataNode(this, canonicalPath);

        // 4. Cache
        _nodeCache[canonicalPath] = node;

        return node;
    }

    public void RegisterProvider(string pathPattern, IDataProvider provider)
    {
        // Parse pattern to canonical
        var canonicalPattern = _translation.ParseAny(pathPattern);

        // Store with canonical pattern
        _providers[canonicalPattern] = provider;
    }
}
```

**Benefits:**
- Container doesn't care about syntax
- Cache works across syntaxes (XPath and JSONPath for same data use same cached node)
- Provider matching uses canonical comparison

---

## Performance Optimizations

### 1. Parse Caching
```csharp
public class CachedPathTranslationService : IPathTranslationService
{
    private readonly IPathTranslationService _inner;
    private readonly ConcurrentDictionary<string, ICanonicalPath> _parseCache;

    public ICanonicalPath ParseAny(string path)
    {
        return _parseCache.GetOrAdd(path, p => _inner.ParseAny(p));
    }
}
```

### 2. Navigator Selection Optimization
```csharp
// Fast path for common cases
if (path.StartsWith("$"))
    return _jsonPathNavigator.Parse(path);
else if (path.Contains("/"))
    return _xpathNavigator.Parse(path);
else
    return _dotNotationNavigator.Parse(path);
```

---

## Error Handling

### Parse Errors
```csharp
public class PathParseException : Exception
{
    public string Path { get; }
    public string NavigatorType { get; }
    public int? Position { get; }

    public PathParseException(string message, string path, string navigatorType, int? position = null)
        : base(message)
    {
        Path = path;
        NavigatorType = navigatorType;
        Position = position;
    }
}
```

### Translation Errors
```csharp
try
{
    var translated = _translation.Translate(
        "$.Customer.Orders[0].Total",
        "jsonpath",
        "xpath");
}
catch (PathParseException ex)
{
    Console.WriteLine($"Failed to parse {ex.Path} as {ex.NavigatorType} at position {ex.Position}");
}
```

---

## Testing Strategy

### Unit Tests
- Parse each syntax to canonical (verify segments)
- Format canonical to each syntax
- Round-trip tests (parse → format → parse)
- Edge cases (empty paths, wildcards, recursive descent)

### Integration Tests
- Cross-syntax translation (all pairs)
- DataContainer with different syntaxes
- Template engine integration

### Example Test
```csharp
[TestMethod]
public void Translate_XPathToJSONPath_CorrectFormat()
{
    // Arrange
    var service = new PathTranslationService();
    var xpath = "Customer/Orders/0/Total";

    // Act
    var jsonpath = service.Translate(xpath, "xpath", "jsonpath");

    // Assert
    Assert.AreEqual("$.Customer.Orders[0].Total", jsonpath);
}

[TestMethod]
public void ParseAny_AutoDetect_CorrectNavigator()
{
    // Arrange
    var service = new PathTranslationService();

    // Act & Assert
    var canonical1 = service.ParseAny("$.Customer.Name");  // JSONPath
    var canonical2 = service.ParseAny("Customer/Name");    // XPath
    var canonical3 = service.ParseAny("Customer.Name");    // Dot Notation

    // All should produce same canonical path
    Assert.AreEqual(canonical1, canonical2);
    Assert.AreEqual(canonical2, canonical3);
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container Architecture](../CoreContainer/architecture.md)
