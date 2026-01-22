# How-To: Variables and Configuration in .runsettings Files

**Last Updated:** 2026-01-21
**Applies To:** MSTest, VSTest, dotnet test

---

## Overview

This guide explains how to use variables and dynamic configuration in `.runsettings` files for MSTest-based testing in the OoBDev project.

## Table of Contents

- [What Works and What Doesn't](#what-works-and-what-doesnt)
- [Environment Variables](#environment-variables)
- [Workarounds for MSBuild Variables](#workarounds-for-msbuild-variables)
- [OoBDev Best Practices](#oobdev-best-practices)
- [Examples](#examples)
- [Troubleshooting](#troubleshooting)

---

## What Works and What Doesn't

### ❌ NOT Supported - MSBuild Variables

MSBuild properties like `$(SolutionDir)`, `$(Configuration)`, `$(ProjectDir)` are **NOT** natively supported in .runsettings files:

```xml
<!-- ❌ DOES NOT WORK -->
<RunSettings>
  <RunConfiguration>
    <ResultsDirectory>$(SolutionDir)\TestResults</ResultsDirectory>
  </RunConfiguration>

  <TestRunParameters>
    <Parameter name="DATA_PATH" value="$(SolutionDir)\TestData" />
  </TestRunParameters>
</RunSettings>
```

These variables will be treated as literal strings, not expanded.

### ✅ Supported - Environment Variables

Environment variables ARE supported:

**Windows:**
```xml
<RunSettings>
  <RunConfiguration>
    <ResultsDirectory>%TEMP%\TestResults</ResultsDirectory>
  </RunConfiguration>

  <TestRunParameters>
    <Parameter name="USER_HOME" value="%USERPROFILE%" />
    <Parameter name="DATA_PATH" value="%APPDATA%\TestData" />
  </TestRunParameters>
</RunSettings>
```

**Linux/macOS:**
```xml
<RunSettings>
  <RunConfiguration>
    <ResultsDirectory>/tmp/TestResults</ResultsDirectory>
  </RunConfiguration>

  <TestRunParameters>
    <Parameter name="USER_HOME" value="$HOME" />
    <Parameter name="DATA_PATH" value="$HOME/.local/share/TestData" />
  </TestRunParameters>
</RunSettings>
```

### ✅ Supported - Relative Paths

Relative paths work from the test assembly output directory:

```xml
<RunSettings>
  <RunConfiguration>
    <!-- Relative to test assembly location (e.g., bin/Debug/net10.0/) -->
    <ResultsDirectory>.\TestResults\</ResultsDirectory>
  </RunConfiguration>

  <TestRunParameters>
    <!-- Navigate up to solution root -->
    <Parameter name="TEST_DATA_DIR" value="../../../../TestData" />
    <Parameter name="SAMPLE_FILES_DIR" value="../../../../TestData/SampleFiles" />
  </TestRunParameters>
</RunSettings>
```

---

## Environment Variables

### Setting Environment Variables

**During Test Execution:**
```bash
# Linux/macOS
export REDIS_HOST=testserver.example.com
dotnet test --settings src/.runsettings

# Windows (PowerShell)
$env:REDIS_HOST = "testserver.example.com"
dotnet test --settings src/.runsettings

# Windows (CMD)
set REDIS_HOST=testserver.example.com
dotnet test --settings src/.runsettings
```

**In .runsettings with Fallback:**
```xml
<TestRunParameters>
  <!-- Use environment variable if set, otherwise use default -->
  <Parameter name="REDIS_HOST" value="%REDIS_HOST%" />

  <!-- Note: This does NOT work as a fallback (env vars don't support defaults) -->
  <!-- <Parameter name="REDIS_HOST" value="%REDIS_HOST%,localhost%" /> -->
</TestRunParameters>
```

**Best Practice - Provide Defaults:**
```xml
<TestRunParameters>
  <!-- Always provide a sensible default in .runsettings -->
  <Parameter name="REDIS_HOST" value="localhost" />
  <Parameter name="REDIS_PORT" value="6379" />
</TestRunParameters>
```

Then override via environment when needed:
```bash
REDIS_HOST=production.redis.example.com dotnet test
```

---

## Workarounds for MSBuild Variables

If you need MSBuild variable support, here are three approaches:

### Approach 1: MSBuild Transformation (Advanced)

**Step 1:** Create `.runsettings.template`:
```xml
<?xml version="1.0" encoding="utf-8"?>
<RunSettings>
  <TestRunParameters>
    <Parameter name="SOLUTION_DIR" value="$(SolutionDir)" />
    <Parameter name="PROJECT_DIR" value="$(ProjectDir)" />
    <Parameter name="CONFIGURATION" value="$(Configuration)" />
    <Parameter name="TEST_DATA_DIR" value="$(SolutionDir)TestData" />
  </TestRunParameters>
</RunSettings>
```

**Step 2:** Add MSBuild target to test project:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <Target Name="GenerateRunSettings" BeforeTargets="VSTest">
    <PropertyGroup>
      <RunSettingsInputFile>$(MSBuildProjectDirectory)\.runsettings.template</RunSettingsInputFile>
      <RunSettingsOutputFile>$(MSBuildProjectDirectory)\.runsettings</RunSettingsOutputFile>
    </PropertyGroup>

    <!-- Read template -->
    <ReadLinesFromFile File="$(RunSettingsInputFile)">
      <Output TaskParameter="Lines" ItemName="RunSettingsLines"/>
    </ReadLinesFromFile>

    <!-- Replace variables -->
    <PropertyGroup>
      <RunSettingsContent>@(RunSettingsLines, '%0D%0A')</RunSettingsContent>
      <RunSettingsContent>$(RunSettingsContent.Replace('$(SolutionDir)', '$(SolutionDir)'))</RunSettingsContent>
      <RunSettingsContent>$(RunSettingsContent.Replace('$(ProjectDir)', '$(ProjectDir)'))</RunSettingsContent>
      <RunSettingsContent>$(RunSettingsContent.Replace('$(Configuration)', '$(Configuration)'))</RunSettingsContent>
    </PropertyGroup>

    <!-- Write output -->
    <WriteLinesToFile File="$(RunSettingsOutputFile)"
                      Lines="$(RunSettingsContent)"
                      Overwrite="true" />
  </Target>

</Project>
```

**Step 3:** Add to .gitignore:
```gitignore
# Generated runsettings
.runsettings
```

### Approach 2: Multiple Environment-Specific Files

Create separate runsettings files for each environment:

```
src/
  .runsettings              # Local development (localhost, standard ports)
  .runsettings.ci           # CI/CD environment
  .runsettings.docker       # Docker Compose testing
  .runsettings.production   # Production-like testing
```

Then specify in commands:
```bash
# Local development (default)
dotnet test

# CI/CD
dotnet test --settings src/.runsettings.ci

# Docker environment
dotnet test --settings src/.runsettings.docker
```

### Approach 3: Command-Line Override (Simplest)

Override specific parameters via command line:

```bash
dotnet test \
  --settings src/.runsettings \
  -- TestRunParameters.Parameter\(name=\"REDIS_HOST\",value=\"testserver\"\)
```

Multiple parameters:
```bash
dotnet test \
  --settings src/.runsettings \
  -- \
  TestRunParameters.Parameter\(name=\"REDIS_HOST\",value=\"testserver\"\) \
  TestRunParameters.Parameter\(name=\"REDIS_PORT\",value=\"6380\"\)
```

---

## OoBDev Best Practices

### Current Approach (Recommended ✅)

The OoBDev project uses **hardcoded localhost values** in `.runsettings`:

```xml
<TestRunParameters>
  <!-- Docker containers always use standard ports -->
  <Parameter name="REDIS_CONNECTION_STRING" value="localhost:6379" />
  <Parameter name="MONGODB_CONNECTION_STRING" value="mongodb://localhost:27017" />
  <Parameter name="TIKA_URL" value="http://localhost:9998" />
  <!-- ... etc -->
</TestRunParameters>
```

**Why this works:**
- ✅ Docker containers bind to localhost with standard ports
- ✅ Same configuration works for local dev and CI/CD
- ✅ Simple and maintainable
- ✅ No environment variable management needed

### When to Use Environment Variables

Use environment variables for:

**1. Credentials (sensitive data):**
```xml
<TestRunParameters>
  <!-- LiveIntegration tests with real services -->
  <Parameter name="AZURE_B2C_CLIENT_SECRET" value="%AZURE_B2C_CLIENT_SECRET%" />
  <Parameter name="AWS_SECRET_ACCESS_KEY" value="%AWS_SECRET_ACCESS_KEY%" />
  <Parameter name="GROQ_API_KEY" value="%GROQ_API_KEY%" />
</TestRunParameters>
```

**2. Environment-specific endpoints:**
```xml
<TestRunParameters>
  <!-- Allow override for special test environments -->
  <Parameter name="CUSTOM_TEST_ENDPOINT" value="%CUSTOM_TEST_ENDPOINT%" />
</TestRunParameters>
```

### Test Data Paths

For test data files, use relative paths:

```xml
<TestRunParameters>
  <!-- Relative to test assembly output (bin/Debug/net10.0/) -->
  <Parameter name="TEST_DATA_ROOT" value="../../../../TestData" />
  <Parameter name="SAMPLE_IMAGES" value="../../../../TestData/Images" />
  <Parameter name="SAMPLE_DOCUMENTS" value="../../../../TestData/Documents" />
</TestRunParameters>
```

Access in tests:
```csharp
[TestMethod]
public void LoadTestDataTest()
{
    var testDataRoot = TestContext.GetRequiredProperty<string>("TEST_DATA_ROOT");
    var fullPath = Path.GetFullPath(testDataRoot); // Resolves relative path

    var imageFile = Path.Combine(fullPath, "Images", "sample.png");
    Assert.IsTrue(File.Exists(imageFile));
}
```

---

## Examples

### Example 1: Docker Integration Tests (Current OoBDev Setup)

**src/.runsettings:**
```xml
<RunSettings>
  <TestRunParameters>
    <!-- All services run on localhost via Docker -->
    <Parameter name="REDIS_CONNECTION_STRING" value="localhost:6379" />
    <Parameter name="MONGODB_CONNECTION_STRING" value="mongodb://localhost:27017" />
    <Parameter name="RABBITMQ_HOST" value="localhost" />
    <Parameter name="RABBITMQ_PORT" value="5673" />
  </TestRunParameters>
</RunSettings>
```

**Test Code:**
```csharp
[TestClass]
public class RedisIntegrationTests
{
    public required TestContext TestContext { get; set; }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task StoreAndRetrieveTest()
    {
        // Uses localhost:6379 from .runsettings
        var connectionString = TestContext.GetRequiredProperty<string>("REDIS_CONNECTION_STRING");

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "Redis:ConnectionString", connectionString }
            })
            .Build();

        // ... test logic
    }
}
```

### Example 2: Environment-Specific Configuration

**src/.runsettings (base):**
```xml
<RunSettings>
  <TestRunParameters>
    <Parameter name="API_ENDPOINT" value="http://localhost:5000" />
    <Parameter name="TIMEOUT_SECONDS" value="30" />
  </TestRunParameters>
</RunSettings>
```

**src/.runsettings.staging:**
```xml
<RunSettings>
  <TestRunParameters>
    <Parameter name="API_ENDPOINT" value="https://staging.example.com" />
    <Parameter name="TIMEOUT_SECONDS" value="60" />
  </TestRunParameters>
</RunSettings>
```

**Usage:**
```bash
# Local testing
dotnet test

# Staging environment
dotnet test --settings src/.runsettings.staging
```

### Example 3: Combining Static and Dynamic Values

**src/.runsettings:**
```xml
<RunSettings>
  <TestRunParameters>
    <!-- Static defaults -->
    <Parameter name="REDIS_HOST" value="localhost" />
    <Parameter name="REDIS_PORT" value="6379" />

    <!-- Environment variable overrides -->
    <Parameter name="REDIS_PASSWORD" value="%REDIS_PASSWORD%" />

    <!-- Relative paths -->
    <Parameter name="TEST_DATA_DIR" value="../../../../TestData" />
  </TestRunParameters>
</RunSettings>
```

**Test Code:**
```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public void ConfigurationTest()
{
    var host = TestContext.GetRequiredProperty<string>("REDIS_HOST");
    var port = TestContext.GetPropertyOrDefault("REDIS_PORT", 6379);
    var password = TestContext.GetProperty<string>("REDIS_PASSWORD"); // May be null

    var connectionString = password != null
        ? $"{host}:{port},password={password}"
        : $"{host}:{port}";

    // ... test logic
}
```

### Example 4: Results Directory Configuration

**Relative path (recommended):**
```xml
<RunSettings>
  <RunConfiguration>
    <!-- Creates TestResults in test assembly directory -->
    <ResultsDirectory>.\TestResults\</ResultsDirectory>
  </RunConfiguration>
</RunSettings>
```

**Absolute path with environment variable:**
```xml
<RunSettings>
  <RunConfiguration>
    <!-- Windows: C:\Users\YourName\AppData\Local\Temp\TestResults -->
    <ResultsDirectory>%TEMP%\TestResults\</ResultsDirectory>

    <!-- Linux/macOS: /tmp/TestResults -->
    <ResultsDirectory>/tmp/TestResults</ResultsDirectory>
  </RunConfiguration>
</RunSettings>
```

---

## Troubleshooting

### Problem: Variables Not Expanding

**Symptom:**
```csharp
var path = TestContext.GetProperty<string>("DATA_PATH");
// Returns: "$(SolutionDir)\TestData" (literal string)
```

**Solution:**
MSBuild variables are not supported. Use one of these instead:
- Environment variables: `%SOLUTION_DIR%\TestData`
- Relative paths: `../../../../TestData`
- Multiple .runsettings files

### Problem: Environment Variable Not Set

**Symptom:**
```csharp
var apiKey = TestContext.GetRequiredProperty<string>("API_KEY");
// Throws: KeyNotFoundException - no such key API_KEY
```

**Cause:**
Environment variable `%API_KEY%` is not set, so the parameter value is empty.

**Solution 1 - Provide default:**
```xml
<Parameter name="API_KEY" value="default-test-key" />
```

**Solution 2 - Use GetProperty instead:**
```csharp
var apiKey = TestContext.GetProperty<string>("API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Assert.Inconclusive("API_KEY not configured");
    return;
}
```

### Problem: Relative Path Not Found

**Symptom:**
```csharp
var dataDir = TestContext.GetRequiredProperty<string>("TEST_DATA_DIR");
var fullPath = Path.GetFullPath(dataDir);
// fullPath: C:\repos\oobd\src\bin\Debug\net10.0\..\..\..\..\TestData (exists)
// But file access fails
```

**Cause:**
Relative path is relative to the test assembly output directory, which varies by project.

**Solution:**
Use consistent relative paths from bin/Debug/net10.0:
```
bin/Debug/net10.0/           ← Test assembly location
../../../../TestData/        ← Navigate to solution root
../../../../TestData/Images/ ← Actual test data
```

Or use absolute paths with environment variables:
```xml
<Parameter name="TEST_DATA_DIR" value="%USERPROFILE%\TestData" />
```

### Problem: CI/CD Using Different Paths

**Symptom:**
Tests pass locally but fail in CI/CD with path errors.

**Solution 1 - Use environment variables:**
```bash
# CI/CD pipeline
export TEST_DATA_DIR=/opt/test-data
dotnet test
```

**Solution 2 - Separate runsettings:**
```bash
# CI/CD pipeline
dotnet test --settings src/.runsettings.ci
```

**Solution 3 - Copy test data to output:**
```xml
<!-- In test project .csproj -->
<ItemGroup>
  <None Include="..\..\TestData\**\*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <Link>TestData\%(RecursiveDir)%(Filename)%(Extension)</Link>
  </None>
</ItemGroup>
```

Then use relative path:
```xml
<Parameter name="TEST_DATA_DIR" value=".\TestData" />
```

---

## Reference

### Official Documentation

- [Configure unit tests with .runsettings](https://learn.microsoft.com/en-us/visualstudio/test/configure-unit-tests-by-using-a-dot-runsettings-file)
- [VSTest task - Azure Pipelines](https://learn.microsoft.com/en-us/azure/devops/pipelines/tasks/reference/vstest-v2)
- [dotnet test command](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)

### Related OoBDev Documentation

- [TEST_VARIABLES.md](../TEST_VARIABLES.md) - Complete list of test parameters
- [containers/testing/README.md](../../containers/testing/README.md) - Docker infrastructure
- [TESTING-CHECKLIST.md](../../containers/testing/TESTING-CHECKLIST.md) - Local validation

---

## Summary

**Key Takeaways:**

1. ❌ MSBuild variables (`$(SolutionDir)`) are NOT supported in .runsettings
2. ✅ Environment variables (`%VAR%` or `$VAR`) ARE supported
3. ✅ Relative paths work from test assembly output directory
4. ✅ OoBDev uses hardcoded localhost values (works with Docker)
5. ✅ Override via command line: `--settings` or `-- TestRunParameters.Parameter(...)`
6. ✅ Use multiple .runsettings files for different environments
7. ✅ Always provide sensible defaults, allow environment overrides

**Recommended Pattern for OoBDev:**
- Use hardcoded localhost values for Docker-based integration tests
- Use environment variables only for sensitive data or environment-specific endpoints
- Use relative paths for test data files
- Create separate .runsettings files only when truly needed (e.g., .runsettings.ci)

---

**Last Updated:** 2026-01-21
**Maintainer:** OoBDev Framework Team
