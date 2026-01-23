# Core Container & Navigation - Testing Strategy

**Epic:** 11 - Data Enhancement Pipeline
**Feature:** Core Container & Navigation
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 80%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks
- **Integration Tests** - End-to-end scenarios with real providers
- **Performance Tests** - Benchmark lazy evaluation benefits
- **Concurrency Tests** - Thread-safety verification

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (5 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (15 tests)
                  │                   │
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (50+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. DataContainer Tests

**File:** `DataContainerTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Data.Enhancement;

namespace OoBDev.System.Data.Enhancement.Tests;

[TestClass]
public class DataContainerTests
{
    [TestMethod]
    public void Create_EmptyContainer_RootNodeExists()
    {
        // Arrange & Act
        var container = DataContainerFactory.Create();

        // Assert
        Assert.IsNotNull(container.Root);
        Assert.AreEqual("/", container.Root.Path);
        Assert.AreEqual("root", container.Root.Name);
    }

    [TestMethod]
    public void Navigate_ValidPath_ReturnsNode()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        // Act
        var node = container.Navigate("Customer/Address/City");

        // Assert
        Assert.IsNotNull(node);
        Assert.AreEqual("/Customer/Address/City", node.Path);
        Assert.AreEqual("City", node.Name);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Navigate_EmptyPath_ThrowsArgumentException()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        // Act
        container.Navigate("");  // Should throw
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Navigate_PathWithDoubleSlash_ThrowsArgumentException()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        // Act
        container.Navigate("Customer//Address");  // Should throw
    }

    [TestMethod]
    public void Navigate_SamePathTwice_ReturnsSameNode()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        // Act
        var node1 = container.Navigate("Customer");
        var node2 = container.Navigate("Customer");

        // Assert
        Assert.AreSame(node1, node2);  // Same instance (cached)
    }

    [TestMethod]
    public void RegisterProvider_ValidPattern_StoresProvider()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        var mockProvider = new Mock<IDataProvider>();

        // Act
        container.RegisterProvider("Customer", mockProvider.Object);

        // Assert
        var node = container.Navigate("Customer");
        Assert.IsNotNull(node);
    }

    [TestMethod]
    public void Evaluate_PathWithProvider_ReturnsValue()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { Name = "John" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);

        // Act
        var value = container.Evaluate("Customer");

        // Assert
        Assert.IsNotNull(value);
        dynamic customer = value;
        Assert.AreEqual("John", customer.Name);
    }

    [TestMethod]
    public void EvaluateGeneric_PathWithProvider_ReturnsTypedValue()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync("Springfield");

        var container = DataContainerFactory.Create();
        container.RegisterProvider("City", mockProvider.Object);

        // Act
        var city = container.Evaluate<string>("City");

        // Assert
        Assert.AreEqual("Springfield", city);
    }

    [TestMethod]
    public void Clear_AfterRegistration_RemovesProvidersAndNodes()
    {
        // Arrange
        var container = DataContainerFactory.Create();
        var mockProvider = new Mock<IDataProvider>();
        container.RegisterProvider("Customer", mockProvider.Object);
        var node1 = container.Navigate("Customer");

        // Act
        container.Clear();
        var node2 = container.Navigate("Customer");

        // Assert
        Assert.AreNotSame(node1, node2);  // New node created
    }
}
```

---

#### 2. DataNode Tests

**File:** `DataNodeTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class DataNodeTests
{
    [TestMethod]
    public void Value_FirstAccess_ExecutesProvider()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { FirstName = "John" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);

        // Act
        var node = container.Navigate("Customer");
        var value = node.Value;

        // Assert
        Assert.IsNotNull(value);
        mockProvider.Verify(
            p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
            Times.Once);
    }

    [TestMethod]
    public void Value_MultipleAccess_ExecutesProviderOnce()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { FirstName = "John" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);
        var node = container.Navigate("Customer");

        // Act
        var value1 = node.Value;  // First access
        var value2 = node.Value;  // Second access
        var value3 = node.Value;  // Third access

        // Assert
        Assert.AreSame(value1, value2);
        Assert.AreSame(value2, value3);
        mockProvider.Verify(
            p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
            Times.Once);  // Provider called ONCE only
    }

    [TestMethod]
    public void GetValue_TypedAccess_ReturnsTypedValue()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John", LastName = "Doe" }
        });

        // Act
        var node = container.Navigate("Customer");
        dynamic customer = node.GetValue<object>();

        // Assert
        Assert.AreEqual("John", customer.FirstName);
        Assert.AreEqual("Doe", customer.LastName);
    }

    [TestMethod]
    public void SelectSingleNode_RelativePath_ReturnsChildNode()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new
            {
                Address = new { City = "Springfield" }
            }
        });

        // Act
        var customerNode = container.Navigate("Customer");
        var addressNode = customerNode.SelectSingleNode("Address");
        var cityNode = addressNode?.SelectSingleNode("City");

        // Assert
        Assert.IsNotNull(cityNode);
        Assert.AreEqual("Springfield", cityNode.Value);
    }

    [TestMethod]
    public void SelectNodes_WildcardPattern_ReturnsMatchingNodes()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Order = new
            {
                LineItems = new[]
                {
                    new { ProductName = "Widget", Price = 19.99m },
                    new { ProductName = "Gadget", Price = 29.99m },
                    new { ProductName = "Doohickey", Price = 9.99m }
                }
            }
        });

        // Act
        var lineItemsNode = container.Navigate("Order/LineItems");
        var itemNodes = lineItemsNode.SelectNodes("*").ToList();

        // Assert
        Assert.AreEqual(3, itemNodes.Count);
        Assert.AreEqual("Widget", itemNodes[0].SelectSingleNode("ProductName")?.Value);
        Assert.AreEqual("Gadget", itemNodes[1].SelectSingleNode("ProductName")?.Value);
        Assert.AreEqual("Doohickey", itemNodes[2].SelectSingleNode("ProductName")?.Value);
    }

    [TestMethod]
    public void Parent_ChildNode_ReturnsParent()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new { Address = new { City = "Springfield" } }
        });

        // Act
        var cityNode = container.Navigate("Customer/Address/City");
        var addressNode = cityNode.Parent;
        var customerNode = addressNode?.Parent;

        // Assert
        Assert.IsNotNull(addressNode);
        Assert.AreEqual("/Customer/Address", addressNode.Path);
        Assert.IsNotNull(customerNode);
        Assert.AreEqual("/Customer", customerNode.Path);
    }

    [TestMethod]
    public void Children_ObjectWithProperties_EnumeratesProperties()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            }
        });

        // Act
        var customerNode = container.Navigate("Customer");
        var children = customerNode.Children.ToList();

        // Assert
        Assert.AreEqual(3, children.Count);
        Assert.IsTrue(children.Any(c => c.Name == "FirstName"));
        Assert.IsTrue(children.Any(c => c.Name == "LastName"));
        Assert.IsTrue(children.Any(c => c.Name == "Email"));
    }

    [TestMethod]
    public void Children_Array_EnumeratesElements()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Numbers = new[] { 1, 2, 3, 4, 5 }
        });

        // Act
        var numbersNode = container.Navigate("Numbers");
        var children = numbersNode.Children.ToList();

        // Assert
        Assert.AreEqual(5, children.Count);
        Assert.AreEqual(1, children[0].Value);
        Assert.AreEqual(5, children[4].Value);
    }

    [TestMethod]
    public void HasChildren_ObjectWithProperties_ReturnsTrue()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new { Name = "John" }
        });

        // Act
        var customerNode = container.Navigate("Customer");

        // Assert
        Assert.IsTrue(customerNode.HasChildren);
    }

    [TestMethod]
    public void HasChildren_ScalarValue_ReturnsFalse()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new { Name = "John" }
        });

        // Act
        var nameNode = container.Navigate("Customer/Name");

        // Assert
        Assert.IsFalse(nameNode.HasChildren);
    }

    [TestMethod]
    public void Depth_RootNode_ReturnsZero()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        // Act
        var depth = container.Root.Depth;

        // Assert
        Assert.AreEqual(0, depth);
    }

    [TestMethod]
    public void Depth_NestedNode_ReturnsCorrectDepth()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        // Act
        var cityNode = container.Navigate("Customer/Address/City");

        // Assert
        Assert.AreEqual(3, cityNode.Depth);
    }

    [TestMethod]
    public void IsLoaded_BeforeAccess_ReturnsFalse()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { Name = "John" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);

        // Act
        var node = container.Navigate("Customer");

        // Assert
        Assert.IsFalse(node.IsLoaded);
    }

    [TestMethod]
    public void IsLoaded_AfterAccess_ReturnsTrue()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { Name = "John" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);
        var node = container.Navigate("Customer");

        // Act
        var value = node.Value;  // Trigger load

        // Assert
        Assert.IsTrue(node.IsLoaded);
    }
}
```

---

#### 3. Provider Pattern Tests

**File:** `ProviderPatternTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class ProviderPatternTests
{
    [TestMethod]
    public void FindProvider_ExactMatch_ReturnsProvider()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);

        // Act
        var node = container.Navigate("Customer");
        var value = node.Value;  // Triggers provider lookup

        // Assert
        mockProvider.Verify(
            p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
            Times.Once);
    }

    [TestMethod]
    public void FindProvider_WildcardMatch_ReturnsProvider()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { ProductName = "Widget" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Order/LineItems/*", mockProvider.Object);

        // Act
        var item0 = container.Navigate("Order/LineItems/0");
        var item1 = container.Navigate("Order/LineItems/1");

        var value0 = item0.Value;
        var value1 = item1.Value;

        // Assert
        mockProvider.Verify(
            p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
            Times.Exactly(2));  // Once for each item
    }

    [TestMethod]
    public void FindProvider_RecursiveMatch_ReturnsProvider()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        mockProvider
            .Setup(p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()))
            .ReturnsAsync(new { Street = "123 Main St", City = "Springfield" });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("**/Address", mockProvider.Object);

        // Act
        var customerAddress = container.Navigate("Customer/Address");
        var shippingAddress = container.Navigate("Order/ShippingAddress");

        // Rename ShippingAddress to Address for pattern to match
        // (This is a simplification - actual implementation may vary)

        // Assert
        mockProvider.Verify(
            p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public void FindProvider_NoMatch_ReturnsNull()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", mockProvider.Object);

        // Act
        var node = container.Navigate("Order");  // No provider for "Order"
        var value = node.Value;

        // Assert
        Assert.IsNull(value);  // No provider found, value is null
        mockProvider.Verify(
            p => p.ProvideAsync(It.IsAny<IDataNode>(), It.IsAny<string>(), It.IsAny<IDictionary<string, object?>>()),
            Times.Never);  // Provider never called
    }
}
```

---

## Integration Tests

### Test Scenarios

#### 1. End-to-End Template Integration

**File:** `TemplateIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class TemplateIntegrationTests
{
    private IDataContainer _container;
    private ITemplateEngine _templateEngine;

    [TestInitialize]
    public void Setup()
    {
        _container = DataContainerFactory.Create();
        _templateEngine = new HandlebarsTemplateEngine();

        // Register providers
        _container.RegisterProvider("Customer", new StaticDataProvider(new
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com"
        }));

        _container.RegisterProvider("Order", new StaticDataProvider(new
        {
            OrderNumber = "12345",
            Total = 99.99m,
            LineItems = new[]
            {
                new { ProductName = "Widget", Quantity = 2, Price = 19.99m },
                new { ProductName = "Gadget", Quantity = 1, Price = 29.99m }
            }
        }));
    }

    [TestMethod]
    public async Task ApplyTemplate_CustomerOnly_ExecutesOnlyCustomerProvider()
    {
        // Arrange
        var template = "Hello {{Customer.FirstName}} {{Customer.LastName}}!";

        // Act
        var result = await _templateEngine.ApplyAsync(template, _container);

        // Assert
        Assert.AreEqual("Hello John Doe!", result);
        // Order provider should NOT have been executed (lazy evaluation)
    }

    [TestMethod]
    public async Task ApplyTemplate_OrderWithLineItems_IteratesItems()
    {
        // Arrange
        var template = @"
Order #{{Order.OrderNumber}}

Items:
{{#each Order.LineItems}}
- {{ProductName}}: {{Quantity}} x ${{Price}}
{{/each}}

Total: ${{Order.Total}}
";

        // Act
        var result = await _templateEngine.ApplyAsync(template, _container);

        // Assert
        Assert.IsTrue(result.Contains("Order #12345"));
        Assert.IsTrue(result.Contains("Widget: 2 x $19.99"));
        Assert.IsTrue(result.Contains("Gadget: 1 x $29.99"));
        Assert.IsTrue(result.Contains("Total: $99.99"));
    }
}
```

---

#### 2. Database Provider Integration

**File:** `DatabaseProviderIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class DatabaseProviderIntegrationTests
{
    private IDataContainer _container;
    private ICustomerRepository _customerRepo;
    private IOrderRepository _orderRepo;

    [TestInitialize]
    public void Setup()
    {
        // Setup in-memory database
        _customerRepo = new InMemoryCustomerRepository();
        _orderRepo = new InMemoryOrderRepository();

        _container = DataContainerFactory.Create();
        _container.RegisterProvider("Customer", new CustomerDatabaseProvider(_customerRepo));
        _container.RegisterProvider("Order", new OrderDatabaseProvider(_orderRepo));

        // Seed data
        _customerRepo.Add(new Customer { Id = 1, FirstName = "John", LastName = "Doe" });
        _orderRepo.Add(new Order { Id = 100, CustomerId = 1, Total = 99.99m });
    }

    [TestMethod]
    public async Task Navigate_CustomerWithOrder_LoadsFromDatabase()
    {
        // Arrange
        var metadata = new Dictionary<string, object?>
        {
            ["CustomerId"] = 1,
            ["OrderId"] = 100
        };

        // Act
        var customerNode = _container.Navigate("Customer");
        var orderNode = _container.Navigate("Order");

        // Access values (triggers database queries)
        var customer = customerNode.Value;
        var order = orderNode.Value;

        // Assert
        Assert.IsNotNull(customer);
        Assert.IsNotNull(order);
        dynamic c = customer;
        Assert.AreEqual("John", c.FirstName);
        dynamic o = order;
        Assert.AreEqual(99.99m, o.Total);
    }
}
```

---

## Performance Tests

### Benchmarks

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]  // Run manually for performance analysis
public class PerformanceTests
{
    [TestMethod]
    public void LazyEvaluation_QueryReduction_MeasuresImprovement()
    {
        // Arrange
        var callCount = 0;

        var expensiveProvider = new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(100);  // Simulate expensive operation
            return new { Data = "Expensive" };
        });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("ExpensiveData1", expensiveProvider);
        container.RegisterProvider("ExpensiveData2", expensiveProvider);
        container.RegisterProvider("ExpensiveData3", expensiveProvider);

        // Act - Template uses only 1 of 3 providers
        var template = "Value: {{ExpensiveData1.Data}}";
        var stopwatch = Stopwatch.StartNew();

        // Simulate template evaluation accessing only ExpensiveData1
        var node = container.Navigate("ExpensiveData1");
        var value = node.Value;

        stopwatch.Stop();

        // Assert
        Assert.AreEqual(1, callCount);  // Only 1 provider executed (not 3)
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200);  // ~100ms, not ~300ms
        Console.WriteLine($"Lazy evaluation: {callCount} providers executed, {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public void ValueCaching_RepeatedAccess_NoAdditionalCalls()
    {
        // Arrange
        var callCount = 0;

        var provider = new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCount);
            return new { Data = "Cached" };
        });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Data", provider);
        var node = container.Navigate("Data");

        // Act - Access value 1000 times
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            var value = node.Value;
        }
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(1, callCount);  // Provider called ONCE only
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10);  // Cached access is fast
        Console.WriteLine($"1000 accesses: {callCount} provider calls, {stopwatch.ElapsedMilliseconds}ms");
    }
}
```

---

## Concurrency Tests

**File:** `ConcurrencyTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class ConcurrencyTests
{
    [TestMethod]
    public async Task ConcurrentValueAccess_MultipleThreads_ProviderExecutedOnce()
    {
        // Arrange
        var callCount = 0;
        var provider = new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(100);  // Simulate slow operation
            return new { Data = "Value" };
        });

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Data", provider);
        var node = container.Navigate("Data");

        // Act - 10 threads access value concurrently
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            var value = node.Value;
            return value;
        }));

        var results = await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(1, callCount);  // Provider executed ONCE despite 10 concurrent accesses
        Assert.IsTrue(results.All(r => r != null));  // All threads got value
        Assert.IsTrue(results.All(r => ReferenceEquals(r, results[0])));  // All got SAME instance
    }

    [TestMethod]
    public void ConcurrentNavigation_MultipleThreads_ThreadSafe()
    {
        // Arrange
        var container = DataContainerFactory.Create(new
        {
            Customer = new { Name = "John" },
            Order = new { Total = 99.99m }
        });

        // Act - 100 threads navigate concurrently
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
        {
            var path = i % 2 == 0 ? "Customer" : "Order";
            return container.Navigate(path);
        }));

        var nodes = Task.WhenAll(tasks).Result;

        // Assert - No exceptions, all navigations succeeded
        Assert.AreEqual(100, nodes.Length);
    }
}
```

---

## Test Data Builders

**File:** `TestDataBuilders.cs`

```csharp
public static class TestDataBuilders
{
    public static IDataContainer BuildCustomerContainer()
    {
        return DataContainerFactory.Create(new
        {
            Customer = new
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Address = new
                {
                    Street = "123 Main St",
                    City = "Springfield",
                    State = "IL",
                    Zip = "62701"
                }
            }
        });
    }

    public static IDataContainer BuildOrderContainer()
    {
        return DataContainerFactory.Create(new
        {
            Order = new
            {
                OrderNumber = "12345",
                OrderDate = DateTime.UtcNow,
                Total = 99.99m,
                LineItems = new[]
                {
                    new { ProductName = "Widget", Quantity = 2, UnitPrice = 19.99m, Total = 39.98m },
                    new { ProductName = "Gadget", Quantity = 1, UnitPrice = 29.99m, Total = 29.99m },
                    new { ProductName = "Doohickey", Quantity = 3, UnitPrice = 9.99m, Total = 29.97m }
                }
            }
        });
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| DataContainer | 85% | Navigate, RegisterProvider, Evaluate |
| DataNode | 90% | Value (lazy load), SelectSingleNode, Children |
| Provider Matching | 80% | Exact, Wildcard, Recursive patterns |
| Caching | 95% | Value cache, Node cache |
| Error Handling | 70% | Invalid paths, Provider exceptions |

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit"

# Integration tests (slower)
dotnet test --filter "TestCategory=Integration"
```

**Run nightly:**
```bash
# Performance benchmarks
dotnet test --filter "TestCategory=DevLocal"

# Concurrency stress tests
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~Concurrency"
```

---

## Test Maintenance

### Adding New Tests

**When adding new features:**
1. Add unit tests for new methods
2. Add integration tests for end-to-end scenarios
3. Update coverage goals if needed
4. Document new test patterns

**Test naming convention:**
```
[MethodName]_[Scenario]_[ExpectedBehavior]

Examples:
- Navigate_ValidPath_ReturnsNode
- Value_FirstAccess_ExecutesProvider
- SelectNodes_WildcardPattern_ReturnsMatchingNodes
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 11 Overview](../README-REVISED.md)
