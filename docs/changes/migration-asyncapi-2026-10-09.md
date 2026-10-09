# AsyncAPI document and viewer for the message queues

**Date:** 2026-10-09 · **Epic:** API documentation (phase 2 of [OpenApiScalar](migration-openapi-scalar-2026-10-09.md)) · **Status:** Complete

## Summary

The application now publishes an AsyncAPI 3.0 document for its message queues, with a viewer page beside Scalar. Design: [AsyncApi](../design/AsyncApi/README.md).

## What changed

**Table 1 — Changes**

| Item | Change |
|------|--------|
| New project | `OoBDev.AsyncApi`: model, `AsyncApiDocumentBuilder`, `AsyncApiJsonWriter`, `IAsyncApiContributor`, `TryAddAsyncApiServices()` |
| Adapters | `SqsAsyncApiContributor`, `ServiceBusAsyncApiContributor`, `RabbitMQAsyncApiContributor`, registered by each adapter's `TryAdd...` method |
| Endpoints | `MapAsyncApi()` in `OoBDev.AspNetCore.Mvc`: `/asyncapi/{name}.json` and the viewer `/asyncapi/{name}` |
| Config | Optional `AsyncApi:Title`, `Version`, `ViewerScriptUrl` |
| Example | `AddApplicationServices` registers the builder; `Program.cs` maps it in Development |
| Tests | `AsyncApiDocumentTests` (7 Simulate tests: structure, SQS/RabbitMQ/Service Bus channels, no secrets, per-assembly name, 404, viewer) |

## Decisions

- A thin first-party model, not Saunter (attribute driven, AsyncAPI 2.x) or LEGO.AsyncAPI (third party).
- Receive channels come from the registered handlers; payload schemas come from `JsonSchemaExporter`.
- `IMessageQueueSender<T>` is an open generic, so send channels come only from `MessageQueue` configuration entries no handler covers.
- Credentials (access keys, connection string keys, passwords) are never emitted; a test checks this.

## Follow-up

- Describe send-only queues by type; add an in-process contributor.
- The viewer page has not been checked in a browser (the JSON was checked against the running example).
