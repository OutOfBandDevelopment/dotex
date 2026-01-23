# Document Extraction Service - Requirements

**Epic:** 6 - Document Services
**Feature:** Document Extraction Service
**Priority:** HIGH (Core Functionality)
**Complexity:** MEDIUM
**Estimated LOC:** ~350

---

## Overview

Context-based document extraction service for extracting text, metadata, images, and structured data from various document formats (PDF, Word, Excel, PowerPoint, HTML, images with OCR, etc.). Applications provide operational context to influence extraction behavior including OCR settings, metadata depth, and data structure preferences.

---

## Business Requirements

### BR-1: Text Extraction with Context
**As a** developer
**I want** to extract text content from documents with operational context
**So that** I can access document text for indexing, search, and analysis

**Acceptance Criteria:**
- Extract plain text from documents
- Context includes requesting application, user ID, and extraction options
- Providers use context to optimize extraction (OCR, layout preservation, etc.)
- Returns extracted text with metadata
- Supports multiple document formats
- Preserves text structure optionally (paragraphs, headings, lists)

**Supported Formats:**
```
Text Extraction:
  - PDF (native text + OCR for scanned)
  - Word (DOCX, DOC)
  - Excel (XLSX, XLS) - cell text
  - PowerPoint (PPTX, PPT) - slide text
  - HTML
  - Plain text (TXT, CSV, JSON, XML)
  - Images (with OCR)
  - Scanned documents (OCR)
```

---

### BR-2: Metadata Extraction
**As a** developer
**I want** to extract document metadata
**So that** I can categorize, index, and manage documents

**Acceptance Criteria:**
- Extract standard metadata (title, author, subject, keywords, created date, modified date)
- Extract format-specific metadata (page count, word count, fonts, etc.)
- Extract EXIF data from images
- Extract custom properties
- Context controls metadata depth (basic, standard, extended)
- Returns structured metadata dictionary

**Metadata Types:**
```csharp
public enum MetadataDepth
{
    Basic,      // Title, author, dates only
    Standard,   // Basic + subject, keywords, page count
    Extended    // All available metadata including format-specific
}
```

---

### BR-3: Image Extraction
**As a** developer
**I want** to extract embedded images from documents
**So that** I can access images for processing or display

**Acceptance Criteria:**
- Extract images from PDF, Word, PowerPoint, HTML
- Returns images as byte arrays with format information
- Context specifies image format preferences
- Optionally convert images to specific format
- Returns image metadata (width, height, format, size)
- Supports batch image extraction

---

### BR-4: Table and Structured Data Extraction
**As a** developer
**I want** to extract tables and structured data from documents
**So that** I can process tabular data programmatically

**Acceptance Criteria:**
- Extract tables from PDF, Word, Excel, HTML
- Returns tables as 2D arrays or DataTable objects
- Preserve table structure (rows, columns, cells)
- Extract cell formatting optionally
- Context specifies table output format (CSV, JSON, DataTable)
- Handle merged cells and complex table layouts

---

### BR-5: Multi-Provider Support
**As a** system architect
**I want** pluggable extraction providers
**So that** documents can be extracted using different engines

**Acceptance Criteria:**
- Provider pattern for extraction engines
- Built-in providers: Apache Tika, iText, PDFSharp, Office Interop, OCR engines
- Provider selection based on document format
- Fallback providers if primary fails
- Provider registration via dependency injection

**Supported Providers:**
```
- Apache Tika (universal format support)
- iText (PDF extraction)
- PDFSharp (.NET native PDF)
- Office Interop (Office formats - Windows only)
- OpenXML SDK (Office formats - cross-platform)
- Tesseract OCR (image text extraction)
- Custom providers via IDocumentExtractionProvider
```

---

### BR-6: OCR Integration
**As a** developer
**I want** OCR text extraction from images and scanned PDFs
**So that** I can extract text from non-searchable documents

**Acceptance Criteria:**
- Detect if PDF is scanned (no text layer)
- Automatically use OCR for scanned documents
- Context specifies OCR language(s)
- Context specifies OCR quality/speed tradeoff
- Returns OCR confidence scores
- Integration with OcrService (Epic 6)

---

### BR-7: Batch Extraction
**As a** developer
**I want** to extract data from multiple documents in a single operation
**So that** I can efficiently process large document sets

**Acceptance Criteria:**
- Batch extract text, metadata, or images from multiple documents
- Parallel processing for performance
- Progress reporting for long-running batches
- Partial success handling
- Returns extraction results with success/failure status per document

---

### BR-8: Selective Extraction
**As a** developer
**I want** to extract specific content types only
**So that** I can optimize extraction performance

**Acceptance Criteria:**
- Extract text only, metadata only, images only, or combinations
- Context flags: ExtractText, ExtractMetadata, ExtractImages, ExtractTables
- Skip unnecessary extraction for better performance
- Provider optimizes based on extraction flags

---

## Technical Requirements

### TR-1: Interface Design
```csharp
public interface IDocumentExtractionService
{
    /// <summary>
    /// Extracts text content from document.
    /// </summary>
    Task<ExtractionResult> ExtractTextAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null);

    /// <summary>
    /// Extracts metadata from document.
    /// </summary>
    Task<DocumentMetadata> ExtractMetadataAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null);

    /// <summary>
    /// Extracts images from document.
    /// </summary>
    Task<IEnumerable<ExtractedImage>> ExtractImagesAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null);

    /// <summary>
    /// Extracts tables from document.
    /// </summary>
    Task<IEnumerable<ExtractedTable>> ExtractTablesAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null);

    /// <summary>
    /// Extracts all content (text, metadata, images, tables).
    /// </summary>
    Task<ExtractionResult> ExtractAllAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null);

    /// <summary>
    /// Batch extracts content from multiple documents.
    /// </summary>
    Task<IEnumerable<ExtractionResult>> ExtractBatchAsync(
        IEnumerable<ExtractionRequest> requests,
        ExtractionContext? context = null);
}

public class ExtractionContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public bool ExtractText { get; set; } = true;
    public bool ExtractMetadata { get; set; } = true;
    public bool ExtractImages { get; set; } = false;
    public bool ExtractTables { get; set; } = false;
    public bool PreserveLayout { get; set; } = false;
    public MetadataDepth MetadataDepth { get; set; } = MetadataDepth.Standard;
    public bool UseOcr { get; set; } = true;
    public string[] OcrLanguages { get; set; } = new[] { "eng" };
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}

public class ExtractionRequest
{
    public Guid RequestId { get; set; } = Guid.NewGuid();
    public byte[] DocumentContent { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "";
}

public class ExtractionResult
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public DocumentMetadata? Metadata { get; set; }
    public IEnumerable<ExtractedImage> Images { get; set; } = Array.Empty<ExtractedImage>();
    public IEnumerable<ExtractedTable> Tables { get; set; } = Array.Empty<ExtractedTable>();
    public TimeSpan Duration { get; set; }
    public string? ProviderName { get; set; }
    public string? ErrorMessage { get; set; }
}

public class DocumentMetadata
{
    public string? Title { get; set; }
    public string? Author { get; set; }
    public string? Subject { get; set; }
    public string[]? Keywords { get; set; }
    public DateTime? CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public int? PageCount { get; set; }
    public int? WordCount { get; set; }
    public IDictionary<string, object> CustomProperties { get; set; } = new Dictionary<string, object>();
}

public class ExtractedImage
{
    public int Index { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
}

public class ExtractedTable
{
    public int Index { get; set; }
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public string[][] Cells { get; set; } = Array.Empty<string[]>();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

public enum MetadataDepth
{
    Basic,      // Title, author, dates
    Standard,   // Basic + subject, keywords, counts
    Extended    // All metadata including format-specific
}
```

---

### TR-2: Provider Interface
```csharp
public interface IDocumentExtractionProvider
{
    string ProviderName { get; }
    DocumentExtractionCapabilities Capabilities { get; }

    Task<ExtractionResult> ExtractAsync(
        byte[] documentContent,
        string format,
        ExtractionContext context);

    bool SupportsFormat(string format);
    IEnumerable<string> GetSupportedFormats();
}

public class DocumentExtractionCapabilities
{
    public bool SupportsTextExtraction { get; set; }
    public bool SupportsMetadataExtraction { get; set; }
    public bool SupportsImageExtraction { get; set; }
    public bool SupportsTableExtraction { get; set; }
    public bool SupportsOcr { get; set; }
    public bool SupportsLayoutPreservation { get; set; }
    public string[] SupportedFormats { get; set; } = Array.Empty<string>();
    public long MaxDocumentSize { get; set; } = long.MaxValue;
}
```

---

### TR-3: Performance Requirements
- **Single document extraction:** < 3 seconds (format-dependent)
- **Small documents (< 1MB):** < 1 second
- **Large documents (> 10MB):** < 15 seconds
- **OCR extraction:** < 10 seconds per page
- **Batch operations:** 20+ documents per minute
- **Concurrent extractions:** 10+ simultaneous requests

---

### TR-4: Error Handling
**Exception Types:**
```csharp
public class ExtractionNotSupportedException : Exception { }
public class ExtractionFailedException : Exception { }
public class OcrFailedException : Exception { }
public class DocumentCorruptedException : Exception { }
```

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Supports async/await patterns
- Compatible with dependency injection
- Cross-platform where possible

### NFR-2: Scalability
- Handle documents up to 100MB
- Support 10,000+ extractions per day
- Concurrent extraction processing
- Memory-efficient streaming for large documents

### NFR-3: Accuracy
- Text extraction accuracy > 99% for native text
- OCR accuracy > 95% for quality scans
- Metadata extraction completeness > 90%
- Table structure preservation > 85%

### NFR-4: Testability
- Mock providers for unit testing
- Test files for format validation
- Deterministic behavior
- Performance benchmarks

---

## Success Criteria

- ✅ Extract text from 15+ document formats
- ✅ Extract metadata with configurable depth
- ✅ Extract images from documents
- ✅ Extract tables with structure preservation
- ✅ OCR integration for scanned documents
- ✅ Multiple extraction providers
- ✅ Batch extraction support
- ✅ 80%+ test coverage
- ✅ Performance: < 3 seconds typical extraction

---

## Out of Scope

- ❌ Document editing (use separate editing service)
- ❌ Document conversion (use ConversionService)
- ❌ Document indexing (use dedicated search service)
- ❌ Complex layout analysis (basic layout only)

---

## Dependencies

### Internal
- OoBDev.System.Documents.Abstractions
- OoBDev.System.Documents.Ocr (OCR integration)
- OoBDev.System.Documents.MediaTypeDetection

### External
- .NET 10.0 BCL
- Apache.Tika.NET
- iText7
- DocumentFormat.OpenXml
- Tesseract (OCR)

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [OcrService Requirements](../OcrService/requirements.md)
- [Epic 6 Overview](../README.md)
