# Migration TODO - TextTemplating

**Projects:** TextTemplating (424 LOC), TextTemplating.Abstractions (117 LOC)
**Source:** Incoming/SharedFramework/
**Status:** ⚠️ CONSOLIDATION - Main scattered across projects, SF unified
**Priority:** MEDIUM

---

## Tasks

### Phase 1: Analysis of Scattered Implementation
- [ ] Audit `OoBDev.System` for template abstractions
- [ ] Audit `OoBDev.Tools/OoBDev.TemplateEngine.Cli` for CLI implementation
- [ ] Audit `OoBDev.Handlebars` integration
- [ ] Document current template usage patterns
- [ ] Identify what to consolidate vs keep separate

### Phase 2: TextTemplating.Abstractions (117 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.TextTemplating.Abstractions/`
- [ ] Copy centralized contracts from SF
- [ ] Update namespace to `OoBDev.TextTemplating`
- [ ] Add to solution
- [ ] Create README

### Phase 3: TextTemplating Core (424 LOC - NEW)
- [ ] Create `src/Framework/OoBDev.TextTemplating/`
- [ ] Copy SF implementation (template engine, repository, cache)
- [ ] Update namespace
- [ ] Reference Abstractions
- [ ] Add ServiceCollectionExtensions
- [ ] Add to solution

### Phase 4: Move Scattered Abstractions
- [ ] Identify template abstractions in OoBDev.System
- [ ] Move to OoBDev.TextTemplating.Abstractions
- [ ] Update references in System
- [ ] Ensure no breaking changes

### Phase 5: Handlebars Integration
- [ ] Create `src/Framework/OoBDev.TextTemplating.Handlebars/`
- [ ] Wrap existing OoBDev.Handlebars as provider
- [ ] Implement ITemplateEngine interface
- [ ] Keep OoBDev.Handlebars for direct use
- [ ] Add adapter for unified TextTemplating system

### Phase 6: CLI Integration
- [ ] Update OoBDev.TemplateEngine.Cli to use unified framework
- [ ] Integrate with TextTemplating abstractions
- [ ] Test CLI functionality
- [ ] Update CLI documentation

### Phase 7: Testing
- [ ] Migrate TextTemplating tests from SF
- [ ] Test template repository/persistence
- [ ] Test template caching
- [ ] Test Handlebars provider
- [ ] Test CLI integration
- [ ] Target 80%+ coverage

### Phase 8: Documentation
- [ ] Document unified templating architecture
- [ ] Document template persistence
- [ ] Document caching strategy
- [ ] Document provider pattern (Handlebars, future providers)
- [ ] Add usage examples
- [ ] Create migration guide

### Phase 9: Integration
- [ ] Build entire solution
- [ ] Run all tests
- [ ] Update TODO.md
- [ ] Update architectural patterns

---

## Project Structure

```
src/
├── Framework/
│   ├── OoBDev.TextTemplating.Abstractions/    # NEW - Centralized contracts
│   ├── OoBDev.TextTemplating/                 # NEW - Core implementation
│   ├── OoBDev.TextTemplating.Handlebars/      # NEW - Handlebars provider
│   ├── OoBDev.TextTemplating.Tests/           # NEW - Tests
│   ├── OoBDev.System/                         # UPDATE - Move abstractions out
│   └── OoBDev.Handlebars/                     # KEEP - Direct Handlebars use
└── Tools/
    └── OoBDev.TemplateEngine.Cli/             # UPDATE - Use unified framework
```

---

## Current State (Scattered)

**Main Issues:**
- ⚠️ Template abstractions scattered in OoBDev.System
- ⚠️ CLI separate from framework
- ⚠️ Handlebars integration not unified
- ⚠️ No template persistence
- ⚠️ No template caching

**SF Provides:**
- ✅ Unified TextTemplating framework
- ✅ Template repository pattern
- ✅ Template caching
- ✅ Centralized abstractions
- ✅ Metadata management

---

## Key Features

**Adding from SF:**
- ✅ Unified templating framework
- ✅ Template repository/persistence
- ✅ Template caching
- ✅ Template resolution
- ✅ Metadata management
- ✅ Centralized contracts

**Keeping from Main:**
- ✅ Handlebars integration (as provider)
- ✅ CLI tool (integrated)

---

## Provider Pattern

```
ITemplateEngine
├── HandlebarsTemplateEngine (wrap OoBDev.Handlebars)
├── RazorTemplateEngine (future)
└── ScribanTemplateEngine (future)
```

---

## LOC Summary

- SF TextTemplating: 424 LOC
- SF TextTemplating.Abstractions: 117 LOC
- Main scattered: ~200 LOC
- Handlebars provider wrapper: ~50 LOC (new)
- **Total Unified:** ~600 LOC

---

**Effort:** 2-3 days
**Risk:** MEDIUM - Requires consolidation, integration with existing tools
