# Epic 10: Text Templating Extensions (REVISED)

**Priority:** MEDIUM
**Status:** 📋 Design Phase (Architecture Revised)
**Complexity:** LOW (leverage existing framework)
**Impact:** ~400 LOC (additions to existing template engine)

---

## Overview

**IMPORTANT:** The OoBDev framework **already has a solid template engine** at `OoBDev.System.Text.Templating` with provider-based architecture. This epic is about **extending** it, NOT replacing it.

**Existing Framework:**
- ✅ `ITemplateEngine` - Main engine interface
- ✅ `ITemplateProvider` - Provider abstraction for different template engines
- ✅ `ITemplateSource` - Template storage abstraction
- ✅ `ITemplateContext` - Template metadata/context
- ✅ `XsltTemplateProvider` - XSLT support (already implemented)
- ✅ `FileTemplateSource` - File-based template storage

**This Epic Adds:**
1. **Additional industry-standard template providers** (Handlebars, maybe Liquid/Scriban)
2. **Enhanced template storage** (database, Azure Blob)
3. **Integration with IDataContainer** (Epic 11) for lazy evaluation
4. **Template caching improvements**

---

## Architecture (Existing Framework)

### Current Implementation

```csharp
// Main engine (already exists)
public interface ITemplateEngine
{
    Task<string?> ApplyAsync(string templateName, object data);
    Task<ITemplateContext?> ApplyAsync(string templateName, object data, Stream target);
    ITemplateContext? Get(string templateName);
    IEnumerable<ITemplateContext> GetAll(string templateName);
}

// Provider abstraction (already exists)
public interface ITemplateProvider
{
    IReadOnlyCollection<string> SupportedContentTypes { get; }
    bool CanApply(ITemplateContext context);
    Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target);
}

// Template storage (already exists)
public interface ITemplateSource
{
    IEnumerable<ITemplateContext> GetTemplates();
}

// Existing provider (already implemented)
public class XsltTemplateProvider : ITemplateProvider
{
    public IReadOnlyCollection<string> SupportedContentTypes => new[] { "text/xml", "application/xslt+xml" };
    public bool CanApply(ITemplateContext context) => context.ContentType?.StartsWith("text/xml") ?? false;
    public async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target) { /* ... */ }
}
```

### Provider Pattern (Already Established)

```csharp
// Registration (already works this way)
services.AddSingleton<ITemplateProvider, XsltTemplateProvider>();
services.AddSingleton<ITemplateProvider, HandlebarsTemplateProvider>();  // NEW
services.AddSingleton<ITemplateProvider, LiquidTemplateProvider>();      // NEW

services.AddSingleton<ITemplateSource, FileTemplateSource>();
services.AddSingleton<ITemplateSource, DatabaseTemplateSource>();        // NEW

services.AddSingleton<ITemplateEngine, TemplateEngine>();
```

**Engine automatically discovers and uses appropriate provider based on ContentType.**

---

## Feature Breakdown

### Feature 1: Handlebars Template Provider (NEW)
**Path:** `./HandlebarsProvider/`
**Priority:** HIGH
**LOC:** ~150

**Description:** Industry-standard Handlebars template provider

**Why Handlebars:**
- ✅ Industry standard (used by Ember.js, Ghost, etc.)
- ✅ Logic-less templates (clean separation)
- ✅ Wide adoption and documentation
- ✅ NuGet package available: `Handlebars.Net`

**Implementation:**
```csharp
public class HandlebarsTemplateProvider : ITemplateProvider
{
    public IReadOnlyCollection<string> SupportedContentTypes => new[]
    {
        "text/x-handlebars-template",
        "application/x-handlebars"
    };

    public bool CanApply(ITemplateContext context)
    {
        return context.ContentType?.Contains("handlebars") ?? false;
    }

    public async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target)
    {
        var templateContent = await context.Source.GetContentAsync();
        var template = Handlebars.Compile(templateContent);
        var result = template(data);

        await using var writer = new StreamWriter(target, leaveOpen: true);
        await writer.WriteAsync(result);
        return true;
    }
}
```

**Usage:**
```csharp
// Template file: order-confirmation.hbs (ContentType: text/x-handlebars-template)
Hello {{Customer/FirstName}},

Your order #{{Order/OrderNumber}} totaling ${{Order/Total}} has been confirmed.

{{#each Order/LineItems}}
  - {{ProductName}}: {{Quantity}} x ${{UnitPrice}}
{{/each}}

// Apply template
var result = await _templateEngine.ApplyAsync("order-confirmation", dataContainer);
```

**Documentation:**
- [Requirements](./HandlebarsProvider/requirements.md)
- [Architecture](./HandlebarsProvider/architecture.md)
- [API Design](./HandlebarsProvider/api-design.md)
- [Testing Strategy](./HandlebarsProvider/testing-strategy.md)

---

### Feature 2: Database Template Source (NEW)
**Path:** `./DatabaseTemplateSource/`
**Priority:** MEDIUM
**LOC:** ~200

**Description:** Store templates in database instead of files

**Why Database Storage:**
- ✅ Dynamic template management (edit without deployment)
- ✅ Versioning and audit trail
- ✅ Multi-tenant template isolation
- ✅ Template search and indexing

**Implementation:**
```csharp
public class DatabaseTemplateSource : ITemplateSource
{
    private readonly ITemplateRepository _repository;

    public IEnumerable<ITemplateContext> GetTemplates()
    {
        var templates = _repository.GetAll();

        return templates.Select(t => new TemplateContext
        {
            Name = t.Name,
            ContentType = t.ContentType,
            Version = t.Version,
            Source = new DatabaseTemplateContentSource(t.Id, _repository)
        });
    }
}

public class DatabaseTemplateContentSource : ITemplateContentSource
{
    private readonly int _templateId;
    private readonly ITemplateRepository _repository;

    public async Task<string> GetContentAsync()
    {
        var template = await _repository.GetByIdAsync(_templateId);
        return template.Content;
    }
}
```

**Database Schema:**
```sql
CREATE TABLE Templates (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(200) NOT NULL,
    ContentType NVARCHAR(100) NOT NULL,
    Content NVARCHAR(MAX) NOT NULL,
    Version INT NOT NULL DEFAULT 1,
    Culture NVARCHAR(10) NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    IsActive BIT NOT NULL DEFAULT 1,

    INDEX IX_Templates_Name_Culture (Name, Culture)
);
```

**Documentation:**
- [Requirements](./DatabaseTemplateSource/requirements.md)
- [Architecture](./DatabaseTemplateSource/architecture.md)
- [API Design](./DatabaseTemplateSource/api-design.md)

---

### Feature 3: IDataContainer Integration (NEW)
**Path:** `./DataContainerIntegration/`
**Priority:** HIGH
**LOC:** ~100

**Description:** Bridge between IDataContainer (Epic 11) and template providers for lazy evaluation

**Why Integration:**
- ✅ Templates work with lazy-evaluated data containers
- ✅ Only load data paths template actually uses
- ✅ Massive performance improvement for large datasets

**Implementation:**
```csharp
public class DataContainerAdapter
{
    /// <summary>
    /// Converts IDataContainer to object graph that template providers can use.
    /// Preserves lazy evaluation - paths evaluated as template engine accesses them.
    /// </summary>
    public static object AdaptForTemplate(IDataContainer container)
    {
        // Returns dynamic proxy that evaluates paths on access
        return new LazyDataProxy(container);
    }
}

// Used by template engine
public class TemplateEngine : ITemplateEngine
{
    public async Task<string?> ApplyAsync(string templateName, IDataContainer data)
    {
        var adapted = DataContainerAdapter.AdaptForTemplate(data);
        return await ApplyAsync(templateName, adapted);
    }
}
```

**Example:**
```csharp
// Build data container with providers
var container = DataContainerFactory.Create(new { OrderId = 12345 });
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Order/LineItems", lineItemsProvider);

// Template only uses customer name
var template = "Hello {{Customer/FirstName}}!";

// Apply template (ONLY customerProvider executes)
var result = await _templateEngine.ApplyAsync("welcome-email", container);
```

**Documentation:**
- [Requirements](./DataContainerIntegration/requirements.md)
- [Architecture](./DataContainerIntegration/architecture.md)

---

### Feature 4: Template Caching Enhancements (OPTIONAL)
**Path:** `./TemplateCaching/`
**Priority:** LOW
**LOC:** ~100

**Description:** Enhanced caching for compiled templates

**Why Caching:**
- ✅ Handlebars.Compile() is expensive (should cache compiled templates)
- ✅ XSLT compilation is expensive
- ✅ Reduce load on database/file system

**Implementation:**
```csharp
public class CachedTemplateProvider : ITemplateProvider
{
    private readonly ITemplateProvider _innerProvider;
    private readonly IMemoryCache _cache;

    public async Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target)
    {
        var cacheKey = $"template:{context.Name}:{context.Version}";

        var compiledTemplate = await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
            return await CompileTemplateAsync(context);
        });

        return await ApplyCompiledTemplateAsync(compiledTemplate, data, target);
    }
}
```

---

## Additional Providers (Optional - Future)

### Liquid Template Provider
**NuGet:** `Fluid` or `DotLiquid`
**Use Case:** Shopify-style templates, Jekyll compatibility
**Priority:** LOW

```csharp
public class LiquidTemplateProvider : ITemplateProvider
{
    public IReadOnlyCollection<string> SupportedContentTypes => new[] { "text/x-liquid" };
    // ...
}
```

### Scriban Template Provider
**NuGet:** `Scriban`
**Use Case:** High-performance templates, Liquid-compatible syntax
**Priority:** LOW

```csharp
public class ScribanTemplateProvider : ITemplateProvider
{
    public IReadOnlyCollection<string> SupportedContentTypes => new[] { "text/x-scriban" };
    // ...
}
```

### Razor Template Provider
**NuGet:** `RazorLight` or `RazorEngine`
**Use Case:** C# expressions in templates, strong typing
**Priority:** LOW (security concerns - code execution)

---

## What We're NOT Doing

### ❌ Custom HTML Template Syntax
**Reason:** Handlebars and XSLT already cover HTML scenarios
**Decision:** Use industry standards, not custom syntax

### ❌ Custom Template Language
**Reason:** Reinventing the wheel, no benefit over Handlebars/Liquid
**Decision:** Use proven, documented template languages

### ❌ Replacing Existing Template Engine
**Reason:** Current engine is solid with good architecture
**Decision:** Extend, don't replace

---

## Integration with Other Epics

### Epic 11: Data Enhancement Pipeline
```csharp
// Build lazy data container
var container = DataContainerFactory.Create(new { OrderId = orderId });
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);

// Apply template (lazy evaluation)
var html = await _templateEngine.ApplyAsync("order-confirmation", container);
```

### Epic 2: Communications Platform
```csharp
public class MessageCompositionService
{
    public async Task<IEmailMessage> ComposeEmailAsync(string messageType, IDataContainer data)
    {
        // Render email using template engine + lazy data container
        var subject = await _templateEngine.ApplyAsync($"{messageType}.subject", data);
        var htmlContent = await _templateEngine.ApplyAsync($"{messageType}.html", data);
        var textContent = await _templateEngine.ApplyAsync($"{messageType}.text", data);

        return new EmailMessage
        {
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent
        };
    }
}
```

### Epic 6: Document Management
```csharp
// Generate PDF from template
var htmlContent = await _templateEngine.ApplyAsync("invoice-template", invoiceData);
var pdf = await _pdfConverter.ConvertHtmlToPdfAsync(htmlContent);
```

---

## Template Organization

### File Structure (FileTemplateSource)
```
Templates/
├── email/
│   ├── order-confirmation.subject.hbs
│   ├── order-confirmation.html.hbs
│   ├── order-confirmation.text.hbs
│   ├── password-reset.subject.hbs
│   ├── password-reset.html.hbs
│   └── password-reset.text.hbs
├── pdf/
│   ├── invoice.xslt
│   └── receipt.xslt
└── sms/
    ├── order-shipped.hbs
    └── password-reset.hbs
```

### ContentType Conventions
```
File Extension → ContentType → Provider
.hbs         → text/x-handlebars-template → HandlebarsTemplateProvider
.xslt        → application/xslt+xml       → XsltTemplateProvider
.liquid      → text/x-liquid              → LiquidTemplateProvider
.scriban     → text/x-scriban             → ScribanTemplateProvider
```

---

## Success Metrics

- ✅ Handlebars provider works with existing ITemplateEngine
- ✅ Database template source allows dynamic template management
- ✅ IDataContainer integration preserves lazy evaluation
- ✅ Template caching reduces compilation overhead by 90%+
- ✅ 80%+ test coverage for new providers
- ✅ No breaking changes to existing template engine
- ✅ Documentation for adding new providers

---

## Dependencies

### OoBDev Framework (Existing)
- **OoBDev.System.Abstractions** - `ITemplateEngine`, `ITemplateProvider`, `ITemplateSource`
- **OoBDev.System** - `TemplateEngine` implementation, `XsltTemplateProvider`

### New Dependencies
- **Handlebars.Net** - Handlebars template compilation
- **Fluid** OR **DotLiquid** - Liquid templates (optional)
- **Scriban** - High-performance templates (optional)

### Integration Dependencies
- **Epic 11: Data Enhancement Pipeline** - `IDataContainer` for lazy evaluation

---

## Migration from SharedFramework

**SharedFramework TextTemplating (~550 LOC):**
- Template loading and storage
- Template metadata management
- Some custom template logic

**Action:**
- ✅ **Keep:** Template storage concepts
- ✅ **Migrate:** Database template source
- ❌ **Discard:** Custom template syntax (use Handlebars instead)
- ✅ **Integrate:** Leverage existing OoBDev template engine

---

## Implementation Priority

### Phase 1: Essential (Week 1)
1. **Handlebars Provider** - Industry-standard template support
2. **IDataContainer Integration** - Lazy evaluation support

### Phase 2: Enhanced Storage (Week 2)
3. **Database Template Source** - Dynamic template management
4. **Template Caching** - Performance optimization

### Phase 3: Optional (Future)
5. Liquid Provider (if Shopify compatibility needed)
6. Scriban Provider (if high performance needed)
7. Azure Blob Template Source (if cloud storage needed)

---

## Related Documentation

- [Epic Review](../EPIC_REVIEW.md)
- [Epic 11: Data Enhancement Pipeline](../11-DataEnhancement/README-REVISED.md)
- [Existing Template Engine](../../../Framework/OoBDev.System/Text/Templating/)

---

## Next Steps

1. Implement Handlebars provider following ITemplateProvider pattern
2. Create IDataContainer adapter for lazy evaluation
3. Implement database template source
4. Add template caching wrapper
5. Document provider creation guide for future engines
