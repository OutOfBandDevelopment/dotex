# Media Type Detection - Requirements

**Epic:** 6 - Document Services
**Feature:** Media Type Detection
**Priority:** HIGH (Foundation)
**Complexity:** MEDIUM
**Estimated LOC:** ~280

---

## Overview

Context-based media type detection service for identifying document formats using magic bytes, file extensions, MIME types, and content analysis. Critical foundation service used by all other document services.

---

## Business Requirements

### BR-1: Magic Byte Detection
**As a** developer
**I want** to detect document format from content
**So that** I can process documents without knowing their format beforehand

**Acceptance Criteria:**
- Detect format from file signature (magic bytes)
- Support 50+ common formats
- Return MIME type and file extension
- Confidence score for detection
- Fast detection (< 100ms)

**Supported Formats:**
```
Documents: PDF, DOCX, DOC, XLSX, XLS, PPTX, PPT, ODT, ODS, ODP
Images: PNG, JPEG, GIF, TIFF, BMP, WebP, SVG
Archives: ZIP, RAR, 7Z, TAR, GZ
Text: TXT, CSV, JSON, XML, HTML
Others: RTF, PS, EPS
```

---

### BR-2: Multi-Strategy Detection
**As a** developer
**I want** multiple detection strategies
**So that** I can achieve high accuracy

**Detection Strategies:**
1. Magic bytes (primary)
2. File extension
3. MIME type
4. Content analysis
5. Combined strategy (uses all)

**Acceptance Criteria:**
- Provider pattern for detection strategies
- Configurable strategy selection
- Fallback to next strategy on failure
- Confidence scoring per strategy

---

### BR-3: Custom Format Registration
**As a** developer
**I want** to register custom format signatures
**So that** I can detect proprietary formats

**Acceptance Criteria:**
- Register custom magic byte patterns
- Register custom MIME types
- Register custom file extensions
- Priority ordering for custom formats

---

### BR-4: Batch Detection
**As a** developer
**I want** to detect formats for multiple files
**So that** I can process batches efficiently

**Acceptance Criteria:**
- Batch detect multiple files
- Parallel processing
- Returns results for all files

---

## Technical Requirements

### TR-1: Interface Design
```csharp
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

### TR-2: Performance Requirements
- **Single detection:** < 50ms
- **Batch detection (100 files):** < 2 seconds
- **Memory usage:** < 50MB
- **Accuracy:** > 95%

---

## Success Criteria

- ✅ Detect 50+ formats via magic bytes
- ✅ 95%+ detection accuracy
- ✅ < 50ms detection time
- ✅ Support custom format registration
- ✅ 80%+ test coverage

---

## Dependencies

### Internal
- OoBDev.System.Documents.Abstractions

### External
- .NET 10.0 BCL

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 6 Overview](../README.md)
