# dotnet-lib to OoBDev Feature Mapping

**Version:** 2.0 (INVESTIGATION COMPLETE)
**Last Updated:** 2026-01-12
**Source:** dotnet-lib (`Incomming/dotnet-lib`)
**Target:** OoBDev (dotex) Framework
**Status:** ✅ COMPLETE - Comparison finished, 1 bug fixed

---

## Overview

The dotnet-lib directory contains a collection of **40 C# source files** (no .csproj files) organized into 8 namespaces. These files appear to be extracted implementations or reference code for advanced ASP.NET Core functionality, message queuing, search capabilities, and core infrastructure components.

**Investigation Result:** ALL 40 files exist in main codebase, 95% are identical, 1 critical bug was discovered and fixed.

---

## Executive Summary

**dotnet-lib Statistics:**
- Total Files: 40 C# files
- Total Lines of Code: ~1,582
- Namespaces: 8 distinct areas
- Project Files: **0** (loose source files only)
- Target Framework: Unknown (no .csproj)

**Investigation Status (COMPLETE):**
- **IDENTICAL**: 38 files (95%) - No action needed
- **BUG FIXED**: 1 file - AdditionalSwaggerGenEndpointsOptions.cs (critical namespace bug in main codebase)
- **STYLISTIC ONLY**: 1 file - HealthCheckSwaggerGenEndpointOptions.cs (no functional difference)
- **MISSING**: 0 files (all exist in main)

**Result:** dotnet-lib is fully synchronized with the main codebase. The investigation uncovered a critical bug in the main codebase that has been fixed.

**See:** [dotnet-lib Comparison Matrix](./dotnet-lib-comparison-matrix.md) for detailed file-by-file analysis.

---

## Part 1: ASP.NET Core Extensions

### 1.1 JWT Authentication & OAuth2 Swagger Integration

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.AspNetCore.JwtAuthentication/`
**Target:** `src/Framework/OoBDev.AspNetCore.JwtAuthentication/`
**Priority:** MEDIUM

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| OAuth2 Swagger Gen Options | ✓ ConfigureOAuthSwaggerGenOptions.cs | Check if exists | Compare & UPDATE |
| OAuth2 Swagger UI Options | ✓ ConfigureOAuthSwaggerUIOptions.cs | Check if exists | Compare & UPDATE |
| OAuth2SwaggerOptions | ✓ OAuth2SwaggerOptions.cs | Check if exists | Compare & UPDATE |

**Files in dotnet-lib:**
1. `ConfigureOAuthSwaggerGenOptions.cs` - Configures Swagger code generation with OAuth2 security definitions
2. `ConfigureOAuthSwaggerUIOptions.cs` - Configures Swagger UI for OAuth2 (PKCE flow)
3. `OAuth2SwaggerOptions.cs` - Configuration options (AuthorizationUrl, TokenUrl, UserReadApiClaim)

**Dependencies:**
- Requires `ConfigurationMissingException` (should exist in main)
- Uses `IConfigureOptions<T>` pattern

**Investigation Required:**
- [ ] Does `OoBDev.AspNetCore.JwtAuthentication` project exist in main?
- [ ] Do these files exist in main codebase?
- [ ] Are dotnet-lib versions newer with improvements?
- [ ] Compare line-by-line for differences

---

### 1.2 MVC Extensions (Model Binding & Swagger)

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.AspNetCore.Mvc/`
**Target:** `src/Framework/OoBDev.AspNetCore.Mvc/`
**Priority:** HIGH

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| Search Model Builder | ✓ ISearchModelBuilder + Impl | Check if exists | Compare & UPDATE |
| Search Model Mapper | ✓ ISearchModelMapper + Impl | Check if exists | Compare & UPDATE |
| Advanced Swagger Gen | ✓ AdditionalSwaggerGenEndpointsOptions | Check if exists | Compare & UPDATE |
| Advanced Swagger UI | ✓ AdditionalSwaggerUIEndpointsOptions | Check if exists | Compare & UPDATE |
| Health Check Swagger | ✓ HealthCheckSwaggerGenEndpointOptions | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (6 files):**
1. `ISearchModelBuilder.cs` + `SearchModelBuilder.cs` - Extracts search models from HTTP context
2. `ISearchModelMapper.cs` + `SearchModelMapper.cs` - Maps HTTP context to action descriptors
3. `AdditionalSwaggerGenEndpointsOptions.cs` - Groups endpoints by assembly with permissions
4. `AdditionalSwaggerUIEndpointsOptions.cs` - Multi-assembly Swagger UI dropdown
5. `HealthCheckSwaggerGenEndpointOptions.cs` - Health check documentation filter

**Key Features:**
- Intelligent model binding from Form/JSON/Query parameters
- Multi-content-type support
- Assembly-based Swagger documentation grouping
- Permission-based API filtering

**Dependencies:**
- Requires `ISearchQuery`, `RequestType` enum
- Requires `ApplicationPermissionsApiFilter`, `HealthChecksDocumentFilter`

**Investigation Required:**
- [ ] Does `OoBDev.AspNetCore.Mvc` project exist in main?
- [ ] Which files exist and which are missing?
- [ ] Are dotnet-lib versions newer?
- [ ] Compare implementations for improvements

---

## Part 2: Message Queuing

### 2.1 Message Queuing Abstractions

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.MessageQueueing.Abstractions/`
**Target:** `src/Framework/OoBDev.MessageQueueing.Abstractions/`
**Priority:** MEDIUM

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| IMessageHandlerProviderWrapped | ✓ | Check if exists | Compare & UPDATE |
| IMessagePropertyResolver | ✓ | Check if exists | Compare & UPDATE |
| WrappedQueueMessage | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (3 files):**
1. `IMessageHandlerProviderWrapped.cs` - Fluent builder for message handlers
2. `IMessagePropertyResolver.cs` - Property resolution with safe/throwing variants
3. `WrappedQueueMessage.cs` - Record type for queue messages

**Dependencies:**
- Requires `IMessageHandlerProvider`, `IMessageQueueHandler`, `IQueueMessage`

---

### 2.2 Message Queuing Implementation

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.MessageQueueing/`
**Target:** `src/Framework/OoBDev.MessageQueueing/`
**Priority:** MEDIUM

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| MessagePropertyResolver | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (1 file):**
1. `MessagePropertyResolver.cs` - Hierarchical configuration lookup implementation

**Key Features:**
- Hierarchical config: `{Root}:{Target}:{Message}` → `{Root}:{Message}` → `{Root}:{Target}` → `{Root}:Default`
- Safe and throwing variants for all operations
- MessageQueueAttribute-based simple name extraction

**Investigation Required:**
- [ ] Does implementation exist in main?
- [ ] Is dotnet-lib version newer with better logic?

---

## Part 3: Communications

### 3.1 Communications Abstractions

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.Communications.Abstractions/`
**Target:** `src/Framework/OoBDev.Communications.Abstractions/`
**Priority:** LOW

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| ReceivedEmailMessageModel | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (1 file):**
1. `ReceivedEmailMessageModel.cs` - Inbound email message model

**Dependencies:**
- Requires `EmailMessageModel` base class

---

## Part 4: System Abstractions & Core

### 4.1 ComponentModel (Attributes)

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Abstractions/ComponentModel/`
**Target:** `src/Framework/OoBDev.System.Abstractions/ComponentModel/`
**Priority:** MEDIUM

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| EndStateAttribute | ✓ | Check if exists | Compare & UPDATE |
| EnumValueAttribute | ✓ | Check if exists | Compare & UPDATE |
| ExcludeFromUniqueAttribute | ✓ | Check if exists | Compare & UPDATE |
| IVersionProvider | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (4 files):**
1. `EndStateAttribute.cs` - Marks valid end states for state machines
2. `EnumValueAttribute.cs` - Custom JSON serialization for enums
3. `ExcludeFromUniqueAttribute.cs` - Excludes from uniqueness validation
4. `IVersionProvider.cs` - Assembly version metadata interface

---

### 4.2 ComponentModel.Search

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Abstractions/ComponentModel/Search/`
**Target:** `src/Framework/OoBDev.System.Abstractions/ComponentModel/Search/`
**Priority:** HIGH

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| ISearchQueryIntercept | ✓ | Check if exists | Compare & UPDATE |
| SearchTermDefaultAttribute | ✓ | Check if exists | Compare & UPDATE |
| IgnoreStringComparisonReplacementAttribute | ✓ | Check if exists | Compare & UPDATE |
| SearchTermDefaults enum | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (4 files):**
1. `ISearchQueryIntercept.cs` - Search query manipulation interface
2. `SearchTermDefaultAttribute.cs` - Default search behavior (EqualTo, StartsWith, EndsWith, Contains)
3. `IgnoreStringComparisonReplacementAttribute.cs` - Property exclusion
4. `SearchTermDefaults.cs` - Enum for search behavior

**Key Features:**
- Attribute-based search term manipulation
- Quote stripping for exact phrases
- Wildcard pattern handling
- Per-class default search behavior

---

### 4.3 Configuration

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Abstractions/Configuration/`
**Target:** `src/Framework/OoBDev.System.Abstractions/Configuration/`
**Priority:** LOW

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| CommandParameterAttribute | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (1 file):**
1. `CommandParameterAttribute.cs` - CLI parameter binding metadata

---

### 4.4 IO

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Abstractions/IO/`
**Target:** `src/Framework/OoBDev.System.Abstractions/IO/`
**Priority:** LOW

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| ContentChunk | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (1 file):**
1. `ContentChunk.cs` - Record type for content segments

---

### 4.5 ResponseModel

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Abstractions/ResponseModel/`
**Target:** `src/Framework/OoBDev.System.Abstractions/ResponseModel/`
**Priority:** HIGH

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| IResult | ✓ | Check if exists | Compare & UPDATE |
| IModelResult | ✓ | Check if exists | Compare & UPDATE |
| IQueryResult | ✓ | Check if exists | Compare & UPDATE |
| IPagedQueryResult | ✓ | Check if exists | Compare & UPDATE |
| ResultMessage | ✓ | Check if exists | Compare & UPDATE |
| MessageLevels | ✓ | Check if exists | Compare & UPDATE |
| ICaptureResultMessage | ✓ | Check if exists | Compare & UPDATE |
| CaptureResultMessage | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (8 files):**
1. `IResult.cs` - Base result interface with messages
2. `IModelResult.cs` - Generic result with model data
3. `IQueryResult.cs` - Query result interface
4. `IPagedQueryResult.cs` - Paged query results
5. `ResultMessage.cs` - Record type for messages
6. `MessageLevels.cs` - Enum with EnumValueAttribute mapping
7. `ICaptureResultMessage.cs` - Result message capture interface
8. `CaptureResultMessage.cs` - Thread-safe, async-context-aware implementation

**Key Features:**
- Comprehensive API response model hierarchy
- Thread-safe message capture with AsyncLocal
- Paging support with metadata
- Message levels with custom JSON serialization

---

### 4.6 Text.Templating

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Abstractions/Text/Templating/`
**Target:** `src/Framework/OoBDev.System.Abstractions/Text/Templating/`
**Priority:** MEDIUM

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| IFileType | ✓ | Check if exists | Compare & UPDATE |
| FileType | ✓ | Check if exists | Compare & UPDATE |
| IFileTypeProvider | ✓ | Check if exists | Compare & UPDATE |
| ITemplateContext | ✓ | Check if exists | Compare & UPDATE |
| TemplateContext | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (5 files):**
1. `IFileType.cs` + `FileType.cs` - File type metadata (extension, MIME type, template flag)
2. `IFileTypeProvider.cs` - Collection of file types
3. `ITemplateContext.cs` + `TemplateContext.cs` - Template processing context with priority

**Dependencies:**
- Requires `ITemplateSource` interface

---

## Part 5: System & LINQ

### 5.1 System (Core)

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System/`
**Target:** `src/Framework/OoBDev.System/`
**Priority:** LOW

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| No files present | - | - | N/A |

**Note:** Directory exists but contains no files in dotnet-lib

---

### 5.2 System.Linq (Expression Visitors)

**Status:** UPDATE (needs comparison)
**Source:** `Incomming/dotnet-lib/OoBDev.System.Linq/`
**Target:** `src/Framework/OoBDev.System.Linq/`
**Priority:** MEDIUM

#### Feature Breakdown

| Feature | dotnet-lib | OoBDev Main | Action |
|---------|-----------|-------------|--------|
| ParameterReplacerExpressionVisitor | ✓ | Check if exists | Compare & UPDATE |
| SkipInstanceMethodOnNullExpressionVisitor | ✓ | Check if exists | Compare & UPDATE |

**Files in dotnet-lib (2 files):**
1. `ParameterReplacerExpressionVisitor.cs` - Internal parameter replacement for LINQ
2. `SkipInstanceMethodOnNullExpressionVisitor.cs` - Adds null checks to instance method calls

**Key Features:**
- Expression tree manipulation
- Automatic null safety injection
- LINQ query transformation

**Dependencies:**
- Requires `IPostBuildExpressionVisitor` interface

---

## Part 6: Critical Investigation Tasks

### 6.1 File-by-File Comparison Required

**For EACH of the 40 files in dotnet-lib:**
1. Search for corresponding file in `/current/src/src/Framework/`
2. If exists:
   - Run diff to compare versions
   - Determine which is newer
   - Identify improvements/bug fixes
3. If missing:
   - Confirm it should be added
   - Determine correct project location

**Validation:**
- [ ] All 40 files compared against main codebase
- [ ] Newer versions identified
- [ ] Missing files catalogued
- [ ] Improvements documented

---

### 6.2 Dependency Verification

**Verify these dependencies exist in main codebase:**
- [ ] `ConfigurationMissingException`
- [ ] `ISearchQuery` interface
- [ ] `EmailMessageModel` base class
- [ ] `IMessageHandlerProvider`, `IMessageQueueHandler`, `IQueueMessage`
- [ ] `ITemplateSource` interface
- [ ] `IPostBuildExpressionVisitor` interface
- [ ] `ApplicationPermissionsApiFilter`, `HealthChecksDocumentFilter`
- [ ] `MessageQueueAttribute`
- [ ] `RequestType` enum

---

## Migration Strategy Matrix

| Category | Files | Status | Priority | Target Layer | Action |
|----------|-------|--------|----------|--------------|--------|
| JWT Auth | 3 | UPDATE | MEDIUM | Framework | Compare & merge |
| MVC Extensions | 6 | UPDATE | HIGH | Framework | Compare & merge |
| Message Queue Abstractions | 3 | UPDATE | MEDIUM | Framework | Compare & merge |
| Message Queue Impl | 1 | UPDATE | MEDIUM | Framework | Compare & merge |
| Communications | 1 | UPDATE | LOW | Framework | Compare & merge |
| ComponentModel | 4 | UPDATE | MEDIUM | Framework | Compare & merge |
| Search | 4 | UPDATE | HIGH | Framework | Compare & merge |
| Configuration | 1 | UPDATE | LOW | Framework | Compare & merge |
| IO | 1 | UPDATE | LOW | Framework | Compare & merge |
| ResponseModel | 8 | UPDATE | HIGH | Framework | Compare & merge |
| Templating | 5 | UPDATE | MEDIUM | Framework | Compare & merge |
| LINQ Expressions | 2 | UPDATE | MEDIUM | Framework | Compare & merge |

**Total:** 40 files across 12 feature areas, all UPDATE status

---

## Key Questions to Answer

1. **When were dotnet-lib files created?** Check git history
2. **Why are they in a separate directory?** Refactoring? Migration prep?
3. **Are they newer or older than main?** Version comparison needed
4. **Should they replace main versions?** Or vice versa?
5. **Are there any breaking changes?** API compatibility check
6. **Why no .csproj files?** Were they extracted for a reason?

---

## Related Documentation

- [Architectural Guidelines](../architecture/architectural-guidelines.md)
- [Architectural Standards](../architecture/architectural-standards.md)
- [BinaryDataDecoders Migration](./binarydatadecoders-feature-mapping.md)

---

## Change Log

- 2026-01-12 v1.0: Initial feature mapping created from exploration
