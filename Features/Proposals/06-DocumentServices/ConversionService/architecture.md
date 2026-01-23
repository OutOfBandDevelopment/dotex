# Document Conversion Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Conversion Service
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Document Conversion Service implements a **Provider Pattern** with **Context-Based Conversion** for transforming documents between formats using multiple conversion engines. Applications provide operational context that providers use to optimize conversion quality, speed, and options.

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│    (Report Generator, Document Processor, API Gateway)       │
└────────────────────┬────────────────────────────────────────┘
                     │ ConvertAsync(content, source, target, context)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│          IDocumentConversionService                          │
│  - ConvertAsync(content, source, target, context)            │
│  - ConvertBatchAsync(requests, context)                      │
│  - IsSupportedAsync(source, target)                          │
│  - GetSupportedTargetFormatsAsync(source)                    │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼────────────┬────────────┬────────────┐
         ↓           ↓            ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│ LibreOffice  │ │  Tika  │ │PDFSharp │ │ImageMagic│ │Playwright│
│  Provider    │ │Provider│ │ Provider│ │ Provider │ │ Provider │
└──────┬───────┘ └────┬───┘ └────┬────┘ └────┬─────┘ └────┬─────┘
       │              │          │            │            │
       ↓              ↓          ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│ Office →PDF  │ │Metadata│ │PDF Ops  │ │Image Cvt │ │ HTML→PDF │
│ PDF → Office │ │Extract │ │.NET Only│ │Format Cvt│ │ HTML→IMG │
└──────────────┘ └────────┘ └─────────┘ └──────────┘ └──────────┘
```

---

## Core Components

### 1. DocumentConversionService (Main Entry Point)

**Responsibilities:**
- Coordinate document conversion across providers
- Select appropriate provider based on source/target formats
- Handle provider fallback on failure
- Manage conversion caching
- Validate conversion results

**Key Design Decisions:**
- **Multi-provider support** - Route requests to appropriate conversion engine
- **Context propagation** - Pass operational context to all providers
- **Fallback strategy** - Try alternate providers on failure
- **Result caching** - Cache conversions for identical content
- **Format detection** - Auto-detect source format when not specified

**Implementation Pattern:**
```csharp
public class DocumentConversionService : IDocumentConversionService
{
    private readonly IDocumentConversionProviderFactory _providerFactory;
    private readonly IMediaTypeDetectionService _mediaTypeDetection;
    private readonly IMemoryCache _cache;
    private readonly DocumentConversionOptions _options;
    private readonly ILogger<DocumentConversionService> _logger;

    public async Task<ConversionResult> ConvertAsync(
        byte[] sourceContent,
        string sourceFormat,
        string targetFormat,
        ConversionContext? context = null)
    {
        context ??= new ConversionContext();
        var startTime = DateTime.UtcNow;

        // 1. Check cache
        var cacheKey = GenerateCacheKey(sourceContent, sourceFormat, targetFormat, context);
        if (_options.EnableCaching && _cache.TryGetValue<ConversionResult>(cacheKey, out var cachedResult))
        {
            _logger.LogDebug("Cache hit for conversion {Source} → {Target}", sourceFormat, targetFormat);
            return cachedResult;
        }

        // 2. Detect source format if needed
        if (string.IsNullOrEmpty(sourceFormat))
        {
            sourceFormat = await _mediaTypeDetection.DetectAsync(sourceContent);
            _logger.LogDebug("Detected source format: {Format}", sourceFormat);
        }

        // 3. Validate conversion is supported
        if (!await IsSupportedAsync(sourceFormat, targetFormat))
        {
            throw new ConversionNotSupportedException(
                $"Conversion from {sourceFormat} to {targetFormat} is not supported");
        }

        // 4. Select provider
        var provider = await SelectProviderAsync(sourceFormat, targetFormat, context);
        _logger.LogDebug("Selected provider {Provider} for {Source} → {Target}",
            provider.ProviderName, sourceFormat, targetFormat);

        ConversionResult result;
        try
        {
            // 5. Perform conversion
            result = await provider.ConvertAsync(sourceContent, sourceFormat, targetFormat, context);
            result.Duration = DateTime.UtcNow - startTime;

            // 6. Validate output if requested
            if (context.ValidateOutput)
            {
                var validation = await ValidateConversionAsync(result);
                if (!validation.IsValid)
                {
                    throw new ValidationFailedException(
                        $"Conversion validation failed: {validation.ErrorMessage}");
                }
            }

            // 7. Cache result
            if (_options.EnableCaching)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromHours(_options.CacheDurationHours));
            }

            _logger.LogInformation(
                "Converted document {Source} → {Target} using {Provider} in {Duration}ms (size: {SourceSize} → {TargetSize} bytes)",
                sourceFormat, targetFormat, provider.ProviderName, result.Duration.TotalMilliseconds,
                result.SourceSize, result.ConvertedSize);

            return result;
        }
        catch (ConversionNotSupportedException)
        {
            throw;  // Don't retry unsupported conversions
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Conversion failed using provider {Provider}", provider.ProviderName);

            // Try fallback provider
            if (_options.EnableFallback)
            {
                var fallbackProvider = await GetFallbackProviderAsync(sourceFormat, targetFormat, provider.ProviderName);
                if (fallbackProvider != null)
                {
                    _logger.LogInformation("Retrying conversion with fallback provider {Provider}",
                        fallbackProvider.ProviderName);
                    result = await fallbackProvider.ConvertAsync(sourceContent, sourceFormat, targetFormat, context);
                    result.Duration = DateTime.UtcNow - startTime;
                    return result;
                }
            }

            throw new ConversionFailedException(
                $"Failed to convert {sourceFormat} to {targetFormat}: {ex.Message}", ex);
        }
    }

    private async Task<IDocumentConversionProvider> SelectProviderAsync(
        string sourceFormat,
        string targetFormat,
        ConversionContext context)
    {
        // 1. Check context for preferred provider
        if (context.AdditionalOptions.TryGetValue("PreferredProvider", out var preferredProvider))
        {
            var provider = await _providerFactory.GetProviderAsync(preferredProvider.ToString()!);
            if (provider.SupportsConversion(sourceFormat, targetFormat))
            {
                return provider;
            }
        }

        // 2. Find providers supporting this conversion
        var providers = await _providerFactory.GetProvidersAsync();
        var supportingProviders = providers
            .Where(p => p.SupportsConversion(sourceFormat, targetFormat))
            .OrderByDescending(p => GetProviderScore(p, context))
            .ToList();

        if (!supportingProviders.Any())
        {
            throw new ConversionNotSupportedException(
                $"No provider supports conversion from {sourceFormat} to {targetFormat}");
        }

        // 3. Return highest-scoring provider
        return supportingProviders.First();
    }

    private int GetProviderScore(IDocumentConversionProvider provider, ConversionContext context)
    {
        var score = 0;

        // Prefer providers with quality control if high quality requested
        if (context.Quality >= ConversionQuality.High && provider.Capabilities.SupportsQualityControl)
            score += 10;

        // Prefer providers with metadata preservation if enabled
        if (context.PreserveMetadata && provider.Capabilities.SupportsMetadataPreservation)
            score += 5;

        // Prefer providers with batch support for batch operations
        if (provider.Capabilities.SupportsBatchConversion)
            score += 3;

        return score;
    }

    public async Task<IEnumerable<ConversionResult>> ConvertBatchAsync(
        IEnumerable<ConversionRequest> requests,
        ConversionContext? context = null)
    {
        context ??= new ConversionContext();

        // Parallel processing with degree of parallelism
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxConcurrentConversions
        };

        var results = new ConcurrentBag<ConversionResult>();

        await Parallel.ForEachAsync(requests, parallelOptions, async (request, ct) =>
        {
            try
            {
                var result = await ConvertAsync(
                    request.SourceContent,
                    request.SourceFormat,
                    request.TargetFormat,
                    context);
                result.RequestId = request.RequestId;
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Batch conversion failed for request {RequestId}", request.RequestId);
                results.Add(new ConversionResult
                {
                    RequestId = request.RequestId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        });

        return results;
    }

    public async Task<bool> IsSupportedAsync(string sourceFormat, string targetFormat)
    {
        var providers = await _providerFactory.GetProvidersAsync();
        return providers.Any(p => p.SupportsConversion(sourceFormat, targetFormat));
    }

    public async Task<IEnumerable<string>> GetSupportedTargetFormatsAsync(string sourceFormat)
    {
        var providers = await _providerFactory.GetProvidersAsync();
        var targetFormats = new HashSet<string>();

        foreach (var provider in providers)
        {
            var conversions = provider.GetSupportedConversions()
                .Where(c => c.SourceFormat.Equals(sourceFormat, StringComparison.OrdinalIgnoreCase));

            foreach (var conversion in conversions)
            {
                targetFormats.Add(conversion.TargetFormat);
            }
        }

        return targetFormats;
    }

    public async Task<ValidationResult> ValidateConversionAsync(ConversionResult result)
    {
        if (!result.Success)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "Conversion failed" };
        }

        if (result.ConvertedContent == null || result.ConvertedContent.Length == 0)
        {
            return new ValidationResult { IsValid = false, ErrorMessage = "Converted content is empty" };
        }

        // Validate format matches target
        var detectedFormat = await _mediaTypeDetection.DetectAsync(result.ConvertedContent);
        if (!detectedFormat.Equals(result.TargetFormat, StringComparison.OrdinalIgnoreCase))
        {
            return new ValidationResult
            {
                IsValid = false,
                ErrorMessage = $"Expected {result.TargetFormat}, got {detectedFormat}"
            };
        }

        return new ValidationResult { IsValid = true };
    }

    private string GenerateCacheKey(byte[] content, string sourceFormat, string targetFormat, ConversionContext context)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(content);
        var hashString = Convert.ToBase64String(hash);
        return $"{hashString}|{sourceFormat}|{targetFormat}|{context.Quality}";
    }
}
```

---

### 2. Provider Implementations

#### LibreOfficeConversionProvider (Office Formats ↔ PDF)

**Responsibilities:**
- Convert Office formats to PDF
- Convert PDF to Office formats (limited)
- Preserve document formatting and metadata
- Handle complex documents with images, tables, charts

**Implementation Pattern:**
```csharp
public class LibreOfficeConversionProvider : IDocumentConversionProvider
{
    private readonly string _libreOfficePath;
    private readonly ILogger<LibreOfficeConversionProvider> _logger;

    public string ProviderName => "libreoffice";

    public DocumentConversionCapabilities Capabilities => new()
    {
        SupportsBatchConversion = true,
        SupportsMetadataPreservation = true,
        SupportsQualityControl = true,
        MaxInputSize = 100 * 1024 * 1024,  // 100MB
        SupportedQualityLevels = new[] { ConversionQuality.Medium, ConversionQuality.High, ConversionQuality.Maximum }
    };

    public async Task<ConversionResult> ConvertAsync(
        byte[] sourceContent,
        string sourceFormat,
        string targetFormat,
        ConversionContext context)
    {
        var tempInputFile = Path.GetTempFileName();
        var tempOutputDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempOutputDir);

        try
        {
            // Write source content to temp file
            await File.WriteAllBytesAsync(tempInputFile + GetExtension(sourceFormat), sourceContent);

            // Build LibreOffice command
            var arguments = BuildConversionArguments(sourceFormat, targetFormat, context, tempInputFile, tempOutputDir);

            // Execute LibreOffice conversion
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _libreOfficePath,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                var error = await process.StandardError.ReadToEndAsync();
                throw new ConversionFailedException($"LibreOffice conversion failed: {error}");
            }

            // Read converted file
            var outputFile = Directory.GetFiles(tempOutputDir).FirstOrDefault();
            if (outputFile == null)
            {
                throw new ConversionFailedException("LibreOffice did not produce output file");
            }

            var convertedContent = await File.ReadAllBytesAsync(outputFile);

            return new ConversionResult
            {
                Success = true,
                ConvertedContent = convertedContent,
                SourceFormat = sourceFormat,
                TargetFormat = targetFormat,
                SourceSize = sourceContent.Length,
                ConvertedSize = convertedContent.Length,
                ProviderName = ProviderName
            };
        }
        finally
        {
            // Cleanup temp files
            try
            {
                if (File.Exists(tempInputFile))
                    File.Delete(tempInputFile);
                if (Directory.Exists(tempOutputDir))
                    Directory.Delete(tempOutputDir, true);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to cleanup temp files");
            }
        }
    }

    public bool SupportsConversion(string sourceFormat, string targetFormat)
    {
        var supportedConversions = new[]
        {
            ("docx", "pdf"), ("doc", "pdf"), ("odt", "pdf"),
            ("xlsx", "pdf"), ("xls", "pdf"), ("ods", "pdf"),
            ("pptx", "pdf"), ("ppt", "pdf"), ("odp", "pdf"),
            ("docx", "html"), ("doc", "html"),
            ("xlsx", "html"), ("xls", "html")
        };

        return supportedConversions.Any(c =>
            c.Item1.Equals(sourceFormat, StringComparison.OrdinalIgnoreCase) &&
            c.Item2.Equals(targetFormat, StringComparison.OrdinalIgnoreCase));
    }

    public IEnumerable<FormatConversion> GetSupportedConversions()
    {
        return new[]
        {
            new FormatConversion { SourceFormat = "docx", TargetFormat = "pdf" },
            new FormatConversion { SourceFormat = "doc", TargetFormat = "pdf" },
            new FormatConversion { SourceFormat = "xlsx", TargetFormat = "pdf" },
            new FormatConversion { SourceFormat = "pptx", TargetFormat = "pdf" },
            new FormatConversion { SourceFormat = "docx", TargetFormat = "html" },
            // ... more conversions
        };
    }

    private string BuildConversionArguments(
        string sourceFormat,
        string targetFormat,
        ConversionContext context,
        string inputFile,
        string outputDir)
    {
        var args = new List<string>
        {
            "--headless",
            "--convert-to", targetFormat,
            "--outdir", $"\"{outputDir}\"",
            $"\"{inputFile}\""
        };

        // Add quality-specific options
        if (context.Quality == ConversionQuality.Maximum)
        {
            args.Add("--nolockcheck");
        }

        return string.Join(" ", args);
    }
}
```

---

#### PlaywrightConversionProvider (HTML → PDF/Image)

**Responsibilities:**
- Convert HTML to PDF with CSS and JavaScript support
- Convert HTML to images (PNG, JPEG)
- Handle modern web content rendering
- Support custom viewport and wait conditions

**Implementation Pattern:**
```csharp
public class PlaywrightConversionProvider : IDocumentConversionProvider
{
    private readonly IPlaywright _playwright;
    private readonly ILogger<PlaywrightConversionProvider> _logger;

    public string ProviderName => "playwright";

    public DocumentConversionCapabilities Capabilities => new()
    {
        SupportsBatchConversion = true,
        SupportsMetadataPreservation = false,
        SupportsQualityControl = true,
        MaxInputSize = 10 * 1024 * 1024,  // 10MB HTML
        SupportedQualityLevels = Enum.GetValues<ConversionQuality>()
    };

    public async Task<ConversionResult> ConvertAsync(
        byte[] sourceContent,
        string sourceFormat,
        string targetFormat,
        ConversionContext context)
    {
        var browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        try
        {
            // Load HTML content
            var html = Encoding.UTF8.GetString(sourceContent);
            await page.SetContentAsync(html);

            // Wait for network idle if requested
            if (context.AdditionalOptions.TryGetValue("WaitForNetworkIdle", out var waitObj) &&
                waitObj is bool wait && wait)
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }

            // Execute JavaScript if requested
            if (context.AdditionalOptions.TryGetValue("ExecuteJavaScript", out var executeJs) &&
                executeJs is bool exec && exec)
            {
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            }

            byte[] convertedContent;

            if (targetFormat.Equals("pdf", StringComparison.OrdinalIgnoreCase))
            {
                // Convert to PDF
                var pdfOptions = BuildPdfOptions(context);
                convertedContent = await page.PdfAsync(pdfOptions);
            }
            else
            {
                // Convert to image
                var screenshotOptions = BuildScreenshotOptions(targetFormat, context);
                convertedContent = await page.ScreenshotAsync(screenshotOptions);
            }

            return new ConversionResult
            {
                Success = true,
                ConvertedContent = convertedContent,
                SourceFormat = sourceFormat,
                TargetFormat = targetFormat,
                SourceSize = sourceContent.Length,
                ConvertedSize = convertedContent.Length,
                ProviderName = ProviderName
            };
        }
        finally
        {
            await browser.CloseAsync();
        }
    }

    public bool SupportsConversion(string sourceFormat, string targetFormat)
    {
        return sourceFormat.Equals("html", StringComparison.OrdinalIgnoreCase) &&
               (targetFormat.Equals("pdf", StringComparison.OrdinalIgnoreCase) ||
                targetFormat.Equals("png", StringComparison.OrdinalIgnoreCase) ||
                targetFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
                targetFormat.Equals("jpg", StringComparison.OrdinalIgnoreCase));
    }

    private PagePdfOptions BuildPdfOptions(ConversionContext context)
    {
        var options = new PagePdfOptions
        {
            Format = "A4",
            PrintBackground = true
        };

        // Apply context options
        if (context.AdditionalOptions.TryGetValue("PageSize", out var pageSize))
            options.Format = pageSize.ToString();

        if (context.AdditionalOptions.TryGetValue("Orientation", out var orientation))
            options.Landscape = orientation.ToString()?.Equals("Landscape", StringComparison.OrdinalIgnoreCase) ?? false;

        if (context.AdditionalOptions.TryGetValue("Margins", out var margins) && margins is Dictionary<string, object> m)
        {
            options.Margin = new Margin
            {
                Top = m.TryGetValue("Top", out var top) ? $"{top}mm" : "0",
                Bottom = m.TryGetValue("Bottom", out var bottom) ? $"{bottom}mm" : "0",
                Left = m.TryGetValue("Left", out var left) ? $"{left}mm" : "0",
                Right = m.TryGetValue("Right", out var right) ? $"{right}mm" : "0"
            };
        }

        return options;
    }
}
```

---

## Data Flow

### Sequence: Document Conversion with Provider Selection

```
┌───────────┐     ┌──────────────────┐     ┌──────────┐     ┌──────────┐
│Application│     │ConversionService │     │ Provider │     │  Engine  │
└─────┬─────┘     └────────┬─────────┘     └─────┬────┘     └─────┬────┘
      │                    │                      │                 │
      │ ConvertAsync(...)  │                      │                 │
      ├───────────────────>│                      │                 │
      │                    │                      │                 │
      │                    │ SelectProvider()     │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │ Provider instance    │                 │
      │                    │<─────────────────────┤                 │
      │                    │                      │                 │
      │                    │ ConvertAsync(...)    │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │                      │ Execute conversion
      │                    │                      ├────────────────>│
      │                    │                      │                 │
      │                    │                      │ Converted data  │
      │                    │                      │<────────────────┤
      │                    │                      │                 │
      │                    │ ConversionResult     │                 │
      │                    │<─────────────────────┤                 │
      │                    │                      │                 │
      │                    │ ValidateResult()     │                 │
      │                    │                      │                 │
      │ ConversionResult   │                      │                 │
      │<───────────────────┤                      │                 │
      │                    │                      │                 │
```

---

## Design Patterns

### 1. Provider Pattern
- Multiple conversion engines via pluggable providers
- Provider selection based on format compatibility
- Fallback providers for resilience

### 2. Strategy Pattern
- Different conversion strategies per format
- Quality-based strategy selection
- Context-driven option application

### 3. Factory Pattern
- Provider factory creates provider instances
- Provider pooling for reuse
- Provider configuration via dependency injection

### 4. Cache-Aside Pattern
- Check cache before conversion
- Cache miss triggers conversion
- Cache result for future requests

---

## Performance Optimizations

### 1. Result Caching
- Cache based on content hash + format + quality
- Avoid redundant conversions
- Configurable cache TTL
- Cache size limits

### 2. Batch Processing
- Parallel conversion processing
- Configurable degree of parallelism
- Resource pooling for providers

### 3. Provider Pooling
- Reuse provider instances
- Process pooling for external tools
- Connection pooling where applicable

### 4. Streaming Support
- Stream large files to avoid memory issues
- Chunked processing for batch operations
- Async I/O throughout

---

## Error Handling

### Retry Strategy
```csharp
public async Task<ConversionResult> ConvertWithRetryAsync(...)
{
    var retryCount = 0;
    var maxRetries = 3;

    while (retryCount < maxRetries)
    {
        try
        {
            return await provider.ConvertAsync(...);
        }
        catch (Exception ex) when (IsTransientError(ex))
        {
            retryCount++;
            if (retryCount >= maxRetries)
                throw;

            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));  // Exponential backoff
        }
    }
}
```

---

## Thread Safety

- Service is thread-safe (stateless or properly synchronized)
- Providers must be thread-safe
- Concurrent conversions supported
- Provider pooling handles concurrent access

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [MediaTypeDetection Architecture](../MediaTypeDetection/architecture.md)
