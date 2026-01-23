# Document Splitting Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Splitting Service
**Last Updated:** 2026-01-23

---

## API Overview

Complete API surface for document splitting with provider pattern, context-based behavior, and comprehensive format support for dividing documents into smaller parts.

---

## Core Interfaces

### IDocumentSplittingService

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Documents.Splitting;

/// <summary>
/// Service for splitting documents into smaller parts with provider pattern support.
/// </summary>
public interface IDocumentSplittingService
{
    /// <summary>
    /// Splits document by page ranges.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format (e.g., "pdf", "docx")</param>
    /// <param name="pageRanges">Array of [startPage, endPage] ranges (1-based)</param>
    /// <param name="context">Optional splitting context</param>
    /// <returns>Splitting result with split parts</returns>
    Task<SplittingResult> SplitByPagesAsync(
        byte[] documentContent,
        string format,
        int[][] pageRanges,
        SplittingContext? context = null);

    /// <summary>
    /// Splits document into N-page chunks.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="pageCount">Pages per chunk</param>
    /// <param name="context">Optional splitting context</param>
    /// <returns>Splitting result with split parts</returns>
    Task<SplittingResult> SplitEveryNPagesAsync(
        byte[] documentContent,
        string format,
        int pageCount,
        SplittingContext? context = null);

    /// <summary>
    /// Splits document by size limit.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="maxSizeBytes">Maximum size per part in bytes</param>
    /// <param name="context">Optional splitting context</param>
    /// <returns>Splitting result with split parts</returns>
    Task<SplittingResult> SplitBySizeAsync(
        byte[] documentContent,
        string format,
        long maxSizeBytes,
        SplittingContext? context = null);

    /// <summary>
    /// Splits PDF document at bookmarks.
    /// </summary>
    /// <param name="documentContent">PDF document content</param>
    /// <param name="bookmarkDepth">Bookmark depth level for splitting (default: 1)</param>
    /// <param name="context">Optional splitting context</param>
    /// <returns>Splitting result with split parts</returns>
    Task<SplittingResult> SplitByBookmarksAsync(
        byte[] documentContent,
        int bookmarkDepth = 1,
        SplittingContext? context = null);

    /// <summary>
    /// Splits document using specified mode.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="mode">Split mode</param>
    /// <param name="context">Optional splitting context</param>
    /// <returns>Splitting result with split parts</returns>
    Task<SplittingResult> SplitAsync(
        byte[] documentContent,
        string format,
        SplitMode mode,
        SplittingContext? context = null);

    /// <summary>
    /// Batch splits multiple documents.
    /// </summary>
    /// <param name="requests">Collection of splitting requests</param>
    /// <param name="context">Optional splitting context</param>
    /// <returns>Collection of splitting results</returns>
    Task<IEnumerable<SplittingResult>> SplitBatchAsync(
        IEnumerable<SplittingRequest> requests,
        SplittingContext? context = null);

    /// <summary>
    /// Validates splitting result.
    /// </summary>
    /// <param name="result">Splitting result to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateSplitAsync(SplittingResult result);
}
```

---

### IDocumentSplittingProvider

```csharp
namespace OoBDev.System.Documents.Splitting.Providers;

/// <summary>
/// Provider interface for document splitting implementations.
/// </summary>
public interface IDocumentSplittingProvider
{
    /// <summary>
    /// Provider name (e.g., "pdfsharp", "itext", "pdfbox").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Provider capabilities and supported features.
    /// </summary>
    DocumentSplittingCapabilities Capabilities { get; }

    /// <summary>
    /// Splits document according to specified mode.
    /// </summary>
    Task<SplittingResult> SplitAsync(
        byte[] documentContent,
        string format,
        SplitMode mode,
        SplittingContext context);

    /// <summary>
    /// Checks if format is supported by this provider.
    /// </summary>
    bool SupportsFormat(string format);

    /// <summary>
    /// Checks if split mode is supported by this provider.
    /// </summary>
    bool SupportsMode(SplitMode mode);

    /// <summary>
    /// Gets all formats supported by this provider.
    /// </summary>
    IEnumerable<string> GetSupportedFormats();
}
```

---

## Data Models

### SplittingContext

```csharp
namespace OoBDev.System.Documents.Splitting;

/// <summary>
/// Context for document splitting operations.
/// </summary>
public class SplittingContext
{
    /// <summary>
    /// Requesting application name for auditing.
    /// </summary>
    public string? RequestingApplication { get; set; }

    /// <summary>
    /// User ID for auditing.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Preserve document metadata during splitting (default: true).
    /// </summary>
    public bool PreserveMetadata { get; set; } = true;

    /// <summary>
    /// Validate output after splitting (default: true).
    /// </summary>
    public bool ValidateOutput { get; set; } = true;

    /// <summary>
    /// Number of pages to overlap between parts (default: 0).
    /// </summary>
    public int OverlapPages { get; set; } = 0;

    /// <summary>
    /// Preserve bookmarks in split parts (default: true).
    /// </summary>
    public bool PreserveBookmarks { get; set; } = true;

    /// <summary>
    /// Additional split-specific options.
    /// </summary>
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}
```

### SplittingRequest

```csharp
/// <summary>
/// Request for document splitting (used in batch operations).
/// </summary>
public class SplittingRequest
{
    /// <summary>
    /// Unique request identifier.
    /// </summary>
    public Guid RequestId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Document content.
    /// </summary>
    public byte[] DocumentContent { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Document format (e.g., "pdf", "docx").
    /// </summary>
    public string Format { get; set; } = "";

    /// <summary>
    /// Split mode.
    /// </summary>
    public SplitMode Mode { get; set; }

    /// <summary>
    /// Mode-specific parameters.
    /// </summary>
    public IDictionary<string, object> Parameters { get; set; } = new Dictionary<string, object>();
}
```

### SplittingResult

```csharp
/// <summary>
/// Result of document splitting operation.
/// </summary>
public class SplittingResult
{
    /// <summary>
    /// Request identifier (for batch operations).
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Indicates if splitting was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Split document parts.
    /// </summary>
    public IEnumerable<SplitPart> Parts { get; set; } = Array.Empty<SplitPart>();

    /// <summary>
    /// Total number of parts created.
    /// </summary>
    public int TotalParts { get; set; }

    /// <summary>
    /// Source document page count.
    /// </summary>
    public int SourcePageCount { get; set; }

    /// <summary>
    /// Total pages across all split parts.
    /// </summary>
    public int TotalSplitPages { get; set; }

    /// <summary>
    /// Splitting duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Provider used for splitting.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Error message if splitting failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
```

### SplitPart

```csharp
/// <summary>
/// Individual split document part.
/// </summary>
public class SplitPart
{
    /// <summary>
    /// Part number (1-based).
    /// </summary>
    public int PartNumber { get; set; }

    /// <summary>
    /// Part content.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// First page in this part (1-based).
    /// </summary>
    public int StartPage { get; set; }

    /// <summary>
    /// Last page in this part (1-based).
    /// </summary>
    public int EndPage { get; set; }

    /// <summary>
    /// Number of pages in this part.
    /// </summary>
    public int PageCount { get; set; }

    /// <summary>
    /// Part size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Part-specific metadata.
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

### SplitMode

```csharp
/// <summary>
/// Document splitting modes.
/// </summary>
public enum SplitMode
{
    /// <summary>
    /// Each page becomes separate document.
    /// </summary>
    SinglePages,

    /// <summary>
    /// Specify explicit page ranges.
    /// </summary>
    PageRanges,

    /// <summary>
    /// Split every N pages.
    /// </summary>
    EveryNPages,

    /// <summary>
    /// Split by file size limit.
    /// </summary>
    BySize,

    /// <summary>
    /// Split at PDF bookmarks.
    /// </summary>
    ByBookmarks,

    /// <summary>
    /// Split at section breaks (Office documents).
    /// </summary>
    BySections
}
```

### DocumentSplittingCapabilities

```csharp
namespace OoBDev.System.Documents.Splitting.Providers;

/// <summary>
/// Capabilities of a document splitting provider.
/// </summary>
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

## Exception Types

```csharp
namespace OoBDev.System.Documents.Splitting;

public class SplittingNotSupportedException : Exception
{
    public SplittingNotSupportedException(string message) : base(message) { }
}

public class SplittingFailedException : Exception
{
    public SplittingFailedException(string message) : base(message) { }
    public SplittingFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}

public class InvalidSplitRangeException : Exception
{
    public InvalidSplitRangeException(string message) : base(message) { }
}

public class SplitValidationFailedException : Exception
{
    public SplitValidationFailedException(string message) : base(message) { }
}
```

---

## Usage Examples

### Example 1: Split PDF by Pages

```csharp
// Split PDF into 10-page chunks
var pdfContent = await File.ReadAllBytesAsync("large-document.pdf");

var result = await _splittingService.SplitEveryNPagesAsync(
    pdfContent,
    "pdf",
    pageCount: 10);

if (result.Success)
{
    int partNum = 1;
    foreach (var part in result.Parts)
    {
        await File.WriteAllBytesAsync($"part-{partNum}.pdf", part.Content);
        Console.WriteLine($"Part {partNum}: Pages {part.StartPage}-{part.EndPage}");
        partNum++;
    }
}
```

### Example 2: Split by Custom Ranges

```csharp
var pageRanges = new[]
{
    new[] { 1, 5 },    // Pages 1-5
    new[] { 6, 10 },   // Pages 6-10
    new[] { 11, 20 }   // Pages 11-20
};

var result = await _splittingService.SplitByPagesAsync(
    pdfContent,
    "pdf",
    pageRanges);
```

### Example 3: Split by Size

```csharp
var maxSizeBytes = 5 * 1024 * 1024; // 5MB per part

var result = await _splittingService.SplitBySizeAsync(
    pdfContent,
    "pdf",
    maxSizeBytes);

Console.WriteLine($"Split into {result.TotalParts} parts to meet size limit");
```

### Example 4: Split by Bookmarks

```csharp
// Split PDF at top-level bookmarks
var result = await _splittingService.SplitByBookmarksAsync(
    pdfContent,
    bookmarkDepth: 1);

foreach (var part in result.Parts)
{
    var bookmarkTitle = part.Metadata["BookmarkTitle"];
    await File.WriteAllBytesAsync($"{bookmarkTitle}.pdf", part.Content);
}
```

### Example 5: Split with Overlap

```csharp
var context = new SplittingContext
{
    OverlapPages = 2,  // 2-page overlap between parts
    PreserveMetadata = true
};

var result = await _splittingService.SplitEveryNPagesAsync(
    pdfContent,
    "pdf",
    pageCount: 10,
    context);
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
