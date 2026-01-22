# Database Template Source - Testing Strategy

**Epic:** 10 - Text Templating Extensions
**Feature:** Database Template Source
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 80%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (40+ tests)
- **Integration Tests** - End-to-end with SQL Server (20+ tests)
- **Performance Tests** - Query and caching benchmarks (5+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (5 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (20 tests)
                  │  (SQL Server)     │
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (40+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### 1. DatabaseTemplateSource Tests

**File:** `DatabaseTemplateSourceTests.cs`

```csharp
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Text.Templating.Sources;

namespace OoBDev.System.Text.Templating.Tests;

[TestClass]
public class DatabaseTemplateSourceTests
{
    private Mock<ITemplateRepository> _mockRepository;
    private IOptions<DatabaseTemplateSourceOptions> _options;
    private DatabaseTemplateSource _source;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<ITemplateRepository>();

        _options = Options.Create(new DatabaseTemplateSourceOptions
        {
            ConnectionString = "Server=test;Database=test;",
            TenantId = Guid.NewGuid(),
            Culture = "en-US"
        });

        _source = new DatabaseTemplateSource(
            _mockRepository.Object,
            _options,
            NullLogger<DatabaseTemplateSource>.Instance
        );
    }

    [TestMethod]
    public void GetTemplates_ReturnsActiveTemplates()
    {
        // Arrange
        var templates = new[]
        {
            new TemplateEntity
            {
                Id = 1,
                Name = "template1",
                ContentType = "text/x-handlebars-template",
                Content = "Hello {{Name}}!",
                Version = 1,
                IsActive = true
            },
            new TemplateEntity
            {
                Id = 2,
                Name = "template2",
                ContentType = "text/x-handlebars-template",
                Content = "Goodbye {{Name}}!",
                Version = 1,
                IsActive = true
            }
        };

        _mockRepository
            .Setup(r => r.GetActiveTemplatesAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(templates);

        // Act
        var result = _source.GetTemplates();

        // Assert
        Assert.AreEqual(2, result.Count());
        Assert.AreEqual("template1", result.First().Name);
        Assert.AreEqual("template2", result.Last().Name);
    }

    [TestMethod]
    public void GetTemplates_PassesTenantAndCultureToRepository()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var culture = "es-ES";

        _options = Options.Create(new DatabaseTemplateSourceOptions
        {
            ConnectionString = "Server=test;",
            TenantId = tenantId,
            Culture = culture
        });

        _source = new DatabaseTemplateSource(
            _mockRepository.Object,
            _options,
            NullLogger<DatabaseTemplateSource>.Instance
        );

        _mockRepository
            .Setup(r => r.GetActiveTemplatesAsync(
                tenantId,
                culture,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<TemplateEntity>());

        // Act
        _source.GetTemplates();

        // Assert
        _mockRepository.Verify(
            r => r.GetActiveTemplatesAsync(tenantId, culture, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public void GetTemplate_ByName_ReturnsMatchingTemplate()
    {
        // Arrange
        var template = new TemplateEntity
        {
            Id = 1,
            Name = "welcome-email",
            ContentType = "text/x-handlebars-template",
            Content = "Hello!",
            Version = 1
        };

        _mockRepository
            .Setup(r => r.GetByNameAsync(
                "welcome-email",
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        var result = _source.GetTemplate("welcome-email");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("welcome-email", result.Name);
        Assert.AreEqual("text/x-handlebars-template", result.ContentType);
    }

    [TestMethod]
    public void GetTemplate_NotFound_ReturnsNull()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetByNameAsync(
                "non-existent",
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemplateEntity?)null);

        // Act
        var result = _source.GetTemplate("non-existent");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetTemplate_WithVersion_RequestsSpecificVersion()
    {
        // Arrange
        var template = new TemplateEntity
        {
            Id = 1,
            Name = "template",
            Version = 2
        };

        _mockRepository
            .Setup(r => r.GetByNameAsync(
                "template",
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        var result = _source.GetTemplate("template", version: 2);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("2", result.Version);
    }

    [TestMethod]
    public void GetTemplate_IncludesMetadata()
    {
        // Arrange
        var template = new TemplateEntity
        {
            Id = 123,
            Name = "template",
            ContentType = "text/plain",
            Content = "content",
            Version = 1,
            Culture = "en-US",
            Category = "email",
            TenantId = Guid.NewGuid(),
            CreatedAt = new DateTime(2026, 1, 1),
            UpdatedAt = new DateTime(2026, 1, 22),
            CreatedBy = "admin@example.com",
            UpdatedBy = "editor@example.com",
            Description = "Test template",
            Tags = "test,email"
        };

        _mockRepository
            .Setup(r => r.GetByNameAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        // Act
        var result = _source.GetTemplate("template");

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(123, result.Metadata["TemplateId"]);
        Assert.AreEqual(template.TenantId, result.Metadata["TenantId"]);
        Assert.AreEqual(template.CreatedAt, result.Metadata["CreatedAt"]);
        Assert.AreEqual(template.UpdatedAt, result.Metadata["UpdatedAt"]);
        Assert.AreEqual("admin@example.com", result.Metadata["CreatedBy"]);
        Assert.AreEqual("editor@example.com", result.Metadata["UpdatedBy"]);
        Assert.AreEqual("Test template", result.Metadata["Description"]);
        Assert.AreEqual("test,email", result.Metadata["Tags"]);
    }

    [TestMethod]
    [ExpectedException(typeof(TemplateRepositoryException))]
    public void GetTemplates_RepositoryError_ThrowsException()
    {
        // Arrange
        _mockRepository
            .Setup(r => r.GetActiveTemplatesAsync(
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new SqlException("Connection failed"));

        // Act
        _source.GetTemplates();  // Should throw
    }
}
```

---

### 2. DatabaseTemplateContentSource Tests

**File:** `DatabaseTemplateContentSourceTests.cs`

```csharp
[TestClass]
public class DatabaseTemplateContentSourceTests
{
    [TestMethod]
    public async Task GetContentAsync_LoadsContentFromRepository()
    {
        // Arrange
        var mockRepository = new Mock<ITemplateRepository>();
        var template = new TemplateEntity
        {
            Id = 1,
            Content = "Hello {{Name}}!"
        };

        mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var source = new DatabaseTemplateContentSource(
            1,
            mockRepository.Object,
            NullLogger.Instance
        );

        // Act
        var content = await source.GetContentAsync();

        // Assert
        Assert.AreEqual("Hello {{Name}}!", content);
    }

    [TestMethod]
    public async Task GetContentAsync_CachesContent()
    {
        // Arrange
        var mockRepository = new Mock<ITemplateRepository>();
        var template = new TemplateEntity
        {
            Id = 1,
            Content = "Hello!"
        };

        mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var source = new DatabaseTemplateContentSource(
            1,
            mockRepository.Object,
            NullLogger.Instance
        );

        // Act
        var content1 = await source.GetContentAsync();
        var content2 = await source.GetContentAsync();

        // Assert
        Assert.AreEqual(content1, content2);
        mockRepository.Verify(
            r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);  // Only called once (cached)
    }

    [TestMethod]
    [ExpectedException(typeof(TemplateNotFoundException))]
    public async Task GetContentAsync_TemplateNotFound_ThrowsException()
    {
        // Arrange
        var mockRepository = new Mock<ITemplateRepository>();

        mockRepository
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TemplateEntity?)null);

        var source = new DatabaseTemplateContentSource(
            999,
            mockRepository.Object,
            NullLogger.Instance
        );

        // Act
        await source.GetContentAsync();  // Should throw
    }

    [TestMethod]
    public async Task GetContentStreamAsync_ReturnsUtf8Stream()
    {
        // Arrange
        var mockRepository = new Mock<ITemplateRepository>();
        var template = new TemplateEntity
        {
            Id = 1,
            Content = "Test Content"
        };

        mockRepository
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        var source = new DatabaseTemplateContentSource(
            1,
            mockRepository.Object,
            NullLogger.Instance
        );

        // Act
        await using var stream = await source.GetContentStreamAsync();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();

        // Assert
        Assert.AreEqual("Test Content", content);
    }
}
```

---

### 3. SqlServerTemplateRepository Tests (Unit)

**File:** `SqlServerTemplateRepositoryUnitTests.cs`

```csharp
[TestClass]
public class SqlServerTemplateRepositoryUnitTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_NullConnectionString_ThrowsException()
    {
        // Arrange
        var options = Options.Create(new DatabaseTemplateSourceOptions
        {
            ConnectionString = null!
        });

        // Act
        var repository = new SqlServerTemplateRepository(
            options,
            NullLogger<SqlServerTemplateRepository>.Instance
        );  // Should throw
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_EmptyConnectionString_ThrowsException()
    {
        // Arrange
        var options = Options.Create(new DatabaseTemplateSourceOptions
        {
            ConnectionString = ""
        });

        // Act
        var repository = new SqlServerTemplateRepository(
            options,
            NullLogger<SqlServerTemplateRepository>.Instance
        );  // Should throw
    }
}
```

---

## Integration Tests

### 1. SqlServerTemplateRepository Integration Tests

**File:** `SqlServerTemplateRepositoryIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class SqlServerTemplateRepositoryIntegrationTests
{
    private ITemplateRepository _repository;
    private string _connectionString;
    private Guid _testTenantId;

    [TestInitialize]
    public async Task Setup()
    {
        _connectionString = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");
        _testTenantId = Guid.NewGuid();

        var options = Options.Create(new DatabaseTemplateSourceOptions
        {
            ConnectionString = _connectionString
        });

        _repository = new SqlServerTemplateRepository(
            options,
            NullLogger<SqlServerTemplateRepository>.Instance
        );

        // Create test database schema
        await CreateSchemaAsync();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Clean up test data
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "DELETE FROM Templates WHERE TenantId = @TenantId",
            new { TenantId = _testTenantId }
        );
    }

    private async Task CreateSchemaAsync()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        var sql = @"
            IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Templates')
            BEGIN
                CREATE TABLE Templates (
                    Id INT PRIMARY KEY IDENTITY(1,1),
                    Name NVARCHAR(200) NOT NULL,
                    ContentType NVARCHAR(100) NOT NULL,
                    Content NVARCHAR(MAX) NOT NULL,
                    Version INT NOT NULL DEFAULT 1,
                    Culture NVARCHAR(10) NULL,
                    Category NVARCHAR(50) NULL,
                    TenantId UNIQUEIDENTIFIER NULL,
                    IsActive BIT NOT NULL DEFAULT 1,
                    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    CreatedBy NVARCHAR(100) NULL,
                    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
                    UpdatedBy NVARCHAR(100) NULL,
                    Description NVARCHAR(500) NULL,
                    Tags NVARCHAR(500) NULL
                );

                CREATE INDEX IX_Templates_Name_Culture_Tenant
                    ON Templates(Name, Culture, TenantId)
                    WHERE IsActive = 1;
            END";

        await connection.ExecuteAsync(sql);
    }

    [TestMethod]
    public async Task CreateAsync_ValidTemplate_CreatesSuccessfully()
    {
        // Arrange
        var template = new TemplateEntity
        {
            Name = "test-template",
            ContentType = "text/x-handlebars-template",
            Content = "Hello {{Name}}!",
            Version = 1,
            TenantId = _testTenantId,
            CreatedBy = "test@example.com"
        };

        // Act
        var created = await _repository.CreateAsync(template);

        // Assert
        Assert.IsTrue(created.Id > 0);
        Assert.AreEqual("test-template", created.Name);
    }

    [TestMethod]
    public async Task GetByIdAsync_ExistingTemplate_ReturnsTemplate()
    {
        // Arrange
        var template = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "test",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId
        });

        // Act
        var retrieved = await _repository.GetByIdAsync(template.Id);

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(template.Id, retrieved.Id);
        Assert.AreEqual("test", retrieved.Name);
    }

    [TestMethod]
    public async Task GetByNameAsync_ExistingTemplate_ReturnsTemplate()
    {
        // Arrange
        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "unique-template",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId
        });

        // Act
        var retrieved = await _repository.GetByNameAsync(
            "unique-template",
            _testTenantId
        );

        // Assert
        Assert.IsNotNull(retrieved);
        Assert.AreEqual("unique-template", retrieved.Name);
    }

    [TestMethod]
    public async Task GetActiveTemplatesAsync_MultipleTemplates_ReturnsOnlyActive()
    {
        // Arrange
        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "active1",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId,
            IsActive = true
        });

        var inactive = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "inactive",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId,
            IsActive = true
        });

        await _repository.DeactivateAsync(inactive.Id);

        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "active2",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId,
            IsActive = true
        });

        // Act
        var active = await _repository.GetActiveTemplatesAsync(_testTenantId);

        // Assert
        Assert.AreEqual(2, active.Count());
        Assert.IsFalse(active.Any(t => t.Name == "inactive"));
    }

    [TestMethod]
    public async Task CreateNewVersionAsync_ExistingTemplate_CreatesNewVersion()
    {
        // Arrange
        var v1 = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "versioned-template",
            ContentType = "text/plain",
            Content = "Version 1",
            Version = 1,
            TenantId = _testTenantId,
            CreatedBy = "user1@example.com"
        });

        // Act
        var v2 = await _repository.CreateNewVersionAsync(
            v1.Id,
            "Version 2",
            "user2@example.com"
        );

        // Assert
        Assert.IsTrue(v2.Id > 0);
        Assert.AreNotEqual(v1.Id, v2.Id);
        Assert.AreEqual("versioned-template", v2.Name);
        Assert.AreEqual(2, v2.Version);
        Assert.AreEqual("Version 2", v2.Content);
        Assert.AreEqual("user2@example.com", v2.CreatedBy);
    }

    [TestMethod]
    public async Task GetVersionHistoryAsync_MultipleVersions_ReturnsAllVersions()
    {
        // Arrange
        var v1 = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "history-test",
            ContentType = "text/plain",
            Content = "V1",
            Version = 1,
            TenantId = _testTenantId
        });

        var v2 = await _repository.CreateNewVersionAsync(v1.Id, "V2", "user");
        var v3 = await _repository.CreateNewVersionAsync(v2.Id, "V3", "user");

        // Act
        var history = await _repository.GetVersionHistoryAsync(
            "history-test",
            _testTenantId
        );

        // Assert
        var historyList = history.ToList();
        Assert.AreEqual(3, historyList.Count);
        Assert.AreEqual(3, historyList[0].Version);  // Newest first
        Assert.AreEqual(2, historyList[1].Version);
        Assert.AreEqual(1, historyList[2].Version);
    }

    [TestMethod]
    public async Task GetByCategoryAsync_MultipleCategories_ReturnsOnlyMatchingCategory()
    {
        // Arrange
        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "email1",
            ContentType = "text/plain",
            Content = "content",
            Category = "email",
            TenantId = _testTenantId
        });

        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "pdf1",
            ContentType = "text/plain",
            Content = "content",
            Category = "pdf",
            TenantId = _testTenantId
        });

        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "email2",
            ContentType = "text/plain",
            Content = "content",
            Category = "email",
            TenantId = _testTenantId
        });

        // Act
        var emailTemplates = await _repository.GetByCategoryAsync("email", _testTenantId);

        // Assert
        Assert.AreEqual(2, emailTemplates.Count());
        Assert.IsTrue(emailTemplates.All(t => t.Category == "email"));
    }

    [TestMethod]
    public async Task ActivateAsync_DeactivatedTemplate_Activates()
    {
        // Arrange
        var template = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "test",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId,
            IsActive = false
        });

        // Act
        var result = await _repository.ActivateAsync(template.Id);

        // Assert
        Assert.IsTrue(result);

        var retrieved = await _repository.GetByIdAsync(template.Id);
        Assert.IsTrue(retrieved.IsActive);
    }

    [TestMethod]
    public async Task DeactivateAsync_ActiveTemplate_Deactivates()
    {
        // Arrange
        var template = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "test",
            ContentType = "text/plain",
            Content = "content",
            TenantId = _testTenantId,
            IsActive = true
        });

        // Act
        var result = await _repository.DeactivateAsync(template.Id);

        // Assert
        Assert.IsTrue(result);

        var retrieved = await _repository.GetByIdAsync(template.Id);
        Assert.IsFalse(retrieved.IsActive);
    }

    [TestMethod]
    public async Task TenantIsolation_DifferentTenants_DoesNotReturnOtherTenantsData()
    {
        // Arrange
        var tenant1 = Guid.NewGuid();
        var tenant2 = Guid.NewGuid();

        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "template1",
            ContentType = "text/plain",
            Content = "content",
            TenantId = tenant1
        });

        await _repository.CreateAsync(new TemplateEntity
        {
            Name = "template2",
            ContentType = "text/plain",
            Content = "content",
            TenantId = tenant2
        });

        // Act
        var tenant1Templates = await _repository.GetActiveTemplatesAsync(tenant1);
        var tenant2Templates = await _repository.GetActiveTemplatesAsync(tenant2);

        // Assert
        Assert.AreEqual(1, tenant1Templates.Count());
        Assert.AreEqual(1, tenant2Templates.Count());
        Assert.AreEqual("template1", tenant1Templates.First().Name);
        Assert.AreEqual("template2", tenant2Templates.First().Name);
    }

    public TestContext TestContext { get; set; } = null!;
}
```

---

## Performance Tests

**File:** `PerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class DatabaseTemplateSourcePerformanceTests
{
    private ITemplateRepository _repository;
    private string _connectionString;
    private Guid _testTenantId;

    [TestInitialize]
    public async Task Setup()
    {
        _connectionString = TestContext.GetRequiredProperty<string>("SQLSERVER_CONNECTION_STRING");
        _testTenantId = Guid.NewGuid();

        var options = Options.Create(new DatabaseTemplateSourceOptions
        {
            ConnectionString = _connectionString
        });

        _repository = new SqlServerTemplateRepository(
            options,
            NullLogger<SqlServerTemplateRepository>.Instance
        );

        // Seed test data
        await SeedTestDataAsync();
    }

    private async Task SeedTestDataAsync()
    {
        for (int i = 1; i <= 100; i++)
        {
            await _repository.CreateAsync(new TemplateEntity
            {
                Name = $"template-{i}",
                ContentType = "text/x-handlebars-template",
                Content = $"Template {i} content with {{{{Data}}}}",
                Category = i % 2 == 0 ? "email" : "pdf",
                TenantId = _testTenantId,
                CreatedBy = "test@example.com"
            });
        }
    }

    [TestMethod]
    public async Task GetActiveTemplatesAsync_100Templates_CompletesFast()
    {
        // Act
        var stopwatch = Stopwatch.StartNew();
        var templates = await _repository.GetActiveTemplatesAsync(_testTenantId);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(100, templates.Count());
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);  // < 100ms
        Console.WriteLine($"GetActiveTemplatesAsync: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task GetByNameAsync_WithIndex_CompletesFast()
    {
        // Act
        var stopwatch = Stopwatch.StartNew();
        for (int i = 1; i <= 50; i++)
        {
            await _repository.GetByNameAsync($"template-{i}", _testTenantId);
        }
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500);  // < 10ms per query
        Console.WriteLine($"50 GetByNameAsync calls: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ContentCaching_MultipleAccesses_UsesCacheEffectively()
    {
        // Arrange
        var template = await _repository.CreateAsync(new TemplateEntity
        {
            Name = "cached-template",
            ContentType = "text/plain",
            Content = new string('X', 10000),  // 10KB content
            TenantId = _testTenantId
        });

        var source = new DatabaseTemplateContentSource(
            template.Id,
            _repository,
            NullLogger.Instance
        );

        // Act - First load (uncached)
        var stopwatch1 = Stopwatch.StartNew();
        var content1 = await source.GetContentAsync();
        stopwatch1.Stop();

        // Act - Second load (cached)
        var stopwatch2 = Stopwatch.StartNew();
        var content2 = await source.GetContentAsync();
        stopwatch2.Stop();

        // Assert
        Assert.AreEqual(content1, content2);
        Assert.IsTrue(stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds / 10);
        Console.WriteLine($"Uncached: {stopwatch1.ElapsedMilliseconds}ms, Cached: {stopwatch2.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task GetByCategoryAsync_LargeDataset_CompletesFast()
    {
        // Act
        var stopwatch = Stopwatch.StartNew();
        var emailTemplates = await _repository.GetByCategoryAsync("email", _testTenantId);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(50, emailTemplates.Count());
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100);
        Console.WriteLine($"GetByCategoryAsync: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();

        await connection.ExecuteAsync(
            "DELETE FROM Templates WHERE TenantId = @TenantId",
            new { TenantId = _testTenantId }
        );
    }

    public TestContext TestContext { get; set; } = null!;
}
```

---

## Test Data Builders

**File:** `TestDataBuilders.cs`

```csharp
public static class TemplateTestDataBuilders
{
    public static TemplateEntity BuildBasicTemplate(
        string name = "test-template",
        Guid? tenantId = null)
    {
        return new TemplateEntity
        {
            Name = name,
            ContentType = "text/x-handlebars-template",
            Content = "Hello {{Name}}!",
            Version = 1,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "test@example.com",
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = "test@example.com"
        };
    }

    public static TemplateEntity BuildEmailTemplate(
        string name,
        Guid? tenantId = null)
    {
        return new TemplateEntity
        {
            Name = name,
            ContentType = "text/x-handlebars-template",
            Content = @"
                <html>
                <body>
                    <h1>{{Subject}}</h1>
                    <p>{{Body}}</p>
                </body>
                </html>",
            Category = "email",
            Version = 1,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin@example.com"
        };
    }

    public static TemplateEntity BuildMultiCultureTemplate(
        string name,
        string culture,
        string content,
        Guid? tenantId = null)
    {
        return new TemplateEntity
        {
            Name = name,
            ContentType = "text/x-handlebars-template",
            Content = content,
            Culture = culture,
            Version = 1,
            TenantId = tenantId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "admin@example.com"
        };
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| DatabaseTemplateSource | 85% | GetTemplates, GetTemplate, Error handling |
| SqlServerTemplateRepository | 80% | CRUD operations, Versioning, Tenant isolation |
| DatabaseTemplateContentSource | 90% | GetContentAsync, Caching, Error handling |
| Configuration | 70% | Options validation, DI registration |

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit&FullyQualifiedName~DatabaseTemplateSource"
```

**Run on SQL Server integration test schedule:**
```bash
# Integration tests (requires SQL Server)
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~DatabaseTemplateSource"
```

**Run nightly:**
```bash
# Performance benchmarks
dotnet test --filter "TestCategory=DevLocal&FullyQualifiedName~DatabaseTemplateSource.Performance"
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 10 Overview](../README-REVISED.md)
