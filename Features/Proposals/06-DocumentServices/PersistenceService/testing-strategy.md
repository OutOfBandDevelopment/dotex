# Document Persistence Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Persistence Service
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (55+ tests)
- **Integration Tests** - End-to-end with real storage providers (30+ tests)
- **Performance Tests** - Benchmark persistence speed and optimization (12+ tests)
- **Concurrency Tests** - Thread-safety and race condition verification (6+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (12 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (30 tests)
                  │                   │
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (55+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. DocumentPersistenceService Tests

**File:** `DocumentPersistenceServiceTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Documents.Persistence;

namespace OoBDev.System.Documents.Persistence.Tests;

[TestClass]
public class DocumentPersistenceServiceTests
{
    private Mock<IDocumentPersistenceProviderFactory> _mockProviderFactory;
    private Mock<IDocumentPersistenceProvider> _mockProvider;
    private Mock<IContentDeduplicationService> _mockDeduplication;
    private DocumentPersistenceService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockProviderFactory = new Mock<IDocumentPersistenceProviderFactory>();
        _mockProvider = new Mock<IDocumentPersistenceProvider>();
        _mockDeduplication = new Mock<IContentDeduplicationService>();

        _mockProvider.Setup(p => p.ProviderName).Returns("test-provider");
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsVersioning = true,
            SupportsDeduplication = true,
            SupportsSoftDelete = true
        });

        _mockProviderFactory
            .Setup(f => f.GetProviderAsync(It.IsAny<string>()))
            .ReturnsAsync(_mockProvider.Object);

        var options = new DocumentPersistenceOptions
        {
            DefaultProvider = "test-provider"
        };

        _service = new DocumentPersistenceService(
            _mockProviderFactory.Object,
            _mockDeduplication.Object,
            options,
            Mock.Of<ILogger<DocumentPersistenceService>>());
    }

    [TestMethod]
    public async Task CreateAsync_ValidRequest_ReturnsDocumentId()
    {
        // Arrange
        var request = new DocumentCreateRequest
        {
            Name = "test.pdf",
            MediaType = "application/pdf",
            Content = new byte[] { 1, 2, 3 }
        };

        _mockProvider
            .Setup(p => p.CreateAsync(It.IsAny<Guid>(), request, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        var documentId = await _service.CreateAsync(request);

        // Assert
        Assert.IsNotNull(documentId);
        Assert.AreNotEqual(Guid.Empty, documentId);
        _mockProvider.Verify(p => p.CreateAsync(It.IsAny<Guid>(), request, It.IsAny<PersistenceContext>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_WithContext_PassesContextToProvider()
    {
        // Arrange
        var request = new DocumentCreateRequest
        {
            Name = "test.pdf",
            Content = new byte[] { 1, 2, 3 }
        };

        var context = new PersistenceContext
        {
            RequestingApplication = "test-app",
            UserId = "user123",
            EnableVersioning = true
        };

        _mockProvider
            .Setup(p => p.CreateAsync(It.IsAny<Guid>(), request, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(request, context);

        // Assert
        _mockProvider.Verify(p => p.CreateAsync(
            It.IsAny<Guid>(),
            request,
            It.Is<PersistenceContext>(ctx =>
                ctx.RequestingApplication == "test-app" &&
                ctx.UserId == "user123" &&
                ctx.EnableVersioning == true)),
            Times.Once);
    }

    [TestMethod]
    public async Task CreateAsync_WithDeduplication_ChecksForExistingContent()
    {
        // Arrange
        var request = new DocumentCreateRequest
        {
            Name = "test.pdf",
            Content = new byte[] { 1, 2, 3 }
        };

        var context = new PersistenceContext
        {
            EnableDeduplication = true
        };

        var existingContentId = Guid.NewGuid();
        _mockDeduplication
            .Setup(d => d.ComputeHashAsync(request.Content))
            .ReturnsAsync("hash123");
        _mockDeduplication
            .Setup(d => d.FindContentByHashAsync("hash123"))
            .ReturnsAsync(existingContentId);

        _mockProvider
            .Setup(p => p.CreateAsync(It.IsAny<Guid>(), request, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CreateAsync(request, context);

        // Assert
        _mockDeduplication.Verify(d => d.ComputeHashAsync(request.Content), Times.Once);
        _mockDeduplication.Verify(d => d.FindContentByHashAsync("hash123"), Times.Once);
        _mockProvider.Verify(p => p.CreateAsync(
            It.IsAny<Guid>(),
            request,
            It.Is<PersistenceContext>(ctx =>
                ctx.AdditionalContext.ContainsKey("ContentHash") &&
                ctx.AdditionalContext.ContainsKey("ExistingContentId"))),
            Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(DocumentTooLargeException))]
    public async Task CreateAsync_ContentTooLarge_ThrowsException()
    {
        // Arrange
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            MaxDocumentSize = 100
        });

        var request = new DocumentCreateRequest
        {
            Name = "large.pdf",
            Content = new byte[200]  // Exceeds limit
        };

        // Act
        await _service.CreateAsync(request);  // Should throw
    }

    [TestMethod]
    public async Task UpdateAsync_ValidRequest_UpdatesDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var request = new DocumentUpdateRequest
        {
            Name = "updated.pdf",
            Metadata = new Dictionary<string, object> { ["Status"] = "Updated" }
        };

        _mockProvider
            .Setup(p => p.UpdateAsync(documentId, request, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(documentId, request);

        // Assert
        _mockProvider.Verify(p => p.UpdateAsync(documentId, request, It.IsAny<PersistenceContext>()), Times.Once);
    }

    [TestMethod]
    public async Task UpdateAsync_WithVersioning_CreatesNewVersion()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var request = new DocumentUpdateRequest
        {
            Content = new byte[] { 4, 5, 6 }
        };

        var context = new PersistenceContext
        {
            EnableVersioning = true,
            ChangeDescription = "Updated content"
        };

        _mockProvider
            .Setup(p => p.UpdateAsync(documentId, request, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.UpdateAsync(documentId, request, context);

        // Assert
        _mockProvider.Verify(p => p.UpdateAsync(
            documentId,
            request,
            It.Is<PersistenceContext>(ctx =>
                ctx.EnableVersioning == true &&
                ctx.ChangeDescription == "Updated content" &&
                ctx.AdditionalContext.ContainsKey("CreateVersion"))),
            Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(VersioningNotSupportedException))]
    public async Task UpdateAsync_VersioningNotSupported_ThrowsException()
    {
        // Arrange
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsVersioning = false
        });

        var context = new PersistenceContext
        {
            EnableVersioning = true
        };

        // Act
        await _service.UpdateAsync(Guid.NewGuid(), new DocumentUpdateRequest(), context);  // Should throw
    }

    [TestMethod]
    public async Task DeleteAsync_SoftDelete_CallsProviderDelete()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var context = new PersistenceContext
        {
            SoftDelete = true  // Default
        };

        _mockProvider
            .Setup(p => p.DeleteAsync(documentId, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(documentId, context);

        // Assert
        _mockProvider.Verify(p => p.DeleteAsync(documentId, It.Is<PersistenceContext>(ctx => ctx.SoftDelete)), Times.Once);
    }

    [TestMethod]
    public async Task DeleteAsync_HardDelete_DeletesContentIfNoReferences()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var context = new PersistenceContext
        {
            SoftDelete = false,
            EnableDeduplication = true
        };

        _mockDeduplication
            .Setup(d => d.GetContentHashAsync(documentId))
            .ReturnsAsync("hash123");
        _mockDeduplication
            .Setup(d => d.DecrementReferenceAsync("hash123"))
            .ReturnsAsync(0);  // No more references

        _mockProvider
            .Setup(p => p.DeleteAsync(documentId, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteAsync(documentId, context);

        // Assert
        _mockDeduplication.Verify(d => d.DecrementReferenceAsync("hash123"), Times.Once);
        _mockProvider.Verify(p => p.DeleteAsync(
            documentId,
            It.Is<PersistenceContext>(ctx =>
                ctx.AdditionalContext.ContainsKey("DeleteContent") &&
                (bool)ctx.AdditionalContext["DeleteContent"] == true)),
            Times.Once);
    }

    [TestMethod]
    public async Task RestoreAsync_ValidDocument_RestoresDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var context = new PersistenceContext();

        _mockProvider
            .Setup(p => p.RestoreAsync(documentId, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RestoreAsync(documentId, context);

        // Assert
        _mockProvider.Verify(p => p.RestoreAsync(documentId, It.IsAny<PersistenceContext>()), Times.Once);
    }

    [TestMethod]
    public async Task CreateBatchAsync_MultipleDocuments_CreatesAll()
    {
        // Arrange
        var requests = new[]
        {
            new DocumentCreateRequest { Name = "doc1.pdf", Content = new byte[] { 1 } },
            new DocumentCreateRequest { Name = "doc2.pdf", Content = new byte[] { 2 } },
            new DocumentCreateRequest { Name = "doc3.pdf", Content = new byte[] { 3 } }
        };

        var expectedIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        _mockProvider
            .Setup(p => p.CreateBatchAsync(It.IsAny<IEnumerable<(Guid, DocumentCreateRequest)>>(), It.IsAny<PersistenceContext>()))
            .ReturnsAsync(expectedIds);

        // Act
        var documentIds = await _service.CreateBatchAsync(requests);

        // Assert
        Assert.AreEqual(3, documentIds.Count());
        _mockProvider.Verify(p => p.CreateBatchAsync(It.IsAny<IEnumerable<(Guid, DocumentCreateRequest)>>(), It.IsAny<PersistenceContext>()), Times.Once);
    }

    [TestMethod]
    public async Task ChangeStorageTierAsync_ValidTier_ChangesT tier()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var context = new PersistenceContext();

        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsStorageTiers = true,
            SupportedTiers = new[] { StorageTier.Hot, StorageTier.Cool, StorageTier.Archive }
        });

        _mockProvider
            .Setup(p => p.ChangeStorageTierAsync(documentId, StorageTier.Archive, It.IsAny<PersistenceContext>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.ChangeStorageTierAsync(documentId, StorageTier.Archive, context);

        // Assert
        _mockProvider.Verify(p => p.ChangeStorageTierAsync(documentId, StorageTier.Archive, It.IsAny<PersistenceContext>()), Times.Once);
    }

    [TestMethod]
    [ExpectedException(typeof(NotSupportedException))]
    public async Task ChangeStorageTierAsync_TierNotSupported_ThrowsException()
    {
        // Arrange
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentStorageCapabilities
        {
            SupportsStorageTiers = false
        });

        // Act
        await _service.ChangeStorageTierAsync(Guid.NewGuid(), StorageTier.Archive);  // Should throw
    }

    [TestMethod]
    public async Task PurgeDeletedAsync_OldDocuments_PurgesDocuments()
    {
        // Arrange
        var olderThan = TimeSpan.FromDays(30);
        var context = new PersistenceContext();

        _mockProvider
            .Setup(p => p.PurgeDeletedAsync(olderThan, It.IsAny<PersistenceContext>()))
            .ReturnsAsync(42);  // Purged 42 documents

        // Act
        var purgedCount = await _service.PurgeDeletedAsync(olderThan, context);

        // Assert
        Assert.AreEqual(42, purgedCount);
        _mockProvider.Verify(p => p.PurgeDeletedAsync(olderThan, It.IsAny<PersistenceContext>()), Times.Once);
    }
}
```

---

#### 2. Content Deduplication Tests

**File:** `ContentDeduplicationServiceTests.cs`

```csharp
[TestClass]
public class ContentDeduplicationServiceTests
{
    private Mock<IContentHashRepository> _mockRepository;
    private ContentDeduplicationService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockRepository = new Mock<IContentHashRepository>();
        _service = new ContentDeduplicationService(_mockRepository.Object);
    }

    [TestMethod]
    public async Task ComputeHashAsync_ValidContent_ReturnsSHA256Hash()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var hash = await _service.ComputeHashAsync(content);

        // Assert
        Assert.IsNotNull(hash);
        Assert.IsTrue(hash.Length > 0);
        Assert.AreEqual(44, hash.Length);  // Base64-encoded SHA256 is 44 chars
    }

    [TestMethod]
    public async Task ComputeHashAsync_SameContent_ReturnsSameHash()
    {
        // Arrange
        var content1 = new byte[] { 1, 2, 3, 4, 5 };
        var content2 = new byte[] { 1, 2, 3, 4, 5 };

        // Act
        var hash1 = await _service.ComputeHashAsync(content1);
        var hash2 = await _service.ComputeHashAsync(content2);

        // Assert
        Assert.AreEqual(hash1, hash2);
    }

    [TestMethod]
    public async Task FindContentByHashAsync_ExistingHash_ReturnsContentId()
    {
        // Arrange
        var hash = "test-hash-123";
        var expectedContentId = Guid.NewGuid();

        _mockRepository
            .Setup(r => r.FindByHashAsync(hash))
            .ReturnsAsync(new ContentHashEntry { ContentId = expectedContentId });

        // Act
        var contentId = await _service.FindContentByHashAsync(hash);

        // Assert
        Assert.AreEqual(expectedContentId, contentId);
    }

    [TestMethod]
    public async Task FindContentByHashAsync_NonExistingHash_ReturnsNull()
    {
        // Arrange
        var hash = "non-existing-hash";

        _mockRepository
            .Setup(r => r.FindByHashAsync(hash))
            .ReturnsAsync((ContentHashEntry?)null);

        // Act
        var contentId = await _service.FindContentByHashAsync(hash);

        // Assert
        Assert.IsNull(contentId);
    }

    [TestMethod]
    public async Task RegisterContentAsync_NewHash_IncreasesReferenceCount()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var hash = "test-hash-123";

        _mockRepository
            .Setup(r => r.AddOrUpdateAsync(It.IsAny<ContentHashEntry>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RegisterContentAsync(documentId, hash);

        // Assert
        _mockRepository.Verify(r => r.AddOrUpdateAsync(It.Is<ContentHashEntry>(e =>
            e.DocumentId == documentId &&
            e.ContentHash == hash)),
            Times.Once);
    }

    [TestMethod]
    public async Task DecrementReferenceAsync_ExistingHash_ReturnsNewCount()
    {
        // Arrange
        var hash = "test-hash-123";

        _mockRepository
            .Setup(r => r.DecrementReferenceCountAsync(hash))
            .ReturnsAsync(2);  // 2 references remaining

        // Act
        var newCount = await _service.DecrementReferenceAsync(hash);

        // Assert
        Assert.AreEqual(2, newCount);
    }

    [TestMethod]
    public async Task DecrementReferenceAsync_LastReference_ReturnsZero()
    {
        // Arrange
        var hash = "test-hash-123";

        _mockRepository
            .Setup(r => r.DecrementReferenceCountAsync(hash))
            .ReturnsAsync(0);  // Last reference removed

        // Act
        var newCount = await _service.DecrementReferenceAsync(hash);

        // Assert
        Assert.AreEqual(0, newCount);
    }
}
```

---

## Integration Tests

### Test Scenarios

#### 1. Database Provider Integration

**File:** `DatabasePersistenceProviderIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class DatabasePersistenceProviderIntegrationTests
{
    private IDbConnection _connection;
    private DatabasePersistenceProvider _provider;
    private TestContext _testContext;

    [TestInitialize]
    public async Task Setup()
    {
        var connectionString = _testContext.GetRequiredProperty<string>("SQL_SERVER_CONNECTION_STRING");
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();

        _provider = new DatabasePersistenceProvider(_connection, Mock.Of<ILogger<DatabasePersistenceProvider>>());

        await CreateTestSchema();
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await DropTestSchema();
        await _connection.DisposeAsync();
    }

    [TestMethod]
    public async Task CreateAsync_NewDocument_StoresSuccessfully()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        var request = new DocumentCreateRequest
        {
            Name = "test.pdf",
            MediaType = "application/pdf",
            Content = new byte[] { 1, 2, 3 },
            Tags = new[] { "test", "integration" }
        };

        var context = new PersistenceContext
        {
            RequestingApplication = "integration-test",
            UserId = "test-user"
        };

        // Act
        await _provider.CreateAsync(documentId, request, context);

        // Assert - Query database to verify
        var document = await _connection.QuerySingleAsync<Document>(
            "SELECT * FROM Documents WHERE Id = @Id",
            new { Id = documentId });

        Assert.IsNotNull(document);
        Assert.AreEqual("test.pdf", document.Name);
        Assert.AreEqual("application/pdf", document.MediaType);
        Assert.AreEqual(3, document.Size);
    }

    [TestMethod]
    public async Task CreateAsync_WithDeduplication_ReusesExistingContent()
    {
        // Arrange
        var doc1Id = Guid.NewGuid();
        var doc2Id = Guid.NewGuid();

        var content = new byte[] { 1, 2, 3, 4, 5 };
        var contentHash = ComputeHash(content);

        var request1 = new DocumentCreateRequest
        {
            Name = "doc1.pdf",
            Content = content
        };

        var request2 = new DocumentCreateRequest
        {
            Name = "doc2.pdf",
            Content = content  // Same content
        };

        var context = new PersistenceContext
        {
            EnableDeduplication = true
        };
        context.AdditionalContext["ContentHash"] = contentHash;

        // Act
        await _provider.CreateAsync(doc1Id, request1, context);

        // Mark content as existing for doc2
        var contentId = await GetContentIdForDocument(doc1Id);
        context.AdditionalContext["ExistingContentId"] = contentId;

        await _provider.CreateAsync(doc2Id, request2, context);

        // Assert - Both documents should reference same content
        var doc1ContentId = await GetContentIdForDocument(doc1Id);
        var doc2ContentId = await GetContentIdForDocument(doc2Id);

        Assert.AreEqual(doc1ContentId, doc2ContentId);

        // Only one content blob should exist
        var contentCount = await _connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM DocumentContent WHERE Id = @Id",
            new { Id = contentId });

        Assert.AreEqual(1, contentCount);
    }

    [TestMethod]
    public async Task UpdateAsync_WithVersioning_CreatesNewVersion()
    {
        // Arrange
        var documentId = Guid.NewGuid();

        // Create initial document
        await InsertTestDocument(documentId, "test.pdf", new byte[] { 1, 2, 3 });

        var updateRequest = new DocumentUpdateRequest
        {
            Content = new byte[] { 4, 5, 6 }
        };

        var context = new PersistenceContext
        {
            EnableVersioning = true,
            ChangeDescription = "Updated content"
        };
        context.AdditionalContext["CreateVersion"] = true;

        // Act
        await _provider.UpdateAsync(documentId, updateRequest, context);

        // Assert - Check version was created
        var versions = await _connection.QueryAsync<DocumentVersion>(
            "SELECT * FROM DocumentVersions WHERE DocumentId = @Id",
            new { Id = documentId });

        Assert.AreEqual(1, versions.Count());
        Assert.AreEqual(1, versions.First().Version);
        Assert.AreEqual("Updated content", versions.First().ChangeDescription);

        // Check document version incremented
        var document = await _connection.QuerySingleAsync<Document>(
            "SELECT * FROM Documents WHERE Id = @Id",
            new { Id = documentId });

        Assert.AreEqual(2, document.Version);
    }

    [TestMethod]
    public async Task DeleteAsync_SoftDelete_MarksAsDeleted()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocument(documentId, "test.pdf", new byte[] { 1, 2, 3 });

        var context = new PersistenceContext
        {
            SoftDelete = true,
            UserId = "test-user"
        };

        // Act
        await _provider.DeleteAsync(documentId, context);

        // Assert - Document should be marked as deleted
        var document = await _connection.QuerySingleAsync<dynamic>(
            "SELECT IsDeleted, DeletedDate, DeletedBy FROM Documents WHERE Id = @Id",
            new { Id = documentId });

        Assert.IsTrue((bool)document.IsDeleted);
        Assert.IsNotNull(document.DeletedDate);
        Assert.AreEqual("test-user", document.DeletedBy);
    }

    [TestMethod]
    public async Task DeleteAsync_HardDelete_RemovesDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocument(documentId, "test.pdf", new byte[] { 1, 2, 3 });

        var context = new PersistenceContext
        {
            SoftDelete = false
        };
        context.AdditionalContext["DeleteContent"] = true;

        // Act
        await _provider.DeleteAsync(documentId, context);

        // Assert - Document should be removed
        var count = await _connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Documents WHERE Id = @Id",
            new { Id = documentId });

        Assert.AreEqual(0, count);
    }

    [TestMethod]
    public async Task RestoreAsync_SoftDeletedDocument_RestoresDocument()
    {
        // Arrange
        var documentId = Guid.NewGuid();
        await InsertTestDocument(documentId, "test.pdf", new byte[] { 1, 2, 3 });

        // Soft delete
        await _connection.ExecuteAsync(
            "UPDATE Documents SET IsDeleted = 1, DeletedDate = @Date WHERE Id = @Id",
            new { Id = documentId, Date = DateTime.UtcNow });

        var context = new PersistenceContext();

        // Act
        await _provider.RestoreAsync(documentId, context);

        // Assert - Document should be restored
        var document = await _connection.QuerySingleAsync<dynamic>(
            "SELECT IsDeleted, DeletedDate FROM Documents WHERE Id = @Id",
            new { Id = documentId });

        Assert.IsFalse((bool)document.IsDeleted);
        Assert.IsNull(document.DeletedDate);
    }

    [TestMethod]
    public async Task PurgeDeletedAsync_OldDeletedDocuments_PermanentlyRemoves()
    {
        // Arrange
        var doc1Id = Guid.NewGuid();
        var doc2Id = Guid.NewGuid();
        var doc3Id = Guid.NewGuid();

        await InsertTestDocument(doc1Id, "old1.pdf", new byte[] { 1 });
        await InsertTestDocument(doc2Id, "old2.pdf", new byte[] { 2 });
        await InsertTestDocument(doc3Id, "recent.pdf", new byte[] { 3 });

        // Soft delete doc1 and doc2 (45 days ago)
        await _connection.ExecuteAsync(@"
            UPDATE Documents
            SET IsDeleted = 1, DeletedDate = @OldDate
            WHERE Id IN (@Id1, @Id2)",
            new
            {
                OldDate = DateTime.UtcNow.AddDays(-45),
                Id1 = doc1Id,
                Id2 = doc2Id
            });

        // Soft delete doc3 (5 days ago)
        await _connection.ExecuteAsync(@"
            UPDATE Documents
            SET IsDeleted = 1, DeletedDate = @RecentDate
            WHERE Id = @Id3",
            new
            {
                RecentDate = DateTime.UtcNow.AddDays(-5),
                Id3 = doc3Id
            });

        var context = new PersistenceContext();

        // Act - Purge documents deleted more than 30 days ago
        var purgedCount = await _provider.PurgeDeletedAsync(TimeSpan.FromDays(30), context);

        // Assert
        Assert.AreEqual(2, purgedCount);

        // doc1 and doc2 should be gone
        var doc1Exists = await _connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Documents WHERE Id = @Id", new { Id = doc1Id });
        var doc2Exists = await _connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Documents WHERE Id = @Id", new { Id = doc2Id });

        Assert.AreEqual(0, doc1Exists);
        Assert.AreEqual(0, doc2Exists);

        // doc3 should still exist (soft-deleted)
        var doc3Exists = await _connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM Documents WHERE Id = @Id", new { Id = doc3Id });

        Assert.AreEqual(1, doc3Exists);
    }

    [TestMethod]
    public async Task CreateBatchAsync_MultipleDocuments_CreatesAllTransactionally()
    {
        // Arrange
        var requests = new[]
        {
            (Guid.NewGuid(), new DocumentCreateRequest { Name = "doc1.pdf", Content = new byte[] { 1 } }),
            (Guid.NewGuid(), new DocumentCreateRequest { Name = "doc2.pdf", Content = new byte[] { 2 } }),
            (Guid.NewGuid(), new DocumentCreateRequest { Name = "doc3.pdf", Content = new byte[] { 3 } })
        };

        var context = new PersistenceContext();

        // Act
        var documentIds = await _provider.CreateBatchAsync(requests, context);

        // Assert
        Assert.AreEqual(3, documentIds.Count());

        foreach (var id in documentIds)
        {
            var exists = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM Documents WHERE Id = @Id", new { Id = id });

            Assert.AreEqual(1, exists);
        }
    }

    private async Task InsertTestDocument(Guid id, string name, byte[] content)
    {
        var contentId = Guid.NewGuid();

        await _connection.ExecuteAsync(@"
            INSERT INTO DocumentContent (Id, Content, CreatedDate)
            VALUES (@ContentId, @Content, @CreatedDate)",
            new { ContentId = contentId, Content = content, CreatedDate = DateTime.UtcNow });

        await _connection.ExecuteAsync(@"
            INSERT INTO Documents (Id, Name, MediaType, ContentId, Size, CreatedDate, ModifiedDate, Version, IsDeleted)
            VALUES (@Id, @Name, @MediaType, @ContentId, @Size, @CreatedDate, @ModifiedDate, 1, 0)",
            new
            {
                Id = id,
                Name = name,
                MediaType = "application/pdf",
                ContentId = contentId,
                Size = content.Length,
                CreatedDate = DateTime.UtcNow,
                ModifiedDate = DateTime.UtcNow
            });
    }
}
```

---

## Performance Tests

**File:** `PersistencePerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class PersistencePerformanceTests
{
    [TestMethod]
    public async Task CreateAsync_SmallDocument_PerformanceTest()
    {
        // Arrange
        var request = new DocumentCreateRequest
        {
            Name = "small.pdf",
            Content = new byte[1024]  // 1KB
        };

        var context = new PersistenceContext();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var documentId = await _persistence.CreateAsync(request, context);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200,
            $"Create took {stopwatch.ElapsedMilliseconds}ms (should be < 200ms)");

        Console.WriteLine($"Small document create: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task CreateAsync_WithDeduplication_Performance()
    {
        // Arrange
        var content = new byte[1024 * 100];  // 100KB
        new Random().NextBytes(content);

        var request1 = new DocumentCreateRequest { Name = "doc1.pdf", Content = content };
        var request2 = new DocumentCreateRequest { Name = "doc2.pdf", Content = content };

        var context = new PersistenceContext { EnableDeduplication = true };

        // Act - First create (stores content)
        var stopwatch = Stopwatch.StartNew();
        await _persistence.CreateAsync(request1, context);
        var firstCreateTime = stopwatch.ElapsedMilliseconds;

        // Act - Second create (deduplicates)
        stopwatch.Restart();
        await _persistence.CreateAsync(request2, context);
        var secondCreateTime = stopwatch.ElapsedMilliseconds;

        // Assert - Second create should be faster (no content storage)
        Assert.IsTrue(secondCreateTime < firstCreateTime * 0.8,
            $"Deduplication should be faster (first: {firstCreateTime}ms, second: {secondCreateTime}ms)");

        Console.WriteLine($"Deduplication performance:");
        Console.WriteLine($"  First create: {firstCreateTime}ms");
        Console.WriteLine($"  Second create (deduplicated): {secondCreateTime}ms");
        Console.WriteLine($"  Speedup: {firstCreateTime / (double)secondCreateTime:F1}x");
    }

    [TestMethod]
    public async Task CreateBatchAsync_100Documents_PerformanceTest()
    {
        // Arrange
        var requests = Enumerable.Range(0, 100).Select(i => new DocumentCreateRequest
        {
            Name = $"doc{i}.pdf",
            Content = new byte[1024]  // 1KB each
        });

        var context = new PersistenceContext();

        // Act
        var stopwatch = Stopwatch.StartNew();
        var documentIds = await _persistence.CreateBatchAsync(requests, context);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(100, documentIds.Count());
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 5000,
            $"Batch create took {stopwatch.ElapsedMilliseconds}ms (should be < 5000ms)");

        var docsPerSecond = 100 / (stopwatch.ElapsedMilliseconds / 1000.0);
        Console.WriteLine($"Batch create (100 documents): {stopwatch.ElapsedMilliseconds}ms ({docsPerSecond:F0} docs/sec)");
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| DocumentPersistenceService | 90% | CreateAsync, UpdateAsync, DeleteAsync, Deduplication |
| ContentDeduplicationService | 95% | Hash calculation, reference counting |
| PersistenceContext | 95% | Property setters, validation |
| Provider Implementations | 85% | CreateAsync, UpdateAsync, DeleteAsync, versioning |
| Error Handling | 80% | Exceptions, failover logic |

**Overall Target:** 85%+ code coverage

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
