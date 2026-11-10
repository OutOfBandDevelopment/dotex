# Message Queue Pattern: Generic Channel-Based

**Pattern Origin:** SharedFramework (Incoming code)
**Status:** Not currently in main codebase
**Complexity:** Higher (generics, typed channels)
**Type Safety:** Compile-time

---

## Overview

The Generic Channel-Based pattern uses C# generics to provide compile-time type safety for message queue channels. Each queue/channel is represented by a distinct type parameter, ensuring that queue selection is explicit and type-safe at compile time.

---

## Core Architecture

### Key Interfaces

```csharp
namespace OoBDev.MessageQueueing.Contracts.Services
{
    /// <summary>
    /// Provides message sending capabilities for a specific channel.
    /// </summary>
    /// <typeparam name="TChannel">The channel type (e.g., OrderChannel, NotificationChannel)</typeparam>
    public interface IMessageSenderProvider<TChannel>
    {
        Task<string> SendAsync<T>(T message, string messageId,
            IDictionary<string, object> properties) where T : class;
    }

    /// <summary>
    /// Resolves queue configuration for a specific channel.
    /// </summary>
    /// <typeparam name="TChannel">The channel type</typeparam>
    public interface IQueueResolver<TChannel>
    {
        string GetQueueName();
        IQueueConnectionString GetConnectionString();
        string GetConfigurationValue(string key);
    }

    /// <summary>
    /// Channel-specific queue configuration.
    /// </summary>
    /// <typeparam name="TChannel">The channel type</typeparam>
    public interface IQueueConfig<TChannel>
    {
        string? AccessKeyId { get; }
        string? SecretAccessKey { get; }
        RegionEndpoint Region { get; }
        int MaxNumberOfMessages { get; }
        int WaitTimeSeconds { get; }
        int DelaySeconds { get; }
    }

    /// <summary>
    /// Serializes objects to byte arrays with content type.
    /// </summary>
    public interface IObjectSerializer
    {
        (string contentType, byte[] data) Serialize(object obj);
        T Deserialize<T>(byte[] data, string contentType);
    }
}
```

### Channel Type Definitions

```csharp
namespace MyApp.MessageQueuing.Channels
{
    // Define channel types as marker classes
    public class OrderChannel { }
    public class NotificationChannel { }
    public class AuditLogChannel { }
    public class PaymentChannel { }
}
```

---

## Implementation Example: AWS SQS

### Provider Implementation

```csharp
using Amazon.SQS.Model;
using OoBDev.MessageQueueing.Contracts;
using OoBDev.MessageQueueing.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    [MessageQueue(QueueType = QueueTypes.AmazonSimpleQueue)]
    public class AmazonSqsMessageSender<TChannel> : IMessageSenderProvider<TChannel>
    {
        private readonly IAmazonSqsFactory _factory;
        private readonly IQueueResolver<TChannel> _resolver;
        private readonly IObjectSerializer _serializer;
        private readonly IQueueConfig<TChannel> _config;

        public AmazonSqsMessageSender(
            IAmazonSqsFactory factory,
            IQueueResolver<TChannel> resolver,
            IObjectSerializer serializer,
            IQueueConfig<TChannel> config
        )
        {
            _factory = factory;
            _resolver = resolver;
            _config = config;
            _serializer = serializer;
        }

        public async Task<string> SendAsync<T>(T message, string messageId,
            IDictionary<string, object> properties) where T : class
        {
            // Resolve queue name from channel-specific resolver
            var queueName = _resolver.GetQueueName();

            // Create AWS client with channel-specific configuration
            var client = _factory.Create(
                _config.AccessKeyId ?? throw new ApplicationException($"Missing SQS AccessKeyId for {queueName}"),
                _config.SecretAccessKey ?? throw new ApplicationException($"Missing SQS SecretAccessKey for {queueName}"),
                _config.Region);

            var queueUrl = await client.GetQueueUrlAsync(queueName);

            // Serialize message
            var (contentType, data) = _serializer.Serialize(message);

            var request = new SendMessageRequest
            {
                QueueUrl = queueUrl.QueueUrl,
                MessageBody = Encoding.UTF8.GetString(data),
                MessageAttributes =
                {
                    {"Content-Type", new MessageAttributeValue { DataType = "String", StringValue = contentType }},
                    {"External-MessageId", new MessageAttributeValue { DataType = "String", StringValue = messageId }},
                },
                DelaySeconds = _config.DelaySeconds,
            };

            // Add custom properties
            foreach (var property in properties.Where(p => p.Value != null))
                request.MessageAttributes.Add(
                    property.Key,
                    new MessageAttributeValue { DataType = "String", StringValue = property.Value.ToString() });

            var sent = await client.SendMessageAsync(request);
            return sent.MessageId;
        }
    }
}
```

### Configuration Classes

```csharp
using Amazon;
using OoBDev.Extensions;
using OoBDev.MessageQueueing.Contracts.Services;

namespace OoBDev.Amazon.Sqs.MessageQueueing
{
    public class QueueConfig<TChannel> : IQueueConfig<TChannel>
    {
        public QueueConfig(IQueueResolver<TChannel> resolver)
        {
            var connectionString = resolver?.GetConnectionString();

            var region = connectionString?[nameof(Region)];
            Region = !string.IsNullOrEmpty(region)
                ? RegionEndpoint.GetBySystemName(region)
                : RegionEndpoint.USEast1;

            AccessKeyId = connectionString?[nameof(AccessKeyId)];
            SecretAccessKey = connectionString?[nameof(SecretAccessKey)];

            string? config(string key) => resolver?.GetConfigurationValue(key);

            MaxNumberOfMessages = config(nameof(MaxNumberOfMessages)).ToInteger(@default: 10, min: 0, max: 10);
            WaitTimeSeconds = config(nameof(WaitTimeSeconds)).ToInteger(@default: 20, min: 0, max: 20);
            DelaySeconds = config(nameof(DelaySeconds)).ToInteger(@default: 0, min: 0, max: 900);
        }

        public RegionEndpoint Region { get; }
        public string? AccessKeyId { get; }
        public string? SecretAccessKey { get; }
        public int MaxNumberOfMessages { get; }
        public int WaitTimeSeconds { get; }
        public int DelaySeconds { get; }
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
using OoBDev.MessageQueueing.Contracts.Services;
using MyApp.MessageQueuing.Channels;

namespace OoBDev.Amazon.Sqs
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection TryAddAmazonSqsServices(this IServiceCollection services)
        {
            // Register generic implementations
            services.AddTransient(typeof(IMessageSenderProvider<>), typeof(AmazonSqsMessageSender<>));
            services.TryAddTransient(typeof(IQueueConfig<>), typeof(QueueConfig<>));
            services.TryAddTransient<IAmazonSqsFactory, AmazonSqsFactory>();

            // Register channel-specific resolvers (would need implementations)
            services.TryAddTransient<IQueueResolver<OrderChannel>, OrderChannelResolver>();
            services.TryAddTransient<IQueueResolver<NotificationChannel>, NotificationChannelResolver>();

            return services;
        }
    }
}
```

### Configuration (appsettings.json)

```json
{
  "MessageQueuing": {
    "OrderChannel": {
      "QueueName": "production-orders",
      "ConnectionString": "AccessKeyId=AKIA...;SecretAccessKey=abc123...;Region=us-east-1",
      "MaxNumberOfMessages": 10,
      "WaitTimeSeconds": 20,
      "DelaySeconds": 0
    },
    "NotificationChannel": {
      "QueueName": "production-notifications",
      "ConnectionString": "AccessKeyId=AKIA...;SecretAccessKey=abc123...;Region=us-west-2",
      "MaxNumberOfMessages": 10,
      "WaitTimeSeconds": 20,
      "DelaySeconds": 5
    }
  }
}
```

---

## Usage Examples

### Basic Usage

```csharp
using MyApp.MessageQueuing.Channels;
using OoBDev.MessageQueueing.Contracts.Services;

public class OrderService
{
    private readonly IMessageSenderProvider<OrderChannel> _orderQueue;
    private readonly IMessageSenderProvider<NotificationChannel> _notificationQueue;

    // Dependency injection clearly shows which queues are used
    public OrderService(
        IMessageSenderProvider<OrderChannel> orderQueue,
        IMessageSenderProvider<NotificationChannel> notificationQueue)
    {
        _orderQueue = orderQueue;
        _notificationQueue = notificationQueue;
    }

    public async Task ProcessOrder(Order order)
    {
        // Send to order queue - compile-time type safety
        var orderId = await _orderQueue.SendAsync(
            message: order,
            messageId: order.Id.ToString(),
            properties: new Dictionary<string, object>
            {
                { "CustomerId", order.CustomerId },
                { "Priority", "High" }
            });

        // Send to notification queue - different channel
        var notification = new OrderNotification
        {
            OrderId = order.Id,
            CustomerEmail = order.CustomerEmail
        };

        await _notificationQueue.SendAsync(
            message: notification,
            messageId: Guid.NewGuid().ToString(),
            properties: new Dictionary<string, object>
            {
                { "Type", "OrderConfirmation" }
            });
    }
}
```

### Multiple Providers Per Channel

```csharp
public class MultiProviderService
{
    // Can have different providers for the same channel type
    private readonly IMessageSenderProvider<OrderChannel> _primaryQueue;
    private readonly IMessageSenderProvider<OrderChannel> _backupQueue;

    public MultiProviderService(
        [FromKeyedServices("primary")] IMessageSenderProvider<OrderChannel> primaryQueue,
        [FromKeyedServices("backup")] IMessageSenderProvider<OrderChannel> backupQueue)
    {
        _primaryQueue = primaryQueue;
        _backupQueue = backupQueue;
    }

    public async Task SendOrderWithFailover(Order order)
    {
        try
        {
            await _primaryQueue.SendAsync(order, order.Id.ToString(), new Dictionary<string, object>());
        }
        catch
        {
            // Failover to backup queue
            await _backupQueue.SendAsync(order, order.Id.ToString(), new Dictionary<string, object>());
        }
    }
}
```

---

## Advantages

### 1. **Compile-Time Type Safety**
```csharp
// ✅ Compile-time error if you try to inject wrong channel
public class OrderService
{
    // Compiler ensures you get OrderChannel, not NotificationChannel
    public OrderService(IMessageSenderProvider<OrderChannel> queue) { }
}

// ❌ This would be a compile error:
IMessageSenderProvider<OrderChannel> orderQueue =
    serviceProvider.GetService<IMessageSenderProvider<NotificationChannel>>(); // Type mismatch!
```

### 2. **Self-Documenting Dependencies**
```csharp
// Clear from signature what queues this service uses
public class PaymentProcessor
{
    public PaymentProcessor(
        IMessageSenderProvider<PaymentChannel> paymentQueue,      // Needs payment queue
        IMessageSenderProvider<AuditLogChannel> auditQueue,       // Needs audit queue
        IMessageSenderProvider<NotificationChannel> notifyQueue)  // Needs notification queue
    {
        // Dependencies are explicit and type-safe
    }
}
```

### 3. **Configuration Isolation**
```csharp
// Each channel gets its own config instance
public class QueueConfig<TChannel> : IQueueConfig<TChannel>
{
    public QueueConfig(IQueueResolver<TChannel> resolver)
    {
        // Resolver ONLY reads config for TChannel
        // No risk of reading wrong section
    }
}
```

### 4. **Refactoring Safety**
```csharp
// If you rename OrderChannel -> OrderProcessingChannel
// Compiler finds ALL usages:
IMessageSenderProvider<OrderChannel>      // ❌ Compile error
IMessageSenderProvider<OrderProcessingChannel>  // ✅ Fixed
```

---

## Disadvantages

### 1. **Missing Abstractions in Main Codebase**
```csharp
// ❌ These interfaces don't exist in main codebase:
IQueueResolver<TChannel>        // Need to create
IObjectSerializer               // Need to create (main has IJsonSerializer)
IQueueConnectionString          // Need to create

// Estimated effort: ~200-300 LOC + tests
```

### 2. **Generic Registration Complexity**
```csharp
// More complex DI setup
services.AddTransient(typeof(IMessageSenderProvider<>), typeof(AmazonSqsMessageSender<>));
services.TryAddTransient(typeof(IQueueConfig<>), typeof(QueueConfig<>));

// Must register resolver for EACH channel type
services.TryAddTransient<IQueueResolver<OrderChannel>, OrderChannelResolver>();
services.TryAddTransient<IQueueResolver<NotificationChannel>, NotificationChannelResolver>();
services.TryAddTransient<IQueueResolver<PaymentChannel>, PaymentChannelResolver>();
// ... one per channel
```

### 3. **Channel Type Boilerplate**
```csharp
// Must define marker classes for each channel
public class OrderChannel { }
public class NotificationChannel { }
public class AuditLogChannel { }
public class PaymentChannel { }
public class EmailChannel { }
public class SmsChannel { }
// ... one per queue
```

### 4. **Runtime Queue Selection Difficult**
```csharp
// ❌ Can't easily select queue based on runtime conditions
public async Task SendToQueue(string queueType, object message)
{
    // How do you get IMessageSenderProvider<T> when T is determined at runtime?
    // Would need complex factory with type switching

    switch (queueType)
    {
        case "Order":
            var orderQueue = _serviceProvider.GetService<IMessageSenderProvider<OrderChannel>>();
            await orderQueue.SendAsync(...);
            break;
        case "Notification":
            var notifyQueue = _serviceProvider.GetService<IMessageSenderProvider<NotificationChannel>>();
            await notifyQueue.SendAsync(...);
            break;
        // ... ugly!
    }
}
```

### 5. **Inconsistent with Existing Code**
```csharp
// Main codebase uses non-generic pattern:
public class RabbitMQQueueMessageProvider : IMessageSenderProvider  // ← No generic
{
    public async Task<string?> SendAsync(object message, IMessageContext context)
    {
        // Uses context, not TChannel
    }
}

// Would have TWO different patterns in same codebase:
IMessageSenderProvider           // RabbitMQ (existing)
IMessageSenderProvider<TChannel> // SQS/ServiceBus (new)

// Confusing for developers!
```

---

## Migration Effort

### New Code Required

**1. IQueueResolver<TChannel> (~50 LOC)**
```csharp
public interface IQueueResolver<TChannel>
{
    string GetQueueName();
    IQueueConnectionString GetConnectionString();
    string GetConfigurationValue(string key);
}

public class QueueResolver<TChannel> : IQueueResolver<TChannel>
{
    private readonly IConfiguration _configuration;

    public QueueResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetQueueName() =>
        _configuration[$"MessageQueuing:{typeof(TChannel).Name}:QueueName"];

    public IQueueConnectionString GetConnectionString() =>
        new QueueConnectionString(
            _configuration[$"MessageQueuing:{typeof(TChannel).Name}:ConnectionString"]);

    public string GetConfigurationValue(string key) =>
        _configuration[$"MessageQueuing:{typeof(TChannel).Name}:{key}"];
}
```

**2. IObjectSerializer (~100 LOC)**
```csharp
public interface IObjectSerializer
{
    (string contentType, byte[] data) Serialize(object obj);
    T Deserialize<T>(byte[] data, string contentType);
}

public class JsonObjectSerializer : IObjectSerializer
{
    private readonly IJsonSerializer _jsonSerializer;

    public JsonObjectSerializer(IJsonSerializer jsonSerializer)
    {
        _jsonSerializer = jsonSerializer;
    }

    public (string contentType, byte[] data) Serialize(object obj)
    {
        using var stream = new MemoryStream();
        _jsonSerializer.Serialize(obj, stream);
        return ("application/json", stream.ToArray());
    }

    public T Deserialize<T>(byte[] data, string contentType)
    {
        using var stream = new MemoryStream(data);
        return _jsonSerializer.Deserialize<T>(stream);
    }
}
```

**3. IQueueConnectionString (~30 LOC)**
```csharp
public interface IQueueConnectionString
{
    string this[string key] { get; }
}

public class QueueConnectionString : IQueueConnectionString
{
    private readonly Dictionary<string, string> _values;

    public QueueConnectionString(string connectionString)
    {
        _values = connectionString
            .Split(';')
            .Select(part => part.Split('='))
            .ToDictionary(parts => parts[0], parts => parts[1]);
    }

    public string this[string key] => _values.TryGetValue(key, out var value) ? value : string.Empty;
}
```

**Total New Code:** ~200-300 LOC + comprehensive tests (~500 LOC)

---

## When This Pattern Is Best

### Ideal Scenarios

1. **Fixed Set of Well-Known Channels**
   - You have 5-10 queues that never change
   - Example: OrderQueue, PaymentQueue, NotificationQueue, AuditQueue
   - Channel types are known at compile time

2. **Strong Type Safety Requirements**
   - Financial systems where wrong queue = money loss
   - Medical systems where wrong queue = safety issue
   - Regulatory compliance requiring audit trails

3. **Multiple Developers/Teams**
   - Large codebase with many contributors
   - Need to prevent accidental queue misuse
   - Self-documenting code important

4. **Single Provider Per Channel**
   - Each channel uses exactly one provider type
   - Don't need runtime provider selection

### Poor Fit Scenarios

1. **Dynamic Queue Creation**
   - Multi-tenant systems where each tenant has their own queue
   - Queue names come from database
   - Number of queues not known at compile time

2. **Runtime Provider Selection**
   - Need to switch providers based on conditions
   - Failover between providers
   - A/B testing different providers

3. **Existing Codebase with Different Pattern**
   - Main codebase already uses non-generic pattern
   - Would create inconsistency

---

## Summary

**Complexity Level:** ⭐⭐⭐⭐ (4/5 - High)

**Type Safety:** ⭐⭐⭐⭐⭐ (5/5 - Excellent)

**Flexibility:** ⭐⭐ (2/5 - Low)

**Maintainability:** ⭐⭐⭐ (3/5 - Medium, requires understanding generics)

**Consistency with Main:** ❌ (Not used in main codebase)

**Missing Code:** ~200-300 LOC + tests

**Best For:** Systems with fixed queues requiring strong compile-time type safety

**Avoid If:** Need runtime flexibility or consistency with existing code

---

## Next Steps

See also:
- [Context-Based Pattern](./pattern-context-based.md) - Main codebase approach
- [Pattern Comparison](./pattern-comparison.md) - Side-by-side comparison
