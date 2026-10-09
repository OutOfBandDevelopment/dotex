# OoBDev.Amazon.Sqs - AWS SQS Message Queue Provider

Context-based message queue provider for AWS Simple Queue Service (SQS).

## Features

- ✅ Standard and FIFO queue support
- ✅ Message attributes and metadata
- ✅ Delay queues (0-900 seconds)
- ✅ Long polling support
- ✅ AWS credential chain integration
- ✅ Multi-region support
- ✅ Runtime queue configuration
- ✅ Multi-provider compatibility

## Installation

```bash
dotnet add package OoBDev.Amazon.Sqs
```

## Quick Start

### 1. Register Services

```csharp
using OoBDev.Amazon.Sqs;
using OoBDev.System.Text.Json;

services.TryAddAmazonSqsServices();
services.TryAddJsonSerializer();  // OoBDev.System
```

### 2. Configure Queue

**Using appsettings.json:**

```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "Provider": "sqs",
      "QueueName": "production-orders",
      "Region": "us-east-1",
      "DelaySeconds": 0
    },
    "FifoQueue": {
      "Provider": "sqs",
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123/orders.fifo",
      "MessageGroupId": "order-processing"
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
        context.Headers["Priority"] = "High";
        context.Headers["OrderType"] = order.Type;

        var messageId = await _sender.SendAsync(order, context);
    }
}
```

## Configuration Reference

| Key | Required | Description | Default |
|-----|----------|-------------|---------|
| `QueueName` | Yes* | SQS queue name | - |
| `QueueUrl` | Yes* | Full SQS queue URL | - |
| `Region` | No | AWS region (e.g., "us-east-1", "eu-west-1") | us-east-1 |
| `AccessKeyId` | No | AWS access key ID | (uses AWS credential chain) |
| `SecretAccessKey` | No | AWS secret access key | (uses AWS credential chain) |
| `DelaySeconds` | No | Message delay in seconds (0-900) | 0 |
| `MessageGroupId` | No | FIFO queue message group ID | - |

*Either `QueueName` or `QueueUrl` must be provided

## AWS Credential Chain

If `AccessKeyId` and `SecretAccessKey` are not provided, the provider uses the AWS SDK default credential chain:

1. Environment variables (`AWS_ACCESS_KEY_ID`, `AWS_SECRET_ACCESS_KEY`)
2. AWS credentials file (`~/.aws/credentials`)
3. IAM role for Amazon EC2 instances
4. IAM role for containers (ECS, EKS)

## FIFO Queue Support

For FIFO (First-In-First-Out) queues:

```json
{
  "MessageQueuing": {
    "OrderQueue": {
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123/orders.fifo",
      "MessageGroupId": "order-processing"
    }
  }
}
```

**Important:** FIFO queue URLs must end with `.fifo`

## Multi-Provider Usage

Use keyed services to work with multiple message queue providers simultaneously:

```csharp
using Microsoft.Extensions.DependencyInjection;

public class BridgeService
{
    private readonly IMessageSenderProvider _sqsSender;
    private readonly IMessageSenderProvider _rabbitSender;

    public BridgeService(
        [FromKeyedServices("sqs")] IMessageSenderProvider sqsSender,
        [FromKeyedServices("rabbitmq")] IMessageSenderProvider rabbitSender)
    {
        _sqsSender = sqsSender;
        _rabbitSender = rabbitSender;
    }

    public async Task BridgeMessage(Order order)
    {
        // Read from SQS
        // ...

        // Forward to RabbitMQ
        var context = _contextFactory.Create("RabbitQueue", typeof(Order).FullName);
        await _rabbitSender.SendAsync(order, context);
    }
}
```

## Message Headers

Custom headers are automatically converted to SQS message attributes:

```csharp
var context = _contextFactory.Create("OrderQueue", typeof(Order).FullName);
context.Headers["CustomerId"] = order.CustomerId;
context.Headers["Priority"] = "High";
context.Headers["ProcessingRegion"] = "US-EAST";

await _sender.SendAsync(order, context);
```

## Error Handling

The provider throws `ConfigurationMissingException` if required configuration is missing:

```csharp
try
{
    var messageId = await _sender.SendAsync(order, context);
}
catch (ConfigurationMissingException ex)
{
    // Handle missing configuration
    // Ex: "Configuration \"MessageQueuing:OrderQueue:QueueName\" is missing"
}
```

## See Also

- [Message Queue Architecture](../../../Features/MessageQueuing/README.md)
- [Context-Based Pattern Documentation](../../../Features/MessageQueuing/pattern-context-based.md)
- [Multi-Provider Examples](../../../Features/MessageQueuing/archive/multi-provider-bridge-example.md)
- [AWS SQS Documentation](https://docs.aws.amazon.com/sqs/)
