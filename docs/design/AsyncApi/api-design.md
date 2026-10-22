# AsyncApi — API Design

[← Architecture](architecture.md) · [Testing strategy →](testing-strategy.md)

## Registration

```csharp
// services
services.TryAddAsyncApiServices();                  // builder; adapters add their IAsyncApiContributor
services.AddAsyncApiSendMessage<OrdersChannel, OrderPlaced>(); // optional: list a sent message so its channel gets a schema

// pipeline
app.MapAsyncApi();                                   // /asyncapi/{name}.json and /asyncapi/{name}
```

Configuration (all optional; `Title` defaults to `Messaging` for `all` and to the assembly name otherwise, `Version` to `1.0.0`, `ViewerScriptUrl` to `AsyncApiEndpointRouteBuilderExtensions.DefaultViewerScriptUrl`, the pinned unpkg build shown below):

```json
"AsyncApi": {
  "Title": "Example messaging",
  "Version": "1.0.0",
  "ViewerScriptUrl": "https://unpkg.com/@asyncapi/web-component@2.6.5/lib/asyncapi-web-component.js"
}
```

## Public types

**Table 1 — Namespace `OoBDev.AsyncApi`**

| Type | Role |
|------|------|
| `AsyncApiDocument`, `AsyncApiInfo`, `AsyncApiServer`, `AsyncApiChannel`, `AsyncApiOperation`, `AsyncApiMessage`, `AsyncApiComponents`, `AsyncApiReference` | AsyncAPI 3.0 model (small classes) |
| `AsyncApiDocumentBuilder` | `DocumentNames()` and `Build(name)` (null for an unknown name); `AllDocumentName` is `all`. Built from the injected handlers and the `MessageQueue` configuration |
| `IAsyncApiContributor` | `ProviderKey` and `Describe(IConfigurationSection config)`, which returns `AsyncApiContribution?` (null when the queue cannot be described) |
| `AsyncApiJsonWriter` | Serializes the model with `System.Text.Json` |
| `AsyncApiContribution` | Record returned by an adapter: `ServerName`, `Server`, `Address`, optional `Bindings` |

**Table 2 — Namespace `OoBDev.AspNetCore.Mvc` (OoBDev.AspNetCore.Mvc project)**

| Type | Role |
|------|------|
| `AsyncApiEndpointRouteBuilderExtensions.MapAsyncApi` | Maps the JSON and viewer endpoints; renders the viewer HTML |

## Document shape

For a handler `IMessageQueueHandler<OrdersChannel, OrderPlaced>` on SQS:

```json
{
  "asyncapi": "3.0.0",
  "info": { "title": "Example messaging", "version": "1.0.0" },
  "servers": { "sqs": { "host": "sqs.us-east-1.amazonaws.com", "protocol": "sqs" } },
  "channels": {
    "OrdersChannel.OrderPlaced": {
      "address": "orders",
      "servers": [ { "$ref": "#/servers/sqs" } ],
      "messages": { "OrderPlaced": { "payload": { "$ref": "#/components/schemas/OrderPlaced" } } }
    }
  },
  "operations": {
    "receiveOrderPlaced": { "action": "receive", "channel": { "$ref": "#/channels/OrdersChannel.OrderPlaced" } }
  },
  "components": { "schemas": { "OrderPlaced": { "type": "object" } } }
}
```

Payload schemas are produced from the message type by reflection (properties, nullability, enums), with XML summaries when available. Secrets are never written.

[← Architecture](architecture.md) · [Testing strategy →](testing-strategy.md)
