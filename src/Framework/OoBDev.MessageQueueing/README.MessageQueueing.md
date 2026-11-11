# OoBDev.MessageQueueing

Sends messages to a queue and routes received messages to handlers, with the queue technology chosen by configuration. The contracts are in [OoBDev.MessageQueueing.Abstractions](../OoBDev.MessageQueueing.Abstractions/README.MessageQueueing.Abstractions.md); the receiving loop is in [OoBDev.MessageQueueing.Hosting](../OoBDev.MessageQueueing.Hosting/README.MessageQueueing.Hosting.md); the SQS and Service Bus adapters add providers.

## Use

```csharp
services.TryAddMessageQueueingServices();   // sender, resolver, factories, in-process provider
services.TryAddMessageQueueingHosting();    // OoBDev.MessageQueueing.Hosting: runs the receivers
```

```json
{
  "MessageQueue": {
    "Default": { "Provider": "in-process" }
  }
}
```

Inject `IMessageQueueSender` (channel `object`) or `IMessageQueueSender<TChannel>` and call `SendAsync(message, messageId)`; implement `IMessageQueueHandler` types for the receiving side.

## Components

**Table 1 — Components (namespace `OoBDev.MessageQueueing`, services in `.Services`)**

| Type | Role |
|------|------|
| `MessageSender<TChannel>` | `SendAsync` resolves the message id, request id and configuration, builds an `IMessageContext` (with the caller member, file and line), picks the sender provider for the channel and message type, logs sending, sent and failed, and returns the sent id, else the correlation id, else the message id given, else the generated request id. A failing provider is logged and the exception is rethrown. |
| `MessagePropertyResolver` | Finds configuration under `MessageQueue:` in this order: `{Channel}:{Message}`, `{Message}`, `{Channel}`, `Default`; the first section with children wins. Names are the type names, or `SimpleName` of a `MessageQueueAttribute`; the `object` channel is `Default`. A nested `Config` section is used when present. `Provider` of the section is the provider key. |
| `MessageSenderProviderFactory` | Resolves the keyed `IMessageSenderProvider` for the provider key; when none is registered the key is read as an assembly-qualified type name and created with `ActivatorUtilities`. |
| `MessageReceiverProviderFactory` | Groups the registered `IMessageQueueHandler` instances by provider key, configuration path and channel, gets the keyed `IMessageReceiverProvider` for each group, and gives it a `MessageHandlerProvider` holding the group's handlers. Groups without a provider are logged and skipped; a section with `DisableReceiver` set to `true` or `1` is skipped. |
| `MessageHandlerProvider` | Builds the message context, converts the payload to the message's `PayloadType` through the JSON serializer, and calls every handler of the group in turn. |
| `MessageContext`, `MessageContextFactory` | The context passed to senders and handlers (ids, headers, configuration, caller information). |
| `InProcessMessageProvider` | Sender and receiver in one: a singleton `ConcurrentQueue` per process, provider key `in-process`. `RunAsync` polls once a second (fixed, a TODO in the code) and throws when no handler provider was set. For tests and single-process use. |

Messages are wrapped as `WrappedQueueMessage` (content type, payload type, correlation id, payload, properties) by the in-process provider.

## Tests

`OoBDev.MessageQueueing.Tests` covers the sender, the in-process provider, the resolver, the receiver factory, the handler provider and the receiver host.
