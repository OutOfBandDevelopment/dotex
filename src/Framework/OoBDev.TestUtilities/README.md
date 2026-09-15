# OoBDev.TestUtilities

Test utilities and helpers for MSTest-based testing in OoBDev projects.

## Overview

This library provides utilities that simplify test development and improve test maintainability across OoBDev projects. It includes assertion helpers, test extensions, and integration with .NET configuration systems.

## Features

### 1. TestContext Configuration Provider

**NEW:** Seamlessly integrate MSTest `.runsettings` test parameters with the .NET `IConfiguration` system.

#### Purpose

When writing integration tests, you often need to pass environment-specific configuration (connection strings, API endpoints, credentials) through `.runsettings` files. This provider bridges `TestContext.Properties` into the standard .NET Core configuration system.

#### Usage

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class MyIntegrationTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    public void TestWithConfiguration()
    {
        // Build configuration from TestContext
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddTestContext(TestContext)  // Adds test parameters
            .Build();

        // Use standard IConfiguration patterns
        var connectionString = config["Database:ConnectionString"];
        var apiUrl = config["Api:BaseUrl"];

        // Or bind to strong types
        var dbConfig = config.GetSection("Database").Get<DatabaseConfig>();
    }
}
```

#### .runsettings Example

```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <TestRunParameters>
    <!-- Flat values -->
    <Parameter name="Environment" value="Integration" />

    <!-- Hierarchical with colons -->
    <Parameter name="Database:Server" value="localhost" />
    <Parameter name="Database:Port" value="5432" />

    <!-- Hierarchical with double underscores (cross-platform safe) -->
    <Parameter name="Api__BaseUrl" value="https://test-api.example.com" />
    <Parameter name="Api__Timeout" value="30" />

    <!-- Arrays -->
    <Parameter name="Servers__0" value="server1.test.local" />
    <Parameter name="Servers__1" value="server2.test.local" />
  </TestRunParameters>
</RunSettings>
```

#### Features

- **Hierarchical Configuration**: Supports nested configuration using `:` or `__` separators
- **Key Normalization**: Automatically converts `__` to `:` for cross-platform compatibility
- **Case-Insensitive**: Matches behavior of other .NET configuration providers
- **Prefix Filtering**: Load only parameters with a specific prefix
- **Array Support**: Handle numeric suffixes for array binding (`Servers__0`, `Servers__1`)
- **Strong-Typed Binding**: Works with `IOptions<T>` and configuration binding

#### Prefix Filtering

```csharp
// Only load parameters starting with "MyApp:"
var config = new ConfigurationBuilder()
    .AddTestContext(TestContext, prefix: "MyApp")
    .Build();

// Parameter "MyApp:Database:Server" becomes "Database:Server"
var server = config["Database:Server"];
```

#### Configuration Precedence

Add `TestContext` last to override other configuration sources:

```csharp
var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")       // Base configuration
    .AddEnvironmentVariables()             // Environment overrides
    .AddTestContext(TestContext)           // Test parameters override all
    .Build();
```

### 2. Numeric Assertions

Floating-point comparison helpers that handle rounding differences.

```csharp
// Compare doubles with default tolerance (1e-10)
NumericAsserts.AreSimilar(expected, actual);

// Compare with custom tolerance
NumericAsserts.AreSimilar(expected, actual, tolerance: 0.001);

// Works with float, double, decimal
NumericAsserts.AreSimilar(123.456f, 123.457f, tolerance: 0.01f);
```

### 3. TestContext Extensions

Extension methods for working with MSTest TestContext.

```csharp
// Get required test property (throws if missing)
var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");

// Get property with default value
var port = TestContext.GetPropertyOrDefault("PORT", 5432);
var timeout = TestContext.GetPropertyOrDefault("TIMEOUT", TimeSpan.FromSeconds(30));
```

### 4. Test Categories

Predefined test category constants for consistent test organization.

```csharp
[TestMethod]
[TestCategory(TestCategories.Unit)]           // Fast, isolated, no external dependencies
public void MyUnitTest() { }

[TestMethod]
[TestCategory(TestCategories.Integration)]    // Docker-based services
public void MyIntegrationTest() { }

[TestMethod]
[TestCategory(TestCategories.LiveIntegration)] // Cloud services (manual only)
public void MyLiveTest() { }
```

**Available Categories:**
- `Unit` - Fast, isolated tests (run in CI/CD)
- `Simulate` - Full stack with mocked persistence (run in CI/CD)
- `Integration` - Docker-based external services (run daily)
- `DevLocal` - Manual/exploratory testing only
- `LiveIntegration` - Live cloud services (manual only)

## Installation

This package is part of the OoBDev framework. Reference it in your test projects:

```xml
<ItemGroup>
  <ProjectReference Include="..\OoBDev.TestUtilities\OoBDev.TestUtilities.csproj" />
</ItemGroup>
```

## Dependencies

- `Microsoft.Extensions.Configuration` (10.0.2)
- `Microsoft.Extensions.Logging` (10.0.2)
- `MSTest.TestFramework` (4.0.2)
- `Moq` (4.20.72)
- `JsonDiffPatch.Net` (2.5.0)

## Running Tests with .runsettings

### Visual Studio
1. Test → Configure Run Settings → Select Solution Wide runsettings File
2. Select `src/.runsettings`

### Command Line
```bash
dotnet test --settings src/.runsettings
```

### Filter by Category
```bash
# Run only unit tests
dotnet test --filter TestCategory=Unit

# Run integration tests
dotnet test --filter TestCategory=Integration
```

## Best Practices

### TestContext Configuration Provider

1. **Always use for integration tests** - Keeps configuration out of source control
2. **Use prefix filtering** - Organize test parameters by project/feature
3. **Leverage strong typing** - Bind to configuration classes for type safety
4. **Override appsettings** - Add TestContext last in the chain

### Test Properties

1. **Use `GetRequiredProperty`** for mandatory configuration
2. **Use `GetPropertyOrDefault`** for optional settings with sensible defaults
3. **Document required properties** in test class comments

### Numeric Assertions

1. **Use for all floating-point comparisons** - Avoids flaky tests due to rounding
2. **Choose appropriate tolerance** - Tighter for precise calculations, looser for approximate

## Examples

### Complete Integration Test Example

```csharp
[TestClass]
public class DatabaseIntegrationTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task TestDatabaseConnection()
    {
        // Arrange - Get configuration from .runsettings
        var config = new ConfigurationBuilder()
            .AddTestContext(TestContext)
            .Build();

        var connectionString = config.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");
        var databaseName = $"IntegrationTest_{Guid.NewGuid():N}";

        // Act
        using var client = new MongoClient(connectionString);
        var database = client.GetDatabase(databaseName);
        var collection = database.GetCollection<TestDocument>("test");

        await collection.InsertOneAsync(new TestDocument { Name = "Test" });
        var result = await collection.Find(_ => true).FirstOrDefaultAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Test", result.Name);

        // Cleanup
        await client.DropDatabaseAsync(databaseName);
    }
}
```

## See Also

- [Configuration in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration)
- [MSTest Documentation](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests)
- [.runsettings Reference](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file)
- [OoBDev Testing Guide](../../../docs/how-tos/runsettings-variables-and-configuration.md)
