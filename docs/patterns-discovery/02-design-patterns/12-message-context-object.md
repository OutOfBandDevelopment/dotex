# Pattern 12 — Message context object

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 11 — Marker-generic channels & handlers](./11-marker-generic-channels-handlers.md) · [Pattern 13 — Supervised hosted service →](./13-supervised-hosted-service.md)
<!-- nav -->

`IMessageContext` bundles correlation (`CorrelationId`, `RequestId`, `OriginMessageId`, `SentId`), provenance (`SentAt`, `SentBy`, `SentFrom`, caller method/line/file captured from the stack), a header bag, and the resolved `IConfigurationSection`. It flows to **both** sender providers and handlers, so cross-cutting information is never in the message payload. `IQueueMessage`/`WrappedQueueMessage` is the wire envelope (`ContentType`, `PayloadType`, `Payload`, `Properties`).

**Rough edge (under review, tracked in [`TODO.md`](../../../TODO.md)):** caller info is taken from `new StackFrame(5, true)`, which is brittle against async state-machine depth changes. The usual replacement is `[CallerMemberName]`/`[CallerFilePath]`/`[CallerLineNumber]` attributes (commented out in `IMessageContextFactory`), but they were deliberately avoided because they pollute the method signatures. The trade-off is acknowledged; whether to change is undecided.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 11 — Marker-generic channels & handlers](./11-marker-generic-channels-handlers.md) · [Pattern 13 — Supervised hosted service →](./13-supervised-hosted-service.md)
<!-- nav -->
