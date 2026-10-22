# AsyncApi — Testing Strategy

[← API design](api-design.md) · [Overview](README.md)

## Unit tests

`AsyncApiDocumentBuilderTests` (in `OoBDev.AsyncApi.Tests`, 9 tests) covers the builder with a mocked `IMessagePropertyResolver`: document names, unknown name, receive operation, payload schema with enums, title and version, contributor wiring, provider-only description, and sender operations from configuration. The contributor rows exist as Unit tests in the adapter test projects (`SqsAsyncApiContributorTests` 5, `ServiceBusAsyncApiContributorTests` 3, `RabbitMQAsyncApiContributorTests` 3), including the secrets check per contributor. Config resolution order and the `[MessageQueue]` name override are behaviors of `IMessagePropertyResolver` (OoBDev.MessageQueueing) and are covered by `MessagePropertyResolverTests` there.

**Table 1 — Builder and contributors**

| Test | Checks |
|------|--------|
| Config resolution order | Channel section chosen by `{channel}:{message}`, `{message}`, `{channel}`, `Default` |
| `[MessageQueue]` name | Attribute overrides the simple name |
| Sender and handler | `send` and `receive` operations appear for the right types |
| SQS, Service Bus, RabbitMQ contributors | Server host, protocol, address and bindings from sample configuration |
| Secrets | `AccessKeyId`, `SecretAccessKey`, `ConnectionString` key, `Password` never appear in the JSON |
| Payload schema | Properties, required, enums |

## Simulate tests

`AsyncApiDocumentTests` (in `OoBDev.AspNetCore.Tests`, 7 tests) starts a Kestrel host (as `OpenApiDocumentTests` does) with in-process handlers and stub provider configuration, then requests:

- `/asyncapi/all.json`: `asyncapi` is `3.0.0`, expected channels, operations and servers.
- `/asyncapi/{assembly}.json`: only that assembly's channels (case-insensitive name).
- `/asyncapi/all`: HTML containing the script URL and the JSON path.
- Unknown name returns 404.

## Manual

Run the example web API and open `/asyncapi/all` to confirm the viewer renders the SQS, Service Bus and RabbitMQ channels.

## Gaps

- Validation against the official AsyncAPI JSON schema is structural only (no new dependency).

[← API design](api-design.md) · [Overview](README.md)
