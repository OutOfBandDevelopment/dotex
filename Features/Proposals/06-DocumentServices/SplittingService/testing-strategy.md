# Document Splitting Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Splitting Service
**Last Updated:** 2026-01-23

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (50+ tests)
- **Integration Tests** - End-to-end with real splitting providers (20+ tests)
- **Performance Tests** - Benchmark splitting speed and accuracy (10+ tests)
- **Validation Tests** - Verify split integrity (8+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Validation  │  (8 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │ Performance Tests │  (10 tests)
                  │                   │
                  └───────────────────┘
              ┌───────────────────────────┐
              │   Integration Tests       │  (20 tests)
              │                           │
              └───────────────────────────┘
          ┌─────────────────────────────────┐
          │       Unit Tests                │  (50+ tests)
          │                                 │
          └─────────────────────────────────┘
```

---

## Unit Tests

### DocumentSplittingServiceTests.cs

```csharp
[TestClass]
public class DocumentSplittingServiceTests
{
    private Mock<IDocumentSplittingProviderFactory> _mockProviderFactory;
    private Mock<IDocumentSplittingProvider> _mockProvider;
    private DocumentSplittingService _service;

    [TestMethod]
    public async Task SplitByPagesAsync_ValidRanges_ReturnsParts()
    {
        // Arrange
        var pageRanges = new[] { new[] { 1, 5 }, new[] { 6, 10 } };
        _mockProvider
            .Setup(p => p.SplitAsync(It.IsAny<byte[]>(), "pdf", SplitMode.PageRanges, It.IsAny<SplittingContext>()))
            .ReturnsAsync(new SplittingResult
            {
                Success = true,
                Parts = new[]
                {
                    new SplitPart { PartNumber = 1, StartPage = 1, EndPage = 5, PageCount = 5 },
                    new SplitPart { PartNumber = 2, StartPage = 6, EndPage = 10, PageCount = 5 }
                },
                TotalParts = 2
            });

        // Act
        var result = await _service.SplitByPagesAsync(new byte[] { 1, 2, 3 }, "pdf", pageRanges);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.TotalParts);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidSplitRangeException))]
    public async Task SplitByPagesAsync_InvalidRanges_ThrowsException()
    {
        var invalidRanges = new[] { new[] { 10, 5 } }; // End before start
        await _service.SplitByPagesAsync(new byte[] { 1, 2, 3 }, "pdf", invalidRanges);
    }

    [TestMethod]
    public async Task SplitEveryNPagesAsync_10Pages_CreatesCorrectParts()
    {
        // Test implementation
    }
}
```

---

## Integration Tests

### SplittingIntegrationTests.cs

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class SplittingIntegrationTests
{
    private IDocumentSplittingService _service;

    [TestMethod]
    public async Task SplitByPagesAsync_PdfDocument_ProducesValidParts()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample-10pages.pdf");
        var pageRanges = new[] { new[] { 1, 3 }, new[] { 4, 7 }, new[] { 8, 10 } };

        var result = await _service.SplitByPagesAsync(pdfContent, "pdf", pageRanges);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.TotalParts);
        Assert.IsTrue(result.Parts.All(p => p.Content.Length > 0));
    }
}
```

---

## Performance Tests

### SplittingPerformanceTests.cs

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class SplittingPerformanceTests
{
    [TestMethod]
    public async Task SplitEveryNPagesAsync_LargeDocument_CompletesUnder15Seconds()
    {
        var largeContent = await File.ReadAllBytesAsync("TestFiles/500pages.pdf");
        var stopwatch = Stopwatch.StartNew();

        var result = await _service.SplitEveryNPagesAsync(largeContent, "pdf", 50);

        stopwatch.Stop();
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 15000);
    }
}
```

---

## Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| DocumentSplittingService | 90%+ |
| Providers | 80%+ |
| Overall | 85%+ |

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
