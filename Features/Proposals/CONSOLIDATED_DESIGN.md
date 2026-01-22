# Consolidated Design - Final Architecture

**Date:** 2026-01-22
**Status:** ✅ Complete - All User Feedback Incorporated (13 Revisions)
**Purpose:** Single source of truth for final architecture after all feedback

---

## Epic 11: Data Enhancement Pipeline

### Core Innovation: Generic, Lazy-Evaluated Data Container

**Key Interfaces:**
```csharp
// Main container
public interface IDataContainer
{
    IDataNode Root { get; }
    IDataNode Navigate(string path);
    object? Evaluate(string path);
    T? Evaluate<T>(string path);
    void RegisterProvider(string pathPattern, IDataProvider provider);
}

// Navigator pattern (like XPathNavigator)
public interface IDataNode
{
    string Path { get; }
    string Name { get; }
    object? Value { get; }  // Triggers lazy loading

    IDataNode? SelectSingleNode(string relativePath);
    IEnumerable<IDataNode> SelectNodes(string pattern);

    IDataNode? Parent { get; }
    IEnumerable<IDataNode> Children { get; }
}

// Lazy data provider
public interface IDataProvider
{
    // ONLY executes when path is accessed
    Task<object?> ProvideAsync(IDataNode node, string context, IDictionary<string, object?>? metadata);
}
```

**Benefits:**
- ✅ **Generic** - Works for messages, reports, documents, exports (NOT message-specific)
- ✅ **Lazy** - Providers execute ONLY when path is accessed (50-70% query reduction)
- ✅ **XPath-like** - Industry-standard navigation pattern
- ✅ **Wildcard patterns** - `Customer/*/Address`, `**/LineItems`

---

### Path Syntax Translation (NEW)

**Key Innovation:** Modular translation between different path syntaxes

**Supported Syntaxes:**
```
XPath:        Customer/Address/City
JSONPath:     $.Customer.Address.City
Dot Notation: Customer.Address.City
```

**Key Interfaces:**
```csharp
// Path translator abstraction
public interface IPathTranslator
{
    string SyntaxType { get; }  // "xpath", "jsonpath", "dotnotation"

    ICanonicalPath Parse(string path);      // Syntax → Canonical
    string Format(ICanonicalPath canonical); // Canonical → Syntax
    bool CanParse(string path);              // Syntax detection
}

// Translation service
public interface IPathTranslationService
{
    void RegisterTranslator(IPathTranslator translator);
    string Translate(string path, string sourceSyntax, string targetSyntax);
    ICanonicalPath ParseAny(string path);  // Auto-detect syntax
}

// Canonical internal representation
public interface ICanonicalPath
{
    IReadOnlyList<IPathSegment> Segments { get; }
    bool IsAbsolute { get; }
}

public interface IPathSegment
{
    PathSegmentType Type { get; }  // Property, Index, Wildcard, RecursiveDescent
    string Value { get; }
    int? Index { get; }
}
```

**Benefits:**
- ✅ **Template engines use native syntax** - Handlebars uses `.`, XSLT uses `/`, JSONPath uses `$`
- ✅ **Automatic translation** - Container translates between syntaxes
- ✅ **Extensible** - Add new syntaxes via `IPathTranslator`
- ✅ **Provider flexibility** - Register with any syntax

**Translation Examples:**
```csharp
// JSONPath → XPath
"$.Customer.Orders[0].Total" → "Customer/Orders/0/Total"

// XPath → Dot Notation
"Customer/Address/City" → "Customer.Address.City"

// Dot Notation → JSONPath
"Customer.Orders.0.Total" → "$.Customer.Orders[0].Total"
```

**Template Engine Integration:**
```csharp
// Handlebars template (uses dot notation)
var template = "Hello {{Customer.FirstName}}!";

// Container (internal XPath)
var container = DataContainerFactory.Create();
container.RegisterProvider("Customer", customerProvider);

// Adapter translates Handlebars dot notation → Container XPath
var adapter = new HandlebarsTemplateAdapter(pathTranslation);
var result = Handlebars.Compile(template)(adapter.Adapt(container));
// Template accesses "Customer.FirstName" → Adapter translates to "Customer/FirstName" → Container evaluates
```

---

## Epic 10: Text Templating Extensions

### Leverage Existing Framework

**What Already Exists:**
```csharp
// ✅ Already in OoBDev.System.Text.Templating
public interface ITemplateEngine
{
    Task<string?> ApplyAsync(string templateName, object data);
}

public interface ITemplateProvider
{
    IReadOnlyCollection<string> SupportedContentTypes { get; }
    bool CanApply(ITemplateContext context);
    Task<bool> ApplyAsync(ITemplateContext context, object data, Stream target);
}

public interface ITemplateSource
{
    IEnumerable<ITemplateContext> GetTemplates();
}

// ✅ Already implemented
public class XsltTemplateProvider : ITemplateProvider { }
public class FileTemplateSource : ITemplateSource { }
```

**What We're Adding:**

### 1. Industry-Standard Template Providers

**Handlebars Provider (HIGH Priority)**
```csharp
public class HandlebarsTemplateProvider : ITemplateProvider
{
    public IReadOnlyCollection<string> SupportedContentTypes => new[] { "text/x-handlebars-template" };

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

**Liquid Provider (OPTIONAL - if Shopify compatibility needed)**
**Scriban Provider (OPTIONAL - if high performance needed)**

### 2. Additional Template Sources

**Database Template Source (HIGH Priority)**
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
            Source = new DatabaseTemplateContentSource(t.Id, _repository)
        });
    }
}
```

**Azure Blob Template Source (MEDIUM Priority)**
```csharp
public class AzureBlobTemplateSource : ITemplateSource
{
    private readonly BlobContainerClient _container;

    public IEnumerable<ITemplateContext> GetTemplates()
    {
        // List blobs in container, return as template contexts
    }
}
```

**Embedded Resource Template Source (LOW Priority)**
```csharp
public class EmbeddedResourceTemplateSource : ITemplateSource
{
    private readonly Assembly _assembly;

    public IEnumerable<ITemplateContext> GetTemplates()
    {
        // Get manifest resources from assembly
    }
}
```

**URL/API Template Source (OPTIONAL)**
```csharp
public class UrlTemplateSource : ITemplateSource
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;

    public IEnumerable<ITemplateContext> GetTemplates()
    {
        // Fetch templates from REST API
    }
}
```

### 3. IDataContainer Integration

**Adapter for Lazy Evaluation:**
```csharp
public static class TemplateEngineExtensions
{
    public static Task<string?> ApplyAsync(
        this ITemplateEngine engine,
        string templateName,
        IDataContainer data)
    {
        // Adapt IDataContainer for template engine
        var adapted = DataContainerAdapter.AdaptForTemplate(data);
        return engine.ApplyAsync(templateName, adapted);
    }
}
```

**Benefits:**
- ✅ Template engines work with lazy-evaluated containers
- ✅ Only load data paths template actually uses
- ✅ Massive performance improvement

### Template Organization

**File Structure:**
```
Templates/
├── email/
│   ├── order-confirmation.subject.hbs
│   ├── order-confirmation.html.hbs
│   └── order-confirmation.text.hbs
├── pdf/
│   ├── invoice.xslt
│   └── receipt.xslt
└── sms/
    └── order-shipped.hbs
```

**ContentType Conventions:**
```
Extension → ContentType → Provider
.hbs      → text/x-handlebars-template → HandlebarsTemplateProvider
.xslt     → application/xslt+xml       → XsltTemplateProvider
.liquid   → text/x-liquid              → LiquidTemplateProvider
```

**What We're NOT Doing:**
- ❌ Custom HTML template syntax
- ❌ Replacing existing template engine
- ❌ Custom template language

---

## Epic 2: Communications Platform (SIMPLIFIED)

### Core Responsibility: Send & Receive Messages via Channels

**Communications ONLY handles:**
1. ✅ **Channel abstraction** (Protocol + Provider + Name)
2. ✅ **Send messages** via channels (Email, SMS, Live Chat, Chatrooms, Push)
3. ✅ **Receive messages** from channels (webhooks, polling)
4. ✅ **User preferences** (quiet hours, weekends, holidays)
5. ✅ **Channel routing** (based on user preferences)
6. ✅ **Deferral scheduling** (respect quiet hours)
7. ✅ **Correlation tracking** (request/response matching)

**Communications does NOT handle:**
- ❌ Data enhancement (Epic 11)
- ❌ Template loading (Epic 10)
- ❌ Variable substitution (Epic 10)
- ❌ Message composition (Epic 12 - NEW)

---

### Channel Abstraction

**Channel Components:**
- **Protocol:** `email`, `sms`, `teams`, `slack`, `livechat`, `webhook`
- **Provider:** `sendgrid`, `twilio`, `microsoft-teams`, `slack-api`, `intercom`
- **Name:** User-friendly identifier (e.g., "Sales Team Slack", "Support Email")

**Channel Interface:**
```csharp
public interface IChannel
{
    /// <summary>
    /// Channel identifier (e.g., "sales-team-slack", "support-email")
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Communication protocol (e.g., "slack", "email", "sms", "teams")
    /// </summary>
    string Protocol { get; }

    /// <summary>
    /// Provider implementation (e.g., "slack-api", "sendgrid", "twilio")
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Channel-specific configuration (e.g., Slack workspace ID, email address)
    /// </summary>
    IDictionary<string, object> Configuration { get; }
}

public interface IChannelProvider
{
    string ProviderName { get; }  // "sendgrid", "twilio", "slack-api"
    string[] SupportedProtocols { get; }  // ["email"], ["sms"], ["slack"]

    Task<SendResult> SendAsync(IChannel channel, IMessage message);
    Task<IMessage?> ReceiveAsync(IChannel channel);  // Polling
    Task RegisterWebhookAsync(IChannel channel, string webhookUrl);  // Webhooks
}
```

**Channel Examples:**
```csharp
// Email channel via SendGrid
var emailChannel = new Channel
{
    Name = "support-email",
    Protocol = "email",
    Provider = "sendgrid",
    Configuration = new Dictionary<string, object>
    {
        ["FromAddress"] = "support@example.com",
        ["ApiKey"] = "SG.xxx"
    }
};

// SMS channel via Twilio
var smsChannel = new Channel
{
    Name = "alerts-sms",
    Protocol = "sms",
    Provider = "twilio",
    Configuration = new Dictionary<string, object>
    {
        ["FromNumber"] = "+15551234567",
        ["AccountSid"] = "ACxxx",
        ["AuthToken"] = "xxx"
    }
};

// Slack channel
var slackChannel = new Channel
{
    Name = "sales-team-slack",
    Protocol = "slack",
    Provider = "slack-api",
    Configuration = new Dictionary<string, object>
    {
        ["WorkspaceId"] = "T123456",
        ["ChannelId"] = "C789012",
        ["BotToken"] = "xoxb-xxx"
    }
};

// Microsoft Teams channel
var teamsChannel = new Channel
{
    Name = "engineering-teams",
    Protocol = "teams",
    Provider = "microsoft-teams",
    Configuration = new Dictionary<string, object>
    {
        ["TeamId"] = "abc123",
        ["ChannelId"] = "19:xxx@thread.tacv2",
        ["TenantId"] = "xxx"
    }
};
```

---

### User Preferences

**Quiet Hours, Weekends, Holidays:**
```csharp
public interface IUserCommunicationPreferences
{
    Guid UserId { get; }

    // Quiet hours (e.g., 9 PM - 7 AM)
    TimeSpan? QuietHoursStart { get; }
    TimeSpan? QuietHoursEnd { get; }

    // Weekend preferences
    bool AllowWeekendsDelivery { get; }

    // Holiday calendar
    string[] HolidayCalendars { get; }  // ["US-Federal", "Company-Holidays"]

    // Channel preferences (ordered by priority)
    string[] PreferredChannels { get; }  // ["email", "sms", "slack"]

    // Override settings (urgent messages)
    bool AllowUrgentOverride { get; }
}

public interface IUserPreferencesService
{
    Task<IUserCommunicationPreferences> GetPreferencesAsync(Guid userId);
    Task<bool> IsQuietTimeAsync(Guid userId, DateTimeOffset deliveryTime);
    Task<bool> IsHolidayAsync(Guid userId, DateTimeOffset date);
    Task<string> GetPreferredChannelAsync(Guid userId, MessagePriority priority);
}
```

---

### Communications Service Interface

```csharp
public interface ICommunicationsService
{
    // ===== SENDING =====

    /// <summary>
    /// Sends pre-formatted message via specified channel.
    /// Respects user preferences (quiet hours, holidays).
    /// </summary>
    Task<SendResult> SendAsync(Guid userId, IMessage message, string channelName, SendOptions? options = null);

    /// <summary>
    /// Sends message via user's preferred channel.
    /// </summary>
    Task<SendResult> SendViaPreferredChannelAsync(Guid userId, IMessage message, SendOptions? options = null);

    /// <summary>
    /// Deferred delivery (respects quiet hours/holidays).
    /// </summary>
    Task<SendResult> DeferSendAsync(Guid userId, IMessage message, DateTimeOffset deliveryTime, string channelName);

    // ===== RECEIVING =====

    /// <summary>
    /// Receives messages from channel (polling).
    /// </summary>
    Task<IEnumerable<IMessage>> ReceiveAsync(string channelName);

    /// <summary>
    /// Webhook handler for incoming messages.
    /// </summary>
    Task<WebhookResult> HandleWebhookAsync(string channelName, HttpRequest request);

    // ===== CHANNEL MANAGEMENT =====

    Task RegisterChannelAsync(IChannel channel);
    Task<IChannel?> GetChannelAsync(string channelName);
    Task<IEnumerable<IChannel>> GetChannelsAsync();
}

public class SendOptions
{
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    public bool OverrideQuietHours { get; set; } = false;  // Urgent messages
    public bool OverrideHolidays { get; set; } = false;
    public IDictionary<string, object>? Metadata { get; set; }
}
```

---

### Provider Implementations

**Email Provider (SendGrid):**
```csharp
public class SendGridProvider : IChannelProvider
{
    public string ProviderName => "sendgrid";
    public string[] SupportedProtocols => new[] { "email" };

    public async Task<SendResult> SendAsync(IChannel channel, IMessage message)
    {
        var emailMessage = (IEmailMessage)message;
        var apiKey = channel.Configuration["ApiKey"].ToString();

        var client = new SendGridClient(apiKey);
        var msg = MailHelper.CreateSingleEmail(
            from: new EmailAddress(channel.Configuration["FromAddress"].ToString()),
            to: new EmailAddress(emailMessage.ToAddress),
            subject: emailMessage.Subject,
            plainTextContent: emailMessage.TextContent,
            htmlContent: emailMessage.HtmlContent
        );

        var response = await client.SendEmailAsync(msg);

        return new SendResult
        {
            Success = response.IsSuccessStatusCode,
            MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
            ChannelName = channel.Name
        };
    }
}
```

**SMS Provider (Twilio):**
```csharp
public class TwilioProvider : IChannelProvider
{
    public string ProviderName => "twilio";
    public string[] SupportedProtocols => new[] { "sms" };

    public async Task<SendResult> SendAsync(IChannel channel, IMessage message)
    {
        var smsMessage = (ISmsMessage)message;

        TwilioClient.Init(
            channel.Configuration["AccountSid"].ToString(),
            channel.Configuration["AuthToken"].ToString()
        );

        var messageResource = await MessageResource.CreateAsync(
            to: new PhoneNumber(smsMessage.ToPhoneNumber),
            from: new PhoneNumber(channel.Configuration["FromNumber"].ToString()),
            body: smsMessage.Body
        );

        return new SendResult
        {
            Success = messageResource.Status != MessageResource.StatusEnum.Failed,
            MessageId = messageResource.Sid,
            ChannelName = channel.Name
        };
    }
}
```

**Slack Provider:**
```csharp
public class SlackProvider : IChannelProvider
{
    public string ProviderName => "slack-api";
    public string[] SupportedProtocols => new[] { "slack" };

    public async Task<SendResult> SendAsync(IChannel channel, IMessage message)
    {
        var slackMessage = (ISlackMessage)message;
        var botToken = channel.Configuration["BotToken"].ToString();

        var client = new SlackClient(botToken);
        var response = await client.Chat.PostMessageAsync(
            channel: channel.Configuration["ChannelId"].ToString(),
            text: slackMessage.Text,
            blocks: slackMessage.Blocks
        );

        return new SendResult
        {
            Success = response.Ok,
            MessageId = response.Ts,
            ChannelName = channel.Name
        };
    }

    public async Task<IMessage?> ReceiveAsync(IChannel channel)
    {
        // Poll for messages (or use webhooks)
        var botToken = channel.Configuration["BotToken"].ToString();
        var client = new SlackClient(botToken);

        var history = await client.Conversations.HistoryAsync(
            channel: channel.Configuration["ChannelId"].ToString(),
            limit: 1
        );

        if (history.Ok && history.Messages.Any())
        {
            var msg = history.Messages.First();
            return new SlackMessage
            {
                Text = msg.Text,
                User = msg.User,
                Timestamp = msg.Ts
            };
        }

        return null;
    }
}
```

---

### Usage Examples

**Example 1: Send via Preferred Channel**
```csharp
// 1. Compose message (Epic 12)
var message = await _composition.ComposeEmailAsync("order.confirmation", userId, data);

// 2. Send via user's preferred channel (Epic 2)
var result = await _communications.SendViaPreferredChannelAsync(userId, message);

// Flow:
// - Looks up user preferences
// - User prefers "email" channel
// - Checks quiet hours (9 PM - 7 AM)
// - Currently 10 PM → defer until 7 AM
// - Schedules delivery for 7 AM next day
```

**Example 2: Send Urgent Message (Override Quiet Hours)**
```csharp
var message = await _composition.ComposeSmsAsync("account.security-alert", userId, data);

var result = await _communications.SendAsync(
    userId: userId,
    message: message,
    channelName: "alerts-sms",
    options: new SendOptions
    {
        Priority = MessagePriority.Urgent,
        OverrideQuietHours = true  // Send immediately
    }
);
```

**Example 3: Receive Messages from Slack**
```csharp
// Webhook handler
[HttpPost("/webhooks/slack/{channelName}")]
public async Task<IActionResult> SlackWebhook(string channelName)
{
    var result = await _communications.HandleWebhookAsync(channelName, Request);

    if (result.Success)
    {
        // Process received message
        var message = result.ReceivedMessage;
        await _messageProcessor.ProcessAsync(message);
    }

    return Ok();
}
```

**Example 4: Multi-Channel Notification**
```csharp
var message = await _composition.ComposeMultiChannelAsync("system.maintenance", userId, data);

// Send to multiple channels
await _communications.SendAsync(userId, message.Email, "support-email");
await _communications.SendAsync(userId, message.Slack, "engineering-teams");
await _communications.SendAsync(userId, message.Teams, "ops-teams");
```

---

## Epic 4: Distributed Caching (Transparent AOP)

### Core Responsibility: Transparent Caching via Dynamic Proxy

**Caching is TRANSPARENT via:**
1. ✅ **Attribute-based declarative caching** - Developer declares caching behavior
2. ✅ **Dynamic proxy generation** - Castle DynamicProxy or DispatchProxy
3. ✅ **AOP interception** - Intercepts method calls automatically
4. ✅ **No manual cache logic** - Developer writes only business logic
5. ✅ **Extensible** - Custom attributes and interceptors

---

### Attribute-Based Declarative Caching

**Developer Experience (Transparent):**
```csharp
// Interface with caching attributes (DECLARATIVE)
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
    private readonly IOrderRepository _repository;

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

---

### Dynamic Proxy Registration

**Startup Configuration:**
```csharp
// Register service with transparent caching proxy
public void ConfigureServices(IServiceCollection services)
{
    // Register dependencies
    services.AddSingleton<IOrderRepository, OrderRepository>();
    services.AddSingleton<ICacheService, RedisCacheService>();

    // Option 1: Castle DynamicProxy (full-featured)
    services.AddCachedProxy<IOrderService, OrderService>(options =>
    {
        options.DefaultDuration = TimeSpan.FromMinutes(5);
        options.CacheProvider = CacheProvider.Redis;
        options.KeyPrefix = "app";
        options.EnableAsyncInterception = true;
    });

    // Option 2: DispatchProxy (built-in .NET, lighter)
    services.AddCachedService<IOrderService, OrderService>();

    // Option 3: Source Generators (compile-time, zero-overhead)
    services.AddCachedService<IOrderService, OrderService>()
        .UseSourceGeneratedProxy();
}
```

**Usage (Completely Transparent):**
```csharp
public class CheckoutController : ControllerBase
{
    private readonly IOrderService _orderService;

    public CheckoutController(IOrderService orderService)
    {
        _orderService = orderService;  // Injected PROXY (not real implementation)
    }

    [HttpGet("orders/{orderId}")]
    public async Task<Order> GetOrder(int orderId)
    {
        // First call: executes method, caches result
        // Second call: returns cached result (transparent to developer)
        return await _orderService.GetOrderAsync(orderId);
    }
}
```

---

### Caching Attributes

**Core Attributes:**
```csharp
[AttributeUsage(AttributeTargets.Method)]
public class CacheAttribute : Attribute
{
    /// <summary>
    /// Cache duration in seconds
    /// </summary>
    public int Duration { get; set; } = 300;

    /// <summary>
    /// Vary cache key by method parameters
    /// </summary>
    public bool VaryByParameters { get; set; } = true;

    /// <summary>
    /// Vary by current user ID
    /// </summary>
    public bool VaryByUser { get; set; } = false;

    /// <summary>
    /// Vary by current culture
    /// </summary>
    public bool VaryByCulture { get; set; } = false;

    /// <summary>
    /// Cache region/partition
    /// </summary>
    public string? Region { get; set; }

    /// <summary>
    /// Override default cache provider
    /// </summary>
    public CacheProvider? Provider { get; set; }

    /// <summary>
    /// Use sliding expiration instead of absolute
    /// </summary>
    public bool SlidingExpiration { get; set; } = false;

    /// <summary>
    /// Conditional caching (expression evaluated at runtime)
    /// </summary>
    public string? Condition { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class CacheInvalidateAttribute : Attribute
{
    /// <summary>
    /// Pattern to match cache keys (e.g., "order:*")
    /// </summary>
    public string? Pattern { get; set; }

    /// <summary>
    /// Specific cache keys to invalidate (supports placeholders: "order:{orderId}")
    /// </summary>
    public string[]? Keys { get; set; }

    /// <summary>
    /// Cache region to invalidate
    /// </summary>
    public string? Region { get; set; }
}

[AttributeUsage(AttributeTargets.Method)]
public class CacheEvictAttribute : Attribute
{
    /// <summary>
    /// Evict all caches
    /// </summary>
    public bool All { get; set; } = false;

    /// <summary>
    /// Specific regions to evict
    /// </summary>
    public string[]? Regions { get; set; }
}
```

---

### Interceptor Implementation

**Castle DynamicProxy Interceptor:**
```csharp
public class CacheInterceptor : IAsyncInterceptor
{
    private readonly ICacheService _cache;
    private readonly CacheProxyOptions _options;
    private readonly ILogger<CacheInterceptor> _logger;

    public void InterceptSynchronous(IInvocation invocation)
    {
        var cacheAttr = invocation.Method.GetCustomAttribute<CacheAttribute>();
        if (cacheAttr == null)
        {
            invocation.Proceed();
            return;
        }

        var cacheKey = BuildCacheKey(invocation.Method, invocation.Arguments, cacheAttr);

        // Check cache
        var cached = _cache.Get(cacheKey);
        if (cached != null)
        {
            invocation.ReturnValue = cached;
            _logger.LogDebug("Cache hit: {CacheKey}", cacheKey);
            return;
        }

        // Execute method
        invocation.Proceed();

        // Cache result
        var duration = TimeSpan.FromSeconds(cacheAttr.Duration);
        _cache.Set(cacheKey, invocation.ReturnValue, duration, cacheAttr.SlidingExpiration);
        _logger.LogDebug("Cache miss: {CacheKey}, cached for {Duration}", cacheKey, duration);
    }

    public void InterceptAsynchronous(IInvocation invocation)
    {
        invocation.ReturnValue = InterceptAsynchronousImpl(invocation);
    }

    private async Task<T> InterceptAsynchronousImpl<T>(IInvocation invocation)
    {
        var cacheAttr = invocation.Method.GetCustomAttribute<CacheAttribute>();
        if (cacheAttr == null)
        {
            invocation.Proceed();
            return await (Task<T>)invocation.ReturnValue;
        }

        var cacheKey = BuildCacheKey(invocation.Method, invocation.Arguments, cacheAttr);

        // Check cache
        var cached = await _cache.GetAsync<T>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Cache hit: {CacheKey}", cacheKey);
            return cached;
        }

        // Execute method
        invocation.Proceed();
        var result = await (Task<T>)invocation.ReturnValue;

        // Check condition (if specified)
        if (!string.IsNullOrEmpty(cacheAttr.Condition))
        {
            if (!EvaluateCondition(cacheAttr.Condition, result))
            {
                _logger.LogDebug("Condition not met, not caching: {CacheKey}", cacheKey);
                return result;
            }
        }

        // Cache result
        var duration = TimeSpan.FromSeconds(cacheAttr.Duration);
        await _cache.SetAsync(cacheKey, result, duration, cacheAttr.SlidingExpiration);
        _logger.LogDebug("Cache miss: {CacheKey}, cached for {Duration}", cacheKey, duration);

        return result;
    }

    private string BuildCacheKey(MethodInfo method, object[] arguments, CacheAttribute attr)
    {
        var keyBuilder = new StringBuilder();
        keyBuilder.Append(_options.KeyPrefix);
        keyBuilder.Append(':');
        keyBuilder.Append(method.DeclaringType?.Name);
        keyBuilder.Append(':');
        keyBuilder.Append(method.Name);

        if (attr.VaryByParameters && arguments.Length > 0)
        {
            foreach (var arg in arguments)
            {
                keyBuilder.Append(':');
                keyBuilder.Append(arg?.ToString() ?? "null");
            }
        }

        if (attr.VaryByUser)
        {
            keyBuilder.Append(':');
            keyBuilder.Append(_httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "anonymous");
        }

        if (attr.VaryByCulture)
        {
            keyBuilder.Append(':');
            keyBuilder.Append(CultureInfo.CurrentCulture.Name);
        }

        return keyBuilder.ToString();
    }
}
```

---

### Extension Methods for Registration

```csharp
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

        // Register implementation
        services.TryAddSingleton<TImplementation>();

        // Register interface with proxy
        services.AddSingleton<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var cache = provider.GetRequiredService<ICacheService>();
            var logger = provider.GetRequiredService<ILogger<CacheInterceptor>>();
            var httpContextAccessor = provider.GetService<IHttpContextAccessor>();

            var interceptor = new CacheInterceptor(cache, options, logger, httpContextAccessor);

            var proxyGenerator = new ProxyGenerator();
            return proxyGenerator.CreateInterfaceProxyWithTarget<TInterface>(
                implementation,
                new AsyncInterceptorAdapter(interceptor)
            );
        });

        return services;
    }

    public static IServiceCollection AddCachedService<TInterface, TImplementation>(
        this IServiceCollection services)
        where TInterface : class
        where TImplementation : class, TInterface
    {
        // DispatchProxy-based implementation (lighter, built-in .NET)
        services.TryAddSingleton<TImplementation>();

        services.AddSingleton<TInterface>(provider =>
        {
            var implementation = provider.GetRequiredService<TImplementation>();
            var cache = provider.GetRequiredService<ICacheService>();

            return CacheProxy<TInterface>.Create(implementation, cache);
        });

        return services;
    }
}
```

---

### Advanced Usage Examples

**Conditional Caching:**
```csharp
public interface IProductService
{
    // Only cache if product is published
    [Cache(Duration = 600, Condition = "result.IsPublished == true")]
    Task<Product> GetProductAsync(int productId);

    // Only cache if result count > 0
    [Cache(Duration = 300, Condition = "result.Count > 0")]
    Task<IEnumerable<Product>> SearchProductsAsync(string query);
}
```

**Multi-Level Caching:**
```csharp
public interface ICatalogService
{
    // Memory cache (fast, small)
    [Cache(Duration = 60, Provider = CacheProvider.Memory)]
    Task<IEnumerable<Category>> GetCategoriesAsync();

    // Distributed cache (slower, large)
    [Cache(Duration = 1800, Provider = CacheProvider.Redis)]
    Task<byte[]> GetProductImageAsync(int productId);

    // Hybrid (Memory + Redis)
    [Cache(Duration = 300, Provider = CacheProvider.Hybrid)]
    Task<Product> GetFeaturedProductAsync();
}
```

**Cache Invalidation:**
```csharp
public interface IInventoryService
{
    [Cache(Duration = 600)]
    Task<int> GetStockLevelAsync(int productId);

    // Invalidate specific product cache
    [CacheInvalidate(Keys = new[] { "InventoryService:GetStockLevelAsync:{productId}" })]
    Task UpdateStockLevelAsync(int productId, int quantity);

    // Invalidate all inventory caches
    [CacheInvalidate(Pattern = "InventoryService:*")]
    Task ResetAllInventoryAsync();

    // Evict entire region
    [CacheEvict(Regions = new[] { "inventory" })]
    Task RefreshInventoryDataAsync();
}
```

**Region-Based Caching:**
```csharp
public interface IReportService
{
    [Cache(Duration = 3600, Region = "reports")]
    Task<SalesReport> GetSalesReportAsync(DateTime startDate, DateTime endDate);

    [Cache(Duration = 1800, Region = "reports")]
    Task<InventoryReport> GetInventoryReportAsync();

    // Evict all reports at once
    [CacheEvict(Regions = new[] { "reports" })]
    Task InvalidateAllReportsAsync();
}
```

---

### Cache Warming and Preloading

**Application-Specific Pattern (No Special Infrastructure):**

Cache warming is application-specific - services simply call the cached tier and discard results:

```csharp
public class CacheWarmingService : BackgroundService
{
    private readonly IProductService _productService;
    private readonly ICatalogService _catalogService;
    private readonly ILogger<CacheWarmingService> _logger;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await WarmCachesAsync();
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache warming failed");
            }
        }
    }

    private async Task WarmCachesAsync()
    {
        _logger.LogInformation("Starting cache warming...");

        // Warm product categories (throws away result - just populates cache)
        _ = await _catalogService.GetCategoriesAsync();

        // Warm featured products
        _ = await _productService.GetFeaturedProductAsync();

        // Warm common product queries
        var commonProductIds = new[] { 1, 2, 3, 5, 10, 15, 20 };
        foreach (var productId in commonProductIds)
        {
            _ = await _productService.GetProductAsync(productId);
        }

        _logger.LogInformation("Cache warming complete");
    }
}

// Registration
services.AddHostedService<CacheWarmingService>();
```

**Startup Warming:**
```csharp
public class Startup
{
    public void Configure(IApplicationBuilder app, IHostApplicationLifetime lifetime)
    {
        // Warm caches on application startup
        lifetime.ApplicationStarted.Register(() =>
        {
            _ = Task.Run(async () =>
            {
                var warmingService = app.ApplicationServices.GetRequiredService<CacheWarmingService>();
                await warmingService.WarmCachesAsync();
            });
        });

        app.UseRouting();
        app.UseEndpoints(endpoints => endpoints.MapControllers());
    }
}
```

**Event-Driven Warming:**
```csharp
public class ProductEventHandler
{
    private readonly IProductService _productService;

    public async Task HandleProductPublishedAsync(ProductPublishedEvent evt)
    {
        // Product published - warm cache proactively
        _ = await _productService.GetProductAsync(evt.ProductId);

        // Result discarded - cache is now warm for future requests
    }
}
```

**Key Points:**
- ✅ **No special infrastructure** - Just call cached services
- ✅ **Results discarded** - `_ = await ...` throws away result
- ✅ **Cache populated as side effect** - Interceptor caches result
- ✅ **Application controls** - Warming logic is application-specific
- ✅ **Flexible timing** - Startup, periodic, event-driven

---

### Benefits

- ✅ **Transparent** - Developer writes zero cache logic
- ✅ **Declarative** - Attributes describe caching behavior
- ✅ **AOP/Dynamic Proxy** - Interception happens at runtime
- ✅ **Extensible** - Custom attributes, custom interceptors
- ✅ **Centralized** - Cache logic in one place (interceptor)
- ✅ **Testable** - Can inject real implementation for tests (bypass proxy)
- ✅ **Flexible** - Change caching strategy without touching code
- ✅ **Performance** - Zero allocation for cache hits
- ✅ **Multi-Provider** - Memory, Redis, Hybrid support
- ✅ **Cache Warming** - Application-specific, no special infrastructure

---

## Epic 12: Message Composition Service (NEW)

**Purpose:** Combines Epic 11 (Data Enhancement) + Epic 10 (Templates) + Epic 6 (Conversion) to produce pre-formatted messages

**Interface:**
```csharp
public interface IMessageCompositionService
{
    /// <summary>
    /// Composes email message with automatic format conversion.
    /// </summary>
    Task<IEmailMessage> ComposeEmailAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        string? requiredFormat = null);

    Task<ISmsMessage> ComposeSmsAsync(string messageType, Guid userId, IDataContainer data);

    Task<MultiChannelMessage> ComposeMultiChannelAsync(string messageType, Guid userId, IDataContainer data);
}
```

**Implementation with Template Rendering + Conversion:**
```csharp
public class MessageCompositionService : IMessageCompositionService
{
    private readonly IDataEnhancementPipeline _enhancement;
    private readonly ITemplateEngine _templates;
    private readonly IConversionPipeline _conversion;
    private readonly IPathTranslationService _pathTranslation;

    public async Task<IEmailMessage> ComposeEmailAsync(
        string messageType,
        Guid userId,
        IDataContainer data,
        string? requiredFormat = null)
    {
        // 1. Build data container with providers
        data.RegisterProvider("User", userProvider);
        data.RegisterProvider("System", systemProvider);

        // 2. Get user culture from enhanced data (lazy)
        var culture = data.Evaluate<CultureInfo>("User/Culture") ?? CultureInfo.CurrentCulture;

        // 3. Render template to native format (lazy evaluation)
        var rendered = await _templates.RenderAsync($"{messageType}.email", data);
        // rendered.MediaType might be "text/markdown" (template's native format)

        // 4. Convert if needed
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

        // 5. Return pre-formatted message
        return new EmailMessage
        {
            ToAddress = data.Evaluate<string>("User/Email"),
            Subject = rendered.Content,  // Subject from template
            HtmlContent = finalMediaType == "text/html" ? finalContent : null,
            TextContent = finalMediaType == "text/plain" ? finalContent : null,
            MessageType = messageType,
            RequestId = Guid.NewGuid()
        };
    }
}
```

**Usage Example:**
```csharp
// Markdown template → HTML email
var data = DataContainerFactory.Create(new { OrderId = 12345 });
data.RegisterProvider("Customer", customerProvider);
data.RegisterProvider("Order", orderProvider);

var email = await _composition.ComposeEmailAsync(
    messageType: "order.confirmation",
    userId: customerId,
    data: data,
    requiredFormat: "text/html"  // Template renders Markdown, auto-converts to HTML
);

// Flow:
// 1. Template renders to Markdown (native format)
// 2. Conversion pipeline: Markdown → HTML
// 3. Email message gets HTML content
```

---

## Epic 6: Document Services (11 Context-Based Services)

**NOT just 3 features** - Comprehensive document operations with context-based providers.

**All services are context-aware** - Applications provide operational context that providers use to adjust behavior.

---

### Overview of 11 Services

1. **Retrieval** - Get documents from storage
2. **Persistence** - Store documents with versioning
3. **Conversion** - Transform formats with chaining (PDF → Image → Thumbnail)
4. **Extraction** - Extract text from documents
5. **Rendering** - Render text to images
6. **Splitting** - Multipage to single pages or page sets
7. **Composition** - Pages/page sets back to multipage
8. **Packing** - Documents to archives (ZIP, RAR, etc.)
9. **Unpacking** - Archives to individual files
10. **Media Type Detection** - Detect document type (headers, fingerprinting)
11. **OCR** - Optical character recognition

---

### Context-Based Pattern (All Services)

**Every service accepts context:**
```csharp
// Pattern: All services have context parameter
Task<Result> OperationAsync(Parameters..., Context? context = null);

// Context allows application-specific customization
public class OperationContext
{
    public string? RequestingApplication { get; set; }  // Who is calling
    public IDictionary<string, object> AdditionalContext { get; set; }  // Custom properties
}
```

**Providers receive context and adjust:**
```csharp
public interface IDocumentConversionProvider
{
    Task<ConvertedDocument> ConvertAsync(
        Document source,
        string targetMediaType,
        ConversionContext context);  // ← Provider gets context
}

public class PdfToImageProvider : IDocumentConversionProvider
{
    public async Task<ConvertedDocument> ConvertAsync(Document source, string targetMediaType, ConversionContext context)
    {
        // Adjust based on context
        var dpi = context.DPI ?? 300;

        // Application-specific adjustments
        if (context.RequestingApplication == "thumbnail-generator")
        {
            dpi = 72;  // Lower for thumbnails
        }

        // Provider options from context
        if (context.ProviderOptions.TryGetValue("anti-aliasing", out var aa))
        {
            // Use custom setting
        }

        return await ConvertWithSettings(source, dpi, ...);
    }
}
```

---

### Service Interfaces (Summary)

**1. Retrieval**
```csharp
public interface IDocumentRetrievalService
{
    Task<Document> GetAsync(Guid documentId, RetrievalContext? context = null);
    Task<IEnumerable<Document>> QueryAsync(DocumentQuery query, RetrievalContext? context = null);
}
```

**2. Persistence**
```csharp
public interface IDocumentPersistenceService
{
    Task<Guid> SaveAsync(Document document, PersistenceContext? context = null);
    Task DeleteAsync(Guid documentId, PersistenceContext? context = null);
}
```

**3. Conversion (with Chaining)**
```csharp
public interface IDocumentConversionService
{
    Task<ConvertedDocument> ConvertAsync(Document source, string targetMediaType, ConversionContext? context = null);
    Task<ConvertedDocument> ConvertChainAsync(Document source, string[] targetMediaTypes, ConversionContext? context = null);
}
```

**4. Extraction**
```csharp
public interface IDocumentExtractionService
{
    Task<ExtractedText> ExtractTextAsync(Document document, ExtractionContext? context = null);
}
```

**5. Rendering**
```csharp
public interface IDocumentRenderingService
{
    Task<RenderedDocument> RenderToImageAsync(string textContent, string sourceMediaType, RenderingContext? context = null);
}
```

**6. Splitting**
```csharp
public interface IDocumentSplittingService
{
    Task<IEnumerable<Document>> SplitToPagesAsync(Document document, SplittingContext? context = null);
    Task<IEnumerable<Document>> SplitToPageSetsAsync(Document document, int pagesPerSet, SplittingContext? context = null);
}
```

**7. Composition**
```csharp
public interface IDocumentCompositionService
{
    Task<Document> ComposeMultipageAsync(IEnumerable<Document> pages, string targetMediaType, CompositionContext? context = null);
}
```

**8. Packing**
```csharp
public interface IDocumentPackingService
{
    Task<PackedDocument> PackAsync(IEnumerable<Document> documents, string archiveFormat, PackingContext? context = null);
}
```

**9. Unpacking**
```csharp
public interface IDocumentUnpackingService
{
    Task<IEnumerable<Document>> UnpackAsync(Document archive, UnpackingContext? context = null);
}
```

**10. Media Type Detection**
```csharp
public interface IMediaTypeDetectionService
{
    Task<MediaTypeResult> DetectAsync(Stream content, DetectionContext? context = null);
}
```

**11. OCR**
```csharp
public interface IOcrService
{
    Task<OcrResult> RecognizeTextAsync(Document document, OcrContext? context = null);
}
```

---

### Usage Example: Invoice Processing Pipeline

```csharp
public class InvoiceProcessingService
{
    public async Task ProcessInvoiceAsync(Document scannedInvoice)
    {
        var appContext = new { RequestingApplication = "invoice-processor", UserId = "system" };

        // 1. Detect media type
        var detected = await _mediaTypeDetection.DetectAsync(
            scannedInvoice.Content,
            new DetectionContext { RequestingApplication = appContext.RequestingApplication });

        // 2. Convert to PDF if needed (with context)
        if (detected.MediaType != "application/pdf")
        {
            scannedInvoice = await _conversion.ConvertAsync(
                scannedInvoice,
                "application/pdf",
                new ConversionContext
                {
                    RequestingApplication = appContext.RequestingApplication,
                    Quality = 95,
                    DPI = 300
                });
        }

        // 3. OCR (with context for language detection)
        var ocrResult = await _ocr.RecognizeTextAsync(
            scannedInvoice,
            new OcrContext
            {
                RequestingApplication = appContext.RequestingApplication,
                Languages = new[] { "eng", "fra" },
                DetectTables = true  // Invoices have tables
            });

        // 4. Extract text (with context)
        var extractedText = await _extraction.ExtractTextAsync(
            scannedInvoice,
            new ExtractionContext
            {
                RequestingApplication = appContext.RequestingApplication,
                PreserveFormatting = true
            });

        // 5. Store (with context for versioning)
        await _persistence.SaveAsync(
            scannedInvoice,
            new PersistenceContext
            {
                RequestingApplication = appContext.RequestingApplication,
                UserId = appContext.UserId,
                PreferredProvider = "azure-blob",
                EnableVersioning = true
            });
    }
}
```

---

### Benefits

- ✅ **Comprehensive** - 11 services covering all document operations
- ✅ **Context-Based** - Applications provide operational context
- ✅ **Standalone** - Each service works independently
- ✅ **Composable** - Chain services together
- ✅ **Provider Pattern** - Extensible implementations
- ✅ **Application-Aware** - Providers adjust based on requesting app
- ✅ **Flexible** - Per-request customization via context

---

## Epic 5: Master Data & Test Data Management

### Core Responsibility: Initialize Production Data Stores + Load Test Datasets

**NOT an ETL pipeline** - This is a master data initialization and test data management tool.

**Two Primary Use Cases:**
1. ✅ **Master Data Setup (Production)** - Initialize new tenants/environments with reference data
2. ✅ **Test Data Loading (Testing)** - Load reproducible test scenarios for integration/manual testing

---

### Master Data Loader

**Interface:**
```csharp
public interface IMasterDataLoader
{
    /// <summary>
    /// Loads master data into a new data store.
    /// Used when setting up new production environments/tenants.
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
```

**Master Data Set:**
```csharp
public class MasterDataSet
{
    public string Name { get; set; } = "";  // "reference-data-v1"
    public string Version { get; set; } = "";  // "1.0.0"
    public DataSetType Type { get; set; } = DataSetType.Master;

    // Data sources (JSON, CSV, SQL, Excel, etc.)
    public IEnumerable<DataSource> Sources { get; set; } = [];

    // Master data to load
    public IDictionary<string, object> Data { get; set; } = new Dictionary<string, object>
    {
        ["Countries"] = new[] { /* country data */ },
        ["States"] = new[] { /* state data */ },
        ["Currencies"] = new[] { /* currency data */ },
        ["TimeZones"] = new[] { /* timezone data */ },
        ["FeatureFlags"] = new[] { /* default feature flags */ },
        ["ConfigSettings"] = new[] { /* default configs */ }
    };
}
```

**Usage - New Production Tenant:**
```csharp
public class TenantProvisioningService
{
    public async Task ProvisionNewTenantAsync(string tenantId)
    {
        // 1. Create database
        await _databaseProvisioner.CreateDatabaseAsync($"tenant-{tenantId}-db");

        // 2. Run migrations
        await _migrationService.ApplyMigrationsAsync($"tenant-{tenantId}-db");

        // 3. Load master data (reference data, configs, feature flags)
        var masterData = await _masterDataLoader.GetAvailableMasterDataAsync()
            .FirstOrDefault(m => m.Name == "default-master-data");

        await _masterDataLoader.LoadMasterDataAsync($"tenant-{tenantId}-db", masterData);

        // Tenant ready for production use
    }
}
```

---

### Test Data Loader

**Interface:**
```csharp
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

**Test Data Set:**
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
            new { Id = 1000, CustomerId = 1, ProductId = 100, Quantity = 2 },
            new { Id = 1001, CustomerId = 2, ProductId = 101, Quantity = 1 }
        }
    };
}
```

**Usage - Integration Tests:**
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
        // Test data already loaded - known IDs from dataset
        var order = new CreateOrderRequest
        {
            CustomerId = 1,   // From test dataset
            ProductId = 100,  // From test dataset
            Quantity = 2
        };

        var result = await _orderService.CreateOrderAsync(order);
        Assert.IsTrue(result.Success);
    }

    [TestCleanup]
    public async Task Cleanup()
    {
        await _testDataLoader.ClearTestDataAsync("integration-test-db", "order-processing-scenarios");
    }
}
```

---

### Data Source Providers

**Support multiple data source formats:**

```csharp
public interface IDataSourceProvider
{
    Task<IDictionary<string, object>> LoadDataAsync(DataSource source);
    string[] SupportedSourceTypes { get; }
}

// JSON file provider
public class JsonFileDataSourceProvider : IDataSourceProvider
{
    public string[] SupportedSourceTypes => new[] { "json", "json-file" };
}

// CSV file provider
public class CsvFileDataSourceProvider : IDataSourceProvider
{
    public string[] SupportedSourceTypes => new[] { "csv" };
}

// SQL script provider
public class SqlScriptDataSourceProvider : IDataSourceProvider
{
    public string[] SupportedSourceTypes => new[] { "sql", "sql-script" };
}

// Excel file provider
public class ExcelFileDataSourceProvider : IDataSourceProvider
{
    public string[] SupportedSourceTypes => new[] { "xlsx", "xls", "excel" };
}

// Embedded resource provider
public class EmbeddedResourceDataSourceProvider : IDataSourceProvider
{
    public string[] SupportedSourceTypes => new[] { "embedded-resource" };
}
```

---

### What This IS

- ✅ **Master data initialization** - Setup new production tenants/environments
- ✅ **Test data management** - Reproducible test scenarios
- ✅ **Environment provisioning** - Part of tenant onboarding
- ✅ **Data versioning** - Track master/test data versions
- ✅ **Multiple formats** - JSON, CSV, SQL, Excel, embedded resources

### What This IS NOT

- ❌ **General ETL pipeline** - Not for ongoing data integration
- ❌ **Data warehousing** - Not for analytics/BI data loading
- ❌ **Real-time sync** - Not for continuous data synchronization
- ❌ **Data transformation** - Not a general transformation framework

---

### Benefits

- ✅ **Production Setup** - Initialize new tenants with reference data
- ✅ **Reproducible Tests** - Known test datasets for consistent testing
- ✅ **Data Versioning** - Track master/test data changes
- ✅ **Multiple Sources** - JSON, CSV, SQL, Excel support
- ✅ **Validation** - Validate data before loading
- ✅ **Cleanup** - Clear test data after testing
- ✅ **Snapshots** - Capture current state as test dataset

---

## Epic 7: Identity & Session Management (NEW)

### Core Responsibility: Account Management + Modular Profile System

**Identity & Session Management provides:**
1. ✅ **Account Management** - Create, update, delete user accounts
2. ✅ **Role & Claims Management** - Assign roles and claims to users
3. ✅ **Session Management** - Track active sessions, SSO, timeouts
4. ✅ **Modular Profile Management** - Component features advertise profile data

---

### Account Management Service

**Interface:**
```csharp
public interface IAccountManagementService
{
    /// <summary>
    /// Creates a new user account.
    /// </summary>
    Task<Account> CreateAccountAsync(CreateAccountRequest request);

    /// <summary>
    /// Updates existing account details.
    /// </summary>
    Task<Account> UpdateAccountAsync(Guid accountId, UpdateAccountRequest request);

    /// <summary>
    /// Deletes account (soft delete with retention period).
    /// </summary>
    Task DeleteAccountAsync(Guid accountId, AccountDeletionOptions options);

    /// <summary>
    /// Gets account by ID.
    /// </summary>
    Task<Account?> GetAccountAsync(Guid accountId);

    /// <summary>
    /// Searches accounts with filtering.
    /// </summary>
    Task<IEnumerable<Account>> SearchAccountsAsync(AccountSearchQuery query);

    /// <summary>
    /// Activates/deactivates account.
    /// </summary>
    Task SetAccountStatusAsync(Guid accountId, AccountStatus status);
}

public class Account
{
    public Guid Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string? PhoneNumber { get; set; }
    public AccountStatus Status { get; set; }  // Active, Suspended, Deleted
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset? LastLoginAt { get; set; }
    public DateTimeOffset? DeletedAt { get; set; }
    public IDictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
}
```

**Usage:**
```csharp
// Create new account
var account = await _accountService.CreateAccountAsync(new CreateAccountRequest
{
    Username = "john.doe",
    Email = "john@example.com",
    PhoneNumber = "+15551234567"
});

// Update account
await _accountService.UpdateAccountAsync(account.Id, new UpdateAccountRequest
{
    Email = "john.doe@example.com"
});

// Search accounts
var results = await _accountService.SearchAccountsAsync(new AccountSearchQuery
{
    Status = AccountStatus.Active,
    EmailDomain = "example.com"
});
```

---

### Role & Claims Management Service

**Interfaces:**
```csharp
public interface IRoleManagementService
{
    /// <summary>
    /// Creates a new role.
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
    Task<IEnumerable<Role>> GetRolesAsync(Guid accountId);

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
    Task RemoveClaimAsync(Guid accountId, string claimType, string? claimValue = null);

    /// <summary>
    /// Gets all claims for account.
    /// </summary>
    Task<IEnumerable<Claim>> GetClaimsAsync(Guid accountId);

    /// <summary>
    /// Checks if account has claim.
    /// </summary>
    Task<bool> HasClaimAsync(Guid accountId, string claimType, string? claimValue = null);
}

public class Role
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public IEnumerable<string> Permissions { get; set; } = [];
}

public class Claim
{
    public string Type { get; set; } = "";  // "department", "location", "clearance-level"
    public string Value { get; set; } = ""; // "engineering", "us-west", "secret"
}
```

**Usage:**
```csharp
// Assign role
await _roleService.AssignRoleAsync(accountId, "administrator");
await _roleService.AssignRoleAsync(accountId, "content-editor");

// Add claims
await _claimsService.AddClaimAsync(accountId, new Claim
{
    Type = "department",
    Value = "engineering"
});

await _claimsService.AddClaimAsync(accountId, new Claim
{
    Type = "location",
    Value = "us-west"
});

// Check authorization
var isAdmin = await _roleService.HasRoleAsync(accountId, "administrator");
var hasEngDept = await _claimsService.HasClaimAsync(accountId, "department", "engineering");
```

---

### Session Management Service

**Interface:**
```csharp
public interface ISessionManagementService
{
    /// <summary>
    /// Creates a new session for account.
    /// </summary>
    Task<Session> CreateSessionAsync(Guid accountId, SessionOptions options);

    /// <summary>
    /// Gets session by ID.
    /// </summary>
    Task<Session?> GetSessionAsync(string sessionId);

    /// <summary>
    /// Validates session (checks expiration, account status).
    /// </summary>
    Task<SessionValidationResult> ValidateSessionAsync(string sessionId);

    /// <summary>
    /// Refreshes session (extends timeout).
    /// </summary>
    Task<Session> RefreshSessionAsync(string sessionId);

    /// <summary>
    /// Terminates session.
    /// </summary>
    Task TerminateSessionAsync(string sessionId);

    /// <summary>
    /// Gets all active sessions for account.
    /// </summary>
    Task<IEnumerable<Session>> GetActiveSessionsAsync(Guid accountId);

    /// <summary>
    /// Terminates all sessions for account (force logout everywhere).
    /// </summary>
    Task TerminateAllSessionsAsync(Guid accountId);
}

public class Session
{
    public string Id { get; set; } = "";
    public Guid AccountId { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? LastActivityAt { get; set; }
    public string IpAddress { get; set; } = "";
    public string UserAgent { get; set; } = "";
    public SessionType Type { get; set; }  // Interactive, API, SSO
    public IDictionary<string, object> Claims { get; set; } = new Dictionary<string, object>();
}

public class SessionOptions
{
    public TimeSpan Timeout { get; set; } = TimeSpan.FromHours(8);
    public bool SlidingExpiration { get; set; } = true;
    public SessionType Type { get; set; } = SessionType.Interactive;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
}

public enum SessionType
{
    Interactive,  // User login via browser
    API,          // API key authentication
    SSO,          // Single Sign-On
    Impersonation // Admin impersonating user
}
```

**Usage:**
```csharp
// Create session on login
var session = await _sessionService.CreateSessionAsync(accountId, new SessionOptions
{
    Timeout = TimeSpan.FromHours(8),
    SlidingExpiration = true,
    IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
    UserAgent = httpContext.Request.Headers["User-Agent"]
});

// Validate session on each request
var validation = await _sessionService.ValidateSessionAsync(sessionId);
if (!validation.IsValid)
{
    return Unauthorized();
}

// Refresh session on activity
await _sessionService.RefreshSessionAsync(sessionId);

// Logout (terminate session)
await _sessionService.TerminateSessionAsync(sessionId);

// Force logout everywhere (security action)
await _sessionService.TerminateAllSessionsAsync(accountId);
```

---

### Modular Profile Management (KEY INNOVATION)

**Core Innovation:** Component features advertise profile data via providers

**Problem Solved:**
- Static user profiles can't accommodate feature-specific data
- Each component has different profile needs (schedules, contacts, defaults)
- Hard to extend without modifying core profile structure

**Solution:**
- `IProfileProvider` pattern
- Component features register profile providers
- Providers advertise their profile data (schedules, contacts, defaults, etc.)
- `AccountProfile` aggregates all registered modules

---

#### Profile Management Interface

```csharp
public interface IProfileManagementService
{
    /// <summary>
    /// Registers profile provider (called by component features during startup).
    /// </summary>
    void RegisterProfileProvider(IProfileProvider provider);

    /// <summary>
    /// Gets complete account profile (aggregates all registered providers).
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

    /// <summary>
    /// Gets profile schema (for UI generation, validation).
    /// </summary>
    Task<ProfileSchema> GetProfileSchemaAsync();
}

public class AccountProfile
{
    public Guid AccountId { get; set; }
    public Account Account { get; set; } = null!;

    /// <summary>
    /// Aggregated profile modules from all registered providers.
    /// Key: Module name (e.g., "schedules", "contacts", "defaults")
    /// Value: Module data from provider
    /// </summary>
    public IDictionary<string, object> Modules { get; set; } = new Dictionary<string, object>();
}
```

---

#### Profile Provider Interface

```csharp
public interface IProfileProvider
{
    /// <summary>
    /// Module name (e.g., "schedules", "contacts", "defaults", "communications")
    /// </summary>
    string ModuleName { get; }

    /// <summary>
    /// Display name for UI (e.g., "Work Schedules", "Contact Lists", "User Preferences")
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets profile data for account.
    /// </summary>
    Task<object> GetProfileDataAsync(Guid accountId);

    /// <summary>
    /// Updates profile data for account.
    /// </summary>
    Task UpdateProfileDataAsync(Guid accountId, object data);

    /// <summary>
    /// Schema for this module (used for UI generation, validation).
    /// </summary>
    ProfileModuleSchema Schema { get; }
}

public class ProfileModuleSchema
{
    public string ModuleName { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public IDictionary<string, PropertySchema> Properties { get; set; } = new Dictionary<string, PropertySchema>();
}

public class PropertySchema
{
    public string Name { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Type { get; set; } = "";  // "string", "int", "datetime", "object"
    public bool Required { get; set; }
    public object? DefaultValue { get; set; }
}
```

---

#### Example Providers

**1. Schedules Profile Provider**
```csharp
public class SchedulesProfileProvider : IProfileProvider
{
    public string ModuleName => "schedules";
    public string DisplayName => "Work Schedules";

    public ProfileModuleSchema Schema => new ProfileModuleSchema
    {
        ModuleName = "schedules",
        DisplayName = "Work Schedules",
        Properties = new Dictionary<string, PropertySchema>
        {
            ["TimeZone"] = new PropertySchema { Name = "TimeZone", DisplayName = "Time Zone", Type = "string", Required = true },
            ["WorkingHours"] = new PropertySchema { Name = "WorkingHours", DisplayName = "Working Hours", Type = "object" },
            ["WeekendDays"] = new PropertySchema { Name = "WeekendDays", DisplayName = "Weekend Days", Type = "array" }
        }
    };

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        // Load schedule data from database
        var schedules = await _scheduleRepository.GetSchedulesAsync(accountId);

        return new
        {
            TimeZone = schedules.TimeZone,
            WorkingHours = new
            {
                Start = schedules.WorkStartTime,
                End = schedules.WorkEndTime
            },
            WeekendDays = schedules.WeekendDays
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var scheduleData = (dynamic)data;
        await _scheduleRepository.UpdateSchedulesAsync(accountId, scheduleData);
    }
}
```

**2. Contacts Profile Provider**
```csharp
public class ContactsProfileProvider : IProfileProvider
{
    public string ModuleName => "contacts";
    public string DisplayName => "Contact Lists";

    public ProfileModuleSchema Schema => new ProfileModuleSchema
    {
        ModuleName = "contacts",
        DisplayName = "Contact Lists",
        Properties = new Dictionary<string, PropertySchema>
        {
            ["PhoneNumbers"] = new PropertySchema { Name = "PhoneNumbers", DisplayName = "Phone Numbers", Type = "array" },
            ["EmailAddresses"] = new PropertySchema { Name = "EmailAddresses", DisplayName = "Email Addresses", Type = "array" },
            ["SocialProfiles"] = new PropertySchema { Name = "SocialProfiles", DisplayName = "Social Profiles", Type = "object" }
        }
    };

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        var contacts = await _contactRepository.GetContactsAsync(accountId);

        return new
        {
            PhoneNumbers = contacts.PhoneNumbers,
            EmailAddresses = contacts.EmailAddresses,
            SocialProfiles = new
            {
                LinkedIn = contacts.LinkedInUrl,
                Twitter = contacts.TwitterHandle,
                GitHub = contacts.GitHubUsername
            }
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var contactData = (dynamic)data;
        await _contactRepository.UpdateContactsAsync(accountId, contactData);
    }
}
```

**3. User Defaults Profile Provider**
```csharp
public class UserDefaultsProfileProvider : IProfileProvider
{
    public string ModuleName => "defaults";
    public string DisplayName => "User Preferences";

    public ProfileModuleSchema Schema => new ProfileModuleSchema
    {
        ModuleName = "defaults",
        DisplayName = "User Preferences",
        Properties = new Dictionary<string, PropertySchema>
        {
            ["Language"] = new PropertySchema { Name = "Language", DisplayName = "Language", Type = "string", DefaultValue = "en-US" },
            ["Currency"] = new PropertySchema { Name = "Currency", DisplayName = "Currency", Type = "string", DefaultValue = "USD" },
            ["Theme"] = new PropertySchema { Name = "Theme", DisplayName = "UI Theme", Type = "string", DefaultValue = "light" },
            ["DateFormat"] = new PropertySchema { Name = "DateFormat", DisplayName = "Date Format", Type = "string", DefaultValue = "MM/dd/yyyy" }
        }
    };

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        var defaults = await _defaultsRepository.GetDefaultsAsync(accountId);

        return new
        {
            Language = defaults.Language,
            Currency = defaults.Currency,
            Theme = defaults.Theme,
            DateFormat = defaults.DateFormat,
            TimeFormat = defaults.TimeFormat
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var defaultsData = (dynamic)data;
        await _defaultsRepository.UpdateDefaultsAsync(accountId, defaultsData);
    }
}
```

**4. Communications Profile Provider (Epic 2 Integration)**
```csharp
public class CommunicationsProfileProvider : IProfileProvider
{
    public string ModuleName => "communications";
    public string DisplayName => "Communication Preferences";

    public ProfileModuleSchema Schema => new ProfileModuleSchema
    {
        ModuleName = "communications",
        DisplayName = "Communication Preferences",
        Properties = new Dictionary<string, PropertySchema>
        {
            ["PreferredChannels"] = new PropertySchema { Name = "PreferredChannels", Type = "array" },
            ["QuietHours"] = new PropertySchema { Name = "QuietHours", Type = "object" },
            ["AllowWeekends"] = new PropertySchema { Name = "AllowWeekends", Type = "bool", DefaultValue = false }
        }
    };

    public async Task<object> GetProfileDataAsync(Guid accountId)
    {
        var prefs = await _preferencesService.GetPreferencesAsync(accountId);

        return new
        {
            PreferredChannels = prefs.PreferredChannels,
            QuietHours = new
            {
                Start = prefs.QuietHoursStart,
                End = prefs.QuietHoursEnd
            },
            AllowWeekends = prefs.AllowWeekendsDelivery,
            HolidayCalendars = prefs.HolidayCalendars
        };
    }

    public async Task UpdateProfileDataAsync(Guid accountId, object data)
    {
        var commData = (dynamic)data;
        await _preferencesService.UpdatePreferencesAsync(accountId, commData);
    }
}
```

---

#### Registration and Usage

**Startup Registration:**
```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Register core services
    services.AddSingleton<IAccountManagementService, AccountManagementService>();
    services.AddSingleton<IProfileManagementService, ProfileManagementService>();

    // Component features register their profile providers
    services.AddSingleton<IProfileProvider, SchedulesProfileProvider>();
    services.AddSingleton<IProfileProvider, ContactsProfileProvider>();
    services.AddSingleton<IProfileProvider, UserDefaultsProfileProvider>();
    services.AddSingleton<IProfileProvider, CommunicationsProfileProvider>();

    // Profile service auto-discovers and registers all IProfileProvider instances
}
```

**Getting Complete Profile:**
```csharp
// Get complete account profile (all modules aggregated)
var profile = await _profileService.GetProfileAsync(accountId);

// Access modules
var schedules = profile.Modules["schedules"];  // From SchedulesProfileProvider
var contacts = profile.Modules["contacts"];    // From ContactsProfileProvider
var defaults = profile.Modules["defaults"];    // From UserDefaultsProfileProvider
var commPrefs = profile.Modules["communications"];  // From CommunicationsProfileProvider

// JSON serialization
var json = JsonSerializer.Serialize(profile);
/*
{
    "accountId": "...",
    "account": { ... },
    "modules": {
        "schedules": {
            "timeZone": "America/New_York",
            "workingHours": { "start": "09:00", "end": "17:00" },
            "weekendDays": ["Saturday", "Sunday"]
        },
        "contacts": {
            "phoneNumbers": ["+15551234567"],
            "emailAddresses": ["john@example.com"],
            "socialProfiles": { "linkedIn": "...", "twitter": "..." }
        },
        "defaults": {
            "language": "en-US",
            "currency": "USD",
            "theme": "dark",
            "dateFormat": "MM/dd/yyyy"
        },
        "communications": {
            "preferredChannels": ["email", "sms"],
            "quietHours": { "start": "21:00", "end": "07:00" },
            "allowWeekends": false
        }
    }
}
*/
```

**Getting Specific Module:**
```csharp
// Get just one module
var scheduleData = await _profileService.GetProfileModuleAsync<ScheduleModule>(accountId);

// Type-safe access
Console.WriteLine($"Time Zone: {scheduleData.TimeZone}");
Console.WriteLine($"Work Start: {scheduleData.WorkingHours.Start}");
```

**Updating Module:**
```csharp
// Update specific module
await _profileService.UpdateProfileModuleAsync(accountId, "defaults", new
{
    Language = "fr-FR",
    Currency = "EUR",
    Theme = "dark"
});
```

---

### Benefits of Modular Profile System

- ✅ **Extensible** - Component features add profile modules without modifying core
- ✅ **Decoupled** - Each module managed by its owning component
- ✅ **Discovery** - Profile service auto-discovers registered providers
- ✅ **Schema-Driven** - UI can auto-generate forms from schema
- ✅ **Aggregated** - Single API call gets complete profile
- ✅ **Selective Updates** - Update specific modules independently
- ✅ **Type-Safe** - Strongly-typed module access when needed
- ✅ **JSON-Friendly** - Serializes naturally to JSON for APIs

---

### Integration with Other Epics

**Epic 2: Communications Platform**
```csharp
// Communications reads user preferences from profile
var profile = await _profileService.GetProfileAsync(userId);
var commPrefs = profile.Modules["communications"];

// Respect quiet hours from profile
var quietHours = commPrefs.QuietHours;
if (IsQuietTime(quietHours))
{
    await _communications.DeferSendAsync(...);
}
```

**Epic 11: Data Enhancement Pipeline**
```csharp
// Profile data available via data container
var container = DataContainerFactory.Create(new { UserId = userId });
container.RegisterProvider("Profile", profileDataProvider);

// Templates access profile data
// Template: "Welcome {{Profile/defaults/Language}}"
var rendered = await _templateEngine.RenderAsync("welcome-message", container);
```

---

### What We're NOT Doing

- ❌ **OAuth/OIDC Provider** - Use existing identity providers (Azure B2C, Keycloak)
- ❌ **Password Management** - Use external authentication systems
- ❌ **MFA Implementation** - Integrate with existing MFA providers
- ❌ **Authorization Rules Engine** - Basic role/claims checking only

---

## Architecture Summary

```
┌────────────────────────────────────────────────────────────────┐
│ Application (Order Service, User Service, etc.)                │
└──────────────────────────┬─────────────────────────────────────┘
                           ↓
┌───────────────────────────────────────────────────────────────┐
│ Epic 12: Message Composition Service                          │
│ - Orchestrates: Data Enhancement + Templating + Conversion    │
│ - Produces: Pre-formatted IMessage (Email/SMS/Slack/Teams)    │
└──────────────────────────┬────────────────────────────────────┘
                           ↓
          ┌────────────────┼────────────────┐
          ↓                ↓                ↓
┌─────────────────┐ ┌──────────────┐ ┌─────────────────┐
│ Epic 11:        │ │ Epic 10:     │ │ Epic 6.2:       │
│ Data            │ │ Templating   │ │ Document        │
│ Enhancement     │ │ Extensions   │ │ Conversion      │
│                 │ │              │ │                 │
│ - IDataContainer│ │ - Handlebars │ │ - Markdown→HTML │
│ - Lazy eval     │ │ - XSLT       │ │ - HTML→PDF      │
│ - XPath/JSON    │ │ - DB source  │ │ - XML→JSON      │
│ - Path xlation  │ │ - Lazy int.  │ │ - Multi-step    │
└─────────────────┘ └──────────────┘ └─────────────────┘
          ↓
┌───────────────────────────────────────────────────────────────┐
│ Epic 2: Communications Platform                               │
│ - Send & Receive via Channels                                 │
│ - Channel: Protocol + Provider + Name                         │
│ - User Preferences: Quiet hours, weekends, holidays           │
└──────────────────────────┬────────────────────────────────────┘
                           ↓
          ┌────────────────┼────────────────┬────────────────┐
          ↓                ↓                ↓                ↓
┌─────────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│ Email Channel   │ │ SMS Channel  │ │ Slack Channel│ │ Teams Channel│
│ Protocol: email │ │ Protocol: sms│ │ Protocol:    │ │ Protocol:    │
│ Provider:       │ │ Provider:    │ │   slack      │ │   teams      │
│   SendGrid      │ │   Twilio     │ │ Provider:    │ │ Provider:    │
│                 │ │              │ │   slack-api  │ │   ms-teams   │
└─────────────────┘ └──────────────┘ └──────────────┘ └──────────────┘

** Data Flow: Application → Composition → (Enhancement + Templates + Conversion) → Communications → Channels **
```

---

## Key Architectural Principles

1. ✅ **Standalone Services** - Each epic is a STANDALONE service (can be used independently)
2. ✅ **Composite Orchestrations** - Orchestrations CHAIN services together (optional, not required)
3. ✅ **Transparent Infrastructure** - Caching via AOP/Dynamic Proxy (developer writes zero cache logic)
4. ✅ **Generic over Specific** - `IDataContainer` works for ANY data scenario
5. ✅ **Lazy Evaluation** - Load data only when accessed (50-70% performance gain)
6. ✅ **Industry Standards** - XPath, JSONPath, Handlebars (NOT custom syntax)
7. ✅ **Path Translation** - Template engines use native syntax, auto-translated
8. ✅ **Leverage Existing** - Use OoBDev.System.Text.Templating, don't replace
9. ✅ **Provider Pattern** - Template sources, path translators, data providers, channels all pluggable
10. ✅ **Separation of Concerns** - Enhancement, Templating, Conversion, Communications, Caching all separate
11. ✅ **No Hard Dependencies** - Services can be used individually or composed
12. ✅ **Declarative over Imperative** - Attributes declare behavior, not code
13. ✅ **Platform Agnostic Background Processes** - `IBackgroundTask` runs on Azure Functions, AWS Lambda, Windows Services, Linux daemons, Quartz, Hangfire, etc.
14. ✅ **Modular Profiles** - Component features advertise profile data via `IProfileProvider` (schedules, contacts, defaults)
15. ✅ **Injectable Validation** - `IValidator<T>` supports DataAnnotations, FluentValidation, custom validators via provider pattern

---

## Service Independence & Orchestration

### Standalone Services (Independent)

Each epic is a **STANDALONE SERVICE** that can be used independently:

**Epic 11: Data Enhancement**
```csharp
// Use ALONE for data enrichment
var container = DataContainerFactory.Create(new { OrderId = 123 });
container.RegisterProvider("Customer", customerProvider);
var customerName = container.Evaluate<string>("Customer/Name");
```

**Epic 10: Text Templating**
```csharp
// Use ALONE for template rendering
var data = new { CustomerName = "John", OrderTotal = 100.50 };
var html = await _templateEngine.RenderAsync("invoice-template", data);
```

**Epic 6: Document Conversion**
```csharp
// Use ALONE for format conversion
var markdown = "# Invoice\n\n**Total:** $100.50";
var html = await _conversion.ConvertAsync(markdown, "text/markdown", "text/html");
```

**Epic 2: Communications**
```csharp
// Use ALONE for sending pre-formatted messages
var message = new EmailMessage
{
    ToAddress = "customer@example.com",
    Subject = "Invoice #123",
    HtmlContent = "<h1>Invoice</h1><p>Total: $100.50</p>"
};
await _communications.SendAsync(userId, message, "support-email");
```

---

### Composite Orchestrations (Optional)

**Epic 12: Message Composition Service** is a **COMPOSITE ORCHESTRATION** that CHAINS standalone services:

```csharp
public class MessageCompositionService : IMessageCompositionService
{
    private readonly IDataEnhancementPipeline _enhancement;  // Epic 11 (standalone)
    private readonly ITemplateEngine _templates;             // Epic 10 (standalone)
    private readonly IConversionPipeline _conversion;        // Epic 6 (standalone)

    public async Task<IEmailMessage> ComposeEmailAsync(...)
    {
        // 1. Use Epic 11 (Data Enhancement) - STANDALONE
        var container = await _enhancement.EnhanceAsync(data);

        // 2. Use Epic 10 (Templating) - STANDALONE
        var rendered = await _templates.RenderAsync(templateName, container);

        // 3. Use Epic 6 (Conversion) - STANDALONE (optional)
        if (requiredFormat != rendered.MediaType)
        {
            var converted = await _conversion.ConvertAsync(rendered.Content, rendered.MediaType, requiredFormat);
            finalContent = converted.Content;
        }

        // 4. Return message for Epic 2 (Communications) - STANDALONE
        return new EmailMessage { HtmlContent = finalContent };
    }
}
```

**Key Points:**
- ✅ Epic 12 is a **CONVENIENCE ORCHESTRATION** (not required)
- ✅ You can chain services yourself without Epic 12
- ✅ Each service (11, 10, 6, 2) works independently
- ✅ Applications can use services directly or via orchestrations

**Alternative Usage (Without Epic 12):**
```csharp
// Application can orchestrate services directly
public class OrderService
{
    private readonly IDataEnhancementPipeline _enhancement;
    private readonly ITemplateEngine _templates;
    private readonly ICommunicationsService _communications;

    public async Task SendOrderConfirmationAsync(int orderId)
    {
        // Manually chain services (no Epic 12 needed)
        var data = DataContainerFactory.Create(new { OrderId = orderId });
        data.RegisterProvider("Order", _orderProvider);
        data.RegisterProvider("Customer", _customerProvider);

        var html = await _templates.RenderAsync("order-confirmation", data);

        var message = new EmailMessage
        {
            ToAddress = data.Evaluate<string>("Customer/Email"),
            Subject = "Order Confirmation",
            HtmlContent = html.Content
        };

        await _communications.SendAsync(userId, message, "support-email");
    }
}
```

---

## Implementation Priority

### Phase 1: Foundation (Weeks 1-2)
1. **Epic 11: Data Enhancement Pipeline**
   - Core container & navigation
   - Lazy data providers
   - Path syntax translation (XPath, JSONPath, Dot Notation)

2. **Epic 10: Text Templating Extensions**
   - Handlebars provider
   - Database template source
   - IDataContainer integration

### Phase 2: Composition & Communication (Weeks 3-4)
3. **Epic 12: Message Composition Service**
   - Combines Epic 11 + Epic 10
   - Produces pre-formatted messages

4. **Epic 2: Communications Platform**
   - Simplified routing and delivery
   - SendGrid/Twilio providers

### Phase 3: Domain Features (Weeks 5-7)
5. Epic 3: Spatial Services
6. Epic 5: Data Loading Pipeline
7. Epic 6: Document Management (3 features)

### Phase 4: Advanced (Weeks 8-10)
8. Epic 7: Identity & Session
9. Epic 8: Complex Events
10. Epic 9: Test Data Generation

---

## Success Metrics

### Epic 11: Data Enhancement
- ✅ Works with messages, reports, documents, exports (generic)
- ✅ Lazy evaluation reduces queries by 50-70%
- ✅ XPath, JSONPath, Dot Notation supported
- ✅ Auto-detection of path syntax
- ✅ < 50ms navigation overhead
- ✅ 80%+ test coverage

### Epic 10: Text Templating
- ✅ Handlebars provider works with existing ITemplateEngine
- ✅ Database template source for dynamic templates
- ✅ IDataContainer integration preserves lazy evaluation
- ✅ No breaking changes to existing engine
- ✅ 80%+ test coverage

### Epic 2: Communications
- ✅ Sends pre-formatted messages (no composition logic)
- ✅ Routes to Email/SMS/Push based on preferences
- ✅ Respects quiet hours
- ✅ Deferral scheduling works
- ✅ < 200ms routing time
- ✅ 80%+ test coverage

---

## Related Documents

- [Epic Review](./EPIC_REVIEW.md) - All epics breakdown
- [Architectural Improvements](./ARCHITECTURAL_IMPROVEMENTS.md) - Comparison with SharedFramework
- [Revisions Summary](./REVISIONS_SUMMARY.md) - Detailed revision history
- [Epic 11: Data Enhancement (Revised)](./11-DataEnhancement/README-REVISED.md)
- [Epic 11: Path Translation](./11-DataEnhancement/PathTranslation/README.md)
- [Epic 10: Text Templating (Revised)](./10-TextTemplating/README-REVISED.md)
- [Epic 2: Communications (Revised)](./02-Communications/README-REVISED.md)
