# Path Syntax Translation - Requirements

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Path Syntax Translation
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~250

---

## Overview

Modular path translation system that converts between different path syntaxes (XPath, JSONPath, Dot Notation, etc.) using a canonical internal representation. XPath is **one of many** navigation providers, not the only navigation system.

---

## Business Requirements

### BR-1: Multiple Path Syntaxes
**As a** developer
**I want** to use different path syntaxes based on my preference or template engine
**So that** I can use familiar syntax (XPath `/`, JSONPath `$.`, or Dot `.`)

**Acceptance Criteria:**
- Support XPath syntax (`Customer/Address/City`)
- Support JSONPath syntax (`$.Customer.Address.City`)
- Support Dot Notation syntax (`Customer.Address.City`)
- Extensible to add custom syntaxes
- Syntax auto-detection when possible

---

### BR-2: Canonical Path Representation
**As a** framework
**I want** a canonical internal path representation
**So that** different syntaxes can be normalized and compared

**Acceptance Criteria:**
- Canonical representation is syntax-agnostic
- All syntaxes translate to/from canonical form
- Canonical paths support: properties, array indices, wildcards, recursive descent
- Internal container uses canonical paths

---

### BR-3: Bidirectional Translation
**As a** developer
**I want** to convert between any two path syntaxes
**So that** I can use XPath data with JSONPath templates (or vice versa)

**Acceptance Criteria:**
- Translate XPath ↔ JSONPath
- Translate XPath ↔ Dot Notation
- Translate JSONPath ↔ Dot Notation
- Translate Custom ↔ Canonical
- Preserve semantics during translation

---

### BR-4: Template Engine Integration
**As a** template engine
**I want** to use my native path syntax
**So that** templates work with data containers without manual conversion

**Acceptance Criteria:**
- Handlebars uses Dot Notation (`{{Customer.FirstName}}`)
- XSLT uses XPath (`<xsl:value-of select="Customer/FirstName"/>`)
- Custom templates use registered syntax
- Automatic translation to canonical paths

---

## Technical Requirements

### TR-1: Path Navigator Abstraction

**XPath is ONE OF MANY navigation providers, not THE navigation system.**

```csharp
/// <summary>
/// Path navigator abstraction.
/// Implementations: XPathNavigator, JSONPathNavigator, DotNotationNavigator, etc.
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
    ICanonicalPath Parse(string path);

    /// <summary>
    /// Formats canonical path back to this syntax.
    /// </summary>
    string Format(ICanonicalPath canonical);

    /// <summary>
    /// Checks if path string matches this syntax.
    /// </summary>
    bool CanParse(string path);
}
```

---

### TR-2: Canonical Path Structure

```csharp
/// <summary>
/// Canonical internal path representation (syntax-agnostic).
/// </summary>
public interface ICanonicalPath
{
    /// <summary>
    /// Path segments (e.g., ["Customer", "Address", "City"]).
    /// </summary>
    IReadOnlyList<IPathSegment> Segments { get; }

    /// <summary>
    /// Is this an absolute path (vs relative)?
    /// </summary>
    bool IsAbsolute { get; }

    /// <summary>
    /// Combines with another canonical path.
    /// </summary>
    ICanonicalPath Combine(ICanonicalPath other);

    /// <summary>
    /// Gets parent path (removes last segment).
    /// </summary>
    ICanonicalPath? GetParent();

    /// <summary>
    /// Converts to string representation (canonical format).
    /// </summary>
    string ToString();
}

public interface IPathSegment
{
    /// <summary>
    /// Segment type (property, array index, wildcard, recursive descent).
    /// </summary>
    PathSegmentType Type { get; }

    /// <summary>
    /// Property name or wildcard pattern.
    /// </summary>
    string? Value { get; }

    /// <summary>
    /// Array index (for array segments).
    /// </summary>
    int? Index { get; }
}

public enum PathSegmentType
{
    Property,           // Customer, Address, City
    ArrayIndex,         // Orders[0], Orders[1]
    Wildcard,           // Orders/* (any single element)
    RecursiveDescent    // **/LineItems (any depth)
}
```

---

### TR-3: Path Translation Service

```csharp
/// <summary>
/// Translates between different path syntaxes.
/// </summary>
public interface IPathTranslationService
{
    /// <summary>
    /// Registers path navigator.
    /// </summary>
    void RegisterNavigator(IPathNavigator navigator);

    /// <summary>
    /// Translates path from one syntax to another.
    /// </summary>
    string Translate(string path, string sourceSyntax, string targetSyntax);

    /// <summary>
    /// Parses path using any registered navigator (auto-detect syntax).
    /// </summary>
    ICanonicalPath ParseAny(string path);

    /// <summary>
    /// Formats canonical path using specified syntax.
    /// </summary>
    string Format(ICanonicalPath canonical, string targetSyntax);

    /// <summary>
    /// Gets navigator by type.
    /// </summary>
    IPathNavigator? GetNavigator(string navigatorType);
}
```

---

### TR-4: Syntax Examples

**XPath Navigator:**
```
Customer/Address/City           → Property navigation
Customer/Orders/0/Total         → Array indexing
Customer/Orders/*/Total         → Wildcard (all orders)
**/LineItems                    → Recursive descent
```

**JSONPath Navigator:**
```
$.Customer.Address.City         → Property navigation
$.Customer.Orders[0].Total      → Array indexing
$.Customer.Orders[*].Total      → Wildcard (all orders)
$..LineItems                    → Recursive descent
```

**Dot Notation Navigator:**
```
Customer.Address.City           → Property navigation
Customer.Orders.0.Total         → Array indexing
Customer.Orders.*.Total         → Wildcard (all orders)
Customer.**.LineItems           → Recursive descent
```

**Canonical Representation (Internal):**
```
Segments:
  [0] Property: "Customer"
  [1] Property: "Address"
  [2] Property: "City"

Segments:
  [0] Property: "Customer"
  [1] Property: "Orders"
  [2] ArrayIndex: 0
  [3] Property: "Total"
```

---

### TR-5: Translation Matrix

| Source Syntax | Target Syntax | Example |
|--------------|---------------|---------|
| XPath | JSONPath | `Customer/Orders/0` → `$.Customer.Orders[0]` |
| XPath | Dot Notation | `Customer/Orders/0` → `Customer.Orders.0` |
| JSONPath | XPath | `$.Customer.Orders[0]` → `Customer/Orders/0` |
| JSONPath | Dot Notation | `$.Customer.Orders[0]` → `Customer.Orders.0` |
| Dot Notation | XPath | `Customer.Orders.0` → `Customer/Orders/0` |
| Dot Notation | JSONPath | `Customer.Orders.0` → `$.Customer.Orders[0]` |

---

### TR-6: Auto-Detection Rules

```csharp
public class PathSyntaxDetector
{
    public string? DetectSyntax(string path)
    {
        // JSONPath: starts with $
        if (path.StartsWith("$"))
            return "jsonpath";

        // XPath: contains / but not .
        if (path.Contains("/") && !path.Contains("."))
            return "xpath";

        // Dot Notation: contains . but not / or $
        if (path.Contains(".") && !path.Contains("/") && !path.StartsWith("$"))
            return "dotnotation";

        // Default: assume dot notation
        return "dotnotation";
    }
}
```

---

## Non-Functional Requirements

### NFR-1: Performance
- Translation overhead: < 5ms per path
- Canonical parsing: < 10ms per path
- Caching of parsed paths

### NFR-2: Compatibility
- Works with .NET 10.0
- No external dependencies for built-in navigators
- Extensible for custom navigators

### NFR-3: Maintainability
- Clear separation of navigator implementations
- Shared canonical representation
- Well-documented translation rules

---

## Constraints

### C-1: Syntax Limitations
- Not all syntaxes support all features (e.g., Dot Notation may not support recursive descent)
- Translation may be lossy for complex XPath expressions (functions, predicates)
- Array syntax varies by navigator ([0] vs .0)

### C-2: Translation Scope
- Focus on path navigation, not full XPath/JSONPath expressions
- No support for XPath functions (count(), sum(), etc.)
- No support for JSONPath filters ([?(@.price < 10)])

---

## Success Criteria

- ✅ Three navigators implemented: XPath, JSONPath, Dot Notation
- ✅ Bidirectional translation between all pairs
- ✅ Canonical representation supports all common patterns
- ✅ Auto-detection works for 95%+ of paths
- ✅ Template engines use native syntax transparently
- ✅ 80%+ test coverage

---

## Out of Scope

- ❌ Full XPath 1.0/2.0 spec (only path navigation)
- ❌ Full JSONPath spec (only path navigation)
- ❌ XPath functions (count(), sum(), etc.)
- ❌ JSONPath filters ([?(...)])
- ❌ LINQ integration (future enhancement)

---

## Dependencies

### Internal
- Core Container & Navigation (Epic 11)

### External
- .NET 10.0 BCL
- System.Text.RegularExpressions (for parsing)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Core Container Feature](../CoreContainer/requirements.md)
