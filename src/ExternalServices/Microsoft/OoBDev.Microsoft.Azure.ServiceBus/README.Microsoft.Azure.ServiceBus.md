# OoBDev.Microsoft.Azure.ServiceBus - Azure Service Bus Message Provider

Context-based message queue provider for Azure Service Bus (Queues and Topics).

## Features

- ✅ Queue and Topic support
- ✅ Session-based messaging
- ✅ Message correlation
- ✅ Application properties (metadata)
- ✅ Scheduled delivery support
- ✅ Runtime queue/topic configuration
- ✅ Multi-provider compatibility
- ✅ Connection string authentication

## Installation

```bash
dotnet add package OoBDev.Microsoft.Azure.ServiceBus
```

## Quick Start

### 1. Register Services

```csharp
using OoBDev.Microsoft.Azure.ServiceBus;
using OoBDev.System.Text.Json;

services.TryAddAzureServiceBusServices();
services.TryAddJsonSerializer();  // OoBDev.System
```

### 2. Configure Queue or Topic

**Using appsettings.json:**

```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "servicebus",
      "ConnectionString": "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=...",
      "QueueName": "orders"
    },
    "NotificationTopic": {
      "Provider": "servicebus",
      "ConnectionString": "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=...",
      "TopicName": "notifications"
    }
  }
}
```

### 3. Send Messages

```csharp
using OoBDev.MessageQueueing.Services;

public class OrderService
{
    private readonly IMessageSenderProvider _sender;
    private readonly IMessageContextFactory _contextFactory;

    public OrderService(
        IMessageSenderProvider sender,
        IMessageContextFactory contextFactory)
    {
        _sender = sender;
        _contextFactory = contextFactory;
    }

    public async Task ProcessOrder(Order order)
    {
        var context = _contextFactory.Create("OrderQueue", typeof(Order).FullName);
        context.Headers["CustomerId"] = order.CustomerId;
        context.Headers["Priority"] = "High";

        var correlationId = await _sender.SendAsync(order, context);
    }
}
```

## Configuration Reference

| Key | Required | Description | Default |
|-----|----------|-------------|---------|
| `ConnectionString` | Yes | Azure Service Bus connection string | - |
| `QueueName` | Yes* | Service Bus queue name | - |
| `TopicName` | Yes* | Service Bus topic name | - |
| `SessionId` | No | Session ID for session-based messaging | - |

*Either `QueueName` or `TopicName` must be provided (not both)

## Queue vs Topic

### Queues (Point-to-Point)
Messages are sent to a single queue and consumed by one receiver:

```json
{
  "OrderQueue": {
    "ConnectionString": "Endpoint=...",
    "QueueName": "orders"
  }
}
```

### Topics (Publish-Subscribe)
Messages are sent to a topic and can be consumed by multiple subscriptions:

```json
{
  "NotificationTopic": {
    "ConnectionString": "Endpoint=...",
    "TopicName": "notifications"
  }
}
```

## Session-Based Messaging

For guaranteed message ordering and stateful processing:

```json
{
  "OrderQueue": {
    "ConnectionString": "Endpoint=...",
    "QueueName": "orders",
    "SessionId": "customer-12345"
  }
}
```

```csharp
var context = _contextFactory.Create("OrderQueue", typeof(Order).FullName);
await _sender.SendAsync(order, context);
```

**Note:** The queue or topic must be session-enabled in Azure.

## Multi-Provider Usage

Use keyed services to work with multiple message queue providers simultaneously:

```csharp
using Microsoft.Extensions.DependencyInjection;

public class BridgeService
{
    private readonly IMessageSenderProvider _serviceBusSender;
    private readonly IMessageSenderProvider _sqsSender;

    public BridgeService(
        [FromKeyedServices("servicebus")] IMessageSenderProvider serviceBusSender,
        [FromKeyedServices("sqs")] IMessageSenderProvider sqsSender)
    {
        _serviceBusSender = serviceBusSender;
        _sqsSender = sqsSender;
    }

    public async Task BridgeMessage(Order order)
    {
        // Read from Service Bus
        // ...

        // Forward to AWS SQS
        var context = _contextFactory.Create("SqsQueue", typeof(Order).FullName);
        await _sqsSender.SendAsync(order, context);
    }
}
```

## Application Properties (Headers)

Custom headers are automatically converted to Service Bus ApplicationProperties:

```csharp
var context = _contextFactory.Create("OrderQueue", typeof(Order).FullName);
context.Headers["CustomerId"] = order.CustomerId;
context.Headers["OrderType"] = "Premium";
context.Headers["Region"] = "US-EAST";

await _sender.SendAsync(order, context);
```

## Connection String

Obtain your Service Bus connection string from:
1. Azure Portal → Service Bus Namespace → Shared access policies
2. Select a policy (e.g., "RootManageSharedAccessKey")
3. Copy the "Primary Connection String"

**Format:**
```
Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=<policy>;SharedAccessKey=<key>
```

## Error Handling

The provider throws `ConfigurationMissingException` if required configuration is missing:

```csharp
try
{
    var correlationId = await _sender.SendAsync(order, context);
}
catch (ConfigurationMissingException ex)
{
    // Handle missing configuration
    // Ex: "Configuration \"MessageQueuing:OrderQueue:ConnectionString\" is missing"
}
```

## Resource Disposal

The provider automatically disposes of `ServiceBusClient` and `ServiceBusSender` instances after each send operation to prevent connection leaks.

## See Also

- [Message Queue Architecture](../../../../Features/MessageQueuing/README.md)
- [Context-Based Pattern Documentation](../../../../Features/MessageQueuing/pattern-context-based.md)
- [Multi-Provider Examples](../../../../Features/MessageQueuing/archive/multi-provider-bridge-example.md)
- [Azure Service Bus Documentation](https://docs.microsoft.com/en-us/azure/service-bus-messaging/)
