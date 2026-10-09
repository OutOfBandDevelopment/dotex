# AsyncApi — API Design

[← Architecture](architecture.md) · [Testing strategy →](testing-strategy.md)

## Registration

```csharp
// services
services.TryAddAsyncApiExtensions(configuration);   // model, builder, catalog

// pipeline
app.MapAsyncApi();                                   // /asyncapi/{name}.json and /asyncapi/{name}
```

Configuration (all optional):

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
| `AsyncApiDocument`, `AsyncApiInfo`, `AsyncApiServer`, `AsyncApiChannel`, `AsyncApiOperation`, `AsyncApiMessage` | AsyncAPI 3.0 model (records) |
| `AsyncApiDocumentBuilder` | Builds a document by name from configuration and registered handler and sender types |
| `IAsyncApiContributor` | `Describe(string providerKey, IConfigurationSection config)` returns `AsyncApiContribution` (server, address, bindings) |
| `AsyncApiJsonWriter` | Serializes the model with `System.Text.Json` |
| `AsyncApiDocumentCatalog` | Document names (`all` plus assemblies) |

**Table 2 — Namespace `OoBDev.AsyncApi.AspNetCore`**

| Type | Role |
|------|------|
| `EndpointRouteBuilderExtensions.MapAsyncApi` | Maps the JSON and viewer endpoints |
| `AsyncApiViewerPage` | Renders the viewer HTML |

## Document shape

For a sender `IMessageQueueSender<OrdersChannel>` and handler `IMessageQueueHandler<OrdersChannel, OrderPlaced>` on SQS:

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
