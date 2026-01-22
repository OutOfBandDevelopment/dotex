# Message Composition Service - API Design

**Epic:** 12 - Message Composition Service
**Feature:** Message Composition Orchestration
**Last Updated:** 2026-01-22

---

## API Overview

The Message Composition API provides interfaces for composing pre-formatted messages by orchestrating template rendering, data enhancement, and format conversion. The API integrates Epic 10 (Templates), Epic 11 (Data Enhancement), and Epic 6 (Document Conversion).

---

## Core Interfaces

### IMessageComposer

**Purpose:** Main entry point for message composition operations.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Orchestrates template rendering, data enhancement, and format conversion
/// to produce pre-formatted messages ready for delivery.
/// </summary>
public interface IMessageComposer
{
    /// <summary>
    /// Composes email message with automatic format conversion.
    /// </summary>
    /// <param name="messageType">Message type identifier (e.g., "order.confirmation")</param>
    /// <param name="userId">User ID for culture/personalization lookup</param>
    /// <param name="data">Data container with lazy providers (Epic 11)</param>
    /// <param name="requiredFormat">
    /// Target format (e.g., "text/html", "text/plain").
    /// If null, uses template's native format.
    /// </param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Composed email message ready for delivery</returns>
    /// <exception cref="TemplateNotFoundException">Template not found</exception>
    /// <exception cref="TemplateRenderingException">Template rendering failed</exception>
    /// <exception cref="FormatConversionException">Format conversion failed</exception>
    Task<ComposedMessage> ComposeEmailAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        string? requiredFormat = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes SMS message with plain text content.
    /// </summary>
    /// <param name="messageType">Message type identifier (e.g., "order.shipped")</param>
    /// <param name="userId">User ID for personalization</param>
    /// <param name="data">Data container with lazy providers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Composed SMS message (plain text only)</returns>
    Task<ComposedMessage> ComposeSmsAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes multi-channel message with format-specific variants.
    /// Generates email, SMS, and push notification variants in parallel.
    /// </summary>
    /// <param name="messageType">Message type identifier</param>
    /// <param name="userId">User ID for personalization</param>
    /// <param name="data">Data container with lazy providers (shared across channels)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Multi-channel message with all variants</returns>
    Task<MultiChannelMessage> ComposeMultiChannelAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes email message with document attachments.
    /// </summary>
    /// <param name="messageType">Message type identifier</param>
    /// <param name="userId">User ID for personalization</param>
    /// <param name="data">Data container for message body</param>
    /// <param name="attachments">Attachment requests (templates + data)</param>
    /// <param name="requiredFormat">Target format for message body</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Composed email with attachments</returns>
    Task<ComposedMessage> ComposeEmailWithAttachmentsAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        IEnumerable<AttachmentRequest> attachments,
        string? requiredFormat = null,
        CancellationToken cancellationToken = default);
}
```

---

### ITemplateSelector

**Purpose:** Selects appropriate template based on message type and culture.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Selects appropriate template based on message type and user culture.
/// Implements fallback logic: culture-specific → language → default.
/// </summary>
public interface ITemplateSelector
{
    /// <summary>
    /// Selects template ID for message type and culture.
    /// </summary>
    /// <param name="messageType">Message type (e.g., "order.confirmation")</param>
    /// <param name="culture">User culture for localization</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Template ID</returns>
    /// <exception cref="TemplateNotFoundException">No template found (including fallbacks)</exception>
    /// <remarks>
    /// Selection logic:
    /// 1. Try culture-specific: "order.confirmation.en-US"
    /// 2. Try language-only: "order.confirmation.en"
    /// 3. Try default: "order.confirmation"
    /// 4. Throw if none found
    /// </remarks>
    Task<string> SelectTemplateAsync(
        string messageType,
        CultureInfo culture,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if template exists for message type and culture.
    /// </summary>
    Task<bool> ExistsAsync(
        string messageType,
        CultureInfo culture,
        CancellationToken cancellationToken = default);
}
```

---

### IMessageRenderer

**Purpose:** Renders template with data container (lazy evaluation).

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Renders template using data container with lazy evaluation.
/// Adapts Epic 10 template engine for message composition.
/// </summary>
public interface IMessageRenderer
{
    /// <summary>
    /// Renders template using data container.
    /// Data providers execute ONLY when template accesses their paths (lazy).
    /// </summary>
    /// <param name="templateId">Template ID</param>
    /// <param name="data">Data container with registered providers</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rendered content with metadata</returns>
    /// <exception cref="TemplateRenderingException">Rendering failed</exception>
    Task<RenderedContent> RenderAsync(
        string templateId,
        IDataContainer data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders template with culture override.
    /// </summary>
    Task<RenderedContent> RenderAsync(
        string templateId,
        IDataContainer data,
        CultureInfo culture,
        CancellationToken cancellationToken = default);
}
```

---

### IFormatConverter

**Purpose:** Converts rendered content between formats.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Converts rendered content from source format to target format.
/// Supports conversion chaining (e.g., Markdown → HTML → PDF).
/// Adapts Epic 6 document conversion service.
/// </summary>
public interface IFormatConverter
{
    /// <summary>
    /// Converts content from source format to target format.
    /// </summary>
    /// <param name="content">Source content</param>
    /// <param name="sourceMediaType">Source media type (e.g., "text/markdown")</param>
    /// <param name="targetMediaType">Target media type (e.g., "text/html")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Converted content</returns>
    /// <exception cref="FormatConversionException">Conversion failed or not supported</exception>
    Task<ConvertedContent> ConvertAsync(
        string content,
        string sourceMediaType,
        string targetMediaType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Converts content using chained conversions.
    /// Example: Markdown → HTML → PDF
    /// </summary>
    /// <param name="content">Source content</param>
    /// <param name="conversionChain">Array of media types (source → intermediate → target)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Converted content</returns>
    Task<ConvertedContent> ConvertChainAsync(
        string content,
        string[] conversionChain,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if conversion is supported.
    /// </summary>
    /// <param name="sourceMediaType">Source media type</param>
    /// <param name="targetMediaType">Target media type</param>
    /// <returns>True if conversion supported (direct or chained)</returns>
    bool SupportsConversion(string sourceMediaType, string targetMediaType);

    /// <summary>
    /// Gets supported conversions from source media type.
    /// </summary>
    IEnumerable<string> GetSupportedTargetFormats(string sourceMediaType);
}
```

---

## Data Models

### ComposedMessage

**Purpose:** Pre-formatted message ready for delivery.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Composed message ready for delivery via Epic 2 channels.
/// </summary>
public class ComposedMessage
{
    /// <summary>
    /// Unique message identifier.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Message type (e.g., "order.confirmation").
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// User ID this message is for.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Message subject (for emails).
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Message content (HTML, plain text, or other format).
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content media type (e.g., "text/html", "text/plain").
    /// </summary>
    public string MediaType { get; set; } = "text/plain";

    /// <summary>
    /// Plain text variant of content (if Content is HTML).
    /// Auto-generated if not provided.
    /// </summary>
    public string? PlainTextContent { get; set; }

    /// <summary>
    /// Message attachments.
    /// </summary>
    public IReadOnlyList<Attachment> Attachments { get; set; } = Array.Empty<Attachment>();

    /// <summary>
    /// Message metadata (template ID, version, etc.).
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

    /// <summary>
    /// Timestamp when message was composed.
    /// </summary>
    public DateTime ComposedAt { get; set; }

    /// <summary>
    /// Culture used for composition.
    /// </summary>
    public CultureInfo? Culture { get; set; }
}
```

---

### MultiChannelMessage

**Purpose:** Message with format-specific variants for multiple channels.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Multi-channel message with variants for email, SMS, and push notifications.
/// All variants composed in parallel using shared IDataContainer (lazy evaluation).
/// </summary>
public class MultiChannelMessage
{
    /// <summary>
    /// Unique message identifier (shared across variants).
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Message type.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// User ID.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Email variant (HTML + plain text).
    /// </summary>
    public ComposedMessage EmailVariant { get; set; } = null!;

    /// <summary>
    /// SMS variant (plain text, respects character limits).
    /// </summary>
    public ComposedMessage SmsVariant { get; set; } = null!;

    /// <summary>
    /// Push notification variant (optional).
    /// </summary>
    public ComposedMessage? PushVariant { get; set; }

    /// <summary>
    /// Timestamp when message was composed.
    /// </summary>
    public DateTime ComposedAt { get; set; }

    /// <summary>
    /// Culture used for composition.
    /// </summary>
    public CultureInfo? Culture { get; set; }
}
```

---

### AttachmentRequest

**Purpose:** Request for generating message attachment from template.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Request for generating attachment from template.
/// </summary>
public class AttachmentRequest
{
    /// <summary>
    /// Template name for attachment (e.g., "invoice.pdf").
    /// </summary>
    public string TemplateName { get; set; } = string.Empty;

    /// <summary>
    /// Data container for attachment rendering.
    /// Can be same as message data or separate.
    /// </summary>
    public IDataContainer Data { get; set; } = null!;

    /// <summary>
    /// Attachment filename (e.g., "Invoice_12345.pdf").
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Target media type (e.g., "application/pdf").
    /// If null, uses template's native format.
    /// </summary>
    public string? MediaType { get; set; }

    /// <summary>
    /// Attachment metadata.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
```

---

### Attachment

**Purpose:** Generated attachment ready for delivery.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Generated attachment for message.
/// </summary>
public class Attachment
{
    /// <summary>
    /// Attachment filename.
    /// </summary>
    public string Filename { get; set; } = string.Empty;

    /// <summary>
    /// Attachment content.
    /// </summary>
    public byte[] Content { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// Content media type.
    /// </summary>
    public string MediaType { get; set; } = string.Empty;

    /// <summary>
    /// Content size in bytes.
    /// </summary>
    public long Size => Content.Length;

    /// <summary>
    /// Attachment metadata.
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
```

---

### RenderedContent

**Purpose:** Result of template rendering.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Result of template rendering.
/// </summary>
public class RenderedContent
{
    /// <summary>
    /// Rendered content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Content media type (template's native format).
    /// </summary>
    public string MediaType { get; set; } = "text/plain";

    /// <summary>
    /// Message subject (extracted from template or data).
    /// </summary>
    public string Subject { get; set; } = string.Empty;

    /// <summary>
    /// Rendering metadata (template ID, version, etc.).
    /// </summary>
    public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
}
```

---

### ConvertedContent

**Purpose:** Result of format conversion.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Result of format conversion.
/// </summary>
public class ConvertedContent
{
    /// <summary>
    /// Converted content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Target media type.
    /// </summary>
    public string MediaType { get; set; } = string.Empty;
}
```

---

## Factory & Builder

### MessageComposerFactory

**Purpose:** Factory for creating IMessageComposer instances.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Factory for creating message composers.
/// </summary>
public static class MessageComposerFactory
{
    /// <summary>
    /// Creates message composer with all dependencies.
    /// </summary>
    public static IMessageComposer Create(
        ITemplateEngine templateEngine,
        ITemplateProvider templateProvider,
        IDocumentConversionService conversionService)
    {
        var templateSelector = new TemplateSelector(templateProvider);
        var renderer = new MessageRenderer(templateEngine, templateProvider);
        var converter = new FormatConverter(conversionService);

        return new MessageComposer(templateSelector, renderer, converter);
    }
}
```

---

### MessageComposerBuilder

**Purpose:** Fluent builder for configuring message composer.

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Fluent builder for message composer.
/// </summary>
public class MessageComposerBuilder
{
    private ITemplateEngine? _templateEngine;
    private ITemplateProvider? _templateProvider;
    private IDocumentConversionService? _conversionService;
    private ILogger<MessageComposer>? _logger;

    public MessageComposerBuilder WithTemplateEngine(ITemplateEngine templateEngine)
    {
        _templateEngine = templateEngine;
        return this;
    }

    public MessageComposerBuilder WithTemplateProvider(ITemplateProvider templateProvider)
    {
        _templateProvider = templateProvider;
        return this;
    }

    public MessageComposerBuilder WithConversionService(IDocumentConversionService conversionService)
    {
        _conversionService = conversionService;
        return this;
    }

    public MessageComposerBuilder WithLogger(ILogger<MessageComposer> logger)
    {
        _logger = logger;
        return this;
    }

    public IMessageComposer Build()
    {
        if (_templateEngine == null)
            throw new InvalidOperationException("Template engine not configured");
        if (_templateProvider == null)
            throw new InvalidOperationException("Template provider not configured");
        if (_conversionService == null)
            throw new InvalidOperationException("Conversion service not configured");

        var templateSelector = new TemplateSelector(_templateProvider);
        var renderer = new MessageRenderer(_templateEngine, _templateProvider);
        var converter = new FormatConverter(_conversionService);

        return new MessageComposer(templateSelector, renderer, converter, _logger);
    }
}
```

---

## Usage Examples

### Example 1: Compose Email (Simple)

```csharp
using OoBDev.System.Communications.Composition;
using OoBDev.System.Data.Enhancement;

// Create data container
var data = DataContainerFactory.Create(new
{
    Customer = new
    {
        FirstName = "John",
        LastName = "Doe",
        Email = "john@example.com"
    },
    Order = new
    {
        OrderNumber = "12345",
        Total = 99.99m,
        OrderDate = DateTime.UtcNow
    }
});

// Compose email (template renders to Markdown, auto-converts to HTML)
var email = await _composer.ComposeEmailAsync(
    messageType: "order.confirmation",
    userId: customerId,
    data: data,
    requiredFormat: "text/html"
);

Console.WriteLine($"Subject: {email.Subject}");
Console.WriteLine($"Format: {email.MediaType}");
Console.WriteLine($"HTML Content: {email.Content}");
Console.WriteLine($"Plain Text: {email.PlainTextContent}");
```

---

### Example 2: Compose Email (Lazy Evaluation)

```csharp
// Register providers (NOT executed)
var data = DataContainerFactory.Create();
data.RegisterProvider("Customer", new CustomerProvider(_customerRepo));
data.RegisterProvider("Order", new OrderProvider(_orderRepo));
data.RegisterProvider("Inventory", new InventoryProvider(_inventoryService));

// Template uses ONLY Customer and Order
// Template: "Hello {{Customer/FirstName}}, order {{Order/OrderNumber}} confirmed."

// Compose email
var email = await _composer.ComposeEmailAsync(
    messageType: "order.confirmation",
    userId: customerId,
    data: data
);

// Result: ONLY CustomerProvider and OrderProvider executed
// InventoryProvider NEVER ran (lazy evaluation saves 33% of queries)
```

---

### Example 3: Compose SMS

```csharp
// SMS template respects 160-character limit
var data = DataContainerFactory.Create(new
{
    Customer = new { FirstName = "John" },
    Order = new { OrderNumber = "12345", TrackingNumber = "1Z999AA10123456784" }
});

var sms = await _composer.ComposeSmsAsync(
    messageType: "order.shipped",
    userId: customerId,
    data: data
);

Console.WriteLine($"SMS ({sms.Content.Length} chars): {sms.Content}");
// Output: "Hi John! Order 12345 shipped. Track: 1Z999AA10123456784"
```

---

### Example 4: Compose Multi-Channel

```csharp
var data = DataContainerFactory.Create(new
{
    Customer = new { FirstName = "John", Email = "john@example.com", Phone = "+1234567890" },
    Order = new { OrderNumber = "12345", EstimatedDelivery = DateTime.UtcNow.AddDays(3) }
});

// Compose all variants in parallel
var multiChannel = await _composer.ComposeMultiChannelAsync(
    messageType: "order.shipped",
    userId: customerId,
    data: data
);

// Email variant (HTML + plain text)
Console.WriteLine($"Email Subject: {multiChannel.EmailVariant.Subject}");
Console.WriteLine($"Email Format: {multiChannel.EmailVariant.MediaType}");

// SMS variant (plain text, 160 chars)
Console.WriteLine($"SMS: {multiChannel.SmsVariant.Content}");

// Push notification variant
if (multiChannel.PushVariant != null)
{
    Console.WriteLine($"Push Title: {multiChannel.PushVariant.Subject}");
    Console.WriteLine($"Push Body: {multiChannel.PushVariant.Content}");
}
```

---

### Example 5: Compose Email with PDF Attachment

```csharp
var invoiceData = DataContainerFactory.Create(new
{
    Invoice = new
    {
        InvoiceNumber = "INV-2024-001",
        Date = DateTime.UtcNow,
        Customer = new { Name = "John Doe", Address = "123 Main St" },
        LineItems = new[]
        {
            new { Description = "Widget", Quantity = 2, Price = 19.99m, Total = 39.98m },
            new { Description = "Gadget", Quantity = 1, Price = 29.99m, Total = 29.99m }
        },
        Subtotal = 69.97m,
        Tax = 5.60m,
        Total = 75.57m
    }
});

var email = await _composer.ComposeEmailWithAttachmentsAsync(
    messageType: "invoice.notification",
    userId: customerId,
    data: invoiceData,
    attachments: new[]
    {
        new AttachmentRequest
        {
            TemplateName = "invoice.pdf",
            Data = invoiceData,
            Filename = $"Invoice_{invoiceData.Evaluate<string>("Invoice/InvoiceNumber")}.pdf",
            MediaType = "application/pdf"
        }
    },
    requiredFormat: "text/html"
);

Console.WriteLine($"Email: {email.Subject}");
Console.WriteLine($"Attachments: {email.Attachments.Count}");
foreach (var attachment in email.Attachments)
{
    Console.WriteLine($"  - {attachment.Filename} ({attachment.Size} bytes)");
}
```

---

### Example 6: Format Conversion Chaining

```csharp
// Template renders to Markdown, needs PDF output
var data = DataContainerFactory.Create(new { Report = "Monthly Report Data" });

var email = await _composer.ComposeEmailAsync(
    messageType: "monthly.report",
    userId: userId,
    data: data,
    requiredFormat: "application/pdf"  // Convert: Markdown → HTML → PDF
);

// Flow:
// 1. Template renders to Markdown (native format)
// 2. Conversion chain: Markdown → HTML → PDF
// 3. Email Content is PDF (base64 or byte array)

Console.WriteLine($"Final Format: {email.MediaType}");  // "application/pdf"
```

---

### Example 7: Culture-Specific Templates

```csharp
// User culture determines template selection
var data = DataContainerFactory.Create(new
{
    User = new { Culture = new CultureInfo("fr-FR") },
    Order = new { OrderNumber = "12345" }
});

// Template selection:
// 1. Try "order.confirmation.fr-FR"
// 2. Try "order.confirmation.fr"
// 3. Try "order.confirmation" (default)

var email = await _composer.ComposeEmailAsync(
    messageType: "order.confirmation",
    userId: customerId,
    data: data
);

Console.WriteLine($"Culture: {email.Culture?.Name}");  // "fr-FR"
Console.WriteLine($"Subject: {email.Subject}");  // "Confirmation de commande #12345"
```

---

### Example 8: Error Handling with Fallbacks

```csharp
try
{
    var data = DataContainerFactory.Create(new { Order = "data" });

    var email = await _composer.ComposeEmailAsync(
        messageType: "order.confirmation",
        userId: customerId,
        data: data,
        requiredFormat: "text/html"
    );
}
catch (TemplateNotFoundException ex)
{
    // Template not found (including fallbacks)
    _logger.LogError(ex, "Template not found: {MessageType}", ex.MessageType);
}
catch (TemplateRenderingException ex)
{
    // Template rendering failed (syntax error, missing data, etc.)
    _logger.LogError(ex, "Template rendering failed: {TemplateId}", ex.TemplateId);
}
catch (FormatConversionException ex)
{
    // Format conversion failed (fallback to original format already attempted)
    _logger.LogError(ex, "Format conversion failed: {Source} → {Target}",
        ex.SourceMediaType, ex.TargetMediaType);
}
catch (MessageCompositionException ex)
{
    // General composition error
    _logger.LogError(ex, "Message composition failed: {MessageType}", ex.MessageType);
}
```

---

### Example 9: Builder Pattern

```csharp
var composer = new MessageComposerBuilder()
    .WithTemplateEngine(templateEngine)
    .WithTemplateProvider(templateProvider)
    .WithConversionService(conversionService)
    .WithLogger(logger)
    .Build();

var email = await composer.ComposeEmailAsync("order.confirmation", userId, data);
```

---

## Extension Methods

### Composition Extensions

```csharp
namespace OoBDev.System.Communications.Composition;

public static class MessageComposerExtensions
{
    /// <summary>
    /// Composes email with inline data (no IDataContainer).
    /// </summary>
    public static async Task<ComposedMessage> ComposeEmailAsync(
        this IMessageComposer composer,
        string messageType,
        Guid userId,
        object data,
        string? requiredFormat = null,
        CancellationToken cancellationToken = default)
    {
        var container = DataContainerFactory.Create(data);
        return await composer.ComposeEmailAsync(messageType, userId, container, requiredFormat, cancellationToken);
    }

    /// <summary>
    /// Composes email and converts to MailMessage for System.Net.Mail.
    /// </summary>
    public static async Task<MailMessage> ComposeMailMessageAsync(
        this IMessageComposer composer,
        string messageType,
        Guid userId,
        IDataContainer data,
        CancellationToken cancellationToken = default)
    {
        var composed = await composer.ComposeEmailAsync(messageType, userId, data, "text/html", cancellationToken);

        var mailMessage = new MailMessage
        {
            Subject = composed.Subject,
            Body = composed.Content,
            IsBodyHtml = composed.MediaType == "text/html"
        };

        if (composed.PlainTextContent != null)
        {
            mailMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(
                composed.PlainTextContent, null, "text/plain"));
        }

        foreach (var attachment in composed.Attachments)
        {
            mailMessage.Attachments.Add(new System.Net.Mail.Attachment(
                new MemoryStream(attachment.Content), attachment.Filename, attachment.MediaType));
        }

        return mailMessage;
    }
}
```

---

## Exception Types

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Base exception for message composition errors.
/// </summary>
public class MessageCompositionException : Exception
{
    public string MessageType { get; }

    public MessageCompositionException(string message, string messageType)
        : base(message)
    {
        MessageType = messageType;
    }

    public MessageCompositionException(string message, Exception innerException, string messageType)
        : base(message, innerException)
    {
        MessageType = messageType;
    }
}

/// <summary>
/// Exception thrown when template is not found.
/// </summary>
public class TemplateNotFoundException : MessageCompositionException
{
    public string TemplateId { get; }

    public TemplateNotFoundException(string message, string messageType, string templateId)
        : base(message, messageType)
    {
        TemplateId = templateId;
    }
}

/// <summary>
/// Exception thrown when template rendering fails.
/// </summary>
public class TemplateRenderingException : MessageCompositionException
{
    public string TemplateId { get; }

    public TemplateRenderingException(string message, Exception innerException, string messageType, string templateId)
        : base(message, innerException, messageType)
    {
        TemplateId = templateId;
    }
}

/// <summary>
/// Exception thrown when format conversion fails.
/// </summary>
public class FormatConversionException : MessageCompositionException
{
    public string SourceMediaType { get; }
    public string TargetMediaType { get; }

    public FormatConversionException(
        string message,
        Exception innerException,
        string messageType,
        string sourceMediaType,
        string targetMediaType)
        : base(message, innerException, messageType)
    {
        SourceMediaType = sourceMediaType;
        TargetMediaType = targetMediaType;
    }
}

/// <summary>
/// Exception thrown when attachment generation fails.
/// </summary>
public class AttachmentGenerationException : MessageCompositionException
{
    public string AttachmentTemplate { get; }

    public AttachmentGenerationException(
        string message,
        Exception innerException,
        string messageType,
        string attachmentTemplate)
        : base(message, innerException, messageType)
    {
        AttachmentTemplate = attachmentTemplate;
    }
}
```

---

## Dependency Injection

### Service Registration

```csharp
namespace Microsoft.Extensions.DependencyInjection;

public static class MessageCompositionServiceCollectionExtensions
{
    /// <summary>
    /// Adds message composition services to dependency injection.
    /// </summary>
    public static IServiceCollection AddMessageComposition(
        this IServiceCollection services)
    {
        services.TryAddSingleton<IMessageComposer, MessageComposer>();
        services.TryAddSingleton<ITemplateSelector, TemplateSelector>();
        services.TryAddSingleton<IMessageRenderer, MessageRenderer>();
        services.TryAddSingleton<IFormatConverter, FormatConverter>();

        return services;
    }

    /// <summary>
    /// Adds message composition with configuration.
    /// </summary>
    public static IServiceCollection AddMessageComposition(
        this IServiceCollection services,
        Action<MessageCompositionOptions> configure)
    {
        services.Configure(configure);
        return services.AddMessageComposition();
    }
}

/// <summary>
/// Configuration options for message composition.
/// </summary>
public class MessageCompositionOptions
{
    /// <summary>
    /// Default culture for template selection fallback.
    /// </summary>
    public CultureInfo DefaultCulture { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Maximum attachment size in bytes.
    /// </summary>
    public long MaxAttachmentSize { get; set; } = 10 * 1024 * 1024;  // 10 MB

    /// <summary>
    /// Maximum number of attachments per message.
    /// </summary>
    public int MaxAttachmentsPerMessage { get; set; } = 5;

    /// <summary>
    /// Enable template caching.
    /// </summary>
    public bool EnableTemplateCaching { get; set; } = true;

    /// <summary>
    /// Template cache expiration (minutes).
    /// </summary>
    public int TemplateCacheExpirationMinutes { get; set; } = 60;
}
```

---

## Best Practices

### 1. Data Container Usage
```csharp
// ✅ GOOD: Register providers, let composition trigger lazy evaluation
var data = DataContainerFactory.Create();
data.RegisterProvider("Customer", customerProvider);
data.RegisterProvider("Order", orderProvider);

var email = await _composer.ComposeEmailAsync("order.confirmation", userId, data);

// ❌ BAD: Pre-fetch all data (defeats lazy evaluation)
var customer = await _customerRepo.GetByIdAsync(customerId);
var order = await _orderRepo.GetByIdAsync(orderId);
var data = DataContainerFactory.Create(new { Customer = customer, Order = order });
```

### 2. Format Conversion
```csharp
// ✅ GOOD: Let composer handle conversion
var email = await _composer.ComposeEmailAsync(
    "order.confirmation", userId, data, requiredFormat: "text/html");

// ❌ BAD: Manual conversion
var rendered = await _renderer.RenderAsync(templateId, data);
var converted = await _converter.ConvertAsync(rendered.Content, rendered.MediaType, "text/html");
```

### 3. Multi-Channel Composition
```csharp
// ✅ GOOD: Single call generates all variants in parallel
var multiChannel = await _composer.ComposeMultiChannelAsync("order.shipped", userId, data);

// ❌ BAD: Sequential composition
var email = await _composer.ComposeEmailAsync("order.shipped", userId, data);
var sms = await _composer.ComposeSmsAsync("order.shipped", userId, data);
```

---

## Performance Considerations

### Lazy Evaluation Benefit
```csharp
// Without lazy evaluation: 3 provider executions
var customer = await _customerProvider.ProvideAsync(...);  // Query 1
var order = await _orderProvider.ProvideAsync(...);        // Query 2
var inventory = await _inventoryProvider.ProvideAsync(...); // Query 3

// With lazy evaluation: 1 provider execution (template uses ONLY Customer)
data.RegisterProvider("Customer", customerProvider);
data.RegisterProvider("Order", orderProvider);
data.RegisterProvider("Inventory", inventoryProvider);

var email = await _composer.ComposeEmailAsync("welcome.email", userId, data);
// Template: "Hello {{Customer/FirstName}}!"
// ONLY customerProvider executes (66% query reduction)
```

---

## Related Documents

- [Requirements](./requirements.md)
- [Architecture](./architecture.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 10: Templates API](../10-TextTemplating/HandlebarsProvider/api-design.md)
- [Epic 11: Data Enhancement API](../11-DataEnhancement/CoreContainer/api-design.md)
