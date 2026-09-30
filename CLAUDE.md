# OoBDev (dotex) Framework - Claude Development Guide

**Last Updated:** 2026-09-30
**Framework:** OoBDev (dotex) - Enterprise .NET Library Suite
**Target:** net10.0
**Current Work:** Patterns discovery (branch `dev/patterns-discovery`, see "Patterns Discovery Work" below) | Design-first approach for SharedFramework migrations | Docker testing ready for CI/CD

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
- `/TODO.md` - Current migration status and pending work (the `📌 Backlog:` sections hold open review items)
- `/docs/patterns-discovery/README.md` - How this codebase is actually built (patterns, conventions, blueprint, alternatives)
- `/docs/architecture/README.md` - Architecture overview
- `/docs/architecture/architectural-standards.md` - Enforceable coding standards

**Key Principles:**
- ALL features from BinaryDataDecoders will be migrated (phases = priority order, not selection)
- Follow provider/factory pattern for extensibility
- Use dependency injection (TryAdd* extensions)
- 80% test coverage for Framework layer
- `README.{Project}.md` (upper-case `README`) expected per project; `PackageReadmeFile` is derived from that name in `src/Directory.Build.props`, and a missing file is only warning OBDPK001
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

**Incomming Projects Pending Decisions:**
- ⏸️ **BotChat** - Sample app, decision needed (archive, enhance, or extract patterns)
- ⏸️ **BinaryDataDecoders** - ~50,000 LOC, awaiting 14+ critical decisions
- ⏸️ **ContractParser** - Decision needed (implement now, later, or keep as spec)
- ⏸️ **Tools** - Decision needed (consolidate BulkLlm tools or archive)

**BinaryDataDecoders Migration** - ⏸️ BLOCKED (Awaiting decisions)

**Phase 1: Foundation Enhancement** - ⏸️ PENDING (Awaiting decisions)
- Endianness support improvements
- Utility enhancements
- BinaryPrimitives expansion

---

## Architecture Layers

### Common Layer (6 projects)
Foundation abstractions and contracts
- Aggregator: MSBuild globs include/remove ProjectReferences (not purely interfaces)

### Framework Layer (39 projects)
Core functionality implementations
- Depends only on Common
- 80%+ test coverage required
- `README.{Project}.md` expected (warning if missing)

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

## Documentation Diagram Standards

- **All diagrams in markdown documents MUST be PlantUML** (fenced ```` ```plantuml ```` blocks with `@startuml`/`@enduml`). Never use ASCII/box-drawing art or Mermaid.
- **All UI mockups MUST be PlantUML + Salt** (`@startsalt`/`@endsalt`), embedded in the markdown document.
- **Validate docs before finishing**: run `python scripts/docs/validate-docs.py docs` (renders every PlantUML diagram via the `plantuml/plantuml-server` docker image, checks links/captions). A diagram is not done until it renders. Never hand-type escape sequences in shell-heredoc'd Python; write scripts with the Write tool and build backslash-n as `chr(92)+'n'`.
- **Prefer deterministic scripts over agentic LLM work** for repeatable tasks (validation, linting, TOC/caption generation, renames, link fixing). Put them in `scripts/` with a README, and reuse them.
- **Architecture diagrams use C4 style (Context/Container/Component) written in plain PlantUML** (rectangles + stereotypes + skinparam). Never `!include` C4-PlantUML templates – they break in production rendering.
- **Include a Table of Contents, List of Figures and List of Tables where useful** (3+ sections / 2+ figures / 2+ tables). Caption figures `*Figure N — title*` (after) and tables `**Table N — title**` (before).
- **Split long documents** (>~250 lines or >~6 major sections) into a topic folder with a `README.md` index and one file per headline; nav links at top/bottom of each file. **Prefer valid relative cross references** (file + `#anchor`) wherever a concept has its own document; verify links resolve.
- Design documents follow `docs/patterns-discovery/06-design-document-standard.md` (4 documents per feature).
- Applies to every new or edited doc (`docs/**`, READMEs, design docs, protocols). Convert existing ASCII diagrams when touching a document.

---

## Coding Standards

### File Structure
```
OoBDev.{Layer}.{Feature}/
├── README.{Project}.md (expected; upper-case README, warning if missing)
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
- Extensions: `{Name}Extensions`; DI registration class is always `ServiceCollectionExtensions` (never `ServiceCollectionEx`; 6 projects still to rename, see TODO.md)
- Provider keys: kebab-case constants in each adapter's `{Vendor}Globals` (never a global registry; keeps adapters referencing only Abstractions)
- Prefer platform primitives (e.g. `TimeProvider`) over hand-built abstractions; inject by interface

### UI Preferences
- MVVM with command binding wherever possible (e.g. WPF: view models, `ICommand`, data binding, minimal code-behind)
- Commands are real, retestable command types (e.g. `SaveDocumentCommand : ICommand` with injected dependencies), not just relay commands routed to lambda expressions
- When a JS/TS framework is required, prefer one that supports view-model binding and command-style handlers as closely as possible; note where it diverges from MVVM

### Authentication Preferences
- Use OAuth 2.0 / OIDC / JWT bearer wherever possible; applications validate tokens and never store passwords
- Services only need to understand JWT: any other credential (API key, HMAC, basic auth, legacy bearer) is converted to JWT by the STS or an edge adapter, never by per-scheme code inside a service
- Windows-integrated (Kerberos/NTLM) and SAML sign-in are SSO inputs too: the STS exchanges them for JWT so everything downstream is JWT only
- When required, support an STS with token exchange (RFC 8693) that converts SSO tokens into application-specific tokens; optional and config-gated, injected by interface (see `docs/patterns-discovery/03-practices-and-conventions/09-authentication-practices.md`)

### HTTP API and Authorization Preferences
- REST wherever possible; OpenAPI is the contract; errors are RFC 9457 problem details
- Evaluate OData over the HTTP `QUERY` verb (with a `POST` fallback) and GraphQL as additional query surfaces beside the custom search syntax; no breaking changes to the existing syntax
- RBAC with application rights: endpoints declare rights (`[ApplicationRight]`), roles are translated to rights in middleware; claims exchange at the STS is an alternative only while tokens stay small
- Same rights checks on every API surface (see `docs/patterns-discovery/03-practices-and-conventions/10-http-api-practices.md` and `11-authorization-practices.md`)

### Rejected and Preferred Dependencies (owner decisions)
- No third-party IoC/DI containers and no third-party logging libraries (use `Microsoft.Extensions.*` and `[LoggerMessage]`)
- Avoid Polly (license change); add OpenTelemetry; default hash is SHA-512
- MSTest stays; central package management to be restored; GitVersion stays with app projects versioned together
- Provider selection: a container-registered factory picks a keyed service from a configuration path (replaces `ISelectedService<T>`); shared options validation via `AddValidatedOptions<T>()`
- Keep all .NET libraries/packages as current for .NET 10 as practical; Microsoft.Extensions.AI is expected to replace the hand-built AI abstractions (spike in `docs/patterns-discovery/08-spikes/`)
- Abstractions projects (interfaces and models only) need no tests; if one holds testable implementation it gets a test library. Missing project readmes should be created
- Owner answers are recorded under each verdict in `docs/patterns-discovery/05-industry-alternatives/`

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

Integration tests run against **15 Docker services** managed by the testing infrastructure:

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
- Azurinsight (Application Insights emulator)

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
- Commit messages must NEVER reference Claude or AI (no Co-Authored-By trailer, no "generated with" line)

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

## Patterns Discovery Work (branch `dev/patterns-discovery`)

**Resume here.** Goal: document how the codebase is built so future products follow the same patterns, then compare with industry alternatives.

- **Docs:** `docs/patterns-discovery/` (01 architecture, 02 design patterns, 03 practices, 04 new-project blueprint, 05 industry alternatives, 06 design-document standard; README has the doc/code drift table). Patterns are the owner's preferences: record them, don't "correct" them; rough edges go to `TODO.md` backlogs.
- **Templates:** `templates/` (`dotnet new` pack: `oobdev-capability`, `oobdev-adapter`, `oobdev-webapp`); verify with `scripts/templates/verify-templates.ps1` (generates under `src/`, builds from `src/Framework` cwd because `Directory.Build.props` computes `SolutionDir` from the cwd, then cleans up).
- **Scripts:** `scripts/docs/` (`validate-docs.py`, `build-index.py`, `build-project-catalog.py`, `fix-plantuml-newlines.py`). Validate with `python scripts/docs/validate-docs.py docs/patterns-discovery` (expect 80 files, 0 problems; the whole `docs/` tree has ~566 pre-existing problems, e.g. `docs/sbom`). Needs docker `plantuml/plantuml-server` on port 18080.
- **Decisions made:** `#if DEBUG` required builder parameters are intentional (forces child builders to be forwarded); `ServiceCollectionExtensions` everywhere; provider keys kebab-case; readmes are `README.X.md`; options were deliberately unvalidated (strict/relaxed mode under analysis).
- **Open backlogs in `TODO.md`:** `ISelectedService` rough edges (intent unknown, needs owner review), naming consistency, Roslyn analyzers, options validation modes, caching proxy (`Retreive` to `Retrieve`), message context caller info, replace hand-built providers with platform primitives (`TimeProvider`), HTTP querying and rights middleware, documentation coverage gaps (build these out in the listed order).
- **Branch state:** history was rewritten to remove AI trailers from commit messages and the branch matches `origin/dev/patterns-discovery` (as of 3f54bfe). Never force-push without the owner's explicit approval.

---

## Recently Completed Work

### 2026-09-30
- **Patterns Discovery** - architecture/patterns/practices/blueprint/alternatives docs, `dotnet new` templates, doc validation scripts, readme casing normalized (74 files), review backlogs recorded in `TODO.md`

### 2026-01-29
- **TestContext Configuration Provider** - Integrated MSTest `.runsettings` with .NET `IConfiguration`
  - Added to OoBDev.TestUtilities (not separate package as originally planned)
  - 12 unit tests + 8 integration tests
  - Supports hierarchical configuration (`Database:Server` or `Database__Server`)
  - Strong-typed binding with `IOptions<T>`
  - Prefix filtering and case-insensitive keys
  - Documentation: OoBDev.TestUtilities README + runsettings how-to guide updated
- **Ollama Auto-Initialization** - Converted to Dockerfile approach with phi3 model baked into image
  - Model pulled during build, not runtime
  - Faster startup (no 2.2GB download on container start)
  - Removed obsolete entrypoint script approach

### 2026-01-24
- **Integration Testing Scripts** - Enhanced integration-up scripts with `--build` flag, fixed Windows batch path handling (PUSHD)
- **Health Check Fixes** - Updated all 15 Docker services to use universal bash TCP checks (`</dev/tcp/HOST/PORT`)
  - Status: 13/15 services healthy (azurinsight, servicebus pending investigation)
  - Replaced curl/wget/nc dependencies with bash/node/python built-ins
- **Service Display** - Added all 15 services to startup script output (Redis, Service Bus, Ollama, Azurinsight)
- **Protocol Updates** - Updated integration-test-maintenance.md to v1.1.0 with expanded checklists

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

**Active Priorities:**
1. **Integration Testing** - Finalizing health checks for all 15 Docker services (13/15 healthy)
   - ⏳ azurinsight health check needs investigation
   - ⏳ servicebus startup validation (30s start period)
   - Next: Enable CI/CD pipeline after all services validated
2. **SharedFramework** - Design-first approach (Epic 2, 6, 7, 10)
3. **Incoming Projects** - All investigated (decisions pending)

**Latest Updates (2026-01-24):**
- Enhanced integration-up scripts with `--build` flag
- Fixed Windows batch file path handling (PUSHD)
- Updated all health checks to use bash TCP built-ins
- Added missing services to startup output display

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

