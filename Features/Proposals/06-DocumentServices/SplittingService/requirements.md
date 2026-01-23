# Document Splitting Service - Requirements

**Epic:** 6 - Document Services
**Feature:** Document Splitting Service
**Priority:** MEDIUM (Core Functionality)
**Complexity:** MEDIUM
**Estimated LOC:** ~320

---

## Overview

Context-based document splitting service for dividing documents into smaller parts based on pages, sections, size limits, or content markers. Applications provide operational context to influence splitting behavior including split criteria, overlap settings, and output format preferences.

---

## Business Requirements

### BR-1: Page-Based Splitting with Context
**As a** developer
**I want** to split documents by page ranges
**So that** I can create smaller document chunks from large documents

**Acceptance Criteria:**
- Split by single pages (1 page per output)
- Split by page ranges (e.g., pages 1-5, 6-10)
- Split by page count (e.g., every 10 pages)
- Context includes requesting application, user ID, and split options
- Returns collection of split documents with metadata
- Supports both synchronous and asynchronous splitting
- Preserves document quality and formatting

**Split Modes:**
```csharp
public enum SplitMode
{
    SinglePages,       // Each page becomes separate document
    PageRanges,        // Specify explicit ranges
    EveryNPages,       // Split every N pages
    BySize,            // Split by file size limit
    ByBookmarks,       // Split at PDF bookmarks
    BySections         // Split at section breaks (Office docs)
}
```

---

### BR-2: Size-Based Splitting
**As a** developer
**I want** to split documents to fit within size limits
**So that** I can ensure documents don't exceed size constraints

**Acceptance Criteria:**
- Split by maximum file size (e.g., max 5MB per part)
- Split by maximum page count per part
- Intelligent splitting at logical boundaries
- Context specifies size limits
- Returns parts within size constraints
- Handles edge cases (single page exceeds limit)

---

### BR-3: Bookmark/Section-Based Splitting
**As a** developer
**I want** to split PDF documents at bookmarks
**So that** I can create logical document divisions

**Acceptance Criteria:**
- Split PDF at bookmark/outline entries
- Split Office documents at section breaks
- Preserve bookmark hierarchy in split parts
- Context controls bookmark depth for splitting
- Returns documents aligned with document structure
- Maintains navigation in split parts

---

### BR-4: Multi-Format Support
**As a** system architect
**I want** pluggable splitting providers
**So that** documents can be split using different engines

**Acceptance Criteria:**
- Provider pattern for splitting engines
- Built-in providers: PDFSharp, iText, Apache PDFBox, Office Interop
- Provider selection based on document format
- Fallback providers if primary fails
- Provider registration via dependency injection

**Supported Formats:**
```
- PDF (page splits, bookmark splits, size splits)
- Word (DOCX, DOC) - section splits, page splits
- Excel (XLSX, XLS) - worksheet splits
- PowerPoint (PPTX, PPT) - slide splits
- Large text files - line/chunk splits
```

---

### BR-5: Overlap and Context Preservation
**As a** developer
**I want** to preserve context across splits
**So that** split parts remain usable

**Acceptance Criteria:**
- Optional page overlap (e.g., last page of part N = first page of part N+1)
- Preserve headers/footers in all parts
- Maintain document metadata in all parts
- Context controls overlap size
- Returns parts with preserved context

---

### BR-6: Batch Splitting
**As a** developer
**I want** to split multiple documents in a single operation
**So that** I can efficiently process large document sets

**Acceptance Criteria:**
- Batch split multiple documents
- Parallel processing for performance
- Progress reporting for long-running batches
- Partial success handling
- Returns splitting results with success/failure status per document

---

### BR-7: Split Validation
**As a** developer
**I want** automatic validation of split results
**So that** I can ensure splits were successful

**Acceptance Criteria:**
- Validate all pages accounted for
- Check total page count matches source
- Verify split part readability
- Optional content comparison
- Validation failures throw exception
- Validation can be disabled via context

---

### BR-8: Metadata Preservation
**As a** developer
**I want** document metadata preserved during splitting
**So that** I don't lose important document properties

**Acceptance Criteria:**
- Preserve title, author, subject, keywords
- Preserve creation/modification dates
- Add split metadata (part number, total parts, source document)
- Update page-specific metadata
- Metadata mapping between parts
- Context flag to control metadata preservation

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IDocumentSplittingService
{
    /// <summary>
    /// Splits document by page ranges.
    /// </summary>
    Task<SplittingResult> SplitByPagesAsync(
        byte[] documentContent,
        string format,
        int[][] pageRanges,
        SplittingContext? context = null);

    /// <summary>
    /// Splits document into N-page chunks.
    /// </summary>
    Task<SplittingResult> SplitEveryNPagesAsync(
        byte[] documentContent,
        string format,
        int pageCount,
        SplittingContext? context = null);

    /// <summary>
    /// Splits document by size limit.
    /// </summary>
    Task<SplittingResult> SplitBySizeAsync(
        byte[] documentContent,
        string format,
        long maxSizeBytes,
        SplittingContext? context = null);

    /// <summary>
    /// Splits PDF at bookmarks.
    /// </summary>
    Task<SplittingResult> SplitByBookmarksAsync(
        byte[] documentContent,
        int bookmarkDepth = 1,
        SplittingContext? context = null);

    /// <summary>
    /// Splits document using specified mode.
    /// </summary>
    Task<SplittingResult> SplitAsync(
        byte[] documentContent,
        string format,
        SplitMode mode,
        SplittingContext? context = null);

    /// <summary>
    /// Batch splits multiple documents.
    /// </summary>
    Task<IEnumerable<SplittingResult>> SplitBatchAsync(
        IEnumerable<SplittingRequest> requests,
        SplittingContext? context = null);

    /// <summary>
    /// Validates splitting result.
    /// </summary>
    Task<ValidationResult> ValidateSplitAsync(SplittingResult result);
}

public class SplittingContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public bool PreserveMetadata { get; set; } = true;
    public bool ValidateOutput { get; set; } = true;
    public int OverlapPages { get; set; } = 0;
    public bool PreserveBookmarks { get; set; } = true;
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}

public class SplittingRequest
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public byte[] DocumentContent { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "";
    public SplitMode Mode { get; set; }
    public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}

public class SplittingResult
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public IEnumerable<SplitPart> Parts { get; set; } = Array.Empty<SplitPart>();
    public int TotalParts { get; set; }
    public int SourcePageCount { get; set; }
    public int TotalSplitPages { get; set; }
    public TimeSpan Duration { get; set; }
    public string? ProviderName { get; set; }
    public string? ErrorMessage { get; set; }
}

public class SplitPart
{
    public int PartNumber { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public int StartPage { get; set; }
    public int EndPage { get; set; }
    public int PageCount { get; set; }
    public long Size { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

public enum SplitMode
{
    SinglePages,
    PageRanges,
    EveryNPages,
    BySize,
    ByBookmarks,
    BySections
}
```

---

### TR-2: Provider Interface
```csharp
public interface IDocumentSplittingProvider
{
    string ProviderName { get; }
    DocumentSplittingCapabilities Capabilities { get; }

    Task<SplittingResult> SplitAsync(
        byte[] documentContent,
        string format,
        SplitMode mode,
        SplittingContext context);

    bool SupportsFormat(string format);
    bool SupportsMode(SplitMode mode);
    IEnumerable<string> GetSupportedFormats();
}

public class DocumentSplittingCapabilities
{
    public bool SupportsPageSplitting { get; set; }
    public bool SupportsSizeSplitting { get; set; }
    public bool SupportsBookmarkSplitting { get; set; }
    public bool SupportsSectionSplitting { get; set; }
    public bool SupportsOverlap { get; set; }
    public bool SupportsMetadataPreservation { get; set; }
    public string[] SupportedFormats { get; set; } = Array.Empty<string>();
    public SplitMode[] SupportedModes { get; set; } = Array.Empty<SplitMode>();
}
```

---

### TR-3: Performance Requirements
- **Single document split:** < 3 seconds (format-dependent)
- **Small documents (< 50 pages):** < 2 seconds
- **Large documents (> 500 pages):** < 15 seconds
- **Batch operations:** 10+ documents per minute
- **Concurrent splits:** 10+ simultaneous requests
- **Memory usage:** < 500MB per split operation

---

### TR-4: Error Handling
**Exception Types:**
```csharp
public class SplittingNotSupportedException : Exception { }
public class SplittingFailedException : Exception { }
public class InvalidSplitRangeException : Exception { }
public class SplitValidationFailedException : Exception { }
```

**Error Scenarios:**
- Unsupported format → `SplittingNotSupportedException`
- Provider fails → Try fallback provider or throw `SplittingFailedException`
- Invalid page ranges → `InvalidSplitRangeException`
- Validation fails → `SplitValidationFailedException`
- Corrupted source → `SplittingFailedException` with details

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Supports async/await patterns
- Compatible with dependency injection
- Provider pattern for extensibility
- Cross-platform (Windows, Linux, macOS)

### NFR-2: Scalability
- Handle documents up to 10,000 pages
- Support 1,000+ splits per day
- Concurrent splitting processing
- Multi-tenant support (tenant ID in context)
- Horizontal scaling via provider instances

### NFR-3: Reliability
- Retry on transient failures (3 attempts)
- Provider fallback for resilience
- Timeout protection (60 second default)
- Resource cleanup on failures
- Graceful degradation

### NFR-4: Accuracy
- 100% page accounting (no lost pages)
- Correct page range mapping
- Metadata completeness > 95%
- Split validation accuracy > 99%

### NFR-5: Testability
- Mock providers for unit testing
- In-memory provider for integration tests
- Deterministic behavior for testing
- Performance metrics trackable
- Test helpers for common scenarios

---

## Constraints

### C-1: Provider Constraints
- Providers must be thread-safe
- Provider exceptions propagate to caller
- Not all providers support all split modes
- Provider selection impacts available features
- External tools (PDFBox, iText) may be required

### C-2: Format Constraints
- Some formats don't support all split modes
- Office formats require specific libraries
- Bookmark splitting PDF-specific
- Section splitting Office-specific
- Large splits memory-intensive

### C-3: Resource Constraints
- Splitting processes memory-intensive
- Large documents may timeout
- Concurrent splits limited by CPU/memory
- Temporary files cleaned up after split
- Provider process isolation recommended

### C-4: Quality Constraints
- Split quality depends on source quality
- Some metadata may be lost
- Bookmarks may need reconstruction
- Page ranges must be valid
- Overlap increases total size

---

## Success Criteria

- ✅ Split documents by pages, size, bookmarks, sections
- ✅ Multiple splitting providers (PDFSharp, iText, PDFBox)
- ✅ Context-based splitting options
- ✅ Batch splitting support
- ✅ Overlap and context preservation
- ✅ Metadata preservation
- ✅ Split validation
- ✅ 80%+ test coverage
- ✅ Performance: < 3 seconds typical split

---

## Out of Scope

- ❌ Document merging (use CompositionService)
- ❌ Document editing (use separate editing service)
- ❌ OCR text extraction (use OcrService)
- ❌ Format conversion during split (use ConversionService)
- ❌ Content-based intelligent splitting
- ❌ Machine learning-based splitting

---

## Dependencies

### Internal
- OoBDev.System.Documents.Abstractions (Document model)
- OoBDev.System.Documents.Providers (provider abstractions)
- OoBDev.System.Documents.Conversion (format detection)

### External
- .NET 10.0 BCL
- PdfSharp (PDF splitting)
- iText7 (advanced PDF operations)
- Apache.PDFBox.NET (Java-based PDF operations)
- DocumentFormat.OpenXml (Office format splitting)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [CompositionService Requirements](../CompositionService/requirements.md)
- [Epic 6 Overview](../README.md)
