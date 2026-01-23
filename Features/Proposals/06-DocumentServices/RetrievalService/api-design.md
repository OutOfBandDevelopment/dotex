# Document Retrieval Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Retrieval Service
**Last Updated:** 2026-01-22

---

## API Overview

The Document Retrieval Service API provides context-based document retrieval with support for multiple storage providers (database, file system, Azure Blob, S3, etc.).

---

## Core Interfaces

### IDocumentRetrievalService

**Purpose:** Main service interface for retrieving documents from storage with operational context.

```csharp
namespace OoBDev.System.Documents.Retrieval;

/// <summary>
/// Service for retrieving documents from storage with context-based behavior.
/// </summary>
public interface IDocumentRetrievalService
{
    /// <summary>
    /// Retrieves document by ID with optional context.
    /// </summary>
    /// <param name="documentId">Unique document identifier</param>
    /// <param name="context">Optional retrieval context (requesting app, user, flags)</param>
    /// <returns>Document with metadata and optionally content</returns>
    /// <exception cref="DocumentNotFoundException">Document not found</exception>
    /// <exception cref="DocumentRetrievalException">Retrieval failed</exception>
    Task<Document> GetAsync(Guid documentId, RetrievalContext? context = null);

    /// <summary>
    /// Retrieves specific version of document.
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="version">Version number (1 = original)</param>
    /// <param name="context">Optional retrieval context</param>
    /// <returns>Document at specified version</returns>
    /// <exception cref="VersionNotSupportedException">Provider doesn't support versioning</exception>
    Task<Document> GetVersionAsync(Guid documentId, int version, RetrievalContext? context = null);

    /// <summary>
    /// Queries documents matching criteria with pagination.
    /// </summary>
    /// <param name="query">Query filters and options</param>
    /// <param name="context">Optional retrieval context</param>
    /// <returns>Documents matching query criteria</returns>
    Task<IEnumerable<Document>> QueryAsync(DocumentQuery query, RetrievalContext? context = null);

    /// <summary>
    /// Checks if document exists without loading content.
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="context">Optional retrieval context</param>
    /// <returns>True if document exists</returns>
    Task<bool> ExistsAsync(Guid documentId, RetrievalContext? context = null);

    /// <summary>
    /// Gets all versions of document (if provider supports versioning).
    /// </summary>
    /// <param name="documentId">Document identifier</param>
    /// <param name="context">Optional retrieval context</param>
    /// <returns>List of document versions</returns>
    /// <exception cref="VersionNotSupportedException">Provider doesn't support versioning</exception>
    Task<IEnumerable<DocumentVersion>> GetVersionsAsync(Guid documentId, RetrievalContext? context = null);
}
```

---

### Context Classes

**RetrievalContext:**
```csharp
namespace OoBDev.System.Documents.Retrieval;

/// <summary>
/// Operational context for document retrieval.
/// Providers use context to adjust retrieval behavior.
/// </summary>
public class RetrievalContext
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
    /// Include document metadata (default: true).
    /// Set false to skip metadata loading (minimal retrieval).
    /// </summary>
    public bool IncludeMetadata { get; set; } = true;

    /// <summary>
    /// Include document content (default: true).
    /// Set false for metadata-only queries (10-100x faster).
    /// </summary>
    public bool IncludeContent { get; set; } = true;

    /// <summary>
    /// Additional context properties for provider-specific behavior.
    /// Examples: PreferredProvider, CacheResults, PreferredRegion
    /// </summary>
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

---

### Document Model

**Document:**
```csharp
namespace OoBDev.System.Documents;

/// <summary>
/// Represents a document with metadata and optional content.
/// </summary>
public class Document
{
    /// <summary>
    /// Unique document identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Document name (filename or title).
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// Media type (MIME type) - e.g., "application/pdf", "image/png".
    /// </summary>
    public string MediaType { get; set; } = "";

    /// <summary>
    /// Document content bytes (null when IncludeContent = false).
    /// </summary>
    public byte[]? Content { get; set; }

    /// <summary>
    /// Document size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Document metadata (tags, custom properties, etc.).
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Document creation date (UTC).
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Document last modified date (UTC).
    /// </summary>
    public DateTime ModifiedDate { get; set; }

    /// <summary>
    /// Document version (1 = original, increments on updates).
    /// </summary>
    public int Version { get; set; } = 1;

    /// <summary>
    /// User/system that created the document.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// User/system that last modified the document.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Document tags for categorization and search.
    /// </summary>
    public string[] Tags { get; set; } = [];
}
```

---

### Query Model

**DocumentQuery:**
```csharp
namespace OoBDev.System.Documents.Retrieval;

/// <summary>
/// Query criteria for searching documents.
/// </summary>
public class DocumentQuery
{
    /// <summary>
    /// Filter by media type (e.g., "application/pdf").
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Filter by document name (supports wildcards if provider allows).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Filter by tags (AND logic - document must have all tags).
    /// </summary>
    public string[]? Tags { get; set; }

    /// <summary>
    /// Filter documents created after this date (UTC).
    /// </summary>
    public DateTime? CreatedAfter { get; set; }

    /// <summary>
    /// Filter documents created before this date (UTC).
    /// </summary>
    public DateTime? CreatedBefore { get; set; }

    /// <summary>
    /// Filter by creator user ID.
    /// </summary>
    public string? CreatedBy { get; set; }

    /// <summary>
    /// Filter by modifier user ID.
    /// </summary>
    public string? ModifiedBy { get; set; }

    /// <summary>
    /// Page size for pagination (default: 100, max: 1000).
    /// </summary>
    public int? PageSize { get; set; } = 100;

    /// <summary>
    /// Page number (1-indexed).
    /// </summary>
    public int? PageNumber { get; set; } = 1;

    /// <summary>
    /// Sort field (e.g., "CreatedDate", "Name", "Size").
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort direction (true = ascending, false = descending).
    /// </summary>
    public bool SortAscending { get; set; } = true;

    /// <summary>
    /// Custom filters for provider-specific queries.
    /// Example: { ["ContentContains"] = "invoice", ["MinSize"] = 1024 }
    /// </summary>
    public IDictionary<string, object> CustomFilters { get; set; } = new Dictionary<string, object>();
}
```

---

### Version Model

**DocumentVersion:**
```csharp
namespace OoBDev.System.Documents;

/// <summary>
/// Represents a specific version of a document.
/// </summary>
public class DocumentVersion
{
    /// <summary>
    /// Document ID.
    /// </summary>
    public Guid DocumentId { get; set; }

    /// <summary>
    /// Version number (1 = original).
    /// </summary>
    public int Version { get; set; }

    /// <summary>
    /// Version creation date (UTC).
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// User who created this version.
    /// </summary>
    public string CreatedBy { get; set; } = "";

    /// <summary>
    /// Version size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Description of changes in this version.
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Version-specific metadata.
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

---

## Provider Interface

### IDocumentRetrievalProvider

```csharp
namespace OoBDev.System.Documents.Retrieval.Providers;

/// <summary>
/// Provider interface for document retrieval from specific storage backend.
/// </summary>
public interface IDocumentRetrievalProvider
{
    /// <summary>
    /// Provider name (e.g., "azure-blob", "sql-server", "file-system").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Provider capabilities (versioning, full-text search, etc.).
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
    /// Provider supports full-text search in content.
    /// </summary>
    public bool SupportsFullTextSearch { get; set; }

    /// <summary>
    /// Provider supports document tagging.
    /// </summary>
    public bool SupportsTagging { get; set; }

    /// <summary>
    /// Provider supports metadata queries.
    /// </summary>
    public bool SupportsMetadataQuery { get; set; }

    /// <summary>
    /// Maximum document size supported (bytes).
    /// </summary>
    public long MaxDocumentSize { get; set; } = long.MaxValue;
}
```

---

## Exception Types

```csharp
namespace OoBDev.System.Documents.Retrieval;

/// <summary>
/// Exception thrown when document not found.
/// </summary>
public class DocumentNotFoundException : Exception
{
    public Guid DocumentId { get; }

    public DocumentNotFoundException(Guid documentId, string? message = null)
        : base(message ?? $"Document {documentId} not found")
    {
        DocumentId = documentId;
    }
}

/// <summary>
/// Exception thrown when document retrieval fails.
/// </summary>
public class DocumentRetrievalException : Exception
{
    public Guid? DocumentId { get; }

    public DocumentRetrievalException(string message, Exception? innerException = null, Guid? documentId = null)
        : base(message, innerException)
    {
        DocumentId = documentId;
    }
}

/// <summary>
/// Exception thrown when provider doesn't support requested operation.
/// </summary>
public class VersionNotSupportedException : Exception
{
    public string ProviderName { get; }

    public VersionNotSupportedException(string providerName)
        : base($"Provider '{providerName}' does not support versioning")
    {
        ProviderName = providerName;
    }
}

/// <summary>
/// Exception thrown when provider is unavailable.
/// </summary>
public class ProviderNotAvailableException : Exception
{
    public string ProviderName { get; }

    public ProviderNotAvailableException(string providerName, Exception? innerException = null)
        : base($"Provider '{providerName}' is not available", innerException)
    {
        ProviderName = providerName;
    }
}
```

---

## Usage Examples

### Example 1: Basic Document Retrieval

```csharp
using OoBDev.System.Documents.Retrieval;

public class InvoiceService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<byte[]> GetInvoicePdfAsync(Guid invoiceId)
    {
        // Basic retrieval with default context
        var document = await _retrieval.GetAsync(invoiceId);

        if (document.MediaType != "application/pdf")
        {
            throw new InvalidOperationException($"Document is {document.MediaType}, not PDF");
        }

        return document.Content ?? throw new InvalidOperationException("Document content is null");
    }
}
```

---

### Example 2: Context-Based Retrieval

```csharp
public class DocumentViewerService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<Document> GetDocumentForUserAsync(Guid documentId, string userId)
    {
        // Provide context for auditing and provider selection
        var context = new RetrievalContext
        {
            RequestingApplication = "document-viewer",
            UserId = userId,
            IncludeMetadata = true,
            IncludeContent = true,
            AdditionalContext = new Dictionary<string, object>
            {
                ["AuditAccess"] = true,
                ["PreferredProvider"] = "azure-blob"
            }
        };

        return await _retrieval.GetAsync(documentId, context);
    }
}
```

---

### Example 3: Metadata-Only Retrieval (Fast Listing)

```csharp
public class DocumentListService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<IEnumerable<DocumentSummary>> ListDocumentsAsync(string[] documentIds)
    {
        // Retrieve metadata only (10-100x faster)
        var context = new RetrievalContext
        {
            RequestingApplication = "document-list",
            IncludeMetadata = true,
            IncludeContent = false  // Skip content loading
        };

        var summaries = new List<DocumentSummary>();

        foreach (var id in documentIds.Select(Guid.Parse))
        {
            try
            {
                var document = await _retrieval.GetAsync(id, context);

                summaries.Add(new DocumentSummary
                {
                    Id = document.Id,
                    Name = document.Name,
                    MediaType = document.MediaType,
                    Size = document.Size,
                    CreatedDate = document.CreatedDate,
                    Tags = document.Tags
                });
            }
            catch (DocumentNotFoundException)
            {
                // Skip missing documents
            }
        }

        return summaries;
    }
}
```

---

### Example 4: Querying Documents

```csharp
public class InvoiceSearchService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<IEnumerable<Document>> SearchInvoicesAsync(
        DateTime startDate,
        DateTime endDate,
        string? customerName = null)
    {
        var query = new DocumentQuery
        {
            MediaType = "application/pdf",
            Tags = new[] { "invoice" },
            CreatedAfter = startDate,
            CreatedBefore = endDate,
            PageSize = 100,
            PageNumber = 1,
            SortBy = "CreatedDate",
            SortAscending = false
        };

        // Add custom filter for customer name (if provider supports)
        if (!string.IsNullOrEmpty(customerName))
        {
            query.CustomFilters["CustomerName"] = customerName;
        }

        var context = new RetrievalContext
        {
            RequestingApplication = "invoice-search",
            IncludeMetadata = true,
            IncludeContent = false  // Metadata only for search results
        };

        return await _retrieval.QueryAsync(query, context);
    }
}
```

---

### Example 5: Versioned Document Retrieval

```csharp
public class DocumentHistoryService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<IEnumerable<DocumentVersion>> GetDocumentHistoryAsync(Guid documentId)
    {
        try
        {
            return await _retrieval.GetVersionsAsync(documentId);
        }
        catch (VersionNotSupportedException ex)
        {
            _logger.LogWarning(ex, "Versioning not supported for document {DocumentId}", documentId);
            return [];
        }
    }

    public async Task<Document> GetDocumentVersionAsync(Guid documentId, int version)
    {
        var context = new RetrievalContext
        {
            RequestingApplication = "document-history",
            IncludeContent = true
        };

        return await _retrieval.GetVersionAsync(documentId, version, context);
    }
}
```

---

### Example 6: Multi-Provider Retrieval with Fallback

```csharp
public class ReliableDocumentService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<Document?> TryGetDocumentAsync(Guid documentId)
    {
        var context = new RetrievalContext
        {
            RequestingApplication = "reliable-service",
            AdditionalContext = new Dictionary<string, object>
            {
                // Primary provider
                ["PreferredProvider"] = "azure-blob",
                // Will automatically fall back to configured FallbackProvider on failure
            }
        };

        try
        {
            return await _retrieval.GetAsync(documentId, context);
        }
        catch (DocumentNotFoundException)
        {
            _logger.LogInformation("Document {DocumentId} not found", documentId);
            return null;
        }
        catch (DocumentRetrievalException ex)
        {
            _logger.LogError(ex, "Failed to retrieve document {DocumentId}", documentId);
            return null;
        }
    }
}
```

---

### Example 7: Check Document Existence

```csharp
public class DocumentValidationService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<bool> ValidateDocumentReferencesAsync(Guid[] documentIds)
    {
        var context = new RetrievalContext
        {
            RequestingApplication = "document-validator"
        };

        foreach (var docId in documentIds)
        {
            if (!await _retrieval.ExistsAsync(docId, context))
            {
                _logger.LogWarning("Document {DocumentId} does not exist", docId);
                return false;
            }
        }

        return true;
    }
}
```

---

### Example 8: Paginated Query

```csharp
public class DocumentBrowserService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<PagedResult<Document>> GetDocumentsPageAsync(int pageNumber, int pageSize = 50)
    {
        var query = new DocumentQuery
        {
            PageSize = pageSize,
            PageNumber = pageNumber,
            SortBy = "CreatedDate",
            SortAscending = false
        };

        var context = new RetrievalContext
        {
            RequestingApplication = "document-browser",
            IncludeMetadata = true,
            IncludeContent = false
        };

        var documents = await _retrieval.QueryAsync(query, context);
        var documentList = documents.ToList();

        return new PagedResult<Document>
        {
            Items = documentList,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = documentList.Count,  // Note: Provider should return total count
            HasNextPage = documentList.Count == pageSize
        };
    }
}
```

---

### Example 9: Custom Provider-Specific Context

```csharp
public class HighPerformanceRetrievalService
{
    private readonly IDocumentRetrievalService _retrieval;

    public async Task<Document> GetDocumentOptimizedAsync(Guid documentId)
    {
        var context = new RetrievalContext
        {
            RequestingApplication = "high-performance-service",
            AdditionalContext = new Dictionary<string, object>
            {
                // Azure Blob-specific
                ["DownloadTransferOptions"] = new { MaximumConcurrency = 8 },

                // S3-specific
                ["PreferredRegion"] = "us-west-2",
                ["UseAcceleration"] = true,

                // Database-specific
                ["CommandTimeout"] = 30,
                ["NoLock"] = true
            }
        };

        return await _retrieval.GetAsync(documentId, context);
    }
}
```

---

## Dependency Injection Setup

### Service Registration

```csharp
using OoBDev.System.Documents.Retrieval;
using Microsoft.Extensions.DependencyInjection;

public static class DocumentRetrievalServiceExtensions
{
    /// <summary>
    /// Adds document retrieval service with providers.
    /// </summary>
    public static IServiceCollection AddDocumentRetrieval(
        this IServiceCollection services,
        Action<DocumentRetrievalOptions>? configureOptions = null)
    {
        // Register service
        services.TryAddSingleton<IDocumentRetrievalService, DocumentRetrievalService>();

        // Register provider factory
        services.TryAddSingleton<IDocumentRetrievalProviderFactory, DocumentRetrievalProviderFactory>();

        // Register built-in providers
        services.TryAddSingleton<IDocumentRetrievalProvider, DatabaseRetrievalProvider>();
        services.TryAddSingleton<IDocumentRetrievalProvider, AzureBlobRetrievalProvider>();
        services.TryAddSingleton<IDocumentRetrievalProvider, S3RetrievalProvider>();
        services.TryAddSingleton<IDocumentRetrievalProvider, FileSystemRetrievalProvider>();

        // Configure options
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        return services;
    }
}

/// <summary>
/// Configuration options for document retrieval.
/// </summary>
public class DocumentRetrievalOptions
{
    /// <summary>
    /// Default provider name (e.g., "database", "azure-blob").
    /// </summary>
    public string DefaultProvider { get; set; } = "database";

    /// <summary>
    /// Fallback provider when primary fails.
    /// </summary>
    public string? FallbackProvider { get; set; }

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
        services.AddDocumentRetrieval(options =>
        {
            options.DefaultProvider = "azure-blob";
            options.FallbackProvider = "database";
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

### 1. Use Metadata-Only Queries
```csharp
// ✅ GOOD: Fast listing (metadata only)
var context = new RetrievalContext { IncludeContent = false };
var documents = await _retrieval.QueryAsync(query, context);

// ❌ BAD: Slow listing (loads all content)
var documents = await _retrieval.QueryAsync(query);  // IncludeContent defaults to true
```

### 2. Provide Context for Auditing
```csharp
// ✅ GOOD: Context includes user ID for audit trail
var context = new RetrievalContext
{
    RequestingApplication = "report-generator",
    UserId = currentUser.Id
};

// ❌ BAD: No context (can't audit who accessed document)
var document = await _retrieval.GetAsync(documentId);
```

### 3. Handle Not Found Gracefully
```csharp
// ✅ GOOD: Handle DocumentNotFoundException
try
{
    var document = await _retrieval.GetAsync(documentId, context);
}
catch (DocumentNotFoundException)
{
    return NotFound($"Document {documentId} not found");
}

// ❌ BAD: Let exception propagate as 500 Internal Server Error
var document = await _retrieval.GetAsync(documentId, context);
```

### 4. Use Pagination for Large Queries
```csharp
// ✅ GOOD: Paginated query
var query = new DocumentQuery
{
    PageSize = 100,
    PageNumber = 1
};

// ❌ BAD: Unbounded query (may return millions of documents)
var query = new DocumentQuery { };  // No pagination
```

---

## Performance Considerations

### Metadata-Only vs Full Retrieval
| Operation | Metadata-Only | Full Retrieval | Speedup |
|-----------|---------------|----------------|---------|
| 1KB document | 5ms | 10ms | 2x |
| 100KB document | 5ms | 50ms | 10x |
| 10MB document | 5ms | 2000ms | 400x |

**Recommendation:** Always use `IncludeContent = false` for listing/searching operations.

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [PersistenceService API](../PersistenceService/api-design.md)
