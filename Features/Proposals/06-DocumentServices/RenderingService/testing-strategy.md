# Document Rendering Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Rendering Service
**Last Updated:** 2026-01-23

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (55+ tests)
- **Integration Tests** - End-to-end with real rendering providers (25+ tests)
- **Performance Tests** - Benchmark rendering speed and quality (12+ tests)
- **Visual Tests** - Validate rendered image quality (10+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │   Visual    │  (10 tests)
                    │    Tests    │
                    └─────────────┘
                  ┌───────────────────┐
                  │ Performance Tests │  (12 tests)
                  │                   │
                  └───────────────────┘
              ┌───────────────────────────┐
              │   Integration Tests       │  (25 tests)
              │                           │
              └───────────────────────────┘
          ┌─────────────────────────────────┐
          │       Unit Tests                │  (55+ tests)
          │                                 │
          └─────────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. DocumentRenderingService Tests

**File:** `DocumentRenderingServiceTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Documents.Rendering;

namespace OoBDev.System.Documents.Rendering.Tests;

[TestClass]
public class DocumentRenderingServiceTests
{
    private Mock<IDocumentRenderingProviderFactory> _mockProviderFactory;
    private Mock<IDocumentRenderingProvider> _mockProvider;
    private Mock<IMemoryCache> _mockCache;
    private DocumentRenderingService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockProviderFactory = new Mock<IDocumentRenderingProviderFactory>();
        _mockProvider = new Mock<IDocumentRenderingProvider>();
        _mockCache = new Mock<IMemoryCache>();

        _mockProvider.Setup(p => p.ProviderName).Returns("test-provider");
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentRenderingCapabilities
        {
            SupportsHighQuality = true,
            SupportsFastRendering = true,
            SupportedFormats = new[] { "pdf", "html" },
            MaxDpi = 600
        });

        _mockProviderFactory
            .Setup(f => f.GetProviderAsync(It.IsAny<string>()))
            .ReturnsAsync(_mockProvider.Object);

        _mockProviderFactory
            .Setup(f => f.GetProvidersAsync())
            .ReturnsAsync(new[] { _mockProvider.Object });

        var options = new DocumentRenderingOptions
        {
            EnableCaching = false,
            EnableFallback = false
        };

        _service = new DocumentRenderingService(
            _mockProviderFactory.Object,
            _mockCache.Object,
            options,
            Mock.Of<ILogger<DocumentRenderingService>>());
    }

    [TestMethod]
    public async Task RenderPageAsync_ValidPage_ReturnsResult()
    {
        // Arrange
        var documentContent = new byte[] { 1, 2, 3, 4 };
        var renderedImage = new byte[] { 5, 6, 7, 8 };

        _mockProvider
            .Setup(p => p.SupportsFormat("pdf"))
            .Returns(true);

        _mockProvider
            .Setup(p => p.RenderPageAsync(documentContent, "pdf", 1, It.IsAny<RenderingContext>()))
            .ReturnsAsync(new RenderingResult
            {
                Success = true,
                RenderedImage = renderedImage,
                PageNumber = 1,
                Width = 800,
                Height = 600,
                OutputFormat = "png",
                Size = renderedImage.Length,
                ProviderName = "test-provider"
            });

        // Act
        var result = await _service.RenderPageAsync(documentContent, "pdf", 1);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.PageNumber);
        Assert.AreEqual(800, result.Width);
        Assert.AreEqual(600, result.Height);
        CollectionAssert.AreEqual(renderedImage, result.RenderedImage);
    }

    [TestMethod]
    [ExpectedException(typeof(RenderingNotSupportedException))]
    public async Task RenderPageAsync_UnsupportedFormat_ThrowsException()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.SupportsFormat("unknown"))
            .Returns(false);

        // Act
        await _service.RenderPageAsync(new byte[] { 1, 2, 3 }, "unknown", 1);
    }

    [TestMethod]
    public async Task RenderPageAsync_WithContext_PassesContextToProvider()
    {
        // Arrange
        var context = new RenderingContext
        {
            RequestingApplication = "test-app",
            UserId = "user123",
            Dpi = 300,
            Width = 1920,
            Height = 1080,
            OutputFormat = "jpeg",
            Quality = 95
        };

        _mockProvider.Setup(p => p.SupportsFormat("pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.RenderPageAsync(It.IsAny<byte[]>(), "pdf", 1, It.IsAny<RenderingContext>()))
            .ReturnsAsync(new RenderingResult { Success = true });

        // Act
        await _service.RenderPageAsync(new byte[] { 1, 2, 3 }, "pdf", 1, context);

        // Assert
        _mockProvider.Verify(p => p.RenderPageAsync(
            It.IsAny<byte[]>(),
            "pdf",
            1,
            It.Is<RenderingContext>(ctx =>
                ctx.RequestingApplication == "test-app" &&
                ctx.UserId == "user123" &&
                ctx.Dpi == 300 &&
                ctx.Width == 1920 &&
                ctx.Height == 1080 &&
                ctx.OutputFormat == "jpeg" &&
                ctx.Quality == 95)),
            Times.Once);
    }

    [TestMethod]
    public async Task RenderPageAsync_ProviderFails_TriesFallbackProvider()
    {
        // Arrange
        var mockFallbackProvider = new Mock<IDocumentRenderingProvider>();
        mockFallbackProvider.Setup(p => p.ProviderName).Returns("fallback-provider");
        mockFallbackProvider.Setup(p => p.SupportsFormat("pdf")).Returns(true);
        mockFallbackProvider
            .Setup(p => p.RenderPageAsync(It.IsAny<byte[]>(), "pdf", 1, It.IsAny<RenderingContext>()))
            .ReturnsAsync(new RenderingResult { Success = true, ProviderName = "fallback-provider" });

        _mockProvider
            .Setup(p => p.SupportsFormat("pdf"))
            .Returns(true);

        _mockProvider
            .Setup(p => p.RenderPageAsync(It.IsAny<byte[]>(), "pdf", 1, It.IsAny<RenderingContext>()))
            .ThrowsAsync(new Exception("Provider failed"));

        _mockProviderFactory
            .Setup(f => f.GetProvidersAsync())
            .ReturnsAsync(new[] { _mockProvider.Object, mockFallbackProvider.Object });

        var options = new DocumentRenderingOptions { EnableFallback = true };
        var service = new DocumentRenderingService(
            _mockProviderFactory.Object,
            _mockCache.Object,
            options,
            Mock.Of<ILogger<DocumentRenderingService>>());

        // Act
        var result = await service.RenderPageAsync(new byte[] { 1, 2, 3 }, "pdf", 1);

        // Assert
        Assert.AreEqual("fallback-provider", result.ProviderName);
    }

    [TestMethod]
    public async Task RenderPagesAsync_MultiplePages_RendersAll()
    {
        // Arrange
        var pageNumbers = new[] { 1, 2, 3, 4, 5 };

        _mockProvider.Setup(p => p.SupportsFormat("pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.RenderPageAsync(It.IsAny<byte[]>(), "pdf", It.IsAny<int>(), It.IsAny<RenderingContext>()))
            .ReturnsAsync((byte[] content, string format, int page, RenderingContext ctx) =>
                new RenderingResult
                {
                    Success = true,
                    PageNumber = page,
                    RenderedImage = new byte[] { (byte)page }
                });

        // Act
        var results = await _service.RenderPagesAsync(new byte[] { 1, 2, 3 }, "pdf", pageNumbers);

        // Assert
        Assert.AreEqual(5, results.Count());
        Assert.IsTrue(results.All(r => r.Success));
        CollectionAssert.AreEqual(pageNumbers, results.Select(r => r.PageNumber).ToArray());
    }

    [TestMethod]
    public async Task RenderAllPagesAsync_Document_RendersAllPages()
    {
        // Arrange
        var documentContent = new byte[] { 1, 2, 3 };
        var pageCount = 10;

        _mockProvider.Setup(p => p.SupportsFormat("pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.GetPageCountAsync(documentContent, "pdf"))
            .ReturnsAsync(pageCount);

        _mockProvider
            .Setup(p => p.RenderPageAsync(documentContent, "pdf", It.IsAny<int>(), It.IsAny<RenderingContext>()))
            .ReturnsAsync((byte[] content, string format, int page, RenderingContext ctx) =>
                new RenderingResult
                {
                    Success = true,
                    PageNumber = page
                });

        // Act
        var results = await _service.RenderAllPagesAsync(documentContent, "pdf");

        // Assert
        Assert.AreEqual(pageCount, results.Count());
        Assert.IsTrue(results.All(r => r.Success));
    }

    [TestMethod]
    public async Task GenerateThumbnailAsync_Document_RendersThumbnail()
    {
        // Arrange
        var documentContent = new byte[] { 1, 2, 3 };

        _mockProvider.Setup(p => p.SupportsFormat("pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.RenderPageAsync(documentContent, "pdf", 1, It.IsAny<RenderingContext>()))
            .ReturnsAsync(new RenderingResult
            {
                Success = true,
                PageNumber = 1,
                Width = 256,
                Height = 256
            });

        // Act
        var result = await _service.GenerateThumbnailAsync(documentContent, "pdf");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.PageNumber);
        Assert.AreEqual(256, result.Width);
        Assert.AreEqual(256, result.Height);
    }

    [TestMethod]
    public async Task GetPageCountAsync_Document_ReturnsPageCount()
    {
        // Arrange
        var documentContent = new byte[] { 1, 2, 3 };
        var expectedPageCount = 15;

        _mockProvider.Setup(p => p.SupportsFormat("pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.GetPageCountAsync(documentContent, "pdf"))
            .ReturnsAsync(expectedPageCount);

        // Act
        var pageCount = await _service.GetPageCountAsync(documentContent, "pdf");

        // Assert
        Assert.AreEqual(expectedPageCount, pageCount);
    }
}
```

---

#### 2. Provider Tests (Example: PDFiumRenderingProvider)

**File:** `PDFiumRenderingProviderTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class PDFiumRenderingProviderTests
{
    private PDFiumRenderingProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        _provider = new PDFiumRenderingProvider(
            Mock.Of<ILogger<PDFiumRenderingProvider>>());
    }

    [TestMethod]
    public void SupportsFormat_Pdf_ReturnsTrue()
    {
        Assert.IsTrue(_provider.SupportsFormat("pdf"));
    }

    [TestMethod]
    public void SupportsFormat_UnsupportedFormat_ReturnsFalse()
    {
        Assert.IsFalse(_provider.SupportsFormat("html"));
    }

    [TestMethod]
    public void GetSupportedFormats_ReturnsFormats()
    {
        var formats = _provider.GetSupportedFormats().ToList();

        Assert.IsTrue(formats.Contains("pdf"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task RenderPageAsync_PdfPage_RendersSuccessfully()
    {
        // Requires test PDF file
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample.pdf");
        var context = new RenderingContext
        {
            Dpi = 96,
            OutputFormat = "png"
        };

        var result = await _provider.RenderPageAsync(pdfContent, "pdf", 1, context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.RenderedImage);
        Assert.IsTrue(result.RenderedImage.Length > 0);
        Assert.AreEqual("png", result.OutputFormat);
    }
}
```

---

## Integration Tests

### Test Coverage Areas

#### 1. End-to-End Rendering Tests

**File:** `RenderingIntegrationTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class RenderingIntegrationTests
{
    private IDocumentRenderingService _service;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDocumentRendering();
        services.AddMemoryCache();

        var serviceProvider = services.BuildServiceProvider();
        _service = serviceProvider.GetRequiredService<IDocumentRenderingService>();
    }

    [TestMethod]
    public async Task RenderPageAsync_PdfPage_ProducesValidImage()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/document.pdf");

        var result = await _service.RenderPageAsync(pdfContent, "pdf", 1);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.RenderedImage);
        Assert.IsTrue(result.RenderedImage.Length > 0);

        // Validate PNG structure
        Assert.IsTrue(IsPng(result.RenderedImage));
    }

    [TestMethod]
    public async Task RenderPageAsync_HighDpi_ProducesLargerImage()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample.pdf");

        var resultLowDpi = await _service.RenderPageAsync(
            pdfContent, "pdf", 1,
            new RenderingContext { Dpi = 72 });

        var resultHighDpi = await _service.RenderPageAsync(
            pdfContent, "pdf", 1,
            new RenderingContext { Dpi = 300 });

        Assert.IsTrue(resultHighDpi.Width > resultLowDpi.Width);
        Assert.IsTrue(resultHighDpi.Height > resultLowDpi.Height);
    }

    [TestMethod]
    public async Task GenerateThumbnailAsync_Pdf_ProducesThumbnail()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample.pdf");

        var result = await _service.GenerateThumbnailAsync(pdfContent, "pdf");

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.Width <= 512);
        Assert.IsTrue(result.Height <= 512);
    }

    [TestMethod]
    public async Task RenderAllPagesAsync_MultiPagePdf_RendersAll()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/multipage.pdf");

        var results = await _service.RenderAllPagesAsync(pdfContent, "pdf");

        Assert.IsTrue(results.Count() > 1);
        Assert.IsTrue(results.All(r => r.Success));

        // Verify sequential page numbers
        var pageNumbers = results.Select(r => r.PageNumber).OrderBy(p => p).ToArray();
        for (int i = 0; i < pageNumbers.Length; i++)
        {
            Assert.AreEqual(i + 1, pageNumbers[i]);
        }
    }

    private bool IsPng(byte[] content)
    {
        return content.Length > 8 &&
               content[0] == 0x89 && content[1] == 0x50 &&  // PNG
               content[2] == 0x4E && content[3] == 0x47;
    }
}
```

---

## Performance Tests

### Test Coverage Areas

**File:** `RenderingPerformanceTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class RenderingPerformanceTests
{
    private IDocumentRenderingService _service;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDocumentRendering(options =>
        {
            options.EnableCaching = true;
            options.MaxConcurrentRenders = 10;
        });
        services.AddMemoryCache();

        _service = services.BuildServiceProvider().GetRequiredService<IDocumentRenderingService>();
    }

    [TestMethod]
    public async Task RenderPageAsync_SinglePage_CompletesUnder2Seconds()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/sample.pdf");
        var stopwatch = Stopwatch.StartNew();

        var result = await _service.RenderPageAsync(content, "pdf", 1);

        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000,
            $"Rendering took {stopwatch.ElapsedMilliseconds}ms (expected < 2000ms)");
    }

    [TestMethod]
    public async Task GenerateThumbnailAsync_CompletesUnder1Second()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/sample.pdf");
        var stopwatch = Stopwatch.StartNew();

        var result = await _service.GenerateThumbnailAsync(content, "pdf");

        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000,
            $"Thumbnail generation took {stopwatch.ElapsedMilliseconds}ms (expected < 1000ms)");
    }

    [TestMethod]
    public async Task RenderPageAsync_CachedContent_ReturnsCachedResult()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/sample.pdf");

        // First render
        var stopwatch1 = Stopwatch.StartNew();
        await _service.RenderPageAsync(content, "pdf", 1);
        stopwatch1.Stop();

        // Second render (should use cache)
        var stopwatch2 = Stopwatch.StartNew();
        await _service.RenderPageAsync(content, "pdf", 1);
        stopwatch2.Stop();

        Assert.IsTrue(stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds / 10,
            "Cached rendering should be significantly faster");
    }

    [TestMethod]
    public async Task RenderAllPagesAsync_10Pages_Throughput()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/10page.pdf");

        var stopwatch = Stopwatch.StartNew();
        var results = await _service.RenderAllPagesAsync(content, "pdf");
        stopwatch.Stop();

        var successCount = results.Count(r => r.Success);
        var pagesPerSecond = successCount / stopwatch.Elapsed.TotalSeconds;

        Assert.IsTrue(pagesPerSecond >= 5,
            $"Throughput: {pagesPerSecond:F2} pages/second (expected >= 5)");
    }
}
```

---

## Visual Tests

### Test Coverage Areas

**File:** `RenderingVisualTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class RenderingVisualTests
{
    private IDocumentRenderingService _service;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDocumentRendering();
        _service = services.BuildServiceProvider().GetRequiredService<IDocumentRenderingService>();
    }

    [TestMethod]
    public async Task RenderPageAsync_DifferentDpi_ProducesExpectedSizes()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample.pdf");
        var results = new Dictionary<int, RenderingResult>();

        foreach (var dpi in new[] { 72, 96, 150, 300 })
        {
            var context = new RenderingContext { Dpi = dpi };
            results[dpi] = await _service.RenderPageAsync(pdfContent, "pdf", 1, context);
        }

        // Higher DPI should produce larger images
        Assert.IsTrue(results[300].Width > results[150].Width);
        Assert.IsTrue(results[150].Width > results[96].Width);
        Assert.IsTrue(results[96].Width > results[72].Width);
    }

    [TestMethod]
    public async Task RenderPageAsync_WithWatermark_AppliesWatermark()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample.pdf");

        var context = new RenderingContext
        {
            Watermark = new Watermark
            {
                Text = "TEST WATERMARK",
                Position = WatermarkPosition.Center,
                Opacity = 0.3f
            }
        };

        var result = await _service.RenderPageAsync(pdfContent, "pdf", 1, context);

        Assert.IsTrue(result.Success);

        // Save for manual inspection
        await File.WriteAllBytesAsync("TestOutput/watermarked.png", result.RenderedImage!);
    }

    [TestMethod]
    public async Task RenderPageAsync_OutputFormats_ProducesCorrectFormat()
    {
        var pdfContent = await File.ReadAllBytesAsync("TestFiles/sample.pdf");

        // Test PNG
        var pngResult = await _service.RenderPageAsync(
            pdfContent, "pdf", 1,
            new RenderingContext { OutputFormat = "png" });
        Assert.IsTrue(IsPng(pngResult.RenderedImage!));

        // Test JPEG
        var jpegResult = await _service.RenderPageAsync(
            pdfContent, "pdf", 1,
            new RenderingContext { OutputFormat = "jpeg" });
        Assert.IsTrue(IsJpeg(jpegResult.RenderedImage!));
    }

    private bool IsPng(byte[] content)
    {
        return content.Length > 8 &&
               content[0] == 0x89 && content[1] == 0x50 &&
               content[2] == 0x4E && content[3] == 0x47;
    }

    private bool IsJpeg(byte[] content)
    {
        return content.Length > 2 &&
               content[0] == 0xFF && content[1] == 0xD8;
    }
}
```

---

## Test Data Requirements

### Required Test Files

**TestFiles Directory Structure:**
```
TestFiles/
├── sample.pdf (1 page, simple content)
├── multipage.pdf (5 pages)
├── 10page.pdf (10 pages)
├── complex.pdf (images, tables, fonts)
├── sample.html
└── README.md
```

---

## Test Categories

- **Unit** - Fast, isolated, mocked dependencies
- **Integration** - Real providers, requires external libraries
- **DevLocal** - Performance tests, run manually
- **LiveIntegration** - Cloud-based rendering services (if applicable)

---

## Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| DocumentRenderingService | 90%+ |
| Providers | 80%+ |
| Extensions | 85%+ |
| Overall | 85%+ |

---

## CI/CD Integration

### Pipeline Tests
```yaml
- Unit tests: Run on every commit
- Integration tests: Run on PR merge
- Performance tests: Run nightly
- Visual tests: Run weekly with manual review
```

### Required Tools for Integration Tests
- PDFium library
- MuPDF library
- Playwright browsers
- .NET 10.0 SDK

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
