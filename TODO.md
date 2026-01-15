# TODO - OoBDev (dotex) Framework

**Last Updated:** 2026-01-15 (Swashbuckle 10.1.0 breaking changes fixes in progress)

⚠️ **IN PROGRESS:** Swashbuckle 10.1.0 breaking changes in OoBDev.AspNetCore.Mvc (see section below)

> **New to this project?** Read `/CLAUDE.md` first for a complete development guide including architecture, patterns, and migration scope.

---

## Overview

This document tracks architectural work, migration planning, and bug fixes for the OoBDev framework.

### Migration Scope

**ALL features from BinaryDataDecoders will be migrated** - phases indicate priority order, not feature selection.

- Even highly specialized and niche features will be maintained
- Incomplete features will be migrated and tracked here for future completion
- No features will be skipped unless:
  - Stub/placeholder projects with zero implementation
  - Platform-specific with no .NET 9.0 equivalent (e.g., Silverlight-only)
  - Completely obsolete without modern use cases

---

## Completed Work ✓

### Architecture Documentation
- [x] Created `docs/architecture/README.md` - Overview and quick reference
- [x] Created `docs/architecture/architectural-guidelines.md` - 14 core principles
- [x] Created `docs/architecture/architectural-standards.md` - Enforceable standards
- [x] Created `docs/architecture/architectural-patterns.md` - 11 pattern catalog
- [x] Created `docs/architecture/layering-architecture.md` - Layer-by-layer breakdown

### Migration Analysis - BinaryDataDecoders
- [x] Deep comparison of `Incomming/BinaryDecoders` vs `src/`
- [x] Created `docs/migration/binarydatadecoders-feature-mapping.md` - Feature comparison
- [x] Created `docs/migration/binarydatadecoders-migration-plan.md` - 5-phase plan
- [x] Created `docs/migration/binarydatadecoders-critical-questions.md` - Decision points
- [x] Created `docs/migration/README.md` - Migration tracking

### Migration Analysis - dotnet-lib (COMPLETE & DELETED)
- [x] Deep comparison of `Incomming/dotnet-lib` vs `src/` - All 40 files
- [x] Created `docs/migration/dotnet-lib-feature-mapping.md` - Feature analysis
- [x] Created `docs/migration/dotnet-lib-migration-plan.md` - Investigation plan
- [x] Created `docs/migration/dotnet-lib-comparison-matrix.md` - File-by-file comparison
- [x] **Deleted `Incomming/dotnet-lib/`** - All valuable information extracted

### dotnet-lib Investigation Results (COMPLETE)
- [x] **Phase 0: Investigation**
  - [x] Compared all 40 C# files against main codebase
  - [x] Result: 38 files (95%) IDENTICAL, 2 files differ
  - [x] All files exist in main codebase - No migration needed
- [x] **Phase 1: Critical Bug Fix**
  - [x] **AdditionalSwaggerGenEndpointsOptions.cs** - Fixed missing namespace prefix for generic types
    - Location: `src/Framework/OoBDev.AspNetCore.Mvc/SwaggerGen/AdditionalSwaggerGenEndpointsOptions.cs:111-116`
    - Issue: Main codebase missing `{type.Namespace}.` in `ResolveSchemaType` method
    - Impact: Swagger schema IDs for generic types now properly qualified
    - Severity: HIGH - Prevented potential schema naming collisions
  - [x] **HealthCheckSwaggerGenEndpointOptions.cs** - Stylistic difference only, no action needed
- [x] **Phase 2: Documentation**
  - [x] Documented synchronization status (95% identical)
  - [x] Updated all migration documentation with completion status
  - [x] Deleted source directory after extracting all valuable information

**Outcome:** dotnet-lib was fully synchronized with main codebase. Critical bug fix applied. **Directory deleted 2026-01-12.**

### Migration Analysis - OoBDev.Oobtainium (INVESTIGATION COMPLETE)
- [x] Deep analysis of `Incomming/OoBDev.Oobtainium` structure and features
- [x] Created `docs/migration/oobtainium-feature-mapping.md` - Comprehensive feature analysis
- [x] Created `docs/migration/oobtainium-migration-plan.md` - Migration options and plan
- [x] Compared against existing mocking frameworks (Moq, NSubstitute, FakeItEasy)

### OoBDev.Oobtainium Investigation Results (COMPLETE)
- [x] **Discovery:**
  - [x] Oobtainium does NOT exist in main codebase (new library)
  - [x] Complete mocking/proxy framework (48 files, ~1,578 LOC)
  - [x] Uses .NET Standard 2.1 with 3.1.9 dependencies (outdated)
  - [x] GitHub repository: https://github.com/OutOfBandDevelopment/oobtainium/
- [x] **Analysis:**
  - [x] Runtime proxy creation using DispatchProxy
  - [x] Method binding with fluent API
  - [x] Call recording infrastructure
  - [x] Full async/await support
  - [x] DI integration (ServiceCollection)
  - [x] Good architecture (Abstractions + Implementation + Tests)
- [x] **Comparison:**
  - [x] Simpler than Moq but less feature-rich
  - [x] Lightweight (1,578 LOC vs Moq's 20,000+)
  - [x] Limited matchers and verification compared to established frameworks
- [x] **Decision Options:**
  - Option 1: MIGRATE to main framework (requires .NET 9.0 upgrade, moderate effort)
  - Option 2: REFERENCE as external NuGet package (low effort)
  - Option 3: ARCHIVE in Incomming/ (no effort)
  - Option 4: DELETE (minimal effort)

**RECOMMENDATION: Option 4 (Delete)** - Mocking is well-solved by Moq/NSubstitute. OoBDev's value is in unique features (binary processing, protocols, hardware) not mocking tools. Focus resources on differentiating capabilities.

**Outcome:** Investigation complete. **Awaiting decision on which option to execute.**

### Migration Analysis - Incomming/BotChat (INVESTIGATION COMPLETE)
- [x] Deep analysis of `Incomming/BotChat` structure and features
- [x] Created `docs/migration/botchat-feature-mapping.md` - Comprehensive feature analysis
- [x] Created `docs/migration/botchat-migration-plan.md` - Migration options (Archive/Enhance/Extract)
- [x] Compared against existing OoBDev.Ollama library

### Incomming/BotChat Investigation Results (COMPLETE)
- [x] **Discovery:**
  - [x] Classification: **Sample/Demo Application** (not a library)
  - [x] 12 C# files, ~393 LOC console application
  - [x] Demonstrates Microsoft SemanticKernel + Ollama integration
  - [x] Uses older SemanticKernel version (1.32.0 vs main's 1.40.0-alpha)
- [x] **Analysis:**
  - [x] Interactive chat loop with conversation history
  - [x] GenericRunnerHost<T> pattern (reusable background service host)
  - [x] API key support for Ollama (missing in main OoBDev.Ollama)
  - [x] Fluent configuration with IConfiguration binding
- [x] **Comparison:**
  - [x] BotChat is sample/demo, OoBDev.Ollama is production library
  - [x] BotChat demonstrates usage patterns
  - [x] Main codebase has newer SemanticKernel integration
- [x] **Decision Options:**
  - Option 1: ARCHIVE with comprehensive README (recommended)
  - Option 2: ENHANCE as official demo/sample project
  - Option 3: EXTRACT patterns only (RunnerHost<T>, API key support)

**RECOMMENDATION: Option 1 (Archive)** - Create comprehensive README documenting the sample, its purpose, and relationship to OoBDev.Ollama. Keep as reference for developers learning SemanticKernel integration.

**Outcome:** Investigation complete. **Awaiting decision on which option to execute.**

### Migration Analysis - Incomming/SharedFramework (INVESTIGATION COMPLETE)
- [x] Deep analysis of `Incomming/SharedFramework` structure and features (52 projects)
- [x] Created `docs/migration/sharedframework-feature-mapping.md` - Comprehensive 52-project analysis
- [x] Created `docs/migration/sharedframework-migration-plan.md` - 12-phase migration plan
- [x] Compared all projects against main OoBDev codebase

### Incomming/SharedFramework Investigation Results (COMPLETE)
- [x] **Discovery:**
  - [x] Classification: **Production Framework Library** (52 projects total)
  - [x] 34 implementation projects + 18 test projects
  - [x] ~28,582 LOC across all projects
  - [x] 11 major categories of external service integrations
  - [x] Multi-targets .NET 8.0 (requires upgrade to .NET 9.0)
- [x] **Analysis:**
  - [x] **24 projects (71%)** - NEW capabilities not in main codebase (~9,000+ LOC)
  - [x] **10 projects (29%)** - ENHANCED versions of existing frameworks (DIFFERS status)
  - [x] **0 projects (0%)** - IDENTICAL to main codebase
- [x] **Categories:**
  - [x] Message Queueing (4 projects) - AWS SQS, Azure Service Bus
  - [x] Communications (6 projects) - CRITICAL: Main has 16 LOC stub, SF has 1,145 LOC (71x larger!)
  - [x] Spatial Services (7 projects) - Census, Google Maps, Bing Maps geocoding
  - [x] Complex Events (5 projects) - Event sourcing, CQRS, Azure EventHub
  - [x] Data Loading (4 projects) - CSV/JSON ETL pipeline with CLI
  - [x] Code Generation (4 projects) - Attribute-driven test data generation
  - [x] Document Management (3 projects) - Enhanced document contracts
  - [x] Distributed Caching (7 projects) - Redis provider
  - [x] Identity & Session (3 projects) - Enhanced identity management
  - [x] Text Templating (3 projects) - Unified templating
  - [x] Accounting (1 project) - Domain-specific (evaluate usefulness)
- [x] **Migration Plan:**
  - [x] Phase 0: Framework upgrades (.NET 9.0, namespace cleanup)
  - [x] Phases 1-11: Categorized migrations by priority (CRITICAL, HIGH, MEDIUM, LOW)
  - [x] Phase 12: Final integration and testing
  - [x] All 380+ migration tasks added to TODO.md below

**COMPLEXITY:** VERY HIGH - 52 projects requiring careful integration, critical merges, and .NET 9.0 upgrades.

**CRITICAL FINDING:** OoBDev.Communications in main codebase is a 16 LOC stub. SharedFramework has full 1,145 LOC implementation with Twilio SendGrid and SMS providers. This is a critical capability gap that must be merged.

**RECOMMENDATION:** Execute 12-phase migration plan. Prioritize Phase 2 (Communications merge) as CRITICAL.

**Outcome:** Investigation complete. Migration plan ready. **All tasks added to TODO.md.**

### Incomming Project Tracking
- [x] Created `Incomming/CHECKLIST.md` - Master tracking document for all 9 Incomming projects
- [x] Updated protocol v1.0 → v1.1 with checklist management, project type classifications, and TODO.md integration

### Protocols
- [x] Created `.claude/protocols/software/incoming-codebase-comparison.md` (v1.0)
- [x] Updated `.claude/protocols/software/incoming-codebase-comparison.md` to v1.1:
  - [x] Added project type classifications (6 types)
  - [x] Added Step 1.5: Update Checklist (register investigation start)
  - [x] Added Step 5.5: Add Migration Tasks to TODO.md
  - [x] Added Step 13: Update Checklist - Investigation Complete
  - [x] Added Example 2: Sample Application (BotChat)
- [x] Updated `.claude/protocols/software/README.md` with new protocol

### CI/CD
- [x] Updated `.github/workflows/dotnet.yml` - Added path filters for selective compilation

### Phase 0: Critical Bug Fixes (COMPLETED)
- [x] **PathEx.cs** - Fixed lambda expression syntax error (lines 42, 66, 92)
- [x] **StreamDevice.cs** - Fixed nullable annotations and transmission delay typo
- [x] **SerialPortFactory.cs** - Simplified verbose ternary expression
- [x] **ShiftCommutativeVariablesRight.cs** - Replaced non-functional stub with working implementation
- [x] **ExpressionParserTests.cs** - Fixed floating-point precision test failures by adding epsilon tolerance
- [x] **NumericAsserts.cs** - Created reusable numeric comparison utility in OoBDev.TestUtilities

---

## Pending Work

### OoBDev.AspNetCore.Mvc - Swashbuckle 10.1.0 Breaking Changes (IN PROGRESS)

**Issue:** Assembly updates introduced breaking changes in Swashbuckle 10.1.0 API

**Changes Made (2026-01-15):**

**File: FormFileOperationFilter.cs**
- [x] Fixed CS0200: `IOpenApiSchema.Properties` is read-only
  - Changed from: `schema.Properties = new Dictionary<...>()`
  - Changed to: Loop through fileParams and add to `schema.Properties[propertyName]`
  - Location: Lines 34-43
- [x] Fixed CS0029: String to JsonSchemaType conversion
  - Changed from: `Type = "string"`
  - Changed to: `Type = JsonSchemaType.String`
  - Location: Line 40

**File: HealthChecksDocumentFilter.cs**
- [x] Fixed CS1503: `OpenApiTag` vs `OpenApiTagReference` mismatch
  - Changed from: `operation.Tags.Add(new OpenApiTag { Name = "ApiHealth" })`
  - Changed to: `operation.Tags.Add(new OpenApiTagReference { Name = "ApiHealth" })`
  - Location: Line 28
- [x] Fixed CS0200 & CS0266: Properties assignment and type conversion
  - Changed from: `Properties = properties` (assignment)
  - Changed to: Build schema with `.Properties.Add()` calls
  - Location: Lines 30-36

**File: SearchQueryOperationFilter.cs**
- [x] Fixed CS1503: OpenApiTag references (3 occurrences)
  - Changed from: `new OpenApiTag { Name = ... }`
  - Changed to: `new OpenApiTagReference { Name = ... }`
  - Location: Lines 48, 52, 57
- [x] Fixed CS0019: Coalesce assignment with mismatched types
  - Changed from: `operation.Parameters ??= new List<OpenApiParameter>()`
  - Changed to: Traditional null check and assignment
  - Location: Lines 114-118
- [x] Fixed UpdateRequestSchema method (major refactor)
  - Changed from: Creating mutable dictionary copy with `.ChangeComparer()`
  - Changed to: Direct schema property access via `schema.Properties[key]`
  - Removed unnecessary `.Properties = properties` reassignment
  - Added null checks for schema lookups
  - Fixed IOpenApiSchema vs OpenApiSchema conversions
  - Location: Lines 187-292

**Remaining:**
- [ ] Verify build: `dotnet build src/Framework/OoBDev.AspNetCore.Mvc/OoBDev.AspNetCore.Mvc.csproj`
- [ ] Verify tests pass: `dotnet test src/ --filter "OoBDev.AspNetCore.Mvc"`
- [ ] Check for any remaining compiler errors in related filters

**Breaking Change Summary:**
- `OpenApiTag` collection now expects `OpenApiTagReference` objects
- `IOpenApiSchema.Properties` is read-only collection (use `.Add()` instead of assignment)
- `JsonSchemaType` is enum (not string) - use enum values like `JsonSchemaType.String`, `JsonSchemaType.Object`, etc.
- Schema reference API requires proper null checking on `?.Reference`

---

### Build Verification (COMPLETED)
- [x] Run `dotnet build src/` to verify all bug fixes compile
- [x] Run `dotnet test src/` to verify all tests pass - ALL PASSED
- [x] Address any compilation or test failures - None found

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

### Option 2: REFERENCE as External NuGet Package
- **Action:** Verify if published to NuGet, or publish it, then reference where needed
- **Effort:** LOW (add PackageReference)
- **Maintenance:** MINIMAL (external updates)
- **Pros:** No code maintenance, separate evolution
- **Cons:** Dependency on external package, may not be published

### Option 3: ARCHIVE in Incomming/
- **Action:** Create README.md documenting decision, keep for reference
- **Effort:** MINIMAL (documentation only)
- **Maintenance:** NONE
- **Pros:** Available for future reconsideration, no commitment
- **Cons:** Not integrated, users won't discover it

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

**Documentation:**
- [Feature Mapping](docs/migration/oobtainium-feature-mapping.md) - Complete feature analysis and comparison
- [Migration Plan](docs/migration/oobtainium-migration-plan.md) - All 4 options with detailed steps

**Next Steps:**
- [ ] **DECISION:** Choose Option 1, 2, 3, or 4
- [ ] Execute chosen option per migration plan sections below

---

### Oobtainium Migration - Option 1: MIGRATE to Main Framework

**Execute ONLY if Option 1 is chosen**

**Effort:** MEDIUM | **Maintenance:** HIGH | **See:** [Migration Plan Phase 1-5](docs/migration/oobtainium-migration-plan.md)

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

### Oobtainium Migration - Option 2: REFERENCE as External NuGet

**Execute ONLY if Option 2 is chosen**

**Effort:** LOW | **Maintenance:** MINIMAL

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

### Oobtainium Migration - Option 3: ARCHIVE in Incomming/

**Execute ONLY if Option 3 is chosen**

**Effort:** MINIMAL | **Maintenance:** NONE

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

### Oobtainium Migration - Option 4: DELETE ⭐ RECOMMENDED

**Execute ONLY if Option 4 is chosen**

**Effort:** MINIMAL | **Maintenance:** NONE

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

---

## Incomming/Framework Migration Work

**Status:** 🔍 INVESTIGATION COMPLETE - Ready for Phase 0 systematic comparison

**Complexity:** HIGH - 55 files with mix of NEW features and DIFFERS requiring individual review

**Documentation:**
- [Feature Mapping](docs/migration/framework-feature-mapping.md) - Complete 30 NEW + 25 DIFFERS analysis
- [Vector Comparison](docs/migration/vector-comparison.md) - Detailed vector implementation analysis

### Phase 0: Systematic File Comparison (REQUIRED FIRST)

**Purpose:** Compare all 25 DIFFERS files to identify bug fixes, improvements, and breaking changes

**Priority:** CRITICAL - Must complete before any migration

- [ ] **0.1 Core Abstractions (8 files)**
  - [ ] Compare `IAccessor.cs` - Async-local accessor pattern
  - [ ] Compare `IDataConverter.cs` - Type conversion interface
  - [ ] Compare `IDatabaseMapper.cs` - Database mapping interface
  - [ ] Compare `IDatabaseQuery.cs` - Database query interface
  - [ ] Compare `IHttpPrepareRequest.cs` - HTTP request customization
  - [ ] Compare `IHttpPrepareRequestFeature.cs` - Middleware feature
  - [ ] Compare `IJsonSerializer.cs` - JSON serialization interface
  - [ ] Compare `Accessor.cs` - Accessor implementation
  - [ ] Create comparison matrix documenting differences

- [ ] **0.2 Core Implementations (8 files)**
  - [ ] Compare `DataConverter.cs` - Type conversion implementation
  - [ ] Compare `DatabaseQuery.cs` - Query execution
  - [ ] Compare `HttpPrepareRequest.cs` - HTTP preparation base
  - [ ] Compare `CorrelationInfo.cs` - Correlation tracking
  - [ ] Compare `CorrelationInfoMiddleware.cs` - Middleware implementation
  - [ ] Compare `CorrelationInfoHttpPrepareRequestFeature.cs` - Feature implementation
  - [ ] Compare `ServiceCollectionExtensions.cs` (both projects)
  - [ ] Document bug fixes and improvements found

- [ ] **0.3 Validation Attributes (7 files)**
  - [ ] Compare `ConnectionStringNameAttribute.cs` - Database connection attribute
  - [ ] Compare `QueryParameterAttribute.cs` - Stored procedure parameters
  - [ ] Compare `QueryResultAttribute.cs` - Result mapping
  - [ ] Compare `StoredProcedureAttribute.cs` - SP mapping
  - [ ] Compare `ZipCodeAttribute.cs` - Zip code validation
  - [ ] Compare `ZipCodesAttribute.cs` - Multi-zip validation
  - [ ] Verify regex patterns and validation logic

- [ ] **0.4 ASP.NET Core (2 files)**
  - [ ] Compare `ApplicationBuilderExtensions.cs` - Pipeline configuration
  - [ ] Compare `AdditionalSwaggerGenEndpointsOptions.cs` - **May already be fixed in dotnet-lib migration**
  - [ ] Document middleware additions

- [ ] **0.5 Constants & Headers (1 file)**
  - [ ] Compare `DefinedHttpHeaders.cs` - HTTP header constants
  - [ ] Check for new headers

- [ ] **0.7 Namespace Mapping Decision**
  - [ ] Decide: `OoBDev.Common` → `OoBDev.System` or keep as `OoBDev.Common`?
  - [ ] Map all Incomming namespaces to target main framework namespaces
  - [ ] Document namespace migration strategy

- [ ] **0.8 Phase 0 Summary Report**
  - [ ] Create `docs/migration/framework-phase0-comparison.md`
  - [ ] Document all differences found in 25 DIFFERS files
  - [ ] Identify bug fixes to apply
  - [ ] Identify breaking changes
  - [ ] Create prioritized migration task list

### Phase 1: Vector Math Library Migration (READY)

**Status:** ✅ APPROVED - Analysis complete, ready to execute

**Complexity:** LOW - 4 self-contained files, well-tested

**Priority:** HIGH - Critical for SemanticKernel and AI/ML features

**See:** [Vector Comparison](docs/migration/vector-comparison.md) for detailed analysis

- [ ] **1.1 Create Target Directory Structure**
  - [ ] Create `src/Framework/OoBDev.System/Math/` directory
  - [ ] Create `src/Framework/OoBDev.System.Tests/Math/` directory

- [ ] **1.2 Migrate Core Vector Files**
  - [ ] Migrate `Vector.cs` to `src/Framework/OoBDev.System/Math/Vector.cs`
    - Change namespace: `OoBDev.Common.Math` → `OoBDev.System.Math`
    - Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to VectorMath methods
  - [ ] Migrate `VectorMath.cs` to `src/Framework/OoBDev.System/Math/VectorMath.cs`
    - Change namespace to `OoBDev.System.Math`
    - Add aggressive inlining to distance metric methods
    - Add `CosineSimilarity` method (returns `1.0 - CosineDistance(...)`)
  - [ ] Migrate `VectorDistanceMetrics.cs` to `src/Framework/OoBDev.System/Math/VectorDistanceMetrics.cs`
    - Change namespace to `OoBDev.System.Math`
  - [ ] Migrate `VectorComparer.cs` to `src/Framework/OoBDev.System/Math/VectorComparer.cs`
    - Change namespace to `OoBDev.System.Math`

- [ ] **1.3 Migrate Tests**
  - [ ] Migrate `VectorTests.cs` to `src/Framework/OoBDev.System.Tests/Math/VectorTests.cs`
    - Update namespace references
    - Add additional test cases for edge cases
    - Target 80%+ coverage

- [ ] **1.4 Update Project References**
  - [ ] Add Math files to `OoBDev.System.csproj`
  - [ ] Add test references to `OoBDev.System.Tests.csproj`
  - [ ] Verify compilation with `dotnet build`

- [ ] **1.5 Create SqlVector Conversion Utilities**
  - [ ] Create `src/Extensions/OoBDev.Data.Vectors/VectorConversions.cs`
  - [ ] Add extension method: `ToSqlVector(this Vector vector)`
  - [ ] Add extension method: `ToVector(this SqlVector sqlVector)`
  - [ ] Add unit tests for conversions

- [ ] **1.6 Enhance OoBDev.Data.Vectors**
  - [ ] Add `CosineDistance` method to `VectorFunctions.cs` (returns `1.0 - CosineSimilarity(...)`)
  - [ ] Ensure both `CosineSimilarity` and `CosineDistance` available
  - [ ] Add XML documentation explaining difference
  - [ ] Update tests

- [ ] **1.7 Update SemanticKernel Integration**
  - [ ] Check `src/Framework/OoBDev.SemanticKernel/` for vector dependencies
  - [ ] Add reference to `OoBDev.System` if needed
  - [ ] Update any vector-related code to use new namespace
  - [ ] Verify SemanticKernel plugins still work

- [ ] **1.8 Documentation**
  - [ ] Add XML documentation to all public Vector APIs
  - [ ] Create `src/Framework/OoBDev.System/Math/README.md` explaining usage
  - [ ] Document difference between `Vector` (application) and `SqlVector` (database)
  - [ ] Add examples of RAG/AI workflow using both

- [ ] **1.9 Testing & Validation**
  - [ ] Run `dotnet build src/Framework/OoBDev.System/`
  - [ ] Run `dotnet test src/Framework/OoBDev.System.Tests/`
  - [ ] Verify 80%+ test coverage on new Math namespace
  - [ ] Run integration tests if available

### Phase 2: High-Priority NEW Features Migration

**Status:** ⏸️ BLOCKED - Awaiting Phase 0 completion and namespace decisions

- [ ] **2.1 SQL Database Mapper**
  - [ ] Target: `src/Extensions/OoBDev.Data.Common/` or new `OoBDev.Data.SqlServer/`
  - [ ] Migrate `SqlDatabaseMapper.cs`
  - [ ] Add comprehensive tests
  - [ ] Document usage patterns

- [ ] **2.2 Audit Logging Middleware**
  - [ ] Target: `src/Framework/OoBDev.AspNetCore.Mvc/Middleware/`
  - [ ] Migrate all 5 audit logging files
  - [ ] Create sample `IAuditLoggingRecorder` implementation
  - [ ] Add configuration options
  - [ ] Document setup and usage

- [ ] **2.3 User & Application Access**
  - [ ] Target: `src/Framework/OoBDev.System/` or `src/Framework/OoBDev.AspNetCore.Mvc/`
  - [ ] Migrate `IApplicationAccess.cs`, `ICurrentUserAccessor.cs`
  - [ ] Migrate implementations: `CurrentUserAccessor.cs`, `HttpCurrentUserAccessor.cs`
  - [ ] Integrate with existing identity system
  - [ ] Add multi-tenancy examples

- [ ] **2.4 Environment Settings**
  - [ ] Target: `src/Framework/OoBDev.AspNetCore.Mvc/Configuration/`
  - [ ] Migrate all 4 environment settings files
  - [ ] Integrate with existing configuration patterns
  - [ ] Add examples

### Phase 3: Medium-Priority NEW Features Migration

**Status:** ⏸️ BLOCKED - Awaiting Phase 0 and Phase 2 completion

- [ ] **3.1 HTTP Extensions**
  - [ ] Migrate `HttpContextExtensions.cs`
  - [ ] Migrate `OoBDevClientOptions.cs`
  - [ ] Migrate `OoBDevHttpPrepareRequest.cs`
  - [ ] Add tests

- [ ] **3.3 JSON Serializer Wrapper**
  - [ ] Migrate `WrappedJsonSerializer.cs`
  - [ ] Integrate with existing JSON configuration
  - [ ] Add tests

- [ ] **3.4 Validation Attributes**
  - [ ] Migrate `QuoteIdAttribute.cs`
  - [ ] Review for domain-specific logic that needs generalization
  - [ ] Add tests

### Phase 4: Merge DIFFERS Files

**Status:** ⏸️ BLOCKED - Requires Phase 0 comparison results

- [ ] **4.1 Apply Bug Fixes**
  - [ ] Apply any bug fixes found in Phase 0 comparison
  - [ ] Test thoroughly
  - [ ] Document changes

- [ ] **4.2 Merge Improvements**
  - [ ] Merge improvements from Incomming to main
  - [ ] Maintain backward compatibility
  - [ ] Update tests

- [ ] **4.3 Breaking Change Analysis**
  - [ ] Document any breaking changes
  - [ ] Create migration guide if needed
  - [ ] Update CHANGELOG

### Phase 5: Final Testing & Cleanup

**Status:** ⏸️ BLOCKED - Awaiting Phase 1-4 completion

- [ ] **5.1 Integration Testing**
  - [ ] Test all migrated features together
  - [ ] Verify SemanticKernel integration works
  - [ ] Test audit logging with real scenarios
  - [ ] Verify SQL Database Mapper with actual database

- [ ] **5.2 Documentation**
  - [ ] Update main README.md
  - [ ] Create migration guide for users
  - [ ] Add examples for all new features
  - [ ] Update architecture docs

- [ ] **5.3 Cleanup**
  - [ ] Delete or archive `Incomming/Framework/` after successful migration
  - [ ] Update TODO.md with results
  - [ ] Create final migration report

---

## Incomming/SharedFramework Migration Work

**Status:** 🔍 INVESTIGATION COMPLETE - Ready for systematic migration

**Complexity:** HIGH - 52 projects (~28,582 LOC) requiring careful integration

**Documentation:**
- [Feature Mapping](docs/migration/sharedframework-feature-mapping.md) - Complete 52-project analysis
- [Migration Plan](docs/migration/sharedframework-migration-plan.md) - 12-phase execution plan

**Summary:**
- 24 projects (71%) provide NEW capabilities
- 10 projects (29%) are ENHANCED versions (DIFFERS - need merge)
- 52 total projects: 34 implementation + 18 test projects
- Critical external service integrations (AWS SQS, Azure, Twilio, geocoding)
- Completes partially-implemented main framework components

### Phase 0: Framework Upgrades (FOUNDATION)

**Priority:** CRITICAL

- [ ] **0.1 Upgrade All Projects to .NET 9.0**
  - [ ] Update all 52 `.csproj` files: `net8.0` → `net9.0`
  - [ ] Update NuGet packages to .NET 9-compatible versions
  - [ ] Test compilation
  - [ ] Fix breaking API changes

- [ ] **0.2 Namespace Cleanup**
  - [ ] Create namespace mapping matrix
  - [ ] Remove "Api." prefix: `OoBDev.Api.Twilio` → `OoBDev.Twilio`
  - [ ] Update all using statements
  - [ ] Verify no broken references

### Phase 1: Message Queueing Providers (HIGH PRIORITY)

**Priority:** HIGH - Critical cloud messaging

- [ ] **1.1 Migrate Amazon SQS**
  - [ ] Create `src/ExternalServices/Amazon/OoBDev.Amazon.Sqs/`
  - [ ] Copy source files (183 LOC)
  - [ ] Add `AWSSDK.SQS` NuGet package
  - [ ] Add ServiceCollectionExtensions with TryAddAmazonSqs()
  - [ ] Migrate test project
  - [ ] Create README with usage examples
  - [ ] Verify compilation and tests

- [ ] **1.2 Migrate Azure Service Bus**
  - [ ] Create `src/ExternalServices/Azure/OoBDev.Azure.ServiceBus/`
  - [ ] Copy source files (114 LOC)
  - [ ] Add `Azure.Messaging.ServiceBus` NuGet package
  - [ ] Verify topics, sessions, dead-letter queue support
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate test project
  - [ ] Create README
  - [ ] Verify compilation and tests

### Phase 2: Communications Complete Implementation (CRITICAL)

**Priority:** CRITICAL - Main has only 16 LOC stub, SF has 1,145 LOC implementation

- [ ] **2.1 Merge OoBDev.Communications.Contracts**
  - [ ] Backup main version (125 LOC)
  - [ ] Compare interfaces for breaking changes
  - [ ] Replace main with SharedFramework version (695 LOC)
  - [ ] Update namespace to match main: `OoBDev.Communications`
  - [ ] Merge unique main features (if any)
  - [ ] Update dependent projects
  - [ ] Migrate tests
  - [ ] Create migration guide for users

- [ ] **2.2 Merge OoBDev.Communications**
  - [ ] Backup main stub (16 LOC)
  - [ ] Replace with SharedFramework implementation (1,145 LOC)
  - [ ] Update namespace
  - [ ] Verify composers, handlers, providers integrated
  - [ ] Migrate tests
  - [ ] Update ServiceCollectionExtensions
  - [ ] Create comprehensive README

- [ ] **2.3 Migrate Twilio SendGrid Provider**
  - [ ] Create `src/ExternalServices/Twilio/OoBDev.Twilio.SendGrid/`
  - [ ] Copy files (267 LOC), update namespace
  - [ ] Add `SendGrid` NuGet package
  - [ ] Integrate with OoBDev.Communications abstractions
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README with API key setup
  - [ ] Verify email delivery works

- [ ] **2.4 Migrate Twilio SMS Provider**
  - [ ] Create `src/ExternalServices/Twilio/OoBDev.Twilio.SmsMessaging/`
  - [ ] Copy files (151 LOC), update namespace
  - [ ] Add `Twilio` NuGet package
  - [ ] Integrate with OoBDev.Communications abstractions
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README
  - [ ] Verify SMS works

### Phase 3: Spatial Services Suite (HIGH PRIORITY)

**Priority:** HIGH - Main has ZERO spatial/geocoding capability

- [ ] **3.1 Migrate Spatial Services Contracts**
  - [ ] Create `src/Framework/OoBDev.SpatialServices.Abstractions/`
  - [ ] Copy contracts (85 LOC): ILocationServices, address models
  - [ ] Update namespace to `OoBDev.SpatialServices`
  - [ ] Add to solution
  - [ ] Create README

- [ ] **3.2 Migrate Spatial Services Common**
  - [ ] Create `src/Framework/OoBDev.SpatialServices/`
  - [ ] Copy utilities (21 LOC)
  - [ ] Add reference to Abstractions
  - [ ] Add ServiceCollectionExtensions
  - [ ] Add to solution

- [ ] **3.3 Migrate Census Geocoding Provider**
  - [ ] Create `src/ExternalServices/Census/OoBDev.Census.Geocoding/`
  - [ ] Copy files (420 LOC), update namespace
  - [ ] Implement ILocationServices interface
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README (note: free, no API key)
  - [ ] Verify geocoding works

- [ ] **3.4 Migrate Google Maps Provider**
  - [ ] Create `src/ExternalServices/Google/OoBDev.Google.Maps/`
  - [ ] Copy files (269 LOC), update namespace
  - [ ] Implement ILocationServices
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README with API key setup
  - [ ] Verify geocoding works

- [ ] **3.5 Migrate Bing Maps Provider**
  - [ ] Create `src/ExternalServices/Microsoft/OoBDev.Microsoft.BingMaps/`
  - [ ] Copy files (288 LOC), update namespace
  - [ ] Implement ILocationServices
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README
  - [ ] Verify geocoding works

### Phase 4: Distributed Caching (HIGH PRIORITY)

**Priority:** HIGH - Main has NO distributed caching

- [ ] **4.1 Migrate Caching Contracts**
  - [ ] Create `src/Framework/OoBDev.Caching.Abstractions/`
  - [ ] Copy interfaces (83 LOC)
  - [ ] Copy attributes: `[IsCacheable]`, `[FlushCache]`
  - [ ] Update namespace
  - [ ] Add to solution
  - [ ] Create README

- [ ] **4.2 Migrate Caching Common**
  - [ ] Create `src/Framework/OoBDev.Caching/`
  - [ ] Copy factories, managers (290 LOC)
  - [ ] Add reference to Abstractions
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README

- [ ] **4.3 Migrate Microsoft Caching Provider**
  - [ ] Create `src/ExternalServices/Microsoft/OoBDev.Microsoft.Caching/`
  - [ ] Copy files (70 LOC)
  - [ ] Add `Microsoft.Extensions.Caching.Memory` package
  - [ ] Implement caching provider interface
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README

- [ ] **4.4 Migrate Redis Caching Provider**
  - [ ] Create `src/ExternalServices/Redis/OoBDev.Redis.Caching/`
  - [ ] Copy files (119 LOC)
  - [ ] Add `StackExchange.Redis` NuGet package
  - [ ] Implement caching provider interface
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README with Redis connection setup
  - [ ] Verify Redis caching works

### Phase 5: Data Loading Pipeline (HIGH PRIORITY)

**Priority:** HIGH - Critical ETL tool (1,633 LOC)

- [ ] **5.1 Migrate DataLoader Contracts**
  - [ ] Create `src/Framework/OoBDev.DataLoader.Abstractions/`
  - [ ] Copy interfaces and models (435 LOC)
  - [ ] Update namespace
  - [ ] Add to solution
  - [ ] Create README

- [ ] **5.2 Migrate DataLoader Implementation**
  - [ ] Create `src/Framework/OoBDev.DataLoader/`
  - [ ] Copy ETL pipeline (1,633 LOC)
  - [ ] Add `CsvHelper`, `YamlDotNet` NuGet packages
  - [ ] Add reference to Abstractions
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README with CSV/JSON examples
  - [ ] Verify CSV to database works
  - [ ] Verify JSON to database works

- [ ] **5.3 Migrate DataLoader CLI Tool**
  - [ ] Create `src/Tools/OoBDev.DataLoader.Cli/`
  - [ ] Copy CLI application (228 LOC)
  - [ ] Add reference to OoBDev.DataLoader
  - [ ] Test CLI execution
  - [ ] Create README with command-line usage
  - [ ] Add to solution under Tools

### Phase 6: Document Management Enhancement (HIGH PRIORITY)

**Priority:** HIGH - SF is 71% larger than main (911 LOC vs 531 LOC)

- [ ] **6.1 Migrate DocumentCenter.Contracts**
  - [ ] Create `src/Framework/OoBDev.Documents.Abstractions/`
  - [ ] Copy handlers, providers, storage interfaces (422 LOC)
  - [ ] Update namespace to `OoBDev.Documents`
  - [ ] Add to solution
  - [ ] Create README

- [ ] **6.2 Merge DocumentCenter Implementation**
  - [ ] Backup main version (531 LOC)
  - [ ] Compare implementations
  - [ ] Merge: Keep main abstractions, add SF packaging/conversion
  - [ ] Update namespace
  - [ ] Migrate tests from both
  - [ ] Create comprehensive README
  - [ ] Verify no breaking changes

### Phase 7: Identity & Session Enhancement (HIGH PRIORITY)

**Priority:** HIGH - SF is 2.3x larger (291 LOC vs 125 LOC)

- [ ] **7.1 Merge IdentityModel.Contracts**
  - [ ] Backup main version (125 LOC)
  - [ ] Compare contracts
  - [ ] Merge: Add SF's user session, rights, claims
  - [ ] Update namespace
  - [ ] Migrate tests
  - [ ] Create README
  - [ ] Verify no breaking changes

- [ ] **7.2 Migrate IdentityModel.Extensions**
  - [ ] Copy authorization services (204 LOC)
  - [ ] Copy globalization support
  - [ ] Add to existing OoBDev.Identity project
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Update README

### Phase 8: Complex Events & Event Sourcing (MEDIUM PRIORITY)

**Priority:** MEDIUM - Main has NO event sourcing

- [ ] **8.1 Migrate ComplexEvents.Contracts**
  - [ ] Create `src/Framework/OoBDev.ComplexEvents.Abstractions/`
  - [ ] Copy event sourcing contracts (315 LOC)
  - [ ] Copy CQRS interfaces
  - [ ] Update namespace
  - [ ] Add to solution
  - [ ] Create README

- [ ] **8.2 Migrate ComplexEvents.Common**
  - [ ] Create `src/Framework/OoBDev.ComplexEvents/`
  - [ ] Copy resolvers, subscribers, schedulers (680 LOC)
  - [ ] Add `NCrontab` NuGet package
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README with cron examples
  - [ ] Verify `[ScheduleAt("0 */45 * * * *")]` works

- [ ] **8.3 Migrate ComplexEvents.DatabaseExtensions**
  - [ ] Create `src/Framework/OoBDev.ComplexEvents.Database/`
  - [ ] Copy T-SQL schema scripts
  - [ ] Create migration scripts
  - [ ] Document schema in README

- [ ] **8.4 Migrate ComplexEvents.EntityFrameworkCore**
  - [ ] Create `src/Framework/OoBDev.ComplexEvents.EntityFrameworkCore/`
  - [ ] Copy DbContext (252 LOC)
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README
  - [ ] Verify EF Core persistence works

- [ ] **8.5 Migrate Azure EventHub**
  - [ ] Create `src/ExternalServices/Azure/OoBDev.Azure.EventHub/`
  - [ ] Copy integration (141 LOC)
  - [ ] Add `Azure.Messaging.EventHubs` package
  - [ ] Integrate with ComplexEvents abstractions
  - [ ] Add ServiceCollectionExtensions
  - [ ] Migrate tests
  - [ ] Create README

### Phase 9: Test Data Generation (MEDIUM PRIORITY)

**Priority:** MEDIUM - Attribute-driven test data (1,256 LOC)

- [ ] **9.1 Migrate Generations.Contracts**
  - [ ] Create `src/Extensions/OoBDev.Extensions.TestData.Abstractions/`
  - [ ] Copy generation rules (488 LOC)
  - [ ] Update namespace to `OoBDev.Extensions.TestData`
  - [ ] Add to solution
  - [ ] Create README

- [ ] **9.2 Migrate Generations Implementation**
  - [ ] Create `src/Extensions/OoBDev.Extensions.TestData/`
  - [ ] Copy generators (1,256 LOC): `[EmailAddress]`, `[Phone]`, etc.
  - [ ] Copy type converters
  - [ ] Add reference to Abstractions
  - [ ] Migrate tests
  - [ ] Create README with attribute examples
  - [ ] Verify attribute-driven generation works

- [ ] **9.3 Merge Generations DI Extensions**
  - [ ] Copy ServiceCollectionExtensions (21 LOC)
  - [ ] Merge into TestData project
  - [ ] Test DI registration

### Phase 10: Text Templating Unification (MEDIUM PRIORITY)

**Priority:** MEDIUM - Unify scattered implementations

- [ ] **10.1 Migrate TextTemplating.Contracts**
  - [ ] Create `src/Framework/OoBDev.TextTemplating.Abstractions/`
  - [ ] Copy template contracts (117 LOC)
  - [ ] Update namespace
  - [ ] Add to solution
  - [ ] Create README

- [ ] **10.2 Merge TextTemplating Implementation**
  - [ ] Create `src/Framework/OoBDev.TextTemplating/`
  - [ ] Copy SF templating engine (424 LOC)
  - [ ] Integrate with main's abstractions
  - [ ] Keep OoBDev.TemplateEngine.Cli separate
  - [ ] Migrate tests
  - [ ] Create README
  - [ ] Verify template engine works

### Phase 11: Domain-Specific Review (LOW PRIORITY)

**Priority:** LOW - Assess if generally useful

- [ ] **11.1 Review Accounting.Contracts**
  - [ ] Review invoice/payment models (166 LOC)
  - [ ] Determine if generally useful or application-specific
  - [ ] Decision: Migrate to Extensions, Archive, or Delete
  - [ ] If migrating: Create `src/Extensions/OoBDev.Extensions.Accounting/`

### Phase 12: Final Integration & Testing (FINAL)

**Priority:** CRITICAL - Final validation

- [ ] **12.1 Solution-Wide Integration**
  - [ ] Verify all 52 projects added to OoBDev.sln
  - [ ] Resolve circular dependencies
  - [ ] Build entire solution
  - [ ] Verify no compilation errors

- [ ] **12.2 Test Suite Execution**
  - [ ] Run all migrated test projects
  - [ ] Verify 80%+ coverage
  - [ ] Fix failing tests
  - [ ] Document test limitations

- [ ] **12.3 Documentation Updates**
  - [ ] Update FEATURE_INVENTORY.md with 52 projects
  - [ ] Update main README.md
  - [ ] Create ConfigurationSettings.md updates
  - [ ] Create migration guide for users

- [ ] **12.4 Cleanup & Archive**
  - [ ] Delete or archive `Incomming/SharedFramework/`
  - [ ] Update Incomming/CHECKLIST.md
  - [ ] Update TODO.md with completion
  - [ ] Create final migration report

---

## BinaryDataDecoders Migration Work

### Phase 1: Foundation Enhancement

#### 1.1 Endianness Support (UPDATE)
- [ ] Review `Incomming/BinaryDecoders/Utilities/Endian.cs`
- [ ] Enhance `OoBDev.System/BinaryPrimitives/EndianType.cs` with:
  - [ ] Runtime detection methods
  - [ ] Conversion methods for all primitive types
  - [ ] Extension methods for `BinaryReader`/`BinaryWriter`
- [ ] Add comprehensive unit tests
- [ ] Update documentation

**Files to modify:**
- `src/Framework/OoBDev.System/BinaryPrimitives/EndianType.cs`

#### 1.2 Utility Enhancements (UPDATE)
- [ ] Review utilities in `Incomming/BinaryDecoders/Utilities/`
- [ ] Enhance `OoBDev.System/IO/PathEx.cs` with:
  - [ ] Add `GetRelativePath` if missing
  - [ ] Add `NormalizePath` if missing
  - [ ] Add file enumeration improvements
- [ ] Migrate UI/MVVM collections:
  - [ ] ObservableDictionary (INotifyPropertyChanged + INotifyCollectionChanged)
  - [ ] Other observable collections for WPF/Windows Forms/Blazor
  - [ ] Create `OoBDev.Extensions.UI.Collections` if needed
- [ ] Add unit tests for new functionality

**Files to review:**
- `src/Framework/OoBDev.System/IO/PathEx.cs`
- `Incomming/BinaryDecoders/Utilities/PathHelper.cs`
- `Incomming/BinaryDecoders/ToolKit/Collections/`

#### 1.3 BinaryPrimitives Expansion (NEW)
- [ ] Create `src/Framework/OoBDev.System/BinaryPrimitives/BinaryReaderExtensions.cs`
  - [ ] Add `ReadInt16BigEndian()`, `ReadInt16LittleEndian()`
  - [ ] Add `ReadInt32BigEndian()`, `ReadInt32LittleEndian()`
  - [ ] Add `ReadInt64BigEndian()`, `ReadInt64LittleEndian()`
  - [ ] Add UInt variants
  - [ ] Add float/double variants
- [ ] Create `src/Framework/OoBDev.System/BinaryPrimitives/BinaryWriterExtensions.cs`
  - [ ] Add corresponding Write methods
- [ ] Add comprehensive unit tests
- [ ] Add XML documentation

**New files to create:**
- `src/Framework/OoBDev.System/BinaryPrimitives/BinaryReaderExtensions.cs`
- `src/Framework/OoBDev.System/BinaryPrimitives/BinaryWriterExtensions.cs`
- `src/Tests/OoBDev.System.Tests/BinaryPrimitives/BinaryReaderExtensionsTests.cs`
- `src/Tests/OoBDev.System.Tests/BinaryPrimitives/BinaryWriterExtensionsTests.cs`

### Phase 2: High-Value Features

#### 2.1 CodeAnalysis Migration (NEW)
- [ ] Review `Incomming/BinaryDecoders/CodeAnalysis/` structure
- [ ] Design namespace: `OoBDev.CodeAnalysis.*` or `OoBDev.Roslyn.*`
- [ ] Create project structure:
  - [ ] `src/Framework/OoBDev.CodeAnalysis/`
  - [ ] `src/Framework/OoBDev.CodeAnalysis.CSharp/`
  - [ ] `src/Tests/OoBDev.CodeAnalysis.Tests/`

**Key components to migrate:**
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Extensions/CompilationUnitSyntaxExtensions.cs`
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Extensions/SyntaxNodeExtensions.cs`
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Visitors/` → adapter pattern
- [ ] `Incomming/BinaryDecoders/CodeAnalysis/Analyzers/` → Roslyn analyzers

**Sub-tasks:**
- [ ] Add Roslyn package references
- [ ] Implement using OoBDev patterns (Handler, Visitor)
- [ ] Add comprehensive unit tests
- [ ] Create analyzer test harness
- [ ] Document usage patterns

#### 2.2 ExpressionCalculator Migration (NEW)
- [ ] Review `Incomming/BinaryDecoders/ExpressionCalculator/` structure
- [ ] Create `src/Framework/OoBDev.ExpressionCalculator/` (or move from System)
- [ ] Verify current implementation vs incoming:
  - [ ] Compare ANTLR grammar files
  - [ ] Compare optimizer implementations
  - [ ] Compare evaluator implementations

**Key components to migrate/update:**
- [ ] `Expressions/` - Compare implementations
- [ ] `Evaluators/` - Compare implementations
- [ ] `Optimizers/` - Already fixed ShiftCommutativeVariablesRight, check others
- [ ] `Parser/` - Compare ANTLR grammars

**Sub-tasks:**
- [ ] Audit existing `src/Framework/OoBDev.System/ExpressionCalculator/`
- [ ] Identify gaps vs `Incomming/BinaryDecoders/ExpressionCalculator/`
- [ ] Create migration checklist for each sub-component
- [ ] Add missing optimizers
- [ ] Add comprehensive expression tests
- [ ] Performance benchmark comparisons

#### 2.3 Archive Support (NEW)
- [ ] Review `Incomming/BinaryDecoders/Archives/` structure
- [ ] Create `src/Framework/OoBDev.IO.Archives/`
- [ ] Implement formats:
  - [ ] TAR support
  - [ ] CPIO support
  - [ ] ZIP support (if not using System.IO.Compression)

**Architecture:**
- [ ] Design `IArchiveReader` interface
- [ ] Design `IArchiveWriter` interface
- [ ] Implement provider/factory pattern
- [ ] Add streaming support for large archives
- [ ] Add compression support

**Sub-tasks:**
- [ ] Create core abstractions
- [ ] Implement TAR reader/writer
- [ ] Implement CPIO reader/writer
- [ ] Add format detection
- [ ] Add comprehensive tests (including corrupted archives)
- [ ] Add performance tests

#### 2.4 BinaryData Enhancements (UPDATE)
- [ ] Review `Incomming/BinaryDecoders/BinaryData/`
- [ ] Enhance `src/Framework/OoBDev.System/BinaryData/`
- [ ] Add missing features:
  - [ ] Bit-level operations
  - [ ] Checksum/CRC utilities
  - [ ] Binary pattern matching

### Phase 3: Protocol Support

#### 3.1 NMEA Protocol Support (NEW)
- [ ] Review `Incomming/BinaryDecoders/Nmea/` structure
- [ ] Create `src/Framework/OoBDev.Protocols.Nmea/`
- [ ] Implement NMEA 0183 support:
  - [ ] Sentence parser
  - [ ] Sentence formatter
  - [ ] Checksum validation
  - [ ] Common sentence types (GGA, RMC, GSA, etc.)

**Architecture:**
- [ ] Design `INmeaSentence` interface
- [ ] Implement sentence-specific handlers
- [ ] Add validation and error handling
- [ ] Follow existing pipeline patterns

**Sub-tasks:**
- [ ] Create core parser
- [ ] Implement common sentence types
- [ ] Add extensibility for custom sentences
- [ ] Add comprehensive tests with real NMEA data
- [ ] Add documentation and examples

#### 3.2 Drawing/Geometry (NEW)
- [ ] Review `Incomming/BinaryDecoders/Drawing/`
- [ ] Decide: Migrate or use System.Drawing/SkiaSharp?
- [ ] If migrating:
  - [ ] Create `src/Framework/OoBDev.Drawing/`
  - [ ] Implement core primitives (Point, Size, Rectangle, etc.)
  - [ ] Implement geometry operations
- [ ] Add comprehensive tests

### Phase 4: Specialized Domain Features

#### 4.1 FileSystems (ISO 9660)
- [ ] Review `Incomming/BinaryDecoders/FileSystems/`
- [ ] Create `src/Extensions/OoBDev.Extensions.FileSystems.ISO9660/`
- [ ] Migrate ISO 9660 filesystem implementation
- [ ] Add comprehensive tests for ISO image reading
- [ ] Document use cases (CD/DVD access, image mounting)
- [ ] Add examples for common scenarios

**New files to create:**
- `src/Extensions/OoBDev.Extensions.FileSystems.ISO9660/`
- Full ISO 9660 reader implementation
- Tests and documentation

#### 4.2 Classic Cryptography (Educational)
- [ ] Review `Incomming/BinaryDecoders/Cryptography/`
- [ ] Create `src/Extensions/OoBDev.Security.Cryptography.Classic/`
- [ ] Migrate historical cipher implementations:
  - [ ] Enigma machine simulation
  - [ ] Lorenz cipher
  - [ ] Caesar cipher
  - [ ] Vigenère cipher
  - [ ] PlayFair cipher
- [ ] Add `[Obsolete("For educational use only. NOT SECURE.")]` to all classes
- [ ] Add strong security warnings in XML documentation
- [ ] Add comprehensive tests
- [ ] Document educational and CTF use cases

**Important:** Package separately, do NOT include in main distribution

**New files to create:**
- `src/Extensions/OoBDev.Security.Cryptography.Classic/`
- All cipher implementations with security warnings
- Educational documentation

#### 4.3 Retro Computing (Apple II)
- [ ] Review `Incomming/BinaryDecoders/Apple2/`
- [ ] Create `src/Extensions/OoBDev.Retro.Apple2/`
- [ ] Migrate Apple II disk format support
- [ ] Migrate DOS 3.3 filesystem support
- [ ] Add disk image readers/writers
- [ ] Add comprehensive tests
- [ ] Document use cases (legacy data recovery, historical preservation)

**New files to create:**
- `src/Extensions/OoBDev.Retro.Apple2/`
- Disk format implementations
- Tests and documentation

#### 4.4 Hardware Device Support (8 Devices)
- [ ] Review `Incomming/BinaryDecoders/` hardware projects
- [ ] Create `src/Extensions/OoBDev.Extensions.Hardware/` structure
- [ ] Migrate specialized hardware support:
  - [ ] **Kuando Busylight** - Presence indicators
    - Create `OoBDev.Extensions.Hardware.KuandoBusylight`
    - Device factory and provider pattern
    - Tests and documentation
  - [ ] **RadexOne** - Radiation detection
    - Create `OoBDev.Extensions.Hardware.RadexOne`
    - Safety and educational use cases
  - [ ] **Velleman K8055** - Experiment board
    - Create `OoBDev.Extensions.Hardware.VellemanK8055`
    - Hobbyist and educational applications
  - [ ] **Zoom H4n** - Audio equipment
    - Create `OoBDev.Extensions.Hardware.ZoomH4n`
    - Legacy audio device control
  - [ ] **Fencing equipment** - Sport automation
    - Create `OoBDev.Extensions.Hardware.Fencing`
    - Specialized sport equipment control
  - [ ] **LANC** - Camera protocol
    - Create `OoBDev.Extensions.Hardware.LANC`
    - Video production equipment
  - [ ] **EByte modules** - IoT/LoRa
    - Create `OoBDev.Extensions.Hardware.EByte`
    - LoRa and RS485 communication
  - [ ] **ZWave** - Home automation (if exists)
    - Create `OoBDev.Extensions.Hardware.ZWave`
    - Smart home device control
- [ ] Apply provider/factory pattern to all hardware
- [ ] Add comprehensive tests for each device type
- [ ] Package separately for specialized users
- [ ] Document use cases and setup for each device

**Note:** Rigol project is stub only - DELETE

#### 4.5 CLI Tools Migration
- [ ] Review `Incomming/BinaryDecoders/` CLI tools
- [ ] Migrate command-line utilities:
  - [ ] **IO.Controller.Cli** - Device controller
    - Migrate to `src/Tools/OoBDev.IO.Controller.Cli`
    - Device control and automation
  - [ ] **ServiceHost.Cli** - Network service host
    - Migrate to `src/Tools/OoBDev.Net.ServiceHost.Cli`
    - Service hosting utilities
  - [ ] **PackMan.Cli** - Package manager
    - Migrate to `src/Tools/OoBDev.PackMan.Cli`
    - Package management utilities
  - [ ] **Xslt.Cli** - XSLT transformation
    - Merge into existing `OoBDev.TemplateEngine.Cli`
    - XSLT transformation capabilities
- [ ] Add comprehensive help documentation for each tool
- [ ] Add unit and integration tests
- [ ] Package as dotnet tools

#### 4.6 Windows Forms Components
- [ ] Review `Incomming/BinaryDecoders/Windows.Forms/`
- [ ] Create `src/Extensions/OoBDev.Extensions.Windows.Forms/`
- [ ] Migrate Windows Forms validation controls:
  - [ ] Custom validators
  - [ ] Data binding helpers
  - [ ] UI validation components
- [ ] Ensure .NET 9.0 Windows Forms compatibility
- [ ] Add comprehensive tests
- [ ] Document desktop application scenarios
- [ ] Package separately for desktop/UI use cases

**Note:** OoBDev supports both desktop and server scenarios - Windows Forms extends the framework for desktop UI applications

#### 4.7 Platform-Specific Code Review
- [ ] Review UWP-specific code
  - [ ] Determine if any UWP features should be migrated
  - [ ] Update to modern Windows App SDK if needed
  - [ ] Otherwise DELETE
- [ ] Review .NET Framework-specific code
  - [ ] Port to .NET 9.0 if valuable
  - [ ] Otherwise DELETE
- [ ] Review other platform-specific features
  - [ ] Migrate valuable cross-platform features
  - [ ] DELETE platform-locked code without modern equivalent

### Phase 5: Cleanup & Documentation

#### 5.1 Future Development: DeepZoom Viewer Controls (NEW)

**Note:** These are NEW controls, not migrations. To be implemented after migration complete.

##### 5.1.1 WPF DeepZoom Viewer Control
- [ ] Create `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/`
- [ ] Design WPF control architecture:
  - [ ] DeepZoomViewer control (pan, zoom, multi-touch)
  - [ ] Progressive tile loading with caching
  - [ ] Smooth zoom transitions (animation)
  - [ ] Touch/gesture support (pinch-to-zoom)
  - [ ] Mouse wheel and drag support
  - [ ] Keyboard navigation
- [ ] Implement rendering pipeline:
  - [ ] Tile fetching (local and HTTP)
  - [ ] Tile caching strategy
  - [ ] View frustum culling
  - [ ] Level-of-detail (LOD) management
- [ ] Add features:
  - [ ] Min/max zoom constraints
  - [ ] Initial viewport positioning
  - [ ] Viewport changed events
  - [ ] Custom tile source providers
  - [ ] Overlay support (annotations, markers)
- [ ] Performance optimization:
  - [ ] Background tile loading
  - [ ] Render throttling
  - [ ] Memory management
- [ ] Add comprehensive examples:
  - [ ] Basic viewer usage
  - [ ] Custom tile sources
  - [ ] Annotation overlays
  - [ ] Multi-viewer synchronization
- [ ] Create documentation and demos

**New files to create:**
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/Controls/DeepZoomViewer.xaml`
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/TileLoaders/`
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom/Caching/`
- `src/Extensions/OoBDev.Extensions.WPF.DeepZoom.Demo/` (sample app)

##### 5.1.2 JavaScript/TypeScript DeepZoom Viewer Library
- [ ] Create `src/Web/OoBDev.Web.DeepZoom/` (TypeScript library)
- [ ] Design JavaScript/TypeScript API:
  - [ ] DeepZoomViewer class
  - [ ] Configuration options
  - [ ] Event system
  - [ ] Plugin architecture
- [ ] Implement core viewer:
  - [ ] Canvas-based rendering
  - [ ] Touch/mouse/wheel event handling
  - [ ] Smooth zoom animations (CSS transitions or requestAnimationFrame)
  - [ ] Progressive tile loading with preloading
- [ ] Add features:
  - [ ] Responsive design (resize handling)
  - [ ] Mobile-optimized touch gestures
  - [ ] Accessibility (keyboard navigation, ARIA)
  - [ ] SVG overlay support
  - [ ] Custom controls (zoom in/out, home, fullscreen)
- [ ] Framework integrations:
  - [ ] Vanilla JavaScript/TypeScript
  - [ ] React component wrapper
  - [ ] Angular component wrapper (optional)
  - [ ] Vue component wrapper (optional)
- [ ] Build tooling:
  - [ ] TypeScript compilation
  - [ ] Module bundling (ESM, UMD, CommonJS)
  - [ ] Minification
  - [ ] Source maps
- [ ] Add comprehensive examples:
  - [ ] Basic HTML usage
  - [ ] React integration
  - [ ] Custom tile sources
  - [ ] Annotation layers
- [ ] Create documentation site:
  - [ ] API reference
  - [ ] Interactive demos
  - [ ] Getting started guide

**New files to create:**
- `src/Web/OoBDev.Web.DeepZoom/src/DeepZoomViewer.ts`
- `src/Web/OoBDev.Web.DeepZoom/src/TileLoader.ts`
- `src/Web/OoBDev.Web.DeepZoom/src/Cache.ts`
- `src/Web/OoBDev.Web.DeepZoom/react/` (React wrapper)
- `src/Web/OoBDev.Web.DeepZoom/examples/`
- `src/Web/OoBDev.Web.DeepZoom/docs/`

**Package Distribution:**
- NPM package: `@oodev/deepzoom-viewer`
- NuGet package: `OoBDev.Extensions.WPF.DeepZoom`

#### 5.2 Code Cleanup
- [ ] Remove obsolete/unused code
- [ ] Consolidate duplicate utilities
- [ ] Verify all projects have README.md files
- [ ] Ensure consistent coding standards

#### 5.3 Testing
- [ ] Achieve 80%+ code coverage for Framework layer
- [ ] Add integration tests for migrated components
- [ ] Add performance benchmarks where applicable

#### 5.4 Documentation
- [ ] Update all component README.md files
- [ ] Update architecture documentation with new components
- [ ] Create migration notes document
- [ ] Add API documentation examples

#### 5.5 Final Validation
- [ ] Run full test suite
- [ ] Run security audit (use security-audit.md protocol)
- [ ] Run architectural compliance check
- [ ] Update GitVersion.yml if needed

---

## Migration Priorities

**NOTE:** ALL features from BinaryDataDecoders will be migrated. Phases indicate priority order, not feature selection.

### Priority 1: Foundation (Phase 1)
**Why:** Required by other phases, fixes existing gaps
- Endianness support (needed by protocols and binary readers)
- Utility enhancements (quality of life improvements)
- BinaryPrimitives expansion (foundation for all binary operations)

### Priority 2: Core Features (Phase 2)
**Why:** Substantial new capabilities with broad applicability
- CodeAnalysis (valuable for code generation and analysis tools)
- ExpressionCalculator (already partially exists, needs completion)
- Archive support (useful for many scenarios)
- BinaryData enhancements (bit-level operations, checksums)

### Priority 3: Protocols (Phase 3)
**Why:** Self-contained protocol implementations for specific domains
- NMEA (maritime/GPS applications)
- Drawing/Geometry (graphics primitives and operations)

### Priority 4: Specialized Domains (Phase 4)
**Why:** Domain-specific features for specialized use cases, all will be migrated
- FileSystems (ISO 9660 for CD/DVD image access)
- Classic Cryptography (educational/CTF purposes with security warnings)
- Retro Computing (Apple II legacy data recovery, DOS 3.3 support)
- Hardware Devices (8 specialized devices: Busylight, RadexOne, K8055, H4n, Fencing, LANC, EByte, ZWave)
- CLI Tools (4 command-line utilities)
- Platform-specific code (UWP/Framework - migrate or delete based on modern equivalents)

### Priority 5: Finalization (Phase 5)
**Why:** Polish, documentation, and project completion

---

## Known Issues

### Critical (Fixed, Pending Verification)
- [x] PathEx lambda syntax error - FIXED
- [x] StreamDevice nullable annotations - FIXED
- [x] StreamDevice transmission delay typo - FIXED
- [x] SerialPortFactory verbose ternary - FIXED
- [x] ShiftCommutativeVariablesRight stub - FIXED
- [x] ExpressionParserTests floating-point precision - FIXED (added epsilon tolerance)
- [x] NumericAsserts utility created - Migrated to OoBDev.TestUtilities for reuse

### To Investigate
- [ ] Verify ExpressionCalculator optimizers match incoming implementation
- [ ] Verify ANTLR grammar versions match
- [ ] Check for other stub implementations in codebase

---

## Reference Documentation

### Architecture
- `docs/architecture/README.md` - Overview
- `docs/architecture/architectural-guidelines.md` - Principles
- `docs/architecture/architectural-standards.md` - Standards
- `docs/architecture/architectural-patterns.md` - Patterns
- `docs/architecture/layering-architecture.md` - Layer details

### Migration
- `docs/migration/README.md` - Migration overview
- `docs/migration/binarydatadecoders-feature-mapping.md` - Feature comparison
- `docs/migration/binarydatadecoders-migration-plan.md` - Detailed plan

### Protocols
- `.claude/protocols/software/incoming-codebase-comparison.md` - Comparison protocol
- `.claude/protocols/software/architectural-analysis.md` - Architecture protocol
- `.claude/protocols/software/security-audit.md` - Security protocol

---

## Notes

- All new code must follow `docs/architecture/architectural-standards.md`
- All new projects must have README.md (build-enforced)
- Framework layer requires 80%+ test coverage
- Use provider/factory pattern for extensible components
- Follow existing dependency injection patterns
- Maintain .NET 9.0 target framework
- Keep nullable reference types enabled
- Keep implicit usings disabled
