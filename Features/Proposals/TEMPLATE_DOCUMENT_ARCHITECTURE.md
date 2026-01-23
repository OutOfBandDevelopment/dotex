# Template & Document Architecture - Revised

**Date:** 2026-01-22
**Status:** ✅ Final Architecture
**Key Insight:** Templates generate content in their native format; Document Conversion Pipeline handles transformations

---

## Overview

**Separation of Concerns:**
- **Templates** - Generate content in their native media type
- **Document Conversion** - Transform between media types
- **Message Composition** - Orchestrates template rendering + conversion as needed

---

## Template Responsibility: Content Generation ONLY

### Template Declares Output Media Type

```csharp
public interface ITemplateContext
{
    string Name { get; }                    // "order-confirmation"
    string ContentType { get; }             // Input format: "text/x-handlebars-template"
    string OutputMediaType { get; }         // ⭐ NEW: Output format: "text/html", "text/markdown", etc.
    ITemplateContentSource Source { get; }
}
```

### Examples

**Handlebars Template (produces HTML):**
```
Name: "order-confirmation.html"
ContentType: "text/x-handlebars-template"
OutputMediaType: "text/html"  ← Template declares it produces HTML
```

**Markdown Template (produces Markdown):**
```
Name: "order-confirmation.md"
ContentType: "text/markdown"
OutputMediaType: "text/markdown"  ← Template declares it produces Markdown
```

**XSLT Template (produces XML):**
```
Name: "invoice.xslt"
ContentType: "application/xslt+xml"
OutputMediaType: "application/xml"  ← Template declares it produces XML
```

---

## Template Engine: Render in Native Format

### ITemplateEngine (Updated)

```csharp
public interface ITemplateEngine
{
    /// <summary>
    /// Renders template to its native output format.
    /// Does NOT convert to other formats.
    /// </summary>
    /// <param name="templateName">Template to render</param>
    /// <param name="data">Data for template</param>
    /// <returns>Rendered content in template's declared OutputMediaType</returns>
    Task<RenderedContent> RenderAsync(string templateName, IDataContainer data);
}

public class RenderedContent
{
    /// <summary>
    /// Rendered content (e.g., HTML, Markdown, XML)
    /// </summary>
    public string Content { get; set; } = "";

    /// <summary>
    /// Media type of rendered content (e.g., "text/html", "text/markdown")
    /// From template's OutputMediaType
    /// </summary>
    public string MediaType { get; set; } = "";

    /// <summary>
    /// Template that was rendered
    /// </summary>
    public string TemplateName { get; set; } = "";
}
```

### Template Provider: Just Render

```csharp
public class HandlebarsTemplateProvider : ITemplateProvider
{
    public async Task<RenderedContent> RenderAsync(ITemplateContext context, object data)
    {
        var templateContent = await context.Source.GetContentAsync();
        var template = Handlebars.Compile(templateContent);
        var result = template(data);

        return new RenderedContent
        {
            Content = result,
            MediaType = context.OutputMediaType,  // "text/html" (from template metadata)
            TemplateName = context.Name
        };
    }
}
```

**Templates DON'T:**
- ❌ Convert Markdown → HTML
- ❌ Convert HTML → PDF
- ❌ Handle multiple output formats
- ❌ Know about final delivery format

**Templates DO:**
- ✅ Generate content in their native format
- ✅ Declare what format they produce
- ✅ Render data into content

---

## Document Conversion Pipeline: Format Transformation

### Epic 6, Feature 2: Document Conversion Pipelines

**Responsibility:** Transform content between media types

```csharp
public interface IDocumentConverter
{
    /// <summary>
    /// Converts content from one media type to another.
    /// </summary>
    /// <param name="input">Input content</param>
    /// <param name="fromMediaType">Source media type (e.g., "text/markdown")</param>
    /// <param name="toMediaType">Target media type (e.g., "text/html")</param>
    /// <returns>Converted content</returns>
    Task<ConvertedContent> ConvertAsync(string input, string fromMediaType, string toMediaType);

    /// <summary>
    /// Checks if this converter supports the conversion.
    /// </summary>
    bool CanConvert(string fromMediaType, string toMediaType);
}

public class ConvertedContent
{
    public string Content { get; set; } = "";
    public string MediaType { get; set; } = "";
}
```

### Converter Examples

**Markdown → HTML Converter:**
```csharp
public class MarkdownToHtmlConverter : IDocumentConverter
{
    public bool CanConvert(string from, string to)
    {
        return from == "text/markdown" && to == "text/html";
    }

    public async Task<ConvertedContent> ConvertAsync(string input, string fromMediaType, string toMediaType)
    {
        var html = Markdig.Markdown.ToHtml(input);

        return new ConvertedContent
        {
            Content = html,
            MediaType = "text/html"
        };
    }
}
```

**HTML → PDF Converter:**
```csharp
public class HtmlToPdfConverter : IDocumentConverter
{
    public bool CanConvert(string from, string to)
    {
        return from == "text/html" && to == "application/pdf";
    }

    public async Task<ConvertedContent> ConvertAsync(string input, string fromMediaType, string toMediaType)
    {
        // Use library like PuppeteerSharp, IronPdf, or wkhtmltopdf
        var pdf = await _pdfGenerator.GenerateFromHtmlAsync(input);

        return new ConvertedContent
        {
            Content = Convert.ToBase64String(pdf),  // or Stream
            MediaType = "application/pdf"
        };
    }
}
```

**XML → JSON Converter:**
```csharp
public class XmlToJsonConverter : IDocumentConverter
{
    public bool CanConvert(string from, string to)
    {
        return from == "application/xml" && to == "application/json";
    }

    public async Task<ConvertedContent> ConvertAsync(string input, string fromMediaType, string toMediaType)
    {
        var xml = XDocument.Parse(input);
        var json = JsonConvert.SerializeXNode(xml);

        return new ConvertedContent
        {
            Content = json,
            MediaType = "application/json"
        };
    }
}
```

### Conversion Pipeline: Multi-Step Conversions

```csharp
public interface IConversionPipeline
{
    /// <summary>
    /// Converts content through multiple steps if needed.
    /// Example: Markdown → HTML → PDF
    /// </summary>
    Task<ConvertedContent> ConvertAsync(string input, string fromMediaType, string toMediaType);
}

public class ConversionPipeline : IConversionPipeline
{
    private readonly IEnumerable<IDocumentConverter> _converters;

    public async Task<ConvertedContent> ConvertAsync(string input, string fromMediaType, string toMediaType)
    {
        // Direct conversion available?
        var directConverter = _converters.FirstOrDefault(c => c.CanConvert(fromMediaType, toMediaType));
        if (directConverter != null)
        {
            return await directConverter.ConvertAsync(input, fromMediaType, toMediaType);
        }

        // Multi-step conversion needed (e.g., Markdown → HTML → PDF)
        var path = FindConversionPath(fromMediaType, toMediaType);

        if (path == null)
        {
            throw new InvalidOperationException($"No conversion path from {fromMediaType} to {toMediaType}");
        }

        var current = input;
        var currentMediaType = fromMediaType;

        foreach (var converter in path)
        {
            var result = await converter.ConvertAsync(current, currentMediaType, converter.TargetMediaType);
            current = result.Content;
            currentMediaType = result.MediaType;
        }

        return new ConvertedContent
        {
            Content = current,
            MediaType = toMediaType
        };
    }

    private IEnumerable<IDocumentConverter>? FindConversionPath(string from, string to)
    {
        // Graph search to find conversion path
        // Example: Markdown → HTML → PDF
        // Uses Markdown→HTML converter, then HTML→PDF converter
    }
}
```

---

## Message Composition Service: Orchestrates Rendering + Conversion

### Updated Interface

```csharp
public interface IMessageCompositionService
{
    /// <summary>
    /// Composes email message with automatic format conversion.
    /// </summary>
    /// <param name="messageType">Message type (e.g., "order.confirmation")</param>
    /// <param name="userId">Target user</param>
    /// <param name="data">Data container</param>
    /// <param name="requiredFormat">Required output format (e.g., "text/html", "application/pdf")</param>
    /// <returns>Email message in required format</returns>
    Task<IEmailMessage> ComposeEmailAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        string? requiredFormat = null);  // null = use template's native format
}
```

### Implementation

```csharp
public class MessageCompositionService : IMessageCompositionService
{
    private readonly ITemplateEngine _templates;
    private readonly IConversionPipeline _conversion;

    public async Task<IEmailMessage> ComposeEmailAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        string? requiredFormat = null)
    {
        // 1. Render template to native format
        var rendered = await _templates.RenderAsync($"{messageType}.email", data);
        // rendered.MediaType might be "text/markdown" (template's native format)

        // 2. Convert if needed
        var finalContent = rendered.Content;
        var finalMediaType = rendered.MediaType;

        if (requiredFormat != null && requiredFormat != rendered.MediaType)
        {
            // Template produced Markdown, but we need HTML
            var converted = await _conversion.ConvertAsync(
                rendered.Content,
                rendered.MediaType,
                requiredFormat
            );

            finalContent = converted.Content;
            finalMediaType = converted.MediaType;
        }

        // 3. Build email message
        return new EmailMessage
        {
            ToAddress = data.Evaluate<string>("User/Email"),
            Subject = rendered.Content,  // Subject template
            HtmlContent = finalMediaType == "text/html" ? finalContent : null,
            TextContent = finalMediaType == "text/plain" ? finalContent : null,
            MessageType = messageType,
            RequestId = Guid.NewGuid()
        };
    }
}
```

---

## Usage Examples

### Example 1: Markdown Template → HTML Email

```csharp
// Template: "order-confirmation.md.hbs"
// OutputMediaType: "text/markdown"
// Content:
```
# Order Confirmation

Hi {{Customer/FirstName}},

Your order #{{Order/OrderNumber}} has been confirmed.

**Order Total:** ${{Order/Total}}

## Line Items
{{#each Order/LineItems}}
- {{ProductName}}: {{Quantity}} x ${{UnitPrice}}
{{/each}}
```

// Message composition
var data = DataContainerFactory.Create(new { OrderId = 12345 });
data.RegisterProvider("Customer", customerProvider);
data.RegisterProvider("Order", orderProvider);

var email = await _composition.ComposeEmailAsync(
    messageType: "order.confirmation",
    userId: customerId,
    data: data,
    requiredFormat: "text/html"  // ← Need HTML for email
);

// Flow:
// 1. Template renders to Markdown (native format)
// 2. Conversion pipeline: Markdown → HTML
// 3. Email message gets HTML content
```

### Example 2: XSLT Template → PDF Invoice

```csharp
// Template: "invoice.xslt"
// OutputMediaType: "application/xml"

var data = DataContainerFactory.Create(new { InvoiceId = 67890 });
data.RegisterProvider("Invoice", invoiceProvider);

var rendered = await _templates.RenderAsync("invoice", data);
// rendered.MediaType = "application/xml"

var pdf = await _conversion.ConvertAsync(
    rendered.Content,
    fromMediaType: "application/xml",
    toMediaType: "application/pdf"
);

// Flow:
// 1. XSLT renders to XML (native format)
// 2. Conversion pipeline: XML → HTML → PDF (multi-step)
// 3. PDF document generated
```

### Example 3: HTML Template → Text Email (fallback)

```csharp
// Template: "welcome.html.hbs"
// OutputMediaType: "text/html"

var email = await _composition.ComposeEmailAsync(
    messageType: "user.welcome",
    userId: userId,
    data: data,
    requiredFormat: null  // Use native format
);

// Also generate plain text version
var rendered = await _templates.RenderAsync("user.welcome", data);
// rendered.MediaType = "text/html"

var textVersion = await _conversion.ConvertAsync(
    rendered.Content,
    fromMediaType: "text/html",
    toMediaType: "text/plain"  // Strip HTML tags
);

email.HtmlContent = rendered.Content;
email.TextContent = textVersion.Content;

// Flow:
// 1. Template renders to HTML (native format)
// 2. HTML used as-is for email
// 3. HTML → Plain Text conversion for text fallback
```

### Example 4: Same Template, Multiple Outputs

```csharp
// Single Markdown template can produce multiple formats

var data = DataContainerFactory.Create(new { ReportId = 123 });
data.RegisterProvider("Report", reportProvider);

// Render once
var rendered = await _templates.RenderAsync("monthly-report", data);
// rendered.MediaType = "text/markdown"

// Generate HTML version (for web)
var htmlReport = await _conversion.ConvertAsync(
    rendered.Content, "text/markdown", "text/html"
);

// Generate PDF version (for download)
var pdfReport = await _conversion.ConvertAsync(
    rendered.Content, "text/markdown", "application/pdf"
);

// Generate DOCX version (for editing)
var docxReport = await _conversion.ConvertAsync(
    rendered.Content, "text/markdown", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
);

// Same template, three different outputs!
```

---

## Template Organization (Updated)

### File Naming Convention

```
Templates/
├── email/
│   ├── order-confirmation.subject.hbs          (OutputMediaType: text/plain)
│   ├── order-confirmation.body.md.hbs          (OutputMediaType: text/markdown)
│   └── order-confirmation.body.html.hbs        (OutputMediaType: text/html)
├── pdf/
│   ├── invoice.xslt                            (OutputMediaType: application/xml)
│   └── receipt.md.hbs                          (OutputMediaType: text/markdown)
└── reports/
    └── monthly-sales.md.hbs                     (OutputMediaType: text/markdown)
```

### Template Metadata

**Database Schema:**
```sql
CREATE TABLE Templates (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,           -- Input: "text/x-handlebars-template"
    OutputMediaType NVARCHAR(100) NOT NULL,       -- ⭐ NEW: Output: "text/markdown"
    Content NVARCHAR(MAX) NOT NULL,
    Culture NVARCHAR(10) NULL,
    Version INT NOT NULL DEFAULT 1
);
```

**File Metadata (.json):**
```json
{
  "name": "order-confirmation.body",
  "contentType": "text/x-handlebars-template",
  "outputMediaType": "text/markdown",
  "culture": "en-US",
  "path": "email/order-confirmation.body.md.hbs"
}
```

---

## Converter Registration

```csharp
// Startup
services.AddSingleton<IDocumentConverter, MarkdownToHtmlConverter>();
services.AddSingleton<IDocumentConverter, HtmlToPdfConverter>();
services.AddSingleton<IDocumentConverter, HtmlToPlainTextConverter>();
services.AddSingleton<IDocumentConverter, XmlToHtmlConverter>();
services.AddSingleton<IDocumentConverter, XmlToJsonConverter>();

services.AddSingleton<IConversionPipeline, ConversionPipeline>();
```

**Available Conversion Paths:**
```
Markdown → HTML (direct)
Markdown → PDF (via HTML)
HTML → PDF (direct)
HTML → Plain Text (direct)
XML → HTML (direct)
XML → PDF (via HTML)
XML → JSON (direct)
```

---

## Benefits

### For Templates
✅ **Simple responsibility** - Just render content
✅ **Native format** - Use template engine's natural output
✅ **Reusable** - Same template, multiple output formats
✅ **No conversion logic** - Don't need to know about PDF, DOCX, etc.

### For Document Conversion
✅ **Centralized** - All format transformations in one place
✅ **Composable** - Chain converters (Markdown → HTML → PDF)
✅ **Testable** - Test conversions independently
✅ **Extensible** - Add new converters without touching templates

### For Message Composition
✅ **Flexible** - Specify required format, get automatic conversion
✅ **Clean orchestration** - Render + Convert + Build message
✅ **Performance** - Cache rendered content, convert as needed

---

## Architecture Diagram

```
┌─────────────────────────────────────────┐
│ Message Composition Service              │
│ - Orchestrates rendering + conversion    │
└──────────────┬──────────────────────────┘
               ↓
     ┌─────────┴─────────┐
     ↓                   ↓
┌────────────────┐  ┌──────────────────────┐
│ Template Engine│  │ Conversion Pipeline   │
│                │  │                       │
│ - Render to    │  │ - Convert between     │
│   native format│  │   media types         │
│                │  │                       │
│ Markdown → MD  │  │ MD → HTML            │
│ Handlebars→HTML│  │ HTML → PDF           │
│ XSLT → XML     │  │ XML → JSON           │
└────────────────┘  └──────────────────────┘
       ↓                      ↓
┌─────────────────────────────────────────┐
│ Output (Email, PDF, DOCX, etc.)          │
└──────────────────────────────────────────┘
```

---

## Updated Epic Dependencies

### Epic 10: Text Templating
**Responsibility:** Render templates to their native format
- ❌ Does NOT handle format conversion
- ✅ Declares OutputMediaType
- ✅ Renders content only

### Epic 6: Document Management - Feature 2: Conversion Pipelines
**Responsibility:** Transform between media types
- ✅ Markdown → HTML
- ✅ HTML → PDF
- ✅ XML → JSON
- ✅ Multi-step conversions

### Epic 12: Message Composition Service
**Responsibility:** Orchestrate rendering + conversion
- ✅ Render template (Epic 10)
- ✅ Convert format if needed (Epic 6)
- ✅ Build final message

---

## Migration Impact

### SharedFramework Analysis
**Original:** Templates may have had format-specific logic
**New:** Templates are pure content generators

### Changes Required
1. Templates declare `OutputMediaType`
2. Template providers return `RenderedContent` (not just string)
3. Format conversion moved to `IConversionPipeline`
4. Message composition orchestrates both

---

## Related Documentation

- [Consolidated Design](./CONSOLIDATED_DESIGN.md)
- [Epic 10: Text Templating](./10-TextTemplating/README-REVISED.md)
- [Epic 6: Document Management](./EPIC_REVIEW.md#epic-6-document-management-split)
- [Epic 12: Message Composition](./CONSOLIDATED_DESIGN.md#epic-12-message-composition-service-new)
