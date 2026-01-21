# Migration TODO - Framework

**Project:** Incoming/Framework
**Files:** 55 files (30 NEW + 25 DIFFERS)
**Source:** Incoming/Framework/
**Status:** 🔍 REQUIRES PHASE 0 SYSTEMATIC COMPARISON
**Priority:** HIGH

---

## Overview

**What This Is:**
Collection of 55 files from a different codebase version containing:
- Core abstractions and interfaces
- Database mapper utilities
- Audit logging
- HTTP request preparation
- Validation attributes
- Service extensions

**Key Challenge:**
25 files have DIFFERS status - they exist in both main and Incoming but with differences. Must systematically compare to identify:
- Bug fixes in Incoming
- Improvements in Incoming
- Features unique to main
- Breaking changes

---

## Current Status

**Analysis:** ✅ COMPLETE - Files categorized
**Phase 0 Comparison:** ⏸️ PENDING - Required before any migration
**Migration:** 🚫 BLOCKED - Cannot proceed without Phase 0 comparison

---

## Tasks

### Phase 0: Systematic File Comparison (REQUIRED FIRST)

#### 0.1 Core Abstractions (8 files - DIFFERS)
- [ ] Compare `IAccessor.cs` - Async-local accessor pattern
- [ ] Compare `IDataConverter.cs` - Type conversion interface
- [ ] Compare `IDatabaseMapper.cs` - Database mapping interface
- [ ] Compare `IDatabaseQuery.cs` - Database query interface
- [ ] Compare `IHttpPrepareRequest.cs` - HTTP request customization
- [ ] Compare `IHttpPrepareRequestFeature.cs` - Middleware feature
- [ ] Compare `IJsonSerializer.cs` - JSON serialization interface
- [ ] Compare `Accessor.cs` - Accessor implementation
- [ ] Create comparison matrix documenting differences

#### 0.2 Core Implementations (8 files - DIFFERS)
- [ ] Compare `DataConverter.cs` - Type conversion implementation
- [ ] Compare `DatabaseQuery.cs` - Query execution
- [ ] Compare `HttpPrepareRequest.cs` - HTTP preparation base
- [ ] Compare `CorrelationInfo.cs` - Correlation tracking
- [ ] Compare `CorrelationInfoMiddleware.cs` - Middleware implementation
- [ ] Compare `CorrelationInfoHttpPrepareRequestFeature.cs` - Feature implementation
- [ ] Compare `ServiceCollectionExtensions.cs` (both projects)
- [ ] Document bug fixes and improvements found

#### 0.3 Validation Attributes (7 files - DIFFERS)
- [ ] Compare `ConnectionStringNameAttribute.cs` - Database connection attribute
- [ ] Compare `QueryParameterAttribute.cs` - Stored procedure parameters
- [ ] Compare `QueryResultAttribute.cs` - Result mapping
- [ ] Compare `StoredProcedureAttribute.cs` - SP mapping
- [ ] Compare `ZipCodeAttribute.cs` - Zip code validation
- [ ] Compare `ZipCodesAttribute.cs` - Multi-zip validation
- [ ] Verify regex patterns and validation logic

#### 0.4 ASP.NET Core (2 files - DIFFERS)
- [ ] Compare `ApplicationBuilderExtensions.cs` - Pipeline configuration
- [ ] Compare `AdditionalSwaggerGenEndpointsOptions.cs` - Check if dotnet-lib migration fixed this
- [ ] Document middleware additions

#### 0.5 Constants & Headers (1 file - DIFFERS)
- [ ] Compare `DefinedHttpHeaders.cs` - HTTP header constants
- [ ] Check for new headers

#### 0.6 NEW Files Analysis (30 files)
- [ ] Review all 30 NEW files for value
- [ ] Categorize by feature area
- [ ] Determine migration priority
- [ ] Identify dependencies

#### 0.7 Namespace Mapping Decision
- [ ] Decide: `OoBDev.Common` → `OoBDev.System` or keep as `OoBDev.Common`?
- [ ] Map all Incoming namespaces to target main framework namespaces
- [ ] Document namespace migration strategy

#### 0.8 Phase 0 Summary Report
- [ ] Create `docs/migration/framework-phase0-comparison.md`
- [ ] Document all differences found in 25 DIFFERS files
- [ ] Identify bug fixes to apply
- [ ] Identify breaking changes
- [ ] Create prioritized migration task list
- [ ] Get user approval to proceed

---

### Phase 1+: Migration Execution (BLOCKED until Phase 0 complete)

**Cannot define specific tasks until Phase 0 comparison is complete.**

After Phase 0, will create detailed migration phases based on:
- Which bug fixes to apply
- Which features to merge
- Which implementations to replace
- Which new files to migrate

---

## File Categories

### DIFFERS Files (25 files - Require Comparison)

**Abstractions (8):**
- IAccessor.cs, IDataConverter.cs, IDatabaseMapper.cs, IDatabaseQuery.cs
- IHttpPrepareRequest.cs, IHttpPrepareRequestFeature.cs, IJsonSerializer.cs
- Accessor.cs

**Implementations (8):**
- DataConverter.cs, DatabaseQuery.cs, HttpPrepareRequest.cs
- CorrelationInfo.cs, CorrelationInfoMiddleware.cs, CorrelationInfoHttpPrepareRequestFeature.cs
- ServiceCollectionExtensions.cs (×2)

**Validation (7):**
- ConnectionStringNameAttribute.cs, QueryParameterAttribute.cs, QueryResultAttribute.cs
- StoredProcedureAttribute.cs, ZipCodeAttribute.cs, ZipCodesAttribute.cs

**ASP.NET (2):**
- ApplicationBuilderExtensions.cs, AdditionalSwaggerGenEndpointsOptions.cs

**Constants (1):**
- DefinedHttpHeaders.cs

### NEW Files (30 files - No Main Equivalent)

**See:** [Framework Feature Mapping](docs/migration/framework-feature-mapping.md) for complete list

---

## Vector Library Note

**Status:** ✅ ALREADY IN MAIN - Namespace updated

Vector math files already exist in main codebase at `src/Framework/OoBDev.System.Abstractions/Math/`:
- Vector files already integrated
- Namespace updated from `OoBDev.Common.Math` → `OoBDev.System.Math` (5 files)
- No migration needed

---

## Dependencies

**Blocks:**
- Cannot proceed with any Framework migration until Phase 0 complete
- All other migrations can proceed independently

**Required:**
- User time for Phase 0 comparison review
- Decision on namespace mapping strategy

---

## Risk Assessment

**HIGH RISK:**
- Breaking changes in DIFFERS files
- Namespace conflicts
- Merge conflicts with existing code

**MITIGATION:**
- Systematic Phase 0 comparison (CRITICAL)
- Backup before any changes
- Incremental migration with testing

---

**Effort:**
- Phase 0 Comparison: 3-5 days
- Migration (after Phase 0): TBD based on findings

**Related Documentation:**
- [Framework Feature Mapping](docs/migration/framework-feature-mapping.md)
- [Vector Comparison](docs/migration/vector-comparison.md)
