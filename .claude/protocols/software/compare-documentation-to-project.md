# Compare Documentation to Project Protocol

**Version:** 1.0
**Last Updated:** 2026-01-03
**Purpose:** Compare existing documentation against a target project to identify gaps

---

## Overview

This protocol defines how to compare existing rewrite/review documentation against a target project to identify gaps, missing features, and create an implementation plan.

---

## When to Use

- Comparing documentation to a new/rewritten project
- Validating rewrite coverage
- Identifying what's missing from a migration
- Planning implementation work

---

## Trigger Phrases

- "what's missing from the rewrite?"
- "compare docs to project"
- "validate rewrite coverage"

---

## Inputs Required

1. **Source Documentation**: Path to review docs (e.g., `docs/rewrite-review/`)
2. **Target Project**: Path to project being compared

---

## Procedure

### Phase 1: Load Documentation Baseline

#### 1.1 Parse action-index.md
Extract all documented:
- Roles
- Screens per role
- Actions per screen
- Linked endpoint docs

#### 1.2 Parse screen-index.md
Extract all documented:
- Screen routes
- Endpoints per screen
- Expected functionality

#### 1.3 Parse roles.md
Extract:
- Role IDs and names
- Permissions matrix
- Authorization patterns

#### 1.4 Parse Feature Docs
For each feature documentation file:
- Extract endpoints
- Extract request/response models
- Extract business rules

### Phase 2: Analyze Target Project

#### 2.1 Backend Analysis
```
Explore target project backend:
- List all controllers and endpoints
- Identify request/response models
- Find authorization attributes
- Map to feature areas
```

Output: List of all endpoints grouped by feature area

#### 2.2 Frontend Analysis
```
Explore target project frontend:
- List all routes
- Identify page components
- Find API calls
- Map state management
```

Output: Navigation structure and component map

#### 2.3 Create Target Inventory
Output: Complete list of implemented features in target

### Phase 3: Gap Analysis

#### 3.1 Endpoint Comparison
```
For each endpoint in documentation:
  Search target for matching:
    - Route pattern
    - HTTP method
    - Similar naming

  Categorize as:
    - FOUND: Exact or close match
    - PARTIAL: Similar but different
    - MISSING: Not found
```

#### 3.2 Screen Comparison
```
For each screen in documentation:
  Search target for:
    - Route definition
    - Page component
    - Required functionality

  Categorize as:
    - COMPLETE: All features present
    - PARTIAL: Some features missing
    - MISSING: Screen not implemented
```

#### 3.3 Role Comparison
```
For each role in documentation:
  Verify in target:
    - Role exists
    - Permissions match
    - Authorization enforced
```

#### 3.4 Feature Comparison
```
For each documented business rule:
  Search target for:
    - Implementation
    - Tests covering rule

  Categorize as:
    - IMPLEMENTED
    - PARTIALLY_IMPLEMENTED
    - NOT_IMPLEMENTED
```

### Phase 4: Generate Gap Report

#### 4.1 Create gap-analysis.md

```markdown
# Gap Analysis Report

Generated: {date}
Source Docs: {path}
Target Project: {path}

## Summary

| Category  | Documented | Found | Partial | Missing |
|-----------|------------|-------|---------|---------|
| Endpoints | X          | X     | X       | X       |
| Screens   | X          | X     | X       | X       |
| Features  | X          | X     | X       | X       |

## Coverage: X%

---

## Missing Endpoints

| Endpoint                  | Feature      | Priority | Notes     |
|---------------------------|--------------|----------|-----------|
| POST /api/{entity}/Create | {feature}    | High     | Core CRUD |
| ...                       | ...          | ...      | ...       |

## Partial Implementations

| Feature        | Missing Aspects    | Priority |
|----------------|--------------------|----------|
| File Upload    | OCR not found      | Medium   |
| ...            | ...                | ...      |

## Missing Screens

| Screen           | Route             | Priority | Dependencies |
|------------------|-------------------|----------|--------------|
| Approval         | /approval         | High     | None         |
| ...              | ...               | ...      | ...          |

## Missing Business Rules

| Rule                      | Feature    | Impact           |
|---------------------------|------------|------------------|
| ID must be unique         | {feature}  | Data integrity   |
| ...                       | ...        | ...              |
```

### Phase 5: Generate Implementation Plan

#### 5.1 Prioritize Gaps
```
Priority criteria:
- HIGH: Core functionality, blocks other features
- MEDIUM: Important but not blocking
- LOW: Nice to have, edge cases
```

#### 5.2 Create implementation-plan.md

```markdown
# Implementation Plan

## Phase 1: Critical Missing Features

### 1.1 {Feature Name}
**Gap**: {description}
**Endpoints needed**:
- POST /api/...
- GET /api/...

**Implementation steps**:
1. Create controller
2. Create service
3. Create repository method
4. Create frontend page
5. Add to navigation
6. Test

**Acceptance criteria**:
- [ ] Endpoint responds correctly
- [ ] UI matches wireframe
- [ ] Role permissions enforced
- [ ] Matches documented behavior

---

## Phase 2: Secondary Features
...

## Phase 3: Polish & Edge Cases
...

## Total Gaps Summary
- Critical: X items
- Medium: X items
- Low: X items
```

#### 5.3 Create Tracking Checklist

```markdown
# Implementation Tracking

## Endpoints
- [ ] POST /api/{entity}/Create
- [ ] GET /api/{entity}/GetById
- ...

## Screens
- [ ] /{feature}/create
- [ ] /{feature}/list
- ...

## Business Rules
- [ ] ID uniqueness validation
- [ ] Role-based filtering
- ...
```

### Phase 6: Output Files

Create in target project or docs location:

```
docs/gap-analysis/
├── gap-analysis.md           # What's missing
├── implementation-plan.md    # How to close gaps
└── tracking/
    ├── endpoints.md          # Endpoint checklist
    ├── screens.md            # Screen checklist
    └── business-rules.md     # Rule checklist
```

---

## Usage Examples

### Example 1: Same Repo Comparison
```
User: Compare docs/rewrite-review to the new-api branch

Steps:
1. Load docs from docs/rewrite-review/
2. Checkout new-api branch (or analyze in parallel)
3. Compare endpoints, screens, features
4. Generate gap report
```

### Example 2: Cross-Repo Comparison
```
User: Compare this documentation to /path/to/new-project

Steps:
1. Load docs from current location
2. Explore /path/to/new-project
3. Compare and generate report
```

### Example 3: Partial Comparison
```
User: Just compare the Product Management features

Steps:
1. Load only product-management/ docs
2. Analyze only product-related code in target
3. Generate focused gap report
```

---

## Validation Checklist

- [ ] Gap analysis report generated
- [ ] Implementation plan created
- [ ] Tracking checklists ready
- [ ] Priority assigned to all gaps
- [ ] Dependencies identified

---

## Related Protocols

- [Generate Rewrite Documentation](./generate-rewrite-documentation.md) - Creating the source docs
- [Architectural Analysis](./architectural-analysis.md) - System analysis

---

## Change Log

- 2026-01-03 v1.0: Initial generalized version
