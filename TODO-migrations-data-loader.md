# Migration TODO - DataLoader

**Projects:** DataLoader (2,146 LOC), DataLoader.Abstractions, DataLoader.Cli
**Source:** Incoming/SharedFramework/
**Status:** ✅ SAFE - Main has ZERO, no conflicts
**Priority:** MEDIUM

---

## Tasks

### Phase 1: DataLoader.Abstractions (NEW)
- [ ] Create `src/Framework/OoBDev.DataLoader.Abstractions/`
- [ ] Copy contracts from SF
- [ ] Update namespace to `OoBDev.DataLoader`
- [ ] Add to solution
- [ ] Create README

### Phase 2: DataLoader Core (2,146 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.DataLoader/`
- [ ] Copy data import/export implementation
- [ ] Update namespace
- [ ] Reference Abstractions
- [ ] Add ServiceCollectionExtensions
- [ ] Verify all data source integrations (CSV, Excel, JSON, XML, databases)
- [ ] Add to solution

### Phase 3: DataLoader.Cli (NEW)
- [ ] Create `src/Tools/OoBDev.DataLoader.Cli/`
- [ ] Copy CLI tool from SF
- [ ] Update namespace
- [ ] Reference DataLoader framework
- [ ] Test CLI functionality
- [ ] Create README with usage examples
- [ ] Add to solution

### Phase 4: Testing
- [ ] Migrate DataLoader.Tests
- [ ] Test CSV import/export
- [ ] Test Excel import/export
- [ ] Test JSON import/export
- [ ] Test XML import/export
- [ ] Test database loading
- [ ] Test CLI tool
- [ ] Target 80%+ coverage

### Phase 5: Documentation
- [ ] Document DataLoader architecture
- [ ] Document supported data sources
- [ ] Document transformation pipeline
- [ ] Document CLI usage
- [ ] Add usage examples for each data source
- [ ] Create migration guide

### Phase 6: Integration
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md
- [ ] Add to architectural patterns

---

## Project Structure

```
src/
├── Framework/
│   ├── OoBDev.DataLoader.Abstractions/    # NEW - Interfaces
│   ├── OoBDev.DataLoader/                 # NEW - Core (2,146 LOC)
│   └── OoBDev.DataLoader.Tests/           # NEW - Tests
└── Tools/
    └── OoBDev.DataLoader.Cli/             # NEW - CLI tool
```

---

## Key Features

- ✅ Data import from multiple sources
- ✅ Data export to multiple formats
- ✅ CSV support
- ✅ Excel support
- ✅ JSON support
- ✅ XML support
- ✅ Database loading
- ✅ Transformation pipeline
- ✅ CLI tool for automation

---

## LOC Summary

- DataLoader: 2,146 LOC
- DataLoader.Abstractions: ~100 LOC (estimated)
- DataLoader.Cli: ~50 LOC (estimated)
- **Total:** ~2,300 LOC

---

**Effort:** 2-3 days
**Risk:** LOW - Completely new capability, no conflicts
