# Pattern 12 — Message context object

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 11 — Marker-generic channels & handlers](./11-marker-generic-channels-handlers.md) · [Pattern 13 — Supervised hosted service →](./13-supervised-hosted-service.md)
<!-- nav -->

`IMessageContext` bundles correlation (`CorrelationId`, `RequestId`, `OriginMessageId`, `SentId`), provenance (`SentAt`, `SentBy`, `SentFrom`, caller method/line/file captured from the stack), a header bag, and the resolved `IConfigurationSection`. It flows to **both** sender providers and handlers, so cross-cutting information is never in the message payload. `IQueueMessage`/`WrappedQueueMessage` is the wire envelope (`ContentType`, `PayloadType`, `Payload`, `Properties`).

**Caller info:** `IMessageQueueSender.SendAsync` takes `[CallerMemberName]`, `[CallerFilePath]` and `[CallerLineNumber]` optional parameters and passes them to `IMessageContextFactory.Create`, which records `X-CallerMemberName`, `X-CallerFilePath` and `X-CallerLineNumber`. This replaced a `new StackFrame(5, true)` lookup (2026-10-09) that was brittle against async state-machine depth changes; callers need no change because the parameters are optional.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 11 — Marker-generic channels & handlers](./11-marker-generic-channels-handlers.md) · [Pattern 13 — Supervised hosted service →](./13-supervised-hosted-service.md)
<!-- nav -->
