# Documentation Standards Protocol

**Version:** 1.2
**Last Updated:** 2026-01-03
**Purpose:** Standards for organizing and maintaining project documentation

---

## Root Directory Rules

**ONLY these files are allowed at project root:**

| File | Purpose |
|------|---------|
| `README.md` | Project readme |
| `CLAUDE.md` | Claude Code memory/configuration |
| `TODO.md` | Work tracking |

**All other documentation MUST be under `./docs/`**

---

## Documentation Structure

All documentation must be collected under the `./docs` folder to enable wiki generation.

```
docs/
├── README.md                    # Index and navigation guide
│
├── architecture/                # System architecture and design patterns
│   ├── README.md                # TOC and entry point
│   ├── system-overview.md       # High-level system overview
│   ├── layer-architecture.md    # Architecture layers
│   ├── design-decisions.md      # ADRs and decisions
│   └── anti-patterns.md         # Patterns to AVOID
│
├── api/                         # API documentation
│   ├── endpoints.md             # API endpoints reference
│   ├── data-models.md           # Data model documentation
│   └── extensions.md            # API extensions/metadata
│
├── epics/                       # Epic/feature-based documentation
│   ├── {epic-name}/             # Epic folder
│   │   ├── implementation-guide.md
│   │   ├── test-plan.md
│   │   └── {feature}-proposal.md
│   │
│   └── ... more epics
│
├── features/                    # Cross-cutting features
│   ├── {feature-name}.md
│   └── ...
│
├── changes/                     # Change history
│   └── CHANGELOG.md
│
├── proposals/                   # Design proposals
│   └── {feature-name}.md
│
└── archive/                     # Historical documentation
    ├── sessions/                # Session memory files
    └── historical/              # Completed/outdated docs
```

---

## File Location Rules

| Type | Location | Example |
|------|----------|---------|
| Architecture docs | `docs/architecture/` | system-overview.md |
| API documentation | `docs/api/` | endpoints.md |
| Epic/feature docs | `docs/epics/{epic-name}/` | implementation-guide.md |
| Cross-cutting features | `docs/features/` | correlation-tracking.md |
| Change history | `docs/changes/` | CHANGELOG.md |
| Design proposals | `docs/proposals/` | bulk-actions.md |
| Historical docs | `docs/archive/` | session memories |
| Protocols | `.claude/protocols/` | documentation-standards.md |

---

## Naming Conventions

### Epic/Feature Naming

Features, proposals, and implementation guides use **epic/feature style naming**:

| Style | Convention | Example |
|-------|------------|---------|
| Epic | `{epic-name}.md` | `bulk-actions.md` |
| Feature | `{feature-name}.md` | `user-authentication.md` |
| Sub-feature | `{epic}-{feature}.md` | `bulk-actions-allocate.md` |

**DO NOT use:**
- Generic names like `SUGGESTED_CHANGES.md` or `CHANGES.md`
- Date-based names like `SESSION_2025-12-31.md`
- Underscores (use kebab-case)

### General Naming

| Type | Convention | Example |
|------|------------|---------|
| Main docs | `UPPER_CASE.md` | `ARCHITECTURE.md` |
| Guides | `kebab-case.md` | `swagger-extensions.md` |
| Test plans | `test-plan.md` (in epic folder) | `epics/bulk-actions/test-plan.md` |
| Proposals | `*-proposal.md` | `conditional-readonly-proposal.md` |
| Index files | `README.md` | `docs/README.md` |

---

## Document Standards

### Required Sections for Feature Documentation

1. **Overview** - What the feature does
2. **Usage** - Code examples
3. **Configuration** - Props, options, metadata
4. **Examples** - Real-world usage patterns
5. **Related** - Links to related docs

### Required Sections for Proposals

1. **Status** - Proposed/Approved/Implemented
2. **Date** - When proposed/implemented
3. **Problem Statement** - What problem it solves
4. **Proposed Solution** - How it will be solved
5. **Implementation Details** - Files to modify
6. **Questions** - Open items to resolve

### Required Sections for Architecture Documents

1. Document Information (date, status)
2. Overview/Purpose
3. Diagrams (PlantUML)
4. Details/Implementation
5. Related Documents

---

## Epic-Based Organization

Features should be organized by **epic** (major feature area):

| Epic | Description | Examples |
|------|-------------|----------|
| `authentication/` | Auth, roles, user management | role-system-guide.md |
| `bulk-actions/` | Bulk action features | implementation-guide.md |
| `code-generation/` | Templates, generators | swagger-extensions.md |
| `datagrid/` | DataGrid components | style-guide.md |
| `ui-migration/` | Framework migrations | migration-plan.md |

---

## Consolidation Rules

### Moving Scattered Documentation

If documentation exists outside `./docs`:

1. Identify the appropriate epic folder under `docs/epics/` or category folder
2. Move the file with a descriptive name (not date-based)
3. Update all references in code and other docs
4. Update `docs/README.md` index
5. If a pattern mistake was made, document in `docs/architecture/anti-patterns.md`

### When Documentation Becomes Outdated

1. Move to `docs/archive/historical/` if historical value
2. Move session memories to `docs/archive/sessions/`
3. Delete if no value
4. Update index files

### Creating New Epic Documentation

When starting a new feature/epic:

1. Create folder: `docs/epics/{epic-name}/`
2. Add implementation guide: `implementation-guide.md`
3. Add test plan if testable: `test-plan.md`
4. Add proposals if design needed: `{feature}-proposal.md`
5. Update `docs/README.md` with new epic

---

## Cross-Referencing

### Internal Links

Use relative paths from the document location:

```markdown
See [Architecture](./architecture/ARCHITECTURE.md)
See [API Patterns](../api/API_CALLS.md)
```

### Referencing from Code

When referencing docs in code comments:

```typescript
/**
 * Bulk action registry for grid actions.
 * @see docs/epics/bulk-actions/registry-guide.md
 */
```

### Document Index

Every `docs/` subdirectory should have a `README.md` with:
- Quick navigation table
- Document index
- Related documentation links

---

## Maintenance

### When to Update Documentation

1. After implementing new features
2. After architectural changes
3. After design decisions
4. When API patterns change
5. When templates are modified

### Review Checklist

- [ ] Only README.md, CLAUDE.md, TODO.md at root
- [ ] All docs under `./docs/` (not scattered in source)
- [ ] Protocols in `.claude/protocols/`
- [ ] Proposals use epic/feature naming (not date-based)
- [ ] Code examples are valid
- [ ] Cross-references working
- [ ] `docs/README.md` index updated

---

## Related Protocols

- [documentation-style-guide.md](./documentation-style-guide.md) - Content standards
- [architectural-analysis.md](../software/architectural-analysis.md) - Analysis procedure
- [task-management.md](../workflows/task-management.md) - Work tracking

---

## Change Log

- 2026-01-03 v1.2: Merged best practices from multiple versions, generalized
- 2025-12-31 v1.1: Added epic-based organization, root file restrictions
- 2025-12-31 v1.0: Initial protocol
