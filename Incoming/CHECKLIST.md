# Incomming Directory - Processing Checklist

**Last Updated:** 2026-01-13

This checklist tracks the investigation and migration status of all projects in the `Incomming/` directory.

---

## Processing Status Legend

- ✅ **COMPLETE** - Investigation complete, migration executed or decision made
- 🔍 **INVESTIGATED** - Analysis complete, awaiting decision or migration execution
- ⏸️ **PENDING** - Awaiting user decision on migration approach
- 🚧 **IN PROGRESS** - Currently being analyzed or migrated
- ⏱️ **QUEUED** - Not yet started
- 🗑️ **DELETED** - Directory removed after successful migration/extraction

---

## Projects Status

| Project | Status | Type | Files | LOC | Priority | Action Required |
|---------|--------|------|-------|-----|----------|-----------------|
| **OoBDev.Oobtainium** | ⏸️ **PENDING** | Mocking Framework | 48 | ~1,578 | LOW | Choose Option 1-4 |
| **BotChat** | 🔍 **INVESTIGATED** | Sample Application | 12 | ~393 | LOW | Migration decision needed |
| **BinaryDecoders** | 🔍 **INVESTIGATED** | Massive Codebase | ~500 | ~50,000 | HIGH | Critical questions answered |
| **CloudOrchestrator** | ⏱️ **QUEUED** | ? | ? | ? | ? | Not yet investigated |
| **ContractParser** | ⏱️ **QUEUED** | ? | ? | ? | ? | Not yet investigated |
| **SharedFramework** | 🔍 **INVESTIGATED** | Framework Library | 52 | ~28,582 | HIGH | Systematic migration required |
| **Tools** | ⏱️ **QUEUED** | Tools/CLI | ? | ? | ? | Not yet investigated |

---

## Detailed Status

### ⏸️ OoBDev.Oobtainium (PENDING DECISION)

**Status:** ⏸️ **PENDING** - Awaiting user decision (Option 1, 2, 3, or 4)

**Investigation Date:** 2026-01-12

**Summary:**
- Complete mocking/proxy framework (48 files, ~1,578 LOC)
- Runtime interface proxies using DispatchProxy
- Method call recording and binding
- **Does NOT exist in main OoBDev codebase** (completely new)
- GitHub: https://github.com/OutOfBandDevelopment/oobtainium/

**Current State:**
- .NET Standard 2.1 (needs upgrade to .NET 9.0)
- Microsoft.Extensions.* 3.1.9 (2020 - outdated)
- Good architecture: Abstractions + Implementation + Tests
- Simpler than Moq but less feature-rich

**Decision Options:**
1. **MIGRATE** to main framework (MEDIUM effort, HIGH maintenance)
2. **REFERENCE** as external NuGet package (LOW effort, MINIMAL maintenance)
3. **ARCHIVE** in Incomming/ (MINIMAL effort, ZERO maintenance)
4. **DELETE** ⭐ RECOMMENDED (MINIMAL effort, ZERO maintenance)

**Recommendation:** Option 4 (Delete)
- Mocking well-solved by Moq (460M+ downloads) and NSubstitute (130M+)
- Focus OoBDev resources on unique features (binary processing, protocols, hardware)

**Next Steps:**
- [ ] **USER DECISION REQUIRED:** Choose Option 1, 2, 3, or 4
- [ ] Execute chosen option per migration plan

**Documentation:**
- [Feature Mapping](../docs/migration/oobtainium-feature-mapping.md)
- [Migration Plan](../docs/migration/oobtainium-migration-plan.md) - All 4 options detailed

**Migration Complexity:** MEDIUM (if migrating), MINIMAL (if deleting/archiving)

---

### 🔍 BotChat (INVESTIGATION COMPLETE)

**Status:** 🔍 **INVESTIGATED** - Awaiting migration decision

**Investigation Date:** 2026-01-13

**Summary:**
- Console application (Exe) for chatting with local LLMs via Ollama
- 12 C# files (~393 LOC)
- Already .NET 9.0
- Uses SemanticKernel 1.32.0 and Ollama connector

**Classification:** **SAMPLE/DEMO APPLICATION**

**Architecture:**
- `Program.cs` - Entry point
- `HostRunner/` - Generic hosted service pattern for background tasks
  - `RunnerHost<T>` - Generic runner host
  - `KernelRunner` - Interactive CLI chat loop
  - `IRunner` - Runner interface
- `KernelHost/` - Kernel plugin infrastructure
  - `IKernelPlugIn` - Plugin registration interface
- `Ollama/` - Ollama integration factories (some overlap with main OoBDev.Ollama)

**Relationship to Main Codebase:**
- Main has `src/ExternalServices/Ollama/OoBDev.Ollama/` - production library
- BotChat appears to be earlier prototype/sample
- Some concepts refined into production OoBDev.Ollama
- BotChat demonstrates **how to use** the Ollama library

**Unique Features (NOT in OoBDev.Ollama):**
1. Interactive CLI chat loop with `/done` and `/exit` commands
2. Generic `RunnerHost<T>` pattern for hosted services
3. Kernel plugin registration system (`IKernelPlugIn`)
4. Authorization header support (API key handling)
5. Reference implementation for building Ollama chat applications

**Features in OoBDev.Ollama (NOT in BotChat):**
1. Health check support (`OllamaHealthCheck`)
2. Comprehensive API extensions (embedding retrieval, model validation)
3. Model mapping infrastructure
4. Multiple provider registrations (IMessageCompletion, IEmbeddingProvider, IChatProvider)
5. XML documentation
6. Framework abstraction layers

**Decision Options:**
1. **ARCHIVE** as sample/reference application (RECOMMENDED)
2. **MIGRATE** useful patterns (RunnerHost, IKernelPlugIn) to Examples or Tools
3. **ENHANCE** and keep as official CLI demo tool
4. **DELETE** if no longer relevant

**Recommendation:** Option 1 (Archive) or Option 3 (Enhance as official demo)
- Valuable as learning resource and reference implementation
- Shows developers how to build Ollama applications
- Could be polished into official OoBDev.Ollama.Cli example

**Next Steps:**
- [ ] **DECISION REQUIRED:** Choose migration approach
- [ ] If enhancing: Update dependencies, improve UX, add to Tools/
- [ ] If archiving: Add comprehensive README explaining usage

**Documentation:**
- [Feature Mapping](../docs/migration/botchat-feature-mapping.md)
- [Migration Plan](../docs/migration/botchat-migration-plan.md)

**Migration Complexity:** LOW (if archiving), MEDIUM (if enhancing)

---

### 🔍 BinaryDecoders (INVESTIGATION COMPLETE)

**Status:** 🔍 **INVESTIGATED** - Blocked pending critical questions

**Investigation Date:** 2026-01-10 - 2026-01-11

**Summary:**
- Massive codebase migration from legacy BinaryDataDecoders project
- ~500 files, ~50,000 LOC
- Comprehensive binary data processing, protocols, hardware, UI libraries
- 5-phase migration plan created

**Scope:**
- **ALL features will be migrated** - phases indicate priority, not selection
- Even niche/specialized features will be maintained
- Incomplete features will be tracked for future completion

**Critical Blockers:**
- 18 question areas requiring user decisions before migration can proceed
- Decisions organized into Phase 1-5 priority groups
- Can start Phase 1 with recommended defaults while later phases decided

**Documentation:**
- [Feature Mapping](../docs/migration/binarydatadecoders-feature-mapping.md)
- [Migration Plan](../docs/migration/binarydatadecoders-migration-plan.md)
- [Critical Questions](../docs/migration/binarydatadecoders-critical-questions.md) ⭐ **Start here**

**Next Steps:**
- [ ] Answer Phase 1 critical questions (Endianness API, BinaryPrimitives, UI Collections)
- [ ] Answer Phase 2-5 questions as migration progresses
- [ ] Begin Phase 1 migration with approved approach

**Migration Complexity:** VERY HIGH

**Priority:** HIGH - Contains unique OoBDev differentiating features

---

### ⏱️ CloudOrchestrator (NOT YET INVESTIGATED)

**Status:** ⏱️ **QUEUED**

**Next Steps:**
- [ ] Investigate directory structure
- [ ] Count files and LOC
- [ ] Determine project type and purpose
- [ ] Compare with main codebase
- [ ] Create feature mapping
- [ ] Create migration plan

---

### ⏱️ ContractParser (NOT YET INVESTIGATED)

**Status:** ⏱️ **QUEUED**

**Next Steps:**
- [ ] Investigate directory structure
- [ ] Count files and LOC
- [ ] Determine project type and purpose
- [ ] Compare with main codebase
- [ ] Create feature mapping
- [ ] Create migration plan

---

### 🔍 SharedFramework (INVESTIGATION COMPLETE)

**Status:** 🔍 **INVESTIGATED** - Ready for systematic migration

**Investigation Date:** 2026-01-13

**Summary:**
- Classification: **Production Framework Library** (52 projects)
- 34 implementation + 18 test projects
- ~28,582 LOC total
- Migrated from production application
- Contains critical external service integrations

**Comparison Results:**
- **24 projects (71%)** - NEW capabilities not in main
- **10 projects (29%)** - ENHANCED versions (DIFFERS)
- **0 projects (0%)** - IDENTICAL to main

**Major Categories:**
1. **Message Queueing** (4 projects, ~600 LOC) - AWS SQS, Azure Service Bus - NEW
2. **Communications** (6 projects, ~2,500 LOC) - SendGrid, Twilio SMS, Enhanced orchestration - CRITICAL MERGE
3. **Spatial Services** (7 projects, ~1,100 LOC) - Census, Google Maps, Bing Maps geocoding - NEW
4. **Complex Events** (5 projects, ~1,400 LOC) - Event sourcing, CQRS, Azure EventHub - NEW
5. **Data Loading** (4 projects, ~2,300 LOC) - CSV/JSON ETL + CLI tool - NEW
6. **Code Generation** (4 projects, ~1,800 LOC) - Test data generation - NEW
7. **Document Management** (3 projects, ~1,300 LOC) - Enhanced document center - MERGE
8. **Distributed Caching** (7 projects, ~600 LOC) - Redis, attribute-based caching - NEW
9. **Identity/Session** (3 projects, ~500 LOC) - Enhanced identity, user sessions - MERGE
10. **Text Templating** (3 projects, ~550 LOC) - Unified templating engine - MERGE
11. **Accounting** (1 project, ~170 LOC) - Domain-specific contracts - REVIEW

**Key Findings:**
- **NOT a duplicate framework** - Production-proven external service integrations
- **Fills critical gaps** - Main has abstractions, SF has implementations
- **CRITICAL MERGES needed:**
  - OoBDev.Communications: SF is 71x larger than main's stub (1,145 LOC vs 16 LOC)
  - OoBDev.Communications.Contracts: SF is 5.5x larger (695 LOC vs 125 LOC)
  - OoBDev.DocumentCenter: SF is 71% larger (911 LOC vs 531 LOC)
  - OoBDev.IdentityModel: SF is 2.3x larger (291 LOC vs 125 LOC)

**New Capabilities:**
- AWS SQS and Azure Service Bus message queues
- Complete geocoding suite (Census, Google Maps, Bing Maps)
- Event sourcing and CQRS architecture
- CSV/JSON to database ETL pipeline with CLI
- Attribute-driven test data generation (1,256 LOC)
- Distributed caching with Redis
- SendGrid email and Twilio SMS providers

**Migration Complexity:** HIGH - 52 projects requiring careful integration

**Next Steps:**
- [ ] Upgrade all 52 projects to .NET 9.0 (currently .NET 8.0)
- [ ] Execute Phase 1: Message Queueing (Amazon SQS, Azure Service Bus)
- [ ] Execute Phase 2: Communications MERGE (CRITICAL)
- [ ] Execute Phases 3-11 per priority
- [ ] Systematic integration with 80%+ test coverage

**Documentation:**
- [Feature Mapping](../docs/migration/sharedframework-feature-mapping.md) - Complete 52-project analysis
- [Migration Plan](../docs/migration/sharedframework-migration-plan.md) - 12-phase execution plan

**Migration Complexity:** HIGH - Major framework enhancement

**Priority:** HIGH - Critical external service integrations and framework completions

---

### ⏱️ Tools (NOT YET INVESTIGATED)

**Status:** ⏱️ **QUEUED**

**Next Steps:**
- [ ] List all tools in directory
- [ ] Investigate each tool's purpose
- [ ] Determine which tools are CLI utilities vs libraries
- [ ] Compare with main codebase Tools/
- [ ] Create feature mapping for each tool
- [ ] Create migration/integration plan

**Notes:**
- Grep results showed `OobDev.BulkLlm.Ollama.Cli` exists in Incomming/Tools/
- May contain multiple independent tool projects

---

## Summary Statistics

| Metric | Count |
|--------|-------|
| **Total Projects** | 9 |
| **Completed** | 1 (dotnet-lib - deleted) |
| **Investigated** | 5 (Framework, Oobtainium, BotChat, BinaryDecoders, SharedFramework) |
| **Pending Decision** | 2 (Oobtainium, BotChat) |
| **Blocked** | 1 (BinaryDecoders - awaiting answers) |
| **Ready for Migration** | 2 (Framework Phase 1 - Vector Math, SharedFramework) |
| **Not Yet Started** | 3 (CloudOrchestrator, ContractParser, Tools) |

---

## Processing Order Recommendation

Based on investigation results and priority:

### Phase A: Complete Current Investigations (HIGH PRIORITY)
1. ✅ **Framework Phase 0** - Systematic comparison of 25 DIFFERS files
2. ✅ **Framework Phase 1** - Migrate Vector Math Library (ready to execute)
3. ⚠️ **Oobtainium Decision** - Choose Option 1-4
4. ⚠️ **BotChat Decision** - Archive or enhance as demo

### Phase B: Start New Investigations (MEDIUM PRIORITY)
5. **Tools** - Investigate tools directory
6. **CloudOrchestrator** - Investigate cloud-related features
7. **ContractParser** - Investigate parsing capabilities

### Phase C: Execute Major Migrations (ONGOING)
8. **SharedFramework** - Begin Phase 0-1 migration (52 projects, high priority)
9. **BinaryDecoders** - Begin Phase 1 migration once critical questions answered
10. **Framework Phases 2-5** - Complete remaining framework migrations

---

## Migration Progress Tracking

### Overall Progress

```
Total Projects: 9
├─ Deleted: 1 (11%)  ████
├─ Migrated: 0 (0%)
├─ Decided: 0 (0%)
├─ Investigating: 5 (56%)  ████████████████████████
├─ Queued: 3 (33%)  ███████████████
└─ Remaining Work: 89%  ████████████████████████████████████████
```

### Investigation Progress

```
Investigation Phase Complete: 6/9 (67%)
██████████████████████████████████░░░░░░░░░░░░░░
```

### Migration Execution Progress

```
Execution Phase Complete: 1/9 (11%)
█████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
```

---

## Key Milestones

- [x] 2026-01-12: Oobtainium investigation complete
- [x] 2026-01-13: Vector comparison complete (Framework subset)
- [x] 2026-01-13: BotChat investigation complete
- [x] 2026-01-13: SharedFramework investigation complete (52 projects)
- [ ] TBD: Oobtainium decision made
- [ ] TBD: BotChat decision made
- [ ] TBD: BinaryDecoders critical questions answered
- [ ] TBD: All Incomming projects investigated
- [ ] TBD: All Incomming projects migrated or decided

---

## Notes

- This checklist should be updated after each investigation or migration
- Link to detailed documentation for each project
- Track blockers and decision points
- Maintain priority ordering for efficient resource allocation

---

## Change Log

- 2026-01-13: Initial checklist created
- 2026-01-13: Added dotnet-lib (DELETED), Framework, Oobtainium, BotChat, BinaryDecoders status
- 2026-01-13: Identified 4 remaining projects to investigate
