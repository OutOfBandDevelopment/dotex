# Message Composition Service - Testing Strategy

**Epic:** 12 - Message Composition Service
**Feature:** Message Composition Orchestration
**Last Updated:** 2026-01-22

---

## Testing Overview

**Goal:** 85%+ code coverage with comprehensive unit, integration, and performance tests.

**Test Categories:**
- **Unit Tests** - Isolated component testing with mocks
- **Integration Tests** - End-to-end scenarios with real Epic dependencies
- **Performance Tests** - Benchmark lazy evaluation and composition speed
- **Concurrency Tests** - Thread-safety verification

---

## Test Pyramid

```
                    ┌─────────────┐
                    │ Performance │  (8 tests)
                    │   Tests     │
                    └─────────────┘
                  ┌───────────────────┐
                  │  Integration Tests│  (20 tests)
                  │                   │
                  └───────────────────┘
            ┌─────────────────────────────┐
            │       Unit Tests            │  (45+ tests)
            │                             │
            └─────────────────────────────┘
```

---

## Unit Tests

### Test Coverage Areas

#### 1. MessageComposer Tests

**File:** `MessageComposerTests.cs`

**Test Cases:**

```csharp
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using OoBDev.System.Communications.Composition;
using OoBDev.System.Data.Enhancement;

namespace OoBDev.System.Communications.Composition.Tests;

[TestClass]
public class MessageComposerTests
{
    private Mock<ITemplateSelector> _mockSelector;
    private Mock<IMessageRenderer> _mockRenderer;
    private Mock<IFormatConverter> _mockConverter;
    private MessageComposer _composer;

    [TestInitialize]
    public void Setup()
    {
        _mockSelector = new Mock<ITemplateSelector>();
        _mockRenderer = new Mock<IMessageRenderer>();
        _mockConverter = new Mock<IFormatConverter>();

        _composer = new MessageComposer(
            _mockSelector.Object,
            _mockRenderer.Object,
            _mockConverter.Object);
    }

    [TestMethod]
    public async Task ComposeEmailAsync_ValidData_ReturnsComposedMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John", LastName = "Doe" }
        });

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("order.confirmation");

        _mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedContent
            {
                Content = "Order confirmed!",
                MediaType = "text/plain",
                Subject = "Order Confirmation"
            });

        // Act
        var result = await _composer.ComposeEmailAsync("order.confirmation", userId, data);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Order Confirmation", result.Subject);
        Assert.AreEqual("Order confirmed!", result.Content);
        Assert.AreEqual("text/plain", result.MediaType);
    }

    [TestMethod]
    public async Task ComposeEmailAsync_RequiredFormatDifferent_ConvertsFormat()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "test" });

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("template.id");

        _mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedContent
            {
                Content = "# Markdown Content",
                MediaType = "text/markdown",
                Subject = "Test"
            });

        _mockConverter
            .Setup(c => c.ConvertAsync(
                It.IsAny<string>(),
                "text/markdown",
                "text/html",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConvertedContent
            {
                Content = "<h1>Markdown Content</h1>",
                MediaType = "text/html"
            });

        // Act
        var result = await _composer.ComposeEmailAsync(
            "test.email", userId, data, requiredFormat: "text/html");

        // Assert
        Assert.AreEqual("<h1>Markdown Content</h1>", result.Content);
        Assert.AreEqual("text/html", result.MediaType);
        _mockConverter.Verify(
            c => c.ConvertAsync(
                It.IsAny<string>(),
                "text/markdown",
                "text/html",
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task ComposeEmailAsync_ConversionFails_UsesOriginalFormat()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "test" });

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("template.id");

        _mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedContent
            {
                Content = "Original Content",
                MediaType = "text/markdown",
                Subject = "Test"
            });

        _mockConverter
            .Setup(c => c.ConvertAsync(
                It.IsAny<string>(),
                "text/markdown",
                "text/html",
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new FormatConversionException(
                "Conversion failed", new Exception(), "test.email", "text/markdown", "text/html"));

        // Act
        var result = await _composer.ComposeEmailAsync(
            "test.email", userId, data, requiredFormat: "text/html");

        // Assert - Falls back to original format
        Assert.AreEqual("Original Content", result.Content);
        Assert.AreEqual("text/markdown", result.MediaType);
    }

    [TestMethod]
    [ExpectedException(typeof(TemplateNotFoundException))]
    public async Task ComposeEmailAsync_TemplateNotFound_ThrowsException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "test" });

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TemplateNotFoundException("Not found", "test.email", "test.email"));

        // Act
        await _composer.ComposeEmailAsync("test.email", userId, data);

        // Assert - Exception thrown
    }

    [TestMethod]
    public async Task ComposeSmsAsync_ValidData_ReturnsSmsMessage()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John" },
            Order = new { OrderNumber = "12345" }
        });

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("order.shipped.sms");

        _mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedContent
            {
                Content = "Hi John! Order 12345 shipped.",
                MediaType = "text/plain",
                Subject = ""
            });

        // Act
        var result = await _composer.ComposeSmsAsync("order.shipped", userId, data);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("text/plain", result.MediaType);
        Assert.IsTrue(result.Content.Length <= 160);  // SMS character limit
    }

    [TestMethod]
    public async Task ComposeMultiChannelAsync_ValidData_ReturnsAllVariants()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "test" });

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("test.template");

        _mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedContent
            {
                Content = "Test Content",
                MediaType = "text/plain",
                Subject = "Test Subject"
            });

        // Act
        var result = await _composer.ComposeMultiChannelAsync("test.message", userId, data);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsNotNull(result.EmailVariant);
        Assert.IsNotNull(result.SmsVariant);
        Assert.AreEqual(result.MessageId, result.EmailVariant.MessageId);
        Assert.AreEqual(result.MessageId, result.SmsVariant.MessageId);
    }

    [TestMethod]
    public async Task ComposeEmailWithAttachmentsAsync_ValidAttachments_IncludesAttachments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "test" });
        var attachmentData = DataContainerFactory.Create(new { Invoice = "data" });

        var attachments = new[]
        {
            new AttachmentRequest
            {
                TemplateName = "invoice.pdf",
                Data = attachmentData,
                Filename = "Invoice_12345.pdf",
                MediaType = "application/pdf"
            }
        };

        _mockSelector
            .Setup(s => s.SelectTemplateAsync(It.IsAny<string>(), It.IsAny<CultureInfo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("template.id");

        _mockRenderer
            .Setup(r => r.RenderAsync(It.IsAny<string>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedContent
            {
                Content = "Email Body",
                MediaType = "text/html",
                Subject = "Invoice"
            });

        // Act
        var result = await _composer.ComposeEmailWithAttachmentsAsync(
            "invoice.notification", userId, data, attachments);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Attachments.Count);
        Assert.AreEqual("Invoice_12345.pdf", result.Attachments[0].Filename);
    }
}
```

---

#### 2. TemplateSelector Tests

**File:** `TemplateSelectorTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class TemplateSelectorTests
{
    private Mock<ITemplateProvider> _mockProvider;
    private TemplateSelector _selector;

    [TestInitialize]
    public void Setup()
    {
        _mockProvider = new Mock<ITemplateProvider>();
        _selector = new TemplateSelector(_mockProvider.Object);
    }

    [TestMethod]
    public async Task SelectTemplateAsync_CultureSpecificExists_ReturnsCultureSpecific()
    {
        // Arrange
        var culture = new CultureInfo("en-US");

        _mockProvider
            .Setup(p => p.ExistsAsync("order.confirmation.en-US", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _selector.SelectTemplateAsync("order.confirmation", culture);

        // Assert
        Assert.AreEqual("order.confirmation.en-US", result);
    }

    [TestMethod]
    public async Task SelectTemplateAsync_CultureSpecificNotExists_FallsBackToLanguage()
    {
        // Arrange
        var culture = new CultureInfo("en-GB");

        _mockProvider
            .Setup(p => p.ExistsAsync("order.confirmation.en-GB", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockProvider
            .Setup(p => p.ExistsAsync("order.confirmation.en", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _selector.SelectTemplateAsync("order.confirmation", culture);

        // Assert
        Assert.AreEqual("order.confirmation.en", result);
    }

    [TestMethod]
    public async Task SelectTemplateAsync_LanguageNotExists_FallsBackToDefault()
    {
        // Arrange
        var culture = new CultureInfo("de-DE");

        _mockProvider
            .Setup(p => p.ExistsAsync("order.confirmation.de-DE", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockProvider
            .Setup(p => p.ExistsAsync("order.confirmation.de", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockProvider
            .Setup(p => p.ExistsAsync("order.confirmation", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _selector.SelectTemplateAsync("order.confirmation", culture);

        // Assert
        Assert.AreEqual("order.confirmation", result);
    }

    [TestMethod]
    [ExpectedException(typeof(TemplateNotFoundException))]
    public async Task SelectTemplateAsync_NoTemplateExists_ThrowsException()
    {
        // Arrange
        var culture = new CultureInfo("en-US");

        _mockProvider
            .Setup(p => p.ExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        await _selector.SelectTemplateAsync("nonexistent.template", culture);

        // Assert - Exception thrown
    }
}
```

---

#### 3. MessageRenderer Tests

**File:** `MessageRendererTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class MessageRendererTests
{
    private Mock<ITemplateEngine> _mockEngine;
    private Mock<ITemplateProvider> _mockProvider;
    private MessageRenderer _renderer;

    [TestInitialize]
    public void Setup()
    {
        _mockEngine = new Mock<ITemplateEngine>();
        _mockProvider = new Mock<ITemplateProvider>();
        _renderer = new MessageRenderer(_mockEngine.Object, _mockProvider.Object);
    }

    [TestMethod]
    public async Task RenderAsync_ValidTemplate_ReturnsRenderedContent()
    {
        // Arrange
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John" }
        });

        var template = new Template
        {
            Id = "test.template",
            Name = "Test Template",
            Content = "Hello {{Customer/FirstName}}!",
            MediaType = "text/plain"
        };

        _mockProvider
            .Setup(p => p.GetTemplateAsync("test.template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockEngine
            .Setup(e => e.RenderAsync(It.IsAny<Template>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedTemplate
            {
                Content = "Hello John!",
                Metadata = new Dictionary<string, string>
                {
                    ["Subject"] = "Greeting"
                }
            });

        // Act
        var result = await _renderer.RenderAsync("test.template", data);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual("Hello John!", result.Content);
        Assert.AreEqual("text/plain", result.MediaType);
        Assert.AreEqual("Greeting", result.Subject);
    }

    [TestMethod]
    public async Task RenderAsync_LazyDataEvaluation_PassesDataContainer()
    {
        // Arrange
        var mockProvider = new Mock<IDataProvider>();
        var data = DataContainerFactory.Create();
        data.RegisterProvider("Customer", mockProvider.Object);

        var template = new Template
        {
            Id = "test.template",
            Content = "Hello {{Customer/FirstName}}!",
            MediaType = "text/plain"
        };

        _mockProvider
            .Setup(p => p.GetTemplateAsync("test.template", It.IsAny<CancellationToken>()))
            .ReturnsAsync(template);

        _mockEngine
            .Setup(e => e.RenderAsync(It.IsAny<Template>(), It.IsAny<IDataContainer>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new RenderedTemplate { Content = "Rendered" });

        // Act
        var result = await _renderer.RenderAsync("test.template", data);

        // Assert
        _mockEngine.Verify(
            e => e.RenderAsync(
                It.IsAny<Template>(),
                It.Is<IDataContainer>(c => c == data),  // IDataContainer passed directly
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

---

#### 4. FormatConverter Tests

**File:** `FormatConverterTests.cs`

**Test Cases:**

```csharp
[TestClass]
public class FormatConverterTests
{
    private Mock<IDocumentConversionService> _mockConversionService;
    private FormatConverter _converter;

    [TestInitialize]
    public void Setup()
    {
        _mockConversionService = new Mock<IDocumentConversionService>();
        _converter = new FormatConverter(_mockConversionService.Object);
    }

    [TestMethod]
    public async Task ConvertAsync_SameFormat_ReturnsOriginalContent()
    {
        // Arrange
        var content = "Test Content";
        var mediaType = "text/plain";

        // Act
        var result = await _converter.ConvertAsync(content, mediaType, mediaType);

        // Assert
        Assert.AreEqual(content, result.Content);
        Assert.AreEqual(mediaType, result.MediaType);
        _mockConversionService.Verify(
            s => s.ConvertAsync(It.IsAny<Document>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);  // No conversion called
    }

    [TestMethod]
    public async Task ConvertAsync_MarkdownToHtml_ConvertsSuccessfully()
    {
        // Arrange
        var content = "# Heading";
        var sourceMediaType = "text/markdown";
        var targetMediaType = "text/html";

        _mockConversionService
            .Setup(s => s.ConvertAsync(
                It.IsAny<Document>(),
                targetMediaType,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document
            {
                Content = Encoding.UTF8.GetBytes("<h1>Heading</h1>"),
                MediaType = targetMediaType
            });

        // Act
        var result = await _converter.ConvertAsync(content, sourceMediaType, targetMediaType);

        // Assert
        Assert.AreEqual("<h1>Heading</h1>", result.Content);
        Assert.AreEqual(targetMediaType, result.MediaType);
    }

    [TestMethod]
    public async Task ConvertChainAsync_MarkdownToPdf_UsesConversionChain()
    {
        // Arrange
        var content = "# Heading";
        var chain = new[] { "text/markdown", "text/html", "application/pdf" };

        // Setup: Markdown → HTML
        _mockConversionService
            .Setup(s => s.ConvertAsync(
                It.Is<Document>(d => d.MediaType == "text/markdown"),
                "text/html",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document
            {
                Content = Encoding.UTF8.GetBytes("<h1>Heading</h1>"),
                MediaType = "text/html"
            });

        // Setup: HTML → PDF
        _mockConversionService
            .Setup(s => s.ConvertAsync(
                It.Is<Document>(d => d.MediaType == "text/html"),
                "application/pdf",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Document
            {
                Content = new byte[] { 0x25, 0x50, 0x44, 0x46 },  // PDF header
                MediaType = "application/pdf"
            });

        // Act
        var result = await _converter.ConvertChainAsync(content, chain);

        // Assert
        Assert.AreEqual("application/pdf", result.MediaType);
        _mockConversionService.Verify(
            s => s.ConvertAsync(It.IsAny<Document>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));  // Two conversions in chain
    }

    [TestMethod]
    public void SupportsConversion_MarkdownToHtml_ReturnsTrue()
    {
        // Act
        var result = _converter.SupportsConversion("text/markdown", "text/html");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void SupportsConversion_MarkdownToPdf_ReturnsTrueWithChain()
    {
        // Act (chained: Markdown → HTML → PDF)
        var result = _converter.SupportsConversion("text/markdown", "application/pdf");

        // Assert
        Assert.IsTrue(result);
    }

    [TestMethod]
    public void SupportsConversion_UnsupportedConversion_ReturnsFalse()
    {
        // Act
        var result = _converter.SupportsConversion("text/plain", "video/mp4");

        // Assert
        Assert.IsFalse(result);
    }
}
```

---

## Integration Tests

### Test Scenarios

#### 1. End-to-End Composition Tests

**File:** `MessageCompositionIntegrationTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.Integration)]
public class MessageCompositionIntegrationTests
{
    private IMessageComposer _composer;
    private ITemplateEngine _templateEngine;
    private ITemplateProvider _templateProvider;
    private IDocumentConversionService _conversionService;

    [TestInitialize]
    public void Setup()
    {
        // Real implementations (not mocks)
        _templateEngine = new HandlebarsTemplateEngine();
        _templateProvider = new InMemoryTemplateProvider();
        _conversionService = new DocumentConversionService();

        _composer = MessageComposerFactory.Create(
            _templateEngine,
            _templateProvider,
            _conversionService);

        // Seed templates
        SeedTemplates();
    }

    private void SeedTemplates()
    {
        _templateProvider.AddTemplate(new Template
        {
            Id = "order.confirmation",
            Name = "Order Confirmation",
            Content = "Hello {{Customer/FirstName}}, your order {{Order/OrderNumber}} is confirmed!",
            MediaType = "text/plain"
        });

        _templateProvider.AddTemplate(new Template
        {
            Id = "order.confirmation.en-US",
            Name = "Order Confirmation (US)",
            Content = "Hi {{Customer/FirstName}}, order #{{Order/OrderNumber}} confirmed!",
            MediaType = "text/plain"
        });

        _templateProvider.AddTemplate(new Template
        {
            Id = "invoice.email",
            Name = "Invoice Email",
            Content = @"
# Invoice {{Invoice/InvoiceNumber}}

**Customer:** {{Invoice/Customer/Name}}
**Date:** {{Invoice/Date}}

## Line Items
{{#each Invoice/LineItems}}
- {{Description}}: {{Quantity}} x ${{Price}} = ${{Total}}
{{/each}}

**Total:** ${{Invoice/Total}}
",
            MediaType = "text/markdown"
        });
    }

    [TestMethod]
    public async Task ComposeEmailAsync_OrderConfirmation_ComposesSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John", LastName = "Doe" },
            Order = new { OrderNumber = "12345", Total = 99.99m }
        });

        // Act
        var email = await _composer.ComposeEmailAsync(
            messageType: "order.confirmation",
            userId: userId,
            data: data);

        // Assert
        Assert.IsNotNull(email);
        Assert.IsTrue(email.Content.Contains("John"));
        Assert.IsTrue(email.Content.Contains("12345"));
        Assert.AreEqual("text/plain", email.MediaType);
    }

    [TestMethod]
    public async Task ComposeEmailAsync_CultureSpecificTemplate_UsesCultureVariant()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            User = new { Culture = new CultureInfo("en-US") },
            Customer = new { FirstName = "John" },
            Order = new { OrderNumber = "12345" }
        });

        // Act
        var email = await _composer.ComposeEmailAsync(
            messageType: "order.confirmation",
            userId: userId,
            data: data);

        // Assert
        Assert.IsTrue(email.Content.Contains("Hi John"));  // US variant
        Assert.IsFalse(email.Content.Contains("Hello John"));  // Default variant
    }

    [TestMethod]
    public async Task ComposeEmailAsync_MarkdownToHtml_ConvertsSuccessfully()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Invoice = new
            {
                InvoiceNumber = "INV-001",
                Customer = new { Name = "John Doe" },
                Date = DateTime.UtcNow,
                LineItems = new[]
                {
                    new { Description = "Widget", Quantity = 2, Price = 19.99m, Total = 39.98m }
                },
                Total = 39.98m
            }
        });

        // Act (Markdown → HTML conversion)
        var email = await _composer.ComposeEmailAsync(
            messageType: "invoice.email",
            userId: userId,
            data: data,
            requiredFormat: "text/html");

        // Assert
        Assert.AreEqual("text/html", email.MediaType);
        Assert.IsTrue(email.Content.Contains("<h1>"));  // HTML heading
        Assert.IsTrue(email.Content.Contains("<li>"));  // HTML list
        Assert.IsNotNull(email.PlainTextContent);  // Plain text variant generated
    }

    [TestMethod]
    public async Task ComposeEmailAsync_LazyDataEvaluation_ExecutesOnlyUsedProviders()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var customerProviderCalled = false;
        var orderProviderCalled = false;
        var inventoryProviderCalled = false;

        var data = DataContainerFactory.Create();

        data.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            customerProviderCalled = true;
            return new { FirstName = "John", LastName = "Doe" };
        }));

        data.RegisterProvider("Order", new DelegateDataProvider(async () =>
        {
            orderProviderCalled = true;
            return new { OrderNumber = "12345" };
        }));

        data.RegisterProvider("Inventory", new DelegateDataProvider(async () =>
        {
            inventoryProviderCalled = true;
            return new { Stock = 100 };
        }));

        // Act (Template uses ONLY Customer and Order)
        var email = await _composer.ComposeEmailAsync(
            messageType: "order.confirmation",
            userId: userId,
            data: data);

        // Assert
        Assert.IsTrue(customerProviderCalled);  // Used by template
        Assert.IsTrue(orderProviderCalled);     // Used by template
        Assert.IsFalse(inventoryProviderCalled); // NOT used by template (lazy evaluation)
    }

    [TestMethod]
    public async Task ComposeMultiChannelAsync_OrderShipped_GeneratesAllVariants()
    {
        // Arrange
        _templateProvider.AddTemplate(new Template
        {
            Id = "order.shipped.email",
            Content = "Your order {{Order/OrderNumber}} has shipped!",
            MediaType = "text/html"
        });

        _templateProvider.AddTemplate(new Template
        {
            Id = "order.shipped.sms",
            Content = "Order {{Order/OrderNumber}} shipped!",
            MediaType = "text/plain"
        });

        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Order = new { OrderNumber = "12345", TrackingNumber = "1Z999AA10123456784" }
        });

        // Act
        var multiChannel = await _composer.ComposeMultiChannelAsync(
            messageType: "order.shipped",
            userId: userId,
            data: data);

        // Assert
        Assert.IsNotNull(multiChannel);
        Assert.IsNotNull(multiChannel.EmailVariant);
        Assert.IsNotNull(multiChannel.SmsVariant);
        Assert.AreEqual(multiChannel.MessageId, multiChannel.EmailVariant.MessageId);
        Assert.IsTrue(multiChannel.EmailVariant.Content.Contains("12345"));
        Assert.IsTrue(multiChannel.SmsVariant.Content.Contains("12345"));
    }

    [TestMethod]
    public async Task ComposeEmailWithAttachmentsAsync_PdfAttachment_IncludesPdf()
    {
        // Arrange
        _templateProvider.AddTemplate(new Template
        {
            Id = "invoice.pdf",
            Content = "Invoice: {{Invoice/InvoiceNumber}}",
            MediaType = "text/plain"
        });

        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "email body" });
        var attachmentData = DataContainerFactory.Create(new
        {
            Invoice = new { InvoiceNumber = "INV-001" }
        });

        var attachments = new[]
        {
            new AttachmentRequest
            {
                TemplateName = "invoice.pdf",
                Data = attachmentData,
                Filename = "Invoice_INV-001.pdf",
                MediaType = "application/pdf"
            }
        };

        // Act
        var email = await _composer.ComposeEmailWithAttachmentsAsync(
            messageType: "invoice.email",
            userId: userId,
            data: data,
            attachments: attachments);

        // Assert
        Assert.AreEqual(1, email.Attachments.Count);
        Assert.AreEqual("Invoice_INV-001.pdf", email.Attachments[0].Filename);
        Assert.AreEqual("application/pdf", email.Attachments[0].MediaType);
        Assert.IsTrue(email.Attachments[0].Size > 0);
    }
}
```

---

## Performance Tests

### Benchmarks

**File:** `MessageCompositionPerformanceTests.cs`

```csharp
[TestClass]
[TestCategory(TestCategories.DevLocal)]
public class MessageCompositionPerformanceTests
{
    private IMessageComposer _composer;

    [TestInitialize]
    public void Setup()
    {
        // Setup with real dependencies
        _composer = CreateComposerWithDependencies();
    }

    [TestMethod]
    public async Task ComposeEmailAsync_SimpleMessage_CompletesUnder100ms()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John" },
            Order = new { OrderNumber = "12345" }
        });

        // Act
        var stopwatch = Stopwatch.StartNew();
        var email = await _composer.ComposeEmailAsync("order.confirmation", userId, data);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 100,
            $"Composition took {stopwatch.ElapsedMilliseconds}ms (expected < 100ms)");
        Console.WriteLine($"Simple message composition: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ComposeEmailAsync_HtmlConversion_CompletesUnder200ms()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John" },
            Order = new { OrderNumber = "12345" }
        });

        // Act (Markdown → HTML conversion)
        var stopwatch = Stopwatch.StartNew();
        var email = await _composer.ComposeEmailAsync(
            "order.confirmation", userId, data, requiredFormat: "text/html");
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 200,
            $"Composition with conversion took {stopwatch.ElapsedMilliseconds}ms (expected < 200ms)");
        Console.WriteLine($"HTML conversion composition: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ComposeEmailWithAttachmentsAsync_PdfAttachment_CompletesUnder500ms()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new { Data = "body" });
        var attachmentData = DataContainerFactory.Create(new { Invoice = "data" });

        var attachments = new[]
        {
            new AttachmentRequest
            {
                TemplateName = "invoice.pdf",
                Data = attachmentData,
                Filename = "Invoice.pdf",
                MediaType = "application/pdf"
            }
        };

        // Act
        var stopwatch = Stopwatch.StartNew();
        var email = await _composer.ComposeEmailWithAttachmentsAsync(
            "invoice.email", userId, data, attachments);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 500,
            $"Composition with PDF attachment took {stopwatch.ElapsedMilliseconds}ms (expected < 500ms)");
        Console.WriteLine($"PDF attachment composition: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task ComposeMultiChannelAsync_AllVariants_CompletesUnder300ms()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var data = DataContainerFactory.Create(new
        {
            Customer = new { FirstName = "John" },
            Order = new { OrderNumber = "12345" }
        });

        // Act (Email + SMS + Push variants in parallel)
        var stopwatch = Stopwatch.StartNew();
        var multiChannel = await _composer.ComposeMultiChannelAsync("order.shipped", userId, data);
        stopwatch.Stop();

        // Assert
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 300,
            $"Multi-channel composition took {stopwatch.ElapsedMilliseconds}ms (expected < 300ms)");
        Console.WriteLine($"Multi-channel composition: {stopwatch.ElapsedMilliseconds}ms");
    }

    [TestMethod]
    public async Task LazyEvaluation_QueryReduction_Measures50to70PercentImprovement()
    {
        // Arrange
        var callCounts = new Dictionary<string, int>
        {
            ["Customer"] = 0,
            ["Order"] = 0,
            ["Inventory"] = 0,
            ["Shipping"] = 0
        };

        var data = DataContainerFactory.Create();

        data.RegisterProvider("Customer", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCounts["Customer"]);
            await Task.Delay(10);  // Simulate query
            return new { FirstName = "John" };
        }));

        data.RegisterProvider("Order", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCounts["Order"]);
            await Task.Delay(10);
            return new { OrderNumber = "12345" };
        }));

        data.RegisterProvider("Inventory", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCounts["Inventory"]);
            await Task.Delay(10);
            return new { Stock = 100 };
        }));

        data.RegisterProvider("Shipping", new DelegateDataProvider(async () =>
        {
            Interlocked.Increment(ref callCounts["Shipping"]);
            await Task.Delay(10);
            return new { Carrier = "UPS" };
        }));

        // Act (Template uses ONLY Customer and Order)
        var email = await _composer.ComposeEmailAsync("order.confirmation", Guid.NewGuid(), data);

        // Assert
        var totalProviders = 4;
        var executedProviders = callCounts.Values.Count(c => c > 0);
        var queryReduction = (totalProviders - executedProviders) / (double)totalProviders * 100;

        Assert.IsTrue(queryReduction >= 50,
            $"Query reduction: {queryReduction}% (expected >= 50%)");
        Console.WriteLine($"Lazy evaluation: {executedProviders}/{totalProviders} providers executed ({queryReduction}% reduction)");
    }

    [TestMethod]
    public async Task ConcurrentComposition_100Messages_CompletesUnder10Seconds()
    {
        // Arrange
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(async () =>
        {
            var userId = Guid.NewGuid();
            var data = DataContainerFactory.Create(new
            {
                Customer = new { FirstName = $"User{i}" },
                Order = new { OrderNumber = $"{i}" }
            });

            return await _composer.ComposeEmailAsync("order.confirmation", userId, data);
        }));

        // Act
        var stopwatch = Stopwatch.StartNew();
        var emails = await Task.WhenAll(tasks);
        stopwatch.Stop();

        // Assert
        Assert.AreEqual(100, emails.Length);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 10000,
            $"100 concurrent compositions took {stopwatch.ElapsedMilliseconds}ms (expected < 10000ms)");
        Console.WriteLine($"100 concurrent compositions: {stopwatch.ElapsedMilliseconds}ms ({100000.0 / stopwatch.ElapsedMilliseconds:F1} messages/sec)");
    }
}
```

---

## Coverage Goals

### Minimum Coverage Requirements

| Component | Target Coverage | Critical Paths |
|-----------|----------------|----------------|
| MessageComposer | 90% | ComposeEmailAsync, ComposeMultiChannelAsync, Error handling |
| TemplateSelector | 85% | SelectTemplateAsync, Culture fallback logic |
| MessageRenderer | 85% | RenderAsync, IDataContainer integration |
| FormatConverter | 80% | ConvertAsync, ConvertChainAsync, SupportsConversion |
| Data Models | 70% | Property getters/setters |

---

## Test Data Builders

**File:** `MessageCompositionTestDataBuilders.cs`

```csharp
public static class MessageCompositionTestDataBuilders
{
    public static IDataContainer BuildOrderConfirmationData()
    {
        return DataContainerFactory.Create(new
        {
            Customer = new
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com"
            },
            Order = new
            {
                OrderNumber = "12345",
                OrderDate = DateTime.UtcNow,
                Total = 99.99m,
                LineItems = new[]
                {
                    new { ProductName = "Widget", Quantity = 2, Price = 19.99m, Total = 39.98m },
                    new { ProductName = "Gadget", Quantity = 1, Price = 29.99m, Total = 29.99m }
                }
            }
        });
    }

    public static IDataContainer BuildInvoiceData()
    {
        return DataContainerFactory.Create(new
        {
            Invoice = new
            {
                InvoiceNumber = "INV-2024-001",
                Date = DateTime.UtcNow,
                Customer = new
                {
                    Name = "John Doe",
                    Address = "123 Main St, Springfield, IL 62701"
                },
                LineItems = new[]
                {
                    new { Description = "Professional Services", Quantity = 10, Price = 150.00m, Total = 1500.00m },
                    new { Description = "Software License", Quantity = 1, Price = 500.00m, Total = 500.00m }
                },
                Subtotal = 2000.00m,
                Tax = 160.00m,
                Total = 2160.00m
            }
        });
    }

    public static Template BuildEmailTemplate(string id, string content)
    {
        return new Template
        {
            Id = id,
            Name = id.Replace(".", " ").ToTitleCase(),
            Content = content,
            MediaType = "text/plain",
            Version = new Version(1, 0)
        };
    }

    public static Template BuildMarkdownTemplate(string id, string content)
    {
        return new Template
        {
            Id = id,
            Name = id.Replace(".", " ").ToTitleCase(),
            Content = content,
            MediaType = "text/markdown",
            Version = new Version(1, 0)
        };
    }
}
```

---

## Continuous Integration

### CI Pipeline Tests

**Run on every commit:**
```bash
# Unit tests (fast)
dotnet test --filter "TestCategory=Unit" --logger "console;verbosity=detailed"

# Integration tests (slower)
dotnet test --filter "TestCategory=Integration" --logger "console;verbosity=detailed"
```

**Run nightly:**
```bash
# Performance benchmarks
dotnet test --filter "TestCategory=DevLocal" --logger "console;verbosity=detailed"

# Full test suite with coverage
dotnet test --collect:"XPlat Code Coverage" --logger "console;verbosity=detailed"
```

---

## Test Maintenance

### Adding New Tests

**When adding new features:**
1. Add unit tests for new methods (90%+ coverage)
2. Add integration tests for end-to-end scenarios
3. Add performance tests if performance-critical
4. Update coverage goals if needed

**Test naming convention:**
```
[MethodName]_[Scenario]_[ExpectedBehavior]

Examples:
- ComposeEmailAsync_ValidData_ReturnsComposedMessage
- SelectTemplateAsync_CultureSpecificExists_ReturnsCultureSpecific
- ConvertAsync_MarkdownToHtml_ConvertsSuccessfully
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Epic 12 Overview](../CONSOLIDATED_DESIGN.md#epic-12-message-composition-service)
- [Epic 11: Data Enhancement Testing](../11-DataEnhancement/CoreContainer/testing-strategy.md)
- [Epic 10: Templates Testing](../10-TextTemplating/HandlebarsProvider/testing-strategy.md)
