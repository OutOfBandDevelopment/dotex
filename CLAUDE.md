# OoBDev (dotex) Framework - Claude Development Guide

**Last Updated:** 2026-01-19
**Framework:** OoBDev (dotex) - Enterprise .NET Library Suite
**Target:** .NET 9.0
**Current Work:** Week 1 & 2 Complete (Infrastructure + 19 test migrations) - Awaiting local Docker validation

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
- .NET 9.0 target framework
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

#### Code Generation
- **template-development.md** - Template-based code generation
- **template-swagger-documentation.md** - Template/OpenAPI maintenance

#### Component Standards
- **schema-integration.md** - Schema framework integration
- **datagrid-style-guide.md** - DataGrid component standards

### 3. Current Migration Work

**Incomming Project Investigations** - 🔍 IN PROGRESS (5 of 9 complete)
- ✅ dotnet-lib - Completed and deleted (95% identical to main)
- ✅ Framework - Investigated (55 files, Phase 0 comparison required)
- ✅ Oobtainium - Investigated (mocking framework, deletion recommended)
- ✅ BotChat - Investigated (sample app, awaiting decision)
- ✅ SharedFramework - Investigated (52 projects, migration plan ready)
- ⏱️ BinaryDecoders - Investigated (awaiting critical decisions)
- ⏳ CloudOrchestrator - Not yet started
- ⏳ ContractParser - Not yet started
- ⏳ Tools - Not yet started

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

**SharedFramework Migration** - ✅ .NET VERSION VERIFIED
- ✅ 51 projects already at .NET 10.0 (newer than .NET 9.0 requirement)
- ✅ 1 SQL project at netstandard2.0 (correct for SQL database projects)
- 52 projects analyzed and categorized
- 12-phase migration plan created
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
❌ Features with no .NET 9.0 equivalent

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

Integration tests run against **11 Docker services** managed by the testing infrastructure:

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

## Recently Completed Work (2026-01-19)

### Task: Docker-Based Integration Testing Infrastructure (Week 1)
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

## Previous Completed Work (2026-01-15)

### Task: Convert ExpectedExceptionAttribute to Assert.ThrowsException
**Status:** ✅ COMPLETED

Successfully converted all 40 instances of `[ExpectedException(typeof(T))]` to `Assert.ThrowsException<T>()` across 24 test files:
- Framework Layer: 10 conversions
- Binary Decoders: 4 conversions
- SharedFramework: 26 conversions

Both sync and async test patterns handled correctly. See TODO.md for full file list.

---

## Current Work Context (2026-01-19)

### Task: Integration Testing Infrastructure & Test Migration

**Status:** ✅ WEEK 1 & 2 COMPLETE - ⏳ AWAITING LOCAL DOCKER VALIDATION

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

---

## Previous Work Context (2026-01-15 → 2026-01-20)

### Task: Fix Swashbuckle 10.1.0 & .NET 10.0 Breaking Changes in OoBDev.AspNetCore.Mvc

**Status:** ✅ CODE COMPLETE - Awaiting build/test verification

**What was completed:**
Fixed breaking changes from Swashbuckle 10.1.0 and .NET 10.0 across 5 files:

**Swashbuckle 10.1.0 (4 files):**

1. **FormFileOperationFilter.cs** (2026-01-15) - 2 errors fixed
   - Properties collection is read-only (loop through and add instead of assign)
   - JsonSchemaType is enum (use `JsonSchemaType.String` not `"string"`)

2. **HealthChecksDocumentFilter.cs** (2026-01-15) - 2 errors fixed
   - OpenApiTag changed to OpenApiTagReference in Tags collection
   - Properties is read-only (use .Add() calls instead of assignment)

3. **SearchQueryOperationFilter.cs** (2026-01-15) - 8+ errors fixed
   - OpenApiTag → OpenApiTagReference (3 occurrences)
   - Coalesce assignment with type mismatch (use traditional null check)
   - UpdateRequestSchema method major refactor:
     - Removed dictionary copy with `.ChangeComparer()`
     - Direct property access via `schema.Properties[key]`
     - Fixed IOpenApiSchema vs OpenApiSchema conversions
     - Added null checks for schema lookups

4. **ApplicationPermissionsApiFilter.cs** (2026-01-20) - 1 error fixed
   - Extensions collection can be null and is read-only
   - Added null guard: `if (operation.Extensions != null)`
   - Use indexer syntax: `Extensions["key"] = value` instead of `.Add()`
   - Fixed NullReferenceException at lines 41-44

**.NET 10.0 (1 file):**

5. **ServiceCollectionExtensions.cs** (2026-01-20) - 1 error fixed
   - ClaimsPrincipal has ambiguous constructors in .NET 10.0
   - Changed from type-based registration: `services.TryAddTransient<IPrincipal, ClaimsPrincipal>()`
   - Changed to factory method: `services.TryAddTransient<IPrincipal>(sp => new ClaimsPrincipal(new ClaimsIdentity()))`
   - Fixed DI container ambiguous constructor error at lines 60-64

**Next Steps:**
1. Build the project: `dotnet build src/Framework/OoBDev.AspNetCore.Mvc/OoBDev.AspNetCore.Mvc.csproj`
2. Run tests: `dotnet test src/ --filter "OoBDev.AspNetCore.Mvc"`
3. Verify no regressions in OpenAPI functionality

**Files Modified:**
- `src/Framework/OoBDev.AspNetCore.Mvc/Filters/FormFileOperationFilter.cs`
- `src/Framework/OoBDev.AspNetCore.Mvc/Filters/SearchQueryOperationFilter.cs`
- `src/Framework/OoBDev.AspNetCore.Mvc/SwaggerGen/HealthChecksDocumentFilter.cs`
- `src/Framework/OoBDev.AspNetCore.Mvc/Filters/ApplicationPermissionsApiFilter.cs`
- `src/Framework/OoBDev.AspNetCore.Mvc/ServiceCollectionExtensions.cs`

**Key Breaking Changes Reference:**

**Swashbuckle 10.1.0 - Collections NULL by default:**
- **ALL OpenAPI collections start as null** - must initialize before use
- `operation.Tags ??= new HashSet<OpenApiTagReference>()`
- `operation.Parameters ??= new List<OpenApiParameter>()`
- `operation.Responses ??= new OpenApiResponses()`
- `operation.Extensions ??= new Dictionary<string, IOpenApiExtension>()`
- `schema.Properties ??= new Dictionary<string, IOpenApiSchema>()` (or in object initializer)

**Swashbuckle 10.1.0 - Read-Only Properties:**
- `Content` properties are read-only - create new objects:
  - `new OpenApiRequestBody { Content = new Dictionary<...>() }`
  - `new OpenApiResponse { Content = new Dictionary<...>() }`

**Swashbuckle 10.1.0 - Type Changes:**
- `OpenApiTag` → `OpenApiTagReference` for Tags collection
- `JsonSchemaType` enum instead of string literals
- `IOpenApiSchema.Properties` is read-only (use `.Add()` or `[key] = value`)
- Schema references require null checking: `?.Reference`

**.NET 10.0:**
- `ClaimsPrincipal` has ambiguous constructors - use factory methods in DI registration
- `IActionContextAccessor` is deprecated (ASPDEPR006) - removed from DI (not used in codebase)

---

## Contact & Feedback

- Issues: https://github.com/anthropics/claude-code/issues
- Protocols: `.claude/protocols/`
- Documentation: `/docs/`
- Tracking: `/TODO.md`
