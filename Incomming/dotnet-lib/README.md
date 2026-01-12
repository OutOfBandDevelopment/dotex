# dotnet-lib (SYNCHRONIZED)

**Status:** SYNCHRONIZED with main OoBDev framework
**Date:** 2026-01-12
**Files:** 40 C# source files (no .csproj files)

---

## Overview

This directory contains 40 loose C# source files that were compared against the main OoBDev framework codebase. The investigation revealed that **all 40 files exist in the main codebase** and **38 out of 40 (95%) are byte-for-byte identical**.

## Investigation Results

### File Status

- **IDENTICAL:** 38 files (95%)
- **DIFFERS:** 2 files (5%)
  - AdditionalSwaggerGenEndpointsOptions.cs - **Main had bug** (fixed 2026-01-12)
  - HealthCheckSwaggerGenEndpointOptions.cs - Stylistic difference only
- **MISSING:** 0 files

### Bug Fix Applied

**File:** `AdditionalSwaggerGenEndpointsOptions.cs`
**Issue:** Main codebase was missing `{type.Namespace}.` prefix for generic types in `ResolveSchemaType` method
**Fix Date:** 2026-01-12
**Severity:** HIGH - Affected Swagger schema IDs for generic types

The bug was identified during the dotnet-lib comparison and fixed in the main codebase at:
- `/current/src/src/Framework/OoBDev.AspNetCore.Mvc/SwaggerGen/AdditionalSwaggerGenEndpointsOptions.cs:111-116`

## Purpose of dotnet-lib

Based on the investigation, this directory appears to be:
- A **reference copy** or backup of framework source files
- **Newer versions** being prepared for integration (in the case of the bug fix)
- **Extracted implementations** for review or comparison

## Migration Status

**Phase 0:** Investigation ✅ COMPLETE
- All 40 files catalogued
- File-by-file comparison completed
- Bug identified and documented

**Phase 1:** Critical Bug Fix ✅ COMPLETE
- AdditionalSwaggerGenEndpointsOptions.cs bug fixed in main
- Namespace prefix restored for generic types
- Swagger schema generation corrected

**Phase 2-4:** OBSOLETE
- No feature migration needed (all features already in main)
- All dependencies already exist
- All projects already exist

## File Inventory

### JWT Authentication (3 files)
- ConfigureOAuthSwaggerGenOptions.cs ✅ IDENTICAL
- ConfigureOAuthSwaggerUIOptions.cs ✅ IDENTICAL
- OAuth2SwaggerOptions.cs ✅ IDENTICAL

### MVC Extensions (7 files)
- ISearchModelBuilder.cs ✅ IDENTICAL
- SearchModelBuilder.cs ✅ IDENTICAL
- ISearchModelMapper.cs ✅ IDENTICAL
- SearchModelMapper.cs ✅ IDENTICAL
- AdditionalSwaggerGenEndpointsOptions.cs ⚠️ **BUG FIXED IN MAIN**
- AdditionalSwaggerUIEndpointsOptions.cs ✅ IDENTICAL
- HealthCheckSwaggerGenEndpointOptions.cs 🔧 STYLISTIC (no action needed)

### Search Attributes (4 files)
- ISearchQueryIntercept.cs ✅ IDENTICAL
- SearchTermDefaultAttribute.cs ✅ IDENTICAL
- IgnoreStringComparisonReplacementAttribute.cs ✅ IDENTICAL
- SearchTermDefaults.cs ✅ IDENTICAL

### ResponseModel (8 files)
- IResult.cs ✅ IDENTICAL
- IModelResult.cs ✅ IDENTICAL
- IQueryResult.cs ✅ IDENTICAL
- IPagedQueryResult.cs ✅ IDENTICAL
- ResultMessage.cs ✅ IDENTICAL
- MessageLevels.cs ✅ IDENTICAL
- ICaptureResultMessage.cs ✅ IDENTICAL
- CaptureResultMessage.cs ✅ IDENTICAL

### Message Queueing (4 files)
- IMessageHandlerProviderWrapped.cs ✅ IDENTICAL
- IMessagePropertyResolver.cs ✅ IDENTICAL
- WrappedQueueMessage.cs ✅ IDENTICAL
- MessagePropertyResolver.cs ✅ IDENTICAL

### Template Context (5 files)
- IFileType.cs ✅ IDENTICAL
- FileType.cs ✅ IDENTICAL
- IFileTypeProvider.cs ✅ IDENTICAL
- ITemplateContext.cs ✅ IDENTICAL
- TemplateContext.cs ✅ IDENTICAL

### ComponentModel Attributes (4 files)
- EndStateAttribute.cs ✅ IDENTICAL
- EnumValueAttribute.cs ✅ IDENTICAL
- ExcludeFromUniqueAttribute.cs ✅ IDENTICAL
- IVersionProvider.cs ✅ IDENTICAL

### LINQ Expression Visitors (2 files)
- ParameterReplacerExpressionVisitor.cs ✅ IDENTICAL
- SkipInstanceMethodOnNullExpressionVisitor.cs ✅ IDENTICAL

### Miscellaneous (3 files)
- ReceivedEmailMessageModel.cs ✅ IDENTICAL
- CommandParameterAttribute.cs ✅ IDENTICAL
- ContentChunk.cs ✅ IDENTICAL

## Main Codebase Locations

All files are located in `/current/src/src/Framework/`:

- **OoBDev.AspNetCore.JwtAuthentication/** - JWT/OAuth2 Swagger integration
- **OoBDev.AspNetCore.Mvc/** - MVC extensions, search model binding, Swagger filters
- **OoBDev.System.Abstractions/** - Core abstractions, ComponentModel, ResponseModel, Templating, Search
- **OoBDev.System/** - Core implementations, Template context
- **OoBDev.System.Linq/** - LINQ expression visitors
- **OoBDev.MessageQueueing.Abstractions/** - Message queue abstractions
- **OoBDev.MessageQueueing/** - Message queue implementations
- **OoBDev.Communications.Abstractions/** - Email message models

## Documentation

For detailed analysis, see:
- [dotnet-lib Comparison Matrix](../../docs/migration/dotnet-lib-comparison-matrix.md) - Complete file-by-file comparison
- [dotnet-lib Feature Mapping](../../docs/migration/dotnet-lib-feature-mapping.md) - Original feature analysis
- [dotnet-lib Migration Plan](../../docs/migration/dotnet-lib-migration-plan.md) - Original migration plan (now obsolete)

## Archival Status

**Current Status:** SYNCHRONIZED (not archived)

This directory serves as:
1. **Verification Source** - Reference copy for validating main codebase
2. **Bug Detection Tool** - Helped identify critical bug in main
3. **Historical Record** - Documents state of framework implementations

**Recommendation:** Keep in place as reference, no archival needed. The bug fix proves the value of maintaining this comparison copy.

---

**Last Updated:** 2026-01-12
**Comparison Tool:** File-by-file `diff` comparison across all 40 files
**Bug Fix Commit:** AdditionalSwaggerGenEndpointsOptions.cs namespace prefix restored
