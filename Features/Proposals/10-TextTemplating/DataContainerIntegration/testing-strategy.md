# Data Container Integration - Testing Strategy

**Epic:** 10 - Text Templating Extensions
**Feature:** Data Container Integration
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 80%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (45+ tests)
- **Integration Tests** - End-to-end with template providers (15+ tests)
- **Performance Tests** - Lazy evaluation benchmarks (5+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (5 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (15 tests)
                  │  (Handlebars,XSLT)│
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (45+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### 1. DefaultDataContainerAdapter Tests

**File:** `DefaultDataContainerAdapterTests.cs`

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Data.Enhancement;
using OoBDev.System.Text.Templating.Data;

namespace OoBDev.System.Text.Templating.Tests;

[TestClass]
public class DefaultDataContainerAdapterTests
{
    [TestMethod]
    public void Adapt_ReturnsDataContainerProxy()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        var adapter = new DefaultDataContainerAdapter();

        // Act
        var result = adapter.Adapt(mockContainer.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(DynamicObject));
    }

    [TestMethod]
    public void Adapt_DifferentContainers_ReturnsDifferentProxies()
    {
        // Arrange
        var adapter = new DefaultDataContainerAdapter();
        var container1 = new Mock<IDataContainer>().Object;
        var container2 = new Mock<IDataContainer>().Object;

        // Act
        var proxy1 = adapter.Adapt(container1);
        var proxy2 = adapter.Adapt(container2);

        // Assert
        Assert.AreNotSame(proxy1, proxy2);
    }
}
```

---

### 2. DataContainerProxy Tests

**File:** `DataContainerProxyTests.cs`

```csharp
[TestClass]
public class DataContainerProxyTests
{
    [TestMethod]
    public void TryGetMember_SimpleProperty_CallsEvaluate()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("FirstName"))
            .Returns("John");

        dynamic proxy = new DataContainerProxy(mockContainer.Object);

        // Act
        var result = proxy.FirstName;

        // Assert
        Assert.AreEqual("John", result);
        mockContainer.Verify(c => c.Evaluate("FirstName"), Times.Once);
    }

    [TestMethod]
    public void TryGetMember_NestedProperty_BuildsCorrectPath()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("Customer"))
            .Returns(new { FirstName = "John" });

        mockContainer
            .Setup(c => c.Evaluate("Customer/FirstName"))
            .Returns("John");

        dynamic proxy = new DataContainerProxy(mockContainer.Object);

        // Act
        var customer = proxy.Customer;
        var firstName = customer.FirstName;

        // Assert
        Assert.AreEqual("John", firstName);
        mockContainer.Verify(c => c.Evaluate("Customer"), Times.Once);
        mockContainer.Verify(c => c.Evaluate("Customer/FirstName"), Times.Once);
    }

    [TestMethod]
    public void TryGetMember_ComplexObject_ReturnsNestedProxy()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("Customer"))
            .Returns(new { FirstName = "John", LastName = "Doe" });

        dynamic proxy = new DataContainerProxy(mockContainer.Object);

        // Act
        var customer = proxy.Customer;

        // Assert
        Assert.IsNotNull(customer);
        Assert.IsInstanceOfType(customer, typeof(DataContainerProxy));
    }

    [TestMethod]
    public void TryGetMember_PrimitiveValue_ReturnsDirect()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("Count"))
            .Returns(42);

        dynamic proxy = new DataContainerProxy(mockContainer.Object);

        // Act
        int count = proxy.Count;

        // Assert
        Assert.AreEqual(42, count);
    }

    [TestMethod]
    public void TryGetMember_MultipleAccessesSamePath_CachesResult()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("ExpensiveValue"))
            .Returns("cached");

        dynamic proxy = new DataContainerProxy(mockContainer.Object);

        // Act
        var value1 = proxy.ExpensiveValue;
        var value2 = proxy.ExpensiveValue;
        var value3 = proxy.ExpensiveValue;

        // Assert
        Assert.AreEqual("cached", value1);
        Assert.AreEqual("cached", value2);
        Assert.AreEqual("cached", value3);

        // Verify Evaluate called ONLY ONCE (cached)
        mockContainer.Verify(c => c.Evaluate("ExpensiveValue"), Times.Once);
    }

    [TestMethod]
    public void TryGetIndex_ArrayAccess_BuildsIndexPath()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("Orders/0"))
            .Returns(new { OrderNumber = "12345" });

        dynamic proxy = new DataContainerProxy(mockContainer.Object, "Orders");

        // Act
        var order = proxy[0];

        // Assert
        Assert.IsNotNull(order);
        mockContainer.Verify(c => c.Evaluate("Orders/0"), Times.Once);
    }

    [TestMethod]
    public void TryConvert_ToString_ReturnsStringValue()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("Message"))
            .Returns("Hello World");

        dynamic proxy = new DataContainerProxy(mockContainer.Object, "Message");

        // Act
        string result = (string)proxy;

        // Assert
        Assert.AreEqual("Hello World", result);
    }

    [TestMethod]
    public void TryGetMember_NullValue_ReturnsNull()
    {
        // Arrange
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("OptionalField"))
            .Returns((object?)null);

        dynamic proxy = new DataContainerProxy(mockContainer.Object);

        // Act
        var result = proxy.OptionalField;

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void IsComplexObject_Primitives_ReturnsFalse()
    {
        // Test various primitive types
        Assert.IsFalse(IsComplexObject(42));
        Assert.IsFalse(IsComplexObject(3.14));
        Assert.IsFalse(IsComplexObject(true));
        Assert.IsFalse(IsComplexObject("string"));
        Assert.IsFalse(IsComplexObject(DateTime.Now));
        Assert.IsFalse(IsComplexObject(DateTimeOffset.Now));
    }

    [TestMethod]
    public void IsComplexObject_Objects_ReturnsTrue()
    {
        // Test complex objects
        Assert.IsTrue(IsComplexObject(new { Name = "John" }));
        Assert.IsTrue(IsComplexObject(new List<int> { 1, 2, 3 }));
        Assert.IsTrue(IsComplexObject(new Dictionary<string, int>()));
    }

    // Helper to access private method via reflection
    private static bool IsComplexObject(object? obj)
    {
        var method = typeof(DataContainerProxy).GetMethod(
            "IsComplexObject",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        return (bool)method.Invoke(null, new[] { obj });
    }
}
```

---

### 3. DiagnosticDataContainerAdapter Tests

**File:** `DiagnosticDataContainerAdapterTests.cs`

```csharp
[TestClass]
public class DiagnosticDataContainerAdapterTests
{
    [TestMethod]
    public void Adapt_ReturnsDiagnosticProxy()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DiagnosticDataContainerAdapter>>();
        var mockContainer = new Mock<IDataContainer>();
        var adapter = new DiagnosticDataContainerAdapter(mockLogger.Object);

        // Act
        var result = adapter.Adapt(mockContainer.Object);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(DynamicObject));
    }

    [TestMethod]
    public void TryGetMember_LogsPathAccess()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DiagnosticDataContainerAdapter>>();
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("Customer"))
            .Returns("CustomerData");

        var adapter = new DiagnosticDataContainerAdapter(mockLogger.Object);
        dynamic proxy = adapter.Adapt(mockContainer.Object);

        // Act
        var result = proxy.Customer;

        // Assert
        Assert.AreEqual("CustomerData", result);

        // Verify logging occurred
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Template accessed path: Customer")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [TestMethod]
    public void TryGetMember_LogsEvaluationTiming()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<DiagnosticDataContainerAdapter>>();
        var mockContainer = new Mock<IDataContainer>();
        mockContainer
            .Setup(c => c.Evaluate("SlowData"))
            .Returns(() =>
            {
                Thread.Sleep(10);  // Simulate slow provider
                return "data";
            });

        var adapter = new DiagnosticDataContainerAdapter(mockLogger.Object);
        dynamic proxy = adapter.Adapt(mockContainer.Object);

        // Act
        var result = proxy.SlowData;

        // Assert
        mockLogger.Verify(
            l => l.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("evaluated in") && v.ToString().Contains("ms")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }
}
```

---

## Integration Tests

### 1. Handlebars Integration Tests

**File:** `HandlebarsDataContainerIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class HandlebarsDataContainerIntegrationTests
{
    private ITemplateEngine _templateEngine;
    private IDataContainerAdapter _adapter;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ITemplateEngine, TemplateEngine>();
        services.AddSingleton<ITemplateProvider, HandlebarsTemplateProvider>();
        services.AddSingleton<ITemplateSource, InMemoryTemplateSource>();
        services.AddSingleton<IDataContainerAdapter, DefaultDataContainerAdapter>();

        var serviceProvider = services.BuildServiceProvider();

        _templateEngine = serviceProvider.GetRequiredService<ITemplateEngine>();
        _adapter = serviceProvider.GetRequiredService<IDataContainerAdapter>();
    }

    [TestMethod]
    public async Task ApplyAsync_WithIDataContainer_LazilyEvaluatesData()
    {
        // Arrange
        var customerProviderCalled = false;
        var orderProviderCalled = false;

        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            customerProviderCalled = true;
            return new { FirstName = "John", LastName = "Doe" };
        }));

        container.RegisterProvider("Order", new DelegateDataProvider(async () =>
        {
            orderProviderCalled = true;
            return new { OrderNumber = "12345" };
        }));

        // Template uses ONLY Customer
        var template = "Hello {{Customer/FirstName}}!";
        RegisterTemplate("welcome", template);

        // Act
        var result = await _templateEngine.ApplyAsync("welcome", container);

        // Assert
        Assert.AreEqual("Hello John!", result.Trim());
        Assert.IsTrue(customerProviderCalled);
        Assert.IsFalse(orderProviderCalled);  // Lazy evaluation - not used
    }

    [TestMethod]
    public async Task ApplyAsync_NestedPaths_EvaluatesCorrectly()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new StaticDataProvider(new
        {
            FirstName = "John",
            LastName = "Doe",
            Address = new
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL"
            }
        }));

        var template = @"
{{Customer/FirstName}} {{Customer/LastName}}
{{Customer/Address/Street}}
{{Customer/Address/City}}, {{Customer/Address/State}}
";
        RegisterTemplate("address-label", template);

        // Act
        var result = await _templateEngine.ApplyAsync("address-label", container);

        // Assert
        Assert.IsTrue(result.Contains("John Doe"));
        Assert.IsTrue(result.Contains("123 Main St"));
        Assert.IsTrue(result.Contains("Springfield, IL"));
    }

    [TestMethod]
    public async Task ApplyAsync_ArrayIteration_IteratesCorrectly()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Order", new StaticDataProvider(new
        {
            OrderNumber = "12345",
            LineItems = new[]
            {
                new { ProductName = "Widget", Quantity = 2, Price = 19.99m },
                new { ProductName = "Gadget", Quantity = 1, Price = 29.99m }
            }
        }));

        var template = @"
Order {{Order/OrderNumber}}
{{#each Order/LineItems}}
- {{ProductName}}: {{Quantity}} x ${{Price}}
{{/each}}
";
        RegisterTemplate("order-summary", template);

        // Act
        var result = await _templateEngine.ApplyAsync("order-summary", container);

        // Assert
        Assert.IsTrue(result.Contains("Order 12345"));
        Assert.IsTrue(result.Contains("Widget: 2 x $19.99"));
        Assert.IsTrue(result.Contains("Gadget: 1 x $29.99"));
    }

    [TestMethod]
    public async Task ApplyAsync_ConditionalPaths_EvaluatesOnlyWhenNeeded()
    {
        // Arrange
        var premiumProviderCalled = false;

        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new StaticDataProvider(new
        {
            FirstName = "John",
            IsPremium = false
        }));

        container.RegisterProvider("Premium", new DelegateDataProvider(async () =>
        {
            premiumProviderCalled = true;
            return new { Benefits = "Free shipping, 24/7 support" };
        }));

        var template = @"
Hello {{Customer/FirstName}}!
{{#if Customer/IsPremium}}
Benefits: {{Premium/Benefits}}
{{else}}
Upgrade to premium!
{{/if}}
";
        RegisterTemplate("conditional", template);

        // Act
        var result = await _templateEngine.ApplyAsync("conditional", container);

        // Assert
        Assert.IsTrue(result.Contains("Hello John!"));
        Assert.IsTrue(result.Contains("Upgrade to premium!"));
        Assert.IsFalse(premiumProviderCalled);  // Not called because IsPremium = false
    }

    [TestMethod]
    public async Task ApplyAsync_MultipleTemplates_SharesCachedData()
    {
        // Arrange
        var providerCallCount = 0;

        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            providerCallCount++;
            return new { FirstName = "John", LastName = "Doe" };
        }));

        RegisterTemplate("template1", "Hello {{Customer/FirstName}}!");
        RegisterTemplate("template2", "Goodbye {{Customer/LastName}}!");

        // Act
        var result1 = await _templateEngine.ApplyAsync("template1", container);
        var result2 = await _templateEngine.ApplyAsync("template2", container);

        // Assert
        Assert.AreEqual("Hello John!", result1.Trim());
        Assert.AreEqual("Goodbye Doe!", result2.Trim());
        Assert.AreEqual(1, providerCallCount);  // Provider called ONCE, cached
    }

    private void RegisterTemplate(string name, string content)
    {
        var source = (InMemoryTemplateSource)_templateEngine.GetType()
            .GetProperty("Sources", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(_templateEngine);

        source.RegisterTemplate(name, content, "text/x-handlebars-template");
    }
}
```

---

### 2. XSLT Integration Tests

**File:** `XsltDataContainerIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class XsltDataContainerIntegrationTests
{
    [TestMethod]
    public async Task ApplyAsync_XsltWithIDataContainer_RendersCorrectly()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Order", new StaticDataProvider(new
        {
            OrderNumber = "12345",
            Customer = new { Name = "John Doe" },
            Total = 99.99m
        }));

        var xslt = @"
<xsl:stylesheet version='1.0'>
  <xsl:template match='/'>
    <invoice>
      <order><xsl:value-of select='Order/OrderNumber'/></order>
      <customer><xsl:value-of select='Order/Customer/Name'/></customer>
      <total><xsl:value-of select='Order/Total'/></total>
    </invoice>
  </xsl:template>
</xsl:stylesheet>
";

        // Register template
        RegisterTemplate("invoice", xslt, "application/xslt+xml");

        // Act
        var result = await _templateEngine.ApplyAsync("invoice", container);

        // Assert
        Assert.IsTrue(result.Contains("<order>12345</order>"));
        Assert.IsTrue(result.Contains("<customer>John Doe</customer>"));
        Assert.IsTrue(result.Contains("<total>99.99</total>"));
    }
}
```

---

## Performance Tests

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class DataContainerIntegrationPerformanceTests
{
    [TestMethod]
    public async Task LazyEvaluation_ReducesProviderExecutions()
    {
        // Arrange
        var providerExecutionCount = 0;

        var container = DataContainerFactory.Create();

        // Register 5 providers
        for (int i = 1; i <= 5; i++)
        {
            int index = i;
            container.RegisterProvider($"Data{i}", new DelegateDataProvider(async () =>
            {
                Interlocked.Increment(ref providerExecutionCount);
                await Task.Delay(10);  // Simulate expensive query
                return new { Value = $"Data{index}" };
            }));
        }

        // Template uses ONLY Data1
        var template = "{{Data1/Value}}";
        RegisterTemplate("simple", template);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _templateEngine.ApplyAsync("simple", container);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual("Data1", result.Trim());
        Assert.AreEqual(1, providerExecutionCount);  // Only 1 provider executed (not 5)
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50);  // ~10ms, not ~50ms
        Console.WriteLine($"Lazy evaluation: {providerExecutionCount} providers executed, {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task CachedAccess_PreventsDuplicateExecution()
    {
        // Arrange
        var providerCallCount = 0;

        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref providerCallCount);
            await Task.Delay(50);  // Expensive query
            return new { FirstName = "John", LastName = "Doe" };
        }));

        // Template accesses Customer multiple times
        var template = @"
{{Customer/FirstName}}
{{Customer/LastName}}
{{Customer/FirstName}}
{{Customer/LastName}}
";
        RegisterTemplate("multi-access", template);

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await _templateEngine.ApplyAsync("multi-access", container);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(1, providerCallCount);  // Provider called ONCE (cached)
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);  // ~50ms, not ~200ms
        Console.WriteLine($"Cached access: {providerCallCount} provider executions, {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task VsEagerLoading_PerformanceComparison()
    {
        // EAGER LOADING (baseline)
        var eagerStopwatch = Stopwatch.StartNew();

        var customer = await LoadCustomerAsync();  // 50ms
        var order = await LoadOrderAsync();        // 50ms
        var inventory = await LoadInventoryAsync(); // 50ms
        var shipping = await LoadShippingAsync();   // 50ms

        var eagerData = new { Customer = customer, Order = order, Inventory = inventory, Shipping = shipping };
        var eagerResult = await RenderTemplateEagerAsync("Hello {{Customer/FirstName}}!", eagerData);

        eagerStopwatch.Stop();

        // LAZY LOADING (with IDataContainer)
        var lazyStopwatch = Stopwatch.StartNew();

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new DelegateDataProvider(async () => await LoadCustomerAsync()));
        container.RegisterProvider("Order", new DelegateDataProvider(async () => await LoadOrderAsync()));
        container.RegisterProvider("Inventory", new DelegateDataProvider(async () => await LoadInventoryAsync()));
        container.RegisterProvider("Shipping", new DelegateDataProvider(async () => await LoadShippingAsync()));

        var lazyResult = await _templateEngine.ApplyAsync("hello", container);

        lazyStopwatch.Stop();

        // Assert
        Assert.AreEqual(eagerResult, lazyResult);

        var improvement = 100 * (eagerStopwatch.ElapsedMilliseconds - lazyStopwatch.ElapsedMilliseconds) / eagerStopwatch.ElapsedMilliseconds;

        Assert.IsTrue(lazyStopwatch.ElapsedMilliseconds < eagerStopwatch.ElapsedMilliseconds);
        Assert.IsTrue(improvement > 50);  // At least 50% faster

        Console.WriteLine($"Eager: {eagerStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Lazy: {lazyStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Improvement: {improvement}%");
    }

    private async Task<object> LoadCustomerAsync()
    {
        await Task.Delay(50);
        return new { FirstName = "John", LastName = "Doe" };
    }

    private async Task<object> LoadOrderAsync()
    {
        await Task.Delay(50);
        return new { OrderNumber = "12345" };
    }

    private async Task<object> LoadInventoryAsync()
    {
        await Task.Delay(50);
        return new { InStock = true };
    }

    private async Task<object> LoadShippingAsync()
    {
        await Task.Delay(50);
        return new { TrackingNumber = "TRACK123" };
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| DefaultDataContainerAdapter | 90% | Adapt, proxy creation |
| DataContainerProxy | 85% | TryGetMember, TryGetIndex, caching |
| DiagnosticDataContainerAdapter | 80% | Logging, timing |
| TemplateEngine Integration | 85% | IDataContainer detection, adaptation |

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit&FullyQualifiedName~DataContainerIntegration"
```

**Run on integration test schedule:**
```bash
# Integration tests (requires template providers)
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~DataContainerIntegration"
```

**Run nightly:**
```bash
# Performance benchmarks
dotnet test --filter "TestCategory=DevLocal&FullyQualifiedName~DataContainerIntegration.Performance"
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 10 Overview](../README-REVISED.md)
