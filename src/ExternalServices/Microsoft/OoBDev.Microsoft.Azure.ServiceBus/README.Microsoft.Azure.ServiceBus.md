# OoBDev.Microsoft.Azure.ServiceBus

Sender provider for Azure Service Bus queues and topics in the [message queueing](../../../Framework/OoBDev.MessageQueueing/README.MessageQueueing.md) pipeline. It sends only: there is no receiver provider, so inbound Service Bus handlers are not supported. Authentication is by connection string.

## Use

```csharp
services.TryAddMessageQueueingServices();    // OoBDev.MessageQueueing
services.TryAddAzureServiceBusServices();    // sender provider, sender factory, AsyncAPI contributor
services.TrySerializerExtensions();          // the JSON serializer the provider needs
```

Configuration is read by the message property resolver from the `MessageQueue` section (lookup order in the [MessageQueueing readme](../../../Framework/OoBDev.MessageQueueing/README.MessageQueueing.md#components)):

```json
{
  "MessageQueue": {
    "OrderPlaced": {
      "Provider": "servicebus",
      "ConnectionString": "Endpoint=sb://myservicebus.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...",
      "QueueName": "orders"
    },
    "Default": {
      "Provider": "servicebus",
      "ConnectionString": "Endpoint=sb://...",
      "TopicName": "notifications"
    }
  }
}
```

Inject `IMessageQueueSender` (or `IMessageQueueSender<TChannel>`) and call `SendAsync(message, messageId)`; the sender picks this provider through the keyed service `servicebus` (`AzureServiceBusGlobals.MessageProviderKey`).

## Configuration keys

**Table 1 — Keys under the resolved message section (or its `Config` child)**

| Key | Meaning |
|-----|---------|
| `Provider` | Must be `servicebus` to select this provider |
| `ConnectionString` | Required; a missing value throws `ConfigurationMissingException` naming `<section path>:ConnectionString` |
| `QueueName` | Entity to send to. When both names are set, the queue wins |
| `TopicName` | Topic to send to, used when `QueueName` is empty. Neither set throws `ConfigurationMissingException` |
| `SessionId` | A fixed session id put on every message sent with this configuration; the entity must be session-enabled |

## What is sent

`SendAsync` wraps the message in a `WrappedQueueMessage` (content type `application/json`, assembly-qualified payload type, correlation id or a new GUID, payload, headers) and sends the serialized bytes as the body of a `ServiceBusMessage` with the correlation id and content type set. Each non-null context header becomes an application property. The returned value is the correlation id (Service Bus assigns no id the provider could return), and one `[LoggerMessage]` (event 1) records it.

The provider creates a `ServiceBusClient` and sender for every send and disposes both in a `finally`, so a send does not keep a connection open. Scheduled delivery, per-message session ids and message settlement are not supported.

## AsyncAPI

`ServiceBusAsyncApiContributor` describes the entity for the generated AsyncAPI document: host from the `Endpoint` part of the connection string (else `servicebus`), protocol `amqp`, channel name from `QueueName` or `TopicName`. It does not read the credentials.

## Tests

`OoBDev.Microsoft.Azure.ServiceBus.Tests` holds unit tests and `Integration` tests against the Service Bus emulator; setup is in [TESTING.md](TESTING.md).
