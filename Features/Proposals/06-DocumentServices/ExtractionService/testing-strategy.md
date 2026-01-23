# Document Extraction Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Extraction Service
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and format tests.

**Test Categories:**
- **Unit Tests** - Isolated testing with mocks (55+ tests)
- **Integration Tests** - End-to-end with real providers (35+ tests)
- **Performance Tests** - Benchmark extraction speed (10+ tests)
- **Format Tests** - Validate all supported formats (20+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │  Format     │  (20 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │ Performance Tests │  (10 tests)
                  │                   │
                  └───────────────────┘
              ┌───────────────────────────┐
              │   Integration Tests       │  (35 tests)
              │                           │
              └───────────────────────────┘
          ┌─────────────────────────────────┐
          │       Unit Tests                │  (55+ tests)
          │                                 │
          └─────────────────────────────────┘
```

---

## Unit Tests

**File:** `DocumentExtractionServiceTests.cs`

```csharp
[TestClass]
public class DocumentExtractionServiceTests
{
    [TestMethod]
    public async Task ExtractTextAsync_ValidPdf_ReturnsText()
    {
        // Test text extraction
    }

    [TestMethod]
    public async Task ExtractTextAsync_ScannedPdf_UsesOcr()
    {
        // Test OCR fallback
    }

    [TestMethod]
    public async Task ExtractMetadataAsync_ValidDocument_ReturnsMetadata()
    {
        // Test metadata extraction
    }

    [TestMethod]
    public async Task ExtractImagesAsync_DocumentWithImages_ReturnsImages()
    {
        // Test image extraction
    }

    [TestMethod]
    public async Task ExtractTablesAsync_DocumentWithTables_ReturnsTables()
    {
        // Test table extraction
    }

    [TestMethod]
    public async Task ExtractAllAsync_ValidDocument_ReturnsAllContent()
    {
        // Test full extraction
    }

    [TestMethod]
    public async Task ExtractBatchAsync_MultipleDocuments_ExtractsAll()
    {
        // Test batch extraction
    }
}
```

---

## Integration Tests

**File:** `ExtractionIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class ExtractionIntegrationTests
{
    [TestMethod]
    public async Task ExtractTextAsync_PdfWithNativeText_ExtractsCorrectly()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/sample.pdf");
        var result = await _service.ExtractTextAsync(content, "pdf");

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ExtractedText.Length > 100);
    }

    [TestMethod]
    public async Task ExtractMetadataAsync_Word_ExtractsAllMetadata()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/document.docx");
        var metadata = await _service.ExtractMetadataAsync(content, "docx");

        Assert.IsNotNull(metadata.Title);
        Assert.IsNotNull(metadata.Author);
        Assert.IsTrue(metadata.PageCount > 0);
    }

    [DataTestMethod]
    [DataRow("pdf")]
    [DataRow("docx")]
    [DataRow("xlsx")]
    [DataRow("pptx")]
    [DataRow("html")]
    public async Task ExtractTextAsync_SupportedFormats_Succeeds(string format)
    {
        var testFile = $"TestFiles/sample.{format}";
        var content = await File.ReadAllBytesAsync(testFile);
        var result = await _service.ExtractTextAsync(content, format);

        Assert.IsTrue(result.Success);
    }
}
```

---

## Performance Tests

**File:** `ExtractionPerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class ExtractionPerformanceTests
{
    [TestMethod]
    public async Task ExtractTextAsync_SmallDocument_CompletesUnder1Second()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/small.pdf");
        var stopwatch = Stopwatch.StartNew();

        await _service.ExtractTextAsync(content, "pdf");

        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000);
    }

    [TestMethod]
    public async Task ExtractBatchAsync_10Documents_Throughput()
    {
        // Test batch throughput
    }
}
```

---

## Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| DocumentExtractionService | 90%+ |
| Providers | 80%+ |
| Overall | 85%+ |

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
