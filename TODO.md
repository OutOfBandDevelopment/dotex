# TODO - OoBDev (dotex) Framework

**Last Updated:** 2026-01-20

✅ **COMPLETED:** Docker-based Integration Testing Infrastructure (Week 1 & 2)
✅ **COMPLETED:** Swashbuckle 10.1.0 & .NET 10.0 breaking changes - All fixes verified working
✅ **COMPLETED:** XML Documentation generation - Swagger Summary/Description now appear
⏳ **AWAITING:** Local Docker testing validation before enabling CI/CD
⏳ **AWAITING:** Test verification for OoBDev.AspNetCore.Mvc

> **New to this project?** Read `/CLAUDE.md` first for a complete development guide including architecture, patterns, and migration scope.

---

## Quick Navigation

This document is organized into **epic-based files** for better navigation and maintenance:

### 🧪 [Local Integration Testing (Docker)](./TODO-testing-local-integration.md)
**Status:** ✅ Week 1 & 2 Complete - ⏳ Awaiting Local Validation

Docker-based integration testing with 11 services (Apache Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, OpenSearch, Qdrant, Azurite, LocalStack, Keycloak, SBert). Infrastructure complete, 19 tests migrated.

**Key Tasks:**
- ✅ Docker infrastructure (11 services, compose files, scripts, README with PlantUML)
- ✅ Test category: Integration (Docker-based, runs in CI/CD)
- ✅ CI/CD pipeline (complete but DISABLED until validation)
- ✅ Week 2: Migrated 19 tests from DevLocal to Integration category
- ⏳ **NEXT:** Local testing with TESTING-CHECKLIST.md
- ⏳ Week 4: Docker documentation (11 stacks)

### ☁️ [Live Integration Testing (Cloud)](./TODO-testing-live-integration.md)
**Status:** ✅ Category Added - ⏳ Week 3 Migration Pending

Cloud-based integration testing for services requiring live credentials (Azure B2C, Application Insights, Groq Cloud). Manual execution only, NOT run in CI/CD.

**Key Tasks:**
- ✅ Test category: LiveIntegration (cloud services, manual only)
- ⏳ Week 3: Migrate 3 cloud services (Azure B2C, App Insights, Groq)
- ⏳ Week 4: Cloud documentation (credential management, cost management)

### 📦 [Migrations](./TODO-migrations.md)
**Status:** ✅ Vector & SharedFramework .NET Complete - ⏸️ Multiple Phases Blocked by Decisions

All migration work for Incomming projects and BinaryDataDecoders:
- **Incomming/Framework** - 55 files (30 NEW + 25 DIFFERS) requiring systematic comparison
- **Incomming/SharedFramework** - 52 projects (~28,582 LOC) with 12-phase migration plan
- **BinaryDataDecoders** - 5-phase migration (Foundation → Specialized Features)

**Key Tasks:**
- ✅ Framework Vector namespace updated to OoBDev.System.Math (files already in main)
- ✅ SharedFramework Phase 0: All 51 projects at .NET 10.0, 1 SQL project at netstandard2.0 (verified complete)
- ⏸️ SharedFramework Phase 2: Communications merge (CRITICAL - 1,145 LOC vs 16 LOC stub)
- ⏸️ BinaryDataDecoders: Awaiting critical decisions (see TODO-decisions.md)

### 🐛 [Bug Fixes & Technical Debt](./TODO-bug-fixes.md)
**Status:** ✅ VERIFIED COMPLETE

Bug fixes, breaking changes, and technical debt resolution:
- ✅ Phase 0 critical bugs (6 fixes) - All complete
- ✅ MSTest ExpectedExceptionAttribute conversion (40 instances) - Complete
- ✅ Swashbuckle 10.1.0 breaking changes - All 5 files fixed and verified
- ✅ .NET 10.0 breaking changes - ClaimsPrincipal DI + IActionContextAccessor deprecation
- ✅ XML Documentation generation - Enabled globally, Swagger working

**Completed:**
- ✅ Build verification: All 65 projects build successfully
- ✅ Swagger verification: Summary and Description properties appear
- ⏳ Test verification: `dotnet test src/ --filter "OoBDev.AspNetCore.Mvc"`
- ⏳ Technical debt: 80%+ test coverage, documentation, dependency audits

### ❓ [Decisions Required](./TODO-decisions.md)
**Status:** ⏸️ Multiple Migrations Blocked - Awaiting Strategic Decisions

Critical decisions blocking migration work:
- **BinaryDataDecoders** - Endianness API design, CodeAnalysis use cases, archive formats, hardware devices
- **Oobtainium** - Choose: Migrate, Reference, Archive, or Delete (RECOMMEND: Delete)
- **BotChat** - Choose: Archive, Enhance, or Extract patterns (RECOMMEND: Archive)

**Key Decisions:**
- ⏸️ Oobtainium: Mocking framework (Option 4 DELETE recommended)
- ⏸️ BotChat: Sample application (Option 1 ARCHIVE recommended)
- ⏸️ BinaryDataDecoders: 14+ questions across 4 priority levels

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
❌ Features with no .NET 9.0 equivalent

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

### Migration Investigations ✅

**Completed & Archived:**
- ✅ **dotnet-lib** (2026-01-12) - 95% identical, critical bug fix applied, DELETED
- ✅ **Framework** - 55 files analyzed (30 NEW + 25 DIFFERS), Vector library ready
- ✅ **Oobtainium** - Mocking framework analyzed, 4 options documented, awaiting decision
- ✅ **BotChat** - Sample app analyzed, 3 options documented, awaiting decision
- ✅ **SharedFramework** - 52 projects analyzed (~28,582 LOC), 12-phase plan created
- ✅ **BinaryDataDecoders** - Complete feature mapping, 5-phase plan, decision points identified

**In Progress:**
- 🔍 **CloudOrchestrator** - Not yet started
- 🔍 **ContractParser** - Not yet started
- 🔍 **Tools** - Not yet started

**Tracking:** See `Incomming/CHECKLIST.md` for detailed investigation status

### Testing Infrastructure ✅
- ✅ Week 1 & 2 Complete (2026-01-19): Docker infrastructure, test categories, CI/CD pipeline, 19 tests migrated
- ✅ **Local Integration (Docker):** 11-service stack with health checks, ephemeral volumes, isolated network
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
- Completed `.github/workflows/integration-tests.yml` - DISABLED pending validation

---

## Active Priorities

### Immediate (This Week)
1. **Local Docker Testing** ⏳ CRITICAL
   - Follow `/containers/testing/TESTING-CHECKLIST.md`
   - Verify all 11 services become healthy
   - Test cleanup and restart
   - Enable CI/CD workflow after validation

2. **OoBDev.AspNetCore.Mvc - Swashbuckle & .NET 10.0 Fixes** ✅ VERIFIED WORKING - ⏳ AWAITING TESTS
   - Swashbuckle 10.1.0: 4 files fixed (FormFileOperationFilter, HealthChecksDocumentFilter, SearchQueryOperationFilter, ApplicationPermissionsApiFilter)
   - .NET 10.0: 1 file fixed (ServiceCollectionExtensions - ClaimsPrincipal DI, IActionContextAccessor deprecation)
   - XML Documentation: Enabled globally, removed overrides in 2 projects (AspNetCore.Mvc, WebApi)
   - Build verified: All 65 projects built successfully ✅
   - Swagger verified: Summary and Description properties now appear ✅
   - Remaining: Run tests to ensure no regressions

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

3. **SharedFramework Namespace Cleanup** ⏳ PENDING
   - Remove "Api." prefix (e.g., OoBDev.Api.Twilio → OoBDev.Twilio)
   - Update all using statements
   - Verify no broken references

### Medium Term (Next Month)
1. **SharedFramework Phase 2** ⏸️ CRITICAL (Awaiting Phase 0 namespace cleanup)
   - Merge OoBDev.Communications (1,145 LOC vs 16 LOC stub)
   - Migrate Twilio SendGrid and SMS providers
   - Complete communications implementation

2. **Incomming Project Investigations** 🔍 IN PROGRESS (3 remaining)
   - CloudOrchestrator - Not yet started
   - ContractParser - Not yet started
   - Tools - Not yet started

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
- Maintain .NET 9.0 target framework
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
