# OoBDev.MessageQueueing.Abstractions

Contracts for sending and handling messages over a queue without naming the queue product. Application code uses the sender and handler interfaces; the providers behind them (SQS, Service Bus, RabbitMQ and others) implement the `Services` interfaces. Namespaces: `OoBDev.MessageQueueing` and `OoBDev.MessageQueueing.Services`.

## Contents

- [Application contracts](#application-contracts)
- [Provider contracts](#provider-contracts)
- [Message context](#message-context)

## Application contracts

**Table 1 — Types application code uses**

| Type | Role |
|------|------|
| `IMessageQueueSender<TChannel>` | `SendAsync(message, messageId)` returns the sent id. The caller member, file and line are captured by caller attributes and end up in the context. `TChannel` names the channel (a marker type) so one message type can go to different queues. |
| `IMessageQueueHandler<TChannel, TMessage>` | `HandleAsync(message, context)` processes one received message. The non-generic `IMessageQueueHandler` takes `object` and is what the host calls. |
| `[MessageQueue("name")]` | Gives a channel or message class its simple queue name; repeatable and inherited. |

```csharp
public sealed class OrderChannel;

public sealed class OrderPlacedHandler : IMessageQueueHandler<OrderChannel, OrderPlaced>
{
    public Task HandleAsync(object message, IMessageContext context) => HandleAsync((OrderPlaced)message, context);

    public Task HandleAsync(OrderPlaced message, IMessageContext context)
    {
        // context.CorrelationId and context.Headers identify and describe the message
        return Task.CompletedTask;
    }
}
```

## Provider contracts

**Table 2 — Types a provider or the host implements**

| Type | Role |
|------|------|
| `IMessageSenderProvider` | `SendAsync(message, context)` writes to one queue product. |
| `IMessageSenderProviderFactory` | `Sender(channelType, messageType)` picks the provider for a pair. |
| `IMessageReceiverProvider` | `SetHandlerProvider(...)` then `RunAsync(ct)` receives until cancelled. |
| `IMessageReceiverProviderFactory` | `Create()` returns the configured receivers. |
| `IMessageHandlerProvider` | `HandleAsync(queueMessage, messageId)` routes a received message to the handlers; `Config` is its configuration section. |
| `IMessagePropertyResolver` | Resolves the provider key, simple names, configuration section and ids for a channel and message type. |
| `IMessageContextFactory` | Builds an `IMessageContext` for a send or a received `IQueueMessage`. |
| `IQueueMessage` and `WrappedQueueMessage` | The wire envelope: `ContentType`, `CorrelationId`, `Payload`, `PayloadType` and `Properties`. |

## Message context

`IMessageContext` carries `OriginMessageId`, `CorrelationId`, `RequestId`, `SentId`, `ChannelType`, `MessageType`, `SentAt`, `SentBy`, `SentFrom`, the free-form `Headers` (also reachable through the indexer) and the `Config` section of the channel.

The implementations, hosting and registration are in `OoBDev.MessageQueueing` and `OoBDev.MessageQueueing.Hosting`.
