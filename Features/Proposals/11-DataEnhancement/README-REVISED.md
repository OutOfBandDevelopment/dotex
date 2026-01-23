# Epic 11: Data Enhancement Pipeline (REVISED)

**Priority:** HIGH
**Status:** 📋 Design Phase (Architecture Revised)
**Complexity:** MEDIUM
**Impact:** ~600 LOC (increased scope - more powerful)

---

## Overview

The Data Enhancement Pipeline is a **generic, lazy-evaluated data container framework** with **XPath-like navigation** for enriching data through attribute-discovered providers.

**Key Innovation:** Data is **NOT loaded** until actually accessed, enabling efficient enhancement of large object graphs where only specific paths are needed.

**Key Principle:** This is a **general-purpose framework** for ANY data scenario, NOT specific to messages, templates, or any domain.

---

## Core Concepts

### XPath-Like Navigation
```csharp
var container = DataContainerFactory.Create();

// Navigate to nodes (lazy - doesn't load data)
var addressNode = container.Navigate("Customer/Address");
var cityNode = addressNode.Navigate("City");

// Evaluate (triggers data loading)
var city = container.Evaluate<string>("Customer/Address/City");
```

### Lazy Evaluation
```csharp
// Register enhancement provider for "Order/LineItems"
[EnhancementPath("Order/LineItems")]
public class LineItemsProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(IDataNode node, ...)
    {
        // ONLY executes if template/code accesses "Order/LineItems"
        return await _orderRepository.GetLineItemsAsync(orderId);
    }
}

// Template uses it
var template = "Total items: {{Order/LineItems/Count}}";

// Engine evaluates "Order/LineItems/Count"
// → Navigates to "Order/LineItems" node
// → Triggers LineItemsProvider.ProvideAsync()
// → Gets count from result
```

### Deferred Data Loading
```csharp
// Enhancement pipeline builds navigation tree
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Customer/Orders", ordersProvider);
container.RegisterProvider("Customer/Orders/*/LineItems", lineItemsProvider);

// Template only needs customer name
var template = "Hello {{Customer/FirstName}}";

// ONLY customerProvider executes
// ordersProvider and lineItemsProvider NEVER execute (not needed)
```

---

## Architecture Components

### IDataContainer - Main Container

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Container for hierarchical data with XPath-like navigation and lazy evaluation.
/// </summary>
public interface IDataContainer
{
    /// <summary>
    /// Root node of the data tree.
    /// </summary>
    IDataNode Root { get; }

    /// <summary>
    /// Navigates to a node at the specified path (lazy - doesn't load data).
    /// </summary>
    /// <param name="path">XPath-like path (e.g., "Customer/Address/City")</param>
    /// <returns>Data node (may not have value loaded yet)</returns>
    IDataNode Navigate(string path);

    /// <summary>
    /// Evaluates a path and returns the value (triggers data loading).
    /// </summary>
    /// <param name="path">XPath-like path</param>
    /// <returns>Value at path or null if not found</returns>
    object? Evaluate(string path);

    /// <summary>
    /// Evaluates a path and returns a strongly-typed value.
    /// </summary>
    T? Evaluate<T>(string path);

    /// <summary>
    /// Checks if a path exists (without loading data).
    /// </summary>
    bool PathExists(string path);

    /// <summary>
    /// Registers a data provider for a specific path pattern.
    /// </summary>
    /// <param name="pathPattern">Path pattern (e.g., "Order/LineItems", "Customer/*")</param>
    /// <param name="provider">Provider that loads data when path is accessed</param>
    void RegisterProvider(string pathPattern, IDataProvider provider);

    /// <summary>
    /// Serializes the container to JSON (evaluates all registered providers).
    /// </summary>
    string ToJson();

    /// <summary>
    /// Converts to dictionary (evaluates all registered providers).
    /// </summary>
    IDictionary<string, object?> ToDictionary();
}
```

### IDataNode - Navigation Node

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Represents a node in the data tree with lazy evaluation.
/// </summary>
public interface IDataNode
{
    /// <summary>
    /// Full path to this node (e.g., "Customer/Address/City").
    /// </summary>
    string Path { get; }

    /// <summary>
    /// Local name of this node (e.g., "City").
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Node type (Value, Object, Array).
    /// </summary>
    DataNodeType NodeType { get; }

    /// <summary>
    /// Gets the value at this node (triggers data loading if not yet loaded).
    /// </summary>
    object? Value { get; }

    /// <summary>
    /// Gets a strongly-typed value at this node.
    /// </summary>
    T? GetValue<T>();

    /// <summary>
    /// Checks if this node has a value loaded.
    /// </summary>
    bool HasValue { get; }

    /// <summary>
    /// Navigates to a child node using relative path.
    /// </summary>
    /// <param name="relativePath">Relative path (e.g., "Address/City")</param>
    IDataNode? SelectSingleNode(string relativePath);

    /// <summary>
    /// Selects multiple child nodes matching a pattern.
    /// </summary>
    /// <param name="pattern">Path pattern (e.g., "Orders/*" or "Orders/*/LineItems")</param>
    IEnumerable<IDataNode> SelectNodes(string pattern);

    /// <summary>
    /// Parent node (null for root).
    /// </summary>
    IDataNode? Parent { get; }

    /// <summary>
    /// Child nodes (lazy - may not be loaded yet).
    /// </summary>
    IEnumerable<IDataNode> Children { get; }

    /// <summary>
    /// Sets the value at this node.
    /// </summary>
    void SetValue(object? value);

    /// <summary>
    /// Creates a child node with the specified name.
    /// </summary>
    IDataNode CreateChild(string name);

    /// <summary>
    /// Removes this node from its parent.
    /// </summary>
    void Remove();
}
```

### IDataProvider - Lazy Data Provider

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Provides data for a specific path when accessed.
/// Decorated with [EnhancementPath] attribute for discovery.
/// </summary>
public interface IDataProvider
{
    /// <summary>
    /// Provides data for the specified node.
    /// ONLY called when node.Value is accessed.
    /// </summary>
    /// <param name="node">Node being accessed</param>
    /// <param name="context">Enhancement context (e.g., "order.confirmation")</param>
    /// <param name="metadata">Optional metadata (correlation ID, user ID, etc.)</param>
    /// <returns>Data for this node</returns>
    Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata = null);
}
```

### [EnhancementPath] Attribute

```csharp
namespace OoBDev.Data.Abstractions;

/// <summary>
/// Marks an IDataProvider for automatic discovery.
/// Supports path patterns with wildcards.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EnhancementPathAttribute : Attribute
{
    /// <summary>
    /// Path pattern this provider handles (e.g., "Customer/Orders", "Order/*/LineItems")
    /// Supports wildcards: * (single level), ** (multiple levels)
    /// </summary>
    public string PathPattern { get; set; } = "";

    /// <summary>
    /// Context filter (optional) - only applies to specific contexts
    /// e.g., "order.confirmation" or "*" for all contexts
    /// </summary>
    public string Context { get; set; } = "*";

    /// <summary>
    /// Execution order (lower = earlier)
    /// </summary>
    public int Order { get; set; } = 0;
}
```

---

## Usage Examples

### Example 1: Basic Navigation

```csharp
// Create container with initial data
var container = DataContainerFactory.Create(new
{
    OrderId = 12345,
    CustomerId = Guid.NewGuid()
});

// Navigate (lazy - no data loading)
var customerNode = container.Navigate("Customer");
var addressNode = customerNode.SelectSingleNode("Address");

// Evaluate (triggers data loading)
var city = container.Evaluate<string>("Customer/Address/City");
var zipCode = addressNode.Navigate("ZipCode").GetValue<string>();

// Check existence without loading
if (container.PathExists("Customer/Phone"))
{
    var phone = container.Evaluate<string>("Customer/Phone");
}
```

### Example 2: Lazy Data Providers

```csharp
// Provider for customer data
[EnhancementPath("Customer", Context = "order.confirmation", Order = 0)]
public class CustomerDataProvider : IDataProvider
{
    private readonly ICustomerRepository _customers;

    public async Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata)
    {
        // Get CustomerId from parent node
        var customerId = node.Parent!.Evaluate<Guid>("CustomerId");

        var customer = await _customers.GetByIdAsync(customerId);

        return new
        {
            customer.Email,
            customer.FirstName,
            customer.LastName,
            customer.Phone
        };
    }
}

// Provider for order details (only loads if accessed)
[EnhancementPath("Order", Context = "order.confirmation", Order = 1)]
public class OrderDataProvider : IDataProvider
{
    private readonly IOrderRepository _orders;

    public async Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata)
    {
        var orderId = node.Parent!.Evaluate<int>("OrderId");

        var order = await _orders.GetByIdAsync(orderId);

        return new
        {
            order.OrderNumber,
            order.Total,
            order.CreatedAt,
            order.EstimatedDeliveryDate
        };
    }
}

// Provider for line items (ONLY loads if template accesses it)
[EnhancementPath("Order/LineItems", Context = "order.confirmation", Order = 2)]
public class LineItemsDataProvider : IDataProvider
{
    private readonly IOrderRepository _orders;

    public async Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata)
    {
        var orderId = node.Parent!.Evaluate<int>("OrderId");

        var lineItems = await _orders.GetLineItemsAsync(orderId);

        return lineItems.Select(li => new
        {
            li.ProductName,
            li.Quantity,
            li.UnitPrice,
            Total = li.Quantity * li.UnitPrice
        });
    }
}
```

### Example 3: Lazy Evaluation in Templates

```csharp
// Template 1: Only needs customer name
var template1 = "Hello {{Customer/FirstName}}!";

// Container setup
var container = DataContainerFactory.Create(new { CustomerId = userId });
container.RegisterProvider("Customer", new CustomerDataProvider());
container.RegisterProvider("Order", new OrderDataProvider());
container.RegisterProvider("Order/LineItems", new LineItemsDataProvider());

// Render template 1
var result1 = _templateEngine.Render(template1, container);
// ONLY CustomerDataProvider executes (FirstName accessed)
// OrderDataProvider and LineItemsDataProvider NEVER execute

// Template 2: Needs order details
var template2 = "Order #{{Order/OrderNumber}} total: ${{Order/Total}}";

var result2 = _templateEngine.Render(template2, container);
// CustomerDataProvider executed (if not cached from template1)
// OrderDataProvider executed (OrderNumber and Total accessed)
// LineItemsDataProvider STILL doesn't execute (not accessed)

// Template 3: Needs line items
var template3 = @"
Order #{{Order/OrderNumber}}
Line Items:
{{#each Order/LineItems}}
  - {{ProductName}}: {{Quantity}} x ${{UnitPrice}}
{{/each}}
Total: ${{Order/Total}}
";

var result3 = _templateEngine.Render(template3, container);
// NOW all three providers execute
```

### Example 4: Wildcards in Path Patterns

```csharp
// Provider for ANY customer address
[EnhancementPath("Customer/*/Address")]
public class AddressProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata)
    {
        // node.Path could be "Customer/Shipping/Address" or "Customer/Billing/Address"
        var customerId = node.Navigate("../../CustomerId").GetValue<Guid>();
        var addressType = node.Parent!.Name; // "Shipping" or "Billing"

        var address = await _addressRepository.GetAsync(customerId, addressType);

        return new
        {
            address.Street,
            address.City,
            address.State,
            address.ZipCode
        };
    }
}

// Usage
var city = container.Evaluate<string>("Customer/Shipping/Address/City");  // Triggers provider
var zip = container.Evaluate<string>("Customer/Billing/Address/ZipCode"); // Also triggers provider
```

### Example 5: Collection Navigation

```csharp
// Provider for order collection
[EnhancementPath("Customer/Orders")]
public class CustomerOrdersProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata)
    {
        var customerId = node.Parent!.Evaluate<Guid>("CustomerId");

        var orders = await _orderRepository.GetByCustomerIdAsync(customerId);

        return orders.Select(o => new { o.Id, o.OrderNumber, o.Total, o.CreatedAt });
    }
}

// Provider for individual order line items (nested)
[EnhancementPath("Customer/Orders/*/LineItems")]
public class OrderLineItemsProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata)
    {
        // Navigate up to find OrderId
        var orderId = node.Parent!.Evaluate<int>("Id");

        var lineItems = await _orderRepository.GetLineItemsAsync(orderId);

        return lineItems.Select(li => new { li.ProductName, li.Quantity, li.UnitPrice });
    }
}

// Usage
var container = DataContainerFactory.Create(new { CustomerId = userId });

// Access nested collection
var firstOrderTotal = container.Evaluate<decimal>("Customer/Orders/0/Total");

// Access deeply nested collection (triggers both providers)
var firstLineItemName = container.Evaluate<string>("Customer/Orders/0/LineItems/0/ProductName");

// Select all orders
var orderNodes = container.Root.SelectNodes("Customer/Orders/*");
foreach (var orderNode in orderNodes)
{
    var orderNumber = orderNode.GetValue<string>("OrderNumber");
    var total = orderNode.GetValue<decimal>("Total");
}
```

---

## Benefits of Lazy Evaluation

### Performance
```csharp
// Template only needs customer name
var template = "Welcome back, {{Customer/FirstName}}!";

// WITHOUT lazy evaluation:
// - Load customer (1 DB query)
// - Load order (1 DB query) ❌ NOT NEEDED
// - Load line items (1 DB query) ❌ NOT NEEDED
// - Load shipping address (1 DB query) ❌ NOT NEEDED
// Total: 4 DB queries, 3 wasted

// WITH lazy evaluation:
// - Load customer (1 DB query) ✅ ONLY THIS
// Total: 1 DB query
```

### Memory Efficiency
```csharp
// Large dataset scenario
[EnhancementPath("Report/Sales")]
public class SalesDataProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(...)
    {
        // Load 100,000 sales records
        return await _repository.GetAllSalesAsync();
    }
}

// Template only needs COUNT
var template = "Total sales: {{Report/Sales/Count}}";

// WITH lazy evaluation + LINQ:
container.RegisterProvider("Report/Sales", new SalesDataProvider());

// Template engine evaluates "Report/Sales/Count"
// Provider returns IQueryable/IAsyncEnumerable
// ONLY COUNT is executed on database
// 100,000 records NEVER loaded into memory
```

---

## Feature Breakdown

### Feature 1: Core Container & Navigation
**Path:** `./CoreContainer/`
**Description:** IDataContainer and IDataNode implementation with XPath-like navigation

**Documentation:**
- [Requirements](./CoreContainer/requirements.md)
- [Architecture](./CoreContainer/architecture.md)
- [API Design](./CoreContainer/api-design.md)
- [Business Rules](./CoreContainer/business-rules.md)
- [Testing Strategy](./CoreContainer/testing-strategy.md)

### Feature 2: Lazy Data Providers
**Path:** `./LazyProviders/`
**Description:** IDataProvider abstraction with attribute discovery and lazy evaluation

**Documentation:**
- [Requirements](./LazyProviders/requirements.md)
- [Architecture](./LazyProviders/architecture.md)
- [API Design](./LazyProviders/api-design.md)

### Feature 3: Path Pattern Matching
**Path:** `./PathMatching/`
**Description:** Wildcard path pattern matching for provider registration

**Documentation:**
- [Requirements](./PathMatching/requirements.md)
- [Architecture](./PathMatching/architecture.md)

---

## Key Design Decisions

### 1. XPath-Like Syntax
**Decision:** Use `/` separators instead of `.` (dot notation)

**Rationale:**
- More consistent with XPath/XML navigation
- Avoids conflicts with property names containing dots
- Clearer distinction between navigation and property access

**Examples:**
- `"Customer/Address/City"` ✅
- `"Customer.Address.City"` ❌

### 2. Lazy Evaluation by Default
**Decision:** Providers ONLY execute when path is accessed

**Rationale:**
- Massive performance improvement for large datasets
- Reduces memory footprint
- Supports streaming scenarios (IAsyncEnumerable)
- Enables efficient template rendering

### 3. Wildcard Path Patterns
**Decision:** Support `*` (single level) and `**` (multiple levels) wildcards

**Rationale:**
- Single provider can handle multiple paths
- Example: `"Customer/*/Address"` matches `"Customer/Shipping/Address"` and `"Customer/Billing/Address"`
- Example: `"Order/**/LineItems"` matches any depth

### 4. Node Types
**Decision:** Support Value, Object, and Array node types

**Rationale:**
- Value: Scalar (string, int, bool)
- Object: Complex object with children
- Array: Collection (supports indexing)

---

## Comparison with Original Design

### Original (IMessageData) ❌
```csharp
var data = MessageDataFactory.Create(new { OrderId = 12345 });

// ALL enhancement providers execute immediately
data = await _enhancement.EnhanceAsync("order.confirmation", data);

// Data fully loaded in memory
var email = data.GetValue<string>("Customer.Email");
```

### New (IDataContainer) ✅
```csharp
var container = DataContainerFactory.Create(new { OrderId = 12345 });

// Register providers (no execution yet)
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Order/LineItems", lineItemsProvider);

// ONLY customerProvider executes (lazy)
var email = container.Evaluate<string>("Customer/Email");
```

---

## Integration with Other Features

### Template Engine (Epic 10)
```csharp
public interface ITemplateEngine
{
    string Render(string template, IDataContainer data);
}

// Template engine evaluates paths as needed
var template = "Hello {{Customer/FirstName}}!";
var result = _templateEngine.Render(template, container);
// ONLY "Customer/FirstName" path evaluated
```

### Communications (Epic 2)
```csharp
public class MessageCompositionService
{
    public async Task<IEmailMessage> ComposeEmailAsync(string messageType, IDataContainer data)
    {
        // Load template
        var template = await _templates.GetEmailTemplateAsync(messageType, culture);

        // Render with lazy container (only loads what template needs)
        var subject = _templateEngine.Render(template.Subject, data);
        var htmlContent = _templateEngine.Render(template.HtmlContent, data);

        return new EmailMessage { Subject = subject, HtmlContent = htmlContent };
    }
}
```

### Reporting (Future)
```csharp
public class ReportingService
{
    public async Task<byte[]> GeneratePdfAsync(string reportType, IDataContainer data)
    {
        // Report template only accesses needed data
        return await _pdfGenerator.GenerateAsync(reportType, data);
    }
}
```

---

## Success Metrics

- ✅ No dependencies on Messages, Templates, or any specific domain
- ✅ Lazy evaluation reduces DB queries by 50%+ for typical scenarios
- ✅ Memory usage reduced by 70%+ for large datasets
- ✅ XPath-like navigation is intuitive
- ✅ Wildcard patterns reduce provider count by 50%+
- ✅ 80%+ test coverage
- ✅ < 50ms navigation overhead (excluding provider execution)

---

## Related Documentation

- [Epic Review](../EPIC_REVIEW.md)
- [Architectural Improvements](../ARCHITECTURAL_IMPROVEMENTS.md)
- Epic 10: Text Templating (consumer)
- Epic 2: Communications Platform (consumer via Message Composition)
- Epic 5: Data Loading Pipeline (potential consumer)
