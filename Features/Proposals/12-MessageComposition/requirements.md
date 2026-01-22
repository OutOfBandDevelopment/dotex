# Message Composition Service - Requirements

**Epic:** 12 - Message Composition Service
**Feature:** Message Composition Orchestration
**Priority:** HIGH (Cross-Epic Integration)
**Complexity:** MEDIUM
**Estimated LOC:** ~400

---

## Overview

Orchestrates template rendering, data enhancement, and document conversion to produce pre-formatted messages ready for delivery. Integrates Epic 11 (Data Enhancement), Epic 10 (Templates), and Epic 6 (Document Conversion) into a unified composition pipeline.

---

## Business Requirements

### BR-1: Multi-Format Message Composition
**As a** developer
**I want** to compose messages in multiple formats (HTML, PDF, plain text)
**So that** I can deliver messages via different channels with appropriate formatting

**Acceptance Criteria:**
- Compose email messages with HTML and plain text variants
- Compose SMS messages with plain text
- Compose PDF attachments for emails
- Compose multi-channel messages with format-specific content
- Support format conversion chaining (template → Markdown → HTML → PDF)

---

### BR-2: Template Selection by Message Type
**As a** system
**I want** automatic template selection based on message type
**So that** each message type uses the correct template

**Acceptance Criteria:**
- Template selected by message type identifier (e.g., "order.confirmation")
- Culture-specific template variants (e.g., "order.confirmation.en-US")
- Fallback to default culture if specific variant not found
- Template inheritance supported (e.g., "order.confirmation" extends "base.email")

**Example:**
```csharp
// Template selection logic
var email = await _composer.ComposeEmailAsync(
    messageType: "order.confirmation",  // Selects appropriate template
    userId: customerId,
    data: orderData,
    requiredFormat: "text/html"
);
```

---

### BR-3: Lazy Data Evaluation Integration
**As a** system
**I want** composition to use IDataContainer lazy evaluation
**So that** we only fetch data actually used by the template

**Acceptance Criteria:**
- IDataContainer provided to composer
- Template rendering triggers only required data providers
- Unused data providers never executed
- Performance improvement of 50-70% for typical scenarios

**Example:**
```csharp
// Register providers (NOT executed)
data.RegisterProvider("Customer", customerProvider);
data.RegisterProvider("Order", orderProvider);
data.RegisterProvider("Inventory", inventoryProvider);

// Template uses ONLY Customer and Order
var template = "Hello {{Customer/FirstName}}, your order {{Order/OrderNumber}} is confirmed.";

// Result: ONLY customerProvider and orderProvider execute
// inventoryProvider never runs (lazy evaluation benefit)
var email = await _composer.ComposeEmailAsync("order.confirmation", customerId, data);
```

---

### BR-4: Automatic Format Conversion
**As a** developer
**I want** automatic conversion from template native format to required format
**So that** I don't manually handle format conversions

**Acceptance Criteria:**
- Templates have native format (e.g., Markdown, HTML, plain text)
- Composer automatically converts to required format if different
- Conversion chaining supported (Markdown → HTML → PDF)
- No conversion if template format matches required format

**Example:**
```csharp
// Template renders to Markdown (native format)
// Composer automatically converts to HTML
var email = await _composer.ComposeEmailAsync(
    "order.confirmation",
    customerId,
    data,
    requiredFormat: "text/html"  // Auto-converts from Markdown
);

// Flow:
// 1. Template renders to Markdown
// 2. Conversion: Markdown → HTML
// 3. Email gets HTML content
```

---

### BR-5: Attachment Handling
**As a** developer
**I want** to include document attachments with composed messages
**So that** I can send invoices, receipts, and other documents

**Acceptance Criteria:**
- Attach documents to email messages
- Generate PDF attachments from templates
- Include multiple attachments per message
- Attachment metadata (filename, media type, size)

**Example:**
```csharp
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
            Filename = $"Invoice_{invoiceNumber}.pdf"
        }
    }
);
```

---

### BR-6: Multi-Channel Message Support
**As a** developer
**I want** to compose messages for multiple channels simultaneously
**So that** I can deliver the same message via email, SMS, and push notifications

**Acceptance Criteria:**
- Single composition call produces channel-specific variants
- Email variant includes HTML and plain text
- SMS variant respects character limits
- Push notification variant includes title and body
- All variants use same data container (lazy evaluation)

**Example:**
```csharp
var multiChannel = await _composer.ComposeMultiChannelAsync(
    messageType: "order.shipped",
    userId: customerId,
    data: orderData
);

// Returns:
// - Email: HTML + plain text variants
// - SMS: Plain text (160 chars)
// - Push: Title + body
```

---

## Technical Requirements

### TR-1: Interface Design

```csharp
namespace OoBDev.System.Communications.Composition;

/// <summary>
/// Orchestrates template rendering, data enhancement, and format conversion
/// to produce pre-formatted messages.
/// </summary>
public interface IMessageComposer
{
    /// <summary>
    /// Composes email message with automatic format conversion.
    /// </summary>
    /// <param name="messageType">Message type identifier (e.g., "order.confirmation")</param>
    /// <param name="userId">User ID for culture/personalization</param>
    /// <param name="data">Data container with lazy providers</param>
    /// <param name="requiredFormat">Target format (text/html, text/plain, etc.)</param>
    /// <returns>Composed email message ready for delivery</returns>
    Task<ComposedMessage> ComposeEmailAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        string? requiredFormat = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes SMS message with plain text.
    /// </summary>
    Task<ComposedMessage> ComposeSmsAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes multi-channel message with format-specific variants.
    /// </summary>
    Task<MultiChannelMessage> ComposeMultiChannelAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Composes email with document attachments.
    /// </summary>
    Task<ComposedMessage> ComposeEmailWithAttachmentsAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        IEnumerable<AttachmentRequest> attachments,
        string? requiredFormat = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Selects appropriate template based on message type and context.
/// </summary>
public interface ITemplateSelector
{
    /// <summary>
    /// Selects template ID for message type and culture.
    /// </summary>
    Task<string> SelectTemplateAsync(
        string messageType,
        CultureInfo culture,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Renders template with data container.
/// </summary>
public interface IMessageRenderer
{
    /// <summary>
    /// Renders template using data container (lazy evaluation).
    /// </summary>
    Task<RenderedContent> RenderAsync(
        string templateId,
        IDataContainer data,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Converts rendered content to required format.
/// </summary>
public interface IFormatConverter
{
    /// <summary>
    /// Converts content from source format to target format.
    /// </summary>
    Task<ConvertedContent> ConvertAsync(
        string content,
        string sourceMediaType,
        string targetMediaType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if conversion is supported.
    /// </summary>
    bool SupportsConversion(string sourceMediaType, string targetMediaType);
}
```

---

### TR-2: Data Models

```csharp
/// <summary>
/// Composed message ready for delivery.
/// </summary>
public class ComposedMessage
{
    public Guid MessageId { get; set; }
    public string MessageType { get; set; }
    public Guid UserId { get; set; }
    public string Subject { get; set; }
    public string Content { get; set; }
    public string MediaType { get; set; }
    public string? PlainTextContent { get; set; }
    public IReadOnlyList<Attachment> Attachments { get; set; }
    public IDictionary<string, string> Metadata { get; set; }
    public DateTime ComposedAt { get; set; }
}

/// <summary>
/// Multi-channel message with format-specific variants.
/// </summary>
public class MultiChannelMessage
{
    public Guid MessageId { get; set; }
    public string MessageType { get; set; }
    public Guid UserId { get; set; }
    public ComposedMessage EmailVariant { get; set; }
    public ComposedMessage SmsVariant { get; set; }
    public ComposedMessage? PushVariant { get; set; }
    public DateTime ComposedAt { get; set; }
}

/// <summary>
/// Attachment request for message composition.
/// </summary>
public class AttachmentRequest
{
    public string TemplateName { get; set; }
    public IDataContainer Data { get; set; }
    public string Filename { get; set; }
    public string MediaType { get; set; }
}

/// <summary>
/// Rendered template content.
/// </summary>
public class RenderedContent
{
    public string Content { get; set; }
    public string MediaType { get; set; }
    public string Subject { get; set; }
    public IDictionary<string, string> Metadata { get; set; }
}

/// <summary>
/// Converted content result.
/// </summary>
public class ConvertedContent
{
    public string Content { get; set; }
    public string MediaType { get; set; }
}
```

---

### TR-3: Composition Pipeline

**Pipeline Stages:**
1. **Template Selection** - Select template by message type and culture
2. **Data Enhancement** - Register providers in IDataContainer
3. **Template Rendering** - Render template with lazy data evaluation
4. **Format Conversion** - Convert to required format if needed
5. **Attachment Generation** - Render and convert attachments
6. **Message Assembly** - Assemble final ComposedMessage

**Flow Diagram:**
```
┌──────────────────────┐
│ Template Selection   │
│ (by type + culture)  │
└──────────┬───────────┘
           ↓
┌──────────────────────┐
│ Data Enhancement     │
│ (register providers) │
└──────────┬───────────┘
           ↓
┌──────────────────────┐
│ Template Rendering   │
│ (lazy evaluation)    │
└──────────┬───────────┘
           ↓
┌──────────────────────┐
│ Format Conversion    │
│ (if needed)          │
└──────────┬───────────┘
           ↓
┌──────────────────────┐
│ Attachment Gen.      │
│ (if requested)       │
└──────────┬───────────┘
           ↓
┌──────────────────────┐
│ Message Assembly     │
│ (ComposedMessage)    │
└──────────────────────┘
```

---

### TR-4: Performance Requirements

- **Simple Message (text only):** < 100ms composition time
- **HTML Message (with conversion):** < 200ms composition time
- **PDF Attachment:** < 500ms per attachment
- **Multi-Channel:** < 300ms for all variants
- **Lazy Evaluation Benefit:** 50-70% query reduction vs. eager loading
- **Concurrent Composition:** 100+ messages/second per instance

---

### TR-5: Error Handling

**Error Scenarios:**
1. Template not found → `TemplateNotFoundException`
2. Template rendering failure → `TemplateRenderingException`
3. Format conversion failure → `FormatConversionException`
4. Data provider failure → `DataProviderException` (from Epic 11)
5. Attachment generation failure → `AttachmentGenerationException`

**Error Recovery:**
- Fallback to default culture template
- Fallback to plain text if HTML conversion fails
- Skip failed attachments, log warning
- Graceful degradation preferred over complete failure

---

## Non-Functional Requirements

### NFR-1: Compatibility
- Works with .NET 10.0
- Supports async/await patterns
- Compatible with dependency injection
- Integrates with Epic 2 (Communications), Epic 6 (Document Conversion), Epic 10 (Templates), Epic 11 (Data Enhancement)

### NFR-2: Extensibility
- Custom template selectors supported
- Custom format converters supported
- Custom attachment generators supported
- Pluggable caching strategies

### NFR-3: Testability
- Mock template engine for unit testing
- Mock format converter for unit testing
- Deterministic behavior for integration tests
- Performance metrics trackable

---

## Constraints

### C-1: Template Constraints
- Templates must declare native format (Markdown, HTML, plain text)
- Template IDs must follow naming convention: `{type}.{channel}[.{culture}]`
- Templates cannot exceed 1MB in size
- Template variables must match IDataContainer paths

### C-2: Format Conversion Constraints
- Conversion must preserve content semantics
- Not all conversions supported (e.g., HTML → PDF requires external library)
- Conversion quality configurable per request
- Large documents (> 10MB) may timeout

### C-3: Performance Constraints
- Composition must complete within 5 seconds (hard limit)
- Attachment generation limited to 5 attachments per message
- Multi-channel composition generates all variants in parallel
- Caching recommended for frequently used templates

---

## Success Criteria

- ✅ Compose email messages with HTML and plain text variants
- ✅ Compose SMS messages respecting character limits
- ✅ Compose multi-channel messages with format-specific content
- ✅ Automatic template selection by message type and culture
- ✅ Automatic format conversion from template native format
- ✅ IDataContainer integration with lazy evaluation
- ✅ Attachment generation and inclusion
- ✅ Performance: < 100ms for simple messages, < 500ms with PDF
- ✅ 85%+ test coverage
- ✅ Thread-safe concurrent composition

---

## Out of Scope

- ❌ Message delivery (use Epic 2 Communications)
- ❌ Template authoring UI (templates created externally)
- ❌ Advanced PDF features (forms, signatures, encryption)
- ❌ Real-time message preview (separate feature)

---

## Dependencies

### Internal
- **Epic 11** - Data Enhancement (IDataContainer, IDataProvider)
- **Epic 10** - Templates (ITemplateEngine, ITemplateProvider)
- **Epic 6** - Document Conversion (IDocumentConversionService)
- **Epic 2** - Communications (IChannel for delivery)

### External
- .NET 10.0 BCL
- System.Globalization (culture support)
- Dependency injection framework

---

## Related Documents

- [Architecture](./architecture.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 12 Overview](../CONSOLIDATED_DESIGN.md#epic-12-message-composition-service)
