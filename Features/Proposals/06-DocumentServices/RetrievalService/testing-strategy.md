# Document Retrieval Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Retrieval Service
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (50+ tests)
- **Integration Tests** - End-to-end with real storage providers (25+ tests)
- **Performance Tests** - Benchmark retrieval speed and optimization (10+ tests)
- **Concurrency Tests** - Thread-safety verification (5+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (10 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (25 tests)
                  │                   │
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (50+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. DocumentRetrievalService Tests

**File:** `DocumentRetrievalServiceTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Documents.Retrieval;

namespace OoBDev.System.Documents.Retrieval.Tests;

[TestClass]
public class DocumentRetrievalServiceTests
{
    private Mock<IDocumentRetrievalProviderFactory> _mockProviderFactory;
    private Mock<IDocumentRetrievalProvider> _mockProvider;
    private DocumentRetrievalService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockProviderFactory = new Mock<IDocumentRetrievalProviderFactory>();
        _mockProvider = new Mock<IDocumentRetrievalProvider>();
        _mockProvider.Setup(p => p.ProviderName).Returns("test-provider");

        _mockProviderFactory
            .Setup(f => f.GetProviderAsync(It.IsAny<string>()))
            .ReturnsAsync(_mockProvider.Object);

        var options = new DocumentRetrievalOptions
        {
            DefaultProvider = "test-provider"
        };

        _service = new DocumentRetrievalService(_mockProviderFactory.Object, options, Mock.Of<ILogger<DocumentRetrievalService>>());
    }

    [TestMethod]
    public async Task GetAsync_ValidId_ReturnsDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var expectedDocument = new Document
        {
            Id = documentId,
            Name = "test.pdf",
            MediaType = "application/pdf",
            Content = new byte[] { 1, 2, 3 }
        };

        _mockProvider
            .Setup(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(expectedDocument);

        // Act
        var result = await _service.GetAsync(documentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(documentId, result.Id);
        Assert.AreEqual("test.pdf", result.Name);
        _mockProvider.Verify(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(DocumentNotFoundException))]
    public async Task GetAsync_DocumentNotFound_ThrowsException()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockProvider
            .Setup(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()))
            .ThrowsAsync(new DocumentNotFoundException(documentId));

        // Act
        await _service.GetAsync(documentId);  // Should throw
    }

    [TestMethod]
    public async Task GetAsync_WithContext_PassesContextToProvider()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var context = new RetrievalContext
        {
            RequestingApplication = "test-app",
            UserId = "user123",
            IncludeContent = false
        };

        _mockProvider
            .Setup(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(new Document { Id = documentId });

        // Act
        await _service.GetAsync(documentId, context);

        // Assert
        _mockProvider.Verify(p => p.GetAsync(
            documentId,
            It.Is<RetrievalContext>(ctx =>
                ctx.RequestingApplication == "test-app" &&
                ctx.UserId == "user123" &&
                ctx.IncludeContent == false)),
            Times.Once);
    }

    [TestMethod]
    public async Task GetAsync_ProviderFails_TriesFallbackProvider()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var mockFallbackProvider = new Mock<IDocumentRetrievalProvider>();
        mockFallbackProvider.Setup(p => p.ProviderName).Returns("fallback-provider");
        mockFallbackProvider
            .Setup(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(new Document { Id = documentId });

        _mockProvider
            .Setup(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()))
            .ThrowsAsync(new Exception("Provider failed"));

        _mockProviderFactory
            .Setup(f => f.GetProviderAsync("fallback-provider"))
            .ReturnsAsync(mockFallbackProvider.Object);

        var options = new DocumentRetrievalOptions
        {
            DefaultProvider = "test-provider",
            FallbackProvider = "fallback-provider"
        };

        var service = new DocumentRetrievalService(_mockProviderFactory.Object, options, Mock.Of<ILogger<DocumentRetrievalService>>());

        // Act
        var result = await service.GetAsync(documentId);

        // Assert
        Assert.IsNotNull(result);
        _mockProvider.Verify(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()), Times.Once);
        mockFallbackProvider.Verify(p => p.GetAsync(documentId, It.IsAny<RetrievalContext>()), Times.Once);
    }

    [TestMethod]
    public async Task ExistsAsync_DocumentExists_ReturnsTrue()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockProvider
            .Setup(p => p.ExistsAsync(documentId, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(true);

        // Act
        var exists = await _service.ExistsAsync(documentId);

        // Assert
        Assert.IsTrue(exists);
    }

    [TestMethod]
    public async Task ExistsAsync_DocumentNotExists_ReturnsFalse()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        _mockProvider
            .Setup(p => p.ExistsAsync(documentId, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(false);

        // Act
        var exists = await _service.ExistsAsync(documentId);

        // Assert
        Assert.IsFalse(exists);
    }

    [TestMethod]
    public async Task QueryAsync_ValidQuery_ReturnsDocuments()
    {
        // Arrange
        var query = new DocumentQuery
        {
            MediaType = "application/pdf",
            PageSize = 10
        };

        var expectedDocuments = new[]
        {
            new Document { Id = Guid.NewGuid(), Name = "doc1.pdf" },
            new Document { Id = Guid.NewGuid(), Name = "doc2.pdf" }
        };

        _mockProvider
            .Setup(p => p.QueryAsync(query, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(expectedDocuments);

        // Act
        var results = await _service.QueryAsync(query);

        // Assert
        Assert.AreEqual(2, results.Count());
        Assert.AreEqual("doc1.pdf", results.First().Name);
    }

    [TestMethod]
    public async Task GetVersionAsync_ValidVersion_ReturnsVersionedDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var version = 3;
        var expectedDocument = new Document
        {
            Id = documentId,
            Version = version,
            Name = "test-v3.pdf"
        };

        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsVersioning = true
        });

        _mockProvider
            .Setup(p => p.GetVersionAsync(documentId, version, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(expectedDocument);

        // Act
        var result = await _service.GetVersionAsync(documentId, version);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(version, result.Version);
    }

    [TestMethod]
    [ExpectedException(typeof(VersionNotSupportedException))]
    public async Task GetVersionAsync_ProviderDoesNotSupportVersioning_ThrowsException()
    {
        // Arrange
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsVersioning = false
        });

        // Act
        await _service.GetVersionAsync(Guid.NewGuid(), 2);  // Should throw
    }

    [TestMethod]
    public async Task GetVersionsAsync_MultipleVersions_ReturnsAllVersions()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var expectedVersions = new[]
        {
            new DocumentVersion { DocumentId = documentId, Version = 1 },
            new DocumentVersion { DocumentId = documentId, Version = 2 },
            new DocumentVersion { DocumentId = documentId, Version = 3 }
        };

        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsVersioning = true
        });

        _mockProvider
            .Setup(p => p.GetVersionsAsync(documentId, It.IsAny<RetrievalContext>()))
            .ReturnsAsync(expectedVersions);

        // Act
        var versions = await _service.GetVersionsAsync(documentId);

        // Assert
        Assert.AreEqual(3, versions.Count());
    }
}
```

---

#### 2. Context Tests

**File:** `RetrievalContextTests.cs`

```csharp
[TestClass]
public class RetrievalContextTests
{
    [TestMethod]
    public void Constructor_DefaultValues_SetsCorrectDefaults()
    {
        // Act
        var context = new RetrievalContext();

        // Assert
        Assert.IsTrue(context.IncludeMetadata);
        Assert.IsTrue(context.IncludeContent);
        Assert.IsNotNull(context.AdditionalContext);
        Assert.AreEqual(0, context.AdditionalContext.Count);
    }

    [TestMethod]
    public void AdditionalContext_CustomProperties_CanBeAdded()
    {
        // Arrange
        var context = new RetrievalContext();

        // Act
        context.AdditionalContext["PreferredProvider"] = "azure-blob";
        context.AdditionalContext["CacheResults"] = true;
        context.AdditionalContext["Timeout"] = 30;

        // Assert
        Assert.AreEqual("azure-blob", context.AdditionalContext["PreferredProvider"]);
        Assert.AreEqual(true, context.AdditionalContext["CacheResults"]);
        Assert.AreEqual(30, context.AdditionalContext["Timeout"]);
    }
}
```

---

#### 3. Query Builder Tests

**File:** `DocumentQueryTests.cs`

```csharp
[TestClass]
public class DocumentQueryTests
{
    [TestMethod]
    public void Constructor_DefaultValues_SetsCorrectDefaults()
    {
        // Act
        var query = new DocumentQuery();

        // Assert
        Assert.AreEqual(100, query.PageSize);
        Assert.AreEqual(1, query.PageNumber);
        Assert.IsTrue(query.SortAscending);
        Assert.IsNotNull(query.CustomFilters);
    }

    [TestMethod]
    public void Query_MediaTypeFilter_SetsCorrectly()
    {
        // Arrange & Act
        var query = new DocumentQuery
        {
            MediaType = "application/pdf"
        };

        // Assert
        Assert.AreEqual("application/pdf", query.MediaType);
    }

    [TestMethod]
    public void Query_DateRangeFilter_SetsCorrectly()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-7);
        var endDate = DateTime.UtcNow;

        // Act
        var query = new DocumentQuery
        {
            CreatedAfter = startDate,
            CreatedBefore = endDate
        };

        // Assert
        Assert.AreEqual(startDate, query.CreatedAfter);
        Assert.AreEqual(endDate, query.CreatedBefore);
    }

    [TestMethod]
    public void Query_TagsFilter_SetsMultipleTags()
    {
        // Arrange & Act
        var query = new DocumentQuery
        {
            Tags = new[] { "invoice", "2024", "paid" }
        };

        // Assert
        Assert.AreEqual(3, query.Tags.Length);
        Assert.IsTrue(query.Tags.Contains("invoice"));
    }

    [TestMethod]
    public void Query_Pagination_SetsPageSizeAndNumber()
    {
        // Arrange & Act
        var query = new DocumentQuery
        {
            PageSize = 50,
            PageNumber = 3
        };

        // Assert
        Assert.AreEqual(50, query.PageSize);
        Assert.AreEqual(3, query.PageNumber);
    }

    [TestMethod]
    public void Query_CustomFilters_CanBeAdded()
    {
        // Arrange
        var query = new DocumentQuery();

        // Act
        query.CustomFilters["MinSize"] = 1024;
        query.CustomFilters["MaxSize"] = 1048576;
        query.CustomFilters["ContentContains"] = "invoice";

        // Assert
        Assert.AreEqual(1024, query.CustomFilters["MinSize"]);
        Assert.AreEqual(1048576, query.CustomFilters["MaxSize"]);
        Assert.AreEqual("invoice", query.CustomFilters["ContentContains"]);
    }
}
```

---

## Integration Tests

### Test Scenarios

#### 1. Database Provider Integration

**File:** `DatabaseRetrievalProviderIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class DatabaseRetrievalProviderIntegrationTests
{
    private IDbConnection _connection;
    private DatabaseRetrievalProvider _provider;
    private readonly TestContext _testContext;

    [TestInitialize]
    public async Task Setup()
    {
        var connectionString = _testContext.GetRequiredProperty<string>("SQL_SERVER_CONNECTION_STRING");
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();

        _provider = new DatabaseRetrievalProvider(_connection, Mock.Of<ILogger<DatabaseRetrievalProvider>>());

        // Setup test schema
        await CreateTestSchema();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await DropTestSchema();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task GetAsync_DocumentInDatabase_RetrievesSuccessfully()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocument(documentId, "test.pdf", new byte[] { 1, 2, 3 });

        var context = new RetrievalContext
        {
            RequestingApplication = "integration-test",
            IncludeContent = true
        };

        // Act
        var document = await _provider.GetAsync(documentId, context);

        // Assert
        Assert.IsNotNull(document);
        Assert.AreEqual(documentId, document.Id);
        Assert.AreEqual("test.pdf", document.Name);
        Assert.IsNotNull(document.Content);
        Assert.AreEqual(3, document.Content.Length);
    }

    [TestMethod]
    public async Task GetAsync_MetadataOnly_DoesNotLoadContent()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var largeContent = new byte[1024 * 1024];  // 1MB
        await InsertTestDocument(documentId, "large.pdf", largeContent);

        var context = new RetrievalContext
        {
            IncludeContent = false  // Metadata only
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var document = await _provider.GetAsync(documentId, context);
        stopwatch.Stop();

        // Assert
        Assert.IsNotNull(document);
        Assert.IsNull(document.Content);  // Content not loaded
        Assert.AreEqual(largeContent.Length, document.Size);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 50, $"Metadata retrieval took {stopwatch.ElapsedMilliseconds}ms (should be < 50ms)");
    }

    [TestMethod]
    public async Task QueryAsync_WithFilters_ReturnsMatchingDocuments()
    {
        // Arrange
        await InsertTestDocument(Guid.NewGuid(), "invoice1.pdf", new byte[] { 1 }, tags: new[] { "invoice", "2024" });
        await InsertTestDocument(Guid.NewGuid(), "invoice2.pdf", new byte[] { 2 }, tags: new[] { "invoice", "2024" });
        await InsertTestDocument(Guid.NewGuid(), "report.pdf", new byte[] { 3 }, tags: new[] { "report" });

        var query = new DocumentQuery
        {
            Tags = new[] { "invoice", "2024" },
            PageSize = 10
        };

        var context = new RetrievalContext
        {
            IncludeContent = false
        };

        // Act
        var results = await _provider.QueryAsync(query, context);

        // Assert
        Assert.AreEqual(2, results.Count());
        Assert.IsTrue(results.All(d => d.Name.StartsWith("invoice")));
    }

    [TestMethod]
    public async Task GetVersionAsync_DocumentWithVersions_RetrievesSpecificVersion()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocumentWithVersions(documentId, 3);

        var context = new RetrievalContext();

        // Act
        var version2 = await _provider.GetVersionAsync(documentId, 2, context);

        // Assert
        Assert.IsNotNull(version2);
        Assert.AreEqual(2, version2.Version);
    }

    [TestMethod]
    public async Task GetVersionsAsync_DocumentWithMultipleVersions_ReturnsAllVersions()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocumentWithVersions(documentId, 5);

        var context = new RetrievalContext();

        // Act
        var versions = await _provider.GetVersionsAsync(documentId, context);

        // Assert
        Assert.AreEqual(5, versions.Count());
        Assert.IsTrue(versions.Any(v => v.Version == 1));
        Assert.IsTrue(versions.Any(v => v.Version == 5));
    }

    private async Task InsertTestDocument(Guid id, string name, byte[] content, string[]? tags = null)
    {
        var sql = @"
            INSERT INTO Documents (Id, Name, MediaType, Content, Size, CreatedDate, ModifiedDate, Version, Tags)
            VALUES (@Id, @Name, @MediaType, @Content, @Size, @CreatedDate, @ModifiedDate, @Version, @Tags)";

        await _connection.ExecuteAsync(sql, new
        {
            Id = id,
            Name = name,
            MediaType = "application/pdf",
            Content = content,
            Size = content.Length,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Version = 1,
            Tags = tags != null ? string.Join(",", tags) : null
        });
    }
}
```

---

## Performance Tests

**File:** `RetrievalPerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class RetrievalPerformanceTests
{
    [TestMethod]
    public async Task MetadataOnly_vs_FullRetrieval_PerformanceComparison()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var largeContent = new byte[10 * 1024 * 1024];  // 10MB
        await InsertTestDocument(documentId, largeContent);

        // Act - Metadata only
        var metadataContext = new RetrievalContext { IncludeContent = false };
        var stopwatch = Stopwatch.StartNew();
        await _provider.GetAsync(documentId, metadataContext);
        var metadataTime = stopwatch.ElapsedMilliseconds;

        // Act - Full retrieval
        var fullContext = new RetrievalContext { IncludeContent = true };
        stopwatch.Restart();
        await _provider.GetAsync(documentId, fullContext);
        var fullTime = stopwatch.ElapsedMilliseconds;

        // Assert
        Assert.IsTrue(metadataTime < 100, $"Metadata retrieval took {metadataTime}ms (should be < 100ms)");
        Assert.IsTrue(fullTime > metadataTime * 10, $"Full retrieval should be at least 10x slower (metadata: {metadataTime}ms, full: {fullTime}ms)");

        Console.WriteLine($"Performance comparison:");
        Console.WriteLine($"  Metadata-only: {metadataTime}ms");
        Console.WriteLine($"  Full retrieval: {fullTime}ms");
        Console.WriteLine($"  Speedup: {fullTime / (double)metadataTime:F1}x");
    }

    [TestMethod]
    public async Task ConcurrentRetrieval_MultipleDocuments_PerformanceTest()
    {
        // Arrange
        var documentIds = Enumerable.Range(0, 100).Select(_ => Guid.NewGuid()).ToArray();
        foreach (var id in documentIds)
        {
            await InsertTestDocument(id, new byte[1024]);  // 1KB each
        }

        var context = new RetrievalContext { IncludeContent = false };

        // Act - Sequential
        var stopwatch = Stopwatch.StartNew();
        foreach (var id in documentIds)
        {
            await _provider.GetAsync(id, context);
        }
        var sequentialTime = stopwatch.ElapsedMilliseconds;

        // Act - Concurrent
        stopwatch.Restart();
        var tasks = documentIds.Select(id => _provider.GetAsync(id, context));
        await Task.WhenAll(tasks);
        var concurrentTime = stopwatch.ElapsedMilliseconds;

        // Assert
        Assert.IsTrue(concurrentTime < sequentialTime / 2,
            $"Concurrent should be at least 2x faster (sequential: {sequentialTime}ms, concurrent: {concurrentTime}ms)");

        Console.WriteLine($"Concurrent retrieval (100 documents):");
        Console.WriteLine($"  Sequential: {sequentialTime}ms");
        Console.WriteLine($"  Concurrent: {concurrentTime}ms");
        Console.WriteLine($"  Speedup: {sequentialTime / (double)concurrentTime:F1}x");
    }

    [TestMethod]
    public async Task QueryPagination_LargeResultSet_PerformanceTest()
    {
        // Arrange - Insert 1000 documents
        for (int i = 0; i < 1000; i++)
        {
            await InsertTestDocument(Guid.NewGuid(), new byte[100]);
        }

        var query = new DocumentQuery
        {
            PageSize = 100,
            PageNumber = 1
        };

        var context = new RetrievalContext { IncludeContent = false };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var results = await _provider.QueryAsync(query, context);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(100, results.Count());
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Query with pagination took {stopwatch.ElapsedMilliseconds}ms (should be < 500ms)");

        Console.WriteLine($"Query performance (1000 documents, page size 100):");
        Console.WriteLine($"  Query time: {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine($"  Documents per second: {results.Count() / (stopwatch.ElapsedMilliseconds / 1000.0):F0}");
    }
}
```

---

## Concurrency Tests

**File:** `RetrievalConcurrencyTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class RetrievalConcurrencyTests
{
    [TestMethod]
    public async Task ConcurrentGetAsync_SameDocument_ThreadSafe()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocument(documentId, new byte[] { 1, 2, 3 });

        var context = new RetrievalContext();

        // Act - 50 threads retrieve same document concurrently
        var tasks = Enumerable.Range(0, 50).Select(_ =>
            _provider.GetAsync(documentId, context)
        );

        var results = await Task.WhenAll(tasks);

        // Assert - All threads got same document
        Assert.AreEqual(50, results.Length);
        Assert.IsTrue(results.All(r => r.Id == documentId));
        Assert.IsTrue(results.All(r => r.Name == results[0].Name));
    }

    [TestMethod]
    public async Task ConcurrentQueryAsync_DifferentQueries_ThreadSafe()
    {
        // Arrange
        await SeedTestDocuments(100);

        // Act - 20 threads querying concurrently
        var tasks = Enumerable.Range(0, 20).Select(i =>
            _provider.QueryAsync(new DocumentQuery
            {
                PageSize = 10,
                PageNumber = i % 5 + 1
            }, new RetrievalContext())
        );

        var results = await Task.WhenAll(tasks);

        // Assert - All queries succeeded
        Assert.AreEqual(20, results.Length);
        Assert.IsTrue(results.All(r => r.Any()));
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| DocumentRetrievalService | 90% | GetAsync, QueryAsync, Provider selection |
| RetrievalContext | 95% | Property setters, validation |
| DocumentQuery | 90% | Query building, filters |
| Provider Implementations | 85% | GetAsync, QueryAsync, versioning |
| Error Handling | 80% | Exceptions, fallback logic |

**Overall Target:** 85%+ code coverage

---

## Test Data Builders

```csharp
public static class TestDataBuilders
{
    public static Document BuildTestDocument(Guid? id = null, string? name = null, byte[]? content = null)
    {
        return new Document
        {
            Id = id ?? Guid.NewGuid(),
            Name = name ?? "test-document.pdf",
            MediaType = "application/pdf",
            Content = content ?? new byte[] { 1, 2, 3, 4, 5 },
            Size = content?.Length ?? 5,
            CreatedDate = DateTime.UtcNow,
            ModifiedDate = DateTime.UtcNow,
            Version = 1,
            Tags = new[] { "test" }
        };
    }

    public static DocumentQuery BuildTestQuery(string? mediaType = null, int? pageSize = null)
    {
        return new DocumentQuery
        {
            MediaType = mediaType ?? "application/pdf",
            PageSize = pageSize ?? 100,
            PageNumber = 1
        };
    }

    public static RetrievalContext BuildTestContext(string? application = null, bool includeContent = true)
    {
        return new RetrievalContext
        {
            RequestingApplication = application ?? "test-app",
            UserId = "test-user",
            IncludeMetadata = true,
            IncludeContent = includeContent
        };
    }
}
```

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit"

# Integration tests with Docker
dotnet test --filter "TestCategory=Integration"
```

**Run nightly:**
```bash
# Performance benchmarks
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~Performance"

# Concurrency stress tests
dotnet test --filter "TestCategory=Integration&FullyQualifiedName~Concurrency"
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
