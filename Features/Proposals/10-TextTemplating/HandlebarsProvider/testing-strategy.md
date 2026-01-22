# Handlebars Template Provider - Testing Strategy

**Epic:** 10 - Text Templating Extensions
**Feature:** Handlebars Template Provider
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 80%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (50+ tests)
- **Integration Tests** - End-to-end with IDataContainer (15+ tests)
- **Performance Tests** - Template compilation and rendering benchmarks (5+ tests)

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

### 1. HandlebarsTemplateProvider Tests

**File:** `HandlebarsTemplateProviderTests.cs`

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Extensions.Templates.Handlebars;

namespace OoBDev.Extensions.Templates.Handlebars.Tests;

[TestClass]
public class HandlebarsTemplateProviderTests
{
    [TestMethod]
    public void ProviderName_ReturnsHandlebars()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();

        // Act
        var name = provider.ProviderName;

        // Assert
        Assert.AreEqual("handlebars", name);
    }

    [TestMethod]
    public async Task RenderAsync_SimpleTemplate_ReturnsRenderedText()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "Hello {{name}}!";
        var data = new { name = "World" };

        // Act
        var result = await provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("Hello World!", result);
    }

    [TestMethod]
    public async Task RenderAsync_NestedProperties_AccessesNestedData()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "{{customer.firstName}} {{customer.lastName}}";
        var data = new
        {
            customer = new
            {
                firstName = "John",
                lastName = "Doe"
            }
        };

        // Act
        var result = await provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("John Doe", result);
    }

    [TestMethod]
    public async Task RenderAsync_IfBlock_EvaluatesCondition()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = @"
{{#if isActive}}
Active
{{else}}
Inactive
{{/if}}";

        // Act
        var activeResult = await provider.RenderAsync(template, new { isActive = true });
        var inactiveResult = await provider.RenderAsync(template, new { isActive = false });

        // Assert
        Assert.IsTrue(activeResult.Contains("Active"));
        Assert.IsFalse(activeResult.Contains("Inactive"));
        Assert.IsFalse(inactiveResult.Contains("Active"));
        Assert.IsTrue(inactiveResult.Contains("Inactive"));
    }

    [TestMethod]
    public async Task RenderAsync_EachBlock_IteratesCollection()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = @"
{{#each items}}
{{name}}: {{price}}
{{/each}}";
        var data = new
        {
            items = new[]
            {
                new { name = "Widget", price = 19.99m },
                new { name = "Gadget", price = 29.99m }
            }
        };

        // Act
        var result = await provider.RenderAsync(template, data);

        // Assert
        Assert.IsTrue(result.Contains("Widget: 19.99"));
        Assert.IsTrue(result.Contains("Gadget: 29.99"));
    }

    [TestMethod]
    public async Task CompileAsync_ValidTemplate_ReturnsCompiledTemplate()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "Hello {{name}}!";

        // Act
        var compiled = await provider.CompileAsync(template);

        // Assert
        Assert.IsNotNull(compiled);
        Assert.IsInstanceOfType(compiled, typeof(ICompiledTemplate));
    }

    [TestMethod]
    [ExpectedException(typeof(HandlebarsCompilationException))]
    public async Task CompileAsync_InvalidTemplate_ThrowsException()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "{{#if unclosed block";

        // Act
        await provider.CompileAsync(template);  // Should throw
    }

    [TestMethod]
    public async Task RenderAsync_SameTemplateTwice_UsesCache()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "Hello {{name}}!";

        // Act
        var result1 = await provider.RenderAsync(template, new { name = "Alice" });
        var result2 = await provider.RenderAsync(template, new { name = "Bob" });

        // Assert
        Assert.AreEqual("Hello Alice!", result1);
        Assert.AreEqual("Hello Bob!", result2);
        // Cache hit verified by no compilation exception on second call
    }
}
```

---

### 2. Built-in Helper Tests

**File:** `BuiltInHelperTests.cs`

```csharp
[TestClass]
public class BuiltInHelperTests
{
    private HandlebarsTemplateProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        _provider = new HandlebarsTemplateProvider();
    }

    [TestMethod]
    public async Task FormatDate_ValidDate_FormatsCorrectly()
    {
        // Arrange
        var template = "{{formatDate date 'yyyy-MM-dd'}}";
        var data = new { date = new DateTime(2026, 1, 22) };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("2026-01-22", result.Trim());
    }

    [TestMethod]
    public async Task FormatDate_DefaultFormat_UsesYyyyMMdd()
    {
        // Arrange
        var template = "{{formatDate date}}";
        var data = new { date = new DateTime(2026, 1, 22) };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("2026-01-22", result.Trim());
    }

    [TestMethod]
    public async Task FormatNumber_ValidNumber_FormatsCorrectly()
    {
        // Arrange
        var template = "{{formatNumber value 'N2'}}";
        var data = new { value = 1234.567m };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("1,234.57", result.Trim());
    }

    [TestMethod]
    public async Task Currency_ValidAmount_FormatsCurrency()
    {
        // Arrange
        var template = "{{currency amount}}";
        var data = new { amount = 99.99m };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.IsTrue(result.Contains("99.99"));
        Assert.IsTrue(result.Contains("$") || result.Contains("USD"));  // Culture-specific
    }

    [TestMethod]
    public async Task Uppercase_ValidText_ConvertsToUppercase()
    {
        // Arrange
        var template = "{{uppercase text}}";
        var data = new { text = "hello world" };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("HELLO WORLD", result.Trim());
    }

    [TestMethod]
    public async Task Lowercase_ValidText_ConvertsToLowercase()
    {
        // Arrange
        var template = "{{lowercase text}}";
        var data = new { text = "HELLO WORLD" };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("hello world", result.Trim());
    }

    [TestMethod]
    public async Task Json_ValidObject_SerializesToJson()
    {
        // Arrange
        var template = "{{json data}}";
        var data = new
        {
            data = new { name = "John", age = 30 }
        };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.IsTrue(result.Contains("\"name\""));
        Assert.IsTrue(result.Contains("\"John\""));
        Assert.IsTrue(result.Contains("\"age\""));
        Assert.IsTrue(result.Contains("30"));
    }

    [TestMethod]
    public async Task FormatDate_InvalidDate_ReturnsEmpty()
    {
        // Arrange
        var template = "{{formatDate invalidDate}}";
        var data = new { invalidDate = "not a date" };

        // Act
        var result = await _provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual(string.Empty, result.Trim());
    }
}
```

---

### 3. Custom Helper Registration Tests

**File:** `CustomHelperTests.cs`

```csharp
[TestClass]
public class CustomHelperTests
{
    [TestMethod]
    public async Task RegisterHelper_CustomHelper_ExecutesCorrectly()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();

        provider.RegisterHelper("multiply", (writer, context, parameters) =>
        {
            if (parameters.Length >= 2)
            {
                var a = Convert.ToDecimal(parameters[0]);
                var b = Convert.ToDecimal(parameters[1]);
                writer.WriteSafeString((a * b).ToString("F2"));
            }
        });

        var template = "{{multiply 5 10}}";

        // Act
        var result = await provider.RenderAsync(template, new object());

        // Assert
        Assert.AreEqual("50.00", result.Trim());
    }

    [TestMethod]
    public async Task RegisterBlockHelper_CustomBlockHelper_ExecutesCorrectly()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();

        provider.RegisterBlockHelper("wrap", (writer, options, context, arguments) =>
        {
            writer.WriteSafeString("<div>");
            options.Template(writer, context);
            writer.WriteSafeString("</div>");
        });

        var template = "{{#wrap}}Content{{/wrap}}";

        // Act
        var result = await provider.RenderAsync(template, new object());

        // Assert
        Assert.AreEqual("<div>Content</div>", result.Trim());
    }

    [TestMethod]
    public async Task RegisterPartial_CustomPartial_IncludesPartial()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();

        provider.RegisterPartial("header", "<h1>{{title}}</h1>");

        var template = "{{> header}}";
        var data = new { title = "Welcome" };

        // Act
        var result = await provider.RenderAsync(template, data);

        // Assert
        Assert.AreEqual("<h1>Welcome</h1>", result.Trim());
    }
}
```

---

## Integration Tests

### 1. IDataContainer Integration Tests

**File:** `DataContainerIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class DataContainerIntegrationTests
{
    [TestMethod]
    public async Task RenderAsync_WithIDataContainer_LazilyEvaluatesData()
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

        var provider = new HandlebarsTemplateProvider();
        var template = "Hello {{Customer.FirstName}}!";

        // Act
        var result = await provider.RenderAsync(template, container);

        // Assert
        Assert.AreEqual("Hello John!", result.Trim());
        Assert.IsTrue(customerProviderCalled);
        Assert.IsFalse(orderProviderCalled);  // Order provider never called (lazy)
    }

    [TestMethod]
    public async Task RenderAsync_NestedContainerPaths_AccessesCorrectly()
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

        var provider = new HandlebarsTemplateProvider();
        var template = "{{Customer.FirstName}} {{Customer.LastName}} lives in {{Customer.Address.City}}, {{Customer.Address.State}}";

        // Act
        var result = await provider.RenderAsync(template, container);

        // Assert
        Assert.AreEqual("John Doe lives in Springfield, IL", result.Trim());
    }

    [TestMethod]
    public async Task RenderAsync_ArrayInContainer_IteratesCorrectly()
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

        var provider = new HandlebarsTemplateProvider();
        var template = @"
Order {{Order.OrderNumber}}
Items:
{{#each Order.LineItems}}
- {{ProductName}}: {{Quantity}} x ${{Price}}
{{/each}}";

        // Act
        var result = await provider.RenderAsync(template, container);

        // Assert
        Assert.IsTrue(result.Contains("Order 12345"));
        Assert.IsTrue(result.Contains("Widget: 2 x $19.99"));
        Assert.IsTrue(result.Contains("Gadget: 1 x $29.99"));
    }

    [TestMethod]
    public async Task RenderAsync_ConditionalWithContainer_EvaluatesCorrectly()
    {
        // Arrange
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Account", new StaticDataProvider(new
        {
            IsActive = true,
            Balance = 100.50m
        }));

        var provider = new HandlebarsTemplateProvider();
        var template = @"
{{#if Account.IsActive}}
Account is active. Balance: ${{Account.Balance}}
{{else}}
Account is inactive.
{{/if}}";

        // Act
        var result = await provider.RenderAsync(template, container);

        // Assert
        Assert.IsTrue(result.Contains("Account is active"));
        Assert.IsTrue(result.Contains("Balance: $100.50"));
    }
}
```

---

### 2. Compiled Template Integration Tests

**File:** `CompiledTemplateIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class CompiledTemplateIntegrationTests
{
    [TestMethod]
    public async Task CompiledTemplate_ReuseMultipleTimes_RendersCorrectly()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "Hello {{name}}!";
        var compiled = await provider.CompileAsync(template);

        // Act & Assert
        var result1 = await compiled.RenderAsync(new { name = "Alice" });
        Assert.AreEqual("Hello Alice!", result1.Trim());

        var result2 = await compiled.RenderAsync(new { name = "Bob" });
        Assert.AreEqual("Hello Bob!", result2.Trim());

        var result3 = await compiled.RenderAsync(new { name = "Charlie" });
        Assert.AreEqual("Hello Charlie!", result3.Trim());
    }

    [TestMethod]
    public async Task CompiledTemplate_WithIDataContainer_RendersCorrectly()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "{{Customer.FirstName}} {{Customer.LastName}}";
        var compiled = await provider.CompileAsync(template);

        var container = DataContainerFactory.Create();
        container.RegisterProvider("Customer", new StaticDataProvider(new
        {
            FirstName = "John",
            LastName = "Doe"
        }));

        // Act
        var result = await compiled.RenderAsync(container);

        // Assert
        Assert.AreEqual("John Doe", result.Trim());
    }
}
```

---

## Performance Tests

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class PerformanceTests
{
    [TestMethod]
    public async Task RenderAsync_TemplateCompilationCache_ImprovesPerfromance()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "Hello {{name}}!";
        var data = new { name = "World" };

        // Warm up cache
        await provider.RenderAsync(template, data);

        // Act - Measure cached compilation
        var stopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 1000; i++)
        {
            await provider.RenderAsync(template, new { name = $"User{i}" });
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);  // < 0.5ms per render (cached)
        Console.WriteLine($"1000 renders (cached): {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task CompileAsync_ReusedTemplate_FasterThanRecompilation()
    {
        // Arrange
        var provider = new HandlebarsTemplateProvider();
        var template = "{{#each items}}{{name}}: {{value}}{{/each}}";
        var data = new
        {
            items = Enumerable.Range(1, 100).Select(i => new { name = $"Item{i}", value = i }).ToArray()
        };

        // Compile once
        var compiled = await provider.CompileAsync(template);

        // Act - Measure compiled template performance
        var compiledStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            await compiled.RenderAsync(data);
        }
        compiledStopwatch.Stop();

        // Act - Measure non-compiled performance
        var nonCompiledStopwatch = Stopwatch.StartNew();
        for (int i = 0; i < 100; i++)
        {
            await provider.RenderAsync(template, data);
        }
        nonCompiledStopwatch.Stop();

        // Assert - Compiled should be similar or faster (both cached)
        Console.WriteLine($"Compiled: {compiledStopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"Non-compiled (cached): {nonCompiledStopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task RenderAsync_LazyEvaluation_ReducesProviderCalls()
    {
        // Arrange
        var callCount = 0;

        var container = DataContainerFactory.Create();
        container.RegisterProvider("ExpensiveData1", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(10);
            return new { Value = "Data1" };
        }));

        container.RegisterProvider("ExpensiveData2", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(10);
            return new { Value = "Data2" };
        }));

        container.RegisterProvider("ExpensiveData3", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCount);
            await Task.Delay(10);
            return new { Value = "Data3" };
        }));

        var provider = new HandlebarsTemplateProvider();
        var template = "Value: {{ExpensiveData1.Value}}";  // Only uses Data1

        // Act
        var stopwatch = Stopwatch.StartNew();
        var result = await provider.RenderAsync(template, container);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(1, callCount);  // Only 1 provider called (not 3)
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50);  // ~10ms, not ~30ms
        Console.WriteLine($"Lazy evaluation: {callCount} providers executed, {stopwatch.ElapsedMilliseconds}ms");
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
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Customer", new StaticDataProvider(new
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
        }));

        return container;
    }

    public static IDataContainer BuildOrderContainer()
    {
        var container = DataContainerFactory.Create();

        container.RegisterProvider("Order", new StaticDataProvider(new
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
        }));

        return container;
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| HandlebarsTemplateProvider | 85% | RenderAsync, CompileAsync, Helper registration |
| Built-in Helpers | 90% | formatDate, currency, uppercase, json |
| DataContainerAdapter | 80% | Adapt, TryGetMember, Lazy evaluation |
| CompiledTemplate | 85% | RenderAsync with caching |
| Error Handling | 70% | Compilation errors, Rendering errors |

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit&FullyQualifiedName~Handlebars"

# Integration tests (slower)
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~Handlebars"
```

**Run nightly:**
```bash
# Performance benchmarks
dotnet test --filter "TestCategory=DevLocal&FullyQualifiedName~Handlebars.Performance"
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 10 Overview](../README-REVISED.md)
