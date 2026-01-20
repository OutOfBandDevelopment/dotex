# TODO - Bug Fixes & Technical Debt Epic

**Last Updated:** 2026-01-20

This document tracks bug fixes, breaking changes, and technical debt resolution.

> **Parent Document:** [TODO.md](./TODO.md)

---

## Active Work

*No active bug fixes at this time. All known critical bugs resolved.*

---

## Completed Work

All completed bug fixes have been archived to change documents for reference:

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
