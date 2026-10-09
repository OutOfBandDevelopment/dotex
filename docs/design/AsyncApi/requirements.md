# AsyncApi — Requirements

[← Overview](README.md) · [Architecture →](architecture.md)

## Goals

1. One AsyncAPI 3.0 JSON document per application describing every queue the application sends to or receives from.
2. Derived from what is registered and configured, so it cannot drift from the running code.
3. Provider specific details (queue name, topic, host, region) appear as servers and channel bindings, with no secrets.
4. A viewer page served beside Scalar.
5. Opt-in and config-gated; no change to existing message queue behaviour or APIs.

## Non-goals

- Changing `OoBDev.MessageQueueing.Abstractions` or the provider selection rules.
- Describing HTTP APIs (that stays OpenAPI).
- Code generation from the document.

## Package evaluation

**Table 1 — Options**

| Option | Fit | Verdict |
|--------|-----|---------|
| `Saunter` | Attribute driven (`[AsyncApi]`, `[Channel]`) on classes; targets AsyncAPI 2.x; our channels are configuration driven, not attribute driven | Rejected |
| `LEGO.AsyncAPI` | Good model and writer for AsyncAPI 2/3; third party, no discovery | Rejected as a dependency; its model shape is a useful reference |
| Thin first-party model | About a dozen small types; writes JSON with `System.Text.Json`; matches the owner rule to prefer first-party code | Adopted |

## Source of channels

**Table 2 — Facts used**

| Fact | Where |
|------|-------|
| Config resolution | `MessageQueue:{channel}:{message}`, then `MessageQueue:{message}`, then `MessageQueue:{channel}`, then `MessageQueue:Default` |
| Provider key | `Provider`: `sqs`, `servicebus`, `rabbit-mq`, `in-process` |
| Provider settings | SQS `Region`, `ServiceUrl`, `QueueUrl`, `QueueName`; Service Bus `QueueName`, `TopicName`; RabbitMQ `HostName`, `Port`, `QueueName` |
| Message names | `[MessageQueue("name")]` overrides the simple type name |
| Directions | `IMessageQueueSender<TChannel>` is `send`; `IMessageQueueHandler<TChannel,TMessage>` is `receive` |

Credentials (`AccessKeyId`, `SecretAccessKey`, `ConnectionString`, `Password`, `UserName`) are never emitted.

## Acceptance

- The example web API produces a document with a channel per configured queue, the message payload schema, and the right server per provider.
- The document is valid AsyncAPI 3.0 JSON (checked structurally in tests).
- The viewer loads the document.

[← Overview](README.md) · [Architecture →](architecture.md)
