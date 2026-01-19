# TODO - Decisions Required Epic

**Last Updated:** 2026-01-19

This document tracks all pending decisions that block migration and development work.

> **Parent Document:** [TODO.md](./TODO.md)

---

## Overview

Several migration tasks are blocked awaiting architectural and strategic decisions. This document consolidates all decision points requiring stakeholder input.

---

## BinaryDataDecoders Migration - Decision Points

**Status:** ⏸️ BLOCKED - Awaiting decisions on migration approach

Before proceeding with BinaryDataDecoders migration (Phases 1-5), critical questions need answers. See:
- **[Critical Questions Document](docs/migration/binarydatadecoders-critical-questions.md)** - Complete decision matrix

### Summary of Required Decisions

**Immediate (Phase 1 - Foundation):**
- [ ] **Endianness API design** - Extension methods, static methods, or both?
- [ ] **BinaryPrimitives naming** - `ReadInt32BigEndian()` style preferred?
- [ ] **UI Collections location** - Create `OoBDev.Extensions.UI.Collections`?

**High Priority (Phase 2 - High-Value Features):**
- [ ] **CodeAnalysis use case** - What are you building with Roslyn extensions?
- [ ] **Archive formats** - Which formats needed (TAR, CPIO, ZIP)?
- [ ] **ExpressionCalculator audit** - Is current implementation sufficient?

**Medium Priority (Phase 3 - Protocols):**
- [ ] **NMEA Protocol** - GPS hardware integration or data file parsing?
- [ ] **Drawing/Geometry** - Migrate or use existing libraries (SkiaSharp, ImageSharp)?
- [ ] **Barcode** - Which formats? Use ZXing.Net or custom?

**Lower Priority (Phase 4 - Specialized):**
- [ ] **Hardware devices** - Which of the 8 devices are actively used?
- [ ] **CLI tools** - Which of the 4 tools should be migrated?
- [ ] **ISO 9660, Apple II, Classic Crypto** - Active use cases?
- [ ] **Windows Forms, UWP** - Modernization approach?

**Recommendation:** Can start Phase 1 with recommended defaults while Phase 2-4 decisions are made.

---

## OoBDev.Oobtainium - Migration Decision

**Status:** ⏸️ PENDING DECISION - Choose migration approach

**What is Oobtainium?**
- Complete mocking/proxy framework (48 files, ~1,578 LOC)
- Runtime interface proxies using DispatchProxy
- Method call recording and binding
- Does NOT exist in main OoBDev codebase (new library)
- GitHub: https://github.com/OutOfBandDevelopment/oobtainium/

**Current State:**
- .NET Standard 2.1 (needs upgrade to .NET 9.0)
- Microsoft.Extensions.* 3.1.9 dependencies (2020 - outdated)
- Good architecture: Abstractions + Implementation + Tests
- Simpler than Moq but less feature-rich

**Decision Required - Choose ONE of four options:**

### Option 1: MIGRATE to Main Framework

- **Action:** Upgrade to .NET 9.0, integrate into `src/Framework/OoBDev.Mocking/` or `src/Extensions/OoBDev.Extensions.Mocking/`
- **Effort:** MEDIUM (upgrade dependencies, rename namespaces, add documentation)
- **Maintenance:** HIGH (ongoing .NET updates, feature additions)
- **Pros:** First-party mocking solution, DI-integrated, simpler than Moq for basic scenarios
- **Cons:** Maintenance burden, duplicates existing tools (Moq, NSubstitute)
- **See:** [Migration Plan](docs/migration/oobtainium-migration-plan.md) - Phase 1-5 detailed steps

**Implementation Tasks (if chosen):**

- [ ] **Phase 1: Preparation & Analysis**
  - [ ] Create target project structure: `src/Extensions/OoBDev.Extensions.Mocking/`
  - [ ] Create `OoBDev.Extensions.Mocking.Abstractions/` for interfaces
  - [ ] Create `OoBDev.Extensions.Mocking.Tests/` for unit tests
  - [ ] Decide on namespace: `OoBDev.Mocking` or `OoBDev.Extensions.Mocking`
  - [ ] Review all 48 files for migration requirements

- [ ] **Phase 2: Upgrade & Migrate**
  - [ ] Upgrade all projects to .NET 9.0 (from .NET Standard 2.1)
  - [ ] Upgrade Microsoft.Extensions.* to 9.0.x (from 3.1.9)
  - [ ] Rename namespaces: `OoBDev.Oobtainium` → target namespace
  - [ ] Migrate all 48 source files
  - [ ] Fix any API breaking changes from .NET Standard → .NET 9.0
  - [ ] Enable nullable reference types
  - [ ] Add file-scoped namespaces

- [ ] **Phase 3: Integration**
  - [ ] Add project references to OoBDev.sln
  - [ ] Add ServiceCollection extensions
  - [ ] Add configuration options
  - [ ] Create README.md with usage examples
  - [ ] Add XML documentation to public APIs

- [ ] **Phase 4: Testing**
  - [ ] Migrate all existing tests
  - [ ] Add new tests for .NET 9.0 features
  - [ ] Target 80%+ code coverage
  - [ ] Run `dotnet test` and verify all pass
  - [ ] Create integration test examples

- [ ] **Phase 5: Documentation & Cleanup**
  - [ ] Add to main README.md feature list
  - [ ] Create migration guide for users
  - [ ] Add comparison with Moq/NSubstitute
  - [ ] Update CHANGELOG
  - [ ] Delete `Incomming/OoBDev.Oobtainium/` after successful migration

### Option 2: REFERENCE as External NuGet Package

- **Action:** Verify if published to NuGet, or publish it, then reference where needed
- **Effort:** LOW (add PackageReference)
- **Maintenance:** MINIMAL (external updates)
- **Pros:** No code maintenance, separate evolution
- **Cons:** Dependency on external package, may not be published

**Implementation Tasks (if chosen):**

- [ ] **Verify NuGet Package Availability**
  - [ ] Search NuGet.org for "OoBDev.Oobtainium"
  - [ ] Check GitHub releases: https://github.com/OutOfBandDevelopment/oobtainium/releases
  - [ ] If not published, decide whether to publish it

- [ ] **Option 2A: Package Exists on NuGet**
  - [ ] Add PackageReference to projects that need mocking
  - [ ] Document which package version to use
  - [ ] Add usage examples in docs
  - [ ] Delete `Incomming/OoBDev.Oobtainium/` (using external package)

- [ ] **Option 2B: Publish to NuGet (if needed)**
  - [ ] Create NuGet package from Incomming/OoBDev.Oobtainium
  - [ ] Set package metadata (authors, description, license)
  - [ ] Publish to NuGet.org or internal feed
  - [ ] Add PackageReference to consuming projects
  - [ ] Delete `Incomming/OoBDev.Oobtainium/` after publishing

- [ ] **Documentation**
  - [ ] Document external dependency
  - [ ] Add to README.md under "External Dependencies"
  - [ ] Create examples of usage

### Option 3: ARCHIVE in Incomming/

- **Action:** Create README.md documenting decision, keep for reference
- **Effort:** MINIMAL (documentation only)
- **Maintenance:** NONE
- **Pros:** Available for future reconsideration, no commitment
- **Cons:** Not integrated, users won't discover it

**Implementation Tasks (if chosen):**

- [ ] **Create Archive Documentation**
  - [ ] Create `Incomming/OoBDev.Oobtainium/README.md`
  - [ ] Document decision to archive
  - [ ] Explain what Oobtainium is and does
  - [ ] List reasons for not migrating
  - [ ] Provide alternatives (Moq, NSubstitute)
  - [ ] Add note that it can be reconsidered in future

- [ ] **Update Main Documentation**
  - [ ] Add note to main TODO.md about archived decision
  - [ ] Document location for future reference
  - [ ] Update migration docs with archive status

- [ ] **No Further Action Required**
  - Code remains in `Incomming/OoBDev.Oobtainium/`
  - Available for future reconsideration
  - Zero maintenance burden

### Option 4: DELETE ⭐ RECOMMENDED

- **Action:** Remove `/current/src/Incomming/OoBDev.Oobtainium` directory
- **Effort:** MINIMAL (rm -rf, update docs)
- **Maintenance:** NONE
- **Pros:** No burden, focus on unique OoBDev features, Moq/NSubstitute already solve this
- **Cons:** Lose lightweight alternative
- **Rationale:**
  - Mocking is well-solved (Moq: 460M+ downloads, NSubstitute: 130M+)
  - OoBDev's value is in unique features (binary processing, protocols, hardware)
  - Limited differentiation vs established frameworks
  - Better resource allocation on BinaryDataDecoders migration

**Implementation Tasks (if chosen):**

- [ ] **Final Review Before Deletion**
  - [ ] Verify no references to Oobtainium in main codebase
  - [ ] Verify no dependencies on Oobtainium in other Incomming/ directories
  - [ ] Confirm decision with stakeholders if needed
  - [ ] Backup if desired: `tar -czf oobtainium-backup-$(date +%Y%m%d).tar.gz Incomming/OoBDev.Oobtainium/`

- [ ] **Delete Directory**
  - [ ] Delete `Incomming/OoBDev.Oobtainium/` directory
  - [ ] Verify deletion: `ls Incomming/` should not show OoBDev.Oobtainium

- [ ] **Update Documentation**
  - [ ] Update TODO.md with deletion status
  - [ ] Update `docs/migration/oobtainium-feature-mapping.md` conclusion
  - [ ] Update `docs/migration/oobtainium-migration-plan.md` with decision
  - [ ] Add entry to CHANGELOG if exists

- [ ] **Rationale Documentation**
  - [ ] Document why deleted (focus on unique OoBDev features)
  - [ ] List alternatives: Moq (460M+ downloads), NSubstitute (130M+)
  - [ ] Confirm resource reallocation to BinaryDataDecoders migration

**Documentation:**
- [Feature Mapping](docs/migration/oobtainium-feature-mapping.md) - Complete feature analysis and comparison
- [Migration Plan](docs/migration/oobtainium-migration-plan.md) - All 4 options with detailed steps

**Next Steps:**
- [ ] **DECISION:** Choose Option 1, 2, 3, or 4
- [ ] Execute chosen option per tasks above

---

## Incomming/BotChat - Migration Decision

**Status:** ⏸️ PENDING DECISION - Choose archival approach

**What is BotChat?**
- Sample/Demo Application (12 C# files, ~393 LOC console application)
- Demonstrates Microsoft SemanticKernel + Ollama integration
- Uses older SemanticKernel version (1.32.0 vs main's 1.40.0-alpha)

**Investigation Results:**
- Interactive chat loop with conversation history
- GenericRunnerHost<T> pattern (reusable background service host)
- API key support for Ollama (missing in main OoBDev.Ollama)
- Fluent configuration with IConfiguration binding

**Decision Required - Choose ONE of three options:**

### Option 1: ARCHIVE with Comprehensive README ⭐ RECOMMENDED

- **Action:** Create comprehensive README documenting the sample, its purpose, and relationship to OoBDev.Ollama
- **Effort:** LOW (documentation only)
- **Pros:** Reference for developers learning SemanticKernel integration, preserves example patterns
- **Cons:** Not a production library, older SemanticKernel version

**Implementation Tasks:**
- [ ] Create `Incomming/BotChat/README.md` with:
  - [ ] Purpose and architecture overview
  - [ ] Relationship to OoBDev.Ollama
  - [ ] SemanticKernel integration patterns demonstrated
  - [ ] GenericRunnerHost<T> pattern explanation
  - [ ] API key configuration example
  - [ ] Note about older SemanticKernel version (1.32.0)
  - [ ] Instructions for running the sample
- [ ] Update Incomming/CHECKLIST.md with archive status
- [ ] Update TODO.md with decision

### Option 2: ENHANCE as Official Demo/Sample Project

- **Action:** Update to latest SemanticKernel, move to `samples/` directory, maintain as official example
- **Effort:** MEDIUM (upgrade, refactor, maintain)
- **Pros:** Official sample application, demonstrates best practices
- **Cons:** Maintenance burden, needs ongoing updates

**Implementation Tasks:**
- [ ] Upgrade SemanticKernel to 1.40.0-alpha (match main)
- [ ] Move to `src/Samples/OoBDev.Samples.Ollama.Chat/`
- [ ] Refactor to use latest OoBDev.Ollama library
- [ ] Add comprehensive comments and documentation
- [ ] Create detailed README with setup instructions
- [ ] Add to solution under Samples folder
- [ ] Test and verify functionality
- [ ] Add to main documentation

### Option 3: EXTRACT Patterns Only

- **Action:** Extract valuable patterns (RunnerHost<T>, API key support) into main codebase, delete sample
- **Effort:** LOW-MEDIUM (extract, integrate, test)
- **Pros:** Adds missing features to OoBDev.Ollama, no sample maintenance
- **Cons:** Lose complete working example

**Implementation Tasks:**
- [ ] Extract GenericRunnerHost<T> pattern to `OoBDev.System` or `OoBDev.Hosting`
- [ ] Add API key support to OoBDev.Ollama
- [ ] Add fluent configuration patterns
- [ ] Test extracted features
- [ ] Delete `Incomming/BotChat/` after extraction
- [ ] Update documentation

**Documentation:**
- [Feature Mapping](docs/migration/botchat-feature-mapping.md) - Complete feature analysis
- [Migration Plan](docs/migration/botchat-migration-plan.md) - Archive/Enhance/Extract options

**Next Steps:**
- [ ] **DECISION:** Choose Option 1, 2, or 3
- [ ] Execute chosen option per tasks above

---

## Decision Template

When making decisions, consider:

1. **Strategic Alignment** - Does this align with OoBDev's core mission (binary processing, protocols, hardware)?
2. **Maintenance Burden** - What is the long-term maintenance cost?
3. **User Value** - How many users will benefit?
4. **Alternatives** - Are there existing solutions that solve this better?
5. **Resource Allocation** - Could resources be better spent elsewhere?

---

## Reference

**Related Documents:**
- [TODO.md](./TODO.md) - Main tracking document
- [TODO-migrations.md](./TODO-migrations.md) - Migration work (blocked by decisions)
- [TODO-bug-fixes.md](./TODO-bug-fixes.md) - Bug fixes and technical debt
- [TODO-testing-infrastructure.md](./TODO-testing-infrastructure.md) - Testing infrastructure

**Migration Documentation:**
- [BinaryDataDecoders Critical Questions](docs/migration/binarydatadecoders-critical-questions.md)
- [Oobtainium Feature Mapping](docs/migration/oobtainium-feature-mapping.md)
- [Oobtainium Migration Plan](docs/migration/oobtainium-migration-plan.md)
- [BotChat Feature Mapping](docs/migration/botchat-feature-mapping.md)
- [BotChat Migration Plan](docs/migration/botchat-migration-plan.md)
