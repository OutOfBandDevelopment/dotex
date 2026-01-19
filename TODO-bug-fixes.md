# TODO - Bug Fixes & Technical Debt Epic

**Last Updated:** 2026-01-19

This document tracks bug fixes, breaking changes, and technical debt resolution.

> **Parent Document:** [TODO.md](./TODO.md)

---

## Active Work

### OoBDev.AspNetCore.Mvc - Swashbuckle 10.1.0 Breaking Changes (IN PROGRESS)

**Issue:** Assembly updates introduced breaking changes in Swashbuckle 10.1.0 API

**Changes Made (2026-01-15):**

**File: FormFileOperationFilter.cs**
- [x] Fixed CS0200: `IOpenApiSchema.Properties` is read-only
  - Changed from: `schema.Properties = new Dictionary<...>()`
  - Changed to: Loop through fileParams and add to `schema.Properties[propertyName]`
  - Location: Lines 34-43
- [x] Fixed CS0029: String to JsonSchemaType conversion
  - Changed from: `Type = "string"`
  - Changed to: `Type = JsonSchemaType.String`
  - Location: Line 40

**File: HealthChecksDocumentFilter.cs**
- [x] Fixed CS1503: `OpenApiTag` vs `OpenApiTagReference` mismatch
  - Changed from: `operation.Tags.Add(new OpenApiTag { Name = "ApiHealth" })`
  - Changed to: `operation.Tags.Add(new OpenApiTagReference { Name = "ApiHealth" })`
  - Location: Line 28
- [x] Fixed CS0200 & CS0266: Properties assignment and type conversion
  - Changed from: `Properties = properties` (assignment)
  - Changed to: Build schema with `.Properties.Add()` calls
  - Location: Lines 30-36

**File: SearchQueryOperationFilter.cs**
- [x] Fixed CS1503: OpenApiTag references (3 occurrences)
  - Changed from: `new OpenApiTag { Name = ... }`
  - Changed to: `new OpenApiTagReference { Name = ... }`
  - Location: Lines 48, 52, 57
- [x] Fixed CS0019: Coalesce assignment with mismatched types
  - Changed from: `operation.Parameters ??= new List<OpenApiParameter>()`
  - Changed to: Traditional null check and assignment
  - Location: Lines 114-118
- [x] Fixed UpdateRequestSchema method (major refactor)
  - Changed from: Creating mutable dictionary copy with `.ChangeComparer()`
  - Changed to: Direct schema property access via `schema.Properties[key]`
  - Removed unnecessary `.Properties = properties` reassignment
  - Added null checks for schema lookups
  - Fixed IOpenApiSchema vs OpenApiSchema conversions
  - Location: Lines 187-292

**Remaining:**
- [ ] Verify build: `dotnet build src/Framework/OoBDev.AspNetCore.Mvc/OoBDev.AspNetCore.Mvc.csproj`
- [ ] Verify tests pass: `dotnet test src/ --filter "OoBDev.AspNetCore.Mvc"`
- [ ] Check for any remaining compiler errors in related filters

**Breaking Change Summary:**
- `OpenApiTag` collection now expects `OpenApiTagReference` objects
- `IOpenApiSchema.Properties` is read-only collection (use `.Add()` instead of assignment)
- `JsonSchemaType` is enum (not string) - use enum values like `JsonSchemaType.String`, `JsonSchemaType.Object`, etc.
- Schema reference API requires proper null checking on `?.Reference`

---

## Completed Work

### Phase 0: Critical Bug Fixes (COMPLETED)

All critical bugs fixed and verified:

- [x] **PathEx.cs** - Fixed lambda expression syntax error (lines 42, 66, 92)
- [x] **StreamDevice.cs** - Fixed nullable annotations and transmission delay typo
- [x] **SerialPortFactory.cs** - Simplified verbose ternary expression
- [x] **ShiftCommutativeVariablesRight.cs** - Replaced non-functional stub with working implementation
- [x] **ExpressionParserTests.cs** - Fixed floating-point precision test failures by adding epsilon tolerance
- [x] **NumericAsserts.cs** - Created reusable numeric comparison utility in OoBDev.TestUtilities

### MSTest ExpectedExceptionAttribute Conversion (COMPLETED - 2026-01-15)

**Task:** Convert all `[ExpectedException(typeof(ExceptionType))]` attributes to use `Assert.ThrowsException<T>()`

**Completed:** All 40 instances across 24 files converted

**Files Converted:**

*Framework Layer (4 files, 10 conversions):*
- OoBDev.TestUtilities.Tests/NumericAssertsTests.cs (6)
- OoBDev.MessageQueueing.Tests/MessageSenderTests.cs (1)
- OoBDev.System.Tests/ExpressionCalculator/Parser/ExpressionParserTests.cs (2)
- OoBDev.System.Tests/ExpressionCalculator/Expressions/VariableExpressionTests.cs (2)

*Binary Decoders (2 files, 4 conversions):*
- BinaryDecoders/BinaryDataDecoders.ExpressionCalculator.Tests/Parser/ExpressionParserTests.cs (2)
- BinaryDecoders/BinaryDataDecoders.ExpressionCalculator.Tests/Expressions/VariableExpressionTests.cs (2)

*SharedFramework (18 files, 26 conversions):*
- OoBDev.DocumentCenter.Tests (4 files, 4 conversions)
- OoBDev.Communications.Tests (12 files, 16 conversions)
- OoBDev.DataLoader.Tests (1 file, 1 conversion)
- OoBDev.Caching.Common.Tests (1 file, 1 conversion)
- OoBDev.Api.Twilio.* (3 files, 3 conversions)
- OoBDev.Generations.Tests (1 file, 1 conversion)

**Conversion Details:**
- Removed `[ExpectedException(typeof(ExceptionType))]` attributes
- Wrapped test body in `Assert.ThrowsException<T>(() => { ... })` for sync tests
- Wrapped test body in `await Assert.ThrowsExceptionAsync<T>(async () => { ... })` for async tests
- Preserved all test logic, comments, and variable declarations
- Maintained proper indentation and code structure

**Verification:** Code conversions manually verified by inspection of NumericAssertsTests.cs and CommunicationProviderTests.cs (both showing correct patterns)

### Build Verification (COMPLETED)

- [x] Run `dotnet build src/` to verify all bug fixes compile
- [x] Run `dotnet test src/` to verify all tests pass - ALL PASSED
- [x] Address any compilation or test failures - None found

---

## Known Issues

### To Investigate

- [ ] Verify ExpressionCalculator optimizers match incoming implementation
- [ ] Verify ANTLR grammar versions match
- [ ] Check for other stub implementations in codebase

### Resolved

- [x] PathEx lambda syntax error - FIXED
- [x] StreamDevice nullable annotations - FIXED
- [x] StreamDevice transmission delay typo - FIXED
- [x] SerialPortFactory verbose ternary - FIXED
- [x] ShiftCommutativeVariablesRight stub - FIXED
- [x] ExpressionParserTests floating-point precision - FIXED (added epsilon tolerance)
- [x] NumericAsserts utility created - Migrated to OoBDev.TestUtilities for reuse

---

## Technical Debt

### Code Quality

- [ ] Review all projects for consistent error handling patterns
- [ ] Audit logging middleware standards (see TODO-migrations.md Phase 2.2)
- [ ] Consolidate duplicate utilities across projects
- [ ] Ensure all public APIs have XML documentation

### Testing

- [ ] Achieve 80%+ code coverage for all Framework layer projects
- [ ] Add integration tests for all external service providers
- [ ] Add performance benchmarks for critical paths
- [ ] Migrate all DevLocal tests to appropriate categories (see TODO-testing-infrastructure.md)

### Documentation

- [ ] Verify all projects have README.md files (build-enforced)
- [ ] Create migration guides for breaking changes
- [ ] Document all configuration patterns
- [ ] Add code examples to all major features

### Dependencies

- [ ] Audit NuGet package versions for security vulnerabilities
- [ ] Update outdated packages to .NET 9-compatible versions
- [ ] Remove unused package references
- [ ] Document all external dependencies

---

## Future Improvements

### Architecture

- [ ] Implement consistent retry policies across external services
- [ ] Add circuit breaker pattern for resilience
- [ ] Standardize authentication/authorization patterns
- [ ] Create architectural decision records (ADRs)

### Performance

- [ ] Profile hot paths and optimize
- [ ] Add caching where appropriate
- [ ] Optimize memory allocations in high-frequency code
- [ ] Add performance regression tests

### Security

- [ ] Run security audit (use `.claude/protocols/software/security-audit.md`)
- [ ] Implement security headers middleware
- [ ] Add input validation standards
- [ ] Document secure coding practices

### Observability

- [ ] Standardize structured logging
- [ ] Add distributed tracing support
- [ ] Create monitoring dashboards
- [ ] Document observability best practices

---

## Reference

**Related Documents:**
- [TODO.md](./TODO.md) - Main tracking document
- [TODO-migrations.md](./TODO-migrations.md) - Migration work
- [TODO-decisions.md](./TODO-decisions.md) - Pending decisions
- [TODO-testing-infrastructure.md](./TODO-testing-infrastructure.md) - Testing infrastructure

**Architecture Documentation:**
- [Architectural Standards](docs/architecture/architectural-standards.md) - Enforceable standards
- [Architectural Guidelines](docs/architecture/architectural-guidelines.md) - Best practices
- [Architectural Patterns](docs/architecture/architectural-patterns.md) - Pattern catalog

**Protocols:**
- [Security Audit](. claude/protocols/software/security-audit.md) - Security review protocol
- [Architectural Analysis](.claude/protocols/software/architectural-analysis.md) - Architecture review
