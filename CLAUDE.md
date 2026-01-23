# OoBDev (dotex) Framework - Claude Development Guide

**Last Updated:** 2026-01-22
**Framework:** OoBDev (dotex) - Enterprise .NET Library Suite
**Target:** net10.0
**Current Work:** Design-first approach for SharedFramework migrations | 4 Epics in design phase | Docker testing ready for CI/CD

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
- **configuration-documentation.md** - Discover and document all IConfiguration, IOptions, and Environment variables
  - **Trigger:** "find all my configurations"
  - Creates comprehensive CONFIGURATION_SETTINGS.md reference

#### Code Generation
- **template-development.md** - Template-based code generation
- **template-swagger-documentation.md** - Template/OpenAPI maintenance

#### Testing & Integration
- **integration-test-maintenance.md** - Maintain Docker-based integration test infrastructure
  - **Trigger:** "add new container", "add integration test service"
  - Checklist for Docker services, nginx config, dashboard, .runsettings, documentation

#### Component Standards
- **schema-integration.md** - Schema framework integration
- **datagrid-style-guide.md** - DataGrid component standards

### 3. Current Migration Work

**Incomming Project Investigations** - ✅ COMPLETE (8 of 8 = 100%)
- ✅ dotnet-lib - Completed and deleted (95% identical to main)
- 🗑️ Framework - CANCELLED: Directory removed from Incoming/
- ✅ Oobtainium - RESOLVED: Moved to proving-grounds repository (https://github.com/mwwhited/proving-grounds)
- ✅ BotChat - Investigated (sample app, awaiting decision)
- 🎨 SharedFramework - REPLACED: Code migration replaced with design-first documentation (Features/Proposals/)
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

**Framework Migration** - 🗑️ CANCELLED
- ✅ Vector files already in main codebase at `src/Framework/OoBDev.System.Abstractions/Math/`
- ✅ Namespace updated from `OoBDev.Common.Math` → `OoBDev.System.Math` (5 files)
- 🗑️ Incoming/Framework directory removed - migration cancelled

**SharedFramework Migration** - 🎨 REPLACED WITH DESIGN-FIRST APPROACH
- ✅ Strategic pivot (2026-01-22): Code migration replaced with comprehensive design documentation
- ✅ Caching Framework migrated (4 impl + 3 test projects) - Before pivot
- ✅ Message Queues migrated (AWS SQS + Azure Service Bus providers) - Before pivot
- 🎨 Remaining features: 120 design documents created across 8 epics (90.9% complete)
  - Epic 11: Data Enhancement (16 docs) ✅
  - Epic 10: Text Templating (12 docs) ✅
  - Epic 12: Message Composition (4 docs) ✅
  - Epic 7: Identity & Session (16 docs) ✅
  - Epic 6: Document Services (44 docs) ✅
  - Epic 4: Distributed Caching (12 docs) ✅
  - Epic 2: Communications (6 docs) 🔄 Partial
  - Epic 5: Master Data (10 docs) 🔄 Partial
- 🗑️ Incoming/SharedFramework directory removed - design docs replace code migration

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
- `/docs/how-tos/` - Practical how-to guides
  - [Variables in .runsettings](docs/how-tos/runsettings-variables-and-configuration.md)
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

Integration tests run against **14 Docker services** managed by the testing infrastructure:

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
- LocalStack (AWS emulator - SQS, S3, etc.)
- Azure Service Bus Emulator (Message queue)
- Keycloak (Identity & Access Management)
- SBert (Sentence embeddings - CPU only)
- Ollama (LLM inference - CPU only, phi3 model auto-pulled)

**Test Properties Pattern:**
```csharp
[TestMethod]
[TestCategory(TestCategories.Integration)]
public async Task TestMongoDBOperation()
{
    // ✅ CORRECT: Use GetRequiredProperty for required values
    var connectionString = TestContext.GetRequiredProperty<string>("MONGODB_CONNECTION_STRING");

    // ✅ CORRECT: Use GetPropertyOrDefault for industry-standard defaults
    var port = TestContext.GetPropertyOrDefault("MONGODB_PORT", 27017);

    var databaseName = $"IntegrationTest_{Guid.NewGuid():N}";  // Unique per run

    // ... test logic

    // Cleanup in [TestCleanup]
    await client.DropDatabaseAsync(databaseName);
}
```

**IMPORTANT:** Always use `TestContext.GetRequiredProperty<T>()` or `TestContext.GetPropertyOrDefault<T>()` instead of `Environment.GetEnvironmentVariable()` for test configuration. This integrates with `.runsettings` files and MSTest infrastructure.

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
- Co-author: `Claude Opus 4.5 <noreply@anthropic.com>`

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

### 2026-01-22
- **Strategic Pivot: Design-First Approach** - Replaced SharedFramework code migrations with comprehensive design documentation
  - Following Epic 11 pattern for all features (4 documents per feature: requirements, architecture, api-design, testing-strategy)
  - Updated TODO files: Communications (Epic 2), Text Templating (Epic 10), Identity (Epic 7), Documents (Epic 6)
  - Benefits: Clean architecture, no technical debt, modern .NET 10.0 patterns, complete test coverage planning
  - Design phase replaces code migration for 4 major feature areas
- **Build Warnings** - Reduced from 95+ to 8 warnings (CS1573, CS8604, CS8618, CS8620, CA2022, CA2024 fixed)
- **Test Categories** - Converted DevLocal tests to Integration/Unit/LiveIntegration categories
- **.runsettings How-To** - Created comprehensive guide for variables and configuration
- **HtmlNavigatorTests** - Fixed with ComplexTemplate.html resource and proper assertions
- **Redis Tests** - Fixed IObjectConverter dependency and JSON serialization compatibility

### 2026-01-21
- **Configuration** - 157+ settings. [Details](docs/changes/documentation-configuration-settings-2026-01-21.md)
- **Ollama** - 4 tests, auto-setup. [Details](docs/changes/testing-ollama-integration-2026-01-21.md)

### 2026-01-20
- **ANTLR/Keycloak** - Build & infra fixes. [Details](docs/changes/bug-fixes-antlr-keycloak-2026-01-20.md)
- **Caching** - 4 projects + 3 tests. [Details](docs/changes/migration-caching-framework-2026-01-20.md)
- **Message Queues** - SQS + Service Bus. [Details](docs/changes/migration-message-queues-2026-01-20.md)
- **SharedFramework** - Phase 0 namespace cleanup (27 dirs). [Details](docs/changes/sharedframework-phase0-2026-01-20.md)
- **Swashbuckle** - 10.1.0 + .NET 10.0 fixes. [Details](docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md)

### 2026-01-19
- **Docker Testing** - 14 services, 23 tests. [Details](docs/changes/testing-docker-infrastructure-2026-01-19.md)

### 2026-01-15
- **MSTest** - 40 tests modernized. [Details](docs/changes/bug-fixes-mstest-expected-exception-2026-01-15.md)
- **Phase 0 Bugs** - 6 critical fixes. [Details](docs/changes/bug-fixes-phase0-critical-2026-01-15.md)

---

## Current Work Context

**Strategic Change (2026-01-22): Design-First Approach**

Instead of directly migrating code from SharedFramework, we've pivoted to comprehensive design documentation following the Epic 11 pattern:

**Why Design-First?**
- Avoids technical debt from legacy SharedFramework code
- Ensures modern .NET 10.0 patterns throughout
- Integrates cleanly with new Epic 11 features (IDataContainer, schema discovery, path translation)
- Enables comprehensive test planning upfront (85-90% coverage targets)
- Documents architectural intent before implementation
- No namespace migrations or breaking changes needed

**Epics in Design Phase:**
1. **Epic 2: Communications Platform** - Channel abstraction, send/receive, user preferences, multi-channel routing
2. **Epic 10: Text Templating Extensions** - Template discovery, repository, caching, engine provider pattern
3. **Epic 7: Identity & Session Management** - Claims enhancement, rights management, session management, Azure B2C integration
4. **Epic 6: Document Services** - Document packaging, resolvers, storage abstraction, 11 context-based services

**Pattern: 4 Documents per Feature**
- `requirements.md` - Business requirements and use cases
- `architecture.md` - System design and component relationships
- `api-design.md` - Interface definitions and contracts
- `testing-strategy.md` - Test coverage plan (85-90% targets)

**Active Priorities:**
1. **Design Documentation** - Creating 16 design documents per Epic (64 total across 4 Epics)
2. **Docker Testing** - Ready for CI/CD enablement (14 services, 23+ tests validated)
3. **Incoming Projects** - All investigated (decisions pending)

**Latest Updates:**
- Strategic pivot from code migration to design-first documentation (2026-01-22)
- 4 TODO files updated to reflect design phase (Communications, Text Templating, Identity, Documents)
- Build warnings reduced to 8 (down from 95+)
- Test categories cleaned up (DevLocal → Integration/Unit/LiveIntegration)
- .runsettings how-to guide created
- Configuration documentation complete (CONFIGURATION_SETTINGS.md)
- Ollama integration complete (phi3 auto-setup)
- 14 Docker services ready (Apache Tika, MongoDB, SQL Server, RabbitMQ, Redis, OpenSearch, Qdrant, Azurite, LocalStack, Service Bus, Keycloak, SBert, Ollama)

