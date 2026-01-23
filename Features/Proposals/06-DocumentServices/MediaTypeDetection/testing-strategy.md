# Media Type Detection - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Media Type Detection
**Last Updated:** 2026-01-23

---

## Testing Overview

**Goal:** 85%+ code coverage

**Test Categories:**
- **Unit Tests** - 40+ tests
- **Integration Tests** - 15+ tests
- **Accuracy Tests** - 20+ tests

---

## Unit Tests

```csharp
[TestClass]
public class MediaTypeDetectionServiceTests
{
    [TestMethod]
    public async Task DetectAsync_PdfContent_ReturnsPdfFormat()
    {
        var pdfBytes = new byte[] { 0x25, 0x50, 0x44, 0x46 }; // %PDF
        var result = await _service.DetectAsync(pdfBytes);

        Assert.AreEqual("pdf", result.Format);
        Assert.AreEqual("application/pdf", result.MimeType);
        Assert.IsTrue(result.Confidence > 0.9);
    }

    [DataTestMethod]
    [DataRow(new byte[] { 0x89, 0x50, 0x4E, 0x47 }, "png", "image/png")]
    [DataRow(new byte[] { 0xFF, 0xD8, 0xFF }, "jpeg", "image/jpeg")]
    public async Task DetectAsync_VariousFormats_ReturnsCorrectFormat(byte[] magic, string format, string mime)
    {
        // Test implementation
    }
}
```

---

## Coverage Goals

| Component | Target |
|-----------|--------|
| MediaTypeDetectionService | 90%+ |
| Detectors | 85%+ |
| Overall | 85%+ |

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
