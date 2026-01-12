# Migration Documentation

**Last Updated:** 2026-01-12

---

## Overview

This directory contains migration plans and feature mappings for integrating code from legacy repositories into the OoBDev (dotex) framework.

---

## Active Migrations

### BinaryDataDecoders Migration

**Status:** Planning Complete
**Priority:** HIGH
**Documents:**
- [Feature Mapping](./binarydatadecoders-feature-mapping.md) - Comprehensive feature-by-feature comparison
- [Migration Plan](./binarydatadecoders-migration-plan.md) - Detailed actionable migration steps

**Quick Summary:**

**Source:** `Incomming/BinaryDecoders` (41 projects, ~800 C# files)

**Key Features to Migrate:**
1. **CRITICAL (Phase 0):** 5 bug fixes in existing OoBDev code
2. **HIGH (Phase 1):** Foundation utilities (endianness, BCD, collections)
3. **HIGH (Phase 2):** CodeAnalysis (Roslyn XPath), ExpressionCalculator, Archives
4. **MEDIUM (Phase 3):** NMEA GPS protocol, Drawing (selective)
5. **LOW (Phase 4):** Specialized features (FileSystems, Cryptography, Apple II)

**Critical Bugs Identified:**
- BUG-001: PathEx lambda bug (HIGH) - Wildcard matching broken
- BUG-002: StreamDevice nullable annotations (MEDIUM)
- BUG-003: StreamDevice event typo (MEDIUM)
- BUG-004: SerialPortFactory verbose ternary (LOW)
- BUG-005: ShiftCommutativeVariablesRight stub (CRITICAL) - Expression normalization broken

**Migration Status by Feature:**

| Feature | Status | Priority | Action |
|---------|--------|----------|--------|
| Bug Fixes (5) | UPDATE | CRITICAL | Fix immediately |
| ToolKit Foundation | UPDATE | CRITICAL | Merge into OoBDev.System |
| CodeAnalysis | NEW | HIGH | Create OoBDev.CodeAnalysis |
| ExpressionCalculator | UPDATE | CRITICAL | Fix broken optimizer |
| Archives (TAR/ZIP) | UPDATE | MEDIUM | Create OoBDev.Archives |
| NMEA GPS | NEW | MEDIUM | Create OoBDev.Protocols.Nmea |
| Drawing/Barcodes | NEW | MEDIUM | Evaluate need, modernize |
| Network Utils | EXISTS | LOW | Keep OoBDev |
| FileSystems (ISO) | NEW | LOW | Selective |
| Cryptography (Classic) | NEW | LOW | Educational only |
| Apple II | NEW | VERY LOW | Skip |
| Hardware (9 projects) | NEW | VERY LOW | Skip |
| WinForms UI | DELETE | N/A | Out of scope |

---

## Migration Phases

### Phase 0: Critical Bug Fixes (IMMEDIATE)
**Blocking:** All other phases
**Effort:** Small

Tasks:
1. Fix PathEx lambda bug
2. Fix StreamDevice nullable annotations
3. Fix StreamDevice event typo
4. Fix SerialPortFactory verbose ternary
5. Replace ShiftCommutativeVariablesRight stub

**Completion Criteria:**
- All bugs fixed
- All tests pass
- No warnings

---

### Phase 1: Foundation Enhancement
**Dependencies:** Phase 0
**Effort:** Medium

Tasks:
1. Migrate endianness types (BigEndianInt32, etc.)
2. Migrate FormattableNumber<T>
3. Migrate BCD converter
4. Migrate collections (DoubleLinkedList, ObservableDictionary)
5. Migrate threading utilities

**Completion Criteria:**
- All utilities merged into OoBDev.System
- 90%+ test coverage
- Documentation complete

---

### Phase 2: High-Value Features
**Dependencies:** Phase 0
**Effort:** Large

Tasks:
1. Migrate CodeAnalysis (Roslyn XPath navigation)
2. Complete ExpressionCalculator migration
3. Migrate Archives support (TAR/ZIP)

**Completion Criteria:**
- New Framework projects created
- Provider/factory pattern implemented
- 80%+ test coverage
- Full documentation

---

### Phase 3: Protocols & Extensions
**Dependencies:** Phase 0
**Effort:** Medium

Tasks:
1. Migrate NMEA GPS protocol decoder
2. Evaluate and migrate Drawing features (selective)

**Completion Criteria:**
- NMEA protocol operational
- Drawing uses modern library (SkiaSharp/ImageSharp)
- Tests comprehensive

---

### Phase 4: Specialized Features (Selective)
**Dependencies:** Phase 0
**Effort:** Small

Tasks:
1. Evaluate FileSystems (ISO 9660) - migrate if needed
2. Evaluate Classic Cryptography - migrate if educational use case
3. Skip: Apple II, Hardware devices, WinForms

**Completion Criteria:**
- Only business-justified features migrated
- Clear documentation on limitations

---

### Phase 5: Cleanup & Documentation
**Dependencies:** Phases 0-4
**Effort:** Small

Tasks:
1. Update cross-references
2. Create migration guide
3. Archive BinaryDataDecoders repository
4. Update CHANGELOG
5. Tag release

**Completion Criteria:**
- All documentation updated
- Migration guide complete
- Release tagged

---

## Architectural Compliance

All migrations MUST follow OoBDev architectural patterns from `/docs/architecture`:

**Layer Placement:**
- **Framework:** CodeAnalysis, Archives, Protocols, core utilities
- **Extensions:** Drawing, FileSystems, specialized features
- **ExternalServices:** Roslyn wrappers (CSharp, VisualBasic)

**Required Patterns:**
- Provider/Factory for all integrations
- Dependency Injection via TryAdd* extensions
- IOptions<T> for configuration
- Keyed services for multiple implementations

**Testing Standards:**
- Framework: 80% minimum coverage
- LINQ/Expression: 90% minimum coverage
- MSTest framework
- Test categories: Unit, Simulate

**Documentation Standards:**
- README.md required (enforced by build)
- XML documentation on all public APIs
- Usage examples
- PlantUML diagrams where appropriate

---

## Success Metrics

Migration is complete when:

1. ✅ All critical bugs fixed (Phase 0)
2. ✅ All HIGH priority features migrated
3. ✅ Architectural compliance verified
4. ✅ 80%+ test coverage maintained
5. ✅ Documentation complete
6. ✅ No breaking changes to existing APIs
7. ✅ Build succeeds without warnings
8. ✅ All tests pass
9. ✅ NuGet packages generated
10. ✅ Migration guide published

---

## Quick Start for Contributors

### Before Starting Any Migration:

1. **Read Architecture Docs:**
   - [Architectural Guidelines](../architecture/architectural-guidelines.md)
   - [Architectural Standards](../architecture/architectural-standards.md)
   - [Layering Architecture](../architecture/layering-architecture.md)

2. **Review Feature Mapping:**
   - [BinaryDataDecoders Feature Mapping](./binarydatadecoders-feature-mapping.md)

3. **Follow Migration Plan:**
   - [BinaryDataDecoders Migration Plan](./binarydatadecoders-migration-plan.md)

4. **Start with Phase 0:**
   - Fix critical bugs first
   - All other phases depend on Phase 0

### For Each Feature Migration:

1. Create abstractions project (if new feature)
2. Implement provider/factory pattern
3. Add DI registration via TryAdd* extension
4. Create comprehensive tests (80%+ coverage)
5. Write README.md and XML documentation
6. Verify build succeeds
7. Update migration tracking

---

## Migration Tracking

### Phase 0: Critical Bug Fixes
- [ ] BUG-001: PathEx lambda fix
- [ ] BUG-002: StreamDevice nullable annotations
- [ ] BUG-003: StreamDevice event typo
- [ ] BUG-004: SerialPortFactory simplification
- [ ] BUG-005: ShiftCommutativeVariablesRight replacement

### Phase 1: Foundation
- [ ] Endianness types (BigEndianInt32, etc.)
- [ ] FormattableNumber<T>
- [ ] BCD converter
- [ ] DoubleLinkedList<T>
- [ ] Threading utilities

### Phase 2: High-Value Features
- [ ] CodeAnalysis.Abstractions
- [ ] CodeAnalysis implementation
- [ ] Microsoft.CodeAnalysis.CSharp
- [ ] Microsoft.CodeAnalysis.VisualBasic
- [ ] ExpressionCalculator fixes
- [ ] Archives (TAR)
- [ ] Archives (ZIP enhancements)

### Phase 3: Protocols
- [ ] NMEA protocol decoder
- [ ] Drawing (if needed)

### Phase 4: Specialized
- [ ] FileSystems (if needed)
- [ ] Cryptography (if needed)

### Phase 5: Cleanup
- [ ] Migration guide
- [ ] CHANGELOG update
- [ ] BinaryDataDecoders archive
- [ ] Release tag

---

## Related Documentation

### Architecture
- [Architectural Guidelines](../architecture/architectural-guidelines.md)
- [Architectural Standards](../architecture/architectural-standards.md)
- [Architectural Patterns](../architecture/architectural-patterns.md)
- [Layering Architecture](../architecture/layering-architecture.md)

### Framework Documentation
- [Major Functionality](../Framework/MajorFunctionality.md)
- [Message Queueing](../Framework/MessageQueueing.md)
- [Text Templating](../Framework/TextTemplating.md)

### Migration
- [Feature Mapping](./binarydatadecoders-feature-mapping.md)
- [Migration Plan](./binarydatadecoders-migration-plan.md)

---

## Support

For questions or issues with migration:

1. Review feature mapping and migration plan
2. Check architectural documentation
3. Review existing OoBDev patterns
4. Document decisions and rationale

---

## Change Log

- 2026-01-12 v1.0: Initial migration documentation created
  - BinaryDataDecoders feature mapping complete
  - BinaryDataDecoders migration plan complete
  - Migration phases defined
  - Tracking checklist created
