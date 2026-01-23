# Document Conversion Service - API Design

**Epic:** 6 - Document Services
**Feature:** Document Conversion Service
**Last Updated:** 2026-01-22

---

## API Overview

Complete API surface for document conversion with provider pattern, context-based behavior, and comprehensive format support.

---

## Core Interfaces

### IDocumentConversionService

```csharp
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoBDev.System.Documents.Conversion;

/// <summary>
/// Service for converting documents between formats with provider pattern support.
/// </summary>
public interface IDocumentConversionService
{
    /// <summary>
    /// Converts document from source format to target format.
    /// </summary>
    /// <param name="sourceContent">Source document content</param>
    /// <param name="sourceFormat">Source format (e.g., "docx", "pdf")</param>
    /// <param name="targetFormat">Target format (e.g., "pdf", "html")</param>
    /// <param name="context">Optional conversion context</param>
    /// <returns>Conversion result with converted content</returns>
    /// <exception cref="ConversionNotSupportedException">Conversion not supported</exception>
    /// <exception cref="ConversionFailedException">Conversion failed</exception>
    Task<ConversionResult> ConvertAsync(
        byte[] sourceContent,
        string sourceFormat,
        string targetFormat,
        ConversionContext? context = null);

    /// <summary>
    /// Converts document with automatic format detection.
    /// </summary>
    /// <param name="sourceContent">Source document content</param>
    /// <param name="targetFormat">Target format (e.g., "pdf", "html")</param>
    /// <param name="context">Optional conversion context</param>
    /// <returns>Conversion result with converted content</returns>
    /// <exception cref="InvalidFormatException">Format detection failed</exception>
    /// <exception cref="ConversionFailedException">Conversion failed</exception>
    Task<ConversionResult> ConvertAsync(
        byte[] sourceContent,
        string targetFormat,
        ConversionContext? context = null);

    /// <summary>
    /// Batch converts multiple documents to same or different target formats.
    /// </summary>
    /// <param name="requests">Collection of conversion requests</param>
    /// <param name="context">Optional conversion context</param>
    /// <returns>Collection of conversion results</returns>
    Task<IEnumerable<ConversionResult>> ConvertBatchAsync(
        IEnumerable<ConversionRequest> requests,
        ConversionContext? context = null);

    /// <summary>
    /// Checks if conversion between formats is supported.
    /// </summary>
    /// <param name="sourceFormat">Source format</param>
    /// <param name="targetFormat">Target format</param>
    /// <returns>True if conversion supported</returns>
    Task<bool> IsSupportedAsync(string sourceFormat, string targetFormat);

    /// <summary>
    /// Gets supported target formats for source format.
    /// </summary>
    /// <param name="sourceFormat">Source format</param>
    /// <returns>Collection of supported target formats</returns>
    Task<IEnumerable<string>> GetSupportedTargetFormatsAsync(string sourceFormat);

    /// <summary>
    /// Validates conversion result.
    /// </summary>
    /// <param name="result">Conversion result to validate</param>
    /// <returns>Validation result</returns>
    Task<ValidationResult> ValidateConversionAsync(ConversionResult result);
}
```

---

### IDocumentConversionProvider

```csharp
namespace OoBDev.System.Documents.Conversion.Providers;

/// <summary>
/// Provider interface for document conversion implementations.
/// </summary>
public interface IDocumentConversionProvider
{
    /// <summary>
    /// Provider name (e.g., "libreoffice", "playwright", "imagemagick").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Provider capabilities and supported features.
    /// </summary>
    DocumentConversionCapabilities Capabilities { get; }

    /// <summary>
    /// Converts document between formats.
    /// </summary>
    /// <param name="sourceContent">Source document content</param>
    /// <param name="sourceFormat">Source format</param>
    /// <param name="targetFormat">Target format</param>
    /// <param name="context">Conversion context</param>
    /// <returns>Conversion result</returns>
    Task<ConversionResult> ConvertAsync(
        byte[] sourceContent,
        string sourceFormat,
        string targetFormat,
        ConversionContext context);

    /// <summary>
    /// Checks if conversion is supported by this provider.
    /// </summary>
    /// <param name="sourceFormat">Source format</param>
    /// <param name="targetFormat">Target format</param>
    /// <returns>True if supported</returns>
    bool SupportsConversion(string sourceFormat, string targetFormat);

    /// <summary>
    /// Gets all conversions supported by this provider.
    /// </summary>
    /// <returns>Collection of supported conversions</returns>
    IEnumerable<FormatConversion> GetSupportedConversions();
}
```

---

## Data Models

### ConversionContext

```csharp
namespace OoBDev.System.Documents.Conversion;

/// <summary>
/// Context for document conversion operations.
/// </summary>
public class ConversionContext
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
    /// Conversion quality level (default: High).
    /// </summary>
    public ConversionQuality Quality { get; set; } = ConversionQuality.High;

    /// <summary>
    /// Preserve document metadata during conversion (default: true).
    /// </summary>
    public bool PreserveMetadata { get; set; } = true;

    /// <summary>
    /// Validate output after conversion (default: true).
    /// </summary>
    public bool ValidateOutput { get; set; } = true;

    /// <summary>
    /// Additional format-specific options.
    /// </summary>
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}
```

### ConversionRequest

```csharp
/// <summary>
/// Request for document conversion (used in batch operations).
/// </summary>
public class ConversionRequest
{
    /// <summary>
    /// Unique request identifier.
    /// </summary>
    public Guid RequestId { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Source document content.
    /// </summary>
    public byte[] SourceContent { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Source format (e.g., "docx", "pdf").
    /// </summary>
    public string SourceFormat { get; set; } = "";

    /// <summary>
    /// Target format (e.g., "pdf", "html").
    /// </summary>
    public string TargetFormat { get; set; } = "";

    /// <summary>
    /// Document metadata.
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

### ConversionResult

```csharp
/// <summary>
/// Result of document conversion operation.
/// </summary>
public class ConversionResult
{
    /// <summary>
    /// Request identifier (for batch operations).
    /// </summary>
    public Guid RequestId { get; set; }

    /// <summary>
    /// Indicates if conversion was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Converted document content (null if conversion failed).
    /// </summary>
    public byte[]? ConvertedContent { get; set; }

    /// <summary>
    /// Source format.
    /// </summary>
    public string? SourceFormat { get; set; }

    /// <summary>
    /// Target format.
    /// </summary>
    public string? TargetFormat { get; set; }

    /// <summary>
    /// Source document size in bytes.
    /// </summary>
    public long SourceSize { get; set; }

    /// <summary>
    /// Converted document size in bytes.
    /// </summary>
    public long ConvertedSize { get; set; }

    /// <summary>
    /// Conversion duration.
    /// </summary>
    public TimeSpan Duration { get; set; }

    /// <summary>
    /// Provider used for conversion.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Document metadata preserved from source and conversion-specific metadata.
    /// </summary>
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();

    /// <summary>
    /// Error message if conversion failed.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
```

### ConversionQuality

```csharp
/// <summary>
/// Conversion quality levels.
/// </summary>
public enum ConversionQuality
{
    /// <summary>
    /// Low quality - Fast, smallest file size, lower fidelity.
    /// </summary>
    Low,

    /// <summary>
    /// Medium quality - Balanced performance and quality.
    /// </summary>
    Medium,

    /// <summary>
    /// High quality - Good quality, reasonable file size (default).
    /// </summary>
    High,

    /// <summary>
    /// Maximum quality - Best quality, largest file size, slowest.
    /// </summary>
    Maximum
}
```

### DocumentConversionCapabilities

```csharp
namespace OoBDev.System.Documents.Conversion.Providers;

/// <summary>
/// Capabilities of a document conversion provider.
/// </summary>
public class DocumentConversionCapabilities
{
    /// <summary>
    /// Supports batch conversion operations.
    /// </summary>
    public bool SupportsBatchConversion { get; set; }

    /// <summary>
    /// Preserves document metadata during conversion.
    /// </summary>
    public bool SupportsMetadataPreservation { get; set; }

    /// <summary>
    /// Supports quality control options.
    /// </summary>
    public bool SupportsQualityControl { get; set; }

    /// <summary>
    /// Supports automatic format detection.
    /// </summary>
    public bool SupportsFormatDetection { get; set; }

    /// <summary>
    /// Maximum input document size in bytes.
    /// </summary>
    public long MaxInputSize { get; set; } = long.MaxValue;

    /// <summary>
    /// Supported quality levels.
    /// </summary>
    public ConversionQuality[] SupportedQualityLevels { get; set; } = Array.Empty<ConversionQuality>();
}
```

### FormatConversion

```csharp
/// <summary>
/// Represents a supported format conversion.
/// </summary>
public class FormatConversion
{
    /// <summary>
    /// Source format.
    /// </summary>
    public string SourceFormat { get; set; } = "";

    /// <summary>
    /// Target format.
    /// </summary>
    public string TargetFormat { get; set; } = "";

    /// <summary>
    /// Indicates if conversion is bidirectional (source ↔ target).
    /// </summary>
    public bool IsBidirectional { get; set; }
}
```

### ValidationResult

```csharp
/// <summary>
/// Result of conversion validation.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates if validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Error message if validation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Additional validation details.
    /// </summary>
    public IDictionary<string, object> Details { get; set; } = new Dictionary<string, object>();
}
```

---

## Exception Types

```csharp
namespace OoBDev.System.Documents.Conversion;

/// <summary>
/// Exception thrown when conversion between formats is not supported.
/// </summary>
public class ConversionNotSupportedException : Exception
{
    public ConversionNotSupportedException(string message) : base(message) { }
    public ConversionNotSupportedException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when conversion fails.
/// </summary>
public class ConversionFailedException : Exception
{
    public ConversionFailedException(string message) : base(message) { }
    public ConversionFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when format is invalid or cannot be detected.
/// </summary>
public class InvalidFormatException : Exception
{
    public InvalidFormatException(string message) : base(message) { }
    public InvalidFormatException(string message, Exception innerException)
        : base(message, innerException) { }
}

/// <summary>
/// Exception thrown when document exceeds maximum size.
/// </summary>
public class DocumentTooLargeException : Exception
{
    public long DocumentSize { get; }
    public long MaxSize { get; }

    public DocumentTooLargeException(long documentSize, long maxSize)
        : base($"Document size {documentSize} bytes exceeds maximum {maxSize} bytes")
    {
        DocumentSize = documentSize;
        MaxSize = maxSize;
    }
}

/// <summary>
/// Exception thrown when conversion validation fails.
/// </summary>
public class ValidationFailedException : Exception
{
    public ValidationFailedException(string message) : base(message) { }
    public ValidationFailedException(string message, Exception innerException)
        : base(message, innerException) { }
}
```

---

## Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.System.Documents.Conversion.Extensions;

/// <summary>
/// Extension methods for registering document conversion services.
/// </summary>
public static class DocumentConversionServiceExtensions
{
    /// <summary>
    /// Adds document conversion services with default providers.
    /// </summary>
    public static IServiceCollection AddDocumentConversion(
        this IServiceCollection services,
        Action<DocumentConversionOptions>? configure = null)
    {
        // Register core service
        services.TryAddSingleton<IDocumentConversionService, DocumentConversionService>();
        services.TryAddSingleton<IDocumentConversionProviderFactory, DocumentConversionProviderFactory>();

        // Register default providers
        services.TryAddSingleton<IDocumentConversionProvider, LibreOfficeConversionProvider>();
        services.TryAddSingleton<IDocumentConversionProvider, PlaywrightConversionProvider>();
        services.TryAddSingleton<IDocumentConversionProvider, PdfSharpConversionProvider>();
        services.TryAddSingleton<IDocumentConversionProvider, ImageMagickConversionProvider>();
        services.TryAddSingleton<IDocumentConversionProvider, TikaConversionProvider>();

        // Configure options
        if (configure != null)
        {
            services.Configure(configure);
        }

        return services;
    }

    /// <summary>
    /// Adds a custom conversion provider.
    /// </summary>
    public static IServiceCollection AddConversionProvider<TProvider>(
        this IServiceCollection services)
        where TProvider : class, IDocumentConversionProvider
    {
        services.TryAddSingleton<IDocumentConversionProvider, TProvider>();
        return services;
    }
}

/// <summary>
/// Configuration options for document conversion service.
/// </summary>
public class DocumentConversionOptions
{
    /// <summary>
    /// Enable result caching (default: true).
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Cache duration in hours (default: 1).
    /// </summary>
    public int CacheDurationHours { get; set; } = 1;

    /// <summary>
    /// Enable fallback to alternate providers on failure (default: true).
    /// </summary>
    public bool EnableFallback { get; set; } = true;

    /// <summary>
    /// Maximum concurrent conversions (default: 5).
    /// </summary>
    public int MaxConcurrentConversions { get; set; } = 5;

    /// <summary>
    /// Conversion timeout in seconds (default: 30).
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Default conversion quality (default: High).
    /// </summary>
    public ConversionQuality DefaultQuality { get; set; } = ConversionQuality.High;
}
```

---

## Usage Examples

### Example 1: Basic Document Conversion

```csharp
using OoBDev.System.Documents.Conversion;

// Convert Word document to PDF
var wordContent = await File.ReadAllBytesAsync("document.docx");

var result = await _conversionService.ConvertAsync(
    wordContent,
    "docx",
    "pdf");

if (result.Success)
{
    await File.WriteAllBytesAsync("document.pdf", result.ConvertedContent!);
    Console.WriteLine($"Converted in {result.Duration.TotalSeconds}s using {result.ProviderName}");
}
```

### Example 2: Conversion with Quality Control

```csharp
var context = new ConversionContext
{
    RequestingApplication = "report-generator",
    UserId = "user123",
    Quality = ConversionQuality.Maximum,
    AdditionalOptions = new Dictionary<string, object>
    {
        ["DPI"] = 300,  // High-resolution images
        ["ColorSpace"] = "RGB",
        ["Compress"] = false  // No compression for maximum quality
    }
};

var result = await _conversionService.ConvertAsync(
    imageContent,
    "png",
    "pdf",
    context);
```

### Example 3: HTML to PDF with Custom Options

```csharp
var context = new ConversionContext
{
    Quality = ConversionQuality.High,
    AdditionalOptions = new Dictionary<string, object>
    {
        ["PageSize"] = "A4",
        ["Orientation"] = "Portrait",
        ["Margins"] = new Dictionary<string, int>
        {
            ["Top"] = 20,
            ["Bottom"] = 20,
            ["Left"] = 15,
            ["Right"] = 15
        },
        ["ExecuteJavaScript"] = true,
        ["WaitForNetworkIdle"] = true,
        ["Viewport"] = new { Width = 1920, Height = 1080 }
    }
};

var htmlContent = await File.ReadAllBytesAsync("report.html");
var result = await _conversionService.ConvertAsync(htmlContent, "html", "pdf", context);
```

### Example 4: Batch Conversion

```csharp
var requests = new List<ConversionRequest>
{
    new() { SourceContent = doc1Content, SourceFormat = "docx", TargetFormat = "pdf" },
    new() { SourceContent = doc2Content, SourceFormat = "xlsx", TargetFormat = "pdf" },
    new() { SourceContent = doc3Content, SourceFormat = "pptx", TargetFormat = "pdf" }
};

var context = new ConversionContext
{
    Quality = ConversionQuality.Medium,  // Faster conversion for batch
    RequestingApplication = "batch-processor"
};

var results = await _conversionService.ConvertBatchAsync(requests, context);

foreach (var result in results)
{
    if (result.Success)
    {
        Console.WriteLine($"Request {result.RequestId}: {result.SourceFormat} → {result.TargetFormat} " +
                         $"({result.Duration.TotalSeconds}s, {result.ConvertedSize} bytes)");
    }
    else
    {
        Console.WriteLine($"Request {result.RequestId} failed: {result.ErrorMessage}");
    }
}
```

### Example 5: Format Detection

```csharp
// Automatic format detection
var unknownContent = await File.ReadAllBytesAsync("document.unknown");

var result = await _conversionService.ConvertAsync(
    unknownContent,
    "pdf");  // Only specify target format

Console.WriteLine($"Detected source format: {result.SourceFormat}");
```

### Example 6: Check Format Support

```csharp
// Check if conversion is supported
var isSupported = await _conversionService.IsSupportedAsync("docx", "pdf");
if (!isSupported)
{
    Console.WriteLine("Conversion not supported");
    return;
}

// Get all supported target formats for DOCX
var targetFormats = await _conversionService.GetSupportedTargetFormatsAsync("docx");
Console.WriteLine($"DOCX can be converted to: {string.Join(", ", targetFormats)}");
```

### Example 7: Custom Provider Selection

```csharp
var context = new ConversionContext
{
    AdditionalOptions = new Dictionary<string, object>
    {
        ["PreferredProvider"] = "libreoffice"  // Force use of LibreOffice provider
    }
};

var result = await _conversionService.ConvertAsync(
    wordContent,
    "docx",
    "pdf",
    context);

Console.WriteLine($"Used provider: {result.ProviderName}");
```

### Example 8: Conversion with Validation

```csharp
var context = new ConversionContext
{
    ValidateOutput = true  // Enable automatic validation
};

try
{
    var result = await _conversionService.ConvertAsync(
        pdfContent,
        "pdf",
        "docx",
        context);

    // Explicitly validate
    var validation = await _conversionService.ValidateConversionAsync(result);
    if (!validation.IsValid)
    {
        Console.WriteLine($"Validation failed: {validation.ErrorMessage}");
    }
}
catch (ValidationFailedException ex)
{
    Console.WriteLine($"Conversion validation failed: {ex.Message}");
}
```

### Example 9: Image Format Conversion

```csharp
var context = new ConversionContext
{
    Quality = ConversionQuality.High,
    AdditionalOptions = new Dictionary<string, object>
    {
        ["DPI"] = 300,
        ["ColorSpace"] = "RGB",
        ["Format"] = "PNG",  // Output format options
        ["Compression"] = "Lossless"
    }
};

var jpegContent = await File.ReadAllBytesAsync("image.jpg");
var result = await _conversionService.ConvertAsync(
    jpegContent,
    "jpg",
    "png",
    context);

await File.WriteAllBytesAsync("image.png", result.ConvertedContent!);
```

---

## Configuration Example

```json
{
  "DocumentConversion": {
    "EnableCaching": true,
    "CacheDurationHours": 2,
    "EnableFallback": true,
    "MaxConcurrentConversions": 10,
    "TimeoutSeconds": 60,
    "DefaultQuality": "High",
    "Providers": {
      "LibreOffice": {
        "Path": "/usr/bin/libreoffice",
        "Enabled": true
      },
      "Playwright": {
        "Enabled": true,
        "BrowserType": "Chromium"
      },
      "ImageMagick": {
        "Path": "/usr/bin/convert",
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
