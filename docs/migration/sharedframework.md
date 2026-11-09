# SharedFramework

**Consolidated:** 2026-10-29 · **Status:** replaced by design documents; the January 2026 mapping, plan, DIFFERS comparison and phase 0 coverage notes are removed (`git log -- docs/migration` has them).

SharedFramework (52 projects, about 28,600 LOC, from `Incoming/SharedFramework`) was investigated in January 2026. The owner then replaced the code migration with design-first work: the features are written up as feature documents and built from those, not copied.

## Contents

- [Outcome](#outcome)
- [What the investigation found](#what-the-investigation-found)
- [Already built](#already-built)
- [Still open](#still-open)

## Outcome

- Design documents for the epics live in [docs/design](../design/SharedFramework/README.md) (Communications, Caching, Master data, Document services, Identity, Text templating, Data enhancement, Message composition). Four documents per feature: requirements, architecture, api-design, testing-strategy ([standard](../patterns-discovery/06-design-document-standard.md)).
- Code is written from those designs in the layers and patterns of this framework (rule `coding-standards`), not ported.
- Phase 0 (net10.0 and namespace cleanup) was already done in main.

## What the investigation found

**Table 1 — Findings of the January 2026 comparison**

| Area | Main at the time | SharedFramework | Decision |
|------|------------------|-----------------|----------|
| Communications | Empty stub | Complete composers, handlers, providers | Build from the design |
| Documents | Partial | Adds packaging and storage | Merge by design |
| Identity | Basic user management | Adds claims, rights, sessions | Merge by design |
| Text templating | Scattered | Unified framework | Consolidate by design |
| 19 other projects | No equivalent | New (queues, cloud adapters, data) | Build as needed, in priority order |

## Already built

- **Queue providers:** `OoBDev.Amazon.Sqs`, `OoBDev.Microsoft.Azure.ServiceBus`, `OoBDev.RabbitMQ`, with hosting and AsyncAPI contributors.
- **Storage and caching:** `OoBDev.Microsoft.Azure.StorageAccount`, `OoBDev.Microsoft.Caching`, `OoBDev.Redis.Caching`.
- **Communications:** `OoBDev.Communications`, `OoBDev.Communications.MessageQueueing` and the MailKit adapter.
- **Documents and templates:** `OoBDev.Documents` with its converters, Handlebars templating.
- **Identity:** `OoBDev.Identity`, `OoBDev.Identity.Jws`, the JWT bearer package and the Keycloak adapter.

Each has a project readme and, where finished as a feature, a change document in [docs/changes](../changes/README.md).

## Still open

Tracked in [BACKLOG.md](../../BACKLOG.md) ("SharedFramework is design first"): the remaining epic designs (2 Communications, 6 Documents, 7 Identity, 10 Text templating) and the code that follows them. The per-project gap lists of the removed notes are not carried over; recompute them against the design documents when an epic starts.
