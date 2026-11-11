# OoBDev.Amazon.Sqs

Sender provider for Amazon SQS in the [message queueing](../../../Framework/OoBDev.MessageQueueing/README.MessageQueueing.md) pipeline. It sends only: there is no receiver provider yet (a TODO in `AmazonSqsMessageProvider`), so inbound SQS handlers are not supported.

## Use

```csharp
services.TryAddMessageQueueingServices();   // OoBDev.MessageQueueing
services.TryAddAmazonSqsServices();         // sender provider, client factory, AsyncAPI contributor
services.TrySerializerExtensions();         // the JSON serializer the provider needs
```

Configuration is read by the message property resolver from the `MessageQueue` section (lookup order in the [MessageQueueing readme](../../../Framework/OoBDev.MessageQueueing/README.MessageQueueing.md#components)):

```json
{
  "MessageQueue": {
    "OrderPlaced": {
      "Provider": "sqs",
      "QueueName": "production-orders",
      "Region": "eu-west-1",
      "DelaySeconds": 0
    },
    "Default": {
      "Provider": "sqs",
      "QueueUrl": "https://sqs.us-east-1.amazonaws.com/123/orders.fifo",
      "MessageGroupId": "order-processing"
    }
  }
}
```

Inject `IMessageQueueSender` (or `IMessageQueueSender<TChannel>`) and call `SendAsync(message, messageId)`; the sender picks this provider through the keyed service `sqs` (`AwsSqsGlobals.MessageProviderKey`).

## Configuration keys

**Table 1 — Keys under the resolved message section (or its `Config` child)**

| Key | Meaning | Default |
|-----|---------|---------|
| `Provider` | Must be `sqs` to select this provider | |
| `QueueUrl` | Full queue URL; wins over `QueueName` | |
| `QueueName` | Queue name, resolved to a URL with `GetQueueUrl` | |
| `Region` | AWS region system name | `us-east-1` |
| `AccessKeyId`, `SecretAccessKey` | Explicit credentials; when `AccessKeyId` is empty the AWS SDK default credential chain is used | chain |
| `ServiceUrl` | Endpoint override for emulators such as Moto; also sets the authentication region | unset (real AWS) |
| `DelaySeconds` | Per-message delay, sent when it parses as an integer (SQS accepts 0 to 900; the provider does not check the range) | unset |
| `MessageGroupId` | Sent when non-empty; required by FIFO queues | unset |

Neither `QueueUrl` nor `QueueName` set throws `ConfigurationMissingException` naming `<section path>:QueueName`. FIFO queues are recognised only by their `.fifo` name; the provider sets no deduplication id, so use content-based deduplication on the queue.

## What is sent

`SendAsync` wraps the message in a `WrappedQueueMessage` (content type `application/json`, assembly-qualified payload type, correlation id or a new GUID, payload, headers), serializes it as the message body and sends it with `SendMessage`. Each non-null context header also becomes a `String` message attribute. It logs the SQS message id and correlation id (`[LoggerMessage]` event 1) and returns the SQS message id.

## AsyncAPI

`SqsAsyncApiContributor` describes the queue for the generated AsyncAPI document: host from `ServiceUrl` or `sqs.<region>.amazonaws.com`, queue name from `QueueName` or the last segment of `QueueUrl`, and an SQS binding with `fifoQueue` set for `.fifo` names.

## Tests

`OoBDev.Amazon.Sqs.Tests` holds the unit tests and the Moto-backed `Integration` tests; set-up of the Moto queues is in [TESTING.md](TESTING.md).
