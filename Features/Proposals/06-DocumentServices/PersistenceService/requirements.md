# Document Persistence Service - Requirements

**Epic:** 6 - Document Services
**Feature:** Document Persistence Service
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~280

---

## Overview

Context-based document persistence service with provider pattern for multiple storage backends (database, file system, Azure Blob, S3, etc.). Applications provide operational context to influence storage behavior including versioning, deduplication, and retention policies.

---

## Business Requirements

### BR-1: Document Creation with Context
**As a** developer
**I want** to create/store new documents with operational context
**So that** documents are stored appropriately based on application requirements

**Acceptance Criteria:**
- Create document with unique GUID identifier
- Context includes requesting application, user ID, and custom metadata
- Providers use context to determine storage location and options
- Returns document ID and storage confirmation
- Supports both small and large document uploads

---

### BR-2: Document Update with Versioning
**As a** developer
**I want** to update existing documents with optional versioning
**So that** I can track document history and changes over time

**Acceptance Criteria:**
- Update document content and/or metadata
- Optional versioning (provider-dependent)
- Version history preserved when enabled
- Update creates new version with incrementing version number
- Context includes change description and modifier info

---

### BR-3: Document Deletion with Soft Delete
**As a** developer
**I want** to delete documents with soft delete support
**So that** documents can be recovered if deleted accidentally

**Acceptance Criteria:**
- Hard delete permanently removes document
- Soft delete marks document as deleted but retains data
- Soft-deleted documents excluded from queries by default
- Can restore soft-deleted documents
- Purge operation permanently removes soft-deleted documents

**Deletion Types:**
```
- Hard Delete: Immediate permanent removal
- Soft Delete: Mark as deleted, retains data (default)
- Scheduled Delete: Delete after retention period
- Purge: Remove all soft-deleted documents
```

---

### BR-4: Multi-Provider Support
**As a** system architect
**I want** pluggable storage providers
**So that** documents can be stored in different backends based on requirements

**Acceptance Criteria:**
- Provider pattern for storage backends
- Built-in providers: Database, File System, Azure Blob, S3
- Provider selection based on document metadata or context
- Automatic failover to backup provider if primary fails
- Provider registration via dependency injection

**Supported Providers:**
```
- Database (SQL Server, PostgreSQL, MongoDB)
- File System (local, network share)
- Azure Blob Storage (Hot, Cool, Archive tiers)
- AWS S3 (Standard, IA, Glacier)
- Google Cloud Storage
- Custom providers via IDocumentPersistenceProvider
```

---

### BR-5: Context-Based Persistence
**As a** application developer
**I want** to provide operational context during persistence
**So that** providers can adjust behavior based on use case

**Acceptance Criteria:**
- Context includes requesting application name
- Context includes user ID for auditing
- Context flags: EnableVersioning, EnableDeduplication, RetentionPeriod
- Context includes storage tier preferences (Hot, Cool, Archive)
- Providers use context to optimize storage

**Example Context:**
```csharp
var context = new PersistenceContext
{
    RequestingApplication = "invoice-processor",
    UserId = "user123",
    EnableVersioning = true,
    EnableDeduplication = false,
    RetentionPeriod = TimeSpan.FromDays(2555),  // 7 years
    StorageTier = StorageTier.Hot,
    AdditionalContext = new Dictionary<string, object>
    {
        ["Compress"] = true,
        ["Encrypt"] = true,
        ["ReplicationRegions"] = new[] { "us-east-1", "eu-west-1" }
    }
};
```

---

### BR-6: Content Deduplication
**As a** developer
**I want** automatic content deduplication
**So that** I don't store duplicate content multiple times

**Acceptance Criteria:**
- Content-based deduplication using hash (SHA256)
- Multiple documents can reference same content
- Deduplication configurable via context
- Reference counting for shared content
- Content deleted only when all references removed
- Provider-dependent feature (check capabilities)

---

### BR-7: Storage Tier Management
**As a** developer
**I want** to specify storage tiers for documents
**So that** I can optimize cost vs. access speed

**Acceptance Criteria:**
- Support Hot, Cool, Archive tiers
- Context specifies preferred tier
- Auto-tier migration based on access patterns (optional)
- Provider-specific tier implementations
- Cost optimization for infrequently accessed documents

**Storage Tiers:**
```
- Hot: Frequent access, low latency, higher cost
- Cool: Infrequent access (30+ days), medium cost
- Archive: Rare access (90+ days), lowest cost, higher retrieval latency
```

---

### BR-8: Batch Operations
**As a** developer
**I want** to perform batch create/update/delete operations
**So that** I can efficiently process multiple documents

**Acceptance Criteria:**
- Batch create multiple documents in single call
- Batch update multiple documents
- Batch delete multiple documents
- Transactional support (all-or-nothing) when provider supports
- Progress reporting for long-running batches
- Error handling preserves partial success information

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IDocumentPersistenceService
{
    /// <summary>
    /// Creates new document with content and metadata.
    /// </summary>
    Task<Guid> CreateAsync(DocumentCreateRequest request, PersistenceContext? context = null);

    /// <summary>
    /// Updates existing document content and/or metadata.
    /// </summary>
    Task UpdateAsync(Guid documentId, DocumentUpdateRequest request, PersistenceContext? context = null);

    /// <summary>
    /// Deletes document (soft delete by default).
    /// </summary>
    Task DeleteAsync(Guid documentId, PersistenceContext? context = null);

    /// <summary>
    /// Hard delete permanently removes document.
    /// </summary>
    Task HardDeleteAsync(Guid documentId, PersistenceContext? context = null);

    /// <summary>
    /// Restores soft-deleted document.
    /// </summary>
    Task RestoreAsync(Guid documentId, PersistenceContext? context = null);

    /// <summary>
    /// Purges all soft-deleted documents older than specified age.
    /// </summary>
    Task PurgeDeletedAsync(TimeSpan olderThan, PersistenceContext? context = null);

    /// <summary>
    /// Batch creates multiple documents.
    /// </summary>
    Task<IEnumerable<Guid>> CreateBatchAsync(IEnumerable<DocumentCreateRequest> requests, PersistenceContext? context = null);

    /// <summary>
    /// Batch deletes multiple documents.
    /// </summary>
    Task DeleteBatchAsync(IEnumerable<Guid> documentIds, PersistenceContext? context = null);

    /// <summary>
    /// Changes storage tier for document.
    /// </summary>
    Task ChangeStorageTierAsync(Guid documentId, StorageTier tier, PersistenceContext? context = null);
}

public class PersistenceContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public bool EnableVersioning { get; set; } = false;
    public bool EnableDeduplication { get; set; } = true;
    public TimeSpan? RetentionPeriod { get; set; }
    public StorageTier StorageTier { get; set; } = StorageTier.Hot;
    public bool SoftDelete { get; set; } = true;  // Use soft delete by default
    public string? ChangeDescription { get; set; }
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}

public class DocumentCreateRequest
{
    public string Name { get; set; } = "";
    public string MediaType { get; set; } = "";
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public string[] Tags { get; set; } = Array.Empty<string>();
}

public class DocumentUpdateRequest
{
    public string? Name { get; set; }
    public string? MediaType { get; set; }
    public byte[]? Content { get; set; }
    public IDictionary<string, object>? Metadata { get; set; }
    public string[]? Tags { get; set; }
}

public enum StorageTier
{
    Hot,      // Frequent access, low latency
    Cool,     // Infrequent access (30+ days)
    Archive   // Rare access (90+ days)
}
```

---

### TR-2: Provider Interface
```csharp
public interface IDocumentPersistenceProvider
{
    string ProviderName { get; }
    DocumentStorageCapabilities Capabilities { get; }

    Task<Guid> CreateAsync(DocumentCreateRequest request, PersistenceContext context);
    Task UpdateAsync(Guid documentId, DocumentUpdateRequest request, PersistenceContext context);
    Task DeleteAsync(Guid documentId, PersistenceContext context);
    Task RestoreAsync(Guid documentId, PersistenceContext context);
    Task PurgeDeletedAsync(TimeSpan olderThan, PersistenceContext context);
    Task<IEnumerable<Guid>> CreateBatchAsync(IEnumerable<DocumentCreateRequest> requests, PersistenceContext context);
    Task ChangeStorageTierAsync(Guid documentId, StorageTier tier, PersistenceContext context);
}

public class DocumentStorageCapabilities
{
    public bool SupportsVersioning { get; set; }
    public bool SupportsDeduplication { get; set; }
    public bool SupportsSoftDelete { get; set; }
    public bool SupportsStorageTiers { get; set; }
    public bool SupportsTransactionalBatch { get; set; }
    public long MaxDocumentSize { get; set; } = long.MaxValue;
    public StorageTier[] SupportedTiers { get; set; } = Array.Empty<StorageTier>();
}
```

---

### TR-3: Versioning Strategy
**Version creation logic:**
1. Check if versioning enabled in context
2. Check if provider supports versioning
3. Create new version with incremented version number
4. Preserve previous version(s) based on retention policy
5. Store version metadata (created date, created by, change description)

**Example:**
```csharp
// Context enables versioning
context.EnableVersioning = true;
context.ChangeDescription = "Updated invoice amount";

// Update creates version 2, preserves version 1
await _persistence.UpdateAsync(documentId, updateRequest, context);
```

---

### TR-4: Deduplication Strategy
**Content deduplication logic:**
1. Calculate SHA256 hash of content
2. Check if content with same hash exists
3. If exists, create document reference to existing content
4. If not exists, store new content blob
5. Increment reference count on shared content
6. On delete, decrement reference count, delete content when count = 0

**Example:**
```csharp
// Document A: Content "Hello World" (SHA256: abc123...)
await _persistence.CreateAsync(docA);  // Stores content blob

// Document B: Same content "Hello World" (SHA256: abc123...)
await _persistence.CreateAsync(docB);  // References existing blob, doesn't duplicate

// Delete Document A: Decrement reference count (now 1)
await _persistence.DeleteAsync(docA.Id);

// Delete Document B: Decrement reference count (now 0), delete content blob
await _persistence.DeleteAsync(docB.Id);
```

---

### TR-5: Performance Requirements
- **Single document create:** < 200ms (excluding network/disk I/O)
- **Single document update:** < 150ms
- **Single document delete:** < 100ms
- **Batch operations:** 100+ documents per second
- **Concurrent writes:** 50+ simultaneous requests

---

### TR-6: Error Handling
**Exception Types:**
```csharp
public class DocumentAlreadyExistsException : Exception { }
public class DocumentPersistenceException : Exception { }
public class ProviderNotAvailableException : Exception { }
public class VersioningNotSupportedException : Exception { }
public class DocumentTooLargeException : Exception { }
public class DeduplicationFailedException : Exception { }
```

**Error Scenarios:**
- Document ID collision → Retry with new GUID or throw `DocumentAlreadyExistsException`
- Provider unavailable → Try failover provider or throw `ProviderNotAvailableException`
- Versioning on non-versioning provider → Throw `VersioningNotSupportedException`
- Content exceeds max size → Throw `DocumentTooLargeException` with size details
- Deduplication hash collision (rare) → Log warning, store as unique content

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Supports async/await patterns
- Compatible with dependency injection
- Provider pattern for extensibility

### NFR-2: Scalability
- Handle documents up to 5GB (streaming for large files)
- Support 100,000+ documents in storage
- Concurrent writes to different documents
- Multi-tenant support (tenant ID in context)

### NFR-3: Durability
- Writes confirmed only after data persisted
- Provider ensures data durability (replication, redundancy)
- Transaction support where available
- Automatic retry on transient failures

### NFR-4: Security
- User ID in context for auditing
- Encryption at rest (provider responsibility)
- Encryption in transit (HTTPS/TLS)
- Access control at provider level
- Secure connection strings in configuration

### NFR-5: Testability
- Mock providers for unit testing
- In-memory provider for integration tests
- Deterministic behavior for testing
- Performance metrics trackable

---

## Constraints

### C-1: Provider Constraints
- Providers must be thread-safe
- Provider exceptions propagate to caller
- Not all providers support all capabilities
- Provider selection impacts available features

### C-2: Content Size Constraints
- Documents < 10MB: Store directly in provider
- Documents 10MB - 100MB: Use chunked upload
- Documents > 100MB: Use multipart upload (provider-specific)
- Maximum size: 5GB (configurable per provider)

### C-3: Versioning Constraints
- Version retention policy configurable
- Old versions can be purged based on age or count
- Versioning increases storage costs
- Not all providers support versioning

### C-4: Deduplication Constraints
- Hash calculation overhead for large files
- Deduplication works only within same provider
- Reference counting requires coordination
- Deduplication can be disabled per context

---

## Success Criteria

- ✅ Create documents with context-based behavior
- ✅ Update documents with optional versioning
- ✅ Delete documents with soft delete support
- ✅ Multiple storage providers (Database, File System, Azure Blob, S3)
- ✅ Content deduplication for storage efficiency
- ✅ Storage tier management (Hot, Cool, Archive)
- ✅ Batch operations for bulk processing
- ✅ 80%+ test coverage
- ✅ Performance: < 200ms document create

---

## Out of Scope

- ❌ Document transformation during persistence (use ConversionService)
- ❌ Document validation (use separate validation service)
- ❌ Full-text indexing (use dedicated search service)
- ❌ Access control enforcement (use authorization middleware)
- ❌ Content streaming during upload (use provider-specific APIs)

---

## Dependencies

### Internal
- OoBDev.System.Documents.Abstractions (Document model)
- OoBDev.System.Documents.Providers (provider abstractions)
- OoBDev.System.Documents.Retrieval (for deduplication checks)

### External
- .NET 10.0 BCL
- System.Security.Cryptography (for SHA256 hashing)
- Azure.Storage.Blobs (Azure Blob provider)
- AWSSDK.S3 (S3 provider)
- MongoDB.Driver (MongoDB provider - optional)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [RetrievalService Requirements](../RetrievalService/requirements.md)
- [Epic 6 Overview](../README.md)
- [REVISIONS_SUMMARY - Revision 10](../../REVISIONS_SUMMARY.md#revision-10-comprehensive-document-services-context-based)
