# Design Documentation - Text Templating Extensions

**Epic:** 10 - Text Templating Extensions
**Status:** 📝 DESIGN PHASE (Replaces code migration)
**Priority:** MEDIUM
**Strategy:** Design-first approach with comprehensive documentation before implementation

---

## Overview

**Strategic Change (2026-01-22):** Instead of migrating code from SharedFramework, we are creating comprehensive design documentation following the Epic 11 pattern. This ensures:
- Clean architecture from first principles
- Modern .NET 10.0 patterns throughout
- Proper integration with IDataContainer, schema discovery, and path translation
- No technical debt from legacy code
- Complete test coverage from day one

---

## Documentation Tasks

### ✅ Completed
- Architecture captured in `Features/Proposals/REVISIONS_SUMMARY.md` (Revision 9: Template Discovery)
- High-level design in `Features/Proposals/CONSOLIDATED_DESIGN.md` (Epic 10)

### 🔄 In Progress
Creating detailed 4-document sets for each feature:

**Feature 1: Template Discovery** (4 documents)
- [ ] `Features/Proposals/10-TextTemplating/TemplateDiscovery/requirements.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateDiscovery/architecture.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateDiscovery/api-design.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateDiscovery/testing-strategy.md`

**Feature 2: Template Repository & Persistence** (4 documents)
- [ ] `Features/Proposals/10-TextTemplating/TemplateRepository/requirements.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateRepository/architecture.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateRepository/api-design.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateRepository/testing-strategy.md`

**Feature 3: Template Caching** (4 documents)
- [ ] `Features/Proposals/10-TextTemplating/TemplateCaching/requirements.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateCaching/architecture.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateCaching/api-design.md`
- [ ] `Features/Proposals/10-TextTemplating/TemplateCaching/testing-strategy.md`

**Feature 4: Engine Provider Pattern** (4 documents)
- [ ] `Features/Proposals/10-TextTemplating/EngineProvider/requirements.md`
- [ ] `Features/Proposals/10-TextTemplating/EngineProvider/architecture.md`
- [ ] `Features/Proposals/10-TextTemplating/EngineProvider/api-design.md`
- [ ] `Features/Proposals/10-TextTemplating/EngineProvider/testing-strategy.md`

---

## Key Architectural Decisions

**From REVISIONS_SUMMARY.md:**

### Template Discovery (Revision 9)
- `ITemplateDiscovery` for runtime template enumeration
- `ITemplateRegistry` for pluggable template sources
- Metadata-driven discovery (template name, version, engine, categories)
- Integration with IDataContainer for schema-aware templates

### Template Engine Provider Pattern
- `ITemplateEngine` base interface (Handlebars, Razor, Scriban, etc.)
- `ITemplateEngineProvider` for pluggable implementations
- `ITemplateEngineRegistry` for runtime discovery
- Support for existing OoBDev.Handlebars as first provider

### Integration Points
- **IDataContainer**: Templates bind to container for dynamic data
- **Schema Discovery**: Templates declare required schemas
- **Path Translation**: Templates use wildcard patterns (`Customer/*/Address`)
- **Template Repository**: Persistent storage for templates
- **Template Caching**: Performance optimization with cache invalidation

---

## Architecture Highlights

**Key Interfaces:**
```csharp
// Template discovery
public interface ITemplateDiscovery
{
    Task<IEnumerable<ITemplateMetadata>> DiscoverAsync(CancellationToken ct);
    Task<ITemplateMetadata?> GetMetadataAsync(string templateId, CancellationToken ct);
}

// Template registry
public interface ITemplateRegistry
{
    void RegisterSource(ITemplateSource source);
    IEnumerable<ITemplateSource> GetAllSources();
    Task<ITemplate?> LoadTemplateAsync(string templateId, CancellationToken ct);
}

// Template engine abstraction
public interface ITemplateEngine
{
    string EngineType { get; }
    Task<string> RenderAsync(ITemplate template, IDataContainer data, CancellationToken ct);
    bool SupportsEngine(string engineType);
}

// Template repository
public interface ITemplateRepository
{
    Task<ITemplate?> GetTemplateAsync(string templateId, CancellationToken ct);
    Task SaveTemplateAsync(ITemplate template, CancellationToken ct);
    Task DeleteTemplateAsync(string templateId, CancellationToken ct);
}

// Template caching
public interface ITemplateCache
{
    Task<ITemplate?> GetCachedTemplateAsync(string templateId, CancellationToken ct);
    Task CacheTemplateAsync(string templateId, ITemplate template, TimeSpan expiration, CancellationToken ct);
    Task InvalidateAsync(string templateId, CancellationToken ct);
}
```

---

## Implementation Strategy

**Phase 1: Design Documentation (Current)**
1. Complete all 16 design documents (4 per feature)
2. Review and validate designs
3. Get stakeholder approval

**Phase 2: Implementation (Future)**
1. Implement based on approved designs
2. Follow provider/factory pattern consistently
3. Write tests alongside implementation (TDD)
4. Target 85-90% test coverage
5. Integrate with existing OoBDev.Handlebars

**Phase 3: Migration Path (Future)**
1. Create adapters for existing Handlebars implementation
2. Add template discovery for existing templates
3. Implement repository for template persistence
4. Add caching layer for performance
5. Consolidate scattered abstractions from OoBDev.System
6. Update CLI tool to use unified framework

---

## Reference Materials

**Design Documents:**
- `Features/Proposals/REVISIONS_SUMMARY.md` - Revision 9: Template Discovery
- `Features/Proposals/CONSOLIDATED_DESIGN.md` - Epic 10: Text Templating Extensions
- `Features/Proposals/11-DataEnhancement/` - Pattern reference (Epic 11)

**Existing Implementation:**
- `Framework/OoBDev.System/` - Scattered template abstractions
- `Framework/OoBDev.Handlebars/` - Handlebars integration to adapt
- `Tools/OoBDev.TemplateEngine.Cli/` - CLI tool to integrate

---

## Success Criteria

- ✅ 16 comprehensive design documents created (requirements, architecture, api-design, testing-strategy)
- ✅ All architectural patterns consistent with Epic 11
- ✅ Integration points with IDataContainer, schema discovery, path translation defined
- ✅ Provider pattern for all template engines (Handlebars, Razor, Scriban)
- ✅ Template repository and caching strategies defined
- ✅ Test coverage targets defined (85-90%)
- ✅ Migration path from scattered abstractions planned

---

## Notes

**Why Design-First?**
- Avoids technical debt from legacy SharedFramework code
- Ensures modern .NET 10.0 patterns throughout
- Integrates cleanly with new Epic 11 features (IDataContainer, path translation, schema discovery)
- Enables comprehensive test planning upfront
- Documents intent before implementation
- Consolidates scattered implementations

**Benefits:**
- Clean architecture from scratch
- No namespace migrations needed
- Proper dependency injection from start
- Built-in extensibility with provider pattern
- Complete test coverage planned upfront
- Unified templating framework (vs scattered)

---

**See Also:**
- [Design Progress](Features/Proposals/DOCUMENTATION_PROGRESS.md)
- [Epic 11 Documentation](Features/Proposals/11-DataEnhancement/) - Reference implementation
