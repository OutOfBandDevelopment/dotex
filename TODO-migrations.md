# TODO - Migrations Epic

**Last Updated:** 2026-01-20

This document tracks all migration work for Incomming projects and BinaryDataDecoders.

> **Parent Document:** [TODO.md](./TODO.md)

---

## Individual Migration TODOs

**📋 See:** [TODO-migrations-index.md](./TODO-migrations-index.md) for complete index

Each migration area has a dedicated TODO file:

### SharedFramework (10 projects)
1. [TODO-migrations-communications.md](./TODO-migrations-communications.md) 🔥 CRITICAL
2. [TODO-migrations-caching.md](./TODO-migrations-caching.md) ✅ SAFE
3. [TODO-migrations-message-queues.md](./TODO-migrations-message-queues.md) ✅ SAFE
4. [TODO-migrations-spatial.md](./TODO-migrations-spatial.md) ✅ SAFE
5. [TODO-migrations-identity.md](./TODO-migrations-identity.md) ⚠️ MERGE
6. [TODO-migrations-documents.md](./TODO-migrations-documents.md) ⚠️ MERGE
7. [TODO-migrations-text-templating.md](./TODO-migrations-text-templating.md) ⚠️ CONSOLIDATE
8. [TODO-migrations-data-loader.md](./TODO-migrations-data-loader.md) ✅ SAFE
9. [TODO-migrations-complex-events.md](./TODO-migrations-complex-events.md) ✅ SAFE
10. [TODO-migrations-generations.md](./TODO-migrations-generations.md) ✅ SAFE

### Other Incoming
11. [TODO-migrations-framework.md](./TODO-migrations-framework.md) - 55 files comparison required
12. [TODO-migrations-binarydatadecoders.md](./TODO-migrations-binarydatadecoders.md) - Blocked by decisions

---

## Overview

Migration work includes:
- **Incomming/Framework** - 55 files (30 NEW + 25 DIFFERS) - Vector library, audit logging, database mapper
- **Incomming/SharedFramework** - 52 projects (~28,582 LOC) - External service integrations, communications, spatial services
- **BinaryDataDecoders** - 5-phase migration of binary processing, protocols, and specialized features

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

### Phase 1: Vector Math Library Migration ✅ NAMESPACE UPDATE COMPLETE

**Status:** ⏭️ FILES ALREADY IN MAIN - Namespace updated to OoBDev.System.Math

**Complexity:** N/A - Files already exist in main codebase

**Priority:** HIGH - Critical for SemanticKernel and AI/ML features

**See:** [Vector Comparison](docs/migration/vector-comparison.md) for detailed analysis

**✅ COMPLETED 2026-01-19:**
- [x] Vector files already exist in `src/Framework/OoBDev.System.Abstractions/Math/`
- [x] Updated namespace from `OoBDev.Common.Math` → `OoBDev.System.Math` (4 files)
- [x] Updated test namespace in `src/Framework/OoBDev.System.Tests/Math/VectorTests.cs`
- [x] No migration needed - files already in correct location

**NOTE:** Vector files do NOT exist in `Incoming/Framework` directory - they are already integrated into main codebase.

- [x] **1.1 ~~Create Target Directory Structure~~ Already Exists**
  - [x] ~~Create `src/Framework/OoBDev.System/Math/` directory~~
  - [x] ~~Create `src/Framework/OoBDev.System.Tests/Math/` directory~~
  - Files already at: `src/Framework/OoBDev.System.Abstractions/Math/`

- [x] **1.2 ~~Migrate~~ Update Core Vector Files Namespace**
  - [x] ~~Migrate~~ `Vector.cs` - Namespace updated to `OoBDev.System.Math`
  - [x] ~~Migrate~~ `VectorMath.cs` - Namespace updated to `OoBDev.System.Math`
  - [x] ~~Migrate~~ `VectorDistanceMetrics.cs` - Namespace updated to `OoBDev.System.Math`
  - [x] ~~Migrate~~ `VectorComparer.cs` - Namespace updated to `OoBDev.System.Math`
  - [ ] Future enhancement: Add `[MethodImpl(MethodImplOptions.AggressiveInlining)]` to VectorMath methods
  - [ ] Future enhancement: Add `CosineSimilarity` method (returns `1.0 - CosineDistance(...)`)

- [x] **1.3 ~~Migrate~~ Update Tests Namespace**
  - [x] `VectorTests.cs` - Namespace updated to `OoBDev.System.Tests.Math`
  - [x] Using statement updated to `OoBDev.System.Math`
  - [ ] Future enhancement: Add additional test cases for edge cases
  - [ ] Future enhancement: Target 80%+ coverage

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

**Status:** 🔍 INVESTIGATION COMPLETE - ⚠️ OVERLAP ANALYSIS COMPLETE

**Complexity:** HIGH - 52 projects (~28,582 LOC) requiring careful integration

**Documentation:**
- [Feature Mapping](docs/migration/sharedframework-feature-mapping.md) - Complete 52-project analysis
- [Migration Plan](docs/migration/sharedframework-migration-plan.md) - 12-phase execution plan
- [Coverage Analysis](docs/migration/sharedframework-phase0-coverage-analysis.md) - Overlap analysis with main codebase
- [DIFFERS Detailed Comparison](docs/migration/sharedframework-differs-detailed-comparison.md) - **NEW:** Detailed analysis of 5 conflict projects

**Key Findings from Coverage Analysis:**
- ✅ **Phase 0 Complete** - .NET 10.0 + namespace cleanup done
- ⚠️ **5 DIFFERS projects** - Main has partial/basic implementations (Communications, Documents, Identity, TextTemplating)
- ✅ **19 NEW projects** - No overlap, safe to migrate (Caching, Spatial, Events, etc.)
- 🔥 **Critical:** Communications has 16 LOC stub in main vs 1,145 LOC in SF (71x difference!)

**Verdict from Detailed Comparison:**
- ✅ **ALL 5 DIFFERS projects** - SharedFramework provides significant advantages
  - Communications: Main is empty stub → SF has complete orchestration system
  - Communications.Abstractions: Main has 5 files → SF has 47 files with complete framework
  - Documents: SF adds Packaging, Resolvers, generic Storage abstraction
  - Identity: SF adds Claims enhancement, Rights management, Azure B2C, Graph API
  - TextTemplating: SF provides unified framework vs main's scattered implementation
- 🎯 **Recommendation:** Accept ALL SharedFramework implementations (production-proven)

**Summary:**
- 24 projects (71%) provide NEW capabilities
- 10 projects (29%) are ENHANCED versions (DIFFERS - need merge)
- 52 total projects: 34 implementation + 18 test projects
- Critical external service integrations (AWS SQS, Azure, Twilio, geocoding)
- Completes partially-implemented main framework components

### Phase 0: Framework Upgrades ✅ .NET UPGRADE COMPLETE

**Priority:** CRITICAL

- [x] **0.1 ✅ VERIFIED - Projects Already at .NET 10.0**
  - [x] Verified all 52 projects already target `net10.0` (51 projects) or `netstandard2.0` (1 SQL project)
  - [x] 51 projects at `net10.0` (.NET 10.0)
  - [x] 1 project (OoBDev.ComplexEvents.DatabaseExtensions) at `netstandard2.0` (SQL database project - correct)
  - [x] No upgrade needed - already newer than .NET 9.0 requirement
  - **NOTE:** Projects are at .NET 10.0, which is NEWER than the .NET 9.0 requirement

- [x] **0.2 ✅ COMPLETE - Namespace Cleanup (2026-01-20)**
  - [x] Created namespace mapping matrix (18 projects affected)
  - [x] Removed "Api." prefix from all namespaces:
    - `OoBDev.Api.Twilio` → `OoBDev.Twilio`
    - `OoBDev.Api.Redis.Caching` → `OoBDev.Redis.Caching`
    - `OoBDev.Api.Microsoft.*` → `OoBDev.Microsoft.*`
    - `OoBDev.Api.Google.Maps` → `OoBDev.Google.Maps`
    - `OoBDev.Api.Census.Geocoding` → `OoBDev.Census.Geocoding`
  - [x] Moved Azure under Microsoft hierarchy:
    - `OoBDev.Azure.EventHub` → `OoBDev.Microsoft.Azure.EventHub`
    - `OoBDev.Azure.ServiceBus` → `OoBDev.Microsoft.Azure.ServiceBus`
  - [x] Renamed `.Contracts` to `.Abstractions` (9 projects):
    - Deleted `OoBDev.Accounting.Contracts` (application-specific)
    - `OoBDev.Caching.Contracts` → `OoBDev.Caching.Abstractions`
    - `OoBDev.Communications.Contracts` → `OoBDev.Communications.Abstractions`
    - `OoBDev.ComplexEvents.Contracts` → `OoBDev.ComplexEvents.Abstractions`
    - `OoBDev.DataLoader.Contracts` → `OoBDev.DataLoader.Abstractions`
    - `OoBDev.DocumentCenter.Contracts` → `OoBDev.DocumentCenter.Abstractions`
    - `OoBDev.Generations.Contracts` → `OoBDev.Generations.Abstractions`
    - `OoBDev.IdentityModel.Contracts` → `OoBDev.IdentityModel.Abstractions`
    - `OoBDev.SpatialServices.Contracts` → `OoBDev.SpatialServices.Abstractions`
    - `OoBDev.TextTemplating.Contracts` → `OoBDev.TextTemplating.Abstractions`
  - [x] Updated 87+ namespace declarations in .cs files
  - [x] Updated 37+ using statements in .cs files
  - [x] Updated all .csproj ProjectReference paths
  - [x] Updated all README.md and documentation references
  - [x] Renamed all 27 directories (14 Api. removal + 4 Azure reorganization + 9 Contracts → Abstractions)
  - [x] Verified no broken references

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

**Status:** ⏸️ BLOCKED - Awaiting critical decisions

**See Also:** [TODO-decisions.md](./TODO-decisions.md) - Decision points requiring answers before Phase 1

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
