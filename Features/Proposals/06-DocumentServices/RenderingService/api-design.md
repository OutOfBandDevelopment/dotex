# Document Rendering Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Rendering Service
**Last Updated:** 2026-01-23

---

## API Overview

Complete API surface for document rendering with provider pattern, context-based behavior, and comprehensive format support for generating images, thumbnails, and previews.

---

## Core Interfaces

### IDocumentRenderingService

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Documents.Rendering;

/// <summary>
/// Service for rendering documents to images with provider pattern support.
/// </summary>
public interface IDocumentRenderingService
{
    /// <summary>
    /// Renders single page to image.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format (e.g., "pdf", "html")</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="context">Optional rendering context</param>
    /// <returns>Rendering result with image data</returns>
    /// <exception cref="RenderingNotSupportedException">Format not supported</exception>
    /// <exception cref="RenderingFailedException">Rendering failed</exception>
    Task<RenderingResult> RenderPageAsync(
        byte[] documentContent,
        string format,
        int pageNumber,
        RenderingContext? context = null);

    /// <summary>
    /// Renders multiple pages to images.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="pageNumbers">Array of page numbers (1-based)</param>
    /// <param name="context">Optional rendering context</param>
    /// <returns>Collection of rendering results</returns>
    Task<IEnumerable<RenderingResult>> RenderPagesAsync(
        byte[] documentContent,
        string format,
        int[] pageNumbers,
        RenderingContext? context = null);

    /// <summary>
    /// Renders all pages to images.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="context">Optional rendering context</param>
    /// <returns>Collection of rendering results for all pages</returns>
    Task<IEnumerable<RenderingResult>> RenderAllPagesAsync(
        byte[] documentContent,
        string format,
        RenderingContext? context = null);

    /// <summary>
    /// Generates thumbnail for document (renders first page at reduced size).
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="context">Optional rendering context</param>
    /// <returns>Rendering result with thumbnail image</returns>
    Task<RenderingResult> GenerateThumbnailAsync(
        byte[] documentContent,
        string format,
        RenderingContext? context = null);

    /// <summary>
    /// Batch renders documents.
    /// </summary>
    /// <param name="requests">Collection of rendering requests</param>
    /// <param name="context">Optional rendering context</param>
    /// <returns>Collection of rendering results</returns>
    Task<IEnumerable<RenderingResult>> RenderBatchAsync(
        IEnumerable<RenderingRequest> requests,
        RenderingContext? context = null);

    /// <summary>
    /// Gets page count for document.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <returns>Number of pages</returns>
    Task<int> GetPageCountAsync(byte[] documentContent, string format);
}
```

---

### IDocumentRenderingProvider

```csharp
namespace OoBDev.System.Documents.Rendering.Providers;

/// <summary>
/// Provider interface for document rendering implementations.
/// </summary>
public interface IDocumentRenderingProvider
{
    /// <summary>
    /// Provider name (e.g., "pdfium", "playwright", "mupdf").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Provider capabilities and supported features.
    /// </summary>
    DocumentRenderingCapabilities Capabilities { get; }

    /// <summary>
    /// Renders document page to image.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <param name="pageNumber">Page number (1-based)</param>
    /// <param name="context">Rendering context</param>
    /// <returns>Rendering result</returns>
    Task<RenderingResult> RenderPageAsync(
        byte[] documentContent,
        string format,
        int pageNumber,
        RenderingContext context);

    /// <summary>
    /// Gets page count for document.
    /// </summary>
    /// <param name="documentContent">Document content</param>
    /// <param name="format">Document format</param>
    /// <returns>Number of pages</returns>
    Task<int> GetPageCountAsync(byte[] documentContent, string format);

    /// <summary>
    /// Checks if format is supported by this provider.
    /// </summary>
    /// <param name="format">Document format</param>
    /// <returns>True if supported</returns>
    bool SupportsFormat(string format);

    /// <summary>
    /// Gets all formats supported by this provider.
    /// </summary>
    /// <returns>Collection of supported formats</returns>
    IEnumerable<string> GetSupportedFormats();
}
```

---

## Data Models

### RenderingContext

```csharp
namespace OoBDev.System.Documents.Rendering;

/// <summary>
/// Context for document rendering operations.
/// </summary>
public class RenderingContext
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
    /// Resolution in DPI (default: 96).
    /// </summary>
    public int Dpi { get; set; } = 96;

    /// <summary>
    /// Output image width in pixels (null = auto-calculate from DPI).
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Output image height in pixels (null = auto-calculate from DPI).
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// Output image format (default: "png").
    /// </summary>
    public string OutputFormat { get; set; } = "png";

    /// <summary>
    /// Image quality for lossy formats like JPEG (1-100, default: 90).
    /// </summary>
    public int Quality { get; set; } = 90;

    /// <summary>
    /// Maintain aspect ratio when both width and height specified (default: true).
    /// </summary>
    public bool MaintainAspectRatio { get; set; } = true;

    /// <summary>
    /// Background color for transparent formats (hex color, default: null for transparent).
    /// </summary>
    public string? BackgroundColor { get; set; }

    /// <summary>
    /// Watermark to apply to rendered image (default: null).
    /// </summary>
    public Watermark? Watermark { get; set; }

    /// <summary>
    /// Include annotations when rendering PDF (default: true).
    /// </summary>
    public bool IncludeAnnotations { get; set; } = true;

    /// <summary>
    /// Additional format-specific options.
    /// </summary>
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}
```

### RenderingRequest

```csharp
/// <summary>
/// Request for document rendering (used in batch operations).
/// </summary>
public class RenderingRequest
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
    /// Document format (e.g., "pdf", "html").
    /// </summary>
    public string Format { get; set; } = "";

    /// <summary>
    /// Page number to render (1-based, 0 = all pages).
    /// </summary>
    public int PageNumber { get; set; } = 1;
}
```

### RenderingResult

```csharp
/// <summary>
/// Result of document rendering operation.
/// </summary>
public class RenderingResult
{
    /// <summary>
    /// Request identifier (for batch operations).
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Indicates if rendering was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Rendered image content (null if rendering failed).
    /// </summary>
    public byte[]? RenderedImage { get; set; }

    /// <summary>
    /// Page number that was rendered.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Rendered image width in pixels.
    /// </summary>
    public int Width { get; set; }

    /// <summary>
    /// Rendered image height in pixels.
    /// </summary>
    public int Height { get; set; }

    /// <summary>
    /// Output image format.
    /// </summary>
    public string OutputFormat { get; set; } = "";

    /// <summary>
    /// Rendered image size in bytes.
    /// </summary>
    public long Size { get; set; }

    /// <summary>
    /// Rendering duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Provider used for rendering.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Error message if rendering failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
```

### Watermark

```csharp
/// <summary>
/// Watermark configuration for rendered images.
/// </summary>
public class Watermark
{
    /// <summary>
    /// Text watermark content (null if using image watermark).
    /// </summary>
    public string? Text { get; set; }

    /// <summary>
    /// Image watermark content (null if using text watermark).
    /// </summary>
    public byte[]? ImageContent { get; set; }

    /// <summary>
    /// Watermark position (default: Center).
    /// </summary>
    public WatermarkPosition Position { get; set; } = WatermarkPosition.Center;

    /// <summary>
    /// Watermark opacity (0.0-1.0, default: 0.5).
    /// </summary>
    public float Opacity { get; set; } = 0.5f;

    /// <summary>
    /// Watermark rotation in degrees (default: -45).
    /// </summary>
    public float Rotation { get; set; } = -45f;

    /// <summary>
    /// Font family for text watermark (default: "Arial").
    /// </summary>
    public string? FontFamily { get; set; } = "Arial";

    /// <summary>
    /// Font size for text watermark (default: 48).
    /// </summary>
    public int FontSize { get; set; } = 48;

    /// <summary>
    /// Text color as hex string (default: "#000000").
    /// </summary>
    public string? Color { get; set; } = "#000000";
}
```

### WatermarkPosition

```csharp
/// <summary>
/// Watermark position options.
/// </summary>
public enum WatermarkPosition
{
    /// <summary>
    /// Center of image.
    /// </summary>
    Center,

    /// <summary>
    /// Top-left corner.
    /// </summary>
    TopLeft,

    /// <summary>
    /// Top-right corner.
    /// </summary>
    TopRight,

    /// <summary>
    /// Bottom-left corner.
    /// </summary>
    BottomLeft,

    /// <summary>
    /// Bottom-right corner.
    /// </summary>
    BottomRight,

    /// <summary>
    /// Custom position (specified via additional options).
    /// </summary>
    Custom
}
```

### DocumentRenderingCapabilities

```csharp
namespace OoBDev.System.Documents.Rendering.Providers;

/// <summary>
/// Capabilities of a document rendering provider.
/// </summary>
public class DocumentRenderingCapabilities
{
    /// <summary>
    /// Supports high-quality rendering (300+ DPI).
    /// </summary>
    public bool SupportsHighQuality { get; set; }

    /// <summary>
    /// Supports fast rendering (optimized for speed).
    /// </summary>
    public bool SupportsFastRendering { get; set; }

    /// <summary>
    /// Supports built-in watermark application.
    /// </summary>
    public bool SupportsWatermarks { get; set; }

    /// <summary>
    /// Supports rendering annotations (PDF).
    /// </summary>
    public bool SupportsAnnotations { get; set; }

    /// <summary>
    /// Supported document formats.
    /// </summary>
    public string[] SupportedFormats { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Maximum supported DPI.
    /// </summary>
    public int MaxDpi { get; set; } = 300;

    /// <summary>
    /// Supported output image formats.
    /// </summary>
    public string[] SupportedOutputFormats { get; set; } = Array.Empty<string>();
}
```

---

## Exception Types

```csharp
namespace OoBDev.System.Documents.Rendering;

/// <summary>
/// Exception thrown when rendering format is not supported.
/// </summary>
public class RenderingNotSupportedException : Exception
{
    public RenderingNotSupportedException(string message) : base(message) { }
    public RenderingNotSupportedException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when rendering fails.
/// </summary>
public class RenderingFailedException : Exception
{
    public RenderingFailedException(string message) : base(message) { }
    public RenderingFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when page number is invalid.
/// </summary>
public class InvalidPageNumberException : Exception
{
    public int PageNumber { get; }
    public int PageCount { get; }

    public InvalidPageNumberException(int pageNumber, int pageCount)
        : base($"Page {pageNumber} does not exist (document has {pageCount} pages)")
    {
        PageNumber = pageNumber;
        PageCount = pageCount;
    }
}
```

---

## Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.System.Documents.Rendering.Extensions;

/// <summary>
/// Extension methods for registering document rendering services.
/// </summary>
public static class DocumentRenderingServiceExtensions
{
    /// <summary>
    /// Adds document rendering services with default providers.
    /// </summary>
    public static IServiceCollection AddDocumentRendering(
        this IServiceCollection services,
        Action<DocumentRenderingOptions>? configure = null)
    {
        // Register core service
        services.TryAddSingleton<IDocumentRenderingService, DocumentRenderingService>();
        services.TryAddSingleton<IDocumentRenderingProviderFactory, DocumentRenderingProviderFactory>();

        // Register default providers
        services.TryAddSingleton<IDocumentRenderingProvider, PDFiumRenderingProvider>();
        services.TryAddSingleton<IDocumentRenderingProvider, MuPdfRenderingProvider>();
        services.TryAddSingleton<IDocumentRenderingProvider, LibreOfficeRenderingProvider>();
        services.TryAddSingleton<IDocumentRenderingProvider, PlaywrightRenderingProvider>();
        services.TryAddSingleton<IDocumentRenderingProvider, ImageMagickRenderingProvider>();

        // Configure options
        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Adds a custom rendering provider.
    /// </summary>
    public static IServiceCollection AddRenderingProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IDocumentRenderingProvider
    {
        services.TryAddSingleton<IDocumentRenderingProvider, TProvider>();
        return services;
    }
}

/// <summary>
/// Configuration options for document rendering service.
/// </summary>
public class DocumentRenderingOptions
{
    /// <summary>
    /// Enable result caching (default: true).
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in hours (default: 2).
    /// </summary>
    public int CacheDurationHours { get; set; } = 2;

    /// <summary>
    /// Enable fallback to alternate providers on failure (default: true).
    /// </summary>
    public bool EnableFallback { get; set; } = true;

    /// <summary>
    /// Maximum concurrent render operations (default: 5).
    /// </summary>
    public int MaxConcurrentRenders { get; set; } = 5;

    /// <summary>
    /// Rendering timeout in seconds (default: 30).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Default DPI (default: 96).
    /// </summary>
    public int DefaultDpi { get; set; } = 96;

    /// <summary>
    /// Default output format (default: "png").
    /// </summary>
    public string DefaultOutputFormat { get; set; } = "png";
}
```

---

## Usage Examples

### Example 1: Render PDF Page

```csharp
using OoBDev.System.Documents.Rendering;

// Render page 1 of PDF
var pdfContent = await File.ReadAllBytesAsync("document.pdf");

var result = await _renderingService.RenderPageAsync(
    pdfContent,
    "pdf",
    pageNumber: 1);

if (result.Success)
{
    await File.WriteAllBytesAsync("page1.png", result.RenderedImage!);
    Console.WriteLine($"Rendered {result.Width}x{result.Height} in {result.Duration.TotalMilliseconds}ms");
}
```

### Example 2: Generate High-Resolution Thumbnail

```csharp
var context = new RenderingContext
{
    Width = 512,
    Height = 512,
    Dpi = 150,
    OutputFormat = "jpeg",
    Quality = 85,
    MaintainAspectRatio = true
};

var result = await _renderingService.GenerateThumbnailAsync(
    pdfContent,
    "pdf",
    context);

await File.WriteAllBytesAsync("thumbnail.jpg", result.RenderedImage!);
```

### Example 3: Render All Pages

```csharp
var results = await _renderingService.RenderAllPagesAsync(
    pdfContent,
    "pdf",
    new RenderingContext
    {
        Dpi = 150,
        OutputFormat = "png"
    });

int pageNum = 1;
foreach (var result in results)
{
    if (result.Success)
    {
        await File.WriteAllBytesAsync($"page{pageNum}.png", result.RenderedImage!);
        pageNum++;
    }
}
```

### Example 4: Render with Watermark

```csharp
var context = new RenderingContext
{
    Dpi = 96,
    Watermark = new Watermark
    {
        Text = "CONFIDENTIAL",
        Position = WatermarkPosition.Center,
        Opacity = 0.3f,
        Rotation = -45f,
        FontFamily = "Arial",
        FontSize = 72,
        Color = "#FF0000"
    }
};

var result = await _renderingService.RenderPageAsync(
    pdfContent,
    "pdf",
    pageNumber: 1,
    context);
```

### Example 5: Batch Rendering

```csharp
var requests = new List<RenderingRequest>
{
    new() { DocumentContent = doc1Content, Format = "pdf", PageNumber = 1 },
    new() { DocumentContent = doc2Content, Format = "pdf", PageNumber = 1 },
    new() { DocumentContent = doc3Content, Format = "pdf", PageNumber = 1 }
};

var context = new RenderingContext
{
    Width = 256,
    Height = 256,
    OutputFormat = "jpeg"
};

var results = await _renderingService.RenderBatchAsync(requests, context);

foreach (var result in results.Where(r => r.Success))
{
    await File.WriteAllBytesAsync($"{result.RequestId}.jpg", result.RenderedImage!);
}
```

### Example 6: HTML to Image

```csharp
var htmlContent = Encoding.UTF8.GetBytes(@"
    <html>
        <style>
            body { font-family: Arial; padding: 20px; }
            h1 { color: blue; }
        </style>
        <body>
            <h1>Hello World</h1>
            <p>This is a test document.</p>
        </body>
    </html>");

var context = new RenderingContext
{
    Width = 1920,
    Height = 1080,
    OutputFormat = "png",
    AdditionalOptions = new Dictionary<string, object>
    {
        ["WaitForNetworkIdle"] = true
    }
};

var result = await _renderingService.RenderPageAsync(
    htmlContent,
    "html",
    pageNumber: 1,
    context);
```

### Example 7: Custom Provider Selection

```csharp
var context = new RenderingContext
{
    AdditionalOptions = new Dictionary<string, object>
    {
        ["PreferredProvider"] = "pdfium"  // Force use of PDFium provider
    }
};

var result = await _renderingService.RenderPageAsync(
    pdfContent,
    "pdf",
    pageNumber: 1,
    context);

Console.WriteLine($"Used provider: {result.ProviderName}");
```

### Example 8: Get Page Count

```csharp
var pageCount = await _renderingService.GetPageCountAsync(pdfContent, "pdf");
Console.WriteLine($"Document has {pageCount} pages");

// Render specific pages
var pageNumbers = new[] { 1, 3, 5, 7 };
var results = await _renderingService.RenderPagesAsync(
    pdfContent,
    "pdf",
    pageNumbers);
```

---

## Configuration Example

```json
{
  "DocumentRendering": {
    "EnableCaching": true,
    "CacheDurationHours": 4,
    "EnableFallback": true,
    "MaxConcurrentRenders": 10,
    "TimeoutSeconds": 60,
    "DefaultDpi": 96,
    "DefaultOutputFormat": "png",
    "Providers": {
      "PDFium": {
        "Enabled": true,
        "MaxDpi": 600
      },
      "Playwright": {
        "Enabled": true,
        "BrowserType": "Chromium",
        "Headless": true
      },
      "LibreOffice": {
        "Path": "/usr/bin/libreoffice",
        "Enabled": true
      }
    }
  }
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
