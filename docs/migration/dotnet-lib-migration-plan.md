# dotnet-lib Migration Plan

**Version:** 2.0 (INVESTIGATION COMPLETE)
**Last Updated:** 2026-01-12
**Source Repository:** dotnet-lib (`Incomming/dotnet-lib`)
**Target Repository:** OoBDev (dotex) Framework
**Status:** ✅ COMPLETE - All files synchronized, 1 critical bug fixed

---

## Executive Summary (Added 2026-01-12)

**Investigation Completed:** All 40 files have been compared with the main codebase.

**Key Findings:**
- **38 files (95%):** IDENTICAL to main codebase - No action needed
- **2 files (5%):** Differ from main codebase
  - ✅ **AdditionalSwaggerGenEndpointsOptions.cs** - Critical bug fixed in main (missing namespace for generic types)
  - 🔧 **HealthCheckSwaggerGenEndpointOptions.cs** - Stylistic difference only, no action needed

**Result:** dotnet-lib is **fully synchronized** with the main codebase. The critical bug discovered during comparison has been fixed.

**See:** [dotnet-lib Comparison Matrix](./dotnet-lib-comparison-matrix.md) for complete file-by-file analysis.

---

## Overview

This document provides a detailed, actionable migration plan for integrating dotnet-lib features into the OoBDev framework. Unlike BinaryDataDecoders, dotnet-lib contains **loose source files without .csproj files**, suggesting these are either:
- Extracted reference implementations
- Newer versions being prepared for integration
- Refactored code awaiting consolidation

The plan is organized by investigation and merge phases.

**Reference:** See [dotnet-lib-feature-mapping.md](./dotnet-lib-feature-mapping.md) for comprehensive feature comparison.

---

## Migration Principles

All migration work MUST follow these principles from `/docs/architecture`:

1. **Layered Architecture** - Verify correct layer placement
2. **Provider/Factory Pattern** - Ensure consistent patterns
3. **Dependency Injection** - TryAdd* extensions, builder pattern
4. **Type Safety** - Generic constraints, nullable enabled
5. **Testing** - 80% coverage minimum, MSTest, categorized
6. **Documentation** - README required, XML docs on public APIs
7. **No Breaking Changes** - Maintain backward compatibility

---

## Migration Scope

**ALL 40 files from dotnet-lib will be analyzed and integrated.**

- Phases indicate **priority order**, not feature selection
- Each file will be compared with main codebase equivalent
- Newer/better versions will update main codebase
- Missing files will be added to appropriate projects
- No files will be skipped

---

## Phase 0: Investigation & Analysis (IMMEDIATE)

**Priority:** CRITICAL
**Dependencies:** None
**Goal:** Determine which dotnet-lib files are newer/better than main codebase

### Task 0.1: File-by-File Comparison Matrix

**Create comparison spreadsheet for all 40 files:**

| dotnet-lib File | Main Codebase Location | Exists? | Version Comparison | Action |
|----------------|------------------------|---------|-------------------|--------|
| ConfigureOAuthSwaggerGenOptions.cs | Framework/OoBDev.AspNetCore.JwtAuthentication/ | ? | ? | ? |
| ... | ... | ... | ... | ... |

**Steps:**
1. For each of the 40 dotnet-lib files:
   - Search for exact filename in `/current/src/src/Framework/`
   - If found, note location
   - If not found, mark as NEW
2. For each found file:
   - Run git log to determine last modified date in both locations
   - Run diff to identify differences
   - Determine which version is newer
   - Identify improvements, bug fixes, or breaking changes
3. For each missing file:
   - Determine which project it should belong to
   - Verify project exists in main codebase
   - Plan addition

**Tools:**
```bash
# Search for file across main codebase
find /current/src/src/Framework -name "ConfigureOAuthSwaggerGenOptions.cs"

# Compare file versions
diff /current/src/Incomming/dotnet-lib/.../File.cs \
     /current/src/src/Framework/.../File.cs

# Check git history
git log --oneline -- /current/src/src/Framework/.../File.cs
git log --oneline -- /current/src/Incomming/dotnet-lib/.../File.cs
```

**Validation:**
- [ ] All 40 files catalogued
- [ ] Exists/Missing status determined
- [ ] Version comparison completed for existing files
- [ ] Newer versions identified
- [ ] Improvements documented
- [ ] Breaking changes flagged

---

### Task 0.2: Dependency Verification

**Verify all required dependencies exist in main codebase:**

| Dependency | Expected Location | Found? | Action if Missing |
|-----------|------------------|--------|-------------------|
| ConfigurationMissingException | OoBDev.System.Abstractions | ? | Create or verify |
| ISearchQuery | OoBDev.System.Abstractions/Linq/Search | ? | Create or verify |
| EmailMessageModel | OoBDev.Communications.Abstractions | ? | Create or verify |
| IMessageHandlerProvider | OoBDev.MessageQueueing.Abstractions | ? | Create or verify |
| IMessageQueueHandler | OoBDev.MessageQueueing.Abstractions | ? | Create or verify |
| IQueueMessage | OoBDev.MessageQueueing.Abstractions | ? | Create or verify |
| ITemplateSource | OoBDev.System.Abstractions | ? | Create or verify |
| IPostBuildExpressionVisitor | OoBDev.System.Linq | ? | Create or verify |
| ApplicationPermissionsApiFilter | OoBDev.AspNetCore.Mvc | ? | Create or verify |
| HealthChecksDocumentFilter | OoBDev.AspNetCore.Mvc | ? | Create or verify |
| MessageQueueAttribute | OoBDev.MessageQueueing.Abstractions | ? | Create or verify |
| RequestType enum | OoBDev.AspNetCore.Mvc | ? | Create or verify |

**Steps:**
1. Search for each dependency in main codebase
2. If missing, determine if it should be added from dotnet-lib (if defined there) or elsewhere
3. Document any missing critical dependencies

**Validation:**
- [ ] All 12 dependencies located or identified as missing
- [ ] Missing dependencies planned for creation
- [ ] No orphaned dependencies

---

### Task 0.3: Project Structure Verification

**Verify these projects exist in main codebase:**

- [ ] `src/Framework/OoBDev.AspNetCore.JwtAuthentication/`
- [ ] `src/Framework/OoBDev.AspNetCore.Mvc/`
- [ ] `src/Framework/OoBDev.MessageQueueing.Abstractions/`
- [ ] `src/Framework/OoBDev.MessageQueueing/`
- [ ] `src/Framework/OoBDev.Communications.Abstractions/`
- [ ] `src/Framework/OoBDev.System.Abstractions/`
- [ ] `src/Framework/OoBDev.System/`
- [ ] `src/Framework/OoBDev.System.Linq/`

**For each missing project:**
1. Determine if it should be created
2. Create proper .csproj file following standards
3. Add README.md (build-enforced)
4. Set up test project

**Validation:**
- [ ] All required projects exist or are created
- [ ] Each project has .csproj file
- [ ] Each project has README.md
- [ ] Test projects exist or are planned

---

## Phase 1: High-Priority Updates (ASP.NET Core & Search)

**Priority:** HIGH
**Dependencies:** Phase 0 complete
**Goal:** Integrate critical MVC and search functionality improvements

### Task 1.1: MVC Extensions (Model Binding & Swagger)

**Status:** UPDATE
**Target:** `src/Framework/OoBDev.AspNetCore.Mvc/`
**Files:** 6 files

**Steps:**
1. Compare dotnet-lib versions with main versions:
   - `ISearchModelBuilder.cs` + `SearchModelBuilder.cs`
   - `ISearchModelMapper.cs` + `SearchModelMapper.cs`
   - `AdditionalSwaggerGenEndpointsOptions.cs`
   - `AdditionalSwaggerUIEndpointsOptions.cs`
   - `HealthCheckSwaggerGenEndpointOptions.cs`

2. For each file:
   - If dotnet-lib is newer: Replace main with dotnet-lib version
   - If main is newer: Keep main, discard dotnet-lib
   - If both have improvements: Manually merge best of both

3. Verify dependencies:
   - Ensure `ISearchQuery`, `RequestType` exist
   - Ensure `ApplicationPermissionsApiFilter`, `HealthChecksDocumentFilter` exist

4. Add/update tests:
   - SearchModelBuilder tests (Form/JSON/Query binding)
   - SearchModelMapper tests (action descriptor resolution)
   - Swagger integration tests

5. Update README.md with examples

**Validation:**
- [ ] All 6 files integrated
- [ ] Tests pass
- [ ] No breaking changes
- [ ] Documentation updated

---

### Task 1.2: ComponentModel Search Attributes

**Status:** UPDATE
**Target:** `src/Framework/OoBDev.System.Abstractions/ComponentModel/Search/`
**Files:** 4 files

**Steps:**
1. Compare dotnet-lib versions with main:
   - `ISearchQueryIntercept.cs`
   - `SearchTermDefaultAttribute.cs`
   - `IgnoreStringComparisonReplacementAttribute.cs`
   - `SearchTermDefaults.cs`

2. Integrate newer versions

3. Add tests:
   - SearchTermDefaultAttribute behavior (EqualTo, StartsWith, EndsWith, Contains)
   - Wildcard pattern handling
   - Quote stripping

**Validation:**
- [ ] All 4 files integrated
- [ ] Search term interception working
- [ ] Tests comprehensive

---

### Task 1.3: ResponseModel API Results

**Status:** UPDATE
**Target:** `src/Framework/OoBDev.System.Abstractions/ResponseModel/`
**Files:** 8 files

**Steps:**
1. Compare dotnet-lib versions with main:
   - `IResult.cs`, `IModelResult.cs`, `IQueryResult.cs`, `IPagedQueryResult.cs`
   - `ResultMessage.cs`, `MessageLevels.cs`
   - `ICaptureResultMessage.cs`, `CaptureResultMessage.cs`

2. Pay special attention to:
   - Thread-safe `CaptureResultMessage` implementation with AsyncLocal
   - EnumValueAttribute mapping on MessageLevels
   - Paging metadata

3. Add tests:
   - Result message capture in async contexts
   - Concurrent message publishing
   - Paging calculations

**Validation:**
- [ ] All 8 files integrated
- [ ] Thread safety verified
- [ ] AsyncLocal context tested
- [ ] No breaking API changes

---

## Phase 2: Medium-Priority Updates (Message Queuing & Templating)

**Priority:** MEDIUM
**Dependencies:** Phase 0 complete
**Goal:** Integrate message queue and template improvements

### Task 2.1: Message Queuing

**Status:** UPDATE
**Files:** 4 files total (3 abstractions + 1 implementation)

**Steps:**
1. Abstractions:
   - Compare `IMessageHandlerProviderWrapped.cs`
   - Compare `IMessagePropertyResolver.cs`
   - Compare `WrappedQueueMessage.cs`

2. Implementation:
   - Compare `MessagePropertyResolver.cs`
   - Verify hierarchical config logic is correct

3. Add tests:
   - Hierarchical configuration resolution
   - Safe vs. throwing variants
   - MessageQueueAttribute extraction

**Validation:**
- [ ] All 4 files integrated
- [ ] Config hierarchy working
- [ ] Tests cover all resolution paths

---

### Task 2.2: Template Context System

**Status:** UPDATE
**Target:** `src/Framework/OoBDev.System.Abstractions/Text/Templating/`
**Files:** 5 files

**Steps:**
1. Compare all template-related files:
   - `IFileType.cs`, `FileType.cs`
   - `IFileTypeProvider.cs`
   - `ITemplateContext.cs`, `TemplateContext.cs`

2. Verify `ITemplateSource` dependency exists

3. Add tests:
   - File type registration
   - Template context creation
   - Priority-based selection

**Validation:**
- [ ] All 5 files integrated
- [ ] Template system working
- [ ] Tests comprehensive

---

### Task 2.3: JWT Authentication OAuth2 Integration

**Status:** UPDATE
**Target:** `src/Framework/OoBDev.AspNetCore.JwtAuthentication/`
**Files:** 3 files

**Steps:**
1. Compare:
   - `ConfigureOAuthSwaggerGenOptions.cs`
   - `ConfigureOAuthSwaggerUIOptions.cs`
   - `OAuth2SwaggerOptions.cs`

2. Verify PKCE flow configuration

3. Add integration tests with Swagger

**Validation:**
- [ ] All 3 files integrated
- [ ] OAuth2 Swagger working
- [ ] PKCE flow configured

---

## Phase 3: Low-Priority Updates (Remaining Files)

**Priority:** LOW
**Dependencies:** Phase 0 complete
**Goal:** Complete integration of all remaining files

### Task 3.1: ComponentModel Attributes

**Status:** UPDATE
**Files:** 4 files

- [ ] `EndStateAttribute.cs`
- [ ] `EnumValueAttribute.cs`
- [ ] `ExcludeFromUniqueAttribute.cs`
- [ ] `IVersionProvider.cs`

---

### Task 3.2: LINQ Expression Visitors

**Status:** UPDATE
**Files:** 2 files

- [ ] `ParameterReplacerExpressionVisitor.cs`
- [ ] `SkipInstanceMethodOnNullExpressionVisitor.cs`

**Verify:**
- Null safety injection working
- Expression tree rewriting correct

---

### Task 3.3: Miscellaneous

**Status:** UPDATE
**Files:** 3 files

- [ ] `ReceivedEmailMessageModel.cs` (Communications)
- [ ] `CommandParameterAttribute.cs` (Configuration)
- [ ] `ContentChunk.cs` (IO)

---

## Phase 4: Cleanup & Validation

**Priority:** LOW
**Dependencies:** Phases 0-3 complete
**Goal:** Finalize migration and archive dotnet-lib

### Task 4.1: Archive dotnet-lib Directory

**Once all files are integrated:**

1. Add README.md to `Incomming/dotnet-lib/` with archive notice:

```markdown
# dotnet-lib (ARCHIVED)

**Status:** ARCHIVED - All files migrated to main OoBDev framework

All files from this directory have been integrated into the main OoBDev codebase at:
- `/current/src/src/Framework/OoBDev.AspNetCore.*`
- `/current/src/src/Framework/OoBDev.MessageQueueing.*`
- `/current/src/src/Framework/OoBDev.System.*`

See [Migration Plan](../../docs/migration/dotnet-lib-migration-plan.md) for details.

**Date Archived:** 2026-01-XX
```

2. Consider moving to `old/dotnet-lib-archived/` for historical reference

**Validation:**
- [ ] All files confirmed migrated
- [ ] Archive notice added
- [ ] Directory moved or marked

---

### Task 4.2: Update Documentation

**Steps:**
1. Update architecture documentation if new projects were added
2. Update CHANGELOG.md with dotnet-lib integration notes
3. Create comparison report documenting what was updated

**Validation:**
- [ ] Architecture docs current
- [ ] CHANGELOG updated
- [ ] Comparison report created

---

### Task 4.3: Final Testing

**Comprehensive test pass:**
1. Run all unit tests
2. Run integration tests
3. Verify no breaking changes
4. Check code coverage (80%+ Framework layer)

**Validation:**
- [ ] All tests pass
- [ ] No breaking changes
- [ ] Coverage requirements met

---

## Completion Criteria

**Phase 0:**
- [ ] All 40 files compared and catalogued
- [ ] All dependencies verified
- [ ] All projects verified or created

**Phase 1:**
- [ ] MVC extensions integrated (6 files)
- [ ] Search attributes integrated (4 files)
- [ ] ResponseModel integrated (8 files)

**Phase 2:**
- [ ] Message queuing integrated (4 files)
- [ ] Template system integrated (5 files)
- [ ] JWT auth integrated (3 files)

**Phase 3:**
- [ ] ComponentModel attributes integrated (4 files)
- [ ] LINQ expression visitors integrated (2 files)
- [ ] Miscellaneous files integrated (3 files)

**Phase 4:**
- [ ] dotnet-lib archived
- [ ] Documentation updated
- [ ] All tests passing

**Total:** 40 files integrated, archived, and validated

---

## Risk Assessment

### Low Risk
- Simple file additions (missing in main)
- Newer dotnet-lib versions with clear improvements
- Non-breaking API enhancements

### Medium Risk
- Files where both versions have changes
- Complex merge scenarios
- Breaking API changes

### High Risk
- Files where dotnet-lib depends on missing interfaces
- Major architectural differences
- Thread-safety concerns

### Mitigation
1. Phase 0 investigation identifies all risks early
2. File-by-file comparison prevents mass changes
3. Comprehensive testing catches regressions
4. Rollback plan: Keep dotnet-lib until validation complete

---

## Related Documentation

- [dotnet-lib Feature Mapping](./dotnet-lib-feature-mapping.md)
- [Architectural Guidelines](../architecture/architectural-guidelines.md)
- [Architectural Standards](../architecture/architectural-standards.md)

---

## Change Log

- 2026-01-12 v1.0: Initial migration plan created
