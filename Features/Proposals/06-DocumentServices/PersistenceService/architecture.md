# Document Persistence Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Persistence Service
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Document Persistence Service implements a **Provider Pattern** with **Context-Based Storage** for writing documents to multiple storage backends. The service supports versioning, deduplication, soft delete, and storage tier management based on operational context.

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│         (Invoice Processor, Report Generator, etc.)          │
└────────────────────┬────────────────────────────────────────┘
                     │ CreateAsync(request, context)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│            IDocumentPersistenceService                      │
│  - CreateAsync(request, context)                            │
│  - UpdateAsync(id, request, context)                        │
│  - DeleteAsync(id, context)                                 │
│  - CreateBatchAsync(requests, context)                      │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼────────────┬────────────┐
         ↓           ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌─────────┐
│ Database     │ │  File  │ │  Azure  │ │   S3    │
│ Provider     │ │ System │ │  Blob   │ │ Provider│
└──────┬───────┘ └────┬───┘ └────┬────┘ └────┬────┘
       │              │          │            │
       ↓              ↓          ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌─────────┐
│  SQL Server  │ │  Disk  │ │  Azure  │ │   AWS   │
│  MongoDB     │ │  NAS   │ │ Storage │ │   S3    │
└──────────────┘ └────────┘ └─────────┘ └─────────┘
```

---

## Core Components

### 1. DocumentPersistenceService (Main Entry Point)

**Responsibilities:**
- Coordinate document persistence across providers
- Select appropriate provider based on context and configuration
- Handle content deduplication using SHA256 hashing
- Manage versioning when enabled
- Handle provider failover on failure
- Audit all persistence operations

**Key Design Decisions:**
- **Multi-provider support** - Route requests to appropriate storage backend
- **Context propagation** - Pass operational context to all providers
- **Deduplication** - Calculate content hash and reuse existing blobs
- **Versioning** - Preserve document history when enabled
- **Soft delete** - Mark documents as deleted by default, allow restore

**Implementation Pattern:**
```csharp
public class DocumentPersistenceService : IDocumentPersistenceService
{
    private readonly IDocumentPersistenceProviderFactory _providerFactory;
    private readonly IContentDeduplicationService _deduplication;
    private readonly DocumentPersistenceOptions _options;
    private readonly ILogger<DocumentPersistenceService> _logger;

    public async Task<Guid> CreateAsync(DocumentCreateRequest request, PersistenceContext? context = null)
    {
        context ??= new PersistenceContext();

        // 1. Generate document ID
        var documentId = Guid.NewGuid();

        // 2. Validate request
        ValidateCreateRequest(request);

        // 3. Handle deduplication if enabled
        string? contentHash = null;
        Guid? existingContentId = null;

        if (context.EnableDeduplication)
        {
            contentHash = ComputeContentHash(request.Content);
            existingContentId = await _deduplication.FindContentByHashAsync(contentHash);

            if (existingContentId.HasValue)
            {
                _logger.LogDebug("Found duplicate content with hash {Hash}, reusing content {ContentId}",
                    contentHash, existingContentId);
            }
        }

        // 4. Select provider
        var provider = await SelectProviderAsync(context);

        try
        {
            // 5. Store document
            _logger.LogDebug("Creating document {DocumentId} using provider {Provider}",
                documentId, provider.ProviderName);

            // Pass content hash and existing content ID to provider
            context.AdditionalContext["ContentHash"] = contentHash;
            context.AdditionalContext["ExistingContentId"] = existingContentId;

            await provider.CreateAsync(documentId, request, context);

            // 6. Update deduplication index
            if (context.EnableDeduplication && contentHash != null)
            {
                await _deduplication.RegisterContentAsync(documentId, contentHash);
            }

            // 7. Audit log
            await AuditCreateAsync(documentId, request, context);

            _logger.LogInformation("Created document {DocumentId} (size: {Size} bytes, provider: {Provider})",
                documentId, request.Content.Length, provider.ProviderName);

            return documentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create document using provider {Provider}", provider.ProviderName);

            // Try failover provider
            if (_options.FailoverProvider != null)
            {
                var failoverProvider = await _providerFactory.GetProviderAsync(_options.FailoverProvider);
                return await failoverProvider.CreateAsync(documentId, request, context);
            }

            throw new DocumentPersistenceException($"Failed to create document", ex);
        }
    }

    public async Task UpdateAsync(Guid documentId, DocumentUpdateRequest request, PersistenceContext? context = null)
    {
        context ??= new PersistenceContext();

        // 1. Validate request
        ValidateUpdateRequest(request);

        // 2. Check if versioning enabled
        if (context.EnableVersioning)
        {
            var provider = await SelectProviderAsync(context);

            if (!provider.Capabilities.SupportsVersioning)
            {
                throw new VersioningNotSupportedException(provider.ProviderName);
            }

            // Increment version number (provider handles version creation)
            context.AdditionalContext["CreateVersion"] = true;
        }

        // 3. Handle content deduplication for new content
        if (request.Content != null && context.EnableDeduplication)
        {
            var contentHash = ComputeContentHash(request.Content);
            var existingContentId = await _deduplication.FindContentByHashAsync(contentHash);

            context.AdditionalContext["ContentHash"] = contentHash;
            context.AdditionalContext["ExistingContentId"] = existingContentId;
        }

        // 4. Select provider and update
        var provider = await SelectProviderAsync(context);

        _logger.LogDebug("Updating document {DocumentId} using provider {Provider}", documentId, provider.ProviderName);

        await provider.UpdateAsync(documentId, request, context);

        // 5. Audit log
        await AuditUpdateAsync(documentId, request, context);

        _logger.LogInformation("Updated document {DocumentId} (version: {Version})",
            documentId, context.EnableVersioning ? "new version" : "same version");
    }

    public async Task DeleteAsync(Guid documentId, PersistenceContext? context = null)
    {
        context ??= new PersistenceContext();

        var provider = await SelectProviderAsync(context);

        if (context.SoftDelete && provider.Capabilities.SupportsSoftDelete)
        {
            _logger.LogDebug("Soft deleting document {DocumentId}", documentId);
            await provider.DeleteAsync(documentId, context);
        }
        else
        {
            _logger.LogDebug("Hard deleting document {DocumentId}", documentId);
            await HardDeleteInternalAsync(documentId, provider, context);
        }

        // Audit log
        await AuditDeleteAsync(documentId, context.SoftDelete, context);

        _logger.LogInformation("Deleted document {DocumentId} (soft: {SoftDelete})",
            documentId, context.SoftDelete);
    }

    private async Task HardDeleteInternalAsync(
        Guid documentId,
        IDocumentPersistenceProvider provider,
        PersistenceContext context)
    {
        // 1. Get document info for deduplication cleanup
        if (context.EnableDeduplication)
        {
            var contentHash = await _deduplication.GetContentHashAsync(documentId);
            if (contentHash != null)
            {
                // Decrement reference count
                var refCount = await _deduplication.DecrementReferenceAsync(contentHash);

                // If no more references, allow provider to delete actual content
                context.AdditionalContext["DeleteContent"] = refCount == 0;
            }
        }

        // 2. Delete document
        await provider.DeleteAsync(documentId, context);
    }

    private string ComputeContentHash(byte[] content)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(content);
        return Convert.ToBase64String(hashBytes);
    }

    private async Task<IDocumentPersistenceProvider> SelectProviderAsync(PersistenceContext context)
    {
        // 1. Check context for preferred provider
        if (context.AdditionalContext.TryGetValue("PreferredProvider", out var preferredProvider))
        {
            return await _providerFactory.GetProviderAsync(preferredProvider.ToString()!);
        }

        // 2. Use default provider from configuration
        return await _providerFactory.GetProviderAsync(_options.DefaultProvider);
    }
}
```

---

### 2. Content Deduplication Service

**Responsibilities:**
- Calculate content hashes (SHA256)
- Track content references across documents
- Manage reference counting
- Clean up orphaned content

**Implementation Pattern:**
```csharp
public interface IContentDeduplicationService
{
    Task<string> ComputeHashAsync(byte[] content);
    Task<Guid?> FindContentByHashAsync(string hash);
    Task RegisterContentAsync(Guid documentId, string hash);
    Task<string?> GetContentHashAsync(Guid documentId);
    Task<int> DecrementReferenceAsync(string hash);
    Task<int> GetReferenceCountAsync(string hash);
}

public class ContentDeduplicationService : IContentDeduplicationService
{
    private readonly IContentHashRepository _hashRepository;

    public async Task<Guid?> FindContentByHashAsync(string hash)
    {
        // Query hash index to find existing content with same hash
        var existing = await _hashRepository.FindByHashAsync(hash);
        return existing?.ContentId;
    }

    public async Task RegisterContentAsync(Guid documentId, string hash)
    {
        // Register document -> content hash mapping
        // Increment reference count for hash
        await _hashRepository.AddOrUpdateAsync(new ContentHashEntry
        {
            DocumentId = documentId,
            ContentHash = hash,
            CreatedDate = DateTime.UtcNow
        });
    }

    public async Task<int> DecrementReferenceAsync(string hash)
    {
        // Decrement reference count
        // Return new reference count
        return await _hashRepository.DecrementReferenceCountAsync(hash);
    }
}
```

**Data Model:**
```sql
CREATE TABLE ContentHashes (
    ContentHash VARCHAR(64) PRIMARY KEY,
    ContentId UNIQUEIDENTIFIER NOT NULL,
    ReferenceCount INT NOT NULL DEFAULT 1,
    CreatedDate DATETIME2 NOT NULL,
    INDEX IX_ContentHash (ContentHash)
);

CREATE TABLE DocumentContentMap (
    DocumentId UNIQUEIDENTIFIER PRIMARY KEY,
    ContentHash VARCHAR(64) NOT NULL,
    CreatedDate DATETIME2 NOT NULL,
    FOREIGN KEY (ContentHash) REFERENCES ContentHashes(ContentHash)
);
```

---

### 3. Provider Implementations

#### DatabasePersistenceProvider (SQL Server, PostgreSQL)

**Responsibilities:**
- Store documents in relational database
- Support versioning with version table
- Handle soft delete with IsDeleted flag
- Support transactional operations

**Implementation Pattern:**
```csharp
public class DatabasePersistenceProvider : IDocumentPersistenceProvider
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DatabasePersistenceProvider> _logger;

    public string ProviderName => "database";

    public DocumentStorageCapabilities Capabilities => new()
    {
        SupportsVersioning = true,
        SupportsDeduplication = true,
        SupportsSoftDelete = true,
        SupportsStorageTiers = false,
        SupportsTransactionalBatch = true,
        MaxDocumentSize = 100 * 1024 * 1024  // 100MB (VARBINARY(MAX))
    };

    public async Task CreateAsync(Guid documentId, DocumentCreateRequest request, PersistenceContext context)
    {
        var existingContentId = context.AdditionalContext.TryGetValue("ExistingContentId", out var val)
            ? (Guid?)val
            : null;

        using var transaction = await _connection.BeginTransactionAsync();

        try
        {
            // Insert content (if not deduplicated)
            var contentId = existingContentId ?? Guid.NewGuid();

            if (!existingContentId.HasValue)
            {
                var insertContent = @"
                    INSERT INTO DocumentContent (Id, Content, ContentHash, CreatedDate)
                    VALUES (@Id, @Content, @ContentHash, @CreatedDate)";

                await _connection.ExecuteAsync(insertContent, new
                {
                    Id = contentId,
                    Content = request.Content,
                    ContentHash = context.AdditionalContext["ContentHash"],
                    CreatedDate = DateTime.UtcNow
                }, transaction);
            }

            // Insert document metadata
            var insertDocument = @"
                INSERT INTO Documents (Id, Name, MediaType, ContentId, Size, Metadata, Tags,
                                       CreatedDate, CreatedBy, ModifiedDate, ModifiedBy, Version, IsDeleted)
                VALUES (@Id, @Name, @MediaType, @ContentId, @Size, @Metadata, @Tags,
                        @CreatedDate, @CreatedBy, @ModifiedDate, @ModifiedBy, @Version, 0)";

            await _connection.ExecuteAsync(insertDocument, new
            {
                Id = documentId,
                Name = request.Name,
                MediaType = request.MediaType,
                ContentId = contentId,
                Size = request.Content.Length,
                Metadata = JsonSerializer.Serialize(request.Metadata),
                Tags = string.Join(",", request.Tags),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = context.UserId,
                ModifiedDate = DateTime.UtcNow,
                ModifiedBy = context.UserId,
                Version = 1
            }, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task UpdateAsync(Guid documentId, DocumentUpdateRequest request, PersistenceContext context)
    {
        var createVersion = context.AdditionalContext.TryGetValue("CreateVersion", out var cv) && (bool)cv;

        using var transaction = await _connection.BeginTransactionAsync();

        try
        {
            if (createVersion)
            {
                // Archive current version
                var archiveSql = @"
                    INSERT INTO DocumentVersions (DocumentId, Version, ContentId, Name, MediaType, Size,
                                                   CreatedDate, CreatedBy, ChangeDescription)
                    SELECT Id, Version, ContentId, Name, MediaType, Size,
                           ModifiedDate, ModifiedBy, @ChangeDescription
                    FROM Documents
                    WHERE Id = @DocumentId";

                await _connection.ExecuteAsync(archiveSql, new
                {
                    DocumentId = documentId,
                    ChangeDescription = context.ChangeDescription
                }, transaction);

                // Increment version number in document
                var updateVersion = @"UPDATE Documents SET Version = Version + 1 WHERE Id = @DocumentId";
                await _connection.ExecuteAsync(updateVersion, new { DocumentId = documentId }, transaction);
            }

            // Update document
            var updates = new List<string>();
            var parameters = new DynamicParameters();
            parameters.Add("Id", documentId);

            if (request.Name != null)
            {
                updates.Add("Name = @Name");
                parameters.Add("Name", request.Name);
            }

            if (request.Content != null)
            {
                // Insert new content (handle deduplication)
                var contentId = Guid.NewGuid();
                // ... similar to CreateAsync

                updates.Add("ContentId = @ContentId");
                updates.Add("Size = @Size");
                parameters.Add("ContentId", contentId);
                parameters.Add("Size", request.Content.Length);
            }

            if (request.Metadata != null)
            {
                updates.Add("Metadata = @Metadata");
                parameters.Add("Metadata", JsonSerializer.Serialize(request.Metadata));
            }

            updates.Add("ModifiedDate = @ModifiedDate");
            updates.Add("ModifiedBy = @ModifiedBy");
            parameters.Add("ModifiedDate", DateTime.UtcNow);
            parameters.Add("ModifiedBy", context.UserId);

            var updateSql = $"UPDATE Documents SET {string.Join(", ", updates)} WHERE Id = @Id";
            await _connection.ExecuteAsync(updateSql, parameters, transaction);

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task DeleteAsync(Guid documentId, PersistenceContext context)
    {
        if (context.SoftDelete)
        {
            // Soft delete: Mark as deleted
            var sql = @"
                UPDATE Documents
                SET IsDeleted = 1, DeletedDate = @DeletedDate, DeletedBy = @DeletedBy
                WHERE Id = @Id";

            await _connection.ExecuteAsync(sql, new
            {
                Id = documentId,
                DeletedDate = DateTime.UtcNow,
                DeletedBy = context.UserId
            });
        }
        else
        {
            // Hard delete
            var deleteContent = context.AdditionalContext.TryGetValue("DeleteContent", out var dc) && (bool)dc;

            using var transaction = await _connection.BeginTransactionAsync();

            try
            {
                // Delete document
                await _connection.ExecuteAsync("DELETE FROM Documents WHERE Id = @Id",
                    new { Id = documentId }, transaction);

                // Delete content if no more references
                if (deleteContent)
                {
                    await _connection.ExecuteAsync(
                        "DELETE FROM DocumentContent WHERE Id = (SELECT ContentId FROM Documents WHERE Id = @Id)",
                        new { Id = documentId }, transaction);
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
```

---

#### AzureBlobPersistenceProvider

**Responsibilities:**
- Store documents in Azure Blob Storage
- Support storage tiers (Hot, Cool, Archive)
- Handle blob metadata and tags
- Support soft delete with blob soft delete feature

**Implementation Pattern:**
```csharp
public class AzureBlobPersistenceProvider : IDocumentPersistenceProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobPersistenceProvider> _logger;

    public string ProviderName => "azure-blob";

    public DocumentStorageCapabilities Capabilities => new()
    {
        SupportsVersioning = true,  // Blob versioning
        SupportsDeduplication = false,  // Not natively supported
        SupportsSoftDelete = true,  // Blob soft delete
        SupportsStorageTiers = true,
        SupportsTransactionalBatch = false,
        MaxDocumentSize = 5L * 1024 * 1024 * 1024,  // 5GB
        SupportedTiers = new[] { StorageTier.Hot, StorageTier.Cool, StorageTier.Archive }
    };

    public async Task CreateAsync(Guid documentId, DocumentCreateRequest request, PersistenceContext context)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(context));
        await containerClient.CreateIfNotExistsAsync();

        var blobClient = containerClient.GetBlobClient(documentId.ToString());

        // Set metadata
        var metadata = new Dictionary<string, string>
        {
            ["Name"] = request.Name,
            ["MediaType"] = request.MediaType,
            ["CreatedBy"] = context.UserId ?? "system",
            ["CreatedDate"] = DateTime.UtcNow.ToString("o"),
            ["Tags"] = string.Join(",", request.Tags)
        };

        // Add custom metadata
        foreach (var kvp in request.Metadata)
        {
            metadata[$"Custom_{kvp.Key}"] = kvp.Value?.ToString() ?? "";
        }

        // Set upload options with access tier
        var uploadOptions = new BlobUploadOptions
        {
            Metadata = metadata,
            AccessTier = MapStorageTier(context.StorageTier),
            HttpHeaders = new BlobHttpHeaders
            {
                ContentType = request.MediaType
            }
        };

        // Set tags
        if (request.Tags.Length > 0)
        {
            uploadOptions.Tags = request.Tags.ToDictionary(t => t, t => "");
        }

        // Upload content
        using var stream = new MemoryStream(request.Content);
        await blobClient.UploadAsync(stream, uploadOptions);

        _logger.LogDebug("Created blob {BlobName} in container {Container} (tier: {Tier})",
            documentId, containerClient.Name, context.StorageTier);
    }

    public async Task ChangeStorageTierAsync(Guid documentId, StorageTier tier, PersistenceContext context)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(context));
        var blobClient = containerClient.GetBlobClient(documentId.ToString());

        await blobClient.SetAccessTierAsync(MapStorageTier(tier));

        _logger.LogInformation("Changed storage tier for blob {BlobName} to {Tier}", documentId, tier);
    }

    private AccessTier MapStorageTier(StorageTier tier)
    {
        return tier switch
        {
            StorageTier.Hot => AccessTier.Hot,
            StorageTier.Cool => AccessTier.Cool,
            StorageTier.Archive => AccessTier.Archive,
            _ => AccessTier.Hot
        };
    }

    private string GetContainerName(PersistenceContext context)
    {
        // Use requesting application as container name
        return context.RequestingApplication?.ToLowerInvariant().Replace("_", "-") ?? "documents";
    }
}
```

---

## Data Flow

### Sequence: Document Create with Deduplication

```
┌───────────┐      ┌──────────────────┐      ┌──────────────┐      ┌──────────┐
│Application│      │PersistenceService│      │Deduplication │      │ Provider │
└─────┬─────┘      └────────┬─────────┘      └──────┬───────┘      └─────┬────┘
      │                     │                        │                     │
      │ CreateAsync(req,ctx)│                        │                     │
      ├────────────────────>│                        │                     │
      │                     │ ComputeHash(content)   │                     │
      │                     ├───────────────────────>│                     │
      │                     │ SHA256 hash            │                     │
      │                     │<───────────────────────┤                     │
      │                     │                        │                     │
      │                     │ FindContentByHash()    │                     │
      │                     ├───────────────────────>│                     │
      │                     │ Existing content ID    │                     │
      │                     │<───────────────────────┤                     │
      │                     │                        │                     │
      │                     │ CreateAsync(req, ctx)  │                     │
      │                     ├────────────────────────────────────────────>│
      │                     │                        │                     │
      │                     │                        │  Store metadata     │
      │                     │                        │  (reference content)│
      │                     │                        │                     │
      │                     │ Document ID            │                     │
      │                     │<────────────────────────────────────────────┤
      │                     │                        │                     │
      │                     │ RegisterContent(id,hash)                    │
      │                     ├───────────────────────>│                     │
      │                     │                        │                     │
      │ Document ID         │                        │                     │
      │<────────────────────┤                        │                     │
      │                     │                        │                     │
```

---

## Design Patterns

### 1. Provider Pattern
- Multiple storage backends via pluggable providers
- Provider selection based on context and configuration
- Failover providers for resilience

### 2. Context Pattern
- Operational context passed through call chain
- Providers use context to adjust behavior
- Context includes versioning, deduplication, tier preferences

### 3. Content Addressable Storage (CAS)
- Content identified by cryptographic hash
- Deduplication through content addressing
- Reference counting for shared content

### 4. Soft Delete Pattern
- Documents marked as deleted, not immediately removed
- Recovery possible for soft-deleted documents
- Purge operation for permanent removal

---

## Performance Optimizations

### 1. Content Deduplication
- Reuse existing content blobs
- Reduces storage costs
- Faster uploads (skip content transfer)
- SHA256 hash calculated once

### 2. Batch Operations
- Bulk create/update/delete
- Reduced round trips
- Transactional consistency (where supported)

### 3. Storage Tier Optimization
- Archive tier for cold data
- Auto-tier migration based on access patterns
- Cost optimization

### 4. Chunked Upload
- Large files uploaded in chunks
- Resume capability on failure
- Progress reporting

---

## Error Handling

### Provider Failover Strategy
```csharp
public async Task<Guid> CreateAsync(DocumentCreateRequest request, PersistenceContext? context = null)
{
    var provider = await SelectProviderAsync(context);

    try
    {
        return await provider.CreateAsync(documentId, request, context);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Provider {Provider} failed, trying failover", provider.ProviderName);

        if (_options.FailoverProvider != null)
        {
            var failoverProvider = await _providerFactory.GetProviderAsync(_options.FailoverProvider);
            return await failoverProvider.CreateAsync(documentId, request, context);
        }

        throw new DocumentPersistenceException($"Failed to create document", ex);
    }
}
```

---

## Thread Safety

### Concurrency Strategy
- Providers must be thread-safe
- Service is thread-safe (stateless or properly synchronized)
- Multiple threads can create different documents concurrently
- Deduplication service uses database-level locking for reference counting
- Versioning uses optimistic concurrency (version number check)

---

## Testing Strategy

### Unit Tests
- Mock providers for service tests
- Test provider selection logic
- Test deduplication logic
- Test versioning logic
- Test context propagation

### Integration Tests
- Real providers with test storage
- End-to-end persistence scenarios
- Deduplication validation
- Versioning validation
- Performance benchmarks

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [RetrievalService Architecture](../RetrievalService/architecture.md)
