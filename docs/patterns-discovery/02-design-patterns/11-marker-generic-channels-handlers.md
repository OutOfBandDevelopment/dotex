# Pattern 11 — Marker-generic channels & handlers

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration](./10-attribute-dispatch-proxy.md) · [Pattern 12 — Message context object →](./12-message-context-object.md)
<!-- nav -->

**What:** The *type argument is the routing identity*.

```csharp
IMessageQueueSender<TChannel>                       // send to a channel
IMessageQueueHandler<TChannel>                      // handle anything on a channel
IMessageQueueHandler<TChannel, TMessage>            // handle a specific message on a channel
[MessageQueue("simple-name")]                       // optional friendly name for config keys
```

Any class (even the handler itself, as in `ExampleMessageProvider`) can be the `TChannel`. `MessageSender<TChannel>` is registered as an open generic. Handlers are discovered from DI (`IEnumerable<IMessageQueueHandler>`) and grouped by `(providerKey, configPath, channelType)` by the receiver factory.

**Repeat:** use type parameters as *names* for configuration lookup; give them an attribute to decouple config names from CLR names.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 10 — Attribute-declared behavior + dispatch-proxy decoration](./10-attribute-dispatch-proxy.md) · [Pattern 12 — Message context object →](./12-message-context-object.md)
<!-- nav -->
