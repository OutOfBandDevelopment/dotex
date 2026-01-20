# Testing Documentation

**Last Updated:** 2026-01-20

Comprehensive testing standards, guidelines, and best practices for the OoBDev framework.

---

## Quick Links

| Document | Purpose |
|----------|---------|
| [Testing Guidelines](./testing-guidelines.md) | Comprehensive testing standards and patterns |
| [Test Variables Reference](../../TEST_VARIABLES.md) | All test properties and configuration |
| [Docker Infrastructure](../../../containers/testing/README.md) | Docker-based integration testing setup |

---

## Test Categories Overview

OoBDev uses **5 test categories** to organize tests:

| Category | CI/CD | Services | Use Case |
|----------|-------|----------|----------|
| **Unit** | Every PR | Mocked | Pure logic, < 100ms |
| **Simulate** | Every PR | Mocked | End-to-end with in-memory persistence |
| **Integration** | Daily 4 PM UTC | Docker | MongoDB, SQL Server, RabbitMQ, etc. |
| **DevLocal** | Manual | Local | Performance, GPU tests |
| **LiveIntegration** | Manual | Cloud | Azure B2C, Groq, App Insights |

---

## Test Property Patterns

### Required Values (No Sensible Default)

Use `GetRequiredProperty<T>()` for values that must be explicitly configured:

```csharp
// URLs, credentials, connection strings - no industry default
var url = TestContext.GetRequiredProperty<string>("KEYCLOAK_URL");
var username = TestContext.GetRequiredProperty<string>("KEYCLOAK_ADMIN_USERNAME");
var password = TestContext.GetRequiredProperty<string>("KEYCLOAK_ADMIN_PASSWORD");
var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");
```

### Values with Industry Defaults

Use `GetPropertyOrDefault<T>()` for values with well-known industry standards:

```csharp
// Port numbers with industry standards
var smtpPort = TestContext.GetPropertyOrDefault("SMTP_PORT", 25);       // SMTP default
var imapPort = TestContext.GetPropertyOrDefault("IMAP_PORT", 143);      // IMAP default
var mongoPort = TestContext.GetPropertyOrDefault("MONGODB_PORT", 27017); // MongoDB default
var redisPort = TestContext.GetPropertyOrDefault("REDIS_PORT", 6379);   // Redis default
```

### Decision Guide

| Method | Use When | Examples |
|--------|----------|----------|
| `GetRequiredProperty<T>()` | Value must be explicitly configured | URLs, usernames, passwords, connection strings, realm names, client IDs |
| `GetPropertyOrDefault<T>()` | Industry-standard default exists | Port 5432 (PostgreSQL), Port 27017 (MongoDB), Port 6379 (Redis), Port 25 (SMTP) |

### Anti-Pattern

**NEVER** use `Environment.GetEnvironmentVariable()` directly:

```csharp
// WRONG - Don't do this!
var value = Environment.GetEnvironmentVariable("PARAMETER_NAME");

// CORRECT - Use TestContext extension methods
var value = TestContext.GetRequiredProperty<string>("PARAMETER_NAME");
```

Both `GetRequiredProperty` and `GetPropertyOrDefault` check `.runsettings` first, then fall back to environment variables automatically.

---

## Test Structure Template

```csharp
[TestClass]
public class MyServiceIntegrationTests
{
    public required TestContext TestContext { get; set; }

    private string? _resourceName;
    private IMyClient? _client;

    [TestInitialize]
    public void TestInitialize()
    {
        // Create unique resource name for test isolation
        _resourceName = $"IntegrationTest_{Guid.NewGuid():N}";
    }

    [TestCleanup]
    public async Task TestCleanup()
    {
        // Always cleanup test resources
        if (_client != null && _resourceName != null)
        {
            await _client.DeleteAsync(_resourceName);
        }
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Operation_Scenario_ExpectedBehavior()
    {
        // Arrange - Use required properties for configuration
        var url = TestContext.GetRequiredProperty<string>("SERVICE_URL");
        var username = TestContext.GetRequiredProperty<string>("SERVICE_USERNAME");
        var password = TestContext.GetRequiredProperty<string>("SERVICE_PASSWORD");

        // Create client
        _client = new MyClient(url, username, password);

        // Act
        var result = await _client.CreateAsync(_resourceName, data);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expected, result.Value);
    }
}
```

---

## Related Documentation

- [Testing Guidelines](./testing-guidelines.md) - Full testing standards
- [Test Variables Reference](../../TEST_VARIABLES.md) - Complete property list
- [Docker Infrastructure](../../../containers/testing/README.md) - Docker setup
- [Integration Test Protocol](../../../.claude/protocols/software/integration-test-maintenance.md) - Maintenance checklist

---

## Protocols

When working with tests, follow these protocols:

| Situation | Protocol |
|-----------|----------|
| Adding new Docker service | [integration-test-maintenance.md](../../../.claude/protocols/software/integration-test-maintenance.md) |
| Adding new test parameters | [integration-test-maintenance.md](../../../.claude/protocols/software/integration-test-maintenance.md) |
| Writing new integration tests | [testing-guidelines.md](./testing-guidelines.md) |

