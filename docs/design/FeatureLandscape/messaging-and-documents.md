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
| Communications | `OoBDev.Communications` plus Abstractions, Hosting, MessageQueueing | RFC 5321/5322 (mail), RFC 2045 (MIME) | channel providers beyond email and SMS are not designed |
| MessageComposition | none | MIME, RFC 8058 (one-click unsubscribe) | design only |
| TextTemplating | Handlebars adapter, `OoBDev.System.Text.Templating.Xslt` | XSLT 1.0 | engine selection by key is not shown in the design |
| TemplateTools | `OoBDev.TemplateEngine.Cli` | none | status unclear |
| DataEnhancement | none | none | design only |
| DocumentServices | `OoBDev.Documents` plus Abstractions, ExternalTools, Guides, Hpgl | PDF 2.0 (ISO 32000), Tika detection | none recorded (design reconciled 2026-11-11) |
| SharedCaching | `OoBDev.Caching` plus Abstractions | RFC 9111 (HTTP caching semantics) for the web part | attribute-based and background-task parts unconfirmed |
| SharedFramework | umbrella epics | none | epics 2, 6, 7 and 10 still design-first |
| CqrsEventSourcing | design drafted, no library | CloudEvents 1.0 for event envelopes | decisions list for the owner (see the CQRS design) |
| MasterData | none | none | design only |

## MessageQueuing and SqlServiceBroker

- **Works with:** AsyncApi (documents the queues), Communications (sends through the queue), CqrsEventSourcing (the event feed could ride on it).
- **Known issues:** broker-specific behaviour (visibility timeout, prefetch, ordering) differs per adapter; the design should list which guarantees the abstraction promises.
- **Practices:** handler pattern `IHandler<TRequest, TResponse>`, provider/factory per broker, outbox for publish-after-commit.
- **Built 2026-11-16:** the inbox half: `IdempotentMessageQueueHandler` with `IInboxStore` (idempotent consumers for every adapter).
- **Built 2026-11-16:** the outbox half: `OutboxMessageQueueSender`, `OutboxDispatcher`, `IOutboxStore`.
- **Built 2026-11-16:** database `IInboxStore` and `IOutboxStore` in `OoBDev.MessageQueueing.EntityFrameworkCore` (type name plus JSON with a registered-type allow-list; inbox claim by primary key).
- **Built 2026-11-16:** the outbox write in the caller's business transaction: `modelBuilder.AddOutbox()` in the business context and `db.StageOutbox(types, message)` saved by its own `SaveChanges`; the dispatcher store reads the same table.
- **Missing:** shipped migrations.

## Communications and MessageComposition

- **Works with:** MessageQueuing, TextTemplating, DataEnhancement, DocumentServices (composition feeds delivery).
- **Known issues:** the Communications design still describes a 16-line stub that has since grown into four projects.
- **Standards:** SMTP and message format (RFC 5321/5322), MIME (RFC 2045-2049), DKIM/SPF/DMARC for sending, RFC 8058 unsubscribe.
- **Built:** delivery status tracking and the SES webhook (delivered, bounce, complaint, open, click) exist; 2026-11-16 added the suppression list (`ISuppressionList`, `SuppressingSendService`).
- **Built 2026-11-16:** `DeliveryStatus.Complained`, recipients on `DeliveryStatusUpdate`, and `SuppressionStatusHandler` feed the list from permanent bounces and complaints.
- **Built 2026-11-16:** durable list (`OoBDev.Communications.EntityFrameworkCore`) and signed RFC 8058 unsubscribe links (`UnsubscribeLinks`).
- **Built 2026-11-19:** the unsubscribe HTTP endpoint (`OoBDev.Communications.Web`, `MapUnsubscribe`, POST one-click and a GET confirmation page).
- **Built:** SMS (`ISmsMessage`, Twilio adapter). **Missing:** push notifications (FCM, APNs) as a channel; needs a vendor decision.

## TextTemplating, TemplateTools and DataEnhancement

- **Works with:** MessageComposition (body generation) and DocumentServices (report output).
- **Known issues:** two engines (Handlebars, XSLT) exist; the design should state how a template is matched to an engine and how output encoding (HTML escaping) is guaranteed.
- **Practices:** provider keyed by engine name, sandboxed template execution, no code in templates.
- **Built 2026-11-16:** `TemplateRenderHarness` renders templates against sample data and reports the first differing line.
- **Built 2026-11-16:** `LocalizedTemplateEngine` (culture suffix lookup with parent fallback).
- **Built:** versioned, tenant and culture aware database templates (`OoBDev.System.Text.Templating.EntityFrameworkCore`: publish, latest or specific version, rollback, withdraw, audit columns). Not built: shipped migrations, SQL Server and PostgreSQL runs, a cache in front of the source.

## DocumentServices

- **Works with:** HybridSearch (text extraction), MessageComposition (attachments), GeneratedGuides.
- **Known issues:** external tools (Tika container, converters) make most tests Integration; HPGL is a niche format kept for the migration scope.
- **Standards:** ISO 32000 (PDF), PDF/A for archives, Office Open XML (ECMA-376).
- **Built 2026-11-16:** `IContentScanner` and the fail-closed `ScanningBlobContainer` decorator.
- **Missing:** OCR as a provider. **Built:** `OoBDev.ClamAv` scanner adapter (clamd INSTREAM).

## SharedCaching and SharedFramework

- **Works with:** every feature that caches (embeddings, rights, OpenAPI documents).
- **Known issues:** cache key and invalidation conventions are in the design only; the configured `OoBDev:Caching:Disabled` switch exists.
- **Standards:** RFC 9111 for HTTP caches.
- **Built 2026-11-16:** in-process stampede protection in `CachedProxy` (one compute per key for concurrent misses). The distributed option already exists as the `redis` provider (`OoBDev.Redis.Caching`).
- **Built 2026-11-16:** cross-process single flight: optional `ICachingLock` on the provider (Redis `SET NX` lock with owner token), used by `CachedProxy` so one process computes a cold key and the others wait for it.

## CqrsEventSourcing and MasterData

- **Works with:** MessageQueuing, SharedFramework Complex Events (`OoBDev.ComplexEvents`), Search (projections).
- **Known issues:** CQRS is draft only; MasterData has no code and no owner-confirmed scope.
- **Standards:** CloudEvents 1.0, optimistic concurrency with expected sequence.
- **Built:** snapshots (`ISnapshotable`, `SnapshotOptions`) and event upcasting (`IEventUpcaster`, `EventUpcastPipeline`). GDPR erasure (`ErasureService`, `PersonalDataRule`, `IEventPayloadRewriter`).
