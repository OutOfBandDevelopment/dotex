# Landscape: messaging and documents

Part of the [feature landscape](README.md). Status is "a project exists under `src`", not "verified".

## Contents

1. [Summary](#summary)
2. [MessageQueuing and SqlServiceBroker](#messagequeuing-and-sqlservicebroker)
3. [Communications and MessageComposition](#communications-and-messagecomposition)
4. [TextTemplating, TemplateTools and DataEnhancement](#texttemplating-templatetools-and-dataenhancement)
5. [DocumentServices](#documentservices)
6. [SharedCaching and SharedFramework](#sharedcaching-and-sharedframework)
7. [CqrsEventSourcing and MasterData](#cqrseventsourcing-and-masterdata)

**List of tables**

1. [Table 1 — Messaging and document features](#table-1)

## Summary

**Table 1 — Messaging and document features**

| Feature | Code | Standards | Main gap |
|---------|------|-----------|----------|
| MessageQueuing | adapters (SQS, Service Bus, RabbitMQ) with AsyncAPI contributors | AMQP 0-9-1 and 1.0, AsyncAPI | dead-letter and retry policy is per adapter |
| SqlServiceBroker | built (design and code) | none (SQL Server feature) | none recorded |
| Communications | `OoBDev.Communications` plus Abstractions, Hosting, MessageQueueing | RFC 5321/5322 (mail), RFC 2045 (MIME) | design status stale |
| MessageComposition | none | MIME, RFC 8058 (one-click unsubscribe) | design only |
| TextTemplating | Handlebars adapter, `OoBDev.System.Text.Templating.Xslt` | XSLT 1.0 | engine selection by key is not shown in the design |
| TemplateTools | `OoBDev.TemplateEngine.Cli` | none | status unclear |
| DataEnhancement | none | none | design only |
| DocumentServices | `OoBDev.Documents` plus Abstractions, ExternalTools, Guides, Hpgl | PDF 2.0 (ISO 32000), Tika detection | design status stale |
| SharedCaching | `OoBDev.Caching` plus Abstractions | RFC 9111 (HTTP caching semantics) for the web part | attribute-based and background-task parts unconfirmed |
| SharedFramework | umbrella epics | none | epics 2, 6, 7 and 10 still design-first |
| CqrsEventSourcing | design drafted, no library | CloudEvents 1.0 for event envelopes | decisions list for the owner (see the CQRS design) |
| MasterData | none | none | design only |

## MessageQueuing and SqlServiceBroker

- **Works with:** AsyncApi (documents the queues), Communications (sends through the queue), CqrsEventSourcing (the event feed could ride on it).
- **Known issues:** broker-specific behaviour (visibility timeout, prefetch, ordering) differs per adapter; the design should list which guarantees the abstraction promises.
- **Practices:** handler pattern `IHandler<TRequest, TResponse>`, provider/factory per broker, outbox for publish-after-commit.
- **Built 2026-11-16:** the inbox half: `IdempotentMessageQueueHandler` with `IInboxStore` (idempotent consumers for every adapter).
- **Missing:** the outbox half (publish-after-commit store and dispatcher) and a database `IInboxStore`.

## Communications and MessageComposition

- **Works with:** MessageQueuing, TextTemplating, DataEnhancement, DocumentServices (composition feeds delivery).
- **Known issues:** the Communications design still describes a 16-line stub that has since grown into four projects.
- **Standards:** SMTP and message format (RFC 5321/5322), MIME (RFC 2045-2049), DKIM/SPF/DMARC for sending, RFC 8058 unsubscribe.
- **Missing:** bounce and complaint handling; delivery receipts; SMS and push as further channels behind the same provider.

## TextTemplating, TemplateTools and DataEnhancement

- **Works with:** MessageComposition (body generation) and DocumentServices (report output).
- **Known issues:** two engines (Handlebars, XSLT) exist; the design should state how a template is matched to an engine and how output encoding (HTML escaping) is guaranteed.
- **Practices:** provider keyed by engine name, sandboxed template execution, no code in templates.
- **Built 2026-11-16:** `TemplateRenderHarness` renders templates against sample data and reports the first differing line.
- **Missing:** template versioning and localisation.

## DocumentServices

- **Works with:** HybridSearch (text extraction), MessageComposition (attachments), GeneratedGuides.
- **Known issues:** external tools (Tika container, converters) make most tests Integration; HPGL is a niche format kept for the migration scope.
- **Standards:** ISO 32000 (PDF), PDF/A for archives, Office Open XML (ECMA-376).
- **Built 2026-11-16:** `IContentScanner` and the fail-closed `ScanningBlobContainer` decorator.
- **Missing:** OCR as a provider; a real scanner adapter (ClamAV in the test stack).

## SharedCaching and SharedFramework

- **Works with:** every feature that caches (embeddings, rights, OpenAPI documents).
- **Known issues:** cache key and invalidation conventions are in the design only; the configured `OoBDev:Caching:Disabled` switch exists.
- **Standards:** RFC 9111 for HTTP caches.
- **Built 2026-11-16:** in-process stampede protection in `CachedProxy` (one compute per key for concurrent misses). The distributed option already exists as the `redis` provider (`OoBDev.Redis.Caching`).
- **Missing:** cross-process single flight (each instance computes once on a cold key).

## CqrsEventSourcing and MasterData

- **Works with:** MessageQueuing, SharedFramework Complex Events (`OoBDev.ComplexEvents`), Search (projections).
- **Known issues:** CQRS is draft only; MasterData has no code and no owner-confirmed scope.
- **Standards:** CloudEvents 1.0, optimistic concurrency with expected sequence.
- **Missing:** snapshotting, upcasting of old events, and GDPR erasure for append-only stores.
