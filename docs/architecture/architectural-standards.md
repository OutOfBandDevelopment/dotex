# Architectural Standards

**Version:** 1.1
**Last Updated:** 2026-01-21
**Project:** OoBDev (dotex) - .NET Extensions Framework

---

## Overview

This document defines concrete, measurable standards that MUST be followed for all code in the OoBDev framework. These standards are enforced through code reviews, build processes, and automated tooling where possible.

**Compliance:** All standards marked with ✅ MUST be followed. Deviations require architectural review and approval.

---

## 1. Project Structure Standards

### 1.1 Project Naming

**Standard:** All projects MUST follow the naming convention: `OoBDev.{Category}.{Subcategory}`

**Required Patterns:**
```
✅ MUST: OoBDev.MessageQueueing.Abstractions
✅ MUST: OoBDev.MessageQueueing
✅ MUST: OoBDev.MessageQueueing.Hosting
✅ MUST: OoBDev.MessageQueueing.Tests
✅ MUST: OoBDev.RabbitMQ.Abstractions
✅ MUST: OoBDev.RabbitMQ

❌ MUST NOT: MessageQueueing.Abstractions
❌ MUST NOT: OoBDev-MessageQueueing
❌ MUST NOT: OoBDev_MessageQueueing
```

**Project Type Suffixes:**
- `.Abstractions` - Interfaces, models, enums
- `.Tests` - Test projects (MSTest)
- `.Hosting` - Background service hosting
- `.Cli` - Command-line tools

**Validation:**
- Build process warns if naming convention is violated
- Code review checklist includes naming verification

**Reference:** `src/` directory structure

---

### 1.2 Directory Structure

**Standard:** Projects MUST be organized into layer-specific directories.

**Required Structure:**
```
src/
├── Common/                    ✅ Orchestration projects
├── Framework/                 ✅ Domain-specific libraries
├── Extensions/                ✅ Custom system extensions
├── ExternalServices/          ✅ Third-party integrations
├── Examples/                  ✅ Sample applications
└── Tools/                     ✅ CLI utilities
```

**Placement Rules:**

| Project Type | Directory | Example |
|-------------|-----------|---------|
| Orchestration (all-in-one) | `Common/` | OoBDev.Common.Complete |
| Core framework library | `Framework/` | OoBDev.MessageQueueing |
| System extension | `Extensions/` | OoBDev.Data.Vectors |
| External service wrapper | `ExternalServices/` | OoBDev.RabbitMQ |
| Example application | `Examples/` | OoBDev.WebApi |
| CLI tool | `Tools/` | OoBDev.DacPacCompiler.Cli |

**Validation:**
- Code review verifies correct placement
- Build script can detect misplaced projects

**Reference:** `src/Common/`, `src/Framework/`, `src/ExternalServices/`

---

### 1.3 Required Files

**Standard:** Every project MUST contain a `README.md` file.

**Enforcement:**
```xml
<!-- Directory.Build.targets -->
<Target Name="CheckReadMe" BeforeTargets="PrepareForBuild">
  <Error Condition="!Exists('README.md')" Code="OBDPK002"
         Text="README.md is required for all projects" />
</Target>
```

**README Requirements:**
1. Project summary (1-2 paragraphs)
2. Features list
3. Usage examples
4. Configuration options (if applicable)
5. Related notes/links

**Validation:**
- ✅ Build fails if README.md is missing
- ✅ Build task `CheckReadMeSize` validates README is not empty

**Reference:** `/docs/code/*/README.md` for examples

---

### 1.4 Project File Configuration

**Standard:** All `.csproj` files MUST include required metadata.

**Required Properties:**
```xml
<PropertyGroup>
  <!-- ✅ MUST: Target framework -->
  <TargetFramework>net10.0</TargetFramework>

  <!-- ✅ MUST: Nullable enabled -->
  <Nullable>enable</Nullable>

  <!-- ✅ MUST: Implicit usings disabled -->
  <ImplicitUsings>disable</ImplicitUsings>

  <!-- ✅ MUST: Generate documentation for public APIs -->
  

  <!-- ✅ MUST: Package metadata -->
  <Authors>Matthew Whited</Authors>
  <Company>Out-of-Band Development, LLC</Company>
  <RepositoryUrl>https://github.com/OutOfBandDevelopment/dotex/</RepositoryUrl>

  <!-- ✅ MUST: Package README -->
  <PackageReadmeFile>README.md</PackageReadmeFile>

  <!-- ✅ MUST: License file -->
  <PackageLicenseFile>LICENSE.txt</PackageLicenseFile>
</PropertyGroup>

<!-- ✅ MUST: Include README.md in NuGet package -->
<ItemGroup>
  <None Include="README.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

**Additional Requirements for Libraries:**
```xml
<PropertyGroup>
  <!-- ✅ MUST: Generate NuGet package -->
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>

  <!-- ✅ MUST: Source Link support -->
  <PublishRepositoryUrl>true</PublishRepositoryUrl>
  <EmbedUntrackedSources>true</EmbedUntrackedSources>
  <IncludeSymbols>true</IncludeSymbols>
  <SymbolPackageFormat>snupkg</SymbolPackageFormat>
</PropertyGroup>
```

**Validation:**
- ✅ Build task `AutoSetPackageReadmeFile` validates README configuration
- ✅ Analyzer: OBDPK001 (missing PackageReadmeFile)
- ✅ NuGet package validation ensures metadata is complete
- ✅ Build fails if README.md is missing or not included in package

**Reference:** `Directory.Build.props`, `Directory.Build.targets`

---

## 2. Layer Dependencies

### 2.1 Dependency Direction

**Standard:** Dependencies MUST flow downward through layers.

**Layer Hierarchy (Top to Bottom):**
1. Common (depends on everything)
2. Framework (depends on Extensions, ExternalServices)
3. Extensions (depends on ExternalServices)
4. ExternalServices (depends on nothing in OoBDev)

**Allowed Dependencies:**
```
✅ Common → Framework
✅ Common → Extensions
✅ Common → ExternalServices
✅ Framework → Extensions
✅ Framework → ExternalServices
✅ Extensions → ExternalServices

❌ Framework → Common
❌ Extensions → Framework
❌ ExternalServices → Framework
❌ ExternalServices → Extensions
❌ ExternalServices → Common
```

**Validation:**
- Code review checks project references
- Future: Automated dependency validation tool

**Reference:** `src/Common/OoBDev.Common/OoBDev.Common.csproj` (uses wildcard references)

---

### 2.2 Abstraction Dependencies

**Standard:** Implementation projects MUST reference their corresponding `.Abstractions` project.

**Pattern:**
```xml
<!-- OoBDev.RabbitMQ/OoBDev.RabbitMQ.csproj -->
<ItemGroup>
  <!-- ✅ MUST reference own abstractions -->
  <ProjectReference Include="..\OoBDev.RabbitMQ.Abstractions\OoBDev.RabbitMQ.Abstractions.csproj" />

  <!-- ✅ MUST reference framework abstractions -->
  <ProjectReference Include="..\..\Framework\OoBDev.MessageQueueing.Abstractions\OoBDev.MessageQueueing.Abstractions.csproj" />

  <!-- ❌ MUST NOT reference implementation projects -->
  <ProjectReference Include="..\..\Framework\OoBDev.MessageQueueing\OoBDev.MessageQueueing.csproj" />
</ItemGroup>
```

**Validation:**
- Code review verifies abstraction references
- Future: Build analyzer to detect abstraction violations

**Reference:** `src/ExternalServices/RabbitMQ/OoBDev.RabbitMQ/OoBDev.RabbitMQ.csproj`

---

## 3. Coding Standards

### 3.1 Naming Conventions

**Standard:** Follow C# naming conventions strictly.

**Required Conventions:**

| Element | Convention | Example |
|---------|-----------|---------|
| Namespace | PascalCase, match project | `OoBDev.MessageQueueing` |
| Class | PascalCase | `MessageQueueHandler` |
| Interface | PascalCase with `I` prefix | `IMessageQueueHandler` |
| Method | PascalCase | `SendAsync` |
| Async Method | PascalCase + `Async` suffix | `ProcessMessageAsync` |
| Property | PascalCase | `MessageId` |
| Field (private) | camelCase or _camelCase | `_sender` or `sender` |
| Constant | PascalCase | `MaxRetryCount` |
| Parameter | camelCase | `message` |
| Type Parameter | PascalCase with `T` prefix | `TMessage`, `TChannel` |

**Special Patterns:**
```csharp
// ✅ Async methods end with "Async"
public async Task<Result> SendAsync(Message message);

// ✅ Factory methods named "Create"
public IProvider Create(string key);

// ✅ Extension methods start with "TryAdd" for non-replacing registration
public static IServiceCollection TryAddMessageQueueing(
    this IServiceCollection services);

// ✅ Boolean methods/properties start with "Is", "Has", "Can"
public bool IsValid { get; }
public bool HasValue { get; }
public bool CanProcess(Message message);
```

**Validation:**
- Code review enforces naming conventions
- Analyzers: CA1707, CA1715, CA1716, CA1717

**Reference:** `src/Framework/*/` for examples

---

### 3.2 Implicit Usings

**Standard:** Implicit usings MUST be disabled.

**Configuration:**
```xml
<PropertyGroup>
  <ImplicitUsings>disable</ImplicitUsings>
</PropertyGroup>
```

**Rationale:**
- Explicit control over dependencies
- No hidden using statements
- Easier to understand code dependencies

**Validation:**
- ✅ Build configuration enforces this
- All project files are validated

**Reference:** `Directory.Build.props:9`

---

### 3.3 Nullable Reference Types

**Standard:** Nullable reference types MUST be enabled for all projects.

**Configuration:**
```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
</PropertyGroup>
```

**Usage:**
```csharp
// ✅ Nullable explicitly marked
public string? OptionalValue { get; set; }

// ✅ Non-nullable is default
public string RequiredValue { get; set; } = string.Empty;

// ✅ Null checks before use
if (optionalValue is not null)
{
    Console.WriteLine(optionalValue);
}

// ❌ Compiler warning for potential null reference
Console.WriteLine(optionalValue); // Warning CS8602
```

**Validation:**
- ✅ Build configuration enforces this
- Compiler warnings for nullable violations

**Reference:** `Directory.Build.props:6`

---

### 3.4 XML Documentation

**Standard:** All public APIs MUST have XML documentation.

**Configuration:**
```xml
<PropertyGroup>
  
</PropertyGroup>
```

**Required Documentation:**
```csharp
/// <summary>
/// Sends a message to the specified queue.
/// </summary>
/// <typeparam name="TMessage">The message type. Must be a reference type.</typeparam>
/// <param name="message">The message to send. Cannot be null.</param>
/// <param name="cancellationToken">
/// Optional cancellation token to cancel the operation.
/// </param>
/// <returns>
/// A task representing the asynchronous send operation.
/// </returns>
/// <exception cref="ArgumentNullException">
/// Thrown when <paramref name="message"/> is null.
/// </exception>
public Task SendAsync<TMessage>(
    TMessage message,
    CancellationToken cancellationToken = default)
    where TMessage : class;
```

**Minimum Requirements:**
- `<summary>` for all public types and members
- `<param>` for all parameters
- `<typeparam>` for all type parameters
- `<returns>` for non-void methods
- `<exception>` for all documented exceptions

**Validation:**
- Code review checks documentation
- Analyzer: CS1591 (missing XML documentation)

**Reference:** `src/Framework/*/` for examples

---

### 3.5 Async/Await Patterns

**Standard:** All I/O operations MUST be asynchronous.

**Required Patterns:**
```csharp
// ✅ MUST: Async method signature
public async Task<Result> ProcessAsync(
    Message message,
    CancellationToken cancellationToken = default)
{
    // ✅ MUST: ConfigureAwait(false) in library code
    var data = await _repository.GetAsync(message.Id)
        .ConfigureAwait(false);

    // ✅ MUST: Pass cancellationToken
    await _sender.SendAsync(data, cancellationToken)
        .ConfigureAwait(false);

    return Result.Success();
}

// ❌ MUST NOT: Blocking on async
public Result Process(Message message)
{
    var data = _repository.GetAsync(message.Id).Result; // ❌ Deadlock risk
    return Result.Success();
}

// ❌ MUST NOT: async void (except event handlers)
public async void ProcessMessage() // ❌ No way to await or catch exceptions
{
}
```

**CancellationToken Requirements:**
- ✅ MUST be optional parameter with default value
- ✅ MUST be passed to all async calls
- ✅ MUST be named `cancellationToken`

**Validation:**
- Code review checks async patterns
- Analyzer: CA2007 (ConfigureAwait)
- Analyzer: CA1068 (CancellationToken last parameter)

**Reference:** `src/Framework/*/` for async patterns

---

## 4. Dependency Injection Standards

### 4.1 Registration Pattern

**Standard:** Use `TryAdd*` extensions for non-conflicting registration.

**Required Pattern:**
```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddMyExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<MyExtensionBuilder>? configure = null)
    {
        // ✅ MUST: Use TryAdd* to avoid conflicts
        services.TryAddSingleton<IMyService, MyService>();
        services.TryAddScoped<IMyFactory, MyFactory>();

        // ✅ MUST: Use builder pattern for complex configuration
        var builder = new MyExtensionBuilder(services, configuration);
        configure?.Invoke(builder);

        return services;
    }
}

// ❌ MUST NOT: Use Add* (conflicts with existing registrations)
services.AddSingleton<IMyService, MyService>(); // Throws if already registered
```

**Validation:**
- Code review verifies TryAdd* usage
- Test: Verify double registration doesn't throw

**Reference:** `src/Common/*/` for extension methods

---

### 4.2 Keyed Services

**Standard:** Use keyed services for multiple implementations of the same interface.

**Required Pattern:**
```csharp
// ✅ MUST: Register with keys
services.AddKeyedSingleton<IVectorStoreProvider, QdrantProvider>("qdrant");
services.AddKeyedSingleton<IVectorStoreProvider, OpenSearchProvider>("opensearch");

// ✅ MUST: Provide factory for key resolution
services.AddSingleton<IVectorStoreProviderFactory, VectorStoreProviderFactory>();

public class VectorStoreProviderFactory : IVectorStoreProviderFactory
{
    private readonly IServiceProvider _serviceProvider;

    public VectorStoreProviderFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    // ✅ MUST: Resolve using GetRequiredKeyedService
    public IVectorStoreProvider Create(string providerKey)
    {
        return _serviceProvider.GetRequiredKeyedService<IVectorStoreProvider>(providerKey);
    }
}
```

**When to Use:**
- ✅ Multiple implementations of same interface (message queues, vector stores, LLMs)
- ✅ Configuration-driven provider selection
- ❌ Single implementation (use regular DI)

**Validation:**
- Code review verifies keyed service usage
- Test: Verify factory can resolve multiple keys

**Reference:** `src/Framework/OoBDev.Search/`, `src/Framework/OoBDev.MessageQueueing/`

---

### 4.3 Configuration Options

**Standard:** Use strongly-typed options pattern for all configuration.

**Required Pattern:**
```csharp
// ✅ MUST: Options class
public class RabbitMQOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string? UserName { get; set; }
    public string? Password { get; set; }

    // ✅ SHOULD: Include validation
    public void Validate()
    {
        if (string.IsNullOrEmpty(HostName))
            throw new InvalidOperationException("HostName is required");

        if (Port < 1 || Port > 65535)
            throw new InvalidOperationException("Port must be between 1 and 65535");
    }
}

// ✅ MUST: Register options
services.Configure<RabbitMQOptions>(configuration.GetSection("RabbitMQ"));

// ✅ MUST: Inject IOptions<T>
public class RabbitMQProvider
{
    private readonly RabbitMQOptions _options;

    public RabbitMQProvider(IOptions<RabbitMQOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _options.Validate(); // ✅ Validate on construction
    }
}
```

**Validation:**
- Code review verifies options pattern
- Test: Verify validation throws for invalid config

**Reference:** `src/ExternalServices/*/` for options examples

---

## 5. Testing Standards

### 5.1 Test Framework

**Standard:** All tests MUST use MSTest framework.

**Required Configuration:**
```xml
<ItemGroup>
  <PackageReference Include="MSTest.TestAdapter" />
  <PackageReference Include="MSTest.TestFramework" />
  <PackageReference Include="coverlet.collector" />
</ItemGroup>
```

**Test Class Pattern:**
```csharp
[TestClass]
public class MyServiceTests
{
    [TestMethod]
    [TestCategory("Unit")]
    public void MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var service = new MyService();

        // Act
        var result = service.MethodName(input);

        // Assert
        Assert.AreEqual(expected, result);
    }
}
```

**Validation:**
- ✅ Build includes test projects
- ✅ CI/CD runs tests automatically

**Reference:** `src/Framework/*/Tests/`

---

### 5.2 Test Categories

**Standard:** All tests MUST be categorized.

**Required Categories:**
```csharp
[TestCategory("Unit")]       // Unit tests (no external dependencies)
[TestCategory("Simulate")]   // Simulation tests (mocked external dependencies)
// No category               // Integration tests (real external dependencies)
```

**CI/CD Filtering:**
```
# Only run Unit and Simulate tests in CI
dotnet test --filter "TestCategory=Unit|TestCategory=Simulate"
```

**Validation:**
- Code review verifies test categories
- CI/CD configuration enforces filtering

**Reference:** `.github/workflows/dotnet.yml:77`

---

### 5.3 Code Coverage Goals

**Standard:** Projects MUST meet minimum code coverage thresholds.

**Required Coverage:**
| Project Type | Minimum Coverage |
|-------------|------------------|
| Framework (core) | 80% |
| LINQ/Query | 90% |
| External Services | 50% |
| Overall | 60% |

**Current Coverage:**
- `OoBDev.System.Linq`: 90.5% ✅
- `OoBDev.MessageQueueing`: 90.4% ✅
- `OoBDev.Handlebars`: 85% ✅
- `OoBDev.TestUtilities`: 78.5% ✅

**Validation:**
- ✅ CI/CD collects coverage with Coverlet
- Code review checks coverage reports
- Future: Fail build if coverage drops below threshold

**Reference:** `.runsettings:12-29`

---

### 5.4 Test Organization

**Standard:** Test projects MUST follow structure conventions.

**Required Structure:**
```
OoBDev.MyProject.Tests/
├── README.md                          ✅ Required
├── OoBDev.MyProject.Tests.csproj      ✅ Test project file
├── MyServiceTests.cs                  ✅ One test class per production class
├── MyFactoryTests.cs
└── Integration/                       ❌ Optional: Integration test subdirectory
    └── MyServiceIntegrationTests.cs
```

**Test Class Naming:**
- ✅ MUST: `{ProductionClassName}Tests.cs`
- ✅ Example: `MessageQueueHandler` → `MessageQueueHandlerTests`

**Validation:**
- Code review verifies test organization

**Reference:** `src/Framework/*/Tests/`

---

## 6. Documentation Standards

### 6.1 README Requirements

**Standard:** README.md MUST follow template structure.

**Required Sections:**
```markdown
# ProjectName

## Summary
[Brief description]

## Features
- Feature 1
- Feature 2

## Usage
[Code examples]

## Configuration
[Configuration options]

## Related Notes
- [Link to detailed docs]
```

**Validation:**
- ✅ Build fails if README is missing
- Code review verifies README completeness

**Reference:** `/docs/code/*/README.md`

---

### 6.2 Framework Documentation

**Standard:** New patterns MUST be documented in `/docs/Framework/`.

**Required Documentation:**
- PlantUML diagram showing architecture
- Usage examples with code references
- Configuration options
- Extension points
- Related patterns

**Reference:** `/docs/Framework/MessageQueueing.md`, `/docs/Framework/TextTemplating.md`

---

### 6.3 Library Documentation

**Standard:** New integrations MUST be documented in `/docs/Libraries/`.

**Required Content:**
- Provider description
- Configuration options
- Usage examples
- Limitations
- External service requirements

**Reference:** `/docs/Libraries/*.md`

---

## 7. Build Standards

### 7.1 Version Management

**Standard:** Use GitVersion for semantic versioning.

**Configuration:**
```yaml
mode: ContinuousDeployment
branches:
  main:
    increment: Patch
  feature:
    regex: ^(?!main$).*
    increment: Inherit
```

**Version Format:**
- Production: `1.2.3`
- Debug: `1.2.3-debug.4+5abc123`

**Validation:**
- ✅ GitVersion runs in CI/CD
- ✅ Commit is tagged with version

**Reference:** `GitVersion.yml`

---

### 7.2 NuGet Packaging

**Standard:** All libraries MUST generate NuGet packages.

**Required Configuration:**
```xml
<PropertyGroup>
  <GeneratePackageOnBuild>true</GeneratePackageOnBuild>
  <PackageOutputPath>../../publish/packages/</PackageOutputPath>
</PropertyGroup>
```

**Package Contents:**
- ✅ README.md
- ✅ LICENSE.txt
- ✅ Embedded examples
- ✅ XML documentation

**Validation:**
- ✅ Build creates packages in `/publish/packages/`
- NuGet package validation

**Reference:** `Directory.Build.props:22-23`

---

### 7.3 Embedded Resources

**Standard:** Examples and documentation MUST be embedded.

**Required Pattern:**
```xml
<ItemGroup>
  <EmbeddedResource Include="Examples\*.txt" />
  <EmbeddedResource Include="Examples\*.json" />
  <EmbeddedResource Include="Examples\*.html" />
  <EmbeddedResource Include="Examples\*.csv" />
  <EmbeddedResource Include="Examples\*.sql" />
  <EmbeddedResource Include="Examples\*.xml" />
  <EmbeddedResource Include="Examples\*.yml" />
  <None Include="README.md" Pack="true" PackagePath="\" />
</ItemGroup>
```

**Validation:**
- Code review verifies embedded resources
- Test: Verify resources are accessible at runtime

**Reference:** `Directory.Build.props:37-45`

---

## 8. Security Standards

### 8.1 No Hardcoded Secrets

**Standard:** Secrets MUST NOT be hardcoded in source code.

**Required:**
```csharp
// ✅ MUST: Load from configuration
var connectionString = configuration.GetConnectionString("Database");

// ❌ MUST NOT: Hardcoded secrets
var connectionString = "Server=myserver;User=admin;Password=secret123";
```

**Validation:**
- Code review checks for hardcoded secrets
- Future: Secret scanning in CI/CD

---

### 8.2 Input Validation

**Standard:** All public APIs MUST validate input.

**Required:**
```csharp
public async Task<Result> ProcessAsync(Message message)
{
    // ✅ MUST: Validate arguments
    ArgumentNullException.ThrowIfNull(message);

    if (string.IsNullOrEmpty(message.Id))
        throw new ArgumentException("Message ID is required", nameof(message));

    // Process validated input
}
```

**Validation:**
- Code review verifies input validation
- Analyzer: CA1062 (Validate arguments)

---

## 9. Performance Standards

### 9.1 Async I/O

**Standard:** All I/O operations MUST be asynchronous.

**Required:**
- ✅ Database calls are async
- ✅ HTTP calls are async
- ✅ File I/O is async
- ✅ Message queue operations are async

**Validation:**
- Code review checks for blocking calls
- Analyzer: CA1849 (Async method missing await)

---

### 9.2 ConfigureAwait

**Standard:** Library code MUST use `ConfigureAwait(false)`.

**Required:**
```csharp
// ✅ MUST: ConfigureAwait(false) in libraries
var result = await _httpClient.GetAsync(url).ConfigureAwait(false);

// ❌ Application code: ConfigureAwait not needed
```

**Validation:**
- Code review verifies ConfigureAwait
- Analyzer: CA2007 (ConfigureAwait)

**Reference:** All async methods in `src/Framework/*/`

---

## 10. Compliance Checklist

Use this checklist for code reviews and new projects:

### Project Setup
- [ ] Project follows naming convention
- [ ] Project in correct layer directory
- [ ] README.md exists and is complete
- [ ] .csproj includes all required metadata
- [ ] PackageReadmeFile property set to README.md
- [ ] README.md included in ItemGroup with Pack="true"
- [ ] Nullable enabled
- [ ] ImplicitUsings disabled
- [ ] GenerateDocumentationFile enabled

### Dependencies
- [ ] Dependencies flow downward through layers
- [ ] Abstractions referenced correctly
- [ ] No circular dependencies

### Code Quality
- [ ] Naming conventions followed
- [ ] XML documentation on all public APIs
- [ ] Async/await used correctly
- [ ] ConfigureAwait(false) in libraries
- [ ] Input validation on all public APIs
- [ ] No hardcoded secrets

### Dependency Injection
- [ ] TryAdd* extensions used
- [ ] Keyed services for multiple implementations
- [ ] Options pattern for configuration
- [ ] Configuration validation

### Testing
- [ ] Test project exists
- [ ] Tests use MSTest
- [ ] Tests are categorized
- [ ] Coverage meets minimum threshold
- [ ] Test organization follows conventions

### Documentation
- [ ] README complete
- [ ] Framework docs updated (if new pattern)
- [ ] Library docs created (if integration)
- [ ] PlantUML diagrams included
- [ ] Examples embedded

### Build
- [ ] NuGet package generated
- [ ] Embedded resources included
- [ ] GitVersion configuration correct
- [ ] Build succeeds without warnings

---

## Enforcement

### Build-Time Enforcement
- ✅ README.md required (custom MSBuild target)
- ✅ Nullable enabled (compiler)
- ✅ XML documentation (compiler warnings)
- ✅ NuGet package metadata (build tasks)

### CI/CD Enforcement
- ✅ All tests pass
- ✅ Code coverage collected
- ✅ Version tagging

### Manual Enforcement
- Code review checklist
- Architectural review for new patterns
- Regular architecture audits

---

## Related Documentation

- [architectural-guidelines.md](./architectural-guidelines.md) - High-level principles
- [architectural-patterns.md](./architectural-patterns.md) - Pattern documentation
- [layering-architecture.md](./layering-architecture.md) - Layer details

---

## Change Log

- 2026-01-21 v1.1: Added explicit requirement for README.md ItemGroup in .csproj files
- 2026-01-12 v1.0: Initial architectural standards created
