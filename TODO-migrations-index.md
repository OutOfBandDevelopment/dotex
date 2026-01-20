# Migration TODOs - Index

**Last Updated:** 2026-01-20
**Purpose:** Index of all migration TODO files organized by incoming project

---

## SharedFramework Migration TODOs

### Critical Priority (Start Here)

1. **[TODO-migrations-communications.md](./TODO-migrations-communications.md)** 🔥
   - Status: Main has 16 LOC stub vs SF's 1,145 LOC
   - Impact: Complete multi-channel communications system
   - Priority: IMMEDIATE

2. **[TODO-migrations-caching.md](./Features/Caching/TODO-migrations-caching.md)** ✅ **COMPLETE (2026-01-20)**
   - Status: MIGRATED - 4 implementation + 3 test projects, full documentation
   - Impact: Redis + Microsoft distributed caching
   - Priority: HIGH

3. **[TODO-migrations-message-queues.md](./TODO-migrations-message-queues.md)** ✅ **COMPLETE (2026-01-20)**
   - Status: MIGRATED - AWS SQS + Azure Service Bus providers with Docker testing
   - Impact: Cloud message queue providers
   - Priority: HIGH

### High Priority (Enhance Main)

4. **[TODO-migrations-spatial.md](./TODO-migrations-spatial.md)** ✅
   - Status: Main has ZERO spatial services
   - Impact: Geocoding, Google Maps, Bing Maps (5 projects)
   - Priority: HIGH

5. **[TODO-migrations-identity.md](./TODO-migrations-identity.md)** ⚠️
   - Status: Main has basic 125 LOC, SF has 291 LOC + extensions
   - Impact: Azure B2C, claims, rights management, Graph API
   - Priority: HIGH

### Medium Priority (Merge Required)

6. **[TODO-migrations-documents.md](./TODO-migrations-documents.md)** ⚠️
   - Status: Main has 531 LOC, SF has 911 LOC
   - Impact: Adds packaging, resolvers, storage abstraction
   - Priority: MEDIUM

7. **[TODO-migrations-text-templating.md](./TODO-migrations-text-templating.md)** ⚠️
   - Status: Main scattered across projects, SF unified
   - Impact: Consolidated templating with persistence
   - Priority: MEDIUM

8. **[TODO-migrations-data-loader.md](./TODO-migrations-data-loader.md)** ✅
   - Status: Main has ZERO, SF has 2,146 LOC
   - Impact: Data import/export tooling
   - Priority: MEDIUM

### Lower Priority (New Capabilities)

9. **[TODO-migrations-complex-events.md](./TODO-migrations-complex-events.md)** ✅
   - Status: Main has ZERO
   - Impact: Event sourcing/CQRS (4 projects)
   - Priority: MEDIUM

10. **[TODO-migrations-generations.md](./TODO-migrations-generations.md)** ✅
    - Status: Main has ZERO
    - Impact: Code/data generation framework
    - Priority: LOW

---

## Other Migration TODOs

11. **[TODO-migrations-framework.md](./TODO-migrations-framework.md)**
    - Status: 55 files (30 NEW + 25 DIFFERS)
    - Impact: Core abstractions, database mapper, audit logging
    - Priority: HIGH - Phase 0 comparison required

12. **[TODO-migrations-binarydatadecoders.md](./TODO-migrations-binarydatadecoders.md)**
    - Status: Massive codebase (~50,000 LOC)
    - Impact: Binary processing, protocols, hardware
    - Priority: HIGH - Blocked by 14+ decisions

---

## Legend

- 🔥 CRITICAL - Main is empty or stub, immediate migration needed
- ⚠️ MERGE REQUIRED - Main has partial, SF significantly better
- ✅ SAFE TO MIGRATE - No overlap, straightforward migration
- Priority levels: IMMEDIATE > HIGH > MEDIUM > LOW

---

## Migration Workflow

1. **Review** individual TODO file for project
2. **Execute** migration phases as documented
3. **Test** thoroughly at each phase
4. **Update** TODO file with progress
5. **Mark complete** when all phases done

---

## Quick Reference

| Project | File | Main Status | SF LOC | Priority | Status |
|---------|------|-------------|--------|----------|--------|
| Communications | TODO-migrations-communications.md | 16 LOC stub | 1,145 | 🔥 IMMEDIATE | ⚠️ MailKit adapter needed |
| Caching | TODO-migrations-caching.md | ✅ **COMPLETE** | ~600 | HIGH | ✅ Migrated 2026-01-20 |
| Message Queues | TODO-migrations-message-queues.md | ✅ **COMPLETE** | ~750 | HIGH | ✅ Migrated 2026-01-20 |
| Spatial | TODO-migrations-spatial.md | None | ~1,200 | HIGH | ✅ None |
| Identity | TODO-migrations-identity.md | 125 LOC | 291+204 | HIGH | ⚠️ Merge needed |
| Documents | TODO-migrations-documents.md | 531 LOC | 911 | MEDIUM | ⚠️ Feature merge |
| TextTemplating | TODO-migrations-text-templating.md | Scattered | 424+117 | MEDIUM | ⚠️ Consolidation |
| DataLoader | TODO-migrations-data-loader.md | None | 2,146 | MEDIUM | ✅ None |
| ComplexEvents | TODO-migrations-complex-events.md | None | ~3,000 | MEDIUM | ✅ None |
| Generations | TODO-migrations-generations.md | None | ~600 | LOW | ✅ None |

---

**Parent File:** [TODO-migrations.md](./TODO-migrations.md)
