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
| **OoBDev.Oobtainium** | ⏸️ **PENDING** | Mocking Framework | 48 | ~1,578 | LOW | Keep for user to move to separate repo |
| **BotChat** | 🔍 **INVESTIGATED** | Sample Application | 12 | ~393 | LOW | Migration decision needed |
| **BinaryDecoders** | 🔍 **INVESTIGATED** | Massive Codebase | ~500 | ~50,000 | HIGH | Critical questions answered |
| **ContractParser** | 🔍 **INVESTIGATED** | DSL/Grammar Spec | 5 | ~475 | MEDIUM | Feature documentation complete |
| **SharedFramework** | 🔍 **INVESTIGATED** | Framework Library | 52 | ~28,582 | HIGH | Systematic migration required |
| **Tools** | 🔍 **INVESTIGATED** | CLI Tools (4) | 18 | ~474 | MIXED | Tool-specific recommendations |

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

---

## Summary Statistics

| Metric | Count |
|--------|-------|
| **Total Projects** | 6 |
| **Completed** | 1 (dotnet-lib - deleted) |
| **Investigated** | 6 (Framework, Oobtainium, BotChat, BinaryDecoders, ContractParser, Tools, SharedFramework) |
| **Pending Decision** | 2 (Oobtainium - keep for user; BotChat - archive/enhance) |
| **Blocked** | 1 (BinaryDecoders - awaiting answers) |
| **Feature Documentation Complete** | 2 (ContractParser, Tools) |
| **Ready for Migration** | 2 (Framework Phase 1 - Vector Math, SharedFramework) |
| **Not Yet Started** | 0 |

---

## Processing Order Recommendation

Based on investigation results and priority:

### Phase A: Decisions Required (HIGH PRIORITY)
1. ⚠️ **Oobtainium** - User will move to separate repository (no action needed)
2. ⚠️ **BotChat Decision** - Archive or enhance as demo
3. ⚠️ **BinaryDecoders Questions** - Answer critical questions before Phase 1 migration
4. ⚠️ **ContractParser** - Implement now, later, or keep as specification
5. ⚠️ **Tools (BulkLlm)** - Consolidate into unified LlmCodeGen tool

### Phase B: Ready to Execute (HIGH PRIORITY)
6. ✅ **Framework Phase 1** - Migrate Vector Math Library (ready to execute)
7. ✅ **SharedFramework Phase 0.2** - Namespace cleanup (remove Api. prefix)
8. ✅ **SharedFramework Phase 1-12** - Begin systematic migration (52 projects)

### Phase C: Execute Major Migrations (ONGOING)
9. **BinaryDecoders** - Begin Phase 1 migration once questions answered
10. **Framework Phases 2-5** - Complete remaining framework migrations

---

## Migration Progress Tracking

### Overall Progress

```
Total Projects: 6
├─ Deleted: 1 (17%)  ████████
├─ Migrated: 0 (0%)
├─ Feature Docs Complete: 2 (33%)  ████████████████
├─ Investigating: 0 (0%)
├─ Pending Decisions: 3 (50%)  ████████████████████████
└─ Remaining Work: 83%  █████████████████████████████████████████
```

### Investigation Progress

```
Investigation Phase Complete: 6/6 (100%)
████████████████████████████████████████████████
```

### Migration Execution Progress

```
Execution Phase Complete: 1/6 (17%)
████████░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░░
```

---

## Key Milestones

- [x] 2026-01-12: Oobtainium investigation complete
- [x] 2026-01-13: Vector comparison complete (Framework subset)
- [x] 2026-01-13: BotChat investigation complete
- [x] 2026-01-13: SharedFramework investigation complete (52 projects)
- [x] 2026-01-20: ContractParser investigation complete (feature specification)
- [x] 2026-01-20: Tools investigation complete (4 CLI tools analyzed)
- [x] 2026-01-20: All Incoming projects investigated (6/6 = 100%)
- [ ] TBD: Oobtainium - User will move to separate repository
- [ ] TBD: BotChat decision made (archive or enhance)
- [ ] TBD: ContractParser decision made (implement now, later, or keep as spec)
- [ ] TBD: Tools BulkLlm consolidation decision
- [ ] TBD: BinaryDecoders critical questions answered
- [ ] TBD: All Incoming projects migrated or decided

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
- 2026-01-20: Updated Oobtainium status - User will move to separate repository
- 2026-01-20: All 6 Incoming projects now investigated (100% investigation complete)
