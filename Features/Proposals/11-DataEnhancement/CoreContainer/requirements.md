# Core Container & Navigation - Requirements

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Core Container & Navigation
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~300

---

## Overview

Generic, XPath-like data container with lazy evaluation support. Works for ANY data scenario (messages, reports, documents, exports) - not message-specific.

---

## Business Requirements

### BR-1: Generic Data Container
**As a** developer
**I want** a generic data container that works for any data scenario
**So that** I can use the same abstraction for messages, reports, documents, and exports

**Acceptance Criteria:**
- Container works with message data (Epic 2, 12)
- Container works with report data (Epic 11)
- Container works with document data (Epic 6)
- Container works with export data
- Container is NOT tied to any specific domain concept

---

### BR-2: XPath-Like Navigation
**As a** developer
**I want** XPath-like navigation with forward-slash separators
**So that** I can navigate data using industry-standard syntax

**Acceptance Criteria:**
- Path syntax uses `/` separators (e.g., `Customer/Address/City`)
- Supports absolute paths (e.g., `/Order/Total`)
- Supports relative paths (e.g., `Address/City`)
- Supports array indexing (e.g., `Orders/0/Total`)
- Supports wildcards (e.g., `Orders/*/Total`)
- Supports recursive descent (e.g., `**/LineItems`)

---

### BR-3: Lazy Evaluation
**As a** system
**I want** data providers to execute ONLY when their paths are accessed
**So that** we reduce unnecessary database queries and API calls

**Acceptance Criteria:**
- Providers registered but NOT executed at registration
- Providers execute when path is navigated/evaluated
- Multiple accesses to same path use cached result
- Performance improvement of 50-70% for typical scenarios

**Example:**
```csharp
// Register providers
container.RegisterProvider("Customer", customerProvider);  // NOT executed
container.RegisterProvider("Order", orderProvider);        // NOT executed

// Template uses ONLY Customer data
var template = "Hello {{Customer/FirstName}}!";

// ONLY customerProvider executes (orderProvider never runs)
var result = await _templateEngine.ApplyAsync(template, container);
```

---

### BR-4: Navigator Pattern
**As a** developer
**I want** an XPathNavigator-like interface for data navigation
**So that** I can traverse data like a tree structure

**Acceptance Criteria:**
- `IDataNode` interface similar to `XPathNavigator`
- Navigate to parent nodes
- Navigate to child nodes
- Select single node by path
- Select multiple nodes by pattern
- Get node value (triggers lazy loading)

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IDataContainer
{
    IDataNode Root { get; }
    IDataNode Navigate(string path);
    object? Evaluate(string path);
    T? Evaluate<T>(string path);
    void RegisterProvider(string pathPattern, IDataProvider provider);
}

public interface IDataNode
{
    string Path { get; }
    string Name { get; }
    object? Value { get; }  // Triggers lazy loading

    IDataNode? SelectSingleNode(string relativePath);
    IEnumerable<IDataNode> SelectNodes(string pattern);

    IDataNode? Parent { get; }
    IEnumerable<IDataNode> Children { get; }
    bool HasChildren { get; }
}

public interface IDataProvider
{
    Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata);
}
```

---

### TR-2: Path Syntax
- **Separator:** `/` (forward slash)
- **Absolute path:** Starts with `/` (e.g., `/Order/Total`)
- **Relative path:** No leading `/` (e.g., `Address/City`)
- **Array index:** Numeric (e.g., `Orders/0`, `Orders/1`)
- **Wildcard:** `*` matches any single segment (e.g., `Orders/*/Total`)
- **Recursive descent:** `**` matches any depth (e.g., `**/LineItems`)

**Examples:**
```
Customer/Address/City               → "123 Main St, Springfield"
Order/LineItems/0/ProductName       → "Widget"
Order/LineItems/*/Price             → [19.99, 29.99, 39.99]
**/Total                            → All "Total" properties at any depth
```

---

### TR-3: Provider Registration Patterns
```csharp
// Exact path
container.RegisterProvider("Customer", customerProvider);

// Wildcard path
container.RegisterProvider("Order/LineItems/*", lineItemProvider);

// Recursive pattern
container.RegisterProvider("**/Address", addressProvider);

// Root-level provider
container.RegisterProvider("/", rootDataProvider);
```

---

### TR-4: Lazy Loading Behavior
**Provider Execution:**
1. Providers registered at startup (NOT executed)
2. Navigation does NOT trigger provider execution
3. Value access triggers provider execution
4. Provider result cached for subsequent accesses

**Example Flow:**
```csharp
// 1. Register providers (NO execution)
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);

// 2. Navigate (NO execution)
var node = container.Navigate("Customer/FirstName");
Assert.That(node, Is.Not.Null);  // Navigation succeeds

// 3. Access value (TRIGGERS execution)
var firstName = node.Value;  // customerProvider executes NOW
Assert.That(firstName, Is.EqualTo("John"));

// 4. Second access (CACHED - no execution)
var firstName2 = node.Value;  // Uses cached result
```

---

### TR-5: Performance Requirements
- **Navigation overhead:** < 10ms per path
- **Provider caching:** 100% cache hit rate for repeated accesses
- **Memory efficiency:** Lazy nodes created on-demand
- **Query reduction:** 50-70% fewer provider executions vs. eager loading

---

### TR-6: Thread Safety
- Container is thread-safe for concurrent reads
- Provider execution is synchronized per path
- Multiple threads accessing same path wait for first execution, then use cached result

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Supports async/await patterns
- Compatible with dependency injection

### NFR-2: Extensibility
- Custom providers can be registered
- Custom path patterns supported
- Custom metadata can be passed to providers

### NFR-3: Testability
- Mock providers for unit testing
- Deterministic behavior for integration tests
- Performance metrics trackable

---

## Constraints

### C-1: Path Syntax Limitations
- Path segments cannot contain `/` (use encoding if needed)
- Array indices must be non-negative integers
- Wildcard `*` matches single segment only
- Recursive `**` expensive for deep hierarchies

### C-2: Provider Constraints
- Providers must be thread-safe
- Provider exceptions propagate to caller
- Providers should cache internally if needed

### C-3: Memory Constraints
- Lazy nodes kept in memory once created
- Provider results cached indefinitely per container instance
- Large data sets should use pagination in providers

---

## Success Criteria

- ✅ Container works for messages, reports, documents, exports
- ✅ XPath-like navigation with `/` separators
- ✅ Lazy evaluation reduces provider executions by 50-70%
- ✅ Navigator pattern similar to `XPathNavigator`
- ✅ Thread-safe concurrent access
- ✅ 80%+ test coverage
- ✅ Performance: < 10ms navigation overhead

---

## Out of Scope

- ❌ XPath expressions (functions, predicates, axes)
- ❌ LINQ integration (future enhancement)
- ❌ Schema validation (use injectable validators)
- ❌ Serialization/deserialization (use separate services)

---

## Dependencies

### Internal
- None (foundation component)

### External
- .NET 10.0 BCL
- System.Collections.Generic
- System.Threading

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 11 Overview](../README-REVISED.md)
