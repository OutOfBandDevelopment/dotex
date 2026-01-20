# OoBDev (dotex) Framework - Claude Development Guide

**Last Updated:** 2026-01-20
**Framework:** OoBDev (dotex) - Enterprise .NET Library Suite
**Target:** net10.0
**Current Work:** Swashbuckle 10.1.0 & .NET 10.0 fixes VERIFIED COMPLETE | Docker testing infrastructure awaiting local validation

---

## Overview

OoBDev is a comprehensive collection of .NET framework extensions at various utility levels, providing:
- Binary data processing and device communication
- Code analysis and expression evaluation
- File I/O, networking, and protocols
- Template engines and document generation
- Database access and data management
- UI components (Windows Forms, WPF)
- Specialized hardware and legacy system support
- Desktop and server applications support

**Key Stats:**
- 112+ projects across 4 architectural layers
- net10.0 target framework
- MSTest with 80%+ coverage requirement
- Comprehensive architectural documentation

---

## Quick Start for Claude

### 1. Before Starting Any Work

**Read First:**
- `/TODO.md` - Current migration status and pending work
- `/docs/architecture/README.md` - Architecture overview
- `/docs/architecture/architectural-standards.md` - Enforceable coding standards

**Key Principles:**
- ALL features from BinaryDataDecoders will be migrated (phases = priority order, not selection)
- Follow provider/factory pattern for extensibility
- Use dependency injection (TryAdd* extensions)
- 80% test coverage for Framework layer
- README.md required for all projects (build-enforced)
- Nullable enabled, ImplicitUsings disabled

### 2. Available Protocols

Located in `.claude/protocols/`:

#### Software Analysis
- **architectural-analysis.md** - Systematic architecture documentation
- **incoming-codebase-comparison.md** (v1.1) - Compare incoming code with main codebase
  - Updated with project type classifications, checklist management, and TODO.md integration
- **security-audit.md** - API security audit
- **protocol-validation.md** - Protocol quality assurance

#### Documentation
- **documentation-style-guide.md** - Content standards
- **documentation-standards.md** - File organization
- **change-documentation-archival.md** - Archive completed work to reduce context overhead

#### Code Generation
- **template-development.md** - Template-based code generation
- **template-swagger-documentation.md** - Template/OpenAPI maintenance

#### Component Standards
- **schema-integration.md** - Schema framework integration
- **datagrid-style-guide.md** - DataGrid component standards

### 3. Current Migration Work

**Incomming Project Investigations** - ✅ COMPLETE (6 of 6 = 100%)
- ✅ dotnet-lib - Completed and deleted (95% identical to main)
- ✅ Framework - Investigated (55 files, Phase 0 comparison required)
- ✅ Oobtainium - RESOLVED: Moved to proving-grounds repository (https://github.com/mwwhited/proving-grounds)
- ✅ BotChat - Investigated (sample app, awaiting decision)
- ✅ SharedFramework - Investigated (52 projects, migration plan ready)
- ✅ BinaryDecoders - Investigated (awaiting critical decisions)
- ✅ ContractParser - Investigated (feature specification in Features/ContractParser/)
- ✅ Tools - Investigated (4 CLI tools analyzed in Features/Tools/)

**Tracking:** See `Incomming/CHECKLIST.md` for detailed status

**BinaryDataDecoders Migration** - ⏸️ BLOCKED (Awaiting decisions)

**Phase 0: Critical Bug Fixes** - ✅ COMPLETED
- All 6 critical bugs fixed and tested
- Build verification passed

**Phase 1: Foundation Enhancement** - ⏸️ PENDING (Awaiting decisions)
- Endianness support improvements
- Utility enhancements
- BinaryPrimitives expansion

**Framework Vector Math Migration** - ✅ NAMESPACE UPDATE COMPLETE
- ✅ Vector files already in main codebase at `src/Framework/OoBDev.System.Abstractions/Math/`
- ✅ Namespace updated from `OoBDev.Common.Math` → `OoBDev.System.Math` (5 files)
- ✅ No migration needed - files already integrated

**SharedFramework Migration** - ✅ PHASE 0 COMPLETE
- ✅ 51 projects at .NET 10.0 (1 SQL project at netstandard2.0)
- ✅ Namespace cleanup complete (27 directories renamed)
  - 14 Api. prefix removals
  - 4 Azure reorganizations (moved under Microsoft hierarchy)
  - 9 Contracts → Abstractions renames
- 52 projects analyzed and categorized
- 12-phase migration plan created
- Ready for Phase 1: Message Queueing Providers
- Critical: Communications merge (1,145 LOC vs 16 LOC stub)

---

## Architecture Layers

### Common Layer (6 projects)
Foundation abstractions and interfaces
- No external dependencies
- Pure interfaces and contracts

### Framework Layer (39 projects)
Core functionality implementations
- Depends only on Common
- 80%+ test coverage required
- README.md required (build-enforced)

### Extensions Layer (6 projects)
Optional enhancements and integrations
- Depends on Framework
- Domain-specific features
- Package separately

### ExternalServices Layer (40+ projects)
Third-party integrations
- Azure, AWS, Google Cloud
- Database providers
- External APIs

---

## Key Patterns

### 1. Provider/Factory Pattern
```csharp
IService → IServiceProvider → IServiceProviderFactory
```

### 2. Dependency Injection
```csharp
services.TryAddSingleton<IService, ServiceImpl>();
services.AddServiceProvider(); // Extension method
```

### 3. Options Pattern
```csharp
services.Configure<ServiceOptions>(options => { });
```

### 4. Handler Pattern
```csharp
public interface IHandler<TRequest, TResponse>
{
    Task<TResponse> HandleAsync(TRequest request, CancellationToken ct);
}
```

---

## Migration Scope

**ALL features from BinaryDataDecoders will be migrated.**

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
❌ Features with no net10.0 equivalent

### Migration Principles
1. Phases indicate **priority order**, not feature selection
2. Even incomplete features are migrated and tracked in TODO.md
3. Specialized features packaged separately for target audiences
4. Security warnings added where appropriate (e.g., broken ciphers)
5. All code must follow architectural standards

---

## Coding Standards

### File Structure
```
OoBDev.{Layer}.{Feature}/
├── README.md (REQUIRED - build fails without it)
├── {Feature}.csproj
├── Abstractions/ (interfaces)
├── Implementations/
├── Extensions/ (DI registration)
└── Tests/ (separate project)
```

### Naming Conventions
- Interfaces: `I{Name}`
- Implementations: `{Name}` (no suffix)
- Providers: `{Name}Provider`
- Factories: `{Name}Factory`
- Extensions: `{Name}Extensions`

### Code Style
- Nullable enabled
- ImplicitUsings disabled (explicit using statements)
- XML documentation on public APIs
- Target framework: net10.0
- No breaking changes to existing OoBDev APIs

### Testing
- MSTest framework
- 80% coverage minimum for Framework layer
- Test categories: Unit, Simulate, Integration, DevLocal
- Use `NumericAsserts.AreSimilar()` for floating-point comparisons

---

## Common Tasks

### Starting New Migration Phase
1. Read `/TODO.md` phase section
2. Review architecture docs for layer placement
3. Create project structure following standards
4. Implement using provider/factory pattern
5. Add comprehensive tests
6. Create README.md with usage examples
7. Update TODO.md progress

### Fixing Bugs
1. Read bug description in migration docs
2. Verify current code state
3. Apply fix following architectural standards
4. Add/update tests
5. Verify all tests pass
6. Update TODO.md

### Adding Features
1. Check if similar feature exists
2. Determine correct architectural layer
3. Design using appropriate pattern
4. Follow dependency injection guidelines
5. Add tests (80%+ coverage)
6. Document in README.md
7. Update TODO.md

### Creating Documentation
1. Follow documentation standards protocol
2. Use templates from existing docs
3. Include code examples
4. Link to related documentation
5. Add to appropriate index/README

### Archiving Completed Work
1. **Trigger:** "clean up your task list" or when TODO files > 400 lines
2. Follow `.claude/protocols/documentation/change-documentation-archival.md`
3. Create change document in `docs/changes/`
4. Update TODO files with summary + link
5. Update CLAUDE.md "Recently Completed Work"
6. Update `docs/changes/README.md` index
7. Reduces context overhead by 30-50%

---

## Important Files

### Documentation
- `/docs/architecture/` - Complete architecture documentation
- `/docs/migration/` - Migration plans and feature mappings
- `/TODO.md` - Current work tracking
- `/Incomming/CHECKLIST.md` - Incomming project investigation status

### Configuration
- `/src/GitVersion.yml` - Semantic versioning
- `/src/.runsettings` - Test configuration
- `/.github/workflows/dotnet.yml` - CI/CD pipeline

### Protocols
- `/.claude/protocols/software/` - Software development protocols
- `/.claude/protocols/documentation/` - Documentation protocols
  - `change-documentation-archival.md` - Archive completed work (use "clean up your task list")

---

## Testing Guidelines

### Test Categories

OoBDev uses **5 test categories** to organize tests by execution environment and dependencies:

```csharp
[TestCategory(TestCategories.Unit)]            // Fast, isolated, no external dependencies
[TestCategory(TestCategories.Simulate)]        // Full stack, mocked persistence
[TestCategory(TestCategories.Integration)]     // Docker-based external services (NEW)
[TestCategory(TestCategories.DevLocal)]        // Manual/exploratory testing only
[TestCategory(TestCategories.LiveIntegration)] // Cloud services only (NEW)
```

**Category Definitions:**

| Category | Runs In CI/CD | External Services | Use Case |
|----------|---------------|-------------------|----------|
| **Unit** | YES (every PR/push) | Mocked | Pure logic, < 100ms |
| **Simulate** | YES (every PR/push) | Mocked | End-to-end with in-memory persistence |
| **Integration** | YES (daily at 4 PM UTC) | Docker containers | MongoDB, SQL Server, RabbitMQ, etc. |
| **DevLocal** | NO (manual only) | Local services | Performance tests, GPU tests |
| **LiveIntegration** | NO (manual only) | Live Azure/AWS/GCP | Azure B2C, Groq, App Insights |

**Docker-Based Integration Tests:**

Integration tests run against **12 Docker services** managed by the testing infrastructure:

```bash
# Start Docker services for integration testing
cd containers/testing
./scripts/integration-up.sh --wait

# Run integration tests
cd ../../src
dotnet test --filter "TestCategory=Integration"

# Stop and cleanup
cd ../containers/testing
./scripts/integration-down.sh --clean
```

**Services Available:**
- Apache Tika (Document processing)
- SMTP4Dev (Email testing)
- MongoDB (NoSQL database)
- SQL Server (Relational database)
- RabbitMQ (Message queue)
- Redis (Cache store)
- OpenSearch (Search engine)
- Qdrant (Vector database)
- Azurite (Azure Storage emulator)
- LocalStack (AWS emulator)
- Keycloak (Identity & Access Management)
- SBert (Sentence embeddings)

**Test Properties Pattern:**
```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task TestMongoDBOperation()
{
    // ✅ CORRECT: Use TestContext.GetProperty<T>()
    var connectionString = TestContext.GetProperty<string>("MONGODB_CONNECTION_STRING")
        ?? "mongodb://localhost:27017";
    var databaseName = $"IntegrationTest_{Guid.NewGuid():N}";  // Unique per run

    // ... test logic

    // Cleanup in [TestCleanup]
    await client.DropDatabaseAsync(databaseName);
}
```

**IMPORTANT:** Always use `TestContext.GetProperty<T>()` instead of `Environment.GetEnvironmentVariable()` for test configuration. This integrates with `.runsettings` files and MSTest infrastructure.

**Test Configuration:**
- **All Variables:** See [TEST_VARIABLES.md](./TEST_VARIABLES.md) for complete list of 30+ test properties
- **30+ Properties:** MongoDB, SQL Server, RabbitMQ, OpenSearch, SBert, Azure B2C, Groq, etc.
- **Configuration:** Use `.runsettings` file or test deployment context

**See Also:**
- [TEST_VARIABLES.md](./TEST_VARIABLES.md) - Complete test property reference
- `/containers/testing/README.md` - Complete Docker infrastructure guide
- `/containers/testing/TESTING-CHECKLIST.md` - Local validation steps
- `/containers/testing/STATUS.md` - Implementation progress

### Numeric Assertions
```csharp
// For floating-point comparisons (handles rounding differences)
NumericAsserts.AreSimilar(expected, actual);
NumericAsserts.AreSimilar(expected, actual, tolerance);
```

### Test Structure
```csharp
[TestMethod]
public void MethodName_Scenario_ExpectedBehavior()
{
    // Stage (arrange)

    // Mock (if needed)

    // Test (act)

    // Assert

    // Verify (if using mocks)
}
```

---

## Git Workflow

### Commits
- Only create when user explicitly requests
- Follow security protocol (no force push, no amend unless specific conditions)
- Co-author: `Claude Sonnet 4.5 <noreply@anthropic.com>`

### Pull Requests
- Use `gh pr create` for GitHub PRs
- Include summary of all commits (not just latest)
- Add test plan checklist

---

## Build and Test

### Commands
```bash
# Build entire solution
dotnet build src/

# Run all tests
dotnet test src/

# Run specific test category
dotnet test src/ --filter TestCategory=Unit

# Check code coverage
dotnet test src/ --collect:"XPlat Code Coverage"
```

### Pipeline
- Triggers on `src/**/*.cs` changes only
- Builds on both push and PR
- Runs all tests with coverage
- Uses .runsettings for configuration

---

## Package Structure

### Core Packages
- `OoBDev.System` - Core system utilities
- `OoBDev.Extensions` - General extensions
- `OoBDev.IO.*` - I/O and device communication

### Specialized Packages (Separate)
- `OoBDev.Security.Cryptography.Classic` - Educational only
- `OoBDev.Extensions.Hardware.*` - Specialized devices
- `OoBDev.Retro.Apple2` - Legacy computing
- `OoBDev.Extensions.Windows.Forms` - Windows Forms UI components
- `OoBDev.Extensions.Drawing.*` - Graphics and barcode generation

---

## When in Doubt

1. **Check TODO.md** for current priorities
2. **Read architecture docs** for patterns
3. **Look at similar existing projects** for examples
4. **Follow the protocol** for the task type
5. **Ask user** if requirements unclear

---

## Migration Philosophy

> "We maintain ALL features, even highly specialized ones, to preserve the complete functionality of BinaryDataDecoders. Phases indicate priority order for migration work, not a selection of which features to keep. Incomplete features are migrated and tracked for future completion."

**Remember:** The goal is complete feature parity with improvements, not selective cherry-picking.

---

## Recently Completed Work

### ✅ Caching Framework Migration (2026-01-20)

**Status:** ✅ COMPLETE - All 7 phases done, building successfully

Successfully migrated entire Caching framework from SharedFramework to main codebase:

**Implementation Projects (4):**
- ✅ OoBDev.Caching.Abstractions - Interfaces, attributes (`[IsCacheable]`, `[FlushCache]`)
- ✅ OoBDev.Caching - Core factory, proxy, manager implementations
- ✅ OoBDev.Redis.Caching - Distributed caching with StackExchange.Redis
- ✅ OoBDev.Microsoft.Caching - In-memory caching with IMemoryCache

**Test Projects (3):**
- ✅ OoBDev.Caching.Tests - 7 test files (Unit, Simulate)
- ✅ OoBDev.Redis.Caching.Tests - 2 test files (Unit, DevLocal)
- ✅ OoBDev.Microsoft.Caching.Tests - 1 test file (Simulate)

**Documentation (5 files):**
- ✅ docs/architecture/caching/README.md - Overview, quick start
- ✅ docs/architecture/caching/architecture.md - Component design, patterns
- ✅ docs/architecture/caching/providers.md - Provider pattern guide
- ✅ docs/architecture/caching/configuration.md - Configuration reference
- ✅ docs/architecture/caching/testing.md - Testing strategies

**Key Changes:**
- Namespace updates: `Contracts` → `Abstractions`, `Toolkit.Common` → `System.ComponentModel`
- Removed `IRegistrar` interface (simplified to regular classes)
- Added RootNamespace property: `OoBDev.Caching` (removes `.Abstractions`)
- All projects added to solution and building successfully

**Deliverables:** ~600 LOC implementation + 3 test projects + 15,000 words documentation

**Details:** [docs/changes/migration-caching-framework-2026-01-20.md](docs/changes/migration-caching-framework-2026-01-20.md)

---

### Task: Docker-Based Integration Testing Infrastructure (Week 1 & 2 - 2026-01-19)
**Status:** ✅ COMPLETED (Awaiting Local Testing Validation)

Successfully implemented complete Docker-based testing infrastructure for integration tests:

**Docker Infrastructure** (`/containers/testing/`):
- ✅ 11-service stack (Apache Tika, SMTP4Dev, MongoDB, SQL Server, RabbitMQ, OpenSearch, Qdrant, Azurite, LocalStack, Keycloak, SBert)
- ✅ Cross-platform scripts (Linux/macOS/Windows)
- ✅ Health checks for all services (2-minute timeout)
- ✅ Ephemeral volumes for clean state
- ✅ Comprehensive README with PlantUML deployment diagram

**Test Categories**:
- ✅ Added `LiveIntegration` category (5th category)
- ✅ Updated XML documentation for all categories
- ✅ Clear separation: Integration (Docker) vs LiveIntegration (Cloud)

**CI/CD Pipeline**:
- ✅ Complete implementation in `integration-tests.yml`
- ✅ Docker startup, health checks, tests, cleanup
- ✅ Daily scheduled at 4 PM UTC (after build, before release)
- ⚠️ **DISABLED** - Triggers commented out until local Docker validation

**Next Steps:**
1. Complete `/containers/testing/TESTING-CHECKLIST.md` to validate Docker stack locally
2. Enable workflow triggers after successful validation
3. Migrate 20+ tests from DevLocal to Integration category

See `/containers/testing/STATUS.md` for detailed progress.

---


## Current Work Context (2026-01-20)

### Priority 1: SharedFramework Migrations

**Status:** 🎉 First migration complete! Ready for next project

**Completed:**
- ✅ Caching Framework (2026-01-20) - 4 implementation + 3 test projects

**Next Priority Migrations:**
- 🔥 Communications (CRITICAL - 16 LOC stub → 1,145 LOC complete system)
- Message Queues (AWS SQS + Azure Service Bus)
- Spatial Services (5 geocoding providers)
- Data Loader (ETL pipeline + CLI tool)

### Priority 2: Integration Testing Infrastructure

**Status:** ✅ WEEK 1 & 2 COMPLETE - ⏳ AWAITING LOCAL DOCKER VALIDATION

### Priority 3: Incoming Project Investigations

**Status:** ✅ 100% COMPLETE - All 6 projects investigated

### Priority 4: Swashbuckle 10.1.0 & .NET 10.0 Fixes

**Status:** ✅ COMPLETE - All fixes working, Swagger generation tested and verified

---

## Recently Completed Work

### ✅ SharedFramework Phase 0 Cleanup (2026-01-20)

Completed comprehensive namespace reorganization and standardization for all 52 SharedFramework projects.

**Completed:**
- **Removed "Api." prefix** from 14 projects (Twilio, Redis, Microsoft, Google, Census)
- **Moved Azure under Microsoft hierarchy** (4 projects: EventHub, ServiceBus)
- **Renamed .Contracts to .Abstractions** (9 projects standardized to match main framework)
  - Deleted OoBDev.Accounting.Contracts (application-specific)
  - Standardized: Caching, Communications, ComplexEvents, DataLoader, DocumentCenter, Generations, IdentityModel, SpatialServices, TextTemplating
- **Renamed 27 directories total** (14 + 4 + 9)
- Updated 87+ namespace declarations and 37+ using statements in .cs files
- Updated all .csproj ProjectReference paths
- Updated README.md and documentation
- Verified zero broken references

**Impact:** SharedFramework Phase 0 complete, ready for Phase 1 migration (Message Queueing Providers)

---

### ✅ Incoming Project Investigations Complete (2026-01-20)

Completed investigation of final 2 Incoming projects (ContractParser and Tools), achieving 100% investigation completion.

**ContractParser:**
- ANTLR4-based DSL for service contracts (5 files, ~475 LOC)
- Feature specification created in Features/ContractParser/README.md
- No implementation code - specification only
- Decision pending: Implement now, later, or keep as specification

**Tools:**
- 4 CLI tools analyzed (18 files, ~474 LOC)
- De5000.Ble.Cli: Bluetooth scanner (archive as reference)
- ImageConverter.Cli: Image converter (archive as reference)
- BulkLlm.GroqNet.Cli & BulkLlm.Ollama.Cli: 95% duplicate (consolidation recommended)
- Feature analysis created in Features/Tools/README.md
- Decision pending: Consolidate BulkLlm tools with ILlmProvider abstraction

**Updated:**
- Incoming/CHECKLIST.md: 100% investigation complete (6/6 projects)
- Removed CloudOrchestrator references (doesn't exist)
- Oobtainium: Moved to proving-grounds repository (https://github.com/mwwhited/proving-grounds)

---

### ✅ Swashbuckle 10.1.0 & .NET 10.0 Fixes (2026-01-20)

Fixed all breaking changes from Swashbuckle 10.1.0 and .NET 10.0, plus enabled XML documentation globally. All 65 projects build successfully, Swagger generation tested and verified working.

**Details:** [docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md](docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md)

---

### ✅ MSTest ExpectedExceptionAttribute Conversion (2026-01-15)

Converted 40 test instances from deprecated `[ExpectedException]` to modern `Assert.ThrowsException<T>()` pattern across 24 files.

**Details:** [docs/changes/bug-fixes-mstest-expected-exception-2026-01-15.md](docs/changes/bug-fixes-mstest-expected-exception-2026-01-15.md)

---

### ✅ Phase 0 Critical Bug Fixes (2026-01-15)

Fixed 6 critical bugs including lambda syntax errors, non-functional stub implementations, and floating-point precision issues. Created NumericAsserts utility for consistent floating-point comparisons.

**Details:** [docs/changes/bug-fixes-phase0-critical-2026-01-15.md](docs/changes/bug-fixes-phase0-critical-2026-01-15.md)

---

## Previous Completed Work (2026-01-19)

### Task: Docker-Based Integration Testing Infrastructure

**What's Completed:**

**Week 1 - Docker Infrastructure (✅ COMPLETE):**
- ✅ Complete Docker infrastructure in `/containers/testing/`
- ✅ 11 services configured with health checks
- ✅ Cross-platform startup/shutdown scripts
- ✅ CI/CD pipeline implemented (disabled until validation)
- ✅ Comprehensive documentation with PlantUML diagrams

**Week 2 - Test Migration (✅ COMPLETE):**
- ✅ Migrated 19 tests from DevLocal to Integration category
- ✅ Apache Tika (6 tests) - Updated base class with TIKA_URL env var
- ✅ SMTP/MailKit (2 tests) - Added SMTP_HOST, SMTP_PORT, IMAP_HOST, IMAP_PORT
- ✅ MongoDB (3 tests) - Added unique DB naming, cleanup, MONGODB_CONNECTION_STRING
- ✅ RabbitMQ (3 tests) - Added RABBITMQ_HOST env var
- ✅ OpenSearch (2 tests) - Added unique index naming, cleanup, OPENSEARCH_URL/USERNAME/PASSWORD
- ✅ SBert (2 tests) - Added SBERT_URL env var
- ✅ All tests now use `TestContext.GetProperty<T>()` pattern (corrected by linter)

**Documentation Complete:**
- ✅ [TEST_VARIABLES.md](./TEST_VARIABLES.md) - All 30+ test properties documented
- ✅ [docs/architecture/testing-guidelines.md](./docs/architecture/testing-guidelines.md) - Comprehensive testing guide
- ✅ Updated TODO.md, CLAUDE.md, and child TODO files with references

**What's Needed:**
- [ ] Local validation using `/containers/testing/TESTING-CHECKLIST.md`
- [ ] Verify all 11 services become healthy
- [ ] Run 19 migrated Integration tests locally
- [ ] Test cleanup works correctly
- [ ] Document any issues or adjustments needed

**After Validation:**
- Enable GitHub Actions workflow (uncomment triggers)
- Proceed to Week 3: Migrate LiveIntegration tests (Azure B2C, App Insights, Groq)
- Proceed to Week 4: Complete documentation (11 stack docs + diagrams)


## Contact & Feedback

- Issues: https://github.com/anthropics/claude-code/issues
- Protocols: `.claude/protocols/`
- Documentation: `/docs/`
- Tracking: `/TODO.md`
