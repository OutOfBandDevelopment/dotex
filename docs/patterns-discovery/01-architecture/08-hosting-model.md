# Architecture § 8 — Hosting Model

[↑ Architecture](./README.md) · [← 7. Cross-Cutting Building Blocks (all in `OoBDev.System*`)](./07-cross-cutting-building-blocks.md) · [9. Build & Packaging Architecture →](./09-build-and-packaging.md)

Background work is isolated in `*.Hosting` projects so libraries never force a host dependency:

* `OoBDev.MessageQueueing.Hosting` → `MessageReceiverHost : IHostedService` obtains `IMessageReceiverProviderFactory.Create()`, runs each receiver in its own `Task` with a **supervise-and-restart loop** (10-second back-off, hard-coded TODO), cancels via a shared `CancellationTokenSource`.
* Other hosts: `EmailMessageReceiverHost` (MailKit; only compiled into Common in `DEBUG` builds and disabled by the example app), `EmbeddingSentenceTransformerQueueReaderHost` (vectors).
* Registration is `services.TryAddXxxHosting()`, toggled off individually through `HostingBuilder`.

---

[↑ Architecture](./README.md) · [← 7. Cross-Cutting Building Blocks (all in `OoBDev.System*`)](./07-cross-cutting-building-blocks.md) · [9. Build & Packaging Architecture →](./09-build-and-packaging.md)
