# Feature Request: TestContext Configuration Provider for .NET Core

## Summary
Add a built-in configuration provider for .NET Core that reads configuration values from MSTest's `TestContext.Properties`, enabling seamless integration between test parameters (from `.runsettings` files) and the standard .NET Core configuration system.

## Motivation
When writing integration tests with MSTest, developers often need to pass environment-specific configuration (connection strings, API endpoints, credentials, etc.) through `.runsettings` files. Currently, there's no built-in way to bridge `TestContext.Properties` into the .NET Core `IConfiguration` system, forcing developers to either:

1. Manually read from `TestContext.Properties` and pass values around
2. Duplicate configuration in both `.runsettings` and `appsettings.json`
3. Write custom code to bridge the two systems
4. Use environment variables as a workaround

This creates friction and inconsistency, especially when the application under test expects configuration through `IConfiguration`.

## Proposed Solution

### Add `Microsoft.Extensions.Configuration.TestContext` Package

Create a new NuGet package containing:

```csharp
namespace Microsoft.Extensions.Configuration
{
    public static class TestContextConfigurationExtensions
    {
        public static IConfigurationBuilder AddTestContext(
            this IConfigurationBuilder builder,
            TestContext testContext)
        {
            return builder.AddTestContext(testContext, prefix: null);
        }

        public static IConfigurationBuilder AddTestContext(
            this IConfigurationBuilder builder,
            TestContext testContext,
            string prefix)
        {
            if (builder == null)
                throw new ArgumentNullException(nameof(builder));
            if (testContext == null)
                throw new ArgumentNullException(nameof(testContext));

            return builder.Add(new TestContextConfigurationSource(testContext, prefix));
        }
    }
}
```

### Implementation Details

The provider should:

1. **Normalize keys**: Convert double underscores (`__`) to colons (`:`) to match standard configuration provider behavior
2. **Support hierarchical configuration**: Enable section-based configuration binding
3. **Be case-insensitive**: Match the behavior of other configuration providers
4. **Support optional prefix filtering**: Allow loading only properties with a specific prefix
5. **Support arrays**: Handle numeric suffixes for array binding (e.g., `Servers__0`, `Servers__1`)

### Usage Example

```csharp
[TestClass]
public class IntegrationTests
{
    public TestContext TestContext { get; set; }

    [TestMethod]
    public void TestWithConfiguration()
    {
        // Build configuration from multiple sources
        var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddTestContext(TestContext)  // Test parameters override appsettings
            .Build();

        // Use standard IConfiguration patterns
        var connectionString = config["Database:ConnectionString"];
        var dbConfig = config.GetSection("Database").Get<DatabaseConfig>();
        
        // Test with configuration
        var sut = new SystemUnderTest(config);
        // ... assertions
    }
}
```

### .runsettings Example

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

## Benefits

1. **Consistency**: Tests use the same configuration patterns as production code
2. **Flexibility**: Easy to override configuration per test run without changing code
3. **DI Integration**: Works seamlessly with dependency injection in tests
4. **Environment Management**: Different `.runsettings` files for different environments (dev, staging, CI/CD)
5. **Type Safety**: Leverage configuration binding to strongly-typed objects
6. **Discoverability**: Follows established .NET Core configuration patterns

## Alternatives Considered

1. **Environment Variables**: Requires setting env vars before test execution, less portable
2. **Custom Implementation**: Every team reinvents this wheel
3. **appsettings.Test.json**: Requires file management, doesn't work well in CI/CD pipelines

## Implementation Notes

- Target: `Microsoft.Extensions.Configuration` namespace
- Dependencies: 
  - `Microsoft.Extensions.Configuration.Abstractions`
  - `MSTest.TestFramework` (for `TestContext`)
- Package Name: `Microsoft.Extensions.Configuration.TestContext`
- Compatibility: .NET Core 3.1+, .NET 5+, .NET 6+

## Related Work

- Similar to `AddEnvironmentVariables()` but for MSTest context
- Complements existing configuration providers (JSON, XML, Environment Variables, Command Line)

## Additional Features (Optional)

1. **Reload Support**: Allow reloading configuration if `TestContext.Properties` changes during test execution
2. **Validation**: Add validation helpers for required test parameters
3. **Logging**: Integration with `ILogger` to show which test parameters were loaded

---

## References

- MSTest Documentation: https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-writing-tests
- Configuration in .NET: https://learn.microsoft.com/en-us/dotnet/core/extensions/configuration
- .runsettings file: https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file