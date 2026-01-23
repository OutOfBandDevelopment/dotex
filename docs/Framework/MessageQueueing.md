# OoBDev - Message Queueing

See [back](MajorFunctionality.md)

## Summary

Messages and business event supported is provided by the `OoBDev.MessageQueueing` libraries.  

Handlers are provided in process though an Hosting Engine extension.

### Current Implementations

| Provider | Package | Description |
|----------|---------|-------------|
| In Process `ConcurrentQueue` | Built-in | In-memory queue for testing and development |
| Azure Storage Queues | `OoBDev.Azure.StorageAccount` | Azure Storage Queue integration |
| RabbitMQ | `OoBDev.RabbitMQ` | AMQP message broker integration |
| Azure Service Bus | `OoBDev.Microsoft.Azure.ServiceBus` | Azure Service Bus queues and topics |
| Amazon SQS | `OoBDev.Amazon.Sqs` | AWS Simple Queue Service integration |

### Planned Features

* Support for impersonating the originating ClaimsPrincipal

## Related Notes

* [MessageQueueing](../Libraries/OoBDev.MessageQueueing.md)
  * [MessageQueueing.Abstractions](../Libraries/OoBDev.MessageQueueing.Abstractions.md)
  * [MessageQueueing.Hosting](../Libraries/OoBDev.MessageQueueing.Hosting.md)
* [Azure.StorageAccount](../Libraries/OoBDev.Azure.StorageAccount.md)
* [RabbitMQ](../Libraries/OoBDev.RabbitMQ.md)
* [Amazon SQS](../Libraries/OoBDev.Amazon.Sqs.md)
* [Azure Service Bus](../Libraries/OoBDev.Microsoft.Azure.ServiceBus.md)

## Integration Testing

Message queue providers are tested using Docker-based emulators. See [Testing Guidelines](../architecture/testing/testing-guidelines.md).

### Test Configuration

```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task RabbitMQ_SendAndReceive_Succeeds()
{
    // Required values - must be configured
    var host = TestContext.GetRequiredProperty<string>("RABBITMQ_HOST");
    var connectionString = TestContext.GetRequiredProperty<string>("RABBITMQ_CONNECTION_STRING");

    // Industry-standard port - use default
    var port = TestContext.GetPropertyOrDefault("RABBITMQ_PORT", 5672);

    // ... test implementation
}
```

### Available Emulators

| Service | Port | Docker Service |
|---------|------|----------------|
| RabbitMQ | 5673 (AMQP), 15672 (Management) | `rabbitmq` |
| Azure Service Bus Emulator | 5672 | `servicebus-emulator` |
| LocalStack (SQS) | 4566 | `localstack` |

See [TEST_VARIABLES.md](../../TEST_VARIABLES.md) for complete configuration reference.

## Structure

```plantuml
top to bottom direction 

package Abstractions {
    interface IMessageQueueSender {
    }
    interface IMessageQueueHandler {
        HandleAsync(object, IMessageContext) : Task
    }
    class MessageQueueAttribute {
        + SimpleName : string 
    }

    interface IMessageContext {
        ...
    }
    interface IMessageHandlerProvider {
        + HandleAsync(IQueueMessage message, string messageId) : Task
        + Config : IConfigurationSection
    }
    interface IMessageReceiverProvider {
        + RunAsync(CancellationToken) : Task
    }
    interface IMessageReceiverProviderFactory {
        + Create() : IMessageReceiverProvider[]
    }
    interface IMessageSenderProvider {
        + SendAsync(object, IMessageContext) : Task<string?>
    }
    interface IMessageSenderProviderFactory {
        + Sender(Type, Type) : IMessageSenderProvider
    }
    interface IQueueMessage {
        ...
    }
}

package Implementation {
    class GenericHandler 
    class GenericProvider
}

package Hosting  {
    class MessageReceiverHost {
        - factory IMessageReceiverProviderFactory
        + StartAsync() : Task
        + StopAsync() : Task
    }
}

IMessageQueueHandler --> IMessageContext : uses
IMessageSenderProvider --> IMessageContext : uses
IMessageHandlerProvider o-- IMessageQueueHandler : uses

IMessageQueueSender  --* IMessageSenderProviderFactory : uses
IMessageHandlerProvider  --* IQueueMessage : uses
IMessageSenderProviderFactory  --> IMessageSenderProvider : uses
IMessageReceiverProviderFactory  --o IMessageReceiverProvider : uses
IMessageReceiverProviderFactory  --o IMessageQueueHandler : uses

IMessageSenderProvider --* IMessageHandlerProvider : uses

IMessageSenderProvider ^-- GenericProvider : implements 
IMessageReceiverProvider ^-- GenericProvider : implements 

IMessageQueueHandler ^-- GenericHandler : implements

MessageReceiverHost --* IMessageReceiverProviderFactory : uses

```

---

See [back](MajorFunctionality.md)
