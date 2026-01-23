# Epic 11: Data Enhancement Pipeline

**Priority:** HIGH
**Status:** 📋 Design Phase
**Complexity:** MEDIUM
**Impact:** ~400 LOC (reusable across multiple features)

---

## Overview

The Data Enhancement Pipeline is a **generic, reusable framework** for enriching data through attribute-discovered providers. It provides a clean separation between data loading/enrichment and data consumption.

**Key Principle:** This is a **cross-cutting concern** used by multiple features (Communications, Templating, Reporting, Document Generation, etc.). It has **no knowledge** of emails, SMS, templates, or any specific domain.

---

## Business Problem

**Current State:** Features like Communications have data enhancement logic tightly coupled within them.

**Problems:**
- Cannot reuse enhancement logic across features
- Template engine can't benefit from same enhancement providers
- Reporting features duplicate enhancement code
- Testing is harder (can't test enhancement in isolation)

**Desired State:**
- **One enhancement pipeline** used by all features
- Domain services register enhancement providers once
- Templating, Communications, Reporting all use same enriched data
- Clear separation: Enhancement → Composition → Delivery

---

## Use Cases

### Use Case 1: Order Confirmation Email
```
OrderService
    ↓
Calls: MessageCompositionService.ComposeEmailAsync(
    messageType: "order.confirmation",
    data: { OrderId: 12345 }
)
    ↓
MessageCompositionService uses:
    ├─ DataEnhancementPipeline.EnhanceAsync() → { OrderId, LineItems, Customer, Total }
    ├─ TemplateEngine.LoadTemplate() → HTML template
    ├─ TemplateEngine.Render() → Formatted email
    └─ Returns: IEmailMessage (ready to send)
    ↓
CommunicationsService.SendAsync(emailMessage)
```

### Use Case 2: Report Generation
```
ReportingService
    ↓
Calls: DataEnhancementPipeline.EnhanceAsync(
    context: "monthly-sales-report",
    data: { Month: "January", Year: 2026 }
)
    ↓
Pipeline discovers SalesReportEnhancementProvider
    ↓
Provider loads: { TotalSales, TopProducts, CustomerCount }
    ↓
ReportingService uses enriched data to generate PDF
```

### Use Case 3: Template Preview (Admin UI)
```
TemplateEditorUI
    ↓
Calls: DataEnhancementPipeline.GetSampleDataAsync("order.confirmation")
    ↓
Pipeline generates sample enhanced data
    ↓
UI shows preview of template with sample data
```

---

## Architecture Components

### IMessageData
**Layer:** Framework (OoBDev.DataEnhancement.Abstractions)
**Purpose:** Type-safe, path-navigable data container

```csharp
public interface IMessageData
{
    T? GetValue<T>(string path);
    void SetValue(string path, object? value);
    bool TryGetValue<T>(string path, out T? value);
    bool ContainsPath(string path);
    IMessageData Clone();
    IDictionary<string, object?> ToDictionary();
    string ToJson();
}
```

### IDataEnhancementPipeline
**Layer:** Framework (OoBDev.DataEnhancement.Abstractions)
**Purpose:** Main pipeline orchestrator

```csharp
public interface IDataEnhancementPipeline
{
    /// <summary>
    /// Enhances data by discovering and executing registered providers.
    /// </summary>
    /// <param name="context">Enhancement context (e.g., "order.confirmation")</param>
    /// <param name="data">Data to enhance</param>
    /// <param name="metadata">Optional metadata (correlation ID, user ID, etc.)</param>
    Task<IMessageData> EnhanceAsync(
        string context,
        IMessageData data,
        IDictionary<string, object?>? metadata = null);

    /// <summary>
    /// Seeds initial data with metadata.
    /// </summary>
    IMessageData SeedData(
        IMessageData data,
        params (string Key, object? Value)[] metadata);

    /// <summary>
    /// Gets sample data for a context (for previews, testing).
    /// </summary>
    Task<IMessageData> GetSampleDataAsync(string context);
}
```

### IDataEnhancementProvider
**Layer:** Framework (OoBDev.DataEnhancement.Abstractions)
**Purpose:** Domain-specific enhancement logic

```csharp
public interface IDataEnhancementProvider
{
    /// <summary>
    /// Enhances data with additional context.
    /// </summary>
    /// <param name="context">Enhancement context (e.g., "order.confirmation")</param>
    /// <param name="data">Current data (may be modified)</param>
    /// <param name="metadata">Optional metadata</param>
    /// <returns>Enhanced data</returns>
    Task<IMessageData> EnhanceAsync(
        string context,
        IMessageData data,
        IDictionary<string, object?>? metadata = null);
}
```

### [EnhancementContext] Attribute
**Layer:** Framework (OoBDev.DataEnhancement.Abstractions)
**Purpose:** Marks providers for discovery

```csharp
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class EnhancementContextAttribute : Attribute
{
    /// <summary>
    /// Context identifier this provider handles (e.g., "order.confirmation")
    /// </summary>
    public string Context { get; set; } = "";

    /// <summary>
    /// Execution order (lower = earlier)
    /// </summary>
    public int Order { get; set; } = 0;
}
```

---

## Feature Breakdown

### Feature 1: Core Pipeline
**Path:** `./CorePipeline/`
**Description:** Main enhancement orchestration engine

**Documentation:**
- [Requirements](./CorePipeline/requirements.md)
- [Architecture](./CorePipeline/architecture.md)
- [API Design](./CorePipeline/api-design.md)
- [Business Rules](./CorePipeline/business-rules.md)
- [Configuration](./CorePipeline/configuration.md)
- [Testing Strategy](./CorePipeline/testing-strategy.md)

### Feature 2: Provider Discovery
**Path:** `./ProviderDiscovery/`
**Description:** Attribute-based provider discovery and registration

**Documentation:**
- [Requirements](./ProviderDiscovery/requirements.md)
- [Architecture](./ProviderDiscovery/architecture.md)
- [API Design](./ProviderDiscovery/api-design.md)

---

## Example Providers

### Order Enhancement Provider
```csharp
[EnhancementContext(Context = "order.confirmation", Order = 1)]
[EnhancementContext(Context = "order.shipped", Order = 1)]
public class OrderEnhancementProvider : IDataEnhancementProvider
{
    private readonly IOrderRepository _orders;

    public async Task<IMessageData> EnhanceAsync(
        string context,
        IMessageData data,
        IDictionary<string, object?>? metadata = null)
    {
        var orderId = data.GetValue<int>("OrderId");
        var order = await _orders.GetByIdAsync(orderId);

        // Enrich with order details
        data.SetValue("Order.Total", order.Total);
        data.SetValue("Order.LineItems", order.LineItems);
        data.SetValue("Order.EstimatedDelivery", order.EstimatedDeliveryDate);

        // Enrich with customer info
        data.SetValue("Customer.Email", order.Customer.Email);
        data.SetValue("Customer.FirstName", order.Customer.FirstName);

        return data;
    }
}
```

### User Enhancement Provider
```csharp
[EnhancementContext(Context = "order.confirmation", Order = 0)]  // Runs BEFORE Order provider
[EnhancementContext(Context = "user.welcome", Order = 0)]
public class UserEnhancementProvider : IDataEnhancementProvider
{
    private readonly IUserRepository _users;

    public async Task<IMessageData> EnhanceAsync(
        string context,
        IMessageData data,
        IDictionary<string, object?>? metadata = null)
    {
        // Get userId from data or metadata
        Guid userId = data.ContainsPath("UserId")
            ? data.GetValue<Guid>("UserId")
            : metadata?["UserId"] as Guid? ?? Guid.Empty;

        var user = await _users.GetByIdAsync(userId);

        data.SetValue("User.Email", user.Email);
        data.SetValue("User.FirstName", user.FirstName);
        data.SetValue("User.LastName", user.LastName);
        data.SetValue("User.Culture", user.PreferredCulture);
        data.SetValue("User.Timezone", user.Timezone);

        return data;
    }
}
```

---

## Usage by Consumers

### Message Composition Service
```csharp
public class MessageCompositionService
{
    private readonly IDataEnhancementPipeline _enhancement;
    private readonly ITemplateEngine _templates;

    public async Task<IEmailMessage> ComposeEmailAsync(
        string messageType,
        IMessageData data,
        Guid userId)
    {
        // 1. Enhance data
        var metadata = new Dictionary<string, object?> { ["UserId"] = userId };
        var enrichedData = await _enhancement.EnhanceAsync(messageType, data, metadata);

        // 2. Get user culture from enriched data
        var culture = enrichedData.GetValue<CultureInfo>("User.Culture") ?? CultureInfo.CurrentCulture;

        // 3. Load template
        var template = await _templates.GetEmailTemplateAsync(messageType, culture);

        // 4. Render template with enriched data
        var subject = _templates.Render(template.Subject, enrichedData);
        var htmlContent = _templates.Render(template.HtmlContent, enrichedData);
        var textContent = _templates.Render(template.TextContent, enrichedData);

        // 5. Build email message (ready to send)
        return new EmailMessage
        {
            ToAddress = enrichedData.GetValue<string>("User.Email"),
            Subject = subject,
            HtmlContent = htmlContent,
            TextContent = textContent,
            MessageType = messageType,
            RequestId = Guid.NewGuid()
        };
    }
}
```

### Reporting Service
```csharp
public class ReportingService
{
    private readonly IDataEnhancementPipeline _enhancement;

    public async Task<byte[]> GenerateSalesReportAsync(int month, int year)
    {
        var data = MessageDataFactory.Create(new Dictionary<string, object?>
        {
            ["Month"] = month,
            ["Year"] = year
        });

        // Enhance with sales data
        var enrichedData = await _enhancement.EnhanceAsync("monthly-sales-report", data);

        // Generate PDF using enriched data
        return GeneratePdf(enrichedData);
    }
}
```

---

## Key Design Decisions

### 1. Generic "Context" Instead of "MessageType"
**Decision:** Use generic `context` parameter instead of domain-specific names

**Rationale:**
- Not all enhancements are for messages (reports, exports, etc.)
- More reusable across features
- Clearer separation of concerns

**Examples:**
- `"order.confirmation"` - Email/SMS message context
- `"monthly-sales-report"` - Reporting context
- `"invoice.pdf"` - Document generation context
- `"customer.export"` - Data export context

### 2. Order-Based Execution
**Decision:** Providers execute in order defined by `Order` property

**Rationale:**
- UserEnhancementProvider runs first (Order=0) to get user culture
- OrderEnhancementProvider runs second (Order=1) to load order details
- Allows providers to build on each other's data

### 3. Metadata Dictionary
**Decision:** Support optional metadata dictionary alongside data

**Rationale:**
- Correlation IDs, user IDs, request timestamps can be in metadata
- Keeps actual data clean
- Providers can access both data and metadata

### 4. Sample Data for Testing/Previews
**Decision:** Pipeline supports `GetSampleDataAsync(context)`

**Rationale:**
- Template editors need sample data for previews
- Testing doesn't require real database data
- Providers can return mock data for their context

---

## Consumers of This Pipeline

1. **Message Composition** - Enrich data before rendering templates
2. **Communications** - (Optional) Can use for simple scenarios
3. **Reporting** - Enrich report parameters with full data
4. **Document Generation** - Enrich data before generating PDFs/Word docs
5. **Data Export** - Enrich records before CSV/Excel export
6. **Template Preview** - Show sample data in admin UI

---

## Success Metrics

- ✅ No dependencies on Communications, Templating, or any specific domain
- ✅ Used by 3+ different features (Communications, Templating, Reporting)
- ✅ Providers registered once, used everywhere
- ✅ 80%+ test coverage
- ✅ < 100ms enhancement time (excluding provider I/O)

---

## Dependencies

### OoBDev Framework
- None (this IS a framework component)

### External
- System.Text.Json (built-in)
- Microsoft.Extensions.DependencyInjection
- Microsoft.Extensions.Logging

---

## Related Documentation

- [SharedFramework Analysis](../../docs/migration/sharedframework-feature-mapping.md)
- Epic 2: Communications Platform (consumer)
- Epic 10: Text Templating (consumer)

---

## Next Steps

1. Create detailed documentation for CorePipeline feature
2. Create detailed documentation for ProviderDiscovery feature
3. Update Communications Platform to be a pure channel router
4. Create Message Composition Service that uses both pipelines
