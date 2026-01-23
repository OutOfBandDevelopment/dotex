# TODO - Decisions Required Epic

**Last Updated:** 2026-01-20

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

## OoBDev.Oobtainium - Migration Decision ✅ RESOLVED

**Status:** ✅ **RESOLVED** - Moved to proving-grounds repository (2026-01-20)

**Decision Made:** Moved to separate code playground repository

**What was Oobtainium?**
- Complete mocking/proxy framework (48 files, ~1,578 LOC)
- Runtime interface proxies using DispatchProxy
- Method call recording and binding
- Does NOT exist in main OoBDev codebase (completely new)
- Original GitHub: https://github.com/OutOfBandDevelopment/oobtainium/

**Resolution:**
- **New Location:** https://github.com/mwwhited/proving-grounds
- **Purpose:** Code playground and examples repository
- **Rationale:**
  - Mocking is well-solved by existing tools (Moq: 460M+ downloads, NSubstitute: 130M+)
  - Allows OoBDev to focus on unique capabilities (binary processing, protocols, hardware)
  - Project remains available for experimentation and reference
  - Better resource allocation to BinaryDataDecoders and SharedFramework migrations

**Actions Completed:**
- [x] User moved Oobtainium to proving-grounds repository
- [x] Updated CHECKLIST.md status to COMPLETE
- [x] Updated TODO-decisions.md to reflect resolution
- [x] Project remains accessible for future reference

**Documentation:**
- [Feature Mapping](docs/migration/oobtainium-feature-mapping.md) - Complete feature analysis
- [Migration Plan](docs/migration/oobtainium-migration-plan.md) - All 4 options documented (for reference)

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
