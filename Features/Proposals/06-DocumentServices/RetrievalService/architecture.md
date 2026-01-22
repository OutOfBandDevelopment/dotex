# Document Retrieval Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Retrieval Service
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Document Retrieval Service implements a **Provider Pattern** with **Context-Based Retrieval** for fetching documents from multiple storage backends. Applications provide operational context that providers use to optimize retrieval behavior.

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│         (Invoice Processor, Report Generator, etc.)          │
└────────────────────┬────────────────────────────────────────┘
                     │ GetAsync(id, context)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│            IDocumentRetrievalService                         │
│  - GetAsync(id, context)                                    │
│  - QueryAsync(query, context)                               │
│  - GetVersionAsync(id, version, context)                    │
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

### 1. DocumentRetrievalService (Main Entry Point)

**Responsibilities:**
- Coordinate document retrieval across providers
- Select appropriate provider based on context and document metadata
- Handle provider fallback on failure
- Aggregate results from multiple providers (for queries)

**Key Design Decisions:**
- **Multi-provider support** - Route requests to appropriate storage backend
- **Context propagation** - Pass operational context to all providers
- **Fallback strategy** - Try alternate providers on failure
- **Provider caching** - Cache provider instances for performance

**Implementation Pattern:**
```csharp
public class DocumentRetrievalService : IDocumentRetrievalService
{
    private readonly IDocumentRetrievalProviderFactory _providerFactory;
    private readonly DocumentRetrievalOptions _options;
    private readonly ILogger<DocumentRetrievalService> _logger;

    public async Task<Document> GetAsync(Guid documentId, RetrievalContext? context = null)
    {
        context ??= new RetrievalContext();

        // 1. Determine provider
        var provider = await SelectProviderAsync(documentId, context);

        try
        {
            // 2. Retrieve document
            _logger.LogDebug("Retrieving document {DocumentId} using provider {Provider}",
                documentId, provider.ProviderName);

            var document = await provider.GetAsync(documentId, context);

            _logger.LogInformation("Retrieved document {DocumentId} (size: {Size} bytes)",
                documentId, document.Size);

            return document;
        }
        catch (DocumentNotFoundException)
        {
            throw;  // Document not found - don't retry
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve document {DocumentId} from provider {Provider}",
                documentId, provider.ProviderName);

            // Try fallback provider
            if (_options.FallbackProvider != null)
            {
                var fallbackProvider = await _providerFactory.GetProviderAsync(_options.FallbackProvider);
                return await fallbackProvider.GetAsync(documentId, context);
            }

            throw new DocumentRetrievalException($"Failed to retrieve document {documentId}", ex);
        }
    }

    private async Task<IDocumentRetrievalProvider> SelectProviderAsync(
        Guid documentId,
        RetrievalContext context)
    {
        // 1. Check context for preferred provider
        if (context.AdditionalContext.TryGetValue("PreferredProvider", out var preferredProvider))
        {
            return await _providerFactory.GetProviderAsync(preferredProvider.ToString()!);
        }

        // 2. Check document metadata (if available from index)
        var metadata = await TryGetDocumentMetadataAsync(documentId);
        if (metadata?.TryGetValue("StorageProvider", out var storedProvider) == true)
        {
            return await _providerFactory.GetProviderAsync(storedProvider.ToString()!);
        }

        // 3. Use default provider
        return await _providerFactory.GetProviderAsync(_options.DefaultProvider);
    }

    public async Task<IEnumerable<Document>> QueryAsync(
        DocumentQuery query,
        RetrievalContext? context = null)
    {
        context ??= new RetrievalContext();

        var provider = await SelectProviderForQueryAsync(query, context);

        _logger.LogDebug("Querying documents using provider {Provider}", provider.ProviderName);

        var results = await provider.QueryAsync(query, context);

        _logger.LogInformation("Query returned {Count} documents", results.Count());

        return results;
    }
}
```

---

### 2. Provider Interfaces

**IDocumentRetrievalProvider:**
```csharp
public interface IDocumentRetrievalProvider
{
    /// <summary>
    /// Provider name (e.g., "azure-blob", "sql-server", "file-system")
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Provider capabilities (versioning, full-text search, etc.)
    /// </summary>
    DocumentStorageCapabilities Capabilities { get; }

    /// <summary>
    /// Retrieves document by ID with context.
    /// </summary>
    Task<Document> GetAsync(Guid documentId, RetrievalContext context);

    /// <summary>
    /// Retrieves specific version of document.
    /// </summary>
    Task<Document> GetVersionAsync(Guid documentId, int version, RetrievalContext context);

    /// <summary>
    /// Queries documents matching criteria.
    /// </summary>
    Task<IEnumerable<Document>> QueryAsync(DocumentQuery query, RetrievalContext context);

    /// <summary>
    /// Checks if document exists.
    /// </summary>
    Task<bool> ExistsAsync(Guid documentId, RetrievalContext context);

    /// <summary>
    /// Gets all versions of document.
    /// </summary>
    Task<IEnumerable<DocumentVersion>> GetVersionsAsync(Guid documentId, RetrievalContext context);
}
```

---

### 3. Provider Implementations

#### DatabaseRetrievalProvider (SQL Server, PostgreSQL)

**Responsibilities:**
- Retrieve documents from relational database
- Support metadata queries and full-text search
- Handle versioning (if enabled)

**Implementation Pattern:**
```csharp
public class DatabaseRetrievalProvider : IDocumentRetrievalProvider
{
    private readonly IDbConnection _connection;
    private readonly ILogger<DatabaseRetrievalProvider> _logger;

    public string ProviderName => "database";

    public DocumentStorageCapabilities Capabilities => new()
    {
        SupportsVersioning = true,
        SupportsFullTextSearch = true,
        SupportsTagging = true,
        SupportsMetadataQuery = true,
        MaxDocumentSize = 100 * 1024 * 1024  // 100MB
    };

    public async Task<Document> GetAsync(Guid documentId, RetrievalContext context)
    {
        var sql = context.IncludeContent
            ? "SELECT * FROM Documents WHERE Id = @Id"
            : "SELECT Id, Name, MediaType, Size, Metadata, CreatedDate, ModifiedDate, Version FROM Documents WHERE Id = @Id";

        var document = await _connection.QuerySingleOrDefaultAsync<Document>(sql, new { Id = documentId });

        if (document == null)
        {
            throw new DocumentNotFoundException($"Document {documentId} not found");
        }

        // Log retrieval for audit
        await LogRetrievalAsync(documentId, context);

        return document;
    }

    public async Task<IEnumerable<Document>> QueryAsync(DocumentQuery query, RetrievalContext context)
    {
        // Build dynamic SQL based on query
        var (sql, parameters) = BuildQuerySql(query, context);

        var documents = await _connection.QueryAsync<Document>(sql, parameters);

        return documents;
    }

    public async Task<IEnumerable<DocumentVersion>> GetVersionsAsync(Guid documentId, RetrievalContext context)
    {
        var sql = @"
            SELECT Version, CreatedDate, CreatedBy, Size, ChangeDescription
            FROM DocumentVersions
            WHERE DocumentId = @DocumentId
            ORDER BY Version DESC";

        var versions = await _connection.QueryAsync<DocumentVersion>(sql, new { DocumentId = documentId });

        return versions;
    }
}
```

---

#### AzureBlobRetrievalProvider

**Responsibilities:**
- Retrieve documents from Azure Blob Storage
- Support blob metadata and tags
- Handle large files efficiently

**Implementation Pattern:**
```csharp
public class AzureBlobRetrievalProvider : IDocumentRetrievalProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<AzureBlobRetrievalProvider> _logger;

    public string ProviderName => "azure-blob";

    public DocumentStorageCapabilities Capabilities => new()
    {
        SupportsVersioning = true,  // Blob versioning
        SupportsFullTextSearch = false,
        SupportsTagging = true,
        SupportsMetadataQuery = true,
        MaxDocumentSize = 5L * 1024 * 1024 * 1024  // 5GB
    };

    public async Task<Document> GetAsync(Guid documentId, RetrievalContext context)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(context));
        var blobClient = containerClient.GetBlobClient(documentId.ToString());

        // Check if blob exists
        if (!await blobClient.ExistsAsync())
        {
            throw new DocumentNotFoundException($"Document {documentId} not found in Azure Blob Storage");
        }

        // Download blob properties (metadata)
        var properties = await blobClient.GetPropertiesAsync();

        var document = new Document
        {
            Id = documentId,
            Name = properties.Value.Metadata.TryGetValue("Name", out var name) ? name : documentId.ToString(),
            MediaType = properties.Value.ContentType,
            Size = properties.Value.ContentLength,
            CreatedDate = properties.Value.CreatedOn.DateTime,
            ModifiedDate = properties.Value.LastModified.DateTime,
            Metadata = properties.Value.Metadata.ToDictionary(kvp => kvp.Key, kvp => (object)kvp.Value)
        };

        // Download content if requested
        if (context.IncludeContent)
        {
            using var memoryStream = new MemoryStream();
            await blobClient.DownloadToAsync(memoryStream);
            document.Content = memoryStream.ToArray();
        }

        _logger.LogDebug("Retrieved document {DocumentId} from Azure Blob (size: {Size} bytes)",
            documentId, document.Size);

        return document;
    }

    public async Task<IEnumerable<Document>> QueryAsync(DocumentQuery query, RetrievalContext context)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(GetContainerName(context));

        // Build tag filter if tags specified
        var tagFilter = BuildTagFilter(query);

        var documents = new List<Document>();

        // Query blobs using tags
        if (!string.IsNullOrEmpty(tagFilter))
        {
            await foreach (var taggedBlob in _blobServiceClient.FindBlobsByTagsAsync(tagFilter))
            {
                var blobClient = containerClient.GetBlobClient(taggedBlob.BlobName);
                var document = await GetDocumentFromBlobAsync(blobClient, context);
                documents.Add(document);
            }
        }
        else
        {
            // List all blobs (less efficient)
            await foreach (var blobItem in containerClient.GetBlobsAsync())
            {
                if (MatchesQuery(blobItem, query))
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    var document = await GetDocumentFromBlobAsync(blobClient, context);
                    documents.Add(document);
                }
            }
        }

        return documents;
    }

    private string GetContainerName(RetrievalContext context)
    {
        // Use requesting application as container name (or default)
        return context.RequestingApplication?.ToLowerInvariant() ?? "documents";
    }
}
```

---

#### FileSystemRetrievalProvider

**Responsibilities:**
- Retrieve documents from file system
- Support directory-based organization
- Handle file metadata

**Implementation Pattern:**
```csharp
public class FileSystemRetrievalProvider : IDocumentRetrievalProvider
{
    private readonly string _basePath;
    private readonly ILogger<FileSystemRetrievalProvider> _logger;

    public string ProviderName => "file-system";

    public DocumentStorageCapabilities Capabilities => new()
    {
        SupportsVersioning = false,  // No built-in versioning
        SupportsFullTextSearch = false,
        SupportsTagging = false,
        SupportsMetadataQuery = false,
        MaxDocumentSize = long.MaxValue
    };

    public async Task<Document> GetAsync(Guid documentId, RetrievalContext context)
    {
        var filePath = GetFilePath(documentId, context);

        if (!File.Exists(filePath))
        {
            throw new DocumentNotFoundException($"Document {documentId} not found at {filePath}");
        }

        var fileInfo = new FileInfo(filePath);

        var document = new Document
        {
            Id = documentId,
            Name = fileInfo.Name,
            MediaType = GetMediaType(fileInfo.Extension),
            Size = fileInfo.Length,
            CreatedDate = fileInfo.CreationTimeUtc,
            ModifiedDate = fileInfo.LastWriteTimeUtc
        };

        // Load metadata from sidecar file
        var metadataPath = filePath + ".metadata.json";
        if (File.Exists(metadataPath))
        {
            var metadataJson = await File.ReadAllTextAsync(metadataPath);
            document.Metadata = JsonSerializer.Deserialize<Dictionary<string, object>>(metadataJson) ?? [];
        }

        // Load content if requested
        if (context.IncludeContent)
        {
            document.Content = await File.ReadAllBytesAsync(filePath);
        }

        return document;
    }

    private string GetFilePath(Guid documentId, RetrievalContext context)
    {
        // Organize by subdirectories (first 2 chars of GUID to avoid single dir with millions of files)
        var guidString = documentId.ToString("N");
        var subDir = guidString.Substring(0, 2);

        return Path.Combine(_basePath, subDir, guidString);
    }
}
```

---

## Data Flow

### Sequence: Document Retrieval with Context

```
┌───────────┐      ┌──────────────────┐      ┌──────────┐      ┌──────────┐
│Application│      │RetrievalService  │      │ Provider │      │  Storage │
└─────┬─────┘      └────────┬─────────┘      └─────┬────┘      └─────┬────┘
      │                     │                       │                  │
      │ GetAsync(id, ctx)   │                       │                  │
      ├────────────────────>│                       │                  │
      │                     │                       │                  │
      │                     │ SelectProvider(ctx)   │                  │
      │                     ├──────────────────────>│                  │
      │                     │                       │                  │
      │                     │ Provider instance     │                  │
      │                     │<──────────────────────┤                  │
      │                     │                       │                  │
      │                     │ GetAsync(id, ctx)     │                  │
      │                     ├──────────────────────>│                  │
      │                     │                       │                  │
      │                     │                       │ Load document    │
      │                     │                       ├─────────────────>│
      │                     │                       │                  │
      │                     │                       │ Document data    │
      │                     │                       │<─────────────────┤
      │                     │                       │                  │
      │                     │ Document              │                  │
      │                     │<──────────────────────┤                  │
      │                     │                       │                  │
      │ Document            │                       │                  │
      │<────────────────────┤                       │                  │
      │                     │                       │                  │
```

**Key Points:**
1. Application provides retrieval context
2. Service selects appropriate provider
3. Provider uses context to optimize retrieval
4. Document returned with metadata and optionally content

---

## Design Patterns

### 1. Provider Pattern
- Multiple storage backends via pluggable providers
- Provider selection based on context and metadata
- Fallback providers for resilience

### 2. Context Pattern
- Operational context passed through call chain
- Providers use context to adjust behavior
- Context includes user ID, app name, flags, custom properties

### 3. Factory Pattern
- Provider factory creates provider instances
- Provider caching for performance
- Provider configuration via dependency injection

### 4. Strategy Pattern
- Different retrieval strategies based on provider capabilities
- Metadata-only vs full content retrieval
- Query optimization per provider

---

## Performance Optimizations

### 1. Metadata-Only Retrieval
- Skip content loading when `IncludeContent = false`
- 10-100x faster for listing/searching
- Minimal memory usage

### 2. Provider Caching
- Cache provider instances (not documents)
- Reuse connections and clients
- Provider lifetime: Singleton or Scoped

### 3. Lazy Loading
- Load content on-demand
- Stream large files (provider-specific API)
- Pagination for large query results

### 4. Parallel Queries
- Query multiple providers concurrently (advanced scenario)
- Aggregate results from different storages
- Use `Task.WhenAll` for parallel execution

---

## Error Handling

### Provider Fallback Strategy
```csharp
public async Task<Document> GetAsync(Guid documentId, RetrievalContext? context = null)
{
    var provider = await SelectProviderAsync(documentId, context);

    try
    {
        return await provider.GetAsync(documentId, context);
    }
    catch (DocumentNotFoundException)
    {
        throw;  // Don't retry for not found
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Provider {Provider} failed, trying fallback", provider.ProviderName);

        if (_options.FallbackProvider != null)
        {
            var fallbackProvider = await _providerFactory.GetProviderAsync(_options.FallbackProvider);
            return await fallbackProvider.GetAsync(documentId, context);
        }

        throw new DocumentRetrievalException($"Failed to retrieve document {documentId}", ex);
    }
}
```

---

## Thread Safety

### Concurrency Strategy
- Providers must be thread-safe
- Service is thread-safe (stateless or properly synchronized)
- Multiple threads can retrieve different documents concurrently
- Same document can be retrieved by multiple threads (providers handle locking if needed)

---

## Testing Strategy

### Unit Tests
- Mock providers for service tests
- Test provider selection logic
- Test fallback behavior
- Test context propagation

### Integration Tests
- Real providers with test storage
- End-to-end retrieval scenarios
- Performance benchmarks
- Concurrent access tests

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [PersistenceService Architecture](../PersistenceService/architecture.md)
