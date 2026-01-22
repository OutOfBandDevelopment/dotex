# Channel Abstraction - Architecture

**Epic:** 2 - Communications Platform
**Feature:** Channel Abstraction
**Last Updated:** 2026-01-22

---

## Architectural Overview

The Channel Abstraction implements a **Provider Pattern** with **Registry Pattern** for pluggable communication channels. Each channel is identified by a unique combination of **Name + Protocol + Provider**.

```
┌─────────────────────────────────────────────────────────────┐
│                  Application Service                        │
│            (OrderService, NotificationService)              │
└────────────────────┬────────────────────────────────────────┘
                     │
                     ↓
┌─────────────────────────────────────────────────────────────┐
│               ICommunicationsService                        │
│         (Routes messages to channels)                       │
└────────────────────┬────────────────────────────────────────┘
                     │
         ┌───────────┼───────────┐
         ↓           ↓           ↓
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│   IChannel   │ │   IChannel   │ │   IChannel   │
│"support-email"│ │"alerts-sms"  │ │"sales-slack" │
│Protocol:email│ │Protocol:sms  │ │Protocol:slack│
│Provider:      │ │Provider:     │ │Provider:     │
│sendgrid      │ │twilio        │ │slack-api     │
└──────┬───────┘ └──────┬───────┘ └──────┬───────┘
       │                │                │
       ↓                ↓                ↓
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│IChannelProvider│IChannelProvider│IChannelProvider│
│SendGrid      │ │Twilio        │ │Slack API     │
└──────────────┘ └──────────────┘ └──────────────┘
       │                │                │
       ↓                ↓                ↓
┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│SendGrid API  │ │Twilio API    │ │Slack API     │
└──────────────┘ └──────────────┘ └──────────────┘
```

---

## Core Components

### 1. Channel (Data Model)

**Responsibilities:**
- Store channel configuration
- Identify channel by name, protocol, provider
- Track enabled/disabled state
- Store provider-specific configuration

**Key Design Decisions:**
- **Immutable name** - Channel name never changes after creation
- **Flexible configuration** - Dictionary<string, object> for provider settings
- **Enabled flag** - Channels can be temporarily disabled without deletion

**Implementation Pattern:**
```csharp
public class Channel : IChannel
{
    public string Name { get; init; }  // Immutable
    public string Protocol { get; init; }  // Immutable
    public string Provider { get; init; }  // Immutable
    public IDictionary<string, object> Configuration { get; init; }
    public bool IsEnabled { get; set; }
    public DateTimeOffset? CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }

    public Channel(string name, string protocol, string provider)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Channel name is required", nameof(name));

        if (string.IsNullOrWhiteSpace(protocol))
            throw new ArgumentException("Protocol is required", nameof(protocol));

        if (string.IsNullOrWhiteSpace(provider))
            throw new ArgumentException("Provider is required", nameof(provider));

        // Validate name: alphanumeric + hyphens + underscores only
        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_-]+$"))
            throw new ArgumentException("Invalid channel name format", nameof(name));

        // Validate protocol: lowercase alphanumeric only
        if (!Regex.IsMatch(protocol, @"^[a-z0-9]+$"))
            throw new ArgumentException("Protocol must be lowercase alphanumeric", nameof(protocol));

        Name = name;
        Protocol = protocol;
        Provider = provider;
        Configuration = new Dictionary<string, object>();
        IsEnabled = true;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
```

---

### 2. ChannelProvider (Provider Pattern)

**Responsibilities:**
- Send messages via channel
- Receive messages from channel (optional)
- Register webhooks for inbound messages (optional)
- Validate channel configuration

**Key Design Decisions:**
- **Stateless** - Providers have no mutable state
- **Thread-safe** - Providers can be called concurrently
- **Protocol-specific** - Each provider supports one or more protocols

**Implementation Example (SendGrid Email Provider):**
```csharp
public class SendGridEmailProvider : IChannelProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SendGridEmailProvider> _logger;

    public string ProviderName => "sendgrid";
    public string[] SupportedProtocols => new[] { "email" };
    public bool SupportsSending => true;
    public bool SupportsReceiving => false;  // SendGrid is send-only
    public bool SupportsWebhooks => true;    // Can register webhooks for events

    public SendGridEmailProvider(
        HttpClient httpClient,
        ILogger<SendGridEmailProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public Task<bool> CanSendAsync(IChannel channel, IMessage message)
    {
        // Validate configuration
        if (!channel.Configuration.ContainsKey("ApiKey"))
            return Task.FromResult(false);

        if (!channel.Configuration.ContainsKey("FromEmail"))
            return Task.FromResult(false);

        // Validate message type
        if (message is not IEmailMessage)
            return Task.FromResult(false);

        return Task.FromResult(true);
    }

    public async Task<SendResult> SendAsync(IChannel channel, IMessage message)
    {
        if (message is not IEmailMessage emailMessage)
        {
            throw new ArgumentException("Message must be IEmailMessage", nameof(message));
        }

        // Extract configuration
        var apiKey = channel.Configuration["ApiKey"].ToString();
        var fromEmail = channel.Configuration["FromEmail"].ToString();

        // Build SendGrid request
        var request = new
        {
            personalizations = new[]
            {
                new
                {
                    to = new[] { new { email = emailMessage.To.First() } },
                    subject = emailMessage.Subject
                }
            },
            from = new { email = fromEmail },
            content = new[]
            {
                new { type = "text/plain", value = emailMessage.TextContent },
                new { type = "text/html", value = emailMessage.HtmlContent }
            }
        };

        // Send via SendGrid API
        _logger.LogDebug("Sending email via SendGrid: {To}", emailMessage.To.First());

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.sendgrid.com/v3/mail/send",
            request);

        if (response.IsSuccessStatusCode)
        {
            _logger.LogInformation("Email sent successfully via SendGrid");
            return new SendResult
            {
                Success = true,
                MessageId = response.Headers.GetValues("X-Message-Id").FirstOrDefault(),
                Timestamp = DateTimeOffset.UtcNow
            };
        }
        else
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("SendGrid API error: {Error}", error);
            return new SendResult
            {
                Success = false,
                ErrorMessage = error,
                Timestamp = DateTimeOffset.UtcNow
            };
        }
    }

    public Task<IMessage?> ReceiveAsync(IChannel channel)
    {
        throw new NotSupportedException("SendGrid does not support polling-based receive");
    }

    public async Task RegisterWebhookAsync(IChannel channel, string webhookUrl)
    {
        // Register webhook for delivery events (opens, clicks, bounces, etc.)
        _logger.LogInformation("Registering SendGrid webhook: {Url}", webhookUrl);

        var apiKey = channel.Configuration["ApiKey"].ToString();

        // Call SendGrid webhook registration API
        // (Implementation details omitted)

        await Task.CompletedTask;
    }

    public Task UnregisterWebhookAsync(IChannel channel)
    {
        _logger.LogInformation("Unregistering SendGrid webhook");
        return Task.CompletedTask;
    }
}
```

---

### 3. ChannelRegistry (Registry Pattern)

**Responsibilities:**
- Discover all `IChannelProvider` implementations
- Register providers at startup
- Lookup providers by protocol and name
- Cache provider instances

**Key Design Decisions:**
- **Singleton** - One registry instance per application
- **Lazy registration** - Providers registered on first use
- **Thread-safe** - Concurrent provider lookups

**Implementation Pattern:**
```csharp
public class ChannelRegistry : IChannelRegistry
{
    private readonly IServiceProvider _serviceProvider;
    private readonly Dictionary<string, List<IChannelProvider>> _providersByProtocol;
    private readonly Dictionary<string, IChannelProvider> _providersByName;
    private readonly object _lock = new object();
    private readonly ILogger<ChannelRegistry> _logger;

    public ChannelRegistry(
        IServiceProvider serviceProvider,
        ILogger<ChannelRegistry> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _providersByProtocol = new Dictionary<string, List<IChannelProvider>>();
        _providersByName = new Dictionary<string, IChannelProvider>();

        // Discover providers at startup
        DiscoverProviders();
    }

    private void DiscoverProviders()
    {
        _logger.LogInformation("Discovering channel providers...");

        // Get all registered IChannelProvider implementations
        var providers = _serviceProvider.GetServices<IChannelProvider>();

        foreach (var provider in providers)
        {
            RegisterProvider(provider);
        }

        _logger.LogInformation("Discovered {Count} channel providers", _providersByName.Count);
    }

    public void RegisterProvider(IChannelProvider provider)
    {
        lock (_lock)
        {
            // Register by name
            _providersByName[provider.ProviderName] = provider;

            // Register by supported protocols
            foreach (var protocol in provider.SupportedProtocols)
            {
                if (!_providersByProtocol.ContainsKey(protocol))
                {
                    _providersByProtocol[protocol] = new List<IChannelProvider>();
                }

                _providersByProtocol[protocol].Add(provider);
            }

            _logger.LogDebug(
                "Registered provider {Provider} for protocols: {Protocols}",
                provider.ProviderName,
                string.Join(", ", provider.SupportedProtocols));
        }
    }

    public IChannelProvider? GetProvider(string protocol, string providerName)
    {
        lock (_lock)
        {
            // Exact match: protocol + provider name
            if (_providersByName.TryGetValue(providerName, out var provider))
            {
                if (provider.SupportedProtocols.Contains(protocol))
                {
                    return provider;
                }
            }

            return null;
        }
    }

    public IEnumerable<IChannelProvider> GetProvidersByProtocol(string protocol)
    {
        lock (_lock)
        {
            if (_providersByProtocol.TryGetValue(protocol, out var providers))
            {
                return providers.ToList();  // Return copy
            }

            return Enumerable.Empty<IChannelProvider>();
        }
    }

    public IEnumerable<string> GetSupportedProtocols()
    {
        lock (_lock)
        {
            return _providersByProtocol.Keys.ToList();  // Return copy
        }
    }
}
```

---

### 4. ChannelRepository (Repository Pattern)

**Responsibilities:**
- Store channels in database
- Retrieve channels by name, protocol
- Update channel configuration
- Soft-delete (archive) channels

**Key Design Decisions:**
- **Database-backed** - Channels stored in persistent storage
- **Caching** - In-memory cache for frequently accessed channels
- **Soft-delete** - Archived channels retained for audit

**Implementation Pattern (Entity Framework):**
```csharp
public class ChannelRepository : IChannelRepository
{
    private readonly CommunicationsDbContext _dbContext;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ChannelRepository> _logger;

    private const string CACHE_KEY_PREFIX = "Channel:";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(15);

    public ChannelRepository(
        CommunicationsDbContext dbContext,
        IMemoryCache cache,
        ILogger<ChannelRepository> logger)
    {
        _dbContext = dbContext;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IChannel?> GetByNameAsync(string name)
    {
        // Check cache first
        var cacheKey = $"{CACHE_KEY_PREFIX}{name}";
        if (_cache.TryGetValue<IChannel>(cacheKey, out var cachedChannel))
        {
            _logger.LogDebug("Channel {Name} retrieved from cache", name);
            return cachedChannel;
        }

        // Fetch from database
        var channel = await _dbContext.Channels
            .Where(c => c.Name == name && !c.IsArchived)
            .FirstOrDefaultAsync();

        if (channel != null)
        {
            // Cache result
            _cache.Set(cacheKey, channel, CACHE_DURATION);
        }

        return channel;
    }

    public async Task<IEnumerable<IChannel>> GetByProtocolAsync(string protocol)
    {
        var channels = await _dbContext.Channels
            .Where(c => c.Protocol == protocol && !c.IsArchived)
            .ToListAsync();

        return channels;
    }

    public async Task<IEnumerable<IChannel>> GetAllAsync()
    {
        var channels = await _dbContext.Channels
            .Where(c => !c.IsArchived)
            .ToListAsync();

        return channels;
    }

    public async Task<IChannel> CreateAsync(IChannel channel)
    {
        _logger.LogInformation("Creating channel: {Name}", channel.Name);

        // Validate unique name
        var existing = await _dbContext.Channels
            .Where(c => c.Name == channel.Name)
            .AnyAsync();

        if (existing)
        {
            throw new InvalidOperationException($"Channel with name '{channel.Name}' already exists");
        }

        // Insert into database
        _dbContext.Channels.Add((Channel)channel);
        await _dbContext.SaveChangesAsync();

        // Cache result
        var cacheKey = $"{CACHE_KEY_PREFIX}{channel.Name}";
        _cache.Set(cacheKey, channel, CACHE_DURATION);

        return channel;
    }

    public async Task UpdateAsync(IChannel channel)
    {
        _logger.LogInformation("Updating channel: {Name}", channel.Name);

        var entity = await _dbContext.Channels
            .Where(c => c.Name == channel.Name)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new InvalidOperationException($"Channel '{channel.Name}' not found");
        }

        // Update properties
        entity.IsEnabled = channel.IsEnabled;
        entity.Configuration = channel.Configuration;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Invalidate cache
        var cacheKey = $"{CACHE_KEY_PREFIX}{channel.Name}";
        _cache.Remove(cacheKey);
    }

    public async Task DeleteAsync(string name)
    {
        _logger.LogInformation("Deleting channel: {Name}", name);

        var entity = await _dbContext.Channels
            .Where(c => c.Name == name)
            .FirstOrDefaultAsync();

        if (entity == null)
        {
            throw new InvalidOperationException($"Channel '{name}' not found");
        }

        // Soft-delete (archive)
        entity.IsArchived = true;
        entity.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        // Invalidate cache
        var cacheKey = $"{CACHE_KEY_PREFIX}{name}";
        _cache.Remove(cacheKey);
    }
}
```

---

## Data Flow

### Sequence: Send Message via Channel

```
┌─────────┐         ┌──────────────┐         ┌──────────────┐         ┌──────────┐
│ Service │         │Communications│         │ChannelRepo   │         │ Provider │
└────┬────┘         │Service       │         │              │         │          │
     │              └──────┬───────┘         └──────┬───────┘         └────┬─────┘
     │                     │                        │                      │
     │ SendAsync(userId,   │                        │                      │
     │   message,          │                        │                      │
     │   "support-email")  │                        │                      │
     ├────────────────────>│                        │                      │
     │                     │                        │                      │
     │                     │ GetByNameAsync(        │                      │
     │                     │   "support-email")     │                      │
     │                     ├───────────────────────>│                      │
     │                     │                        │                      │
     │                     │                        │ Check cache          │
     │                     │                        │ Fetch from DB        │
     │                     │                        │                      │
     │                     │ IChannel (sendgrid)    │                      │
     │                     │<───────────────────────┤                      │
     │                     │                        │                      │
     │                     │ Registry.GetProvider(  │                      │
     │                     │   "email", "sendgrid") │                      │
     │                     │                        │                      │
     │                     │ IChannelProvider       │                      │
     │                     │                        │                      │
     │                     │ CanSendAsync(channel,  │                      │
     │                     │   message)             │                      │
     │                     ├──────────────────────────────────────────────>│
     │                     │                        │                      │
     │                     │ true                   │                      │
     │                     │<──────────────────────────────────────────────┤
     │                     │                        │                      │
     │                     │ SendAsync(channel,     │                      │
     │                     │   message)             │                      │
     │                     ├──────────────────────────────────────────────>│
     │                     │                        │                      │
     │                     │                        │                      │ SendGrid API
     │                     │                        │                      │ POST /v3/mail/send
     │                     │                        │                      │
     │                     │ SendResult             │                      │
     │                     │<──────────────────────────────────────────────┤
     │                     │                        │                      │
     │ SendResult          │                        │                      │
     │<────────────────────┤                        │                      │
     │                     │                        │                      │
```

**Key Points:**
1. Channel fetched from repository (cached)
2. Provider resolved from registry by protocol + provider name
3. Pre-flight check: `CanSendAsync()`
4. Send via provider: `SendAsync()`
5. Provider calls external API (SendGrid, Twilio, etc.)

---

## Design Patterns

### 1. Provider Pattern
- `IChannelProvider` interface for pluggable implementations
- Multiple providers per protocol
- Provider selection based on channel configuration

### 2. Registry Pattern
- `IChannelRegistry` for provider discovery
- Lazy registration at startup
- Thread-safe provider lookup

### 3. Repository Pattern
- `IChannelRepository` for persistent storage
- Caching layer for performance
- Soft-delete for audit trail

### 4. Strategy Pattern
- Provider selection strategy (cost, reliability, region)
- Channel routing strategy (user preferences)

---

## Performance Optimizations

### 1. Channel Caching
- Channels cached in-memory for 15 minutes
- Cache key: `"Channel:{name}"`
- Invalidate on update/delete

### 2. Registry Caching
- Provider instances cached in registry
- No repeated provider instantiation
- Singleton registry per application

### 3. Lazy Provider Discovery
- Providers discovered at startup
- No runtime reflection overhead
- Fast provider lookup via dictionary

---

## Thread Safety

### Concurrency Strategy
- **Registry** - Lock-based thread safety for provider registration/lookup
- **Repository** - Database transactions for data integrity
- **Providers** - Stateless, inherently thread-safe

### Synchronization Points
```csharp
// Registry lock (coarse-grained)
private readonly object _lock = new object();

// Repository uses database transactions
using var transaction = await _dbContext.Database.BeginTransactionAsync();
```

---

## Error Handling

### Channel Errors
```csharp
public class ChannelException : Exception
{
    public string? ChannelName { get; }
    public string? Protocol { get; }

    public ChannelException(string message, string? channelName = null)
        : base(message)
    {
        ChannelName = channelName;
    }
}

public class ChannelNotFoundException : ChannelException
{
    public ChannelNotFoundException(string channelName)
        : base($"Channel '{channelName}' not found", channelName)
    {
    }
}

public class ProviderNotFoundException : ChannelException
{
    public ProviderNotFoundException(string protocol, string providerName)
        : base($"Provider '{providerName}' not found for protocol '{protocol}'")
    {
        Protocol = protocol;
    }
}
```

---

## Testing Strategy

### Unit Tests
- Mock providers for deterministic behavior
- Test channel validation logic
- Test registry provider lookup
- Test repository CRUD operations

### Integration Tests
- Real database for repository tests
- In-memory cache for caching tests
- Mock external APIs (SendGrid, Twilio)

### Example Test
```csharp
[TestMethod]
public async Task GetProvider_ValidProtocolAndName_ReturnsProvider()
{
    // Arrange
    var mockProvider = new Mock<IChannelProvider>();
    mockProvider.Setup(p => p.ProviderName).Returns("sendgrid");
    mockProvider.Setup(p => p.SupportedProtocols).Returns(new[] { "email" });

    var registry = new ChannelRegistry(_serviceProvider, _logger);
    registry.RegisterProvider(mockProvider.Object);

    // Act
    var provider = registry.GetProvider("email", "sendgrid");

    // Assert
    Assert.IsNotNull(provider);
    Assert.AreEqual("sendgrid", provider.ProviderName);
}
```

---

## Related Documents

- [Requirements](./requirements.md)
- [API Design](./api-design.md)
- [Testing Strategy](./testing-strategy.md)
- [Epic 2 Overview](../README-REVISED.md)
