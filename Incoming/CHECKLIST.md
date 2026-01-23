# Incomming Directory - Processing Checklist

**Last Updated:** 2026-01-20

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
| **dotnet-lib** | ✅ **COMPLETE** | Framework subset | N/A | N/A | N/A | Completed and deleted (95% identical to main) |
| **Framework** | 🗑️ **CANCELLED** | Framework subset | 55 | N/A | N/A | Directory removed (Vector library already in main) |
| **Oobtainium** | ✅ **COMPLETE** | Mocking Framework | 48 | ~1,578 | LOW | Moved to proving-grounds repository |
| **BotChat** | 🔍 **INVESTIGATED** | Sample Application | 12 | ~393 | LOW | Migration decision needed |
| **SharedFramework** | 🎨 **REPLACED** | Framework Library | 52 | ~28,582 | HIGH | Code migration replaced with design documentation (120 docs, 90.9% complete) |
| **BinaryDecoders** | 🔍 **INVESTIGATED** | Massive Codebase | ~500 | ~50,000 | HIGH | Critical questions answered |
| **ContractParser** | 🔍 **INVESTIGATED** | DSL/Grammar Spec | 5 | ~475 | MEDIUM | Feature documentation complete |
| **Tools** | 🔍 **INVESTIGATED** | CLI Tools (4) | 18 | ~474 | MIXED | Tool-specific recommendations |

---

## Detailed Status

### ✅ dotnet-lib (COMPLETE)

**Status:** ✅ **COMPLETE** - Completed and deleted (2026-01-12)

**Investigation Date:** 2026-01-12
**Resolution Date:** 2026-01-12

**Summary:**
- 95% identical to main OoBDev codebase
- Critical bug fix extracted and applied to main
- Deleted from Incoming/ after successful extraction

**Decision Made:** Completed and deleted
- Bug fix applied to main codebase
- No other unique features found
- Directory removed after extraction

**Documentation:**
- Bug fix documented in main codebase

---

### 🗑️ Framework (CANCELLED)

**Status:** 🗑️ **CANCELLED** - Directory removed from Incoming/

**Investigation Date:** 2026-01-13
**Resolution Date:** 2026-01-22

**Summary:**
- 55 files analyzed (30 NEW + 25 DIFFERS)
- Vector library already integrated in main codebase at `src/Framework/OoBDev.System.Abstractions/Math/`
- Vector namespace updated from `OoBDev.Common.Math` → `OoBDev.System.Math` (5 files)
- Other Framework files not migrated

**Decision Made:** Cancelled
- Vector library already in main codebase (only unique valuable component)
- Directory removed from Incoming/
- No additional migration needed

**Documentation:**
- [Feature Mapping](../docs/migration/framework-feature-mapping.md) - Analysis of what was in Framework
- [Vector Comparison](../docs/migration/vector-comparison.md) - Detailed vector implementation analysis

---

### ✅ OoBDev.Oobtainium (COMPLETE)

**Status:** ✅ **COMPLETE** - Moved to separate repository (2026-01-20)

**Investigation Date:** 2026-01-12
**Resolution Date:** 2026-01-20

**Summary:**
- Complete mocking/proxy framework (48 files, ~1,578 LOC)
- Runtime interface proxies using DispatchProxy
- Method call recording and binding
- **Does NOT exist in main OoBDev codebase** (completely new)
- Original GitHub: https://github.com/OutOfBandDevelopment/oobtainium/

**Decision Made:** Moved to proving-grounds repository
- New location: https://github.com/mwwhited/proving-grounds
- Purpose: Code playground and examples repository
- Rationale: Mocking well-solved by existing tools (Moq, NSubstitute)
- Allows OoBDev to focus on unique capabilities (binary processing, protocols, hardware)

**Action Taken:**
- [x] User moved Oobtainium to proving-grounds repository
- [x] Updated CHECKLIST.md status
- [x] Project remains available for reference and experimentation

**Documentation:**
- [Feature Mapping](../docs/migration/oobtainium-feature-mapping.md)
- [Migration Plan](../docs/migration/oobtainium-migration-plan.md) - All 4 options detailed

**Migration Complexity:** N/A (moved to separate repository)

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

### 🔍 ContractParser (INVESTIGATION COMPLETE)

**Status:** 🔍 **INVESTIGATED** - Feature specification complete

**Investigation Date:** 2026-01-20

**Summary:**
- ANTLR4-based DSL for service contract specifications
- 5 grammar/example files (~475 LOC total)
- NO implementation code (specification only)
- Grammar versions: v1 and v2 (v2 is current)
- 5 real-world service contract examples from automotive/financial domain

**Classification:** **FEATURE SPECIFICATION / DSL GRAMMAR**

**Components:**
- `ServiceContract.g4` - ANTLR4 grammar v1 (original)
- `ServiceContract_v2.g4` - ANTLR4 grammar v2 (enhanced)
- 5 example .ServiceContract files (225-15 LOC each)

**Purpose:**
- DSL for describing service contracts (APIs, DTOs, enums)
- Contract-first development approach
- Code generation from contracts (C#, TypeScript, documentation)
- Single source of truth for service specifications

**Grammar Features (v2):**
- Service definitions with `@service` decorator
- Operations: `+` (read) and `*` (write) prefix
- Parameters: `>` (input), `<` (output)
- DTO definitions with `@dto` decorator
- Enum definitions with `@enum` decorator and numeric values
- Type system with generic support (`List<T>`)
- Documentation comments and links

**Relationship to Main Codebase:**
- NO overlap - completely new capability
- Similar ANTLR4 usage patterns exist in:
  - `src/Framework/OoBDev.ExpressionCalculator/` - Expression parsing
  - `src/Framework/OoBDev.Extensions.JsonPath/` - JSON path queries

**Proposed Use Cases:**
1. API documentation generation (markdown/HTML from contracts)
2. DTO code generation (C# classes with attributes)
3. Service interface generation (C# interfaces for operations)
4. Client SDK generation (TypeScript/JavaScript proxies)
5. Contract validation & testing (verify implementations match contracts)

**Decision Options:**
1. **Specification Only** - Keep as reference, implement later (CURRENT STATUS)
2. **Full Implementation** - Build parser + code generators (~230 hours)
3. **Extract Patterns** - Document requirements, implement when prioritized

**Recommendation:** Option 1 (Specification Only) - Captured in Features/ContractParser/README.md

**Documentation Created:**
- `Features/ContractParser/README.md` - Comprehensive feature specification
  - 5 use cases with user journeys
  - Functional and non-functional requirements
  - Technical specifications (grammar features, limitations, enhancements)
  - Proposed architecture (OoBDev.Contracts + CodeGen)
  - 3 decision points (timing, grammar enhancement, generation approach)
  - Migration complexity estimate: 230 hours (~6 weeks)

**Migration Complexity:** MEDIUM-HIGH (if implementing)

**Priority:** MEDIUM - Valuable for contract-first development, but not critical

**Next Steps:**
- [ ] **DECISION:** Implement now, later, or archive as specification
- [ ] If implementing: Prioritize grammar enhancements (nullable types, defaults, attributes)
- [ ] If implementing: Choose code generation approach (template-based vs Roslyn)

---

### 🔍 Tools (INVESTIGATION COMPLETE)

**Status:** 🔍 **INVESTIGATED** - Tool-specific analysis complete

**Investigation Date:** 2026-01-20

**Summary:**
- 4 standalone CLI applications
- 18 total files (~474 LOC total)
- Mixed purposes: hardware, utilities, AI/LLM code generation
- 2 tools are 95% duplicate (BulkLlm variants)

**Classification:** **CLI UTILITIES COLLECTION**

**Tools Found:**

**1. OoBDev.De5000.Ble.Cli** (75 LOC, 4 files)
- Purpose: Bluetooth LE device scanner for DE-5000 LCR meter
- Status: Incomplete/Early-stage
- Framework: net8.0-windows10.0.19041.0
- Dependencies: InTheHand.BluetoothLE v4.0.37
- **Priority:** LOW - Specialized hardware, incomplete
- **Recommendation:** Archive as reference

**2. OoBDev.ImageConverter.Cli** (32 LOC, 2 files)
- Purpose: Batch converts HEIC/NEF images to JPEG
- Status: Basic but functional
- Framework: net10.0
- Dependencies: Magick.NET-Q16-x64 v14.10.1
- Limitations: Hard-coded paths, no CLI args, no config
- **Priority:** LOW - Overlaps with DocumentConverter.Cli
- **Recommendation:** Archive as reference

**3. OobDev.BulkLlm.GroqNet.Cli** (189 LOC, 5 files)
- Purpose: Cloud LLM code documentation generator (uses Groq API)
- Status: Functional for API interaction, file generation disabled
- Framework: net8.0
- Dependencies: GroqNet v1.0.1, Handlebars.Net v2.1.6
- Templates: GenerateDocumentationForThisCode.md.hbs
- Limitations: Disabled file extraction (lines 83-89 commented), hard-coded paths
- **Priority:** MEDIUM-HIGH - Valuable developer tool
- **Recommendation:** Consolidate with Ollama variant

**4. OobDev.BulkLlm.Ollama.Cli** (178 LOC, 7 files)
- Purpose: Local LLM code documentation generator (uses local Ollama)
- Status: Functional for local integration, file generation disabled
- Framework: net8.0
- Dependencies: OllamaSharp v2.0.10, Handlebars.Net v2.1.6
- Templates: 3 templates (documentation, unit tests, Angular→React)
- Limitations: Disabled file extraction (lines 72-78 commented), hard-coded endpoint
- **Priority:** MEDIUM-HIGH - Valuable for offline/local LLM usage
- **Recommendation:** Consolidate with GroqNet variant

**Code Duplication Analysis:**
- BulkLlm.GroqNet and BulkLlm.Ollama are **95% identical**
- Only difference: LLM provider (GroqNet vs OllamaSharp)
- Both use Handlebars templating
- Both have disabled file generation
- Both need configuration support

**Proposed Consolidation:**
- Create `ILlmProvider` abstraction
- Implement `GroqLlmProvider` and `OllamaLlmProvider`
- Create unified `OoBDev.LlmCodeGen.Cli` tool
- Add configuration support (appsettings.json + CLI args)
- Complete file extraction feature
- **Effort estimate:** 60 hours (~1.5 weeks)

**Documentation Created:**
- `Features/Tools/README.md` - Comprehensive tool analysis
  - Overview of all 4 tools with use cases
  - Requirements and dependencies
  - Migration recommendations per tool
  - BulkLlm consolidation plan with ILlmProvider abstraction
  - Implementation tasks and effort estimates

**Migration Complexity:**
- De5000.Ble.Cli: LOW (archive)
- ImageConverter.Cli: LOW (archive)
- BulkLlm tools: MEDIUM (consolidation effort)

**Priority:** MIXED - Low for hardware/image tools, Medium-High for LLM tools

**Next Steps:**
- [ ] **Archive:** De5000.Ble.Cli and ImageConverter.Cli (keep as reference)
- [ ] **Consolidate:** BulkLlm tools into unified OoBDev.LlmCodeGen solution
- [ ] Create ILlmProvider abstraction layer
- [ ] Complete file extraction feature (currently disabled)
- [ ] Add configuration management (remove hard-coded values)
- [ ] **When migrating:** Review existing CLI tool patterns and refactor to match

---

### 🎨 SharedFramework (REPLACED WITH DESIGN DOCUMENTATION)

**Status:** 🎨 **REPLACED** - Code migration replaced with comprehensive design documentation

**Investigation Date:** 2026-01-13
**Strategic Pivot Date:** 2026-01-22
**Resolution Date:** 2026-01-23

**Strategic Change:**
Instead of migrating 52 projects of SharedFramework code (~28,582 LOC), we created comprehensive design documentation following the Epic 11 pattern. This ensures clean architecture from first principles without inheriting technical debt from legacy code.

**Design Documentation Created:**
- **120 of 132 documents created (90.9% complete)**
- **Epic 11:** Data Enhancement Pipeline (16 docs) ✅ COMPLETE
- **Epic 10:** Text Templating Extensions (12 docs) ✅ COMPLETE
- **Epic 12:** Message Composition Service (4 docs) ✅ COMPLETE
- **Epic 7:** Identity & Session Management (16 docs) ✅ COMPLETE
- **Epic 6:** Document Services (44 docs) ✅ COMPLETE
- **Epic 4:** Distributed Caching (12 docs) ✅ COMPLETE
- **Epic 2:** Communications Platform (6 docs) 🔄 PARTIAL (6/16)
- **Epic 5:** Master Data Management (10 docs) 🔄 PARTIAL (10/12)

**Completed Migrations (Before Pivot):**
- ✅ Caching Framework (4 impl + 3 test projects) - [Details](../docs/changes/migration-caching-framework-2026-01-20.md)
- ✅ Message Queues (AWS SQS + Azure Service Bus) - [Details](../docs/changes/migration-message-queues-2026-01-20.md)

**Decision Made:** Design-first approach
- Directory removed from Incoming/
- Comprehensive design documents created instead
- Modern .NET 10.0 patterns throughout
- Clean architecture from scratch
- No technical debt migration
- Implementation will follow approved designs

**Why Design-First?**
- Avoids technical debt from legacy SharedFramework code
- Ensures modern .NET 10.0 patterns throughout
- Integrates cleanly with Epic 11 features (IDataContainer, schema discovery, path translation)
- Enables comprehensive test planning upfront (85-90% coverage targets)
- Documents intent before implementation

**Next Steps:**
- [ ] Complete remaining 12 design documents (Epic 2: 10 docs, Epic 5: 2 docs)
- [ ] Review and approve design documentation
- [ ] Begin implementation based on approved designs

**Documentation:**
- [Design Progress](../Features/Proposals/DOCUMENTATION_PROGRESS.md) - Documentation status
- [Feature Mapping](../docs/migration/sharedframework-feature-mapping.md) - Original 52-project analysis (archived)
- [Migration Plan](../docs/migration/sharedframework-migration-plan.md) - Original 12-phase plan (archived)

---

---

## Summary Statistics

| Metric | Count |
|--------|-------|
| **Total Projects** | 8 |
| **Completed** | 2 (dotnet-lib - deleted, Oobtainium - moved) |
| **Cancelled** | 1 (Framework - directory removed) |
| **Replaced with Design Docs** | 1 (SharedFramework - 120 docs created) |
| **Investigated** | 4 (BotChat, BinaryDecoders, ContractParser, Tools) |
| **Pending Decision** | 4 (BotChat, ContractParser, Tools, BinaryDecoders) |
| **Blocked** | 1 (BinaryDecoders - awaiting critical decisions) |
| **Feature Documentation Complete** | 2 (ContractParser, Tools) |

---

## Processing Order Recommendation

Based on investigation results and priority:

### Phase A: Completed
1. ✅ **dotnet-lib** - Completed and deleted (2026-01-12)
2. ✅ **Framework** - Cancelled, directory removed (2026-01-22)
3. ✅ **Oobtainium** - Moved to proving-grounds repository (2026-01-20)
4. ✅ **SharedFramework** - Replaced with design documentation (2026-01-22)

### Phase B: Decisions Required (HIGH PRIORITY)
1. ⚠️ **BotChat Decision** - Archive or enhance as demo
2. ⚠️ **BinaryDecoders Questions** - Answer critical questions before Phase 1 migration
3. ⚠️ **ContractParser** - Implement now, later, or keep as specification
4. ⚠️ **Tools (BulkLlm)** - Consolidate into unified LlmCodeGen tool

### Phase C: Complete Remaining Design Docs
1. 🎨 **Epic 2: Communications** - 10 documents remaining
2. 🎨 **Epic 5: Master Data** - 2 documents remaining

### Phase D: Execute Major Migrations (FUTURE)
1. **BinaryDecoders** - Begin Phase 1 migration once questions answered
2. **SharedFramework Features** - Implement based on approved design documentation

---

## Migration Progress Tracking

### Overall Progress

```
Total Projects: 8
├─ Completed: 2 (25%)  ████████████
├─ Cancelled: 1 (12%)  ██████
├─ Replaced with Design Docs: 1 (12%)  ██████
├─ Pending Decisions: 4 (50%)  █████████████████████████
└─ Remaining Work: 50%  █████████████████████████
```

### Investigation Progress

```
Investigation Phase Complete: 8/8 (100%)
████████████████████████████████████████████████
```

### Migration Execution Progress

```
Projects Resolved: 4/8 (50%)
- dotnet-lib: Completed and deleted
- Framework: Cancelled
- Oobtainium: Moved to proving-grounds
- SharedFramework: Replaced with 120 design docs (90.9% complete)

Projects Pending: 4/8 (50%)
- BotChat: Awaiting decision
- BinaryDecoders: Awaiting critical decisions
- ContractParser: Awaiting decision
- Tools: Awaiting decision
```

---

## Key Milestones

- [x] 2026-01-12: dotnet-lib investigation complete
- [x] 2026-01-12: dotnet-lib completed and deleted
- [x] 2026-01-12: Oobtainium investigation complete
- [x] 2026-01-13: Framework investigation complete (Vector library subset)
- [x] 2026-01-13: BotChat investigation complete
- [x] 2026-01-13: SharedFramework investigation complete (52 projects)
- [x] 2026-01-20: Oobtainium moved to proving-grounds repository
- [x] 2026-01-20: ContractParser investigation complete (feature specification)
- [x] 2026-01-20: Tools investigation complete (4 CLI tools analyzed)
- [x] 2026-01-20: All Incoming projects investigated (8/8 = 100%)
- [x] 2026-01-22: SharedFramework strategic pivot - Code migration replaced with design documentation
- [x] 2026-01-22: Framework cancelled - Directory removed from Incoming/
- [x] 2026-01-23: Cleanup complete - All TODO files updated to reflect Framework cancellation and SharedFramework design pivot
- [ ] TBD: BotChat decision made (archive or enhance)
- [ ] TBD: ContractParser decision made (implement now, later, or keep as spec)
- [ ] TBD: Tools BulkLlm consolidation decision
- [ ] TBD: BinaryDecoders critical questions answered
- [ ] TBD: Complete remaining 12 design documents (Epic 2: 10 docs, Epic 5: 2 docs)

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
- 2026-01-20: ContractParser investigation complete - Feature specification in Features/ContractParser/
- 2026-01-20: Tools investigation complete - 4 CLI tools analyzed in Features/Tools/
- 2026-01-20: Removed CloudOrchestrator (doesn't exist in Incoming directory)
- 2026-01-20: Updated Oobtainium status - Moved to proving-grounds repository
- 2026-01-20: All 8 Incoming projects now investigated (100% investigation complete)
- 2026-01-23: **Major Update** - Added Framework and SharedFramework resolution status
  - Framework: CANCELLED (directory removed, only Vector library preserved)
  - SharedFramework: REPLACED with 120 design documents (90.9% complete)
  - Updated all TODO files to reflect strategic pivot from code migration to design-first approach
  - Updated project counts from 6 to 8 total projects
  - Updated Summary Statistics to show 4 projects resolved (50%) and 4 pending decisions (50%)
