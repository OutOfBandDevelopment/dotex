# TODO - Bug Fixes & Technical Debt Epic

**Last Updated:** 2026-01-22

This document tracks bug fixes, breaking changes, and technical debt resolution.

> **Parent Document:** [TODO.md](./TODO.md)

---

## Active Work

### ⚠️ Build Warnings Resolution (8 warnings)

**Status:** 🟢 MOSTLY COMPLETE - 8 Remaining Warnings

**📋 Detailed Tracking:** [TODO-build-warnings.md](./TODO-build-warnings.md)

**Current State:**
- 8 build warnings remaining (down from 95+)
- Critical warnings resolved (CS1573, CS8604, CS8618, CS8620, CA2022, CA2024)
- Priority: Monitor but acceptable for now

**4-Phase Approach:**
1. **Phase 1: Investigation** - Capture and categorize all warnings
2. **Phase 2: Prioritization** - Assign priority levels (Critical → Low)
3. **Phase 3: Resolution** - Fix warnings systematically by category
4. **Phase 4: Enforcement** - Enable warnings-as-errors in CI/CD

**Quick Start:**
```bash
cd src
dotnet build > build-warnings.txt 2>&1
# Then analyze warnings by category
```

**Expected Categories:**
- CS1591 - Missing XML documentation (architectural standard)
- CS8600/CS8602/CS8604 - Nullable reference warnings
- CS8618 - Non-nullable field initialization
- CS0618 - Obsolete API usage
- OBDPK001 - Missing PackageReadmeFile
- Other codes (to be identified)

**Success Criteria:**
- [ ] Zero warnings in Release build
- [ ] All warnings categorized and documented
- [ ] Resolution strategy documented for each type
- [ ] Optional: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` enabled

**Related:**
- [architectural-standards.md](docs/architecture/architectural-standards.md) - XML documentation requirements
- [TODO-build-warnings.md](./TODO-build-warnings.md) - Detailed tracking with progress tables

---

## Completed Work

All completed bug fixes have been archived to change documents for reference:

### ✅ Build Warnings & Test Cleanup (2026-01-22)

**Summary:** Reduced build warnings from 95+ to 8, converted DevLocal tests to appropriate categories, and created .runsettings documentation.

**Impact:**
- Fixed critical warnings: CS1573 (XML docs), CS8604/CS8618 (nullable), CA2022/CA2024 (async patterns)
- Converted multiple DevLocal tests to Integration/Unit/LiveIntegration categories
- Fixed HtmlNavigatorTests with proper ComplexTemplate.html resource
- Fixed Redis integration tests (IObjectConverter dependency, JSON serialization)
- Created comprehensive .runsettings variables how-to guide

**Files Changed:**
- 24+ test files converted to appropriate categories
- `docs/how-tos/runsettings-variables-and-configuration.md` created
- `ComplexTemplate.html` test resource created
- Multiple compiler warning fixes across solution

---

### ✅ Swashbuckle 10.1.0 & .NET 10.0 Breaking Changes (2026-01-20)

**Summary:** Fixed all breaking changes from Swashbuckle 10.1.0 upgrade and .NET 10.0 migration + enabled XML documentation generation globally.

**Impact:**
- 5 files fixed (4 Swashbuckle, 1 .NET 10.0)
- 3 files for XML documentation
- All 65 projects build successfully
- Swagger Summary/Description properties now appear

**Details:** [docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md](docs/changes/bug-fixes-swashbuckle-dotnet10-2026-01-20.md)

---

### ✅ MSTest ExpectedExceptionAttribute Conversion (2026-01-15)

**Summary:** Converted all deprecated `[ExpectedException]` attributes to modern `Assert.ThrowsException<T>()` pattern.

**Impact:**
- 40 conversions across 24 files
- Framework (10), Binary Decoders (4), SharedFramework (26)
- All tests passing

**Details:** [docs/changes/bug-fixes-mstest-expected-exception-2026-01-15.md](docs/changes/bug-fixes-mstest-expected-exception-2026-01-15.md)

---

### ✅ Phase 0: Critical Bug Fixes (2026-01-15)

**Summary:** Fixed 6 critical bugs including syntax errors, non-functional implementations, and floating-point precision issues.

**Impact:**
- PathEx lambda syntax
- StreamDevice nullable/typo
- SerialPortFactory ternary
- ShiftCommutativeVariables stub → working implementation
- ExpressionParser precision
- NumericAsserts utility created

**Details:** [docs/changes/bug-fixes-phase0-critical-2026-01-15.md](docs/changes/bug-fixes-phase0-critical-2026-01-15.md)

---

## Known Issues

### To Investigate

- [ ] Check for other stub implementations in codebase

### Resolved

- [x] PathEx lambda syntax error - FIXED
- [x] StreamDevice nullable annotations - FIXED
- [x] StreamDevice transmission delay typo - FIXED
- [x] SerialPortFactory verbose ternary - FIXED
- [x] ShiftCommutativeVariablesRight stub - FIXED
- [x] ExpressionParserTests floating-point precision - FIXED (added epsilon tolerance)
- [x] NumericAsserts utility created - Migrated to OoBDev.TestUtilities for reuse
- [x] ExpressionCalculator optimizers - VERIFIED (all tests green)
- [x] ANTLR grammar versions - VERIFIED (all tests green)

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
- [ ] Update outdated packages to .NET 10-compatible versions
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
- [Security Audit](.claude/protocols/software/security-audit.md) - Security review protocol
- [Architectural Analysis](.claude/protocols/software/architectural-analysis.md) - Architecture review
