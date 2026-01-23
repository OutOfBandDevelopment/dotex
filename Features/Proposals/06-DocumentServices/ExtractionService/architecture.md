# Document Extraction Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Extraction Service
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Document Extraction Service implements a **Provider Pattern** with **Context-Based Extraction** for extracting text, metadata, images, and tables from documents using multiple extraction engines. Applications provide operational context that providers use to optimize extraction behavior.

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│         (Search Indexer, Content Analyzer, etc.)             │
└────────────────────┬────────────────────────────────────────┘
                     │ ExtractTextAsync(content, format, context)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│           IDocumentExtractionService                         │
│  - ExtractTextAsync(content, format, context)                │
│  - ExtractMetadataAsync(content, format, context)            │
│  - ExtractImagesAsync(content, format, context)              │
│  - ExtractTablesAsync(content, format, context)              │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼────────────┬────────────┬────────────┐
         ↓           ↓            ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│    Tika      │ │ iText  │ │PDFSharp │ │ OpenXML  │ │Tesseract │
│  Provider    │ │Provider│ │ Provider│ │ Provider │ │   OCR    │
└──────┬───────┘ └────┬───┘ └────┬────┘ └────┬─────┘ └────┬─────┘
       │              │          │            │            │
       ↓              ↓          ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│ Universal    │ │PDF Text│ │PDF Meta │ │Office Fmt│ │Image Text│
│ Format       │ │Extract │ │Extract  │ │Extract   │ │  (OCR)   │
└──────────────┘ └────────┘ └─────────┘ └──────────┘ └──────────┘
```

---

## Core Components

### 1. DocumentExtractionService (Main Entry Point)

**Responsibilities:**
- Coordinate document extraction across providers
- Select appropriate provider based on format and capabilities
- Handle provider fallback on failure
- Aggregate extraction results
- Integrate OCR when needed

**Implementation Pattern:**
```csharp
public class DocumentExtractionService : IDocumentExtractionService
{
    private readonly IDocumentExtractionProviderFactory _providerFactory;
    private readonly IOcrService _ocrService;
    private readonly ILogger<DocumentExtractionService> _logger;

    public async Task<ExtractionResult> ExtractTextAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null)
    {
        context ??= new ExtractionContext();
        var startTime = DateTime.UtcNow;

        // 1. Select provider
        var provider = await SelectProviderAsync(format, context);
        _logger.LogDebug("Selected provider {Provider} for format {Format}",
            provider.ProviderName, format);

        try
        {
            // 2. Set extraction flags
            var extractionContext = context with
            {
                ExtractText = true,
                ExtractMetadata = false,
                ExtractImages = false,
                ExtractTables = false
            };

            // 3. Extract content
            var result = await provider.ExtractAsync(documentContent, format, extractionContext);

            // 4. Check if OCR needed for scanned documents
            if (context.UseOcr && IsScannedDocument(result, format))
            {
                _logger.LogInformation("Document appears scanned, applying OCR");
                result = await ApplyOcrExtractionAsync(documentContent, format, context, result);
            }

            result.Duration = DateTime.UtcNow - startTime;

            _logger.LogInformation(
                "Extracted text from {Format} using {Provider} in {Duration}ms (length: {Length} chars)",
                format, provider.ProviderName, result.Duration.TotalMilliseconds,
                result.ExtractedText?.Length ?? 0);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Extraction failed using provider {Provider}", provider.ProviderName);

            // Try fallback provider
            var fallbackProvider = await GetFallbackProviderAsync(format, provider.ProviderName);
            if (fallbackProvider != null)
            {
                _logger.LogInformation("Retrying with fallback provider {Provider}",
                    fallbackProvider.ProviderName);
                return await fallbackProvider.ExtractAsync(documentContent, format, context);
            }

            throw new ExtractionFailedException(
                $"Failed to extract from {format}: {ex.Message}", ex);
        }
    }

    public async Task<ExtractionResult> ExtractAllAsync(
        byte[] documentContent,
        string format,
        ExtractionContext? context = null)
    {
        context ??= new ExtractionContext();
        var startTime = DateTime.UtcNow;

        // Extract all content types
        context = context with
        {
            ExtractText = true,
            ExtractMetadata = true,
            ExtractImages = true,
            ExtractTables = true
        };

        var provider = await SelectProviderAsync(format, context);
        var result = await provider.ExtractAsync(documentContent, format, context);

        // Apply OCR if needed
        if (context.UseOcr && IsScannedDocument(result, format))
        {
            result = await ApplyOcrExtractionAsync(documentContent, format, context, result);
        }

        result.Duration = DateTime.UtcNow - startTime;
        return result;
    }

    public async Task<IEnumerable<ExtractionResult>> ExtractBatchAsync(
        IEnumerable<ExtractionRequest> requests,
        ExtractionContext? context = null)
    {
        context ??= new ExtractionContext();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5  // Configurable
        };

        var results = new ConcurrentBag<ExtractionResult>();

        await Parallel.ForEachAsync(requests, parallelOptions, async (request, ct) =>
        {
            try
            {
                var result = await ExtractAllAsync(request.DocumentContent, request.Format, context);
                result.RequestId = request.RequestId;
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch extraction failed for request {RequestId}", request.RequestId);
                results.Add(new ExtractionResult
                {
                    RequestId = request.RequestId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        });

        return results;
    }

    private async Task<IDocumentExtractionProvider> SelectProviderAsync(
        string format,
        ExtractionContext context)
    {
        // 1. Check context for preferred provider
        if (context.AdditionalOptions.TryGetValue("PreferredProvider", out var preferredProvider))
        {
            var provider = await _providerFactory.GetProviderAsync(preferredProvider.ToString()!);
            if (provider.SupportsFormat(format))
            {
                return provider;
            }
        }

        // 2. Find providers supporting this format
        var providers = await _providerFactory.GetProvidersAsync();
        var supportingProviders = providers
            .Where(p => p.SupportsFormat(format))
            .OrderByDescending(p => GetProviderScore(p, context))
            .ToList();

        if (!supportingProviders.Any())
        {
            throw new ExtractionNotSupportedException($"No provider supports format {format}");
        }

        // 3. Return highest-scoring provider
        return supportingProviders.First();
    }

    private int GetProviderScore(IDocumentExtractionProvider provider, ExtractionContext context)
    {
        var score = 0;

        if (context.ExtractText && provider.Capabilities.SupportsTextExtraction)
            score += 10;

        if (context.ExtractMetadata && provider.Capabilities.SupportsMetadataExtraction)
            score += 5;

        if (context.ExtractImages && provider.Capabilities.SupportsImageExtraction)
            score += 5;

        if (context.ExtractTables && provider.Capabilities.SupportsTableExtraction)
            score += 5;

        if (context.PreserveLayout && provider.Capabilities.SupportsLayoutPreservation)
            score += 3;

        return score;
    }

    private bool IsScannedDocument(ExtractionResult result, string format)
    {
        // PDF with no text or very little text likely scanned
        if (format.Equals("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return string.IsNullOrWhiteSpace(result.ExtractedText) ||
                   result.ExtractedText.Length < 50;  // Threshold
        }

        return false;
    }

    private async Task<ExtractionResult> ApplyOcrExtractionAsync(
        byte[] documentContent,
        string format,
        ExtractionContext context,
        ExtractionResult currentResult)
    {
        // Use OCR service to extract text from images
        var ocrResult = await _ocrService.ExtractTextAsync(documentContent, format, new OcrContext
        {
            Languages = context.OcrLanguages,
            RequestingApplication = context.RequestingApplication,
            UserId = context.UserId
        });

        if (ocrResult.Success)
        {
            currentResult.ExtractedText = ocrResult.Text;
            currentResult.Metadata ??= new DocumentMetadata();
            currentResult.Metadata.CustomProperties["OcrConfidence"] = ocrResult.Confidence;
            currentResult.Metadata.CustomProperties["OcrLanguage"] = ocrResult.DetectedLanguage;
        }

        return currentResult;
    }
}
```

---

### 2. Provider Implementations

#### TikaExtractionProvider (Universal Format Support)

**Responsibilities:**
- Extract content from 100+ document formats
- Provide universal fallback extraction
- Handle metadata extraction
- Support basic text extraction

**Implementation Pattern:**
```csharp
public class TikaExtractionProvider : IDocumentExtractionProvider
{
    private readonly TikaWrapper _tika;
    private readonly ILogger<TikaExtractionProvider> _logger;

    public string ProviderName => "tika";

    public DocumentExtractionCapabilities Capabilities => new()
    {
        SupportsTextExtraction = true,
        SupportsMetadataExtraction = true,
        SupportsImageExtraction = false,  // Limited
        SupportsTableExtraction = false,  // Limited
        SupportsOcr = false,
        SupportsLayoutPreservation = false,
        SupportedFormats = new[] { "pdf", "docx", "doc", "xlsx", "xls", "pptx", "ppt", "html", "txt", "rtf", /* 100+ more */ },
        MaxDocumentSize = 100 * 1024 * 1024  // 100MB
    };

    public async Task<ExtractionResult> ExtractAsync(
        byte[] documentContent,
        string format,
        ExtractionContext context)
    {
        var result = new ExtractionResult
        {
            Success = true,
            ProviderName = ProviderName
        };

        // Extract text if requested
        if (context.ExtractText)
        {
            result.ExtractedText = await _tika.ExtractTextAsync(documentContent);
        }

        // Extract metadata if requested
        if (context.ExtractMetadata)
        {
            var tikaMetadata = await _tika.ExtractMetadataAsync(documentContent);
            result.Metadata = MapTikaMetadata(tikaMetadata, context.MetadataDepth);
        }

        return result;
    }

    public bool SupportsFormat(string format)
    {
        return Capabilities.SupportedFormats.Contains(format, StringComparer.OrdinalIgnoreCase);
    }

    public IEnumerable<string> GetSupportedFormats()
    {
        return Capabilities.SupportedFormats;
    }

    private DocumentMetadata MapTikaMetadata(IDictionary<string, object> tikaMetadata, MetadataDepth depth)
    {
        var metadata = new DocumentMetadata();

        // Basic metadata
        if (tikaMetadata.TryGetValue("title", out var title))
            metadata.Title = title?.ToString();

        if (tikaMetadata.TryGetValue("author", out var author))
            metadata.Author = author?.ToString();

        if (depth >= MetadataDepth.Standard)
        {
            if (tikaMetadata.TryGetValue("subject", out var subject))
                metadata.Subject = subject?.ToString();

            if (tikaMetadata.TryGetValue("keywords", out var keywords))
                metadata.Keywords = keywords?.ToString()?.Split(',');

            if (tikaMetadata.TryGetValue("created", out var created) && DateTime.TryParse(created?.ToString(), out var createdDate))
                metadata.CreatedDate = createdDate;

            if (tikaMetadata.TryGetValue("modified", out var modified) && DateTime.TryParse(modified?.ToString(), out var modifiedDate))
                metadata.ModifiedDate = modifiedDate;
        }

        if (depth >= MetadataDepth.Extended)
        {
            // Add all remaining metadata as custom properties
            foreach (var kvp in tikaMetadata)
            {
                if (!IsStandardMetadataKey(kvp.Key))
                {
                    metadata.CustomProperties[kvp.Key] = kvp.Value;
                }
            }
        }

        return metadata;
    }

    private bool IsStandardMetadataKey(string key)
    {
        return key.Equals("title", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("author", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("subject", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("keywords", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("created", StringComparison.OrdinalIgnoreCase) ||
               key.Equals("modified", StringComparison.OrdinalIgnoreCase);
    }
}
```

---

#### OpenXmlExtractionProvider (Office Formats)

**Responsibilities:**
- Extract text from Word, Excel, PowerPoint
- Extract tables with structure
- Extract images
- Preserve document structure

**Implementation Pattern:**
```csharp
public class OpenXmlExtractionProvider : IDocumentExtractionProvider
{
    public string ProviderName => "openxml";

    public DocumentExtractionCapabilities Capabilities => new()
    {
        SupportsTextExtraction = true,
        SupportsMetadataExtraction = true,
        SupportsImageExtraction = true,
        SupportsTableExtraction = true,
        SupportsOcr = false,
        SupportsLayoutPreservation = true,
        SupportedFormats = new[] { "docx", "xlsx", "pptx" },
        MaxDocumentSize = 50 * 1024 * 1024  // 50MB
    };

    public async Task<ExtractionResult> ExtractAsync(
        byte[] documentContent,
        string format,
        ExtractionContext context)
    {
        return format.ToLower() switch
        {
            "docx" => await ExtractFromWordAsync(documentContent, context),
            "xlsx" => await ExtractFromExcelAsync(documentContent, context),
            "pptx" => await ExtractFromPowerPointAsync(documentContent, context),
            _ => throw new ExtractionNotSupportedException($"Format {format} not supported by OpenXML provider")
        };
    }

    private async Task<ExtractionResult> ExtractFromWordAsync(byte[] content, ExtractionContext context)
    {
        using var stream = new MemoryStream(content);
        using var document = WordprocessingDocument.Open(stream, false);

        var result = new ExtractionResult
        {
            Success = true,
            ProviderName = ProviderName
        };

        // Extract text
        if (context.ExtractText)
        {
            var body = document.MainDocumentPart?.Document?.Body;
            result.ExtractedText = body?.InnerText ?? "";
        }

        // Extract metadata
        if (context.ExtractMetadata)
        {
            result.Metadata = ExtractWordMetadata(document, context.MetadataDepth);
        }

        // Extract images
        if (context.ExtractImages)
        {
            result.Images = ExtractWordImages(document);
        }

        // Extract tables
        if (context.ExtractTables)
        {
            result.Tables = ExtractWordTables(document);
        }

        return result;
    }

    private DocumentMetadata ExtractWordMetadata(WordprocessingDocument document, MetadataDepth depth)
    {
        var metadata = new DocumentMetadata();
        var coreProps = document.PackageProperties;

        metadata.Title = coreProps.Title;
        metadata.Author = coreProps.Creator;
        metadata.Subject = coreProps.Subject;
        metadata.Keywords = coreProps.Keywords?.Split(',');
        metadata.CreatedDate = coreProps.Created;
        metadata.ModifiedDate = coreProps.Modified;

        if (depth >= MetadataDepth.Standard)
        {
            var body = document.MainDocumentPart?.Document?.Body;
            if (body != null)
            {
                metadata.WordCount = body.Descendants<Paragraph>().Sum(p => p.InnerText.Split(' ').Length);
                metadata.PageCount = body.Descendants<PageCount>().FirstOrDefault()?.Val ?? 1;
            }
        }

        return metadata;
    }

    private IEnumerable<ExtractedImage> ExtractWordImages(WordprocessingDocument document)
    {
        var images = new List<ExtractedImage>();
        var imageParts = document.MainDocumentPart?.ImageParts;

        if (imageParts != null)
        {
            int index = 0;
            foreach (var imagePart in imageParts)
            {
                using var stream = imagePart.GetStream();
                using var ms = new MemoryStream();
                stream.CopyTo(ms);

                images.Add(new ExtractedImage
                {
                    Index = index++,
                    Content = ms.ToArray(),
                    Format = GetImageFormat(imagePart.ContentType),
                    Size = ms.Length
                });
            }
        }

        return images;
    }

    private IEnumerable<ExtractedTable> ExtractWordTables(WordprocessingDocument document)
    {
        var tables = new List<ExtractedTable>();
        var wordTables = document.MainDocumentPart?.Document?.Body?.Descendants<Table>();

        if (wordTables != null)
        {
            int index = 0;
            foreach (var table in wordTables)
            {
                var rows = table.Descendants<TableRow>().ToList();
                var cells = rows.Select(row => row.Descendants<TableCell>()
                    .Select(cell => cell.InnerText)
                    .ToArray())
                    .ToArray();

                tables.Add(new ExtractedTable
                {
                    Index = index++,
                    RowCount = cells.Length,
                    ColumnCount = cells.FirstOrDefault()?.Length ?? 0,
                    Cells = cells
                });
            }
        }

        return tables;
    }
}
```

---

## Data Flow

### Sequence: Text Extraction with OCR Fallback

```
┌───────────┐     ┌──────────────────┐     ┌──────────┐     ┌──────────┐
│Application│     │ExtractionService │     │ Provider │     │OcrService│
└─────┬─────┘     └────────┬─────────┘     └─────┬────┘     └─────┬────┘
      │                    │                      │                 │
      │ ExtractText(...)   │                      │                 │
      ├───────────────────>│                      │                 │
      │                    │                      │                 │
      │                    │ SelectProvider()     │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │ ExtractAsync()       │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │ Extraction result    │                 │
      │                    │<─────────────────────┤                 │
      │                    │                      │                 │
      │                    │ IsScanned? (Yes)     │                 │
      │                    │                      │                 │
      │                    │ ExtractTextAsync()   │                 │
      │                    ├─────────────────────────────────────────>│
      │                    │                      │                 │
      │                    │ OCR result           │                 │
      │                    │<─────────────────────────────────────────┤
      │                    │                      │                 │
      │ ExtractionResult   │                      │                 │
      │<───────────────────┤                      │                 │
      │                    │                      │                 │
```

---

## Design Patterns

### 1. Provider Pattern
- Multiple extraction engines via pluggable providers
- Provider selection based on format and capabilities
- Fallback providers for resilience

### 2. Strategy Pattern
- Different extraction strategies per format
- Capability-based strategy selection
- OCR integration as fallback strategy

### 3. Composite Pattern
- Aggregate extraction results from multiple providers
- Combine text, metadata, images, tables into single result

---

## Performance Optimizations

### 1. Selective Extraction
- Extract only requested content types
- Skip unnecessary processing
- Provider optimization based on flags

### 2. Batch Processing
- Parallel extraction processing
- Configurable degree of parallelism
- Resource pooling

### 3. Streaming for Large Documents
- Stream large files to avoid memory issues
- Chunked processing where applicable

---

## Thread Safety

- Service is thread-safe (stateless)
- Providers must be thread-safe
- Concurrent extractions supported
- Provider pooling handles concurrent access

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [OcrService Architecture](../OcrService/architecture.md)
