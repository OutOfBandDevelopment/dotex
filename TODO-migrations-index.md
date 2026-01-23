# Migration TODOs - Index

🎨 **STRATEGIC PIVOT** - Code migration replaced with design-first documentation

**Last Updated:** 2026-01-23
**Purpose:** Index of all migration TODO files organized by incoming project

---

## Strategic Change (2026-01-22)

Instead of migrating SharedFramework code (~28,582 LOC across 52 projects), we created **120 comprehensive design documents** (90.9% complete) following the Epic 11 pattern. This ensures clean architecture from first principles without inheriting technical debt.

---

## Design Documentation Status

### ✅ Complete (6 epics - 104 documents)

1. **Epic 11: Data Enhancement Pipeline** (16 docs)
   - IDataContainer, lazy evaluation, path translation, schema discovery

2. **Epic 10: Text Templating Extensions** (12 docs)
   - Handlebars provider, database template source, IDataContainer integration

3. **Epic 12: Message Composition Service** (4 docs)
   - Orchestrator pattern, multi-format composition

4. **Epic 7: Identity & Session Management** (16 docs)
   - Account management, roles/claims, sessions, modular profiles

5. **Epic 6: Document Services** (44 docs)
   - 11 services: retrieval, persistence, conversion, extraction, rendering, splitting, composition, packing, unpacking, media detection, OCR

6. **Epic 4: Distributed Caching** (12 docs)
   - Transparent AOP caching, attribute-based declarative caching, platform-agnostic background tasks

### 🔄 Partial (2 epics - 16 documents)

7. **Epic 2: Communications Platform** (6 of 16 docs - 37.5%)
   - [TODO-migrations-communications.md](./TODO-migrations-communications.md)
   - Channel abstraction (4 docs complete)
   - Send & Receive (2 docs remaining)
   - User Preferences (4 docs remaining)
   - Multi-Channel Routing (4 docs remaining)

8. **Epic 5: Master Data & Test Data Management** (10 of 12 docs - 83%)
   - [TODO-migrations-data-loader.md](./TODO-migrations-data-loader.md)
   - [TODO-migrations-generations.md](./TODO-migrations-generations.md)
   - Master data loader (4 docs complete)
   - Test data loader (4 docs complete)
   - Data source providers (2 docs remaining)

---

## Code Migrations - Completed Before Pivot

### ✅ Complete (2 migrations)

1. **[Caching Framework](./Features/Caching/TODO-migrations-caching.md)** ✅ **COMPLETE (2026-01-20)**
   - 4 implementation + 3 test projects
   - Redis + Microsoft distributed caching
   - [Details](docs/changes/migration-caching-framework-2026-01-20.md)

2. **Message Queues** ✅ **COMPLETE (2026-01-20)** - [Archived](./docs/changes/TODO-migrations-message-queues.md)
   - AWS SQS + Azure Service Bus providers
   - LocalStack + Azure emulator integration
   - [Details](docs/changes/migration-message-queues-2026-01-20.md)

---

## Design Documentation References (Archived Code Migration Plans)

These files have been replaced with design documentation but are kept for reference:

3. **[TODO-migrations-communications.md](./TODO-migrations-communications.md)** 🎨
   - Now: Epic 2 design documentation (6 of 16 docs complete)

4. **[TODO-migrations-spatial.md](./TODO-migrations-spatial.md)** 🎨
   - Now: Part of Epic 6 (Document Services) design documentation

5. **[TODO-migrations-identity.md](./TODO-migrations-identity.md)** 🎨
   - Now: Epic 7 design documentation (16 docs complete)

6. **[TODO-migrations-documents.md](./TODO-migrations-documents.md)** 🎨
   - Now: Epic 6 design documentation (44 docs complete)

7. **[TODO-migrations-text-templating.md](./TODO-migrations-text-templating.md)** 🎨
   - Now: Epic 10 design documentation (12 docs complete)

8. **[TODO-migrations-data-loader.md](./TODO-migrations-data-loader.md)** 🎨
   - Now: Epic 5 design documentation (10 of 12 docs complete)

9. **[TODO-migrations-complex-events.md](./TODO-migrations-complex-events.md)** 🎨
   - Now: Part of Epic 6 (Document Services) design documentation

10. **[TODO-migrations-generations.md](./TODO-migrations-generations.md)** 🎨
    - Now: Part of Epic 5 design documentation (10 of 12 docs complete)

---

## Other Migration TODOs

11. **[TODO-migrations-framework.md](./TODO-migrations-framework.md)** 🗑️ **CANCELLED**
    - Directory removed from Incoming/
    - Vector library already in main codebase

12. **[TODO-migrations-binarydatadecoders.md](./TODO-migrations-binarydatadecoders.md)** ⏸️ **BLOCKED**
    - Massive codebase (~50,000 LOC)
    - Awaiting 14+ critical decisions

---

## Legend

- ✅ **COMPLETE** - Migration finished
- 🎨 **REPLACED** - Code migration replaced with design documentation
- 🗑️ **CANCELLED** - Directory removed, migration cancelled
- ⏸️ **BLOCKED** - Awaiting decisions or prerequisites
- 🔄 **PARTIAL** - Design documentation in progress

---

## Next Steps

1. Complete remaining 12 design documents:
   - Epic 2: Communications (10 docs)
   - Epic 5: Master Data (2 docs)

2. Review and approve all design documentation

3. Begin implementation based on approved designs

---

**Parent File:** [TODO-migrations.md](./TODO-migrations.md)
