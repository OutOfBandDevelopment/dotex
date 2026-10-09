# Writing Integration Tests

[← Services](services.md) · [Integration testing](README.md)

## Contents

- [Rules](#rules)
- [Worked example](#worked-example)
- [Adding a service](#adding-a-service)

## Rules

1. Mark the test `[TestCategory(TestCategories.Integration)]`. A test that needs an external service is never `Unit` or `Simulate`; `DevLocal` is only for manual runs such as GPU or performance work.
2. Read settings with `TestContext.GetRequiredProperty<T>()` for values with no sensible default (URLs, credentials, connection strings) and `GetPropertyOrDefault<T>()` for industry-standard defaults. Do not call `Environment.GetEnvironmentVariable`.
3. Use unique resource names per run (`$"IntegrationTest_{Guid.NewGuid():N}"`) and delete them in `[TestCleanup]`, because volumes outlive a run.
4. Run with `src/.runsettings` (the default) or a copy with changed ports; see [Docker infrastructure](docker-infrastructure.md#ports).
5. Assert on the service's real behaviour, not on mocks. Floating-point results use `NumericAsserts.AreSimilar`.

## Worked example

```csharp
[TestClass]
public class MongoRoundTripTests
{
    public TestContext TestContext { get; set; } = null!;
    private IMongoClient? _client;
    private string? _database;

    [TestInitialize]
    public void Initialize()
    {
        var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");
        _client = new MongoClient(connectionString);
        _database = $"IntegrationTest_{Guid.NewGuid():N}";
    }

    [TestCleanup]
    public async Task CleanupAsync()
    {
        if (_client != null && _database != null) await _client.DropDatabaseAsync(_database);
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task Insert_ThenFind_ReturnsDocument()
    {
        // Stage
        var collection = _client!.GetDatabase(_database).GetCollection<BsonDocument>("items");

        // Test
        await collection.InsertOneAsync(new BsonDocument("name", "widget"));
        var found = await collection.Find(new BsonDocument("name", "widget")).FirstOrDefaultAsync();

        // Assert
        Assert.IsNotNull(found);
    }
}
```

Settings can also bind to options through the `TestContext` configuration provider in `OoBDev.TestUtilities` (`Database:Server` or `Database__Server` style keys), see the [runsettings how-to](../../../how-tos/runsettings-variables-and-configuration.md).

## Adding a service

Follow the `integration-test-maintenance` protocol (`.claude/protocols/`): compose entry with a `TEST_PORT_*` host port and a health check, nginx dashboard link where it has a UI, `.runsettings` parameters, [TEST_VARIABLES.md](../../../../TEST_VARIABLES.md), and a section in [Services](services.md) plus a row in the [dependency matrix](README.md#dependency-matrix).

[← Services](services.md) · [Integration testing](README.md)
