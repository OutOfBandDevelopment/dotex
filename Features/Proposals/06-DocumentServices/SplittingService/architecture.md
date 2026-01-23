# Document Splitting Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Splitting Service
**Last Updated:** 2026-01-23

---

## Architectural Overview

The Document Splitting Service implements a **Provider Pattern** with **Context-Based Splitting** for dividing documents into smaller parts based on pages, size, bookmarks, or sections. Applications provide operational context that providers use to optimize splitting behavior.

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│    (Document Processor, Batch Splitter, Archive Manager)    │
└────────────────────┬────────────────────────────────────────┘
                     │ SplitByPagesAsync(content, ranges, context)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│          IDocumentSplittingService                           │
│  - SplitByPagesAsync(content, ranges, context)               │
│  - SplitEveryNPagesAsync(content, N, context)                │
│  - SplitBySizeAsync(content, maxSize, context)               │
│  - SplitByBookmarksAsync(content, depth, context)            │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼────────────┬────────────┬────────────┐
         ↓           ↓            ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│  PDFSharp    │ │ iText  │ │ PDFBox  │ │ OpenXML  │ │  Custom  │
│  Provider    │ │Provider│ │ Provider│ │ Provider │ │ Provider │
└──────┬───────┘ └────┬───┘ └────┬────┘ └────┬─────┘ └────┬─────┘
       │              │          │            │            │
       ↓              ↓          ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│ PDF Split    │ │Advanced│ │Java-Based│ │Office Doc│ │Text/Other│
│ Basic Ops    │ │PDF Ops │ │PDF Split │ │ Splitting│ │ Formats  │
└──────────────┘ └────────┘ └─────────┘ └──────────┘ └──────────┘
```

---

## Core Components

### 1. DocumentSplittingService (Main Entry Point)

**Responsibilities:**
- Coordinate document splitting across providers
- Select appropriate provider based on format and split mode
- Handle split validation
- Manage metadata preservation
- Track page accounting

**Implementation Pattern:**
```csharp
public class DocumentSplittingService : IDocumentSplittingService
{
    private readonly IDocumentSplittingProviderFactory _providerFactory;
    private readonly DocumentSplittingOptions _options;
    private readonly ILogger<DocumentSplittingService> _logger;

    public async Task<SplittingResult> SplitByPagesAsync(
        byte[] documentContent,
        string format,
        int[][] pageRanges,
        SplittingContext? context = null)
    {
        context ??= new SplittingContext();
        var startTime = DateTime.UtcNow;

        // 1. Select provider
        var provider = await SelectProviderAsync(format, SplitMode.PageRanges, context);
        _logger.LogDebug("Selected provider {Provider} for page-based splitting",
            provider.ProviderName);

        // 2. Validate page ranges
        ValidatePageRanges(pageRanges);

        SplittingResult result;
        try
        {
            // 3. Perform split
            context.AdditionalOptions["PageRanges"] = pageRanges;
            result = await provider.SplitAsync(documentContent, format, SplitMode.PageRanges, context);
            result.Duration = DateTime.UtcNow - startTime;

            // 4. Validate split if requested
            if (context.ValidateOutput)
            {
                var validation = await ValidateSplitAsync(result);
                if (!validation.IsValid)
                {
                    throw new SplitValidationFailedException(
                        $"Split validation failed: {validation.ErrorMessage}");
                }
            }

            _logger.LogInformation(
                "Split document into {Parts} parts using {Provider} in {Duration}ms",
                result.TotalParts, provider.ProviderName, result.Duration.TotalMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Splitting failed using provider {Provider}", provider.ProviderName);
            throw new SplittingFailedException($"Failed to split document: {ex.Message}", ex);
        }
    }

    public async Task<SplittingResult> SplitEveryNPagesAsync(
        byte[] documentContent,
        string format,
        int pageCount,
        SplittingContext? context = null)
    {
        context ??= new SplittingContext();

        // Get total page count
        var provider = await SelectProviderAsync(format, SplitMode.EveryNPages, context);
        var totalPages = await GetPageCountAsync(documentContent, format, provider);

        // Generate page ranges
        var pageRanges = GeneratePageRanges(totalPages, pageCount, context.OverlapPages);

        // Split using generated ranges
        return await SplitByPagesAsync(documentContent, format, pageRanges, context);
    }

    public async Task<SplittingResult> SplitBySizeAsync(
        byte[] documentContent,
        string format,
        long maxSizeBytes,
        SplittingContext? context = null)
    {
        context ??= new SplittingContext();
        context.AdditionalOptions["MaxSizeBytes"] = maxSizeBytes;

        var provider = await SelectProviderAsync(format, SplitMode.BySize, context);
        return await provider.SplitAsync(documentContent, format, SplitMode.BySize, context);
    }

    public async Task<SplittingResult> SplitByBookmarksAsync(
        byte[] documentContent,
        int bookmarkDepth = 1,
        SplittingContext? context = null)
    {
        context ??= new SplittingContext();
        context.AdditionalOptions["BookmarkDepth"] = bookmarkDepth;

        var provider = await SelectProviderAsync("pdf", SplitMode.ByBookmarks, context);
        return await provider.SplitAsync(documentContent, "pdf", SplitMode.ByBookmarks, context);
    }

    public async Task<ValidationResult> ValidateSplitAsync(SplittingResult result)
    {
        if (!result.Success)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "Splitting failed" };
        }

        if (!result.Parts.Any())
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "No split parts generated" };
        }

        // Verify page accounting
        var totalPages = result.Parts.Sum(p => p.PageCount);
        if (totalPages != result.TotalSplitPages)
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Page count mismatch: {totalPages} vs {result.TotalSplitPages}"
            };
        }

        // Verify all parts have content
        if (result.Parts.Any(p => p.Content == null || p.Content.Length == 0))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = "One or more parts have no content"
            };
        }

        return new ValidationResult { IsValid = true };
    }

    private int[][] GeneratePageRanges(int totalPages, int pagesPerPart, int overlap)
    {
        var ranges = new List<int[]>();
        int currentPage = 1;

        while (currentPage <= totalPages)
        {
            int endPage = Math.Min(currentPage + pagesPerPart - 1, totalPages);
            ranges.Add(new[] { currentPage, endPage });
            currentPage = endPage + 1 - overlap;

            if (overlap > 0 && currentPage > endPage)
                break;
        }

        return ranges.ToArray();
    }

    private void ValidatePageRanges(int[][] pageRanges)
    {
        if (pageRanges == null || pageRanges.Length == 0)
        {
            throw new InvalidSplitRangeException("Page ranges cannot be empty");
        }

        foreach (var range in pageRanges)
        {
            if (range.Length != 2)
            {
                throw new InvalidSplitRangeException("Each range must have start and end page");
            }

            if (range[0] < 1 || range[1] < range[0])
            {
                throw new InvalidSplitRangeException($"Invalid range: {range[0]}-{range[1]}");
            }
        }
    }
}
```

---

### 2. Provider Implementations

#### PDFSharpSplittingProvider (Basic PDF Splitting)

**Implementation Pattern:**
```csharp
public class PDFSharpSplittingProvider : IDocumentSplittingProvider
{
    public string ProviderName => "pdfsharp";

    public DocumentSplittingCapabilities Capabilities => new()
    {
        SupportsPageSplitting = true,
        SupportsSizeSplitting = false,
        SupportsBookmarkSplitting = false,
        SupportsSectionSplitting = false,
        SupportsOverlap = true,
        SupportsMetadataPreservation = true,
        SupportedFormats = new[] { "pdf" },
        SupportedModes = new[] { SplitMode.SinglePages, SplitMode.PageRanges, SplitMode.EveryNPages }
    };

    public async Task<SplittingResult> SplitAsync(
        byte[] documentContent,
        string format,
        SplitMode mode,
        SplittingContext context)
    {
        var startTime = DateTime.UtcNow;
        var parts = new List<SplitPart>();

        using var sourceStream = new MemoryStream(documentContent);
        var sourceDocument = PdfReader.Open(sourceStream, PdfDocumentOpenMode.Import);

        var pageRanges = GetPageRanges(mode, context, sourceDocument.PageCount);

        int partNumber = 1;
        foreach (var range in pageRanges)
        {
            var outputDocument = new PdfDocument();

            // Copy metadata
            if (context.PreserveMetadata)
            {
                outputDocument.Info.Title = sourceDocument.Info.Title;
                outputDocument.Info.Author = sourceDocument.Info.Author;
                outputDocument.Info.Subject = $"{sourceDocument.Info.Subject} - Part {partNumber}";
            }

            // Copy pages
            for (int pageNum = range[0]; pageNum <= range[1]; pageNum++)
            {
                outputDocument.AddPage(sourceDocument.Pages[pageNum - 1]);
            }

            // Save to byte array
            using var outputStream = new MemoryStream();
            outputDocument.Save(outputStream);
            var partContent = outputStream.ToArray();

            parts.Add(new SplitPart
            {
                PartNumber = partNumber++,
                Content = partContent,
                StartPage = range[0],
                EndPage = range[1],
                PageCount = range[1] - range[0] + 1,
                Size = partContent.Length,
                Metadata = new Dictionary<string, object>
                {
                    ["PartNumber"] = partNumber - 1,
                    ["TotalParts"] = pageRanges.Length,
                    ["SourceDocument"] = sourceDocument.Info.Title ?? "Unknown"
                }
            });
        }

        return new SplittingResult
        {
            Success = true,
            Parts = parts,
            TotalParts = parts.Count,
            SourcePageCount = sourceDocument.PageCount,
            TotalSplitPages = parts.Sum(p => p.PageCount),
            Duration = DateTime.UtcNow - startTime,
            ProviderName = ProviderName
        };
    }

    private int[][] GetPageRanges(SplitMode mode, SplittingContext context, int totalPages)
    {
        if (mode == SplitMode.PageRanges && context.AdditionalOptions.TryGetValue("PageRanges", out var ranges))
        {
            return (int[][])ranges;
        }

        if (mode == SplitMode.SinglePages)
        {
            return Enumerable.Range(1, totalPages).Select(p => new[] { p, p }).ToArray();
        }

        throw new SplittingNotSupportedException($"Split mode {mode} not supported by {ProviderName}");
    }
}
```

---

## Data Flow

### Sequence: Document Splitting

```
┌───────────┐     ┌──────────────────┐     ┌──────────┐     ┌──────────┐
│Application│     │SplittingService  │     │ Provider │     │  Parts   │
└─────┬─────┘     └────────┬─────────┘     └─────┬────┘     └─────┬────┘
      │                    │                      │                 │
      │ SplitByPagesAsync()│                      │                 │
      ├───────────────────>│                      │                 │
      │                    │                      │                 │
      │                    │ SelectProvider()     │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │ SplitAsync()         │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │                      │ Split document  │
      │                    │                      ├────────────────>│
      │                    │                      │                 │
      │                    │                      │ Part 1, 2, 3... │
      │                    │                      │<────────────────┤
      │                    │                      │                 │
      │                    │ SplittingResult      │                 │
      │                    │<─────────────────────┤                 │
      │                    │                      │                 │
      │                    │ ValidateSplit()      │                 │
      │                    │                      │                 │
      │ SplittingResult    │                      │                 │
      │<───────────────────┤                      │                 │
      │                    │                      │                 │
```

---

## Design Patterns

### 1. Provider Pattern
- Multiple splitting engines via pluggable providers
- Provider selection based on format and mode
- Fallback providers for resilience

### 2. Strategy Pattern
- Different splitting strategies per mode
- Context-driven splitting behavior
- Provider-specific optimizations

### 3. Factory Pattern
- Provider factory creates provider instances
- Provider configuration via dependency injection

---

## Performance Optimizations

### 1. Parallel Processing
- Split multiple documents concurrently
- Configurable degree of parallelism
- Resource pooling

### 2. Streaming Support
- Stream large documents to avoid memory issues
- Chunked processing for batch operations
- Async I/O throughout

### 3. Metadata Caching
- Cache document metadata for reuse
- Avoid redundant metadata extraction

---

## Thread Safety

- Service is thread-safe (stateless or properly synchronized)
- Providers must be thread-safe
- Concurrent splitting operations supported

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
