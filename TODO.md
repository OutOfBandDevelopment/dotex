# TODO - OoBDev (dotex) Framework

**Last Updated:** 2026-01-22

✅ **COMPLETED:** Build Warnings Resolution - Reduced from 95+ to 8 warnings (2026-01-22)
✅ **COMPLETED:** Test Category Cleanup - DevLocal tests converted to Integration/Unit/LiveIntegration (2026-01-22)
✅ **COMPLETED:** .runsettings Documentation - Comprehensive how-to guide created (2026-01-22)
✅ **COMPLETED:** Configuration Documentation - Comprehensive CONFIGURATION_SETTINGS.md created (157+ settings)
✅ **COMPLETED:** Ollama Integration Test Setup - Automated model pulling in integration-up scripts
✅ **COMPLETED:** Docker-based Integration Testing Infrastructure (Week 1 & 2) - 14 services + Ollama
✅ **COMPLETED:** Swashbuckle 10.1.0 & .NET 10.0 breaking changes - All fixes verified working
✅ **COMPLETED:** XML Documentation generation - Swagger Summary/Description now appear
✅ **COMPLETED:** Swagger generation tested and verified working
✅ **COMPLETED:** Local Docker testing validation - All Integration tests passing (2026-01-21)

> **New to this project?** Read `/CLAUDE.md` first for a complete development guide including architecture, patterns, and migration scope.

---

## Quick Navigation

This document is organized into **epic-based files** for better navigation and maintenance:

### 🧪 [Local Integration Testing (Docker)](./TODO-testing-local-integration.md)
**Status:** ✅ VALIDATED & COMPLETE - Ready for CI/CD Enablement

Docker-based integration testing with 14 services (Apache Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch, Qdrant, Azurite, LocalStack, Azure Service Bus Emulator, Keycloak, SBert, Ollama). Infrastructure complete, 23 tests migrated and validated.

**Key Tasks:**
- ✅ Docker infrastructure (14 services, compose files, scripts, README with PlantUML)
- ✅ Test category: Integration (Docker-based, runs in CI/CD)
- ✅ Local validation complete - All Integration tests passing (2026-01-21)
- ✅ Week 2: Migrated 23 tests from DevLocal to Integration category (Apache Tika, SMTP/MailKit, MongoDB, RabbitMQ, OpenSearch, SBert, Ollama)
- ✅ Ollama integration: Auto-pull phi3 model in integration-up scripts
- ⏳ **NEXT:** Enable CI/CD pipeline for daily Integration test runs
- ⏳ Week 4: Docker documentation (11 stacks)

### ☁️ [Live Integration Testing (Cloud)](./TODO-testing-live-integration.md)
**Status:** ✅ Category Added - ⏳ Week 3 Migration Pending

Cloud-based integration testing for services requiring live credentials (Azure B2C, Application Insights, Groq Cloud). Manual execution only, NOT run in CI/CD.

**Key Tasks:**
- ✅ Test category: LiveIntegration (cloud services, manual only)
- ⏳ Week 3: Migrate 3 cloud services (Azure B2C, App Insights, Groq)
- ⏳ Week 4: Cloud documentation (credential management, cost management)

### 📦 [Migrations](./TODO-migrations.md)
**Status:** ✅ Analysis Complete - 📋 Individual TODOs Created - Ready to Execute

**📋 Individual Migration TODOs:** [TODO-migrations-index.md](./TODO-migrations-index.md)

All migration work for Incomming projects and BinaryDataDecoders:
- 🗑️ **Incomming/Framework** - CANCELLED: Directory removed from Incoming/
- 🎨 **Incomming/SharedFramework** - REPLACED: Code migration replaced with design-first documentation (120 docs created, 90.9% complete)
- **BinaryDataDecoders** - 5-phase migration (Foundation → Specialized Features)

**Key Accomplishments:**
- ✅ Framework Vector namespace updated to OoBDev.System.Math (files already in main)
- 🗑️ Incoming/Framework directory removed - migration cancelled
- 🎨 SharedFramework replaced with design-first documentation (120 docs, 90.9% complete)
- ✅ SharedFramework Phase 0: .NET 10.0 verified + Namespace cleanup (before pivot)
- ✅ All 8 Incoming projects investigated (100% complete)
- ✅ Strategic pivot: Code migration → comprehensive design documentation

**Completed Migrations:**
- ✅ **Caching (COMPLETE - 2026-01-20)** - 4 implementation + 3 test projects, property chains, Redis Docker integration
  - **Details:** [docs/changes/migration-caching-framework-2026-01-20.md](docs/changes/migration-caching-framework-2026-01-20.md)
- ✅ **Message Queues (COMPLETE - 2026-01-20)** - AWS SQS + Azure Service Bus providers, LocalStack + Azure emulator integration
  - **Details:** See [Features/MessageQueuing/README.md](Features/MessageQueuing/README.md)

**Design Documentation (Replaces SharedFramework Code Migration):**
- 🎨 **120 comprehensive design documents created** (90.9% complete)
  - Epic 11: Data Enhancement (16 docs) ✅
  - Epic 10: Text Templating (12 docs) ✅
  - Epic 12: Message Composition (4 docs) ✅
  - Epic 7: Identity & Session (16 docs) ✅
  - Epic 6: Document Services (44 docs) ✅
  - Epic 4: Distributed Caching (12 docs) ✅
  - Epic 2: Communications (6 docs) 🔄 Partial
  - Epic 5: Master Data (10 docs) 🔄 Partial

**Ready to Migrate:**
- ⏸️ BinaryDataDecoders (decisions required)

### 🐛 [Bug Fixes & Technical Debt](./TODO-bug-fixes.md)
**Status:** 🟢 Mostly Complete - 8 Warnings Remaining

**Active Work:**
- 🟢 **Build Warnings** - 8 remaining (down from 95+), acceptable for now

**Completed:**
- ✅ Build warnings reduced from 95+ to 8 (2026-01-22)
- ✅ Test category cleanup - DevLocal → Integration/Unit/LiveIntegration (2026-01-22)
- ✅ .runsettings documentation - Comprehensive how-to guide (2026-01-22)
- ✅ HtmlNavigatorTests fixed - ComplexTemplate.html + proper assertions (2026-01-22)
- ✅ Redis integration tests fixed - IObjectConverter + JSON serialization (2026-01-22)
- ✅ Phase 0 critical bugs (6 fixes) - All complete
- ✅ MSTest ExpectedExceptionAttribute conversion (40 instances) - Complete
- ✅ Swashbuckle 10.1.0 breaking changes - All 5 files fixed and verified
- ✅ .NET 10.0 breaking changes - ClaimsPrincipal DI + IActionContextAccessor deprecation
- ✅ XML Documentation generation - Enabled globally, Swagger working
- ✅ Build verification: All 65 projects build successfully
- ✅ Swagger verification: Summary and Description properties appear
- ✅ Swagger generation: Tested and verified working

**Ongoing:**
- ⏳ Technical debt: 80%+ test coverage, documentation, dependency audits

### 📝 [Documentation](./TODO-documentation.md)
**Status:** ✅ Configuration Reference Complete - 📋 Ongoing Maintenance

Comprehensive framework documentation including configuration, testing, and architecture:

**Completed:**
- ✅ **.runsettings How-To Guide** (2026-01-22) - Variables and configuration best practices
  - MSBuild variables NOT supported, environment variables + relative paths documented
  - OoBDev Docker testing patterns explained
  - Workarounds for MSBuild variables provided
- ✅ **CONFIGURATION_SETTINGS.md** (2026-01-21) - 157+ configuration points
  - 31 Options classes (FileTemplating, OAuth2, GroqCloud, MongoDB, Ollama, etc.)
  - 24 direct IConfiguration keys (RabbitMQ, ServiceBus, SQS, Redis, SQL Server)
  - 102 environment variables (Runtime, Database, Message Queues, Cloud, AI/ML)
  - Connection string formats, validation rules, best practices, migration guide
- ✅ **TEST_VARIABLES.md** - 30+ test parameters for integration testing
- ✅ **Testing documentation** - Complete testing guidelines and infrastructure docs

**See Also:**
- [docs/how-tos/runsettings-variables-and-configuration.md](./docs/how-tos/runsettings-variables-and-configuration.md) - .runsettings guide
- [CONFIGURATION_SETTINGS.md](./CONFIGURATION_SETTINGS.md) - Configuration reference
- [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Test property reference

### ❓ [Decisions Required](./TODO-decisions.md)
**Status:** ⏸️ Multiple Migrations Blocked - Awaiting Strategic Decisions

Critical decisions blocking migration work:
- **BinaryDataDecoders** - Endianness API design, CodeAnalysis use cases, archive formats, hardware devices
- **BotChat** - Choose: Archive, Enhance, or Extract patterns (RECOMMEND: Archive)
- ✅ **Oobtainium** - RESOLVED: Moved to proving-grounds repository

**Key Decisions:**
- ⏸️ BotChat: Sample application (Option 1 ARCHIVE recommended)
- ⏸️ BinaryDataDecoders: 14+ questions across 4 priority levels
- ✅ Oobtainium: RESOLVED - Moved to separate code playground repository

---

## Epic Files Structure

```
TODO.md                                  # This file - Index and navigation
├── TODO-testing-local-integration.md    # Docker-based integration testing (11 services)
├── TODO-testing-live-integration.md     # Cloud-based integration testing (3 services)
├── TODO-migrations.md                   # All Incomming/ and BinaryDataDecoders migrations
├── TODO-bug-fixes.md                    # Bug fixes and technical debt
├── TODO-decisions.md                    # Pending strategic decisions
└── TEST_VARIABLES.md                    # All test properties and configuration variables
```

---

## Migration Scope Philosophy

**ALL features from BinaryDataDecoders will be migrated** - phases indicate priority order, not feature selection.

### What Gets Migrated
✅ Core features (obviously)
✅ Highly specialized features (ISO 9660, hardware devices)
✅ Niche features (Apple II, retro computing, fencing equipment)
✅ Educational features (classic cryptography with warnings)
✅ Incomplete features (migrate as-is, track TODOs)
✅ UI components (Windows Forms validation controls)

### What Gets Deleted (Very Rare)
❌ Stub projects with zero implementation (e.g., Rigol)
❌ Silverlight-only or obsolete platform code
❌ Features with no .NET 10.0 equivalent

### Migration Principles
1. Phases indicate **priority order**, not feature selection
2. Even incomplete features are migrated and tracked
3. Specialized features packaged separately for target audiences
4. Security warnings added where appropriate (e.g., broken ciphers)
5. All code must follow architectural standards

---

## Completed Work Summary

### Architecture Documentation ✅
- Created complete architecture documentation (5 documents)
- Established 14 core architectural principles
- Documented enforceable standards and pattern catalog
- Created layer-by-layer breakdown

### Migration Investigations ✅ COMPLETE

**All 8 Incoming Projects Investigated:**
- ✅ **dotnet-lib** (2026-01-12) - 95% identical, critical bug fix applied, DELETED
- 🗑️ **Framework** - Vector library namespace updated, directory removed from Incoming/
- ✅ **Oobtainium** - Mocking framework analyzed, moved to proving-grounds repository
- ✅ **BotChat** - Sample app analyzed, 3 options documented, awaiting decision
- 🎨 **SharedFramework** - Replaced with 120 design documents (90.9% complete), directory removed
- ✅ **BinaryDataDecoders** - Complete feature mapping, 5-phase plan, decision points identified
- ✅ **ContractParser** (2026-01-20) - Feature specification created in Features/ContractParser/
- ✅ **Tools** (2026-01-20) - 4 CLI tools analyzed in Features/Tools/

**Tracking:** See `Incomming/CHECKLIST.md` for detailed investigation status

### Testing Infrastructure ✅
- ✅ Week 1 & 2 Complete (2026-01-19): Docker infrastructure, test categories, CI/CD pipeline, 19 tests migrated
- ✅ **Local Integration (Docker):** 13-service stack with health checks, ephemeral volumes, isolated network
- ✅ **Live Integration (Cloud):** Added LiveIntegration category for cloud-only services
- ✅ **Test Variables:** Documented all 30+ test properties in [TEST_VARIABLES.md](./TEST_VARIABLES.md)
- ✅ Complete CI/CD workflow (DISABLED until local testing validates)
- ✅ Comprehensive documentation with PlantUML deployment diagram
- ✅ Split into 2 epics: Local (Docker) and Live (Cloud) integration testing

### Bug Fixes ✅
- ✅ Phase 0: Critical bug fixes (6 fixes) - All complete
- ✅ MSTest ExpectedExceptionAttribute conversion (40 instances across 24 files)
- ✅ Swashbuckle 10.1.0 breaking changes - All 5 files fixed and verified
- ✅ .NET 10.0 breaking changes - ClaimsPrincipal DI + IActionContextAccessor deprecation
- ✅ XML Documentation - Enabled globally, Swagger working

### Protocols & Documentation ✅
- Created `.claude/protocols/software/incoming-codebase-comparison.md` (v1.1)
- Updated protocol with checklist management and project type classifications
- Created comprehensive migration documentation for all investigations

### CI/CD ✅
- Updated `.github/workflows/dotnet.yml` - Added path filters
- Fixed build tag format (2026-01-20) - Removed duplicate branch name in tags
- Completed `.github/workflows/integration-tests.yml` - DISABLED pending validation

---

## Active Priorities

### Immediate (This Week)
1. **Local Docker Testing** ⏳ CRITICAL
   - Follow `/containers/testing/TESTING-CHECKLIST.md`
   - Verify all 11 services become healthy
   - Test cleanup and restart
   - Enable CI/CD workflow after validation

2. **OoBDev.AspNetCore.Mvc - Swashbuckle & .NET 10.0 Fixes** ✅ COMPLETE
   - Swashbuckle 10.1.0: 4 files fixed (FormFileOperationFilter, HealthChecksDocumentFilter, SearchQueryOperationFilter, ApplicationPermissionsApiFilter)
   - .NET 10.0: 1 file fixed (ServiceCollectionExtensions - ClaimsPrincipal DI, IActionContextAccessor deprecation)
   - XML Documentation: Enabled globally, removed overrides in 2 projects (AspNetCore.Mvc, WebApi)
   - Build verified: All 65 projects built successfully ✅
   - Swagger verified: Summary and Description properties now appear ✅
   - Swagger generation: Tested and verified working ✅

3. **Strategic Decisions** ⏸️ BLOCKING
   - Decide on Oobtainium migration (RECOMMEND: Delete)
   - Decide on BotChat migration (RECOMMEND: Archive)
   - Review BinaryDataDecoders decision points

### Short Term (Next 2 Weeks)
1. **Live Integration Test Migration (Week 3)** ⏳ PENDING
   - Azure B2C tests - Move from DevLocal to LiveIntegration
   - Application Insights tests - Move to LiveIntegration
   - Groq Cloud tests - Move to LiveIntegration
   - Create .env.liveintegration.template files
   - Document cloud credential setup

2. **Testing Documentation (Week 4)** ⏳ PENDING
   - Create 11 stack-specific documentation files
   - Document each Docker service (MongoDB, RabbitMQ, etc.)
   - Add PlantUML diagrams for service architecture
   - Create LiveIntegration setup guides
   - Update SemanticKernel integration

3. **SharedFramework Phase 0 Namespace Cleanup** ✅ COMPLETE (2026-01-20)
   - Removed "Api." prefix from all namespaces (14 projects)
   - Moved Azure under Microsoft hierarchy (4 projects)
   - Renamed .Contracts to .Abstractions (9 projects, deleted 1 application-specific)
   - Updated 87+ namespace declarations and 37+ using statements
   - Renamed all 27 directories (14 + 4 + 9)
   - Verified no broken references

### Medium Term (Next Month)
1. **SharedFramework Migration Strategy** ⚠️ DECISIONS NEEDED (Coverage analysis complete)
   - ✅ Phase 0 complete (.NET 10.0 + namespace cleanup)
   - ⚠️ **5 DIFFERS projects require merge decisions:**
     - Communications (16 LOC stub → 1,145 LOC implementation) - **CRITICAL**
     - Communications.Abstractions (125 LOC → 695 LOC) - **CRITICAL**
     - Documents vs DocumentCenter (531 LOC vs 911 LOC)
     - Identity vs IdentityModel (125 LOC vs 291 LOC)
     - TextTemplating (scattered vs unified)
   - ✅ **19 NEW projects ready to migrate** (no conflicts):
     - Caching (4 projects) - HIGH PRIORITY
     - Message Queueing Providers: AWS SQS, Azure Service Bus
     - Spatial Services (5 projects): Geocoding, Maps
     - Complex Events, Data Loader, Generations
   - **See:** [Coverage Analysis](docs/migration/sharedframework-phase0-coverage-analysis.md)

2. **Feature Implementation Decisions** ⏸️ PENDING
   - ContractParser - Decide: Implement now, later, or keep as specification
   - Tools (BulkLlm) - Decide: Consolidate into unified LlmCodeGen tool

3. **BinaryDataDecoders Phase 1** ⏸️ BLOCKED (Awaiting decisions)
   - Endianness support enhancements
   - Utility enhancements
   - BinaryPrimitives expansion

---

## Reference Documentation

### Architecture
- `docs/architecture/README.md` - Overview
- `docs/architecture/architectural-guidelines.md` - 14 principles
- `docs/architecture/architectural-standards.md` - Enforceable standards
- `docs/architecture/architectural-patterns.md` - Pattern catalog
- `docs/architecture/layering-architecture.md` - Layer details

### Migration
- `docs/migration/README.md` - Migration overview
- `docs/migration/binarydatadecoders-feature-mapping.md` - Feature comparison
- `docs/migration/binarydatadecoders-migration-plan.md` - Detailed plan
- `docs/migration/binarydatadecoders-critical-questions.md` - Decision matrix
- `docs/migration/framework-feature-mapping.md` - Framework analysis
- `docs/migration/vector-comparison.md` - Vector library comparison
- `docs/migration/sharedframework-feature-mapping.md` - 52 projects analyzed
- `docs/migration/sharedframework-migration-plan.md` - 12-phase plan
- `docs/migration/sharedframework-phase0-coverage-analysis.md` - Overlap analysis with main codebase
- `docs/migration/sharedframework-differs-detailed-comparison.md` - **NEW:** Detailed DIFFERS project comparison
- `docs/migration/oobtainium-feature-mapping.md` - Mocking framework analysis
- `docs/migration/oobtainium-migration-plan.md` - 4 migration options
- `docs/migration/botchat-feature-mapping.md` - Sample app analysis
- `docs/migration/botchat-migration-plan.md` - 3 migration options

### Testing
- `TEST_VARIABLES.md` - All test properties reference (30+ variables)
- `docs/architecture/testing-guidelines.md` - Comprehensive testing best practices
- `containers/testing/README.md` - Infrastructure guide with PlantUML
- `containers/testing/TESTING-CHECKLIST.md` - Local validation procedure
- `containers/testing/STATUS.md` - Implementation progress tracker
- `Incomming/CHECKLIST.md` - Investigation tracking

### Change History
- `docs/changes/README.md` - Archived completed work (reduces context overhead)
- `docs/changes/documentation-configuration-settings-2026-01-21.md` - Configuration documentation (157+ settings)
- `docs/changes/testing-ollama-integration-2026-01-21.md` - Ollama integration (4 tests, auto-setup)
- `docs/changes/sharedframework-phase0-2026-01-20.md` - Namespace cleanup (27 directories)
- `docs/changes/migration-message-queues-2026-01-20.md` - AWS SQS + Azure Service Bus
- `docs/changes/testing-docker-infrastructure-2026-01-19.md` - Docker testing (14 services, 23 tests)

### Protocols
- `.claude/protocols/software/incoming-codebase-comparison.md` - v1.1
- `.claude/protocols/software/architectural-analysis.md` - Architecture review
- `.claude/protocols/software/security-audit.md` - Security protocol

---

## Notes

- All new code must follow `docs/architecture/architectural-standards.md`
- All new projects must have README.md (build-enforced)
- Framework layer requires 80%+ test coverage
- Use provider/factory pattern for extensible components
- Follow existing dependency injection patterns
- Maintain .NET 10.0 target framework
- Keep nullable reference types enabled
- Keep implicit usings disabled

---

## Quick Commands

```bash
# Build entire solution
dotnet build src/

# Run all tests
dotnet test src/

# Run specific test categories
dotnet test src/ --filter "TestCategory=Unit"
dotnet test src/ --filter "TestCategory=Simulate"
dotnet test src/ --filter "TestCategory=Integration"

# Run Integration tests with custom settings
dotnet test src/ --settings integration.runsettings --filter "TestCategory=Integration"

# Start integration test services
cd containers/testing
./scripts/integration-up.sh --wait

# Stop integration test services
./scripts/integration-down.sh --clean

# Run integration tests (after services started)
dotnet test src/ --filter TestCategory=Integration
```

---

**For detailed information on each epic, see the respective TODO-{epic}.md files above.**
