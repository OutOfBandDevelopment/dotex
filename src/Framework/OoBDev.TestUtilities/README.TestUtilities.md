# OoBDev.TestUtilities

Helpers for MSTest projects: test parameters as configuration, result attachments, embedded test data, tier categories, numeric assertions, a test logger and a guard for integration tests. The tier rules are in `.claude/rules/testing.md`; the `.runsettings` parameters are in the [runsettings how-to](../../../docs/how-tos/runsettings-variables-and-configuration.md).

## Contents

- [Test parameters](#test-parameters)
- [Results and test data](#results-and-test-data)
- [Categories](#categories)
- [Assertions, logging and guards](#assertions-logging-and-guards)
- [Tests](#tests)

## Test parameters

`.runsettings` `<Parameter>` values reach tests through `TestContext.Properties`. Two routes:

```csharp
var url = TestContext.GetRequiredProperty<string>("MOTO_URL");          // throws when missing
var port = TestContext.GetPropertyOrDefault("MOTO_PORT", 4566);          // typed, with default
// also GetProperty<T> (null when missing) and TryGetProperty<T>

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddTestContext(TestContext)                  // or AddTestContext(TestContext, "MyApp")
    .Build();
```

`AddTestContext` is an `IConfigurationBuilder` extension (namespace `Microsoft.Extensions.Configuration`, provider `TestContextConfigurationProvider`). With a prefix, only the properties starting with it are loaded and the prefix is removed from the keys. Add it last to let test parameters override the other sources. Read settings with these helpers, never with `Environment.GetEnvironmentVariable` (rule `testing`).

## Results and test data

**Table 1 — TestContext extensions (`TestContextExtensions`)**

| Method | Purpose |
|--------|---------|
| `AddResult(...)`, `AddResultFile(name, bytes)` | Attach an object (as JSON), text or bytes to the test result |
| `GetTestRunResultFiles()` | The files attached during the run |
| `GetTestData(type, target)`, `GetTestDataAsync<T>(target)` | Deserialize an embedded resource of the test project, named by the test method or an explicit target |
| `GetQualifiedTestName()`, `ResolveTestType()` | The test's qualified name and its class type |
| `GetLogger<T>()` | A logger writing to the test output |

Related: `ContextualTestMethodAttribute` and `ContextualTestClassBase` expose the current test method and instance through `TestContext.Properties`; `TestTargetAttribute` names the class and member under test; `Xml/Xsl/Extensions/TrxExtensions` support XSL over TRX results.

## CSV test data

`[CsvData("Data/cases.csv")]` on a test method (namespace `OoBDev.TestUtilities.Data`) turns each row of the file into a test case. The header names the columns; each is matched to the method parameter of the same name, so column order is free. Values convert with the invariant culture (enums by name, hex text to `byte[]`, empty text to `null` for nullable value types). A case is named `Method (cases.csv:LINE)`, and a bad value, short row, missing column or unclosed quote fails with the file and line. Mark the file copy to output (`<None Update="Data\*.csv" CopyToOutputDirectory="PreserveNewest" />`). MSTest 4 has no CSV source of its own, which is why this exists.

## Categories

`TestCategories` holds the constants for `[TestCategory(...)]`: the tiers `Unit`, `Simulate`, `Integration`, `DevLocal`, `LiveIntegration` and `Hardware`; transport and area tags (`Serial`, `Tcp`, `Hid`, `Usbtmc`, `Ble`, `Loopback`, `Rfc2217`, `Vxi11`, `Mqtt`, `Brokers`, `Web`, `Scpi`, `Logging`, `BugRegression`); and one tag per bench device (for example `Korad_Ka3005p`, `Radex_One`, `Zoom_H4n`). A test carries exactly one tier.

```bash
dotnet test src/ --filter TestCategory=Unit
dotnet test src/ --filter "TestCategory=Integration"
```

## Assertions, logging and guards

- `NumericAsserts.AreSimilar(expected, actual[, tolerance])`: exact for integers; for `double`, `float` and `decimal` a relative tolerance of `Max(|expected|, |actual|)` times 1e-10 (`double`, `decimal`) or 1e-5 (`float`), or the tolerance given.
- `AddTestLoggingServices(context)` (`OoBDev.TestUtilities.Logging`) registers `ILogger` and `ILogger<>` that write to the test output and the debug window.
- `TestInstanceGuard.Require(connectionString)` fails an Integration test unless every host in the connection string (key/value pairs or URL) is loopback or `host.docker.internal`; `IsTestInstance` returns the boolean. The SQL Server deployment tests use it.

## Tests

`OoBDev.TestUtilities.Tests` covers the configuration provider (Unit and Integration), the property helpers, the data helpers, numeric asserts and the guard.

## Integration containers

`[IntegrationTestMethod("oobd-test-redis,oobd-test-sqlserver")]` replaces `[TestMethod]` on an Integration test. Before the test runs, `IntegrationContainers.EnsureAsync` inspects each container: ready ones are used as they are, stopped or missing ones are started and awaited until healthy, and the test ends Inconclusive when that takes longer than `CONTAINER_STARTUP_TIMEOUT_SECONDS` (default 5), Docker is missing or `CONTAINER_AUTOSTART` is false. Containers the tests started (and only those) are stopped at process exit unless `CONTAINER_STOP_AFTER_TESTS` is false. The compose health checks use `start_interval: 1s` so a container reports healthy within seconds. See [TEST_VARIABLES.md](../../../TEST_VARIABLES.md).
