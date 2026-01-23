# Document Rendering Service - Architecture

**Epic:** 6 - Document Services
**Feature:** Document Rendering Service
**Last Updated:** 2026-01-23

---

## Architectural Overview

The Document Rendering Service implements a **Provider Pattern** with **Context-Based Rendering** for transforming documents into images, thumbnails, and previews. Applications provide operational context that providers use to optimize rendering quality, resolution, and performance.

```
┌─────────────────────────────────────────────────────────────┐
│                   Application Layer                          │
│    (Document Management, Preview Generator, CMS)             │
└────────────────────┬────────────────────────────────────────┘
                     │ RenderPageAsync(content, format, page, context)
                     ↓
┌─────────────────────────────────────────────────────────────┐
│          IDocumentRenderingService                           │
│  - RenderPageAsync(content, format, page, context)           │
│  - RenderPagesAsync(content, format, pages[], context)       │
│  - RenderAllPagesAsync(content, format, context)             │
│  - GenerateThumbnailAsync(content, format, context)          │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼────────────┬────────────┬────────────┐
         ↓           ↓            ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│   PDFium     │ │ MuPDF  │ │LibreOffice│ │Playwright│ │ImageMagic│
│  Provider    │ │Provider│ │ Provider │ │ Provider │ │ Provider │
└──────┬───────┘ └────┬───┘ └────┬────┘ └────┬─────┘ └────┬─────┘
       │              │          │            │            │
       ↓              ↓          ↓            ↓            ↓
┌──────────────┐ ┌────────┐ ┌─────────┐ ┌──────────┐ ┌──────────┐
│ PDF Render   │ │Fast PDF│ │Office→IMG│ │ HTML→IMG │ │Image Xfrm│
│ High Quality │ │Render  │ │Render    │ │Web Render│ │Format Cvt│
└──────────────┘ └────────┘ └─────────┘ └──────────┘ └──────────┘
```

---

## Core Components

### 1. DocumentRenderingService (Main Entry Point)

**Responsibilities:**
- Coordinate document rendering across providers
- Select appropriate provider based on document format
- Handle rendering caching for performance
- Apply watermarks and annotations
- Manage concurrent rendering operations

**Key Design Decisions:**
- **Multi-provider support** - Route requests to appropriate rendering engine
- **Context propagation** - Pass rendering preferences to all providers
- **Result caching** - Cache rendered images for identical documents
- **Parallel rendering** - Process multiple pages concurrently
- **Watermark application** - Apply watermarks post-rendering

**Implementation Pattern:**
```csharp
public class DocumentRenderingService : IDocumentRenderingService
{
    private readonly IDocumentRenderingProviderFactory _providerFactory;
    private readonly IMemoryCache _cache;
    private readonly DocumentRenderingOptions _options;
    private readonly ILogger<DocumentRenderingService> _logger;

    public async Task<RenderingResult> RenderPageAsync(
        byte[] documentContent,
        string format,
        int pageNumber,
        RenderingContext? context = null)
    {
        context ??= new RenderingContext();
        var startTime = DateTime.UtcNow;

        // 1. Check cache
        var cacheKey = GenerateCacheKey(documentContent, format, pageNumber, context);
        if (_options.EnableCaching && _cache.TryGetValue<RenderingResult>(cacheKey, out var cachedResult))
        {
            _logger.LogDebug("Cache hit for page {Page} of {Format}", pageNumber, format);
            return cachedResult;
        }

        // 2. Select provider
        var provider = await SelectProviderAsync(format, context);
        _logger.LogDebug("Selected provider {Provider} for {Format} rendering",
            provider.ProviderName, format);

        RenderingResult result;
        try
        {
            // 3. Perform rendering
            result = await provider.RenderPageAsync(documentContent, format, pageNumber, context);
            result.Duration = DateTime.UtcNow - startTime;

            // 4. Apply watermark if requested
            if (context.Watermark != null)
            {
                result.RenderedImage = await ApplyWatermarkAsync(result.RenderedImage!, context.Watermark);
            }

            // 5. Cache result
            if (_options.EnableCaching)
            {
                _cache.Set(cacheKey, result, TimeSpan.FromHours(_options.CacheDurationHours));
            }

            _logger.LogInformation(
                "Rendered page {Page} of {Format} using {Provider} in {Duration}ms (size: {Width}x{Height}, {Size} bytes)",
                pageNumber, format, provider.ProviderName, result.Duration.TotalMilliseconds,
                result.Width, result.Height, result.Size);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Rendering failed for page {Page} using provider {Provider}",
                pageNumber, provider.ProviderName);

            // Try fallback provider
            if (_options.EnableFallback)
            {
                var fallbackProvider = await GetFallbackProviderAsync(format, provider.ProviderName);
                if (fallbackProvider != null)
                {
                    _logger.LogInformation("Retrying rendering with fallback provider {Provider}",
                        fallbackProvider.ProviderName);
                    result = await fallbackProvider.RenderPageAsync(documentContent, format, pageNumber, context);
                    result.Duration = DateTime.UtcNow - startTime;
                    return result;
                }
            }

            throw new RenderingFailedException(
                $"Failed to render page {pageNumber} of {format}: {ex.Message}", ex);
        }
    }

    public async Task<IEnumerable<RenderingResult>> RenderPagesAsync(
        byte[] documentContent,
        string format,
        int[] pageNumbers,
        RenderingContext? context = null)
    {
        context ??= new RenderingContext();

        // Parallel processing with degree of parallelism
        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = _options.MaxConcurrentRenders
        };

        var results = new ConcurrentBag<RenderingResult>();

        await Parallel.ForEachAsync(pageNumbers, parallelOptions, async (pageNumber, ct) =>
        {
            try
            {
                var result = await RenderPageAsync(documentContent, format, pageNumber, context);
                results.Add(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to render page {Page}", pageNumber);
                results.Add(new RenderingResult
                {
                    Success = false,
                    PageNumber = pageNumber,
                    ErrorMessage = ex.Message
                });
            }
        });

        return results.OrderBy(r => r.PageNumber);
    }

    public async Task<IEnumerable<RenderingResult>> RenderAllPagesAsync(
        byte[] documentContent,
        string format,
        RenderingContext? context = null)
    {
        context ??= new RenderingContext();

        // Get page count from provider
        var provider = await SelectProviderAsync(format, context);
        var pageCount = await provider.GetPageCountAsync(documentContent, format);

        _logger.LogDebug("Rendering all {Count} pages of {Format} document", pageCount, format);

        var pageNumbers = Enumerable.Range(1, pageCount).ToArray();
        return await RenderPagesAsync(documentContent, format, pageNumbers, context);
    }

    public async Task<RenderingResult> GenerateThumbnailAsync(
        byte[] documentContent,
        string format,
        RenderingContext? context = null)
    {
        context ??= new RenderingContext();

        // Override context for thumbnail generation
        context.Width = context.Width ?? 256;
        context.Height = context.Height ?? 256;
        context.MaintainAspectRatio = true;

        // Render first page as thumbnail
        return await RenderPageAsync(documentContent, format, 1, context);
    }

    private async Task<IDocumentRenderingProvider> SelectProviderAsync(
        string format,
        RenderingContext context)
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
            throw new RenderingNotSupportedException(
                $"No provider supports rendering format: {format}");
        }

        // 3. Return highest-scoring provider
        return supportingProviders.First();
    }

    private int GetProviderScore(IDocumentRenderingProvider provider, RenderingContext context)
    {
        var score = 0;

        // Prefer high-quality providers for high DPI
        if (context.Dpi >= 150 && provider.Capabilities.SupportsHighQuality)
            score += 10;

        // Prefer fast providers for thumbnails
        if (context.Width <= 512 && provider.Capabilities.SupportsFastRendering)
            score += 5;

        // Prefer providers with watermark support if watermark requested
        if (context.Watermark != null && provider.Capabilities.SupportsWatermarks)
            score += 3;

        return score;
    }

    private async Task<byte[]> ApplyWatermarkAsync(byte[] imageContent, Watermark watermark)
    {
        using var image = Image.Load(imageContent);

        if (!string.IsNullOrEmpty(watermark.Text))
        {
            // Text watermark
            var font = SystemFonts.CreateFont(watermark.FontFamily ?? "Arial", watermark.FontSize);
            var color = Rgba32.ParseHex(watermark.Color ?? "#000000");
            color.A = (byte)(watermark.Opacity * 255);

            image.Mutate(ctx => ctx
                .DrawText(
                    watermark.Text,
                    font,
                    color,
                    CalculateWatermarkPosition(image.Width, image.Height, watermark.Position))
                .Rotate(watermark.Rotation));
        }
        else if (watermark.ImageContent != null)
        {
            // Image watermark
            using var watermarkImage = Image.Load(watermark.ImageContent);
            watermarkImage.Mutate(ctx => ctx.Opacity(watermark.Opacity));

            var position = CalculateWatermarkPosition(image.Width, image.Height, watermark.Position);
            image.Mutate(ctx => ctx.DrawImage(watermarkImage, position, 1f));
        }

        using var outputStream = new MemoryStream();
        await image.SaveAsPngAsync(outputStream);
        return outputStream.ToArray();
    }

    private Point CalculateWatermarkPosition(int imageWidth, int imageHeight, WatermarkPosition position)
    {
        return position switch
        {
            WatermarkPosition.Center => new Point(imageWidth / 2, imageHeight / 2),
            WatermarkPosition.TopLeft => new Point(20, 20),
            WatermarkPosition.TopRight => new Point(imageWidth - 20, 20),
            WatermarkPosition.BottomLeft => new Point(20, imageHeight - 20),
            WatermarkPosition.BottomRight => new Point(imageWidth - 20, imageHeight - 20),
            _ => new Point(imageWidth / 2, imageHeight / 2)
        };
    }

    private string GenerateCacheKey(byte[] content, string format, int pageNumber, RenderingContext context)
    {
        using var sha256 = SHA256.Create();
        var hash = sha256.ComputeHash(content);
        var hashString = Convert.ToBase64String(hash);
        return $"{hashString}|{format}|{pageNumber}|{context.Dpi}|{context.Width}|{context.Height}";
    }
}
```

---

### 2. Provider Implementations

#### PDFiumRenderingProvider (High-Quality PDF Rendering)

**Responsibilities:**
- Render PDF pages to images with high fidelity
- Support multiple image formats (PNG, JPEG, TIFF)
- Handle high-resolution rendering (up to 600 DPI)
- Preserve PDF annotations if requested

**Implementation Pattern:**
```csharp
public class PDFiumRenderingProvider : IDocumentRenderingProvider
{
    private readonly ILogger<PDFiumRenderingProvider> _logger;

    public string ProviderName => "pdfium";

    public DocumentRenderingCapabilities Capabilities => new()
    {
        SupportsHighQuality = true,
        SupportsFastRendering = false,
        SupportsWatermarks = false,
        SupportsAnnotations = true,
        SupportedFormats = new[] { "pdf" },
        MaxDpi = 600,
        SupportedOutputFormats = new[] { "png", "jpeg", "tiff", "webp" }
    };

    public async Task<RenderingResult> RenderPageAsync(
        byte[] documentContent,
        string format,
        int pageNumber,
        RenderingContext context)
    {
        var startTime = DateTime.UtcNow;

        using var document = PdfDocument.Load(documentContent);

        if (pageNumber < 1 || pageNumber > document.PageCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                $"Page {pageNumber} does not exist (document has {document.PageCount} pages)");
        }

        using var page = document.Pages[pageNumber - 1];

        // Calculate dimensions
        var scale = context.Dpi / 72.0;  // 72 DPI is default
        var width = context.Width ?? (int)(page.Width * scale);
        var height = context.Height ?? (int)(page.Height * scale);

        if (context.MaintainAspectRatio && context.Width.HasValue && context.Height.HasValue)
        {
            var aspectRatio = page.Width / page.Height;
            if (width / height > aspectRatio)
            {
                width = (int)(height * aspectRatio);
            }
            else
            {
                height = (int)(width / aspectRatio);
            }
        }

        // Render page
        using var bitmap = new PDFiumBitmap(width, height, true);

        var backgroundColor = ParseColor(context.BackgroundColor ?? "#FFFFFF");
        bitmap.FillRect(0, 0, width, height, backgroundColor);

        var flags = PDFiumRenderFlags.None;
        if (context.IncludeAnnotations)
            flags |= PDFiumRenderFlags.Annotations;

        page.Render(bitmap, 0, 0, width, height, 0, flags);

        // Convert to output format
        byte[] renderedImage;
        using var stream = new MemoryStream();

        if (context.OutputFormat.Equals("png", StringComparison.OrdinalIgnoreCase))
        {
            bitmap.SaveAsPng(stream);
        }
        else if (context.OutputFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase) ||
                 context.OutputFormat.Equals("jpg", StringComparison.OrdinalIgnoreCase))
        {
            bitmap.SaveAsJpeg(stream, context.Quality);
        }
        else if (context.OutputFormat.Equals("tiff", StringComparison.OrdinalIgnoreCase))
        {
            bitmap.SaveAsTiff(stream);
        }
        else
        {
            throw new NotSupportedException($"Output format {context.OutputFormat} not supported");
        }

        renderedImage = stream.ToArray();

        return new RenderingResult
        {
            Success = true,
            RenderedImage = renderedImage,
            PageNumber = pageNumber,
            Width = width,
            Height = height,
            OutputFormat = context.OutputFormat,
            Size = renderedImage.Length,
            Duration = DateTime.UtcNow - startTime,
            ProviderName = ProviderName
        };
    }

    public async Task<int> GetPageCountAsync(byte[] documentContent, string format)
    {
        using var document = PdfDocument.Load(documentContent);
        return document.PageCount;
    }

    public bool SupportsFormat(string format)
    {
        return format.Equals("pdf", StringComparison.OrdinalIgnoreCase);
    }

    private uint ParseColor(string hexColor)
    {
        hexColor = hexColor.TrimStart('#');
        if (hexColor.Length == 6)
        {
            var r = Convert.ToByte(hexColor.Substring(0, 2), 16);
            var g = Convert.ToByte(hexColor.Substring(2, 2), 16);
            var b = Convert.ToByte(hexColor.Substring(4, 2), 16);
            return (uint)((0xFF << 24) | (r << 16) | (g << 8) | b);
        }
        return 0xFFFFFFFF;  // White
    }
}
```

---

#### PlaywrightRenderingProvider (HTML Rendering)

**Responsibilities:**
- Render HTML to images with CSS and JavaScript support
- Handle modern web content
- Support custom viewport sizes
- Fast rendering for web previews

**Implementation Pattern:**
```csharp
public class PlaywrightRenderingProvider : IDocumentRenderingProvider
{
    private readonly IPlaywright _playwright;
    private readonly ILogger<PlaywrightRenderingProvider> _logger;

    public string ProviderName => "playwright";

    public DocumentRenderingCapabilities Capabilities => new()
    {
        SupportsHighQuality = true,
        SupportsFastRendering = true,
        SupportsWatermarks = false,
        SupportsAnnotations = false,
        SupportedFormats = new[] { "html", "htm" },
        MaxDpi = 300,
        SupportedOutputFormats = new[] { "png", "jpeg" }
    };

    public async Task<RenderingResult> RenderPageAsync(
        byte[] documentContent,
        string format,
        int pageNumber,
        RenderingContext context)
    {
        var browser = await _playwright.Chromium.LaunchAsync(new() { Headless = true });
        var page = await browser.NewPageAsync();

        try
        {
            // Set viewport size
            var viewportWidth = context.Width ?? 1920;
            var viewportHeight = context.Height ?? 1080;
            await page.SetViewportSizeAsync(viewportWidth, viewportHeight);

            // Load HTML content
            var html = Encoding.UTF8.GetString(documentContent);
            await page.SetContentAsync(html);

            // Wait for network idle if requested
            if (context.AdditionalOptions.TryGetValue("WaitForNetworkIdle", out var waitObj) &&
                waitObj is bool wait && wait)
            {
                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            }

            // Build screenshot options
            var screenshotOptions = new PageScreenshotOptions
            {
                Type = context.OutputFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase)
                    ? ScreenshotType.Jpeg
                    : ScreenshotType.Png,
                Quality = context.OutputFormat.Equals("jpeg", StringComparison.OrdinalIgnoreCase)
                    ? context.Quality
                    : null,
                FullPage = true
            };

            // Render page
            var renderedImage = await page.ScreenshotAsync(screenshotOptions);

            return new RenderingResult
            {
                Success = true,
                RenderedImage = renderedImage,
                PageNumber = pageNumber,
                Width = viewportWidth,
                Height = viewportHeight,
                OutputFormat = context.OutputFormat,
                Size = renderedImage.Length,
                ProviderName = ProviderName
            };
        }
        finally
        {
            await browser.CloseAsync();
        }
    }

    public Task<int> GetPageCountAsync(byte[] documentContent, string format)
    {
        return Task.FromResult(1);  // HTML is single page
    }

    public bool SupportsFormat(string format)
    {
        return format.Equals("html", StringComparison.OrdinalIgnoreCase) ||
               format.Equals("htm", StringComparison.OrdinalIgnoreCase);
    }
}
```

---

## Data Flow

### Sequence: Document Page Rendering with Caching

```
┌───────────┐     ┌──────────────────┐     ┌──────────┐     ┌──────────┐
│Application│     │RenderingService  │     │  Cache   │     │ Provider │
└─────┬─────┘     └────────┬─────────┘     └─────┬────┘     └─────┬────┘
      │                    │                      │                 │
      │ RenderPageAsync()  │                      │                 │
      ├───────────────────>│                      │                 │
      │                    │                      │                 │
      │                    │ Check cache          │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │                    │ Cache miss           │                 │
      │                    │<─────────────────────┤                 │
      │                    │                      │                 │
      │                    │ SelectProvider()     │                 │
      │                    ├─────────────────────────────────────>│
      │                    │                      │                 │
      │                    │ RenderPageAsync()    │                 │
      │                    ├─────────────────────────────────────>│
      │                    │                      │                 │
      │                    │                      │   Render page   │
      │                    │                      │                 │
      │                    │ RenderingResult      │                 │
      │                    │<─────────────────────────────────────┤
      │                    │                      │                 │
      │                    │ ApplyWatermark()     │                 │
      │                    │                      │                 │
      │                    │ Cache result         │                 │
      │                    ├─────────────────────>│                 │
      │                    │                      │                 │
      │ RenderingResult    │                      │                 │
      │<───────────────────┤                      │                 │
      │                    │                      │                 │
```

---

## Design Patterns

### 1. Provider Pattern
- Multiple rendering engines via pluggable providers
- Provider selection based on format and capabilities
- Fallback providers for resilience

### 2. Strategy Pattern
- Different rendering strategies per format
- Quality-based strategy selection
- Context-driven option application

### 3. Factory Pattern
- Provider factory creates provider instances
- Provider pooling for browser instances
- Provider configuration via dependency injection

### 4. Cache-Aside Pattern
- Check cache before rendering
- Cache miss triggers rendering
- Cache result for future requests

---

## Performance Optimizations

### 1. Result Caching
- Cache based on content hash + page + DPI + dimensions
- Avoid redundant rendering operations
- Configurable cache TTL
- Cache size limits with LRU eviction

### 2. Parallel Processing
- Render multiple pages concurrently
- Configurable degree of parallelism
- Resource pooling for providers

### 3. Provider Pooling
- Reuse browser instances (Playwright)
- Connection pooling where applicable
- Process pooling for external tools

### 4. Lazy Loading
- Load document metadata without full render
- Progressive rendering for large documents
- Streaming support for image output

---

## Error Handling

### Retry Strategy
```csharp
public async Task<RenderingResult> RenderWithRetryAsync(...)
{
    var retryCount = 0;
    var maxRetries = 3;

    while (retryCount < maxRetries)
    {
        try
        {
            return await provider.RenderPageAsync(...);
        }
        catch (Exception ex) when (IsTransientError(ex))
        {
            retryCount++;
            if (retryCount >= maxRetries)
                throw;

            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));
        }
    }
}

private bool IsTransientError(Exception ex)
{
    return ex is TimeoutException ||
           ex is IOException ||
           ex.Message.Contains("temporary");
}
```

---

## Thread Safety

- Service is thread-safe (stateless or properly synchronized)
- Providers must be thread-safe or pooled
- Concurrent rendering operations supported
- Browser instance pooling handles concurrent access

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
