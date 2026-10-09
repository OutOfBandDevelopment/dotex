# AsyncApi

**Status:** Design (phase 2 of [OpenApiScalar](../OpenApiScalar/architecture.md#phase-2--asyncapi)) · **Owner decision:** an AsyncAPI viewer beside Scalar (answered 2026-10-09)

Publishes an AsyncAPI 3.0 document for the message queue surfaces (SQS, Service Bus, RabbitMQ, in-process) and a viewer page, so queue contracts are as discoverable as the REST contracts.

## Documents

| Document | Contents |
|----------|----------|
| [Requirements](requirements.md) | Goals, non-goals, package evaluation |
| [Architecture](architecture.md) | Components, discovery flow, adapter contributions |
| [API design](api-design.md) | Types, registration, document shape |
| [Testing strategy](testing-strategy.md) | Simulate tests and manual checks |

## Decision summary

- A thin first-party model (`OoBDev.AsyncApi`) is the default; neither `Saunter` nor `LEGO.AsyncAPI` is adopted (see [requirements](requirements.md#package-evaluation)).
- Channels come from the `MessageQueue` configuration plus the registered `IMessageQueueHandler<TChannel,TMessage>` and `IMessageQueueSender<TChannel>` types. Nothing is added to the Abstractions projects.
- Each adapter contributes a server and channel bindings through an `IAsyncApiContributor`.
- `/asyncapi/{name}.json` serves the document; `/asyncapi/{name}` serves a viewer.
