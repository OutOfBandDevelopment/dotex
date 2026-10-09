# Pattern 13 — Supervised hosted service

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 12 — Message context object](./12-message-context-object.md) · [Pattern 14 — Strategy collections (engine → providers → sources) →](./14-strategy-collections.md)
<!-- nav -->

`MessageReceiverHost : IHostedService, IDisposable` — one `Task` per receiver, `while(!cancelled) { try { await provider.RunAsync } catch { log; delay 10s } }`, single `CancellationTokenSource`, `StopAsync` cancels then `Task.WhenAll`. Library projects never reference hosting; `*.Hosting` projects do.

**Repeat:** receivers implement `RunAsync(CancellationToken)` and *throw to be restarted*; the host owns retry.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 12 — Message context object](./12-message-context-object.md) · [Pattern 14 — Strategy collections (engine → providers → sources) →](./14-strategy-collections.md)
<!-- nav -->
