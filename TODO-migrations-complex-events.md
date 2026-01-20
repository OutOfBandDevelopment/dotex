# Migration TODO - ComplexEvents

**Projects:** 4 projects (Abstractions, Common, DatabaseExtensions, EntityFrameworkCore)
**Source:** Incoming/SharedFramework/
**Status:** ✅ SAFE - Main has ZERO, no conflicts
**Priority:** MEDIUM

---

## Tasks

### Phase 1: ComplexEvents.Abstractions (NEW)
- [ ] Create `src/Framework/OoBDev.ComplexEvents.Abstractions/`
- [ ] Copy event sourcing/CQRS contracts
- [ ] Review IEventHubSource interface (uses CallerMemberName attributes)
- [ ] Update namespace to `OoBDev.ComplexEvents`
- [ ] Add to solution
- [ ] Create README

### Phase 2: ComplexEvents.Common (NEW)
- [ ] Create `src/Framework/OoBDev.ComplexEvents/`
- [ ] Copy common event handling implementation
- [ ] Update namespace
- [ ] Reference Abstractions
- [ ] Add ServiceCollectionExtensions
- [ ] Add to solution

### Phase 3: ComplexEvents.DatabaseExtensions (NEW)
- [ ] Create `src/Framework/OoBDev.ComplexEvents.DatabaseExtensions/`
- [ ] Copy database persistence for events
- [ ] Note: Currently netstandard2.0 (SQL database project - correct)
- [ ] Update namespace
- [ ] Reference ComplexEvents.Abstractions
- [ ] Add to solution

### Phase 4: ComplexEvents.EntityFrameworkCore (NEW)
- [ ] Create `src/Framework/OoBDev.ComplexEvents.EntityFrameworkCore/`
- [ ] Copy EF Core integration
- [ ] Add EF Core packages
- [ ] Update namespace
- [ ] Reference ComplexEvents abstractions
- [ ] Add ServiceCollectionExtensions
- [ ] Add to solution

### Phase 5: Testing
- [ ] Migrate ComplexEvents.Common.Tests
- [ ] Test event sourcing patterns
- [ ] Test CQRS patterns
- [ ] Test database persistence
- [ ] Test EF Core integration
- [ ] Target 80%+ coverage

### Phase 6: Documentation
- [ ] Document ComplexEvents architecture
- [ ] Document event sourcing pattern
- [ ] Document CQRS pattern
- [ ] Document event persistence strategies
- [ ] Add usage examples
- [ ] Create migration guide

### Phase 7: Integration
- [ ] Verify EF Core compatibility
- [ ] Test with SQL Server
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md

---

## Project Structure

```
src/Framework/
├── OoBDev.ComplexEvents.Abstractions/         # NEW - Event sourcing contracts
├── OoBDev.ComplexEvents/                      # NEW - Common implementation
├── OoBDev.ComplexEvents.DatabaseExtensions/   # NEW - SQL persistence
├── OoBDev.ComplexEvents.EntityFrameworkCore/  # NEW - EF Core integration
└── OoBDev.ComplexEvents.Tests/                # NEW - Tests
```

---

## Key Features

- ✅ Event sourcing support
- ✅ CQRS pattern implementation
- ✅ Event hub abstraction
- ✅ Database persistence
- ✅ EF Core integration
- ✅ Caller information tracking (via attributes)

---

## Architectural Pattern

**Event Sourcing:**
- Events as first-class citizens
- Event store persistence
- Event replay capability

**CQRS:**
- Command/Query separation
- Event-driven updates
- Read model synchronization

---

## LOC Summary

- ComplexEvents.Abstractions: ~200 LOC (estimated)
- ComplexEvents.Common: ~500 LOC (estimated)
- ComplexEvents.DatabaseExtensions: ~300 LOC (estimated)
- ComplexEvents.EntityFrameworkCore: ~400 LOC (estimated)
- **Total:** ~3,000 LOC (estimated from SF analysis)

---

**Effort:** 2-3 days
**Risk:** LOW - Completely new capability, no conflicts
