# Message Queue Pattern: Context-Based

**Pattern Origin:** Main OoBDev Codebase (Current)
**Status:** ✅ Currently implemented (RabbitMQ provider)
**Complexity:** Lower (simple interfaces)
**Type Safety:** Runtime

---

## Overview

The Context-Based pattern uses a single `IMessageContext` object to carry all queue configuration, headers, and metadata. Queue selection happens at runtime through configuration rather than compile-time generics. This pattern provides flexibility and simplicity at the cost of compile-time type safety.

---

## Core Architecture

### Key Interfaces

```csharp
namespace OoBDev.MessageQueueing.Services
{
    /// <summary>
    /// Provides message sending capabilities (non-generic).
    /// </summary>
    public interface IMessageSenderProvider
    {
        /// <summary>
        /// Sends a message asynchronously to the message queue.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="context">The context with configuration and metadata.</param>
        /// <returns>The message ID assigned by the queue, or null if not supported.</returns>
        Task<string?> SendAsync(object message, IMessageContext context);
    }

    /// <summary>
    /// Context containing all information needed to send/receive messages.
    /// </summary>
    public interface IMessageContext
    {
        string? OriginMessageId { get; }
        string? CorrelationId { get; set; }
        string? RequestId { get; }
        string? SentId { get; set; }
        string? ChannelType { get; }
        string? MessageType { get; }
        DateTimeOffset? SentAt { get; }
        string? SentBy { get; }
        string? SentFrom { get; }

        /// <summary>
        /// Access to custom properties by key.
        /// </summary>
        object? this[string key] { get; set; }

        /// <summary>
        /// Collection of all headers.
        /// </summary>
        Dictionary<string, object?> Headers { get; }

        /// <summary>
        /// Configuration section for this message (queue name, connection string, etc).
        /// </summary>
        IConfigurationSection Config { get; }
    }

    /// <summary>
    /// Factory for selecting appropriate message sender provider.
    /// </summary>
    public interface IMessageSenderProviderFactory
    {
        /// <summary>
        /// Gets the appropriate message sender provider based on configuration.
        /// </summary>
        IMessageSenderProvider GetProvider(IMessageContext context);
    }

    /// <summary>
    /// Provides message receiving capabilities.
    /// </summary>
    public interface IMessageReceiverProvider
    {
        IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider);
        Task RunAsync(CancellationToken cancellationToken = default);
    }
}
```

### Existing Infrastructure (Already in Main)

```csharp
namespace OoBDev.System.Text.Json.Serialization
{
    /// <summary>
    /// JSON serialization (already exists in OoBDev.System).
    /// </summary>
    public interface IJsonSerializer
    {
        void Serialize(object obj, Stream stream);
        Task SerializeAsync(object obj, Stream stream, CancellationToken cancellationToken);
        T? Deserialize<T>(Stream stream);
        Task<T?> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken);
    }
}
```

---

## Implementation Example: RabbitMQ (Existing)

### Current RabbitMQ Provider

```csharp
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.RabbitMQ.MessageQueueing
{
    /// <summary>
    /// RabbitMQ implementation using context-based pattern.
    /// </summary>
    public class RabbitMQQueueMessageProvider : IMessageSenderProvider, IMessageReceiverProvider
    {
        private readonly IJsonSerializer _serializer;
        private readonly IQueueClientFactory _clientFactory;
        private readonly ILogger<RabbitMQQueueMessageProvider> _logger;

        public RabbitMQQueueMessageProvider(
            IJsonSerializer serializer,              // ✅ Already exists
            IQueueClientFactory clientFactory,
            ILogger<RabbitMQQueueMessageProvider> logger)
        {
            _serializer = serializer;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Send message using context for all configuration.
        /// </summary>
        public async Task<string?> SendAsync(object message, IMessageContext context)
        {
            // Get queue name from context.Config
            var (connection, channel, queueName) = await _clientFactory.CreateAsync(context.Config);

            // Wrap message with metadata from context
            var wrapped = new WrappedQueueMessage
            {
                ContentType = "application/json;",
                PayloadType = message.GetType().AssemblyQualifiedName
                    ?? throw new NotSupportedException(),
                CorrelationId = context.CorrelationId ?? "",
                Payload = message,
                Properties = context.Headers,  // All headers from context
            };

            // Serialize using IJsonSerializer (already exists)
            using var stream = new MemoryStream();
            await _serializer.SerializeAsync(wrapped, stream, default);
            ReadOnlyMemory<byte> body = stream.ToArray();

            using (connection)
            using (channel)
            {
                await channel.BasicPublishAsync(
                    exchange: string.Empty,
                    routingKey: queueName,
                    basicProperties: null,
                    mandatory: true,
                    body: body);

                return context.CorrelationId;
            }
        }

        // Receiver implementation omitted for brevity
        public IMessageReceiverProvider SetHandlerProvider(IMessageHandlerProvider handlerProvider) { /*...*/ }
        public async Task RunAsync(CancellationToken cancellationToken = default) { /*...*/ }
    }
}
```

---

## Implementation Example: AWS SQS (Adapted)

### SQS Provider Using Context Pattern

```csharp
using Amazon;
using Amazon.SQS;
using Amazon.SQS.Model;
using OoBDev.MessageQueueing.Services;
using OoBDev.System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    /// <summary>
    /// AWS SQS implementation using context-based pattern.
    /// </summary>
    public class AmazonSqsMessageProvider : IMessageSenderProvider
    {
        private readonly IJsonSerializer _serializer;
        private readonly IAmazonSQS _sqsClient;

        public AmazonSqsMessageProvider(
            IJsonSerializer serializer,           // ✅ Already exists
            IConfiguration configuration)          // ✅ Standard DI
        {
            _serializer = serializer;

            // Read AWS credentials from configuration
            var accessKey = configuration["AWS:AccessKeyId"];
            var secretKey = configuration["AWS:SecretAccessKey"];
            var regionName = configuration["AWS:Region"] ?? "us-east-1";

            var region = RegionEndpoint.GetBySystemName(regionName);
            _sqsClient = new AmazonSQSClient(accessKey, secretKey, region);
        }

        public async Task<string?> SendAsync(object message, IMessageContext context)
        {
            // Get queue configuration from context.Config
            var queueName = context.Config["QueueName"]
                ?? throw new ArgumentException("QueueName not found in context.Config");

            var delaySeconds = int.TryParse(context.Config["DelaySeconds"], out var delay) ? delay : 0;

            // Get queue URL
            var queueUrlResponse = await _sqsClient.GetQueueUrlAsync(queueName);

            // Wrap message with context metadata
            var wrapped = new WrappedQueueMessage
            {
                ContentType = "application/json",
                PayloadType = message.GetType().AssemblyQualifiedName ?? "",
                CorrelationId = context.CorrelationId ?? Guid.NewGuid().ToString(),
                Payload = message,
                Properties = context.Headers,
            };

            // Serialize using existing IJsonSerializer
            using var stream = new MemoryStream();
            await _serializer.SerializeAsync(wrapped, stream, default);
            var messageBody = Encoding.UTF8.GetString(stream.ToArray());

            // Build SQS request
            var request = new SendMessageRequest
            {
                QueueUrl = queueUrlResponse.QueueUrl,
                MessageBody = messageBody,
                DelaySeconds = delaySeconds,
                MessageAttributes =
                {
                    {"Content-Type", new MessageAttributeValue { DataType = "String", StringValue = "application/json" }},
                    {"CorrelationId", new MessageAttributeValue { DataType = "String", StringValue = wrapped.CorrelationId }},
                }
            };

            // Add all headers from context as message attributes
            foreach (var header in context.Headers.Where(h => h.Value != null))
            {
                request.MessageAttributes[header.Key] = new MessageAttributeValue
                {
                    DataType = "String",
                    StringValue = header.Value?.ToString() ?? ""
                };
            }

            // Send message
            var response = await _sqsClient.SendMessageAsync(request);
            return response.MessageId;
        }
    }
}
```

---

## Dependency Injection Setup

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Services;

namespace OoBDev.Amazon.Sqs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddAmazonSqsServices(this IServiceCollection services)
        {
            // Simple registration - no generics
            services.TryAddSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>();

            // Or keyed registration for multiple providers
            services.AddKeyedSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>("sqs");

            return services;
        }

        public static IServiceCollection TryAddAzureServiceBusServices(this IServiceCollection services)
        {
            services.TryAddSingleton<IMessageSenderProvider, AzureServiceBusMessageProvider>();

            // Or keyed
            services.AddKeyedSingleton<IMessageSenderProvider, AzureServiceBusMessageProvider>("servicebus");

            return services;
        }

        public static IServiceCollection TryAddMessageSenderFactory(this IServiceCollection services)
        {
            // Factory to select provider at runtime
            services.TryAddSingleton<IMessageSenderProviderFactory, MessageSenderProviderFactory>();
            return services;
        }
    }
}
```

### Configuration (appsettings.json)

```json
{
  "AWS": {
    "AccessKeyId": "AKIA...",
    "SecretAccessKey": "abc123...",
    "Region": "us-east-1"
  },
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "sqs",
      "QueueName": "production-orders",
      "DelaySeconds": 0
    },
    "NotificationQueue": {
      "Provider": "servicebus",
      "QueueName": "production-notifications",
      "DelaySeconds": 5,
      "ConnectionString": "Endpoint=sb://..."
    },
    "PaymentQueue": {
      "Provider": "rabbitmq",
      "QueueName": "production-payments",
      "Host": "localhost",
      "Port": 5672
    }
  }
}
```

---

## Usage Examples

### Basic Usage with Factory

```csharp
using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Configuration;

public class OrderService
{
    private readonly IMessageSenderProviderFactory _senderFactory;
    private readonly IMessageContextFactory _contextFactory;

    public OrderService(
        IMessageSenderProviderFactory senderFactory,
        IMessageContextFactory contextFactory)
    {
        _senderFactory = senderFactory;
        _contextFactory = contextFactory;
    }

    public async Task ProcessOrder(Order order)
    {
        // Create context with queue configuration
        var context = _contextFactory.Create(
            channelType: "OrderQueue",
            messageType: order.GetType().FullName);

        // Add custom headers
        context.Headers["CustomerId"] = order.CustomerId;
        context.Headers["Priority"] = "High";

        // Factory selects correct provider based on context.Config
        var sender = _senderFactory.GetProvider(context);

        // Send message
        var messageId = await sender.SendAsync(order, context);
    }
}
```

### Direct Provider Injection

```csharp
using OoBDev.MessageQueueing.Services;

public class NotificationService
{
    private readonly IMessageSenderProvider _sender;
    private readonly IMessageContextFactory _contextFactory;

    // Inject provider directly (gets first registered, or use keyed services)
    public NotificationService(
        IMessageSenderProvider sender,
        IMessageContextFactory contextFactory)
    {
        _sender = sender;
        _contextFactory = contextFactory;
    }

    public async Task SendNotification(string email, string message)
    {
        var notification = new EmailNotification
        {
            To = email,
            Subject = "Order Confirmation",
            Body = message
        };

        // Create context
        var context = _contextFactory.Create(
            channelType: "NotificationQueue",
            messageType: typeof(EmailNotification).FullName);

        // Send using injected provider
        await _sender.SendAsync(notification, context);
    }
}
```

### Keyed Services (Multiple Providers)

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.MessageQueueing.Services;

public class MultiProviderService
{
    private readonly IMessageSenderProvider _sqsProvider;
    private readonly IMessageSenderProvider _serviceBusProvider;
    private readonly IMessageSenderProvider _rabbitMqProvider;
    private readonly IMessageContextFactory _contextFactory;

    public MultiProviderService(
        [FromKeyedServices("sqs")] IMessageSenderProvider sqsProvider,
        [FromKeyedServices("servicebus")] IMessageSenderProvider serviceBusProvider,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitMqProvider,
        IMessageContextFactory contextFactory)
    {
        _sqsProvider = sqsProvider;
        _serviceBusProvider = serviceBusProvider;
        _rabbitMqProvider = rabbitMqProvider;
        _contextFactory = contextFactory;
    }

    public async Task SendToMultipleQueues(Order order)
    {
        var context = _contextFactory.Create("OrderQueue", typeof(Order).FullName);

        // Send to all providers
        await _sqsProvider.SendAsync(order, context);
        await _serviceBusProvider.SendAsync(order, context);
        await _rabbitMqProvider.SendAsync(order, context);
    }
}
```

### Runtime Queue Selection

```csharp
public class DynamicRoutingService
{
    private readonly IMessageSenderProviderFactory _factory;
    private readonly IMessageContextFactory _contextFactory;
    private readonly IConfiguration _configuration;

    public DynamicRoutingService(
        IMessageSenderProviderFactory factory,
        IMessageContextFactory contextFactory,
        IConfiguration configuration)
    {
        _factory = factory;
        _contextFactory = contextFactory;
        _configuration = configuration;
    }

    public async Task RouteMessage(object message, string queueType)
    {
        // Queue selection happens at runtime
        var queueName = queueType switch
        {
            "urgent" => "UrgentQueue",
            "normal" => "NormalQueue",
            "low" => "LowPriorityQueue",
            _ => "DefaultQueue"
        };

        // Create context with runtime-determined queue
        var context = _contextFactory.Create(queueName, message.GetType().FullName);

        // Get provider based on configuration
        var provider = _factory.GetProvider(context);

        await provider.SendAsync(message, context);
    }
}
```

### Multi-Tenant Scenarios

```csharp
public class TenantAwareQueueService
{
    private readonly IMessageSenderProviderFactory _factory;
    private readonly IMessageContextFactory _contextFactory;

    public TenantAwareQueueService(
        IMessageSenderProviderFactory factory,
        IMessageContextFactory contextFactory)
    {
        _factory = factory;
        _contextFactory = contextFactory;
    }

    public async Task SendTenantMessage(string tenantId, object message)
    {
        // Queue name comes from database/configuration
        var queueName = $"tenant-{tenantId}-orders";

        var context = _contextFactory.Create(queueName, message.GetType().FullName);

        // Add tenant context
        context.Headers["TenantId"] = tenantId;
        context.Headers["TenantRegion"] = GetTenantRegion(tenantId);

        var provider = _factory.GetProvider(context);
        await provider.SendAsync(message, context);
    }

    private string GetTenantRegion(string tenantId)
    {
        // Runtime lookup
        return tenantId.StartsWith("US") ? "us-east-1" : "eu-west-1";
    }
}
```

---

## Advantages

### 1. **Uses Existing Infrastructure**
```csharp
// ✅ All these already exist in main codebase
IJsonSerializer              // OoBDev.System.Text.Json.Serialization
IMessageContext              // OoBDev.MessageQueueing.Abstractions
IMessageSenderProvider       // OoBDev.MessageQueueing.Abstractions
IConfiguration               // Microsoft.Extensions.Configuration

// No new abstractions needed!
```

### 2. **Simple Dependency Injection**
```csharp
// No generics, straightforward registration
services.TryAddSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>();
services.TryAddSingleton<IMessageSenderProvider, AzureServiceBusMessageProvider>();
services.TryAddSingleton<IMessageSenderProvider, RabbitMQQueueMessageProvider>();

// Or keyed services for explicit selection
services.AddKeyedSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>("sqs");
```

### 3. **Runtime Flexibility**
```csharp
// Queue selection at runtime
public async Task Send(string queueType, object message)
{
    var context = _factory.Create(queueType, message.GetType().FullName);
    var provider = _providerFactory.GetProvider(context);
    await provider.SendAsync(message, context);
}

// Easy to change without code changes
// All configuration in appsettings.json
```

### 4. **Consistent with Existing Code**
```csharp
// ✅ Matches RabbitMQ provider pattern (already in main)
public class RabbitMQQueueMessageProvider : IMessageSenderProvider
{
    public Task<string?> SendAsync(object message, IMessageContext context) { }
}

// ✅ New providers follow same pattern
public class AmazonSqsMessageProvider : IMessageSenderProvider
{
    public Task<string?> SendAsync(object message, IMessageContext context) { }
}

// All providers implement same interface!
```

### 5. **Configuration-Driven**
```csharp
// All settings in appsettings.json - no code changes
{
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "sqs",           // Change to "rabbitmq" without code change
      "QueueName": "orders-v2",    // Change queue name
      "DelaySeconds": 10           // Change delay
    }
  }
}
```

### 6. **Dynamic Queue Creation**
```csharp
// Easy to support multi-tenant with per-tenant queues
public async Task SendToTenantQueue(string tenantId, Order order)
{
    var queueName = $"tenant-{tenantId}-orders";  // Runtime queue name
    var context = _factory.Create(queueName, typeof(Order).FullName);
    await _sender.SendAsync(order, context);
}

// Queue names from database, API, etc.
```

---

## Disadvantages

### 1. **No Compile-Time Type Safety**
```csharp
// ❌ All providers use same interface - can't distinguish at DI time
public class OrderService
{
    // Which provider is this? SQS? RabbitMQ? ServiceBus?
    public OrderService(IMessageSenderProvider sender)  // Ambiguous!
    {
        // Must rely on DI configuration order or keyed services
    }
}

// Can inject wrong provider without compiler error
```

### 2. **Runtime Configuration Errors**
```csharp
// ❌ Typos discovered at runtime
var queueName = context.Config["QueueNam"];  // Oops! Should be "QueueName"
// No compiler error, fails at runtime

// ❌ Missing configuration discovered at runtime
var delay = int.Parse(context.Config["DelaySeconds"]);  // Null reference if missing
```

### 3. **Context "Bag of Everything"**
```csharp
// Context can become bloated
public interface IMessageContext
{
    string? OriginMessageId { get; }
    string? CorrelationId { get; set; }
    string? RequestId { get; }
    string? SentId { get; set; }
    string? ChannelType { get; }
    string? MessageType { get; }
    DateTimeOffset? SentAt { get; }
    string? SentBy { get; }
    string? SentFrom { get; }
    object? this[string key] { get; set; }
    Dictionary<string, object?> Headers { get; }
    IConfigurationSection Config { get; }
}

// Hard to know what properties are required vs optional
// Documentation burden
```

### 4. **Less Self-Documenting**
```csharp
// ❌ Can't tell from signature what queues are used
public class PaymentProcessor
{
    public PaymentProcessor(IMessageSenderProvider sender)  // Which queue?
    {
        // Must read code or documentation to understand
    }
}

// vs Generic Pattern ✅
public class PaymentProcessor
{
    public PaymentProcessor(IMessageSenderProvider<PaymentQueue> paymentQueue)  // Clear!
    {
    }
}
```

### 5. **Factory Pattern Required for Multiple Providers**
```csharp
// If you have multiple providers, need factory
public interface IMessageSenderProviderFactory
{
    IMessageSenderProvider GetProvider(IMessageContext context);
}

public class MessageSenderProviderFactory : IMessageSenderProviderFactory
{
    private readonly IEnumerable<IMessageSenderProvider> _providers;

    public IMessageSenderProvider GetProvider(IMessageContext context)
    {
        var providerType = context.Config["Provider"];

        // Runtime selection - must maintain mapping
        return providerType switch
        {
            "sqs" => _providers.OfType<AmazonSqsMessageProvider>().First(),
            "servicebus" => _providers.OfType<AzureServiceBusMessageProvider>().First(),
            "rabbitmq" => _providers.OfType<RabbitMQQueueMessageProvider>().First(),
            _ => throw new InvalidOperationException($"Unknown provider: {providerType}")
        };
    }
}
```

---

## Migration Effort

### Changes to Incoming Code

**Minimal changes required** - adapt SQS/ServiceBus to existing pattern:

```csharp
// BEFORE (SharedFramework pattern)
public class AmazonSqsMessageSender<TChannel> : IMessageSenderProvider<TChannel>
{
    public AmazonSqsMessageSender(
        IAmazonSqsFactory factory,
        IQueueResolver<TChannel> resolver,       // ❌ Doesn't exist
        IObjectSerializer serializer,            // ❌ Doesn't exist
        IQueueConfig<TChannel> config)
    {
    }

    public Task<string> SendAsync<T>(T message, string messageId,
        IDictionary<string, object> properties) { }
}

// AFTER (Adapted to main pattern)
public class AmazonSqsMessageProvider : IMessageSenderProvider
{
    public AmazonSqsMessageProvider(
        IJsonSerializer serializer,              // ✅ Already exists
        IConfiguration configuration)            // ✅ Standard DI
    {
    }

    public Task<string?> SendAsync(object message, IMessageContext context) { }
}
```

**Estimated Changes:**
- Remove generic `<TChannel>` parameters
- Change `IObjectSerializer` → `IJsonSerializer`
- Change method signature to accept `IMessageContext`
- Read configuration from `context.Config` instead of `IQueueResolver`
- ~100-150 LOC changes per provider (SQS + ServiceBus)

**Total Migration Effort:** ~200-300 LOC changes + tests (~300 LOC)

---

## When This Pattern Is Best

### Ideal Scenarios

1. **Dynamic Queue Management**
   - Multi-tenant systems with per-tenant queues
   - Queue names from database/configuration
   - Unknown number of queues at compile time

2. **Runtime Provider Selection**
   - Need to switch providers based on conditions
   - A/B testing different providers
   - Failover scenarios

3. **Consistent with Existing Codebase**
   - Main codebase already uses this pattern (RabbitMQ)
   - Want one consistent pattern across all providers
   - Team already understands the pattern

4. **Flexible Configuration**
   - Settings can change without recompilation
   - Support environment-specific configurations
   - Configuration-driven routing

5. **Simple Dependency Injection**
   - Prefer simple DI registrations
   - Avoid generic type complexity
   - Use keyed services for explicit selection

### Poor Fit Scenarios

1. **Fixed Channels Requiring Type Safety**
   - Small number of well-known queues
   - Compile-time type safety is critical
   - Financial/medical systems where wrong queue = big problem

2. **Multiple Queues in Single Service**
   - Service needs 5+ different queues
   - Want DI to clearly show all dependencies
   - Generic pattern makes this more explicit

---

## Summary

**Complexity Level:** ⭐⭐ (2/5 - Low)

**Type Safety:** ⭐⭐ (2/5 - Runtime only)

**Flexibility:** ⭐⭐⭐⭐⭐ (5/5 - Excellent)

**Maintainability:** ⭐⭐⭐⭐ (4/5 - Good, simple to understand)

**Consistency with Main:** ✅ (Already used in RabbitMQ provider)

**Missing Code:** None (uses existing infrastructure)

**Migration Effort:** ~200-300 LOC changes

**Best For:** Systems needing runtime flexibility and consistency with existing code

**Avoid If:** Compile-time type safety is critical requirement

---

## Next Steps

See also:
- [Generic Channel-Based Pattern](./pattern-generic-channel-based.md) - SharedFramework approach
- [Pattern Comparison](./pattern-comparison.md) - Side-by-side comparison
