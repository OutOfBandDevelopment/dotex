# Incomming/Framework Feature Mapping

**Version:** 1.0
**Last Updated:** 2026-01-12
**Source:** Incomming/Framework (`Incomming/Framework`)
**Target:** OoBDev (dotex) Framework
**Status:** 🔍 INVESTIGATION - Major differences found, migration required

---

## Executive Summary

**Incomming/Framework** is a comprehensive .NET framework suite (55 files, ~2,054 LOC) containing **significant new features and updates** compared to the main OoBDev codebase.

### Critical Finding

Unlike dotnet-lib (95% identical) or Oobtainium (completely new), Incomming/Framework is a **mix of new features and updates**:

- **0 files (0%)** - IDENTICAL to main codebase
- **25 files (45%)** - DIFFER from main (exist in both but have changes)
- **30 files (55%)** - NEW (only in Incomming, not in main)

### Main vs Incomming Comparison

| Aspect | Main OoBDev.Common | Incomming/Framework |
|--------|-------------------|---------------------|
| **Purpose** | Aggregate/umbrella project | Actual implementation |
| **Files** | 1 file (ServiceCollectionExtensions.cs) | 55 implementation files |
| **Projects** | 2 projects (aggregators) | 3 projects + tests |
| **Pattern** | References other projects | Contains implementations |
| **Target** | .NET 9.0 only | .NET 8.0 + 9.0 multi-targeting |

**Key Insight:** Main codebase's `OoBDev.Common` is a **meta-package** that aggregates Framework and Extensions projects. Incomming/Framework contains **actual implementations** that should be distributed across the Framework layer.

---

## Project Structure

###  Incomming/Framework Projects

1. **OoBDev.Common.Abstractions** (24 files, ~646 LOC)
   - Interfaces and contracts
   - Zero implementation dependencies
   - Target: .NET 8.0 + 9.0

2. **OoBDev.Common** (11 files, ~624 LOC)
   - Core implementations
   - Depends on Abstractions
   - Target: .NET 8.0 + 9.0

3. **OoBDev.AspNetCore.Extensions** (19 files, ~747 LOC)
   - ASP.NET Core specific features
   - Depends on Common
   - Target: .NET 8.0 + 9.0
   - **NEW - Does not exist in main codebase**

4. **OoBDev.Common.Tests** (1 file, ~37 LOC)
   - Unit tests for Vector math
   - MSTest framework

---

## Feature Breakdown

### NEW Features (30 files - 55%)

Features that **do not exist** in the main codebase:

#### 1. Audit Logging System (5 files)
**Location:** OoBDev.AspNetCore.Extensions/Middleware/

- `AuditLoggingMiddleware.cs` - Request/response capture middleware
- `AuditRequestAttribute.cs` - Mark endpoints for auditing
- `AuditLogEntry.cs` - Data model for log entries
- `AuditLogInfo.cs` - Request information
- `AuditLogResponse.cs` - Response information
- `IAuditLoggingRecorder.cs` - Persistence interface

**Purpose:**
- Capture HTTP request/response pairs
- Store request body, response body, exceptions
- Correlation tracking
- Configurable via attributes

**Migration Decision:** **MIGRATE** - Valuable for API auditing

---

#### 2. Vector Mathematics Library (5 files)
**Location:** OoBDev.Common/Math/, OoBDev.Common.Tests/Math/

- `Vector.cs` - Vector struct with math operations
- `VectorComparer.cs` - Equality comparison
- `VectorDistanceMetrics.cs` - Cosine, Euclidean, Manhattan, Dot Product
- `VectorMath.cs` - Normalization, magnitude
- `VectorTests.cs` - Unit tests

**Purpose:**
- AI/ML vector operations
- Semantic search similarity
- RAG (Retrieval-Augmented Generation) support

**Dependencies:** Used by SemanticKernel integration

**Migration Decision:** **MIGRATE** - Critical for AI/ML features

**Detailed Analysis:** See [Vector Comparison Document](./vector-comparison.md) for:
- Comparison with existing OoBDev.Data.Vectors (SQL CLR UDT)
- Coexistence strategy (application vs database vectors)
- Implementation differences and recommendations

---

#### 3. SQL Database Mapper (1 file)
**Location:** OoBDev.Common/Data/

- `SqlDatabaseMapper.cs` - ORM-like stored procedure mapper

**Features:**
- Attribute-driven stored procedure mapping
- Automatic parameter binding via reflection
- Expression tree-based result mapping
- IAsyncEnumerable streaming
- JSON parameter support

**Migration Decision:** **MIGRATE** - Core data access pattern

---

#### 4. User & Application Access (5 files)
**Location:** OoBDev.Common.Abstractions/ApplicationInputs/, OoBDev.AspNetCore.Extensions/

- `IApplicationAccess.cs` - Application context interface
- `ICurrentUserAccessor.cs` - User resolution interface
- `CurrentUserAccessor.cs` - Abstract base class
- `HttpCurrentUserAccessor.cs` - HTTP context implementation
- `HttpModelNameAccessor.cs` - Model name resolution

**Purpose:**
- Resolve current user in request pipeline
- Application-level context access
- Multi-tenancy support

**Migration Decision:** **MIGRATE** - Common pattern for web APIs

---

#### 5. Environment Settings (4 files)
**Location:** OoBDev.AspNetCore.Extensions/

- `IEnvironmentSettings.cs` - Environment configuration interface
- `EnvironmentSettings.cs` - Implementation
- `HostEnvironmentExtensions.cs` - IHostEnvironment extensions
- `HostExtensions.cs` - IHost extensions

**Purpose:**
- Centralized environment configuration
- OpenAPI options
- Host information

**Migration Decision:** **MIGRATE** - Standard configuration pattern

---

#### 6. HTTP Extensions (2 files)
**Location:** OoBDev.AspNetCore.Extensions/

- `HttpContextExtensions.cs` - HttpContext utilities
- `OoBDevClientOptions.cs` - Client configuration options

**Purpose:**
- HTTP context access helpers
- Client configuration

**Migration Decision:** **MIGRATE** - Useful utilities

---

#### 7. Swagger/OpenAPI Customizations (4 files)
**Location:** OoBDev.AspNetCore.Extensions/SwaggerGen/

- `OoBDevInternalAttribute.cs` - Mark internal endpoints
- `OoBDevInternalMiddleware.cs` - Hide internal endpoints
- `EriskInternalDocumentFilter.cs` - Swagger filter (note: "Erisk" branding)
- `OpenApiOptions.cs` - OpenAPI configuration

**Purpose:**
- Hide internal/admin endpoints from public API docs
- Custom Swagger filtering

**Migration Decision:** **REVIEW** - Check for "Erisk" branding to rename to "OoBDev"

---

#### 8. HTTP Request Preparation (2 files)
**Location:** OoBDev.Common/Net/Http/

- `OoBDevHttpPrepareRequest.cs` - OoBDev-specific HTTP request setup
- `OoBDevClientOptions.cs` - Client options

**Purpose:**
- Standardized HTTP client setup
- Request customization

**Migration Decision:** **MIGRATE** - Useful for HTTP client factory

---

#### 9. Validation Attributes (1 file)
**Location:** OoBDev.Common.Abstractions/ComponentModel/DataAnnotations/

- `QuoteIdAttribute.cs` - Custom quote ID validation

**Purpose:**
- Domain-specific validation

**Migration Decision:** **REVIEW** - Domain-specific, may need generalization

---

#### 10. JSON Serialization Wrapper (1 file)
**Location:** OoBDev.Common/Text/Json/

- `WrappedJsonSerializer.cs` - IJsonSerializer implementation with custom settings

**Purpose:**
- Centralized JSON configuration
- Debug vs Release formatting

**Migration Decision:** **MIGRATE** - Standard utility

---

### DIFFERS (25 files - 45%)

Files that **exist in both** but have differences:

#### Core Abstractions (8 files - Updated Interfaces)

1. `IAccessor.cs` - Async-local accessor pattern
2. `IDataConverter.cs` - Type conversion interface
3. `IDatabaseMapper.cs` - Database mapping interface
4. `IDatabaseQuery.cs` - Database query interface
5. `IHttpPrepareRequest.cs` - HTTP request customization
6. `IHttpPrepareRequestFeature.cs` - Middleware feature
7. `IJsonSerializer.cs` - JSON serialization interface
8. `Accessor.cs` - Accessor implementation

**Analysis Required:**
- Need to diff each file to identify improvements
- May have bug fixes or new methods
- Could have breaking changes

**Migration Decision:** **COMPARE & MERGE** - Take best of both versions

---

#### Core Implementations (8 files - Updated Features)

1. `DataConverter.cs` - Type conversion implementation
2. `DatabaseQuery.cs` - Query execution
3. `HttpPrepareRequest.cs` - HTTP preparation base
4. `CorrelationInfo.cs` - Correlation tracking
5. `CorrelationInfoMiddleware.cs` - Middleware implementation
6. `CorrelationInfoHttpPrepareRequestFeature.cs` - Feature implementation
7. `ServiceCollectionExtensions.cs` (appears twice - both projects)

**Analysis Required:**
- Compare implementations for improvements
- Check for bug fixes
- Verify backward compatibility

**Migration Decision:** **COMPARE & MERGE**

---

#### Attributes (7 files - Enhanced Validation)

1. `ConnectionStringNameAttribute.cs` - Database connection attribute
2. `QueryParameterAttribute.cs` - Stored procedure parameters
3. `QueryResultAttribute.cs` - Result mapping
4. `StoredProcedureAttribute.cs` - SP mapping
5. `ZipCodeAttribute.cs` - Zip code validation
6. `ZipCodesAttribute.cs` - Multi-zip validation

**Analysis Required:**
- Check for validation improvements
- Verify regex patterns
- Test coverage

**Migration Decision:** **COMPARE & MERGE**

---

#### ASP.NET Core (2 files - Middleware Updates)

1. `ApplicationBuilderExtensions.cs` - Pipeline configuration
2. `AdditionalSwaggerGenEndpointsOptions.cs` - Swagger customization *(already fixed in dotnet-lib migration!)*

**Analysis Required:**
- AdditionalSwaggerGenEndpointsOptions may already be fixed
- ApplicationBuilderExtensions may have new middleware

**Migration Decision:** **COMPARE** - AdditionalSwaggerGenEndpointsOptions already fixed

---

#### Constants (1 file)

1. `DefinedHttpHeaders.cs` - HTTP header constants

**Analysis Required:**
- May have new headers defined

**Migration Decision:** **COMPARE & MERGE**

---

## Migration Complexity Assessment

### Straightforward Migrations (Low Complexity)

**Estimated Effort:** LOW to MEDIUM

- Vector math library (self-contained, tested)
- Validation attributes (simple classes)
- Environment settings (configuration pattern)
- HTTP extensions (utility methods)
- JSON serializer wrapper (simple wrapper)

### Moderate Complexity

**Estimated Effort:** MEDIUM

- Audit logging middleware (needs IAuditLoggingRecorder implementation)
- User accessor pattern (needs integration with identity system)
- Correlation tracking (middleware ordering matters)

### High Complexity

**Estimated Effort:** MEDIUM to HIGH

- SQL Database Mapper (complex reflection, expression trees)
- File comparisons (25 DIFFERS files need individual review)
- Namespace decisions (where to place in Framework hierarchy)
- Swagger customizations (check for branding issues)

---

## Branding Concerns

**Found "Erisk" branding in:**
- `EriskInternalDocumentFilter.cs`

**Action Required:**
- Rename to `OoBDevInternalDocumentFilter.cs`
- Search for other "Erisk" references
- Ensure consistent "OoBDev" branding

---

## Namespace Mapping Strategy

### Proposed Mapping

| Incomming Namespace | Target Main Namespace | Project |
|---------------------|----------------------|---------|
| OoBDev.Common.ApplicationInputs | OoBDev.System.ApplicationInputs | OoBDev.System.Abstractions |
| OoBDev.Common.ComponentModel | OoBDev.System.ComponentModel | OoBDev.System.Abstractions |
| OoBDev.Common.Data | OoBDev.Data.Common | OoBDev.Data.Common |
| OoBDev.Common.Math | OoBDev.System.Math | OoBDev.System (NEW) |
| OoBDev.Common.Net.Http | OoBDev.System.Net.Http | OoBDev.System |
| OoBDev.Common.Text.Json | OoBDev.System.Text.Json | OoBDev.System |
| OoBDev.AspNetCore.Extensions.* | OoBDev.AspNetCore.Mvc.* | OoBDev.AspNetCore.Mvc |

**Key Decision:** Should "OoBDev.Common" become "OoBDev.System" in main framework?

---

## Dependencies Analysis

### External Dependencies (Incomming)
- Microsoft.Data.SqlClient 6.0.1
- Microsoft.Extensions.* 9.0.1
- Swashbuckle.AspNetCore 7.2.0
- Microsoft.IdentityModel.Logging 8.3.1
- Asp.Versioning.Mvc.ApiExplorer 8.1.0

### Main Codebase Dependencies
- Similar Microsoft.Extensions.* 9.0.x
- May need to add Microsoft.Data.SqlClient if not present

**Compatibility:** Good - versions align well

---

## Migration Decision Matrix

| Feature | Priority | Complexity | Recommendation |
|---------|----------|------------|----------------|
| Vector Math | HIGH | LOW | MIGRATE - Critical for AI/ML |
| SQL Database Mapper | HIGH | HIGH | MIGRATE - Core data access |
| Audit Logging | MEDIUM | MEDIUM | MIGRATE - Valuable feature |
| User Accessor | MEDIUM | MEDIUM | MIGRATE - Common pattern |
| Environment Settings | MEDIUM | LOW | MIGRATE - Configuration |
| Correlation Middleware | MEDIUM | MEDIUM | COMPARE - May exist |
| Swagger Customizations | LOW | LOW | REVIEW - Check branding |
| HTTP Extensions | LOW | LOW | MIGRATE - Utilities |
| Validation Attributes | LOW | LOW | COMPARE & MERGE |
| 25 DIFFERS files | HIGH | HIGH | **SYSTEMATIC COMPARISON REQUIRED** |

---

## Recommended Migration Approach

### Phase 0: Systematic Comparison (CRITICAL)

**Must complete before migration:**

1. **File-by-File Diff Analysis**
   - Compare all 25 DIFFERS files
   - Identify bug fixes, new features, breaking changes
   - Document differences in comparison matrix
   - Determine which version is newer/better

2. **Branding Audit**
   - Search for "Erisk" references
   - Plan renaming strategy

3. **Namespace Decision**
   - Decide on final namespace structure
   - Map Incomming to main Framework projects

### Phase 1: New Features Migration

**Priority: HIGH**
- Vector Math Library
- SQL Database Mapper
- Audit Logging Middleware

### Phase 2: File Merging

**Priority: HIGH**
- Merge 25 DIFFERS files (best of both versions)
- Apply bug fixes
- Maintain backward compatibility

### Phase 3: Testing & Validation

**Priority: HIGH**
- 80% test coverage
- Integration testing
- Breaking change analysis

---

## Questions Requiring Answers

1. **Namespace Strategy:**
   - Should "OoBDev.Common" map to "OoBDev.System"?
   - Or keep as separate "OoBDev.Common" project?

2. **OoBDev.AspNetCore.Extensions:**
   - Merge into existing OoBDev.AspNetCore.Mvc?
   - Or keep as separate project?

3. **Vector Math:**
   - Where should it live? (OoBDev.System.Math? New project?)
   - Is this for SemanticKernel integration?

4. **SQL Database Mapper:**
   - Should this be in OoBDev.Data.Common?
   - Or new OoBDev.Data.SqlServer project?

5. **Audit Logging:**
   - What persistence implementation is needed?
   - Database? File? Custom?

6. **Branding:**
   - Confirm all "Erisk" references should become "OoBDev"
   - Any other branding concerns?

---

## Related Documents

- [Framework Migration Plan](./framework-migration-plan.md) - Detailed migration tasks
- [Vector Comparison](./vector-comparison.md) - Detailed analysis of vector implementations
- [Architectural Guidelines](../architecture/architectural-guidelines.md)

---

## Change Log

- 2026-01-12 v1.0: Initial feature mapping created
