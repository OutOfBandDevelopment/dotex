# Media Type Detection - API Design

**Epic:** 6 - Document Services
**Feature:** Media Type Detection
**Last Updated:** 2026-01-23

---

## Core Interfaces

```csharp
namespace OoBDev.System.Documents.MediaTypeDetection;

public interface IMediaTypeDetectionService
{
    Task<DetectionResult> DetectAsync(byte[] content, DetectionContext? context = null);
    Task<DetectionResult> DetectFromExtensionAsync(string extension);
    Task<DetectionResult> DetectFromMimeTypeAsync(string mimeType);
    Task<IEnumerable<DetectionResult>> DetectBatchAsync(IEnumerable<byte[]> contents);
    void RegisterCustomFormat(FormatSignature signature);
}

public class DetectionResult
{
    public string Format { get; set; } = "";
    public string MimeType { get; set; } = "";
    public string Extension { get; set; } = "";
    public double Confidence { get; set; }
    public string DetectionStrategy { get; set; } = "";
}

public class FormatSignature
{
    public string Format { get; set; } = "";
    public byte[] MagicBytes { get; set; } = Array.Empty<byte>();
    public int Offset { get; set; } = 0;
    public string MimeType { get; set; } = "";
    public string[] Extensions { get; set; } = Array.Empty<string>();
}
```

---

## Usage Examples

### Example 1: Detect from Content

```csharp
var content = await File.ReadAllBytesAsync("unknown-file");
var result = await _detectionService.DetectAsync(content);

Console.WriteLine($"Format: {result.Format}");
Console.WriteLine($"MIME: {result.MimeType}");
Console.WriteLine($"Extension: {result.Extension}");
Console.WriteLine($"Confidence: {result.Confidence:P}");
```

### Example 2: Register Custom Format

```csharp
var customSignature = new FormatSignature
{
    Format = "custom",
    MagicBytes = new byte[] { 0x43, 0x55, 0x53, 0x54 }, // "CUST"
    MimeType = "application/x-custom",
    Extensions = new[] { "cst", "custom" }
};

_detectionService.RegisterCustomFormat(customSignature);
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
