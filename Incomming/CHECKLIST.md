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
| **dotnet-lib** | 🗑️ **DELETED** | Framework Library | 40 | ~8,000 | - | None - Successfully migrated |
| **Framework** | 🔍 **INVESTIGATED** | Framework Library | 55 | ~2,054 | HIGH | Phase 0 systematic comparison |
| **OoBDev.Oobtainium** | ⏸️ **PENDING** | Mocking Framework | 48 | ~1,578 | LOW | Choose Option 1-4 |
| **BotChat** | 🔍 **INVESTIGATED** | Sample Application | 12 | ~393 | LOW | Migration decision needed |
| **BinaryDecoders** | 🔍 **INVESTIGATED** | Massive Codebase | ~500 | ~50,000 | HIGH | Critical questions answered |
| **CloudOrchestrator** | ⏱️ **QUEUED** | ? | ? | ? | ? | Not yet investigated |
| **ContractParser** | ⏱️ **QUEUED** | ? | ? | ? | ? | Not yet investigated |
| **SharedFramework** | ⏱️ **QUEUED** | ? | ? | ? | ? | Not yet investigated |
| **Tools** | ⏱️ **QUEUED** | Tools/CLI | ? | ? | ? | Not yet investigated |

---

## Detailed Status

### ✅ dotnet-lib (COMPLETE & DELETED)

**Status:** 🗑️ **DELETED 2026-01-12**

**Investigation Date:** 2026-01-11
**Deletion Date:** 2026-01-12

**Summary:**
- 40 C# files compared with main codebase
- Result: 38 files (95%) IDENTICAL to main
- 2 files with differences:
  - `AdditionalSwaggerGenEndpointsOptions.cs` - **Critical bug fixed** in main codebase
  - `HealthCheckSwaggerGenEndpointOptions.cs` - Stylistic difference only

**Outcome:**
- Bug fix applied to main codebase
- All valuable information extracted
- Directory deleted successfully

**Documentation:**
- [Feature Mapping](../docs/migration/dotnet-lib-feature-mapping.md)
- [Migration Plan](../docs/migration/dotnet-lib-migration-plan.md)
- [Comparison Matrix](../docs/migration/dotnet-lib-comparison-matrix.md)

---

### 🔍 Framework (INVESTIGATION COMPLETE)

**Status:** 🔍 **INVESTIGATED** - Awaiting Phase 0 execution

**Investigation Date:** 2026-01-12 - 2026-01-13

**Summary:**
- 3 projects: OoBDev.Common.Abstractions, OoBDev.Common, OoBDev.AspNetCore.Extensions
- 55 implementation files (~2,054 LOC)
- Comparison: 0 identical, 25 differs (45%), 30 new (55%)

**Key Findings:**
- Main `OoBDev.Common` is meta-package (aggregator)
- Incomming has actual implementations
- Branding issue: "EriskInternalDocumentFilter.cs" needs renaming
- Vector math library analyzed separately (ready for migration)

**Major Features Discovered:**
1. ✅ Vector Math Library (5 files) - **Analysis complete, ready to migrate**
2. SQL Database Mapper (1 file) - ORM-like stored procedure mapping
3. Audit Logging System (5 files) - Request/response capture
4. User & Application Access (5 files) - Multi-tenancy support
5. Environment Settings (4 files) - Configuration patterns
6. Swagger/OpenAPI Customizations (4 files)
7. HTTP Extensions (2 files)
8. JSON Serializer Wrapper (1 file)
9. Validation Attributes (1 file)

**Next Steps:**
- [ ] Execute Phase 0: Systematic comparison of 25 DIFFERS files
- [ ] Make namespace mapping decisions
- [ ] Execute Phase 1: Migrate Vector Math Library to `OoBDev.System.Math`
- [ ] Execute Phases 2-5 per migration plan

**Documentation:**
- [Feature Mapping](../docs/migration/framework-feature-mapping.md)
- [Vector Comparison](../docs/migration/vector-comparison.md) - Detailed vector analysis

**Migration Complexity:** HIGH

---

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

### ⏱️ SharedFramework (NOT YET INVESTIGATED)

**Status:** ⏱️ **QUEUED**

**Next Steps:**
- [ ] Investigate directory structure
- [ ] Count files and LOC
- [ ] Determine project type and purpose
- [ ] Compare with main codebase (likely significant overlap)
- [ ] Create feature mapping
- [ ] Create migration plan

**Notes:**
- Name suggests this might overlap with Framework or dotnet-lib
- Could be another framework aggregator or older version

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
| **Investigated** | 4 (Framework, Oobtainium, BotChat, BinaryDecoders) |
| **Pending Decision** | 2 (Oobtainium, BotChat) |
| **Blocked** | 1 (BinaryDecoders - awaiting answers) |
| **Ready for Migration** | 1 (Framework Phase 1 - Vector Math) |
| **Not Yet Started** | 4 (CloudOrchestrator, ContractParser, SharedFramework, Tools) |

---

## Processing Order Recommendation

Based on investigation results and priority:

### Phase A: Complete Current Investigations (HIGH PRIORITY)
1. ✅ **Framework Phase 0** - Systematic comparison of 25 DIFFERS files
2. ✅ **Framework Phase 1** - Migrate Vector Math Library (ready to execute)
3. ⚠️ **Oobtainium Decision** - Choose Option 1-4
4. ⚠️ **BotChat Decision** - Archive or enhance as demo

### Phase B: Start New Investigations (MEDIUM PRIORITY)
5. **SharedFramework** - Investigate next (likely framework-related, may inform Framework migration)
6. **Tools** - Investigate tools directory
7. **CloudOrchestrator** - Investigate cloud-related features
8. **ContractParser** - Investigate parsing capabilities

### Phase C: Execute Major Migrations (ONGOING)
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
├─ Investigating: 4 (44%)  ███████████████████
├─ Queued: 4 (44%)  ███████████████████
└─ Remaining Work: 89%  ████████████████████████████████████████
```

### Investigation Progress

```
Investigation Phase Complete: 5/9 (56%)
████████████████████████████░░░░░░░░░░░░░░░░░░░░
```

### Migration Execution Progress

```
Execution Phase Complete: 1/9 (11%)
█████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
```

---

## Key Milestones

- [x] 2026-01-11: dotnet-lib investigation complete
- [x] 2026-01-12: dotnet-lib deleted (95% identical, bug fixed)
- [x] 2026-01-12: Oobtainium investigation complete
- [x] 2026-01-12: Framework investigation complete
- [x] 2026-01-13: Vector comparison complete (Framework subset)
- [x] 2026-01-13: BotChat investigation complete
- [ ] TBD: Oobtainium decision made
- [ ] TBD: BotChat decision made
- [ ] TBD: Framework Phase 0 complete
- [ ] TBD: Vector Math migrated to OoBDev.System.Math
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
