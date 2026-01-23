# OCR Service - API Design

**Epic:** 6 - Document Services
**Feature:** OCR Service
**Last Updated:** 2026-01-23

---

## Core Interfaces

```csharp
namespace OoBDev.System.Documents.Ocr;

public interface IOcrService
{
    Task<OcrResult> RecognizeAsync(byte[] imageContent, OcrContext? context = null);
    Task<OcrResult> RecognizeFromDocumentAsync(byte[] documentContent, string format, int pageNumber, OcrContext? context = null);
    Task<IEnumerable<OcrResult>> RecognizeBatchAsync(IEnumerable<byte[]> imageContents, OcrContext? context = null);
    Task<bool> SupportsLanguageAsync(string languageCode);
}

public class OcrContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public string[] Languages { get; set; } = new[] { "eng" };
    public bool AutoDetectLanguage { get; set; } = false;
    public bool PreserveLayout { get; set; } = true;
    public OcrPreprocessing Preprocessing { get; set; } = OcrPreprocessing.Auto;
    public double MinConfidence { get; set; } = 0.0;
    public IDictionary<string, object> AdditionalOptions { get; set; } = new Dictionary<string, object>();
}

public class OcrResult
{
    public bool Success { get; set; }
    public string? ExtractedText { get; set; }
    public double Confidence { get; set; }
    public string? DetectedLanguage { get; set; }
    public IEnumerable<TextBlock> Blocks { get; set; } = Array.Empty<TextBlock>();
    public TimeSpan Duration { get; set; }
    public string? ProviderName { get; set; }
    public string? ErrorMessage { get; set; }
}
```

---

## Usage Examples

### Example 1: Extract Text from Image

```csharp
var imageContent = await File.ReadAllBytesAsync("scanned-document.png");

var result = await _ocrService.RecognizeAsync(imageContent);

if (result.Success)
{
    Console.WriteLine($"Text: {result.ExtractedText}");
    Console.WriteLine($"Confidence: {result.Confidence:P}");
}
```

### Example 2: Multi-Language OCR

```csharp
var context = new OcrContext
{
    Languages = new[] { "eng", "spa", "fra" }, // English, Spanish, French
    AutoDetectLanguage = true
};

var result = await _ocrService.RecognizeAsync(imageContent, context);
Console.WriteLine($"Detected language: {result.DetectedLanguage}");
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
