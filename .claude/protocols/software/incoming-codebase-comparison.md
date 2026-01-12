# Incoming Codebase Comparison Protocol

**Version:** 1.0
**Last Updated:** 2026-01-12
**Purpose:** Standardized procedure for comparing incoming codebases with the main OoBDev framework to create comprehensive migration plans

---

## Overview

This protocol guides the systematic comparison of an incoming codebase (legacy repository, acquisition, or migration candidate) with the OoBDev framework. It produces:

1. **Feature Mapping Document** - Feature-by-feature comparison with status classification
2. **Migration Plan** - Detailed, actionable migration steps organized by priority
3. **Bug Report** - Critical issues discovered in either codebase
4. **Tracking Checklist** - Migration progress tracking

All outputs follow OoBDev architectural standards and patterns.

---

## When to Use

- Initial assessment of code in `Incomming/` directory
- Planning migration from legacy repository
- Evaluating acquired codebase for integration
- Comparing external library for custom implementation
- Quarterly review of pending migrations

---

## Prerequisites

- [ ] Access to incoming codebase directory
- [ ] Access to main OoBDev `src/` directory
- [ ] [Architectural Guidelines](../../docs/architecture/architectural-guidelines.md) reviewed
- [ ] [Architectural Standards](../../docs/architecture/architectural-standards.md) reviewed
- [ ] [Layering Architecture](../../docs/architecture/layering-architecture.md) reviewed
- [ ] Write access to create documentation

---

## Procedure

### Step 1: Initial Assessment

Gather basic information about the incoming codebase.

**1.1: Identify Location**
```bash
# Note the incoming directory path
INCOMING_DIR="Incomming/{DirectoryName}"
```

**1.2: Quick Stats**
```bash
# Count projects
find $INCOMING_DIR -name "*.csproj" | wc -l

# Count source files
find $INCOMING_DIR -name "*.cs" | wc -l

# Identify solution files
find $INCOMING_DIR -name "*.sln"

# Check for documentation
find $INCOMING_DIR -name "README.md" -o -name "*.md"
```

**Validation:**
- [ ] Directory path confirmed
- [ ] Project count estimated
- [ ] Source file count estimated
- [ ] Existing documentation identified

---

### Step 2: Deep Exploration of Incoming Codebase

Use the Explore agent to thoroughly understand the incoming codebase.

**2.1: Launch Exploration Agent**

Create a task with the following prompt template:

```
I need you to thoroughly explore the `{INCOMING_DIR}` directory and provide a comprehensive understanding of:

1. What projects/libraries exist in this directory
2. What functionality they provide
3. The architecture and patterns used
4. Key classes, interfaces, and implementations
5. Dependencies and external libraries used
6. Any unique features or capabilities
7. Code quality indicators (tests, documentation)
8. Current state (actively maintained, deprecated, etc.)

Be very thorough - I need to understand everything this codebase offers so I can compare it to the main src codebase and plan a migration.
```

**Task Parameters:**
- Agent: `Explore`
- Thoroughness: `very thorough`
- Model: `sonnet` (for comprehensive analysis)

**Expected Output:**
Comprehensive report covering:
- Project inventory (count, names, purposes)
- Functionality breakdown by area
- Architecture patterns identified
- Key abstractions and implementations
- Dependency analysis
- Test coverage assessment
- Documentation quality
- Code quality indicators

**Validation:**
- [ ] All projects cataloged
- [ ] Functionality areas identified
- [ ] Patterns documented
- [ ] Dependencies listed
- [ ] Test coverage assessed
- [ ] Code quality evaluated

---

### Step 3: Identify Related Features in Main Codebase

Use the Explore agent to find overlapping or related functionality in OoBDev.

**3.1: Launch Feature Discovery Agent**

Based on the incoming codebase analysis, create a task with prompt template:

```
I need you to search through the `src/` directory to find any existing functionality that might overlap with or relate to the {IncomingName} features. Specifically look for:

1. **{Feature Area 1}** - {Description}
2. **{Feature Area 2}** - {Description}
3. **{Feature Area 3}** - {Description}
[... list all feature areas from Step 2]

For each area you find, identify:
- Which projects contain the functionality
- The interfaces and key classes
- How complete the implementation is
- Any gaps or limitations

This will help me create a comprehensive feature mapping between {IncomingName} and the existing OoBDev framework.
```

**Customize Feature Areas** based on Step 2 findings. Common areas:
- I/O and Stream handling
- Expression/LINQ building
- Serialization
- Text templating
- Utilities
- Network protocols
- Binary data handling
- Archive/compression
- Code analysis
- Drawing/graphics
- Database access
- Message queuing
- Authentication/authorization

**Task Parameters:**
- Agent: `Explore`
- Thoroughness: `very thorough`
- Model: `sonnet`

**Expected Output:**
- OoBDev projects with related functionality
- Feature completeness assessment
- Gap analysis
- Interface/API comparison

**Validation:**
- [ ] All feature areas searched
- [ ] Overlapping functionality identified
- [ ] Gaps documented
- [ ] Completeness assessed

---

### Step 4: Create Feature Mapping Document

Synthesize Steps 2 and 3 into a comprehensive feature mapping.

**4.1: Create Documentation Directory**
```bash
mkdir -p docs/migration
```

**4.2: Create Feature Mapping Document**

File: `docs/migration/{incoming-name}-feature-mapping.md`

**Required Sections:**

```markdown
# {IncomingName} to OoBDev Feature Mapping

**Version:** 1.0
**Last Updated:** {DATE}
**Source:** {IncomingName} ({IncomingPath})
**Target:** OoBDev (dotex) Framework

---

## Overview

[One paragraph description of incoming codebase]

---

## Executive Summary

**{IncomingName} Statistics:**
- Total Projects: {count}
- Total C# Files: {count}
- Core Projects: {count}
- Test Projects: {count}
- Target Frameworks: {list}

**Migration Status Overview:**
- **NEW Features**: {count} major feature areas
- **UPDATE Required**: {count} areas with critical bugs/improvements
- **EXISTS (Keep OoBDev)**: {count} areas
- **DELETE (No Migration)**: {count} specialized/obsolete areas

---

## Part 1: {Feature Category 1}

### {Feature 1.1}

**Status:** {NEW | EXISTS | UPDATE | DELETE}
**Source:** `{source path}`
**Target:** `{target path}`
**Priority:** {CRITICAL | HIGH | MEDIUM | LOW | VERY LOW}

#### Feature Breakdown

{Detailed breakdown table}

#### Critical Bugs to Fix

{If UPDATE status, list bugs}

#### Detailed Feature Mapping

{Feature-by-feature comparison}

**Recommendation:**
{Action items and rationale}

---

[Repeat for all feature categories]

---

## Migration Strategy Matrix

| Category | Projects | Status | Priority | Target Layer | Effort |
|----------|----------|--------|----------|--------------|--------|
| {Category} | {count} | {status} | {priority} | {layer} | {effort} |

---

## Architectural Compliance Plan

Following `/docs/architecture` guidelines:

### Layer Placement

{Define where each feature goes}

### Provider/Factory Pattern Application

{Define provider patterns needed}

### Dependency Injection Pattern

{Define DI registrations}

### Testing Standards

{Define test requirements}

### Documentation Standards

{Define documentation requirements}

---

## Related Documentation

[Links to architecture docs]

---

## Change Log

- {DATE} v1.0: Initial feature mapping created
```

**Feature Status Classifications:**

| Status | Meaning | Action |
|--------|---------|--------|
| **NEW** | Does not exist in OoBDev | Migrate |
| **EXISTS** | Exists in OoBDev, similar quality | Keep OoBDev |
| **UPDATE** | Exists but incoming has improvements | Migrate improvements |
| **DELETE** | Obsolete or out of scope | Do not migrate |

**Priority Levels:**

| Priority | Meaning | Blocking |
|----------|---------|----------|
| **CRITICAL** | Bugs or missing critical functionality | All other work |
| **HIGH** | Important features, significant value | None |
| **MEDIUM** | Useful features, moderate value | None |
| **LOW** | Nice-to-have features | None |
| **VERY LOW** | Specialized, niche use cases | None |

**Effort Estimates:**

| Effort | Description |
|--------|-------------|
| **SMALL** | <1 week individual work |
| **MEDIUM** | 1-2 weeks individual work |
| **LARGE** | 2-4 weeks individual work |
| **VERY LARGE** | >1 month individual work |

**Validation:**
- [ ] All features categorized
- [ ] Status assigned to each feature
- [ ] Priority assigned based on value
- [ ] Target layer identified
- [ ] Effort estimated
- [ ] Architectural compliance addressed
- [ ] Bug list complete (if any)

---

### Step 5: Create Migration Plan Document

Transform the feature mapping into actionable migration steps.

**5.1: Create Migration Plan Document**

File: `docs/migration/{incoming-name}-migration-plan.md`

**Required Sections:**

```markdown
# {IncomingName} Migration Plan

**Version:** 1.0
**Last Updated:** {DATE}
**Source Repository:** {IncomingName} ({IncomingPath})
**Target Repository:** OoBDev (dotex) Framework

---

## Overview

This document provides a detailed, actionable migration plan for integrating {IncomingName} features into the OoBDev framework. The plan is organized by migration phases, with each feature area broken down into specific tasks that follow OoBDev architectural standards.

**Reference:** See [{incoming-name}-feature-mapping.md](./{incoming-name}-feature-mapping.md) for comprehensive feature comparison.

---

## Migration Principles

All migration work MUST follow these principles from `/docs/architecture`:

1. **Layered Architecture** - Place projects in correct layer
2. **Provider/Factory Pattern** - Use for all integrations
3. **Dependency Injection** - TryAdd* extensions, builder pattern
4. **Type Safety** - Generic constraints, nullable enabled
5. **Testing** - 80% coverage minimum, MSTest, categorized
6. **Documentation** - README required, XML docs on public APIs
7. **No Breaking Changes** - Maintain backward compatibility

---

## Phase 0: Critical Bug Fixes (IMMEDIATE)

**Priority:** CRITICAL
**Dependencies:** None
**Impact:** Fixes broken functionality in current OoBDev

{List all critical bugs with fix details}

### Task 0.1: {Bug Name}

**Status:** BUG - {CRITICAL | HIGH | MEDIUM | LOW}
**File:** {file path}
**Issue:** {description}

**Current Code:**
```csharp
{broken code}
```

**Fixed Code:**
```csharp
{fixed code}
```

**Steps:**
1. {step 1}
2. {step 2}
...

**Validation:**
- [ ] {validation item 1}
- [ ] {validation item 2}

---

## Phase 1: {Phase Name}

**Priority:** {CRITICAL | HIGH | MEDIUM | LOW}
**Dependencies:** {Phase X complete}
**Goal:** {one sentence goal}

### Task 1.1: {Task Name}

**Status:** {NEW | UPDATE | DELETE}
**Target:** `{OoBDev project path}`
**Source:** `{incoming project path}`

**Current State:**
{describe OoBDev current state}

**Steps:**
1. {actionable step 1}
2. {actionable step 2}
...

**Pattern to Follow:**
```csharp
{code example showing OoBDev pattern}
```

**Validation:**
- [ ] {validation checklist item 1}
- [ ] {validation checklist item 2}

---

[Repeat for all phases and tasks]

---

## Phase {N} Completion Criteria

- [ ] {completion item 1}
- [ ] {completion item 2}

**Estimated Effort:** {Small | Medium | Large | Very Large}
**Blocking:** {What this blocks}

---

## Success Metrics

Migration is complete when:

1. ✅ All critical bugs fixed
2. ✅ All HIGH priority features migrated
3. ✅ Architectural compliance verified
4. ✅ 80%+ test coverage maintained
5. ✅ Documentation complete
6. ✅ No breaking changes
7. ✅ Build succeeds without warnings
8. ✅ All tests pass
9. ✅ NuGet packages generated
10. ✅ Migration guide published

---

## Risk Mitigation

### Risk: {Risk Name}

**Mitigation:**
{mitigation strategy}

---

## Rollback Plan

{If migration causes issues, how to rollback}

---

## Related Documentation

[Links to architecture docs and feature mapping]

---

## Change Log

- {DATE} v1.0: Initial migration plan created
```

**Phase Organization:**

Typical phase structure:
- **Phase 0:** Critical bug fixes (always first if bugs found)
- **Phase 1:** Foundation/infrastructure
- **Phase 2:** High-value features
- **Phase 3:** Medium-value features
- **Phase 4:** Low-value/specialized features
- **Phase 5:** Cleanup and documentation

**Validation:**
- [ ] All phases defined
- [ ] All tasks actionable
- [ ] Dependencies clear
- [ ] Validation checklists complete
- [ ] Success metrics defined
- [ ] Risk mitigation addressed
- [ ] Rollback plan included

---

### Step 6: Create Migration Tracking Document

Create a README to track migration progress.

**6.1: Create Migration Directory README**

File: `docs/migration/README.md`

**Required Sections:**
- Overview
- Active migrations (list)
- Migration phases (summary)
- Migration tracking (checklists)
- Quick start for contributors
- Related documentation

**Validation:**
- [ ] README created
- [ ] All migrations listed
- [ ] Tracking checklists included
- [ ] Links to detailed docs working

---

### Step 7: Identify Critical Issues

Compile all critical bugs and issues found during comparison.

**7.1: Create Issues List**

For each critical bug found:

**In OoBDev (Current Codebase):**
- File path
- Line number(s)
- Issue description
- Impact assessment
- Proposed fix
- Priority (CRITICAL, HIGH, MEDIUM, LOW)

**In Incoming Codebase:**
- Note for reference
- Assess if fix should be included in migration

**7.2: Prioritize Issues**

Sort by:
1. CRITICAL - Broken functionality, data loss risk
2. HIGH - Significant impact, workarounds difficult
3. MEDIUM - Moderate impact, workarounds available
4. LOW - Minor impact, cosmetic issues

**Validation:**
- [ ] All bugs documented
- [ ] Impact assessed
- [ ] Fixes proposed
- [ ] Priorities assigned
- [ ] Added to Phase 0 if critical

---

### Step 8: Define Architectural Mapping

Map incoming codebase to OoBDev architecture.

**8.1: Layer Placement**

For each incoming project, determine target layer:

| Incoming Project | Target Layer | Rationale |
|-----------------|--------------|-----------|
| {project} | Common / Framework / Extensions / ExternalServices | {why} |

**Rules:**
- **Common:** Orchestration, all-in-one packages (rare for migration)
- **Framework:** Core domain logic, abstractions, multi-provider features
- **Extensions:** Custom .NET enhancements, specialized data types
- **ExternalServices:** Third-party integrations, vendor-specific wrappers

**8.2: Namespace Mapping**

Define namespace transformations:

| Incoming Namespace | OoBDev Namespace | Notes |
|-------------------|------------------|-------|
| {incoming} | {oobd} | {transformation notes} |

**8.3: Pattern Mapping**

Identify patterns to apply:

| Incoming Pattern | OoBDev Pattern | Changes Required |
|-----------------|----------------|------------------|
| {pattern} | {pattern} | {changes} |

**Required Patterns:**
- Provider/Factory for integrations
- Dependency Injection via TryAdd*
- Options pattern for configuration
- Attribute-based configuration where appropriate

**Validation:**
- [ ] All projects mapped to layers
- [ ] Namespace transformations defined
- [ ] Patterns identified
- [ ] Changes documented

---

### Step 9: Assess Test Coverage

Compare test coverage between codebases.

**9.1: Incoming Codebase Tests**

Analyze:
- Test project count
- Test framework (MSTest, xUnit, NUnit)
- Coverage percentage (if available)
- Test organization
- Test quality

**9.2: OoBDev Equivalent Coverage**

For overlapping features:
- Existing test coverage in OoBDev
- Gaps in testing
- Areas needing additional tests

**9.3: Test Migration Plan**

Decide for each test suite:
- **MIGRATE** - Incoming tests superior or fill gaps
- **KEEP** - OoBDev tests adequate
- **ENHANCE** - Merge best of both
- **CREATE** - New tests needed

**Validation:**
- [ ] Test coverage assessed
- [ ] Gaps identified
- [ ] Migration plan for tests created
- [ ] Coverage targets defined (80% Framework, 90% LINQ)

---

### Step 10: Estimate Dependencies

Identify external dependencies and their impact.

**10.1: NuGet Package Analysis**

For incoming codebase:
- List all NuGet packages
- Identify version conflicts with OoBDev
- Note deprecated or problematic packages
- Plan for package updates/replacements

**10.2: Framework Version Analysis**

Compare:
- Incoming: Target frameworks
- OoBDev: net9.0 primary, net8.0 support
- Migration: Plan for framework updates

**10.3: Dependency Conflicts**

Identify:
- Version conflicts
- Incompatible packages
- Breaking changes in updates
- Mitigation strategies

**Validation:**
- [ ] All dependencies cataloged
- [ ] Conflicts identified
- [ ] Mitigation plans created
- [ ] Package updates planned

---

### Step 11: Create Migration Checklist

Compile all tasks into a master checklist.

**11.1: Phase Checklists**

For each phase:
- [ ] Phase {N}: {Name}
  - [ ] Task {N.1}: {Task name}
  - [ ] Task {N.2}: {Task name}
  - ...

**11.2: Success Criteria Checklist**

- [ ] All critical bugs fixed
- [ ] All HIGH priority features migrated
- [ ] Architectural compliance verified
- [ ] Test coverage targets met
- [ ] Documentation complete
- [ ] No breaking changes
- [ ] Build succeeds
- [ ] All tests pass
- [ ] NuGet packages generated
- [ ] Migration guide published

**Validation:**
- [ ] All tasks included
- [ ] Checklist format consistent
- [ ] Progress trackable
- [ ] Success criteria clear

---

### Step 12: Document Decision Points

Record key decisions and rationale.

**12.1: Migration Decisions**

For each major decision:

**Decision:** {What was decided}
**Options Considered:**
1. {Option 1}
2. {Option 2}
**Chosen:** {Option X}
**Rationale:** {Why this option}
**Trade-offs:** {What was sacrificed}

**12.2: Deferred Features**

For features NOT migrated:

**Feature:** {Feature name}
**Reason:** {Why not migrating}
**Alternatives:** {What to use instead}
**Future:** {Conditions for reconsidering}

**12.3: Architectural Decisions**

Document any architectural changes:
- New layers created
- New patterns introduced
- Exceptions to standards
- Justifications

**Validation:**
- [ ] All decisions documented
- [ ] Rationale clear
- [ ] Deferred features noted
- [ ] Future reconsideration criteria defined

---

## Validation Checklist

Complete comparison validation:

### Documentation
- [ ] Feature mapping document created
- [ ] Migration plan document created
- [ ] Migration README created
- [ ] All cross-references working
- [ ] Markdown renders correctly

### Analysis Quality
- [ ] All incoming projects cataloged
- [ ] All features identified
- [ ] All overlaps found
- [ ] All gaps identified
- [ ] All bugs documented

### Architectural Compliance
- [ ] Layer placement defined for all projects
- [ ] Namespace mapping complete
- [ ] Pattern mapping documented
- [ ] Provider/factory pattern identified where needed
- [ ] DI registration planned

### Migration Planning
- [ ] Phases defined
- [ ] Tasks actionable
- [ ] Dependencies identified
- [ ] Priorities assigned
- [ ] Effort estimated
- [ ] Success criteria defined
- [ ] Risk mitigation included
- [ ] Rollback plan created

### Completeness
- [ ] No major features missed
- [ ] No critical bugs overlooked
- [ ] Test coverage assessed
- [ ] Dependencies analyzed
- [ ] Decisions documented

---

## Common Issues

| Issue | Cause | Resolution |
|-------|-------|------------|
| Incoming codebase too large | Thousands of files, complex | Break into multiple comparisons by feature area |
| Can't determine functionality | Poor documentation, complex code | Focus on public APIs and tests to infer behavior |
| Many overlapping features | Duplicate implementations | Prioritize by quality, tests, and maintainability |
| Version conflicts | Different framework versions | Plan framework upgrade as separate phase |
| Unclear architecture | Legacy code, no patterns | Document current state, plan refactoring |
| Missing tests | Legacy codebase | Treat as gap, create test plan in migration |

---

## Output Template Structure

All comparisons should produce these files:

```
docs/migration/
├── README.md                                    # Overview and tracking
├── {incoming-name}-feature-mapping.md           # Feature comparison
└── {incoming-name}-migration-plan.md            # Actionable plan
```

### Feature Mapping Template

```markdown
# {Name} Feature Mapping
## Overview
## Executive Summary
## Part 1: {Category}
### {Feature}
- Status: NEW|EXISTS|UPDATE|DELETE
- Priority: CRITICAL|HIGH|MEDIUM|LOW|VERY LOW
- Details
- Recommendation
## Migration Strategy Matrix
## Architectural Compliance Plan
## Related Documentation
## Change Log
```

### Migration Plan Template

```markdown
# {Name} Migration Plan
## Overview
## Migration Principles
## Phase 0: Critical Bug Fixes
### Task 0.1: {Bug}
- Status, File, Issue
- Current/Fixed code
- Steps
- Validation
## Phase 1-N: {Feature Phases}
### Task N.M: {Task}
- Status, Target, Source
- Current State
- Steps
- Pattern to Follow
- Validation
## Success Metrics
## Risk Mitigation
## Rollback Plan
## Related Documentation
## Change Log
```

---

## Best Practices

### During Exploration
1. **Be Thorough** - Don't miss hidden functionality
2. **Document Patterns** - Note architectural patterns used
3. **Assess Quality** - Code quality, tests, documentation
4. **Identify Dependencies** - External packages, framework versions
5. **Find Documentation** - READMEs, comments, docs directories

### During Comparison
1. **Fair Assessment** - Compare functionality, not style
2. **Consider Context** - Age of code, use case, constraints
3. **Focus on Value** - What provides most benefit to OoBDev
4. **Think Long-term** - Maintenance burden, future needs
5. **Respect History** - Understand why decisions were made

### During Planning
1. **Start with Bugs** - Fix critical issues first (Phase 0)
2. **High Value First** - Prioritize features with most impact
3. **No Time Estimates** - Focus on what, not when
4. **Clear Tasks** - Make every task actionable
5. **Validate Everything** - Include validation checklists

### Documentation Quality
1. **Use Examples** - Show code, not just describe
2. **Link Everything** - Cross-reference related docs
3. **Be Specific** - File paths, line numbers, exact issues
4. **Explain Why** - Rationale for decisions
5. **Track Progress** - Checklists for everything

---

## Example Usage

### Scenario: New Directory "Incomming/BinaryDecoders"

**Step 1: Initial Assessment**
```bash
INCOMING_DIR="Incomming/BinaryDecoders"
find $INCOMING_DIR -name "*.csproj" | wc -l
# Output: 41
```

**Step 2: Launch Exploration**
```
Task: Explore Incomming/BinaryDecoders thoroughly...
Result: 41 projects, 800+ C# files, foundation utilities, code analysis, etc.
```

**Step 3: Find Related in OoBDev**
```
Task: Search src/ for I/O, expression building, serialization, etc...
Result: OoBDev.System.IO exists, CodeAnalysis missing, etc.
```

**Step 4-6: Create Documents**
```
Created:
- docs/migration/binarydatadecoders-feature-mapping.md
- docs/migration/binarydatadecoders-migration-plan.md
- docs/migration/README.md
```

**Step 7: Identify Issues**
```
Found 5 critical bugs in OoBDev:
- PathEx lambda bug (HIGH)
- ShiftCommutativeVariablesRight stub (CRITICAL)
- etc.
```

**Result:**
Comprehensive migration plan with:
- 5 critical bug fixes (Phase 0)
- 18 NEW features identified
- 7 UPDATE areas found
- 5 phases defined
- All tasks actionable
- Architectural compliance verified
```

---

## Related Protocols

- [Architectural Analysis](./architectural-analysis.md) - For analyzing architecture of single codebase
- [Protocol Validation](./protocol-validation.md) - Validate this protocol
- [Compare Documentation to Project](./compare-documentation-to-project.md) - For documentation gaps

---

## Change Log

- 2026-01-12 v1.0: Initial incoming codebase comparison protocol created based on BinaryDataDecoders analysis
