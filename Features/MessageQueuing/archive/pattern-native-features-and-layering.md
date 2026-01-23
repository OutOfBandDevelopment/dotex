# Native Platform Features & Pattern Layering

**Date:** 2026-01-20
**Questions Addressed:**
1. Which pattern allows easier integration with native platform features?
2. Can the generic pattern be layered on top of the context pattern?

---

## Question 1: Native Platform Feature Integration

### Summary: **Context Pattern is Superior for Native Features**

The context-based pattern provides **unrestricted access** to platform-specific features, while the generic pattern **abstracts away** provider-specific capabilities.

---

## Platform-Specific Features Comparison

### AWS SQS Native Features

**Available Features:**
- FIFO queues (guaranteed ordering)
- Dead-letter queues
- Message attributes (custom metadata)
- Visibility timeout
- Delay queues
- Long polling
- Message deduplication
- Content-based deduplication
- Message groups (FIFO)

#### Context Pattern: ✅ **Full Native Access**

```csharp
public class AmazonSqsMessageProvider : IMessageSenderProvider
{
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        // ✅ Read ANY SQS-specific config from context.Config
        var queueUrl = context.Config["QueueUrl"];
        var delaySeconds = int.Parse(context.Config["DelaySeconds"] ?? "0");
        var messageGroupId = context.Config["MessageGroupId"];        // FIFO queues
        var deduplicationId = context.Config["DeduplicationId"];      // FIFO deduplication
        var visibilityTimeout = context.Config["VisibilityTimeout"];  // Custom timeout

        var request = new SendMessageRequest
        {
            QueueUrl = queueUrl,
            DelaySeconds = delaySeconds,

            // ✅ SQS-specific: Message groups for FIFO queues
            MessageGroupId = messageGroupId,
            MessageDeduplicationId = deduplicationId,
        };

        // ✅ Add ANY custom message attributes from context.Headers
        foreach (var header in context.Headers)
        {
            request.MessageAttributes[header.Key] = new MessageAttributeValue
            {
                DataType = "String",
                StringValue = header.Value?.ToString()
            };
        }

        // ✅ Can add SQS-specific attributes
        context.Headers["AWS.TraceHeader"] = GetXRayTraceHeader();
        context.Headers["Priority"] = "High";

        return await _sqsClient.SendMessageAsync(request);
    }
}
```

**Configuration:**
```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "sqs",
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123456789/orders.fifo",
      "MessageGroupId": "order-processing",      // ✅ SQS-specific
      "DeduplicationId": "use-content-hash",      // ✅ SQS-specific
      "DelaySeconds": 0,
      "VisibilityTimeout": 30,                    // ✅ SQS-specific
      "ContentBasedDeduplication": true           // ✅ SQS-specific
    }
  }
}
```

#### Generic Pattern: ⚠️ **Limited by Interface**

```csharp
public interface IQueueConfig<TChannel>
{
    RegionEndpoint Region { get; }
    string? AccessKeyId { get; }
    string? SecretAccessKey { get; }
    int MaxNumberOfMessages { get; }
    int WaitTimeSeconds { get; }
    int DelaySeconds { get; }
    // ❌ No MessageGroupId
    // ❌ No DeduplicationId
    // ❌ No VisibilityTimeout
    // ❌ No ContentBasedDeduplication
}

public class AmazonSqsMessageSender<TChannel> : IMessageSenderProvider<TChannel>
{
    public async Task<string> SendAsync<T>(T message, string messageId,
        IDictionary<string, object> properties)
    {
        // ⚠️ Can only use properties defined in IQueueConfig<TChannel>
        var request = new SendMessageRequest
        {
            DelaySeconds = _config.DelaySeconds,  // ✅ Defined in interface
            // ❌ Can't set MessageGroupId - not in interface
            // ❌ Can't set DeduplicationId - not in interface
        };

        // ⚠️ To add new SQS feature, must:
        // 1. Update IQueueConfig<TChannel> interface
        // 2. Update QueueConfig<TChannel> implementation
        // 3. Update all other provider configs (ServiceBus, RabbitMQ)
        // 4. Breaking change for all channels
    }
}
```

---

### Azure Service Bus Native Features

**Available Features:**
- Topics and subscriptions
- Sessions (ordered message processing)
- Dead-letter queues
- Scheduled messages
- Transactions
- Duplicate detection
- Auto-forwarding
- Message deferral
- Peek-lock vs receive-delete

#### Context Pattern: ✅ **Full Native Access**

```csharp
public class AzureServiceBusMessageProvider : IMessageSenderProvider
{
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        // ✅ Read Service Bus-specific config
        var topicName = context.Config["TopicName"];              // Topics!
        var sessionId = context.Config["SessionId"];              // Sessions!
        var scheduledTime = context.Config["ScheduledEnqueueTime"]; // Scheduled messages!
        var partitionKey = context.Config["PartitionKey"];        // Partitioning!
        var requiresSession = bool.Parse(context.Config["RequiresSession"] ?? "false");

        var serviceBusMessage = new ServiceBusMessage(messageBody)
        {
            // ✅ Service Bus-specific features
            SessionId = sessionId,
            PartitionKey = partitionKey,
            ScheduledEnqueueTime = DateTimeOffset.Parse(scheduledTime),

            // ✅ Custom properties from context
            Subject = context.Headers["Subject"]?.ToString(),
            CorrelationId = context.CorrelationId,
        };

        // ✅ Add user properties
        foreach (var header in context.Headers)
        {
            serviceBusMessage.ApplicationProperties[header.Key] = header.Value;
        }

        // ✅ Send to topic or queue based on config
        if (!string.IsNullOrEmpty(topicName))
        {
            await _topicSender.SendMessageAsync(serviceBusMessage);
        }
        else
        {
            await _queueSender.SendMessageAsync(serviceBusMessage);
        }
    }
}
```

**Configuration:**
```json
{
  "MessageQueuing": {
    "OrderProcessingQueue": {
      "Provider": "servicebus",
      "TopicName": "orders-topic",               // ✅ Service Bus topics
      "SubscriptionName": "processing",          // ✅ Subscriptions
      "SessionId": "session-{orderId}",          // ✅ Sessions
      "RequiresSession": true,                   // ✅ Session-enabled
      "PartitionKey": "{customerId}",            // ✅ Partitioning
      "ScheduledEnqueueTime": "+5m",             // ✅ Delayed delivery
      "EnableDeadLettering": true,               // ✅ DLQ
      "MaxDeliveryCount": 10                     // ✅ Retry policy
    }
  }
}
```

#### Generic Pattern: ⚠️ **Abstraction Limits Features**

```csharp
// ❌ IQueueConfig<TChannel> doesn't support:
// - Topics (only queue names)
// - Sessions
// - Scheduled messages
// - Partition keys
// - Transactions

// Must either:
// 1. Add ALL features to IQueueConfig (breaks other providers)
// 2. Create IServiceBusQueueConfig (separate interface, loses abstraction)
```

---

### RabbitMQ Native Features

**Available Features:**
- Exchanges (direct, fanout, topic, headers)
- Routing keys
- TTL (time-to-live)
- Priority queues
- Confirms and acknowledgments
- Consumer prefetch
- Message persistence
- Alternate exchanges

#### Context Pattern: ✅ **Full Native Access**

```csharp
public class RabbitMQQueueMessageProvider : IMessageSenderProvider
{
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        // ✅ RabbitMQ-specific config
        var exchangeName = context.Config["ExchangeName"];     // ✅ Exchanges
        var exchangeType = context.Config["ExchangeType"];     // ✅ Direct/Fanout/Topic/Headers
        var routingKey = context.Config["RoutingKey"];         // ✅ Routing keys
        var priority = byte.Parse(context.Config["Priority"] ?? "0"); // ✅ Priority
        var ttl = TimeSpan.Parse(context.Config["TTL"] ?? "00:00:00"); // ✅ TTL
        var persistent = bool.Parse(context.Config["Persistent"] ?? "true");

        var properties = channel.CreateBasicProperties();
        properties.Priority = priority;
        properties.Expiration = ttl.TotalMilliseconds.ToString();
        properties.Persistent = persistent;
        properties.CorrelationId = context.CorrelationId;

        // ✅ Use exchange instead of direct queue
        await channel.BasicPublishAsync(
            exchange: exchangeName,      // ✅ Custom exchange
            routingKey: routingKey,      // ✅ Routing key
            basicProperties: properties,
            mandatory: true,
            body: body);
    }
}
```

**Configuration:**
```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "rabbitmq",
      "ExchangeName": "orders-exchange",         // ✅ RabbitMQ exchanges
      "ExchangeType": "topic",                   // ✅ Exchange types
      "RoutingKey": "order.created.{region}",    // ✅ Routing keys
      "Priority": 5,                             // ✅ Priority queues
      "TTL": "01:00:00",                         // ✅ Message TTL
      "Persistent": true,                        // ✅ Durability
      "PrefetchCount": 10                        // ✅ Consumer prefetch
    }
  }
}
```

---

## Summary: Native Feature Access

| Platform Feature | Context Pattern | Generic Pattern |
|-----------------|----------------|-----------------|
| **AWS SQS FIFO** | ✅ Full support | ⚠️ Requires interface change |
| **SQS Message Groups** | ✅ Full support | ❌ Not in interface |
| **Service Bus Topics** | ✅ Full support | ❌ Not in interface |
| **Service Bus Sessions** | ✅ Full support | ❌ Not in interface |
| **Service Bus Scheduled Messages** | ✅ Full support | ❌ Not in interface |
| **RabbitMQ Exchanges** | ✅ Full support | ❌ Not in interface |
| **RabbitMQ Routing Keys** | ✅ Full support | ❌ Not in interface |
| **RabbitMQ Priority** | ✅ Full support | ❌ Not in interface |
| **Custom Message Attributes** | ✅ Unlimited | ⚠️ Limited to properties dict |
| **Provider-Specific Headers** | ✅ Any header | ⚠️ Must fit abstraction |

**Verdict:** ✅ **Context Pattern is vastly superior for native platform integration**

---

## Question 2: Can Generic Pattern Layer On Top of Context Pattern?

### Answer: ✅ **Yes! This is an Excellent Architectural Pattern**

You can build a type-safe generic layer as a **facade** over the flexible context-based layer. This gives you:

- ✅ **Type safety** where you want it (internal application code)
- ✅ **Flexibility** where you need it (provider implementations)
- ✅ **Best of both worlds**

---

## Layered Architecture Pattern

```
┌─────────────────────────────────────────────────────────────┐
│  Application Layer (Type-Safe Generic Interface)            │
│  IMessageSenderProvider<TChannel>                           │
│  - Compile-time type safety                                 │
│  - Self-documenting dependencies                            │
└─────────────────────────────────────────────────────────────┘
                            │
                            │ Adapter Layer
                            ▼
┌─────────────────────────────────────────────────────────────┐
│  Infrastructure Layer (Flexible Context Interface)          │
│  IMessageSenderProvider (non-generic)                       │
│  - Provider implementations (SQS, ServiceBus, RabbitMQ)     │
│  - Full access to native platform features                  │
└─────────────────────────────────────────────────────────────┘
```

---

## Implementation: Generic Facade Over Context

### Layer 1: Context-Based Infrastructure (Existing)

```csharp
namespace OoBDev.MessageQueueing.Services
{
    // ✅ Already exists
    public interface IMessageSenderProvider
    {
        Task<string?> SendAsync(object message, IMessageContext context);
    }

    // ✅ Implementations use context for flexibility
    public class AmazonSqsMessageProvider : IMessageSenderProvider
    {
        public Task<string?> SendAsync(object message, IMessageContext context)
        {
            // Full access to SQS-specific features via context.Config
        }
    }

    public class AzureServiceBusMessageProvider : IMessageSenderProvider
    {
        public Task<string?> SendAsync(object message, IMessageContext context)
        {
            // Full access to Service Bus-specific features via context.Config
        }
    }
}
```

### Layer 2: Generic Type-Safe Facade (Optional Add-On)

```csharp
namespace OoBDev.MessageQueueing.TypedChannels
{
    /// <summary>
    /// Type-safe generic interface for application code.
    /// </summary>
    public interface ITypedMessageSender<TChannel>
    {
        Task<string?> SendAsync<TMessage>(TMessage message) where TMessage : class;
        Task<string?> SendAsync<TMessage>(TMessage message, Action<IMessageOptions> configure) where TMessage : class;
    }

    /// <summary>
    /// Options for customizing message send behavior.
    /// </summary>
    public interface IMessageOptions
    {
        string? CorrelationId { get; set; }
        Dictionary<string, object?> Headers { get; }
        void AddHeader(string key, object? value);
    }

    /// <summary>
    /// Adapter that bridges generic interface to context-based infrastructure.
    /// </summary>
    public class TypedMessageSenderAdapter<TChannel> : ITypedMessageSender<TChannel>
    {
        private readonly IMessageSenderProvider _contextBasedProvider;  // ← Context infrastructure
        private readonly IMessageContextFactory _contextFactory;
        private readonly IChannelConfigurationProvider<TChannel> _channelConfig;

        public TypedMessageSenderAdapter(
            IMessageSenderProvider contextBasedProvider,       // ← Inject context provider
            IMessageContextFactory contextFactory,
            IChannelConfigurationProvider<TChannel> channelConfig)
        {
            _contextBasedProvider = contextBasedProvider;
            _contextFactory = contextFactory;
            _channelConfig = channelConfig;
        }

        public Task<string?> SendAsync<TMessage>(TMessage message) where TMessage : class
        {
            return SendAsync(message, _ => { });
        }

        public async Task<string?> SendAsync<TMessage>(
            TMessage message,
            Action<IMessageOptions> configure) where TMessage : class
        {
            // 1. Get channel-specific configuration
            var channelConfig = _channelConfig.GetConfiguration();

            // 2. Create context from channel config
            var context = _contextFactory.Create(
                channelType: typeof(TChannel).Name,
                messageType: typeof(TMessage).FullName);

            // 3. Apply channel-specific config to context
            foreach (var kvp in channelConfig)
            {
                context.Config[kvp.Key] = kvp.Value;
            }

            // 4. Apply user options
            var options = new MessageOptions();
            configure(options);
            context.CorrelationId = options.CorrelationId;
            foreach (var header in options.Headers)
            {
                context.Headers[header.Key] = header.Value;
            }

            // 5. Delegate to context-based infrastructure
            return await _contextBasedProvider.SendAsync(message, context);
        }
    }

    internal class MessageOptions : IMessageOptions
    {
        public string? CorrelationId { get; set; }
        public Dictionary<string, object?> Headers { get; } = new();
        public void AddHeader(string key, object? value) => Headers[key] = value;
    }
}
```

### Channel Configuration Provider

```csharp
namespace OoBDev.MessageQueueing.TypedChannels
{
    /// <summary>
    /// Provides configuration for a specific channel type.
    /// </summary>
    public interface IChannelConfigurationProvider<TChannel>
    {
        Dictionary<string, string> GetConfiguration();
    }

    /// <summary>
    /// Reads channel configuration from IConfiguration.
    /// </summary>
    public class ChannelConfigurationProvider<TChannel> : IChannelConfigurationProvider<TChannel>
    {
        private readonly IConfiguration _configuration;

        public ChannelConfigurationProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Dictionary<string, string> GetConfiguration()
        {
            var channelName = typeof(TChannel).Name;
            var section = _configuration.GetSection($"MessageQueuing:Channels:{channelName}");

            return section.GetChildren()
                .ToDictionary(x => x.Key, x => x.Value ?? string.Empty);
        }
    }
}
```

---

## Usage: Type-Safe Application Code

### Define Channel Types

```csharp
namespace MyApp.MessageChannels
{
    // Marker types for compile-time type safety
    public class OrderChannel { }
    public class PaymentChannel { }
    public class NotificationChannel { }
}
```

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using OoBDev.MessageQueueing.Services;
using OoBDev.MessageQueueing.TypedChannels;
using MyApp.MessageChannels;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageQueuing(this IServiceCollection services)
    {
        // Layer 1: Register context-based infrastructure
        services.TryAddSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>();
        services.TryAddSingleton<IMessageContextFactory, MessageContextFactory>();

        // Layer 2: Register generic type-safe facade
        services.TryAddTransient(typeof(IChannelConfigurationProvider<>), typeof(ChannelConfigurationProvider<>));
        services.TryAddTransient(typeof(ITypedMessageSender<>), typeof(TypedMessageSenderAdapter<>));

        return services;
    }
}
```

### Application Code: Type-Safe API

```csharp
using MyApp.MessageChannels;
using OoBDev.MessageQueueing.TypedChannels;

public class OrderService
{
    private readonly ITypedMessageSender<OrderChannel> _orderSender;
    private readonly ITypedMessageSender<PaymentChannel> _paymentSender;

    // ✅ Compile-time type safety
    // ✅ Clear dependencies
    // ✅ Self-documenting
    public OrderService(
        ITypedMessageSender<OrderChannel> orderSender,
        ITypedMessageSender<PaymentChannel> paymentSender)
    {
        _orderSender = orderSender;
        _paymentSender = paymentSender;
    }

    public async Task ProcessOrder(Order order)
    {
        // ✅ Type-safe send with options
        await _orderSender.SendAsync(order, options =>
        {
            options.CorrelationId = order.Id.ToString();
            options.AddHeader("Priority", "High");
            options.AddHeader("CustomerId", order.CustomerId);
        });

        // ✅ Simple send without options
        var payment = new PaymentRequest { OrderId = order.Id, Amount = order.Total };
        await _paymentSender.SendAsync(payment);
    }
}
```

### Configuration: Still Flexible

```json
{
  "MessageQueuing": {
    "Channels": {
      "OrderChannel": {
        "Provider": "sqs",
        "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123/orders.fifo",
        "MessageGroupId": "order-processing",        // ✅ SQS-specific
        "DeduplicationId": "use-content-hash"        // ✅ Still accessible!
      },
      "PaymentChannel": {
        "Provider": "servicebus",
        "TopicName": "payments-topic",               // ✅ Service Bus-specific
        "SessionId": "payment-{orderId}",            // ✅ Still accessible!
        "RequiresSession": true
      },
      "NotificationChannel": {
        "Provider": "rabbitmq",
        "ExchangeName": "notifications-exchange",    // ✅ RabbitMQ-specific
        "ExchangeType": "fanout",                    // ✅ Still accessible!
        "RoutingKey": "notify.{region}"
      }
    }
  }
}
```

---

## Benefits of Layered Approach

### ✅ **Best of Both Worlds**

| Feature | With Layering |
|---------|--------------|
| **Application Code Type Safety** | ✅ Generic `ITypedMessageSender<TChannel>` |
| **Provider Implementation Flexibility** | ✅ Context-based `IMessageSenderProvider` |
| **Native Platform Features** | ✅ Full access via context.Config |
| **Compile-Time Queue Safety** | ✅ Channel types enforce correctness |
| **Runtime Flexibility** | ✅ Configuration-driven behavior |
| **Migration Bridges** | ✅ Easy with context layer |
| **Multi-Provider Support** | ✅ Simple with context layer |
| **Self-Documenting Code** | ✅ Generic interface shows dependencies |

### ✅ **Gradual Adoption**

```csharp
// Old services: Continue using context directly
public class LegacyService
{
    public LegacyService(IMessageSenderProvider sender, IMessageContextFactory factory)
    {
        // Still works!
    }
}

// New services: Use type-safe generic interface
public class ModernService
{
    public ModernService(ITypedMessageSender<OrderChannel> sender)
    {
        // Type-safe!
    }
}

// Both work simultaneously!
```

### ✅ **Easy Testing**

```csharp
// Mock the generic interface for tests
var mockSender = new Mock<ITypedMessageSender<OrderChannel>>();
mockSender
    .Setup(s => s.SendAsync(It.IsAny<Order>(), It.IsAny<Action<IMessageOptions>>()))
    .ReturnsAsync("test-message-id");

var service = new OrderService(mockSender.Object, ...);
```

---

## Implementation Roadmap

### Phase 1: Context-Based Infrastructure (Current Migration)
1. Migrate SQS and Service Bus to context pattern
2. All providers implement `IMessageSenderProvider`
3. Full native feature support via context

### Phase 2: Generic Facade (Optional Future Enhancement)
1. Create `ITypedMessageSender<TChannel>` interface
2. Create `TypedMessageSenderAdapter<TChannel>` adapter
3. Create `IChannelConfigurationProvider<TChannel>`
4. Document usage patterns
5. Gradual adoption in new services

**Timeline:** Phase 1 (1-2 weeks), Phase 2 (optional, 1 week)

---

## Conclusion

### Question 1: Native Platform Features
**Answer:** ✅ **Context Pattern provides superior native feature access**

- Unrestricted access to provider-specific features
- No interface constraints
- Easy to add new platform capabilities
- Configuration-driven feature enablement

### Question 2: Generic Layer On Top
**Answer:** ✅ **Yes, and it's a recommended pattern**

- Build type-safe facade over flexible infrastructure
- Best of both worlds
- Gradual adoption possible
- No breaking changes to existing code

### Recommendation

**Start with Context Pattern (Phase 1):**
- Migrate SQS and Service Bus using context pattern
- Get full native feature support immediately
- Proven pattern (RabbitMQ already works)
- Less migration effort

**Add Generic Facade Later if Needed (Phase 2):**
- Optional enhancement
- No rush - can add anytime
- Gradual adoption
- Non-breaking change

This approach gives you **maximum flexibility now** with option to add **type safety later**.

---

## See Also

- [Context-Based Pattern](./pattern-context-based.md)
- [Generic Channel-Based Pattern](./pattern-generic-channel-based.md)
- [Pattern Comparison](./pattern-comparison.md)
- [Multi-Provider Bridge Example](./multi-provider-bridge-example.md)
