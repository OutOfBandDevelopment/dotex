# Document Composition Service - Requirements

**Epic:** 6 - Document Services
**Feature:** Document Composition Service
**Priority:** MEDIUM (Core Functionality)
**Complexity:** MEDIUM-HIGH
**Estimated LOC:** ~400

---

## Overview

Context-based document composition service for merging multiple documents into a single document, combining pages from different sources, and creating compound documents. Applications provide operational context to influence composition behavior including merge order, page insertion, and formatting preservation.

---

## Business Requirements

### BR-1: Document Merging with Context
**As a** developer
**I want** to merge multiple documents into one
**So that** I can create combined documents from separate sources

**Acceptance Criteria:**
- Merge 2+ documents into single output
- Context includes requesting application, user ID, and merge options
- Preserve source document formatting
- Maintain page order from source documents
- Returns merged document with metadata
- Supports synchronous and asynchronous merging
- Handles different page sizes and orientations

---

### BR-2: Page-Level Composition
**As a** developer
**I want** to compose documents from specific pages of multiple sources
**So that** I can create custom document combinations

**Acceptance Criteria:**
- Select specific pages from each source document
- Insert pages at specific positions
- Reorder pages during composition
- Context controls page selection and ordering
- Preserve page-specific attributes
- Support page range notation (e.g., "1-5, 8, 10-12")

---

### BR-3: Multi-Format Support
**As a** system architect
**I want** pluggable composition providers
**So that** documents can be composed using different engines

**Supported Formats:**
```
- PDF (primary format)
- Word (DOCX) - section merging
- PowerPoint (PPTX) - slide combining
- Images - combine into PDF
- Mixed formats - convert to common format then merge
```

**Acceptance Criteria:**
- Provider pattern for composition engines
- Built-in providers: PDFSharp, iText, Apache PDFBox
- Provider selection based on document formats
- Automatic format conversion when needed
- Provider registration via dependency injection

---

### BR-4: Table of Contents Generation
**As a** developer
**I want** automatic ToC generation for merged documents
**So that** users can navigate combined documents

**Acceptance Criteria:**
- Generate ToC from source document bookmarks
- Create ToC from source filenames
- Custom ToC entries via context
- ToC with page numbers
- Hierarchical ToC structure
- Clickable ToC links in output

---

### BR-5: Blank Page Insertion
**As a** developer
**I want** to insert blank pages during composition
**So that** I can ensure proper pagination and chapter breaks

**Acceptance Criteria:**
- Insert blank pages at specified positions
- Insert blank pages between sources
- Ensure right-hand page starts (insert blank on left if needed)
- Configurable blank page size and orientation
- Context controls blank page insertion

---

### BR-6: Metadata Consolidation
**As a** developer
**I want** consolidated metadata for composed documents
**So that** I maintain document properties

**Acceptance Criteria:**
- Merge author lists from all sources
- Combine keywords from all sources
- Preserve creation dates (use oldest)
- Update modification date (current)
- Add composition metadata (source count, composition date)
- Context controls metadata strategy

---

### BR-7: Bookmark Preservation
**As a** developer
**I want** bookmarks preserved during composition
**So that** navigation is maintained in merged documents

**Acceptance Criteria:**
- Preserve bookmarks from all source documents
- Adjust bookmark page numbers for merged document
- Maintain bookmark hierarchy
- Prefix bookmarks with source identifier
- Merge duplicate bookmark names
- Context controls bookmark handling

---

### BR-8: Page Numbering
**As a** developer
**I want** consistent page numbering in composed documents
**So that** page references are correct

**Acceptance Criteria:**
- Renumber pages across composed document
- Multiple numbering schemes (arabic, roman, letters)
- Section-based numbering
- Restart numbering at specified pages
- Header/footer page number updates
- Context controls numbering strategy

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IDocumentCompositionService
{
    /// <summary>
    /// Merges multiple documents into one.
    /// </summary>
    Task<CompositionResult> MergeAsync(
        IEnumerable<byte[]> documents,
        string format,
        CompositionContext? context = null);

    /// <summary>
    /// Composes document from specific pages.
    /// </summary>
    Task<CompositionResult> ComposeFromPagesAsync(
        IEnumerable<PageSource> pageSources,
        string format,
        CompositionContext? context = null);

    /// <summary>
    /// Inserts document at specific position in target.
    /// </summary>
    Task<CompositionResult> InsertAsync(
        byte[] targetDocument,
        byte[] insertDocument,
        int insertPosition,
        CompositionContext? context = null);

    /// <summary>
    /// Appends documents to target.
    /// </summary>
    Task<CompositionResult> AppendAsync(
        byte[] targetDocument,
        IEnumerable<byte[]> appendDocuments,
        CompositionContext? context = null);

    /// <summary>
    /// Batch composes multiple document sets.
    /// </summary>
    Task<IEnumerable<CompositionResult>> ComposeBatchAsync(
        IEnumerable<CompositionRequest> requests,
        CompositionContext? context = null);
}

public class CompositionContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public bool PreserveBookmarks { get; set; } = true;
    public bool PreserveMetadata { get; set; } = true;
    public bool GenerateToC { get; set; } = false;
    public bool InsertBlankPagesBetweenSources { get; set; } = false;
    public PageNumberingStrategy? NumberingStrategy { get; set; }
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}

public class PageSource
{
    public byte[] DocumentContent { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "";
    public int[] PageNumbers { get; set; } = Array.Empty<int>(); // Empty = all pages
    public string? SourceName { get; set; }
}

public class CompositionResult
{
    public bool Success { get; set; }
    public byte[]? ComposedDocument { get; set; }
    public int TotalPages { get; set; }
    public int SourceDocumentCount { get; set; }
    public TimeSpan Duration { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    public string? ErrorMessage { get; set; }
}

public class PageNumberingStrategy
{
    public NumberingScheme Scheme { get; set; } = NumberingScheme.Arabic;
    public int StartNumber { get; set; } = 1;
    public bool RestartPerSection { get; set; } = false;
}

public enum NumberingScheme
{
    Arabic,      // 1, 2, 3...
    RomanUpper,  // I, II, III...
    RomanLower,  // i, ii, iii...
    LetterUpper, // A, B, C...
    LetterLower  // a, b, c...
}
```

---

### TR-2: Performance Requirements
- **Merge 2 documents:** < 3 seconds
- **Merge 10 documents:** < 10 seconds
- **Large merge (50+ docs):** < 60 seconds
- **Concurrent compositions:** 10+ simultaneous
- **Memory usage:** < 1GB per composition

---

### TR-3: Error Handling
```csharp
public class CompositionNotSupportedException : Exception { }
public class CompositionFailedException : Exception { }
public class InvalidPageSourceException : Exception { }
```

---

## Non-Functional Requirements

### NFR-1: Compatibility
- .NET 10.0
- Async/await patterns
- Dependency injection
- Cross-platform

### NFR-2: Quality
- 100% page accounting
- Metadata completeness > 90%
- Bookmark accuracy > 95%
- Format preservation > 90%

### NFR-3: Testability
- Mock providers
- Deterministic behavior
- Performance benchmarks

---

## Success Criteria

- ✅ Merge documents from multiple sources
- ✅ Compose from specific pages
- ✅ Preserve bookmarks and metadata
- ✅ Generate table of contents
- ✅ Multiple composition providers
- ✅ Batch composition support
- ✅ 80%+ test coverage
- ✅ Performance: < 3 seconds for 2-document merge

---

## Out of Scope

- ❌ Document splitting (use SplittingService)
- ❌ Format conversion (use ConversionService)
- ❌ Content editing (use editing service)
- ❌ OCR (use OcrService)

---

## Dependencies

### Internal
- OoBDev.System.Documents.Abstractions
- OoBDev.System.Documents.Providers
- OoBDev.System.Documents.Conversion

### External
- PdfSharp
- iText7
- Apache.PDFBox.NET
- DocumentFormat.OpenXml

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [SplittingService Requirements](../SplittingService/requirements.md)
- [Epic 6 Overview](../README.md)
