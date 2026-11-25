# OoBDev.RabbitMQ

Sender and receiver providers for RabbitMQ queues in the [message queueing](../../../Framework/OoBDev.MessageQueueing/README.MessageQueueing.md) pipeline, using the RabbitMQ .NET client over AMQP. Messages go to the default exchange with the queue name as routing key.

## Use

```csharp
services.TryAddMessageQueueingServices();   // OoBDev.MessageQueueing
services.TryAddMessageQueueingHosting();    // runs the receivers
services.TryAddRabbitMQServices();          // providers, client factory, AsyncAPI contributor
services.TrySerializerExtensions();         // the JSON serializer the provider needs
```

Configuration is read by the message property resolver from the `MessageQueue` section (lookup order in the [MessageQueueing readme](../../../Framework/OoBDev.MessageQueueing/README.MessageQueueing.md#components)):

```json
{
  "MessageQueue": {
    "OrderPlaced": {
      "Provider": "rabbit-mq",
      "HostName": "localhost",
      "Port": 5672,
      "UserName": "guest",
      "Password": "guest",
      "QueueName": "orders"
    }
  }
}
```

The provider key is `rabbit-mq` (`RabbitMQGlobals.MessageProviderKey`); it is registered as both `IMessageSenderProvider` and `IMessageReceiverProvider`.

## Configuration keys

**Table 1 — Keys under the resolved message section (or its `Config` child)**

| Key | Meaning |
|-----|---------|
| `Provider` | Must be `rabbit-mq` |
| `HostName` | Required; a missing value throws `ConfigurationMissingException` |
| `QueueName` | Required; the queue to publish to and consume from |
| `Port` | Optional; client default 5672 |
| `UserName`, `Password` | Optional; client default `guest` |
| `RequestedConnectionTimeout`, `HandshakeContinuationTimeout` | Optional, in milliseconds; shorten them in tests |

## Behaviour

- **Send:** wraps the message in a `WrappedQueueMessage` (payload type, correlation id, payload, headers), serializes it as JSON, declares the queue (durable, not exclusive, not auto-delete) and publishes it persistent and `mandatory` with the correlation id set. A connection and channel are opened and closed per send. Returns the correlation id.
- **Receive:** `RunAsync` declares the queue, consumes with auto-acknowledge and passes each deserialized message to the handler provider until the token is cancelled. Auto-acknowledge means a message is lost when its handler throws; there are no retries, dead-lettering or prefetch settings.
- **AsyncAPI:** `RabbitMQAsyncApiContributor` describes the server (`HostName[:Port]`, protocol `amqp`) and an AMQP queue binding for `QueueName`; it does not read the credentials.

## Tests

`OoBDev.RabbitMQ.Tests` holds `Unit` tests and `Integration` tests against the RabbitMQ container of the test stack (host port 5673, see `TEST_VARIABLES.md`).
