# Pattern 5 — Config-resolved provider per channel/message

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 4 — `ISelectedService<T>` — config-selected provider](./04-selected-service.md) · [Pattern 6 — Builder *records* carrying config section names →](./06-builder-records.md)
<!-- nav -->

**What:** For messaging, *which provider* is chosen from configuration keyed by **channel type** and **message type**, with a most-specific-wins fallback chain:

```
MessageQueue:{Channel}:{Message}   →   MessageQueue:{Message}   →   MessageQueue:{Channel}   →   MessageQueue:Default
```

Each section carries `Provider` (a keyed-service key, **or an assembly-qualified type name** as a last resort resolved with `ActivatorUtilities`) and an optional `Config` sub-section that is handed to the provider through `IMessageContext.Config`.

**See:** `MessagePropertyResolver`, `MessageSenderProviderFactory`, `MessageReceiverProviderFactory`.

**Repeat:** use when routing depends on *what* is being sent. Put the resolver behind an interface (`IMessagePropertyResolver`) with `virtual` members so apps can override naming/ID rules.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 4 — `ISelectedService<T>` — config-selected provider](./04-selected-service.md) · [Pattern 6 — Builder *records* carrying config section names →](./06-builder-records.md)
<!-- nav -->
