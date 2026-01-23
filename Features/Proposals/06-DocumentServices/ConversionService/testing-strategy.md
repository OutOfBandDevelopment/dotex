# Document Conversion Service - Testing Strategy

**Epic:** 6 - Document Services
**Feature:** Document Conversion Service
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks (60+ tests)
- **Integration Tests** - End-to-end with real conversion providers (30+ tests)
- **Performance Tests** - Benchmark conversion speed and quality (15+ tests)
- **Format Tests** - Validate all supported format combinations (25+ tests)

---

## Test Pyramid

```
                    ┌─────────────┐
                    │  Format     │  (25 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │ Performance Tests │  (15 tests)
                  │                   │
                  └───────────────────┘
              ┌───────────────────────────┐
              │   Integration Tests       │  (30 tests)
              │                           │
              └───────────────────────────┘
          ┌─────────────────────────────────┐
          │       Unit Tests                │  (60+ tests)
          │                                 │
          └─────────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. DocumentConversionService Tests

**File:** `DocumentConversionServiceTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Documents.Conversion;

namespace OoBDev.System.Documents.Conversion.Tests;

[TestClass]
public class DocumentConversionServiceTests
{
    private Mock<IDocumentConversionProviderFactory> _mockProviderFactory;
    private Mock<IDocumentConversionProvider> _mockProvider;
    private Mock<IMediaTypeDetectionService> _mockMediaDetection;
    private Mock<IMemoryCache> _mockCache;
    private DocumentConversionService _service;

    [TestInitialize]
    public void Setup()
    {
        _mockProviderFactory = new Mock<IDocumentConversionProviderFactory>();
        _mockProvider = new Mock<IDocumentConversionProvider>();
        _mockMediaDetection = new Mock<IMediaTypeDetectionService>();
        _mockCache = new Mock<IMemoryCache>();

        _mockProvider.Setup(p => p.ProviderName).Returns("test-provider");
        _mockProvider.Setup(p => p.Capabilities).Returns(new DocumentConversionCapabilities
        {
            SupportsBatchConversion = true,
            SupportsMetadataPreservation = true,
            SupportsQualityControl = true
        });

        _mockProviderFactory
            .Setup(f => f.GetProviderAsync(It.IsAny<string>()))
            .ReturnsAsync(_mockProvider.Object);

        _mockProviderFactory
            .Setup(f => f.GetProvidersAsync())
            .ReturnsAsync(new[] { _mockProvider.Object });

        var options = new DocumentConversionOptions
        {
            EnableCaching = false,
            EnableFallback = false
        };

        _service = new DocumentConversionService(
            _mockProviderFactory.Object,
            _mockMediaDetection.Object,
            _mockCache.Object,
            options,
            Mock.Of<ILogger<DocumentConversionService>>());
    }

    [TestMethod]
    public async Task ConvertAsync_ValidConversion_ReturnsResult()
    {
        // Arrange
        var sourceContent = new byte[] { 1, 2, 3, 4 };
        var convertedContent = new byte[] { 5, 6, 7, 8 };

        _mockProvider
            .Setup(p => p.SupportsConversion("docx", "pdf"))
            .Returns(true);

        _mockProvider
            .Setup(p => p.ConvertAsync(sourceContent, "docx", "pdf", It.IsAny<ConversionContext>()))
            .ReturnsAsync(new ConversionResult
            {
                Success = true,
                ConvertedContent = convertedContent,
                SourceFormat = "docx",
                TargetFormat = "pdf",
                SourceSize = sourceContent.Length,
                ConvertedSize = convertedContent.Length,
                ProviderName = "test-provider"
            });

        // Act
        var result = await _service.ConvertAsync(sourceContent, "docx", "pdf");

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual("docx", result.SourceFormat);
        Assert.AreEqual("pdf", result.TargetFormat);
        Assert.AreEqual(convertedContent.Length, result.ConvertedSize);
        CollectionAssert.AreEqual(convertedContent, result.ConvertedContent);
    }

    [TestMethod]
    [ExpectedException(typeof(ConversionNotSupportedException))]
    public async Task ConvertAsync_UnsupportedConversion_ThrowsException()
    {
        // Arrange
        _mockProvider
            .Setup(p => p.SupportsConversion("unknown", "pdf"))
            .Returns(false);

        // Act
        await _service.ConvertAsync(new byte[] { 1, 2, 3 }, "unknown", "pdf");
    }

    [TestMethod]
    public async Task ConvertAsync_WithContext_PassesContextToProvider()
    {
        // Arrange
        var context = new ConversionContext
        {
            RequestingApplication = "test-app",
            UserId = "user123",
            Quality = ConversionQuality.Maximum,
            PreserveMetadata = true
        };

        _mockProvider.Setup(p => p.SupportsConversion("docx", "pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.ConvertAsync(It.IsAny<byte[]>(), "docx", "pdf", It.IsAny<ConversionContext>()))
            .ReturnsAsync(new ConversionResult { Success = true });

        // Act
        await _service.ConvertAsync(new byte[] { 1, 2, 3 }, "docx", "pdf", context);

        // Assert
        _mockProvider.Verify(p => p.ConvertAsync(
            It.IsAny<byte[]>(),
            "docx",
            "pdf",
            It.Is<ConversionContext>(ctx =>
                ctx.RequestingApplication == "test-app" &&
                ctx.UserId == "user123" &&
                ctx.Quality == ConversionQuality.Maximum &&
                ctx.PreserveMetadata == true)),
            Times.Once);
    }

    [TestMethod]
    public async Task ConvertAsync_ProviderFails_TriesFallbackProvider()
    {
        // Arrange
        var mockFallbackProvider = new Mock<IDocumentConversionProvider>();
        mockFallbackProvider.Setup(p => p.ProviderName).Returns("fallback-provider");
        mockFallbackProvider.Setup(p => p.SupportsConversion("docx", "pdf")).Returns(true);
        mockFallbackProvider
            .Setup(p => p.ConvertAsync(It.IsAny<byte[]>(), "docx", "pdf", It.IsAny<ConversionContext>()))
            .ReturnsAsync(new ConversionResult { Success = true, ProviderName = "fallback-provider" });

        _mockProvider
            .Setup(p => p.SupportsConversion("docx", "pdf"))
            .Returns(true);

        _mockProvider
            .Setup(p => p.ConvertAsync(It.IsAny<byte[]>(), "docx", "pdf", It.IsAny<ConversionContext>()))
            .ThrowsAsync(new Exception("Provider failed"));

        _mockProviderFactory
            .Setup(f => f.GetProvidersAsync())
            .ReturnsAsync(new[] { _mockProvider.Object, mockFallbackProvider.Object });

        var options = new DocumentConversionOptions { EnableFallback = true };
        var service = new DocumentConversionService(
            _mockProviderFactory.Object,
            _mockMediaDetection.Object,
            _mockCache.Object,
            options,
            Mock.Of<ILogger<DocumentConversionService>>());

        // Act
        var result = await service.ConvertAsync(new byte[] { 1, 2, 3 }, "docx", "pdf");

        // Assert
        Assert.AreEqual("fallback-provider", result.ProviderName);
        mockFallbackProvider.Verify(p => p.ConvertAsync(
            It.IsAny<byte[]>(), "docx", "pdf", It.IsAny<ConversionContext>()), Times.Once);
    }

    [TestMethod]
    public async Task ConvertAsync_AutoDetectFormat_DetectsAndConverts()
    {
        // Arrange
        var content = new byte[] { 1, 2, 3, 4 };
        _mockMediaDetection
            .Setup(m => m.DetectAsync(content))
            .ReturnsAsync("docx");

        _mockProvider.Setup(p => p.SupportsConversion("docx", "pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.ConvertAsync(content, "docx", "pdf", It.IsAny<ConversionContext>()))
            .ReturnsAsync(new ConversionResult
            {
                Success = true,
                SourceFormat = "docx",
                TargetFormat = "pdf"
            });

        // Act
        var result = await _service.ConvertAsync(content, "pdf");  // No source format specified

        // Assert
        Assert.AreEqual("docx", result.SourceFormat);
        _mockMediaDetection.Verify(m => m.DetectAsync(content), Times.Once);
    }

    [TestMethod]
    public async Task ConvertBatchAsync_MultipleRequests_ConvertsAll()
    {
        // Arrange
        var requests = new[]
        {
            new ConversionRequest { SourceContent = new byte[] { 1 }, SourceFormat = "docx", TargetFormat = "pdf" },
            new ConversionRequest { SourceContent = new byte[] { 2 }, SourceFormat = "xlsx", TargetFormat = "pdf" },
            new ConversionRequest { SourceContent = new byte[] { 3 }, SourceFormat = "pptx", TargetFormat = "pdf" }
        };

        _mockProvider.Setup(p => p.SupportsConversion(It.IsAny<string>(), "pdf")).Returns(true);
        _mockProvider
            .Setup(p => p.ConvertAsync(It.IsAny<byte[]>(), It.IsAny<string>(), "pdf", It.IsAny<ConversionContext>()))
            .ReturnsAsync((byte[] content, string source, string target, ConversionContext ctx) =>
                new ConversionResult
                {
                    Success = true,
                    SourceFormat = source,
                    TargetFormat = target
                });

        // Act
        var results = await _service.ConvertBatchAsync(requests);

        // Assert
        Assert.AreEqual(3, results.Count());
        Assert.IsTrue(results.All(r => r.Success));
        Assert.IsTrue(results.All(r => r.TargetFormat == "pdf"));
    }

    [TestMethod]
    public async Task IsSupportedAsync_SupportedConversion_ReturnsTrue()
    {
        // Arrange
        _mockProvider.Setup(p => p.SupportsConversion("docx", "pdf")).Returns(true);

        // Act
        var isSupported = await _service.IsSupportedAsync("docx", "pdf");

        // Assert
        Assert.IsTrue(isSupported);
    }

    [TestMethod]
    public async Task IsSupportedAsync_UnsupportedConversion_ReturnsFalse()
    {
        // Arrange
        _mockProvider.Setup(p => p.SupportsConversion("unknown", "pdf")).Returns(false);

        // Act
        var isSupported = await _service.IsSupportedAsync("unknown", "pdf");

        // Assert
        Assert.IsFalse(isSupported);
    }

    [TestMethod]
    public async Task GetSupportedTargetFormatsAsync_ValidSourceFormat_ReturnsFormats()
    {
        // Arrange
        var conversions = new[]
        {
            new FormatConversion { SourceFormat = "docx", TargetFormat = "pdf" },
            new FormatConversion { SourceFormat = "docx", TargetFormat = "html" },
            new FormatConversion { SourceFormat = "docx", TargetFormat = "txt" }
        };

        _mockProvider
            .Setup(p => p.GetSupportedConversions())
            .Returns(conversions);

        // Act
        var targetFormats = await _service.GetSupportedTargetFormatsAsync("docx");

        // Assert
        Assert.AreEqual(3, targetFormats.Count());
        CollectionAssert.AreEquivalent(
            new[] { "pdf", "html", "txt" },
            targetFormats.ToArray());
    }

    [TestMethod]
    public async Task ValidateConversionAsync_ValidResult_ReturnsValid()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            ConvertedContent = new byte[] { 1, 2, 3 },
            TargetFormat = "pdf"
        };

        _mockMediaDetection
            .Setup(m => m.DetectAsync(result.ConvertedContent!))
            .ReturnsAsync("pdf");

        // Act
        var validation = await _service.ValidateConversionAsync(result);

        // Assert
        Assert.IsTrue(validation.IsValid);
    }

    [TestMethod]
    public async Task ValidateConversionAsync_EmptyContent_ReturnsInvalid()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            ConvertedContent = Array.Empty<byte>(),
            TargetFormat = "pdf"
        };

        // Act
        var validation = await _service.ValidateConversionAsync(result);

        // Assert
        Assert.IsFalse(validation.IsValid);
        Assert.IsTrue(validation.ErrorMessage!.Contains("empty"));
    }

    [TestMethod]
    public async Task ValidateConversionAsync_WrongFormat_ReturnsInvalid()
    {
        // Arrange
        var result = new ConversionResult
        {
            Success = true,
            ConvertedContent = new byte[] { 1, 2, 3 },
            TargetFormat = "pdf"
        };

        _mockMediaDetection
            .Setup(m => m.DetectAsync(result.ConvertedContent!))
            .ReturnsAsync("html");  // Wrong format

        // Act
        var validation = await _service.ValidateConversionAsync(result);

        // Assert
        Assert.IsFalse(validation.IsValid);
        Assert.IsTrue(validation.ErrorMessage!.Contains("Expected pdf, got html"));
    }
}
```

---

#### 2. Provider Tests (Example: LibreOfficeConversionProvider)

**File:** `LibreOfficeConversionProviderTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class LibreOfficeConversionProviderTests
{
    private LibreOfficeConversionProvider _provider;

    [TestInitialize]
    public void Setup()
    {
        _provider = new LibreOfficeConversionProvider(
            "/usr/bin/libreoffice",
            Mock.Of<ILogger<LibreOfficeConversionProvider>>());
    }

    [TestMethod]
    public void SupportsConversion_DocxToPdf_ReturnsTrue()
    {
        Assert.IsTrue(_provider.SupportsConversion("docx", "pdf"));
    }

    [TestMethod]
    public void SupportsConversion_UnsupportedFormats_ReturnsFalse()
    {
        Assert.IsFalse(_provider.SupportsConversion("unknown", "pdf"));
    }

    [TestMethod]
    public void GetSupportedConversions_ReturnsAllConversions()
    {
        var conversions = _provider.GetSupportedConversions().ToList();

        Assert.IsTrue(conversions.Any(c => c.SourceFormat == "docx" && c.TargetFormat == "pdf"));
        Assert.IsTrue(conversions.Any(c => c.SourceFormat == "xlsx" && c.TargetFormat == "pdf"));
        Assert.IsTrue(conversions.Any(c => c.SourceFormat == "pptx" && c.TargetFormat == "pdf"));
    }

    [TestMethod]
    [TestCategory(TestCategories.Integration)]
    public async Task ConvertAsync_DocxToPdf_ConvertsSuccessfully()
    {
        // Requires LibreOffice installed
        var docxContent = await File.ReadAllBytesAsync("TestFiles/sample.docx");
        var context = new ConversionContext { Quality = ConversionQuality.High };

        var result = await _provider.ConvertAsync(docxContent, "docx", "pdf", context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.ConvertedContent);
        Assert.IsTrue(result.ConvertedContent.Length > 0);
        Assert.AreEqual("pdf", result.TargetFormat);
    }
}
```

---

## Integration Tests

### Test Coverage Areas

#### 1. End-to-End Conversion Tests

**File:** `ConversionIntegrationTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class ConversionIntegrationTests
{
    private IDocumentConversionService _service;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDocumentConversion();
        services.AddMemoryCache();

        var serviceProvider = services.BuildServiceProvider();
        _service = serviceProvider.GetRequiredService<IDocumentConversionService>();
    }

    [TestMethod]
    public async Task ConvertAsync_WordToPdf_ProducesValidPdf()
    {
        var wordContent = await File.ReadAllBytesAsync("TestFiles/document.docx");

        var result = await _service.ConvertAsync(wordContent, "docx", "pdf");

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.ConvertedContent);
        Assert.IsTrue(result.ConvertedContent.Length > 0);

        // Validate PDF structure
        Assert.IsTrue(IsPdf(result.ConvertedContent));
    }

    [TestMethod]
    public async Task ConvertAsync_HtmlToPdf_WithCss_PreservesFormatting()
    {
        var html = @"
            <html>
                <style>
                    body { font-family: Arial; }
                    h1 { color: blue; }
                </style>
                <body>
                    <h1>Test Document</h1>
                    <p>This is a test.</p>
                </body>
            </html>";

        var htmlContent = Encoding.UTF8.GetBytes(html);
        var context = new ConversionContext
        {
            Quality = ConversionQuality.High,
            AdditionalOptions = new Dictionary<string, object>
            {
                ["ExecuteJavaScript"] = false
            }
        };

        var result = await _service.ConvertAsync(htmlContent, "html", "pdf", context);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(IsPdf(result.ConvertedContent!));
    }

    [TestMethod]
    public async Task ConvertBatchAsync_MultipleDocuments_ConvertsAll()
    {
        var requests = new List<ConversionRequest>();

        foreach (var file in Directory.GetFiles("TestFiles", "*.docx"))
        {
            var content = await File.ReadAllBytesAsync(file);
            requests.Add(new ConversionRequest
            {
                SourceContent = content,
                SourceFormat = "docx",
                TargetFormat = "pdf"
            });
        }

        var results = await _service.ConvertBatchAsync(requests);

        Assert.AreEqual(requests.Count, results.Count());
        Assert.IsTrue(results.All(r => r.Success));
    }

    private bool IsPdf(byte[] content)
    {
        return content.Length > 4 &&
               content[0] == 0x25 && content[1] == 0x50 && // %P
               content[2] == 0x44 && content[3] == 0x46;   // DF
    }
}
```

---

## Performance Tests

### Test Coverage Areas

**File:** `ConversionPerformanceTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class ConversionPerformanceTests
{
    private IDocumentConversionService _service;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDocumentConversion(options =>
        {
            options.EnableCaching = true;
            options.MaxConcurrentConversions = 10
        });
        services.AddMemoryCache();

        _service = services.BuildServiceProvider().GetRequiredService<IDocumentConversionService>();
    }

    [TestMethod]
    public async Task ConvertAsync_SmallDocument_CompletesUnder2Seconds()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/small.docx");
        var stopwatch = Stopwatch.StartNew();

        var result = await _service.ConvertAsync(content, "docx", "pdf");

        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 2000,
            $"Conversion took {stopwatch.ElapsedMilliseconds}ms (expected < 2000ms)");
    }

    [TestMethod]
    public async Task ConvertAsync_LargeDocument_CompletesUnder30Seconds()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/large.docx");
        var stopwatch = Stopwatch.StartNew();

        var result = await _service.ConvertAsync(content, "docx", "pdf");

        stopwatch.Stop();

        Assert.IsTrue(result.Success);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 30000,
            $"Conversion took {stopwatch.ElapsedMilliseconds}ms (expected < 30000ms)");
    }

    [TestMethod]
    public async Task ConvertAsync_CachedContent_ReturnsCachedResult()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/sample.docx");

        // First conversion
        var stopwatch1 = Stopwatch.StartNew();
        await _service.ConvertAsync(content, "docx", "pdf");
        stopwatch1.Stop();

        // Second conversion (should use cache)
        var stopwatch2 = Stopwatch.StartNew();
        await _service.ConvertAsync(content, "docx", "pdf");
        stopwatch2.Stop();

        Assert.IsTrue(stopwatch2.ElapsedMilliseconds < stopwatch1.ElapsedMilliseconds / 10,
            "Cached conversion should be significantly faster");
    }

    [TestMethod]
    public async Task ConvertBatchAsync_10Documents_Throughput()
    {
        var requests = Enumerable.Range(0, 10).Select(_ => new ConversionRequest
        {
            SourceContent = CreateTestDocument(),
            SourceFormat = "html",
            TargetFormat = "pdf"
        }).ToList();

        var stopwatch = Stopwatch.StartNew();
        var results = await _service.ConvertBatchAsync(requests);
        stopwatch.Stop();

        var successCount = results.Count(r => r.Success);
        var throughput = successCount / stopwatch.Elapsed.TotalMinutes;

        Assert.IsTrue(throughput >= 10, $"Throughput: {throughput} documents/minute (expected >= 10)");
    }

    private byte[] CreateTestDocument()
    {
        var html = $"<html><body><h1>Test {Guid.NewGuid()}</h1></body></html>";
        return Encoding.UTF8.GetBytes(html);
    }
}
```

---

## Format Tests

### Test Coverage Areas

**File:** `FormatConversionTests.cs`

**Test Cases:**

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class FormatConversionTests
{
    private IDocumentConversionService _service;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        services.AddDocumentConversion();
        _service = services.BuildServiceProvider().GetRequiredService<IDocumentConversionService>();
    }

    [DataTestMethod]
    [DataRow("docx", "pdf")]
    [DataRow("xlsx", "pdf")]
    [DataRow("pptx", "pdf")]
    [DataRow("html", "pdf")]
    [DataRow("docx", "html")]
    [DataRow("png", "pdf")]
    [DataRow("jpg", "pdf")]
    public async Task ConvertAsync_SupportedFormats_ConvertsSuccessfully(string sourceFormat, string targetFormat)
    {
        var testFile = $"TestFiles/sample.{sourceFormat}";
        if (!File.Exists(testFile))
        {
            Assert.Inconclusive($"Test file {testFile} not found");
            return;
        }

        var content = await File.ReadAllBytesAsync(testFile);
        var result = await _service.ConvertAsync(content, sourceFormat, targetFormat);

        Assert.IsTrue(result.Success, $"Conversion {sourceFormat} → {targetFormat} failed: {result.ErrorMessage}");
        Assert.IsNotNull(result.ConvertedContent);
        Assert.AreEqual(targetFormat.ToLower(), result.TargetFormat?.ToLower());
    }

    [TestMethod]
    public async Task ConvertAsync_QualityLevels_ProducesDifferentFileSizes()
    {
        var content = await File.ReadAllBytesAsync("TestFiles/sample.png");
        var results = new Dictionary<ConversionQuality, long>();

        foreach (var quality in Enum.GetValues<ConversionQuality>())
        {
            var context = new ConversionContext { Quality = quality };
            var result = await _service.ConvertAsync(content, "png", "pdf", context);

            Assert.IsTrue(result.Success);
            results[quality] = result.ConvertedSize;
        }

        // Maximum quality should produce largest file
        Assert.IsTrue(results[ConversionQuality.Maximum] > results[ConversionQuality.Low]);
    }
}
```

---

## Test Data Requirements

### Required Test Files

**TestFiles Directory Structure:**
```
TestFiles/
├── small.docx (< 100KB)
├── large.docx (> 10MB)
├── sample.docx
├── sample.xlsx
├── sample.pptx
├── sample.html
├── sample.png
├── sample.jpg
├── sample.pdf
└── README.md
```

### Test File Generators

```csharp
public static class TestFileGenerator
{
    public static byte[] CreateTestHtml(string title, int paragraphs = 5)
    {
        var html = new StringBuilder();
        html.AppendLine("<html><body>");
        html.AppendLine($"<h1>{title}</h1>");

        for (int i = 0; i < paragraphs; i++)
        {
            html.AppendLine($"<p>Paragraph {i + 1}: Lorem ipsum dolor sit amet.</p>");
        }

        html.AppendLine("</body></html>");
        return Encoding.UTF8.GetBytes(html.ToString());
    }

    public static byte[] CreateTestPdf()
    {
        // Create simple PDF using PdfSharp
        var document = new PdfDocument();
        var page = document.AddPage();
        var gfx = XGraphics.FromPdfPage(page);
        gfx.DrawString("Test PDF", new XFont("Arial", 20), XBrushes.Black, new XRect(0, 0, page.Width, page.Height), XStringFormats.TopLeft);

        using var stream = new MemoryStream();
        document.Save(stream);
        return stream.ToArray();
    }
}
```

---

## Test Categories

- **Unit** - Fast, isolated, mocked dependencies
- **Integration** - Real providers, requires external tools (LibreOffice, etc.)
- **DevLocal** - Performance tests, run manually
- **LiveIntegration** - Cloud-based conversion services (if applicable)

---

## Coverage Goals

| Component | Target Coverage |
|-----------|----------------|
| DocumentConversionService | 90%+ |
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
- Format tests: Run weekly
```

### Required Tools for Integration Tests
- LibreOffice 7.0+
- ImageMagick 7.0+
- Playwright browsers
- .NET 10.0 SDK

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 6 Overview](../README.md)
