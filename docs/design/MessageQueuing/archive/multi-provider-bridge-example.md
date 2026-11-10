# Multi-Provider Bridge Pattern Example

**Pattern:** Context-Based
**Use Case:** Service acting as bridge between different queue providers
**Example:** Read from AWS SQS, process, write to RabbitMQ

---

## Overview

The context-based pattern excels at multi-provider scenarios where a service needs to interact with multiple queue systems simultaneously. This is common in:

- **Message Bridges**: Read from SQS, write to RabbitMQ
- **Fan-out**: Write same message to multiple providers
- **Migration**: Read from old queue (RabbitMQ), write to new queue (Azure Service Bus)
- **Multi-Cloud**: Use AWS SQS for ingress, Azure Service Bus for egress

---

## Implementation: SQS → RabbitMQ Bridge

### Service Registration

```csharp
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.MessageQueueing.Services;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMessageQueueBridge(this IServiceCollection services)
    {
        // Register multiple providers with keyed services
        services.AddKeyedSingleton<IMessageSenderProvider, AmazonSqsMessageProvider>("sqs");
        services.AddKeyedSingleton<IMessageSenderProvider, RabbitMQQueueMessageProvider>("rabbitmq");
        services.AddKeyedSingleton<IMessageSenderProvider, AzureServiceBusMessageProvider>("servicebus");

        // Register receivers
        services.AddKeyedSingleton<IMessageReceiverProvider, AmazonSqsMessageReceiver>("sqs-receiver");
        services.AddKeyedSingleton<IMessageReceiverProvider, RabbitMQQueueMessageProvider>("rabbitmq-receiver");

        // Register context factory
        services.TryAddSingleton<IMessageContextFactory, MessageContextFactory>();

        // Register bridge service
        services.AddHostedService<QueueBridgeService>();

        return services;
    }
}
```

### Bridge Service Implementation

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.MessageQueueing.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Hosted service that bridges messages from SQS to RabbitMQ.
/// </summary>
public class QueueBridgeService : BackgroundService
{
    private readonly IMessageReceiverProvider _sqsReceiver;
    private readonly IMessageSenderProvider _rabbitMqSender;
    private readonly IMessageContextFactory _contextFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueueBridgeService> _logger;

    public QueueBridgeService(
        [FromKeyedServices("sqs-receiver")] IMessageReceiverProvider sqsReceiver,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitMqSender,
        IMessageContextFactory contextFactory,
        IConfiguration configuration,
        ILogger<QueueBridgeService> logger)
    {
        _sqsReceiver = sqsReceiver;
        _rabbitMqSender = rabbitMqSender;
        _contextFactory = contextFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Bridge Service starting...");

        // Set up message handler for incoming SQS messages
        var handler = new BridgeMessageHandler(_rabbitMqSender, _contextFactory, _logger);
        _sqsReceiver.SetHandlerProvider(handler);

        // Start receiving from SQS
        await _sqsReceiver.RunAsync(stoppingToken);
    }
}

/// <summary>
/// Handler that processes messages from SQS and forwards to RabbitMQ.
/// </summary>
public class BridgeMessageHandler : IMessageHandlerProvider
{
    private readonly IMessageSenderProvider _rabbitMqSender;
    private readonly IMessageContextFactory _contextFactory;
    private readonly ILogger _logger;

    public IConfigurationSection Config { get; set; }

    public BridgeMessageHandler(
        IMessageSenderProvider rabbitMqSender,
        IMessageContextFactory contextFactory,
        ILogger logger)
    {
        _rabbitMqSender = rabbitMqSender;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task HandleAsync(IQueueMessage queueMessage, string? correlationId)
    {
        try
        {
            _logger.LogInformation("Received message from SQS: {CorrelationId}", correlationId);

            // Extract message from SQS wrapper
            var wrappedMessage = queueMessage as WrappedQueueMessage;
            var payload = wrappedMessage?.Payload;

            if (payload == null)
            {
                _logger.LogWarning("No payload in message {CorrelationId}", correlationId);
                return;
            }

            // Create context for RabbitMQ (different queue)
            var rabbitContext = _contextFactory.Create(
                channelType: "RabbitMQ-Target-Queue",
                messageType: payload.GetType().FullName);

            // Preserve correlation ID
            rabbitContext.CorrelationId = correlationId;

            // Copy headers from SQS message
            if (wrappedMessage?.Properties != null)
            {
                foreach (var prop in wrappedMessage.Properties)
                {
                    rabbitContext.Headers[prop.Key] = prop.Value;
                }
            }

            // Add bridge metadata
            rabbitContext.Headers["BridgedFrom"] = "SQS";
            rabbitContext.Headers["BridgedAt"] = DateTimeOffset.UtcNow;

            // Forward to RabbitMQ
            var messageId = await _rabbitMqSender.SendAsync(payload, rabbitContext);

            _logger.LogInformation(
                "Message bridged successfully. SQS CorrelationId: {SqsId}, RabbitMQ MessageId: {RmqId}",
                correlationId,
                messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bridging message {CorrelationId}", correlationId);
            throw;
        }
    }
}
```

### Configuration

```json
{
  "AWS": {
    "AccessKeyId": "AKIA...",
    "SecretAccessKey": "abc123...",
    "Region": "us-east-1"
  },
  "MessageQueuing": {
    "SQS-Source-Queue": {
      "Provider": "sqs",
      "QueueName": "incoming-orders",
      "MaxNumberOfMessages": 10,
      "WaitTimeSeconds": 20
    },
    "RabbitMQ-Target-Queue": {
      "Provider": "rabbitmq",
      "QueueName": "processed-orders",
      "Host": "localhost",
      "Port": 5672,
      "Username": "guest",
      "Password": "guest"
    }
  }
}
```

---

## Fan-Out Pattern: Write to Multiple Queues

```csharp
/// <summary>
/// Service that sends the same message to multiple queue providers.
/// </summary>
public class FanOutService
{
    private readonly IMessageSenderProvider _sqsSender;
    private readonly IMessageSenderProvider _rabbitMqSender;
    private readonly IMessageSenderProvider _serviceBusSender;
    private readonly IMessageContextFactory _contextFactory;
    private readonly ILogger<FanOutService> _logger;

    public FanOutService(
        [FromKeyedServices("sqs")] IMessageSenderProvider sqsSender,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitMqSender,
        [FromKeyedServices("servicebus")] IMessageSenderProvider serviceBusSender,
        IMessageContextFactory contextFactory,
        ILogger<FanOutService> logger)
    {
        _sqsSender = sqsSender;
        _rabbitMqSender = rabbitMqSender;
        _serviceBusSender = serviceBusSender;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task BroadcastOrder(Order order)
    {
        var correlationId = Guid.NewGuid().ToString();

        // Create contexts for each provider
        var sqsContext = _contextFactory.Create("SQS-Orders", typeof(Order).FullName);
        sqsContext.CorrelationId = correlationId;

        var rabbitContext = _contextFactory.Create("RabbitMQ-Orders", typeof(Order).FullName);
        rabbitContext.CorrelationId = correlationId;

        var serviceBusContext = _contextFactory.Create("ServiceBus-Orders", typeof(Order).FullName);
        serviceBusContext.CorrelationId = correlationId;

        // Send to all providers in parallel
        var tasks = new[]
        {
            SendWithLogging("SQS", () => _sqsSender.SendAsync(order, sqsContext)),
            SendWithLogging("RabbitMQ", () => _rabbitMqSender.SendAsync(order, rabbitContext)),
            SendWithLogging("ServiceBus", () => _serviceBusSender.SendAsync(order, serviceBusContext))
        };

        await Task.WhenAll(tasks);

        _logger.LogInformation("Order {OrderId} broadcast to all queues with correlation {CorrelationId}",
            order.Id, correlationId);
    }

    private async Task SendWithLogging(string providerName, Func<Task<string?>> sendFunc)
    {
        try
        {
            var messageId = await sendFunc();
            _logger.LogInformation("{Provider} send successful. MessageId: {MessageId}",
                providerName, messageId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "{Provider} send failed", providerName);
            throw;
        }
    }
}
```

---

## Migration Pattern: Old Queue → New Queue

```csharp
/// <summary>
/// Service that reads from old queue system and writes to new queue system
/// during migration period. Supports gradual rollout.
/// </summary>
public class QueueMigrationService : BackgroundService
{
    private readonly IMessageReceiverProvider _oldQueueReceiver;  // RabbitMQ
    private readonly IMessageSenderProvider _newQueueSender;      // Azure Service Bus
    private readonly IMessageSenderProvider _oldQueueSender;      // RabbitMQ (for fallback)
    private readonly IMessageContextFactory _contextFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<QueueMigrationService> _logger;

    public QueueMigrationService(
        [FromKeyedServices("rabbitmq-receiver")] IMessageReceiverProvider oldQueueReceiver,
        [FromKeyedServices("servicebus")] IMessageSenderProvider newQueueSender,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider oldQueueSender,
        IMessageContextFactory contextFactory,
        IConfiguration configuration,
        ILogger<QueueMigrationService> logger)
    {
        _oldQueueReceiver = oldQueueReceiver;
        _newQueueSender = newQueueSender;
        _oldQueueSender = oldQueueSender;
        _contextFactory = contextFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Queue Migration Service starting...");

        // Read migration percentage from config (allows gradual rollout)
        var migrationPercentage = _configuration.GetValue<int>("Migration:Percentage", 0);

        var handler = new MigrationMessageHandler(
            _newQueueSender,
            _oldQueueSender,
            _contextFactory,
            migrationPercentage,
            _logger);

        _oldQueueReceiver.SetHandlerProvider(handler);

        await _oldQueueReceiver.RunAsync(stoppingToken);
    }
}

public class MigrationMessageHandler : IMessageHandlerProvider
{
    private readonly IMessageSenderProvider _newQueueSender;
    private readonly IMessageSenderProvider _oldQueueSender;
    private readonly IMessageContextFactory _contextFactory;
    private readonly int _migrationPercentage;
    private readonly ILogger _logger;
    private readonly Random _random = new Random();

    public IConfigurationSection Config { get; set; }

    public MigrationMessageHandler(
        IMessageSenderProvider newQueueSender,
        IMessageSenderProvider oldQueueSender,
        IMessageContextFactory contextFactory,
        int migrationPercentage,
        ILogger logger)
    {
        _newQueueSender = newQueueSender;
        _oldQueueSender = oldQueueSender;
        _contextFactory = contextFactory;
        _migrationPercentage = migrationPercentage;
        _logger = logger;
    }

    public async Task HandleAsync(IQueueMessage queueMessage, string? correlationId)
    {
        var wrappedMessage = queueMessage as WrappedQueueMessage;
        var payload = wrappedMessage?.Payload;

        if (payload == null) return;

        // Determine if this message should go to new queue (gradual rollout)
        var useNewQueue = _random.Next(100) < _migrationPercentage;

        if (useNewQueue)
        {
            // Send to new queue (Azure Service Bus)
            var newContext = _contextFactory.Create("ServiceBus-Queue", payload.GetType().FullName);
            newContext.CorrelationId = correlationId;
            newContext.Headers["MigratedFrom"] = "RabbitMQ";

            await _newQueueSender.SendAsync(payload, newContext);

            _logger.LogInformation(
                "Message {CorrelationId} sent to NEW queue (ServiceBus). Migration: {Percentage}%",
                correlationId,
                _migrationPercentage);
        }
        else
        {
            // Send to old queue (RabbitMQ) - maintaining existing flow
            var oldContext = _contextFactory.Create("RabbitMQ-Queue", payload.GetType().FullName);
            oldContext.CorrelationId = correlationId;

            await _oldQueueSender.SendAsync(payload, oldContext);

            _logger.LogInformation(
                "Message {CorrelationId} sent to OLD queue (RabbitMQ). Migration: {Percentage}%",
                correlationId,
                _migrationPercentage);
        }
    }
}
```

### Migration Configuration

```json
{
  "Migration": {
    "Percentage": 25,  // Start with 25% of traffic to new queue
    "Comment": "Increase gradually: 25% → 50% → 75% → 100%"
  },
  "MessageQueuing": {
    "RabbitMQ-Queue": {
      "Provider": "rabbitmq",
      "QueueName": "old-processing-queue",
      "Host": "old-rabbitmq.company.com",
      "Port": 5672
    },
    "ServiceBus-Queue": {
      "Provider": "servicebus",
      "QueueName": "new-processing-queue",
      "ConnectionString": "Endpoint=sb://new-servicebus.servicebus.windows.net/..."
    }
  }
}
```

---

## Multi-Cloud Hybrid Pattern

```csharp
/// <summary>
/// Uses AWS SQS for public ingress, Azure Service Bus for internal processing.
/// </summary>
public class HybridCloudService
{
    private readonly IMessageReceiverProvider _sqsReceiver;      // AWS ingress
    private readonly IMessageSenderProvider _serviceBusSender;   // Azure processing
    private readonly IMessageContextFactory _contextFactory;
    private readonly ILogger<HybridCloudService> _logger;

    public HybridCloudService(
        [FromKeyedServices("sqs-receiver")] IMessageReceiverProvider sqsReceiver,
        [FromKeyedServices("servicebus")] IMessageSenderProvider serviceBusSender,
        IMessageContextFactory contextFactory,
        ILogger<HybridCloudService> logger)
    {
        _sqsReceiver = sqsReceiver;
        _serviceBusSender = serviceBusSender;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task ProcessIngressMessages(CancellationToken cancellationToken)
    {
        // Receive from AWS (public endpoint)
        // Process and validate
        // Forward to Azure (internal services)

        var handler = new HybridHandler(_serviceBusSender, _contextFactory, _logger);
        _sqsReceiver.SetHandlerProvider(handler);

        await _sqsReceiver.RunAsync(cancellationToken);
    }
}
```

---

## Comparison: Context Pattern vs Generic Pattern for Multi-Provider

### Context Pattern (✅ Excellent Support)

```csharp
// ✅ EASY: Inject multiple providers with keyed services
public class BridgeService
{
    public BridgeService(
        [FromKeyedServices("sqs")] IMessageSenderProvider sqsSender,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitSender,
        [FromKeyedServices("servicebus")] IMessageSenderProvider serviceBusSender)
    {
        // All providers available, clearly labeled
    }
}

// ✅ EASY: Create different contexts for each provider
var sqsContext = _contextFactory.Create("SQS-Queue", typeof(Order).FullName);
var rabbitContext = _contextFactory.Create("RabbitMQ-Queue", typeof(Order).FullName);

await _sqsSender.SendAsync(order, sqsContext);
await _rabbitSender.SendAsync(order, rabbitContext);
```

### Generic Pattern (⚠️ More Complex)

```csharp
// ⚠️ COMPLEX: Need different channel types for each provider
public class OrderChannel { }
public class ProcessedOrderChannel { }

public class BridgeService
{
    public BridgeService(
        IMessageSenderProvider<OrderChannel> sqsSender,           // ❌ Both are IMessageSenderProvider<OrderChannel>
        IMessageSenderProvider<ProcessedOrderChannel> rabbitSender) // Different channel required
    {
        // Must use different channel types even for same logical queue
    }
}

// ⚠️ Can't easily use same channel with multiple providers
// Would need factory or complex DI setup
```

---

## Conclusion

**For multi-provider scenarios (bridges, fan-out, migration), the Context Pattern is superior:**

✅ **Easy to inject multiple providers** using keyed services
✅ **Same interface for all providers** - IMessageSenderProvider
✅ **Flexible routing** - create different contexts for different queues
✅ **Simple configuration** - all in appsettings.json
✅ **Runtime selection** - can decide provider based on conditions

**Generic Pattern challenges for multi-provider:**
❌ Harder to inject multiple providers of same channel type
❌ Need different channel types even for same logical queue
❌ More complex DI configuration
❌ Less flexible for runtime routing

---

## See Also

- [Context-Based Pattern](./pattern-context-based.md)
- [Generic Channel-Based Pattern](./pattern-generic-channel-based.md)
- [Pattern Comparison](./pattern-comparison.md)
