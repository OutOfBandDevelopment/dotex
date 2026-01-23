# Document Persistence Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Persistence Service
**Last Updated:** 2026-01-22

---

## API Overview

The Document Persistence Service API provides context-based document storage with support for multiple storage providers, versioning, deduplication, soft delete, and storage tier management.

---

## Core Interfaces

### IDocumentPersistenceService

**Purpose:** Main service interface for persisting documents to storage with operational context.

```csharp
namespace OoBDev.System.Documents.Persistence;

/// <summary>
/// Service for persisting documents to storage with context-based behavior.
/// </summary>
public interface IDocumentPersistenceService
{
    /// <summary>
    /// Creates new document with content and metadata.
    /// </summary>
    /// <param name="request">Document creation request</param>
    /// <param name="context">Optional persistence context (versioning, deduplication, etc.)</param>
    /// <returns>Created document ID</returns>
    /// <exception cref="DocumentPersistenceException">Creation failed</exception>
    /// <exception cref="DocumentTooLargeException">Document exceeds provider limits</exception>
    Task<Guid> CreateAsync(DocumentCreateRequest request, PersistenceContext? context = null);

    /// <summary>
    /// Updates existing document content and/or metadata.
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="request">Update request (partial updates supported)</param>
    /// <param name="context">Optional persistence context</param>
    /// <exception cref="DocumentNotFoundException">Document not found</exception>
    /// <exception cref="VersioningNotSupportedException">Provider doesn't support versioning</exception>
    Task UpdateAsync(Guid documentId, DocumentUpdateRequest request, PersistenceContext? context = null);

    /// <summary>
    /// Deletes document (soft delete by default).
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="context">Optional persistence context (SoftDelete flag)</param>
    /// <exception cref="DocumentNotFoundException">Document not found</exception>
    Task DeleteAsync(Guid documentId, PersistenceContext? context = null);

    /// <summary>
    /// Hard delete permanently removes document.
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="context">Optional persistence context</param>
    /// <exception cref="DocumentNotFoundException">Document not found</exception>
    Task HardDeleteAsync(Guid documentId, PersistenceContext? context = null);

    /// <summary>
    /// Restores soft-deleted document.
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="context">Optional persistence context</param>
    /// <exception cref="DocumentNotFoundException">Document not found</exception>
    /// <exception cref="InvalidOperationException">Document not soft-deleted</exception>
    Task RestoreAsync(Guid documentId, PersistenceContext? context = null);

    /// <summary>
    /// Purges all soft-deleted documents older than specified age.
    /// </summary>
    /// <param name="olderThan">Age threshold</param>
    /// <param name="context">Optional persistence context</param>
    /// <returns>Number of documents purged</returns>
    Task<int> PurgeDeletedAsync(TimeSpan olderThan, PersistenceContext? context = null);

    /// <summary>
    /// Batch creates multiple documents.
    /// </summary>
    /// <param name="requests">Collection of create requests</param>
    /// <param name="context">Optional persistence context</param>
    /// <returns>Created document IDs</returns>
    Task<IEnumerable<Guid>> CreateBatchAsync(IEnumerable<DocumentCreateRequest> requests, PersistenceContext? context = null);

    /// <summary>
    /// Batch deletes multiple documents.
    /// </summary>
    /// <param name="documentIds">Document identifiers</param>
    /// <param name="context">Optional persistence context (SoftDelete flag)</param>
    Task DeleteBatchAsync(IEnumerable<Guid> documentIds, PersistenceContext? context = null);

    /// <summary>
    /// Changes storage tier for document.
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="tier">Target storage tier</param>
    /// <param name="context">Optional persistence context</param>
    /// <exception cref="DocumentNotFoundException">Document not found</exception>
    /// <exception cref="NotSupportedException">Provider doesn't support storage tiers</exception>
    Task ChangeStorageTierAsync(Guid documentId, StorageTier tier, PersistenceContext? context = null);
}
```

---

### Context Classes

**PersistenceContext:**
```csharp
namespace OoBDev.System.Documents.Persistence;

/// <summary>
/// Operational context for document persistence.
/// Providers use context to adjust storage behavior.
/// </summary>
public class PersistenceContext
{
    /// <summary>
    /// Name of requesting application (e.g., "invoice-processor").
    /// Providers may use this for container/bucket selection.
    /// </summary>
    public string? RequestingApplication { get; set; }

    /// <summary>
    /// User ID for auditing and access control.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Enable versioning (preserves document history).
    /// Requires provider support for versioning.
    /// </summary>
    public bool EnableVersioning { get; set; } = false;

    /// <summary>
    /// Enable content deduplication (storage optimization).
    /// Documents with identical content share storage.
    /// </summary>
    public bool EnableDeduplication { get; set; } = true;

    /// <summary>
    /// Document retention period (null = infinite).
    /// Documents auto-deleted after retention period expires.
    /// </summary>
    public TimeSpan? RetentionPeriod { get; set; }

    /// <summary>
    /// Storage tier for document (Hot, Cool, Archive).
    /// Affects cost and access speed.
    /// </summary>
    public StorageTier StorageTier { get; set; } = StorageTier.Hot;

    /// <summary>
    /// Use soft delete (default: true).
    /// Soft delete allows document recovery.
    /// </summary>
    public bool SoftDelete { get; set; } = true;

    /// <summary>
    /// Description of changes (for versioning).
    /// Stored in version history when EnableVersioning = true.
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Additional context properties for provider-specific behavior.
    /// Examples: Compress, Encrypt, ReplicationRegions
    /// </summary>
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

---

### Request Models

**DocumentCreateRequest:**
```csharp
namespace OoBDev.System.Documents.Persistence;

/// <summary>
/// Request for creating new document.
/// </summary>
public class DocumentCreateRequest
{
    /// <summary>
    /// Document name (filename or title).
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Media type (MIME type) - e.g., "application/pdf", "image/png".
    /// </summary>
    public string MediaType { get; set; } = "";

    /// <summary>
    /// Document content bytes.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Document metadata (custom properties).
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Document tags for categorization and search.
    /// </summary>
    public string[] Tags { get; set; } = Array.Empty<string>();
}
```

**DocumentUpdateRequest:**
```csharp
namespace OoBDev.System.Documents.Persistence;

/// <summary>
/// Request for updating existing document.
/// All properties are optional (partial updates supported).
/// </summary>
public class DocumentUpdateRequest
{
    /// <summary>
    /// New document name (null = no change).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// New media type (null = no change).
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// New document content (null = no change).
    /// </summary>
    public byte[]? Content { get; set; }

    /// <summary>
    /// New metadata (null = no change, empty = clear all).
    /// </summary>
    public IDictionary<string, object>? Metadata { get; set; }

    /// <summary>
    /// New tags (null = no change, empty = clear all).
    /// </summary>
    public string[]? Tags { get; set; }
}
```

---

### Enums

**StorageTier:**
```csharp
namespace OoBDev.System.Documents.Persistence;

/// <summary>
/// Storage tier for cost and access optimization.
/// </summary>
public enum StorageTier
{
    /// <summary>
    /// Hot tier: Frequent access, low latency, higher cost.
    /// </summary>
    Hot = 0,

    /// <summary>
    /// Cool tier: Infrequent access (30+ days), medium cost.
    /// </summary>
    Cool = 1,

    /// <summary>
    /// Archive tier: Rare access (90+ days), lowest cost, higher retrieval latency.
    /// </summary>
    Archive = 2
}
```

---

## Provider Interface

### IDocumentPersistenceProvider

```csharp
namespace OoBDev.System.Documents.Persistence.Providers;

/// <summary>
/// Provider interface for document persistence to specific storage backend.
/// </summary>
public interface IDocumentPersistenceProvider
{
    /// <summary>
    /// Provider name (e.g., "azure-blob", "sql-server", "file-system").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Provider capabilities (versioning, deduplication, etc.).
    /// </summary>
    DocumentStorageCapabilities Capabilities { get; }

    /// <summary>
    /// Creates new document.
    /// </summary>
    Task CreateAsync(Guid documentId, DocumentCreateRequest request, PersistenceContext context);

    /// <summary>
    /// Updates existing document.
    /// </summary>
    Task UpdateAsync(Guid documentId, DocumentUpdateRequest request, PersistenceContext context);

    /// <summary>
    /// Deletes document (respects SoftDelete flag in context).
    /// </summary>
    Task DeleteAsync(Guid documentId, PersistenceContext context);

    /// <summary>
    /// Restores soft-deleted document.
    /// </summary>
    Task RestoreAsync(Guid documentId, PersistenceContext context);

    /// <summary>
    /// Purges soft-deleted documents.
    /// </summary>
    Task<int> PurgeDeletedAsync(TimeSpan olderThan, PersistenceContext context);

    /// <summary>
    /// Batch creates multiple documents.
    /// </summary>
    Task<IEnumerable<Guid>> CreateBatchAsync(IEnumerable<(Guid Id, DocumentCreateRequest Request)> requests, PersistenceContext context);

    /// <summary>
    /// Changes storage tier for document.
    /// </summary>
    Task ChangeStorageTierAsync(Guid documentId, StorageTier tier, PersistenceContext context);
}

/// <summary>
/// Provider capabilities.
/// </summary>
public class DocumentStorageCapabilities
{
    /// <summary>
    /// Provider supports document versioning.
    /// </summary>
    public bool SupportsVersioning { get; set; }

    /// <summary>
    /// Provider supports content deduplication.
    /// </summary>
    public bool SupportsDeduplication { get; set; }

    /// <summary>
    /// Provider supports soft delete.
    /// </summary>
    public bool SupportsSoftDelete { get; set; }

    /// <summary>
    /// Provider supports storage tiers (Hot, Cool, Archive).
    /// </summary>
    public bool SupportsStorageTiers { get; set; }

    /// <summary>
    /// Provider supports transactional batch operations.
    /// </summary>
    public bool SupportsTransactionalBatch { get; set; }

    /// <summary>
    /// Maximum document size supported (bytes).
    /// </summary>
    public long MaxDocumentSize { get; set; } = long.MaxValue;

    /// <summary>
    /// Supported storage tiers.
    /// </summary>
    public StorageTier[] SupportedTiers { get; set; } = Array.Empty<StorageTier>();
}
```

---

## Exception Types

```csharp
namespace OoBDev.System.Documents.Persistence;

/// <summary>
/// Exception thrown when document already exists.
/// </summary>
public class DocumentAlreadyExistsException : Exception
{
    public Guid DocumentId { get; }

    public DocumentAlreadyExistsException(Guid documentId)
        : base($"Document {documentId} already exists")
    {
        DocumentId = documentId;
    }
}

/// <summary>
/// Exception thrown when document persistence fails.
/// </summary>
public class DocumentPersistenceException : Exception
{
    public Guid? DocumentId { get; }

    public DocumentPersistenceException(string message, Exception? innerException = null, Guid? documentId = null)
        : base(message, innerException)
    {
        DocumentId = documentId;
    }
}

/// <summary>
/// Exception thrown when provider doesn't support requested operation.
/// </summary>
public class VersioningNotSupportedException : Exception
{
    public string ProviderName { get; }

    public VersioningNotSupportedException(string providerName)
        : base($"Provider '{providerName}' does not support versioning")
    {
        ProviderName = providerName;
    }
}

/// <summary>
/// Exception thrown when document exceeds size limits.
/// </summary>
public class DocumentTooLargeException : Exception
{
    public long DocumentSize { get; }
    public long MaxSize { get; }

    public DocumentTooLargeException(long documentSize, long maxSize)
        : base($"Document size {documentSize} bytes exceeds maximum {maxSize} bytes")
    {
        DocumentSize = documentSize;
        MaxSize = maxSize;
    }
}

/// <summary>
/// Exception thrown when deduplication fails.
/// </summary>
public class DeduplicationFailedException : Exception
{
    public string ContentHash { get; }

    public DeduplicationFailedException(string contentHash, Exception? innerException = null)
        : base($"Deduplication failed for content hash {contentHash}", innerException)
    {
        ContentHash = contentHash;
    }
}
```

---

## Usage Examples

### Example 1: Basic Document Creation

```csharp
using OoBDev.System.Documents.Persistence;

public class InvoiceService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task<Guid> SaveInvoicePdfAsync(byte[] pdfContent, string invoiceNumber)
    {
        var request = new DocumentCreateRequest
        {
            Name = $"Invoice_{invoiceNumber}.pdf",
            MediaType = "application/pdf",
            Content = pdfContent,
            Tags = new[] { "invoice", "pdf", invoiceNumber },
            Metadata = new Dictionary<string, object>
            {
                ["InvoiceNumber"] = invoiceNumber,
                ["GeneratedDate"] = DateTime.UtcNow
            }
        };

        // Basic creation with default context
        var documentId = await _persistence.CreateAsync(request);

        return documentId;
    }
}
```

---

### Example 2: Context-Based Persistence with Versioning

```csharp
public class ContractManagementService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task<Guid> CreateContractAsync(byte[] contractPdf, string contractNumber)
    {
        var request = new DocumentCreateRequest
        {
            Name = $"Contract_{contractNumber}.pdf",
            MediaType = "application/pdf",
            Content = contractPdf,
            Tags = new[] { "contract", "legal" },
            Metadata = new Dictionary<string, object>
            {
                ["ContractNumber"] = contractNumber,
                ["Status"] = "Draft"
            }
        };

        // Enable versioning and set retention policy
        var context = new PersistenceContext
        {
            RequestingApplication = "contract-management",
            UserId = "user123",
            EnableVersioning = true,  // Track all changes
            RetentionPeriod = TimeSpan.FromDays(2555),  // 7 years retention
            StorageTier = StorageTier.Hot,
            AdditionalContext = new Dictionary<string, object>
            {
                ["Department"] = "Legal",
                ["Encrypt"] = true
            }
        };

        return await _persistence.CreateAsync(request, context);
    }

    public async Task UpdateContractAsync(Guid documentId, byte[] updatedPdf, string changeDescription)
    {
        var request = new DocumentUpdateRequest
        {
            Content = updatedPdf,
            Metadata = new Dictionary<string, object>
            {
                ["Status"] = "Revised",
                ["LastModified"] = DateTime.UtcNow
            }
        };

        var context = new PersistenceContext
        {
            RequestingApplication = "contract-management",
            UserId = "user123",
            EnableVersioning = true,  // Creates new version
            ChangeDescription = changeDescription
        };

        await _persistence.UpdateAsync(documentId, request, context);
    }
}
```

---

### Example 3: Content Deduplication

```csharp
public class TemplateService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task<IEnumerable<Guid>> CreateBulkDocumentsWithTemplateAsync(
        byte[] templateContent,
        IEnumerable<string> customerNames)
    {
        // Enable deduplication - all documents share same template content
        var context = new PersistenceContext
        {
            RequestingApplication = "template-service",
            EnableDeduplication = true,  // Share content across documents
            StorageTier = StorageTier.Cool
        };

        var requests = customerNames.Select(name => new DocumentCreateRequest
        {
            Name = $"Document_{name}.pdf",
            MediaType = "application/pdf",
            Content = templateContent,  // SAME content for all
            Tags = new[] { "template", "bulk" },
            Metadata = new Dictionary<string, object>
            {
                ["CustomerName"] = name
            }
        });

        // All documents reference same content blob (storage savings)
        var documentIds = await _persistence.CreateBatchAsync(requests, context);

        _logger.LogInformation("Created {Count} documents with deduplicated content", documentIds.Count());

        return documentIds;
    }
}
```

---

### Example 4: Storage Tier Management

```csharp
public class ArchiveService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task<Guid> CreateArchiveDocumentAsync(byte[] content, string name)
    {
        var request = new DocumentCreateRequest
        {
            Name = name,
            MediaType = "application/pdf",
            Content = content,
            Tags = new[] { "archive" }
        };

        // Store directly in Archive tier (lowest cost)
        var context = new PersistenceContext
        {
            RequestingApplication = "archive-service",
            StorageTier = StorageTier.Archive,  // Cold storage
            RetentionPeriod = TimeSpan.FromDays(3650)  // 10 years
        };

        return await _persistence.CreateAsync(request, context);
    }

    public async Task MoveToArchiveAsync(Guid documentId)
    {
        // Move existing document to Archive tier
        var context = new PersistenceContext
        {
            RequestingApplication = "archive-service"
        };

        await _persistence.ChangeStorageTierAsync(documentId, StorageTier.Archive, context);

        _logger.LogInformation("Moved document {DocumentId} to Archive tier", documentId);
    }
}
```

---

### Example 5: Soft Delete and Restore

```csharp
public class DocumentLifecycleService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task DeleteDocumentSafelyAsync(Guid documentId)
    {
        // Soft delete (default) - document can be restored
        var context = new PersistenceContext
        {
            RequestingApplication = "lifecycle-service",
            UserId = "user123",
            SoftDelete = true  // Default
        };

        await _persistence.DeleteAsync(documentId, context);

        _logger.LogInformation("Soft deleted document {DocumentId}", documentId);
    }

    public async Task RestoreDocumentAsync(Guid documentId)
    {
        var context = new PersistenceContext
        {
            RequestingApplication = "lifecycle-service",
            UserId = "user123"
        };

        await _persistence.RestoreAsync(documentId, context);

        _logger.LogInformation("Restored document {DocumentId}", documentId);
    }

    public async Task PurgeOldDeletedDocumentsAsync()
    {
        // Permanently delete documents soft-deleted more than 30 days ago
        var context = new PersistenceContext
        {
            RequestingApplication = "lifecycle-service"
        };

        var purgedCount = await _persistence.PurgeDeletedAsync(TimeSpan.FromDays(30), context);

        _logger.LogInformation("Purged {Count} old deleted documents", purgedCount);
    }
}
```

---

### Example 6: Batch Operations

```csharp
public class BulkImportService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task<IEnumerable<Guid>> ImportDocumentsAsync(IEnumerable<(string Name, byte[] Content)> documents)
    {
        var requests = documents.Select(doc => new DocumentCreateRequest
        {
            Name = doc.Name,
            MediaType = "application/pdf",
            Content = doc.Content,
            Tags = new[] { "import", "bulk" }
        });

        var context = new PersistenceContext
        {
            RequestingApplication = "bulk-import",
            EnableDeduplication = true,
            StorageTier = StorageTier.Cool
        };

        // Create all documents in single batch operation
        var documentIds = await _persistence.CreateBatchAsync(requests, context);

        _logger.LogInformation("Imported {Count} documents", documentIds.Count());

        return documentIds;
    }

    public async Task BulkDeleteAsync(IEnumerable<Guid> documentIds)
    {
        var context = new PersistenceContext
        {
            RequestingApplication = "bulk-import",
            SoftDelete = true
        };

        await _persistence.DeleteBatchAsync(documentIds, context);

        _logger.LogInformation("Deleted {Count} documents", documentIds.Count());
    }
}
```

---

### Example 7: Hard Delete

```csharp
public class DataRetentionService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task PermanentlyDeleteDocumentAsync(Guid documentId)
    {
        var context = new PersistenceContext
        {
            RequestingApplication = "data-retention",
            UserId = "admin",
            SoftDelete = false  // Hard delete
        };

        // Permanent deletion - cannot be recovered
        await _persistence.DeleteAsync(documentId, context);

        // Or use explicit hard delete
        await _persistence.HardDeleteAsync(documentId, context);

        _logger.LogWarning("Permanently deleted document {DocumentId}", documentId);
    }
}
```

---

### Example 8: Partial Updates

```csharp
public class MetadataUpdateService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task UpdateDocumentMetadataAsync(Guid documentId, Dictionary<string, object> newMetadata)
    {
        // Update only metadata, leave content unchanged
        var request = new DocumentUpdateRequest
        {
            Metadata = newMetadata
            // Name, MediaType, Content, Tags = null (no change)
        };

        var context = new PersistenceContext
        {
            RequestingApplication = "metadata-service",
            EnableVersioning = false  // Don't create version for metadata-only update
        };

        await _persistence.UpdateAsync(documentId, request, context);
    }

    public async Task UpdateDocumentTagsAsync(Guid documentId, string[] newTags)
    {
        // Update only tags
        var request = new DocumentUpdateRequest
        {
            Tags = newTags
        };

        await _persistence.UpdateAsync(documentId, request);
    }
}
```

---

### Example 9: Provider-Specific Context

```csharp
public class OptimizedStorageService
{
    private readonly IDocumentPersistenceService _persistence;

    public async Task<Guid> CreateWithProviderOptimizationsAsync(byte[] content, string name)
    {
        var request = new DocumentCreateRequest
        {
            Name = name,
            MediaType = "application/pdf",
            Content = content
        };

        var context = new PersistenceContext
        {
            RequestingApplication = "optimized-storage",
            AdditionalContext = new Dictionary<string, object>
            {
                // Azure Blob-specific
                ["BlobAccessTier"] = "Cool",
                ["BlobMetadata"] = new Dictionary<string, string> { ["Custom"] = "Value" },

                // S3-specific
                ["StorageClass"] = "INTELLIGENT_TIERING",
                ["ServerSideEncryption"] = "AES256",

                // Database-specific
                ["Compress"] = true,
                ["FileGroup"] = "ARCHIVE_FG"
            }
        };

        return await _persistence.CreateAsync(request, context);
    }
}
```

---

## Dependency Injection Setup

### Service Registration

```csharp
using OoBDev.System.Documents.Persistence;
using Microsoft.Extensions.DependencyInjection;

public static class DocumentPersistenceServiceExtensions
{
    /// <summary>
    /// Adds document persistence service with providers.
    /// </summary>
    public static IServiceCollection AddDocumentPersistence(
        this IServiceCollection services,
        Action<DocumentPersistenceOptions>? configureOptions = null)
    {
        // Register service
        services.TryAddSingleton<IDocumentPersistenceService, DocumentPersistenceService>();

        // Register provider factory
        services.TryAddSingleton<IDocumentPersistenceProviderFactory, DocumentPersistenceProviderFactory>();

        // Register deduplication service
        services.TryAddSingleton<IContentDeduplicationService, ContentDeduplicationService>();

        // Register built-in providers
        services.TryAddSingleton<IDocumentPersistenceProvider, DatabasePersistenceProvider>();
        services.TryAddSingleton<IDocumentPersistenceProvider, AzureBlobPersistenceProvider>();
        services.TryAddSingleton<IDocumentPersistenceProvider, S3PersistenceProvider>();
        services.TryAddSingleton<IDocumentPersistenceProvider, FileSystemPersistenceProvider>();

        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        return services;
    }
}

/// <summary>
/// Configuration options for document persistence.
/// </summary>
public class DocumentPersistenceOptions
{
    /// <summary>
    /// Default provider name (e.g., "database", "azure-blob").
    /// </summary>
    public string DefaultProvider { get; set; } = "database";

    /// <summary>
    /// Failover provider when primary fails.
    /// </summary>
    public string? FailoverProvider { get; set; }

    /// <summary>
    /// Enable content deduplication globally.
    /// </summary>
    public bool EnableDeduplicationByDefault { get; set; } = true;

    /// <summary>
    /// Enable versioning globally.
    /// </summary>
    public bool EnableVersioningByDefault { get; set; } = false;

    /// <summary>
    /// Default storage tier.
    /// </summary>
    public StorageTier DefaultStorageTier { get; set; } = StorageTier.Hot;

    /// <summary>
    /// Provider-specific configuration.
    /// </summary>
    public IDictionary<string, object> ProviderConfiguration { get; set; } = new Dictionary<string, object>();
}
```

---

### Usage in Startup

```csharp
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDocumentPersistence(options =>
        {
            options.DefaultProvider = "azure-blob";
            options.FailoverProvider = "database";
            options.EnableDeduplicationByDefault = true;
            options.EnableVersioningByDefault = false;
            options.DefaultStorageTier = StorageTier.Hot;
            options.ProviderConfiguration = new Dictionary<string, object>
            {
                ["AzureBlobConnectionString"] = Configuration["Azure:BlobStorage:ConnectionString"],
                ["DatabaseConnectionString"] = Configuration["ConnectionStrings:Documents"],
                ["FileSystemBasePath"] = "/var/documents"
            };
        });
    }
}
```

---

## Best Practices

### 1. Use Deduplication for Templates
```csharp
// ✅ GOOD: Enable deduplication for repeated content
var context = new PersistenceContext { EnableDeduplication = true };
await _persistence.CreateAsync(request, context);

// ❌ BAD: Store duplicate content multiple times
var context = new PersistenceContext { EnableDeduplication = false };
```

### 2. Enable Versioning for Critical Documents
```csharp
// ✅ GOOD: Track changes for contracts and legal documents
var context = new PersistenceContext
{
    EnableVersioning = true,
    ChangeDescription = "Updated terms and conditions"
};

// ❌ BAD: No version history for important documents
var context = new PersistenceContext { EnableVersioning = false };
```

### 3. Use Appropriate Storage Tiers
```csharp
// ✅ GOOD: Archive tier for old documents
var context = new PersistenceContext
{
    StorageTier = StorageTier.Archive,  // Lowest cost
    RetentionPeriod = TimeSpan.FromDays(2555)
};

// ❌ BAD: Hot tier for rarely accessed documents (high cost)
var context = new PersistenceContext { StorageTier = StorageTier.Hot };
```

### 4. Use Soft Delete by Default
```csharp
// ✅ GOOD: Soft delete allows recovery
var context = new PersistenceContext { SoftDelete = true };
await _persistence.DeleteAsync(documentId, context);

// ❌ BAD: Hard delete with no way to recover
await _persistence.HardDeleteAsync(documentId);
```

---

## Performance Considerations

### Deduplication Overhead
| Operation | With Deduplication | Without Deduplication |
|-----------|-------------------|----------------------|
| Create (new content) | +20ms (hash calc) | baseline |
| Create (duplicate) | +10ms (hash lookup) | baseline + storage |
| Storage savings | 50-90% (for templates) | 0% |

**Recommendation:** Enable deduplication when storing similar content (templates, forms).

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [RetrievalService API](../RetrievalService/api-design.md)
