# Test Categories Guide

**Purpose:** Define which tests belong in Unit, Simulate, Integration, and DevLocal categories

## Executive Summary

This document provides a comprehensive decision tree and categorization framework for OoBDev tests. It defines four test categories: Unit (fast, isolated logic), Simulate (component interactions with mocks), Integration (real external dependencies), and DevLocal (developer-only, local setup). The guide includes quick reference tables, detailed definitions with code examples, a decision tree for test categorization, real-world examples specific to OoBDev projects, coverage goals by project type, migration paths between categories, and a comprehensive checklist for new tests. Understanding test categories is critical for maintaining fast feedback on PRs while ensuring comprehensive validation through integration testing.

## Table of Contents

- [Quick Reference](#quick-reference)
- [Category Definitions](#category-definitions)
  - [Unit Tests (TestCategory=Unit)](#unit-tests-testcategoryunit)
  - [Simulate Tests (TestCategory=Simulate)](#simulate-tests-testcategorysimulate)
  - [Integration Tests (TestCategory=Integration)](#integration-tests-testcategoryintegration)
  - [DevLocal Tests (TestCategory=DevLocal)](#devlocal-tests-testcategorydevlocal)
- [Decision Tree](#decision-tree)
- [Real-World Examples for OoBDev](#real-world-examples-for-oodev)
  - [OoBDev.System](#oobdevsystem)
  - [OoBDev.IO](#oobdevio)
  - [OoBDev.Data (Database Projects)](#oobdevdata-database-projects)
- [Coverage Goals by Category](#coverage-goals-by-category)
- [Migration Path](#migration-path)
- [Checklist for New Tests](#checklist-for-new-tests)
- [Q&A](#qa)

## Quick Reference

| Category | Environment | Dependencies | Speed | When to Use | RunsInCI |
|----------|-------------|--------------|-------|------------|----------|
| **Unit** | In-memory only | None (pure logic) | ⚡⚡⚡ Fast | Single method/unit | ✅ Always |
| **Simulate** | In-memory with mocks | Mocked external deps | ⚡⚡ Fast | Component behavior | ✅ Always |
| **Integration** | Real external services | Real I/O, DB, network | 🐢 Slow | Multi-component flows | ⏳ Maybe* |
| **DevLocal** | Developer machine only | Local tools/config | ⏸️ Variable | Development only | ❌ Never |

*Integration tests run on main branch only, not on every push

## Category Definitions

### Unit Tests (`TestCategory=Unit`)

**What:** Test a single class/method in isolation

**Environment:** In-memory, no I/O, no external calls

**Example:**
```csharp
[TestClass]
[TestCategory(TestCategories.Unit)]
public class StringHelperTests
{
    [TestMethod]
    public void TrimWhitespace_RemovesLeadingAndTrailing()
    {
        // Arrange
        var input = "  hello world  ";

        // Act
        var result = StringHelper.Trim(input);

        // Assert
        Assert.AreEqual("hello world", result);
    }
}
```

**Characteristics:**
- No dependencies on external systems
- No I/O operations (file, network, database)
- Runs in < 100ms
- Completely deterministic
- No setup/teardown needed

**When to Use:**
- ✅ Testing business logic
- ✅ Testing calculations
- ✅ Testing data transformations
- ✅ Testing error handling
- ❌ NOT for I/O operations
- ❌ NOT for external service calls

**Run Schedule:**
- Every push/PR
- Fast feedback (< 1 second per test)

---

### Simulate Tests (`TestCategory=Simulate`)

**What:** Test component behavior with mocked external dependencies

**Environment:** In-memory with mocks, no real I/O

**Example:**
```csharp
[TestClass]
[TestCategory(TestCategories.Simulate)]
public class EmailServiceTests
{
    private Mock<ISmtpClient> _mockSmtp;
    private EmailService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockSmtp = new Mock<ISmtpClient>();
        _service = new EmailService(_mockSmtp.Object);
    }

    [TestMethod]
    public void SendEmail_CallsSmtpClient()
    {
        // Arrange
        var email = new Email { To = "test@example.com", Subject = "Test" };
        _mockSmtp.Setup(x => x.Send(It.IsAny<string>()))
                .Returns(Task.CompletedTask);

        // Act
        _service.SendEmail(email);

        // Assert
        _mockSmtp.Verify(x => x.Send(It.IsAny<string>()), Times.Once);
    }
}
```

**Characteristics:**
- Tests interaction with dependencies
- Dependencies are mocked/stubbed
- No real I/O, database, or network
- Runs in < 500ms
- Validates method calls and contracts
- Deterministic

**When to Use:**
- ✅ Testing service orchestration
- ✅ Testing dependency injection
- ✅ Testing error handling paths
- ✅ Testing logging/monitoring
- ✅ Testing business workflows
- ❌ NOT for real database operations
- ❌ NOT for real file I/O
- ❌ NOT for real network calls

**Run Schedule:**
- Every push/PR
- Fast feedback (< 2 seconds per test)

---

### Integration Tests (`TestCategory=Integration`)

**What:** Test against real external dependencies

**Environment:** Real databases, file system, network services

**Example:**
```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class FileRepositoryIntegrationTests : IAsyncLifetime
{
    private DatabaseFixture _database;
    private IFileRepository _repository;

    public async Task InitializeAsync()
    {
        _database = new DatabaseFixture();
        await _database.StartAsync();
        _repository = new FileRepository(_database.Connection);
    }

    [TestMethod]
    public async Task SaveFile_PersistsToDatabase()
    {
        // Arrange
        var file = new FileEntity { Name = "test.txt", Size = 1024 };

        // Act
        await _repository.SaveAsync(file);

        // Assert
        var saved = await _repository.GetAsync(file.Id);
        Assert.IsNotNull(saved);
        Assert.AreEqual("test.txt", saved.Name);
    }

    public async Task DisposeAsync()
    {
        await _database.StopAsync();
    }
}
```

**Characteristics:**
- Real external dependencies (database, file system, network)
- Docker containers or local services
- Runs in 100ms - 5+ seconds per test
- Requires setup/teardown
- Tests actual behavior, not mocks
- May be flaky if services are unstable

**When to Use:**
- ✅ Testing database operations
- ✅ Testing file I/O
- ✅ Testing network operations
- ✅ Testing third-party service integration
- ✅ Testing cross-service workflows
- ✅ Testing schema migrations
- ❌ NOT for unit logic (too slow)
- ❌ NOT for simple mocking scenarios
- ❌ NOT for local-only operations

**Run Schedule:**
- Main branch only (not every PR)
- Or scheduled nightly
- Or manual trigger
- Slower feedback (5-15 minutes total)

---

### DevLocal Tests (`TestCategory=DevLocal`)

**What:** Tests that require local development environment

**Environment:** Developer's local machine only

**Example:**
```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class LocalFileSystemTests
{
    private string _testDir;

    [TestInitialize]
    public void Setup()
    {
        // Uses actual user's Documents folder
        _testDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "OoBDevTests"
        );
        Directory.CreateDirectory(_testDir);
    }

    [TestMethod]
    public void CanAccessUserSpecialFolder()
    {
        var path = Environment.GetFolderPath(
            Environment.SpecialFolder.ApplicationData
        );
        Assert.IsTrue(Directory.Exists(path));
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (Directory.Exists(_testDir))
        {
            Directory.Delete(_testDir, recursive: true);
        }
    }
}
```

**Characteristics:**
- Requires developer's local machine
- Uses special folders, local paths, local configuration
- May depend on user-specific setup
- Variable performance
- Cannot run in CI without special setup
- Often uses actual file system, not test containers

**When to Use:**
- ✅ Testing against real user paths
- ✅ Testing Windows Registry access
- ✅ Testing local application data
- ✅ Testing user profile features
- ✅ Testing machine-specific behavior
- ❌ NOT for standard testing in CI
- ❌ NOT for cross-platform testing
- ❌ NOT unless really necessary

**Run Schedule:**
- Developers only (local test run)
- NOT in CI/CD
- Manual execution: `dotnet test --filter TestCategory=DevLocal`

---

## Decision Tree

Use this tree to categorize a new test:

```
┌─ Start: New test
│
├─ Does it test pure logic without I/O?
│  ├─ YES: Go to Unit ✓
│  └─ NO: Continue
│
├─ Does it depend on external services?
│  ├─ YES (database, network, file):
│  │  ├─ Can we mock the dependency?
│  │  │  ├─ YES: Go to Simulate ✓
│  │  │  └─ NO: Go to Integration ✓
│  │
│  └─ NO: Continue
│
├─ Does it require local development setup?
│  ├─ YES (special folders, user paths, local tools):
│  │  └─ Go to DevLocal ✓
│  │
│  └─ NO: Go to Unit ✓
│
└─ End
```

## Real-World Examples for OoBDev

### OoBDev.System

**Unit Tests:**
```csharp
[TestCategory(TestCategories.Unit)]
public class MathExtensionsTests
{
    public void IsEven_WithEvenNumber_ReturnsTrue() { }
    public void IsOdd_WithOddNumber_ReturnsTrue() { }
    public void Clamp_WithValueInRange_ReturnsSame() { }
}
```

**Simulate Tests:**
```csharp
[TestCategory(TestCategories.Simulate)]
public class ConfigurationServiceTests
{
    // Mocks IConfigurationProvider
    public void GetSetting_UsesProvider() { }
    public void SaveSetting_CallsProvider() { }
}
```

**Integration Tests:**
```csharp
[TestCategory(TestCategories.Integration)]
public class ConfigurationDatabaseTests
{
    // Real database connection
    public void GetSetting_ReadsFromDatabase() { }
    public void SaveSetting_PersistsToDatabase() { }
}
```

---

### OoBDev.IO

**Unit Tests:**
```csharp
[TestCategory(TestCategories.Unit)]
public class PathNormalizerTests
{
    public void Normalize_RemovesDoubleSlashes() { }
    public void Normalize_HandlesBackslashes() { }
}
```

**Simulate Tests:**
```csharp
[TestCategory(TestCategories.Simulate)]
public class FileServiceTests
{
    // Mocks IFileSystem
    public void ReadFile_CallsFileSystem() { }
}
```

**Integration Tests:**
```csharp
[TestCategory(TestCategories.Integration)]
public class FileOperationsIntegrationTests
{
    // Real Docker volume for file I/O
    public void CreateFile_ActuallyCreatesFile() { }
    public void ReadFile_ReadsRealContent() { }
}
```

**DevLocal Tests:**
```csharp
[TestCategory(TestCategories.DevLocal)]
public class UserProfileTests
{
    // Uses Environment.SpecialFolder.ApplicationData
    public void GetAppDataPath_ReturnsValidPath() { }
}
```

---

### OoBDev.Data (Database Projects)

**Unit Tests:**
```csharp
[TestCategory(TestCategories.Unit)]
public class QueryBuilderTests
{
    public void BuildSelect_FormatsCorrectly() { }
}
```

**Simulate Tests:**
```csharp
[TestCategory(TestCategories.Simulate)]
public class RepositoryTests
{
    // Mocks IDatabase
    public void GetById_QuerysDatabase() { }
}
```

**Integration Tests:** ⭐ IMPORTANT
```csharp
[TestCategory(TestCategories.Integration)]
public class SqlServerIntegrationTests
{
    // Real SQL Server in Docker
    public void Migration_CreatesSchema() { }
    public void Query_ReturnsCorrectData() { }
    public void StoredProcedure_Executes() { }
}
```

---

## Coverage Goals by Category

| Project Type | Unit | Simulate | Integration | DevLocal |
|--------------|------|----------|-------------|----------|
| **Logic/Utils** | 80%+ | 10% | - | - |
| **Services** | 50%+ | 40%+ | 10% | - |
| **Repositories** | 30% | 20% | 50%+ | - |
| **Database/SQLCLR** | 20% | 10% | 70%+ | - |
| **I/O Operations** | 30% | 20% | 50%+ | - |

*Plus optional DevLocal for special cases*

## Migration Path

If you have an existing test and want to change its category:

### Unit → Simulate
```diff
- [TestCategory(TestCategories.Unit)]
+ [TestCategory(TestCategories.Simulate)]
  public void TestName()
  {
-     // No mocking needed
+     // Add mocks for dependencies
+     var mock = new Mock<IDependency>();
  }
```

### Simulate → Integration
```diff
- [TestCategory(TestCategories.Simulate)]
+ [TestCategory(TestCategories.Integration)]
  public void TestName()
  {
-     var mock = new Mock<IDatabase>();
+     // Use real database
+     var db = await DatabaseFixture.CreateAsync();
  }
```

### Integration → Unit (by extracting logic)
```csharp
// Before: Integration test of complex method
[TestMethod]
public async Task ComplexOperation_Works()
{
    var result = await _service.DoComplexThingWithDatabase();
}

// After: Extract logic to Unit test
[TestMethod]
[TestCategory(TestCategories.Unit)]
public void ComplexLogic_ProcessesCorrectly()
{
    var logic = new ComplexLogic();
    var result = logic.Process(inputData);
}

[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task ComplexOperation_PersistsToDatabase()
{
    var result = await _service.DoComplexThingWithDatabase();
}
```

## Checklist for New Tests

Before committing a new test, verify:

- [ ] Category is correct (Unit/Simulate/Integration/DevLocal)
- [ ] `[TestCategory(...)]` attribute is set
- [ ] Test name follows pattern: `MethodUnderTest_Scenario_ExpectedResult()`
- [ ] Arrange-Act-Assert structure is clear
- [ ] Test is independent (no test order dependencies)
- [ ] Mock/fixture setup is minimal
- [ ] Comments explain "why", not "what"
- [ ] Integration tests have proper async/await
- [ ] DevLocal tests are documented as developer-only
- [ ] No hardcoded paths (use fixtures/temp)
- [ ] No external service calls (except Integration tests)

## Q&A

**Q: If I'm testing a method that calls a database, should it be Simulate or Integration?**

A: Depends on your goal:
- **Simulate:** Test that the method calls the repository correctly (mock the repository)
- **Integration:** Test that data actually persists (real database)

Both are valid. Use Simulate for logic testing, Integration for end-to-end flows.

**Q: Can I mix mocks and real services in an Integration test?**

A: Yes! Common pattern:
```csharp
[TestCategory(TestCategories.Integration)]
public void TestComplexFlow()
{
    // Real database (Integration)
    var realDb = new RealDatabase();

    // Mocked external API (we can't test that in Integration)
    var mockApi = new Mock<IExternalApi>();

    var service = new ComplexService(realDb, mockApi.Object);
}
```

**Q: Should test performance matter in categorization?**

A: No. Categorize by what you're testing, not how fast it is. But:
- Unit/Simulate should be < 500ms
- Integration can be slower
- If a test is very slow, consider splitting it

**Q: What about tests for async methods?**

A: Use the same categorization:
```csharp
[TestCategory(TestCategories.Unit)]
public async Task AsyncMethod_Returns_CorrectValue()
{
    var result = await MethodUnderTest();
}
```

**Q: Can DevLocal tests be converted to Integration?**

A: Sometimes! If a DevLocal test doesn't actually need the user's machine:
```csharp
// Before: DevLocal (requires actual Windows temp folder)
[TestCategory(TestCategories.DevLocal)]
public void CreateTempFile() { }

// After: Integration (uses Docker volume)
[TestCategory(TestCategories.Integration)]
public void CreateTempFile() { }
```

This will be evaluated in Phase 4.
