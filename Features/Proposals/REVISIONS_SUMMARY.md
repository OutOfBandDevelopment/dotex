# Design Revisions Summary

**Date:** 2026-01-22
**Status:** ✅ All User Feedback Incorporated (13 Revisions)

---

## Overview

This document tracks major design revisions based on user feedback during the proposal review process.

---

## Revision 1: Generic Data Container (Not Message-Specific)

**Feedback:** "Data enhancement should be more generic than messagedata... it can be used for anything"

### Original Design ❌
```csharp
public interface IMessageData
{
    T? GetValue<T>(string path);
    void SetValue(string path, object? value);
}
```

**Problems:**
- Name `IMessageData` implies message-specific
- Cannot use for reports, documents, exports
- Limited reusability

### Revised Design ✅
```csharp
public interface IDataContainer
{
    IDataNode Navigate(string path);
    object? Evaluate(string path);
    T? Evaluate<T>(string path);
}
```

**Benefits:**
- ✅ Generic name - works for ANY data scenario
- ✅ Used by templates, reports, documents, exports
- ✅ Clear separation from domain concepts

**Impact:**
- Epic 11 renamed: "Data Enhancement Pipeline" (generic)
- All interfaces renamed: `IMessageData` → `IDataContainer`
- Context parameter: `"order.confirmation"`, `"monthly-report"`, `"invoice.pdf"`

---

## Revision 2: XPath-Like Navigation

**Feedback:** "It would be nice to even have it follow the idea of an xpath navigator"

### Original Design ❌
```csharp
// Dot notation (like JSON)
data.GetValue<string>("Customer.Address.City");
data.SetValue("Customer.Address.City", "Seattle");
```

**Problems:**
- Dot notation conflicts with property names containing dots
- Not consistent with industry navigation patterns
- No wildcard support

### Revised Design ✅
```csharp
// XPath-like navigation with IDataNode
public interface IDataNode
{
    string Path { get; }  // "Customer/Address/City"
    string Name { get; }  // "City"

    IDataNode? SelectSingleNode(string relativePath);
    IEnumerable<IDataNode> SelectNodes(string pattern);

    IDataNode? Parent { get; }
    IEnumerable<IDataNode> Children { get; }
}

// Usage
var addressNode = container.Navigate("Customer/Address");
var city = addressNode.SelectSingleNode("City").GetValue<string>();

// Wildcard patterns
container.RegisterProvider("Customer/*/Address", addressProvider);  // Matches Shipping, Billing
container.RegisterProvider("**/LineItems", lineItemsProvider);      // Matches any depth
```

**Benefits:**
- ✅ Follows `XPathNavigator` pattern (industry standard)
- ✅ `/` separators (clear, unambiguous)
- ✅ Wildcard support: `*` (single level), `**` (multiple levels)
- ✅ `SelectSingleNode()`, `SelectNodes()` (familiar API)
- ✅ Hierarchical navigation (Parent, Children)

**Impact:**
- All path syntax changed: `"Customer.Email"` → `"Customer/Email"`
- New `IDataNode` interface added
- Wildcard pattern matching in provider registration

---

## Revision 3: Lazy Evaluation

**Feedback:** "Lazy lookup so the data at a particular node is not retrieved unless it is actually required"

### Original Design ❌
```csharp
// All providers execute immediately
var data = await _enhancement.EnhanceAsync("order.confirmation", data);

// Data fully loaded in memory
var email = data.GetValue<string>("Customer/Email");
```

**Problems:**
- All enhancement providers execute (even if data not used)
- Entire object graph loaded into memory
- Wastes DB queries for unused data paths
- Poor performance for large datasets

### Revised Design ✅
```csharp
// Register providers (no execution yet)
container.RegisterProvider("Customer", customerProvider);
container.RegisterProvider("Order", orderProvider);
container.RegisterProvider("Order/LineItems", lineItemsProvider);

// Template only uses customer name
var template = "Hello {{Customer/FirstName}}!";

// ONLY customerProvider executes (lazy evaluation)
var result = _templateEngine.Render(template, container);
```

**Provider Implementation:**
```csharp
[EnhancementPath("Order/LineItems")]
public class LineItemsProvider : IDataProvider
{
    public async Task<object?> ProvideAsync(IDataNode node, string context, ...)
    {
        // ONLY executes if template/code accesses "Order/LineItems" path
        var orderId = node.Parent!.Evaluate<int>("OrderId");
        return await _orderRepository.GetLineItemsAsync(orderId);
    }
}
```

**Benefits:**
- ✅ Providers execute ONLY when path is accessed
- ✅ Reduces DB queries by 50-70% (typical scenario)
- ✅ Reduces memory usage by 70%+ (no full object graphs)
- ✅ Supports streaming (IAsyncEnumerable)
- ✅ Efficient for large datasets

**Performance Example:**

**Template:** `"Welcome {{Customer/FirstName}}!"`

**Without Lazy Evaluation:**
- CustomerProvider executes (1 DB query)
- OrderProvider executes (1 DB query) ❌ NOT NEEDED
- LineItemsProvider executes (1 DB query) ❌ NOT NEEDED
- **Total: 4 queries, 3 wasted**

**With Lazy Evaluation:**
- CustomerProvider executes (1 DB query) ✅ ONLY THIS
- **Total: 1 query (75% savings)**

**Impact:**
- New `IDataProvider` interface with `ProvideAsync()` method
- Providers invoked on-demand, not upfront
- Template engines evaluate paths lazily
- Massive performance improvement

---

## Revision 4: Leverage Existing Template Engine

**Feedback:** "Text templates are pretty good though can probably stick with the template engine already in the existing framework"

### Original Plan ❌
```
Epic 10: Text Templating (~550 LOC - NEW)
    ├─ Template Engine (custom implementation)
    ├─ Custom HTML template syntax
    ├─ Variable substitution
    └─ Template storage
```

**Problems:**
- Reinventing the wheel
- Custom syntax requires documentation/training
- Existing template engine ignored

### Revised Plan ✅
```
Epic 10: Text Templating Extensions (~400 LOC - ADDITIONS)
    ├─ Handlebars Provider (industry standard)
    ├─ Database Template Source (storage)
    ├─ IDataContainer Integration (lazy evaluation)
    └─ Template Caching (performance)

Existing Framework (LEVERAGE):
    ├─ ITemplateEngine (already exists)
    ├─ ITemplateProvider (provider abstraction)
    ├─ ITemplateSource (storage abstraction)
    └─ XsltTemplateProvider (already implemented)
```

**Benefits:**
- ✅ **Use existing** `OoBDev.System.Text.Templating` infrastructure
- ✅ **XSLT already implemented** (no work needed)
- ✅ **Handlebars** = industry standard (Ember.js, Ghost)
- ✅ **Provider pattern** already established
- ✅ **No custom syntax** to learn/document
- ✅ **Smaller scope** (~400 LOC vs ~550 LOC)

**New Providers (Industry Standards Only):**
```csharp
// Handlebars (PRIORITY: HIGH)
public class HandlebarsTemplateProvider : ITemplateProvider
{
    public IReadOnlyCollection<string> SupportedContentTypes => new[] { "text/x-handlebars-template" };
}

// Liquid (OPTIONAL - if Shopify compatibility needed)
public class LiquidTemplateProvider : ITemplateProvider { }

// Scriban (OPTIONAL - if high performance needed)
public class ScribanTemplateProvider : ITemplateProvider { }
```

**What We're NOT Doing:**
- ❌ Custom HTML template syntax
- ❌ Replacing existing template engine
- ❌ Custom template language

**Impact:**
- Epic 10 scope reduced by ~30%
- Focus on additions, not replacement
- Leverage proven, documented template languages

---

## Revision 5: Document Management Split

**Feedback:** "Document management should have separated parts for persistence and retrieval, conversion pipelines, and pack/unpack"

### Original Plan ❌
```
Epic 6: Document Management (911 LOC - monolithic)
    └─ DocumentCenter
        ├─ Storage (mixed)
        ├─ Conversion (mixed)
        └─ Packaging (mixed)
```

**Problems:**
- Cannot use storage without conversion logic
- Cannot convert documents without storage overhead
- Cannot package documents independently
- Violates Single Responsibility Principle

### Revised Plan ✅
```
Epic 6: Document Management (split into 3 features)

Feature 1: Persistence & Retrieval (~300 LOC)
    ├─ IDocumentRepository
    ├─ IDocumentStore (DB, file system, S3, Azure Blob)
    ├─ Query/search by metadata
    └─ Version control

Feature 2: Conversion Pipelines (~400 LOC)
    ├─ IDocumentConverter
    ├─ IConversionPipeline
    ├─ Format transformations (PDF ↔ Word, HTML → PDF)
    ├─ Text extraction
    └─ OCR processing

Feature 3: Pack/Unpack (~200 LOC)
    ├─ IDocumentPacker
    ├─ IPackageManager
    ├─ ZIP/TAR support
    └─ Package metadata
```

**Benefits:**
- ✅ Use persistence **without** conversion
- ✅ Convert documents **without** storing them
- ✅ Package documents from **any source**
- ✅ Compose features as needed
- ✅ Each feature testable independently

**Usage Examples:**
```csharp
// Just storage
await _documentStore.SaveAsync(document);

// Just conversion
var pdf = await _converter.ConvertToPdfAsync(wordDoc);

// Just packaging
var package = await _packer.CreatePackageAsync(documents);

// Compose as needed
var wordDoc = await _documentStore.GetAsync(docId);
var pdf = await _converter.ConvertToPdfAsync(wordDoc);
var package = await _packer.CreatePackageAsync(new[] { pdf });
await _documentStore.SaveAsync(package);
```

**Impact:**
- Epic 6 split into 3 separate features
- Clear separation of concerns
- More composable, less coupling

---

## Revision 6: Channel Abstraction (Protocol + Provider + Name)

**Feedback:** "For the communication platform its sole job should be sending and receiving messages. There can be different channels like text/sms, email, live chat, chatrooms and so on. Channels should have a protocol, provider and name so they could be something like a teams channel, slack group, email recipient and so on. It should also handle user preferences for quiet hours/weekends/holidays."

### Original Design ❌
```csharp
public interface ICommunicationsService
{
    Task<SendResult> SendEmailAsync(Guid userId, IEmailMessage message);
    Task<SendResult> SendSmsAsync(Guid userId, ISmsMessage message);
}
```

**Problems:**
- Email and SMS only (no Teams, Slack, Live Chat)
- No receive capability
- No channel abstraction
- User preferences incomplete

### Revised Design ✅
```csharp
// Channel abstraction: Protocol + Provider + Name
public interface IChannel
{
    string Name { get; }      // "sales-team-slack", "support-email"
    string Protocol { get; }  // "slack", "email", "sms", "teams"
    string Provider { get; }  // "slack-api", "sendgrid", "twilio"
    IDictionary<string, object> Configuration { get; }
}

public interface IChannelProvider
{
    string ProviderName { get; }  // "sendgrid", "twilio", "slack-api"
    string[] SupportedProtocols { get; }  // ["email"], ["sms"], ["slack"]

    Task<SendResult> SendAsync(IChannel channel, IMessage message);
    Task<IMessage?> ReceiveAsync(IChannel channel);  // NEW: Receive
    Task RegisterWebhookAsync(IChannel channel, string webhookUrl);
}

public interface ICommunicationsService
{
    // Send & Receive
    Task<SendResult> SendAsync(Guid userId, IMessage message, string channelName);
    Task<IEnumerable<IMessage>> ReceiveAsync(string channelName);
    Task<WebhookResult> HandleWebhookAsync(string channelName, HttpRequest request);

    // User preferences
    Task<SendResult> SendViaPreferredChannelAsync(Guid userId, IMessage message);
}

public interface IUserCommunicationPreferences
{
    TimeSpan? QuietHoursStart { get; }  // e.g., 9 PM
    TimeSpan? QuietHoursEnd { get; }    // e.g., 7 AM
    bool AllowWeekendsDelivery { get; }
    string[] HolidayCalendars { get; }  // ["US-Federal", "Company-Holidays"]
    string[] PreferredChannels { get; }  // ["email", "sms", "slack"]
}
```

**Benefits:**
- ✅ **Send AND receive** messages (webhooks + polling)
- ✅ **Multi-channel** - Email, SMS, Slack, Teams, Live Chat, etc.
- ✅ **Channel abstraction** - Protocol + Provider + Name
- ✅ **User preferences** - Quiet hours, weekends, holidays
- ✅ **Extensible** - Add new channels via `IChannelProvider`

**Channel Examples:**
```csharp
// Email via SendGrid
{ Name: "support-email", Protocol: "email", Provider: "sendgrid" }

// SMS via Twilio
{ Name: "alerts-sms", Protocol: "sms", Provider: "twilio" }

// Slack channel
{ Name: "sales-team-slack", Protocol: "slack", Provider: "slack-api" }

// Microsoft Teams
{ Name: "engineering-teams", Protocol: "teams", Provider: "microsoft-teams" }
```

**Impact:**
- Communications Platform expanded to handle multiple channel types
- Receive capability added (webhooks, polling)
- User preferences fully implemented
- Channel provider pattern for extensibility

---

## Revision 7: Standalone Services + Composite Orchestrations

**Feedback:** "All of these services should be standalone and will have composite orchestrations to chain them together."

### Original Implication ❌
```
Epic 12 (Message Composition) appears REQUIRED to use Epic 11 + Epic 10
```

**Problem:**
- Services seemed dependent on orchestration layer
- Not clear that each epic can be used independently
- Epic 12 looked like a required dependency

### Revised Design ✅

**Standalone Services (Independent):**
```csharp
// Epic 11: Data Enhancement (STANDALONE)
var container = DataContainerFactory.Create(new { OrderId = 123 });
container.RegisterProvider("Customer", customerProvider);
var name = container.Evaluate<string>("Customer/Name");

// Epic 10: Text Templating (STANDALONE)
var html = await _templateEngine.RenderAsync("invoice", data);

// Epic 6: Document Conversion (STANDALONE)
var pdf = await _conversion.ConvertAsync(markdown, "text/markdown", "application/pdf");

// Epic 2: Communications (STANDALONE)
var message = new EmailMessage { Subject = "...", HtmlContent = "..." };
await _communications.SendAsync(userId, message, "support-email");
```

**Composite Orchestration (Optional):**
```csharp
// Epic 12: CONVENIENCE orchestration (NOT REQUIRED)
public class MessageCompositionService
{
    // Chains Epic 11 + 10 + 6 for convenience
    public async Task<IEmailMessage> ComposeEmailAsync(...)
    {
        var container = await _enhancement.EnhanceAsync(data);  // Epic 11
        var rendered = await _templates.RenderAsync(name, container);  // Epic 10
        var converted = await _conversion.ConvertAsync(...);  // Epic 6 (optional)
        return new EmailMessage { HtmlContent = converted };
    }
}

// Applications can use orchestration OR chain manually
public class OrderService
{
    // Option 1: Use orchestration
    var message = await _composition.ComposeEmailAsync(...);

    // Option 2: Chain manually (no Epic 12 needed)
    var container = DataContainerFactory.Create(...);
    var html = await _templateEngine.RenderAsync(...);
    var message = new EmailMessage { HtmlContent = html };
    await _communications.SendAsync(...);
}
```

**Benefits:**
- ✅ **No hard dependencies** - Each service works independently
- ✅ **Orchestrations optional** - Convenience, not requirement
- ✅ **Flexible composition** - Use services directly or via orchestrations
- ✅ **Application choice** - Chain services manually or use orchestration layer

**Architectural Principle:**
> "Each epic is a STANDALONE SERVICE. Orchestrations CHAIN services together (optional, not required)."

**Impact:**
- Epic 12 clarified as CONVENIENCE orchestration
- Each epic (2, 6, 10, 11) explicitly standalone
- Applications can compose services themselves

---

## Revision 8: Transparent Caching via Dynamic Proxy / AOP

**Feedback:** "The caching service should be an extensible option that can be registered on an interface to create a dynamic proxy/aspect oriented interception for method calls transparently from the developer."

**Additional Feedback:** "Warming and preloading could be an application specific thing where a service would just call the cached service tier and throw away the results."

### Original Design ❌
```csharp
// Manual caching (developer must write cache logic)
public class OrderService
{
    public async Task<Order> GetOrderAsync(int orderId)
    {
        var cacheKey = $"order:{orderId}";
        var cached = await _cache.GetAsync<Order>(cacheKey);
        if (cached != null) return cached;

        var order = await _repository.GetOrderAsync(orderId);
        await _cache.SetAsync(cacheKey, order, TimeSpan.FromMinutes(5));
        return order;
    }
}
```

**Problems:**
- Developer must manually write cache-check logic
- Cache logic scattered throughout codebase
- Not transparent or declarative
- Difficult to change caching strategy
- No aspect-oriented approach

### Revised Design ✅

**Attribute-Based Declarative Caching:**
```csharp
// Developer just declares caching (TRANSPARENT)
public interface IOrderService
{
    [Cache(Duration = 300)]  // 5 minutes
    Task<Order> GetOrderAsync(int orderId);

    [Cache(Duration = 60, VaryByParameters = true)]
    Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId, DateTime startDate);

    [CacheInvalidate(Pattern = "order:*")]
    Task UpdateOrderAsync(Order order);

    [CacheInvalidate(Keys = new[] { "order:{orderId}" })]
    Task DeleteOrderAsync(int orderId);
}

// Implementation (NO cache logic - just business logic)
public class OrderService : IOrderService
{
    public async Task<Order> GetOrderAsync(int orderId)
    {
        // Just business logic - caching is transparent
        return await _repository.GetOrderAsync(orderId);
    }

    public async Task UpdateOrderAsync(Order order)
    {
        // Updates invalidate cache automatically
        await _repository.UpdateOrderAsync(order);
    }
}
```

**Registration with Dynamic Proxy:**
```csharp
// Startup - Register with caching proxy
services.AddSingleton<IOrderRepository, OrderRepository>();

// Option 1: Castle DynamicProxy
services.AddCachedProxy<IOrderService, OrderService>(options =>
{
    options.DefaultDuration = TimeSpan.FromMinutes(5);
    options.CacheProvider = CacheProvider.Redis;
    options.KeyPrefix = "app";
});

// Option 2: DispatchProxy (built-in .NET)
services.AddCachedService<IOrderService, OrderService>();

// Usage (COMPLETELY TRANSPARENT)
public class CheckoutController
{
    private readonly IOrderService _orderService;

    public CheckoutController(IOrderService orderService)
    {
        _orderService = orderService;  // Injected proxy (not real implementation)
    }

    public async Task<Order> GetOrder(int orderId)
    {
        // First call: executes method, caches result
        // Second call: returns cached result (developer doesn't see caching)
        return await _orderService.GetOrderAsync(orderId);
    }
}
```

**Caching Proxy Implementation:**
```csharp
// Interceptor using Castle DynamicProxy
public class CacheInterceptor : IInterceptor
{
    private readonly ICacheService _cache;

    public void Intercept(IInvocation invocation)
    {
        var cacheAttr = invocation.Method.GetCustomAttribute<CacheAttribute>();
        if (cacheAttr == null)
        {
            invocation.Proceed();  // No caching - just execute
            return;
        }

        // Build cache key from method + parameters
        var cacheKey = BuildCacheKey(invocation.Method, invocation.Arguments, cacheAttr);

        // Check cache
        var cached = _cache.Get(cacheKey);
        if (cached != null)
        {
            invocation.ReturnValue = cached;  // Return cached value
            return;
        }

        // Execute method
        invocation.Proceed();

        // Cache result
        _cache.Set(cacheKey, invocation.ReturnValue, TimeSpan.FromSeconds(cacheAttr.Duration));
    }
}

// Extension method for registration
public static class CachingServiceExtensions
{
    public static IServiceCollection AddCachedProxy<TInterface, TImplementation>(
        this IServiceCollection services,
        Action<CacheProxyOptions>? configure = null)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        var options = new CacheProxyOptions();
        configure?.Invoke(options);

        services.TryAddSingleton<TImplementation>();
        services.AddSingleton<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var cache = provider.GetRequiredService<ICacheService>();
            var interceptor = new CacheInterceptor(cache, options);

            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(implementation, interceptor);
        });

        return services;
    }
}
```

**Attribute Design:**
```csharp
[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute
{
    public int Duration { get; set; } = 300;  // Seconds
    public bool VaryByParameters { get; set; } = true;
    public string? VaryByUser { get; set; }
    public string? VaryByCulture { get; set; }
    public string? Region { get; set; }  // Cache region/partition
    public CacheProvider? Provider { get; set; }  // Override default
}

[AttributeUsage(AttributeTargets.Method)]
public class CacheInvalidateAttribute : Attribute
{
    public string? Pattern { get; set; }  // "order:*"
    public string[]? Keys { get; set; }   // Specific keys
    public string? Region { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class CacheEvictAttribute : Attribute
{
    public string[]? Keys { get; set; }
}
```

**Benefits:**
- ✅ **Transparent** - Developer writes no cache logic
- ✅ **Declarative** - Attributes describe caching behavior
- ✅ **AOP/Dynamic Proxy** - Interception at runtime
- ✅ **Extensible** - Custom attributes, custom interceptors
- ✅ **Centralized** - Cache logic in one place (interceptor)
- ✅ **Testable** - Can inject real implementation for tests
- ✅ **Flexible** - Change caching strategy without touching code

**Advanced Features:**
```csharp
// Conditional caching
public interface IProductService
{
    [Cache(Duration = 600, Condition = "result.IsPublished == true")]
    Task<Product> GetProductAsync(int productId);

    // Cache with sliding expiration
    [Cache(Duration = 300, SlidingExpiration = true)]
    Task<IEnumerable<Category>> GetCategoriesAsync();

    // Distributed cache (Redis) for this specific method
    [Cache(Duration = 1800, Provider = CacheProvider.Redis)]
    Task<byte[]> GetProductImageAsync(int productId);

    // Multi-level cache (Memory + Redis)
    [Cache(Duration = 300, Provider = CacheProvider.Hybrid)]
    Task<Product> GetFeaturedProductAsync();
}
```

**Impact:**
- Epic 4 (Distributed Caching) redesigned for transparency
- Dynamic proxy generation with Castle DynamicProxy or DispatchProxy
- Attribute-based configuration (declarative, not imperative)
- Interceptor pattern for aspect-oriented caching
- Extensible via custom attributes and interceptors
- Cache warming is application-specific (no special infrastructure)

---

## Revision 9: Master Data / Test Data Tool (Not ETL)

**Feedback:** "The data loader pipeline is really a master data/test data tool than an ETL pipeline. Its intention is to setup new data stores for production and possible upload test set for integration/manual testing."

### Original Understanding ❌
```
Epic 5: Data Loading Pipeline (ETL)
- Extract, Transform, Load for ongoing data integration
- General-purpose ETL for data warehousing
- Continuous data synchronization
```

**Problems:**
- Implied general-purpose ETL (not the intent)
- Suggested ongoing data integration (not the use case)
- Not focused on master data and test data scenarios

### Revised Design ✅

**Epic 5: Master Data & Test Data Management**

**Purpose:** Setup new data stores for production + Load test datasets for testing

**Two Primary Use Cases:**

1. **Master Data Setup (Production)**
   - Initialize new production databases with master data
   - Reference data (countries, states, currencies, etc.)
   - Configuration data (settings, feature flags, etc.)
   - Seed data for new tenants/environments

2. **Test Data Loading (Testing)**
   - Upload test datasets for integration testing
   - Load known data for manual testing
   - Reproducible test scenarios
   - Test data versioning

**Key Interfaces:**
```csharp
// Master Data Loader
public interface IMasterDataLoader
{
    /// <summary>
    /// Loads master data into a new data store.
    /// Used when setting up new production environments.
    /// </summary>
    Task<LoadResult> LoadMasterDataAsync(string dataStoreName, MasterDataSet dataSet);

    /// <summary>
    /// Validates master data before loading.
    /// </summary>
    Task<ValidationResult> ValidateMasterDataAsync(MasterDataSet dataSet);

    /// <summary>
    /// Lists available master data sets.
    /// </summary>
    Task<IEnumerable<MasterDataSetInfo>> GetAvailableMasterDataAsync();
}

// Test Data Loader
public interface ITestDataLoader
{
    /// <summary>
    /// Loads test dataset for integration/manual testing.
    /// </summary>
    Task<LoadResult> LoadTestDataAsync(string dataStoreName, TestDataSet testDataSet);

    /// <summary>
    /// Clears test data after testing.
    /// </summary>
    Task<ClearResult> ClearTestDataAsync(string dataStoreName, string testDataSetId);

    /// <summary>
    /// Lists available test datasets.
    /// </summary>
    Task<IEnumerable<TestDataSetInfo>> GetAvailableTestDatasetsAsync();

    /// <summary>
    /// Creates snapshot of current data as test dataset.
    /// </summary>
    Task<TestDataSet> CreateTestDataSnapshotAsync(string dataStoreName, string name);
}
```

**Master Data Set Example:**
```csharp
public class MasterDataSet
{
    public string Name { get; set; } = "";  // "reference-data-v1"
    public string Version { get; set; } = "";  // "1.0.0"
    public DataSetType Type { get; set; } = DataSetType.Master;

    // Data sources
    public IEnumerable<DataSource> Sources { get; set; } = [];

    // Data to load
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>
    {
        ["Countries"] = new[] { /* country data */ },
        ["States"] = new[] { /* state data */ },
        ["Currencies"] = new[] { /* currency data */ },
        ["TimeZones"] = new[] { /* timezone data */ }
    };
}

// Usage: Setup new production tenant
var masterData = await _masterDataLoader.GetAvailableMasterDataAsync()
    .FirstOrDefault(m => m.Name == "reference-data-v1");

await _masterDataLoader.LoadMasterDataAsync("tenant-abc-db", masterData);
```

**Test Data Set Example:**
```csharp
public class TestDataSet
{
    public string Id { get; set; } = "";  // "order-integration-tests"
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public DataSetType Type { get; set; } = DataSetType.Test;

    // Test scenario data
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>
    {
        ["Customers"] = new[]
        {
            new { Id = 1, Name = "Test Customer 1", Email = "test1@example.com" },
            new { Id = 2, Name = "Test Customer 2", Email = "test2@example.com" }
        },
        ["Products"] = new[]
        {
            new { Id = 100, Name = "Test Product A", Price = 19.99m },
            new { Id = 101, Name = "Test Product B", Price = 29.99m }
        },
        ["Orders"] = new[]
        {
            new { Id = 1000, CustomerId = 1, ProductId = 100, Quantity = 2, Status = "Pending" },
            new { Id = 1001, CustomerId = 2, ProductId = 101, Quantity = 1, Status = "Shipped" }
        }
    };
}

// Usage: Load test data for integration tests
[TestInitialize]
public async Task SetupTestData()
{
    var testData = await _testDataLoader.GetAvailableTestDatasetsAsync()
        .FirstOrDefault(t => t.Id == "order-integration-tests");

    await _testDataLoader.LoadTestDataAsync("test-db", testData);
}

[TestCleanup]
public async Task ClearTestData()
{
    await _testDataLoader.ClearTestDataAsync("test-db", "order-integration-tests");
}
```

**Data Source Providers:**
```csharp
public interface IDataSourceProvider
{
    /// <summary>
    /// Loads data from source (JSON, CSV, SQL, Excel, etc.)
    /// </summary>
    Task<IDictionary<string, object>> LoadDataAsync(DataSource source);

    /// <summary>
    /// Supported source types
    /// </summary>
    string[] SupportedSourceTypes { get; }
}

// JSON file provider
public class JsonFileDataSourceProvider : IDataSourceProvider
{
    public string[] SupportedSourceTypes => new[] { "json", "json-file" };

    public async Task<IDictionary<string, object>> LoadDataAsync(DataSource source)
    {
        var json = await File.ReadAllTextAsync(source.Path);
        return JsonSerializer.Deserialize<Dictionary<string, object>>(json)!;
    }
}

// CSV file provider
public class CsvFileDataSourceProvider : IDataSourceProvider { }

// SQL script provider
public class SqlScriptDataSourceProvider : IDataSourceProvider { }

// Excel file provider
public class ExcelFileDataSourceProvider : IDataSourceProvider { }

// Embedded resource provider
public class EmbeddedResourceDataSourceProvider : IDataSourceProvider { }
```

**Usage Scenarios:**

**Scenario 1: New Production Tenant Setup**
```csharp
public class TenantProvisioningService
{
    public async Task ProvisionNewTenantAsync(string tenantId)
    {
        // 1. Create database
        await _databaseProvisioner.CreateDatabaseAsync($"tenant-{tenantId}-db");

        // 2. Run migrations
        await _migrationService.ApplyMigrationsAsync($"tenant-{tenantId}-db");

        // 3. Load master data (reference data, configs, etc.)
        var masterData = await _masterDataLoader.GetAvailableMasterDataAsync()
            .FirstOrDefault(m => m.Name == "default-master-data");

        await _masterDataLoader.LoadMasterDataAsync($"tenant-{tenantId}-db", masterData);

        // Tenant ready for use
    }
}
```

**Scenario 2: Integration Tests with Known Dataset**
```csharp
[TestClass]
public class OrderProcessingIntegrationTests
{
    [TestInitialize]
    public async Task Setup()
    {
        // Load known test dataset
        var testData = await _testDataLoader.GetAvailableTestDatasetsAsync()
            .FirstOrDefault(t => t.Name == "order-processing-scenarios");

        await _testDataLoader.LoadTestDataAsync("integration-test-db", testData);
    }

    [TestMethod]
    public async Task ProcessOrder_WithValidData_CreatesOrder()
    {
        // Test data already loaded - known customer ID 1, product ID 100
        var order = new CreateOrderRequest
        {
            CustomerId = 1,  // From test dataset
            ProductId = 100,  // From test dataset
            Quantity = 2
        };

        var result = await _orderService.CreateOrderAsync(order);

        Assert.IsTrue(result.Success);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        // Clear test data
        await _testDataLoader.ClearTestDataAsync("integration-test-db", "order-processing-scenarios");
    }
}
```

**Scenario 3: Manual Testing with Reproducible Data**
```csharp
public class ManualTestingController : ControllerBase
{
    [HttpPost("api/test-data/load/{datasetName}")]
    public async Task<IActionResult> LoadTestDataset(string datasetName)
    {
        var testData = await _testDataLoader.GetAvailableTestDatasetsAsync()
            .FirstOrDefault(t => t.Name == datasetName);

        if (testData == null)
            return NotFound();

        await _testDataLoader.LoadTestDataAsync("dev-db", testData);

        return Ok(new { Message = $"Test dataset '{datasetName}' loaded successfully" });
    }

    [HttpDelete("api/test-data/clear/{datasetId}")]
    public async Task<IActionResult> ClearTestDataset(string datasetId)
    {
        await _testDataLoader.ClearTestDataAsync("dev-db", datasetId);
        return Ok();
    }
}
```

**Data Set Storage:**
```
DataSets/
├── MasterData/
│   ├── reference-data-v1.json
│   ├── default-configs.json
│   └── currencies-timezones.json
└── TestData/
    ├── order-integration-tests.json
    ├── user-scenarios.json
    └── edge-cases.json
```

**Benefits:**
- ✅ **Production Setup** - Initialize new tenants/environments with master data
- ✅ **Test Data Management** - Reproducible test scenarios
- ✅ **Data Versioning** - Track master data and test data versions
- ✅ **Multiple Sources** - JSON, CSV, SQL, Excel, embedded resources
- ✅ **Validation** - Validate data before loading
- ✅ **Cleanup** - Clear test data after testing
- ✅ **Snapshots** - Capture current state as test dataset

**What This Is NOT:**
- ❌ General ETL pipeline for ongoing data integration
- ❌ Data warehousing solution
- ❌ Real-time data synchronization
- ❌ Data transformation framework

**What This IS:**
- ✅ Master data initialization tool
- ✅ Test data upload and management
- ✅ Environment provisioning support
- ✅ Reproducible test scenarios

**Impact:**
- Epic 5 renamed: "Master Data & Test Data Management"
- Focus on initialization, not ongoing ETL
- Two primary use cases: Production setup + Test data loading
- Data source providers for JSON, CSV, SQL, Excel
- Integration with test infrastructure

---

## Revision 10: Comprehensive Document Services (Context-Based)

**Feedback:** "For document services there are multiple topics: retrieval, persistence, conversion (with chaining), extraction (text), rendering (text to image), splitting (multipage to single page or page sets), composition (pages or sets of pages -> multipage), packing (multiple documents -> zip|rar...), unpacking (zip -> files), media type detection (headers, finger printing, ... -> media type), OCR. These should all be context-based so the requesting application can operationally provide additional details that could be picked up by the various providers."

### Original Design ❌
```
Epic 6: Document Management (3 features)
1. Persistence & Retrieval
2. Conversion Pipelines
3. Pack/Unpack
```

**Problems:**
- Missing: Extraction, Rendering, Splitting, Composition, Media Type Detection, OCR
- No context-passing mechanism for providers
- Not comprehensive enough for document operations

### Revised Design ✅

**Epic 6: Document Services (11 Standalone Services)**

All services are **context-based** - applications provide operational context that providers can use.

**1. Document Retrieval**
```csharp
public interface IDocumentRetrievalService
{
    /// <summary>
    /// Retrieves document with context.
    /// </summary>
    Task<Document> GetAsync(Guid documentId, RetrievalContext? context = null);
    Task<IEnumerable<Document>> QueryAsync(DocumentQuery query, RetrievalContext? context = null);
}

public class RetrievalContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public bool IncludeMetadata { get; set; } = true;
    public bool IncludeContent { get; set; } = true;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**2. Document Persistence**
```csharp
public interface IDocumentPersistenceService
{
    /// <summary>
    /// Stores document with context.
    /// </summary>
    Task<Guid> SaveAsync(Document document, PersistenceContext? context = null);
    Task DeleteAsync(Guid documentId, PersistenceContext? context = null);
}

public class PersistenceContext
{
    public string? RequestingApplication { get; set; }
    public string? UserId { get; set; }
    public StorageProvider? PreferredProvider { get; set; }  // "azure-blob", "s3", "file-system"
    public bool EnableVersioning { get; set; } = false;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**3. Document Conversion (with Chaining)**
```csharp
public interface IDocumentConversionService
{
    /// <summary>
    /// Converts document between formats with context.
    /// Supports chaining: PDF → Image → Thumbnail
    /// </summary>
    Task<ConvertedDocument> ConvertAsync(
        Document source,
        string targetMediaType,
        ConversionContext? context = null);

    /// <summary>
    /// Converts through multiple steps (chaining).
    /// Example: DOCX → PDF → PNG
    /// </summary>
    Task<ConvertedDocument> ConvertChainAsync(
        Document source,
        string[] targetMediaTypes,
        ConversionContext? context = null);
}

public class ConversionContext
{
    public string? RequestingApplication { get; set; }
    public int? Quality { get; set; }  // For image conversions
    public int? DPI { get; set; }  // For PDF/image conversions
    public string? ColorSpace { get; set; }  // "RGB", "CMYK", "Grayscale"
    public IDictionary<string, object> ProviderOptions { get; set; } = new Dictionary<string, object>();
}
```

**4. Text Extraction**
```csharp
public interface IDocumentExtractionService
{
    /// <summary>
    /// Extracts text from document with context.
    /// </summary>
    Task<ExtractedText> ExtractTextAsync(Document document, ExtractionContext? context = null);
}

public class ExtractionContext
{
    public string? RequestingApplication { get; set; }
    public bool PreserveFormatting { get; set; } = false;
    public bool IncludeMetadata { get; set; } = true;
    public string? TargetLanguage { get; set; }  // For translation during extraction
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}

public class ExtractedText
{
    public string Content { get; set; } = "";
    public string MediaType { get; set; } = "text/plain";
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

**5. Document Rendering (Text to Image)**
```csharp
public interface IDocumentRenderingService
{
    /// <summary>
    /// Renders text content to image with context.
    /// </summary>
    Task<RenderedDocument> RenderToImageAsync(
        string textContent,
        string sourceMediaType,
        RenderingContext? context = null);
}

public class RenderingContext
{
    public string? RequestingApplication { get; set; }
    public int Width { get; set; } = 800;
    public int Height { get; set; } = 600;
    public int DPI { get; set; } = 96;
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 12;
    public string OutputFormat { get; set; } = "image/png";  // "image/png", "image/jpeg", etc.
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**6. Document Splitting**
```csharp
public interface IDocumentSplittingService
{
    /// <summary>
    /// Splits multipage document to single pages with context.
    /// </summary>
    Task<IEnumerable<Document>> SplitToPagesAsync(Document document, SplittingContext? context = null);

    /// <summary>
    /// Splits multipage document to page sets with context.
    /// Example: 10-page PDF → 3-page sets (pages 1-3, 4-6, 7-9, 10)
    /// </summary>
    Task<IEnumerable<Document>> SplitToPageSetsAsync(
        Document document,
        int pagesPerSet,
        SplittingContext? context = null);
}

public class SplittingContext
{
    public string? RequestingApplication { get; set; }
    public string? NamingPattern { get; set; }  // "document-{page}.pdf"
    public bool PreserveMetadata { get; set; } = true;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**7. Document Composition**
```csharp
public interface IDocumentCompositionService
{
    /// <summary>
    /// Composes multiple pages/documents into single multipage document with context.
    /// </summary>
    Task<Document> ComposeMultipageAsync(
        IEnumerable<Document> pages,
        string targetMediaType,
        CompositionContext? context = null);
}

public class CompositionContext
{
    public string? RequestingApplication { get; set; }
    public string? OutputFileName { get; set; }
    public bool AddPageNumbers { get; set; } = false;
    public bool AddTableOfContents { get; set; } = false;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**8. Document Packing**
```csharp
public interface IDocumentPackingService
{
    /// <summary>
    /// Packs multiple documents into archive with context.
    /// Supports: ZIP, RAR, TAR, 7Z, etc.
    /// </summary>
    Task<PackedDocument> PackAsync(
        IEnumerable<Document> documents,
        string archiveFormat,  // "zip", "rar", "tar", "7z"
        PackingContext? context = null);
}

public class PackingContext
{
    public string? RequestingApplication { get; set; }
    public string? ArchiveName { get; set; }
    public CompressionLevel CompressionLevel { get; set; } = CompressionLevel.Normal;
    public string? Password { get; set; }  // For encrypted archives
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**9. Document Unpacking**
```csharp
public interface IDocumentUnpackingService
{
    /// <summary>
    /// Unpacks archive to individual documents with context.
    /// Supports: ZIP, RAR, TAR, 7Z, etc.
    /// </summary>
    Task<IEnumerable<Document>> UnpackAsync(
        Document archive,
        UnpackingContext? context = null);
}

public class UnpackingContext
{
    public string? RequestingApplication { get; set; }
    public string? Password { get; set; }  // For encrypted archives
    public bool PreserveDirectory Structure { get; set; } = true;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}
```

**10. Media Type Detection**
```csharp
public interface IMediaTypeDetectionService
{
    /// <summary>
    /// Detects media type using headers, fingerprinting, magic numbers, etc.
    /// </summary>
    Task<MediaTypeResult> DetectAsync(Stream content, DetectionContext? context = null);
    Task<MediaTypeResult> DetectAsync(byte[] content, DetectionContext? context = null);
}

public class DetectionContext
{
    public string? RequestingApplication { get; set; }
    public string? FileName { get; set; }  // Hint from filename extension
    public string? DeclaredMediaType { get; set; }  // Hint from Content-Type header
    public DetectionStrategy Strategy { get; set; } = DetectionStrategy.Comprehensive;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}

public enum DetectionStrategy
{
    HeadersOnly,      // Just check headers/magic numbers
    Fingerprinting,   // Deep content analysis
    Comprehensive     // Headers + fingerprinting + validation
}

public class MediaTypeResult
{
    public string MediaType { get; set; } = "";
    public string? FileExtension { get; set; }
    public double Confidence { get; set; }  // 0.0 - 1.0
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

**11. OCR (Optical Character Recognition)**
```csharp
public interface IOcrService
{
    /// <summary>
    /// Performs OCR on document with context.
    /// </summary>
    Task<OcrResult> RecognizeTextAsync(Document document, OcrContext? context = null);
}

public class OcrContext
{
    public string? RequestingApplication { get; set; }
    public string[] Languages { get; set; } = new[] { "eng" };  // "eng", "fra", "deu", etc.
    public OcrEngine? PreferredEngine { get; set; }  // "tesseract", "azure-vision", "aws-textract"
    public bool DetectOrientation { get; set; } = true;
    public bool DetectTables { get; set; } = false;
    public IDictionary<string, object> AdditionalContext { get; set; } = new Dictionary<string, object>();
}

public class OcrResult
{
    public string Text { get; set; } = "";
    public IEnumerable<TextBlock> Blocks { get; set; } = [];
    public double Confidence { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}

public class TextBlock
{
    public string Text { get; set; } = "";
    public BoundingBox BoundingBox { get; set; } = new();
    public double Confidence { get; set; }
}
```

---

### Context-Based Provider Pattern

**All providers receive context from requesting application:**

```csharp
// Conversion provider receives context
public interface IDocumentConversionProvider
{
    string ProviderName { get; }
    string[] SupportedSourceFormats { get; }
    string[] SupportedTargetFormats { get; }

    /// <summary>
    /// Provider receives context from application.
    /// Can use context to adjust behavior (quality, DPI, provider options, etc.)
    /// </summary>
    Task<ConvertedDocument> ConvertAsync(
        Document source,
        string targetMediaType,
        ConversionContext context);
}

// Example: PDF to Image conversion provider
public class PdfToImageConversionProvider : IDocumentConversionProvider
{
    public async Task<ConvertedDocument> ConvertAsync(
        Document source,
        string targetMediaType,
        ConversionContext context)
    {
        // Use context to adjust conversion
        var dpi = context.DPI ?? 300;
        var quality = context.Quality ?? 90;

        // Check for provider-specific options in context
        if (context.ProviderOptions.TryGetValue("anti-aliasing", out var aaValue))
        {
            var antiAliasing = (bool)aaValue;
            // Use anti-aliasing setting
        }

        // Application-specific logic
        if (context.RequestingApplication == "thumbnail-generator")
        {
            dpi = 72;  // Lower DPI for thumbnails
        }

        // Perform conversion with context-adjusted settings
        var image = await ConvertPdfToImageAsync(source, dpi, quality);
        return new ConvertedDocument { Content = image, MediaType = targetMediaType };
    }
}
```

---

### Usage Examples

**Example 1: Document Processing Pipeline with Context**
```csharp
public class InvoiceProcessingService
{
    public async Task ProcessInvoiceAsync(Document scannedInvoice)
    {
        // Context from requesting application
        var context = new
        {
            RequestingApplication = "invoice-processor",
            UserId = "system",
            Quality = 95,
            Languages = new[] { "eng", "fra" }
        };

        // 1. Detect media type
        var detectionContext = new DetectionContext
        {
            RequestingApplication = context.RequestingApplication,
            Strategy = DetectionStrategy.Comprehensive
        };
        var mediaType = await _mediaTypeDetection.DetectAsync(scannedInvoice.Content, detectionContext);

        // 2. Convert to PDF if needed
        if (mediaType.MediaType != "application/pdf")
        {
            var conversionContext = new ConversionContext
            {
                RequestingApplication = context.RequestingApplication,
                Quality = context.Quality,
                DPI = 300
            };
            scannedInvoice = await _conversion.ConvertAsync(scannedInvoice, "application/pdf", conversionContext);
        }

        // 3. Perform OCR
        var ocrContext = new OcrContext
        {
            RequestingApplication = context.RequestingApplication,
            Languages = context.Languages,
            DetectTables = true  // Invoices have tables
        };
        var ocrResult = await _ocr.RecognizeTextAsync(scannedInvoice, ocrContext);

        // 4. Extract text
        var extractionContext = new ExtractionContext
        {
            RequestingApplication = context.RequestingApplication,
            PreserveFormatting = true
        };
        var text = await _extraction.ExtractTextAsync(scannedInvoice, extractionContext);

        // 5. Store processed document
        var persistenceContext = new PersistenceContext
        {
            RequestingApplication = context.RequestingApplication,
            UserId = context.UserId,
            PreferredProvider = "azure-blob",
            EnableVersioning = true
        };
        await _persistence.SaveAsync(scannedInvoice, persistenceContext);
    }
}
```

**Example 2: Multipage Document Splitting and Composition**
```csharp
public class DocumentReorganizerService
{
    public async Task ReorganizeDocumentAsync(Document multipageDoc)
    {
        var context = new { RequestingApplication = "document-organizer" };

        // 1. Split to individual pages
        var splittingContext = new SplittingContext
        {
            RequestingApplication = context.RequestingApplication,
            NamingPattern = "page-{page}.pdf"
        };
        var pages = await _splitting.SplitToPagesAsync(multipageDoc, splittingContext);

        // 2. Reorder pages (business logic)
        var reorderedPages = pages.OrderByDescending(p => p.Metadata["PageNumber"]);

        // 3. Compose back to multipage
        var compositionContext = new CompositionContext
        {
            RequestingApplication = context.RequestingApplication,
            AddPageNumbers = true,
            OutputFileName = "reordered-document.pdf"
        };
        var recomposed = await _composition.ComposeMultipageAsync(reorderedPages, "application/pdf", compositionContext);

        // 4. Pack with related documents
        var packingContext = new PackingContext
        {
            RequestingApplication = context.RequestingApplication,
            ArchiveName = "document-package.zip",
            CompressionLevel = CompressionLevel.Maximum
        };
        await _packing.PackAsync(new[] { recomposed }, "zip", packingContext);
    }
}
```

**Example 3: Text to Image Rendering**
```csharp
public class ReportGeneratorService
{
    public async Task GenerateReportImageAsync(string reportText)
    {
        var renderingContext = new RenderingContext
        {
            RequestingApplication = "report-generator",
            Width = 1920,
            Height = 1080,
            DPI = 150,
            FontFamily = "Courier New",
            FontSize = 14,
            OutputFormat = "image/png",
            AdditionalContext = new Dictionary<string, object>
            {
                ["BackgroundColor"] = "#FFFFFF",
                ["TextColor"] = "#000000"
            }
        };

        var renderedImage = await _rendering.RenderToImageAsync(reportText, "text/plain", renderingContext);
    }
}
```

---

### Benefits

- ✅ **Comprehensive** - 11 document services covering all operations
- ✅ **Context-Based** - Applications provide operational context to providers
- ✅ **Standalone** - Each service works independently
- ✅ **Composable** - Services can be chained together
- ✅ **Provider Pattern** - Extensible via provider implementations
- ✅ **Application-Aware** - Providers can adjust behavior based on requesting application
- ✅ **Flexible** - Context allows per-request customization

**Impact:**
- Epic 6 expanded from 3 to 11 services
- All services context-based (context parameter on all methods)
- Providers receive context and can adjust behavior
- Comprehensive document operations coverage

---

## Revision 11: Modular Identity & Account Profiles

**Feedback:** "For identity and session management this should support features like account management, role/claims management, account profiles (this should even be modules so component features may advertise information they want like schedules, contact lists, user defaults, and more)."

### Original Understanding ❌
```
Epic 7: Identity & Session Management
- Basic authentication and session tracking
- Simple user profiles
```

**Problems:**
- No account management interfaces
- No role/claims management
- Profiles not extensible
- Component features can't contribute profile data

### Revised Design ✅

**Epic 7: Identity & Session Management (4 Core Services + Modular Profiles)**

**Core Services:**
1. Account Management
2. Role & Claims Management
3. Session Management
4. Profile Management (Modular)

---

### 1. Account Management

```csharp
public interface IAccountManagementService
{
    /// <summary>
    /// Creates new user account.
    /// </summary>
    Task<Account> CreateAccountAsync(CreateAccountRequest request);

    /// <summary>
    /// Updates account information.
    /// </summary>
    Task<Account> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request);

    /// <summary>
    /// Deactivates/deletes account.
    /// </summary>
    Task DeactivateAccountAsync(Guid accountId, DeactivationReason reason);

    /// <summary>
    /// Reactivates deactivated account.
    /// </summary>
    Task ReactivateAccountAsync(Guid accountId);

    /// <summary>
    /// Gets account by ID.
    /// </summary>
    Task<Account> GetAccountAsync(Guid accountId);

    /// <summary>
    /// Searches accounts.
    /// </summary>
    Task<IEnumerable<Account>> SearchAccountsAsync(AccountSearchCriteria criteria);

    /// <summary>
    /// Changes password.
    /// </summary>
    Task ChangePasswordAsync(Guid accountId, string currentPassword, string newPassword);

    /// <summary>
    /// Initiates password reset.
    /// </summary>
    Task<PasswordResetToken> InitiatePasswordResetAsync(string email);

    /// <summary>
    /// Completes password reset.
    /// </summary>
    Task CompletePasswordResetAsync(string token, string newPassword);
}

public class Account
{
    public Guid Id { get; set; }
    public string Email { get; set; } = "";
    public string Username { get; set; } = "";
    public AccountStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool EmailVerified { get; set; }
    public bool TwoFactorEnabled { get; set; }
}
```

---

### 2. Role & Claims Management

```csharp
public interface IRoleManagementService
{
    /// <summary>
    /// Creates new role.
    /// </summary>
    Task<Role> CreateRoleAsync(string roleName, string? description = null);

    /// <summary>
    /// Assigns role to account.
    /// </summary>
    Task AssignRoleAsync(Guid accountId, string roleName);

    /// <summary>
    /// Removes role from account.
    /// </summary>
    Task RemoveRoleAsync(Guid accountId, string roleName);

    /// <summary>
    /// Gets all roles for account.
    /// </summary>
    Task<IEnumerable<Role>> GetAccountRolesAsync(Guid accountId);

    /// <summary>
    /// Checks if account has role.
    /// </summary>
    Task<bool> HasRoleAsync(Guid accountId, string roleName);
}

public interface IClaimsManagementService
{
    /// <summary>
    /// Adds claim to account.
    /// </summary>
    Task AddClaimAsync(Guid accountId, Claim claim);

    /// <summary>
    /// Removes claim from account.
    /// </summary>
    Task RemoveClaimAsync(Guid accountId, string claimType, string claimValue);

    /// <summary>
    /// Gets all claims for account.
    /// </summary>
    Task<IEnumerable<Claim>> GetAccountClaimsAsync(Guid accountId);

    /// <summary>
    /// Checks if account has specific claim.
    /// </summary>
    Task<bool> HasClaimAsync(Guid accountId, string claimType, string claimValue);

    /// <summary>
    /// Gets claim value.
    /// </summary>
    Task<string?> GetClaimValueAsync(Guid accountId, string claimType);
}

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public IEnumerable<Claim> Claims { get; set; } = [];
}

public class Claim
{
    public string Type { get; set; } = "";
    public string Value { get; set; } = "";
}
```

---

### 3. Session Management

```csharp
public interface ISessionManagementService
{
    /// <summary>
    /// Creates new session for account.
    /// </summary>
    Task<Session> CreateSessionAsync(Guid accountId, SessionContext context);

    /// <summary>
    /// Validates and refreshes session.
    /// </summary>
    Task<Session> RefreshSessionAsync(string sessionToken);

    /// <summary>
    /// Terminates session.
    /// </summary>
    Task TerminateSessionAsync(string sessionToken);

    /// <summary>
    /// Terminates all sessions for account.
    /// </summary>
    Task TerminateAllSessionsAsync(Guid accountId);

    /// <summary>
    /// Gets active sessions for account.
    /// </summary>
    Task<IEnumerable<Session>> GetActiveSessionsAsync(Guid accountId);
}

public class Session
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string Token { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public SessionStatus Status { get; set; }
}

public class SessionContext
{
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? DeviceId { get; set; }
    public TimeSpan? ExpirationDuration { get; set; }
}
```

---

### 4. Modular Profile Management

**Key Innovation: Component features can register profile providers to advertise their data.**

```csharp
public interface IProfileManagementService
{
    /// <summary>
    /// Registers profile provider (called by component features).
    /// </summary>
    void RegisterProfileProvider(IProfileProvider provider);

    /// <summary>
    /// Gets complete account profile (aggregates all providers).
    /// </summary>
    Task<AccountProfile> GetProfileAsync(Guid accountId);

    /// <summary>
    /// Gets specific profile module.
    /// </summary>
    Task<TProfileModule> GetProfileModuleAsync<TProfileModule>(Guid accountId) where TProfileModule : class;

    /// <summary>
    /// Updates profile module data.
    /// </summary>
    Task UpdateProfileModuleAsync(Guid accountId, string moduleName, object data);
}

public interface IProfileProvider
{
    /// <summary>
    /// Module name (e.g., "schedules", "contacts", "preferences").
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Module display name.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Provides profile data for this module.
    /// </summary>
    Task<object> GetProfileDataAsync(Guid accountId);

    /// <summary>
    /// Updates profile data for this module.
    /// </summary>
    Task UpdateProfileDataAsync(Guid accountId, object data);

    /// <summary>
    /// Module schema (for UI generation, validation).
    /// </summary>
    ProfileModuleSchema Schema { get; }
}

public class AccountProfile
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = new();

    /// <summary>
    /// Profile modules contributed by features.
    /// Key = module name, Value = module data
    /// </summary>
    public IDictionary<string, object> Modules { get; set; } = new Dictionary<string, object>();
}
```

---

### Profile Provider Examples

**Example 1: Schedules Module (from Scheduling Feature)**

```csharp
public class SchedulesProfileProvider : IProfileProvider
{
    public string ModuleName => "schedules";
    public string DisplayName => "My Schedules";

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        var schedules = await _schedulingService.GetUserSchedulesAsync(accountId);

        return new SchedulesProfileModule
        {
            DefaultScheduleId = await _schedulingService.GetDefaultScheduleIdAsync(accountId),
            Schedules = schedules,
            TimeZone = await _schedulingService.GetUserTimeZoneAsync(accountId),
            WorkingHours = await _schedulingService.GetWorkingHoursAsync(accountId)
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var module = (SchedulesProfileModule)data;
        await _schedulingService.UpdateUserSchedulePreferencesAsync(accountId, module);
    }

    public ProfileModuleSchema Schema => new()
    {
        Fields = new[]
        {
            new ProfileField { Name = "DefaultScheduleId", Type = "guid", Required = false },
            new ProfileField { Name = "TimeZone", Type = "string", Required = true },
            new ProfileField { Name = "WorkingHours", Type = "object", Required = false }
        }
    };
}

public class SchedulesProfileModule
{
    public Guid? DefaultScheduleId { get; set; }
    public IEnumerable<Schedule> Schedules { get; set; } = [];
    public string TimeZone { get; set; } = "UTC";
    public WorkingHours? WorkingHours { get; set; }
}

// Registration (in scheduling feature startup)
public class SchedulingFeatureStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Feature registers its profile provider
        services.AddSingleton<IProfileProvider, SchedulesProfileProvider>();
    }
}
```

**Example 2: Contacts Module (from Contacts Feature)**

```csharp
public class ContactsProfileProvider : IProfileProvider
{
    public string ModuleName => "contacts";
    public string DisplayName => "My Contacts";

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        return new ContactsProfileModule
        {
            ContactLists = await _contactsService.GetContactListsAsync(accountId),
            DefaultContactList = await _contactsService.GetDefaultContactListAsync(accountId),
            ContactPreferences = await _contactsService.GetContactPreferencesAsync(accountId)
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var module = (ContactsProfileModule)data;
        await _contactsService.UpdateContactPreferencesAsync(accountId, module.ContactPreferences);
    }

    public ProfileModuleSchema Schema => new()
    {
        Fields = new[]
        {
            new ProfileField { Name = "DefaultContactList", Type = "guid", Required = false },
            new ProfileField { Name = "ContactPreferences", Type = "object", Required = false }
        }
    };
}

public class ContactsProfileModule
{
    public IEnumerable<ContactList> ContactLists { get; set; } = [];
    public Guid? DefaultContactList { get; set; }
    public ContactPreferences? ContactPreferences { get; set; }
}
```

**Example 3: User Defaults Module (from Application Settings Feature)**

```csharp
public class UserDefaultsProfileProvider : IProfileProvider
{
    public string ModuleName => "defaults";
    public string DisplayName => "My Defaults";

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        return new UserDefaultsProfileModule
        {
            Language = await _settingsService.GetUserLanguageAsync(accountId),
            Currency = await _settingsService.GetUserCurrencyAsync(accountId),
            DateFormat = await _settingsService.GetDateFormatAsync(accountId),
            TimeFormat = await _settingsService.GetTimeFormatAsync(accountId),
            Theme = await _settingsService.GetThemeAsync(accountId),
            NotificationPreferences = await _settingsService.GetNotificationPreferencesAsync(accountId)
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var module = (UserDefaultsProfileModule)data;
        await _settingsService.UpdateUserDefaultsAsync(accountId, module);
    }

    public ProfileModuleSchema Schema => new()
    {
        Fields = new[]
        {
            new ProfileField { Name = "Language", Type = "string", Required = true, DefaultValue = "en-US" },
            new ProfileField { Name = "Currency", Type = "string", Required = true, DefaultValue = "USD" },
            new ProfileField { Name = "DateFormat", Type = "string", Required = false },
            new ProfileField { Name = "Theme", Type = "string", Required = false, DefaultValue = "light" }
        }
    };
}

public class UserDefaultsProfileModule
{
    public string Language { get; set; } = "en-US";
    public string Currency { get; set; } = "USD";
    public string DateFormat { get; set; } = "MM/dd/yyyy";
    public string TimeFormat { get; set; } = "12h";
    public string Theme { get; set; } = "light";
    public NotificationPreferences? NotificationPreferences { get; set; }
}
```

---

### Usage Examples

**Example 1: Get Complete Profile (All Modules)**

```csharp
public class UserProfileController : ControllerBase
{
    [HttpGet("api/profile")]
    public async Task<AccountProfile> GetProfile()
    {
        var accountId = User.GetAccountId();

        // Gets ALL profile modules from registered providers
        var profile = await _profileManagement.GetProfileAsync(accountId);

        // Returns:
        // {
        //   "accountId": "...",
        //   "account": { ... },
        //   "modules": {
        //     "schedules": { "defaultScheduleId": "...", "timeZone": "America/New_York", ... },
        //     "contacts": { "contactLists": [...], "defaultContactList": "..." },
        //     "defaults": { "language": "en-US", "currency": "USD", "theme": "dark" },
        //     "communications": { "preferredChannels": ["email", "sms"], "quietHours": {...} }
        //   }
        // }

        return profile;
    }
}
```

**Example 2: Get Specific Profile Module**

```csharp
public class SchedulesController : ControllerBase
{
    [HttpGet("api/profile/schedules")]
    public async Task<SchedulesProfileModule> GetSchedulesModule()
    {
        var accountId = User.GetAccountId();

        // Gets ONLY schedules module
        var schedules = await _profileManagement.GetProfileModuleAsync<SchedulesProfileModule>(accountId);

        return schedules;
    }
}
```

**Example 3: Update Profile Module**

```csharp
public class SettingsController : ControllerBase
{
    [HttpPut("api/profile/defaults")]
    public async Task UpdateDefaults([FromBody] UserDefaultsProfileModule defaults)
    {
        var accountId = User.GetAccountId();

        // Updates ONLY defaults module
        await _profileManagement.UpdateProfileModuleAsync(accountId, "defaults", defaults);
    }
}
```

**Example 4: Feature Registers Profile Provider**

```csharp
// Communications feature wants to add preferences to user profile
public class CommunicationsFeatureStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Register communications feature services
        services.AddSingleton<ICommunicationsService, CommunicationsService>();

        // Register profile provider (advertises communication preferences)
        services.AddSingleton<IProfileProvider, CommunicationsProfileProvider>();
    }
}

public class CommunicationsProfileProvider : IProfileProvider
{
    public string ModuleName => "communications";
    public string DisplayName => "Communication Preferences";

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        return new CommunicationsProfileModule
        {
            PreferredChannels = await _communicationsService.GetPreferredChannelsAsync(accountId),
            QuietHours = await _communicationsService.GetQuietHoursAsync(accountId),
            EmailAddress = await _communicationsService.GetEmailAddressAsync(accountId),
            PhoneNumber = await _communicationsService.GetPhoneNumberAsync(accountId)
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var module = (CommunicationsProfileModule)data;
        await _communicationsService.UpdatePreferencesAsync(accountId, module);
    }

    public ProfileModuleSchema Schema => new()
    {
        Fields = new[]
        {
            new ProfileField { Name = "PreferredChannels", Type = "array", Required = true },
            new ProfileField { Name = "QuietHours", Type = "object", Required = false }
        }
    };
}
```

---

### Benefits

- ✅ **Modular** - Component features contribute profile modules
- ✅ **Extensible** - New features just register profile providers
- ✅ **Discoverable** - Profile modules advertised via schema
- ✅ **Composable** - Get all modules or specific modules
- ✅ **Feature-Owned** - Each feature owns its profile data
- ✅ **Type-Safe** - Strongly-typed profile modules
- ✅ **UI-Friendly** - Schema enables dynamic UI generation

**Impact:**
- Epic 7 expanded to include modular profile system
- Component features register `IProfileProvider` to advertise data
- Account profiles composed from registered providers
- Supports schedules, contacts, user defaults, and more

---

## Revision 12: Background Process Platform Agnosticism

**Feedback:** "For background processes everything in this framework should be hostable in any other platform. azure functions, aws lambda, app services background worker, windows service, linux daemons, self hosted quartz/hangfire, etc."

### Problem ❌
Background services hard-coded to specific hosting platforms create deployment limitations:
- Cache warming tied to `IHostedService` (ASP.NET Core only)
- Scheduled tasks require specific hosting infrastructure
- Serverless platforms (Azure Functions, AWS Lambda) need different patterns
- Cannot reuse logic across Windows Services, Linux daemons, etc.

---

### Revised Design ✅

**Core Principle:** Separate business logic from hosting infrastructure via abstraction layer.

**Background Task Abstraction:**
```csharp
/// <summary>
/// Platform-agnostic background task abstraction.
/// Business logic implements this; hosting platforms provide schedulers.
/// </summary>
public interface IBackgroundTask
{
    /// <summary>
    /// Task identifier (unique per task type).
    /// </summary>
    string TaskId { get; }

    /// <summary>
    /// Executes the task logic (platform-agnostic).
    /// </summary>
    Task ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>
/// Platform-agnostic task scheduler.
/// Implementations provided for each hosting platform.
/// </summary>
public interface IBackgroundTaskScheduler
{
    /// <summary>
    /// Schedules recurring task with cron expression.
    /// </summary>
    Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression);

    /// <summary>
    /// Schedules one-time task for future execution.
    /// </summary>
    Task ScheduleOnceAsync(IBackgroundTask task, DateTimeOffset executeAt);

    /// <summary>
    /// Cancels scheduled task.
    /// </summary>
    Task CancelAsync(string taskId);

    /// <summary>
    /// Gets all scheduled tasks.
    /// </summary>
    Task<IEnumerable<ScheduledTaskInfo>> GetScheduledTasksAsync();
}
```

---

### Example: Cache Warming Task (Platform-Agnostic)

**Business Logic (Platform-Agnostic):**
```csharp
public class CacheWarmingTask : IBackgroundTask
{
    private readonly IProductService _productService;
    private readonly ICatalogService _catalogService;
    private readonly ILogger<CacheWarmingTask> _logger;

    public string TaskId => "cache-warming";

    public CacheWarmingTask(
        IProductService productService,
        ICatalogService catalogService,
        ILogger<CacheWarmingTask> logger)
    {
        _productService = productService;
        _catalogService = catalogService;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting cache warming...");

        // Warm categories (result discarded - just populates cache)
        _ = await _catalogService.GetCategoriesAsync();

        // Warm featured products
        _ = await _productService.GetFeaturedProductAsync();

        // Warm common products
        var commonProductIds = new[] { 1, 2, 3, 5, 10 };
        foreach (var productId in commonProductIds)
        {
            if (cancellationToken.IsCancellationRequested) break;
            _ = await _productService.GetProductAsync(productId);
        }

        _logger.LogInformation("Cache warming complete");
    }
}
```

---

### Platform Implementations

**1. ASP.NET Core (IHostedService) Scheduler:**
```csharp
public class HostedServiceScheduler : IBackgroundTaskScheduler
{
    private readonly IServiceProvider _services;

    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Register as IHostedService
        var hostedService = new BackgroundTaskHostedService(task, cronExpression, _services);
        // Auto-starts with ASP.NET Core host
    }
}

internal class BackgroundTaskHostedService : BackgroundService
{
    private readonly IBackgroundTask _task;
    private readonly string _cronExpression;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var nextRun = CalculateNextRun(_cronExpression);
            await Task.Delay(nextRun - DateTimeOffset.UtcNow, stoppingToken);

            await _task.ExecuteAsync(stoppingToken);
        }
    }
}
```

**2. Azure Functions (Timer Trigger) Adapter:**
```csharp
public class AzureFunctionsScheduler : IBackgroundTaskScheduler
{
    // Generates Azure Functions with timer triggers
    // Uses source generators or reflection to create function classes
}

// Generated/Created Azure Function
public class CacheWarmingFunction
{
    private readonly IBackgroundTask _cacheWarmingTask;

    [FunctionName("CacheWarming")]
    public async Task Run(
        [TimerTrigger("0 */30 * * * *")] TimerInfo timer,  // Every 30 minutes
        CancellationToken cancellationToken)
    {
        await _cacheWarmingTask.ExecuteAsync(cancellationToken);
    }
}
```

**3. AWS Lambda (EventBridge) Adapter:**
```csharp
public class AwsLambdaScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Create EventBridge rule with cron expression
        var eventBridgeClient = new AmazonEventBridgeClient();
        await eventBridgeClient.PutRuleAsync(new PutRuleRequest
        {
            Name = task.TaskId,
            ScheduleExpression = $"cron({ConvertToCron(cronExpression)})",
            State = RuleState.ENABLED
        });

        // Lambda handler invokes task
    }
}

// Lambda Function Handler
public class LambdaBackgroundTaskHandler
{
    public async Task<string> FunctionHandler(
        ScheduledEvent scheduledEvent,
        ILambdaContext context)
    {
        var taskId = scheduledEvent.Resources[0]; // Task ID from EventBridge
        var task = _taskResolver.Resolve(taskId);
        await task.ExecuteAsync(context.CancellationToken);
        return "Success";
    }
}
```

**4. Windows Service Scheduler:**
```csharp
public class WindowsServiceScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Use System.Threading.Timer or Quartz.NET
        var timer = new Timer(async _ =>
        {
            await task.ExecuteAsync(CancellationToken.None);
        }, null, TimeSpan.Zero, CalculateInterval(cronExpression));
    }
}
```

**5. Linux Daemon (systemd timer) Scheduler:**
```csharp
public class LinuxDaemonScheduler : IBackgroundTaskScheduler
{
    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        // Generate systemd timer unit files
        var timerUnit = $@"
[Unit]
Description={task.TaskId} Timer

[Timer]
OnCalendar={ConvertToSystemdCalendar(cronExpression)}
Persistent=true

[Install]
WantedBy=timers.target
";

        await File.WriteAllTextAsync($"/etc/systemd/system/{task.TaskId}.timer", timerUnit);

        // Reload systemd and enable timer
        await Process.Start("systemctl", "daemon-reload").WaitForExitAsync();
        await Process.Start("systemctl", $"enable --now {task.TaskId}.timer").WaitForExitAsync();
    }
}
```

**6. Quartz.NET (Self-Hosted) Scheduler:**
```csharp
public class QuartzScheduler : IBackgroundTaskScheduler
{
    private readonly IScheduler _scheduler;

    public async Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        var job = JobBuilder.Create<BackgroundTaskJob>()
            .WithIdentity(task.TaskId)
            .UsingJobData("taskId", task.TaskId)
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity($"{task.TaskId}-trigger")
            .WithCronSchedule(cronExpression)
            .Build();

        await _scheduler.ScheduleJob(job, trigger);
    }
}

public class BackgroundTaskJob : IJob
{
    private readonly IBackgroundTaskResolver _resolver;

    public async Task Execute(IJobExecutionContext context)
    {
        var taskId = context.JobDetail.JobDataMap.GetString("taskId");
        var task = _resolver.Resolve(taskId);
        await task.ExecuteAsync(context.CancellationToken);
    }
}
```

**7. Hangfire (Self-Hosted) Scheduler:**
```csharp
public class HangfireScheduler : IBackgroundTaskScheduler
{
    public Task ScheduleRecurringAsync(IBackgroundTask task, string cronExpression)
    {
        RecurringJob.AddOrUpdate(
            task.TaskId,
            () => task.ExecuteAsync(CancellationToken.None),
            cronExpression);

        return Task.CompletedTask;
    }
}
```

---

### Registration and Usage

**Startup (Platform-Agnostic):**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register background tasks (platform-agnostic)
    services.AddSingleton<IBackgroundTask, CacheWarmingTask>();
    services.AddSingleton<IBackgroundTask, DataSyncTask>();
    services.AddSingleton<IBackgroundTask, CleanupTask>();

    // Register scheduler based on hosting platform
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && IsWindowsService)
    {
        services.AddSingleton<IBackgroundTaskScheduler, WindowsServiceScheduler>();
    }
    else if (IsAzureFunction)
    {
        services.AddSingleton<IBackgroundTaskScheduler, AzureFunctionsScheduler>();
    }
    else if (IsAwsLambda)
    {
        services.AddSingleton<IBackgroundTaskScheduler, AwsLambdaScheduler>();
    }
    else if (UseQuartz)
    {
        services.AddSingleton<IBackgroundTaskScheduler, QuartzScheduler>();
    }
    else if (UseHangfire)
    {
        services.AddSingleton<IBackgroundTaskScheduler, HangfireScheduler>();
    }
    else
    {
        // Default: ASP.NET Core IHostedService
        services.AddSingleton<IBackgroundTaskScheduler, HostedServiceScheduler>();
    }
}
```

**Schedule Tasks (Platform-Agnostic):**
```csharp
public class ApplicationStartup
{
    public async Task ConfigureBackgroundTasksAsync(IBackgroundTaskScheduler scheduler)
    {
        // Same code works on ALL platforms
        await scheduler.ScheduleRecurringAsync(
            new CacheWarmingTask(...),
            "0 */30 * * * *"  // Every 30 minutes
        );

        await scheduler.ScheduleRecurringAsync(
            new DataSyncTask(...),
            "0 0 2 * * *"  // Daily at 2 AM
        );

        await scheduler.ScheduleOnceAsync(
            new CleanupTask(...),
            DateTimeOffset.UtcNow.AddHours(1)  // One-time in 1 hour
        );
    }
}
```

---

### Benefits

- ✅ **Platform Agnostic** - Same business logic runs on ANY platform
- ✅ **Deployment Flexibility** - Choose hosting platform without code changes
- ✅ **Testable** - Mock `IBackgroundTaskScheduler` for unit tests
- ✅ **Serverless Ready** - Adapt to Azure Functions, AWS Lambda
- ✅ **Traditional Hosting** - Windows Services, Linux daemons
- ✅ **Self-Hosted** - Quartz.NET, Hangfire integration
- ✅ **Cloud-Native** - App Services, Container Apps, Kubernetes
- ✅ **Separation of Concerns** - Business logic separate from scheduling

---

### Impact on Existing Epics

**Epic 4: Distributed Caching**
```csharp
// Cache warming is now platform-agnostic
public class CacheWarmingTask : IBackgroundTask
{
    // Same code runs everywhere
}

// Register in any platform
await scheduler.ScheduleRecurringAsync(new CacheWarmingTask(...), "0 */30 * * * *");
```

**Epic 5: Master Data & Test Data Management**
```csharp
// Data sync background task
public class DataSyncTask : IBackgroundTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _masterDataLoader.LoadMasterDataAsync(...);
    }
}
```

**Epic 2: Communications Platform**
```csharp
// Deferred message sending
public class DeferredMessageProcessorTask : IBackgroundTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var deferredMessages = await _messageQueue.GetDeferredMessagesAsync();
        foreach (var message in deferredMessages)
        {
            await _communications.SendAsync(message);
        }
    }
}
```

**Epic 7: Identity & Session Management**
```csharp
// Session cleanup task
public class SessionCleanupTask : IBackgroundTask
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _sessionService.CleanupExpiredSessionsAsync();
    }
}
```

---

### What We're Providing

- ✅ `IBackgroundTask` abstraction (business logic)
- ✅ `IBackgroundTaskScheduler` abstraction (platform scheduling)
- ✅ Implementations for:
  - ASP.NET Core (`IHostedService`)
  - Azure Functions (Timer Trigger)
  - AWS Lambda (EventBridge)
  - Windows Services (System.Threading.Timer)
  - Linux Daemons (systemd timers)
  - Quartz.NET (self-hosted)
  - Hangfire (self-hosted)
- ✅ Extension methods for registration
- ✅ Cron expression support (standardized)

---

### What We're NOT Doing

- ❌ **Task Queue Processing** - Use existing libraries (Azure Service Bus, RabbitMQ)
- ❌ **Workflow Orchestration** - Use Azure Durable Functions, AWS Step Functions
- ❌ **Real-Time Processing** - Use SignalR, WebSockets
- ❌ **Event Sourcing** - Use EventStore, Marten

---

## Revision 13: Injectable Validation with Provider Pattern

**Feedback:** "Validations should be injectable like everything else... if someone wants to configuration validations though component models, fluent validation or whatever you would just need an implementation that can be injected where required. Along the lines of model validation for asp.net"

### Problem ❌
Hard-coded validation frameworks create limitations:
- Locked into DataAnnotations or FluentValidation
- Cannot switch validation frameworks without code changes
- Different epics use different validation approaches
- Cannot customize validation behavior per deployment
- ASP.NET model validation not reusable outside web context

---

### Revised Design ✅

**Core Principle:** Validation is injectable via provider pattern, similar to ASP.NET `IModelValidator` but framework-agnostic.

**Validation Abstraction:**
```csharp
/// <summary>
/// Provider-based validation abstraction.
/// Implementations: DataAnnotations, FluentValidation, custom validators.
/// </summary>
public interface IValidator<T>
{
    /// <summary>
    /// Validates object and returns validation result.
    /// </summary>
    Task<ValidationResult> ValidateAsync(T instance, ValidationContext? context = null);

    /// <summary>
    /// Validates specific property.
    /// </summary>
    Task<ValidationResult> ValidatePropertyAsync(T instance, string propertyName, ValidationContext? context = null);
}

/// <summary>
/// Validation result with errors.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; set; }
    public ICollection<ValidationError> Errors { get; set; } = new List<ValidationError>();
}

public class ValidationError
{
    public string PropertyName { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public string ErrorCode { get; set; } = "";
    public object? AttemptedValue { get; set; }
}

/// <summary>
/// Optional context for validation (similar to ASP.NET ModelBindingContext).
/// </summary>
public class ValidationContext
{
    public IDictionary<string, object> Items { get; set; } = new Dictionary<string, object>();
    public string? MemberName { get; set; }
    public object? ObjectInstance { get; set; }
}
```

---

### Validation Provider Implementations

**1. DataAnnotations Validator (Component Model)**
```csharp
public class DataAnnotationsValidator<T> : IValidator<T>
{
    public Task<ValidationResult> ValidateAsync(T instance, ValidationContext? context = null)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(instance);
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

        var isValid = Validator.TryValidateObject(
            instance,
            validationContext,
            validationResults,
            validateAllProperties: true);

        var errors = validationResults.Select(vr => new ValidationError
        {
            PropertyName = vr.MemberNames.FirstOrDefault() ?? "",
            ErrorMessage = vr.ErrorMessage ?? "",
            ErrorCode = "DataAnnotationError"
        }).ToList();

        return Task.FromResult(new ValidationResult
        {
            IsValid = isValid,
            Errors = errors
        });
    }

    public Task<ValidationResult> ValidatePropertyAsync(T instance, string propertyName, ValidationContext? context = null)
    {
        var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(instance)
        {
            MemberName = propertyName
        };

        var property = typeof(T).GetProperty(propertyName);
        var value = property?.GetValue(instance);

        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        var isValid = Validator.TryValidateProperty(value, validationContext, validationResults);

        var errors = validationResults.Select(vr => new ValidationError
        {
            PropertyName = propertyName,
            ErrorMessage = vr.ErrorMessage ?? "",
            ErrorCode = "DataAnnotationError",
            AttemptedValue = value
        }).ToList();

        return Task.FromResult(new ValidationResult
        {
            IsValid = isValid,
            Errors = errors
        });
    }
}
```

**Usage with DataAnnotations:**
```csharp
public class CreateAccountRequest
{
    [Required(ErrorMessage = "Username is required")]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be 3-50 characters")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    public string Email { get; set; } = "";

    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }
}

// Validation
var validator = new DataAnnotationsValidator<CreateAccountRequest>();
var result = await validator.ValidateAsync(request);

if (!result.IsValid)
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"{error.PropertyName}: {error.ErrorMessage}");
    }
}
```

---

**2. FluentValidation Adapter**
```csharp
public class FluentValidationAdapter<T> : IValidator<T>
{
    private readonly IValidator<T> _fluentValidator;

    public FluentValidationAdapter(IValidator<T> fluentValidator)
    {
        _fluentValidator = fluentValidator;
    }

    public async Task<ValidationResult> ValidateAsync(T instance, ValidationContext? context = null)
    {
        var fluentResult = await _fluentValidator.ValidateAsync(instance);

        var errors = fluentResult.Errors.Select(failure => new ValidationError
        {
            PropertyName = failure.PropertyName,
            ErrorMessage = failure.ErrorMessage,
            ErrorCode = failure.ErrorCode,
            AttemptedValue = failure.AttemptedValue
        }).ToList();

        return new ValidationResult
        {
            IsValid = fluentResult.IsValid,
            Errors = errors
        };
    }

    public async Task<ValidationResult> ValidatePropertyAsync(T instance, string propertyName, ValidationContext? context = null)
    {
        var fluentResult = await _fluentValidator.ValidateAsync(instance, options =>
        {
            options.IncludeProperties(propertyName);
        });

        var errors = fluentResult.Errors
            .Where(e => e.PropertyName == propertyName)
            .Select(failure => new ValidationError
            {
                PropertyName = failure.PropertyName,
                ErrorMessage = failure.ErrorMessage,
                ErrorCode = failure.ErrorCode,
                AttemptedValue = failure.AttemptedValue
            }).ToList();

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }
}
```

**Usage with FluentValidation:**
```csharp
// FluentValidation validator definition
public class CreateAccountRequestValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .Length(3, 50).WithMessage("Username must be 3-50 characters");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{1,14}$").When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format");
    }
}

// Adapter registration
var fluentValidator = new CreateAccountRequestValidator();
var validator = new FluentValidationAdapter<CreateAccountRequest>(fluentValidator);

var result = await validator.ValidateAsync(request);
```

---

**3. Custom Validator Implementation**
```csharp
public class CustomBusinessRuleValidator<T> : IValidator<T>
{
    private readonly List<Func<T, ValidationContext?, Task<ValidationError?>>> _rules = new();

    public CustomBusinessRuleValidator<T> AddRule(
        string propertyName,
        Func<T, ValidationContext?, Task<bool>> predicate,
        string errorMessage,
        string errorCode = "CustomRule")
    {
        _rules.Add(async (instance, context) =>
        {
            var isValid = await predicate(instance, context);
            if (!isValid)
            {
                return new ValidationError
                {
                    PropertyName = propertyName,
                    ErrorMessage = errorMessage,
                    ErrorCode = errorCode
                };
            }
            return null;
        });
        return this;
    }

    public async Task<ValidationResult> ValidateAsync(T instance, ValidationContext? context = null)
    {
        var errors = new List<ValidationError>();

        foreach (var rule in _rules)
        {
            var error = await rule(instance, context);
            if (error != null)
            {
                errors.Add(error);
            }
        }

        return new ValidationResult
        {
            IsValid = errors.Count == 0,
            Errors = errors
        };
    }

    public Task<ValidationResult> ValidatePropertyAsync(T instance, string propertyName, ValidationContext? context = null)
    {
        // Filter rules by property name (simplified)
        return ValidateAsync(instance, context);
    }
}
```

**Usage with Custom Rules:**
```csharp
var validator = new CustomBusinessRuleValidator<CreateAccountRequest>()
    .AddRule("Username", async (req, ctx) =>
    {
        // Custom business rule: Check if username is available
        return await _accountRepository.IsUsernameAvailableAsync(req.Username);
    }, "Username already taken", "UsernameTaken")
    .AddRule("Email", async (req, ctx) =>
    {
        // Custom business rule: Check if email is in allowed domain
        return req.Email.EndsWith("@company.com");
    }, "Email must be company domain", "InvalidDomain");

var result = await validator.ValidateAsync(request);
```

---

### Validation Service (Composite Pattern)

**Validator Factory:**
```csharp
public interface IValidatorFactory
{
    /// <summary>
    /// Gets validator for type T.
    /// Returns composite validator if multiple validators registered.
    /// </summary>
    IValidator<T> GetValidator<T>();

    /// <summary>
    /// Registers validator provider.
    /// </summary>
    void RegisterValidator<T>(IValidator<T> validator);
}

public class ValidatorFactory : IValidatorFactory
{
    private readonly Dictionary<Type, object> _validators = new();

    public IValidator<T> GetValidator<T>()
    {
        if (_validators.TryGetValue(typeof(T), out var validator))
        {
            return (IValidator<T>)validator;
        }

        // Return no-op validator if none registered
        return new NoOpValidator<T>();
    }

    public void RegisterValidator<T>(IValidator<T> validator)
    {
        if (_validators.ContainsKey(typeof(T)))
        {
            // Wrap in composite validator
            var existing = (IValidator<T>)_validators[typeof(T)];
            _validators[typeof(T)] = new CompositeValidator<T>(existing, validator);
        }
        else
        {
            _validators[typeof(T)] = validator;
        }
    }
}

/// <summary>
/// Composite validator - runs multiple validators in sequence.
/// </summary>
public class CompositeValidator<T> : IValidator<T>
{
    private readonly List<IValidator<T>> _validators;

    public CompositeValidator(params IValidator<T>[] validators)
    {
        _validators = validators.ToList();
    }

    public async Task<ValidationResult> ValidateAsync(T instance, ValidationContext? context = null)
    {
        var allErrors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidateAsync(instance, context);
            if (!result.IsValid)
            {
                allErrors.AddRange(result.Errors);
            }
        }

        return new ValidationResult
        {
            IsValid = allErrors.Count == 0,
            Errors = allErrors
        };
    }

    public async Task<ValidationResult> ValidatePropertyAsync(T instance, string propertyName, ValidationContext? context = null)
    {
        var allErrors = new List<ValidationError>();

        foreach (var validator in _validators)
        {
            var result = await validator.ValidatePropertyAsync(instance, propertyName, context);
            if (!result.IsValid)
            {
                allErrors.AddRange(result.Errors);
            }
        }

        return new ValidationResult
        {
            IsValid = allErrors.Count == 0,
            Errors = allErrors
        };
    }
}

public class NoOpValidator<T> : IValidator<T>
{
    public Task<ValidationResult> ValidateAsync(T instance, ValidationContext? context = null)
    {
        return Task.FromResult(new ValidationResult { IsValid = true });
    }

    public Task<ValidationResult> ValidatePropertyAsync(T instance, string propertyName, ValidationContext? context = null)
    {
        return Task.FromResult(new ValidationResult { IsValid = true });
    }
}
```

---

### Registration and Usage

**Startup Registration:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register validator factory
    services.AddSingleton<IValidatorFactory, ValidatorFactory>();

    // Option 1: DataAnnotations (Component Model)
    services.AddSingleton<IValidator<CreateAccountRequest>>(
        new DataAnnotationsValidator<CreateAccountRequest>());

    // Option 2: FluentValidation
    services.AddSingleton<FluentValidation.IValidator<CreateAccountRequest>, CreateAccountRequestValidator>();
    services.AddSingleton<IValidator<CreateAccountRequest>>(sp =>
    {
        var fluentValidator = sp.GetRequiredService<FluentValidation.IValidator<CreateAccountRequest>>();
        return new FluentValidationAdapter<CreateAccountRequest>(fluentValidator);
    });

    // Option 3: Composite (both DataAnnotations + FluentValidation)
    services.AddSingleton<IValidator<CreateAccountRequest>>(sp =>
    {
        var dataAnnotations = new DataAnnotationsValidator<CreateAccountRequest>();
        var fluentValidator = sp.GetRequiredService<FluentValidation.IValidator<CreateAccountRequest>>();
        var fluentAdapter = new FluentValidationAdapter<CreateAccountRequest>(fluentValidator);

        return new CompositeValidator<CreateAccountRequest>(dataAnnotations, fluentAdapter);
    });

    // Register validators in factory
    services.AddSingleton(sp =>
    {
        var factory = sp.GetRequiredService<IValidatorFactory>();
        factory.RegisterValidator(sp.GetRequiredService<IValidator<CreateAccountRequest>>());
        return factory;
    });
}
```

---

### Integration with Existing Epics

**Epic 7: Identity & Session Management**
```csharp
public class AccountManagementService : IAccountManagementService
{
    private readonly IValidator<CreateAccountRequest> _validator;
    private readonly IAccountRepository _repository;

    public AccountManagementService(
        IValidator<CreateAccountRequest> validator,  // Injected validator
        IAccountRepository repository)
    {
        _validator = validator;
        _repository = repository;
    }

    public async Task<Account> CreateAccountAsync(CreateAccountRequest request)
    {
        // Validate using injected validator (DataAnnotations, FluentValidation, or custom)
        var validationResult = await _validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // Create account
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Username = request.Username,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Status = AccountStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _repository.SaveAsync(account);
        return account;
    }
}
```

**Epic 5: Master Data & Test Data Management**
```csharp
public interface IMasterDataLoader
{
    Task<LoadResult> LoadMasterDataAsync(string dataStoreName, MasterDataSet dataSet);
    Task<ValidationResult> ValidateMasterDataAsync(MasterDataSet dataSet);  // Uses injected validator
}

public class MasterDataLoader : IMasterDataLoader
{
    private readonly IValidator<MasterDataSet> _validator;

    public MasterDataLoader(IValidator<MasterDataSet> validator)
    {
        _validator = validator;
    }

    public async Task<ValidationResult> ValidateMasterDataAsync(MasterDataSet dataSet)
    {
        // Use injected validator (can be DataAnnotations, FluentValidation, custom)
        return await _validator.ValidateAsync(dataSet);
    }

    public async Task<LoadResult> LoadMasterDataAsync(string dataStoreName, MasterDataSet dataSet)
    {
        // Validate before loading
        var validationResult = await ValidateMasterDataAsync(dataSet);
        if (!validationResult.IsValid)
        {
            return new LoadResult
            {
                Success = false,
                Errors = validationResult.Errors.Select(e => e.ErrorMessage).ToList()
            };
        }

        // Load data
        // ...
    }
}
```

**Epic 2: Communications Platform**
```csharp
public class CommunicationsService : ICommunicationsService
{
    private readonly IValidator<IMessage> _messageValidator;
    private readonly IValidator<IChannel> _channelValidator;

    public async Task<SendResult> SendAsync(Guid userId, IMessage message, string channelName, SendOptions? options = null)
    {
        // Validate message using injected validator
        var messageValidation = await _messageValidator.ValidateAsync(message);
        if (!messageValidation.IsValid)
        {
            return new SendResult
            {
                Success = false,
                Errors = messageValidation.Errors.Select(e => e.ErrorMessage).ToList()
            };
        }

        // Send message
        // ...
    }

    public async Task RegisterChannelAsync(IChannel channel)
    {
        // Validate channel configuration
        var channelValidation = await _channelValidator.ValidateAsync(channel);
        if (!channelValidation.IsValid)
        {
            throw new ValidationException(channelValidation.Errors);
        }

        // Register channel
        // ...
    }
}
```

**Epic 6: Document Services**
```csharp
public class DocumentConversionService : IDocumentConversionService
{
    private readonly IValidator<Document> _documentValidator;
    private readonly IValidator<ConversionContext> _contextValidator;

    public async Task<ConvertedDocument> ConvertAsync(
        Document source,
        string targetMediaType,
        ConversionContext? context = null)
    {
        // Validate document
        var docValidation = await _documentValidator.ValidateAsync(source);
        if (!docValidation.IsValid)
        {
            throw new ValidationException(docValidation.Errors);
        }

        // Validate context (if provided)
        if (context != null)
        {
            var ctxValidation = await _contextValidator.ValidateAsync(context);
            if (!ctxValidation.IsValid)
            {
                throw new ValidationException(ctxValidation.Errors);
            }
        }

        // Convert document
        // ...
    }
}
```

---

### ASP.NET Integration (Model Validation)

**ASP.NET Model Validator Adapter:**
```csharp
/// <summary>
/// Adapter that bridges our IValidator to ASP.NET IModelValidator.
/// Allows using our validators in ASP.NET model binding pipeline.
/// </summary>
public class ModelValidatorAdapter : IModelValidator
{
    private readonly IValidatorFactory _validatorFactory;

    public ModelValidatorAdapter(IValidatorFactory validatorFactory)
    {
        _validatorFactory = validatorFactory;
    }

    public IEnumerable<ModelValidationResult> Validate(ModelValidationContext context)
    {
        var validatorType = typeof(IValidator<>).MakeGenericType(context.ModelMetadata.ModelType);
        var getValidatorMethod = typeof(IValidatorFactory).GetMethod(nameof(IValidatorFactory.GetValidator))
            .MakeGenericMethod(context.ModelMetadata.ModelType);

        var validator = getValidatorMethod.Invoke(_validatorFactory, null);
        var validateMethod = validatorType.GetMethod(nameof(IValidator<object>.ValidateAsync));

        var validationTask = (Task)validateMethod.Invoke(validator, new[] { context.Model, null });
        validationTask.Wait();

        var resultProperty = validationTask.GetType().GetProperty("Result");
        var validationResult = (ValidationResult)resultProperty.GetValue(validationTask);

        return validationResult.Errors.Select(e => new ModelValidationResult(e.PropertyName, e.ErrorMessage));
    }
}

// Registration
public void ConfigureServices(IServiceCollection services)
{
    services.AddSingleton<IValidatorFactory, ValidatorFactory>();

    // Register ASP.NET adapter
    services.AddSingleton<IModelValidatorProvider>(sp =>
    {
        var factory = sp.GetRequiredService<IValidatorFactory>();
        return new ModelValidatorProviderAdapter(factory);
    });
}
```

**ASP.NET Controller Usage:**
```csharp
[ApiController]
[Route("api/accounts")]
public class AccountsController : ControllerBase
{
    private readonly IAccountManagementService _accountService;

    [HttpPost]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
    {
        // ASP.NET automatically validates using our IValidator via adapter
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var account = await _accountService.CreateAccountAsync(request);
        return CreatedAtAction(nameof(GetAccount), new { id = account.Id }, account);
    }
}
```

---

### Benefits

- ✅ **Injectable** - Validators injected via DI, not hard-coded
- ✅ **Provider Pattern** - Multiple validation frameworks supported
- ✅ **Framework Agnostic** - Works outside ASP.NET (console apps, background tasks, etc.)
- ✅ **Composable** - Combine DataAnnotations + FluentValidation + Custom rules
- ✅ **ASP.NET Compatible** - Integrates with ASP.NET model validation pipeline
- ✅ **Testable** - Mock validators for unit tests
- ✅ **Consistent** - Same validation interface across all epics
- ✅ **Extensible** - Add custom validators without changing existing code

---

### What We're Providing

- ✅ `IValidator<T>` abstraction
- ✅ `ValidationResult` and `ValidationError` types
- ✅ `IValidatorFactory` for validator management
- ✅ Implementations:
  - DataAnnotationsValidator (Component Model)
  - FluentValidationAdapter (FluentValidation)
  - CustomBusinessRuleValidator (Custom rules)
  - CompositeValidator (Multiple validators)
- ✅ ASP.NET integration adapter (`IModelValidator`)
- ✅ Extension methods for registration

---

### What We're NOT Doing

- ❌ **Replacing Existing Validation Libraries** - Use DataAnnotations, FluentValidation as-is
- ❌ **Custom Validation DSL** - Use industry-standard approaches
- ❌ **Client-Side Validation** - Use existing JavaScript validation libraries
- ❌ **Validation UI** - Use ASP.NET validation tag helpers, Blazor validators

---

## Summary of Changes

| Aspect | Original | Revised | Benefit |
|--------|----------|---------|---------|
| **Data Container** | `IMessageData` | `IDataContainer` | Generic, reusable |
| **Navigation** | Dot notation | XPath-like (`/` separators) | Industry standard |
| **Evaluation** | Eager (all providers execute) | Lazy (on-demand) | 50-70% performance gain |
| **Path Translation** | N/A | XPath ↔ JSONPath ↔ Dot Notation | Template engine interop |
| **Template Engine** | Custom implementation | Leverage existing + Handlebars | No reinventing wheel |
| **Template/Conversion** | Templates handle all formats | Templates → native, Converters → transform | Separation of concerns |
| **Document Mgmt** | Monolithic (911 LOC) | 3 features (900 LOC) | Composable, SRP |
| **Communications** | Email/SMS only, send-only | Multi-channel (Protocol + Provider + Name), send & receive | Extensible, complete |
| **Service Architecture** | Unclear dependencies | Standalone services + Optional orchestrations | No hard dependencies |
| **Caching** | Manual cache logic | Transparent AOP/Dynamic Proxy with attributes | Declarative, centralized |
| **Data Loading** | ETL pipeline | Master Data & Test Data Management | Production setup + Testing focus |
| **Document Services** | 3 features (Persist, Convert, Pack) | 11 context-based services | Comprehensive, flexible |
| **Identity & Session** | Monolithic profiles | Modular profile system with `IProfileProvider` | Extensible, feature-owned |
| **Background Processes** | Platform-specific (`IHostedService`) | Platform-agnostic (`IBackgroundTask`) | Deploy anywhere |
| **Validation** | Hard-coded (DataAnnotations OR FluentValidation) | Injectable `IValidator<T>` with provider pattern | Composable, testable, framework-agnostic |

---

## Revised Epic Priorities

### Foundation (Weeks 1-2)
1. **Epic 11: Data Enhancement Pipeline** - Generic, lazy-evaluated data container
2. **Epic 10: Text Templating Extensions** - Handlebars provider, IDataContainer integration

### Core Services (Weeks 3-4)
3. **Epic 12: Message Composition Service** - Combines Epic 11 + Epic 10
4. **Epic 2: Communications Platform** - Simplified routing and delivery

### Domain Features (Weeks 5-7)
5. Epic 3: Spatial Services
6. Epic 5: Data Loading Pipeline
7. Epic 6: Document Management (3 features)

### Advanced Features (Weeks 8-10)
8. Epic 7: Identity & Session
9. Epic 8: Complex Events
10. Epic 9: Test Data Generation

---

## Architectural Principles Applied

1. ✅ **Generic over Specific** - `IDataContainer` not `IMessageData`
2. ✅ **Industry Standards** - XPath navigation, Handlebars templates
3. ✅ **Lazy Evaluation** - Load data only when needed
4. ✅ **Leverage Existing** - Don't replace what works
5. ✅ **Separation of Concerns** - Split monolithic components
6. ✅ **Composability** - Features work independently or together

---

## Next Steps

**AWAITING USER FEEDBACK on:**
1. Remaining epic structure (Epic 2-9)
2. Feature granularity for other epics
3. Missing features/epics
4. Priority order

**After feedback, continue with:**
1. Detailed documentation for Epic 11 (Data Enhancement)
2. Detailed documentation for Epic 10 (Text Templating)
3. Design Epic 12 (Message Composition Service)
4. Complete remaining epics

---

## Related Documents

- [Epic Review](./EPIC_REVIEW.md) - Original epic breakdown (awaiting feedback)
- [Architectural Improvements](./ARCHITECTURAL_IMPROVEMENTS.md) - Comparison with SharedFramework
- [Epic 11: Data Enhancement (Revised)](./11-DataEnhancement/README-REVISED.md)
- [Epic 10: Text Templating (Revised)](./10-TextTemplating/README-REVISED.md)
- [Epic 2: Communications (Revised)](./02-Communications/README-REVISED.md)
