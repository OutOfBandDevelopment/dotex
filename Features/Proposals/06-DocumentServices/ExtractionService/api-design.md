# Document Extraction Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Extraction Service
**Last Updated:** 2026-01-22

---

## API Overview

Complete API surface for document extraction with provider pattern, context-based behavior, and comprehensive format support for text, metadata, images, and tables.

---

## Core Interfaces

### IDocumentExtractionService

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Documents.Extraction;

/// <summary>
/// Service for extracting content from documents with provider pattern support.
/// </summary>
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

    /// <summary>
    /// Checks if format is supported for extraction.
    /// </summary>
    Task<bool> IsSupportedAsync(string format);

    /// <summary>
    /// Gets extraction capabilities for format.
    /// </summary>
    Task<FormatExtractionCapabilities> GetCapabilitiesAsync(string format);
}
```

---

## Data Models

### ExtractionContext

```csharp
/// <summary>
/// Context for document extraction operations.
/// </summary>
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
```

### ExtractionResult

```csharp
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
```

### DocumentMetadata

```csharp
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
```

### ExtractedImage

```csharp
public class ExtractedImage
{
    public int Index { get; set; }
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string Format { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public long Size { get; set; }
}
```

### ExtractedTable

```csharp
public class ExtractedTable
{
    public int Index { get; set; }
    public int RowCount { get; set; }
    public int ColumnCount { get; set; }
    public string[][] Cells { get; set; } = Array.Empty<string[]>();
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

### MetadataDepth

```csharp
public enum MetadataDepth
{
    Basic,      // Title, author, dates
    Standard,   // Basic + subject, keywords, counts
    Extended    // All metadata
}
```

---

## Usage Examples

### Example 1: Extract Text from PDF

```csharp
var pdfContent = await File.ReadAllBytesAsync("document.pdf");
var result = await _extractionService.ExtractTextAsync(pdfContent, "pdf");

if (result.Success)
{
    Console.WriteLine($"Extracted {result.ExtractedText.Length} characters");
    Console.WriteLine($"Provider: {result.ProviderName}");
}
```

### Example 2: Extract Metadata Only

```csharp
var metadata = await _extractionService.ExtractMetadataAsync(pdfContent, "pdf");
Console.WriteLine($"Title: {metadata.Title}");
Console.WriteLine($"Author: {metadata.Author}");
Console.WriteLine($"Pages: {metadata.PageCount}");
```

### Example 3: Extract with OCR for Scanned Documents

```csharp
var context = new ExtractionContext
{
    UseOcr = true,
    OcrLanguages = new[] { "eng", "fra" },
    MetadataDepth = MetadataDepth.Extended
};

var result = await _extractionService.ExtractTextAsync(scannedPdf, "pdf", context);
```

### Example 4: Extract All Content

```csharp
var context = new ExtractionContext
{
    ExtractText = true,
    ExtractMetadata = true,
    ExtractImages = true,
    ExtractTables = true
};

var result = await _extractionService.ExtractAllAsync(wordDoc, "docx", context);

Console.WriteLine($"Text: {result.ExtractedText?.Length} chars");
Console.WriteLine($"Images: {result.Images.Count()}");
Console.WriteLine($"Tables: {result.Tables.Count()}");
```

### Example 5: Extract Images from Document

```csharp
var images = await _extractionService.ExtractImagesAsync(pptxContent, "pptx");

int index = 0;
foreach (var image in images)
{
    await File.WriteAllBytesAsync($"image_{index}.{image.Format}", image.Content);
    Console.WriteLine($"Image {index}: {image.Width}x{image.Height}, {image.Size} bytes");
    index++;
}
```

### Example 6: Extract Tables from Excel

```csharp
var tables = await _extractionService.ExtractTablesAsync(excelContent, "xlsx");

foreach (var table in tables)
{
    Console.WriteLine($"Table {table.Index}: {table.RowCount} rows, {table.ColumnCount} columns");

    foreach (var row in table.Cells)
    {
        Console.WriteLine(string.Join(" | ", row));
    }
}
```

### Example 7: Batch Extraction

```csharp
var requests = new List<ExtractionRequest>
{
    new() { DocumentContent = pdf1, Format = "pdf" },
    new() { DocumentContent = doc1, Format = "docx" },
    new() { DocumentContent = xls1, Format = "xlsx" }
};

var results = await _extractionService.ExtractBatchAsync(requests);

foreach (var result in results)
{
    Console.WriteLine($"Request {result.RequestId}: {(result.Success ? "Success" : "Failed")}");
}
```

---

## Dependency Injection

```csharp
public static class DocumentExtractionServiceExtensions
{
    public static IServiceCollection AddDocumentExtraction(
        this IServiceCollection services,
        Action<DocumentExtractionOptions>? configure = null)
    {
        services.TryAddSingleton<IDocumentExtractionService, DocumentExtractionService>();
        services.TryAddSingleton<IDocumentExtractionProviderFactory, DocumentExtractionProviderFactory>();

        // Register providers
        services.TryAddSingleton<IDocumentExtractionProvider, TikaExtractionProvider>();
        services.TryAddSingleton<IDocumentExtractionProvider, OpenXmlExtractionProvider>();
        services.TryAddSingleton<IDocumentExtractionProvider, PdfExtractionProvider>();

        if (configure != null)
            services.Configure(configure);

        return services;
    }
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
